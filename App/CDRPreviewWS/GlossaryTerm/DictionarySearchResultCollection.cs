using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using NCI.Web.Dictionary.BusinessObjects;

namespace NCI.Web.Dictionary
{
    /// <summary>
    /// Dictionary Collection for Search and Expand
    /// </summary>
    public class DictionarySearchResultCollection : IEnumerable<DictionarySearchResult>
    {
  
        private IEnumerable<DictionarySearchResult> searchResults;
        /// <summary>
        /// This Results count is the number of results that matched the search from the web service
        /// </summary>
        public int ResultsCount { get; set; }

        /// <summary>
        /// Constructor Method that sets the enumerator
        /// </summary>
        /// <param name="results"> the list passed in as an enumerable</param>
        public DictionarySearchResultCollection(IEnumerable<DictionarySearchResult> results)
        {
            this.searchResults = results;
        }




        #region IEnumerable<DictionarySearchResult> Members
        /// <summary>
        /// Get Enumerator object 
        /// </summary>
        /// <returns>Enumerable list</returns>
        public IEnumerator<DictionarySearchResult> GetEnumerator()
        {
            return searchResults.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Get IEnumerable
        /// </summary>
        /// <returns>the Enumerable list</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return searchResults.GetEnumerator();
        }

        #endregion
    }
}
