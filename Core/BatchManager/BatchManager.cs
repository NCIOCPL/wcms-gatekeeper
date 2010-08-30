using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Data;

using GKManagers.DataAccess;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    static public class BatchManager
    {
        /// <summary>
        /// Oversees the storage of a new Batch object.  The Batch is added to the Batch table,
        /// associated with a list of RequestData objects, and added to ProcessQueue.
        /// This form associates a batch with the entire list of RequestData objects from a request.
        /// </summary>
        /// <param name="source">A Batch object to be stored.</param>
        /// <param name="requestID">ID of a request object.</param>
        /// <returns>True on success, false on error</returns>
        public static bool CreateNewBatch(ref Batch source, int requestID)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            bool success = true;

            source.Status = BatchStatusType.Queued;

            /// If an error occurs, BatchQuery.CreateNewBatch() will throw an exception
            /// instead of returning anything meaningful.
            success = BatchQuery.CreateNewBatch(ref source, requestID);
            if (success)
            {
                WriteBatchCreationHistoryEntry(source);
                RequestManager.StartBatchProcessor();
            }

            return success;
        }

        /// <summary>
        /// Oversees the storage of a new Batch object.  The Batch is added to the Batch table,
        /// associated with a list of RequestData objects, and added to ProcessQueue.
        /// This form associates a batch with a specific list of RequestData objects.
        /// </summary>
        /// <param name="source">A Batch object to be stored.</param>
        /// <param name="requestDataIDList">A list of individual requestData IDs to associate</param>
        /// <returns></returns>
        public static bool CreateNewBatch(ref Batch source, int[] requestDataIDList)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (requestDataIDList == null || requestDataIDList.Length == 0)
                throw new ArgumentException("The 'requestDataIDList' must contain at least one requestData Id.");

            bool success;

            source.Status = BatchStatusType.Queued;

            /// If an error occurs, BatchQuery.CreateNewBatch() will throw an exception
            /// instead of returning anything meaningful.
            success = BatchQuery.CreateNewBatch(ref source, requestDataIDList);
            if (success)
            {
                WriteBatchCreationHistoryEntry(source);
                RequestManager.StartBatchProcessor();
            }

            return success;
        }

        private static void WriteBatchCreationHistoryEntry(Batch source)
        {
            int lastAction = source.Actions.Count - 1;
            string fmt = "Created Batch. Starting action: {0}. Ending action: {1}.";
            string entry = string.Format(fmt, source.Actions[0], source.Actions[lastAction]);
            AddBatchHistoryEntry(source.BatchID, source.UserName, entry);
        }

        /// <summary>
        /// Marks a batch as complete and removes it from the processing queue.
        /// Optionally, mark the batch as Complete-with-errors.
        /// </summary>
        /// <param name="source">The Batch to be dequeued</param>
        /// <param name="hasErrors">If true, mark the batch as complete with errors</param>
        public static void CompleteBatch(Batch source, string user, bool hasErrors)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (hasErrors)
            {
                source.Status = BatchStatusType.CompleteWithErrors;
                AddBatchHistoryEntry(source.BatchID, user, "Completed with Errors.");
            }
            else
            {
                AddBatchHistoryEntry(source.BatchID, user, "Completed Successfully.");
                source.Status = BatchStatusType.Complete;
            }

            BatchQuery.CompleteBatch(source);
        }

        /// <summary>
        /// Marks a batch as complete and removes it from the processing queue.
        /// Optionally, mark the batch as Complete-with-errors.
        /// </summary>
        /// <param name="source">The Batch to be dequeued</param>
        /// <param name="hasErrors">If true, mark the batch as complete with errors</param>
        public static void CompleteBatch(Batch source, string user)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (String.IsNullOrEmpty(user))
                throw new ArgumentException("Argument 'user' must contain a value.");

            CompleteBatch(source, user, false);
        }

        /// <summary>
        /// Marks a batch as cancelled and removes it from the processing queue.
        /// </summary>
        /// <param name="source">The Batch to be dequeued</param>
        public static bool CancelBatch(ref Batch source, string user)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (String.IsNullOrEmpty(user))
                throw new ArgumentException("Argument 'user' must contain a value.");

            source.Status = BatchStatusType.Cancelled;
            AddBatchHistoryEntry(source.BatchID, user, "Batch Cancelled.");

            return BatchQuery.CancelBatch(ref source);
        }

        /// <summary>
        /// Marks a batch as reviewed and removes it from the processing queue.
        /// </summary>
        /// <param name="source">The Batch to be dequeued</param>
        public static bool ReviewBatch(ref Batch source, string user)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            source.Status = BatchStatusType.Reviewed;
            AddBatchHistoryEntry(source.BatchID, user, "Batch Reviewed.");

            return BatchQuery.CompleteBatch(source);
        }

        /// <summary>
        /// Load a saved Batch object from persistent storage based on the system's internal ID.
        /// </summary>
        /// <param name="requestID">internal request ID</param>
        /// <returns>Batch object (null on error)</returns>
        public static Batch LoadBatch(int batchID)
        {
            return BatchQuery.LoadBatch(batchID);
        }

        // Load the entire queue that's eligible to be run.
        public static List<int> LoadActiveBatchList()
        {
            return BatchQuery.LoadBatchList(BatchListFilterType.RetrieveActive);
        }

        // Load the entire queue, regardless of whether it's eligible to run
        public static List<int> LoadCompleteBatchList()
        {
            return BatchQuery.LoadBatchList(BatchListFilterType.RetrieveAll);
        }

        // Load all the failed jobs in the queue
        public static List<int> LoadFailedBatchList()
        {
            return BatchQuery.LoadBatchList(BatchListFilterType.RetrieveError);
        }

        // Load the entire queue that's eligible to be run.
        public static DataSet LoadActiveBatchDetailsList()
        {
            return BatchQuery.LoadBatchDetailsList(BatchListFilterType.RetrieveActive);
        }

        // Load the entire queue, regardless of whether it's eligible to run
        public static DataSet LoadCompleteBatchDetailsList()
        {
            return BatchQuery.LoadBatchDetailsList(BatchListFilterType.RetrieveAll);
        }

        // Load all the failed jobs in the queue
        public static DataSet LoadFailedBatchDetailsList()
        {
            return BatchQuery.LoadBatchDetailsList(BatchListFilterType.RetrieveError);
        }

        // Return the number of failed batches in the process queue 
        public static int GetFailedBatchProcessQueueCount()
        {
            return BatchQuery.GetFailedBatchProcessQueueCount();
        }

        /// <summary>
        /// Changes the status of all documents in the batch to "OK".
        /// This method should only be called after it is no longer possible for
        /// the batch to be cancelled.
        /// </summary>
        /// <param name="batchId"></param>
        public static void ResetBatchDocumentStatus(int batchId)
        {
            BatchQuery.ResetBatchDocumentStatus(batchId);
        }

        public static int StartNextBatch()
        {
            return BatchQuery.StartNextBatch();
        }

        public static void AddBatchHistoryEntry(int batchID, string userName, string entry)
        {
            BatchQuery.AddBatchHistoryEntry(batchID, userName, entry);
        }

        public static void CompletePromotionStep(int batchID, ProcessActionType action)
        {
            BatchQuery.CompletePromotionStep(batchID, action);
        }

        public static void CleanupUnusedModality(int batchID, string userName, List<ProcessActionType> actionList)
        {
            foreach (ProcessActionType action in actionList)
            {
                BatchQuery.RunModalityCleanup(batchID, userName, action);
            }
        }
    }
}
