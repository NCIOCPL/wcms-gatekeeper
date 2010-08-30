using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Summary section type.
    /// </summary>
    public enum SummarySectionType
    {
        /// <summary>
        /// Section is a regular summary section.
        /// </summary>
        SummarySection = 1,

        /// <summary>
        /// Section is a reference (referred to 
        /// by SummarySetions in the same document).
        /// </summary>
        Reference = 2,

        /// <summary>
        /// Section is an enlarged table section.
        /// </summary>
        Table = 3,
    }
}
