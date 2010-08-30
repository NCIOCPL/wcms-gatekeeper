using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Transactions;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GeneticsProfessional;
using GateKeeper.DataAccess.StoreProcedures;
using GateKeeper.DataAccess;
using GateKeeper.Logging;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace GateKeeper.DataAccess.CancerGov
{
    public class GenProfQuery : DocumentQuery
    {
        #region Public methods
        /// <summary>
        /// Save genetic professional document into CDR staging database
        /// </summary>
        /// <param name="glossaryDoc">
        /// Glossary Term document object
        /// </param>
        public override bool SaveDocument(Document document,string userID)
        {
            bool bSuccess = true;
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                GeneticsProfessionalDocument GenProfDoc = (GeneticsProfessionalDocument)document;

                // SP: Clear extracted data
                ClearExtractedData(GenProfDoc.DocumentID, db, transaction);

                // SP: Save genetic professional
                SaveGeneticProfessional(GenProfDoc, db, transaction, userID);

                // SP: Save genetic professional location
                SaveLocation(GenProfDoc, db, transaction, userID);

                //SP: Save genetic professional family cancer syndrome
                SaveFamilyCancerSyndrome(GenProfDoc, db, transaction, userID);

                //SP: Save genetic professional document data
                SaveDBDocument(GenProfDoc, db, transaction);

                transaction.Commit();
            }
            catch (Exception e)
            {
                bSuccess = false;
                transaction.Rollback();
                throw new Exception("Database Error: Saving genetic professional document failed. Document CDRID=" + document.DocumentID.ToString(), e);
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
        /// Delete genetic professional document in database
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
                    ClearExtractedData(document.DocumentID, db, transaction);

                    // SP: Clear document
                    ClearDocument(document.DocumentID, db, transaction, databaseName.ToString());
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Deleting genetic professional document in " + databaseName.ToString() + " database failed. Document CDRID=" + document.DocumentID.ToString(), e);
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
                throw new Exception("Database Error: Deleting genetic professional document data in " + databaseName.ToString() + " database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Push genetic professional document into preview database
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
                    // SP: Call push glossary term document
                    string spPushData = SPGenProf.SP_PUSH_GENPROF;
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
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedGeneticsProfessionalData failed.", e);
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
                throw new Exception("Database Error: Pushing genetic professional document data to preview database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Push genetic professional document into live database
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
                // Rollback the change only if push genetic professional sp failed.
                try
                {
                    // SP: Call push glossary term document
                    string spPushData = SPGenProf.SP_PUSH_GENPROF;
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
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedGeneticsProfessionalData failed", e);
                }

                // SP: Call Push document
                PushDocument(document.DocumentID, db, ContentDatabase.Live.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing genetic professional document data to live database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }
        }
        #endregion

        #region Private region
        // <summary>
        /// Call store procedure to clear existing genetic professional data in database
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
                string spClearExtractedData = SPGenProf.SP_CLEAR_GENPROF_DATA;
                using (DbCommand clearCommand = db.GetStoredProcCommand(spClearExtractedData))
                {
                    clearCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(clearCommand, "@DocumentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(clearCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Clearing genetic professional data failed. Document CDRID=" + documentID.ToString(), e);
            }
        }

        // <summary>
        /// Call store procedure to save genetic professional data in GenProf table
        /// </summary>
        /// <param name="GenProfDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveGeneticProfessional(GeneticsProfessionalDocument GenProfDoc, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                string spSaveGenProf = SPGenProf.SP_SAVE_GENPROF;
                using (DbCommand saveCommand = db.GetStoredProcCommand(spSaveGenProf))
                {
                    saveCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(saveCommand, "@GenProfID", DbType.Int32, GenProfDoc.DocumentID);
                    if (GenProfDoc.ShortName != null)
                        GenProfDoc.ShortName = GenProfDoc.ShortName.Trim();
                    db.AddInParameter(saveCommand, "@ShortName", DbType.String, GenProfDoc.ShortName);
                    if (GenProfDoc.FirstName != null)
                        GenProfDoc.FirstName = GenProfDoc.FirstName.Trim();
                    db.AddInParameter(saveCommand, "@FirstName", DbType.String, GenProfDoc.FirstName);
                    if (GenProfDoc.LastName != null)
                        GenProfDoc.LastName = GenProfDoc.LastName.Trim();
                    db.AddInParameter(saveCommand, "@LastName", DbType.String, GenProfDoc.LastName);
                    if (GenProfDoc.Suffix != null && GenProfDoc.Suffix.Trim().Length > 0)
                        db.AddInParameter(saveCommand, "@Suffix", DbType.String, GenProfDoc.Suffix.Trim());
                    else
                        db.AddInParameter(saveCommand, "@Suffix", DbType.String, null);
                    if (GenProfDoc.Degrees[0] != null)
                        GenProfDoc.Degrees[0] = GenProfDoc.Degrees[0].Trim();
                    db.AddInParameter(saveCommand, "@Degree", DbType.String, GenProfDoc.Degrees[0]);
                    db.AddInParameter(saveCommand, "@XML", DbType.String, GenProfDoc.Xml.OuterXml);
                    db.AddInParameter(saveCommand, "@UpdateUserID", DbType.String, userID);
                    db.ExecuteNonQuery(saveCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving genetic professional data into GenProf table failed. Document CDRID=" + GenProfDoc.DocumentID.ToString(), e);
            }
        }

       // <summary>
        /// Call store procedure to save genetic professional locations data in database
        /// </summary>
        /// <param name="GenProfDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveLocation(GeneticsProfessionalDocument GenProfDoc, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                foreach (PracticeLocation location in GenProfDoc.PracticeLocations)
                {
                    string spSaveLocation = SPGenProf.SP_SAVE_GENPROF_LOCATION;
                    using (DbCommand locationCommand = db.GetStoredProcCommand(spSaveLocation))
                    {
                        locationCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(locationCommand, "@GenProfID", DbType.Int32, GenProfDoc.DocumentID);
                        db.AddInParameter(locationCommand, "@City", DbType.String, location.City.Trim());
                        db.AddInParameter(locationCommand, "@State", DbType.String, location.State.Trim());
                        db.AddInParameter(locationCommand, "@PostalCode", DbType.String, location.PostalCode.Trim());
                        db.AddInParameter(locationCommand, "@Country", DbType.String, location.Country.Trim());
                        db.AddInParameter(locationCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(locationCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving genetic professional location failed. Document CDRID=" + GenProfDoc.DocumentID.ToString(), e);
            }
        }

        // <summary>
        /// Call store procedures to save genetic professional syndromes and cancer types in database
        /// </summary>
        /// <param name="GenProfDoc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void SaveFamilyCancerSyndrome(GeneticsProfessionalDocument GenProfDoc, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                foreach (FamilyCancerSyndrome syndrome in GenProfDoc.FamilyCancerSyndromes)
                {
                    // Save family cancer syndrome
                    string spSaveSyndrome = SPGenProf.SP_SAVE_GENPROF_SYNDROME;
                    using (DbCommand syndromeCommand = db.GetStoredProcCommand(spSaveSyndrome))
                    {
                        syndromeCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(syndromeCommand, "@GenProfID", DbType.Int32, GenProfDoc.DocumentID);
                        if (syndrome.SyndromeName != null)
                            syndrome.SyndromeName.Trim();
                        db.AddInParameter(syndromeCommand, "@FamilyCancerSyndrome", DbType.String, syndrome.SyndromeName);
                        db.AddInParameter(syndromeCommand, "@UpdateUserID", DbType.String, userID);
                        db.AddOutParameter(syndromeCommand, "@FamilyCancerSyndromeID", DbType.Int32, 0);
                        db.ExecuteNonQuery(syndromeCommand, transaction);
                        int syndromeID = (int)db.GetParameterValue(syndromeCommand, "@FamilyCancerSyndromeID");

                        // Save cancer types with sites
                        foreach (string site in syndrome.CancerTypeSites)
                        {
                            string spSaveTypeOfCancer = SPGenProf.SP_SAVE_GENPROF_TYPEOFCANCER;
                            using (DbCommand cancerTypeCommand = db.GetStoredProcCommand(spSaveTypeOfCancer))
                            {
                                cancerTypeCommand.CommandType = CommandType.StoredProcedure;
                                db.AddInParameter(cancerTypeCommand, "@GenProfID", DbType.Int32, GenProfDoc.DocumentID);
                                db.AddInParameter(cancerTypeCommand, "@CancerTypeSite", DbType.String, site.Trim());
                                db.AddInParameter(cancerTypeCommand, "@FamilyCancerSyndromeID", DbType.String, syndromeID);
                                db.AddInParameter(cancerTypeCommand, "@UpdateUserID", DbType.String, userID);
                                db.ExecuteNonQuery(cancerTypeCommand, transaction);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving genetic professional family cancer syndrome failed. Document CDRID=" + GenProfDoc.DocumentID.ToString(), e);
            }
        }

        #endregion
    }
}
