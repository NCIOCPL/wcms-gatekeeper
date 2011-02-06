using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Configuration;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.Logging;

namespace GateKeeper.ContentRendering
{
    /// <summary>
    /// Class to render summary document type.
    /// </summary>
    public class SummaryRenderer : DocumentRenderer
    {
        #region Fields

        private SummarySection _lastTopLevelSection = null;
        private StringBuilder _currentSectionTOC = new StringBuilder();
        private Language _language;

        const string tablePlaceholder = @"
<div inlinetype=""rxvariant"" objectid=""{0}"">
  Placeholder slot
</div>";

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SummaryRenderer()
        {
            string xslPath = ConfigurationManager.AppSettings["Summary"];
            try
            {
                base.LoadTransform(new System.IO.FileInfo(xslPath));
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Loading summary XSL file " + xslPath + " failed.", e);
            }
        }

        #endregion Constructors

        #region Private Methods

        #region Enlarged Table Reference Handling Methods

        /// <summary>
        /// Method to determine if any references are contained in the summary section and 
        /// extract them to a dictionary.
        /// </summary>
        /// <param name="xNav"></param>
        /// <returns>List of reference numbers used in the summary section</returns>
        /// <remarks>Example: 1.29, 1.32...etc </remarks>
        private static List<string> ExtractReferenceLinks(XPathNavigator xNav)
        {
            List<string> referenceLinkList = new List<string>();
            try
            {
                XPathNodeIterator nodeIter = xNav.Select(".//a[starts-with(@href, '#Reference')]");

                List<string> referenceList = new List<string>();
                List<int> numberList = new List<int>();
                while (nodeIter.MoveNext())
                {
                    string referenceNumber = nodeIter.Current.GetAttribute("href", string.Empty).Replace("#Reference", string.Empty);
                    
                    if (!referenceList.Contains(referenceNumber))
                    {
                        numberList.Add(Int32.Parse(referenceNumber.Substring(referenceNumber.IndexOf(".") + 1)));
                        referenceList.Add(referenceNumber);
                       // referenceLinkList.Add(referenceNumber);

                    }
                }

                // Sort the reference in ascending order then add into final list
                numberList.Sort();
                foreach (int num in numberList)
                {
                    foreach (string reference in referenceList)
                    {
                        int subItem = Int32.Parse(reference.Substring(reference.IndexOf(".") + 1));
                        if (subItem == num)
                            referenceLinkList.Add(reference);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Rendering reference link failed.", e);
            }

            return referenceLinkList;
        }

        /// <summary>
        /// Parse out all the references and re-format them so they can be added to 
        /// the bottom of the table section.
        /// </summary>
        /// <param name="referenceSectionNav"></param>
        /// <param name="referenceList"></param>
        /// <returns></returns>
        private string FormatReferences(XPathNavigator referenceSectionNav, List<string> referenceList)
        {
            string finalReferenceLinks = string.Empty;
            try
            {
                // If there are no references, don't bother with a reference section
                if (referenceList.Count != 0)
                {
                    string title = string.Empty;
                    switch (_language)
                    {
                        case Language.English:
                            title = "<p><Span Class=\"Summary-ReferenceSection\">References</Span>";
                            break;
                        case Language.Spanish:
                            title = "<p><Span Class=\"Summary-ReferenceSection\">Bibliografía</Span>";
                             break;
                    }

                    StringBuilder tempBuffer = new StringBuilder(title);
                    // Begin ordered list of references...
                    tempBuffer.Append("<ol>");

                    foreach (string referenceNumber in referenceList)
                    {
                        // Note: Looking for the target of the reference: "<li><a name="Reference1.29" />Zeegers MP,..." 
                        // with xpath //li[a[@name='Reference1.29']]  <li value="29">
                        XPathNavigator xnav = referenceSectionNav.SelectSingleNode("//li[a[@name='Reference" + referenceNumber + "']]");

                        if (xnav != null)
                        {
                            int dot = referenceNumber.IndexOf(".");
                            string value = referenceNumber.Substring(dot + 1);
                            tempBuffer.Append("<li value=\"" + value + "\">" + xnav.InnerXml + "</li>");
                            tempBuffer.Append("\n");
                        }
                    }

                    // End ordered list of references...
                    tempBuffer.Append("</ol></p>");

                    finalReferenceLinks = tempBuffer.ToString();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Formating reference section failed.", e);
            }
            return finalReferenceLinks;
        }

        /// <summary>
        /// Handles references to the table enlarge HTML.
        /// </summary>
        /// <param name="sourceTableSectionNav">Navigator that points to table section: <a name="Section_130"><table...</param>
        private void ProcessTableForStandalone(XPathNavigator sourceTableSectionNav, out string referenceSection)
        {
            try
            {
                string tableSection = sourceTableSectionNav.InnerXml;

                // Remove the table's title.
                int indexStart = tableSection.IndexOf("<span");
                int indexEnd = tableSection.IndexOf("</span>");
                if (indexStart > 0 && indexEnd > 0)
                {
                    string firstStr = tableSection.Substring(0, indexStart);
                    string secondStr = tableSection.Substring(indexEnd + 7);
                    tableSection = firstStr + secondStr;
                }

                tableSection = tableSection.Replace("width=\"90%\"", "width=\"100%\"");
                tableSection = tableSection.Replace(">Enlarge<", "><");
                tableSection = tableSection.Replace(">Ampliar<", "><");
                // TODO:REMOVE- Replace this line for string comparison purpose.
                tableSection = tableSection.Replace("Summary-SummarySection-Small", "Summary");

                // This code is designed to add any references used in the table at the bottom of the section:

                // Note: (must be the one from the section in which the original table was found)
                List<string> referenceLinks = ExtractReferenceLinks(sourceTableSectionNav);

                // Find the reference section "<a name="ReferenceSection">" following the table section...
                XPathNavigator referenceSectionNav =
                    sourceTableSectionNav.SelectSingleNode("./following::a[@name='ReferenceSection']");

                // Call FormatReferences() to pull out and format the reference link "targets" from the
                // reference section (order list (<ol> block of references)...
                string formattedReferenceSection = FormatReferences(referenceSectionNav, referenceLinks);

                // Add re-formatted references ordered list to the bottom of the table enlarge section...
                sourceTableSectionNav.InnerXml = tableSection;
                referenceSection = formattedReferenceSection;
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Enlarging section table failed.", e);
            }
        }

        /// <summary>
        /// Adds the appropriate CSS classes to the table.
        /// </summary>
        /// <param name="sectionNav">Navigator pointing to the section that contains a table</param>
        /// <param name="summary"></param>
        /// <param name="sectionID">Section ID of the table enlarge section</param>
        private void AddTableLinks(XPathNavigator sectionNav, SummaryDocument summary, string sectionID, SummarySectionType sectionType)
        {
            try
            {
                XPathNodeIterator tableSectionIter = sectionNav.Select(".//TableSectionXML");

                while (tableSectionIter.MoveNext())
                {
                    string tableSectionID = string.Empty;
                    XPathNavigator subSectionNav = tableSectionIter.Current.SelectSingleNode(".//a[starts-with(@name, 'SectionXML_')]");
                    if (subSectionNav != null && subSectionNav.HasAttributes)
                    {
                        tableSectionID = subSectionNav.GetAttribute("name", string.Empty).Replace("SectionXML_", string.Empty);
                        string tableSectionXml = tableSectionIter.Current.InnerXml;

                        if (sectionType == SummarySectionType.Reference || sectionType == SummarySectionType.SummarySection)
                        {
                            // Add "small" classes to the elements in the source table...
                            tableSectionXml = Regex.Replace(tableSectionXml, "class=\"SummaryRef\"", "Class=\"SummaryRef-Small\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline); //SummaryRef
                            tableSectionXml = Regex.Replace(tableSectionXml, "class=\"Summary-ProtocolRef\"", "Class=\"Summary-ProtocolRef-Small\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline); //ProtocolRef
                            tableSectionXml = Regex.Replace(tableSectionXml, "class=\"Summary-LOERef\"", "Class=\"Summary-LOERef-Small\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline); //Summary-LOERef
                            tableSectionXml = Regex.Replace(tableSectionXml, "class=\"Summary-GlossaryTermRef\"", "Class=\"Summary-GlossaryTermRef-Small\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline); //GlossaryTermRef
                            tableSectionXml = Regex.Replace(tableSectionXml, "class=\"Protocol-GeneName\"", "Class=\"Protocol-GeneName-Small\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline); //GeneName
                            tableSectionXml = Regex.Replace(tableSectionXml, "class=\"Protocol-ExternalRef\"", "Class=\"Protocol-ExternalRef-Small\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline); //ExternalRef
                            tableSectionXml = Regex.Replace(tableSectionXml, "<a\\s+href=\"#Reference", "<a Class=\"SummarySection-Table-Small\"  href=\"#Reference", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline); //Reference
                        }

                        // Replace the top level of xml with nodes that will be saved into database
                        tableSectionIter.Current.InnerXml = tableSectionXml;
                        string formatedTableXml = tableSectionIter.Current.InnerXml;
                        if (!formatedTableXml.StartsWith("<a name=\"TableSection\"></a>"))
                        {
                            formatedTableXml = "<a name=\"TableSection\"></a><a name=\"Section_" + tableSectionID + "\"></a>" + formatedTableXml;
                            tableSectionIter.Current.InnerXml = formatedTableXml;
                        }

                        // Get the section title
                        XPathNavigator titleNav = tableSectionIter.Current.SelectSingleNode(".//table/span/b");
                        // Set the section title
                        string title = string.Empty;
                        if (titleNav != null)
                        {
                            title = titleNav.InnerXml.Trim();
                            title = title.Replace("Protocol-GeneName-Small", "Protocol-GeneName");
                            titleNav.InnerXml = title;
                        }
                    }
                }  // End while loop

                // Clean up the TableSectionXML tags.
                // TODO: Ideally, this would be done in the same loop as above.
                // Editing the current node of an XPathNodeIterator loses position,
                // so we have to search for the node repeatedly.
                List<XPathNavigator> tableSectionNodes = new List<XPathNavigator>();
                tableSectionIter = sectionNav.Select(".//TableSectionXML");
                while (tableSectionIter.MoveNext())
                {
                    tableSectionIter.Current.OuterXml = tableSectionIter.Current.InnerXml;
                    tableSectionIter = sectionNav.Select(".//TableSectionXML");
                }

            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Adding table link failed. SectionID = " + sectionID.ToString(), e);
            }
        }

        #endregion Enlarged Table Reference Handling Methods

        /// <summary>
        /// Build table of contents.
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="section"></param>
        private void BuildTOC(SummaryDocument summary, SummarySection currentSection)
        {
            try
            {
                // Build TOC for each level 1 (top level) section (only based on levels 2 and 3 => ignore other levels)
                switch (currentSection.Level)
                {
                    case 1:
                        if (this._lastTopLevelSection != null)
                        {
                            // Find last top level section and set it's TOC equal to the current TOC 
                            // (top-level sections TOCs are comprised of all 2nd and 3rd level sections under it)
                            this._lastTopLevelSection.TOC = this._currentSectionTOC.ToString();
                            this._currentSectionTOC = new StringBuilder();
                        }
                        // Keep track of the current top level section, so the TOC can be saved off when we encounter
                        // the next top level section
                        this._lastTopLevelSection = currentSection;
                        break;
                    case 2:
                        if (!currentSection.IsTableSection && currentSection.Title.Trim().Length > 0)
                        {
                            this._currentSectionTOC.Append(string.Format("<a href=\"#Section_{0}\">{1}</a><br>\n",
                                currentSection.SectionID, currentSection.Title));
                        }
                        break;
                    case 3:
                        if (currentSection.Title.Trim().Length > 0)
                        {
                            this._currentSectionTOC.Append(
                            string.Format("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"#Section_{0}\">{1}</a><br>\n",
                                currentSection.SectionID, currentSection.Title));
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Building table of contents failed.", e);
            }
        }

        /// <summary>
        /// Method to extract level 5 elements (OLs, ULs, Ps, PREs...etc).
        /// </summary>
        /// <param name="level5Iter"></param>
        /// <param name="idAttribute"></param>
        /// <param name="summary"></param>
        /// <param name="parentSectionID"></param>
        /// <remarks>Note: priority is not set for the level 5 sections, 
        /// this must be set prior to saving to the database</remarks>
        private void ExtractLevel5Elements(XPathNodeIterator level5Iter, string idAttribute, SummaryDocument summary, Guid parentSectionID, ref int priority)
        {
            // HACK: This method processes rendered level 5 elements (post XSL) by 
            // looking for any element that has an __id attribute
            // TODO: Refactor extraction to parse out the elements
            // Reset the priority for the previous added items
            if (summary.Level5SectionList.Count > 0)
            {
                foreach (SummarySection sect in summary.Level5SectionList)
                {
                    sect.Priority = priority++;
                }
            }
            while (level5Iter.MoveNext())
            {
                string ID = DocumentHelper.GetAttribute(level5Iter.Current, idAttribute).Trim();
                if (ID.Length > 0)
                {
                    /* The unmodified section ID is passed as the sectID parameter, but
                     * a modified version is used in creating the sectionTitle.  See the
                     * notes accompanying the SummarySection.RawID and SummarySection.SectionID
                     * properties for details. */
                    string trimmedID = (string.IsNullOrEmpty(ID) ? string.Empty : ID.Substring(1));
                    summary.AddLevel5Section(ID, "Reference " + trimmedID,
                        parentSectionID, SummarySectionType.Reference, priority++);
                }
            }
        }

        /// <summary>
        /// Method to extract media links and render links into html output format
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="summary"></param>
        [Obsolete("Don't use this method! MediaLink data is now being extracted in (duh!) Extract.")]
        private void BuildMediaLink(XPathNavigator xNav, SummaryDocument summary)
        {
            try
            {
                XPathNavigator mediaLink = xNav.SelectSingleNode(".//MediaLink");
                if (mediaLink != null)
                {
                    string mediaLinkID = DocumentHelper.GetAttribute(mediaLink, "id");
                    string thumb = DocumentHelper.GetAttribute(mediaLink, "thumb");
                    bool isThumb = (thumb.ToUpper() == "YES") ? true : false;
                    string imgRef = DocumentHelper.GetAttribute(mediaLink, "ref");
                    string alt = DocumentHelper.GetAttribute(mediaLink, "alt");
                    bool isInline = false;
                    string inLine = DocumentHelper.GetAttribute(mediaLink, "inline");
                    if (inLine.ToUpper().Equals("YES"))
                        isInline = true;
                    string width = DocumentHelper.GetAttribute(mediaLink, "MinWidth");
                    long minWidth = -1;
                    if ((width != null) && (width.Length > 0))
                        minWidth = long.Parse(width);
                    string size = DocumentHelper.GetAttribute(mediaLink, "size");
                    if (size.Equals(String.Empty))
                        size = "not-set";

                    XmlDocument mediaXml = new XmlDocument();
                    mediaXml.LoadXml(mediaLink.OuterXml);

                    XPathNavigator captionNode = mediaLink.SelectSingleNode("./Caption");
                    Language capLang = Language.English;
                    string caption = string.Empty;
                    if (captionNode != null)
                    {
                        capLang = DocumentHelper.DetermineLanguageString(DocumentHelper.GetAttribute(mediaLink, "language"));
                        caption = captionNode.InnerXml;
                        // Check if the media language is the same as the summary language, if not, log an warning
                        if (capLang != summary.Language)
                        {
                            summary.WarningWriter("Media Link Warning: The media link language does not match the language defined in summary! Summary ID=" + summary.DocumentID + " MediaLinkID=" + mediaLinkID + ".");
                        }
                    }
                    // Find media link's parent node
                    bool isInTable = false;
                    bool isInList = false;
                    // Is the media link embeded in table?
                    XPathNavigator tableNode = mediaLink.SelectSingleNode("./parent::td");
                    if (tableNode != null)
                        isInTable = true;
                    // Is the media link embeded in list?
                    XPathNavigator listNode = mediaLink.SelectSingleNode("./parent::LI");
                    if (listNode != null)
                        isInList = true;
                    int cdrId = Int32.Parse(Regex.Replace(imgRef, "^CDR(0*)", "", RegexOptions.Compiled));

                    MediaLink link = new MediaLink(imgRef, cdrId, alt, isInline, minWidth, size, mediaLinkID, caption, summary.DocumentID, capLang, isThumb, mediaXml);
                    string test = "<MediaHTML>" + DocumentRenderHelper.ProcessMediaLink(link, false, isInTable, isInList) + "</MediaHTML>";
                    mediaLink.OuterXml = test;
                    BuildMediaLink(xNav, summary);
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Building media link failed.", e);
            }

        }

        /// <summary>
        /// Return the maximun priority number
        /// </summary>
        /// <param name="summary"></param>
        private int GetMaxPriority(SummaryDocument summary)
        {
            int priority = 0;
            foreach (SummarySection section in summary.SectionList)
            {
                if (section.Priority > priority)
                    priority = section.Priority;
            }
            return priority;

        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Method to pre-render the summary document.
        /// </summary>
        /// <param name="document"></param>
        public override void Render(Document document)
        {
            try
            {
                base.Render(document);

                SummaryDocument summary = (SummaryDocument)document;
                XPathNavigator xNav = summary.PostRenderXml.CreateNavigator();
                _language = summary.Language;
                int priority = GetMaxPriority(summary);

                // Break apart the PostRenderXML document...
                foreach (SummarySection section in summary.SectionList)
                {
                    XPathNavigator sectionNav = xNav.SelectSingleNode(".//a[@name='Section_" + section.SectionID + "']");
                    if (sectionNav != null)
                    {
                        if (section.IsTopLevel)
                        {
                            // Format any tables in the section (Note: These are the 
                            // tables that the table enlarge (handled above) were 
                            // generated from)
                            AddTableLinks(sectionNav, summary, section.SectionID, section.SectionType);

                            // HACK: Need to extract level 5 sections from the HTML 
                            ExtractLevel5Elements(sectionNav.Select(".//*[@__id]"),
                                "__id", summary, section.SummarySectionID, ref priority);
                        }

                        // Assign section title
                        // Move to the next node to get the title information.
                        if (!sectionNav.HasChildren)
                        {
                            sectionNav.MoveToNext();
                            if (sectionNav.Matches("Span") && !section.IsTopLevel)
                            {
                                if (sectionNav.GetAttribute("Class", string.Empty).Contains("Summary-SummarySection-Title") ||
                                    sectionNav.GetAttribute("class", string.Empty).Contains("Summary-SummarySection-Title"))
                                    section.Title = sectionNav.InnerXml;
                            }
                            // Move back to the original position.
                            sectionNav.MoveToPrevious();
                        }
                        else
                        {
                            // HACK:  This is a hack in the absolute worst sense.  By the time a
                            // top-level section gets here, the title isn't present in the rendered
                            // HTML and can't be fished out of the XML to replace any markup in the existing title.
                            // Prior to the WCM rollout, the bad XML was just dumped into the database
                            // and later sent to the browser where it was simply ignored.  That doesn't work
                            // with WCM, Percussion's use of Tidy detects the tag and refuses to save
                            // the document.  So we're left with the hack of stripping out a non-HTML tag
                            // and at least not having it look any worse than before.
                            if (!string.IsNullOrEmpty(section.Title))
                            {
                                section.Title = section.Title.Replace("<GeneName>", string.Empty).Replace("</GeneName>", string.Empty);
                            }
                        }

                        // Build the Table Of Contents for the section
                        BuildTOC(summary, section);

                        if (section.Title.Trim().Length == 0)
                        {
                            section.Title = "Reference " + section.SectionID.ToString();
                            section.Level = 5;
                        }

                        // Save the results of the transform into the Html property
                        section.Html.LoadXml(sectionNav.OuterXml);
                    }
                    else
                    {
                        throw new Exception("Rendering Error: Can not find section ID = " + section.SectionID);
                    }
                }

                // Format table enlarges...
                /*
                 * This is a HUGE hack.  Table sections are extracted in the Extract phase, but rather than rendering the
                 * Table XML, we end up extracting the rendered HTML from the overall rendered summary and load that into the sections.
                 * 
                 * The placeholder DIV (which by rights belongs in the XSL) is substituted for the
                 * rendered HTML immediately after the HTML is copied to the table section.
                 */
                foreach (SummarySection tableSection in summary.TableSectionList)
                {
                    // SectionNav finds the tables in the top-level HTML.
                    XPathNavigator sectionNav = xNav.SelectSingleNode(".//a[@name='SectionXML_" + tableSection.SectionID + "']");
                    if (sectionNav != null)
                    {
                        RewriteAndExtractTableXml(tableSection, sectionNav);
                        //sectionNav.ReplaceSelf(string.Format(tablePlaceholder, tableSection.RawSectionID, tableSection.Title));

                        ReplaceTableInSectionHtml(summary.SectionList, tableSection);
                    }
                }
                // This is wrong.  We need to update the invdividual summary sections, not the top-level HTML.
                summary.Html = xNav.OuterXml;

                /*
                 * 1. Loop through the collection of sections.
                 * 2. Does the section HTML contain a table?
                 * 3. Look up the corresponding table section.
                 * 4. Replace the node with a placeholder.
                 * 
                 */

                RenderMediaLinkCaptions(summary.MediaLinkSectionList);
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Render data from summary document failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }

        /* HACK: This code rewrites the table XML and copies it into the table section. */
        private void RewriteAndExtractTableXml(SummarySection tableSection, XPathNavigator sectionNav)
        {
            // Get the section title
            XPathNavigator titleNav = sectionNav.SelectSingleNode("./table/span/b");


            // Set the section title
            string title = string.Empty;
            if (titleNav != null)
            {
                title = titleNav.InnerXml.Trim();
                title = title.Replace("Protocol-GeneName-Small", "Protocol-GeneName");
            }

            // Format the table enlarge (put into a separate section during extraction)
            string referenceSection;
            ProcessTableForStandalone(sectionNav, out referenceSection);

            // Save the unmodified results of the transform into the Html property
            tableSection.Html.LoadXml(sectionNav.OuterXml);

            // Now modify the HTML to create the version that will be used
            // in the fullsize (standalone) table.
            string html = sectionNav.InnerXml;

            // Remove the two unneed tag from output xml
            html = html.Replace("<a name=\"TableSection\"></a>", string.Empty);
            html = html.Replace("<a name=\"Section_" + tableSection.SectionID + "\"></a>", string.Empty);

            // Change back the name in table section
            html = html.Replace("Class=\"SummarySection-Table-Small\"", string.Empty);
            html = html.Replace("Class=\"SummaryRef-Small\"", "Class=\"SummaryRef\"");
            html = html.Replace("Class=\"Summary-LOERef-Small\"", "Class=\"Summary-LOERef\"");
            html = html.Replace("Class=\"Summary-ProtocolRef-Small\"", "Class=\"Summary-ProtocolRef\"");
            html = html.Replace("Class=\"Summary-GlossaryTermRef-Small\"", "Class=\"Summary-GlossaryTermRef\"");
            html = html.Replace("Class=\"Protocol-ExternalRef-Small\"", "Class=\"Protocol-ExternalRef\"");
            html = html.Replace("Class=\"Protocol-GeneName-Small\"", "Class=\"Protocol-GeneName\"");
            html = "<a name=\"Section_" + tableSection.SectionID + "\">" + html.Trim() + referenceSection + "</a>";
            tableSection.StandaloneHTML.LoadXml(html);

            if (title.Trim().Length > 0)
                tableSection.Title = title;
        }

        private void ReplaceTableInSectionHtml(List<SummarySection> sectionList, SummarySection tableSection)
        {
            foreach (SummarySection section in sectionList)
            {
                if (section.IsTableSection)
                    continue;

                XPathNavigator sectionNav = section.Html.CreateNavigator();
                XPathNavigator tableNav = sectionNav.SelectSingleNode(".//a[@name='SectionXML_" + tableSection.SectionID + "']");
                if (tableNav != null)
                {
                    tableNav.ReplaceSelf(string.Format(tablePlaceholder, tableSection.RawSectionID, tableSection.Title));
                }

            }
        }

        private void RenderMediaLinkCaptions(List<MediaLink> mediaLinkCollection)
        {
            // HACK:  What we're doing here is running an XSL transform for each MediaLink's
            // caption using the stylesheet which is already in memory for Summary documents.

            foreach (MediaLink item in mediaLinkCollection)
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new System.IO.StringWriter(sb);
                
                // Create temporary XML document to transform with the loaded XSL.
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(string.Format("<Para id=\"InternalUse\">{0}</Para>", item.Caption));
                Render(doc.CreateNavigator(), sw);

                // Reload temp doc with the output from the transform.
                doc.LoadXml(sw.ToString());

                // Replace item caption with the transformed version.
                XmlNode caption = doc.SelectSingleNode("//P[@__id='InternalUse']");
                item.Caption = caption.InnerXml;
            }
        }

        #endregion Public Methods
    }
}
