using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using GateKeeper.Common;
using GKManagers.BusinessObjects;

namespace GKManagers.DataAccess
{
    static class RequestMapper
    {
        public static Request LoadRequest(DataTable data)
        {
            Request requestItem = new Request();

            object column;

            // There should be only one row to load.
            requestItem.RequestID = (int)data.Rows[0]["RequestID"];
            requestItem.ExternalRequestID = (string)data.Rows[0]["ExternalRequestID"];
            requestItem.RequestPublicationType = 
                ConvertEnum<RequestPublicationType>.Convert(data.Rows[0]["RequestType"]);
            requestItem.Description = (string)data.Rows[0]["Description"];
            requestItem.Status = ConvertEnum<RequestStatusType>.Convert(data.Rows[0]["Status"]);
            requestItem.Source = (string)data.Rows[0]["Source"];
            requestItem.DtdVersion = (string)data.Rows[0]["DTDVersion"];

            column = data.Rows[0]["ExpectedDocCount"];
            if (column != DBNull.Value)
                requestItem.ExpectedDocCount = (int)column;
            else
                requestItem.ExpectedDocCount = Request.IgnoreDocumentCount;
            requestItem.ActualDocCount = (int)data.Rows[0]["ActualDocCount"];
            requestItem.DataType = (string)data.Rows[0]["DataType"];
            requestItem.InitiateDate = (DateTime)data.Rows[0]["InitiateDate"];

            // Allow for the request to not be completed yet.
            if( data.Rows[0]["CompleteReceivedTime"] != DBNull.Value)
                requestItem.CompleteReceivedTime = (DateTime)data.Rows[0]["CompleteReceivedTime"];

            requestItem.PublicationTarget =
                ConvertEnum<RequestTargetType>.Convert(data.Rows[0]["PublicationTarget"], RequestTargetType.Invalid);
            requestItem.UpdateDate = (DateTime)data.Rows[0]["UpdateDate"];
            requestItem.UpdateUserID = (string)data.Rows[0]["UpdateUserID"];

            column = data.Rows[0]["MaxPacketNumber"];
            if (column != DBNull.Value)
                requestItem.MaxPacketNumber = (int)(column);
            else
                requestItem.MaxPacketNumber = 0;

            return requestItem;
        }

        public static RequestData LoadRequestDataItem(DataRow data)
        {
            RequestData dataItem = new RequestData();
            LoadRequestDataInfo(dataItem, data);

            if (data["Data"] != DBNull.Value)
                dataItem.DocumentDataString = (string)data["Data"];

            return dataItem;
        }

        public static RequestDataInfo LoadRequestDataInfo(DataRow data)
        {
            RequestDataInfo dataItem = new RequestData();
            LoadRequestDataInfo(dataItem, data);
            return dataItem;
        }

        /// <summary>
        /// Common code for loading RequestData meta data.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="data"></param>
        private static void LoadRequestDataInfo(RequestDataInfo dataItem, DataRow data)
        {
            if (dataItem == null)
                throw new ArgumentNullException("dataItem");
            if (data == null)
                throw new ArgumentNullException("data");

            dataItem.RequestDataID = (int)data["RequestDataID"];
            dataItem.RequestID = (int)data["RequestID"];
            dataItem.PacketNumber = (int)data["PacketNumber"];
            dataItem.ActionType = ConvertEnum<RequestDataActionType>.Convert(data["ActionType"]);
            dataItem.CDRDocType = (CDRDocumentType)data["DataSetID"];
            dataItem.CdrID = (int)data["CDRID"];
            dataItem.CdrVersion = (string)data["CDRVersion"];
            dataItem.ReceivedDate = (DateTime)data["ReceivedDate"];
            dataItem.Status = ConvertEnum<RequestDataStatusType>.Convert(data["Status"]);
            dataItem.DependencyStatus =
                ConvertEnum<RequestDataDependentStatusType>.Convert(data["DependencyStatus"]);
            dataItem.Location = ConvertEnum<RequestDataLocationType>.Convert(data["Location"]);
            dataItem.GroupID = (int)data["GroupID"];
        }

        public static List<int> LoadRequestDataIDList(DataTable data)
        {
            List<int> dataIDs = new List<int>();

            foreach (DataRow currRow in data.Rows)
            {
                dataIDs.Add((int)currRow["requestDataID"]);
            }

            return dataIDs;
        }

        public static Dictionary<int, DocumentVersionEntry> LoadDocumentLocationMap(DataTable data)
        {
            Dictionary<int, DocumentVersionEntry> map = new Dictionary<int, DocumentVersionEntry>();
            DocumentVersionEntry entry = null;

            int cdrid;
            int groupID;
            int stagingVersion;
            int previewVersion;
            int liveVersion;
            Object column;

            int cdrIdColumn = data.Columns.IndexOf("cdrid");
            int groupIdColumn = data.Columns.IndexOf("groupid");
            int stagingColumn = data.Columns.IndexOf("stagingID");
            int previewColumn = data.Columns.IndexOf("previewID");
            int liveColumn = data.Columns.IndexOf("liveID");

            foreach (DataRow currRow in data.Rows)
            {
                cdrid = (int)currRow[cdrIdColumn];

                groupID = (int)currRow[groupIdColumn];

                column = currRow[stagingColumn];
                if (column == DBNull.Value)
                    stagingVersion = -1;
                else
                    stagingVersion = (int)column;

                column = currRow[previewColumn];
                if (column == DBNull.Value)
                    previewVersion = -1;
                else
                    previewVersion = (int)column;

                column = currRow[liveColumn];
                if (column == DBNull.Value)
                    liveVersion = -1;
                else
                    liveVersion = (int)column;

                entry = new DocumentVersionEntry(groupID, stagingVersion, previewVersion, liveVersion);
                map.Add(cdrid, entry);
            }

            return map;
        }

        public static Dictionary<int, DocumentStatusEntry> LoadDocumentStatusMap(DataTable data)
        {
            Dictionary<int, DocumentStatusEntry> map = new Dictionary<int, DocumentStatusEntry>();
            DocumentStatusEntry entry = null;

            int requestDataID;
            RequestDataStatusType status;
            RequestDataDependentStatusType dependencyStatus;
            Object column;

            foreach (DataRow currRow in data.Rows)
            {
                requestDataID = (int)currRow["requestDataID"];

                column = currRow["Status"];
                if (column == DBNull.Value)
                {
                    status = RequestDataStatusType.Invalid;
                }
                else
                {
                    status =
                        ConvertEnum<RequestDataStatusType>.Convert(column, RequestDataStatusType.Invalid);
                }

                column = currRow["DependencyStatus"];
                if (column == DBNull.Value)
                {
                    dependencyStatus = RequestDataDependentStatusType.Invalid;
                }
                else
                {
                    dependencyStatus =
                        ConvertEnum<RequestDataDependentStatusType>.Convert(column, RequestDataDependentStatusType.Invalid);
                }

                entry = new DocumentStatusEntry(status, dependencyStatus);
                map.Add(requestDataID, entry);
            }

            return map;
        }

        public static SystemStatusType LoadSystemStatus(DataSet data)
        {
            SystemStatusType status = SystemStatusType.Stopped;

            if (data != null &&
                data.Tables.Count == 1 &&
                data.Tables[0] != null &&
                data.Tables[0].Rows.Count >= 1)
            {
                object column = data.Tables[0].Rows[0]["CSValue"];
                status = ConvertEnum<SystemStatusType>.Convert(column, SystemStatusType.Stopped);
            }

            return status;
        }

        public static List<RequestLocationInternalIds> LoadDocumentRequestList(DataTable data)
        {
            List<RequestLocationInternalIds> results;

            // Find column positions (faster than looking them up repeatedly)
            int cdridColumn = data.Columns.IndexOf("cdrid");
            int docTypeColumn = data.Columns.IndexOf("DocType");
            int gateKeeperIdColumn = data.Columns.IndexOf("gatekeeper");
            int gateKeeperDateColumn = data.Columns.IndexOf("gatekeeperDateTime");
            int stagingIdColumn = data.Columns.IndexOf("staging");
            int stagingDateColumn = data.Columns.IndexOf("stagingDateTime");
            int previewIdColumn = data.Columns.IndexOf("preview");
            int previewDateColumn = data.Columns.IndexOf("previewDateTime");
            int liveIdColumn = data.Columns.IndexOf("live");
            int liveDateColumn = data.Columns.IndexOf("liveDateTime");

            #region Column Validation
            if (cdridColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "cdrid"));
            if (docTypeColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "DocType"));
            if (gateKeeperIdColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "gatekeeper"));
            if (gateKeeperDateColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "gatekeeperDateTime"));
            if (stagingIdColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "staging"));
            if (stagingDateColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "stagingDateTime"));
            if (previewIdColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "preview"));
            if (previewDateColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "previewDateTime"));
            if (liveIdColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "live"));
            if (liveDateColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "liveDateTime"));
            #endregion

            int cdrid;
            CDRDocumentType docType = CDRDocumentType.Invalid;
            int gateKeeperId, stagingId, previewId, liveId;
            DateTime gateKeeperDate, stagingDate, previewDate, liveDate;

            // Load the data
            results = new List<RequestLocationInternalIds>();
            foreach (DataRow row in data.Rows)
            {
                cdrid = (int)row[cdridColumn];
                docType = (CDRDocumentType)row[docTypeColumn]; //ConvertEnum<CDRDocumentType>.Convert(row[docTypeColumn]);

                // Request IDs
                if (row[gateKeeperIdColumn] != DBNull.Value)
                    gateKeeperId = (int)row[gateKeeperIdColumn];
                else
                    gateKeeperId = RequestLocationInternalIds.LocationNotPresent;

                if (row[stagingIdColumn] != DBNull.Value)
                    stagingId = (int)row[stagingIdColumn];
                else
                    stagingId = RequestLocationInternalIds.LocationNotPresent;

                if (row[previewIdColumn] != DBNull.Value)
                    previewId = (int)row[previewIdColumn];
                else
                    previewId = RequestLocationInternalIds.LocationNotPresent;

                if (row[liveIdColumn] != DBNull.Value)
                    liveId = (int)row[liveIdColumn];
                else
                    liveId = RequestLocationInternalIds.LocationNotPresent;

                // Request date/time (Null if never promoted this far)
                if (row[gateKeeperDateColumn] != DBNull.Value)
                    gateKeeperDate = (DateTime)row[gateKeeperDateColumn];
                else
                    gateKeeperDate = DateTime.MinValue;

                if (row[stagingDateColumn] != DBNull.Value)
                    stagingDate = (DateTime)row[stagingDateColumn];
                else
                    stagingDate = DateTime.MinValue;

                if (row[previewDateColumn] != DBNull.Value)
                    previewDate = (DateTime)row[previewDateColumn];
                else
                    previewDate = DateTime.MinValue;

                if (row[liveDateColumn] != DBNull.Value)
                    liveDate = (DateTime)row[liveDateColumn];
                else
                    liveDate = DateTime.MinValue;

                results.Add(new RequestLocationInternalIds(cdrid, gateKeeperId, gateKeeperDate,
                    stagingId, stagingDate, previewId, previewDate, liveId, liveDate, docType));
            }

            return results;
        }

        public static List<RequestLocationExternalIds> LoadDocumentRequestExternalIdList(DataTable data)
        {
            List<RequestLocationExternalIds> results;

            // Find column positions (faster than looking them up repeatedly)
            int cdridColumn = data.Columns.IndexOf("cdrid");
            int gateKeeperIdColumn = data.Columns.IndexOf("gatekeeper");
            int gateKeeperDateColumn = data.Columns.IndexOf("gatekeeperDateTime");
            int stagingIdColumn = data.Columns.IndexOf("staging");
            int stagingDateColumn = data.Columns.IndexOf("stagingDateTime");
            int previewIdColumn = data.Columns.IndexOf("preview");
            int previewDateColumn = data.Columns.IndexOf("previewDateTime");
            int liveIdColumn = data.Columns.IndexOf("live");
            int liveDateColumn = data.Columns.IndexOf("liveDateTime");

            #region Column Validation
            if (cdridColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "cdrid"));
            if (gateKeeperIdColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "gatekeeper"));
            if (gateKeeperDateColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "gatekeeperDateTime"));
            if (stagingIdColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "staging"));
            if (stagingDateColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "stagingDateTime"));
            if (previewIdColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "preview"));
            if (previewDateColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "previewDateTime"));
            if (liveIdColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "live"));
            if (liveDateColumn < 0)
                throw new Exception(string.Format("Column '{0}' not found.", "liveDateTime"));
            #endregion

            int cdrid;
            string gateKeeperId, stagingId, previewId, liveId;
            DateTime gateKeeperDate, stagingDate, previewDate, liveDate;

            // Load the data
            results = new List<RequestLocationExternalIds>();
            foreach (DataRow row in data.Rows)
            {
                cdrid = (int)row[cdridColumn];

                // Request IDs
                if (row[gateKeeperIdColumn] != DBNull.Value)
                    gateKeeperId = (string)row[gateKeeperIdColumn];
                else
                    gateKeeperId = string.Empty;

                if (row[stagingIdColumn] != DBNull.Value)
                    stagingId = (string)row[stagingIdColumn];
                else
                    stagingId = string.Empty;

                if (row[previewIdColumn] != DBNull.Value)
                    previewId = (string)row[previewIdColumn];
                else
                    previewId = string.Empty;

                if (row[liveIdColumn] != DBNull.Value)
                    liveId = (string)row[liveIdColumn];
                else
                    liveId = string.Empty;

                // Request date/time (Null if never promoted this far)
                if (row[gateKeeperDateColumn] != DBNull.Value)
                    gateKeeperDate = (DateTime)row[gateKeeperDateColumn];
                else
                    gateKeeperDate = DateTime.MinValue;

                if (row[stagingDateColumn] != DBNull.Value)
                    stagingDate = (DateTime)row[stagingDateColumn];
                else
                    stagingDate = DateTime.MinValue;

                if (row[previewDateColumn] != DBNull.Value)
                    previewDate = (DateTime)row[previewDateColumn];
                else
                    previewDate = DateTime.MinValue;

                if (row[liveDateColumn] != DBNull.Value)
                    liveDate = (DateTime)row[liveDateColumn];
                else
                    liveDate = DateTime.MinValue;

                results.Add(new RequestLocationExternalIds(cdrid, gateKeeperId, gateKeeperDate,
                    stagingId, stagingDate, previewId, previewDate, liveId, liveDate));
            }

            return results;
        }
    }
}
