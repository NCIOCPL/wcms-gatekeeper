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
            VerifyRequiredDocumentType(documentObject, DocumentType.DrugInfoSummary);

            DrugInfoSummaryDocument document = documentObject as DrugInfoSummaryDocument;

            InformationWriter(string.Format("Begin Percussion processing for document CDRID = {0}.", document.DocumentID));

            /// All the nifty document processing code starts here.
            //throw new NotImplementedException();

            //Create Target Folder
            //CMSController.CreateTargetDirectory(GetTargetFolder(document.PrettyURL));

            // Get content item (Create new, or load existing) and Convert properties to CMS fields.
            //CMSController.SetContentType("pdqDrugInfoSummary");

            CMSController.CreateContentItem("pdqDrugInfoSummary", GetFields(document), GetTargetFolder(document.PrettyURL));

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

        private List<Dictionary<string, string>> GetFields(GateKeeper.DocumentObjects.DrugInfoSummary.DrugInfoSummaryDocument DocType)
        {
            List<Dictionary<string, string>> itemFields = new List<Dictionary<string, string>>();
            Dictionary<string, string> fields = new Dictionary<string, string>();

            fields.Add("pretty_url_name", DocType.PrettyURL.Substring(DocType.PrettyURL.LastIndexOf('/')+1));
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


            fields.Add("sys_title", "99");

            itemFields.Add(fields);
            return itemFields;
        }

        #endregion
    }
}
