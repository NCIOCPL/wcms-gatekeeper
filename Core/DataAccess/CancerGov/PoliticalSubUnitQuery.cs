using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Transactions;
using GateKeeper.DocumentObjects;
using GateKeeper.DataAccess.StoreProcedures;
using GateKeeper.DataAccess;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using GateKeeper.DocumentObjects.PoliticalSubUnit;

namespace GateKeeper.DataAccess.CancerGov
{
    public class PoliticalSubUnitQuery : DocumentQuery
    {
        /// <summary>
        /// Save PoliticalSubUnit document into CDR staging database
        /// </summary>
        /// <param name="glossaryDoc">
        /// Glossary Term document object
        /// </param>
        public override bool SaveDocument(Document document, string userID)
        {
            bool bSuccess = true;
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                PoliticalSubUnitDocument doc = (PoliticalSubUnitDocument)document;

                // SP: Clear extracted data
                ClearExtractedData(doc.DocumentID, db, transaction, userID);

                // SP: Save PoliticalSubUnit
                SavePoliticalSubUnit(doc, db, transaction, userID);
                SaveDBDocument(doc, db, transaction);

                transaction.Commit();
            }
            catch (Exception e)
            {
                bSuccess = false;
                transaction.Rollback();
                throw new Exception("Database Error: Saving PolitcalSubUnit document failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }

            return bSuccess;
        }

        /// <summary>
        /// Push PoliticalSubUnit document into preview database
        /// </summary>
        /// <param name="documentID">
        /// <param name="userID">
        /// </param>
        public override void PushDocumentToPreview(Document document, string userID)
        {
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                try
                {
                    // SP: Call push PoliticalSubUnit document
                    string spPushData = SPPoliticalSubUnit.SP_PUSH_EXTRACTED_POLITICALSUBUNIT_DATA;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, document.DocumentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedPoliticalSubUnitData failed.", e);
                }
                finally
                {
                    transaction.Dispose();
                }

                // SP: Call push document 
                PushDocument(document.DocumentID, db, ContentDatabase.Preview.ToString());

            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing PoliticalSubUnit document data to preview database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Push PoliticalSubUnit document into live database
        /// </summary>
        /// <param name="documentID">
        /// <param name="userID">
        /// </param>
        public override void PushDocumentToLive(Document document, string userID)
        {
            Database db = this.PreviewDBWrapper.SetDatabase();
            DbConnection conn = this.PreviewDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                // Rollback the change only if push PoliticalSubUnit sp failed.
                try
                {
                    // SP: Call push glossary term document
                    string spPushData = SPPoliticalSubUnit.SP_PUSH_EXTRACTED_POLITICALSUBUNIT_DATA;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, document.DocumentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedPoliticalSubUnitData failed", e);
                }

                // SP: Call Push document
                PushDocument(document.DocumentID, db, ContentDatabase.Live.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing PoliticalSubUnit document data to live database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Delete PoliticalSubUnit document in database
        /// </summary>
        /// <param name="documentID"></param>
        /// <param name="dbName">database name</param>
        /// <param name="userID"></param>
        public override void DeleteDocument(Document document, ContentDatabase databaseName, string userID)
        {
            try
            {
                Database db;
                DbConnection conn;
                switch (databaseName)
                {
                    case ContentDatabase.Staging:
                        db = this.StagingDBWrapper.SetDatabase();
                        conn = this.StagingDBWrapper.EnsureConnection();
                        break;
                    case ContentDatabase.Preview:
                        db = this.PreviewDBWrapper.SetDatabase();
                        conn = this.PreviewDBWrapper.EnsureConnection();
                        break;
                    case ContentDatabase.Live:
                        db = this.LiveDBWrapper.SetDatabase();
                        conn = this.LiveDBWrapper.EnsureConnection();
                        break;
                    default:
                        throw new Exception("Database Error: Invalid database name. DatabaseName = " + databaseName.ToString());
                }
                DbTransaction transaction = conn.BeginTransaction();

                try
                {
                    // SP: Clear extracted data
                    ClearExtractedData(document.DocumentID, db, transaction, userID);

                    // SP: Clear document
                    ClearDocument(document.DocumentID, db, transaction, databaseName.ToString());
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Deleteing PoliticalSubUnit document in " + databaseName.ToString() + " database failed. Document CDRID=" + document.DocumentID.ToString(), e);
                }
                finally
                {
                    transaction.Dispose();
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Deleting PoliticalSubUnit document data in " + databaseName.ToString() + " database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }


        #region Private region

        /// <summary>
        /// Call store procedure to clear existing PoliticalSubUnit data in database
        /// </summary>
        /// <param name="documentID"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void ClearExtractedData(int documentID, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                string spClearExtractedData = SPPoliticalSubUnit.SP_CLEAR_POLITICALSUBUNIT_DATA;
                using (DbCommand clearCommand = db.GetStoredProcCommand(spClearExtractedData))
                {
                    clearCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(clearCommand, "@DocumentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(clearCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Clearing PoliticalSubUnit data failed. Document CDRID=" + documentID.ToString(), e);
            }
        }

        private void SavePoliticalSubUnit(PoliticalSubUnitDocument doc, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                // Save PoliticalSubUnit Document
                using (DbCommand saveCommand = db.GetStoredProcCommand(SPPoliticalSubUnit.SP_SAVE_POLITICALSUBUNIT_DATA))
                {
                    saveCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(saveCommand, "@PoliticalSubUnitID", DbType.Int32, doc.DocumentID);
                    if (doc.FullName.Trim().Length > 0)
                        db.AddInParameter(saveCommand, "@FullName", DbType.String, doc.FullName.Trim());
                    else
                        db.AddInParameter(saveCommand, "@FullName", DbType.String, null);
                    if (doc.ShortName.Trim().Length > 0)
                        db.AddInParameter(saveCommand, "@ShortName", DbType.String, doc.ShortName.Trim());
                    else
                        db.AddInParameter(saveCommand, "@ShortName", DbType.String, null);
                    if (doc.CountryName.Trim().Length > 0)
                        db.AddInParameter(saveCommand, "@CountryName", DbType.String, doc.CountryName.Trim());
                    else
                        db.AddInParameter(saveCommand, "@CountryName", DbType.String, null);
                    db.AddInParameter(saveCommand, "@CountryID", DbType.Int32, doc.CountryId);
                    db.AddInParameter(saveCommand, "@UpdateUserID", DbType.String, userID);
                    db.ExecuteNonQuery(saveCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving PoliticalSubUnit Document failed. Document CDRID=" + doc.DocumentID.ToString(), e);
            }
        }

        
#endregion
    }
}
