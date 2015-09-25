using System;
using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    /// <summary>
    /// Represents a link to a related PDQ Summary document.
    /// </summary>
    [DataContract()]
    public class RelatedSummary
    {
        /// <summary>
        /// the summary link
        /// </summary>
        [DataMember(Name = "url")]
        public String url { get; set; }

        /// <summary>
        /// link text
        /// </summary>
        [DataMember(Name = "text")]
        public String Text { get; set; }
    }
}
