using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    /// <summary>
    /// Outermost data structure for returns from ValidateSearchSuggest().
    /// </summary>
    [DataContract()]
    public class SuggestReturn
    {
        public SuggestReturn()
        {
            // Force collection to be non-null.
            Result = new DictionarySuggestion[] { };
        }

        [DataMember(Name = "meta")]
        public SuggestReturnMeta Meta { get; set; }

        [DataMember(Name = "result")]
        public DictionarySuggestion[] Result { get; set; }
    }
}
