using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                CreateContentItem contentItem = new CreateContentItem(GetFields(document), GetTargetFolder(document.PrettyURL), percussionConfig.ContentType.PDQDrugInfoSummary.Value);
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

                UpdateContentItem contentItem = new UpdateContentItem(mappingInfo.CmsID, GetFields(document), GetTargetFolder(document.PrettyURL));
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
            long[] id = new long[] {mappingInfo.CmsID};
            CMSController.DeleteItem(id);

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

        private Dictionary<string, string> GetFields(GateKeeper.DocumentObjects.DrugInfoSummary.DrugInfoSummaryDocument DocType)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string prettyURLName=DocType.PrettyURL.Substring(DocType.PrettyURL.LastIndexOf('/')+1);

            
            fields.Add("pretty_url_name", prettyURLName);
            fields.Add("long_title", DocType.Title);

            if(DocType.Title.Length>64)
                fields.Add("short_title", DocType.Title.Substring(1,64));
            else
                fields.Add("short_title", DocType.Title);

            fields.Add("long_description", DocType.Description);
            fields.Add("bodyfield", DocType.Html);
            fields.Add("short_description", string.Empty);
            fields.Add("date_next_review", "1/1/2100");
            fields.Add("print_available", "1");
            fields.Add("email_available", "1");
            fields.Add("share_available", "1");
            if(DocType.LastModifiedDate.ToString()!=string.Empty)
                fields.Add("date_last_modified", DocType.LastModifiedDate.ToString());
            else
                fields.Add("date_last_modified", string.Empty);

            fields.Add("date_first_published", DocType.FirstPublishedDate.ToString());

            fields.Add("cdrid", DocType.DocumentID.ToString());


            fields.Add("sys_title", prettyURLName);
            
            return fields;
        }

        #endregion
    }
}
