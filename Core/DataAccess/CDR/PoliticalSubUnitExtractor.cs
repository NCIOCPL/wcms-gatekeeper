using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using GateKeeper.Common.XPathKeys;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.PoliticalSubUnit;
using GateKeeper.Logging;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Helper class to extract PoliticalSubUnitDocuments from source XML.
    /// </summary>
    public static class PoliticalSubUnitExtractor
    {
        #region Public Static Methods

        /// <summary>
        /// Method to extract a political sub unit from the input XML doc.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="polSub"></param>
        public static void Extract(XmlDocument xmlDoc, PoliticalSubUnitDocument polSub, DocumentXPathManager xPathManager)
        {
            string path = string.Empty;
            try {
                XPathNavigator xNav = xmlDoc.CreateNavigator();

                // Extract the CDR ID...
                xNav.MoveToFirstChild();
                int documentID = 0;
                if (CDRHelper.ExtractCDRID(xNav, xPathManager.GetXPath(CommonXPath.CDRID), out documentID))
                    polSub.DocumentID = documentID;
                else
                    throw new Exception("Extraction Error: Failed to extract document (CDR) ID in PoliticalSubUnit document!");

                DocumentHelper.CopyXml(xmlDoc, polSub);

                polSub.DocumentType = DocumentType.PoliticalSubUnit;

                path = xPathManager.GetXPath(PoliticalSubUnitXPath.FullName);
                polSub.FullName = xNav.SelectSingleNode(path).Value;
                path = xPathManager.GetXPath(PoliticalSubUnitXPath.ShortName);
                if (xNav.SelectSingleNode(path) != null)
                    polSub.ShortName = xNav.SelectSingleNode(path).Value;
                path = xPathManager.GetXPath(PoliticalSubUnitXPath.CountryName);
                polSub.CountryName = xNav.SelectSingleNode(path).Value;
                polSub.CountryId = DocumentHelper.GetAttribute(xNav.SelectSingleNode(xPathManager.GetXPath(PoliticalSubUnitXPath.CountryName)), xPathManager.GetXPath(PoliticalSubUnitXPath.CountryRef));
                polSub.CountryId = CDRHelper.ExtractCDRID(polSub.CountryId);

                // Handle modified and published dates
                DocumentHelper.ExtractDates(xNav, polSub, xPathManager.GetXPath(CommonXPath.LastModifiedDate), xPathManager.GetXPath(CommonXPath.FirstPublishedDate));
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + polSub.DocumentID, e);
            }
        }

        #endregion
    }
}
