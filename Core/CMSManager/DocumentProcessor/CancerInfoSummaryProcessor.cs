using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.CMSManager.CMS;

namespace GKManagers.CMSManager.DocumentProcessing
{
    public class CancerInfoSummaryProcessor : DocumentProcessorCommon, IDocumentProcessor, IDisposable
    {
        public CancerInfoSummaryProcessor(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        {
        }

        #region IDocumentProcessor Members

        /// <summary>
        /// Main entry point for processing a Cancer Information Summary (formerly just "Summary")
        /// object which is to be managed in the CMS.
        /// </summary>
        /// <param name="documentObject"></param>
        public void ProcessDocument(Document documentObject)
        {

            VerifyRequiredDocumentType(documentObject, DocumentType.Summary);

            SummaryDocument document = documentObject as SummaryDocument;

            InformationWriter(string.Format("Begin Percussion processing for document CDRID = {0}.", document.DocumentID));


            /// All the nifty document processing code starts here.

            // Are we updating an existing document? Or saving a new one?
            IDMapManager mapManager = new IDMapManager();
            CMSIDMapping mappingInfo = mapManager.LoadCdrIDMappingByCdrid(document.DocumentID);

            // No mapping found, this is a new item.
            if (mappingInfo == null)
            {
                
                // Create the new pdqCancerInfoSummary content item. (All items are created of the same type.)
                CreatePDQCancerInfoSummary(document, mappingInfo);

                //Create new pdqTableSections
                CreatePDQTableSections(document);

                //Create new pdqCancerInfoSummaryPage
                CreatePDQCancerInfoSummaryPage(document);


            }

            else
            {

            }

            // Get content item (Create new, or load existing)


            // Convert properties to CMS fields.
            // Map Relationships.
            // Store content item.

            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
        }

        #endregion
        
        #region Private Methods

        private void CreatePDQCancerInfoSummaryPage(SummaryDocument document)
        {
            int i;
            List<long> idList;
            List<CreateContentItem> contentItemList = new List<CreateContentItem>();

            try
            {
                for (i = 0; i <= document.SectionList.Count - 1; i++)
                {
                    CreateContentItem contentItem = new CreateContentItem(GetFieldsPDQCancerInfoSummaryPage(document.SectionList[i]), GetTargetFolder(document.BasePrettyURL));
                    if (contentItem.Fields["sys_title"] != string.Empty || contentItem.Fields["sys_title"] != "")
                        contentItemList.Add(contentItem);

                }

                // Create the new content item. (All items are created of the same type.)
                InformationWriter(string.Format("Adding document CDRID = {0} to Percussion system.", document.DocumentID));
                idList = CMSController.CreateContentItemList("pdqCancerInfoSummaryPage", contentItemList);

            }

            catch(Exception e)
            {

            }

        }

        private Dictionary<string, string> GetFieldsPDQCancerInfoSummaryPage(SummarySection cancerInfoSummaryPage)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string prettyURLName = cancerInfoSummaryPage.PrettyUrl.Substring(cancerInfoSummaryPage.PrettyUrl.LastIndexOf('/') + 1);

            fields.Add("bodyfield", cancerInfoSummaryPage.Html.InnerXml.Replace("SummaryRef","a"));
            fields.Add("sys_title", prettyURLName);


            return fields;
        }


        private void CreatePDQTableSections(SummaryDocument document)
        {
            int i;
            List<long> idList;

            for (i = 0; i <= document.TableSectionList.Count-1;i++ )
            {
                CreateContentItem contentItem = new CreateContentItem(GetFieldsPDQTableSection(document.TableSectionList[i]), GetTargetFolder(document.BasePrettyURL));
                List<CreateContentItem> contentItemList = new List<CreateContentItem>();
                contentItemList.Add(contentItem);
                
                // Create the new content item. (All items are created of the same type.)
                InformationWriter(string.Format("Adding document CDRID = {0} to Percussion system.", document.DocumentID));
                idList = CMSController.CreateContentItemList("pdqTableSection", contentItemList);
            }

        }

        private Dictionary<string, string> GetFieldsPDQTableSection(SummarySection tableSection)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string prettyURLName = tableSection.PrettyUrl.Substring(tableSection.PrettyUrl.LastIndexOf('/') + 1);

            fields.Add("pretty_url_name", prettyURLName);
            fields.Add("bodyfield", tableSection.Html.InnerXml.Replace("SummaryRef", "a"));
            fields.Add("sys_title", prettyURLName);


            return fields;
        }



        private void CreatePDQCancerInfoSummary(SummaryDocument document, CMSIDMapping mappingInfo)
        {
            List<long> idList;

            // Turn the list of item fields into a list of one item.
            CreateContentItem contentItem = new CreateContentItem(GetFieldsPDQCancerInfoSummary(document), GetTargetFolder(document.BasePrettyURL));
            List<CreateContentItem> contentItemList = new List<CreateContentItem>();
            contentItemList.Add(contentItem);


            // Create the new content item. (All items are created of the same type.)
            InformationWriter(string.Format("Adding document CDRID = {0} to Percussion system.", document.DocumentID));
            idList = CMSController.CreateContentItemList("pdqCancerInfoSummary", contentItemList);

            // Save the mapping between the CDR and CMS IDs.
            IDMapManager mapManager = new IDMapManager();
            mappingInfo = new CMSIDMapping(document.DocumentID, idList[0], document.BasePrettyURL);
            mapManager.InsertCdrIDMapping(mappingInfo);

        }

        private Dictionary<string, string> GetFieldsPDQCancerInfoSummary(SummaryDocument DocType)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string prettyURLName = DocType.BasePrettyURL.Substring(DocType.BasePrettyURL.LastIndexOf('/') + 1);


            fields.Add("pretty_url_name", prettyURLName);
            fields.Add("long_title", DocType.Title);

            if (DocType.Title.Length > 64)
                fields.Add("short_title", DocType.Title.Substring(1, 64));
            else
                fields.Add("short_title", DocType.Title);

            fields.Add("long_description", DocType.Description);
            fields.Add("short_description", string.Empty);
            fields.Add("date_next_review", "1/1/2100");
            fields.Add("print_available", "1");
            fields.Add("email_available", "1");
            fields.Add("share_available", "1");
            if (DocType.LastModifiedDate.ToString() != string.Empty)
                fields.Add("date_last_modified", DocType.LastModifiedDate.ToString());
            else
                fields.Add("date_last_modified", string.Empty);
            DateTime dt = new DateTime(1753, 1, 1);
            if (DocType.FirstPublishedDate < dt)
            {
                fields.Add("date_first_published", dt.ToString());
            }
            else
            {
                fields.Add("date_first_published", DocType.FirstPublishedDate.ToString());
            }

            fields.Add("cdrid", DocType.DocumentID.ToString());
            fields.Add("summary_type", DocType.Type);
            fields.Add("audience", DocType.AudienceType);

            fields.Add("sys_title", prettyURLName);

            return fields;
        }

        private string GetTargetFolder(string targetFolderPath)
        {
            // Remove last part of path, e.g. /cancertopics/druginfo/methotrexate becomes /cancertopics/druginfo
            //Remove hostname and protocol.
            System.Uri URL = new Uri(targetFolderPath);
            targetFolderPath = URL.AbsolutePath;
            string truncUrl = targetFolderPath.Substring(0, targetFolderPath.LastIndexOf('/'));
            if (truncUrl != string.Empty)
            {
                return truncUrl;
            }
            return truncUrl;
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
    }
}
