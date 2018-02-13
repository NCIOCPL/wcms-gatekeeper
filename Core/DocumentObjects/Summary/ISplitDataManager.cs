using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
