using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    /// <summary>
    /// Outermost data structure for returns from Expand().
    /// </summary>
    [DataContract()]
    public class SearchReturn
    {
        public SearchReturn()
        {
            // Force collection to be non-null.
            Result = new DictionarySearchResult[] { };
        }

        /// <summary>
        /// Metadata about the expansion search results.
        /// </summary>
        [DataMember(Name = "meta")]
        public SearchReturnMeta Meta { get; set; }

        /// <summary>
        /// Array of DictionarySearchResultEntry objects containg details of the individual terms which met the search criteria.
        /// </summary>
        [DataMember(Name = "result")]
        public DictionarySearchResult[] Result { get; set; }
    }
}
