using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GKManagers.CMSManager.CMS;

namespace GKManagers.CMSManager
{
    /// <summary>
    /// Abstract base class to implement the functionality shared between
    /// all DocumentProcessors.
    /// </summary>
    public abstract class DocumentProcessorCommon
    {
        #region Properties

        // Percussion control object, shared among all Document Processor types
        // derived from DocumentProcessorCommon
        protected readonly CMSController CMSController = new CMSController();

        /// <summary>
        /// Delegate for writing warnings about potential problems encountered during document processing.
        /// Generally used for non-fatal errors.  Errors which cannot handles are reported by throwing
        /// an exception from the CMSException family. (See Exceptions.cs)
        /// </summary>
        protected HistoryEntryWriter WarningWriter { get; private set; }

        /// <summary>
        /// Delegate for writing informational messages about document processing.
        /// Generally used for reporting progress in the processings steps.
        /// (e.g. "Creating Content Item", "Saving Content Item", "Promoting Content Item to Preview.", etc.)
        /// </summary>
        protected HistoryEntryWriter InformationWriter { get; private set; }

        #endregion


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="percLoader">The instance of PercussionLoader which will be used by
        /// the concrete DocumentProcessor to manipulate the Percussion CMS.</param>
        public DocumentProcessorCommon(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            WarningWriter = warningWriter;
            InformationWriter = informationWriter;
        }

        /// <summary>
        /// Verifies that a document object contains an expected document type.
        /// </summary>
        /// <param name="pdqDocument">A document object to be tested for its concrete document type</param>
        /// <param name="expectedDocType">Enumerated value representing the expected document type.</param>
        protected void VerifyRequiredDocumentType(Document pdqDocument, DocumentType expectedDocType)
        {
            if (pdqDocument.DocumentType!= expectedDocType)
            {
                throw new CMSManagerIncorrectDocumentTypeException(string.Format("Incorrect DocumentType encountered. Expected {0}, found {1}.",
                    expectedDocType, pdqDocument.DocumentType));
            }
        }
    }
}
