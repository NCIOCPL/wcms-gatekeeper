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
using GateKeeper.DocumentObjects.Dictionary;
using CDRPreviewWS.GlossaryTerm;

namespace CDRPreviewWS
{
    public static class GenerateCDRPreview
    {
        #region Public methods

        // <summary>
        /// Protocol Health Professional / Patient Preview
        /// </summary>
        /// <param name="html">Protocol html web page</param>
        /// <param name="protocol">ProtocolDocument object</param>
        /// <returns>html in parameter reference</returns>
        public static void ProtocolPreview(ref string html, ref string headerHtml, ProtocolDocument protocol, PreviewTypes docType)
        {
            headerHtml = createHeaderZoneContent("Clinical Trials (PDQ&#174;)", protocol, docType);

            // Add last modified date and first published date
            headerHtml += "<div id=\"language-dates\">";
            string audience = "Patient";
            if( docType == PreviewTypes.Protocol_HP )
                audience = "Health Professional";
            headerHtml += string.Format("<div class=\"version-language\"><ul><li class=\"one active\">{0}</li></ul></div>", audience);

            headerHtml +=   "<div class=\"document-dates\"><ul>";
            if (protocol.FirstPublishedDate != DateTime.MinValue)
                headerHtml += string.Format("<li><strong>First Published: </strong>{0}</li>", GetDate(protocol.FirstPublishedDate, string.Empty));
            if (protocol.LastModifiedDate != DateTime.MinValue && protocol.LastModifiedDate != null)
                headerHtml += string.Format("<li><strong>Last Modified: </strong>{0}</li>", GetDate(protocol.LastModifiedDate, string.Empty));
            headerHtml += "</ul></div>";

            headerHtml += "</div>";

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
            html = protocolContent;
            html += "<a href=\"#top\" class=\"backtotop-link\"><img src=\"/images/backtotop_red.gif\" alt=\"Back to Top\" border=\"0\"/>Back to Top</a><br/><br/>";
        }

        // <summary>
        /// CTGov Protocol preview
        /// </summary>
        /// <param name="html">CTGov Protocol html web page</param>
        /// <param name="protocol">ProtocolDocument object</param>
        /// <returns>html in parameter reference</returns>
        public static void CTGovProtocolPreview(ref string html, ref string headerHtml , ProtocolDocument protocol)
        {
            headerHtml = "";

            XPathNavigator xNav = protocol.Xml.CreateNavigator();
            string strCDRID = CDRHelper.RebuildCDRID(protocol.ProtocolID.ToString());
            FormatCTGovProtocolSections(xNav, protocol, strCDRID);
            string protocolContent = xNav.OuterXml;
            // Following modification is to resolve the link around title in FireFox browser.
            FormatProtocolHTML(ref protocolContent, strCDRID);
            html += protocolContent;
            
        }


        // <summary>
        /// Glossary Term Preview
        /// </summary>
        /// <param name="html">summary html web page</param>
        /// <param name="glossary">GlossaryTermDocument object</param>
        /// <returns>html in parameter reference</returns>
        public static void GlossaryPreview(ref string html, ref string headerHtml, GlossaryTermDocument glossary)
        {
            GlossaryTermDeserializer glossaryTermHtml = new GlossaryTermDeserializer();
            html = glossaryTermHtml.GenerateGlossaryTermPreview(glossary);

        }


        public static void GeneticsProfessionalPreview(ref string html, ref string headerHtml, GeneticsProfessionalDocument geneticsProfessional)
        {
            StringBuilder sb = new StringBuilder();

            headerHtml = "";
                        
            // Header
            sb.Append(@"<p>This directory lists professionals who provide services related to cancer genetics (cancer risk assessment, genetic counseling, genetic susceptibility testing, and others). These professionals have applied to be listed in this directory. For information on inclusion criteria and applying to the directory, see the <a href=""http://www.cancer.gov/forms/joinGeneticsDirectory"">application form</a>.</p>");						

            // Genetics professional.
            sb.Append("<div class=\"result\">");
              sb.Append(geneticsProfessional.Html);
 
            sb.Append("</div>");

          
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
            string resourceBase = ConfigurationManager.AppSettings["PublishedContentBase"];
            if (resourceBase.EndsWith("/"))
                resourceBase = resourceBase.Substring(0, resourceBase.Length - 1);

            startTag = "<html>" +
                        "<head runat=\"server\">" +
                        "<title>CDR Preview</title>" +
                        "<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />" +
                        "<link rel=\"stylesheet\" href=\"" + resourceBase + "/Styles/nci.css\" type=\"text/css\"/>" +
                        "<link rel=\"stylesheet\" href=\"" + resourceBase + "/Styles/nci-new.css\" type=\"text/css\"/>" +
                        "<link rel=\"stylesheet\" href=\"" + resourceBase + "/Styles/nciplus.css\" type=\"text/css\"/>" +
                         GetJScript() +
                         "<script src=\"" + resourceBase + "/js/modernizr-1.7.min.js\"></script>\n" +
                         "<script src=\"http://ajax.googleapis.com/ajax/libs/jquery/1.5.1/jquery.min.js\" type=\"text/javascript\"></script>\n" +
                         "<script src=\"https://ajax.googleapis.com/ajax/libs/swfobject/2.2/swfobject.js\" type=\"text/javascript\"></script>\n" +
                         "<script src=\"common/wcmsAudio.js\" type=\"text/javascript\"></script>\n" +
                        "</head>" +
                        "<body leftmargin=\"0\" topmargin=\"0\" marginheight=\"0\" marginwidth=\"0\">" +
                        "<div id=\"cgovContainer\">" +
                        "<div align=\"center\">" +
                        "<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\">" +
                        "<tr><td>" +
                        "<table width=\"100%\">" +
                        "<tr><td/>" +
                        "<td>" +
                        "<table width=\"100%\"  cellspacing=\"0\" cellpadding=\"0\" border=\"0\" align=\"center\">";

            endTag = "</table></td><td/></tr></table>" +
                     "</td></tr>" +
                     "</table>" +
                     "</div>" +
                     "</div>" +
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
            begin = "<tr><td><table bgcolor=\"#d4d9d9\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
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
            begin = "<tr><td><table width=\"100%\"  cellspacing=\"0\" cellpadding=\"0\" border=\"0\">" +
                        "<tr><td id=\"leftzone\" valign=\"top\"><img src=\"/images/spacer.gif\" border=\"0\" height=\"1\" width=\"164\"/></td>" +
                            "<td id=\"contentzone\" width=\"100%\" valign=\"top\">" +
                                "<div id=\"cgvBody\">" +
                                    "<div class=\"slot-item only-SI\">";
            // Document content is added in between begin and end
            end = "</div>" +
                                "</div>" +
                        "</td></tr>" +
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

            toc = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
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
            try
            {
                // Summary (Objectives)
                XPathNavigator summaryNav = xNav.SelectSingleNode("//a[@name='Summary']");
                if (summaryNav != null)
                {
                    string html = summaryNav.InnerXml;
                    html = html.Replace("<a name=\"Section\" />", "<a name=\"Section\"></a>");
                    string trial = "<h2 id=\"TrialDescription_" + strCDRID + "\">Trial Description</h2>";
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
                        new ProtocolSection(0, leadOrgsNav.OuterXml, ProtocolSectionType.CTGovLeadOrgs)) + "</LeadOrg>";
                }

                // Handle footer (specified in source document as the required header)
                XPathNavigator requiredHeaderNav = xNav.SelectSingleNode("//a[@name='RequiredHeader']");
                if (requiredHeaderNav != null)
                {
                    requiredHeaderNav.OuterXml = "<RequiredHeader>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, requiredHeaderNav.InnerXml, ProtocolSectionType.CTGovFooter)) + "</RequiredHeader>";
                }

                // Detailed description
                XPathNavigator detailedDescNav = xNav.SelectSingleNode("//a[@name='DetailedDescription']");
                if (detailedDescNav != null)
                {
                    string html = detailedDescNav.InnerXml;
                    html = html.Replace("<a name=\"Section\" />", "<a name=\"Section\"></a>");

                    detailedDescNav.InnerXml = "<a name=\"Outline_" + strCDRID + "\"></a>" + html;
                    detailedDescNav.OuterXml = "<DetailedDesc>" + proQuery.FormatSectionHTML(
                        new ProtocolSection(0, detailedDescNav.OuterXml, ProtocolSectionType.CTGovDetailedDescription)) + "</DetailedDesc>";
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
                    contctInfoNav.OuterXml = html;
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
            protocolContent = protocolContent.Replace("<a name=\"StudySites\"/>", string.Empty);
            protocolContent = protocolContent.Replace("<TrialSites>", string.Empty);
            protocolContent = protocolContent.Replace("</TrialSites>", string.Empty);
            
            protocolContent = protocolContent.Replace("<tr />", string.Empty);
            protocolContent = protocolContent.Replace("<tr></tr>", string.Empty);
            protocolContent = protocolContent.Replace("<p/ >", string.Empty);
            protocolContent = protocolContent.Replace("<p/>", string.Empty);
            protocolContent = protocolContent.Replace("Pharmaceutical/Industry", "Pharmaceutical / Industry");
            protocolContent = protocolContent.Replace("<a name=\"END_ListSection\" />", string.Empty);

            
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

            return month + "/" + day + "/" + date.Year.ToString();

        }

        private static string createHeaderZoneContent(string title, Document document, PreviewTypes docType)
        {
            // Generate the header HTML 
            string titleImage = string.Empty;
            switch (docType)
            {
                case PreviewTypes.CTGovProtocol:
                    titleImage = "title_cancertopics.jpg";
                    break;
                case PreviewTypes.GeneticsProfessional:
                    titleImage = "title_cancertopics.jpg";
                    break;
                case PreviewTypes.GlossaryTerm:
                    titleImage = "title_cancertopics.jpg";
                    break;
                case PreviewTypes.Protocol_HP:
                    titleImage = "title_clinicaltrials.jpg";
                    break;
                case PreviewTypes.Protocol_Patient:
                    titleImage = "title_clinicaltrials.jpg";
                    break;
            }

            string headerHtml = string.Format("<div id=\"cgvcontentheader\"><div class=\"document-title-block\" style=\"background-color:#d4d9d9\" >" +
                    "<img src=\"/PublishedContent/Images/SharedItems/ContentHeaders/{0}\" alt=\"\" style=\"border-width:0px;\" />" +
                    "<h1>{1}</h1></div></div>", titleImage, title);


            return headerHtml;
        }

        #endregion
    }
}
