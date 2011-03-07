using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace GateKeeper.Common
{
    /// <summary>
    /// Class that contains CDR related helper functions.
    /// </summary>
    public sealed class CDRHelper
    {
        #region Public Static Methods

        /// <summary>
        /// Utility function to strip out the base CDR id.
        /// </summary>
        /// <param name="inputCDRID"></param>
        /// <returns></returns>
        /// <example>Given: "CDR0000062779#_51", "62779" is returned</example>
        public static string ExtractCDRID(string inputCDRID)
        {
            // Remove sections (if there are any)
            string temp = Regex.Replace(inputCDRID, "#.*", "", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            // Remove CDR0000 prefix
            return Regex.Replace(temp, "^CDR0+", "", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase).Trim();
        }

        /// <summary>
        /// Utility function to re-create the original CDR id format 
        /// (CDR0000nnnnnn) from the base CDR id.
        /// </summary>
        /// <param name="inputCDRID"></param>
        /// <returns>Given: "62779", "CDR0000062779" is returned</returns>
        public static string RebuildCDRID(string inputCDRID)
        {
            StringBuilder cdrFormatted = new StringBuilder("CDR");
            int cdrMaxLen = 13;

            // Total number of zeros = (Max length) - (length of the id passed in) - (3 chars for CDR)
            int numZeros = cdrMaxLen - inputCDRID.Length - 3;

            // HACK: Need a better way to pad with zeros
            for (int i = 0; i < numZeros; i++)
            {
                cdrFormatted.Append("0");
            }

            cdrFormatted.Append(inputCDRID);

            return cdrFormatted.ToString();
        }

        /// <summary>
        /// Utility function to extract and strip out the base CDR id from an XPathNavigator.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string ExtractCDRID(XPathNavigator xNav, string attributeName)
        {
            string id = string.Empty;

            if (xNav != null && xNav.HasAttributes)
            {
                id = CDRHelper.ExtractCDRID(xNav.GetAttribute(attributeName, string.Empty));
            }

            return id;
        }

        /// <summary>
        /// Utility function to extract and strip out the base CDR id from an XPathNavigator.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="attributeName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool ExtractCDRID(XPathNavigator xNav, string attributeName, out int id)
        {
            bool isValidDocID = false;
            id = 0;

            if (xNav != null && xNav.HasAttributes)
            {
                int documentID = 0;
                isValidDocID =
                    Int32.TryParse(CDRHelper.ExtractCDRID(xNav.GetAttribute(attributeName, string.Empty)),
                    out documentID);

                id = documentID;
            }

            return isValidDocID;
        }

        #endregion
    }
}
