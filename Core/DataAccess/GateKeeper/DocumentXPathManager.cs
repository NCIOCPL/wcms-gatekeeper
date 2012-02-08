using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.Common;

using GateKeeper.DataAccess;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.StoreProcedures;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace GateKeeper.DataAccess.GateKeeper
{
    public class DocumentXPathManager: DbBaseQuery
    {

        #region Private variable
            private Hashtable _documentXPath = new Hashtable();
        #endregion

        #region Constructors

        /// <summary>
        /// Class constructor.
        /// </summary>
        public DocumentXPathManager()
        {
            GetDocumentXPath();
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Retrieves the XPath expression named by key.
        /// </summary>
        /// <param name="key">The name of the XPath expression</param>
        /// <returns>String containing an XPath expression.</returns>
        public string GetXPath(string key)
        {
            string value = string.Empty;
            if (_documentXPath.ContainsKey(key))
                value = _documentXPath[key].ToString();
            else
                throw new Exception("XPath Manager Error: Retrieving xPath for " + key + " failed.");

            return value;
        }

        /// <summary>
        /// Retrieves the XPath expression named by key, replacing format specifiers
        /// with the string equivalent of the values in args.
        /// </summary>
        /// <param name="key">The name of the XPath expression</param>
        /// <param name="args">Array of objects to use in the format specifier.</param>
        /// <returns>String containing an XPath expression.</returns>
        public string GetXPath(string key, params object[] args)
        {
            string value = GetXPath(key);

            return string.Format(value, args);
        }

        #endregion

        #region Query for Store Procedure Calls

        /// <summary>
        /// Save glossary term document into CDR staging database
        /// </summary>
        /// <param name="glossaryDoc">
        /// Glossary Term document object
        /// </param>
        private Hashtable GetDocumentXPath()
        {
            IDataReader reader = null;
            Database db = this.GateKeeperDBWrapper.SetDatabase();
            try
            {
                // SP: Retrieve all the document xpath
                string spGetDocumentXPath = SPDocumentXPath.SP_GET_DOCUMENT_XPATH;
                using (DbCommand xPathCommand = db.GetStoredProcCommand(spGetDocumentXPath))
                {
                    xPathCommand.CommandType = CommandType.StoredProcedure;
                    reader = db.ExecuteReader(xPathCommand);

                    while (reader.Read())
                    {
                        string key = reader["Name"].ToString();
                        string value = reader["QueryText"].ToString();
                        _documentXPath.Add(key, value);
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Running store procedures for retrieving document xpath failed.", e);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        
            return _documentXPath;
        }
        #endregion
    }
}
