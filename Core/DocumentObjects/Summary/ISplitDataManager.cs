namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Interface definition for an object which contains data about which
    /// summaries are to be split into General and Main sections.
    /// </summary>
    public interface ISplitDataManager
    {
        /// <summary>
        /// Checks whether summaryID exists in the collection of summary data.
        /// </summary>
        /// <param name="summaryID"></param>
        /// <returns>True if the summary is supposed to be split, false otherwise.</returns>
        bool SummaryIsSplit(int summaryID);

        /// <summary>
        /// Retrieves a SplitData object containing information about how to split a given summary.
        /// </summary>
        /// <param name="summaryID"></param>
        /// <returns>SplitData object. NULL if summaryID is not found.</returns>
        SplitData GetSplitData(int summaryID);

        /// <summary>
        /// Checks whether an identified summary reference (the combination of summaryID and sectionID) refers to
        /// a section which is on the general page(s) of a split summary.
        /// </summary>
        /// <param name="summaryID">ID of a summary which is potentially part of the split pilot.</param>
        /// <param name="sectionID">ID of a summary section.</param>
        /// <returns>True if summaryID refers to a summary which is in the pilot AND sectionID refers to a section
        /// which is identified as part of the summary's general information section.</returns>
        bool ReferenceIsForGeneralSection(int summaryID, string sectionID);
    }
}
