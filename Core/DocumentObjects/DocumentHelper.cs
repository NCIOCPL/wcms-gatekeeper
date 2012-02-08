using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using GateKeeper.Common;

namespace GateKeeper.DocumentObjects
{
    /// <summary>
    /// Class to encapsulate common document related functions.
    /// </summary>
    public static class DocumentHelper
    {
        #region Public Static Methods

        #region Extraction

        /// <summary>
        /// Removes \r and \n from the input string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripControlCharacters(string input)
        {
            string returnValue = string.Empty;
            if (input != null)
            {
                returnValue = input.Replace("\r", string.Empty);
                returnValue = returnValue.Replace("\n", string.Empty);
            }

            return returnValue;
        }

        /// <summary>
        /// Method to extract last modified date and date first published.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="document"></param>
        /// <param name="lastModifiedDateXPath"></param>
        /// <param name="firstPublishedDateXPath"></param>
        public static void ExtractDates(XPathNavigator xNav, Document document, string lastModifiedDateXPath, string firstPublishedDateXPath)
        {
            string path = string.Empty;
            try
            {
                DateTime lastModifiedDate = DateTime.Now;
                path = lastModifiedDateXPath;
                XPathNavigator node1 = xNav.SelectSingleNode(path);
                if (node1 != null)
                {
                    if (DateTime.TryParse(GetXmlDocumentValue(xNav, path), out lastModifiedDate))
                    {
                        document.LastModifiedDate = lastModifiedDate;
                    }
                    else if (GetXmlDocumentValue(xNav, path).Trim().Length == 0)
                    {
                        throw new Exception("Extraction Error: " + path + " contains no data.");
                    }
                    else
                    {
                        throw new Exception("Extraction Error: " + path + " contains invalid date format. DateLastModified = " + GetXmlDocumentValue(xNav, path).Trim() + ".");
                    }
                }
                else if (document.DocumentType == DocumentType.DrugInfoSummary || 
                        document.DocumentType == DocumentType.Protocol ||
                        document.DocumentType == DocumentType.CTGovProtocol)
                {
                    // If the date is not available, set it to MinValue for future determination before saving into database
                    document.LastModifiedDate = DateTime.MinValue;
                }

                DateTime firstPublishedDate = DateTime.MinValue;
                path = firstPublishedDateXPath;
                XPathNavigator node2 = xNav.SelectSingleNode(path);
                if (node2 != null)
                {
                    if (DateTime.TryParse(GetXmlDocumentValue(xNav, path), out firstPublishedDate))
                    {
                        document.FirstPublishedDate = firstPublishedDate;
                    }
                    else if (GetXmlDocumentValue(xNav, path).Trim().Length == 0)
                    {
                        throw new Exception("Extraction Error: " + path + " contains no data.");
                    }
                    else
                    {
                        throw new Exception("Extraction Error: " + path + " contains invalid date format. DateFirstPublished = " + GetXmlDocumentValue(xNav, path).Trim() + ".");
                    }
                }
                else if (document.DocumentType == DocumentType.DrugInfoSummary)
                {
                    throw new Exception("Extraction Error: The xml node " + path + " is missing.");
                }
                
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed. Document CDRID=" + document.DocumentID.ToString() + ". Exception occured in DocumentHelper.ExtractDates()", e);
            }
        }

        /// <summary>
        /// Method to extract a list of (repeating) string values from an XPathNavigator.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static List<string> ExtractValueList(XPathNavigator xNav, string xpath)
        {
            List<string> valueList = new List<string>();

            XPathNodeIterator nodeIter = xNav.Select(xpath);
            while (nodeIter.MoveNext())
            {
                valueList.Add(nodeIter.Current.Value);
            }

            return valueList;
        }

        /// <summary>
        /// Copies document XML to the document object.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="document"></param>
        public static void CopyXml(XmlDocument xmlDoc, Document document)
        {
            // Deep copy XML data to target document object...
            document.Xml = (XmlDocument)xmlDoc.CloneNode(true);
        }

        /// <summary>
        /// Extracts the pretty url xref from the XPath navigator.
        /// </summary>
        /// <param name="prettyUrlNav"></param>
        /// <returns></returns>
        /// <remarks>This method strips "http://www.cancer.gov" or "http://cancer.gov" if they exist</remarks>
        public static string ExtractPrettyUrl(XPathNavigator prettyUrlNav)
        {
            string prettyUrl = string.Empty;

            if (prettyUrlNav != null && prettyUrlNav.HasAttributes)
            {
                prettyUrl = prettyUrlNav.GetAttribute("xref", string.Empty);

                if (Regex.IsMatch(prettyUrl, "^https?://(www.)?cancer.gov"))
                {
                    prettyUrl = Regex.Replace(prettyUrl, "^https?://(www.)?cancer.gov", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (prettyUrl.EndsWith("/"))
                    {
                        prettyUrl = prettyUrl.Substring(0, prettyUrl.Length - 1);
                    }
                }
                else if (prettyUrl.StartsWith("/cancertopics") || prettyUrl.StartsWith("/espanol"))
                {
                    // Do nothing, it is relative path without site name
                }
                else
                {
                    throw new Exception("The document url is not valid. URL = " + prettyUrl +  ".");
                }
            }

            return prettyUrl;
        }

        /// <summary>
        /// Extracts the pretty url base path.
        /// </summary>
        /// <param name="prettyURL"></param>
        /// <returns></returns>
        /// <remarks>This method strips "http://www.cancer.gov" or "http://cancer.gov" if they exist, throw error msg if the url is not good</remarks>
        public static void GetBasePrettyUrl(ref string prettyURL)
        {
            if (Regex.IsMatch(prettyURL, "^https?://(www.)?cancer.gov"))
            {
                prettyURL = Regex.Replace(prettyURL, "^https?://(www.)?cancer.gov", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (prettyURL.EndsWith("/"))
                {
                    prettyURL = prettyURL.Substring(0, prettyURL.Length - 1);
                }
            }
            else if (prettyURL.StartsWith("/cancertopics") || prettyURL.StartsWith("/espanol"))
            {
                // Do nothing, it is relative path without site name
            }
            else
            {
                throw new Exception("The document url is not valid. URL = " + prettyURL + ".");
            }
        }

        /// <summary>
        /// Determines audience type from input string.
        /// </summary>
        /// <param name="audienceTypeString"></param>
        /// <returns></returns>
        /// <remarks>Note: This method will default to Health Professional audience type</remarks>
        public static AudienceType DetermineAudienceType(string audienceTypeString)
        {
            return (Regex.IsMatch(audienceTypeString, ".*Patient*")) ? AudienceType.Patient : AudienceType.HealthProfessional;
        }

        /// <summary>
        /// Returns audience type string for a given audience type for saving it to database
        /// Patient and Health Professional 
        /// NOTE: DO NOT USE ToString to get the audience when savign to db 
        /// </summary>
        /// <param name="AudienceType"></param>
        /// <returns>string</returns>
        /// <remarks>Note: This method will default to Health Professional audience type</remarks>
        public static string GetAudienceDBString(AudienceType at)
        {
            if (at == AudienceType.HealthProfessional)
                return "Health professional";
            else
                return at.ToString(); 
        }

        /// <summary>
        /// Extracts a single node value from an XPathNavigator.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static string GetXmlDocumentValue(XPathNavigator xNav, string xpath)
        {
            string val = string.Empty;
            XPathNavigator node = xNav.SelectSingleNode(xpath);

            if (node != null)
            {
                val = node.Value.Trim();
            }

            return val;
        }

        /// <summary>
        /// Extracts a single node raw text from an XPathNavigator.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static string GetXmlDocumentRawData(XPathNavigator xNav, string xpath)
        {
            string val = string.Empty;
            XPathNavigator node = xNav.SelectSingleNode(xpath);

            if (node != null)
            {
                val = node.InnerXml.Trim();
            }

            return val;
        }

        /// <summary>
        /// Extracts a single node value from an XPathNavigator.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="xpath"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string GetXmlDocumentAttributeValue(XPathNavigator xNav, string xpath, string attributeName)
        {
            string val = string.Empty;

            if (xNav != null)
            {
                XPathNavigator node = xNav.SelectSingleNode(xpath);

                if (node != null)
                {
                    if (node.HasAttributes)
                    {
                        val = node.GetAttribute(attributeName, string.Empty).Trim();
                    }
                }
            }

            return val;
        }

        /// <summary>
        /// Retrieves the specified attribute from the XML document.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string GetAttribute(XPathNavigator xNav, string attributeName)
        {
            string returnValue = string.Empty;

            if (xNav != null && xNav.HasAttributes)
            {
                returnValue = xNav.GetAttribute(attributeName, string.Empty).Trim();
            }

            return returnValue;
        }

        /// <summary>
        /// Retrieves the specified attribute from the XML document.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="basePath"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string GetAttribute(XPathNavigator xNav, string basePath, string attributeName)
        {
            string returnValue = string.Empty;
            XPathNavigator selectedNode = xNav.SelectSingleNode(basePath);

            if (selectedNode != null && selectedNode.HasAttributes)
            {
                returnValue = selectedNode.GetAttribute(attributeName, string.Empty).Trim();
            }

            return returnValue;
        }

        /// <summary>
        /// Translates the language short version to long form.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
 /*       public static string DetermineLanguageString(string language)
        {
            string lang;
            switch (language.Trim().ToUpper())
            {
                case "ENGLISH": lang = "ENGLISH"; break;
                case "EN": lang = "ENGLISH"; break;
                case "SPANISH": lang = "SPANISH"; break;
                case "ES": lang = "SPANISH"; break;
                default: lang = language.Trim().ToUpper(); break;
            }
            return lang;
        }*/

        /// <summary>
        /// Translates the language short version to long form.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static Language DetermineLanguageString(string language)
        {
            Language lang;
            switch (language.Trim().ToUpper())
            {
                case "ENGLISH": lang = Language.English; break;
                case "EN": lang = Language.English; break;
                case "SPANISH": lang = Language.Spanish; break;
                case "ES": lang = Language.Spanish; break;
                default: lang = Language.English; break;
            }
            return lang;
        }

        /// <summary>
        /// Translates the string into the Language enumerated type.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static Language DetermineLanguage(string language)
        {
            Language lang = Language.English;

            switch (language.Trim().ToUpper())
            {
                case "ENGLISH": lang = Language.English; break;
                case "EN": lang = Language.English; break;
                case "SPANISH": lang = Language.Spanish; break;
                case "ES": lang = Language.Spanish; break;
                default: lang = Language.NotSupported; break;
            }

            return lang;
        }

        #endregion

        #region Validation



        #endregion

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Calculates the level of the given element node.
        /// </summary>
        /// <param name="xNav"></param>
        /// <returns></returns>
        internal static int GetLevel(XPathNavigator xNav)
        {
            int level = 0;

            if (xNav != null)
            {
                level = Convert.ToInt32(xNav.Evaluate(string.Format("count(ancestor-or-self::{0})", xNav.Name)));
            }

            return level;
        }

        #endregion Internal Static Methods
    }
}
