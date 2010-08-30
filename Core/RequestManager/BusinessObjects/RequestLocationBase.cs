using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.BusinessObjects
{
    public class RequestLocationBase
    {
        private int _cdrId;

        private DateTime _gatekeeperDate;
        private DateTime _stagingDate;
        private DateTime _previewDate;
        private DateTime _liveDate;

        #region Constructor

        /// <summary>
        /// The GateKeeper, Staging, Preview and Live databases potentially have different versions
        /// of the the same document.  A RequestLocationInternalIds object contains identifying information
        /// about the requests that each version came from.
        /// </summary>
        /// <param name="cdrId">CDR Document ID</param>
        /// <param name="gatekeeperDate">Date and Time when the document was added to or deleted from
        /// the GateKeeper database.</param>
        /// <param name="stagingDate">Date and Time when the document was promoted to or deleted from
        /// the GateKeeper database.  Contains DateTime.MinValue if the document has never been promoted
        /// to this database.</param>
        /// <param name="previewDate">Date and Time when the document was promoted to or deleted from
        /// the GateKeeper database.  Contains DateTime.MinValue if the document has never been promoted
        /// to this database.</param>
        /// <param name="liveDate">Date and Time when the document was promoted to or deleted from
        /// the GateKeeper database.  Contains DateTime.MinValue if the document has never been promoted
        /// to this database.</param>
        public RequestLocationBase(int cdrId, DateTime gatekeeperDate,
                    DateTime stagingDate, DateTime previewDate, DateTime liveDate)
        {
            _cdrId = cdrId;     // CdrId must be present.

            _gatekeeperDate  = gatekeeperDate;
            _stagingDate  = stagingDate;
            _previewDate  = previewDate;
            _liveDate  = liveDate;
        }

        #endregion


        #region Properties

        public int CdrId
        {
            get { return _cdrId; }
        }

        public DateTime GKDate
        {
            get { return _gatekeeperDate; }
        }

        public DateTime StagingDate
        {
            get { return _stagingDate; }
        }

        public DateTime PreviewDate
        {
            get { return _previewDate; }
        }

        public DateTime LiveDate
        {
            get { return _liveDate; }
        }

        public bool HasDateInGateKeeper
        {
            get { return _gatekeeperDate != DateTime.MinValue; }
        }

        public bool HasDateInStaging
        {
            get { return _stagingDate != DateTime.MinValue; }
        }

        public bool HasDateInPreview
        {
            get { return _previewDate != DateTime.MinValue; }
        }

        public bool HasDateInLive
        {
            get { return _liveDate != DateTime.MinValue; }
        }

        #endregion
    }
}
