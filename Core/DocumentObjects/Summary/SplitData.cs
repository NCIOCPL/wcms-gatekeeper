using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Contains metadata for a single summary which is included in the summary-split SEO pilot.
    /// </summary>
    [DataContract()]
    public class SplitData
    {
        /// <summary>
        /// The summary's CDRID
        /// </summary>
        [DataMember(Name = "cdrid")]
        public int CdrId { get; set; }

        /// <summary>
        /// The url for the general information output. (Exact format is not yet defined.)
        /// </summary>
        [DataMember(Name = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Array containing a list of the section IDs corresponding to the summaries pages.
        /// </summary>
        [DataMember(Name = "page-sections")]
        public string[] PageSections { get; set; }

        /// <summary>
        /// Array containing a l list of top-level sections to be included in general information output.
        /// </summary>
        [DataMember(Name = "general-sections")]
        public string[] GeneralSections { get; set; }

        /// <summary>
        /// Array containing a list of sections in the general information output which are linked from other summaries
        /// </summary>
        [DataMember(Name = "linked-sections")]
        public string[] LinkedSections { get; set; }

        /// <summary>
        /// String containing the long title for the summary's pilot page
        /// </summary>
        [DataMember(Name = "long-title")]
        public string LongTitle { get; set; }

        /// <summary>
        /// String containing the short title for the summary's pilot page
        /// </summary>
        [DataMember(Name = "short-title")]
        public string ShortTitle { get; set; }

        /// <summary>
        /// String containing the pilot page's long description
        /// </summary>
        [DataMember(Name = "long-description")]
        public string LongDescription { get; set; }

        /// <summary>
        /// String containing the pilot page's meta keywords.
        /// </summary>
        [DataMember(Name = "meta-keywords")]
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Checks whether sectionID appears in the summary's linked-section list.
        /// </summary>
        /// <param name="sectionID">ID of the section to be checked.</param>
        /// <returns>Returns true if sectionID appears in the linked-section list, false otherwise.</returns>
        public bool IsLinkedSection(string sectionID)
        {
            sectionID = sectionID.Trim();
            return Array.Exists(LinkedSections, ID => ID.Equals(sectionID, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Checks whether sectionID appears in the summary's general-section list.
        /// </summary>
        /// <param name="sectionID">ID of the section to be checked.</param>
        /// <returns>Returns true if sectionID appears in the general-section list, false otherwise.</returns>
        public bool IsGeneralSection(string sectionID)
        {
            sectionID = sectionID.Trim();
            return Array.Exists(GeneralSections, ID => ID.Equals(sectionID, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
