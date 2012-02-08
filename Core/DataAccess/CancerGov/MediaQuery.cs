using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Transactions;
using System.IO;
using System.Configuration;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.StoreProcedures;
using GateKeeper.DataAccess;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace GateKeeper.DataAccess.CancerGov
{
    public class MediaQuery : DocumentQuery
    {
        public override bool SaveDocument(Document document, string userID)
        {
            bool bSuccess = true;
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();
            try
            {
                SaveDBDocument(document, db, transaction);
                transaction.Commit();
            }
            catch (Exception e)
            {
                bSuccess = false;
                transaction.Rollback();
                throw new Exception("Database Error: Saving media document failed. Document CDRID=" + document.DocumentID.ToString(), e);
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
                switch (databaseName)
                {
                    case ContentDatabase.Staging:
                        db = this.StagingDBWrapper.SetDatabase();
                        break;
                    case ContentDatabase.Preview:
                        db = this.PreviewDBWrapper.SetDatabase();
                        break;
                    case ContentDatabase.Live:
                        db = this.LiveDBWrapper.SetDatabase();
                        break;
                    default:
                        throw new Exception("Database Error: Unable to find image path in the configuation file.");
                }

                // Delete the document in document table
                ClearDocument(document.DocumentID, db, databaseName.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Could not delete media files from staging. Document ID = " + document.DocumentID.ToString(), e);
            }
        }

        public override void PushDocumentToPreview(Document document, string userID)
        {
            Database db = this.StagingDBWrapper.SetDatabase();
            try
            {
                // SP: Call push document 
                PushDocument(document.DocumentID, db, ContentDatabase.Preview.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing media document data to preview database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }

        public override void PushDocumentToLive(Document document, string userID)
        {
            Database db = this.PreviewDBWrapper.SetDatabase();
            try
            {
                // SP: Call Push document
                PushDocument(document.DocumentID, db, ContentDatabase.Live.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing media document data to live database failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }
    }
}