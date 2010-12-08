using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Configuration;
using GateKeeper.DocumentObjects;
using GateKeeper.Logging;
using GateKeeper.DataAccess.StoreProcedures;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace GateKeeper.DataAccess
{
    /// <summary>
    /// Abstract base class for document adapters.
    /// </summary>
    public abstract class DocumentQuery : DbBaseQuery
    {
        #region Public Methods

        /// <summary>
        /// Saves the document to staging with userID identified
        /// </summary>
        /// <param name="document"></param>
        /// <param name="userID">keep track of user login id</param>
        /// <returns></returns>
        public abstract bool SaveDocument(Document document, string userID);

        /// <summary>
        /// Delete the document.
        /// </summary>
        /// <param name="documentID"></param>
        public abstract void DeleteDocument(Document document, ContentDatabase dbName, string userID);

        /// <summary>
        /// Promotes the document to preview.
        /// </summary>
        /// <param name="documentID"></param>
        public abstract void PushDocumentToPreview(Document document, string userID);

        /// <summary>
        /// Promotes the document to live.
        /// </summary>
        /// <param name="documentID"></param>
        public abstract void PushDocumentToLive(Document document, string userID);

        #endregion Public Methods

        #region Protected methods

        // <summary>
        /// Call store procedure to save document data into Document Table
        /// </summary>
        /// <param name="doc"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        protected void SaveDBDocument(Document doc, Database db, DbTransaction transaction)
        {
            try
            {
                String spSaveDocumentData = SPCommon.SP_SAVE_DOCUMENT_DATA;
                using (DbCommand spSaveDocCommand = db.GetStoredProcCommand(spSaveDocumentData))
                {
                    spSaveDocCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(spSaveDocCommand, "@DocumentID", DbType.Int32, doc.DocumentID);
                    db.AddInParameter(spSaveDocCommand, "@DocumentGUID", DbType.String, doc.GUID.ToString());
                    db.AddInParameter(spSaveDocCommand, "@DocumentTypeID", DbType.Int16, (Int16)doc.DocumentType);
                    db.AddInParameter(spSaveDocCommand, "@Version", DbType.String, doc.VersionNumber);
                    db.AddInParameter(spSaveDocCommand, "@IsActive", DbType.Int16, 1);
                    // Set DateLastModified to be null if the node is missing in xml file.
                    if ((doc.DocumentType == DocumentType.DrugInfoSummary ||
                        doc.DocumentType == DocumentType.Protocol ||
                        doc.DocumentType == DocumentType.CTGovProtocol 
                        ) && doc.LastModifiedDate == DateTime.MinValue)
                        db.AddInParameter(spSaveDocCommand, "@DateLastModified", DbType.DateTime, null);
                    else
                        db.AddInParameter(spSaveDocCommand, "@DateLastModified", DbType.DateTime, doc.LastModifiedDate);
                    db.ExecuteNonQuery(spSaveDocCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving data into document table failed. Document CDRID=" + doc.DocumentID.ToString(), e);
            }
        }


        // <summary>
        /// Call store procedure to push document data to preview/live database
        /// </summary>
        /// <param name="documentID"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        protected void PushDocument(int documentID, Database db, string dbName)
        {
            try
            {
                String spPushDocument = SPCommon.SP_PUSH_DOCUMENT;
                using (DbCommand spPushDocCommand = db.GetStoredProcCommand(spPushDocument))
                {
                    spPushDocCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(spPushDocCommand, "@documentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(spPushDocCommand);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing document to " + dbName + " database failed. Document CDRID=" + documentID.ToString(), e);
            }
        }

        // <summary>
        /// Call store procedure to clear document data in preview/live database
        /// </summary>
        /// <param name="documentID"></param
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        protected void ClearDocument(int documentID, Database db, DbTransaction transaction, string dbName)
        {
            try
            {
                String spClearDocument = SPCommon.SP_CLEAR_DOCUMENT;
                using (DbCommand spClearDocCommand = db.GetStoredProcCommand(spClearDocument))
                {
                    spClearDocCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(spClearDocCommand, "@documentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(spClearDocCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Clearing document at " + dbName + " database failed. Document CDRID=" + documentID.ToString(), e);
            }
        }

          // <summary>
        /// Call store procedure to clear document data in preview/live database
        /// This method is for calls that deos not involve with transaction
        /// </summary>
        /// <param name="documentID"></param
        /// <param name="db"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        protected void ClearDocument(int documentID, Database db, string dbName)
        {
            try
            {
                String spClearDocument = SPCommon.SP_CLEAR_DOCUMENT;
                using (DbCommand spClearDocCommand = db.GetStoredProcCommand(spClearDocument))
                {
                    spClearDocCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(spClearDocCommand, "@documentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(spClearDocCommand);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Clearing document at " + dbName + " database failed. Document CDRID=" + documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to retrieve document CDRID or GUID 
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        protected void GetDocumentIDs(ref int documentID, ref Guid documentGuid, Database db)
        {
            IDataReader reader = null;
            try
            {
                String spDocumentIDs = SPCommon.SP_GET_DOCUMENT_IDS;
                using (DbCommand spIDCommand = db.GetStoredProcCommand(spDocumentIDs))
                {
                    spIDCommand.CommandType = CommandType.StoredProcedure;
                    if (documentID > 0)
                        db.AddInParameter(spIDCommand, "@DocumentID", DbType.Int32, documentID);
                    else
                        db.AddInParameter(spIDCommand, "@DocumentID", DbType.Int32, null);
                    if (documentGuid != Guid.Empty)
                        db.AddInParameter(spIDCommand, "@DocumentGUID", DbType.Guid, documentGuid);
                    else
                        db.AddInParameter(spIDCommand, "@DocumentGUID", DbType.Guid, null);
                    reader = db.ExecuteReader(spIDCommand);
                    if (reader.Read())
                    {
                        if (documentID > 0)
                        {
                            if (reader["DocumentGuid"] != DBNull.Value)
                                documentGuid = (Guid)reader["DocumentGuid"];
                            else
                                // Assume this is a new document
                                documentGuid = Guid.Empty;
                        }
                        else
                        {
                            if (reader["DocumentID"] != DBNull.Value)
                                documentID = Int32.Parse(reader["DocumentID"].ToString());
                            else
                                // Assume this is a new document
                                documentID = 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Reading document CDRID/GUID Failed.", e);
            }
            finally {
                reader.Close();
                reader.Dispose();
            }
        }


        /// <summary>
        /// Call store procedure to retrieve document GUID 
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        protected Guid GetNCIViewID(Guid docGuid, Database db)
        {
            IDataReader reader = null;
            Guid nciViewID = Guid.Empty;
            try
            {
                string spGetViewID = SPCommon.SP_GET_NCI_VIEW_ID;
                using (DbCommand viewIDCommand = db.GetStoredProcCommand(spGetViewID))
                {
                    viewIDCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(viewIDCommand, "@docGuid", DbType.Guid, docGuid);
                    reader = db.ExecuteReader(viewIDCommand);
                    if (reader.Read())
                    {
                        nciViewID = (Guid)reader["NCIViewID"];
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Reading document NCIViewID Failed.", e);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
            return nciViewID;
        }

        /// <summary>
        /// This method is called for string comparison purpose to replace the <tag /> with <tag></tag> format
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        protected void ReplaceEndTag (ref string htmlText)
        {
             int endIndex = htmlText.IndexOf(" />");
             while (endIndex > 0)
             {
                  string firstPart = htmlText.Substring(0, endIndex);
                  string lastPart = htmlText.Substring(endIndex + 2);
                  if (lastPart.StartsWith(">"))
                      lastPart = lastPart.Substring(1);
                  int startTagIndex = firstPart.LastIndexOf("<");
                  string inTag = firstPart.Substring(startTagIndex + 1);
                  int tagIndex = inTag.IndexOf(" ");
                  if (inTag.Trim().Length > 0 && tagIndex == 0)
                      tagIndex = inTag.Length;
                  if (tagIndex > 0)
                  {
                      string tag = inTag.Substring(0, tagIndex).Trim();
                      htmlText = firstPart.Trim() + "></" + tag + ">" + lastPart;
                  }
                  endIndex = htmlText.IndexOf("/>");
             }
        }

        // <summary>
        /// This method is called for string comparison purpose to replace the <tag /> with <tag></tag> format
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        protected void ReplaceEndTag(ref string htmlText, string replaceTag)
        {
            int endIndex = htmlText.IndexOf(" />");
            string text = htmlText;
            while (endIndex > 0)
            {
                string firstPart = htmlText.Substring(0, endIndex);
                string lastPart = htmlText.Substring(endIndex + 3);
                int startTagIndex = firstPart.LastIndexOf("<");
                string inTag = firstPart.Substring(startTagIndex + 1);
                int tagIndex = inTag.IndexOf(" ");
                if (inTag.Trim().Length > 0 && tagIndex == 0)
                    tagIndex = inTag.Length;
                if (tagIndex > 0)
                {
                    string tag = inTag.Substring(0, tagIndex).Trim();
                    if (tag.Trim() == replaceTag)
                    {
                        htmlText = firstPart.Trim() + "></" + tag + ">" + lastPart;
                    }
                }

                endIndex = htmlText.IndexOf(" />", endIndex+3);
            }
        }

        // <summary>
        /// Build pretty for SummaryLink
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        internal void BuildSummaryRefLink(ref string html, int isGlossary)
        {
            string startTag = "<SummaryRef";
            string endTag = "</SummaryRef>";
            int startIndex = html.IndexOf(startTag, 0);
            string sectionHTML = html;
            while (startIndex >= 0)
            {
                // Devide the whole piece of string into three parts: a= first part; b = "<summaryref href="CDR0012342" url="/cander_topic/...HP/>..</summaryref>"; c = last part
                int endIndex = sectionHTML.IndexOf(endTag) + endTag.Length;
                string partA = sectionHTML.Substring(0, startIndex);
                string partB = sectionHTML.Substring(startIndex, endIndex - startIndex);
                string partC = sectionHTML.Substring(endIndex);

                // Process partB
                // Get the href, url, text between the tag
                XmlDocument refDoc = new XmlDocument();
                refDoc.LoadXml(partB);
                XPathNavigator xNav = refDoc.CreateNavigator();
                XPathNavigator link = xNav.SelectSingleNode("/SummaryRef");
                string text = link.InnerXml;
                string href = link.GetAttribute("href", string.Empty);
                string url = link.GetAttribute("url", string.Empty).Trim();
                if (url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1);
                }

                // The following code is preserved just in case in the future we need to support
                // prettyURL links in CDRPreview web service.
                // Get prettyurl server if the PrettyURLController is not reside on the same server
                // This is used for CDRPreview web service, GateKeeper should not have this setting.
                // string prettyURLServer = ConfigurationManager.AppSettings["PrettyUrlServer"];
                //if (prettyURLServer != null && prettyURLServer.Trim().Length > 0)
                //    url = prettyURLServer + url;

                // Get the section ID in href
                int index = href.IndexOf("#");
                string sectionID = string.Empty;
                string prettyURL = url;
                if (index > 0)
                {
                    sectionID = href.Substring(index + 2);
                    prettyURL = url + "/" + sectionID + ".cdr#Section_" + sectionID;
                }

                //Create new link string
                if (prettyURL.Trim().Length > 0)
                {
                    // The click on the summary link in the GlossaryTerm will open a new browser for summary document
                    if (isGlossary == 1)
                        partB = "<a class=\"SummaryRef\" href=\"" + prettyURL + "\" target=\"new\">" + text + "</a>";
                    else 
                        partB = "<a class=\"SummaryRef\" href=\"" + prettyURL + "\">" + text + "</a>";
                }
                else
                {
                    throw new Exception("Retrieving SummaryRef url failed. SummaryRef=" + partB + ".");
                }

                // Combine
                // Do not add extra space before the SummaryRef if following sign is lead before the link: ({[ or open ' "
                if (Regex.IsMatch(partA.Trim(), "[({[/]$|[({[\\s]\'$|[({[\\s]\"$"))
                    sectionHTML = partA.Trim() + partB;
                else
                    sectionHTML = partA.Trim() + " " + partB;

                // Do not add extra space after the SummaryRef if following sign
                // is after the SummaryRef )}].,:;? " with )}].,:;? or space after it, ' with )]}.,:;? or space after it.
                if (Regex.IsMatch(partC.Trim(), "^[).,:;!?}]|^]|^\"[).,:;!?}\\s]|^\'[).,:;!?}\\s]|^\"]|^\']"))
                    sectionHTML += partC.Trim();
                else
                    sectionHTML += " " + partC.Trim();

                startIndex = sectionHTML.IndexOf(startTag, 0);
            }
            html = sectionHTML;
        }

        // <summary>
        /// Taking care of the spaces around GlossaryTermRefLink
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        public void BuildGlossaryTermRefLink(ref string html, string tag)
        {
            string startTag = "<a Class=\"" + tag + "\"";
            string endTag = "</a>";
            int startIndex = html.IndexOf(startTag, 0);
            string sectionHTML = html;
            string collectHTML = string.Empty;
            string partC = string.Empty;
            while (startIndex >= 0)
            {
                 string partA = sectionHTML.Substring(0, startIndex);
                string left = sectionHTML.Substring(startIndex);
                int endIndex = left.IndexOf(endTag) + endTag.Length;
                string partB = left.Substring(0, endIndex);
                partC = left.Substring(endIndex);

                // Combine
                // Do not add extra space after the GlossaryTermRef if following sign
                // is after the SummaryRef )}].,:;? " with )}].,:;? or space after it, ' with )]}.,:;? or space after it.
                if (Regex.IsMatch(partA.Trim(), "^[).,:;!?}]|^]|^\"[).,:;!?}\\s]|^\'[).,:;!?}\\s]|^\"]|^\']") || collectHTML.Length == 0)
                    collectHTML += partA.Trim();  
                else
                    collectHTML += " " + partA.Trim();

                // Do not add extra space before the GlossaryTermRef if following sign is lead before the link: ({[ or open ' "
                if (Regex.IsMatch(collectHTML, "[({[/]$|[({[\\s]\'$|[({[\\s]\"$"))
                    collectHTML += partB;
                else
                    collectHTML += " " + partB;

                sectionHTML = partC.Trim();
                startIndex = sectionHTML.IndexOf(startTag, 0);
            }
            html = collectHTML + partC;
        }



        /// <summary>
        /// Call store procedure to check if the document is active
        /// </summary>
        /// <param name="documentID"></param>
        /// <param name="database"></param>
        /// <returns>bool</returns>
        protected bool IsDocumentActive(int documentID, Database db)
        {
            bool isActive = true;
            IDataReader reader = null;
            try
            {
                String spDocumentStatus = SPCommon.SP_GET_DOCUMENT_STATUS;
                using (DbCommand spStatusCommand = db.GetStoredProcCommand(spDocumentStatus))
                {
                    spStatusCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(spStatusCommand, "@DocumentID", DbType.Int32, documentID);
                    reader = db.ExecuteReader(spStatusCommand);
                    string errorMsg = string.Empty;
                    if (reader.Read())
                    {
                        if (!Boolean.Parse(reader["isActive"].ToString()))
                            isActive = false;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Validating document status failed. CDRID = " + documentID.ToString() + ".", e);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
            return isActive;
        }

        #endregion

    }
}
