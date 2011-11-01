using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using GKManagers.CMSManager.Configuration;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.DocumentObjects.Media;
using GKManagers.CMSDocumentProcessing;

using NCI.WCM.CMSManager;
using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSDocumentProcessing
{
    /// <summary>
    /// This class is derived from the standard DrugInfoSummaryProcessor. This method re-implements 
    /// the method ProcessDocument of the base class. This class in addition to processing the document into
    /// CMS also provides method to generate the html from the percussion system.
    /// </summary>
    public class DrugInfoSummaryPreviewProcessor : DrugInfoSummaryProcessor, IDocumentProcessor, IDisposable
    {
        public DrugInfoSummaryPreviewProcessor(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        { }


        /// <summary>
        /// This method generates the document HTML from the percussion system.  
        /// </summary>
        /// <returns>A string which is the html rendering of the content item in CMS</returns>
        public string ProcessCMSPreview(Document documentObject)
        {
            string cmsRenderedHTML = string.Empty;

            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

            // Create the preview url needed to produce the document html. To do this 
            // 1. Read the content id
            // 2. Know the template id of the content type 

            // Even though the content item is created, trying to find the newly created content item 
            // using GetCdrDocumentID returns null. This is problem in publish preview.
            // so the content item guid CurrentContentItem is stored during create in the 
            // base class . This property is used only publish preview derived class.
            PercussionGuid identifier = CurrentContentItem;
            if (identifier == null)
                identifier = GetCdrDocumentID(DrugInfoSummaryContentType, documentObject.DocumentID);

            if (identifier != null)
            {
                cmsRenderedHTML = CMSController.Render(identifier,
                                        percussionConfig.PreviewSettings.PDQDrugInfoSummaryTemplateName.Value,
                                        percussionConfig.PreviewSettings.PublishPreviewContextId.Value,
                                        percussionConfig.SiteId.Value,
                                        percussionConfig.PreviewSettings.ItemFilter.Value);
            }
            else
            {
                DrugInfoSummaryDocument document = documentObject as DrugInfoSummaryDocument;
                string targetPathName = GetTargetFolder(document.PrettyURL);
                string prettyUrlName = GetPrettyUrlName(document.PrettyURL);
                throw new Exception(string.Format("Document does not exists at path {0}/{1}.", targetPathName, prettyUrlName));
            }
            return cmsRenderedHTML;
        }

    }
}
