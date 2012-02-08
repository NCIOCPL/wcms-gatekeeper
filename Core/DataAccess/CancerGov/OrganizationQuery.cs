using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Transactions;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Organization;
using GateKeeper.DataAccess.StoreProcedures;
using GateKeeper.DataAccess;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace GateKeeper.DataAccess.CancerGov
{
    public class OrganizationQuery : DocumentQuery
    {
        #region Public methods
        public override bool SaveDocument(Document document,string userID)
        {
            bool bSuccess = true;
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                OrganizationDocument orgDoc = (OrganizationDocument)document;
                // SP: Clear extracted data
                ClearExtractedData(orgDoc.DocumentID, db, transaction);

                // SP: Save genetic professional
                SaveOrganizationName(orgDoc, db, transaction, userID);

                //SP: Save genetic professional document data
                SaveDBDocument(orgDoc, db, transaction);

                transaction.Commit();
            }
            catch (Exception e)
            {
                bSuccess = false;
                transaction.Rollback();
                throw new Exception("Database Error: Saving organization document failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }

            return bSuccess;
        }

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
                    ClearExtractedData(document.DocumentID, db, transaction);

                    // SP: Clear document
                    ClearDocument(document.DocumentID, db, transaction, databaseName.ToString());
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Deleteing organization document data in " + databaseName.ToString() + " database failed. Document CDRID=" + document.DocumentID.ToString(), e);
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
                throw new Exception("Database Error: Deleting organization document data in " + databaseName.ToString() + " database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }

        public override void PushDocumentToPreview(Document document, string userID)
        {
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                try
                {
                    // SP: Call push terminology document
                    string spPushData = SPOrganization.SP_PUSH_EXTRACTED_ORGANIZATION_DATA;
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
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedOrganizationData failed.", e);
                }
                finally
                {
                    transaction.Dispose();
                    // SP: Call push document 
                    PushDocument(document.DocumentID, db, ContentDatabase.Preview.ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing organization document data to preview database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public override void PushDocumentToLive(Document document, string userID)
        {
            Database db = this.PreviewDBWrapper.SetDatabase();
            DbConnection conn = this.PreviewDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                // Rollback the change only if push organization sp failed.
                try
                {
                    // SP: Call push terminology document
                    string spPushData = SPOrganization.SP_PUSH_EXTRACTED_ORGANIZATION_DATA;
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
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedOrganizationyData failed", e);
                }
                finally
                {
                    transaction.Dispose();
                    // SP: Call Push document
                    PushDocument(document.DocumentID, db, ContentDatabase.Live.ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing organizationy document data to live database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }
        }
        #endregion // End of public methods

        #region Private methods
        // <summary>
        /// Call store procedure to clear existing organization data in database
        /// </summary>
        /// <param name="documentID"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void ClearExtractedData(int documentID, Database db, DbTransaction transaction)
        {
            try
            {
                string spClearExtractedData = SPOrganization.SP_CLEAR_ORGANIZATION_DATA;
                using (DbCommand clearCommand = db.GetStoredProcCommand(spClearExtractedData))
                {
                    clearCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(clearCommand, "@DocumentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(clearCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Clearing organization data failed. Document CDRID=" + documentID.ToString(), e);
            }
        }

        private void SaveOrganizationName(OrganizationDocument orgDoc, Database db, DbTransaction transaction, string userID)
        {
             try
            {
                 // Save ShortName
                 foreach (string name in orgDoc.ShortNames)
                 {
                    string spSaveOrgName = SPOrganization.SP_SAVE_ORGANIZATION_DATA;
                    using (DbCommand saveOrgCommand = db.GetStoredProcCommand(spSaveOrgName))
                    {
                        saveOrgCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(saveOrgCommand, "@OrganizationID", DbType.Int32, orgDoc.DocumentID);
                        db.AddInParameter(saveOrgCommand, "@Name", DbType.String, name.Trim());
                        db.AddInParameter(saveOrgCommand, "@Type", DbType.String, "ShortName");
                        db.AddInParameter(saveOrgCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(saveOrgCommand, transaction);
                    }
                 }

                 // Save AlternateName
                 foreach (string name in orgDoc.AlternateNames)
                 {
                    string spSaveOrgName = SPOrganization.SP_SAVE_ORGANIZATION_DATA;
                    using (DbCommand saveOrgCommand = db.GetStoredProcCommand(spSaveOrgName))
                    {
                        saveOrgCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(saveOrgCommand, "@OrganizationID", DbType.Int32, orgDoc.DocumentID);
                        db.AddInParameter(saveOrgCommand, "@Name", DbType.String, name.Trim());
                        db.AddInParameter(saveOrgCommand, "@Type", DbType.String, "AlternateName");
                        db.AddInParameter(saveOrgCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(saveOrgCommand, transaction);
                    }
                 }

                 // Save OfficialName
                 if (orgDoc.OfficialName.Trim().Length > 0)
                 {
                    string spSaveOrgName = SPOrganization.SP_SAVE_ORGANIZATION_DATA;
                    using (DbCommand saveOrgCommand = db.GetStoredProcCommand(spSaveOrgName))
                    {
                        saveOrgCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(saveOrgCommand, "@OrganizationID", DbType.Int32, orgDoc.DocumentID);
                        db.AddInParameter(saveOrgCommand, "@Name", DbType.String, orgDoc.OfficialName.Trim());
                        db.AddInParameter(saveOrgCommand, "@Type", DbType.String, "OfficialName");
                        db.AddInParameter(saveOrgCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(saveOrgCommand, transaction);
                    }
                 }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving organization name failed. Document CDRID=" + orgDoc.DocumentID.ToString(), e);
            }
        }
        #endregion // End of private methods
    }
}
