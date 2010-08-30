using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;

using GateKeeper.Common;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.DocumentObjects.Protocol;
using GateKeeper.DocumentObjects.GeneticsProfessional;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DocumentObjects.DrugInfoSummary;

namespace GateKeeper.DataAccess
{
    public static class GenerateCDRPreview
    {
        #region Public methods
        // <summary>
        /// Summary Preview
        /// </summary>
        /// <param name="html">summary html web page</param>
        /// <param name="summary">SummaryDocument object</param>
        /// <returns>html in parameter reference</returns>
        public static void SummaryPreview(ref string html, SummaryDocument summary )
        {
            string startTag = string.Empty;
            string endTag = string.Empty;
            PageFrame(ref startTag, ref endTag);
            html = startTag;

            string begin = string.Empty;
            string end = string.Empty;
            string titleImage = "title_cancertopics.jpg";
            int tableWidth = 580;
            GetPageTitleTags(ref begin, ref end, titleImage, tableWidth.ToString());
            // Add document title
            html += begin + summary.Title + end;

            // Add last modified date
            html += "<tr><td><table width=\"651\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\" align=\"right\">";
            html += "<tr><td><img src=\"/images/spacer.gif\" border=\"0\" height=\"6\" width=\"6\"/></td></tr>";
            html += " <tr><td align=\"right\" valign=\"bottom\"><span class=\"protocol-date-label\">Last Modified: </span> ";
            html += "<span class=\"protocol-dates\">" +GetDate(summary.LastModifiedDate, "MM/DD/YYYY") + "</span></td></tr></table></td></tr>";
            html += "<img src=\"/images/spacer.gif\" border=\"0\" height=\"6\" width=\"6\"/>";

            // Start the page content
            string contentBegin = string.Empty;
            string contentEnd = string.Empty;

            GetContentTags(ref contentBegin, ref contentEnd);
            html += contentBegin;
            BuildSummaryTableOfContents(ref html, summary.SectionList, summary.Language);
            foreach (SummarySection sect in summary.SectionList)
            {
                // Replace the SummaryRef with pretty URL in the SummarySection's html field
                if (sect.IsTopLevel)
                {
                    html += "<p><span class=\"page-title\">" + "<a name=\"Section_" + sect.SectionID +"\"></a>" + sect.Title + "</span></p>";
                    string sectHtml = sect.Html.OuterXml;
                    using (SummaryQuery sumQuery = new SummaryQuery())
                    {
                        if (sectHtml.Contains("<SummaryRef"))
                        {
                            sumQuery.BuildSummaryRefLink(ref sectHtml, 0);
                        }

                        sumQuery.FormatSection(ref sectHtml, sect, summary);
                    }
                    // Replace media link 
                    string imagePath = ConfigurationManager.AppSettings["ImageLocation"];
                    sectHtml = sectHtml.Replace("[__imagelocation]", imagePath);
                    // This is to fix the summary table link format problem on Cancer.gov.
                    // Problem: the links in the summary table appear to be larger font than the text. however, hoover the link the font become small.
                    // The problem is in the cancer.gov style sheet.  This is a temporary fix on CDRPreview side).
                    sectHtml = sectHtml.Replace("Class=\"SummarySection-Table-Small\"", "style=\"font-size:7pt\"");

                    string glossaryTermTag = "Summary-GlossaryTermRef";
                    if (sectHtml.Contains(glossaryTermTag))
                    {
                        ProtocolQuery pQuery = new ProtocolQuery();
                        pQuery.BuildGlossaryTermRefLink(ref sectHtml, glossaryTermTag);
                    }

                    html += sectHtml;
                    html += "<p><a href=\"#top\" class=\"backtotop-link\">" +
                            "<img src=\"/images/backtotop_red.gif\" alt=\"Back to Top\" border=\"0\"/>Back to Top</a><p/>";
                }
            }

            html += contentEnd;
            html += endTag;
        }


        // <summary>
        /// Protocol Health Professional / Patient Preview
        /// </summary>
        /// <param name="html">Protocol html web page</param>
        /// <param name="protocol">ProtocolDocument object</param>
        /// <returns>html in parameter reference</returns>
        public static void ProtocolPreview(ref string html, ProtocolDocument protocol, PreviewTypes docType )
        {
            string startTag = string.Empty;
            string endTag = string.Empty;
            PageFrame(ref startTag, ref endTag);
            html = startTag;

            string begin = string.Empty;
            string end = string.Empty;
            string titleImage = "title_cancertopics.jpg";
            int tableWidth = 580;
            GetPageTitleTags(ref begin, ref end, titleImage, tableWidth.ToString());
            // Add document title
            html += begin + "Clinical Trials (PDQ<sup class=\"header\">&#174;</sup>)" + end;

            // Add last modified date and first published date
            bool lineBreak = false;
            html += "<tr><td><table width=\"651\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\" align=\"right\">";
            html += "<tr><td><img src=\"/images/spacer.gif\" border=\"0\" height=\"4\" width=\"8\"/><tr><td>";
            html += "<tr><td align=\"right\" valign=\"bottom\">";
            if (protocol.LastModifiedDate != DateTime.MinValue && protocol.LastModifiedDate != null)
            {
                html += "<span class=\"protocol-date-label\">Last Modified: </span>";
                html +="<span class=\"protocol-dates\">" + GetDate(protocol.LastModifiedDate, string.Empty) + "</span>";
                html += "<img src=\"/images/spacer.gif\" border=\"0\" height=\"1\" width=\"8\"/>";
                lineBreak = true;
            }
            if (protocol.FirstPublishedDate != DateTime.MinValue)
            {
                if (protocol.LastModifiedDate != DateTime.MinValue)
                    html += "&nbsp;&nbsp;";

                html += "<span class=\"protocol-date-label\">First Published: </span> ";
                html += "<span class=\"protocol-dates\">" + GetDate(protocol.FirstPublishedDate, string.Empty) + "</span>";
                html += "<img src=\"/images/spacer.gif\" border=\"0\" height=\"1\" width=\"8\"/>";
                lineBreak = true;
            }
            html += "</td></tr></table></td></tr>";
            if (lineBreak)
                html += "<tr><td height=\"10\"/></tr>";

            // Start the page content
            string contentBegin = string.Empty;
            string contentEnd = string.Empty;
            GetContentTags(ref contentBegin, ref contentEnd);
            html += contentBegin;

             XPathNavigator xNav;
            // Protocol HP version
            if (docType == PreviewTypes.Protocol_HP)
                xNav = protocol.Xml.CreateNavigator();
            else 
                xNav = protocol.PatientXML.CreateNavigator();   // Protocol Patient version

            FormatProtocolSections(xNav, protocol);
            string protocolContent = xNav.OuterXml;
            string strCDRID = CDRHelper.RebuildCDRID(protocol.ProtocolID.ToString()); 
            // Following modification is to resolve the link around title in FireFox browser.
            FormatProtocolHTML(ref protocolContent, strCDRID);
            html += protocolContent;

            html += "<a href=\"#top\" class=\"backtotop-link\"><img src=\"/images/backtotop_red.gif\" alt=\"Back to Top\" border=\"0\"/>Back to Top</a><br/><br/>";
            html += contentEnd;
            html += endTag;
        }

         // <summary>
        /// CTGov Protocol preview
        /// </summary>
        /// <param name="html">CTGov Protocol html web page</param>
        /// <param name="protocol">ProtocolDocument object</param>
        /// <returns>html in parameter reference</returns>
        public static void CTGovProtocolPreview(ref string html, ProtocolDocument protocol)
        {
            string startTag = string.Empty;
            string endTag = string.Empty;
            PageFrame(ref startTag, ref endTag);
            html = startTag;

            // Add document title
            string begin = string.Empty;
            string end = string.Empty;
            string titleImage = "title_cancertopics.jpg";
            int tableWidth = 580;
            GetPageTitleTags(ref begin, ref end, titleImage, tableWidth.ToString());
            html += begin + "Clinical Trials (PDQ<sup class=\"header\">&#174;</sup>)" + end;

            html += "<tr><td height=\"10\"/></tr>";

            // Start the page content
            string contentBegin = string.Empty;
            string contentEnd = string.Empty;
            GetContentTags(ref contentBegin, ref contentEnd);
            html += contentBegin;
            XPathNavigator xNav = protocol.Xml.CreateNavigator();
            string strCDRID = CDRHelper.RebuildCDRID(protocol.ProtocolID.ToString());
            FormatCTGovProtocolSections(xNav, protocol, strCDRID);
            string protocolContent = xNav.OuterXml;
            // Following modification is to resolve the link around title in FireFox browser.
            FormatProtocolHTML(ref protocolContent, strCDRID);
            html += protocolContent;

            html += "<a href=\"#top\" class=\"backtotop-link\"><img src=\"/images/backtotop_red.gif\" alt=\"Back to Top\" border=\"0\"/>Back to Top</a><br/><br/>";
            html += contentEnd;
            html += endTag;
        }


        // <summary>
        /// Glossary Term Preview
        /// </summary>
        /// <param name="html">summary html web page</param>
        /// <param name="glossary">GlossaryTermDocument object</param>
        /// <returns>html in parameter reference</returns>
        public static void GlossaryPreview(ref string html, GlossaryTermDocument glossary)
        {
            string startTag = string.Empty;
            string endTag = string.Empty;
            PageFrame(ref startTag, ref endTag);
            StringBuilder sb = new StringBuilder();
            sb.Append(startTag);

            // Add document title
            string begin = string.Empty;
            string end = string.Empty;
            string titleImage = "title_cancertopics.jpg";
            int tableWidth = 580;
            GetPageTitleTags(ref begin, ref end, titleImage, tableWidth.ToString());
            sb.AppendFormat("{0}Dictionary of Cancer Terms{1}", begin, end);

            // Start the page content
            string contentBegin = string.Empty;
            string contentEnd = string.Empty;
            GetContentTags(ref contentBegin, ref contentEnd);
            foreach (Language lang in glossary.GlossaryTermTranslationMap.Keys)
            {
                GlossaryTermTranslation trans = glossary.GlossaryTermTranslationMap[lang];

                // Get Definition HTML
                string pron = string.Empty;
                if (trans.Pronounciation != null && trans.Pronounciation.Trim().Length > 0)
                    pron = " " + trans.Pronounciation.Trim();

                foreach (GlossaryTermDefinition gtDef in trans.DefinitionList)
                {
                    sb.Append("<tr><td><img src=\"/images/spacer.gif\" border=\"0\" height=\"25\" width=\"10\"/></td></tr>");
                    sb.Append(contentBegin);

                    sb.AppendFormat("<span class=\"header-A\">{0}</span>{1}<br/>", trans.TermName, pron);
                    sb.Append("<img width=\"10\" height=\"5\" border=\"0\" src=\"/images/spacer.gif\"/><br/>");


                    /************************************************/
                    sb.Append("<table width=\"571\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\">");
                    sb.Append("<tbody>");
                    sb.Append("<tr><td valign=\"top\" width=\"3\"/><td valign=\"top\">");

                    /***********************************************/

                    string defHtml= gtDef.Html.Trim();
                    using (GlossaryTermQuery gQuery = new GlossaryTermQuery())
                    {
                        if (defHtml.Contains("<SummaryRef"))
                        {
                            gQuery.BuildSummaryRefLink(ref defHtml, 1);
                        }
                    }
                    sb.Append(defHtml);

                    // Get media link HTML
                    sb.Append("<p>");
                    foreach (MediaLink ml in trans.MediaLinkList)
                    {
                        if (ml.Language == lang)
                        {
                            string mlHtml = ml.Html;
                            // Replace media link 
                            string imagePath = ConfigurationManager.AppSettings["ImageLocation"];
                            mlHtml = mlHtml.Replace("[__imagelocation]", imagePath);
                            mlHtml = mlHtml.Replace("&amps;", "&");
                            sb.Append(mlHtml);
                        }
                    }

                    sb.Append("</p></td></tr></tbody>");
                    sb.Append("</table>");

                    sb.Append("<img width=\"10\" height=\"25\" border=\"0\" src=\"/images/spacer.gif\"/>");
                    sb.Append("<br/>");
                    sb.Append("<img width=\"571\" height=\"1\" border=\"0\" src=\"/images/gray_spacer.gif\"/><br/>");
                    sb.Append(contentEnd);
                }
            }

            sb.Append(endTag);

            html = sb.ToString();           
        }


        // <summary>
        /// Drug Info Summary Preview
        /// </summary>
        /// <param name="html">summary html web page</param>
        /// <param name="drug">DrugInfoSummaryDocument object</param>
        /// <returns>html in parameter reference</returns>
        public static void DrugInfoSummaryPreview(ref string html, DrugInfoSummaryDocument drug)
        {
            string startTag = string.Empty;
            string endTag = string.Empty;
            PageFrame(ref startTag, ref endTag);
            html = startTag;
            
            // Add page title
            string begin = string.Empty;
            string end = string.Empty;
            string titleImage = "title_druginfopills.jpg";
            int tableWidth = 480;
            GetPageTitleTags(ref begin, ref end, titleImage, tableWidth.ToString());
            html += begin + "Drug Information" + end;

            // Display date
            bool lineBreak = false;
            //html += "<tr><td><table width=\"651\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\" align=\"right\"><tr><td valign=\"top\" align=\"right\">";
            html += "<tr><td valign=\"top\" align=\"right\">";
            if (drug.FirstPublishedDate != DateTime.MinValue)
            {
                html += "<b>Posted: </b> " + GetDate(drug.FirstPublishedDate, "MM/DD/YYYY");
                lineBreak = true;
            }
            if (drug.LastModifiedDate != DateTime.MinValue)
            {
                if (drug.ReceivedDate != DateTime.MinValue && drug.ReceivedDate != null)
                    html += "&nbsp;&nbsp;&nbsp;";

                html += "<b>Updated: </b> " + GetDate(drug.LastModifiedDate, "MM/DD/YYYY");
                html += "<img src=\"/images/spacer.gif\" border=\"0\" height=\"1\" width=\"6\"/>";
                lineBreak = true;
            }
            html += "</td></tr>";
            if (lineBreak)
                html += "<tr><td height=\"10\"/></tr>";

            // Start the page content
            string contentBegin = string.Empty;
            string contentEnd = string.Empty;
            GetContentTags(ref contentBegin, ref contentEnd);
            html += contentBegin;
            html += "<span class=\"page-title\">" + drug.Title + "</span>";
            string htmlContent = drug.Html;
            DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery();
            drugQuery.ReformatHTMLEndTag(ref htmlContent);
            string glossaryTermTag = "Summary-GlossaryTermRef";
            if (htmlContent.Contains(glossaryTermTag))
            {
                drugQuery.BuildGlossaryTermRefLink(ref htmlContent, glossaryTermTag);
            }

            html += htmlContent;
            html += "<a href=\"#top\" class=\"backtotop-link\">" +
        "<img src=\"/images/backtotop_red.gif\" alt=\"Back to Top\" border=\"0\"/>Back to Top</a><br/><br/>";
            html += contentEnd;
            html += endTag;

        }

        public static void GeneticsProfessionalPreview(ref string html, GeneticsProfessionalDocument geneticsProfessional)
        {
            StringBuilder sb = new StringBuilder();

            string startTag = string.Empty;
            string endTag = string.Empty;
            PageFrame(ref startTag, ref endTag);
            sb.Append(startTag);

            // Add page title
            string begin = string.Empty;
            string end = string.Empty;
            string titleImage = "title_default.jpg";
            int tableWidth = 575;
            GetPageTitleTags(ref begin, ref end, titleImage, tableWidth.ToString());
            sb.Append(begin + "	Cancer Genetics Professionals" + end);

            // Imaginary spacer row.
            sb.Append(@"<tr><td valign=""top"" align=""right"">&nbsp;</td></tr>");

            // Start the page content
            string contentBegin = string.Empty;
            string contentEnd = string.Empty;
            GetContentTags(ref contentBegin, ref contentEnd);

            sb.Append(contentBegin);



            // Header
            sb.Append(@"<table cellpadding=""1"" width=""100%"" cellspacing=""0"" border=""0"" class=""gray-border"">
						<tr>
							<td>
								<table cellpadding=""7"" cellspacing=""0"" border=""0"" width=""100%"" bgcolor=""#ffffff"">
									<tr>
										<td>
											<span class=""grey-text"">This directory lists professionals who provide services related to cancer genetics (cancer risk assessment, genetic counseling, genetic susceptibility testing, and others). These professionals have applied to be listed in this directory. For information on inclusion criteria and applying to the directory, see the <a href=""http://www.cancer.gov/forms/joinGeneticsDirectory"">application form</a>.</span>
										</td>
									</tr>				
								</table>
							</td>
						</tr>				
					</table>		
					<p>");

            // Genetics professional.
            sb.Append("<table border=\"0\" cellpadding=\"1\" cellspacing=\"0\" class=\"gray-border\" width=\"100%\"><tr><td>");
            sb.Append("<table border=\"0\" cellpadding=\"10\" cellspacing=\"0\" bgcolor=\"#ffffff\" width=\"100%\"><tr><td>");
            sb.Append(geneticsProfessional.Html);
            sb.Append("</td></tr></table>");
            sb.Append("</td></tr></table>");

            // Back to the top link.
            sb.Append("<p>");
            sb.Append("<a href=\"#top\" class=\"backtotop-link\"><img src=\"/images/backtotop_red.gif\" alt=\"Back to Top\" border=\"0\"/>Back to Top</a><br/><br/>");
            sb.Append("<p>");


            sb.Append(contentEnd);
            sb.Append(endTag);

            html = sb.ToString();
        }
        
        #endregion

        #region Private methods

        // <summary>
        /// Build page frame to make it looks close to what's on Cancer.gov website
        /// </summary>
        /// <param name="html">summary html web page</param>
        /// <returns></returns>
        private static void PageFrame(ref string startTag, ref string endTag)
        {
            startTag = "<html>" + 
                        "<head runat=\"server\">" + 
                        "<title>CDR Preview</title>" + 
                        "<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />" +
                        "<link rel=\"stylesheet\" href=\"http://www.cancer.gov/stylesheets/nci.css\" type=\"text/css\"/>" +
		                "<link rel=\"stylesheet\" href=\"http://www.cancer.gov/stylesheets/nci_general_browsers.css\" type=\"text/css\"/>" +
                         GetJScript() + 
                        "</head>" +
                        "<body leftmargin=\"0\" topmargin=\"0\" marginheight=\"0\" marginwidth=\"0\">" +
                        "<form id=\"form1\" runat=\"server\">" + 
                        "<div align=\"center\">" +
                        "<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\">" + 
                        "<tr><td>" +
                        "<table width=\"100%\">" + 
                        "<tr><td/>" +
                        "<td>" +
                        "<table width=\"751\"  cellspacing=\"0\" cellpadding=\"0\" border=\"0\" align=\"center\">";

            endTag = "</table></td><td/></tr></table>" + 
                     "</td></tr>" +
                     "</table>" + 
                     "</div>" + 
                     "</form>" + 
                     "</body>" +
                    "</html> ";
        }

        // <summary>
        /// The grey backgrould document title with two images at the left side of the title bar
        /// </summary>
        /// <param name="begin">begin part of the title html</param>
        /// // <param name="begin">end part of the title html</param>
        /// <returns>begin and end in paremeter reference format</returns>
        private static void GetPageTitleTags(ref string begin, ref string end, string titleImage, string tableWidth)
        {
            // ToDo: Need to put the image location into the config file.
            begin = "<tr><td><table bgcolor=\"#d4d9d9\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"751\">" +
                        "<tr><td valign=\"top\">" +
                            "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + tableWidth + "\"><tbody>" +
                                "<tr><td colspan=\"2\"><img src=\"/images/spacer.gif\" border=\"0\" height=\"4\" width=\"1\"/></td></tr>" +
                                "<tr><td><img src=\"/images/spacer.gif\" alt=\"\" border=\"0\" height=\"1\" width=\"5\"/></td>" +
                                    "<td align=\"left\"><span class=\"document-title\">";

            // Document page title is added in between begin and end
            end = "</span></td></tr>" +
                                "<tr><td colspan=\"2\"><img src=\"/images/spacer.gif\"  border=\"0\" height=\"4\" width=\"1\"/></td></tr></tbody>" +
                            "</table></td>" +
                        "<td valign=\"top\"><img src=\"/images/spacer.gif\" border=\"0\" height=\"1\" width=\"10\"/></td>" +
                        "<td bgcolor=\"#ffffff\" valign=\"top\"><img src=\"/images/spacer.gif\" border=\"0\" height=\"1\" width=\"1\"/></td>" +
                        "<td align=\"right\" bgcolor=\"#d4d9d9\" valign=\"top\"><img src=\"" + GetServerURL() + "/images/" + titleImage + "\" border=\"0\"/></td></tr>" +
                    "</table></td></tr>";
        }           

        // <summary>
        /// Wrapper structure for page content
        /// </summary>
        /// <param name="begin">begin part of the content html</param>
        /// <param name="end">end part of the content html</param>
        /// <returns>begin and end in paremeter reference format</returns>
        private static void GetContentTags(ref string begin, ref string end)
        {
            begin = "<tr><td><table width=\"751\"  cellspacing=\"0\" cellpadding=\"0\" border=\"0\">" + 
                        "<tr><td width=\"180\"></td>" +  
                            "<td align=\"left\" border=\"0\">" +
                                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"571\" align=\"left\">" +
                                    "<tbody><tr><td>";
            // Document content is added in between begin and end
            end =                   "</td></tr></tbody>" + 
                                "</table></td></tr>" + 
                    "</table></td></tr>";
        }


        // <summary>
        /// Build Summary Table of Contents
        /// </summary>
        /// <param name="begin">begin part of the content html</param>
        /// // <param name="begin">end part of the content html</param>
        /// <returns>begin and end in paremeter reference format</returns>
        private static void BuildSummaryTableOfContents(ref string html, List<SummarySection> SectionList, Language lang)
        {
            string toc = string.Empty;
            string index = string.Empty;
            if (lang == Language.Spanish)
                index = "Índice";
            else 
                index = "Table of Contents";

            toc = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"571\">" + 
                    "<tbody><tr><td valign=\"top\"><p><span class=\"page-title\">" + index + "</span></p>";

            foreach (SummarySection sect in SectionList)
            {
                if (sect.ParentSummarySectionID == Guid.Empty)
                {
                    toc += "<a href=\"#Section_" + sect.SectionID + "\">" + sect.Title + "</a><br>";

                    // Add the second level toc
                    if (sect.TOC.Trim() != string.Empty)
                    {
                        toc += "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tbody><tr><td><img src=\"/images/spacer.gif\" alt=\"\" border=\"0\" height=\"1\" width=\"30\"></td><td width=\"100%\">";
                        toc += sect.TOC;
                        toc += "</td></tr></tbody></table>";
                    }
                }
            }

           // Close table on level 1
            toc += "</td></tr></tbody></table><br/>";
            html += toc;
        }

        /// <summary>
        /// Method to parse the HP sections for html output.
        /// For each protocol section call FormatSectionHTML method in query class to match html format with what's saved in database
        /// problems
        /// </summary>
        /// <param name="xNav">XPathNavigator for Protocol document</param>
        /// <param name="protocol"></param>
        /// <return></return>
        private static void FormatProtocolSections(XPathNavigator xNav, ProtocolDocument protocol)
        {
            ProtocolQuery proQuery = new ProtocolQuery();

            try
            {
                // HPDisclaimer
                XPathNavigator disclaimerNav = xNav.SelectSingleNode("//a[@name='Disclaimer']");
                if (disclaimerNav != null)
                {
                    string html = proQuery.FormatSectionHTML(new ProtocolSection(0, disclaimerNav.OuterXml, ProtocolSectionType.HPDisclaimer));
                    if (html.StartsWith("</a>"))
                        html = html.Substring("</a>".Length).Trim();
                    disclaimerNav.OuterXml = html;
                }

                // HPObjectives
                XPathNavigator objectivesNav = xNav.SelectSingleNode("//a[starts-with(@name, 'Objectives:')]");
                if (objectivesNav != null)
                {
                    objectivesNav.OuterXml = "<Objectives>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, objectivesNav.OuterXml, ProtocolSectionType.Objectives)) + "</Objectives>";
                }

                // HPEntryCriteria
                XPathNavigator entryCriteriaNav = xNav.SelectSingleNode("//a[starts-with(@name, 'EntryCriteria:')]");
                if (entryCriteriaNav != null)
                {
                    entryCriteriaNav.OuterXml = "<EntryCriteria>" + proQuery.FormatSectionHTML(
                         new ProtocolSection(0, entryCriteriaNav.OuterXml, ProtocolSectionType.EntryCriteria)) + "</EntryCriteria>";
                }

                // HPExpectedEnrollment
                XPathNavigator expectedEnrollmentNav = xNav.SelectSingleNode("//a[starts-with(@name, 'ExpectedEnrollment:')]");
                if (expectedEnrollmentNav != null)
                {
                    expectedEnrollmentNav.OuterXml = "<ExpectEnrollment>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, expectedEnrollmentNav.OuterXml, ProtocolSectionType.ExpectedEnrollment)) + "</ExpectEnrollment>";
                }

                // HPOutline
                XPathNavigator outlineNav = xNav.SelectSingleNode("//a[starts-with(@name, 'Outline:')]");
                if (outlineNav != null)
                {
                    outlineNav.OuterXml = "<Outline>" + proQuery.FormatSectionHTML(
                         new ProtocolSection(0, outlineNav.OuterXml, ProtocolSectionType.Outline)) + "</Outline>";
                }

                // HPPubResults
                XPathNavigator publishedResultsNav = xNav.SelectSingleNode("//a[starts-with(@name, 'PublishedResults:')]");
                if (publishedResultsNav != null)
                {
                    publishedResultsNav.OuterXml = "<PublishedResult>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, publishedResultsNav.OuterXml, ProtocolSectionType.PublishedResults)) + "</PublishedResult>";
                }

                // HPLeadOrgs
                XPathNavigator leadOrgsNav = xNav.SelectSingleNode("//a[starts-with(@name, 'LeadOrgs:')]");
                if (leadOrgsNav != null)
                {
                    leadOrgsNav.OuterXml = "<LeadOrg>" + proQuery.FormatSectionHTML(
                       new ProtocolSection(0, leadOrgsNav.OuterXml, ProtocolSectionType.LeadOrgs)) + "</LeadOrg>";
                }

                // HPProtocolRelatedLinks
                XPathNavigator protocolRelatedLinksNav = xNav.SelectSingleNode("//a[starts-with(@name, 'ProtocolRelatedLinks:')]");
                if (protocolRelatedLinksNav != null)
                {
                    protocolRelatedLinksNav.OuterXml = "<RelatedLinks>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, protocolRelatedLinksNav.OuterXml, ProtocolSectionType.HPProtocolRelatedLinks)) + "</RelatedLinks>";
                }
                // Outcomes
                XPathNavigator outcomesNav = xNav.SelectSingleNode("//a[starts-with(@name, 'Outcomes:')]");
                if (outcomesNav != null)
                {
                    outcomesNav.OuterXml = "<Outcomes>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, outcomesNav.OuterXml, ProtocolSectionType.Outcomes)) + "</Outcomes>";
                }

                // RegistryInfo 
                XPathNavigator registryInfoNav = xNav.SelectSingleNode("//a[starts-with(@name, 'RegistryInfo:')]");
                if (registryInfoNav != null)
                {
                    registryInfoNav.OuterXml = "<RegistryInfo>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, registryInfoNav.OuterXml, ProtocolSectionType.RegistryInformation)) + "</RegistryInfo>";
                }

                // RelatedPublications 
                XPathNavigator relatedPublicationsNav = xNav.SelectSingleNode("//a[starts-with(@name, 'RelatedPublications:')]");
                if (relatedPublicationsNav != null)
                {
                    relatedPublicationsNav.OuterXml = "<Publications>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, relatedPublicationsNav.OuterXml, ProtocolSectionType.RelatedPublications)) + "</Publications>";
                }

                // HP Trial Site
                XPathNavigator trialSiteNav = xNav.SelectSingleNode("//a[starts-with(@name, 'SitesAndContacts')]");
                if (trialSiteNav != null)
                {
                    trialSiteNav.OuterXml = "<TrialSites>" + proQuery.FormatSectionHTML(
                                            new ProtocolSection(0, trialSiteNav.OuterXml, ProtocolSectionType.RelatedPublications)) + "</TrialSites>";
                }

                // Patient - SpecialCategory 
                XPathNavigator specialCategoryNav = xNav.SelectSingleNode("//a[starts-with(@name, 'SpecialCategory:')]");
                if (specialCategoryNav != null)
                {
                    specialCategoryNav.OuterXml = "<SpecialCategories>" + proQuery.FormatSectionHTML(
                                            new ProtocolSection(0, specialCategoryNav.OuterXml, ProtocolSectionType.SpecialCategory)) + "</SpecialCategories>";
                }
                  
                 // Patient - Trial Description
                XPathNavigator patientAbstractNav = xNav.SelectSingleNode("//a[starts-with(@name, 'TrialDescription:')]");
                if (patientAbstractNav != null)
                {
                    patientAbstractNav.OuterXml = "<TrialDescription>" + proQuery.FormatSectionHTML(
                                            new ProtocolSection(0, patientAbstractNav.OuterXml, ProtocolSectionType.SpecialCategory)) + "</TrialDescription>";
                }
                         
            }
            catch (Exception e)
            {
                throw new Exception("Format Protocol session error:" + e.Message);
            }
        }

        /// <summary>
        /// Method to parse the CTGovProtocol sections for html output.
        /// For each protocol section call FormatSectionHTML method in query class to match html format with what's saved in database
        /// </summary>
        /// <param name="xNav">XPathNavigator for CTGovProcotocol document</param>
        /// <param name="protocol">CTGovProtocol object</param>
        /// <param name="strCDRID"></param>
        /// <return></return>
        private static void FormatCTGovProtocolSections(XPathNavigator xNav, ProtocolDocument protocol, string strCDRID)
        {
            ProtocolQuery proQuery = new ProtocolQuery();
            try{
                 // Summary (Objectives)
                XPathNavigator summaryNav = xNav.SelectSingleNode("//a[@name='Summary']");
                if (summaryNav != null)
                {
                    string html = summaryNav.InnerXml;
                    html = html.Replace("<a name=\"Section\" />", "<a name=\"Section\"></a>");
                    string trial = "<p><a name=\"TrialDescription_" + strCDRID + "\"></a><span class=\"Protocol-Section-Heading\">Trial Description</span></p>";
                    summaryNav.InnerXml = trial + "<a name=\"Summary_" + strCDRID + "\"></a>" + html;
                    summaryNav.OuterXml = "<Summary>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, summaryNav.OuterXml, ProtocolSectionType.CTGovBriefSummary)) + "</Summary>";
                }

                // Disclaimer
                XPathNavigator disclaimerNav = xNav.SelectSingleNode("//a[@name='Disclaimer']");
                if (disclaimerNav != null)
                {
                    disclaimerNav.InnerXml = "<a name=\"Disclaimer_" + strCDRID + "\"></a>" + disclaimerNav.InnerXml;
                    string tempHtml = disclaimerNav.OuterXml;
                     disclaimerNav.OuterXml = "<Disclaimer>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0,
                        tempHtml.Replace("http://www.nlm.nih.gov/contacts/custserv-email.html", "<a href=\"http://www.nlm.nih.gov/contacts/custserv-email.html\">http://www.nlm.nih.gov/contacts/custserv-email.html</a>"), ProtocolSectionType.CTGovDisclaimer)) + "</Disclaimer>";
                }

                // LeadOrgs
                XPathNavigator leadOrgsNav = xNav.SelectSingleNode("//a[@name='LeadOrgs']");
                if (leadOrgsNav != null)
                {
                    leadOrgsNav.InnerXml = "<a name=\"LeadOrgs_" + strCDRID + "\"></a>" + leadOrgsNav.InnerXml;
                    leadOrgsNav.OuterXml = "<LeadOrg>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, leadOrgsNav.OuterXml,   ProtocolSectionType.CTGovLeadOrgs)) +  "</LeadOrg>" ;
                }

                // Handle footer (specified in source document as the required header)
                XPathNavigator requiredHeaderNav = xNav.SelectSingleNode("//a[@name='RequiredHeader']");
                if (requiredHeaderNav != null)
                {
                    requiredHeaderNav.OuterXml = "<RequiredHeader>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0,requiredHeaderNav.InnerXml, ProtocolSectionType.CTGovFooter)) + "</RequiredHeader>";
                }

                // Detailed description
                XPathNavigator detailedDescNav = xNav.SelectSingleNode("//a[@name='DetailedDescription']");
                if (detailedDescNav != null)
                {
                    string html = detailedDescNav.InnerXml;
                    html = html.Replace("<a name=\"Section\" />", "<a name=\"Section\"></a>");

                    detailedDescNav.InnerXml = "<a name=\"Outline_" + strCDRID + "\"></a>" + html;
                    detailedDescNav.OuterXml = "<DetailedDesc>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, detailedDescNav.OuterXml,ProtocolSectionType.CTGovDetailedDescription)) +  "</DetailedDesc>";
                }

                // Entry criteria
                XPathNavigator entryCriteriaNav = xNav.SelectSingleNode("//a[@name='EntryCriteria']");
                if (entryCriteriaNav != null)
                {
                    string html = entryCriteriaNav.InnerXml;
                    html = html.Replace("<a name=\"Section\" />", "<a name=\"Section\"></a>");
                    html = proQuery.FormatSectionHTML(
                        new ProtocolSection(0, entryCriteriaNav.OuterXml, ProtocolSectionType.CTGovEntryCriteria));
                    html = html.Replace("<a name=\"EndEntryCriteria\">", string.Empty);
                    entryCriteriaNav.OuterXml = "<EntryCriteria>" + html + "</EntryCriteria>";
                }
             
                // Trail contact info
                XPathNavigator contctInfoNav = xNav.SelectSingleNode("//a[@name='TrialContact']");
                if (contctInfoNav != null)
                {
                    string html = contctInfoNav.OuterXml;
                    html = html.Replace("<a name=\"TrialContact\" />", "<a name = \"TrialContact_" + strCDRID + "\"></a>");
                    contctInfoNav.OuterXml =html;
                }

                XPathNavigator trialSiteNav = xNav.SelectSingleNode("//a[@name='TrialSites']");
                if (trialSiteNav != null)
                {
                    string html = trialSiteNav.InnerXml;
                    trialSiteNav.OuterXml = "<TrialSites>" + html.Trim() + "</TrialSites>";
                }                
            }
            catch (Exception e)
            {
                throw new Exception("Format CTGovProtocol session error:" + e.Message);

            }
        }

        /// <summary>
        /// Method to modify html to have correct HTML syntax and also use Cancer.Gov stylesheet defined style.
        /// Also resolve the link around title in FireFox browser.
        /// </summary>
        /// <param name="protocolContent"></param>
        /// <param name="strCDRID"></param>
        /// <return></return>        
        private static void FormatProtocolHTML(ref string protocolContent, string strCDRID)
        {
            protocolContent = protocolContent.Replace("<a name=\"AlternateTitle_" + strCDRID + "\" />", "<a name=\"AlternateTitle_" + strCDRID + "\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"TrialIdInfo_" + strCDRID + "\" />", "<a name=\"TrialIdInfo_" + strCDRID + "\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"SpecialCategory_" + strCDRID + "\" />", "<a name=\"SpecialCategory_" + strCDRID + "\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"TrialDescription_" + strCDRID + "\" />", "<a name=\"TrialDescription_" + strCDRID + "\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"TrialContact_" + strCDRID + "\" />", "<a name=\"TrialContact_" + strCDRID + "\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"Purpose_" + strCDRID + "\" />", "<a name=\"Purpose_" + strCDRID + "\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"Eligibility_" + strCDRID + "\" />", "<a name=\"Eligibility_" + strCDRID + "\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"TreatmentIntervention_" + strCDRID + "\" />", "<a name=\"TreatmentIntervention_" + strCDRID + "\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"ListSection\" />", "<a name=\"ListSection\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"StudyIdInfo\" />", "<a name=\"StudyIdInfo\"></a>");
            protocolContent = protocolContent.Replace("<a name=\"StudySites\"/>", string.Empty);
            protocolContent = protocolContent.Replace("<TrialSites>", string.Empty);
            protocolContent = protocolContent.Replace("</TrialSites>", string.Empty);
            protocolContent = protocolContent.Replace("Protocol-BasicStudy-Grayborder", "gray-border");
            protocolContent = protocolContent.Replace("Protocol-BasicStudy-TD-Grayborder", "gray-border");
            protocolContent = protocolContent.Replace("Protocol-BasicStudy-PrimaryID", "protocol-primaryprotocolid");
            protocolContent = protocolContent.Replace("Protocol-BasicStudy-AlternateID", "protocol-alternateprotocolids");
            protocolContent = protocolContent.Replace("Protocol-BasicStudy-Heading", "black-text");
            protocolContent = protocolContent.Replace("<tr />", string.Empty);
            protocolContent = protocolContent.Replace("<tr></tr>", string.Empty);
            protocolContent = protocolContent.Replace("<p/ >", string.Empty);
            protocolContent = protocolContent.Replace("<p/>", string.Empty);
            protocolContent = protocolContent.Replace("Pharmaceutical/Industry", "Pharmaceutical / Industry");
            protocolContent = protocolContent.Replace("<a name=\"END_ListSection\" />", string.Empty);

            // This is to fix the extra spaces between to span tags
            protocolContent = Regex.Replace(protocolContent, "</span>[\\s\\t\\f]*<span", "</span><span", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            protocolContent = Regex.Replace(protocolContent, "<br/ >[\\s\\t\\f]*<span", "<p><span", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            protocolContent = Regex.Replace(protocolContent, "</span>[\\s\\t\\f]*</a>", "</span></a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            protocolContent = Regex.Replace(protocolContent, "</tr>[\\s\\t\\f]*<tr", "</tr><tr", RegexOptions.Compiled | RegexOptions.IgnoreCase);
           
            // Remove the unused tag that has the pattern <a name="Section_alpha_number />, this cause text after it become link in firefox.
            protocolContent = Regex.Replace(protocolContent, "<a name=\"Section_*[A-Za-z]*[_]*[0-9]*\" />", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string glossaryTermTag = "Protocol-GlossaryTermRef";
            if (protocolContent.Contains(glossaryTermTag))
            {
                ProtocolQuery pQuery = new ProtocolQuery();
                pQuery.BuildGlossaryTermRefLink(ref protocolContent, glossaryTermTag);
            }
        }

        /// <summary>
        /// Retrieve web service server url from configuration file
        /// </summary>
        /// <param></param>
        /// <returns>Return server url</returns>
        private static string GetServerURL()
        {
            return ConfigurationManager.AppSettings["ServerURL"];
        }

        /// <summary>
        /// Javascript for the image enlarge popup window
        /// </summary>
        /// <param></param>
        /// <returns>java function</returns>
        private static string GetJScript()
        {
            return @"<script language='javascript'>
            <!--
            function dynPopWindow(url, name, windowAttributes)
            {
	            options = '';
	            optWidth = 'width=500';
	            optHeight = 'height=500';
	            optScrollbar = 'scrollbars=yes';
	            optResizable = 'resizable=yes';
	            optMenubar = 'menubar=yes';
	            optLocation = 'location=yes';
	            optStatus = 'status=yes';
	            optToolbar = 'toolbar=yes';
            	windowOptions = windowAttributes.split(',');

	            for(i = 0; i < windowOptions.length; i++)
	            {
		            attribute = windowOptions[i].substring(0, windowOptions[i].indexOf('=')).toLowerCase();
            	    if(attribute == 'width'){
			            optWidth = windowOptions[i];
		            } else if(attribute == 'height'){
			            optHeight = windowOptions[i];
		            } else if(attribute == 'scrollbars'){
			            optScrollbar = windowOptions[i];	
		            } else if(attribute == 'resizable'){
			            optResizable = windowOptions[i];	
		            } else if(attribute == 'menubar'){
			            optMenubar = windowOptions[i];	
		            } else if(attribute == 'location'){
			            optLocation = windowOptions[i];	
		            } else if(attribute == 'status'){
			            optStatus = windowOptions[i];	
		            } else if(attribute == 'toolbar'){
			            optToolbar = windowOptions[i];	
		            }
	            }
            	
	            options = optWidth + ',' + optHeight + ',' + optScrollbar + ',' + optResizable + ',' + optMenubar + ',' + optLocation + ',' + optStatus + ',' + optToolbar;
            	window.open(url, name, options);
            	
            }
            // --></script>";
        }

          /// <summary>
        /// Convert date into MM/DD/YYYY format
        /// </summary>
        /// <param></param>
        /// <returns>Date</returns>
        private static string GetDate(DateTime date, string format)
        {
            string day = string.Empty;
            string month = string.Empty;
            
            if (format == "MM/DD/YYYY")
            {
                if (date.Day < 10)
                    day = "0" + date.Day.ToString();
                else
                    day = date.Day.ToString();

                if (date.Month < 10)
                    month = "0" + date.Month.ToString();
                else
                    month = date.Month.ToString();
            }
            else
            {
                day = date.Day.ToString();
                month = date.Month.ToString();
            }

            return month+"/"+day+"/"+date.Year.ToString();

        }
        #endregion
    }
}
