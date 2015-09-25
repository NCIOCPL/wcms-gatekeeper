using System;

// For making it clear when the NCI.Services.Dictionary
// types are being used without being overly verbose.
using svcDictionaryType = NCI.Services.Dictionary.DictionaryType;
using svcSearchType = NCI.Services.Dictionary.SearchType;
using svcLanguage = NCI.Services.Dictionary.Language;
using svcAudienceType = NCI.Services.Dictionary.AudienceType;

namespace NCI.Web.Dictionary
{
    /// <summary>
    /// Translates enums from the NCI.Web.Dictionary namespace
    /// to their equivalents in the NCI.Services.Dictionary namespace
    /// </summary>
    internal static class TypeTranslator
    {
        /// <summary>
        /// Translates values of type NCI.Web.Dictionary
        /// to values of the type NCI.Services.Dictionary.DictionaryType.
        /// </summary>
        /// <param name="dictionary">The NCI.Web.Dictionary value to be translated.</param>
        /// <returns>A value of type NCI.Services.Dictionary.DictionaryType.</returns>
        public static svcDictionaryType Translate(DictionaryType xlate)
        {
            svcDictionaryType dictionary;
            switch (xlate)
            {
                case DictionaryType.Unknown:
                    dictionary = svcDictionaryType.Unknown;
                    break;
                case DictionaryType.term:
                    dictionary = svcDictionaryType.term;
                    break;
                case DictionaryType.drug:
                    dictionary = svcDictionaryType.drug;
                    break;
                case DictionaryType.genetic:
                    dictionary = svcDictionaryType.genetic;
                    break;
                default:
                    throw new ArgumentException(String.Format("Uknown type '{0}'.", xlate));
            }

            return dictionary;
        }

        /// <summary>
        /// Translates values of type NCI.Web.SearchType
        /// to values of the type NCI.Services.Dictionary.SearchType.
        /// </summary>
        /// <param name="dictionary">The NCI.Web.SearchType value to be translated.</param>
        /// <returns>A value of type NCI.Services.Dictionary.SearchType.</returns>
        public static svcSearchType Translate(SearchType xlate)
        {
            svcSearchType searchType;
            switch (xlate)
            {
                case SearchType.Unknown:
                    searchType = svcSearchType.Unknown;
                    break;
                case SearchType.Begins:
                    searchType = svcSearchType.Begins;
                    break;
                case SearchType.Contains:
                    searchType = svcSearchType.Contains;
                    break;
                case SearchType.Magic:
                    searchType = svcSearchType.Magic;
                    break;
                default:
                    throw new ArgumentException(String.Format("Uknown type '{0}'.", xlate));
            }

            return searchType;
        }

        public static svcLanguage TranslateLocaleString(String xlate)
        {
            svcLanguage language;
            switch (xlate.ToLowerInvariant())
            {
                case "es":
                    language = svcLanguage.Spanish;
                    break;

                case "en":
                default:
                    language = svcLanguage.English;
                    break;
            }
            return language;
        }


        /// <summary>
        /// Translates values of type NCI.Web.Dictionary.AudienceType.
        /// to values of the type NCI.Services.Dictionary.AudienceType.
        /// </summary>
        /// <param name="dictionary">The NCI.Web.Dictionary.AudienceType value to be translated.</param>
        /// <returns>A value of type NCI.Services.Dictionary.AudienceType.</returns>
        public static svcAudienceType Translate(AudienceType xlate)
        {
            svcAudienceType audience;
            switch (xlate)
            {
                case AudienceType.Unknown:
                    audience = svcAudienceType.Unknown;
                    break;
                case AudienceType.HealthProfessional:
                    audience = svcAudienceType.HealthProfessional;
                    break;
                case AudienceType.Patient:
                    audience = svcAudienceType.Patient;
                    break;
                    break;
                default:
                    throw new ArgumentException(String.Format("Uknown type '{0}'.", xlate));
            }

            return audience;
        }
    }
}
