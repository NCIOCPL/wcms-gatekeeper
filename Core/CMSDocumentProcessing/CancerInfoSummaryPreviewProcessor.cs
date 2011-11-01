using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Configuration;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.DocumentObjects.Media;
using GKManagers.CMSManager.Configuration;

using NCI.WCM.CMSManager;
using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSDocumentProcessing
{
    /// <summary>
    /// This class is derived from the standard CancerInfoSummaryProcessor. This method re-implements 
    /// the method ProcessDocument of the base class. This class in addition to processing the document into
    /// CMS also provides method to generate the html from the percussion system.
    /// </summary>
    public class CancerInfoSummaryPreviewProcessor : CancerInfoSummaryProcessor, IDocumentProcessor, IDisposable
    {
        private Dictionary<string, string> mediaRenderedContentList = new Dictionary<string, string>();

        public CancerInfoSummaryPreviewProcessor(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        { }

        ///// <summary>
        ///// Main entry point for processing a Cancer Information Summary (formerly just "Summary")
        ///// object which is to be managed in the CMS.
        ///// </summary>
        ///// <param name="documentObject"></param>
        //public new void ProcessDocument(Document documentObject)
        //{
        //    VerifyRequiredDocumentType(documentObject, DocumentType.Summary);

        //    SummaryDocument document = documentObject as SummaryDocument;

        //    InformationWriter(string.Format("Begin Percussion processing for document CDRID = {0}.", document.DocumentID));

        //    // Are we updating an existing document? Or saving a new one?
        //    identifier = GetCdrDocumentID(CancerInfoSummaryContentType, document.DocumentID);

        //    // No mapping found, this is a new item.
        //    if (identifier == null)
        //    {
        //        InformationWriter(string.Format("Create new content item for document CDRID = {0}.", document.DocumentID));
        //        CreateNewCancerInformationSummary(document);
        //    }
        //}

        /// <summary>
        /// This method generates the document HTML from the percussion system.  
        /// </summary>
        /// <returns>A string which is the html rendering of the content item in CMS</returns>
        public string ProcessCMSPreview(Document documentObject, PercussionGuid contentItemGuid)
        {
            string cmsRenderedHTML = string.Empty;

            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

            // Create the preview url needed to produce the document html. To do this 
            // 1. Read the content id
            // 2. Know the template id of the content type 
            if (contentItemGuid == null)
                contentItemGuid = GetCdrDocumentID(CancerInfoSummaryContentType, documentObject.DocumentID);

            // identifier cannot be null. All the required content items should have 
            // been created before this step
            if (contentItemGuid != null)
            {
                cmsRenderedHTML = CMSController.Render(contentItemGuid,
                                        percussionConfig.PreviewSettings.PDQCancerSummaryTemplateName.Value,
                                        percussionConfig.PreviewSettings.PublishPreviewContextId.Value,
                                        percussionConfig.SiteId.Value,
                                        percussionConfig.PreviewSettings.ItemFilter.Value);

                string perviewMediaPath = percussionConfig.PreviewSettings.PreviewImageContentLocation.Value;

                // For medialinks generate the required media output to the file system.
                foreach (MediaLink mediaLink in ((SummaryDocument)documentObject).MediaLinkSectionList)
                {
                    PercussionGuid mediaItemID = MediaLinkIDMap[mediaLink.Id];

                    string mediaHtml = CMSController.Render(mediaItemID,
                                            percussionConfig.PreviewSettings.PDQImageTemplateName.Value,
                                            percussionConfig.PreviewSettings.PublishPreviewContextId.Value,
                                            percussionConfig.SiteId.Value,
                                            percussionConfig.PreviewSettings.ItemFilter.Value);

                    mediaHtml = mediaHtml.Replace("/images/spacer.gif", ConfigurationManager.AppSettings["ServerURL"] + "/images/spacer.gif");
                    mediaHtml = mediaHtml.Replace("/stylesheets", ConfigurationManager.AppSettings["ServerURL"] + "/stylesheets");

                    // For Image media the image url references should be pointing to CDR server
                    string imagePath = ConfigurationManager.AppSettings["ImageLocation"];
                    mediaHtml = mediaHtml.Replace("/images/cdr/live/", imagePath);

                    string mediaContentFileName = mediaItemID.ID.ToString() + ".html";

                    mediaRenderedContentList.Add(mediaItemID.ID.ToString(), mediaHtml);

                    using (StreamWriter outfile = new StreamWriter(perviewMediaPath + @"\" + mediaContentFileName))
                    {
                        outfile.Write(mediaHtml);
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Cannot find document in CMS for CDRID ID {0}:", documentObject.DocumentID));
            }

            return cmsRenderedHTML;
        }
         
        /// <summary>
        /// Override the base class VerifyEnglishLanguageVersion. For pubish preview there is no need
        /// to validate the existence of an english version.
        /// </summary>
        /// <param name="summary"></param>
        protected override void VerifyEnglishLanguageVersion(SummaryDocument summary)
        {
            // We do not have to check for the existence of english versio of a spanish document.
            // Because the english version will never exist durin pub preview.
        }

        /// <summary>
        /// Override the base class LinkToAlternateLanguageVersion. For pubish preview there is no need
        /// to validate the existence of an english version.        
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="rootID"></param>
        protected override void LinkToAlternateLanguageVersion(SummaryDocument summary, PercussionGuid rootID)
        {  
            // We do not have to check for the existence of english versio of a spanish document.
            // Because the english version will never exist durin pub preview.
        }

        public Dictionary<string, string> MediaRenderedContentList
        {
            get { return mediaRenderedContentList; }
        }
    }
}
