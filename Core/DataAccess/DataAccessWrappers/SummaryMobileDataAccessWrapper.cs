using System;

using GateKeeper.Common;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DocumentObjects;
using GKManagers.CMSDocumentProcessing;

namespace GateKeeper.DataAccess.DataAccessWrappers
{
    public class SummaryMobileDataAccessWrapper : DocumentDataAccess
    {
        /// <summary>
        /// Performs the steps to save a summary document to persistent storage in the Staging state.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public override void SaveDocument(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            // Save summary data into the Percussion CMS.
            using (CancerInfoSummaryProcessorMobile processor = new CancerInfoSummaryProcessorMobile(warningWriter, informationWriter))
            {
                processor.ProcessDocument(document, SitePathOverride);
            }

            // Save summary metadata into database
            using (SummaryQuery summaryQuery = new SummaryQuery())
            {
                summaryQuery.SaveDocument(document, username);
            }
        }

        /// <summary>
        /// Removes a summary document from the Staging state.  The document's availability in the Preview and Live states is unaffected.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public override void DeleteDocument(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            // Remove from Staging does nothing in Percussion.  This is by design.
            // Attempting to remove the document at this stage would
            // remove it from all stages.

            // Remove summary data from database
            using (SummaryQuery summaryQuery = new SummaryQuery())
            {
                summaryQuery.DeleteDocument(document, ContentDatabase.Staging, username);
            }
        }

        /// <summary>
        /// Moves a summary document to the Preview workflow state.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public override void PromoteToPreview(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            // Save summary data into the Percussion CMS.
            using (CancerInfoSummaryProcessorMobile processor = new CancerInfoSummaryProcessorMobile(warningWriter, informationWriter))
            {
                processor.PromoteToPreview(document.DocumentID, SitePathOverride);
            }

            // Push summary metadata to the preview database
            using (SummaryQuery summaryQuery = new SummaryQuery())
            {
                summaryQuery.PushDocumentToPreview(document, username);
            }
        }

        /// <summary>
        /// Removes a summary document from the Preview state.  The document's availability in the Staging and Live states is unaffected.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public override void RemoveFromPreview(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            // Remove from Preview does nothing in Percussion.  This is by design.
            // Attempting to remove the document at this stage would it from all stages.

            // Remove summary data from database
            using (SummaryQuery summaryQuery = new SummaryQuery())
            {
                summaryQuery.DeleteDocument(document, ContentDatabase.Preview, username);
            }
        }

        /// <summary>
        /// Moves a summary document to the Live workflow state.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public override void PromoteToLive(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            // Save summary data into the Percussion CMS.
            using (CancerInfoSummaryProcessorMobile processor = new CancerInfoSummaryProcessorMobile(warningWriter, informationWriter))
            {
                processor.PromoteToLive(document.DocumentID, SitePathOverride);
            }
            // Push summary metadata to the live database
            using (SummaryQuery summaryQuery = new SummaryQuery())
            {
                summaryQuery.PushDocumentToLive(document, username);
            }
        }

        /// <summary>
        /// Removes a summary document from the Live state.  The document's availability in the Staging and Preview states is unaffected.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public override void RemoveFromLive(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            using (CancerInfoSummaryProcessorMobile processor = new CancerInfoSummaryProcessorMobile(warningWriter, informationWriter))
            {
                processor.DeleteContentItem(document.DocumentID, SitePathOverride);
            }

            // Remove summary data from database
            using (SummaryQuery summaryQuery = new SummaryQuery())
            {
                summaryQuery.DeleteDocument(document, ContentDatabase.Live, username);
            }
        }
    }
}
