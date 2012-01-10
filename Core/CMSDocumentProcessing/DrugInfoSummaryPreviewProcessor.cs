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
        public string ProcessCMSPreview(Document documentObject, PercussionGuid contentItemGuid)
        {
            string cmsRenderedHTML = string.Empty;

            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

            PercussionGuid identifier = contentItemGuid;

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
                throw new Exception(string.Format("The PercussionGuid contentItemGuid is null, this value cannot be null"));
            }
            return cmsRenderedHTML;
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
    }
}
