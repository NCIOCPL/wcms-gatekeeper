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
        const string SECTION_ID_ATTRIBUTE = "id";
        const string SUMMARY_REFERENCE_SELECTOR = "//SummaryRef";
        const string SUMMARY_REFERENCE_ID_ATTRIBUTE  = "href";
        const string SUMMARY_REFERENCE_URL_ATTRIBUTE = "url";

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
                throw new NotImplementedException();
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
            }

            // Validation that applies to any summary goes here.

            // If a SummaryRef references a piloted Summary, verify that the
            // section appears in the piloted Summary's linked sections list.
            ValidateOutgoingSummaryRefs(summary, summaryData);
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
        /// Verifies that if the Summary contains any SummaryRefs which refer to a summary in the pilot,
        /// are those sections listed as part of the split data?
        /// </summary>
        /// <remarks>Throws ValidationException in the case of validation errors.</remarks>
        /// <param name="summary">XML Document containing a PDQ Summary.</param>
        /// <param name="summaryData">Metadata describing summaries which appear in the pilot.</param>
        public void ValidateOutgoingSummaryRefs(XmlDocument summary, ISplitDataManager splitData)
        {
            IEnumerable<string> references = GetSummaryRefList(summary);
            foreach (string item in references)
            {
                // If the reference only has one segment, it's a reference to an entire summary and therefore valid.
                // If there are two segments, the first is the summary's CDRID and the second is the specific section.
                string[] segments = item.Split('#');
                if(segments.Length == 2)
                {
                    int summaryid = CDRHelper.ExtractCDRIDAsInt(segments[0]);
                    string sectionRef = segments[1];
                    if (splitData.SummaryIsSplit(summaryid))
                    {
                        SplitData split = splitData.GetSplitData(summaryid);
                        if (!Array.Exists(split.LinkedSections, section => section.Equals(sectionRef, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            throw new ValidationException(String.Format("SummaryPreprocessor: Section '{1}' is not a known section for document '{0}'", summaryid, sectionRef));
                        }
                    }
                }
            }
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
