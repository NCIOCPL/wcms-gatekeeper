using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Transactions;

using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.StoreProcedures;
using GateKeeper.DataAccess;

namespace GateKeeper.DataAccess.CancerGov
{
    public class GlossaryTermQuery : DocumentQuery
    {
        DictionaryQuery Dictionary = new DictionaryQuery();

        /// <summary>
        /// Class constructor.
        /// </summary>
        public GlossaryTermQuery()
        { }


        #region Query for Store Procedure Calls

        /// <summary>
        /// Save glossary term document into CDR staging database
        /// </summary>
        /// <param name="glossaryDoc">
        /// Glossary Term document object
        /// </param>
        public override bool SaveDocument(Document glossaryDoc, String userID)
        {
            bool bSuccess = true;
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                GlossaryTermDocument GTDocument = (GlossaryTermDocument)glossaryDoc;

                //GTDocument.Dictionary
                Dictionary.SaveDocument(GTDocument.DocumentID, GTDocument.Dictionary, userID);

                // Save Glossary Term document metadata.  Legacy code. (Do we need this?)
                SaveDBDocument(GTDocument, db, transaction);

                transaction.Commit();
            }
            catch (Exception e)
            {
                bSuccess = false;
                transaction.Rollback();
                throw new Exception("Database Error: Saving glossary term document failed. Document CDRID=" + glossaryDoc.DocumentID.ToString(), e);
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
        /// Retrieve a brief summary of a definition.
        /// </summary>
        /// <param name="cdrID">The ID of the GlossaryTerm to retrieve</param>
        /// <returns>A GlossaryTermSimple object containing the requested GlossaryTerm document.</returns>
        public GlossaryTermSimple GetGlossaryTerm(int cdrID)
        {
            Database db = StagingDBWrapper.SetDatabase();
            GlossaryTermSimple term = null;

            // Get document data
            IDataReader reader = null;
            try
            {
                string spGetData = SPGlossaryTerm.SP_GET_GLOSSARY_TERM;
                using (DbCommand getCommand = db.GetStoredProcCommand(spGetData))
                {
                    getCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(getCommand, "@GlossaryTermID", DbType.Int32, cdrID);

                    DataSet ds = db.ExecuteDataSet(getCommand);

                    reader = db.ExecuteReader(getCommand);
                    if (reader.Read())
                    {
                        // Get the Term names and pronunciation
                        String name = reader["TermName"].ToString();
                        String spanishName = reader["SpanishTermName"].ToString();
                        String pronunciation = reader["TermPronunciation"].ToString();

                        term = new GlossaryTermSimple(cdrID, name, spanishName, pronunciation);

                        // Get as many definitions as exist.
                        reader.NextResult();
                        while (reader.Read())
                        {
                            // Retreive fields
                            String tmpAudience = reader["Audience"].ToString();
                            String tmpLanguage = reader["Language"].ToString();
                            String definitionText = reader["DefinitionText"].ToString();
                            String definitionHtml = reader["DefinitionHTML"].ToString();
                            String mediaHtml = reader["MediaHTML"].ToString();
                            String audioHtml = reader["AudioMediaHTML"].ToString();
                            String relatedLinksHtml = reader["RelatedInformationHtml"].ToString();

                            // Convert the two tempoaries into strong types.
                            Language language = ConvertEnum<Language>.Convert(tmpLanguage);

                            // Audience is stored as either "Patient" or "Health professional".  The latter can't be converted
                            // as an enum, so we end up with a string compare.
                            AudienceType audience;
                            if (String.Compare(tmpAudience, "Patient", true) == 0)
                                audience = AudienceType.Patient;
                            else if (String.Compare(tmpAudience, "Health professional", true) == 0)
                                audience = AudienceType.HealthProfessional;
                            else
                                throw new Exception("Don't know how to convert audience type: '" + tmpAudience + "'");

                            GlossaryTermSimpleDefinition definition =
                                new GlossaryTermSimpleDefinition(audience, language, definitionText, definitionHtml, mediaHtml, audioHtml, relatedLinksHtml);
                            term.DefinitionList.Add(definition);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Retrieveing GlossaryTerm data from CDR staging database failed. Document CDRID=" + cdrID, ex);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }

            return term;
        }

        /// <summary>
        /// Delete glossary term document in database
        /// </summary>
        /// <param name="documentID"></param>
        /// <param name="dbName">database name</param>
        /// <param name="userID"></param>
        public override void DeleteDocument(Document glossaryDoc, ContentDatabase databaseName, string userID)
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
                    ClearExtractedData(glossaryDoc.DocumentID, db, transaction);

                    // SP: Clear document
                    ClearDocument(glossaryDoc.DocumentID, db, transaction, databaseName.ToString());
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Deleting glossary term document data in " + databaseName.ToString() + " database failed. Document CDRID=" + glossaryDoc.DocumentID.ToString(), e);
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
                throw new Exception("Database Error: Deleting glossary term document data in " + databaseName.ToString() + " database failed. Document CDRID=" + glossaryDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Push glossary term document into preview database
        /// </summary>
        /// <param name="documentID">
        /// <param name="userID">
        /// </param>
        public override void PushDocumentToPreview(Document glossaryDoc, string userID)
        {
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                try
                {
                    // SP: Call push glossary term document
                    string spPushData = SPGlossaryTerm.SP_PUSH_GT_DOCUMENT_DATA;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, glossaryDoc.DocumentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedGlossaryData failed.", e);
                }
                finally
                {
                    transaction.Dispose();
                }

                // SP: Call push document 
                PushDocument(glossaryDoc.DocumentID, db, ContentDatabase.Preview.ToString());

            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing glossary term document data to preview database failed. Document CDRID=" + glossaryDoc.DocumentID.ToString(), e);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Push glossary term document into live database
        /// </summary>
        /// <param name="documentID">
        /// <param name="userID">
        /// </param>
        public override void PushDocumentToLive(Document glossaryDoc, string userID)
        {
            Database db = this.PreviewDBWrapper.SetDatabase();
            DbConnection conn = this.PreviewDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                // Rollback the change only if push glossary term sp failed.
                try
                {
                    // SP: Call push glossary term document
                    string spPushData = SPGlossaryTerm.SP_PUSH_GT_DOCUMENT_DATA;
                    using (DbCommand pushCommand = db.GetStoredProcCommand(spPushData))
                    {
                        pushCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(pushCommand, "@DocumentID", DbType.Int32, glossaryDoc.DocumentID);
                        db.AddInParameter(pushCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(pushCommand, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedGlossaryData failed", e);
                }

                // SP: Call Push document
                PushDocument(glossaryDoc.DocumentID, db, ContentDatabase.Live.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing glossary term document data to live database failed. Document CDRID=" + glossaryDoc.DocumentID.ToString(), e);
            }
            finally
            {
                transaction.Dispose();
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion


        /// <summary>
        /// Call store procedure to clear existing glossary term data in database
        /// </summary>
        /// <param name="documentID"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [Obsolete("This method goes away in Feline.")]
        private void ClearExtractedData(int documentID, Database db, DbTransaction transaction)
        {
            try
            {
                string spClearExtractedData = SPGlossaryTerm.SP_CLEAR_GLOSSARY_DATA;
                using (DbCommand clearCommand = db.GetStoredProcCommand(spClearExtractedData))
                {
                    clearCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(clearCommand, "@DocumentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(clearCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Clearing glossary term data failed. Document CDRID=" + documentID.ToString(), e);
            }
        }

    }
}
