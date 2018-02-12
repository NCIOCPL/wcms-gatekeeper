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
        public bool MockSummaryIsSplit { get; set; }

        public MockSplitDataManager() { }

        /// <summary>
        /// Checks whether summaryID exists in the collection of summary data.
        /// </summary>
        /// <param name="summaryID"></param>
        /// <returns>True if the summary is supposed to be split, false otherwise.</returns>
        public bool SummaryIsSplit(int summaryID)
        {
            return MockSummaryIsSplit;
        }

    }
}
