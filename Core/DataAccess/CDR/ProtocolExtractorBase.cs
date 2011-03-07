using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Text;

using GateKeeper.Common.XPathKeys;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Protocol;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Common code for functionality shared between ProtocolExtractor and CTGovProtocolExtractor
    /// </summary>
    public abstract class ProtocolExtractorBase
    {
        #region Properties

        /// <summary>
        /// Manager for the collection of element XPaths.
        /// </summary>
        protected DocumentXPathManager XPathManager { get; set; }

        #endregion

        #region Constructor

        public ProtocolExtractorBase(DocumentXPathManager xPath)
        {
            XPathManager = xPath;
        }

        #endregion

        /// <summary>
        /// Extracts the ID strings for use in the protocol's primary and secondary pretty URLs.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="ctgovProtocol">The ProtocolDocument object to be populated.</param>
        public void ExtractProtocolPrettyUrlIDs(XPathNavigator xNav, ProtocolDocument protocol)
        {
            // Primary URL is guaranteed present.
            protocol.PrimaryProtocolUrlID = DocumentHelper.GetXmlDocumentValue(xNav, XPathManager.GetXPath(ProtocolXPath.PrimaryUrlID));
            if (string.IsNullOrEmpty(protocol.PrimaryProtocolUrlID))
            {
                throw new Exception(string.Format("Required element PrimaryUrlID is empty or missing. Document CDRID={0}", protocol.DocumentID));
            }

            // Secondary URL is optional.
            string secondaryUrlID;
            secondaryUrlID = DocumentHelper.GetXmlDocumentValue(xNav, XPathManager.GetXPath(ProtocolXPath.SecondaryUrlID));
            if (!string.IsNullOrEmpty(secondaryUrlID))
                protocol.SecondaryProtocolUrlID = secondaryUrlID;
        }
    }
}
