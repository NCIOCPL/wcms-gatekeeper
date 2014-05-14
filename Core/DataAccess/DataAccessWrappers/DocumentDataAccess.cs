using System;
using System.Xml.Serialization;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;


namespace GateKeeper.DataAccess.DataAccessWrappers
{
    [XmlInclude(typeof(SummaryStandardDataAccessWrapper))]
    [XmlInclude(typeof(SummaryMobileDataAccessWrapper))]
    public abstract class DocumentDataAccess
    {
        /// <summary>
        /// Overrides the base path for storing content items in the CMS.
        /// </summary>
        /// <value>The site path override.</value>
        [XmlAttribute("SitePath")]
        public string SitePathOverride { get; set; }

        /// <summary>
        /// Performs the steps to save a Document to persistent storage in the Staging state.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public abstract void SaveDocument(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter);

        /// <summary>
        /// Removes a Document from the Staging state.  The document's availability in the Preview and Live states is unaffected.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public abstract void DeleteDocument(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter);

        /// <summary>
        /// Moves a document to the Preview workflow state.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public abstract void PromoteToPreview(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter);

        /// <summary>
        /// Removes a document from the Preview state.  The document's availability in the Staging and Live states is unaffected.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public abstract void RemoveFromPreview(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter);

        /// <summary>
        /// Moves a document to the Live workflow state.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public abstract void PromoteToLive(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter);

        /// <summary>
        /// Removes a document from the Live state.  The document's availability in the Staging and Preview states is unaffected.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public abstract void RemoveFromLive(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter);

        /// <summary>
        /// Moves a document to the Live workflow state.
        /// </summary>
        /// <param name="document">An object derived from Document to be stored.</param>
        /// <param name="username">String containing the name of the user who initiated the operation.</param>
        /// <param name="warningWriter">A HistoryEntryWriter for recording warning messages.</param>
        /// <param name="informationWriter">A HistoryEntryWriter for recording informational messages.</param>
        /// <remarks>Errors result in System.Exception being thrown. The calling routine is responsible for
        /// catching the exception and recording it.</remarks>
        public abstract void PromoteToLiveFast(Document document, string username, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter);

    }
}
