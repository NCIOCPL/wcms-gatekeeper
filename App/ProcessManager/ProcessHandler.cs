using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using GKManagers;
using GKManagers.BusinessObjects;

namespace GateKeeper.ProcessManager
{
    /// <summary>
    /// ProcessHandler is the object responsible for controlling document manager from
    /// within the Process Manager.  Only one instance of this object may be used at a time,
    /// it is therefore implemented as a singleton and the instance created/accessed via the
    /// GetInstance() method.
    /// </summary>
    class ProcessHandler
    {
        #region fields

        private bool _processingIsAllowed = true;
        private string _userName;
        private int _currentBatchID = -1;
        private ManualResetEvent _done;

        /// Locking object and flag to stop multiple threads from being
        /// serviced simultaneously by a single instance.
        private object _lockObject = new object();
        private bool _processIdle = true;

        #endregion

        #region Singleton Implementation

        static ProcessHandler _theInstance = null;

        public static ProcessHandler GetInstance()
        {
            if (_theInstance == null)
            {
                object theLock = new object();
                lock (theLock)
                {
                    if (_theInstance == null)
                        _theInstance = new ProcessHandler();
                }
            }

            return _theInstance;
        }

        private ProcessHandler()
        {
            /// Initially create the reset event in a signalled state.
            /// This way the object can exit in the case where ProcessBatches
            /// was never called.  (ProcessBatches will reset/set the
            /// state on its own.)
            _done = new ManualResetEvent(true);
            _userName = "Not Active";
        }

        #endregion

        /// <summary>
        /// Runs all batches in the processing queue.  If the method is already active and
        /// a second thread or event calls it, the additional call will return without
        /// attempting to execute.
        /// </summary>
        public void ProcessBatches(string userName)
        {
            if (_processIdle)
            {
                lock (_lockObject)
                {
                    _processIdle = false;
                    _userName = userName;

                    _done.Reset();
                    _processingIsAllowed = true;

                    try
                    {
                        GKManagers.DocumentManager.ProcessingIsAllowed = true;
                        while (_processingIsAllowed &&
                            (_currentBatchID = BatchManager.StartNextBatch()) > 0)
                        {
                            GKManagers.DocumentManager.PromoteBatch(_currentBatchID, _userName);
                        }
                    }
                    catch (Exception ex)
                    {
                        ProcessMgrLogBuilder.Instance.CreateCritical(this.GetType(), "ProcessBatches",
                            string.Format("Unhandled exception while processing batch {0}.", _currentBatchID),
                            ex);

                        // Rethrow so Process Manager fails and we can diagnose the problem.
                        throw;
                    }
                    _currentBatchID = -1;
                    _done.Set();

                    _processIdle = true;
                    _userName = "Not Active";
                }
            }
        }

        public void HaltProcessing()
        {
            
            _processingIsAllowed = false;
            GKManagers.DocumentManager.ProcessingIsAllowed = false;
            if (_currentBatchID > 1)
            {
                BatchManager.AddBatchHistoryEntry(_currentBatchID, _userName,
                    "Batch processing halted.");
            }
            _done.WaitOne();
        }
    }
}
