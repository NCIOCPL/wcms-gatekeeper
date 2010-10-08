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
using GKManagers.CMSManager.Configuration;


namespace GKManagers.CMSManager.DocumentProcessing
{
    public class CancerInfoSummaryProcessor : DocumentProcessorCommon, IDocumentProcessor, IDisposable
    {
        public static PercussionConfig percussionConfig;
        public CancerInfoSummaryProcessor(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        {
                        percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

        }
        #region IDocumentProcessor Members

        /// <summary>
        /// Main entry point for processing a Cancer Information Summary (formerly just "Summary")
        /// object which is to be managed in the CMS.
        /// </summary>
        /// <param name="documentObject"></param>
        public void ProcessDocument(Document documentObject)
        {
            List<CreateContentItem> contentItemList = new List<CreateContentItem>();
            List<long> idList;

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
                CreatePDQCancerInfoSummary(document, mappingInfo, contentItemList);

                //TODO: Need to handle pre-existing summary link for Patient vs. Health Professional.
                
                //Create new pdqCancerInfoSummaryLink content item
                CreatePDQCancerInfoSummaryLink(document, contentItemList);

                //Create new pdqTableSections content item
                CreatePDQTableSections(document, contentItemList);

                //Create new pdqCancerInfoSummaryPage content item
                CreatePDQCancerInfoSummaryPage(document, contentItemList);
                
                //Save pdqMediaLink

                //TODO:  Set up Percussion slot relationships.

                //Save all the content items in one operation using the contentItemList.
                idList = CMSController.CreateContentItemList(contentItemList);

                
                // Map Relationships.
                //Save the mapping between the CDR and CMS IDs.As the mapping is to be saved only for the pdqCancerInfoSummary just pick
                //the first Id "idList[0]" from the idList to save.
                mappingInfo = new CMSIDMapping(document.DocumentID, idList[0], document.BasePrettyURL);
                mapManager.InsertCdrIDMapping(mappingInfo);

            }

            else
            {
                //Update Content Items

                List<UpdateContentItem> contentItemsListToUpdate = new List<UpdateContentItem>();
                long contentID;
                // Add pdqCancerInfoSummary content item to the contentItemsListToUpdate 
                UpdateContentItem updateContentItem = new UpdateContentItem(mappingInfo.CmsID, GetFieldsPDQCancerInfoSummary(document), GetTargetFolder(document.PrettyURL));
                contentItemsListToUpdate.Add(updateContentItem);


                //Add pdqCancerInfoSummaryLink content item to the contentItemsListToUpdate
                
                //Get the ID for the content item to be updated.
                contentID = GetpdqCancerInfoSummaryLinkID(document);

                updateContentItem = new UpdateContentItem(contentID, GetFieldsPDQCancerInfoSummaryLink(document), GetTargetFolder(document.PrettyURL));
                contentItemsListToUpdate.Add(updateContentItem);

                //Add pdqTableSections content item to the contentItemsListToUpdate
                GetPDQTableSectionsToUpdate(document, contentItemsListToUpdate);

                //Add pdqCancerInfoSummaryPages content item to the contentItemsListToUpdate
                GetPDQCancerInfoSummaryPagesToUpdate(document, contentItemsListToUpdate);


                InformationWriter(string.Format("Updating document CDRID = {0} in Percussion system.", document.DocumentID));               
                
                //Update all the content Item in one operation
                idList = CMSController.UpdateContentItemList(contentItemsListToUpdate);

                //Check if the pdqCancerInfoSummary Pretty URL changed if yes then move the content item to the new folder in percussion.
                string prettyURL=GetTargetFolder(document.BasePrettyURL);
                if (mappingInfo.PrettyURL != prettyURL)
                {
                    long[] id = idList.ToArray();
                    CMSController.GuaranteeFolder(prettyURL);
                    CMSController.MoveContentItemFolder(mappingInfo.PrettyURL, prettyURL, id);

                    //Delete existing mapping for the CDRID.
                    mapManager.DeleteCdrIDMapping(document.DocumentID);

                    // Save the mapping between the CDR and CMS IDs.
                    mappingInfo = new CMSIDMapping(document.DocumentID, idList[0], document.PrettyURL);
                    mapManager.InsertCdrIDMapping(mappingInfo);

                }

            }

            // Get content item (Create new, or load existing)


            
            
            // Store content item.

            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
        }


        /// <summary>
        /// Deletes the content items representing the speicified Cancer Information Summary document.
        /// </summary>
        /// <param name="documentID">The document ID.</param>
        public void DeleteContentItem(int documentID)
        {
            IDMapManager mapManager = new IDMapManager();
            CMSIDMapping mappingInfo = mapManager.LoadCdrIDMappingByCdrid(documentID);

            // Check for items with references.
            VerifyDocumentMayBeDeleted(documentID);

            throw new NotImplementedException();

        }

        /// <summary>
        /// Verifies that a document object has no incoming refernces. Throws CMSCannotDeleteException
        /// if the document is the target of any incoming relationships.
        /// </summary>
        /// <param name="documentCmsID">The document's ID in the CMS.</param>
        protected override void VerifyDocumentMayBeDeleted(long documentCmsID)
        {
            throw new NotImplementedException();
        }



        #endregion
        
        #region Private Methods

        private void CreatePDQCancerInfoSummaryPage(SummaryDocument document, List<CreateContentItem> contentItemList)
        {
            int i;

                for (i = 0; i <= document.SectionList.Count - 1; i++)
                {
                    if (document.SectionList[i].IsTopLevel == true)
                    {
                        CreateContentItem contentItem = new CreateContentItem(GetFieldsPDQCancerInfoSummaryPage(document.SectionList[i]), GetTargetFolder(document.BasePrettyURL), percussionConfig.ContentType.PDQCancerInfoSummaryPage.Value);
                        contentItemList.Add(contentItem);
                    }

                }

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

            fields.Add("bodyfield", html.Replace("<MediaHTML>", string.Empty).Replace("</MediaHTML>", string.Empty).Replace("<TableSectionXML>",string.Empty).Replace("</TableSectionXML>",string.Empty));
            fields.Add("sys_title", cancerInfoSummaryPage.Title);


            return fields;
        }


        private void CreatePDQTableSections(SummaryDocument document, List<CreateContentItem> contentItemList)
        {
            int i;


            for (i = 0; i <= document.TableSectionList.Count-1;i++ )
            {
                CreateContentItem contentItem = new CreateContentItem(GetFieldsPDQTableSection(document.TableSectionList[i]), GetTargetFolder(document.BasePrettyURL), percussionConfig.ContentType.PDQTableSection.Value);
                contentItemList.Add(contentItem);
                
            }

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



        private void CreatePDQCancerInfoSummary(SummaryDocument document, CMSIDMapping mappingInfo, List<CreateContentItem> contentItemList)
        {

            CreateContentItem contentItem = new CreateContentItem(GetFieldsPDQCancerInfoSummary(document), GetTargetFolder(document.BasePrettyURL),percussionConfig.ContentType.PDQCancerInfoSummary.Value);
            contentItemList.Add(contentItem);

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


        private void CreatePDQCancerInfoSummaryLink(SummaryDocument document, List<CreateContentItem> contentItemList)
        {

            CreateContentItem contentItem = new CreateContentItem(GetFieldsPDQCancerInfoSummaryLink(document), GetTargetFolder(document.BasePrettyURL),percussionConfig.ContentType.PDQCancerInfoSummaryLink.Value);
            contentItemList.Add(contentItem);

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

        private long GetpdqCancerInfoSummaryLinkID(SummaryDocument document)
        {
            long contentid;
            contentid = CMSController.GetItemID(GetTargetFolder(document.BasePrettyURL), document.Title);
            return contentid;
        }


        private void GetPDQTableSectionsToUpdate(SummaryDocument document, List<UpdateContentItem> contentItemsListToUpdate)
        {
            int i;
            long contentid;


            for (i = 0; i <= document.TableSectionList.Count - 1; i++)
            {
                string prettyURLName = document.TableSectionList[i].PrettyUrl.Substring(document.TableSectionList[i].PrettyUrl.LastIndexOf('/') + 1);
                contentid = CMSController.GetItemID(GetTargetFolder(document.TableSectionList[i].PrettyUrl), prettyURLName);
                UpdateContentItem updateContentItem = new UpdateContentItem(contentid, GetFieldsPDQTableSection(document.TableSectionList[i]), GetTargetFolder(document.PrettyURL));
                contentItemsListToUpdate.Add(updateContentItem);
            }

        }

        private void GetPDQCancerInfoSummaryPagesToUpdate(SummaryDocument document, List<UpdateContentItem> contentItemsListToUpdate)
        {
            int i;
            long contentid;

            for (i = 0; i <= document.SectionList.Count - 1; i++)
            {
                string prettyURLName = document.SectionList[i].PrettyUrl.Substring(document.SectionList[i].PrettyUrl.LastIndexOf('/') + 1);
                contentid = CMSController.GetItemID(GetTargetFolder(document.SectionList[i].PrettyUrl), prettyURLName);
                UpdateContentItem updateContentItem = new UpdateContentItem(contentid, GetFieldsPDQTableSection(document.SectionList[i]), GetTargetFolder(document.PrettyURL));
                contentItemsListToUpdate.Add(updateContentItem);

            }

        }


        private string GetTargetFolder(string targetFolderPath)
        {
            // Remove the jost name and protocol
            if (targetFolderPath.ToLower().StartsWith("http"))
            {
                //Remove hostname and protocol.
                System.Uri URL = new Uri(targetFolderPath);
                targetFolderPath = URL.AbsolutePath;
                return targetFolderPath;
            }

            else
            {
                return targetFolderPath;

            }
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
    }
}
