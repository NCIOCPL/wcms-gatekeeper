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
          public override bool SaveDocument(Document drugDocument,string userID)
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
                if (IsOKToDelete(documentID, DocumentType.DrugInfoSummary))
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
                             {
                                 db = this.PreviewDBWrapper.SetDatabase();
                                 conn = this.PreviewDBWrapper.EnsureConnection();
                                 Guid previewGuid = Guid.Empty;
                                 // If guid is empty, give warning, don't proceed.
                                 GetDocumentIDs(ref documentID, ref previewGuid, db);
                                 if (previewGuid != Guid.Empty)
                                    DeleteDrugInfoPreview(previewGuid);
                                else
                                {
                                    // Give out warning message
                                    drugDocument.WarningWriter("Database warning: DrugInfoSummary document can not be deleted in preview database.  Can not find document with cdrid = " + drugDocument.DocumentID.ToString() + " in the preview database.");
                                }
                                 break;
                             }
                         case ContentDatabase.Live:
                             {
                                 db = this.LiveDBWrapper.SetDatabase();
                                 conn = this.LiveDBWrapper.EnsureConnection();
                                 Guid liveGuid = Guid.Empty;
                                 // If guid is empty, warning, don't proceed.
                                 GetDocumentIDs(ref documentID, ref liveGuid, db);
                                 if (liveGuid != Guid.Empty)
                                    DeleteDrugInfoLive(liveGuid);
                                 else
                                 {
                                    // Give out warning message
                                    drugDocument.WarningWriter("Database warning: DrugInfoSummary document can not be deleted in live database.  Can not find document with cdrid = " + drugDocument.DocumentID.ToString() + " in the live database.");
                                 }
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
                if (IsOKToPush(drug.DocumentID, drug.PrettyURL, 0, drug.GUID, Guid.Empty, ContentDatabase.Preview, DocumentType.DrugInfoSummary))
                {
                    // Push document to CDR Staging database
                    PushToCDRPreview(drug.DocumentID, userID);

                    // Push document to CancerGovStaging database
                    PushToCancerGovPreview(drug.DocumentID, drug, userID);
                }
                else
                {
                    throw new Exception("Database Error: Could not push document to preview database due to pretty URL duplication. Document CDRID=" + drug.DocumentID.ToString());
                }
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
                if (IsOKToPush(drug.DocumentID, drug.PrettyURL, 0, drug.GUID, Guid.Empty, ContentDatabase.Live, DocumentType.DrugInfoSummary))
                {
                    // Push document to CDR Staging database
                    PushToCDRLive(drug.DocumentID, userID);

                    // Push document to CancerGovStaging database
                    PushToCancerGovLive(drug.DocumentID, userID);
                }
                else
                {
                    throw new Exception("Database Error: Could not push document to live database due to pretty URL duplication. Document CDRID=" + drug.DocumentID.ToString());
                }
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
      /// Call store procedure to push drug info summary document to CancerGovStaging database
      /// </summary>
      /// <param name="documentID"></param>
      /// <param name="userID"></param>
      /// <returns></returns>
      private void PushToCancerGovPreview(int documentID, DrugInfoSummaryDocument drug, string userID)
      {
          // Note: Don't need transaction for alll CancerGov related store procedure
          Database db = this.CancerGovStagingDBWrapper.SetDatabase();
          try
          {
              // SP: Call push drug info summary document to push data to CancerGovStaging database
              string spPushData = SPDrugInfo.SP_PUSH_DRUG_INFO_TO_PREVIEW;
              using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
              {
                  pushCommand.CommandType = CommandType.StoredProcedure;
                  db.AddInParameter(pushCommand, "@NCIViewID", DbType.Guid, null);
                  if (drug.Title != null)
                      drug.Title = drug.Title.Trim();
                  db.AddInParameter(pushCommand, "@Title", DbType.String, drug.Title);
                  // The maximum length of the short title should be 64 chars.
                  if (drug.Title.Trim().Length > 64)
                      db.AddInParameter(pushCommand, "@ShortTitle", DbType.String, drug.Title.Trim().Substring(0, 64));
                  else
                  {
                      if (drug.Title != null)
                          drug.Title = drug.Title.Trim();
                      db.AddInParameter(pushCommand, "@ShortTitle", DbType.String, drug.Title);
                  }
                  if (drug.Description != null)
                      drug.Description = drug.Description.Trim();
                  db.AddInParameter(pushCommand, "@description", DbType.String, drug.Description);
                  if (drug.Html != null)
                      drug.Html = drug.Html.Trim();
                  db.AddInParameter(pushCommand, "@data", DbType.String, drug.Html);
                  db.AddInParameter(pushCommand, "@datasize", DbType.Int32, drug.Html.Length);
                  db.AddInParameter(pushCommand, "@Expirationdate", DbType.DateTime, new DateTime(2100, 1, 1));
                  if (drug.LastModifiedDate == DateTime.MinValue)
                      db.AddInParameter(pushCommand, "@Releasedate", DbType.DateTime, null);
                  else
                    db.AddInParameter(pushCommand, "@Releasedate", DbType.DateTime, drug.LastModifiedDate);
                  db.AddInParameter(pushCommand, "@posteddate", DbType.DateTime, drug.FirstPublishedDate);
                  db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                  if (drug.PrettyURL != null)
                      drug.PrettyURL = drug.PrettyURL.Trim();
                  db.AddInParameter(pushCommand, "@proposedURL", DbType.String, drug.PrettyURL);
                  db.AddInParameter(pushCommand, "@updateDate", DbType.DateTime, DateTime.Now);
                  db.AddInParameter(pushCommand, "@DocumentID", DbType.Guid, drug.GUID);
                  db.ExecuteNonQuery(pushCommand);
              }
          }
          catch (Exception e)
          {
              throw new Exception("Database Error: Pushing drug info summary document to CancerGovStaging database failed. Document CDRID=" + documentID.ToString(), e);
          }
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

          
      /// <summary>
      /// Call store procedure to push drug info summary document to CancerGov database
      /// </summary>
      /// <param name="documentID"></param>
      /// <param name="userID"></param>
      /// <returns></returns>
      private void PushToCancerGovLive(int documentID, string userID)
      {
          // Note: Don't need transaction for alll CancerGov related store procedure because the transaction is nested in store procedure
          Database db = this.CancerGovStagingDBWrapper.SetDatabase();
          try
          {
              // Get document guid
              Guid docGuid = Guid.Empty;
              // If guid is empty, throw an error, don't proceed.
              GetDocumentIDs(ref documentID, ref docGuid, this.PreviewDBWrapper.SetDatabase());
              Guid nciViewID = GetNCIViewID(docGuid, db);
              if (nciViewID != Guid.Empty)
              {
                  // SP: Call push drug info summary document to push data to CancerGovStaging database
                  string spPushData = SPDrugInfo.SP_PUSH_DRUG_INFO_TO_LIVE;
                  using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                  {
                      pushCommand.CommandType = CommandType.StoredProcedure;
                      db.AddInParameter(pushCommand, "@NCIViewID", DbType.Guid, nciViewID);
                      db.AddInParameter(pushCommand, "@documentID", DbType.Guid, docGuid);
                      db.AddInParameter(pushCommand, "@updateUserID", DbType.String, userID);
                      db.ExecuteNonQuery(pushCommand);
                  }
              }
          }
          catch (Exception e)
          {
              throw new Exception("Database Error: Pushing drug info summary document to CancerGov database failed. Document CDRID=" + documentID.ToString(), e);
          }
      }

       /// <summary>
      /// Call store procedure to delete drug info summary document in CancerGovStaging database
      /// </summary>
      /// <param name="documentID"></param>
      /// <returns></returns>
      private void  DeleteDrugInfoPreview(Guid documentGuid)
      {
          // Note: Don't need transaction for alll CancerGov related store procedure because the transaction is nested in store procedure
          Database db = this.CancerGovStagingDBWrapper.SetDatabase();
          try
          {
              // SP: delete drug info summary document from CancerGovStaging database
              string spDeleteData = SPDrugInfo.SP_DELETE_DRUG_INFO_IN_PREVIEW;
              using (DbCommand deleteCommand = db.GetStoredProcCommand(spDeleteData))
              {
                  deleteCommand.CommandType = CommandType.StoredProcedure;
                  db.AddInParameter(deleteCommand, "@DocumentID", DbType.Guid, documentGuid);
                  db.ExecuteNonQuery(deleteCommand);
              }
          }
          catch (Exception e)
          {
              throw new Exception("Database Error: Deleting drug info summary document in CancerGov preview database failed. Document GUID=" + documentGuid.ToString(), e);
          }
      }

      /// <summary>
      /// Call store procedure to delete drug info summary document in CancerGov database
      /// </summary>
      /// <param name="documentID"></param>
      /// <returns></returns>
      private void DeleteDrugInfoLive(Guid documentGuid)
      {
          Database db = this.CancerGovStagingDBWrapper.SetDatabase();
          try
          {
              // SP: delete drug info from CancerGov database
              string spDeleteData = SPDrugInfo.SP_DELETE_DRUG_INFO_IN_LIVE;
              using (DbCommand DeleteCommand = db.GetStoredProcCommand(spDeleteData))
              {
                  DeleteCommand.CommandType = CommandType.StoredProcedure;
                  db.AddInParameter(DeleteCommand, "@DocumentID", DbType.Guid, documentGuid);
                  db.ExecuteNonQuery(DeleteCommand);
              }
          }
          catch (Exception e)
          {
              throw new Exception("Database Error: Deleting drug info summary document in CancerGov live database failed. Document Guid=" + documentGuid.ToString(), e);
          }
      }

    #endregion
    }
}
