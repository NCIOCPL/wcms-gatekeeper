using System;
using System.Collections.Generic;
using System.Linq;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.CMSManager.CMS;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;


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
                
                // Create the new pdqCancerInfoSummary content item.
                CreatePDQCancerInfoSummary(document, mappingInfo);
                //Create new pdqCancerInfoSummaryLink
                CreatePDQCancerInfoSummaryLink(document);

                //Create new pdqTableSections
                CreatePDQTableSections(document);

                //Create new pdqCancerInfoSummaryPage
                CreatePDQCancerInfoSummaryPage(document);

                //Save pdqMediaLink


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

        private Dictionary<string, string> GetFieldsPDQCancerInfoSummaryPage(SummarySection cancerInfoSummaryPage)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string html = cancerInfoSummaryPage.Html.OuterXml;

            string prettyURLName = cancerInfoSummaryPage.PrettyUrl.Substring(cancerInfoSummaryPage.PrettyUrl.LastIndexOf('/') + 1);
            if (cancerInfoSummaryPage.Html.OuterXml.Contains("<SummaryRef"))
            {
                BuildSummaryRefLink(ref html,0);
            }

            if (cancerInfoSummaryPage.Html.OuterXml.Contains("Summary-GlossaryTermRef"))
            {
                string glossaryTermTag = "Summary-GlossaryTermRef";
                BuildGlossaryTermRefLink(ref html, glossaryTermTag);
            }

            fields.Add("bodyfield", html);
            fields.Add("sys_title", prettyURLName);


            return fields;
        }


        private void CreatePDQTableSections(SummaryDocument document)
        {
            int i;
            List<long> idList;
            List<CreateContentItem> contentItemList = new List<CreateContentItem>();


            for (i = 0; i <= document.TableSectionList.Count-1;i++ )
            {
                CreateContentItem contentItem = new CreateContentItem(GetFieldsPDQTableSection(document.TableSectionList[i]), GetTargetFolder(document.BasePrettyURL));
                contentItemList.Add(contentItem);
                
            }

            // Create the new content item. (All items are created of the same type.)
            InformationWriter(string.Format("Adding document CDRID = {0} to Percussion system.", document.DocumentID));
            idList = CMSController.CreateContentItemList("pdqTableSection", contentItemList);

        }

        private Dictionary<string, string> GetFieldsPDQTableSection(SummarySection tableSection)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string prettyURLName = tableSection.PrettyUrl.Substring(tableSection.PrettyUrl.LastIndexOf('/') + 1);

            fields.Add("pretty_url_name", prettyURLName);
            fields.Add("bodyfield", tableSection.Html.InnerXml.Replace("<MediaHTML>", string.Empty).Replace("</MediaHTML>", string.Empty));
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


        private void CreatePDQCancerInfoSummaryLink(SummaryDocument document)
        {
            List<long> idList;

            // Turn the list of item fields into a list of one item.
            CreateContentItem contentItem = new CreateContentItem(GetFieldsPDQCancerInfoSummaryLink(document), GetTargetFolder(document.BasePrettyURL));
            List<CreateContentItem> contentItemList = new List<CreateContentItem>();
            contentItemList.Add(contentItem);


            // Create the new content item. (All items are created of the same type.)
            InformationWriter(string.Format("Adding document CDRID = {0} to Percussion system.", document.DocumentID));
            idList = CMSController.CreateContentItemList("pdqCancerInfoSummaryLink", contentItemList);

        }

        private Dictionary<string, string> GetFieldsPDQCancerInfoSummaryLink(SummaryDocument DocType)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();

            fields.Add("sys_title", DocType.Title);
            fields.Add("Long_title", DocType.Title);
            fields.Add("short_title", DocType.ShortTitle);
            fields.Add("long_description", DocType.Description);

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





        private void BuildSummaryRefLink(ref string html, int isGlossary)
        {
            string startTag = "<SummaryRef";
            string endTag = "</SummaryRef>";
            int startIndex = html.IndexOf(startTag, 0);
            string sectionHTML = html;
            while (startIndex >= 0)
            {
                // Devide the whole piece of string into three parts: a= first part; b = "<summaryref href="CDR0012342" url="/cander_topic/...HP/>..</summaryref>"; c = last part
                int endIndex = sectionHTML.IndexOf(endTag) + endTag.Length;
                string partA = sectionHTML.Substring(0, startIndex);
                string partB = sectionHTML.Substring(startIndex, endIndex - startIndex);
                string partC = sectionHTML.Substring(endIndex);

                // Process partB
                // Get the href, url, text between the tag
                XmlDocument refDoc = new XmlDocument();
                refDoc.LoadXml(partB);
                XPathNavigator xNav = refDoc.CreateNavigator();
                XPathNavigator link = xNav.SelectSingleNode("/SummaryRef");
                string text = link.InnerXml;
                string href = link.GetAttribute("href", string.Empty);
                string url = link.GetAttribute("url", string.Empty).Trim();
                if (url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1);
                }

                // The following code is preserved just in case in the future we need to support
                // prettyURL links in CDRPreview web service.
                // Get prettyurl server if the PrettyURLController is not reside on the same server
                // This is used for CDRPreview web service, GateKeeper should not have this setting.
                // string prettyURLServer = ConfigurationManager.AppSettings["PrettyUrlServer"];
                //if (prettyURLServer != null && prettyURLServer.Trim().Length > 0)
                //    url = prettyURLServer + url;

                // Get the section ID in href
                int index = href.IndexOf("#");
                string sectionID = string.Empty;
                string prettyURL = url;
                if (index > 0)
                {
                    sectionID = href.Substring(index + 2);
                    prettyURL = url + "/" + sectionID + ".cdr#Section_" + sectionID;
                }

                //Create new link string
                if (prettyURL.Trim().Length > 0)
                {
                    // The click on the summary link in the GlossaryTerm will open a new browser for summary document
                    if (isGlossary == 1)
                        partB = "<a class=\"SummaryRef\" href=\"" + prettyURL + "\" target=\"new\">" + text + "</a>";
                    else
                        partB = "<a class=\"SummaryRef\" href=\"" + prettyURL + "\">" + text + "</a>";
                }
                else
                {
                    throw new Exception("Retrieving SummaryRef url failed. SummaryRef=" + partB + ".");
                }

                // Combine
                // Do not add extra space before the SummaryRef if following sign is lead before the link: ({[ or open ' "
                if (Regex.IsMatch(partA.Trim(), "[({[/]$|[({[\\s]\'$|[({[\\s]\"$"))
                    sectionHTML = partA.Trim() + partB;
                else
                    sectionHTML = partA.Trim() + " " + partB;

                // Do not add extra space after the SummaryRef if following sign
                // is after the SummaryRef )}].,:;? " with )}].,:;? or space after it, ' with )]}.,:;? or space after it.
                if (Regex.IsMatch(partC.Trim(), "^[).,:;!?}]|^]|^\"[).,:;!?}\\s]|^\'[).,:;!?}\\s]|^\"]|^\']"))
                    sectionHTML += partC.Trim();
                else
                    sectionHTML += " " + partC.Trim();

                startIndex = sectionHTML.IndexOf(startTag, 0);
            }
            html = sectionHTML;
        }

        // <summary>
        /// Taking care of the spaces around GlossaryTermRefLink
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        public void BuildGlossaryTermRefLink(ref string html, string tag)
        {
            string startTag = "<a Class=\"" + tag + "\"";
            string endTag = "</a>";
            int startIndex = html.IndexOf(startTag, 0);
            string sectionHTML = html;
            string collectHTML = string.Empty;
            string partC = string.Empty;
            while (startIndex >= 0)
            {
                string partA = sectionHTML.Substring(0, startIndex);
                string left = sectionHTML.Substring(startIndex);
                int endIndex = left.IndexOf(endTag) + endTag.Length;
                string partB = left.Substring(0, endIndex);
                partC = left.Substring(endIndex);

                // Combine
                // Do not add extra space after the GlossaryTermRef if following sign
                // is after the SummaryRef )}].,:;? " with )}].,:;? or space after it, ' with )]}.,:;? or space after it.
                if (Regex.IsMatch(partA.Trim(), "^[).,:;!?}]|^]|^\"[).,:;!?}\\s]|^\'[).,:;!?}\\s]|^\"]|^\']") || collectHTML.Length == 0)
                    collectHTML += partA.Trim();
                else
                    collectHTML += " " + partA.Trim();

                // Do not add extra space before the GlossaryTermRef if following sign is lead before the link: ({[ or open ' "
                if (Regex.IsMatch(collectHTML, "[({[/]$|[({[\\s]\'$|[({[\\s]\"$"))
                    collectHTML += partB;
                else
                    collectHTML += " " + partB;

                sectionHTML = partC.Trim();
                startIndex = sectionHTML.IndexOf(startTag, 0);
            }
            html = collectHTML + partC;
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
