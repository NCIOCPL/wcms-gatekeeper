using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using System.Transactions;
using System.Collections;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Protocol;
using GateKeeper.Logging;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.StoreProcedures;
using GateKeeper.Common;

namespace GateKeeper.DataAccess.CancerGov
{
    /// <summary>
    /// Class to encapsulate database access for protocol documents.
    /// </summary>
    public class ProtocolQuery : DocumentQuery
    {
        #region Public Methods

        /// <summary>
        /// Method to clear the protocol search cache.
        /// </summary>
        /// <param name="environment"></param>
        public void ClearSearchCache(ContentDatabase databaseName)
        { 
            if (databaseName != ContentDatabase.Preview && databaseName != ContentDatabase.Live)
            {
                    throw new Exception("Protocol search cache clearing not support for the" + databaseName.ToString());
            }

            Database db = DatabaseFactory.CreateDatabase(databaseName.ToString());
            string storedProcedureName = "usp_ProtocolSearchRefineCache";

            using (DbCommand cmd = db.GetStoredProcCommand(storedProcedureName))
            {
                try
                {
                    cmd.CommandType= CommandType.StoredProcedure;
                    db.ExecuteNonQuery(cmd);
                }
                catch 
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Method to Reindex FullText protocol. This takes about 7-10 mins. So it doesn't belong to the front end.
        /// We don't want people running parallel time-consuming jobs which they can do from the front end. 
        /// It should be done inside the processmanager or by DBA so no one can interfere with that job.
        /// </summary>
        /// <param name="environment"></param>
        public void ReindexProtocolFullText(ContentDatabase databaseName)
        {
            if (databaseName != ContentDatabase.Preview && databaseName != ContentDatabase.Live)
            {
                throw new Exception("Protocol Full Text Reindexing not support for the" + databaseName.ToString());
            }

            Database db = DatabaseFactory.CreateDatabase(databaseName.ToString());
            string storedProcedureName = "usp_protocolFullTextReindex";

            using (DbCommand cmd = db.GetStoredProcCommand(storedProcedureName))
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                   //Since it is a long time job, we set timeoutseconds to 1200 via appSetting
                    cmd.CommandTimeout = ApplicationSettings.VeryLongProcessCommandTimeout;
                    db.ExecuteNonQuery(cmd);
                }
                catch
                {
                    throw;
                }
            }
        }

         /// <summary>
        /// Method to clear the protocol search cache.
        /// </summary>
        /// <param name="environment"></param>
        public void ResetPrettyURLFlag(ContentDatabase databaseName)
        {
           if (databaseName != ContentDatabase.CancerGov && databaseName != ContentDatabase.CancerGovStaging)
            {
                    throw new Exception("Reset pretty URL not support for the" + databaseName.ToString());
            }

            Database db = DatabaseFactory.CreateDatabase(databaseName.ToString());
            string storedProcedureName = "usp_UpdatePrettyURLFlag";

            using (DbCommand cmd = db.GetStoredProcCommand(storedProcedureName))
            {
                try
                {
                    cmd.CommandType= CommandType.StoredProcedure;
                    //QueryWrapper.ExecuteNonQuery(db, cmd);
                    db.ExecuteNonQuery(cmd);
                }
                catch 
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// Call store procedures to save protocol document into database
        /// </summary>
        /// <param name="document"></param>
        /// <param name="userID"></param>
        /// <Return>bSuccess</Return>
        public override bool SaveDocument(Document protocolDocument, string userID)
        {
            bool bSuccess = true;
            ProtocolDocument protocolDoc = (ProtocolDocument)protocolDocument;
            Database db = this.StagingDBWrapper.SetDatabase();

            // Special logic for CTGovProtocol :determine the CTGovProtocol first published date and use the date
            // to determine if the document is new or not.
            if (protocolDoc.IsCTProtocol  == 1)
                DetermineFirstPublishedDate(db, protocolDoc);

            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                // SP: Clear extracted protocol data
                ClearProtocolData(db, transaction, protocolDoc.ProtocolID);

                // SP: Save protocol
                SaveProtocol(db, transaction, protocolDoc, userID);
                
                // SP: Save protocol old id (protocol primary id) 
                SaveProtocolOldID(db, transaction, protocolDoc);
               
                // SP: 	Save protocol drug
                SaveProtocolDrug(db, transaction, protocolDoc, userID);
               
                // SP: Save protocol cancer types
                SaveProtocolCancerType(db, transaction, protocolDoc, userID);
               
                // SP: Save protocol special categories
                SaveProtocolSpecialCategories(db, transaction, protocolDoc, userID);
 
                // SP: Save protocol section
                SaveProtocolSection(db, transaction, protocolDoc, userID);
               
                // SP: Save protocol alternate id
                SaveProtocolAlternateID(db, transaction, protocolDoc, userID);
                
                // SP: Save protocol sponsor
                SaveProtocolSponsors(db, transaction, protocolDoc, userID);

                // SP: Save protocol study category 
                SaveProtocolStudyCategory(db, transaction, protocolDoc, userID);

                // SP: Save protocol modality
                SaveProtocolModality(db, transaction, protocolDoc, userID);
                
                // SP: Save protocol phase 
                SaveProtocolPhase(db, transaction, protocolDoc, userID);
               
                // SP: 	usp_SaveProtocolDetail
                SaveProtocolDetail(db, transaction, protocolDoc, userID);

                // SP: 	usp_SaveProtocolSecondaryPrettyUrlID
                SaveProtocolSecondaryPrettyUrlID(db, transaction, protocolDoc, userID);

                // SP: Save lead orgainzation and site contact information
                SavePrototolContactInfo(db, transaction, protocolDoc, userID);

                // SP: Save document data
                SaveDBDocument(protocolDoc, db, transaction);

                transaction.Commit();
            }
            catch (Exception e)
            {
                bSuccess = false;
                transaction.Rollback();
                throw new Exception("Database Error: Saving protocol document failed. Document CDRID=" + protocolDocument.DocumentID.ToString(), e);
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
        ///  Delete document by document id
        /// </summary>
        /// <param name="documentID"></param>
        public override void DeleteDocument(Document protocolDocument, ContentDatabase databaseName, string userID)
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
                    ClearProtocolData(db, transaction, protocolDocument.DocumentID);

                    // SP: Clear document
                    ClearDocument(protocolDocument.DocumentID, db, transaction, databaseName.ToString());
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Deleteing protocol document data in " + databaseName.ToString() + " database failed. Document CDRID=" + protocolDocument.DocumentID.ToString(), e);
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
                throw new Exception("Database Error: Deleting protocol document data in " + databaseName.ToString() + " database failed. Document CDRID=" + protocolDocument.DocumentID.ToString(), e);
            }
        }

 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentID"></param>
        public override void PushDocumentToPreview(Document protocolDocument, string userID)
        {
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                // SP: Call push map tables
                string spPushMapTable = SPProtocol.SP_PUSH_MAP_TABLE_PREVIEW;
                using (DbCommand mapCommand = db.GetStoredProcCommand(spPushMapTable))
                {
                    mapCommand.CommandType = CommandType.StoredProcedure;
                    db.ExecuteNonQuery(mapCommand);
                }

                try
                {
                    // SP: Call push protocol document
                    string spPushData = SPProtocol.SP_PUSH_PROTOCOL_DATA;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        pushCommand.CommandTimeout = ApplicationSettings.CommandTimeout;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, protocolDocument.DocumentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedProtocolData failed.", e);
                }
                finally
                {
                    transaction.Dispose();
                }

                // SP: Call push document 
                PushDocument(protocolDocument.DocumentID, db, ContentDatabase.Preview.ToString());

            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing protocol document data to preview database failed. Document CDRID=" + protocolDocument.DocumentID.ToString(), e);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentID"></param>
        public override void PushDocumentToLive(Document protocolDocument, string userID)
        {
            Database db = this.PreviewDBWrapper.SetDatabase();
            DbConnection conn = this.PreviewDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                // SP: Call push map tables
                string spPushMapTable = SPProtocol.SP_PUSH_MAP_TABLE_LIVE;
                using (DbCommand mapCommand = db.GetStoredProcCommand(spPushMapTable))
                {
                    mapCommand.CommandType = CommandType.StoredProcedure;
                    db.ExecuteNonQuery(mapCommand);
                }


                // Rollback the change only if push protocol sp failed.
                try
                {
                    // SP: Call push protocol document
                    string spPushData = SPProtocol.SP_PUSH_PROTOCOL_DATA;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        pushCommand.CommandTimeout = ApplicationSettings.CommandTimeout;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, protocolDocument.DocumentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedProtocolData failed", e);
                }

                // SP: Call Push document
                PushDocument(protocolDocument.DocumentID, db, ContentDatabase.Live.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing protocol document data to live database failed. Document CDRID=" + protocolDocument.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Private Method
        /// <summary>
        /// Call store procedure to clear up the protocol data before new saving starts
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <returns></returns>
        private void ClearProtocolData(Database db, DbTransaction transaction, int protocolID)
        {
            try
            {
                string spClearExtractedData = SPProtocol.SP_CLEAR_EXTRACTED_PROTOCOL_DATA;
                using (DbCommand clearCommand = db.GetStoredProcCommand(spClearExtractedData))
                {
                    clearCommand.CommandType = CommandType.StoredProcedure;
                    clearCommand.CommandTimeout = ApplicationSettings.CommandTimeout;
                    db.AddInParameter(clearCommand, "@DocumentID", DbType.Int32, protocolID);
                    db.ExecuteNonQuery(clearCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Clearing protocol data failed. Document CDRID=" + protocolID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into Protocol table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocol(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                string spSaveProtocol = SPProtocol.SP_SAVE_PROTOCOL;
                using (DbCommand saveProtocolCommand = db.GetStoredProcCommand(spSaveProtocol))
                {
                    saveProtocolCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(saveProtocolCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                    db.AddInParameter(saveProtocolCommand, "@IsNew", DbType.Int16, protocolDoc.IsNew);
                    db.AddInParameter(saveProtocolCommand, "@IsNIHClinicalCenterTrial", DbType.Int16, protocolDoc.IsNIHClinicalTrial);
                    db.AddInParameter(saveProtocolCommand, "@IsActiveProtocol", DbType.Int16, protocolDoc.IsActive);
                    db.AddInParameter(saveProtocolCommand, "@UpdateUserid", DbType.String, userID);
                    db.ExecuteNonQuery(saveProtocolCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol old id into ProtocolOldID table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <returns></returns>
        private void SaveProtocolOldID(Database db, DbTransaction transaction, ProtocolDocument protocolDoc)
        {
            try
            {
                string spSaveOldID = SPProtocol.SP_SAVE_PROTOCOL_OLD_ID;
                using (DbCommand oldIDCommand = db.GetStoredProcCommand(spSaveOldID))
                {
                    oldIDCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(oldIDCommand, "@ProtocolID", DbType.Int32, protocolDoc.DocumentID);
                    string primaryID = string.Empty;
                    foreach (AlternateProtocolID alterID in protocolDoc.AlternateIDList)
                    {
                        if (alterID.Type.ToUpper().Equals("PRIMARY"))
                        {
                            primaryID = alterID.IdString;
                            break;
                        }
                    }
                    db.AddInParameter(oldIDCommand, "@OldPrimaryProtocolID", DbType.String, primaryID);
                    db.ExecuteNonQuery(oldIDCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol old id failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol drug into ProtocolDrug table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolDrug(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                foreach (ProtocolDrug drug in protocolDoc.DrugList)
                {
                    string spSaveProtocolDrug = SPProtocol.SP_SAVE_PROTOCOL_DRUG;
                    using (DbCommand drugCommand = db.GetStoredProcCommand(spSaveProtocolDrug))
                    {
                        drugCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(drugCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        db.AddInParameter(drugCommand, "@DrugID", DbType.Int32, drug.DrugID);
                        db.AddInParameter(drugCommand, "@UpdateUserid", DbType.String, userID);
                        db.ExecuteNonQuery(drugCommand, transaction);
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol drug failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolTypeOfCancer table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolCancerType(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                foreach (TypeOfCancer cancer in protocolDoc.TypeOfCancerList)
                {
                    string spSaveTypeOfCancer = SPProtocol.SP_SAVE_PROTOCOL_TYPE_OF_CANCER;
                    using (DbCommand cancerCommand = db.GetStoredProcCommand(spSaveTypeOfCancer))
                    {
                        cancerCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(cancerCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        db.AddInParameter(cancerCommand, "@DiagnosisID", DbType.Int32, cancer.TypeOfCancerID);
                        db.AddInParameter(cancerCommand, "@UpdateUserid", DbType.String, userID);
                        db.ExecuteNonQuery(cancerCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol type of cancer failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolSpecialCategory table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolSpecialCategories(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                foreach (string specialCategory in protocolDoc.SpecialCategoryList)
                {
                    string spSaveSpecialCategory = SPProtocol.SP_SAVE_PROTOCOL_SPECIAL_CATEGORY;
                    using (DbCommand spcialCatCommand = db.GetStoredProcCommand(spSaveSpecialCategory))
                    {
                        spcialCatCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(spcialCatCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        db.AddInParameter(spcialCatCommand, "@ProtocolSpecialCategory", DbType.String, specialCategory);
                        db.AddInParameter(spcialCatCommand, "UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(spcialCatCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol special categories failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolSection table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolSection(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                foreach (ProtocolSection section in protocolDoc.ProtocolSectionList)
                {
                    string spSaveSection = SPProtocol.SP_SAVE_PROTOCOL_SECTION;
                    using (DbCommand sectionCommand = db.GetStoredProcCommand(spSaveSection))
                    {
                        sectionCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(sectionCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        db.AddInParameter(sectionCommand, "@SectionID", DbType.Int32, null);
                        db.AddInParameter(sectionCommand, "@SectionTypeID", DbType.Int32, section.ProtocolSectionType);

                        // Modify data for string comparison
                        string htmlText = FormatSectionHTML(section);
                        if (htmlText != string.Empty)
                            htmlText = htmlText.Trim();

                        db.AddInParameter(sectionCommand, "@HTML", DbType.String, htmlText);
                        // The audience value is set to NULL in the database for all previous record.
                        db.AddInParameter(sectionCommand, "@Audience", DbType.String, null);
                        db.AddInParameter(sectionCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(sectionCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol section failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Reformat the html string to match what's in current database
        /// </summary>
        /// <param name="section"></param>
        /// <returns>htmlText</returns>
        internal string FormatSectionHTML(ProtocolSection section)
        {
            string htmlText = string.Empty;
            // To modify the html format to match what's saved in current database
            string attributeName = section.Html.DocumentElement.GetAttribute("name");
            if (attributeName.Length > 0)
            {
                htmlText = section.Html.DocumentElement.InnerXml;
                attributeName = attributeName.Replace(":", "_");
                htmlText = htmlText.Replace("<a name=\"" + attributeName + "\" />", "<a name=\"" + attributeName + "\"></a>");
            }
            else
                htmlText = section.Html.InnerXml;


                //TODO: REMOVE - Change for string comparison purpose.  
            if (htmlText.Trim().Length > 0)
            {
                if (section.ProtocolSectionType == ProtocolSectionType.Terms ||
                    section.ProtocolSectionType == ProtocolSectionType.CTGovTerms)
                {
                    htmlText = htmlText.Replace("<br></br>", "<br>");
                }
                if (section.ProtocolSectionType == ProtocolSectionType.RegistryInformation)
                {
                    htmlText = htmlText.Replace("</div><p></a>", "</div><p></p>");
                }

                //CTRedesign - remove leading </a>
                if (section.ProtocolSectionType == ProtocolSectionType.HPDisclaimer ||
                    section.ProtocolSectionType == ProtocolSectionType.PDisclaimer)
                {

                    //htmlText = "</a>" + htmlText;
                }
                if (section.ProtocolSectionType == ProtocolSectionType.RegistryInformation)
                {
                    htmlText = htmlText.Replace("<p />", "<p></p>");
                }
                if (section.ProtocolSectionType == ProtocolSectionType.EntryCriteria ||
                    section.ProtocolSectionType == ProtocolSectionType.Outcomes ||
                    section.ProtocolSectionType == ProtocolSectionType.Objectives ||
                    section.ProtocolSectionType == ProtocolSectionType.ExpectedEnrollment ||
                    section.ProtocolSectionType == ProtocolSectionType.Outline ||
                    section.ProtocolSectionType == ProtocolSectionType.PDisclaimer ||
                    section.ProtocolSectionType == ProtocolSectionType.HPDisclaimer ||
                    section.ProtocolSectionType == ProtocolSectionType.PatientAbstract ||
                    section.ProtocolSectionType == ProtocolSectionType.CTGovEntryCriteria ||
                    section.ProtocolSectionType == ProtocolSectionType.CTGovBriefSummary ||
                    section.ProtocolSectionType == ProtocolSectionType.CTGovDetailedDescription)
                {
                    htmlText = htmlText.Replace("<LI class=\"Protocol-IL-Bullet\" />", "<LI class=\"Protocol-IL-Bullet\"></LI>");
                    htmlText = htmlText.Replace("<LI class=\"Protocol-OL-URoman\" />", "<LI class=\"Protocol-OL-URoman\"></LI>");
                    htmlText = htmlText.Replace("<br />", "<br/>");
                    htmlText = htmlText.Replace("<sup />", "<sup></sup>");
                    htmlText = htmlText.Replace("<b />", "<b></b>"); 
                    ReplaceEndTag(ref htmlText, "a");
                    ReplaceEndTag(ref htmlText, "P");
                    //htmlText = htmlText.Replace(" />", "></a>");
                }
                if (section.ProtocolSectionType == ProtocolSectionType.CTGovEntryCriteria)
                {
                    htmlText = htmlText + "<a name=\"EndEntryCriteria\">";
                }
                if (section.ProtocolSectionType == ProtocolSectionType.PublishedResults ||
                    section.ProtocolSectionType == ProtocolSectionType.RelatedPublications)
                {
                    htmlText = htmlText.Replace("<A />", "<A></A>");
                }
                if (section.ProtocolSectionType == ProtocolSectionType.LastMod)
                 {
                    htmlText = htmlText.Replace("<td />", "<td></td>");
                }
                if (section.ProtocolSectionType == ProtocolSectionType.CTGovLeadOrgs ||
                    section.ProtocolSectionType == ProtocolSectionType.LeadOrgs)
                {
                    htmlText = htmlText.Replace("<Span Class=\"Protocol-LeadOrg-Phone\" />", "<Span Class=\"Protocol-LeadOrg-Phone\"></Span>");
                    htmlText = htmlText.Replace("<td valign=\"top\" width=\"54%\" />", "<td valign=\"top\" width=\"54%\"></td>");
                }

                // Take care of GlossaryTermRef link space
                string glossaryTermTag = "Protocol-GlossaryTermRef";
                if (htmlText.Contains("Protocol-GlossaryTermRef"))
                {
                    BuildGlossaryTermRefLink(ref htmlText, glossaryTermTag);
                }
            }
            return htmlText;
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolAlternateID table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolAlternateID(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                foreach (AlternateProtocolID alterID in protocolDoc.AlternateIDList)
                {
                    string spSaveAlternateID = SPProtocol.SP_SAVE_PROTOCOL_ALTERNATE_ID;
                    using (DbCommand alterIDCommand = db.GetStoredProcCommand(spSaveAlternateID))
                    {
                        alterIDCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(alterIDCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        db.AddInParameter(alterIDCommand, "@IDString", DbType.String, alterID.IdString);
                        db.AddInParameter(alterIDCommand, "@idtype", DbType.String, alterID.Type);
                        db.AddInParameter(alterIDCommand, "@updateUserid", DbType.String, userID);
                        db.ExecuteNonQuery(alterIDCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol alternate id failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolSponsors table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolSponsors(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                foreach (string sponsor in protocolDoc.SponsorList)
                {
                    string spSaveSponsor = SPProtocol.SP_SAVE_PROTOCOL_SPONSORS;
                    using (DbCommand sponsorCommand = db.GetStoredProcCommand(spSaveSponsor))
                    {
                        sponsorCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(sponsorCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        db.AddInParameter(sponsorCommand, "@SponsorName", DbType.String, sponsor.Trim());
                        db.AddInParameter(sponsorCommand, "@updateUserid", DbType.String, userID);
                        db.ExecuteNonQuery(sponsorCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol sponsors failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolStudyCategory table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolStudyCategory(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                foreach (string study in protocolDoc.StudyCategoryList)
                {
                    string spStudyCategory = SPProtocol.SP_SAVE_PROTOCOL_STUDY_CATEGORY;
                    using (DbCommand studyCommand = db.GetStoredProcCommand(spStudyCategory))
                    {
                        studyCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(studyCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        db.AddInParameter(studyCommand, "@StudyCategoryName", DbType.String, study.Trim());
                        db.AddInParameter(studyCommand, "@updateUserid", DbType.String, userID);
                        db.ExecuteNonQuery(studyCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol study category failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolModality table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolModality(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                foreach (ProtocolModality modality in protocolDoc.ModalityList)
                {
                    string spSaveModality = SPProtocol.SP_SAVE_PROTOCOL_MODALITY;
                    using (DbCommand modalityCommand = db.GetStoredProcCommand(spSaveModality))
                    {
                        modalityCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(modalityCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        db.AddInParameter(modalityCommand, "@ModalityID", DbType.Int32, modality.ModalityID);
                        db.AddInParameter(modalityCommand, "@ModalityName", DbType.String, modality.Type);
                        db.AddInParameter(modalityCommand, "@updateUserid", DbType.String, userID);
                        db.ExecuteNonQuery(modalityCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol modality failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolPhase table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolPhase(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                foreach (int phase in protocolDoc.ProtocolPhaseList)
                {
                    string spSavePhase = SPProtocol.SP_SAVE_PROTOCOL_PHASE;
                    using (DbCommand phaseCommand = db.GetStoredProcCommand(spSavePhase))
                    {
                        phaseCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(phaseCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        db.AddInParameter(phaseCommand, "@Phase", DbType.Int16, phase);
                        db.AddInParameter(phaseCommand, "@updateUserid", DbType.String, userID);
                        db.ExecuteNonQuery(phaseCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol phase failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolDetail table
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolDetail(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                string spSaveDetail = SPProtocol.SP_SAVE_PROTOCOL_DETAIL;
                using (DbCommand detailCommand = db.GetStoredProcCommand(spSaveDetail))
                {
                    detailCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(detailCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                    string hpTitle = protocolDoc.HealthProfessionalTitle.Trim();
                    if (hpTitle.Length > 0)
                        db.AddInParameter(detailCommand, "@HealthProfessionalTitle", DbType.String, hpTitle);
                    else
                        db.AddInParameter(detailCommand, "@HealthProfessionalTitle", DbType.String, null);
                    string ptTitle = protocolDoc.PatientTitle.Trim();
                    if (ptTitle.Length > 0)
                       db.AddInParameter(detailCommand, "@Patienttitle", DbType.String, ptTitle);
                    else
                       db.AddInParameter(detailCommand, "@Patienttitle", DbType.String, null);
                    db.AddInParameter(detailCommand, "@LowAge", DbType.Int16, protocolDoc.LowAge);
                    db.AddInParameter(detailCommand, "@HighAge", DbType.Int16, protocolDoc.HighAge);
                    string ageRange = protocolDoc.AgeRange;
                    if (ageRange != string.Empty)
                        ageRange = ageRange.Trim();
                    db.AddInParameter(detailCommand, "@AgeRange", DbType.String, ageRange);
                    // TODO: IsPatientVersionExist field is not implemented in the old cold, the value is 0 in database table
                    // Here I set it to 0 unless we discovered later is it needed.
                    db.AddInParameter(detailCommand, "@IsPatientVersionExists", DbType.Int16, 0);
                    db.AddInParameter(detailCommand, "@isCTProtocol", DbType.Int16, protocolDoc.IsCTProtocol);
                    string status = protocolDoc.Status;
                    if (status != string.Empty)
                        status = status.Trim();
                    db.AddInParameter(detailCommand, "@CurrentStatus", DbType.String, status);

                    // Set FirstPublishedDate to NULL if the date is not available in XML
                    if (protocolDoc.FirstPublishedDate == DateTime.MinValue)
                        db.AddInParameter(detailCommand, "@DateFirstPublished", DbType.DateTime, null);
                    else
                        db.AddInParameter(detailCommand, "@DateFirstPublished", DbType.DateTime, protocolDoc.FirstPublishedDate);

                    // Set LastModifiedDate to NULL if the date is not available in XML
                    if (protocolDoc.LastModifiedDate == DateTime.MinValue)
                        db.AddInParameter(detailCommand, "@DateLastModified", DbType.DateTime, null);
                    else
                        db.AddInParameter(detailCommand, "@DateLastModified", DbType.DateTime, protocolDoc.LastModifiedDate);

                    // ID for the protocol's primary pretty URL.
                    db.AddInParameter(detailCommand, "@PrimaryPrettyUrlID", DbType.String, protocolDoc.PrimaryProtocolUrlID);
                    
                    db.AddInParameter(detailCommand, "@updateUserid", DbType.String, userID);
                    db.ExecuteNonQuery(detailCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol detail failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call stored procedure to save the protocol id for the secondary pretty Url
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveProtocolSecondaryPrettyUrlID(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                // Don't save the secondary URL unless it's present.
                if (!string.IsNullOrEmpty(protocolDoc.SecondaryProtocolUrlID))
                {
                    string spSavePrettyUrlID = SPProtocol.SP_SAVE_PROTOCOL_SECONDARY_PRETTY_URL_ID;
                    using (DbCommand prettyUrlCommand = db.GetStoredProcCommand(spSavePrettyUrlID))
                    {
                        prettyUrlCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(prettyUrlCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);

                        // ID for the protocol's primary pretty URL.
                        db.AddInParameter(prettyUrlCommand, "@SecondaryPrettyUrlID", DbType.String, protocolDoc.SecondaryProtocolUrlID);

                        db.AddInParameter(prettyUrlCommand, "@updateUserid", DbType.String, userID);

                        // Run the proc.
                        db.ExecuteNonQuery(prettyUrlCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol secondary pretty URL ID failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save protocol data into ProtocolLeadOrg, ProtocolTrailSite, ProtocolContactInfoHTML, ProtocolContactInfoHTMLMap tables
        /// </summary>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="protocolDoc"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SavePrototolContactInfo(Database db, DbTransaction transaction, ProtocolDocument protocolDoc, string userID)
        {
            try
            {
                List<int> IDList = new List<int>();
                List<int> HTMLIDList = new List<int>();
                int organizationID = 0;
                string state = string.Empty;
                string city = string.Empty;
                string country = string.Empty;
                int count = protocolDoc.ContactInfoList.Count;
                foreach (ProtocolContactInfo site in protocolDoc.ContactInfoList)
                {
                    // Save the lead organization contact information
                    int contactInfoID = 0;
                    int contactInfoHTMLID = 0;

                    if (site.IsLeadOrg)
                    {
                        string spSaveLeadOrg = SPProtocol.SP_SAVE_PROTOCOL_LEAD_ORG;
                        using (DbCommand leadOrgCommand = db.GetStoredProcCommand(spSaveLeadOrg))
                        {
                            leadOrgCommand.CommandType = CommandType.StoredProcedure;
                            db.AddOutParameter(leadOrgCommand, "@ProtocolContactInfoID", DbType.Int32, site.ProtocolContactInfoID);
                            db.AddInParameter(leadOrgCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                            if (site.OrganizationID == 0)
                                db.AddInParameter(leadOrgCommand, "@OrganizationID", DbType.Int32, null);
                            else
                                db.AddInParameter(leadOrgCommand, "@OrganizationID", DbType.Int32, site.OrganizationID);
                            if (site.PersonID > 0)
                                db.AddInParameter(leadOrgCommand, "@PersonID", DbType.Int32, site.PersonID);
                            else
                                db.AddInParameter(leadOrgCommand, "@PersonID", DbType.Int32, null);
                            string orgName = site.OrganizationName;
                            if (orgName != string.Empty)
                                orgName = orgName.Trim();
                            db.AddInParameter(leadOrgCommand, "@OrganizationName", DbType.String, orgName);
                            if (site.PersonGivenName.Length > 0)
                                db.AddInParameter(leadOrgCommand, "@PersonGivenName", DbType.String, site.PersonGivenName.Trim());
                            else
                                db.AddInParameter(leadOrgCommand, "@PersonGivenName", DbType.String, null);
                            if (site.PersonSurName.Length > 0)
                                db.AddInParameter(leadOrgCommand, "@PersonSurName", DbType.String, site.PersonSurName.Trim());
                            else
                                db.AddInParameter(leadOrgCommand, "@PersonSurName", DbType.String, null);
                            if (site.PersonProfessionalSuffix.Length > 0)
                                db.AddInParameter(leadOrgCommand, "@PersonProfessionalSuffix", DbType.String, site.PersonProfessionalSuffix.Trim());
                            else
                                db.AddInParameter(leadOrgCommand, "@PersonProfessionalSuffix", DbType.String, null);
                            if (site.PersonRole.Length > 0)
                                db.AddInParameter(leadOrgCommand, "@PersonRole", DbType.String, site.PersonRole.Trim());
                            else
                                db.AddInParameter(leadOrgCommand, "@PersonRole", DbType.String, null);
                            db.AddInParameter(leadOrgCommand, "@OrganizationRole", DbType.String, site.OrganizationRole);
                            db.AddInParameter(leadOrgCommand, "@City", DbType.String, site.City);
                            db.AddInParameter(leadOrgCommand, "@StateID", DbType.Int32, site.StateID);
                            db.AddInParameter(leadOrgCommand, "@Country", DbType.String, site.Country);
                            db.AddInParameter(leadOrgCommand, "@UpdateUserID", DbType.String, userID);
                            db.ExecuteNonQuery(leadOrgCommand, transaction);
                            contactInfoID = Int32.Parse(db.GetParameterValue(leadOrgCommand, "ProtocolContactInfoID").ToString());
                        }
                    }
                    else
                    {
                        // Save the site contact information
                        string spSaveSite = SPProtocol.SP_SAVE_PROTOCOL_TRIAL_SITE;
                        using (DbCommand siteCommand = db.GetStoredProcCommand(spSaveSite))
                        {
                            siteCommand.CommandType = CommandType.StoredProcedure;
                            db.AddOutParameter(siteCommand, "@ProtocolContactInfoID", DbType.Int32, site.ProtocolContactInfoID);
                            db.AddInParameter(siteCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                            if (site.OrganizationID == 0)
                                db.AddInParameter(siteCommand, "@OrganizationID", DbType.Int32, null);
                            else
                                db.AddInParameter(siteCommand, "@OrganizationID", DbType.Int32, site.OrganizationID);
                            if (site.PersonID > 0)
                                db.AddInParameter(siteCommand, "@PersonID", DbType.Int32, site.PersonID);
                            else
                                db.AddInParameter(siteCommand, "@PersonID", DbType.Int32, null);
                            string orgName = site.OrganizationName.Trim();
                            if (orgName.Length > 0)
                               db.AddInParameter(siteCommand, "@OrganizationName", DbType.String, orgName);
                            else
                               db.AddInParameter(siteCommand, "@OrganizationName", DbType.String, null);
                            if (site.PersonGivenName.Length > 0)
                                db.AddInParameter(siteCommand, "@PersonGivenName", DbType.String, site.PersonGivenName.Trim());
                            else
                                db.AddInParameter(siteCommand, "@PersonGivenName", DbType.String, null);
                            if (site.PersonSurName.Length > 0)
                                db.AddInParameter(siteCommand, "@PersonSurName", DbType.String, site.PersonSurName.Trim());
                            else
                                db.AddInParameter(siteCommand, "@PersonSurName", DbType.String, null);
                            if (site.PersonProfessionalSuffix.Length > 0)
                                db.AddInParameter(siteCommand, "@PersonProfessionalSuffix", DbType.String, site.PersonProfessionalSuffix.Trim());
                            else
                                db.AddInParameter(siteCommand, "@PersonProfessionalSuffix", DbType.String, null);
                            db.AddInParameter(siteCommand, "@OrganizationRole", DbType.String, site.OrganizationRole);
                            db.AddInParameter(siteCommand, "@City", DbType.String, site.City);
                            db.AddInParameter(siteCommand, "@StateID", DbType.Int32, site.StateID);
                            if (site.State.Length > 0)
                                db.AddInParameter(siteCommand, "@statefullname", DbType.String, site.State);
                            else
                                db.AddInParameter(siteCommand, "@statefullname", DbType.String, null);
                            db.AddInParameter(siteCommand, "@Country", DbType.String, site.Country);
                            db.AddInParameter(siteCommand, "@ZIP", DbType.String, site.PostalCodeZip);
                            db.AddInParameter(siteCommand, "@UpdateUserID", DbType.String, userID);
                            db.ExecuteNonQuery(siteCommand, transaction);
                            contactInfoID = Int32.Parse(db.GetParameterValue(siteCommand, "@ProtocolContactInfoID").ToString());
                        }

                        // Save protocol study site / constact html 
                        string html = site.Html;
                        if (html != null && html.Trim() != string.Empty)
                        {
                            string spSaveContactHTML = SPProtocol.SP_SAVE_PROTOCOL_CONTACT_INFO_HTML;
                            using (DbCommand htmlCommand = db.GetStoredProcCommand(spSaveContactHTML))
                            {
                                htmlCommand.CommandType = CommandType.StoredProcedure;
                                db.AddOutParameter(htmlCommand, "@ProtocolContactInfoHTMLID", DbType.Int32, 0);
                                db.AddInParameter(htmlCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                                db.AddInParameter(htmlCommand, "@OrganizationName", DbType.String, site.OrganizationName.Trim());
                                db.AddInParameter(htmlCommand, "@City", DbType.String, site.City);
                                db.AddInParameter(htmlCommand, "@State", DbType.String, site.State);
                                db.AddInParameter(htmlCommand, "@Country", DbType.String, site.Country);
                                //TODO: Remove this is done for format matching the in database
                                html = html.Replace("<td />", "<td></td>");
                                html = html.Replace("<Span Class=\"Protocol-Site-Suffix\" />", "<Span Class=\"Protocol-Site-Suffix\"></Span>");
                                html = html.Replace("<Span Class=\"Protocol-Site-Phone\" />", "<Span Class=\"Protocol-Site-Phone\"></Span>");
                                db.AddInParameter(htmlCommand, "@HTML", DbType.String, html.Trim());
                                db.AddInParameter(htmlCommand, "@updateUserid", DbType.String, userID);
                                db.ExecuteNonQuery(htmlCommand, transaction);
                                contactInfoHTMLID = Int32.Parse(db.GetParameterValue(htmlCommand, "@ProtocolContactInfoHTMLID").ToString());
                            }
                        }
                    }

                    count--;
                    // Add the last contact html map in the list
                    if (count == 0)
                    {
                        // Clear the storage list first if this is a different organization than previous
                        if (organizationID != site.OrganizationID || city != site.City || state != site.State || country != site.Country )
                        {
                            // Save it before clear
                            foreach (int ID in IDList)
                            {
                                foreach (int htmlID in HTMLIDList)
                                {
                                    if (ID > 0 && htmlID > 0)
                                    {
                                        // SP: 	foreach (contactInfoHTMLID): usp_SaveProtocolContactInfoHTMLMap
                                        string spSaveHTMLIDMap = SPProtocol.SP_SAVE_PROTOCOL_CONTACT_INFO_HTML_MAP;
                                        using (DbCommand mapCommand = db.GetStoredProcCommand(spSaveHTMLIDMap))
                                        {
                                            mapCommand.CommandType = CommandType.StoredProcedure;
                                            db.AddInParameter(mapCommand, "@ProtocolContactInfoHTMLID", DbType.Int32, htmlID);
                                            db.AddInParameter(mapCommand, "@ProtocolContactInfoID", DbType.Int32, ID);
                                            db.ExecuteNonQuery(mapCommand, transaction);
                                        }
                                    }
                                }
                            }
                            IDList.Clear();
                            HTMLIDList.Clear();
                        }
                        if (contactInfoID > 0)
                            IDList.Add(contactInfoID);
                        if (contactInfoHTMLID > 0)
                            HTMLIDList.Add(contactInfoHTMLID);
                    }

                    // Save the previous collection under one orgainzation into database map table
                    // There are case the org name are the same and org id is not availabel but all orgs reside in different places
                    if ((organizationID > 0 && organizationID != site.OrganizationID) ||  
                        city != site.City || state != site.State || country != site.Country || count == 0)
                    {
                        foreach (int ID in IDList)
                        {
                            foreach (int htmlID in HTMLIDList)
                            {
                                if (ID > 0 && htmlID > 0)
                                {
                                    // SP: 	foreach (contactInfoHTMLID): usp_SaveProtocolContactInfoHTMLMap
                                    string spSaveHTMLIDMap = SPProtocol.SP_SAVE_PROTOCOL_CONTACT_INFO_HTML_MAP;
                                    using (DbCommand mapCommand = db.GetStoredProcCommand(spSaveHTMLIDMap))
                                    {
                                        mapCommand.CommandType = CommandType.StoredProcedure;
                                        db.AddInParameter(mapCommand, "@ProtocolContactInfoHTMLID", DbType.Int32, htmlID);
                                        db.AddInParameter(mapCommand, "@ProtocolContactInfoID", DbType.Int32, ID);
                                        db.ExecuteNonQuery(mapCommand, transaction);
                                    }
                                }
                            }
                        }
                        IDList.Clear();
                        HTMLIDList.Clear();
                    }

                    // Add new generated contactInfoID and contactInfoHTMLID into collection list
                    if (count > 0)
                    {
                        if (contactInfoID > 0)
                            IDList.Add(contactInfoID);
                        if (contactInfoHTMLID > 0)
                            HTMLIDList.Add(contactInfoHTMLID);
                    }

                    // Remember the organizationID, city, state, country to carry to the next loop;
                    organizationID = site.OrganizationID;
                    city = site.City;
                    state = site.State;
                    country = site.Country;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving protocol contact information failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
        }

       /// <summary>
        /// Call store procedure to set CTGovProtocol DateFirstPublished and isNew value.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="protocolDoc"></param>
        /// <returns></returns>
        private void DetermineFirstPublishedDate(Database db, ProtocolDocument protocolDoc)
        {
            IDataReader reader = null;
            try
            {
                // Special logic for CTGovProtocol :if first published data is missing, get the date from database.  
                // If no record in database, use the document received date
                if (protocolDoc.FirstPublishedDate == DateTime.MinValue)
                {
                    // Get DateFirstPublished from database
                    string spGetDateFirstPublished = SPProtocol.SP_GET_DATE_FIRST_PUBLISHED;
                    using (DbCommand spGetDateCommand = db.GetStoredProcCommand(spGetDateFirstPublished))
                    {
                        spGetDateCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(spGetDateCommand, "@ProtocolID", DbType.Int32, protocolDoc.ProtocolID);
                        reader = db.ExecuteReader(spGetDateCommand);
                        if (reader.Read())
                        {
                            if (reader["DateFirstPublished"].ToString().Trim() != string.Empty || reader["DateFirstPublished"] != DBNull.Value)
                                protocolDoc.FirstPublishedDate = DateTime.Parse(reader["DateFirstPublished"].ToString());
                            else
                                protocolDoc.FirstPublishedDate = protocolDoc.ReceivedDate;
                        }
                        else
                            protocolDoc.FirstPublishedDate = protocolDoc.ReceivedDate;
                    }
                }

                // Determine if this is a new CTGovProtocol 
                // If the document is published within 30 days, it is a new protocol.
                if (protocolDoc.FirstPublishedDate != null)
                {
                    DateTime now = DateTime.Now;
                    DateTime comparedDate = protocolDoc.FirstPublishedDate.AddDays(30);
                    if (comparedDate.CompareTo(now) > 0)
                        protocolDoc.IsNew = 1;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Retrieving protocol DateFirstPublished failed. Document CDRID=" + protocolDoc.DocumentID.ToString(), e);
            }
            {
                reader.Close();
                reader.Dispose();
            }
        }
        #endregion
    }
  }
