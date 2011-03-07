using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.BusinessObjects
{
    public class RequestLocationExternalIds : RequestLocationBase
    {
        private string _gatekeeperRequestId;
        private string _stagingRequestId;
        private string _previewRequestId;
        private string _liveRequestId;

        #region Constructor

        /// <summary>
        /// The GateKeeper, Staging, Preview and Live databases potentially have different versions
        /// of the the same document.  A RequestLocationInternalIds object contains identifying information
        /// about the requests that each version came from.
        /// </summary>
        /// <param name="cdrId">CDR Document ID</param>
        /// <param name="gatekeeperRequestId">Native ID for the request containing the document version
        /// which resides in the GateKeeper database.  Contains -1 if the document has been deleted.</param>
        /// <param name="gatekeeperDate">Date and Time when the document was added to or deleted from
        /// the GateKeeper database.</param>
        /// <param name="stagingRequestId">Native ID for the request containing the document version
        /// which resides in the Staging database.  Contains -1 if the document has been deleted or
        /// was never promoted to this level.</param>
        /// <param name="stagingDate">Date and Time when the document was promoted to or deleted from
        /// the GateKeeper database.  Contains DateTime.MinValue if the document has never been promoted
        /// to this database.</param>
        /// <param name="previewRequestId">Native ID for the request containing the document version
        /// which resides in the Preview database.  Contains -1 if the document has been deleted or
        /// was never promoted to this level.</param>
        /// <param name="previewDate">Date and Time when the document was promoted to or deleted from
        /// the GateKeeper database.  Contains DateTime.MinValue if the document has never been promoted
        /// to this database.</param>
        /// <param name="liveRequestId">Native ID for the request containing the document version
        /// which resides in the Live database.  Contains -1 if the document has been deleted or
        /// was never promoted to this level.</param>
        /// <param name="liveDate">Date and Time when the document was promoted to or deleted from
        /// the GateKeeper database.  Contains DateTime.MinValue if the document has never been promoted
        /// to this database.</param>
        /// <param name="docType">The document's CDRDocumentType value</param>
        public RequestLocationExternalIds(int cdrId, string gatekeeperRequestId, DateTime gatekeeperDate,
                    string stagingRequestId, DateTime stagingDate,
                    string previewRequestId, DateTime previewDate,
                    string liveRequestId, DateTime liveDate)
            : base(cdrId, gatekeeperDate, stagingDate, previewDate, liveDate)
        {
            _gatekeeperRequestId  = gatekeeperRequestId;
            _stagingRequestId  = stagingRequestId;
            _previewRequestId  = previewRequestId;
            _liveRequestId  = liveRequestId;
        }

        #endregion

        #region Properties

        public string GKReqId
        {
            get { return _gatekeeperRequestId; }
        }

        public string StagingReqId
        {
            get { return _stagingRequestId; }
        }

        public string PreviewReqId
        {
            get { return _previewRequestId; }
        }

        public string LiveReqId
        {
            get { return _liveRequestId; }
        }

        public bool IsPresentInGateKeeper
        {
            get { return !String.IsNullOrEmpty(_gatekeeperRequestId); }
        }

        public bool IsPresentInStaging
        {
            get { return !String.IsNullOrEmpty(_stagingRequestId); }
        }

        public bool IsPresentInPreview
        {
            get { return !String.IsNullOrEmpty(_previewRequestId); }
        }

        public bool IsPresentInLive
        {
            get { return !String.IsNullOrEmpty(_liveRequestId); }
        }

        #endregion
    }
}
