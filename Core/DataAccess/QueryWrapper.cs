using System;
using System.Data;
using System.Data.Common;

using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

using GateKeeper.Common;

namespace GateKeeper.DataAccess
{
    public class QueryWrapper
    {
        /// <summary>
        /// Executes the stored proc or query contained in a DbCommand.  This method should only
        /// be used with commands which do not require transactions.
        /// </summary>
        /// <param name="db">Database object to execute the command against.</param>
        /// <param name="command">DbCommand object referring to a stored procedure or query</param>
        /// <returns>A DataSet object containing the rows output by running the command. If no
        /// DataSet is returned, consider  using one of the ExecuteNonQuery overloads instead.</returns>
        public static DataSet ExecuteDataSet(Database db, DbCommand command)
        {
            return ExecuteDataSet(db, command, null);
        }

        /// <summary>
        /// Executes the stored proc or query contained in a DbCommand.  This method is used with
        /// commands which require transactions.  It is the responsibility of the caller to determine
        /// whether the transaction should be committed or rolledback.
        /// </summary>
        /// <param name="db">Database object to execute the command against.</param>
        /// <param name="command">DbCommand object referring to a stored procedure or query</param>
        /// <param name="transaction">DbTransaction object.</param>
        /// <returns>A DataSet object containing the rows output by running the command. If no
        /// DataSet is returned, consider  using one of the ExecuteNonQuery overloads instead.</returns>
        public static DataSet ExecuteDataSet(Database db, DbCommand command, DbTransaction transaction)
        {
            DataSet results = null;

            db.AddOutParameter(command, "@Status_Code", DbType.Int32, 10);
            db.AddOutParameter(command, "@Status_Text", DbType.String, 255);

            if (transaction != null)
            {
                results = db.ExecuteDataSet(command, transaction);
            }
            else
            {
                results = db.ExecuteDataSet(command);
            }

            int statusCode = (int)db.GetParameterValue(command, "@Status_Code");
            string statusText = (string)db.GetParameterValue(command, "@Status_Text");

            /*
             * Status codes
             * 
             *  0 - Success
             * -1 - Record not found
             * -2 - Duplicate record found
             * -3 - Request is already complete
             * 
             */
            if ((DBStatusCodeType)statusCode < DBStatusCodeType.Success)
            {
                results = null;

                string message = string.Format("Failed in call to {0}: {1}",
                    command.CommandText, statusText);

                FrameworkLogBuilder.Instance.CreateWarning(typeof(QueryWrapper), "ExecuteDataSet", message);
                throw new Exception(string.Format("Error ({0}): {1}", statusCode, statusText));
            }

            return results;
        }

        /// <summary>
        /// Executes the stored proc or query contained in a DbCommand.  This method should only
        /// be used with commands which do not require transactions.
        /// </summary>
        /// <param name="db">Database object to execute the command against.</param>
        /// <param name="command">DbCommand object referring to a stored procedure or query</param>
        /// <returns>The number of rows modified by the call.</returns>
        public static int ExecuteNonQuery(Database db, DbCommand command)
        {
            return ExecuteNonQuery(db, command, null);
        }

        /// <summary>
        /// Executes the stored proc or query contained in a DbCommand.  This method is used with
        /// commands which require transactions.  It is the responsibility of the caller to determine
        /// whether the transaction should be committed or rolledback.
        /// </summary>
        /// <param name="db">Database object to execute the command against.</param>
        /// <param name="command">DbCommand object referring to a stored procedure or query</param>
        /// <param name="transaction">DbTransaction object.</param>
        /// <returns>The number of rows modified by the call.</returns>
        public static int ExecuteNonQuery(Database db, DbCommand command, DbTransaction transaction)
        {
            int results = 0;

            db.AddOutParameter(command, "@Status_Code", DbType.Int32, 10);
            db.AddOutParameter(command, "@Status_Text", DbType.String, 255);

            if (transaction != null)
            {
                results = db.ExecuteNonQuery(command, transaction);
            }
            else
            {
                results = db.ExecuteNonQuery(command);
            }

            int statusCode = (int)db.GetParameterValue(command, "@Status_Code");
            string statusText = (string)db.GetParameterValue(command, "@Status_Text");

            /*
             * Status codes
             * 
             *  0 - Success
             * -1 - Record not found
             * -2 - Duplicate record found
             * -3 - Request is already complete
             * 
             */
            if ((DBStatusCodeType)statusCode < DBStatusCodeType.Success)
            {
                string message = string.Format("Failed in call to {0}: {1}",
                    command.CommandText, statusText);

                FrameworkLogBuilder.Instance.CreateWarning(typeof(QueryWrapper), "ExecuteNonQuery", message);
                throw new Exception(string.Format("Error ({0}): {1}", statusCode, statusText));
            }

            return results;
        }
    }
}
