using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using GateKeeper.Common;
using GKManagers.BusinessObjects;

namespace GKManagers.DataAccess
{
    static class BatchMapper
    {
        public static Batch LoadBatch(DataSet data)
        {
            Batch results = null;

            if (data != null && 
                data.Tables.Count == 3 &&
                data.Tables[0].Rows.Count == 1)
            {
                results = new Batch();

                // Load scalar items from Batch table.
                results.BatchID = (int)data.Tables[0].Rows[0]["BatchID"];
                results.Status = ConvertEnum<BatchStatusType>.Convert(data.Tables[0].Rows[0]["Status"]);
                results.BatchName = (string)data.Tables[0].Rows[0]["BatchName"];
                results.UserName = (string)data.Tables[0].Rows[0]["UserName"];

                // Load the list of RequestData IDs.
                foreach (DataRow row in data.Tables[1].Rows)
                {
                    int requestID = (int)row["RequestDataID"];
                    results.RequestDataIDs.Add(requestID);
                }

                // Load the list of Publication actions
                foreach (DataRow row in data.Tables[2].Rows)
                {
                    ProcessActionType action;
                    action = ConvertEnum<ProcessActionType>.Convert(row["name"]);
                    results.Actions.Add(action);
                }
            }

            return results;
        }

        public static List<int> LoadBatchList(DataTable data)
        {
            // Allow the possibility of returning an empty list.
            List<int> results = new List<int>();

            foreach (DataRow row in data.Rows)
            {
                results.Add((int)row["BatchID"]);
            }

            return results;
        }

        public static int LoadBatchID(DataSet data)
        {
            int result = -1;

            if (data != null &&
                data.Tables.Count == 1 &&
                data.Tables[0] != null &&
                data.Tables[0].Rows.Count >= 1)
            {

                object item = data.Tables[0].Rows[0]["BatchID"];
                if (item != DBNull.Value)
                    result = (int)item;
            }

            return result;
        }
    }
}
