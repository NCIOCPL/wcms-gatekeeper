using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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
        const String SP_GET_GLOSSARY_TERM = "usp_GetGlossaryTerm";

        DictionaryQuery Dictionary = new DictionaryQuery();

        /// <summary>
        /// Class constructor.
        /// </summary>
        public GlossaryTermQuery()
        { }


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

            using (DbConnection conn = this.StagingDBWrapper.EnsureConnection())
            {
                using (DbTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        GlossaryTermDocument GTDocument = (GlossaryTermDocument)glossaryDoc;

                        // Save dictionary terms. That's the only artifact to come
                        // from GlossaryTerm documents.
                        Dictionary.SaveDocument(GTDocument.DocumentID, GTDocument.Dictionary, GTDocument.AliasList, transaction);

                        // Save Glossary Term document metadata.  Legacy code. (Do we need this?)
                        SaveDBDocument(GTDocument, db, transaction);

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        bSuccess = false;
                        transaction.Rollback();
                        throw new Exception("Error saving glossary term. Document CDRID=" + glossaryDoc.DocumentID.ToString(), e);
                    }
                }
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
                string spGetData = SP_GET_GLOSSARY_TERM;
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
                // The same DeleteDocument method is called across all promotion levels, and each 
                // database contains a same-named proc for deleting the data.  So,all we need to do
                // here is create a connection to the appropriate database and pass that along to
                // the routines which do the database call.

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
                    Dictionary.DeleteDocument(glossaryDoc.DocumentID, transaction);

                    // Clear Glossary document metadata.  Legacy code.
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

            using (DbConnection conn = this.StagingDBWrapper.EnsureConnection())
            {
                using (DbTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Copy dictionary term from the staging database to preview.
                        Dictionary.PushDocumentToPreview(glossaryDoc.DocumentID, transaction);

                        // Update Glossary document metadata.  Legacy code.
                        // ?????  WHY DOESN'T THIS USE A TRANSACTION  ?????
                        PushDocument(glossaryDoc.DocumentID, db, ContentDatabase.Preview.ToString());

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw new Exception(String.Format("Database error promoting document {0} to preview database.", glossaryDoc.DocumentID), e);
                    }
                }
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

            using (DbConnection conn = this.PreviewDBWrapper.EnsureConnection())
            {
                using (DbTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Copy dictionary term from the preview database to live.
                        Dictionary.PushDocumentToLive(glossaryDoc.DocumentID, transaction);

                        // Update Glossary document metadata.  Legacy code.
                        // ?????  WHY DOESN'T THIS USE A TRANSACTION  ?????
                        PushDocument(glossaryDoc.DocumentID, db, ContentDatabase.Live.ToString());
                        
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw new Exception(String.Format("Database error promoting document {0} to live database.", glossaryDoc.DocumentID), e);
                    }
                }
            }
        }

    }
}
