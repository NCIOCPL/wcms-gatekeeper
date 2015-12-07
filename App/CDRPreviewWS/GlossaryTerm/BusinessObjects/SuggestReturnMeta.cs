using System.Runtime.Serialization;

namespace CDRPreviewWS.GlossaryTerm.BusinessObjects
{
    [DataContract()]
    public class SuggestReturnMeta : MetaCommon
    {
        /// <summary>
        /// The total number of terms matching the request
        /// </summary>
        [DataMember(Name = "result_count")]
        public int ResultCount { get; set; }
    }
}
