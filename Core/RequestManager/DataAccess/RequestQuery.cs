using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Xml;

using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

using GateKeeper.Common;
using GateKeeper.DataAccess;
using GKManagers.BusinessObjects;

namespace GKManagers.DataAccess
{
    static class RequestQuery
    {

        /// <summary>
        /// Oversees the storage of a Request object.  Upon successful completion,
        /// the object is modified to reflect its new request ID.
        /// </summary>
        /// <param name="source">A Request object to be stored.</param>
        /// <returns>True on success, False on failure.</returns>
        public static bool CreateNewRequest(ref Request source)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_CREATE_NEW_REQUEST))
            {

                try
                {
                    db.AddInParameter(cmd, "@ExternalRequestID", DbType.String, source.ExternalRequestID);
                    db.AddInParameter(cmd, "@Source", DbType.String, source.Source);
                    db.AddInParameter(cmd, "@RequestType", DbType.String, source.RequestPublicationType.ToString());
                    db.AddInParameter(cmd, "@PublicationTarget", DbType.String, source.PublicationTarget.ToString());
                    db.AddInParameter(cmd, "@Description", DbType.String, source.Description);
                    db.AddInParameter(cmd, "@Status", DbType.String, source.Status);
                    db.AddInParameter(cmd, "@DTDVersion", DbType.String, source.DtdVersion);
                    db.AddInParameter(cmd, "@DataType", DbType.String, source.DataType);
                    db.AddInParameter(cmd, "@UpdateUserID", DbType.String, source.UpdateUserID);

                    db.AddOutParameter(cmd, "@RequestID", DbType.Int32, 10);

                    QueryWrapper.ExecuteNonQuery(db, cmd);

                    // Check for non-fatal errors.
                    DBStatusCodeType statusCode = (DBStatusCodeType)db.GetParameterValue(cmd, "@Status_Code");
                    if (statusCode == DBStatusCodeType.Success)
                        source.RequestID = (int)db.GetParameterValue(cmd, "@RequestID");
                    else
                        succeeded = false;
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "CreateNewRequest", ex);
                    throw new Exception("Error in RequestQuery.CreateNewRequest", ex);
                }
            }

            return succeeded;
        }

        /// <summary>
        /// Mark the specified Request object as being abnormally terminated.  If an attempt
        /// is made to abort a request which is no longer open (e.g. previously aborted or
        /// completed), the attempt will be ignored. The method will report a failure, but
        /// will not throw an exception.
        /// </summary>
        /// <param name="externalRequestID">The external ID of the Request being closed.</param>
        /// <param name="updateUserID">The userID responsible for this action</param>
        /// <returns>true on sucess, false on failure</returns>
        public static bool AbortRequest(string externalRequestID, string requestSource, string userID)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_ABORT_REQUEST))
            {

                try
                {
                    db.AddInParameter(cmd, "@ExternalRequestID", DbType.String, externalRequestID);
                    db.AddInParameter(cmd, "@Source", DbType.String, requestSource);
                    db.AddInParameter(cmd, "@UpdateUserID", DbType.String, userID);

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "AbortRequest", ex);

                    DBStatusCodeType statusCode = (DBStatusCodeType)db.GetParameterValue(cmd, "@Status_Code");

                    if (statusCode == DBStatusCodeType.RecordAlreadyComplete)
                        throw new Exception(string.Format("Attempt to abort a request which was already complete. ({0})",
                            externalRequestID));
                    else
                        throw new Exception("Error in RequestQuery.AbortRequest", ex);
                }
            }

            return succeeded;
        }

        /// <summary>
        /// Mark the specified Request object as having received all its data items.  If an attempt
        /// is made to complete a request which has already been marked complete, the attempt will
        /// be ignored. The method will report a failure, but will not throw an exception.
        /// </summary>
        /// <param name="externalRequestID">The external ID of the Request being closed.</param>
        /// <param name="updateUserID">The userID responsible for this action</param>
        /// <returns>true on sucess, false on failure</returns>
        public static bool CompleteRequest(string externalRequestID, string requestSource,
            string userID, int expectedDocCount)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_COMPLETE_REQUEST))
            {
                try
                {
                    db.AddInParameter(cmd, "@ExternalRequestID", DbType.String, externalRequestID);
                    db.AddInParameter(cmd, "@Source", DbType.String, requestSource);
                    db.AddInParameter(cmd, "@UpdateUserID", DbType.String, userID);
                    db.AddInParameter(cmd, "@ExpectedDocCount", DbType.Int32, expectedDocCount);

                    QueryWrapper.ExecuteNonQuery(db, cmd);

                    // Check for non-fatal errors.
                    DBStatusCodeType statusCode = (DBStatusCodeType)db.GetParameterValue(cmd, "@Status_Code");
                    if (statusCode == DBStatusCodeType.Success &&
                        GetRequestPublicationType(externalRequestID, requestSource) == RequestPublicationType.FullLoad)
                    {
                        GenerateComplementaryRemoveRequest(externalRequestID, requestSource);
                    }
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "CompleteRequest", ex);

                    DBStatusCodeType statusCode = (DBStatusCodeType)db.GetParameterValue(cmd, "@Status_Code");
                    if (statusCode == DBStatusCodeType.RecordAlreadyComplete)
                        throw new Exception(string.Format("Attempt to complete a request which had been aborted. ({0})",
                            externalRequestID));
                    else
                        throw new Exception("Error in RequestQuery.CompleteRequest", ex);

                }
            }

            return succeeded;
        }

        private static void GenerateComplementaryRemoveRequest(string externalRequestID, string requestSource)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GENERATE_COMPLEMENTARY_REMOVE_REQUEST))
            {
                DbConnection conn = db.CreateConnection();
                conn.Open();
                DbTransaction transaction = conn.BeginTransaction();

                try
                {
                    db.AddInParameter(cmd, "@FullLoadRequestID", DbType.String, externalRequestID);
                    db.AddInParameter(cmd, "@FullLoadSource", DbType.String, requestSource);

                    QueryWrapper.ExecuteNonQuery(db, cmd, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "GenerateComplementaryRemoveRequest", ex);
                    throw new Exception("Error in RequestQuery.GenerateComplementaryRemoveRequest", ex);
                }
                finally
                {
                    transaction.Dispose();
                    conn.Dispose();
                }
            }
        }

        private static RequestPublicationType GetRequestPublicationType(string externalRequestID,
                        string requestSource)
        {
            RequestPublicationType publicationType = RequestPublicationType.Invalid;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_PUBLICATION_TYPE))
            {
                try
                {
                    db.AddInParameter(cmd, "@ExternalRequestID", DbType.String, externalRequestID);
                    db.AddInParameter(cmd, "@Source", DbType.String, requestSource);
                    db.AddOutParameter(cmd, "@PublicationType", DbType.String, 50);

                    QueryWrapper.ExecuteNonQuery(db, cmd);

                    Object pubType = db.GetParameterValue(cmd, "@PublicationType");
                    publicationType = ConvertEnum<RequestPublicationType>.Convert(pubType);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "GetRequestPublicationType", ex);
                    throw new Exception("Error in RequestQuery.GetRequestPublicationType", ex);
                }
            }

            return publicationType;
        }

        /// <summary>
        /// Stores a RequestData object and creates an association between it and a parent
        /// Request.
        /// </summary>
        /// <param name="requestID">The external ID of the parent Request object</param>
        /// <param name="updateUserID">The userID responsible for this action</param>
        /// <returns>true on sucess, false on failure</returns>
        public static bool InsertRequestData(string externalRequestID, string requestSource, string updateUserID,
            ref RequestData child)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_INSERT_REQUEST_DATA))
            {
                DbConnection conn = db.CreateConnection();
                conn.Open();
                DbTransaction transaction = conn.BeginTransaction();

                try
                {
                    db.AddInParameter(cmd, "@ExternalRequestID", DbType.String, externalRequestID);
                    db.AddInParameter(cmd, "@Source", DbType.String, requestSource);
                    db.AddInParameter(cmd, "@UpdateUserID", DbType.String, updateUserID);

                    db.AddInParameter(cmd, "@PacketNumber", DbType.Int32, child.PacketNumber);
                    db.AddInParameter(cmd, "@ActionType", DbType.String, child.ActionType);
                    db.AddInParameter(cmd, "@DataSetID", DbType.Int32, (int)child.CDRDocType);
                    db.AddInParameter(cmd, "@CDRID", DbType.Int32, child.CdrID);
                    db.AddInParameter(cmd, "@CDRVersion", DbType.String, child.CdrVersion);
                    db.AddInParameter(cmd, "@Status", DbType.String, child.Status);
                    db.AddInParameter(cmd, "@DependencyStatus", DbType.String, child.DependencyStatus);
                    db.AddInParameter(cmd, "@Location", DbType.String, child.Location);
                    db.AddInParameter(cmd, "@GroupID", DbType.Int32, child.GroupID);
                    db.AddInParameter(cmd, "@DocumentData", DbType.String, child.DocumentDataString);

                    db.AddOutParameter(cmd, "@RequestDataID", DbType.Int32, 10);

                    QueryWrapper.ExecuteNonQuery(db, cmd, transaction);

                    child.RequestDataID = (int)db.GetParameterValue(cmd, "RequestDataID");

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "InsertRequestData", ex);
                    throw new Exception("Error in RequestQuery.InsertRequestData", ex);
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

        /// <summary>
        /// Load a saved Request object based on GateKeeper's internal RequestID.
        /// </summary>
        /// <param name="requestID">GateKeeper internal request ID</param>
        /// <returns>Request object (null on error)</returns>
        public static Request LoadRequestByID(int requestID)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            Request request = null;

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_LOAD_REQUEST_BY_ID))
            {

                try
                {
                    DataSet results;
                    db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);

                    results = QueryWrapper.ExecuteDataSet(db, cmd);
                    request = RequestMapper.LoadRequest(results.Tables[0]);
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestByID", ex);
                    throw new Exception("Error in RequestQuery.LoadRequestByID", ex);
                }
            }

            return request;
        }

        /// <summary>
        /// Load a saved Request object based on its external ID.
        /// </summary>
        /// <param name="systemStatus">externally generatd request ID</param>
        /// <returns>Request object (null on error)</returns>
        public static Request LoadRequestByExternalID(string externalID, string requestSource)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            Request request = null;

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_LOAD_REQUEST_BY_EXTERNAL_ID))
            {
                try
                {
                    DataSet results;
                    db.AddInParameter(cmd, "@ExternalRequestID", DbType.String, externalID);
                    db.AddInParameter(cmd, "@Source", DbType.String, requestSource);

                    results = QueryWrapper.ExecuteDataSet(db, cmd);
                    request = RequestMapper.LoadRequest(results.Tables[0]);
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestByExternalID", ex);
                    throw new Exception("Error in RequestQuery.LoadRequestByExternalID", ex);
                }
            }

            return request;
        }

        /// <summary>
        /// Load the RequestData object identified by the unique combination of
        /// parent requestID and CDRID.
        /// </summary>
        /// <param name="requestID">Gatekeeper internal identifier for the request
        /// that the RequestData object is part of.</param>
        /// <param name="cdrid">Unique CDR-assigned document identifier</param>
        /// <returns>RequestData object</returns>
        public static RequestData LoadRequestDataByCdrid(int requestID, int cdrid)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            RequestData requestData = null;

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_LOAD_REQUEST_DATA_BY_CDRID))
            {

                try
                {
                    DataSet results;
                    db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);
                    db.AddInParameter(cmd, "@CDRID", DbType.Int32, cdrid);

                    results = QueryWrapper.ExecuteDataSet(db, cmd);
                    if (results != null && results.Tables[0] != null)
                    {
                        requestData = RequestMapper.LoadRequestDataItem(results.Tables[0].Rows[0]);
                    }
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestDataByCdrid", ex);
                    throw new Exception("Error in RequestQuery.LoadRequestDataByCdrid", ex);
                }
            }

            return requestData;
        }

        /// <summary>
        /// Load the RequestData object identified by its unique requestDataID
        /// </summary>
        /// <param name="requestDataID">Internal requestDataID</param>
        /// <returns>RequestData object</returns>
        public static RequestData LoadRequestDataByID(int requestDataID)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            RequestData requestData = null;

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_LOAD_REQUEST_DATA_BY_ID))
            {
                try
                {
                    DataSet results;
                    db.AddInParameter(cmd, "@RequestDataID", DbType.Int32, requestDataID);

                    results = QueryWrapper.ExecuteDataSet(db, cmd);
                    if (results != null && results.Tables[0] != null)
                    {
                        requestData = RequestMapper.LoadRequestDataItem(results.Tables[0].Rows[0]);
                    }
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestDataByID", ex);
                    throw new Exception("Error in RequestQuery.LoadRequestDataByID", ex);
                }

            }

            return requestData;
        }

        /// <summary>
        /// Load the RequestData meta-data for the object identified by its unique requestDataID
        /// </summary>
        /// <param name="requestDataID">Internal requestDataID</param>
        /// <returns>RequestData object</returns>
        public static RequestDataInfo LoadRequestDataInfo(int requestDataID)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            RequestDataInfo requestData = null;

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_LOAD_REQUEST_DATA_INFO))
            {
                try
                {
                    DataSet results;
                    db.AddInParameter(cmd, "@RequestDataID", DbType.Int32, requestDataID);

                    results = QueryWrapper.ExecuteDataSet(db, cmd);
                    if (results != null && results.Tables[0] != null)
                    {
                        requestData = RequestMapper.LoadRequestDataInfo(results.Tables[0].Rows[0]);
                    }
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestDataByID", ex);
                    throw new Exception("Error in RequestQuery.LoadRequestDataByID", ex);
                }
            }

            return requestData;
        }

        /// <summary>
        /// Load the RequestData objects for up to three request data object for a given CDR ID 
        /// one for the latest request data id that's one staging, one for preview, one for live 
        /// </summary>
        /// <param name="CDRID">CDRID</param>
        /// <returns>array of RequestData objects</returns>
        public static Dictionary<RequestDataLocationType, RequestDataInfo> LoadRequestDataListForCDRLocations(int cdrID)
        {
            Dictionary<RequestDataLocationType, RequestDataInfo> arr = new Dictionary<RequestDataLocationType, RequestDataInfo>();
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            
            // The order is significant.
            RequestDataLocationType[] locations =
                {RequestDataLocationType.GateKeeper, RequestDataLocationType.Staging,
                RequestDataLocationType.Preview, RequestDataLocationType.Live};

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_DATA_FOR_CDR_LOCATIONS))
            {

                try
                {
                    DataSet results;
                    RequestDataInfo reqData;
                    db.AddInParameter(cmd, "@CDRID", DbType.Int32, cdrID);

                    results = db.ExecuteDataSet(cmd);

                    if (results != null)
                    {
                        if (results.Tables.Count != locations.Length)
                        {
                            string fmt = "Unexpected number of results. Expected {0}, Found {1}";
                            RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery),
                                "LoadRequestDataListForCDRLocations",
                                string.Format(fmt, locations.Length, results.Tables.Count));
                        }

                        RequestDataLocationType currentLocation;
                        for (int i = 0; i < locations.Length; ++i)
                        {
                            currentLocation = locations[i];
                            reqData = null;
                            if (results.Tables[i] != null)
                            {
                                foreach (System.Data.DataRow row in results.Tables[i].Rows)
                                {
                                    reqData = RequestMapper.LoadRequestDataInfo(row);
                                }
                            }
                            if (reqData != null)
                                arr.Add(currentLocation, reqData);
                            else
                                arr.Add(currentLocation, new RequestDataInfo());
                        }
                    }
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestDataListForCDRLocations", ex);
                    throw new Exception("Error in RequestQuery.LoadRequestDataListByReqIDorBatchID", ex);
                }

            }

            return arr;
        }


        /// <summary>
        /// Load the RequestData objects from a request with unique requestID
        /// Only a subset of documents returned filtered by the parameters in the filter object 
        /// </summary>
        /// <param name="requestID">Internal requestID</param>
        /// <param name="requestID">Internal requestID</param>
        /// <param name="filter">filter objects with search parameters</param>
        /// <param name="pageNumber">page number, 1 for first page</param>
        /// <param name="resultsPerPage">number of results per page, 0 returns all rows</param>
        /// <returns>array of RequestData objects</returns>
        public static List<RequestData> LoadRequestDataListByReqIDorBatchID(int requestID, int batchID, RequestDataFilter filter, int pageNumber, int resultsPerPage, ref int totalRequestDataCount)
        {
            List<RequestData> arr = new List<RequestData>();
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_SEARCH_REQUEST_DATA))
            {

                try
                {
                    DataSet results;
                    if (requestID > 0)
                        db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);
                    if (batchID > 0)
                        db.AddInParameter(cmd, "@BatchID", DbType.Int32, batchID);

                    if (filter.ActionType != RequestDataActionType.Invalid)
                    {
                        db.AddInParameter(cmd, "@actionType", DbType.String, filter.ActionType.ToString());
                    }
                    if (filter.DocTypeId != CDRDocumentType.Invalid)
                    {
                        int docTypeId = (int)filter.DocTypeId;
                        db.AddInParameter(cmd, "datasetID", DbType.String, docTypeId.ToString());
                    }
                    if (filter.DocStatus != RequestDataStatusType.Invalid)
                    {
                        db.AddInParameter(cmd, "@documentStatus", DbType.String, filter.DocStatus.ToString());
                    }
                    if (filter.DependencyStatus != RequestDataDependentStatusType.Invalid)
                    {
                        db.AddInParameter(cmd, "@dependencyStatus", DbType.String, filter.DependencyStatus.ToString());
                    }
                    if (filter.RequestDataLocation != RequestDataLocationType.Invalid)
                    {
                        db.AddInParameter(cmd, "@location", DbType.String, filter.RequestDataLocation.ToString());
                    }
                    if (filter.GetSelectedColumnSortHash() > 0)
                    {
                        db.AddInParameter(cmd, "@sortOrder", DbType.Int32, filter.GetSelectedColumnSortHash());
                    }
                    if (pageNumber > 0)
                    {
                        db.AddInParameter(cmd, "@pageNumber", DbType.Int32, pageNumber);
                    }
                    if (resultsPerPage > 0)
                    {
                        db.AddInParameter(cmd, "@ResultsPerPage", DbType.Int32, resultsPerPage);
                    }
                    db.AddOutParameter(cmd, "@TotalRequestDataCount", DbType.Int32, totalRequestDataCount);

                    results = db.ExecuteDataSet(cmd);
                    if (results != null && results.Tables[0] != null)
                    {
                        foreach (System.Data.DataRow row in results.Tables[0].Rows)
                        {
                            RequestData reqData = RequestMapper.LoadRequestDataItem(row);
                            arr.Add(reqData);
                        }
                        totalRequestDataCount = Strings.ToInt(db.GetParameterValue(cmd, "@TotalRequestDataCount"));
                    }
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestDataListByReqIDorBatchID", ex);
                    throw new Exception("Error in RequestQuery.LoadRequestDataListByReqIDorBatchID", ex);
                }

            }

            return arr;
        }

        /// <summary>
        /// Load the Request data from Request table - for RequestHistory admin tool screen 
        /// Only a subset of documents returned filtered by the parameters in the filter object 
        /// </summary>
        /// <param name="filter">filter objects with search parameters</param>
        /// <param name="pageNumber">page number, 1 for first page</param>
        /// <param name="resultsPerPage">number of results per page, 0 returns all rows</param>
        /// <returns>dataset of results</returns>
        public static DataSet LoadRequests(RequestHistoryFilter filter, int pageNumber, int resultsPerPage)
        {
            DataSet results = null;
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_SEARCH_REQUEST))
            {

                try
                {
                    cmd.CommandTimeout = ApplicationSettings.CommandTimeout;

                    if (filter.NumberOfMonth > 0)
                        db.AddInParameter(cmd, "@month", DbType.Int32, filter.NumberOfMonth);
                    if (filter.CdrID > 0)
                        db.AddInParameter(cmd, "@CDRID", DbType.Int32, filter.CdrID);

                    if (filter.RequestStatus != RequestStatusType.Invalid)
                    {
                        db.AddInParameter(cmd, "@status", DbType.String, filter.RequestStatus.ToString());
                    }
                    if (filter.PublishingDestination != RequestDataLocationType.Invalid)
                    {
                        db.AddInParameter(cmd, "@publicationtarget", DbType.String, filter.PublishingDestination.ToString());
                    }
                    if (filter.GetSelectedColumnSortHash() > 0)
                    {
                        db.AddInParameter(cmd, "@sortOrder", DbType.Int32, filter.GetSelectedColumnSortHash());
                    }
                    if (pageNumber > 0)
                    {
                        db.AddInParameter(cmd, "@pageNumber", DbType.Int32, pageNumber);
                    }
                    if (resultsPerPage > 0)
                    {
                        db.AddInParameter(cmd, "@ResultsPerPage", DbType.Int32, resultsPerPage);
                    }

                    results = db.ExecuteDataSet(cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequests", ex);
                    throw new Exception("Error in RequestQuery.LoadRequests", ex);
                }

            }

            return results;
        }


        /// <summary>
        /// Load the RequestData objects from a request that are in the list of groups passed in as parameter 
        /// </summary>
        /// <param name="requestID">Internal requestID</param>
        /// <param name="groupIdList">a list of group ids</param>
        /// <returns>array of RequestData objects</returns>
        public static List<RequestData> LoadRequestDataListByReqIDGroups(int requestID, List<int> groupIdList)
        {
            List<RequestData> arr = new List<RequestData>();

            // The largest value a DbType.String parameter may hold is 4000 characters.
            const int MAX_STRING_SIZE = 4000;

            // Convert list of numbers to one or more strings containing comma-delimited values.
            string[] valueList = ConvertGeneral<int>.ArrayToString(groupIdList.ToArray(), MAX_STRING_SIZE);

            // Send the lists of values to the database, one at a time.
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            foreach (string value in valueList)
            {
                using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_DATA_BY_GROUP_ID))
                {
                    try
                    {
                        DataSet results;
                        db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);
                        db.AddInParameter(cmd, "@groupID", DbType.String, value);

                        results = db.ExecuteDataSet(cmd);
                        if (results != null && results.Tables[0] != null)
                        {
                            foreach (System.Data.DataRow row in results.Tables[0].Rows)
                            {
                                RequestData reqData = RequestMapper.LoadRequestDataItem(row);
                                arr.Add(reqData);
                            }
                        }
                        results.Dispose();

                    }
                    catch (Exception ex)
                    {
                        RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestDataListByReqIDGroups", ex);
                        throw new Exception("Error in RequestQuery.LoadRequestDataListByReqIDGroups", ex);
                    }
                }
            }

            return arr;
        }

        /// <summary>
        /// Load the RequestData objects that are in the list of groups passed in as parameter 
        /// </summary>
        /// <param name="requestID">Internal requestID</param>
        /// <param name="groupIdList">a list of group ids</param>
        /// <returns>array of RequestData objects</returns>
        public static List<RequestData> LoadRequestDataListByReqIDGroups(List<int> groupIdList)
        {
            List<RequestData> arr = new List<RequestData>();

            // Convert list of numbers to one or more strings containing comma-delimited values.
            string[] valueList =new string[groupIdList.Count];

            int count =0;
            foreach(int value  in groupIdList)
            {
                valueList[count]=value.ToString();
                count++;
            }

            string valueString = string.Join(",", valueList);

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ContentDatabase.GateKeeper.ToString()].ToString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(StoredProcNames.SP_GET_REQUEST_DATA_BY_DATA_ID_LIST, connection);
                    
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();

                try
                {
                    cmd.Parameters.Add("@requestDataIDList", SqlDbType.Text);
                    cmd.Parameters["@requestDataIDList"].Value = valueString;

                    SqlDataReader reader = cmd.ExecuteReader();

                    // Call Read before accessing data.
                    while (reader.Read())
                    {
                        RequestData dataItem = new RequestData();

                        dataItem.RequestDataID = (int)reader["RequestDataID"];
                        dataItem.RequestID = (int)reader["RequestID"];
                        dataItem.PacketNumber = (int)reader["PacketNumber"];
                        dataItem.ActionType = ConvertEnum<RequestDataActionType>.Convert(reader["ActionType"]);
                        dataItem.CDRDocType = (CDRDocumentType)reader["DataSetID"];
                        dataItem.CdrID = (int)reader["CDRID"];
                        dataItem.CdrVersion = (string)reader["CDRVersion"];
                        dataItem.ReceivedDate = (DateTime)reader["ReceivedDate"];
                        dataItem.Status = ConvertEnum<RequestDataStatusType>.Convert(reader["Status"]);
                        dataItem.DependencyStatus = 
                            ConvertEnum<RequestDataDependentStatusType>.Convert(reader["DependencyStatus"]);

                        if (reader["Data"] != DBNull.Value)
                        {
                            dataItem.DocumentDataString = (string)reader["Data"];
                            //XmlDocument documentData = new XmlDocument();
                            //documentData.PreserveWhitespace = true;
                            //documentData.LoadXml(dataItem.DocumentDataString);
                            //dataItem.DocumentData = documentData;
                        }
                        dataItem.Location = ConvertEnum<RequestDataLocationType>.Convert(reader["Location"]);
                        dataItem.GroupID = (int)reader["GroupID"];

                        arr.Add(dataItem);
                    }

                    // Call Close when done reading.
                    reader.Close();

                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestDataListByReqIDGroups", ex);
                    throw new Exception("Error in RequestQuery.LoadRequestDataListByReqIDGroups", ex);
                }
            }

            return arr;
        }


        public static List<int> LoadRequestDataIDList(int requestID)
        {
            List<int> dataIDList = null;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_LOAD_REQUEST_DATA_ID_LIST))
            {

                try
                {
                    DataSet results;
                    db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);

                    results = QueryWrapper.ExecuteDataSet(db, cmd);
                    if (results != null && results.Tables[0] != null)
                    {
                        dataIDList = RequestMapper.LoadRequestDataIDList(results.Tables[0]);
                    }
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestDataIDList", ex);
                    throw new Exception("Error in RequestQuery.LoadRequestDataIDList", ex);
                }
            }

            return dataIDList;
        }

        public static bool AddRequestDataHistoryEntry(int requestID, int requestDataID,
            int batchID, string entryText, RequestDataHistoryType entryType)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_ADD_REQUEST_DATA_HISTORY_ENTRY))
            {

                try
                {
                    db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);
                    db.AddInParameter(cmd, "@RequestDataID", DbType.Int32, requestDataID);
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, batchID);
                    db.AddInParameter(cmd, "@Entry", DbType.String, entryText);
                    db.AddInParameter(cmd, "@EntryType", DbType.String, entryType.ToString());

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "AddRequestDataHistoryEntry", ex);
                    throw new Exception("Error in RequestQuery.AddRequestDataHistoryEntry", ex);
                }
            }

            return succeeded;
        }

        /// <summary>
        /// Updates the Location field for the specified RequestData object.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <returns></returns>
        public static bool SaveDocumentLocation(RequestData dataObject)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_SAVE_DOCUMENT_LOCATION))
            {
                DbConnection conn = db.CreateConnection();
                conn.Open();
                DbTransaction transaction = conn.BeginTransaction();

                try
                {
                    db.AddInParameter(cmd, "@RequestDataID", DbType.Int32, dataObject.RequestDataID);
                    db.AddInParameter(cmd, "@ActionType", DbType.String, dataObject.ActionType.ToString());
                    db.AddInParameter(cmd, "@Location", DbType.String, dataObject.Location.ToString());

                    QueryWrapper.ExecuteNonQuery(db, cmd, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "SaveDocumentLocation", ex);
                    throw new Exception("Error in RequestQuery.SaveDocumentLocation", ex);
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

        public static Dictionary<int, DocumentVersionEntry> LoadDocumentLocationMap(int requestID)
        {
            Dictionary<int, DocumentVersionEntry> documentMap = null;

            Database dbconn = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = dbconn.GetStoredProcCommand(StoredProcNames.SP_GET_DOCUMENT_LOCATION_MAP))
            {

                try
                {
                    DataSet results;
                    dbconn.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);

                    results = QueryWrapper.ExecuteDataSet(dbconn, cmd);
                    documentMap = RequestMapper.LoadDocumentLocationMap(results.Tables[0]);
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadDocumentLocationMap", ex);
                    throw new Exception("Error in RequestQuery.LoadDocumentLocationMap", ex);
                }
            }

            return documentMap;
        }

        public static Dictionary<int, DocumentStatusEntry> LoadDocumentStatusMap(int requestID)
        {
            Dictionary<int, DocumentStatusEntry> statusMap = null;

            Database dbconn = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = dbconn.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_DOCUMENT_STATUS_MAP))
            {

                try
                {
                    DataSet results;
                    dbconn.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);

                    results = QueryWrapper.ExecuteDataSet(dbconn, cmd);
                    statusMap = RequestMapper.LoadDocumentStatusMap(results.Tables[0]);
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadDocumentStatusMap", ex);
                    throw new Exception("Error in RequestQuery.LoadDocumentStatusMap", ex);
                }
            }

            return statusMap;
        }

        public static bool MarkDocumentWithErrors(int requestDataID)
        {
            bool succeeded = true;
            int requestID;
            int groupID;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_DATA_IDENTIFIERS))
            {
                try
                {
                    db.AddInParameter(cmd, "@RequestDataID", DbType.Int32, requestDataID);
                    db.AddOutParameter(cmd, "@RequestID", DbType.Int32, 10);
                    db.AddOutParameter(cmd, "@GroupID", DbType.Int32, 10);

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                    requestID = (int)db.GetParameterValue(cmd, "@RequestID");
                    groupID = (int)db.GetParameterValue(cmd, "@GroupID");
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "MarkDocumentWithErrors", ex);
                    throw;
                }
            }

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_MARK_DOCUMENT_WITH_ERRORS))
            {
                DbConnection conn = db.CreateConnection();
                conn.Open();
                DbTransaction transaction = conn.BeginTransaction();

                try
                {
                    db.AddInParameter(cmd, "@RequestDataID", DbType.Int32, requestDataID);
                    db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);
                    db.AddInParameter(cmd, "@GroupID", DbType.Int32, groupID);

                    QueryWrapper.ExecuteNonQuery(db, cmd/*, transaction*/);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "MarkDocumentWithErrors", ex);
                    throw new Exception("Error in RequestQuery.MarkDocumentWithErrors", ex);
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

        public static bool MarkDocumentWithWarnings(int requestDataID)
        {
            bool succeeded = true;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_MARK_DOCUMENT_WITH_WARNINGS))
            {
                DbConnection conn = db.CreateConnection();
                conn.Open();

                try
                {
                    db.AddInParameter(cmd, "@RequestDataID", DbType.Int32, requestDataID);
                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "MarkDocumentWithWarnings", ex);
                    throw new Exception("Error in RequestQuery.MarkDocumentWithWarnings", ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return succeeded;
        }

        public static DataSet LoadDataHistoryDS(int reqDataID, string EntryType, bool DebugChecked)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            DataSet ds;

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_DATA_HISTORY))
            {

                try
                {
                    db.AddInParameter(cmd, "@RequestDataID", DbType.Int32, reqDataID);
                    if (Strings.Clean(EntryType) != null)
                        db.AddInParameter(cmd, "@EntryType", DbType.AnsiString, EntryType);
                    if (DebugChecked)
                        db.AddInParameter(cmd, "@DebugType", DbType.AnsiString, "Debug");
                    db.AddOutParameter(cmd, "@Status_Code", DbType.Int32, 10);
                    db.AddOutParameter(cmd, "@Status_Text", DbType.AnsiString, 255);
                    ds = db.ExecuteDataSet(cmd);

                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadDataHistoryDS", ex);
                    throw new Exception("Error in RequestQuery.LoadDataHistoryDS", ex);
                }
            }

            return ds;
        }

        public static DataSet LoadBatchHistoryDS(int reqBatchID)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            DataSet ds;

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_BATCH_HISTORY))
            {

                try
                {
                    db.AddInParameter(cmd, "@BatchID", DbType.Int32, reqBatchID);
                    db.AddOutParameter(cmd, "@Status_Code", DbType.Int32, 10);
                    db.AddOutParameter(cmd, "@Status_Text", DbType.AnsiString, 255);
                    ds = db.ExecuteDataSet(cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadBatchHistoryDS", ex);
                    throw new Exception("Error in RequestQuery.LoadBatchHistoryDS", ex);
                }
            }

            return ds;
        }

        public static DataSet LoadReqBatchHistoryData(int ReqID)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            DataSet ds;

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_BATCH_HISTORY))
            {
                try
                {
                    db.AddInParameter(cmd, "@RequestID", DbType.Int32, ReqID);
                    db.AddOutParameter(cmd, "@Status_Code", DbType.Int32, 10);
                    db.AddOutParameter(cmd, "@Status_Text", DbType.AnsiString, 255);
                    ds = db.ExecuteDataSet(cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadReqBatchHistoryData", ex);
                    throw new Exception("Error in RequestQuery.LoadReqBatchHistoryData", ex);
                }
            }

            return ds;
        }

        public static DataSet GetReports(int nNumber)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.Staging.ToString());
            DataSet ds;

            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REPORTS))
            {
                try
                {
                    db.AddInParameter(cmd, "@ReportNum", DbType.Int32, nNumber);
                    db.AddOutParameter(cmd, "@Status_Code", DbType.Int32, 10);
                    db.AddOutParameter(cmd, "@Status_Text", DbType.AnsiString, 255);
                    ds = db.ExecuteDataSet(cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "GetReports", ex);
                    throw new Exception("Error in RequestQuery.GetReports", ex);
                }
            }

            return ds;
        }

        public static Hashtable LoadRequestCounts(int ReqID)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_COUNTS);
            DataSet ds = null;
            Hashtable htblTypeCounts = new Hashtable();

            try
            {
                db.AddInParameter(cmd, "@RequestID", DbType.Int32, ReqID);
                db.AddOutParameter(cmd, "@Status_Code", DbType.Int32, 10);
                db.AddOutParameter(cmd, "@Status_Text", DbType.AnsiString, 255);
                ds = db.ExecuteDataSet(cmd);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int nDataSetId = (int)row[0];
                    foreach (int i in Enum.GetValues(typeof(CDRDocumentType)))
                    {
                        if (nDataSetId == i)
                            htblTypeCounts[Enum.GetName(typeof(CDRDocumentType), i)] = row[1];
                    }
                }
            }
            catch (Exception ex)
            {
                RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestCounts", ex);
                throw new Exception("Error in RequestQuery.LoadRequestCounts", ex);
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
                if (cmd != null)
                    cmd.Dispose();
            }

            return htblTypeCounts;
        }

        /// <summary>
        /// Looks up the exteranal requestIDs associated with the versions of a document in the
        /// GateKeeper, Staging, Preview and Live promotion levels.  Based on the input, either a single
        /// document will be reported, or the entire universe of documents.
        /// </summary>
        /// <param name="CdrID">The CDR document ID to be looked up.  If set to zero, the known universe
        /// of documents will be reported.</param>
        /// <returns></returns>
        public static List<RequestLocationExternalIds> LoadRequestLocationExternalIds(int CdrID)
        {
            List<RequestLocationExternalIds> results;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_LOCATION_EXTERNAL_IDS);
            DataSet ds = null;
            try
            {
                db.AddInParameter(cmd, "@CdrID", DbType.Int32, CdrID);
                ds = QueryWrapper.ExecuteDataSet(db, cmd);

                results = RequestMapper.LoadDocumentRequestExternalIdList(ds.Tables[0]);
            }
            catch (Exception ex)
            {
                RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestLocationExternalIds", ex);
                throw;
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
                if (cmd != null)
                    cmd.Dispose();
            }

            return results;
        }

        public static List<RequestLocationInternalIds> LoadRequestLocationInternalIds(int DocType, int CdrID)
        {
            List<RequestLocationInternalIds> results;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_LOCATION);
            DataSet ds = null;
            try
            {
                db.AddInParameter(cmd, "@DataSetID", DbType.Int32, DocType);
                db.AddInParameter(cmd, "@CdrID", DbType.Int32, CdrID);
                ds = QueryWrapper.ExecuteDataSet(db, cmd);

                results = RequestMapper.LoadDocumentRequestList(ds.Tables[0]);
            }
            catch (Exception ex)
            {
                RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "LoadRequestLocationInternalIds", ex);
                throw;
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
                if (cmd != null)
                    cmd.Dispose();
            }

            return results;
        }

        /// <summary>
        /// Confirms that the various GateKeeper databases are available.
        /// </summary>
        /// <param name="ready">Returns as True if the database is available, false otherwise.</param>
        /// <param name="message">Returns null under normal circumstances, a message containing
        /// the failing database name(s) otherwise.</param>
        public static void CheckDatabaseStatus(out bool ready, out string message)
        {
            ContentDatabase[] dbList = { ContentDatabase.GateKeeper, ContentDatabase.Staging,
                ContentDatabase.Preview, ContentDatabase.Live };

            Database db;
            DbCommand cmd;
            string query = "select db_name()";
            string result;
            StringBuilder sb = new StringBuilder();

            ready = true;

            foreach (ContentDatabase dbName in dbList)
            {
                try
                {
                    db = DatabaseFactory.CreateDatabase(dbName.ToString());
                    using (cmd = db.GetSqlStringCommand(query))
                    {
                        result = (string)db.ExecuteScalar(cmd);
                        if (result == null ||
                            result.Length == 0)
                        {
                            ready = false;
                            if (sb.Length > 0)
                                sb.Append('\n');
                            sb.Append(string.Format("Database {0} not ready.", dbName.ToString()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ready = false;
                    if (sb.Length > 0)
                        sb.Append('\n');
                    sb.Append(string.Format("Database {0} not ready.", dbName.ToString()));
                    RequestMgrLogBuilder.Instance.CreateCritical(typeof(RequestQuery), "CheckDatabaseStatus", ex);
                }
            }

            message = sb.ToString();
        }

        public static string GetMostRecentExternalID(string requestSource)
        {
            string externalID;

            Database dbconn = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = dbconn.GetStoredProcCommand(StoredProcNames.SP_GET_MOST_RECENT_EXTERNAL_ID))
            {

                try
                {
                    dbconn.AddInParameter(cmd, "@RequestSource", DbType.String, requestSource);
                    dbconn.AddOutParameter(cmd, "@MostRecentExternalID", DbType.String, 50);
                    QueryWrapper.ExecuteNonQuery(dbconn, cmd);

                    externalID = (string)dbconn.GetParameterValue(cmd, "@MostRecentExternalID");
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "GetMostRecentExternalID", ex);
                    throw new Exception("Error in RequestQuery.GetMostRecentExternalID", ex);
                }
            }

            return externalID;
        }

        public static void SetGateKeeperSystemStatus(SystemStatusType status)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_SET_GATEKEEPER_CONTROL_VALUE))
            {
                try
                {
                    db.AddInParameter(cmd, "@SettingName", DbType.String, "SystemStatus");
                    db.AddInParameter(cmd, "@SettingValue", DbType.String, status.ToString());

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "SetGateKeeperSystemStatus", ex);
                    throw new Exception("Error in RequestQuery.SetGateKeeperSystemStatus", ex);
                }
            }
        }

        public static SystemStatusType GetGateKeeperSystemStatus()
        {
            SystemStatusType systemStatus = SystemStatusType.Invalid;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_GATEKEEPER_CONTROL_VALUE))
            {
                try
                {
                    DataSet results;

                    db.AddInParameter(cmd, "@SettingName", DbType.String, "SystemStatus");
                    results = QueryWrapper.ExecuteDataSet(db, cmd);
                    systemStatus = RequestMapper.LoadSystemStatus(results);
                    results.Dispose();
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "GetGateKeeperSystemStatus", ex);
                    throw new Exception("Error in RequestQuery.GetGateKeeperSystemStatus", ex);
                }
            }

            return systemStatus;
        }

        public static RequestStatusType GetRequestStatus(int requestID)
        {
            RequestStatusType status = RequestStatusType.Invalid;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_STATUS))
            {
                try
                {
                    string result;

                    db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);
                    db.AddOutParameter(cmd, "@Status", DbType.String, 50);
                    QueryWrapper.ExecuteNonQuery(db, cmd);

                    result = (string)db.GetParameterValue(cmd, "@Status");
                    status = ConvertEnum<RequestStatusType>.Convert(result);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "GetRequestStatus", ex);
                    throw new Exception("Error in RequestQuery.GetRequestStatus", ex);
                }
            }

            return status;
        }

        public static RequestStatusType GetRequestStatusFromDocumentID(int requestDataID)
        {
            RequestStatusType status = RequestStatusType.Invalid;

            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_GET_REQUEST_STATUS_FROM_DOCUMENT_ID))
            {
                try
                {
                    string result;

                    db.AddInParameter(cmd, "@RequestDataID", DbType.Int32, requestDataID);
                    db.AddOutParameter(cmd, "@Status", DbType.String, 50);
                    QueryWrapper.ExecuteNonQuery(db, cmd);

                    result = (string)db.GetParameterValue(cmd, "@Status");
                    status = ConvertEnum<RequestStatusType>.Convert(result);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "GetRequestStatusFromDocumentID", ex);
                    throw new Exception("Error in RequestQuery.GetRequestStatusFromDocumentID", ex);
                }
            }

            return status;
        }

        public static void AddRequestComment(int requestID, string comment)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_ADD_REQUEST_COMMENT))
            {
                try
                {
                    db.AddInParameter(cmd, "@RequestID", DbType.Int32, requestID);
                    db.AddInParameter(cmd, "@Comment", DbType.String, comment);

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "AddRequestComment", ex);
                    throw new Exception("Error in RequestQuery.AddRequestComment", ex);
                }
            }
        }

        public static void AddRequestComment(string externalID, string requestSource, string comment)
        {
            Database db = DatabaseFactory.CreateDatabase(ContentDatabase.GateKeeper.ToString());
            using (DbCommand cmd = db.GetStoredProcCommand(StoredProcNames.SP_ADD_REQUEST_COMMENT_EXTERNAL_ID))
            {
                try
                {
                    db.AddInParameter(cmd, "@ExternalRequestID", DbType.String, externalID);
                    db.AddInParameter(cmd, "@Source", DbType.String, requestSource);
                    db.AddInParameter(cmd, "@Comment", DbType.String, comment);

                    QueryWrapper.ExecuteNonQuery(db, cmd);
                }
                catch (Exception ex)
                {
                    RequestMgrLogBuilder.Instance.CreateError(typeof(RequestQuery), "AddRequestComment", ex);
                    throw new Exception("Error in RequestQuery.AddRequestComment", ex);
                }
            }
        }

    }
}
