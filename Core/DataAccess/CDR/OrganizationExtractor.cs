using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using GateKeeper.Common;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Organization;
using GateKeeper.Logging;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Helper class to extract OrganizationDocuments from source XML.
    /// </summary>
    public static class OrganizationExtractor
    {
        #region Public Static Methods

        /// <summary>
        /// Method to extract an organization document from the input XML.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="organization"></param>
        public static void Extract(XmlDocument xmlDoc, OrganizationDocument organization, DocumentXPathManager xPathManager)
        {
          try{
                XPathNavigator xNav = xmlDoc.CreateNavigator();

                // Extract the CDR ID...
                xNav.MoveToFirstChild();
                int documentID = 0;
                if (CDRHelper.ExtractCDRID(xNav, xPathManager.GetXPath(CommonXPath.CDRID), out documentID))
                    organization.DocumentID = documentID;
                else
                   throw new Exception("Extraction Error: Failed to extract document (CDR) ID in organization document!");           

                DocumentHelper.CopyXml(xmlDoc, organization);

                organization.DocumentType = DocumentType.Organization;

                organization.OfficialName = xNav.SelectSingleNode(xPathManager.GetXPath(OrganizationXPath.OfficialName)).Value;

                XPathNodeIterator shortNameIter = xNav.Select(xPathManager.GetXPath(OrganizationXPath.ShortName));
                while (shortNameIter.MoveNext())
                {
                    organization.ShortNames.Add(shortNameIter.Current.Value);
                }

                XPathNodeIterator altNameIter = xNav.Select(xPathManager.GetXPath(OrganizationXPath.AlterName));
                while (altNameIter.MoveNext())
                {
                    organization.AlternateNames.Add(altNameIter.Current.Value);
                }

                // Handle modified and published dates
                DocumentHelper.ExtractDates(xNav, organization, xPathManager.GetXPath(CommonXPath.LastModifiedDate), xPathManager.GetXPath(CommonXPath.FirstPublishedDate));
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Failed to extract organization document", e);
            }
        }

        #endregion Public Static Methods
    }
}
