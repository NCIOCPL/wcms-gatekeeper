using System;
using System.Collections.Generic;
using System.Linq;

using NCI.Util;

using GateKeeper.Common;

namespace GateKeeper.DocumentObjects.Summary
{
    public class SummaryReference
    {
        /// <summary>
        /// Business object describing a reference to a summary
        /// (This may be an internal reference).
        /// </summary>
        /// <param name="reference">The document being referenced, may include a fragment identifier,
        /// separated by a hash mark (#).</param>
        /// <param name="url">The pretty URL of the referenced document/</param>
        public SummaryReference(string reference, string url)
        {
            Url = url;
            CdrID = NCI.Util.Strings.ToInt(CDRHelper.ExtractCDRID(reference), true);
            if (reference.Contains('#'))
            {
                SectionID = reference.Substring(reference.IndexOf('#') + 1);
            }
            else
            {
                SectionID = string.Empty;
            }
        }

        /// <summary>
        /// CDR ID of the referenced summary.
        /// </summary>
        public int CdrID { get; private set; }

        /// <summary>
        /// ID of the referenced summary section.  String.Empty if no specific section is
        /// referenced.
        /// </summary>
        public string SectionID { get; private set; }

        /// <summary>
        /// Top-level pretty-URL of the referenced summary.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Indicates whether a specific summary section was referenced.  If false,
        /// the overall summary is referenced.
        /// </summary>
        public bool IsSectionReference { get { return !string.IsNullOrEmpty(SectionID); } }
    }
}
