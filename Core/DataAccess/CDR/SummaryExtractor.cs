using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using GateKeeper.Common;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Class to extract summary object from XML.
    /// </summary>
    public class SummaryExtractor : DocumentExtractor
    {
        #region fields
        private int _documentID = 0;
        #endregion

        #region Private Methods

        #region Extraction

        /// <summary>
        /// Extracts all supported summary metadata.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="summary"></param>
        private void ExtractMetadata(XPathNavigator xNav, SummaryDocument summary, DocumentXPathManager xPathManager)
        {
            string path = string.Empty;
            try
            {
                // Main Summary URL.
                path = xPathManager.GetXPath(SummaryXPath.URL);
                XPathNavigator prettyUrlNav = xNav.SelectSingleNode(path);
                string basePrettyURL = prettyUrlNav.GetAttribute(xPathManager.GetXPath(SummaryXPath.BasePrettyURL), string.Empty).Trim();
                if (basePrettyURL.EndsWith("/"))
                {
                    basePrettyURL = basePrettyURL.Substring(0, basePrettyURL.Length - 1);
                }
                summary.BasePrettyURL = basePrettyURL;
                summary.ValidOutputDevices.Add(TargetedDevice.screen);  // Always valid for web.

                // Mobile URL.  (Optional element)
                path = xPathManager.GetXPath(SummaryXPath.MobileURL);
                XPathNavigator mobileUrlNav = xNav.SelectSingleNode(path);
                if (mobileUrlNav != null)
                {
                    string baseMobileURL = mobileUrlNav.GetAttribute(xPathManager.GetXPath(SummaryXPath.BaseMobileURL), string.Empty).Trim();
                    if (string.IsNullOrEmpty(baseMobileURL))
                    {
                        throw new Exception("Extraction Error: MobileURL node is present, but contains no value.");
                    }

                    if (baseMobileURL.EndsWith("/"))
                    {
                        baseMobileURL = baseMobileURL.Substring(0, baseMobileURL.Length - 1);
                    }
                    summary.BaseMobileURL = baseMobileURL;
                    summary.ValidOutputDevices.Add(TargetedDevice.mobile);
                }

                path = xPathManager.GetXPath(SummaryXPath.Title);
                summary.Title = DocumentHelper.GetXmlDocumentValue(xNav, path);
                // Get Summary short title
                path = xPathManager.GetXPath(SummaryXPath.AlterTitle);
                string alterTitleType = DocumentHelper.GetAttribute(xNav, path, xPathManager.GetXPath(SummaryXPath.TitleType));
                if (alterTitleType.ToUpper() == "SHORT")
                {
                    path = xPathManager.GetXPath(SummaryXPath.AlterTitle);
                    summary.ShortTitle = DocumentHelper.GetXmlDocumentValue(xNav, path);
                }
                else
                {
                    throw new Exception("Extraction Error: Summary short name is not defined in XPath " + xPathManager.GetXPath(SummaryXPath.AlterTitle) + "/@" + xPathManager.GetXPath(SummaryXPath.TitleType) + ". Document CDRID= " + _documentID.ToString());
                }

                path = xPathManager.GetXPath(SummaryXPath.Type);
                summary.Type = DocumentHelper.GetXmlDocumentValue(xNav, path);
                path = xPathManager.GetXPath(SummaryXPath.Descript);
                summary.Description = DocumentHelper.GetXmlDocumentValue(xNav, path);
                path = xPathManager.GetXPath(SummaryXPath.Lang);
                summary.Language = DocumentHelper.DetermineLanguageString(DocumentHelper.GetXmlDocumentValue(xNav, path));
                path = xPathManager.GetXPath(SummaryXPath.Audience);
                summary.AudienceType = DocumentHelper.GetXmlDocumentValue(xNav, path);

                string tempReplacementForID = DocumentHelper.GetAttribute(xNav, ".", xPathManager.GetXPath(SummaryXPath.Replacement));
                int replacementForID = 0;
                if (tempReplacementForID.Length > 0)
                {
                    if (Int32.TryParse(CDRHelper.ExtractCDRID(tempReplacementForID), out replacementForID))
                    {
                        summary.ReplacementForID = replacementForID;
                    }
                    else
                    {
                        throw new Exception("Extraction Error: Attribute " + xPathManager.GetXPath(SummaryXPath.Replacement) + " should be a valid CDRID. CurrentValue=" + tempReplacementForID + ". Document CDRID= " + _documentID.ToString());
                    }
                }


                // Get Permanent Links in Summary Meta Data
                path = xPathManager.GetXPath(SummaryXPath.PermanentLinkList);
                XPathNavigator permanentLinkNav = xNav.SelectSingleNode(path);

                // Document must have encasing list to consider further extraction
                if (permanentLinkNav != null)
                {
                    // Only picks up items in the encasing list
                    XPathNodeIterator nodeIter = permanentLinkNav.SelectChildren(XPathNodeType.Element);

                    // Data that we will extract from the XML
                    string id, sectionID, sectionTitle;

                    // XPath Strings for extraction
                    string idPath = xPathManager.GetXPath(SummaryXPath.PermanentLinkID);
                    string sectionIDPath = xPathManager.GetXPath(SummaryXPath.PermanentLinkTargetID);
                    string sectionTitlePath = xPathManager.GetXPath(SummaryXPath.PermanentLinkTitle);

                    // Keep track of IDs so we can reject duplicates
                    List<string> linkIDs = new List<string>();

                    // Keep track of duplicates so we can provide a detailed exception
                    List<PermanentLink> duplicates = new List<PermanentLink>();

                    // For each Permanent Link found in the XML
                    while (nodeIter.MoveNext())
                    {
                        // Ensures that each item in encasing list (PermanentLinkList) is the right type (PermanentLink)
                        // Not grouped with "while" loop, so bad info can be ignored and document can continue
                        path = xPathManager.GetXPath(SummaryXPath.PermanentLink);
                        if (nodeIter.Current.LocalName.Equals(path))
                        {
                            // Values that the CDR team gives us
                            id = null;
                            sectionID = null;
                            sectionTitle = null;

                            // Only attempt to get information from Permanent Link if it has attributes
                            if (nodeIter.Current.HasAttributes)
                            {
                                /*
                                 * Please keep until updated DTD for 6.5
                                 * 
                                // Removes the beginning of the line _ for sections
                                Regex regex = new Regex("^_", RegexOptions.IgnoreCase);

                                id = nodeIter.Current.GetAttribute("id", string.Empty).Trim();
                                id = regex.Replace(id, "", 1); // Replace leading _
                                sectionID = nodeIter.Current.GetAttribute("PermaTargId", string.Empty).Trim();
                                sectionID = regex.Replace(sectionID, "", 1);// Replace leading _
                                sectionTitle = nodeIter.Current.GetAttribute("PermaTargTitle", string.Empty).Trim();
                                */

                                id = nodeIter.Current.GetAttribute(idPath, string.Empty).Trim();
                                sectionID = nodeIter.Current.GetAttribute(sectionIDPath, string.Empty).Trim();
                                sectionTitle = nodeIter.Current.GetAttribute(sectionTitlePath, string.Empty).Trim();

                                // If CDR team does not give us all of the required info about the 
                                // PermanentLink, reject it and throw a Null Error
                                if (id != null && sectionID != null && sectionTitle != null)
                                {
                                    // If we have already encountered this link ID, collect it so we can
                                    // throw a detailed exception
                                    if (linkIDs.Contains(id))
                                    {
                                        // Add new incoming link to Duplicates list
                                        duplicates.Add(new PermanentLink(id, sectionID, sectionTitle));
                                        // Add other instance to Duplicates list
                                        duplicates.Add(summary.PermanentLinkList.Find(link => link.ID == id));
                                    }
                                    else
                                    {
                                        // Add link ID to set of link IDs so we can check for duplicates
                                        linkIDs.Add(id);

                                        // Add link to Permanent Link List
                                        summary.PermanentLinkList.Add(new PermanentLink(id, sectionID, sectionTitle));
                                    }

                                }
                                else
                                {
                                    throw new NullReferenceException("Permanent Link contains null components.");
                                }
                            }
                        }
                    }

                    // If there were any duplicate Permanent Links found, throw a detailed exception
                    if (duplicates.Count > 0)
                    {
                        throw new Exception("Permanent Link Duplicates Found: " + duplicates.Count + " Permanent Links with the IDs [" + string.Join(",", duplicates.ConvertAll(duplicateLink => duplicateLink.ID).ToArray()) + "] linked to the Sections [" + string.Join(",", duplicates.ConvertAll(duplicateLink => duplicateLink.SectionID).ToArray()) + "]");
                    }

                } // This bracket ends the Permanent Link section
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extracts summary relations (currently patient and spanish versions).
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="summary"></param>
        private void ExtractRelations(XPathNavigator xNav, SummaryDocument summary, DocumentXPathManager xPathManager)
        {
            // This is used to track error
            string path = xPathManager.GetXPath(SummaryXPath.PatientVersion);
            try
            {
                // Handle summary relations...
                int patientVersionOfID = 0;

                string relationCDRID = DocumentHelper.GetAttribute(xNav.SelectSingleNode(path), xPathManager.GetXPath(SummaryXPath.RelationID));
                if (relationCDRID.Length > 0)
                {
                    if (Int32.TryParse(CDRHelper.ExtractCDRID(relationCDRID), out patientVersionOfID))
                    {
                        summary.RelationList.Add(new SummaryRelation(patientVersionOfID, SummaryRelationType.PatientVersionOf));
                    }
                    else
                    {
                        throw new Exception("Extraction Error: " + path + " should be a valid CDRID. CurrentValue=" + relationCDRID + ". Document CDRID= " + _documentID.ToString());
                    }
                }

                int spanishVersionOfID = 0;
                path = xPathManager.GetXPath(SummaryXPath.Translation);
                string tempSpanishVersionID = DocumentHelper.GetAttribute(xNav.SelectSingleNode(path), xPathManager.GetXPath(SummaryXPath.RelationID));
                if (tempSpanishVersionID.Length > 0)
                {
                    if (Int32.TryParse(CDRHelper.ExtractCDRID(tempSpanishVersionID), out spanishVersionOfID))
                    {
                        summary.RelationList.Add(new SummaryRelation(spanishVersionOfID, SummaryRelationType.SpanishTranslationOf));
                    }
                    else
                    {
                        throw new Exception("Extraction Error: " + path + " should be a valid CDRID. CurrentValue=" + tempSpanishVersionID + ". Document CDRID= " + _documentID.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract all summary references.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="summary"></param>
        private void ExtractSummaryReferences(XPathNavigator xNav, SummaryDocument summary, DocumentXPathManager xPathManager)
        {
            string path = string.Empty;
            try
            {
                // Handle PrettyUrlMap 
                path = xPathManager.GetXPath(SummaryXPath.Ref);
                XPathNodeIterator nodeIter = xNav.Select(path);
                while (nodeIter.MoveNext())
                {
                    if (nodeIter.Current.HasAttributes)
                    {
                        // Look up the reference ID and URL.
                        // TODO: Set up the "url" attribute in xPathManager.
                        string id = nodeIter.Current.GetAttribute(xPathManager.GetXPath(SummaryXPath.PrettyURLHref), string.Empty).Trim();
                        string url = nodeIter.Current.GetAttribute("url", string.Empty).Trim();
                        if (!summary.SummaryReferenceMap.ContainsKey(id))
                        {
                            //summary.SummaryReferenceMap.Add(id, string.Empty);
                            SummaryReference reference = new SummaryReference(id, url);
                            summary.SummaryReferenceMap.Add(id, reference);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extracts a summary section from the xpath navigator parameter,
        /// assigns and increments the display priority and determines the 
        /// section level.
        /// </summary>
        /// <param name="sectionNav"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        private SummarySection ExtractSection(XPathNavigator sectionNav, DocumentXPathManager xPathManager, ref int priority)
        {
            SummarySection summarySection = new SummarySection();
            try
            {
                if (sectionNav.HasAttributes)
                {
                    /* The unmodified section ID is used here.  See the notes accompanying the
                     * SummarySection.RawID and legacy SummarySection.SectionID properties for details. */
                    summarySection.RawSectionID = sectionNav.GetAttribute(xPathManager.GetXPath(CommonXPath.CDRID), string.Empty).Trim();
                }

                summarySection.SummarySectionID = Guid.NewGuid();
                summarySection.Text = sectionNav.Value;
                summarySection.Xml.LoadXml(sectionNav.OuterXml);

                // Find all the internal nodes which this and other documents
                // might link to.
                XmlNodeList linkableNodeList = summarySection.Xml.SelectNodes("//*[@id]");
                foreach (XmlNode node in linkableNodeList)
                {
                    summarySection.LinkableNodeRawIDList.Add(node.Attributes["id"].Value);
                }

                XPathNavigator titleNode = sectionNav.SelectSingleNode("./Title");
                string title = string.Empty;
                if (titleNode != null)
                {
                    title = titleNode.InnerXml.Replace("<Subscript>", "<sub>");
                    title = title.Replace("</Subscript>", "</sub>");
                    title = title.Replace("<Emphasis>", "<i>");
                    title = title.Replace("</Emphasis>", "</i>");
                }
                summarySection.Title = title.Trim();
                summarySection.SectionType = SummarySectionType.SummarySection;
                summarySection.Level = DocumentHelper.GetLevel(sectionNav);
                summarySection.Priority = priority;
                priority++;
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting section failed.  Document CDRID=" + _documentID.ToString(), e);
            }

            return summarySection;
        }



        /// <summary>
        /// Method to extract tables and make separate "table enlarge" sections.
        /// </summary>
        /// <param name="sectionNav">Navigator pointing to the section that contains the table(s)</param>
        /// <param name="summary">The summary document object</param>
        /// <param name="parentSectionID">The section ID (Guid) of the parent section</param>
        /// <param name="priority">Display order/priority</param>
        /// <param name="tableID">Identifier for each table (used as part of the pretty URL for each table enlarge section)</param>
        private void ExtractTableSections(XPathNavigator sectionNav, SummaryDocument summary, Guid parentSectionID, TargetedDevice device, DocumentXPathManager xPathManager, int sectionPriority, ref int tableID)
        {
            string path = xPathManager.GetXPath(SummaryXPath.SectTable, device);
            try
            {
                // Extract tables and create separate sections for them. 
                // These sections will be the "enlarged" version of the 
                // table which gets it's own pretty URL and entry in the 
                // SummarySection table.
                XPathNodeIterator tableIter = sectionNav.Select(path);
                int priority = 0;

                while (tableIter.MoveNext())
                {
                    SummarySection tableSection = ExtractSection(tableIter.Current, xPathManager, ref priority);

                    tableSection.IsTableSection = true;
                    tableSection.SectionType = SummarySectionType.Table;
                    tableSection.ParentSummarySectionID = parentSectionID;
                    tableSection.Level = 2; // Tables are always level 2
                    tableSection.PrettyUrl = summary.BasePrettyURL + "/Table" + tableID;
                    //TODO: This formular is from the old code, need to investigate how the table priority is used the decide
                    // if we need to modify the formular
                    tableSection.Priority = sectionPriority * 1000 + priority;
                    summary.PrettyUrlMap.Add(tableSection.SectionID, tableSection.PrettyUrl);

                    // Add to the summary section list...
                    summary.TableSectionList.Add(tableSection);
                    tableID++;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        private void ExtractMediaLink(XPathNavigator mediaLink, SummaryDocument summary)
        {
            if (mediaLink != null)
            {
                string mediaLinkID = DocumentHelper.GetAttribute(mediaLink, "id");
                string thumb = DocumentHelper.GetAttribute(mediaLink, "thumb");
                bool isThumb = (thumb.ToUpper() == "YES") ? true : false;
                string imgRef = DocumentHelper.GetAttribute(mediaLink, "ref");
                int cdrId = Int32.Parse(Regex.Replace(imgRef, "^CDR(0*)", "", RegexOptions.Compiled));

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
                    size = "half";

                // Get the MimeType of the Media.
                string type = DocumentHelper.GetAttribute(mediaLink, "type");

                XmlDocument mediaXml = new XmlDocument();
                mediaXml.PreserveWhitespace = true;
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
                // Find media link's parent node. Is the media link embeded in table?
                bool showEnlargeLink = true;
                XPathNavigator tableNode = mediaLink.SelectSingleNode("./ancestor::Table");
                if (tableNode != null)
                    showEnlargeLink = false;

                MediaLink link = new MediaLink(imgRef, cdrId, alt, isInline, showEnlargeLink, minWidth, size, mediaLinkID, caption, summary.DocumentID, capLang, isThumb, type, mediaXml);
                summary.MediaLinkSectionList.Add(link);
            }
        }

        /// <summary>
        /// Extracts top-level summary sections from the source document.
        /// Note: This method calls ExtractTableSections(), ExtractSubSection() and
        /// ExtractMediaLink() as well.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="summary"></param>
        private void ExtractTopLevelSections(XPathNavigator xNav, SummaryDocument summary, DocumentXPathManager xPathManager, TargetedDevice device)
        {
            string path = xPathManager.GetXPath(SummaryXPath.TopSection, device);

            try
            {
                int pageNumber = 1;
                int tableID = 1;
                int priority = 1;

                // Handle top-level summary sections
                XPathNodeIterator sectionIter = xNav.Select(path);
                int topSectionPriority = 0;
                while (sectionIter.MoveNext())
                {
                    SummarySection topLevelSection = ExtractSection(sectionIter.Current, xPathManager, ref priority);

                    // Validate for items required in top-level sections.
                    if (string.IsNullOrEmpty(topLevelSection.Title.Trim()))
                    {
                        String fmt = "Top-level section {0} is missing a required Title.";
                        String msg = String.Format(fmt, topLevelSection.RawSectionID);
                        throw new MissingElementException(msg);
                    }

                    // Build pretty URL...
                    topLevelSection.PrettyUrl = summary.BasePrettyURL + "/Page" + pageNumber++; // increment page number for the next top-level section
                    summary.SectionList.Add(topLevelSection);

                    // Add to the summary's pretty URL list...
                    summary.PrettyUrlMap.Add(topLevelSection.SectionID, topLevelSection.PrettyUrl);

                    // Catch the top level section priority
                    if (topLevelSection.Level == 1)
                        topSectionPriority = topLevelSection.Priority;

                    // Extract tables from the top-level section (if there are any)
                    ExtractTableSections(sectionIter.Current, summary, topLevelSection.SummarySectionID, device, xPathManager, topSectionPriority, ref tableID);

                    // Handle sub-sections
                    path = xPathManager.GetXPath(SummaryXPath.SubSection, device);

                    XPathNodeIterator subSectionIter = sectionIter.Current.Select(path);
                    SummarySection secondLevelSection = new SummarySection();
                    SummarySection thirdLevelSection = new SummarySection();
                    while (subSectionIter.MoveNext())
                    {
                        SummarySection subSection = ExtractSection(subSectionIter.Current, xPathManager, ref priority);
                        subSection.ParentSummarySectionID = topLevelSection.SummarySectionID;
                        XPathNavigator nav = subSection.Xml.CreateNavigator();
                        // Check if the second level is a parent section.
                        XPathNavigator nextLevel = nav.SelectSingleNode(xPathManager.GetXPath(SummaryXPath.Section));
                        if (nextLevel != null && subSection.Level == 2 && subSection.Title.Trim() != string.Empty)
                            secondLevelSection = subSection;

                        if (subSection.Level == 3 && secondLevelSection.ParentSummarySectionID != Guid.Empty && subSection.Title.Trim().Length > 0)
                            subSection.ParentSummarySectionID = secondLevelSection.SummarySectionID;

                        if (subSection.Level >= 4)
                        {
                            subSection.Level = 4;
                            if (subSection.Title.Trim().Length > 0)
                                summary.Level4SectionList.Add(subSection);
                            else
                            {
                                subSection.Title = "Reference " + subSection.SectionID;
                                subSection.Level = 5;
                                summary.Level5SectionList.Add(subSection);
                            }
                        }
                        else
                        {
                            summary.SectionList.Add(subSection);
                        }
                    }

                    // Handle media link reference id, apparently it is save as summary section level 5 in gatekeeper database
                    string mediaLinkPath = xPathManager.GetXPath(SummaryXPath.MediaLink, device);
                    XPathNavigator topSectionNav = topLevelSection.Xml.CreateNavigator();
                    XPathNodeIterator mediaLinkIter = topSectionNav.Select(mediaLinkPath);
                    while (mediaLinkIter.MoveNext())
                    {
                        XPathNavigator mediaLink = mediaLinkIter.Current;

                        /* The unmodified section ID is used here.  See the notes accompanying the
                         * SummarySection.RawID and SummarySection.SectionID properties for details. */
                        string mediaLinkID = DocumentHelper.GetAttribute(mediaLink, xPathManager.GetXPath(CommonXPath.CDRID));
                        summary.AddLevel5Section(mediaLinkID, "Reference " + mediaLinkID, topLevelSection.SummarySectionID, SummarySectionType.Reference, 0);

                        ExtractMediaLink(mediaLink, summary);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        #endregion Extraction

        #region Permanent Link Verification

        /// <summary>
        /// Determines if all Permanent Link targets even exist as a section.
        /// </summary>
        /// <param name="summary">Summary document needed for SectionList and PermanentLink Targets.</param>
        private void VerifyPermanentLinkSections(SummaryDocument summary)
        {
            List<String> sectionsByIDs = summary.SectionList.ConvertAll(new Converter<SummarySection, String>(SummarySection.SectionByID));
            List<String> permanentLinkTargetNonExistant = new List<String>();
            foreach (PermanentLink possibility in summary.PermanentLinkList)
            {
                if (!sectionsByIDs.Contains(possibility.SectionID))
                {
                    permanentLinkTargetNonExistant.Add(possibility.SectionID);
                }
            }

            if (permanentLinkTargetNonExistant.Count > 0)
            {
                throw new Exception("Extraction Error: Permanent Link Target Section(s) not found in summary document: ['" + string.Join("', '", permanentLinkTargetNonExistant.ToArray()) + "']");
            }
        }


        /// <summary>
        /// Verifies that there are no Permanent Links to device specific sections
        /// </summary>
        /// <param name="deviceSpecificSectionNav">Navigator pointing to the section that contains the device-specific attributes.</param>
        /// <param name="summary">The Summary is needed so that this may be compared to Permanent Links (or whatever in the future).</param>
        /// <param name="xPathManager"></param>
        private void VerifyPermanentLinkDeviceSpecificSections(XPathNavigator xNav, SummaryDocument summary, DocumentXPathManager xPathManager)
        {
            string path = string.Empty;
            HashSet<String> sectionsWithSpecificDevices = new HashSet<String>(); // Sections with specific devices
            try
            {
                // Access the path that determines whether the attribute "IncludedDevice" or "ExcludedDevice"
                // exists in a SummarySection
                path = xPathManager.GetXPath(SummaryXPath.SummarySectionDeviceSpecific);
                XPathNodeIterator nodeIter = xNav.Select(path);

                // For each section that has the attribute IncludedDevice or ExcludedDeivce
                while (nodeIter.MoveNext())
                {
                    // Double check to make sure the found SummarySection has attributes
                    if (nodeIter.Current.HasAttributes)
                    {
                        // Determine the id of this section; copied from ExtractSection (same idea/target)
                        string id = nodeIter.Current.GetAttribute(xPathManager.GetXPath(CommonXPath.CDRID), string.Empty).Trim();

                        // Add this sections ID to the list of sections with specific devices
                        sectionsWithSpecificDevices.Add(id);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }

            // Determine if any of the Sections that were Device-Specific are linked to by PermanentLinks
            List<String> permanentLinkSectionsWithExclusions = new List<String>();
            foreach (PermanentLink permanentLink in summary.PermanentLinkList)
            {
                // If the PermanentLink has a section that is marked as device specific, add it to the list
                if (sectionsWithSpecificDevices.Contains(permanentLink.SectionID))
                {
                    permanentLinkSectionsWithExclusions.Add(permanentLink.SectionID);
                }
            }
            // If there is at least one PermanentLink to a section that is device specific, we must throw an error
            if (permanentLinkSectionsWithExclusions.Count > 0)
            {
                throw new Exception("Extraction Error: Permanent Link Target Section must be found on every targeted device. Sections that have ExcludedDevices and IncludedDevices attributes: ['" + string.Join("', '", permanentLinkSectionsWithExclusions.ToArray()) + "']");
            }
        }

        #endregion

        #region Reference Formatting

        /// <summary>
        /// Formats groups of continuous and discontinuous references.
        /// </summary>
        /// <param name="referenceNodeList"></param>
        /// <returns></returns>
        private string FormatReferences(List<XmlNode> referenceNodeList, DocumentXPathManager xPathManager)
        {
            // Buffer that contains the re-formatted references
            StringBuilder nodeBuffer = new StringBuilder("[");
            try
            {
                // List to hold nodes in a sequence until we have determined it is continuous or not
                List<XmlNode> tempNodeList = new List<XmlNode>();
                // refidx attributes of the current and previous (last reference) for comparison
                int currentRefID = 0, lastRefID = 0;
                // Indicates that the sequence now in progress is numerically continuous
                bool isContinuous = false;

                foreach (XmlNode referenceNode in referenceNodeList)
                {
                    // Get refidx from the current node...
                    int.TryParse(referenceNode.Attributes[xPathManager.GetXPath(SummaryXPath.ReferenceID)].InnerText, out currentRefID);

                    if (lastRefID != 0) // Make sure we aren't on the first node of the sequence
                    {
                        if (currentRefID == (lastRefID + 1)) // Are the current and last nodes numerically continuous?
                        {
                            // Possible continuous sequence (1,2,3...etc) encountered
                            isContinuous = true;
                            tempNodeList.Add(referenceNode); // save nodes in a temporary list
                        }
                        else
                        {
                            if (isContinuous)
                            {
                                // The continuous sequence we have been processing is now broken, so 
                                // "dump" the sequence to the buffer 
                                DumpReferenceList(tempNodeList, nodeBuffer, isContinuous);
                                tempNodeList.Clear();
                            }

                            // Discontinuous sequence encountered, so prepend a comma
                            nodeBuffer.Append(",");
                            nodeBuffer.Append(referenceNode.OuterXml);

                            // Indicate that we are currently working on a discontinuous 
                            // sequence of references, until we test the next node
                            isContinuous = false;
                        }
                    }
                    else
                    {
                        // First node of the sequence of references encountered, 
                        // so throw it in the buffer...
                        nodeBuffer.Append(referenceNode.OuterXml);
                    }

                    // Save the current reference id for comparison to the next reference id...
                    lastRefID = currentRefID;
                }

                // Format any remaining references in the temporary list to the final buffer
                DumpReferenceList(tempNodeList, nodeBuffer, isContinuous);
                nodeBuffer.Append("]"); // End the sequence
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Formatting summary reference failed.  Document CDRID=" + _documentID.ToString(), e);
            }

            return nodeBuffer.ToString();
        }

        /// <summary>
        /// Method to extract a group of sibling reference nodes. 
        /// (<Reference refidx="1"/><Reference refidx="2"/>...etc).
        /// </summary>
        /// <param name="referenceNav"></param>
        /// <returns></returns>
        private List<XmlNode> GetReferenceNodeList(XPathNavigator referenceNav)
        {
            List<XmlNode> referenceNodeList = new List<XmlNode>();
            try
            {
                if (referenceNav != null)
                {
                    if (referenceNav.Name == "Reference")
                    {
                        // Add the reference to the list of reference nodes
                        referenceNodeList.Add(((IHasXmlNode)referenceNav).GetNode());
                    }

                    while (referenceNav.MoveToNext())
                    {
                        if (referenceNav.Name == "Reference")
                        {
                            // Add the reference to the list of reference nodes
                            referenceNodeList.Add(((IHasXmlNode)referenceNav).GetNode());
                        }
                        else
                        {
                            if (referenceNav.NodeType != XPathNodeType.Whitespace)
                            {
                                // If we have encountered a non-whitespace node that 
                                // isn't a reference, end the loop:
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting summary reference node failed.  Document CDRID=" + _documentID.ToString(), e);
            }
            return referenceNodeList;
        }

        /// <summary>
        /// Replaces existing references with re-formatted XML string.
        /// </summary>
        /// <param name="referencesXml"></param>
        /// <param name="firstNode"></param>
        /// <param name="lastNode"></param>
        private void ReplaceNodes(string referencesXml, XmlNode firstNode, XmlNode lastNode)
        {
            try
            {
                // Insert new reformatted references before the first reference node
                firstNode.CreateNavigator().InsertBefore(referencesXml);
                // Delete the old unformatted references 
                firstNode.CreateNavigator().DeleteRange(lastNode.CreateNavigator());
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Replacing summary reference node failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Dumps the list of references into the XML string buffer with appropriate delimiters.
        /// </summary>
        /// <param name="tempNodeList"></param>
        /// <param name="nodeBuffer"></param>
        /// <param name="isContinuous"></param>
        private void DumpReferenceList(List<XmlNode> tempNodeList, StringBuilder nodeBuffer, bool isContinuous)
        {
            try
            {
                // take the first element and the last element and make sure 
                // there are more than 2 and it is a continuous sequence
                if (tempNodeList.Count >= 2 && isContinuous)
                {
                    // format the continuous sequence ([1-4]):
                    nodeBuffer.Append("-");
                    // Grab the last reference node in the group to finish the range (-4])
                    // Note: The first node is already in the buffer
                    nodeBuffer.Append(tempNodeList[tempNodeList.Count - 1].OuterXml);
                }
                else
                {
                    // Format the discontinuous sequence ([1,5,9]): 
                    for (int i = 0; i < tempNodeList.Count; i++)
                    {
                        nodeBuffer.Append(","); // pre-pend comma to delimit existing sequence
                        nodeBuffer.Append(tempNodeList[i].OuterXml);
                        if (i != (tempNodeList.Count - 1))
                        {
                            // If we are processing the last node in the list,
                            // suppress the trailing comma...
                            nodeBuffer.Append(",");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Processing summary reference node failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        #endregion Reference Formatting

        #endregion

        #region Public Methods

        /// <summary>
        /// Modifies the document XML, so that subsequent processing is based on
        /// ideal input.
        /// </summary>
        /// <param name="xmlDoc">Source Summary XML document</param>
        /// <remarks>This function modifies the summary XML to combine adjacent Reference nodes. (see example).
        /// </remarks>
        /// <example>Input: <Reference refidx="1" /><Reference refidx="2" />
        /// <Reference refidx="3" /><Reference refidx="4" /><Reference refidx="5" />
        /// <Reference refidx="7" /><Reference refidx="9" /> 
        /// should be converted to [1-4,5,6,9]</example>
        private void ConsolidateReferences(XmlDocument xmlDoc, DocumentXPathManager xPathManager)
        {
            try
            {
                XPathNavigator xNav = xmlDoc.CreateNavigator();
                xNav.MoveToFirst();
                XPathNavigator referenceNav = xNav.SelectSingleNode(xPathManager.GetXPath(SummaryXPath.Reference));

                while (referenceNav != null)
                {
                    // Extract a contiguous list of reference nodes
                    List<XmlNode> referenceNodeList = GetReferenceNodeList(referenceNav);

                    // Format the list of reference nodes according to presentation rules 
                    string formattedReferenceXml = FormatReferences(referenceNodeList, xPathManager);

                    XmlNode firstNode = referenceNodeList[0];
                    XmlNode lastNode = referenceNodeList[referenceNodeList.Count - 1];

                    // Find the next group of references 
                    // Note: need to find the next section of references before we re-write the document 
                    // (this interferes with the xpath)
                    referenceNav = referenceNav.SelectSingleNode(xPathManager.GetXPath(SummaryXPath.ReferenceII));

                    // Insert the re-formatted references back into the source document.
                    ReplaceNodes(formattedReferenceXml, firstNode, lastNode);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Preparing summary XML failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Determines the summary section type from an input string.
        /// </summary>
        /// <param name="summarySectionTypeString"></param>
        /// <returns></returns>
        public SummarySectionType DetermineSummarySectionType(string summarySectionTypeString)
        {
            SummarySectionType type = SummarySectionType.SummarySection;

            try
            {
                if (Enum.IsDefined(typeof(SummarySectionType), summarySectionTypeString))
                {
                    type = (SummarySectionType)Enum.Parse(typeof(SummarySectionType), summarySectionTypeString);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting summary section type failed.  Document CDRID=" + _documentID.ToString(), e);
            }
            return type;
        }

        /// <summary>
        /// Extracts the summary metadata from the input XML document. This method can be 
        /// overridden in the derived class.
        /// </summary>
        /// <param name="xmlDoc">Summary XML</param>
        /// <param name="summary">Summary document object</param>
        /// <remarks>Extract must be run to determine what devices are valid for a document.
        /// The caller is responsible for verifying that the device is supported for the
        /// currently targetted device before proceeding with document processing.
        /// </remarks>
        public override void Extract(XmlDocument xmlDoc, Document document, DocumentXPathManager xPathManager, TargetedDevice targetedDevice)
        {
            SummaryDocument summary = document as SummaryDocument;
            if (summary == null)
            {
                string message = "Expected document of type SummaryDocument, found {0}.";
                throw new DocumentTypeMismatchException(string.Format(message, document.GetType().Name));
            }

            try
            {
                // Consolidate adjacent references.
                ConsolidateReferences(xmlDoc, xPathManager);

                XPathNavigator xNav = xmlDoc.CreateNavigator();

                // Extract the CDR ID...
                xNav.MoveToFirstChild();
                if (CDRHelper.ExtractCDRID(xNav, xPathManager.GetXPath(CommonXPath.CDRID), out _documentID))
                {
                    summary.DocumentID = _documentID;
                }
                else
                {
                    throw new Exception("Extraction Error: Failed to extract document (CDR) ID in summary document!");
                }

                DocumentHelper.CopyXml(xmlDoc, summary);

                // Extract misc metadata and PermanentLinks...
                ExtractMetadata(xNav, summary, xPathManager);

                // Handle summary relations...
                ExtractRelations(xNav, summary, xPathManager);

                // Handle summary references URL...
                ExtractSummaryReferences(xNav, summary, xPathManager);

                // Handle sections...
                ExtractTopLevelSections(xNav, summary, xPathManager, targetedDevice);

                // Handle modified and published dates
                DocumentHelper.ExtractDates(xNav, summary, xPathManager.GetXPath(CommonXPath.LastModifiedDate), xPathManager.GetXPath(CommonXPath.FirstPublishedDate));

                // Ensure that all permanent link targets are found in the document
                VerifyPermanentLinkSections(summary);

                // Ensure that all permanent link targets are found in every device
                VerifyPermanentLinkDeviceSpecificSections(xNav, summary, xPathManager);

            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Failed to extract summary document", e);
            }
        }

        #endregion
    }
}
