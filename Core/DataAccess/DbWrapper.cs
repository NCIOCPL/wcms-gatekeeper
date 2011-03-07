using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using GateKeeper.Logging;

namespace GateKeeper.DataAccess
{
    public class DbWrapper: IDisposable
    {
        private string _databaseName;
        private DbConnection _dbConnection;
        private Database _database;

        public DbWrapper(string databaseName)
		{
			this._databaseName = databaseName;
		}

        public string DatabaseName
        {
            get
            {
                return this._databaseName;
            }
        }

        public Database SetDatabase()
        {
            if (_database == null)
            {
                _database = DataAccessManager.CreateDatabase(_databaseName);
            }
            return _database;
        }

        /// <summary>
        /// Make sure there is one database connection and the connection status is open.
        /// </summary>
        /// <param name="bCloseFirst">
        /// 
        /// </param>
       public DbConnection EnsureConnection()
        {
            try
            {
                if (_dbConnection != null && _dbConnection.State == ConnectionState.Open)
                {
                    //we're good
                    return _dbConnection;
                }
                //we're not good
                Connect(true);
            }
            catch (Exception e)
            {
                throw new Exception("Exception occurred in DocumentQuery " + this.GetType().Name + ".EnsureConnection()", e);
            }
            return _dbConnection;
        }

        /// <summary>
        /// Opens a database connection.
        /// </summary>
        /// <param name="bCloseFirst">
        /// Closes the connection first if one exists
        /// </param>
        private void Connect(bool bCloseFirst)
        {
            if (bCloseFirst == true)
                this.Close();

            if (_dbConnection == null)
            {
                _dbConnection = DataAccessManager.CreateConnection(_database);
            }
            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }
        }

        /// <summary>
        /// Closes the db connection
        /// </summary>
        private void Close()
        {
            if (_dbConnection != null)
                _dbConnection.Close();

            _dbConnection = null;
        }

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources only.
                if (_dbConnection != null)
                    Close();
                if (_database != null)
                    _database = null;
            }
        }

        #endregion

    }
}
