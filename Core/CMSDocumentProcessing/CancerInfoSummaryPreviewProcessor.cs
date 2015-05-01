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
    /// This class is derived from the standard CancerInfoSummaryProcessorStandard. This method re-implements 
    /// the method ProcessDocument of the base class. This class in addition to processing the document into
    /// CMS also provides method to generate the html from the percussion system.
    /// </summary>
    public class CancerInfoSummaryPreviewProcessor : CancerInfoSummaryProcessorStandard, IDocumentProcessor, IDisposable
    {
        private Dictionary<string, string> mediaRenderedContentList = new Dictionary<string, string>();

        public CancerInfoSummaryPreviewProcessor(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        { }

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

            // identifier cannot be null. All the required content items should have 
            // been created before this step
            if (contentItemGuid != null)
            {
                cmsRenderedHTML = CMSController.Render(contentItemGuid,
                                        percussionConfig.PreviewSettings.PDQCancerSummaryTemplateName.Value,
                                        percussionConfig.PreviewSettings.PublishPreviewContextId.Value,
                                        percussionConfig.SiteId.Value,
                                        percussionConfig.PreviewSettings.ItemFilter.Value);
                                
            }
            else
            {
                throw new Exception(string.Format("The PercussionGuid contentItemGuid is null, this value cannot be null"));
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

        /// <summary>
        /// For Publish preview always return null, which signifies the document as not being present in CMS.
        /// so new document is always created.
        /// </summary>
        /// <param name="contentType">The document type</param>
        /// <param name="cdrID">The cdrid of the document.</param>
        /// <returns>Returns null, so a new document is alwasy created in new folder.</returns>
        public override PercussionGuid GetCdrDocumentID(string contentType, int cdrID)
        {
            return null;
        }
                
        public Dictionary<string, string> MediaRenderedContentList
        {
            get { return mediaRenderedContentList; }
        }

        //OCEPROJECT-1765 - Remove summary link dependency
        /// <summary>
        /// Updates the Navon which resides in the same folder as the summary root node.
        /// </summary>
        /// <param name="document">SummaryDocument object containing information about the
        /// summary being processed.</param>
        /// <param name="summaryRootItemID">Guid of document's Percussion content item.</param>
        /// <param name="path">The folder path containing the navon to be updated.</param>
        protected override void UpdateNavOn(SummaryDocument document, PercussionGuid summaryRootItemID, string path)
        {
            // For publish preview, we deliberately do nothing.
            // There is no need to set a nav label on preview.
        }

        /// <summary>
        /// Overrides DeleteContentItem to delete the entire content tree, not just the folder
        /// containing a single pdqCancerInfoSummary object.
        /// </summary>
        /// <param name="itemGuid"></param>
        public override void DeleteContentItem(PercussionGuid itemGuid)
        {
            // A null rootItem means the document has already been deleted.
            // No further work is required.
            if (itemGuid != null)
            {
               // PercussionGuid summaryLink = LocateExistingSummaryLink(itemGuid);

                // Get access to the PSFolderItem object for removing the folder and its content.
                PSItem[] rootItem = CMSController.LoadContentItems(new long[] { itemGuid.ID });

                // Get path from the first item of the the PSFolderItem of the rootItem folder collection.
                string folderPath = CMSController.GetPathInSite(rootItem[0]);
                string parentPath = GetParentFolder(folderPath);

                // Purge all the contents inside in the folder and remove  the folder.
                CMSController.DeleteFolder(parentPath, true);
            }

        }
    }
}
