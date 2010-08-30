using System;
using System.Collections;
using System.Text;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace GateKeeper.DataAccess
{
    public class DataAccessManager : IDisposable
    {

        private ArrayList dbWrappers = new ArrayList();
        private string[] databaseList = { "GateKeeper", "Staging", "Preview", "Live", "CancerGovStaging", "CancerGov" };

        /// <summary>
        /// Looks up a connection int the configuration cache based on a connection id
        /// and creates the appropriate database wrapper, or gives you back an existing wrapper if one has
        /// already been created for that connection
        /// </summary>
        /// <param name="lConnectionID"></param>
        /// <returns></returns>
        public DbWrapper GetDatabaseWrapper(string databaseName)
        {
            // This lock is to make sure the DataAccessManager should not be accessed across threads
            lock (dbWrappers) 
            {
                foreach (DbWrapper dbw in dbWrappers)
                {
                    if (dbw.DatabaseName.Equals(databaseName))
                    {
                        return dbw;
                    }
                }

                // The specified database has not been connected yet. Create one.
                foreach (string db in databaseList)
                {
                    if (db.Equals(databaseName) )
                    {
                        DbWrapper dbWrapper = new DbWrapper(databaseName);
                        dbWrappers.Add(dbWrapper);
                        return dbWrapper;
                    }
                }
                throw new ArgumentException("Database name= " + databaseName.ToString() + " not found in config", "databaseName");
            }
        }

        /// <summary>
        /// Create database.
        /// </summary>
        /// <param name="databaseName">
        /// 
        /// </param>
        internal static Database CreateDatabase(string databaseName)
        {
            Database db = null;
            db = DatabaseFactory.CreateDatabase(databaseName);
            return db;
        }

        /// <summary>
        /// Create database connection.
        /// </summary>
        /// <param name="bCloseFirst">
        /// 
        /// </param>
        internal static DbConnection CreateConnection(Database database)
        {
     
            DbConnection conn = null;
            conn = database.CreateConnection();
    
            return conn;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Free managed resources only.
            if (disposing)
            {
                foreach (DbWrapper dbw in dbWrappers)
                {
                    dbw.Dispose();
                }
                dbWrappers.Clear();
            }
        }

    }
}
