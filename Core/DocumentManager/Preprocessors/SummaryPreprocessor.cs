
using System;
using System.Xml;

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

        public void Validate(XmlDocument document, ISplitDataManager summaryData)
        {
            XmlElement root = document.DocumentElement;
            if (root.Name.CompareTo(SUMMARY_TYPE) != 0)
                throw new ValidationException(string.Format("Expected document type Summary, found '{0}' instead.", root.Name));

            // Check whether we need to do anything with this document.
            string idString = document.DocumentElement.GetAttribute(CDRID_ATTRIBUTE);
            int cdrid = CDRHelper.ExtractCDRIDAsInt(idString);

            if (summaryData.SummaryIsSplit(cdrid))
            {
                SplitData split = summaryData.GetSplitData(cdrid);
                // Validation for summaries in the split.
                // Validate top-level sections.
                // Validate that sections ID'ed as general sections exist.
            }
        }

    }
}
