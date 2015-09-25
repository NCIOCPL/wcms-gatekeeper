using System;
using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    /// <summary>
    /// represents an external link
    /// </summary>
    [DataContract()]
    public class RelatedExternalLink
    {
        /// <summary>
        /// the external link
        /// </summary>
        [DataMember(Name = "url")]
        public String Url { get; set; }

        /// <summary>
        /// link text
        /// </summary>
        [DataMember(Name = "text")]
        public String Text { get; set; }
    }
}
