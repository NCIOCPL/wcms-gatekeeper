using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.Logging;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Helper class to extract DrugInfoSummary from source XML.
    /// </summary>
    public class DrugInfoSummaryExtractor
    {
        #region Fields
        private int _documentID = 0;
        #endregion

        #region Public Methods
        /// <summary>
        /// Modifies the document XML, so that subsequent processing is based on
        /// ideal input.
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PrepareXml()
        {}

        /// <summary>
        /// Method to extract a drug information summary document from the input XML document.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        public void Extract(XmlDocument xmlDoc, DrugInfoSummaryDocument drugInfoSummary, DocumentXPathManager xPathManager)
        {
            try {
                XPathNavigator xNav = xmlDoc.CreateNavigator();

                // Extract the CDR ID...
                xNav.MoveToFirstChild();
                if (CDRHelper.ExtractCDRID(xNav, xPathManager.GetXPath(CommonXPath.CDRID), out _documentID))
                {
                    drugInfoSummary.DocumentID = _documentID;
                }
                else
                {
                    throw new Exception("Extraction Error: Failed to extract document (CDR) ID in drug information summary document!");
                }

                // Set document type
                drugInfoSummary.DocumentType = DocumentType.DrugInfoSummary;
                DocumentHelper.CopyXml(xmlDoc, drugInfoSummary);

                // Extract misc metadata
                drugInfoSummary.Title = xNav.SelectSingleNode(xPathManager.GetXPath(DrugInfoXPath.Title)).Value;
                drugInfoSummary.Description = xNav.SelectSingleNode(xPathManager.GetXPath(DrugInfoXPath.Description)).Value;
                drugInfoSummary.PrettyURL = DocumentHelper.ExtractPrettyUrl(xNav.SelectSingleNode(xPathManager.GetXPath(DrugInfoXPath.URL)));

                // Extract terminology link
                int terminologyLinkID = 0;
                string tempIDString = DocumentHelper.GetAttribute(xNav.SelectSingleNode(xPathManager.GetXPath(DrugInfoXPath.TermLink)), xPathManager.GetXPath(DrugInfoXPath.TermLinkRef));
                if (tempIDString.Trim().Length > 0)
                {
                    if (Int32.TryParse(CDRHelper.ExtractCDRID(tempIDString), out terminologyLinkID))
                    {
                        drugInfoSummary.TerminologyLink = terminologyLinkID;
                    }
                    else
                    {
                        throw new Exception("Extraction Error: " + xPathManager.GetXPath(DrugInfoXPath.TermLink) + "/@" + xPathManager.GetXPath(DrugInfoXPath.TermLinkRef) + " should be a valid CDRID. CurrentValue=" + tempIDString + ". Document CDRID= " + _documentID.ToString());
                    }
                }
 
                // Handle modified and published dates
                DocumentHelper.ExtractDates(xNav, drugInfoSummary, xPathManager.GetXPath(CommonXPath.LastModifiedDate), xPathManager.GetXPath(CommonXPath.FirstPublishedDate));
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Failed to extract drug information summary document", e);
            }
        }

        #endregion


    }
}
