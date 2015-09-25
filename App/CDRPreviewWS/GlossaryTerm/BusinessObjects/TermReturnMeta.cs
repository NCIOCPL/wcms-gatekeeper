using System;
using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    /// <summary>
    /// Metadata about a call to GetTerm().
    /// </summary>
    [DataContract()]
    public class TermReturnMeta : MetaCommon
    {
        /// <summary>
        /// The Term's audience Patient or HealthProfessional
        /// </summary>
        [DataMember(Name = "audience")]
        public String Audience { get; set; }

        /// <summary>
        /// The Term's language
        /// </summary>
        [DataMember(Name = "language")]
        public String Language { get; set; }
    }
}
