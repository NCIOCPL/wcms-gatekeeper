using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    /// <summary>
    /// Defines the overall data structure for returns from an individual
    /// Term lookup.
    /// </summary>
    [DataContract()]
    public class TermReturn
    {
        [DataMember(Name = "meta")]
        public TermReturnMeta Meta { get; set; }

        [DataMember(Name = "term")]
        public DictionaryTerm Term { get; set; }
    }
}
