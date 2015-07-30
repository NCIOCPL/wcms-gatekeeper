using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Transactions;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Terminology;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.StoreProcedures;
using GateKeeper.DataAccess;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace GateKeeper.DataAccess.CancerGov
{
    public class TerminologyQuery : DocumentQuery
    {
        DictionaryQuery Dictionary = new DictionaryQuery();


        public override bool SaveDocument(Document terminologyDoc, string userID)
        {
            bool bSuccess = true;
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                TerminologyDocument TermDoc = (TerminologyDocument)terminologyDoc;

                // SP: Clear extracted data
                ClearTerminologyData(TermDoc.DocumentID, db, transaction);

                // SP: Save CDR menus
                SaveCDRMenus(TermDoc, db, transaction, userID);

                // SP: Save terminology
                SaveTerminology(TermDoc, db, transaction, userID);

                // SP: Save term definition
                if (TermDoc.DefinitionText.Trim().Length > 0)
                    SaveTermDefinition(TermDoc, db, transaction, userID);

                // SP: Save term semantic type
                SaveTermSemanticType(TermDoc, db, transaction, userID);

                // SP: Save term other name
                SaveTermOtherName(TermDoc, db, transaction, userID);

                // SP: Save document data
                SaveDBDocument(TermDoc, db, transaction);

                // Save the extracted dictionary entry.
                Dictionary.SaveDocument(TermDoc.DocumentID, TermDoc.Dictionary, transaction);

                transaction.Commit();
            }
            catch (Exception e)
            {
                bSuccess = false;
                transaction.Rollback();
                throw new Exception("Database Error: Saving teminology document failed. Document CDRID=" + terminologyDoc.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }

            return bSuccess;
        }

        public override void DeleteDocument(Document terminologyDoc, ContentDatabase databaseName, string userID)
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
                    ClearTerminologyData(terminologyDoc.DocumentID, db, transaction);

                    Dictionary.DeleteDocument(terminologyDoc.DocumentID, transaction);

                    // SP: Clear document
                    ClearDocument(terminologyDoc.DocumentID, db, transaction, databaseName.ToString());
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Deleteing terminology document data in " + databaseName.ToString() + " database failed. Document CDRID=" + terminologyDoc.DocumentID.ToString(), e);
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
                throw new Exception("Database Error: Deleting terminology document data in " + databaseName.ToString() + " database failed. Document CDRID=" + terminologyDoc.DocumentID.ToString(), e);
            }
        }

        public override void PushDocumentToPreview(Document terminologyDoc, string userID)
        {
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                try
                {
                    // SP: Call push terminology document
                    string spPushData = SPTerminology.SP_PUSH_TERM_DATA;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, terminologyDoc.DocumentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                    }

                    Dictionary.PushDocumentToPreview(terminologyDoc.DocumentID, transaction);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedTerminologyData failed.", e);
                }
                finally
                {
                    transaction.Dispose();
                    // SP: Call push document 
                    PushDocument(terminologyDoc.DocumentID, db, ContentDatabase.Preview.ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing terminology document data to preview database failed. Document CDRID=" + terminologyDoc.DocumentID.ToString(), e);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public override void PushDocumentToLive(Document terminologyDoc, string userID)
        {
            Database db = this.PreviewDBWrapper.SetDatabase();
            DbConnection conn = this.PreviewDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                // Rollback the change only if push terminology sp failed.
                try
                {
                    // SP: Call push terminology document
                    string spPushData = SPTerminology.SP_PUSH_TERM_DATA;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, terminologyDoc.DocumentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                    }

                    Dictionary.PushDocumentToPreview(terminologyDoc.DocumentID, transaction);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedTerminologyData failed", e);
                }
                finally
                {
                    transaction.Dispose();
                    // SP: Call Push document
                    PushDocument(terminologyDoc.DocumentID, db, ContentDatabase.Live.ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing terminology document data to live database failed. Document CDRID=" + terminologyDoc.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }
        }

        #region Private Methods
        /// <summary>
        /// Call store procedure to clear existing terminology data
        /// </summary>
        /// <param name="TermDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
         private void ClearTerminologyData(int documentID, Database db, DbTransaction transaction)
         {
             try{
                string spClearExtractedData = SPTerminology.SP_CLEAR_TERMINOLOGY_DATA;
                using (DbCommand clearCommand = db.GetStoredProcCommand(spClearExtractedData))
                {
                    clearCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(clearCommand, "@DocumentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(clearCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Clearing terminology data failed. Document CDRID=" + documentID.ToString(), e);
            }
         }

        /// <summary>
        /// Save CDR menu data
        /// </summary>
        /// <param name="TermDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
         private void SaveCDRMenus(TerminologyDocument TermDoc, Database db, DbTransaction transaction, string userID)
         {
             try{
                 // Loop through the menu items, call store procedure to save one by one
                 foreach (TerminologyMenu menu in TermDoc.Menus)
                 {
                     if (menu.MenuParentIDList.Count > 0)
                     {
                         foreach (int menuParentId in menu.MenuParentIDList)
                         {
                             SaveOneCDRMenuRow(TermDoc, menu, menuParentId, db, transaction, userID);  
                         }
                     }
                     else
                     {
                         //save menu item with parentId = NULL 
                         SaveOneCDRMenuRow(TermDoc, menu, -1, db, transaction, userID);  
                     }
                 }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Save terminology CDR menus failed. Document CDRID=" + TermDoc.DocumentID.ToString(), e);
            }
         }

        /// <summary>
        /// Call store procedure to save CDR menu data
        /// </summary>
        /// <param name="TermDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveOneCDRMenuRow(TerminologyDocument TermDoc, TerminologyMenu menu, int menuParentId, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                string spSaveCDRMenu = SPTerminology.SP_SAVE_CDR_MENUS;
                using (DbCommand saveCDRMenuCommand = db.GetStoredProcCommand(spSaveCDRMenu))
                {
                    saveCDRMenuCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(saveCDRMenuCommand, "@CDRID", DbType.Int32, TermDoc.DocumentID);
                    db.AddInParameter(saveCDRMenuCommand, "@MenuTypeId", DbType.Int32, menu.MenuType);
                    if (menuParentId > 0)
                        db.AddInParameter(saveCDRMenuCommand, "@ParentID", DbType.Int32, menuParentId);
                    else
                        db.AddInParameter(saveCDRMenuCommand, "@ParentID", DbType.Int32, null);
                    string displayName = menu.DisplayName.Trim();
                    if (displayName.Length == 0)
                        displayName = TermDoc.PreferredName.Trim();
                    db.AddInParameter(saveCDRMenuCommand, "@DisplayName", DbType.String, displayName.Trim());
                    if (menu.SortName.Trim().Length > 0)
                        db.AddInParameter(saveCDRMenuCommand, "@SortName", DbType.String, menu.SortName.Trim());
                    else
                        db.AddInParameter(saveCDRMenuCommand, "@SortName", DbType.String, null);
                    db.AddInParameter(saveCDRMenuCommand, "@UpdateUserID", DbType.String, userID);
                    db.ExecuteNonQuery(saveCDRMenuCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Save terminology CDR menus failed. Document CDRID=" + TermDoc.DocumentID.ToString(), e);
            }

        }

        /// <summary>
        /// Call store procedure to save terminology data
        /// </summary>
        /// <param name="TermDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveTerminology(TerminologyDocument TermDoc, Database db, DbTransaction transaction, string userID)
         {
             try{
                string spSaveTerminology = SPTerminology.SP_SAVE_TERMINOLOGY;
                using (DbCommand saveTerminologyCommand = db.GetStoredProcCommand(spSaveTerminology))
                {
                    saveTerminologyCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(saveTerminologyCommand, "@TermID", DbType.Int32, TermDoc.DocumentID);
                    db.AddInParameter(saveTerminologyCommand, "@PreferredName", DbType.String, TermDoc.PreferredName.Trim());
                    // TermStatus is all "Unreviewed" in database.  It is not exacted from XML document
                    db.AddInParameter(saveTerminologyCommand, "@TermStatus", DbType.String, "Unreviewed");
                    // Comment is all null in database.  This part is also not extracted from XML document
                    db.AddInParameter(saveTerminologyCommand, "@Comment", DbType.String, null);
                    db.AddInParameter(saveTerminologyCommand, "@UpdateUserID", DbType.String, userID);
                    db.ExecuteNonQuery(saveTerminologyCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving terminology data failed. Document CDRID=" + TermDoc.DocumentID.ToString(), e);
            }
         }

        /// <summary>
        /// Call store procedure to clear existing terminology data
        /// </summary>
        /// <param name="TermDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
         private void SaveTermDefinition(TerminologyDocument TermDoc, Database db, DbTransaction transaction, string userID)
         {
             try{
                string spSaveDefinition = SPTerminology.SP_SAVE_TERM_DEFINITION;
                using (DbCommand definitionCommand = db.GetStoredProcCommand(spSaveDefinition))
                {
                    definitionCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(definitionCommand, "@TermID", DbType.Int32, TermDoc.DocumentID);
                    db.AddInParameter(definitionCommand, "@Definition", DbType.String, TermDoc.DefinitionText.Trim());
                    db.AddInParameter(definitionCommand, "@DefinitionType", DbType.String, DocumentHelper.GetAudienceDBString(TermDoc.DefinitionAudience).Trim());
                    db.AddInParameter(definitionCommand, "@Comment", DbType.String, null);
                    db.AddInParameter(definitionCommand, "@DefinitionHTML", DbType.String, TermDoc.Html.Trim());
                    db.AddInParameter(definitionCommand, "@UpdateUserID", DbType.String, userID);
                    db.ExecuteNonQuery(definitionCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving term definition failed. Document CDRID=" + TermDoc.DocumentID.ToString(), e);
            }
         }


        /// <summary>
        /// Call store procedure to save terminology semantic type data
        /// </summary>
        /// <param name="TermDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
         private void SaveTermSemanticType(TerminologyDocument TermDoc, Database db, DbTransaction transaction, string userID)
         {
             try{
                 foreach (TermSemanticType semantic in TermDoc.SemanticTypes)
                 {
                     string spSaveSemanticType = SPTerminology.SP_SAVE_SEMANTIC_TYPE;
                     using (DbCommand saveSemanticTypeCommand = db.GetStoredProcCommand(spSaveSemanticType))
                     {
                         saveSemanticTypeCommand.CommandType = CommandType.StoredProcedure;
                         db.AddInParameter(saveSemanticTypeCommand, "@TermID", DbType.Int32, TermDoc.DocumentID);
                         db.AddInParameter(saveSemanticTypeCommand, "@SemanticTypeID", DbType.Int32, semantic.ID);
                         db.AddInParameter(saveSemanticTypeCommand, "@SemanticTypeName", DbType.String, semantic.Name.Trim());
                         db.AddInParameter(saveSemanticTypeCommand, "@UpdateUserID", DbType.String, userID);
                         db.ExecuteNonQuery(saveSemanticTypeCommand, transaction);
                     }
                 }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving terminology semantic type. Document CDRID=" + TermDoc.DocumentID.ToString(), e);
            }
         }

        /// <summary>
        /// Call store procedure to save terminology other names
        /// </summary>
        /// <param name="TermDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveTermOtherName(TerminologyDocument TermDoc, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                foreach (TerminologyOtherName otherName in TermDoc.OtherNames)
                {
                    string spSaveOtherName = SPTerminology.SP_SAVE_TERM_OTHER_NAME;
                    using (DbCommand saveOtherNameCommand = db.GetStoredProcCommand(spSaveOtherName))
                    {
                        saveOtherNameCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(saveOtherNameCommand, "@TermID", DbType.Int32, TermDoc.DocumentID);
                        db.AddInParameter(saveOtherNameCommand, "@OtherName", DbType.String, otherName.Name.Trim());
                        db.AddInParameter(saveOtherNameCommand, "@OtherNameType", DbType.String, otherName.Type.Trim());
                        db.AddInParameter(saveOtherNameCommand, "@ReviewStatus", DbType.String, "Unreviewed");
                        db.AddInParameter(saveOtherNameCommand, "@Comment", DbType.String, null);
                        db.AddInParameter(saveOtherNameCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(saveOtherNameCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving terminology other names failed. Document CDRID=" + TermDoc.DocumentID.ToString(), e);
            }
        }

        #endregion
    }
}
