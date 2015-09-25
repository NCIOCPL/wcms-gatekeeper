using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NCI.Web.Dictionary.BusinessObjects;

namespace NCI.Web.Dictionary
{
    public class DictionarySuggestionCollection : IEnumerable<DictionarySuggestion>
    {
        private IEnumerable<DictionarySuggestion> suggestReturn;
        /// <summary>
        /// Total number of Results the suggest is returning
        /// </summary>
        public int ResultsCount { get; set; }


        /// <summary>
        /// Constructor Method that sets the enumerator
        /// </summary>
        /// <param name="results"> the list passed in as an enumerable</param>
        public DictionarySuggestionCollection(IEnumerable<DictionarySuggestion> results)
        {
            this.suggestReturn = results;
        }

        #region IEnumerable<DictionarySuggestion> Members

        public IEnumerator<DictionarySuggestion> GetEnumerator()
        {
            return suggestReturn.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return suggestReturn.GetEnumerator();
        }

        #endregion
    }
}
