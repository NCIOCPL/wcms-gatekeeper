using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.BusinessObjects
{
    /// <summary>
    /// The DocumentStatusEntry class is used in conjunction by DocumentStatusMap
    /// to track the status and dependency status of a single document.
    /// </summary>
    public class DocumentStatusEntry
    {
        private RequestDataStatusType _docStatus;
        private RequestDataDependentStatusType _dependencyStatus;

        public DocumentStatusEntry(RequestDataStatusType status,
            RequestDataDependentStatusType dependencyStatus)
        {
            _docStatus = status;
            _dependencyStatus = dependencyStatus;
        }

        #region Properties

        public RequestDataStatusType DocStatus
        {
            get { return _docStatus; }
            set { _docStatus = value; }
        }

        public RequestDataDependentStatusType DependencyStatus
        {
            get { return _dependencyStatus; }
            set { _dependencyStatus = value; }
        }

        #endregion
    }

    /// <summary>
    /// DocumentStatusMap maintains a list of documents (keyed by ID) with their
    /// Status and Dependency Status.  Two parallel lists are kept.  The Original status
    /// map and the Live status map.  The CheckStatus and CheckDependencyStatus methods
    /// return the document's original status.  The SetLiveStatus and SetLiveDependencyStatus
    /// methods store upates into the live status.  The live status never overwrites
    /// the original values.
    /// </summary>
    public class DocumentStatusMap
    {
        private Dictionary<int, DocumentStatusEntry> _originalStatusMap;
        private Dictionary<int, DocumentStatusEntry> _liveStatusMap;

        /// <summary>
        /// Initialize the status map.
        /// </summary>
        /// <param name="map">A collection of document status objects keyed by ID.</param>
        public DocumentStatusMap(Dictionary<int, DocumentStatusEntry> map)
        {
            lock (this)
            {
                // Make two copies of the map that was passed in.
                _originalStatusMap = new Dictionary<int, DocumentStatusEntry>(map);
                _liveStatusMap = new Dictionary<int, DocumentStatusEntry>(map);
            }
        }

        /// <summary>
        /// Retrieves the original status of the specified document.
        /// </summary>
        /// <param name="requestDataID">Identifier of the document to retrieve status</param>
        /// <returns>A RequestDataStatusType representing whether the document is OK, has warnings,
        /// or has errors.</returns>
        public RequestDataStatusType CheckDocumentStatus(int requestDataID)
        {
            lock (_originalStatusMap[requestDataID])
            {
                return _originalStatusMap[requestDataID].DocStatus;
            }
        }

        /// <summary>
        /// Retrieves the original dependency status of the specified document.
        /// </summary>
        /// <param name="requestDataID">Identifier of the document to retrieve status</param>
        /// <returns>A RequestDataDependentStatusType value representing whether any other documents in
        /// the same processing group have errors.</returns>
        public RequestDataDependentStatusType CheckDependencyStatus(int requestDataID)
        {
            lock (_originalStatusMap[requestDataID])
            {
                return _originalStatusMap[requestDataID].DependencyStatus;
            }
        }

        /// <summary>
        /// Verifies that a given requestData ID is present in the map.
        /// </summary>
        /// <param name="requestDataID">RequestData ID ID to verify.</param>
        /// <returns>true if the document is present, false otherwise.</returns>
        public bool Contains(int requestDataID)
        {
            /// This method only verifies that a record exists and makes no attempt
            /// to evaluate it.  Since the class offers no ability to add/remove records,
            /// there is no need to lock the document map.
            return _originalStatusMap.ContainsKey(requestDataID);
        }

        /// <summary>
        /// Retrieves the live status of the specified document.
        /// </summary>
        /// <param name="requestDataID">Identifier of the document to retrieve status</param>
        /// <returns>A RequestDataStatusType representing whether the document is OK, has warnings,
        /// or has errors.</returns>
        public RequestDataStatusType CheckLiveDocumentStatus(int requestDataID)
        {
            lock (_liveStatusMap[requestDataID])
            {
                return _liveStatusMap[requestDataID].DocStatus;
            }
        }

        public void UpdateLiveDocumentStatus(int requestDataID, RequestDataStatusType status)
        {
            lock (_liveStatusMap[requestDataID])
            {
                _liveStatusMap[requestDataID].DocStatus = status;
            }
        }

        /// <summary>
        /// Retrieves the live dependency status of the specified document.
        /// </summary>
        /// <param name="requestDataID">Identifier of the document to retrieve dependency status</param>
        /// <returns>A RequestDataDependentStatusType value representing whether any other documents in
        /// the same processing group have errors.</returns>
        public RequestDataDependentStatusType CheckLiveDependencyStatus(int requestDataID)
        {
            lock (_liveStatusMap[requestDataID])
            {
                return _liveStatusMap[requestDataID].DependencyStatus;
            }
        }

        public void UpdateLiveDependencyStatus(int requestDataID, RequestDataDependentStatusType status)
        {
            lock (_liveStatusMap[requestDataID])
            {
                _liveStatusMap[requestDataID].DependencyStatus = status;
            }
        }
    }
}
