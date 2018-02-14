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

        private HistoryEntryWriter WarningWriter;
        private HistoryEntryWriter InformationWriter;

        public void Preprocess(XmlDocument document, HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            WarningWriter = warningWriter;
            InformationWriter = informationWriter;

            if (document == null)
                throw new ArgumentNullException("document");

            // Get Split data. (Data is loaded prior to document processing.)
            ISplitDataManager splitData = SplitDataManager.Instance;

            Validate(document, splitData);
            // Rewrite SummaryRef URL attributes.
        }

        public void Validate(XmlDocument summary, ISplitDataManager summaryData)
        {
            XmlElement root = summary.DocumentElement;
            if (root.Name.CompareTo(SUMMARY_TYPE) != 0)
                throw new ValidationException(string.Format("Expected document type Summary, found '{0}' instead.", root.Name));

            // Check whether we need to do anything with this document.
            string idString = summary.DocumentElement.GetAttribute(CDRID_ATTRIBUTE);
            int cdrid = CDRHelper.ExtractCDRIDAsInt(idString);

            // Validation for summaries appearing in the pilot.
            if (summaryData.SummaryIsSplit(cdrid))
            {
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
        /// <param name="summary"></param>
        /// <param name="expectedSections"></param>
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
        /// <param name="summary"></param>
        /// <param name="expectedSections"></param>
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
        /// <param name="summary"></param>
        /// <returns>A List of string values.</returns>
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

        public void ValidateOutgoingSummaryRefs(XmlDocument summary, ISplitDataManager splitData)
        {
            throw new NotImplementedException();
        }
    }
}
