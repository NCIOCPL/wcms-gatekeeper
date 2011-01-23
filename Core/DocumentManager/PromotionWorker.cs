using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using GateKeeper.DataAccess.GateKeeper;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    class PromotionWorker
    {
        #region Delegates & Private types.
        private delegate void VersionUpdater(int cdrid, int requestID);

        #endregion

        private ManualResetEvent _doneEvent;
        private SynchronizedIDQueue _idQueue;
        private int _batchID;
        private ProcessActionType _action;
        private bool _validationNeeded;
        private string _userName;
        private DocumentXPathManager _xPathManager;
        private DocumentVersionMap _locationMap;
        private DocumentStatusMap _statusMap;

        /// Used in PromotionCallback() to prevent multiple threads from logging
        /// critical errors.
        static private bool _systemIsKnownToBeCrashing = false;

        #region Properties

        public bool AllPromotionsSucceeded { get; private set; }

        /// <summary>
        /// Determines whether the thread is allowed to continue running.
        /// </summary>
        private bool OKToContinueRunning
        {
            get { return !_systemIsKnownToBeCrashing; }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doneEvent"></param>
        /// <param name="batchID"></param>
        /// <param name="action"></param>
        /// <param name="mustValidate"></param>
        /// <param name="userName">username responsible for scheduling the batch.</param>
        /// <param name="XPathManager"></param>
        /// <param name="locationMap"></param>
        /// <param name="statusMap"></param>
        public PromotionWorker(
            ManualResetEvent doneEvent,
            SynchronizedIDQueue idQueue, 
            int batchID, 
            ProcessActionType action,
            bool mustValidate,
            string userName, 
            DocumentXPathManager xPathManager, 
            DocumentVersionMap locationMap, 
            DocumentStatusMap statusMap)
        {
            // Value types.
            _action = action;
            _batchID = batchID;
            _validationNeeded = mustValidate;

            // One ID Queue per thread.
            _idQueue = idQueue;

            // References to shared external objects.  (Single instances of these objects are shared by
            // all threads.)
            _doneEvent = doneEvent;
            _locationMap = locationMap;
            _statusMap = statusMap;
            _userName = userName;
            _xPathManager = xPathManager;
        }

        public void PromotionCallback(Object unused)
        {
            int requestDataID;// = (int)threadContext;

            while (_idQueue.Count > 0 && OKToContinueRunning)
            {
                lock (_idQueue)
                {
                    if (_idQueue.Count > 0)
                        requestDataID = _idQueue.Dequeue();
                    else
                        requestDataID = int.MinValue;
                }
                if (requestDataID != int.MinValue)
                    Promote(requestDataID);
            }

            _doneEvent.Set();
        }

        private void Promote(int requestDataID)
        {
            RequestData docData;

            try
            {
                bool promotionSucceeded;

                docData = RequestManager.LoadRequestDataByID(requestDataID);

                bool documentValid = true;
                if (_validationNeeded)
                    documentValid = ValidateDocument(docData);

                if (documentValid)
                {
                    DocumentPromoterBase documentPromoter =
                        DocumentPromoterFactory.Create(docData, _batchID, _action, _userName);
                    documentPromoter.Promote(_xPathManager);
                    promotionSucceeded = documentPromoter.PromotionWasSuccessful;
                    documentPromoter = null;
                }
                else
                {
                    RequestManager.MarkDocumentWithErrors(docData.RequestDataID);
                    RequestManager.AddRequestDataHistoryEntry(docData.RequestID, docData.RequestDataID,
                        _batchID, "Failed DTD validation, not promoted.", RequestDataHistoryType.Error);
                    promotionSucceeded = false;
                }

                if (promotionSucceeded)
                {
                    // Keep the document map up to date so we don't have to reload it.
                    // CDRDocumentLocation table gets updated in PromoterBase.
                    VersionUpdater updater = SelectUpdater(_action, docData.ActionType, _locationMap);
                    updater(docData.CdrID, docData.RequestID);
                    updater = null;
                }

                // Update global status for whether all documents in this thread
                // have been successfully promoted.
                AllPromotionsSucceeded &= promotionSucceeded;
            }
            catch (Exception ex)
            {
                // Prevent multiple threads from logging a system crash.
                lock (ex)
                {
                    if (!_systemIsKnownToBeCrashing)
                    {
                        _systemIsKnownToBeCrashing = true;
                        DocMgrLogBuilder.Instance.CreateCritical(this.GetType(), "PromotionCallback",
                            "Thread crash.", ex);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Helper function to choose the correct method for updating the DocumentVersionMap.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        private static VersionUpdater SelectUpdater(ProcessActionType action,
            RequestDataActionType actionType,
            DocumentVersionMap locationMap)
        {
            VersionUpdater updater;

            switch (action)
            {
                case ProcessActionType.PromoteToStaging:
                    if (actionType == RequestDataActionType.Export)
                        updater = locationMap.UpdateStagingVersion;
                    else
                        updater = locationMap.DeleteStagingVersion;
                    break;

                case ProcessActionType.PromoteToPreview:
                    if (actionType == RequestDataActionType.Export)
                        updater = locationMap.UpdatePreviewVersion;
                    else
                        updater = locationMap.DeletePreviewVersion;
                    break;

                case ProcessActionType.PromoteToLive:
                    if (actionType == RequestDataActionType.Export)
                        updater = locationMap.UpdateLiveVersion;
                    else
                        updater = locationMap.DeleteLiveVersion;
                    break;

                default:
                    {
                        string format = "Unknown promotion action: {0}.";
                        string message = string.Format(format, action.ToString());
                        DocMgrLogBuilder.Instance.CreateError(typeof(PromotionWorker), "SelectUpdater", message);
                        throw new Exception(format);
                    }
            }

            return updater;
        }

        /// <summary>
        /// Encapsulate a call to validate the document against the DTD.
        /// </summary>
        /// <param name="docData">Document to match</param>
        /// <returns>true if valid, false otherwise</returns>
        private bool ValidateDocument(RequestData docData)
        {
            string message = RequestManager.ValidateRequestData(docData);
            if (message != null)
            {
                RequestManager.AddRequestDataHistoryEntry(docData.RequestID, docData.RequestDataID,
                    _batchID, "Validation Failure: " + message, RequestDataHistoryType.Error);
            }

            return message == null;
        }
    }
}
