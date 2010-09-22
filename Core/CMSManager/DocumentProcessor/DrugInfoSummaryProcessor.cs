using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                //List<Dictionary<string, string>> fieldCollection = new List<Dictionary<string, string>>();
                //fieldCollection.Add(GetFields(document));
                
                // Turn the list of item fields into a list of one item.

                CreateContentItem contentItem = new CreateContentItem(GetFields(document), GetTargetFolder(document.PrettyURL));
                List<CreateContentItem> contentItemList = new List<CreateContentItem>();
                contentItemList.Add(contentItem);


                // Create the new content item. (All items are created of the same type.)
                idList = CMSController.CreateContentItemList("pdqDrugInfoSummary", contentItemList);

                // Save the mapping between the CDR and CMS IDs.
                mappingInfo = new CMSIDMapping(document.DocumentID, idList[0], document.PrettyURL);
                mapManager.InsertCdrIDMapping(mappingInfo);
            }
            else
            {
                
                // A mapping exists, we're updating an item.
                UpdateContentItem contentItem = new UpdateContentItem(mappingInfo.CmsID, GetFields(document), GetTargetFolder(document.PrettyURL));
                List<UpdateContentItem> contentItemList = new List<UpdateContentItem>();
                contentItemList.Add(contentItem);
                idList = CMSController.UpdateContentItemList(contentItemList);


            }

            // Map Relationships.
            // Store content item.

            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
        }

        #endregion


        #region Disposable Pattern Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Free managed resources only.
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

            fields.Add("date_first_published", DocType.LastModifiedDate.ToString());

            fields.Add("cdrid", DocType.DocumentID.ToString());


            fields.Add("sys_title", prettyURLName);
            
            return fields;
        }

        #endregion
    }
}
