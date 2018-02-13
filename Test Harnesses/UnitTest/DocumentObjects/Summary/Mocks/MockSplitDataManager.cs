using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
