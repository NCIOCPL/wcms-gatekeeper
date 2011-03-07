using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

using GateKeeper.Common;
using GateKeeper.DataAccess;
using GKManagers.BusinessObjects;

namespace GKManagers.DataAccess
{
    static class BatchQuery
    {
        public static bool CreateNewBatch(ref Batch source, int requestID)
        {
            bool succeeded = true;

            try
            {
                /// This is a HACK.  The optimal solution is to put the integrity check into
                /// the stored procedure.
                RequestStatusType status = RequestManager.GetRequestStatus(requestID);
                if (status != RequestStatusType.DataReceived)
                {
                    string fmt = "Request {0} must be in a DataReceived state before a batch is created. The current state is {1}.";
                    string message = string.Format(fmt, requestID, status.ToString());
                    BatchLogBuilder.Instance.CreateError(typeof(BatchManager), "CreateNewBatch", message);
                    throw new Exception(message);
                }

                succeeded = CreateNewBatch(ref source);
                if (succeeded)
                    succeeded = AddRequestToBatch(ref source, requestID);
                if (succeeded)
                    succeeded = InsertBatchInQueue(ref source);
            }
            catch (Exception ex)
            {
                BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "CreateNewBatch", ex);
                throw new Exception("Error in BatchQuery.CreateNewBatch", ex);
            }

            return succeeded;
        }

        public static bool CreateNewBatch(ref Batch source, int[] requestDataIDList)
        {
            bool succeeded = true;

            if (requestDataIDList == null)
                throw new ArgumentNullException("requestDataIDList");

            try
            {
                if (requestDataIDList.Length < 1)
                {
                    BatchLogBuilder.Instance.CreateError(typeof(BatchManager), "CreateNewBatch",
                        "requestDataIDList has zero entries.");
                    throw new Exception("requestDataIDList must contain at least one requestDataID.");
                }

                /// This is a HACK.  The optimal solution is to put the integrity check into
                /// the stored procedure.
                RequestStatusType status = RequestManager.GetRequestStatusFromDocumentID(requestDataIDList[0]);
                if (status != RequestStatusType.DataReceived)
                {
                    string fmt = "Parent request for requestDataID {0} must be in a DataReceived state before a batch is created. The current state is {1}.";
                    string message = string.Format(fmt, requestDataIDList[0], status.ToString());
                    BatchLogBuilder.Instance.CreateError(typeof(BatchManager), "CreateNewBatch", message);
                    throw new Exception(message);
                }

                succeeded = CreateNewBatch(ref source);
                if (succeeded)
                    succeeded = AddRequestDataToBatch(ref source, requestDataIDList);
                if (succeeded)
                    succeeded = InsertBatchInQueue(ref source);
            }
            catch (Exception ex)
            {
                BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "CreateNewBatch", ex);
                throw new Exception("Error in BatchQuery.CreateNewBatch", ex);
            }

            return succeeded;
        }

        /// <summary>
        /// Common code to do the shared portion of creating a new entry in the Batch table.
        /// </summary>
        /// <param name="source">Reference to the Batch object to be created.</param>
        /// <returns>true on success, throws an exception on error.</returns>
        private static bool CreateNewBatch(ref Batch source)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_CREATE_NEW_BATCH))
            {
                DbConnection conn = db.CreateConnection();
                conn.Open();
                DbTransaction transaction = conn.BeginTransaction();

                try
                {
                    StringBuilder actionList = new StringBuilder();
                    for (int i = 0; i < source.Actions.Count; ++i)
                    {
                        if (i > 0)
                            actionList.Append(",");
                        actionList.Append(source.Actions[i].ToString());
                    }

                    db.AddInParameter(cmd, "@BatchName", DbType.String, source.BatchName);
                    db.AddInParameter(cmd, "@UserName", DbType.String, source.UserName);
                    db.AddInParameter(cmd, "@PubActions", DbType.String, actionList.ToString());

                    db.AddOutParameter(cmd, "@BatchID", DbType.Int32, 10);

                    QueryWrapper.ExecuteNonQuery(db, cmd, transaction);
                    source.BatchID = (int)db.GetParameterValue(cmd, "@BatchID");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "CreateNewBatch", ex);
                    throw new Exception("Error in BatchQuery.CreateNewBatch", ex);
                }
                finally
                {
                    transaction.Dispose();
                    conn.Close();
                    conn.Dispose();
                }
            }

            return succeeded;
        }

        private static bool AddRequestToBatch(ref Batch source, int requestID)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_ADD_REQUEST_TO_BATCH))
            {
                try
                {
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, source.BatchID);
                    db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    string statusText = (string)db.GetParameterValue(cmd, "@Status_Text");

                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "AddRequestToBatch",
                        statusText, ex);
                    throw new Exception("Error in BatchQuery.AddRequestToBatch", ex);
                }
            }

            return succeeded;
        }

        private static bool AddRequestDataToBatch(ref Batch source, int[] requestDataIDList)
        {
            bool succeeded = true;

            // The largest value a DbType.String parameter may hold is 4000 characters.
            const int MAX_STRING_SIZE = 4000;

            // Convert list of numbers to one or more strings containing comma-delimited values.
            string[] valueList = ConvertGeneral<int>.ArrayToString(requestDataIDList, MAX_STRING_SIZE);

            Database dbconn = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());

            try
            {
                // Send the lists of values to the database, one at a time.
                foreach (string values in valueList)
                {
                    using (DbCommand cmd =
                        dbconn.GetStoredProcCommand(StoredProcNames.SP_ADD_REQUESTDATA_TO_BATCH))
                    {
                        dbconn.AddInParameter(cmd, "@BatchID", DbType.Int32, source.BatchID);
                        dbconn.AddInParameter(cmd, "@RequestDataIDs", DbType.String, values);

                        QueryWrapper.ExecuteNonQuery(dbconn, cmd);
                    }
                }
            }
            catch (Exception ex)
            {
                BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "AddRequestToBatch", ex);
                throw new Exception("Error in BatchQuery.AddRequestDataToBatch", ex);
            }

            return succeeded;
        }

        private static bool InsertBatchInQueue(ref Batch source)
        {
            bool succeeded = true;
             
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_INSERT_BATCH_IN_QUEUE))
            {
                DbConnection conn = db.CreateConnection();
                conn.Open();
                DbTransaction transaction = conn.BeginTransaction();

                try
                {
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, source.BatchID);
                    db.AddInParameter(cmd, "@Status", DbType.String, source.Status);

                    QueryWrapper.ExecuteNonQuery(db, cmd, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "InsertBatchInQueue", ex);
                    throw new Exception("Error in BatchQuery.InsertBatchInQueue", ex);
                }
                finally
                {
                    transaction.Dispose();
                    conn.Dispose();
                }
            }

            return succeeded;
        }

        /// <summary>
        /// Removes the specified batch from the ProcessQueue.  The batch is logically deleted,
        /// but is not physically removed from the database.
        /// </summary>
        /// <param name="target">The batch to be removed.</param>
        /// <returns>true on success, false on error</returns>
        public static bool CancelBatch(ref Batch target)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_CANCEL_BATCH))
            {
                DbConnection conn = db.CreateConnection();
                conn.Open();
                DbTransaction transaction = conn.BeginTransaction();

                try
                {
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, target.BatchID);
                    db.AddInParameter(cmd, "@Status", DbType.String, target.Status);

                    QueryWrapper.ExecuteNonQuery(db, cmd, transaction);

                    // Check that the stored procedure was successful.
                    int statusCode = (int)db.GetParameterValue(cmd, "@Status_code");
                    if (statusCode > 0)
                    {
                        /// status code > 0 means no updates occured, no need to rollback.
                        succeeded = false;
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "CancelBatch", ex);
                    throw new Exception("Error in BatchQuery.CancelBatch", ex);
                }
                finally
                {
                    transaction.Dispose();
                    conn.Close();
                    conn.Dispose();
                }
            }

            return succeeded;
        }

        public static bool CompleteBatch(Batch target)
        {
            bool succeeded = true;

            string procedureName;
            if (target.Status == BatchStatusType.CompleteWithErrors)
                procedureName = StoredProcNames.SP_COMPLETE_BATCH_WITH_ERRORS;
            else
                procedureName = StoredProcNames.SP_REMOVE_BATCH_FROM_QUEUE;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(procedureName))
            {
                DbConnection conn = db.CreateConnection();
                conn.Open();
                DbTransaction transaction = conn.BeginTransaction();

                try
                {
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, target.BatchID);
                    db.AddInParameter(cmd, "@Status", DbType.String, target.Status);

                    QueryWrapper.ExecuteNonQuery(db, cmd, transaction);
                    transaction.Commit();                    
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "CompleteBatch", ex);
                    throw new Exception("Error in BatchQuery.CompleteBatch", ex);
                }
                finally
                {
                    transaction.Dispose();
                    conn.Close();
                    conn.Dispose();
                }
            }

            return succeeded;
        }

        public static Batch LoadBatch(int batchID)
        {
            Batch batch = null;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_BATCH))
            {

                try
                {
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, batchID);

                    DataSet results = QueryWrapper.ExecuteDataSet(db, cmd);
                    batch = BatchMapper.LoadBatch(results);
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "LoadBatch", ex);
                    throw new Exception("Error in BatchQuery.LoadBatch", ex);
                }
            }

            return batch;
        }

        public static List<int> LoadBatchList(BatchListFilterType filterType)
        {
            List<int> batchList = null;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_BATCH_LIST))
            {
                try
                {
                    db.AddInParameter(cmd, "@Filter", DbType.Int32, (int)filterType);

                    DataSet results = QueryWrapper.ExecuteDataSet(db, cmd);
                    batchList = BatchMapper.LoadBatchList(results.Tables[0]);
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "LoadBatchList", ex);
                    throw new Exception("Error in BatchQuery.LoadBatchList", ex);
                }
            }

            return batchList;
        }

        public static DataSet LoadBatchDetailsList(BatchListFilterType filterType)
        {
            DataSet results = null; 

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_BATCH_DETAILS_LIST))
            {
                try
                {
                    db.AddInParameter(cmd, "@Filter", DbType.Int32, (int)filterType);

                    results = QueryWrapper.ExecuteDataSet(db, cmd);
                }
                catch (Exception ex)
                {
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "LoadBatchDetailsList", ex);
                    throw new Exception("Error in BatchQuery.LoadBatchDetailsList", ex);
                }
            }

            return results;
        }

        public static int GetFailedBatchProcessQueueCount()
        {
            //TODO this should be optimized, so that it does not return dataset back
            int count = 0;
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_FAILED_BATCH_COUNT))
            {
                try
                {
                    DataSet results = QueryWrapper.ExecuteDataSet(db, cmd);

                    if (results != null
                        && results.Tables.Count >= 1
                        && results.Tables[0].Rows.Count > 0)
                    {
                        count = (int)results.Tables[0].Rows[0].ItemArray[0];
                        results.Dispose();
                    }

                }
                catch (Exception ex)
                {
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "GetFailedBatchProcessQueueCount", ex);
                    throw new Exception("Error in BatchQuery.GetFailedBatchProcessQueueCount", ex);
                }
            }
            return count;
        }

        public static void ResetBatchDocumentStatus(int batchId)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_RESET_BATCH_DOCUMENT_STATUS))
            {
                try
                {
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, batchId);

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "ResetBatchDocumentStatus", ex);
                    throw new Exception("Error in BatchQuery.ResetBatchDocumentStatus", ex);
                }
            }
        }

        public static int StartNextBatch()
        {
            int nextID = 0;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_START_NEXT_BATCH))
            {
                try
                {
                    // TODO: Modify the stored proc to return a single ID instead of DataSet.
                    DataSet results = QueryWrapper.ExecuteDataSet(db, cmd);
                    nextID = BatchMapper.LoadBatchID(results);
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "StartNextBatch", ex);
                    throw new Exception("Error in BatchQuery.StartNextBatch", ex);
                }
            }

            return nextID;
        }

        public static void AddBatchHistoryEntry(int batchID, string userName, string entry)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_ADD_BATCH_HISTORY_ENTRY))
            {
                try
                {
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, batchID);
                    db.AddInParameter(cmd, "@UserName", DbType.String, userName);
                    db.AddInParameter(cmd, "@Entry", DbType.String, entry);

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "AddBatchHistoryEntry", ex);
                    throw new Exception("Error in BatchQuery.AddBatchHistoryEntry", ex);
                }
            }
        }

        public static void CompletePromotionStep(int batchID, ProcessActionType action)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_UPDATE_BATCH_ACTIONS))
            {
                try
                {
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, batchID);
                    db.AddInParameter(cmd, "@ActionName", DbType.String, action.ToString());

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "CompletePromotionStep", ex);
                    throw new Exception("Error in BatchQuery.CompletePromotionStep", ex);
                }
            }
        }

        public static void RunModalityCleanup(int batchID, string userName, ProcessActionType action)
        {
            try
            {
                string connectionString = string.Empty;
                string database = string.Empty;
                if (action == ProcessActionType.PromoteToStaging)
                {
                    database = ContentDatabase.Staging.ToString();
                    connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ContentDatabase.Staging.ToString()].ToString();
                }
                else if (action == ProcessActionType.PromoteToPreview)
                {
                    database = ContentDatabase.Preview.ToString();
                    connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ContentDatabase.Preview.ToString()].ToString();
                }
                else if (action == ProcessActionType.PromoteToLive)
                {
                    database = ContentDatabase.Live.ToString();
                    connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ContentDatabase.Live.ToString()].ToString();
                }

                if ((connectionString.Trim().Length > 0) && (database.Trim().Length > 0))
                {
                    // Clean up unused modality from database
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand cmd = new SqlCommand(StoredProcNames.SP_DELETE_UNUSED_MODALITY, connection);

                        cmd.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        try
                        {
                            SqlDataReader reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                string modalityList = reader["ModalityID"].ToString();
                                if (modalityList.Trim().Length > 0)
                                {
                                    string entryText = "Unused modality(s) has been cleaned up in CDR" + database + "GK database.  Modality ID=" + modalityList + ".";
                                    AddBatchHistoryEntry(batchID, userName, entryText);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error in executing store procedure " + StoredProcNames.SP_DELETE_UNUSED_MODALITY + " in CDR" + database + "GK database.", ex);
                        }
                        finally
                        {
                            connection.Close();
                            connection.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BatchLogBuilder.Instance.CreateError(typeof(BatchQuery), "RunModalityCleanup", ex);
                throw new Exception("Error in BatchQuery.RunModalityCleanup", ex);
            }
        }

    }
}
