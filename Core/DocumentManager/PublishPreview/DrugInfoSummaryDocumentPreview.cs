using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.DocumentObjects.Media;
using GKManagers.BusinessObjects;
using GateKeeper.ContentRendering;
using GateKeeper.DataAccess.CDR;
using GKManagers.CMSDocumentProcessing;
using GKManagers.CMSManager.Configuration;
using GateKeeper.Common.XPathKeys;

using NCI.WCM.CMSManager;
using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GKPreviews
{
    /// <summary>
    /// This class provides functions which process a summary document for preview purposes.
    /// </summary>
    public class DrugInfoSummaryDocumentPreview : DocumentPreviewBase
    {
        public DrugInfoSummaryDocumentPreview(XmlDocument documentData, string userName)
            : base(documentData, userName)
        {

        }

        /// <summary>
        /// A concrete implementation of the base class method of the same name. This
        /// method is used to preform all the pre and post processing steps of a document before 
        /// the document submitted to the processor.
        /// </summary>
        protected override void ProcessPreview(ref string contentHtml, ref string headerContent)
        {
            contentHtml = string.Empty;

            DrugInfoSummaryDocument drugInfoSummary = new DrugInfoSummaryDocument();
            DrugInfoSummaryPreviewExtractor extractor = new DrugInfoSummaryPreviewExtractor();

            // Extract drug info summary data
            extractor.Extract(DocumentData, drugInfoSummary, DocXPathManager);

            // Rendering drug info summary data
            DrugInfoSummaryRenderer render = new DrugInfoSummaryRenderer();
            render.Render(drugInfoSummary);

            // Any media links in the document should be processed.
            processMediaReferences(DocumentData);

            PercussionGuid contentItemGuid = null;

            // Save drug info summary data into the Percussion CMS.
            using (DrugInfoSummaryPreviewProcessor processor = new DrugInfoSummaryPreviewProcessor(WriteHistoryWarningEntry, WriteHistoryInformationEntry))
            {
                try
                {
                    // Create the content items in CMS from information in drugInfoSummary document
                    processor.ProcessDocument(drugInfoSummary, ref contentItemGuid);

                    // Generate the CMS HTML rendering of the content item
                    contentHtml = processor.ProcessCMSPreview(drugInfoSummary, contentItemGuid);

                    headerContent = createHeaderZoneContent(drugInfoSummary);
                }
                catch (Exception ex)
                {
                    throw new Exception(" Preview generation failed for document Id:" + drugInfoSummary.DocumentID.ToString(), ex);
                }
                finally
                {
                    ////// For Pub Preview the document will be removed from CMS once 
                    ////// the job of generating the preview html is complete.
                   processor.DeleteContentItem(contentItemGuid);
                }
            }
        }

        /// <summary>
        /// Create the preview audio media content item. This audio media content item is 
        /// used as subsitute for all media link in the DurginfoSummaryDocument for resolving inline slots. 
        /// For each media link in DIS a substitue media content item is created.
        /// </summary>
        private void processMediaReferences(XmlDocument documentData)
        {
            XmlDocument xmlMediaDoc = null;
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            List<int> mediaIds = new List<int>();

            // Find all Medialinks in the druginfosummary xml document. And for each media link 
            // create media document
            XPathNavigator xNav = documentData.CreateNavigator();
            string path = "//DrugInformationSummary/DrugInfoMetaData/PronunciationInfo/MediaLink";

            try
            {
                XPathNodeIterator mediaLinkIter = xNav.Select(path);

                // No media links found, no further processing required.
                if (mediaLinkIter.Count == 0)
                    return;

                while (mediaLinkIter.MoveNext())
                {
                    string tempMediaDocumentID = string.Empty;
                    int mediaDocumentID = 0;
                    tempMediaDocumentID = DocumentHelper.GetAttribute(mediaLinkIter.Current, DocXPathManager.GetXPath(GlossaryTermXPath.MediaRef));
                    if (!Int32.TryParse(CDRHelper.ExtractCDRID(tempMediaDocumentID), out mediaDocumentID))
                    {
                        throw new Exception("Failed to parse media document id from value: " + tempMediaDocumentID);
                    }


                    string mediaLinkID = DocumentHelper.GetAttribute(mediaLinkIter.Current, DocXPathManager.GetXPath(GlossaryTermXPath.MediaID));

                    // get the mime type
                    string type = DocumentHelper.GetAttribute(mediaLinkIter.Current, DocXPathManager.GetXPath(GlossaryTermXPath.MediaType));
                    type = string.IsNullOrEmpty(type) ? String.Empty : type;

                    mediaIds.Add(mediaDocumentID);

                }

                using (MediaProcessor processor = new MediaProcessor(WriteHistoryWarningEntry, WriteHistoryInformationEntry))
                {
                    // Create a media document only if it does not already exists in CMS
                    foreach (int mediaDocumentID in mediaIds)
                    {
                        if (processor.GetCdrDocumentID(percussionConfig.ContentType.PDQMedia.Value, mediaDocumentID) == null)
                        {
                            // Load Media XML from file
                            if (xmlMediaDoc == null)
                            {
                                xmlMediaDoc = new XmlDocument();
                                xmlMediaDoc.PreserveWhitespace = true;
                                xmlMediaDoc.Load(percussionConfig.PreviewSettings.PreviewAudioFilePath.Value);
                            }

                            MediaDocument media = new MediaDocument();

                            // Call Media Extractor
                            MediaExtractor.Extract(xmlMediaDoc, media, mediaDocumentID, DocXPathManager);

                            // Call Media Renderer
                            MediaRenderer mediaRender = new MediaRenderer();
                            mediaRender.Render(media);

                            // Call Media Processor
                            processor.ProcessDocument(media);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("processing media links failed in DIS preview", ex);
            }
        }

        private string createHeaderZoneContent(DrugInfoSummaryDocument document)
        {
            // Create the content header html content
            string html = string.Format("<div id=\"cgvcontentheader\"><div class=\"document-title-block\" style=\"background-color:#d4d9d9\" >" +
        "<img src=\"/PublishedContent/Images/SharedItems/ContentHeaders/{0}\" alt=\"\" style=\"border-width:0px;\" />" +
        "<h1>{1}</h1></div></div>", "title_druginfopills.jpg", "Cancer Drug Information");


            // Create the cgvLanguageDate html content
            html += "<div class=\"language-dates\"><div class=\"document-dates\">" +
                "<div class=\"slot-item only-SI\"><ul style=\"float:right;\">";

            if (document.FirstPublishedDate != DateTime.MinValue)
            {
                html += "<li><strong>Posted: </strong>" + String.Format("{0:MM/dd/yyyy}", document.FirstPublishedDate) + "</li>";
            }
            if (document.LastModifiedDate != DateTime.MinValue)
            {
                //if (drug.ReceivedDate != DateTime.MinValue && drug.ReceivedDate != null)
                html += "<li><strong>Updated: </strong>" + String.Format("{0:MM/dd/yyyy}", document.LastModifiedDate) + "</li>";
            }

            html += "</ul></div></div></div>";

            return html;
        }

    }
}
