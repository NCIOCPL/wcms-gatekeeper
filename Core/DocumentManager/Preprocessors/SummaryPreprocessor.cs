using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;


using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.Common;

namespace GKManagers.Preprocessors
{
    /// <summary>
    /// Preprocessor for Summary documents
    /// </summary>
    public class SummaryPreprocessor
    {
        const string SUMMARY_TYPE = "Summary";
        const string CDRID_ATTRIBUTE = "id";
        const string PAGE_SECTION_SELECTOR = "/Summary/SummarySection";
        const string SPECIFIC_PAGE_SECTION_SELECTOR_FMT = "/Summary/SummarySection[@id='{0}']";
        const string SPECIFIC_SECTION_SELECTOR_FMT = "descendant-or-self::*[@id='{0}']";  // Any element at or below the current one with an ID attribute = the value of {0}
        const string SECTION_ID_ATTRIBUTE = "id";
        const string SUMMARY_REFERENCE_SELECTOR = "//SummaryRef";
        const string SUMMARY_REFERENCE_ID_ATTRIBUTE  = "href";
        const string SUMMARY_REFERENCE_URL_ATTRIBUTE = "url";
        const string SUMMARY_SECTION_INCLUDED_ATTRIBUTE = "IncludedDevices";

        private HistoryEntryWriter WarningWriter;
        private HistoryEntryWriter InformationWriter;

        public void Preprocess(XmlDocument summary, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            WarningWriter = warningWriter;
            InformationWriter = informationWriter;

            if (summary == null)
                throw new ArgumentNullException("document");

            // Get Split data. (Data is loaded prior to document processing.)
            ISplitDataManager splitData = SplitDataManager.Instance;

            Validate(summary, splitData);

            // Rewrite SummaryRef URL attributes.
            RewriteSummaryRefAttributes(summary, splitData);

            // Add IncludedDevices attributes.
            SetIncludedDevices(summary, splitData);
        }

        /// <summary>
        /// Checks a summary for SummaryRef elements. If found, checks whether the reference is to the general
        /// page of a piloted summary and rewrites the url attribute if necessary.
        /// </summary>
        /// <param name="summary">XML Document containing a PDQ Summary.</param>
        /// <param name="summaryData">Metadata describing summaries which appear in the pilot.</param>
        public void RewriteSummaryRefAttributes(XmlDocument summary, ISplitDataManager summaryData)
        {
            // Get the summary's ID.
            string idString = summary.DocumentElement.GetAttribute(CDRID_ATTRIBUTE);
            int cdrid = CDRHelper.ExtractCDRIDAsInt(idString);

            XPathNavigator xNav = summary.CreateNavigator();
            XPathNodeIterator nodeList = xNav.Select(SUMMARY_REFERENCE_SELECTOR);

            foreach (XPathNavigator node in nodeList)
            {
                string reference = DocumentHelper.GetAttribute(node, SUMMARY_REFERENCE_ID_ATTRIBUTE);
                string[] segments = reference.Split('#');

                // If there's only one segment, then the reference is just a CDRID, which means it refers to
                // the summary as a whole.  In this case, there is no need to modify the url.
                if( segments.Length == 1 || String.IsNullOrWhiteSpace(segments[1]))
                    continue;

                // If there are two segments, that means there's a section ID.  (CDR validation enforces no other segments counts are possible.)
                // segment[0] may be empty, in which case the reference is internal.
                bool theReferenceIsInternal = String.IsNullOrWhiteSpace(segments[0]);

                // If it's an internal reference, the summary being processed is the referenced summary.
                // Otherwise, get the summaryID.
                int referencedSummaryId;
                if (theReferenceIsInternal)
                    referencedSummaryId = cdrid;
                else
                    referencedSummaryId = CDRHelper.ExtractCDRIDAsInt(segments[0]);


                // Does the reference go to a split summary?
                if (summaryData.ReferenceIsForGeneralSection(referencedSummaryId, segments[1]))
                {
                    SplitData data = summaryData.GetSplitData(referencedSummaryId);

                    // Rewrite URL
                    node.MoveToAttribute(SUMMARY_REFERENCE_URL_ATTRIBUTE, String.Empty);
                    node.SetValue(data.Url);
                }
            }
        }

        /// <summary>
        /// If summary is part of the Summary Split pilot, then all top-level sections which are part of
        /// the general information page(s) are marked to be included only on the "general" and "syndication"
        /// meta-devices.
        /// </summary>
        /// <param name="summary">XML Document containing a PDQ Summary.</param>
        /// <param name="summaryData">Metadata describing summaries which appear in the pilot.</param>
        public void SetIncludedDevices(XmlDocument summary, ISplitDataManager splitData)
        {
            // Get the summary's ID.
            string idString = summary.DocumentElement.GetAttribute(CDRID_ATTRIBUTE);
            int cdrid = CDRHelper.ExtractCDRIDAsInt(idString);

            // Only make changes if this summary is part of the pilot
            if (splitData.SummaryIsSplit(cdrid))
            {
                XPathNavigator xNav = summary.CreateNavigator();
                XPathNodeIterator pageList = xNav.Select(PAGE_SECTION_SELECTOR);

                foreach (XPathNavigator page in pageList)
                {
                    string section = DocumentHelper.GetAttribute(page, SECTION_ID_ATTRIBUTE);
                    string devices;
                    if (splitData.SectionIsAGeneralInformationPage(cdrid, section))
                    {
                        // Set IncludedDevices to general and syndication with some shenanigans because SummarySectionDeviceType isn't marked as
                        // a bitflag and I don't want to risk break things by changing it.
                        devices = SummarySectionDeviceType.general + " " + SummarySectionDeviceType.syndication;
                    }
                    else
                    {
                        // Set IncludedDevices to main and syndication
                        devices = SummarySectionDeviceType.main + " " + SummarySectionDeviceType.syndication;
                    }

                    // True if Attribute exists, false if it doesn't.
                    if (page.MoveToAttribute(SUMMARY_SECTION_INCLUDED_ATTRIBUTE, String.Empty))
                        page.SetValue(devices);
                    else
                        page.CreateAttribute(String.Empty, SUMMARY_SECTION_INCLUDED_ATTRIBUTE, String.Empty, devices.ToString());
                }
            }
        }

        /// <summary>
        /// Validate assumptions for the summary and the summary split data.
        /// - Is the document really a summary?
        /// - If it's part of the split pilot:
        ///     - Are all the top-level sections known?
        ///     - Are all the "General Information" sections valid?
        /// - For all summaries:
        ///     - If it references a pilot summary, does the link exist in the list of links?
        /// </summary>
        /// <remarks>Throws ValidationException in the case of validation errors.</remarks>
        /// <param name="summary">XML Document containing a PDQ Summary.</param>
        /// <param name="summaryData">Metadata describing summaries which appear in the pilot.</param>
        public void Validate(XmlDocument summary, ISplitDataManager summaryData)
        {
            XmlElement root = summary.DocumentElement;
            if (root.Name.CompareTo(SUMMARY_TYPE) != 0)
                throw new ValidationException(string.Format("Expected document type Summary, found '{0}' instead.", root.Name));

            // Get the summary's ID.
            string idString = summary.DocumentElement.GetAttribute(CDRID_ATTRIBUTE);
            int cdrid = CDRHelper.ExtractCDRIDAsInt(idString);

            // Check whether we need to do anything with this document.
            if (summaryData.SummaryIsSplit(cdrid))
            {

                // Validation for summaries appearing in the pilot.
                SplitData split = summaryData.GetSplitData(cdrid);
                
                // Verify the top-level sections are identified correctly.
                ValidateTopLevelSections(summary, split.PageSections);

                // Validate that sections ID'ed as general sections exist.
                ValidateGeneralInformationSections(summary, split.GeneralSections);

                // Validate list of summary references.
                ValidateSummaryRefs(summary, split);

            }

            // Validation that applies to any summary goes here.

            /// NOTE: We keep trying to validate all SummaryRefs, but that's not possible.
            /// The split data only contains SummaryRef targets which appear on one of the
            /// summary's general information sections.
        }

        /// <summary>
        /// Verify that all top-level sections in document appear in expectedSections and that all
        /// sections in expectedSections are present as top-level sections in document.
        /// </summary>
        /// <remarks>Throws ValidationException in the case of validation errors.</remarks>
        /// <param name="summary">XML Document containing a PDQ Summary.</param>
        /// <param name="expectedSections">List of the top-level sections which are expected to exist.</param>
        public void ValidateTopLevelSections(XmlDocument summary, string[] expectedSections)
        {
            // Build hash set for expected sections.
            HashSet<string> expectedLookup = new HashSet<string>(expectedSections);

            // Build hash set for actual sections.
            IEnumerable<string> foundSections = GetTopSections(summary);
            HashSet<string> actualLookup = new HashSet<string>(foundSections);

            // Are all expected sections present?
            foreach(string section in expectedSections)
            {
                if (!actualLookup.Contains(section))
                    throw new ValidationException(String.Format("Expected page-level section '{0}' was not found in the summary.", section));
            }

            // Are all page-level sections present in the expected list?
            foreach(string section in foundSections)
            {
                if (!expectedLookup.Contains(section))
                    throw new ValidationException(String.Format("Page-level section '{0}' was found in the expected list.", section));
            }
        }

        /// <summary>
        /// Verify that all sections identifed as General Information sections are also top-level sections.
        /// </summary>
        /// <remarks>Throws ValidationException in the case of validation errors.</remarks>
        /// <param name="summary">XML Document containing a PDQ Summary.</param>
        /// <param name="expectedSections">The list of sections which belong in the General Information</param>
        public void ValidateGeneralInformationSections(XmlDocument summary, string[] expectedSections)
        {
            // Build hash set for expected sections.
            HashSet<string> expectedLookup = new HashSet<string>(expectedSections);

            // Build hash set for actual sections.
            IEnumerable<string> foundSections = GetTopSections(summary);
            HashSet<string> actualLookup = new HashSet<string>(foundSections);

            // Are all expected sections present?
            foreach (string section in expectedSections)
            {
                if (!actualLookup.Contains(section))
                    throw new ValidationException(String.Format("General Information '{0}' was not a page-level section.", section));
            }
        }

        /// <summary>
        /// Verify that all summary references in the summary's linked section list occur within
        /// one of the top-level sections identified as a general information page.
        /// </summary>
        /// <param name="summary"></param>
        /// <param name=""></param>
        public void ValidateSummaryRefs(XmlDocument summary, SplitData summaryData)
        {
            XPathNavigator xNav = summary.CreateNavigator();

            // Check each linked section item.
            Array.ForEach(summaryData.LinkedSections, sectionID => {
                bool sectionWasNotFound = true;

                // Look for the section inside each of the General Information sections.
                Array.ForEach(summaryData.GeneralSections, giSectionID => {
                    string selector = String.Format(SPECIFIC_PAGE_SECTION_SELECTOR_FMT, giSectionID);
                    XPathNodeIterator pageFinder = xNav.Select(selector);
                    if (pageFinder.MoveNext())
                    {
                        // Go to the actual top-level section.
                        XPathNavigator pageNav = pageFinder.Current;

                        // Search within the current page.
                        string sectionSelector = String.Format(SPECIFIC_SECTION_SELECTOR_FMT, sectionID);
                        XPathNodeIterator sectionFinder = pageNav.Select(sectionSelector);
                        if(sectionFinder.MoveNext())
                        {
                            // We found the section, set as found, exit the inner loop, go check the next one.
                            sectionWasNotFound = false;
                            return; // This loop is actually an anonymous function. To exit the loop, return from the function.
                        }
                    }
                    else
                        throw new ValidationException(String.Format("Section '{0}', is not a top-level section ID.", giSectionID));
                });

                if (sectionWasNotFound)
                    throw new ValidationException(String.Format("Section '{0}' was not found in a General Information section.", sectionID));
            });
        }

        /// <summary>
        /// Helper method to find all of a summary's page-level section IDs.
        /// </summary>
        /// <remarks>Throws ValidationException in the case of validation errors.</remarks>
        /// <param name="summary">XML Document containing a PDQ Summary.</param>
        /// <returns>A List of strings containing the IDs of the summary's top-level sections.</returns>
        private List<string> GetTopSections(XmlDocument summary)
        {
            List<string> foundSections = new List<string>();

            XPathNavigator xNav = summary.CreateNavigator();
            XPathNodeIterator nodeList = xNav.Select(PAGE_SECTION_SELECTOR);

            foreach (XPathNavigator node in nodeList)
            {
                string section = DocumentHelper.GetAttribute(node, SECTION_ID_ATTRIBUTE);
                if (String.IsNullOrWhiteSpace(section))
                    throw new ValidationException("Section ID not specified.");

                foundSections.Add(section.Trim());
            }

            return foundSections;
        }

        /// <summary>
        /// Helper method to get a list of all Summary references 
        /// </summary>
        /// <param name="summary"></param>
        /// <returns></returns>
        private IEnumerable<string> GetSummaryRefList(XmlDocument summary)
        {
            List<string> references = new List<string>();

            XPathNavigator xNav = summary.CreateNavigator();
            XPathNodeIterator nodeList = xNav.Select(SUMMARY_REFERENCE_SELECTOR);

            foreach (XPathNavigator node in nodeList)
            {
                string reference = DocumentHelper.GetAttribute(node, SUMMARY_REFERENCE_ID_ATTRIBUTE);
                references.Add(reference);
            }

            return references;
        }
    }
}
