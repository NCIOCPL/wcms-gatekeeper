using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Transactions;
using System.Xml;
using System.Xml.XPath;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.StoreProcedures;
using GateKeeper.DataAccess;
using GateKeeper.Logging;
using GateKeeper.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace GateKeeper.DataAccess.CancerGov
{
    /// <summary>
    /// Class to persist, retrieve and promote summary documents.
    /// </summary>
    public class SummaryQuery : DocumentQuery
    {
        #region Constants
        public const string SummarySectionTableName = "SummarySection";
        public const string SummaryTableName = "Summary";
    #endregion Constants

        #region Constructors
        /// <summary>
        /// Class constructor.
        /// </summary>
        public SummaryQuery()
        {}

        #endregion Constructors

         
      
 
        #region Public Methods

        /// <summary>
        /// Save Summary document into CDR staging database
        /// </summary>
        /// <param name="glossaryDoc">
        /// Summary document object
        /// </param>
        public override bool SaveDocument(Document summaryDocument, String userID)
        {
            bool bSuccess = true;
            Database db = this.StagingDBWrapper.SetDatabase();
            DbConnection conn = this.StagingDBWrapper.EnsureConnection();
            DbTransaction transaction = conn.BeginTransaction();

            try
            {
                SummaryDocument summaryDoc = (SummaryDocument)summaryDocument;

                // SP: Clear extracted summary data
                ClearSummaryData(summaryDoc.DocumentID, db, transaction);
                
                // SP: Save summary
                SaveSummary(summaryDoc, db, transaction, userID);
               
                // SP: Save summary sections
                SaveSummarySections(summaryDoc, db, transaction);

                // SP: Loop to save summary relations
                SaveSummaryRelations(summaryDoc, db, transaction, userID);

                // SP: Save document data
                SaveDBDocument(summaryDoc, db, transaction);
                transaction.Commit();
            }
            catch (Exception e)
            {
                bSuccess = false;
                transaction.Rollback();
                throw new Exception("Database Error: Saving summary document failed. Document CDRID=" + summaryDocument.DocumentID.ToString(), e);
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
        /// Deletes the summary document.
        /// </summary>
        /// <param name="documentID"></param>
        public override void DeleteDocument(Document summary, ContentDatabase databaseName, string userID)
        {
            try
            {
                int documentID = summary.DocumentID;
                // Check if the document ID is referenced else where
                if (IsOKToDelete(documentID, DocumentType.Summary))
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
                            Guid guid = Guid.Empty;
                            // If guid is empty, don't delete, create a warning 
                            GetDocumentIDs(ref documentID, ref guid, db);
                            //if (guid != Guid.Empty && IsDocumentActive(documentID, db))
                            //    DeleteSummaryPreview(guid);
                            //else
                            //{
                            //    // Give out warning message
                            //    summary.WarningWriter("Database warning: Summary document could not be deleted in the preview database.  The document either does not exist or is not active. cdrid = " + summary.DocumentID.ToString() + ".");
                            //    conn.Close();
                            //    conn.Dispose();
                            //    return;
                            //}
                            break;
                        case ContentDatabase.Live:
                            db = this.LiveDBWrapper.SetDatabase();
                            conn = this.LiveDBWrapper.EnsureConnection();
                            Guid liveGuid = Guid.Empty;
                            // If guid is empty, don't delete, create a warning 
                            GetDocumentIDs(ref documentID, ref liveGuid, db);
                            //if (liveGuid != Guid.Empty && IsDocumentActive(documentID, db))
                            //    PushToCancerGovLive(summary, true, userID);
                            //else
                            //{
                            //    // Give out warning message
                            //    summary.WarningWriter("Database warning: Summary document could not be deleted in the live database.   The document either does not exist or is not active. cdrid = " + summary.DocumentID.ToString() + ".");
                            //    conn.Close();
                            //    conn.Dispose();
                            //    return;
                            //}
                            break;
                        default:
                            throw new Exception("Database Error: Invalid database name. DatabaseName = " + databaseName.ToString());
                    }
                    DbTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // SP: Clear extracted data
                        ClearSummaryData(documentID, db, transaction);

                        // SP: Clear document
                        ClearDocument(documentID, db, transaction, databaseName.ToString());
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw new Exception("Database Error: Deleting summary document in " + databaseName.ToString() + " failed. Document CDRID=" + documentID.ToString(), e);
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
                throw new Exception("Database Error: Deleting summary document in " + databaseName.ToString() + " failed. Document CDRID=" + summary.DocumentID.ToString(), e);
            }
        }

         /// <summary>
        /// Pushes the document from staging to preview environment for QC.
        /// </summary>
        /// <param name="documentID"></param>
        public override void PushDocumentToPreview(Document summary, string userID)
        {
           // Call check of it is OK to Push
            SummaryDocument summaryDoc = (SummaryDocument)summary;
            if (GetSummaryInfo(summaryDoc, this.StagingDBWrapper.SetDatabase()))
            {
                if (IsOKToPush(summaryDoc.DocumentID, summaryDoc.PrettyURL, 1, summaryDoc.GUID, summaryDoc.ReplacementforDocumentGUID, ContentDatabase.Preview, DocumentType.Summary))
                {
                    // Push document to CDR Staging database
                    PushToCDRPreview(summaryDoc.DocumentID, userID);
                    
                    // Push document to CancerGovStaging database
                    //PushToCancerGovPreview(summaryDoc.DocumentID, summaryDoc, userID);
                }
                else
                {
                    throw new Exception("Database Error: Could not push document to preview database due to pretty URL duplication. Document CDRID=" + summary.DocumentID.ToString());
                }
            }
            else
            {
                throw new Exception("Database Error: Retrieveing summary data from CDR staging database failed. Document CDRID=" + summary.DocumentID.ToString());
            }
        }

        /// <summary>
        /// Pushes document from preview to live production environment.
        /// </summary>
        /// <param name="documentID"></param>
        public override void PushDocumentToLive(Document summary, string userID)
        {
             SummaryDocument summaryDoc = (SummaryDocument)summary;
             if (GetSummaryInfo(summaryDoc, this.PreviewDBWrapper.SetDatabase()))
             {
                 if (IsOKToPush(summaryDoc.DocumentID, summaryDoc.PrettyURL, 1, summary.GUID, summaryDoc.ReplacementforDocumentGUID, ContentDatabase.Live, DocumentType.Summary))
                 {
                     // Push document to CDR Staging database
                     PushToCDRLive(summaryDoc.DocumentID, userID);
                 }
                 else
                 {
                     throw new Exception("Database Error: Could not push document to live database due to pretty URL duplication. Document CDRID=" + summary.DocumentID.ToString());
                 }
             }
             else
             {
                 throw new Exception("Database Error: Retrieveing summary data from CDR staging database failed. Document CDRID=" + summary.DocumentID.ToString());
             }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Call store procedure to clear summary data
        /// </summary>
        /// <param name="summaryDoc"></param>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        private void ClearSummaryData(int documentID, Database db, DbTransaction transaction)
        {
            try{
                string spClearExtractedData = SPSummary.SP_CLEAR_SUMMARY_DATA;
                using (DbCommand clearCommand = db.GetStoredProcCommand(spClearExtractedData))
                {
                    clearCommand.CommandType = CommandType.StoredProcedure;
                    clearCommand.CommandTimeout = ApplicationSettings.CommandTimeout;
                    db.AddInParameter(clearCommand, "@DocumentID", DbType.Int32, documentID);
                    db.ExecuteNonQuery(clearCommand, transaction);
                }
             }
             catch (Exception e)
             {
                 throw new Exception("Database Error: Clearing summary data failed. Document CDRID=" + documentID.ToString(), e);
             }
        }

        /// <summary>
        /// Call store procedure to save summary data
        /// </summary>
        /// <param name="summaryDoc"></param>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        private void SaveSummary(SummaryDocument summaryDoc, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                string spSaveSummary = SPSummary.SP_SAVE_SUMMARY;
                using (DbCommand summaryCommand = db.GetStoredProcCommand(spSaveSummary))
                {
                    summaryCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(summaryCommand, "@SummaryID", DbType.Int32, summaryDoc.DocumentID);
                    db.AddInParameter(summaryCommand, "@Type", DbType.String, summaryDoc.Type.Trim());
                    db.AddInParameter(summaryCommand, "@Audience", DbType.String, summaryDoc.AudienceType.Trim());
                    db.AddInParameter(summaryCommand, "@Language", DbType.String, summaryDoc.Language);
                    db.AddInParameter(summaryCommand, "@Title", DbType.String, summaryDoc.Title.Trim());
                    // The maximum length of the short title should be 64 chars.
                    if (summaryDoc.ShortTitle.Trim().Length > 64)
                        db.AddInParameter(summaryCommand, "@ShortTitle", DbType.String, summaryDoc.ShortTitle.Trim().Substring(0, 64));
                    else
                        db.AddInParameter(summaryCommand, "@ShortTitle", DbType.String, summaryDoc.ShortTitle.Trim());
                    db.AddInParameter(summaryCommand, "@UpdateUserID", DbType.String, userID);
                    db.AddInParameter(summaryCommand, "@Description", DbType.String, summaryDoc.Description.Trim());
                    db.AddInParameter(summaryCommand, "@PrettyURL", DbType.String, summaryDoc.BasePrettyURL.Trim());
                    if (summaryDoc.ReplacementForID > 0)
                    {
                        // Validate the replacement document.  Throw if the replacement document is not valid.
                        // Three types of error message 1) replacement does not exist; replacement is inactive; 3) replacement doc is not summary
                        // Comment the validation out since the data is not consistant enough for this type of checking.
                        //ValidReplacementSummary(summaryDoc.ReplacementForID, db);
                        db.AddInParameter(summaryCommand, "@replacementDocumentID", DbType.Int32, summaryDoc.ReplacementForID);
                    }
                    else
                        db.AddInParameter(summaryCommand, "@replacementDocumentID", DbType.Int32, null);
                    // If either Date is DateTime.MinValue, then it did not exist in the XML and should be set NULL.
                    if(summaryDoc.FirstPublishedDate == DateTime.MinValue)
                        db.AddInParameter(summaryCommand, "@DateFirstPublished", DbType.DateTime, null);
                    else
                        db.AddInParameter(summaryCommand, "@DateFirstPublished", DbType.DateTime, summaryDoc.FirstPublishedDate);
                    if (summaryDoc.LastModifiedDate == DateTime.MinValue)
                        db.AddInParameter(summaryCommand, "@DateLastModified", DbType.DateTime, null);
                    else
                        db.AddInParameter(summaryCommand, "@DateLastModified", DbType.DateTime, summaryDoc.LastModifiedDate);
                    db.ExecuteNonQuery(summaryCommand, transaction);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving summary data failed. Document CDRID=" + summaryDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Loop through each level of summary sections to save sections
        /// </summary>
        /// <param name="summaryDoc"></param>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        private void SaveSummarySections(SummaryDocument summaryDoc, Database db, DbTransaction transaction)
        {
            try
            {
                //Get the correct GUID Id from the Document table 
                //If this is a new document and the GUID it's not found, keep Document GUID which is new 
                int docID = summaryDoc.DocumentID; 
                Guid docGUID = Guid.Empty;
                GetDocumentIDs(ref docID, ref docGUID, db);
                if (docGUID != Guid.Empty)
                    summaryDoc.GUID = docGUID;


                // SP: Loop to save level1-3 section list
                foreach (SummarySection sect in summaryDoc.SectionList)
                {
                    // Replace the SummaryRef with pretty URL in the SummarySection's html field
                    // Enable this block when string comparison is done.
                       string html = sect.Html.OuterXml;
                       if (html.Contains("<SummaryRef"))
                       {
                           BuildSummaryRefLink(ref html,0);
                           // Reload the updated xml back to the section HTML field
                           sect.Html.LoadXml(html);
                       }
                       string glossaryTermTag = "Summary-GlossaryTermRef";
                       if (html.Contains(glossaryTermTag))
                       {
                           BuildGlossaryTermRefLink(ref html, glossaryTermTag);
                           // Reload the updated xml back to the section HTML field
                           sect.Html.LoadXml(html);
                       }
                       SaveSections(db, transaction, summaryDoc, sect);
                }

                // SP: Loop to save level4 section list
                foreach (SummarySection sect4 in summaryDoc.Level4SectionList)
                    SaveSections(db, transaction, summaryDoc, sect4);

                // SP: Loop to save level5 section list
                foreach (SummarySection sect5 in summaryDoc.Level5SectionList)
                    SaveSections(db, transaction, summaryDoc, sect5);

                // SP: Loop to save table section list
                foreach (SummarySection table in summaryDoc.TableSectionList)
                    SaveSections(db, transaction, summaryDoc, table);

            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving summary sections failed. Document CDRID=" + summaryDoc.DocumentID.ToString(), e);
            }
        }        

        /// <summary>
        /// Call store procedure to save summary relations
        /// </summary>
        /// <param name="summaryDoc"></param>
        /// <param name="db"></param>
        /// <param name="transaction"></param>
        private void SaveSummaryRelations(SummaryDocument summaryDoc, Database db, DbTransaction transaction, string userID)
        {
            try
            {
                // Loop to save summary relations
                foreach (SummaryRelation relation in summaryDoc.RelationList)
                {
                    string spSaveRelations = SPSummary.SP_SAVE_SUMMARY_RELATIONS;
                    using (DbCommand relationCommand = db.GetStoredProcCommand(spSaveRelations))
                    {
                        relationCommand.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(relationCommand, "@SummaryID", DbType.Int32, summaryDoc.DocumentID);
                        db.AddInParameter(relationCommand, "@RelatedSummaryID", DbType.Int32, relation.RelatedSummaryID);
                        db.AddInParameter(relationCommand, "@RelationType", DbType.String, relation.RelationType.ToString().Trim());
                        db.AddInParameter(relationCommand, "@UpdateUserID", DbType.String, userID);
                        db.ExecuteNonQuery(relationCommand, transaction);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Saving summary relations failed. Document CDRID=" + summaryDoc.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Call store procedure to save summary sections
        /// </summary>
        /// <param name="summaryID"></param>
        /// <param name="sectionList"></param>
        private void SaveSections(Database db, DbTransaction transaction, SummaryDocument summaryDoc, SummarySection sect)
        {
            string spSaveSection = SPSummary.SP_SAVE_SUMMARY_SECTION;
            using (DbCommand sectionCommand = db.GetStoredProcCommand(spSaveSection))
            {
                sectionCommand.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(sectionCommand, "@SummarySectionID", DbType.String, sect.SummarySectionID.ToString().Trim());
                db.AddInParameter(sectionCommand, "@SummaryGUID", DbType.String, summaryDoc.GUID.ToString().Trim());
                db.AddInParameter(sectionCommand, "@SummaryID", DbType.Int32, summaryDoc.DocumentID);
                db.AddInParameter(sectionCommand, "@SectionID", DbType.String, sect.SectionID.Trim());
                db.AddInParameter(sectionCommand, "@SectionType", DbType.String, sect.SectionType);
                db.AddInParameter(sectionCommand, "@SectionTitle", DbType.String, sect.Title.Trim());
                db.AddInParameter(sectionCommand, "@Priority", DbType.Int32, sect.Priority);
                db.AddInParameter(sectionCommand, "@SectionLevel", DbType.Int32, sect.Level);
                if (sect.Level == 1 || sect.ParentSummarySectionID == Guid.Empty)
                    db.AddInParameter(sectionCommand, "@ParentSummarySectionID", DbType.String, null);
                else
                    db.AddInParameter(sectionCommand, "@ParentSummarySectionID", DbType.String, sect.ParentSummarySectionID.ToString().Trim());

                if ((sect.TOC.Trim().Length == 0) && (sect.Level != 1))
                    db.AddInParameter(sectionCommand, "@TOC", DbType.String, null);
                // This is for one test cases that the section is not table but have toc. 
                // According to the string comparison, we should not save it.
                else if (sect.TOC.Trim().Length > 0 && !sect.Html.FirstChild.InnerXml.Trim().StartsWith("<table"))
                    db.AddInParameter(sectionCommand, "@TOC", DbType.String, sect.TOC.Trim());
                else
                    db.AddInParameter(sectionCommand, "@TOC", DbType.String, string.Empty);

                string html = sect.Html.OuterXml;
                if (html.Trim().Length > 0 && sect.Html.FirstChild.HasChildNodes && (sect.Level == 1 || sect.Level == 2))
                {
                    FormatSection(ref html, sect, summaryDoc);
                    html = html.Trim();
                }
                else
                    html = null;
                db.AddInParameter(sectionCommand, "@HTML", DbType.String, html);
                db.ExecuteNonQuery(sectionCommand, transaction);
            }
        }

        /// <summary>
        /// Reformat the html data to match the original format in database
        /// </summary>
        /// <param name="html"></param>
        /// <param name="sect"></param>
        /// <param name="summary"></param>
        public void FormatSection(ref string html, SummarySection sect, SummaryDocument summary )
        {
            string topStart = "<a name=\"Section_" + sect.SectionID + "\">";
            if (html.StartsWith(topStart))
            {
                string topEnd = "</a>";
                html = html.Insert(topStart.Length, topEnd);
                int wholeLength = html.Length - topEnd.Length;
                html = html.Substring(0, wholeLength);
            }

            if (sect.Level == 1)
            {
                // Take out the top level xml
                //html = html.Replace("<TableSectionXML>", string.Empty);
                //html = html.Replace("</TableSectionXML>", string.Empty);
                html = html.Replace("<a name=\"TableSection\"/>", "<a name=\"TableSection\"></a>");
                // To get rid of the extra xml tag around the table section
                foreach (SummarySection tableSection in summary.TableSectionList)
                {
                    if (tableSection.ParentSummarySectionID == sect.SummarySectionID)
                    {
                        string searchString1 = "<a name=\"SectionXML_" + tableSection.SectionID + "\">";
                        string searchString2 = "<a name=\"END_TableSection\" />";
                        int position1 = html.IndexOf(searchString1);
                        if (position1 > 0)
                        {
                            string part1 = html.Substring(0, position1 - 1);
                            string part2 = html.Substring(position1 + searchString1.Length);
                            int position2 = part2.IndexOf(searchString2);
                            if (position2 > 0)
                            {
                                string part3 = part2.Substring(0, position2 + searchString2.Length);
                                string part4 = part2.Substring(part3.Length).Trim();
                                part4 = part4.Substring("</a>".Length);
                                html = part1 + part3 + part4;
                            }
                        }
                    }
                }
                html = html.Replace("<a name=\"END_TableSection\" />", "<a name=\"END_TableSection\"></a>");
            }
            // Remove the 	<a name="END_TableSection"/> tag from html document
            else if (sect.Level == 2)
                html = html.Replace("<a name=\"END_TableSection\" />", string.Empty);

            // Remove the MediaHTML tag around the media link xml
            html = html.Replace("<MediaHTML>", string.Empty);
            html = html.Replace("</MediaHTML>", string.Empty);

            // Remove the table section tag around the tables
            html = html.Replace("<TableSectionXML>", string.Empty);
            html = html.Replace("</TableSectionXML>", string.Empty);


            // TODO:REMOVE - This is for string comparison purpose
            html = html.Replace("<div align=\"right\">", "<div align=right>");
            html = html.Replace("<COL Width=\"0*\" />", "<COL Width=\"0*\"></COL>");
            html = html.Replace("<COL Width=\"1*\" />", "<COL Width=\"1*\"></COL>");
            html = html.Replace("<COL Width=\"2*\" />", "<COL Width=\"2*\"></COL>");
            html = html.Replace("<COL Width=\"3*\" />", "<COL Width=\"3*\"></COL>");
            html = html.Replace("<COL Width=\"4*\" />", "<COL Width=\"4*\"></COL>");
            html = html.Replace("<br />", "<br/>");
            html = html.Replace("<Br />", "<Br/>");
            html = TrimBRSpace(html);
            html = html.Replace("<b />", "<b></b>");
            html = html.Replace("<i />", "<i></i>");
            html = html.Replace("<i/>", "<i></i>");
            html = html.Replace ("<table align=\"center\"", "<table align=center");
            html = html.Replace("&amp;amps;", "&");
            html = html.Replace("&amp;#", "&#");
            html = html.Replace(".jpg\" />", ".jpg\"/>");
            html = html.Replace("<THEAD />", "<THEAD></THEAD>");
            html = html.Replace("<img src=\"/images/spacer.gif\" width=\"12\" height=\"10\" border=\"0\" />", "<img src=\"/images/spacer.gif\" width=\"12\" height=\"10\" border=\"0\">");
            html = html.Replace("<img src=\"/images/spacer.gif\" width=\"12\" height=\"3\" border=\"0\" />", "<img src=\"/images/spacer.gif\" width=\"12\" height=\"3\" border=\"0\">");

            html = html.Replace("<td class=\"Summary-SummarySection-Small\" rowspan=\"0\" colspan=\"0\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary-SummarySection-Small\" rowspan=\"0\" colspan=\"0\" align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace("<td class=\"Summary-SummarySection-Small\" rowspan=\"2\" colspan=\"0\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary-SummarySection-Small\" rowspan=\"2\" colspan=\"0\" align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace(" <td class=\"Summary-SummarySection-Small\" rowspan=\"3\" colspan=\"0\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary-SummarySection-Small\" rowspan=\"3\" colspan=\"0\" align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace(" <td class=\"Summary-SummarySection-Small\" rowspan=\"4\" colspan=\"0\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary-SummarySection-Small\" rowspan=\"4\" colspan=\"0\" align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace("<td class=\"Summary-SummarySection-Small\" rowspan=\"0\" colspan=\"0\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary-SummarySection-Small\" rowspan=\"0\" colspan=\"0\" align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace("<td class=\"Summary-SummarySection-Small\" rowspan=\"0\" colspan=\"2\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary-SummarySection-Small\" rowspan=\"0\" colspan=\"2\" align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace(" <td class=\"Summary-SummarySection-Small\" rowspan=\"0\" colspan=\"3\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary-SummarySection-Small\" rowspan=\"0\" colspan=\"3\" align=\"Left\" Valign=\"Top\"></td>");
          
            html = html.Replace("<td colspan=\"0\" align=\"Center\" Valign=\"Middle\" class=\"Summary\" />", "<td colspan=\"0\" align=\"Center\" Valign=\"Middle\" class=\"Summary\"></td>");
            html = html.Replace("<td class=\"Summary\" rowspan=\"0\" colspan=\"0\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary\" rowspan=\"0\" colspan=\"0\" align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace("<td class=\"Summary\" rowspan=\"0\" colspan=\"3\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary\" rowspan=\"0\" colspan=\"3\"align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace("<td class=\"Summary\" rowspan=\"0\" colspan=\"2\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary\" rowspan=\"0\" colspan=\"2\"align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace("<td class=\"Summary\" rowspan=\"0\" colspan=\"7\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary\" rowspan=\"0\" colspan=\"7\"align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace("<td class=\"Summary\" rowspan=\"2\" colspan=\"0\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary\" rowspan=\"2\" colspan=\"0\" align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace("<td class=\"Summary\" rowspan=\"3\" colspan=\"0\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary\" rowspan=\"3\" colspan=\"0\" align=\"Left\" Valign=\"Top\"></td>");
            html = html.Replace("<td class=\"Summary\" rowspan=\"4\" colspan=\"0\" align=\"Left\" Valign=\"Top\" />", "<td class=\"Summary\" rowspan=\"4\" colspan=\"0\" align=\"Left\" Valign=\"Top\"></td>");

            html = html.Replace("<span class=\"QandA-Answer\" />", "<span class=\"QandA-Answer\"></span>");

            html = html.Replace("<sup />", "<sup></sup>");
            html = html.Replace("<A />", "<A></A>");
            html = html.Replace("<COL Width=\"2*\" />", "<COL Width=\"2*\"></COL>");
            html = html.Replace("<COL Width=\"\" />", "<COL Width=\"\"></COL>");
            if (summary.AudienceType == "Patients")
                html = html.Replace("border=\"0\" />", "border=\"0\"/>");
            ReplaceEndTag(ref html, "P");
            ReplaceEndTag(ref html, "a");
         }


        #region Promote Methods
        /// <summary>
        /// Call store procedure to push summary document to CDR preview database
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
                    // SP: Call push summary document
                    string spPushData = SPSummary.SP_Push_Extracted_Summary_Data;
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
                    throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedSummaryData failed at push to preview database", e);
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
                throw new Exception("Database Error: Pushing summary document to CDR preview database failed. Document CDRID=" + documentID.ToString(), e);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Call store procedure to push summary document to CancerGovStaging database
        /// </summary>
        /// <param name="documentID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private void PushToCancerGovPreview(int documentID, SummaryDocument summary,  string userID)
        {
            // Get document information
            // Note: Don't need transaction for alll CancerGov related store procedure
            Database db = this.CancerGovStagingDBWrapper.SetDatabase();
            try
            {
                // SP: Call store procedure to push summary document to CancerGovStaging database
                string spUpdateViewData = SPSummary.SP_Update_NCI_View_And_ViewObject;
                using (DbCommand updateCommand = db.GetStoredProcCommand(spUpdateViewData))
                {
                    updateCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(updateCommand, "@DocumentGUID", DbType.Guid, summary.GUID);
                    db.AddInParameter(updateCommand, "@replacementforDocumentGUID", DbType.Guid, summary.ReplacementforDocumentGUID);
                    db.AddInParameter(updateCommand, "@RelatedDocumentGUID", DbType.Guid, summary.RelatedDocumentGUID);
                    db.AddInParameter(updateCommand, "@OtherLanguageDocumentGUID", DbType.Guid, summary.OtherLanguageDocumentGUID);
                    db.AddInParameter(updateCommand, "@Title", DbType.String, summary.Title.Trim());
                    // The maximum length of the short title should be 64 chars.
                    if (summary.ShortTitle.Trim().Length > 64)
                        db.AddInParameter(updateCommand, "@ShortTitle", DbType.String, summary.ShortTitle.Trim().Substring(0, 64));
                    else
                        db.AddInParameter(updateCommand, "@ShortTitle", DbType.String, summary.ShortTitle.Trim());

                    db.AddInParameter(updateCommand, "@Description", DbType.String, summary.Description.Trim());
                    db.AddInParameter(updateCommand, "@Audience", DbType.String, summary.AudienceType.Trim());
                    if (summary.Language == Language.English)
                        db.AddInParameter(updateCommand, "@Language", DbType.String, Language.English.ToString().Trim());
                    else if (summary.Language == Language.Spanish)
                        db.AddInParameter(updateCommand, "@Language", DbType.String, Language.Spanish.ToString().Trim());
                    db.AddInParameter(updateCommand, "@ExpirationDate", DbType.DateTime, DateTime.Parse("1/1/2100"));
                    db.AddInParameter(updateCommand, "@ReleaseDate", DbType.DateTime, DateTime.Now);
                    db.AddInParameter(updateCommand, "@PostedDate", DbType.DateTime, DateTime.Now);
                    db.AddInParameter(updateCommand, "@DisplayDateMode", DbType.String, "None");
                    db.AddInParameter(updateCommand, "@BasePrettyURL", DbType.String, summary.BasePrettyURL.Trim());
                    db.AddInParameter(updateCommand, "@UpdateUserID", DbType.String, userID);
                    db.ExecuteNonQuery(updateCommand);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Pushing summary document to CancerGovStaging database failed. Document CDRID=" + documentID.ToString(), e);
            }
         }

        /// <summary>
        /// Call store procedure to get summary document info from CDRStaging database
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        private bool GetSummaryInfo(SummaryDocument summary, Database db)
        {
            // Get document information
            // Note: Don't need transaction for alll CancerGov related store procedure
            bool bSuccess = true;
            IDataReader reader = null;
            try
            {
                // SP: Call to get summary document info to push data to CancerGovStaging database
                string spSummaryData = SPSummary.SP_Get_Summary_Info;
                using (DbCommand summaryCommand = db.GetStoredProcCommand(spSummaryData))
                {
                    summaryCommand.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(summaryCommand, "@SummaryID", DbType.Int32, summary.DocumentID);
                    reader = db.ExecuteReader(summaryCommand);

                    if (reader.Read())
                    {
                        summary.GUID = (Guid)reader["DocumentGUID"];
                        summary.IsActive = (bool)reader["IsActive"];
                        if (reader["DateLastModified"].ToString().Trim().Length > 0)
                            summary.LastModifiedDate = DateTime.Parse(reader["DateLastModified"].ToString());
                        summary.Type = reader["Type"].ToString();
                        summary.AudienceType = reader["Audience"].ToString();
                        string language = reader["Language"].ToString();
                        if (language.ToUpper() == Language.English.ToString().ToUpper())
                            summary.Language = Language.English;
                        else if (language.ToUpper() == Language.Spanish.ToString().ToUpper())
                            summary.Language = Language.Spanish;

                        // Get Summary title and short title
                        summary.Title = reader["Title"].ToString();
                        summary.ShortTitle = reader["ShortTitle"].ToString();
                        
                        if (reader["RelatedDocumentGUID"] != DBNull.Value)
                            summary.RelatedDocumentGUID = (Guid)reader["RelatedDocumentGUID"];
                        if (reader["OtherLanguageDocumentGUID"] != DBNull.Value)
                            summary.OtherLanguageDocumentGUID = (Guid)reader["OtherLanguageDocumentGUID"];
                        if (reader["replacementforDocumentGUID"] != DBNull.Value)
                            summary.ReplacementforDocumentGUID = (Guid)reader["replacementforDocumentGUID"];
                        summary.Description = reader["Description"].ToString();
                        string prettyURL = reader["PrettyURL"].ToString();
                        DocumentHelper.GetBasePrettyUrl(ref prettyURL);
                        summary.PrettyURL = prettyURL;
                        prettyURL = prettyURL.Substring(0, prettyURL.LastIndexOf("/"));
                        summary.BasePrettyURL = prettyURL;
                    }
                    else
                        bSuccess = false;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Retrieving summary document data failed. Document CDRID=" + summary.DocumentID.ToString(), e);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
            return bSuccess;
        }

        /// <summary>
        /// Call store procedure to push summary document to CDRLive database
        /// </summary>
        /// <param name="documentID"></param>
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
                    // SP: Call push summary document
                    string spPushData = SPSummary.SP_Push_Extracted_Summary_Data;
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
                  throw new Exception("Database Error: Store procedure dbo.usp_PushExtractedSummaryData failed at push to live database", e);
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
                throw new Exception("Database Error: Pushing summary document to CDR live database failed. Document CDRID=" + documentID.ToString(), e);
          }
          finally
          {
              conn.Close();
              conn.Dispose();
          }
      }

         /// <summary>
        /// Trim extra space between <br/> tag
        /// </summary>
        /// <param name="html"></param>
        /// <returns>string</returns>
        private string TrimBRSpace(string html)
        {
            string tag = " <br/>";
            int startIndex = html.IndexOf(tag);
            string firstPart;
            string secondPart;
            while (startIndex > 0)
            {
                firstPart = html.Substring(0, startIndex);
                secondPart = html.Substring(startIndex);
                html = firstPart.Trim() + secondPart.Trim();
                startIndex = html.IndexOf(tag);
            }
            return html.Replace("<br/><br/><br/><br/>", "<br/><br/>");
        }
         


        /// <summary>
        /// Call store procedure to check if the replacement summary is valid
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        private void ValidReplacementSummary(int documentID,  Database db)
        {
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
                        if (reader["DocumentGuid"] == DBNull.Value)
                            errorMsg = "The replacement document does not exist.";
                        else if (!Boolean.Parse(reader["isActive"].ToString()))
                            errorMsg = "The replacement document is not an active document.";
                        else if (Int32.Parse(reader["DocumentTypeID"].ToString()) != (int)DocumentType.Summary)
                            errorMsg = "The replacement document is not a summary document.";
                    }
                    else
                    {
                        errorMsg = "The replacement document does not exist.";
                    }

                    if (errorMsg.Trim().Length > 0)
                        throw new Exception(errorMsg);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Database Error: Validating replacement summary document failed. ReplacementCDRID = " + documentID.ToString() + ".", e);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        }
   }
        #endregion

         #endregion  // End private methods  
}
