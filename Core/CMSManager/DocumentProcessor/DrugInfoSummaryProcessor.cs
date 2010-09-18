using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.DrugInfoSummary;

namespace GKManagers.CMSManager.DocumentProcessor
{
    class DrugInfoSummaryProcessor : DocumentProcessorCommon, IDocumentProcessor
    {
        public DrugInfoSummaryProcessor(PercussionLoader percLoader,
            HistoryEntryWriter warningWriter,
            HistoryEntryWriter informationWriter)
            : base(percLoader, warningWriter, informationWriter)
        {
        }

        #region IDocumentProcessor Members

        /// <summary>
        /// Main entry point for processing a DrugInfoSummary object which is to be
        /// managed in the CMS.
        /// </summary>
        /// <param name="documentObject"></param>
        public void ProcessDocument(Document documentObject)
        {
            VerifyRequiredDocumentType(documentObject, DocumentType.DrugInfoSummary);

            DrugInfoSummaryDocument document = documentObject as DrugInfoSummaryDocument;

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
