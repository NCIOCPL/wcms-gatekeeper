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
    public class DrugInfoSummaryProcessor : DocumentProcessorCommon, IDocumentProcessor
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

            /// All the nifty document processing code starts here.
            //throw new NotImplementedException();

            List<Dictionary<string, string>> fieldCollection = new List<Dictionary<string, string>>();
            fieldCollection.Add(GetFields(document));

            idList = CMSController.CreateContentItemList("pdqDrugInfoSummary", fieldCollection, GetTargetFolder(document.PrettyURL));
            ContentMetaItem cmi = new ContentMetaItem(9999,GetFields(document));
           
            // Map Relationships.
            // Store content item.

            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
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
