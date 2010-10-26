using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using GKManagers.CMSManager.Configuration;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using NCI.WCM.CMSManager;
using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSDocumentProcessing
{
    public class DrugInfoSummaryProcessor : DocumentProcessorCommon, IDocumentProcessor, IDisposable
    {
        #region Fields

        // Contains the name of the Drug Information Summary ContentType as used in the CMS.
        // Set in the constructor.
        readonly private string DrugInfoSummaryContentType;
        
        #endregion

        public DrugInfoSummaryProcessor(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        {
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            DrugInfoSummaryContentType = percussionConfig.ContentType.PDQDrugInfoSummary.Value;
        }

        #region IDocumentProcessor Members

        /// <summary>
        /// Main entry point for processing a DrugInfoSummary object which is to be
        /// managed in the CMS.
        /// </summary>
        /// <param name="documentObject"></param>
        public void ProcessDocument(Document documentObject)
        {
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

            List<long> idList;

            VerifyRequiredDocumentType(documentObject, DocumentType.DrugInfoSummary);

            DrugInfoSummaryDocument document = documentObject as DrugInfoSummaryDocument;

            InformationWriter(string.Format("Begin Percussion processing for document CDRID = {0}.", document.DocumentID));

            // Are we updating an existing document? Or saving a new one?
            PercussionGuid identifier = GetCdrDocumentID(DrugInfoSummaryContentType, document.DocumentID);

            // No identifier was returned, this is a new item.
            if (identifier == null)
            {
                // Turn the list of item fields into a list of one item.
                ContentItemForCreating contentItem = new ContentItemForCreating(CreateFieldValueMap(document), GetTargetFolder(document.PrettyURL), DrugInfoSummaryContentType);
                List<ContentItemForCreating> contentItemList = new List<ContentItemForCreating>();
                contentItemList.Add(contentItem);


                // Create the new content item. (All items are created of the same type.)
                InformationWriter(string.Format("Adding document CDRID = {0} to Percussion system.", document.DocumentID));
                idList = CMSController.CreateContentItemList(contentItemList);
            }
            else
            {
                // We're updating an existing item, go fetch it.
                PSItem[] oldItem = CMSController.LoadContentItems(new long[] { identifier.ID });

                // This is an existing item, we must therefore put it in an editable state.
                TransitionItemsToStaging(new long[1] { identifier.ID });

                // TODO: Refactor this.  We've already loaded the item so we can check its path,
                // and the CMSController.UpdateContentItemList method goes and loads it again
                // before overwriting the field values. Making the communications layer responsible
                // for knowing exactly what to update may be too much logic in that layer.

                ContentItemForUpdating contentItem = new ContentItemForUpdating(identifier.ID, CreateFieldValueMap(document), GetTargetFolder(document.PrettyURL));
                List<ContentItemForUpdating> contentItemList = new List<ContentItemForUpdating>();
                contentItemList.Add(contentItem);
                InformationWriter(string.Format("Updating document CDRID = {0} in Percussion system.", document.DocumentID));
                idList = CMSController.UpdateContentItemList(contentItemList);

                //Check if the Pretty URL changed if yes then move the content item to the new folder in percussion.
                string targetFolder = GetTargetFolder(document.PrettyURL);
                string oldPath = CMSController.GetPathInSite(oldItem[0]);
                if (!oldPath.Equals(targetFolder, StringComparison.InvariantCultureIgnoreCase))
                {
                    long[] id = idList.ToArray() ;
                    //string sourceFolder = GetTargetFolder(oldItem[0].Folders[0].path);

                    CMSController.GuaranteeFolder(targetFolder);
                    CMSController.MoveContentItemFolder(oldPath, targetFolder, id);
                }

            }

            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
        }

        /// <summary>
        /// Deletes the content item.
        /// </summary>
        /// <param name="documentID">The document ID.</param>
        public void DeleteContentItem(int documentID)
        {
            PercussionGuid identifier = GetCdrDocumentID(DrugInfoSummaryContentType, documentID);

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
        /// <param name="documentCmsID">The document's ID in the CMS.</param>
        protected override void VerifyDocumentMayBeDeleted(long documentCmsID)
        {
            PSItem[] itemsWithLinks = CMSController.LoadLinkingContentItems(documentCmsID);

            // Filter out any items where this content item is both the target and the
            // source of the relationship. (Internal link)
            itemsWithLinks =
                Array.FindAll(itemsWithLinks, item => !PSItemUtils.CompareItemIds(item.id, documentCmsID));

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
                sb.AppendFormat("Document is referenced by:\n", documentCmsID);
                itemPaths.ForEach(path => sb.AppendLine(path));
                throw new CMSCannotDeleteException(sb.ToString());
            }
        }


        /// <summary>
        /// Move the specified DrugInfoSummary document from Staging to Preview.
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public void PromoteToPreview(int documentID)
        {
            PercussionGuid identifier = GetCdrDocumentID(DrugInfoSummaryContentType, documentID);

            TransitionItemsToPreview(new long[1]{identifier.ID});
        }

        /// <summary>
        /// Move the specified DrugInfoSummary document from Preview to Live.
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public void PromoteToLive(int documentID)
        {
            PercussionGuid identifier = GetCdrDocumentID(DrugInfoSummaryContentType, documentID);

            TransitionItemsToLive(new long[1] { identifier.ID });
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
        /// Parses the URL for a drug information summary item and separates the folder path
        /// from the item name.
        /// </summary>
        /// <param name="itemPath">Server-relative path of a drug information summary.</param>
        /// <returns>The folder path where the item is to be stored.</returns>
        /// <remarks>The rules for determining the target folder path vary between content items.
        /// In the case of drug information summaries, the path item after the final, non-trailing,
        /// path separator is the item name and the rest is used to determine the folder path.
        /// e.g. For the URL /cancertopics/druginfo/methotrexate, the target path is /cancertopics/druginfo
        /// </remarks>
        private string GetTargetFolder(string itemPath)
        {
            // Elminate leading/trailing whitespace and trailing slash.
            string targetFolder = itemPath.Trim();
            if (targetFolder.EndsWith("/") && targetFolder.Length > 1)
            {
                targetFolder = targetFolder.Substring(0, targetFolder.Length - 1);
            }

            // Separate out the target folder.
            int separatorIndex = itemPath.LastIndexOf('/');
            if (separatorIndex >= 0)
                targetFolder = itemPath.Substring(0, separatorIndex);

            return targetFolder;
        }


        /// <summary>
        /// Creates a mapping of drug information summary data points and the fields used
        /// in the CMS content type.
        /// </summary>
        /// <param name="drugInfo">DrugInfoSummaryDocument object to map</param>
        /// <returns>A Dictionary of key/value pairs. Keys are the names of fields in the
        /// CMS content type.</returns>
        private Dictionary<string, string> CreateFieldValueMap(DrugInfoSummaryDocument drugInfo)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string prettyURLName = drugInfo.PrettyURL.Substring(drugInfo.PrettyURL.LastIndexOf('/') + 1);


            fields.Add("pretty_url_name", prettyURLName);
            fields.Add("long_title", drugInfo.Title);

            if (drugInfo.Title.Length > 64)
                fields.Add("short_title", drugInfo.Title.Substring(1, 64));
            else
                fields.Add("short_title", drugInfo.Title);

            fields.Add("long_description", drugInfo.Description);
            fields.Add("bodyfield", drugInfo.Html);
            fields.Add("short_description", string.Empty);
            fields.Add("date_next_review", "1/1/2100");

            if (drugInfo.LastModifiedDate != DateTime.MinValue)
                fields.Add("date_last_modified", drugInfo.LastModifiedDate.ToString());
            else
                fields.Add("date_last_modified", null);

            fields.Add("date_first_published", drugInfo.FirstPublishedDate.ToString());

            fields.Add("cdrid", drugInfo.DocumentID.ToString());


            fields.Add("sys_title", prettyURLName);

            return fields;
        }

        #endregion
    }
}
