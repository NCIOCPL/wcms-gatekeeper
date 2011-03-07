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
using GateKeeper.Logging;
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
                string imagePath = string.Empty;
                Database db;
                switch (databaseName)
                {
                    case ContentDatabase.Staging:
                        db = this.StagingDBWrapper.SetDatabase();
                        imagePath = ConfigurationManager.AppSettings["ImageStaging"];
                        break;
                    case ContentDatabase.Preview:
                        db = this.PreviewDBWrapper.SetDatabase();
                        imagePath = ConfigurationManager.AppSettings["ImagePreview"];
                        break;
                    case ContentDatabase.Live:
                        db = this.LiveDBWrapper.SetDatabase();
                        imagePath = ConfigurationManager.AppSettings["ImageLive"];
                        break;
                    default:
                        throw new Exception("Database Error: Unable to find image path in the configuation file.");
                }

                // Delete the document in document table
                ClearDocument(document.DocumentID, db, databaseName.ToString());

                // Clear document in the server directory
                foreach (string filename in Directory.GetFiles(imagePath, "cdr" + document.DocumentID.ToString() + "*.*"))
                {
                     File.Delete(filename);
                }
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
            try
            {
                // Copy the files from staging location to the preview location
                string stagingPath = ConfigurationManager.AppSettings["ImageStaging"];
                string previewPath = ConfigurationManager.AppSettings["ImagePreview"];
                if (!Directory.Exists(previewPath))
                    Directory.CreateDirectory(previewPath);
                foreach (string filename in Directory.GetFiles(stagingPath, "cdr" + document.DocumentID.ToString() + "*.*"))
                {
                    string dest = filename.Replace(stagingPath, previewPath);
                    File.Copy(filename, dest, true);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Copy File Error: Could not copy media files from staging to preview. Document CDRID=" + document.DocumentID.ToString(), e);
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
            try
            {
                // Copy the files from preview location to the live location
                string previewPath = ConfigurationManager.AppSettings["ImagePreview"];
                string livePath = ConfigurationManager.AppSettings["ImageLive"];
                if (!Directory.Exists(livePath))
                    Directory.CreateDirectory(livePath);
                foreach (string filename in Directory.GetFiles(previewPath, "cdr" + document.DocumentID.ToString() + "*.*"))
                {
                    string dest = filename.Replace(previewPath, livePath);
                    File.Copy(filename, dest, true);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Copy File Error: Could not copy media files from preview to live. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }
    }
}