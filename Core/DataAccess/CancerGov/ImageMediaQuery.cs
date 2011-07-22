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
    public class ImageMediaQuery : MediaQuery
    {
        public override void DeleteDocument(Document document, ContentDatabase databaseName, string userID)
        {
            try
            {
                string imagePath = string.Empty;
                base.DeleteDocument(document, databaseName, userID);
                switch (databaseName)
                {
                    case ContentDatabase.Staging:
                        imagePath = ConfigurationManager.AppSettings["ImageStaging"];
                        break;
                    case ContentDatabase.Preview:
                        imagePath = ConfigurationManager.AppSettings["ImagePreview"];
                        break;
                    case ContentDatabase.Live:
                        imagePath = ConfigurationManager.AppSettings["ImageLive"];
                        break;
                    default:
                        throw new Exception("Database Error: Unable to find image path in the configuation file.");
                }
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
            base.PushDocumentToPreview(document, userID);

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
            base.PushDocumentToLive(document, userID);

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