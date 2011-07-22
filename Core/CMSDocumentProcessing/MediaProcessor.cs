using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GKManagers.CMSManager.Configuration;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Media;
using NCI.WCM.CMSManager;
using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSDocumentProcessing
{
    public class MediaProcessor : DocumentProcessorCommon, IDocumentProcessor, IDisposable
    {
        #region Fields

        // Contains the name of the Media Information Summary ContentType as used in the CMS.
        // Set in the constructor.
        readonly private string MediaContentType;

        #endregion

        public MediaProcessor(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        {
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            MediaContentType = percussionConfig.ContentType.PDQMedia.Value;
        }

        #region IDocumentProcessor Members

        /// <summary>
        /// Main entry point for processing a Media object which is to be
        /// managed in the CMS.
        /// </summary>
        /// <param name="documentObject"></param>
        public void ProcessDocument(Document documentObject)
        {
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

            List<long> idList;

            VerifyRequiredDocumentType(documentObject, DocumentType.Media);

            MediaDocument document = documentObject as MediaDocument;

            InformationWriter(string.Format("Begin Percussion processing for document CDRID = {0}.", document.DocumentID));


            // Are we updating an existing document? Or saving a new one?
            PercussionGuid identifier = GetCdrDocumentID(MediaContentType, document.DocumentID);

            // No identifier was returned, this is a new item.
            if (identifier == null)
            {
                string targetPathName = GetTargetFolder();

                // Turn the list of item fields into a list of one item.
                ContentItemForCreating contentItem = new ContentItemForCreating(MediaContentType, CreateFieldValueMap(document), targetPathName);
                List<ContentItemForCreating> contentItemList = new List<ContentItemForCreating>();
                contentItemList.Add(contentItem);


                // Create the new content item. (All items are created of the same type.)
                InformationWriter(string.Format("Adding document CDRID = {0} to Percussion system.", document.DocumentID));
                idList = CMSController.CreateContentItemList(contentItemList);
            }
            else
            {
                // We're updating an existing item, go fetch it.
                PSItem[] oldItem = CMSController.LoadContentItems(new PercussionGuid[] { identifier });

                string targetFolder = GetTargetFolder();

                // This is an existing item, we must therefore put it in an editable state.
                TransitionItemsToStaging(new PercussionGuid[1] { identifier });

                ContentItemForUpdating contentItem = new ContentItemForUpdating(identifier.ID, CreateFieldValueMap(document));
                List<ContentItemForUpdating> contentItemList = new List<ContentItemForUpdating>();
                contentItemList.Add(contentItem);
                InformationWriter(string.Format("Updating document CDRID = {0} in Percussion system.", document.DocumentID));
                idList = CMSController.UpdateContentItemList(contentItemList);
            }

            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
        }

        /// <summary>
        /// Deletes the content item.
        /// </summary>
        /// <param name="documentID">The document ID.</param>
        public void DeleteContentItem(int documentID)
        {
            PercussionGuid identifier = GetCdrDocumentID(MediaContentType, documentID);

            if (identifier != null)
            {
                // Check for items with references.
                VerifyDocumentMayBeDeleted(identifier.ID);

                // Delete item from CMS.
                CMSController.DeleteItem(identifier.ID);
            }
            else
            {
                // Don't report an error on attempt to delete item which has already
                // been deleted.  Necessary in case a delete is run twice.
                ;
            }

        }

        /// <summary>
        /// Verifies that a document object has no incoming refernces. Throws CMSCannotDeleteException
        /// if the document is the target of any incoming relationships.
        /// </summary>
        /// <param name="documentCmsIdentifier">The document's ID in the CMS.</param>
        protected void VerifyDocumentMayBeDeleted(long documentCmsIdentifier)
        {
            PSItem[] itemsWithLinks = CMSController.LoadLinkingContentItems(documentCmsIdentifier);

            // Filter out any items where this content item is both the target and the
            // source of the relationship. (Internal link)
            itemsWithLinks =
                Array.FindAll(itemsWithLinks, item => !PSItemUtils.CompareItemIds(item.id, documentCmsIdentifier));

            // Build up a list of item URLs which refer to the targeted item.
            List<string> itemPaths = new List<string>();
            foreach (PSItem item in itemsWithLinks)
            {
                string prettyUrlName = PSItemUtils.GetFieldValue(item, "pretty_url_name");
                itemPaths.AddRange(
                    from PSItemFolders folder in item.Folders
                    select folder.path + "/" + prettyUrlName
                    );
            }

            if (itemPaths.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Document is referenced by:\n", documentCmsIdentifier);
                itemPaths.ForEach(path => sb.AppendLine(path));
                throw new CMSCannotDeleteException(sb.ToString());
            }
        }


        /// <summary>
        /// Move the specified Media document from Staging to Preview.
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public void PromoteToPreview(int documentID)
        {
            PercussionGuid identifier = GetCdrDocumentID(MediaContentType, documentID);

            TransitionItemsToPreview(new PercussionGuid[1] { identifier });
        }

        /// <summary>
        /// Move the specified Media document from Preview to Live.
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public void PromoteToLive(int documentID)
        {
            PercussionGuid identifier = GetCdrDocumentID(MediaContentType, documentID);

            TransitionItemsToLive(new PercussionGuid[1] { identifier });
        }

        #endregion


        #region Disposable Pattern Members

        protected override void Dispose(bool disposing)
        {
            // Free managed resources.
            if (disposing)
            {
                base.Dispose(disposing);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the name of the percussion folder where the media will be stored from the 
        /// configuration file.
        /// </summary>
        private string GetTargetFolder()
        {
            string folderPath = System.Configuration.ConfigurationManager.AppSettings["MediaFolderPath"];
            if (string.IsNullOrEmpty(folderPath))
                throw new Exception(@"The folder path to store media content is not specified, 
                                      please provide value for AppSettings configuration key 
                                      'MediaFolderPath'");
            return folderPath;
        }

        /// <summary>
        /// Parses the pretty URL for a Media information summary item and returns the final
        /// path section as the item name.  If no path separators are found, the entire
        /// itemPath is treated as the pretty URL name.
        /// </summary>
        /// <param name="itemPath">Pretty URL of the Media info summary.</param>
        /// <returns>The </returns>
        private string GetPrettyUrlName(string itemPath)
        {
            string prettyURLName;

            int lastIndex = itemPath.LastIndexOf('/');

            // Handle trailing /
            if (lastIndex == itemPath.Length)
            {
                itemPath = itemPath.Remove(lastIndex);
                lastIndex = itemPath.LastIndexOf('/');
            }

            if (lastIndex >= 0)
                prettyURLName = itemPath.Substring(lastIndex + 1);
            else
                prettyURLName = itemPath;

            return prettyURLName;
        }

        /// <summary>
        /// Creates a mapping of Media information summary data points and the fields used
        /// in the CMS content type.
        /// </summary>
        /// <param name="MediaInfo">MediaDocument object to map</param>
        /// <returns>A Dictionary of key/value pairs. Keys are the names of fields in the
        /// CMS content type.</returns>
        private FieldSet CreateFieldValueMap(MediaDocument mediaInfo)
        {
            FieldSet fieldSet = new FieldSet();

            // TODO : how to get pretty url name
            string prettyURLName = mediaInfo.DocumentID.ToString();
            string title = mediaInfo.DocumentID.ToString();
            string fileName = mediaInfo.DocumentID.ToString() + mediaInfo.Extension;

            fieldSet.Add("cdrid", mediaInfo.DocumentID.ToString());
            fieldSet.Add("sys_title", prettyURLName);
            fieldSet.Add("sys_lang", LanguageEnglish);

            fieldSet.Add("long_title", title);
            fieldSet.Add("short_title", title.Substring(0, Math.Min(100, title.Length)));
            fieldSet.Add("item_file_attachment", mediaInfo.EncodedData);
            fieldSet.Add("item_file_attachment_ext", mediaInfo.Extension);
            fieldSet.Add("item_file_attachment_filename", fileName);
            fieldSet.Add("item_file_attachment_size", mediaInfo.Size);
            fieldSet.Add("item_file_attachment_type", mediaInfo.MimeType);
            fieldSet.Add("pretty_url_name", prettyURLName);
            fieldSet.Add("filename", fileName);
            fieldSet.Add("sys_suffix", mediaInfo.Extension);


            return fieldSet;
        }

        #endregion

    }
}
