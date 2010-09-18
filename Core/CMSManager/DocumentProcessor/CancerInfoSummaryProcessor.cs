using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;

namespace GKManagers.CMSManager.DocumentProcessor
{
    class CancerInfoSummaryProcessor : DocumentProcessorCommon, IDocumentProcessor
    {
        public CancerInfoSummaryProcessor(PercussionLoader percLoader,
            HistoryEntryWriter warningWriter,
            HistoryEntryWriter informationWriter)
            : base(percLoader, warningWriter, informationWriter)
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
            throw new NotImplementedException();

            // Get content item (Create new, or load existing)
            // Convert properties to CMS fields.
            // Map Relationships.
            // Store content item.

            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
        }

        #endregion
    }
}
