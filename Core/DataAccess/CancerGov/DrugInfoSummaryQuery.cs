using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Transactions;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.DataAccess.StoreProcedures;
using GateKeeper.DataAccess;
using GateKeeper.Logging;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace GateKeeper.DataAccess.CancerGov
{
    public class DrugInfoSummaryQuery : DocumentQuery
    {
        #region Public methods
        public override bool SaveDocument(Document drugDocument, string userID)
        {
            bool bSuccess = true;
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                DrugInfoSummaryDocument drugDoc = (DrugInfoSummaryDocument)drugDocument;

                // SP: Clear extracted data
                ClearExtractedData(drugDoc.DocumentID, db, transaction);

                // SP: Save drug info summary
                SaveDrugInfoSummary(drugDoc, db, transaction, userID);

                //SP: Save drug info summary document data
                SaveDBDocument(drugDoc, db, transaction);

                transaction.Commit();
            }
            catch (Exception e)
            {
                bSuccess = false;
                transaction.Rollback();
                throw new Exception("Database Error: Saving drug info summary document failed. Document CDRID=" + drugDocument.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }

            return bSuccess;
        }

        public override void DeleteDocument(Document drugDocument, ContentDatabase databaseName, string userID)
        {
            try
            {
                // Check if the document ID is referenced else where
                int documentID = drugDocument.DocumentID;

                Database db;
                    DbConnection conn;
                    switch (databaseName)
                    {
                        case ContentDatabase.Staging:
                            db = this.StagingDBWrapper.SetDatabase();
                            conn = this.StagingDBWrapper.EnsureConnection();
                            break;
                        case ContentDatabase.Preview:
                            {
                                db = this.PreviewDBWrapper.SetDatabase();
                                conn = this.PreviewDBWrapper.EnsureConnection();
                                Guid previewGuid = Guid.Empty;
                                // If guid is empty, give warning, don't proceed.
                                GetDocumentIDs(ref documentID, ref previewGuid, db);
                                break;
                            }
                        case ContentDatabase.Live:
                            {
                                db = this.LiveDBWrapper.SetDatabase();
                                conn = this.LiveDBWrapper.EnsureConnection();
                                Guid liveGuid = Guid.Empty;
                                // If guid is empty, warning, don't proceed.
                                GetDocumentIDs(ref documentID, ref liveGuid, db);
                                break;
                            }
                        default:
                            throw new Exception("Database Error: Invalid database name. DatabaseName = " + databaseName.ToString());
                    }
                    DbTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // SP: Clear extracted data
                        ClearExtractedData(documentID, db, transaction);

                        // SP: Clear document
                        ClearDocument(documentID, db, transaction, databaseName.ToString());
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw new Exception("Database Error: Deleteing drug info summary document in " + databaseName.ToString() + " failed. Document CDRID=" + documentID.ToString(), e);
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
                throw new Exception("Database Error: Deleting drug info summary document in " + databaseName.ToString() + " failed. Document CDRID=" + drugDocument.DocumentID.ToString(), e);
            }
        }

        public override void PushDocumentToPreview(Document drugDocument, string userID)
        {
            // Call check of it is OK to Push
            DrugInfoSummaryDocument drug = (DrugInfoSummaryDocument)drugDocument;
            if (GetDocumentData(drug, this.StagingDBWrapper.SetDatabase()))
            {
                PushToCDRPreview(drug.DocumentID, userID);
            }
            else
            {
                throw new Exception("Database Error: Retrieveing drug info summary data from CDR staging database failed. Document CDRID=" + drug.DocumentID.ToString());
            }
        }

        public override void PushDocumentToLive(Document drugDocument, string userID)
        {
            DrugInfoSummaryDocument drug = (DrugInfoSummaryDocument)drugDocument;
            if (GetDocumentData(drug, this.PreviewDBWrapper.SetDatabase()))
            {
                // Push document to CDR Staging database
                PushToCDRLive(drug.DocumentID, userID);
            }
            else
            {
                throw new Exception("Database Error: Retrieveing drug info summary data from CDR staging database failed. Document CDRID=" + drug.DocumentID.ToString());
            }
        }

        /// <summary>
        /// Change <tag /> to be <tag></tag> for content to be displayed in Firefox correctly
        /// </summary>
        /// <param name="html">reference string</param>
        /// <returns>html</returns>
        public void ReformatHTMLEndTag(ref string html)
        {
            ReplaceEndTag(ref html);
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Call store procedure to clear existing drug info summary data in database
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
                string spClearExtractedData = SPDrugInfo.SP_CLEAR_DRUG_DATA;
                using (DbCommand clearCommand = db.GetStoredProcCommand(spClearExtractedData))
                {
                    clearCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(clearCommand, "@DocumentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(clearCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Clearing drug info summary data failed. Document CDRID=" + documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to saving drug info summary data
        /// </summary>
        /// <param name="drugDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveDrugInfoSummary(DrugInfoSummaryDocument drugDoc, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                string spSaveDrugInfo = SPDrugInfo.SP_SAVE_DRUG_INFO;
                using (DbCommand saveDrugCommand = db.GetStoredProcCommand(spSaveDrugInfo))
                {
                    saveDrugCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(saveDrugCommand, "@DrugInfoSummaryID", DbType.Int32, drugDoc.DocumentID);
                    if (drugDoc.Title != null)
                        drugDoc.Title = drugDoc.Title.Trim();
                    db.AddInParameter(saveDrugCommand, "@Title", DbType.String, drugDoc.Title);
                    if (drugDoc.Description != null)
                        drugDoc.Description = drugDoc.Description.Trim();
                    db.AddInParameter(saveDrugCommand, "@Description", DbType.String, drugDoc.Description);
                    if (drugDoc.PrettyURL != null)
                        drugDoc.PrettyURL = drugDoc.PrettyURL.Trim();
                    db.AddInParameter(saveDrugCommand, "@PrettyURL", DbType.String, drugDoc.PrettyURL);
                    // TODO: REMOVE - this method is called for tweak string for string comparison purpose
                    if (drugDoc.Html != null)
                        drugDoc.Html = AdjustString(drugDoc.Html).Trim();
                    db.AddInParameter(saveDrugCommand, "@HTMLData", DbType.String, drugDoc.Html);
                    db.AddInParameter(saveDrugCommand, "@DateFirstPublished", DbType.DateTime, drugDoc.FirstPublishedDate);
                    // If the last modified date is DateTime.MinValuedate, the data field is missing, we should set it to null
                    if (drugDoc.LastModifiedDate == DateTime.MinValue)
                        db.AddInParameter(saveDrugCommand, "@DateLastModified", DbType.DateTime, null);
                    else
                        db.AddInParameter(saveDrugCommand, "@DateLastModified", DbType.DateTime, drugDoc.LastModifiedDate);
                    db.AddInParameter(saveDrugCommand, "@TerminologyLink", DbType.Int32, drugDoc.TerminologyLink);
                    db.AddInParameter(saveDrugCommand, "@NCIViewID", DbType.Guid, null);
                    db.AddInParameter(saveDrugCommand, "@UpdateUserID", DbType.String, userID);
                    db.ExecuteNonQuery(saveDrugCommand, transaction);
                }

            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving data to DrugInfoSummary table failed. Document CDRID=" + drugDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// This method is used to change html string to match the orignal string for string comparison purpose
        /// </summary>
        /// <param name="Html"></param
        /// <returns>Reformated HTML string</returns>
        private string AdjustString(string Html)
        {
            Html = Html.Replace("border=\"0\" />", "border=\"0\"></img>");
            Html = Html.Replace("<br />", "<br/>");
            Html = Html.Replace("<td valign=\"top\" width=\"25%\" />", "<td valign=\"top\" width=\"25%\"></td>");
            Html = Html.Replace(" />", "></a>");
            string glossaryTermTag = "Summary-GlossaryTermRef";
            if (Html.Contains(glossaryTermTag))
            {
                BuildGlossaryTermRefLink(ref Html, glossaryTermTag);
            }
            return Html;
        }

        /// <summary>
        /// Call store procedure to push drug info summary document to CDR preview database
        /// </summary>
        /// <param name="documentID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void PushToCDRPreview(int documentID, string userID)
        {
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                try
                {
                    // SP: Call push drug info summary document
                    string spPushData = SPDrugInfo.SP_PUSH_DRUG_INFO;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, documentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedDrugInfoSummaryData failed at push to preview database", e);
                }
                finally
                {
                    transaction.Dispose();
                }

                // SP: Call push document 
                PushDocument(documentID, db, ContentDatabase.Preview.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing drug info summary document to CDR preview database failed. Document CDRID=" + documentID.ToString(), e);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Call store procedure to push drug info summary document to CDR live database
        /// </summary>
        /// <param name="documentID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void PushToCDRLive(int documentID, string userID)
        {
            Database db = this.PreviewDBWrapper.SetDatabase();
            DbConnection conn = this.PreviewDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                try
                {
                    // SP: Call push drug info summary document
                    string spPushData = SPDrugInfo.SP_PUSH_DRUG_INFO;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, documentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedDrugInfoSummaryData failed at push to live database", e);
                }
                finally
                {
                    transaction.Dispose();
                }

                // SP: Call push document 
                PushDocument(documentID, db, ContentDatabase.Live.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing drug info summary document to CDR live database failed. Document CDRID=" + documentID.ToString(), e);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Returns the drug info summary document for a given documentid or cdrid
        /// from the staging database.
        /// </summary>
        /// <param name="cdrID">The documenidt of the document</param>
        /// <returns>DrugInfoSummaryDocument object</returns>
        public DrugInfoSummaryDocument GetDocumentData(int cdrID)
        {
            DrugInfoSummaryDocument drug = new DrugInfoSummaryDocument();
            drug.DocumentID = cdrID;
            return GetDocumentData(drug, this.StagingDBWrapper.SetDatabase()) ? drug : null;
        }

        /// <summary>
        /// Call store procedure to retrieve drug info summary data from cdr staging database
        /// </summary>
        /// <param name="documentID"></param>
        /// <param name="userID"></param>
        /// <returns>Flag of if the getting document succeeded</returns>
        private bool GetDocumentData(DrugInfoSummaryDocument drug, Database db)
        {
            // Get document data
            IDataReader reader = null;
            bool bSucceeded = true;
            try
            {
                string spGetData = SPDrugInfo.SP_GET_DRUG_INFO;
                using (DbCommand getCommand = db.GetStoredProcCommand(spGetData))
                {
                    getCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(getCommand, "@DrugInfoSummaryID", DbType.Int32, drug.DocumentID);
                    reader = db.ExecuteReader(getCommand);

                    if (reader.Read())
                    {
                        drug.Title = reader["Title"].ToString();
                        drug.Description = reader["Description"].ToString();
                        drug.PrettyURL = reader["PrettyURL"].ToString();
                        drug.Html = reader["HTMLData"].ToString();
                        if (reader["DateFirstPublished"].ToString().Length > 0)
                            drug.FirstPublishedDate = DateTime.Parse(reader["DateFirstPublished"].ToString());
                        if (reader["DateLastModified"].ToString().Length > 0)
                            drug.LastModifiedDate = DateTime.Parse(reader["DateLastModified"].ToString());
                        else
                            drug.LastModifiedDate = DateTime.MinValue;
                        drug.TerminologyLink = Int32.Parse(reader["TerminologyLink"].ToString());
                        drug.GUID = (Guid)reader["documentGUID"];
                    }
                    else
                        bSucceeded = false;
                }
            }
            catch (Exception e)
            {
                bSucceeded = false;
                throw new Exception("Database Error: Retrieveing drug info summary data from CDR staging database failed. Document CDRID=" + drug.DocumentID.ToString(), e);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
            return bSucceeded;
        }

        #endregion
    }
}
