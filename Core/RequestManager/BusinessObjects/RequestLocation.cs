using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.BusinessObjects
{
    public class RequestLocationInternalIds : RequestLocationBase
    {
        public const int LocationNotPresent = -1;

        private int _gatekeeperRequestId = LocationNotPresent;
        private int _stagingRequestId = LocationNotPresent;
        private int _previewRequestId = LocationNotPresent;
        private int _liveRequestId = LocationNotPresent;

        private CDRDocumentType _docType;

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
        public RequestLocationInternalIds(int cdrId, int gatekeeperRequestId, DateTime gatekeeperDate,
                    int stagingRequestId, DateTime stagingDate, 
                    int previewRequestId, DateTime previewDate,
                    int liveRequestId, DateTime liveDate, CDRDocumentType docType)
            : base(cdrId, gatekeeperDate, stagingDate, previewDate, liveDate)
        {
            _docType = docType;

            if (gatekeeperRequestId > 0)
                _gatekeeperRequestId  = gatekeeperRequestId;
            if (stagingRequestId > 0)
                _stagingRequestId  = stagingRequestId;
            if (previewRequestId > 0)
                _previewRequestId  = previewRequestId;
            if (liveRequestId > 0)
                _liveRequestId  = liveRequestId;
        }

        #endregion

        #region Properties
        public int GKReqId
        {
            get { return _gatekeeperRequestId; }
        }

        public int StagingReqId
        {
            get { return _stagingRequestId; }
        }

        public int PreviewReqId
        {
            get { return _previewRequestId; }
        }

        public int LiveReqId
        {
            get { return _liveRequestId; }
        }

        /// <summary>
        /// Notes whether a document is stored in GateKeeper.  If true, a RequestData object can be
        /// created.  If false, further clarification is needed (via IsRemovedFromGateKeeper) to
        /// determine whether the document has been removed, or has never been promoted to this level.
        /// For GateKeeper only, IsPresentInGateKeeper will only be false for remove requests.
        /// </summary>
        public bool IsPresentInGateKeeper
        {
            get { return (_gatekeeperRequestId > 0); }
        }

        /// <summary>
        /// Notes whether a document is stored in Staging.  If true, a RequestData object can be
        /// created.  If false, further clarification is needed (via IsRemovedFromStaging) to
        /// determine whether the document has been removed, or has never been promoted to this level.
        /// </summary>
        public bool IsPresentInStaging
        {
            get { return (_stagingRequestId > 0); }
        }

        /// <summary>
        /// Notes whether a document is stored in Preview.  If true, a RequestData object can be
        /// created.  If false, further clarification is needed (via IsRemovedFromPreview) to
        /// determine whether the document has been removed, or has never been promoted to this level.
        /// </summary>
        public bool IsPresentInPreview
        {
            get { return (_previewRequestId > 0); }
        }

        /// <summary>
        /// Notes whether a document is stored in Live.  If true, a RequestData object can be
        /// created.  If false, further clarification is needed (via IsRemovedFromLive) to
        /// determine whether the document has been removed, or has never been promoted to this level.
        /// </summary>
        public bool IsPresentInLive
        {
            get { return (_liveRequestId > 0); }
        }

        /// <summary>
        /// Clarifies the IsPresentInGateKeeper property.  If true, the document is not present because
        /// it has been removed.  For GateKeeper only, IsPresentInGateKeeper is only false for
        /// remove requests.
        /// </summary>
        public bool IsRemovedFromGateKeeper
        {
            get { return (_gatekeeperRequestId == LocationNotPresent) && HasDateInGateKeeper; }
        }

        /// <summary>
        /// Clarifies the IsPresentInStaging property.  If true, the document is not present because
        /// it has been removed.  If false, the document is not present because it was never promoted to
        /// Staging.
        /// </summary>
        public bool IsRemovedFromStaging
        {
            get { return (_stagingRequestId == LocationNotPresent) && HasDateInStaging; }
        }

        /// <summary>
        /// Clarifies the IsPresentInPreview property.  If true, the document is not present because
        /// it has been removed.  If false, the document is not present because it was never promoted to
        /// Preview.
        /// </summary>
        public bool IsRemovedFromPreview
        {
            get { return (_previewRequestId == LocationNotPresent) && HasDateInPreview; }
        }

        /// <summary>
        /// Clarifies the IsPresentInLive property.  If true, the document is not present because
        /// it has been removed.  If false, the document is not present because it was never promoted to
        /// Live.
        /// </summary>
        public bool IsRemovedFromLive
        {
            get { return (_liveRequestId == LocationNotPresent) && HasDateInLive; }
        }

        public string DocType
        {
            get { return _docType.ToString(); }
        }

        #endregion

    }
}
