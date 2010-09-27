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



            // Get content item (Create new, or load existing)
            // Convert properties to CMS fields.
            // Map Relationships.
            // Store content item.

            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
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
