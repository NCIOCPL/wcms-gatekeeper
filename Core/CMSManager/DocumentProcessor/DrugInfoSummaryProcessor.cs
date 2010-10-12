using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GKManagers.CMSManager.PercussionWebSvc;

using GKManagers.CMSManager.Configuration;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GKManagers.CMSManager.CMS;

namespace GKManagers.CMSManager.DocumentProcessing
{
    public class DrugInfoSummaryProcessor : DocumentProcessorCommon, IDocumentProcessor, IDisposable
    {
        public DrugInfoSummaryProcessor(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        {
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
            IDMapManager mapManager = new IDMapManager();
            CMSIDMapping mappingInfo = mapManager.LoadCdrIDMappingByCdrid(document.DocumentID);

            // No mapping found, this is a new item.
            if (mappingInfo == null)
            {
                // Turn the list of item fields into a list of one item.
                CreateContentItem contentItem = new CreateContentItem(CreateFieldValueMap(document), GetTargetFolder(document.PrettyURL), percussionConfig.ContentType.PDQDrugInfoSummary.Value);
                List<CreateContentItem> contentItemList = new List<CreateContentItem>();
                contentItemList.Add(contentItem);


                // Create the new content item. (All items are created of the same type.)
                InformationWriter(string.Format("Adding document CDRID = {0} to Percussion system.", document.DocumentID));
                idList = CMSController.CreateContentItemList(contentItemList);

                // Save the mapping between the CDR and CMS IDs.
                mappingInfo = new CMSIDMapping(document.DocumentID, idList[0], document.PrettyURL);
                mapManager.InsertCdrIDMapping(mappingInfo);
            }
            else
            {
                // A mapping exists, we're updating an item.

                // This is an existing item, we must therefore put it in an editable state.
                TransitionItemsToStaging(new long[1] { mappingInfo.CmsID });

                UpdateContentItem contentItem = new UpdateContentItem(mappingInfo.CmsID, CreateFieldValueMap(document), GetTargetFolder(document.PrettyURL));
                List<UpdateContentItem> contentItemList = new List<UpdateContentItem>();
                contentItemList.Add(contentItem);
                InformationWriter(string.Format("Updating document CDRID = {0} in Percussion system.", document.DocumentID));
                idList = CMSController.UpdateContentItemList(contentItemList);

                //Check if the Pretty URL changed if yes then move the content item to the new folder in percussion.
                if (mappingInfo.PrettyURL != document.PrettyURL)
                {
                    long[] id = idList.ToArray() ;
                    CMSController.GuaranteeFolder(document.PrettyURL);
                    CMSController.MoveContentItemFolder(mappingInfo.PrettyURL, document.PrettyURL, id);
                   
                    //Delete existing mapping for the CDRID.
                    mapManager.DeleteCdrIDMapping(document.DocumentID);

                    // Save the mapping between the CDR and CMS IDs.
                    mappingInfo = new CMSIDMapping(document.DocumentID, idList[0], document.PrettyURL);
                    mapManager.InsertCdrIDMapping(mappingInfo);

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
            IDMapManager mapManager = new IDMapManager();
            CMSIDMapping mappingInfo = mapManager.LoadCdrIDMappingByCdrid(documentID);

            if (mappingInfo != null)
            {
                // Check for items with references.
                VerifyDocumentMayBeDeleted(mappingInfo.CmsID);

                // Delete item from CMS.
                CMSController.DeleteItem(mappingInfo.CmsID);

                // Delete item mapping.
                mapManager.DeleteCdrIDMapping(documentID);
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
            IDMapManager mapManager = new IDMapManager();
            CMSIDMapping mappingInfo = mapManager.LoadCdrIDMappingByCdrid(documentID);

            TransitionItemsToPreview(new long[1]{mappingInfo.CmsID});
        }

        /// <summary>
        /// Move the specified DrugInfoSummary document from Preview to Live.
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public void PromoteToLive(int documentID)
        {
            IDMapManager mapManager = new IDMapManager();
            CMSIDMapping mappingInfo = mapManager.LoadCdrIDMappingByCdrid(documentID);

            TransitionItemsToLive(new long[1] { mappingInfo.CmsID });
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
        private string GetTargetFolder(string targetFolderPath)
        {
            //1. Remove last part of path, e.g. /cancertopics/druginfo/methotrexate becomes /cancertopics/druginfo
            string truncUrl = targetFolderPath.Substring(0, targetFolderPath.LastIndexOf('/'));
            if (truncUrl != string.Empty)
            {
                return truncUrl;
            }
            return truncUrl;
        }

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
            fields.Add("print_available", "1");
            fields.Add("email_available", "1");
            fields.Add("share_available", "1");
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
