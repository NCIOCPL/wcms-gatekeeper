using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    [DataContract()]
    public class DictionarySearchResult
    {
        // For serialization
        public DictionarySearchResult()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">term's id</param>
        /// <param name="matchedTerm">the term that matched</param>
        /// <param name="term">The term as type</param>
        public DictionarySearchResult(int id, String matchedTerm, DictionaryTerm term)
        {
            this.ID = id;
            this.MatchedTerm = matchedTerm;
            this.Term = term;
        }

        /// <summary>
        ///  The Term's ID
        /// </summary>
        [DataMember(Name = "id")]
        public int ID { get; set; }

        /// <summary>
        /// The Term name (or alias) that matched the expansion
        /// </summary>
        [DataMember(Name = "matched")]
        public String MatchedTerm { get; set; }

        /// <summary>
        /// Dictionary Terms
        /// </summary>
        [DataMember(Name = "term")]
        public DictionaryTerm Term { get; set; }
    }
}
