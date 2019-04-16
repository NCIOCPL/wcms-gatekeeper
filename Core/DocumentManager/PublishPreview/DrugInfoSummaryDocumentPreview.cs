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
            throw new NotImplementedException();
        }
    }
}
