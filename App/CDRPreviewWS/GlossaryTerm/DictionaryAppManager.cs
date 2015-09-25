using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using NCI.Logging;
using NCI.Web.Dictionary.BusinessObjects;
using NCI.Services.Dictionary;

using svcDictionaryType = NCI.Services.Dictionary.DictionaryType;
using svcSearchType = NCI.Services.Dictionary.SearchType;
using svcLanguage = NCI.Services.Dictionary.Language;
using svcAudienceType = NCI.Services.Dictionary.AudienceType;

namespace NCI.Web.Dictionary
{
    public class DictionaryAppManager
    {
        static Log log = new Log(typeof(DictionaryAppManager));


        /// <summary>
        /// Get Term from Dictionary Service to be deserialized and returned to the app module
        /// </summary>
        /// <param name="termId">int of the CDR ID</param>
        /// <param name="dictionary">which dictionary is being passed through</param>
        /// <param name="language">English/Spanish term</param>
        /// <param name="version">the version of the dictionary service</param>
        /// <returns>the term deserialized and the meta data from the database</returns>
        public DictionaryTerm GetTerm(int termId, DictionaryType dictionary, String language, String version)
        {
            // Translate from types the AppManager exposes to types the Dictionary Service exposes.
            svcDictionaryType svcDictionary = TypeTranslator.Translate(dictionary);
            svcLanguage svcLanguage = TypeTranslator.TranslateLocaleString(language);

            //sets up Dictionary Service so methods can be called
            DictionaryService service = new DictionaryService();

            //Sets up the services return type so that the meta data can be transfered to this term return.

            NCI.Services.Dictionary.BusinessObjects.TermReturn termRet = null;
            try
            {
                termRet = service.GetTerm(termId, svcDictionary, svcLanguage);
            }
            catch (Exception ex)
            {
                log.error("Error in Dictionary Web Service for Get Term: " + ex);
            }

            //String of JSON returned from the Database to be deserialized.
            DictionaryTerm dicTerm = null;
            
            try
            {
                //deserialize term details as returned by the service layer (termret.term)
                dicTerm = JsonConvert.DeserializeObject<DictionaryTerm>(termRet.Term);
            }
            catch (JsonReaderException ex)
            {
                log.error("Error in Json string from service: " + ex.ToString());
            }

           return dicTerm;
        }
                

        // <summary>
        /// Get Term from Dictionary Service to be deserialized and returned to the app module
        /// </summary>
        /// <param name="termId">int of the CDR ID</param>
        /// <param name="dictionary">which dictionary is being passed through</param>
        /// <param name="language">English/Spanish term</param>
        /// <param name="version">the version of the dictionary service</param>
        /// <param name="audience">The Term's desired audience.
        ///     Supported values are:
        ///         Patient
        ///         HealthProfessional
        /// </param>
        /// <returns>the term deserialized and the meta data from the database</returns>
        public DictionaryTerm GetTerm(int termId, DictionaryType dictionary, String language, String version, AudienceType audience)
        {
            // Translate from types the AppManager exposes to types the Dictionary Service exposes.
            svcDictionaryType svcDictionary = TypeTranslator.Translate(dictionary);
            svcLanguage svcLanguage = TypeTranslator.TranslateLocaleString(language);
            svcAudienceType svcAudience = TypeTranslator.Translate(audience);

            //sets up Dictionary Service so methods can be called
            DictionaryService service = new DictionaryService();

            //Sets up the services return type so that the meta data can be transfered to this term return.

            NCI.Services.Dictionary.BusinessObjects.TermReturn termRet = null;
            try
            {
                termRet = service.GetTerm(termId, svcDictionary, svcLanguage, svcAudience);
            }
            catch (Exception ex)
            {
                log.error("Error in Dictionary Web Service for Get Term: " + ex);
            }

            //String of JSON returned from the Database to be deserialized.
            DictionaryTerm dicTerm = null;

            try
            {
                //deserialize term details as returned by the service layer (termret.term)
                dicTerm = JsonConvert.DeserializeObject<DictionaryTerm>(termRet.Term);
            }
            catch (JsonReaderException ex)
            {
                log.error("Error in Json string from service: " + ex.ToString());
            }

            return dicTerm;
        }

        /// <summary>
        /// Dictionary search that will return a list of dictionary terms based on the dictionary type passed in
        /// and what kind of search is desired
        /// </summary>
        /// <param name="searchText">the text that has been typed into the search box</param>
        /// <param name="searchType">the type of search being executed</param>
        /// <param name="offset">how many to offset the first return result</param>
        /// <param name="maxResults">the max results to return<remarks>int.maxvalue should be used for retrieving all unless we have more</remarks></param>
        /// <param name="dictionary">the dictionary type (cancert term, drug, genetic)</param>
        /// <param name="language">English/Spanish</param>
        /// <returns>returns a list of dictioanry terms and related metadata</returns>
        public DictionarySearchResultCollection Search(String searchText, SearchType searchType, int offset, int maxResults, DictionaryType dictionary, String language)
        {
            // Translate from types the AppManager exposes to types the Dictionary Service exposes.
            svcDictionaryType svcDictionary = TypeTranslator.Translate(dictionary);
            svcSearchType svcSearchType = TypeTranslator.Translate(searchType);
            svcLanguage svcLanguage = TypeTranslator.TranslateLocaleString(language);

            DictionaryService service = new DictionaryService();

            //sets up SearchReturn from Web Service
            NCI.Services.Dictionary.BusinessObjects.SearchReturn searchRet = null;

            //tries the dictionary service to get the strings back
            try
            {
                searchRet = service.Search(searchText, svcSearchType, offset, maxResults, svcDictionary, svcLanguage);
            }
            catch (Exception ex)
            {
                log.error("There is an error in the Search Method of the Dictionary Service: " + ex);
            }

            List<DictionarySearchResult> resultList = DeserializeList(searchRet.Result);
            DictionarySearchResultCollection collection = new DictionarySearchResultCollection(resultList.AsEnumerable());
            collection.ResultsCount = searchRet.Meta.ResultCount;
            return collection;
        }

        /// <summary>
        /// Term suggestions from what is being typed into the search box.  Used for autosuggest
        /// </summary>
        /// <param name="searchText">the string being typed</param>
        /// <param name="searchType">Type of search being done (contains, starts with, etc.)</param>
        /// <param name="dictionary">Which dictionary is being searched</param>
        /// <param name="language">Language</param>
        /// <returns>returns list of suggestions</returns>
        public DictionarySuggestionCollection SearchSuggest(String searchText, SearchType searchType, DictionaryType dictionary, String language)
        {
            // Translate from types the AppManager exposes to types the Dictionary Service exposes.
            svcDictionaryType svcDictionary = TypeTranslator.Translate(dictionary);
            svcSearchType svcSearchType = TypeTranslator.Translate(searchType);
            svcLanguage svcLanguage = TypeTranslator.TranslateLocaleString(language);
            
            //Set up variables we will use
            List<DictionarySuggestion> list = new List<DictionarySuggestion>();
            DictionaryService service = new DictionaryService();
            SuggestReturnMeta meta = new SuggestReturnMeta();

            NCI.Services.Dictionary.BusinessObjects.SuggestReturn suggestRet = null;
            
            try
            {
                service.SearchSuggest(searchText, svcSearchType, svcDictionary, svcLanguage);
            }   
            catch(Exception ex)
            {
                log.error("Error in search suggest method in Dictionary Web Service: " + ex);
            }

            //sets up the suggest so the list of suggestions
            DictionarySuggestion suggest = new DictionarySuggestion();
            foreach (NCI.Services.Dictionary.BusinessObjects.DictionarySuggestion m in suggestRet.Result)
            {
                //get properties and set them then add to list
                suggest.ID = m.ID;
                suggest.Term = m.Term;
                list.Add(suggest);
        
            }
            //create return variable based on list
            DictionarySuggestionCollection result = new DictionarySuggestionCollection(list.AsEnumerable());
            result.ResultsCount = suggestRet.Meta.ResultCount;
            return result;

        }

        /// <summary>
        /// Expand Search is used for A-Z Lists on dictionaries
        /// </summary>
        /// <param name="searchText">search text</param>
        /// <param name="includeTypes">types to include</param>
        /// <param name="offset">how many results to offset</param>
        /// <param name="numResults"># of results to return</param>
        /// <param name="dictionary">The dictionary</param>
        /// <param name="language">which language</param>
        /// <param name="version">version of dictionary service</param>
        /// <returns>Collection of Dictionary Search Results</returns>
        public DictionarySearchResultCollection Expand(String searchText, String includeTypes, int offset, int maxResults, DictionaryType dictionary, String language, String version)
        {
            // Translate from types the AppManager exposes to types the Dictionary Service exposes.
            svcDictionaryType svcDictionary = TypeTranslator.Translate(dictionary);
            svcLanguage svcLanguage = TypeTranslator.TranslateLocaleString(language);

            DictionaryService service = new DictionaryService();
            NCI.Services.Dictionary.BusinessObjects.SearchReturn expandRet = null;
            try
            {
                expandRet = service.Expand(searchText, includeTypes, offset, maxResults, svcDictionary, svcLanguage);
            }
            catch (Exception ex)
            {
                log.error("Error in Expand Method in Dictionary Web Service: " + ex);
            }

            List<DictionarySearchResult> expansionList = DeserializeList(expandRet.Result);
            DictionarySearchResultCollection collection = new DictionarySearchResultCollection(expansionList.AsEnumerable());
            collection.ResultsCount = expandRet.Meta.ResultCount;
            

            return collection;
            
        }

        /// <summary>
        /// this is to utilize shared code between search and expand as they return the same objects and do the same action
        /// </summary>
        /// <param name="list">the string list that the dictionary service would return</param>
        /// <returns>deserialized list of DictionarySearchResults</returns>
        private List<DictionarySearchResult> DeserializeList(NCI.Services.Dictionary.BusinessObjects.DictionarySearchResultEntry[] list)
        {
            List<DictionarySearchResult> returnList = new List<DictionarySearchResult>(); ;

            foreach (NCI.Services.Dictionary.BusinessObjects.DictionarySearchResultEntry m in list)
            {
                try
                {
                    int id  = m.ID;
                    string termName  = m.MatchedTerm;
                    DictionaryTerm term = JsonConvert.DeserializeObject<DictionaryTerm>(m.TermDetail);

                    DictionarySearchResult expansion = new DictionarySearchResult(id, termName, term);
                    returnList.Add(expansion);
                }
                catch (JsonReaderException ex)
                {
                    log.error("Error in Json string from service: " + ex.ToString());
                }

            }

            return returnList;
        }   
    }
}
