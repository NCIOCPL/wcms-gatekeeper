using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.BusinessObjects;
using GateKeeper.ContentRendering;
using GateKeeper.DataAccess.CDR;
using GKManagers.CMSDocumentProcessing;
using NCI.WCM.CMSManager.CMS;

namespace GKPreviews
{
    /// <summary>
    /// This class provides functions which process a summary document for preview purposes.
    /// </summary>
    public class SummaryDocumentPreview : DocumentPreviewBase
    {
        public SummaryDocumentPreview(XmlDocument documentData, string userName)
            : base(documentData, userName)
        {

        }

        /// <summary>
        /// A concrete implementation of the base class method of the same name. This
        /// method is used to preform all the pre and post processing steps of a document before 
        /// the document submitted to the preview processor.
        /// </summary>
        protected override void ProcessPreview(ref string contentHtml, ref string headerContent)
        {
            throw new NotImplementedException();
        }

    }

}
