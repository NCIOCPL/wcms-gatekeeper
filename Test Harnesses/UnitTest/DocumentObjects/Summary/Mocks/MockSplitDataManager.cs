using System;


using GateKeeper.DocumentObjects.Summary;

namespace GateKeeper.UnitTest.DocumentObjects.Summary
{
    class MockSplitDataManager : ISplitDataManager
    {
        public MockSplitDataManager()
        {
            MockSummaryIsSplit = false;
            MockSplitData = null;
        }

        /// <summary>
        /// Set true/false to control return from SummaryIsSplit().
        /// </summary>
        public bool MockSummaryIsSplit { get; set; }

        /// <summary>
        /// Set to control return from GetSplitData().
        /// </summary>
        public SplitData MockSplitData { get; set; }

        /// <summary>
        /// Emulates check for whether summaryID exists in the collection of summary data.
        /// Set MockSummaryIsSplit to control the return value.
        /// </summary>
        /// <param name="summaryID"></param>
        /// <returns>True if the summary is supposed to be split, false otherwise.</returns>
        public bool SummaryIsSplit(int summaryID)
        {
            return MockSummaryIsSplit;
        }

        /// <summary>
        /// Retrieves a SplitData object containing information about how to split a given summary.
        /// </summary>
        /// <param name="summaryID"></param>
        /// <returns>SplitData object. NULL if summaryID is not found.</returns>
        public SplitData GetSplitData(int summaryID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether an identified summary reference (the combination of summaryID and sectionID) refers to
        /// a section which is on the general page(s) of a split summary.
        /// </summary>
        /// <param name="summaryID">ID of a summary which is potentially part of the split pilot.</param>
        /// <param name="sectionID">ID of a summary section.</param>
        /// <returns>True if summaryID refers to a summary which is in the pilot AND sectionID refers to a section
        /// which is identified as part of the summary's general information section.</returns>
        public bool ReferenceIsForGeneralSection(int summaryID, string sectionID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether sectionID appears in the summary's general-section list.
        /// </summary>
        /// <param name="sectionID">ID of the section to be checked.</param>
        /// <returns>Returns true if sectionID appears in the general-section list, false otherwise.</returns>
        public bool SectionIsAGeneralInformationPage(int summaryID, string sectionID)
        {
            throw new NotImplementedException();
        }
    }
}
