using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Configuration;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Protocol;

namespace GateKeeper.ContentRendering
{
    /// <summary>
    /// Class to render the Protocol document type.
    /// </summary>
    public class ProtocolRenderer : DocumentRenderer
    {
        #region Fields
        private ProtocolDocument _protocolDocument = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ProtocolRenderer()
        {}

        #endregion

        #region Private Methods


        #region CTGov

        /// <summary>
        /// Main method for rendering/parsing the CTGov XSL output.
        /// </summary>
        private void RenderCTGovProtocol()
        {
            try{
                string xslPath = ConfigurationManager.AppSettings["CTGovProtocol"];
                FileInfo xslFile = new FileInfo(xslPath);
                if (!xslFile.Exists)
                {
                    throw new Exception("Rendering Error: XSLT file for rendering CTGovProtocol does not exist. XPath = " + xslPath  + ".");
                }

                base.LoadTransform(xslFile);
                base.Render(this._protocolDocument);
                // Perform post XSL processing...
                XPathNavigator xNav = this._protocolDocument.PostRenderXml.CreateNavigator();
                // Parse main sections 
                ParseCTGovSections(xNav);
                // Parse StudSites (or TrialSites) 
                // Note: These are parse from the HP version only
                ParseStudySites(xNav.SelectSingleNode("//a[@name = 'TrialSites']"));
                this._protocolDocument.Xml.LoadXml(xNav.OuterXml);
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Rendering CTGovProtocol failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);

            }
        }

        /// <summary>
        /// Method to parse the CTGov sections from the XSL output.
        /// </summary>
        /// <param name="xNav"></param>
        /// <remarks>Note: Each HTML/XML fragment extracted by this routine 
        /// corresponds to a section in the final document.</remarks>
        private void ParseCTGovSections(XPathNavigator xNav)
        {
            try
            {
                // Need to rebuild the CDR formatted CDR ID (CDR0000123456) for the section names
                string strCDRID = CDRHelper.RebuildCDRID(this._protocolDocument.DocumentID.ToString());

                // Summary (Objectives)
                XPathNavigator summaryNav = xNav.SelectSingleNode("//a[@name='Summary']");
                if (summaryNav != null)
                {
                    //TODO: REMOVE - This replacement is done for string comparison purpose
                    string html = summaryNav.InnerXml;
                    html = html.Replace("<a name=\"Section\" />", "");

                    summaryNav.InnerXml = html;
                    this._protocolDocument.ProtocolSectionList.Add(new ProtocolSection(0, summaryNav.OuterXml, ProtocolSectionType.CTGovBriefSummary));
                }

                // Disclaimer
                XPathNavigator disclaimerNav = xNav.SelectSingleNode("//a[@name='Disclaimer']");
                if (disclaimerNav != null)
                {
                    disclaimerNav.InnerXml = "<a name=\"Disclaimer_" + strCDRID + "\"></a>" + disclaimerNav.InnerXml;
                    string tempHtml = disclaimerNav.OuterXml;
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0,
                        tempHtml,
                        ProtocolSectionType.CTGovDisclaimer));
                }

                // LeadOrgs
                XPathNavigator leadOrgsNav = xNav.SelectSingleNode("//a[@name='LeadOrgs']");
                if (leadOrgsNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, leadOrgsNav.OuterXml,   ProtocolSectionType.CTGovLeadOrgs));
                }

                // Handle footer (specified in source document as the required header)
                XPathNavigator requiredHeaderNav = xNav.SelectSingleNode("//a[@name='RequiredHeader']");
                if (requiredHeaderNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(new ProtocolSection(0,requiredHeaderNav.InnerXml, ProtocolSectionType.CTGovFooter));
                }

                // Detailed description
                XPathNavigator detailedDescNav = xNav.SelectSingleNode("//a[@name='DetailedDescription']");
                if (detailedDescNav != null)
                {
                    //TODO: REMOVE - This replacement is done for string comparison purpose
                    string html = detailedDescNav.InnerXml;
                    html = html.Replace("<a name=\"Section\" />", "");

                    detailedDescNav.InnerXml = html;
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, detailedDescNav.OuterXml,ProtocolSectionType.CTGovDetailedDescription));
                }

                // Entry criteria
                XPathNavigator entryCriteriaNav = xNav.SelectSingleNode("//a[@name='EntryCriteria']");
                if (entryCriteriaNav != null)
                {
                    //TODO: REMOVE - This replacement is done for string comparison purpose
                    string html = entryCriteriaNav.InnerXml;
                    html = html.Replace("<a name=\"Section\" />", "");

                    entryCriteriaNav.InnerXml = html;
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, entryCriteriaNav.OuterXml, ProtocolSectionType.CTGovEntryCriteria));
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Rendering CTGovProtocol section failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);
            }

        }

        #endregion CTGov

        #region Study/Trial Sites

        /// <summary>
        /// Method to parse study site information from the HTML.  
        /// This method is modified from the previous version of the GateKeeper
        /// </summary>
        /// <param name="studySitesNav"></param>
        /// <remarks>
        /// * Here is the deal with the study sites.
        /// *	 
        /// *  1.  The location of the study sites is actually the postal
        ///	*		address of the contacts under it.
        /// * 
        /// *  2.  This means that contact addresses can actually be anywhere
        /// *		and possibly no where close to the study organization.
        /// * 
        /// *		(So JHU can actually show up under FL)
        /// * 
        /// *  3.  What we want to do is to prerender study sites, and key them
        /// *		by Country, State, City.  Also we will include a list of 
        /// *		references to contacts to get names.  We also will include a
        /// *		list of zip codes.
        /// * 
        /// *  4.  We then will be able to search by zip, contact name, org name,
        /// *		state, city... blah blah blah
        /// </remarks>
        private void ParseStudySites(XPathNavigator studySitesNav)
        {
            if (studySitesNav != null)
            {
                try
                {
                    XPathNodeIterator locationIter = studySitesNav.Select("//locationData/location");
                    foreach (XPathNavigator location in locationIter)
                    {
                        // These nodes are always created by the XSL, but the value may be empty.
                        XPathNavigator cityNode = location.SelectSingleNode("city");
                        XPathNavigator stateNode = location.SelectSingleNode("politicalSubUnitName");
                        XPathNavigator countryNode = location.SelectSingleNode("country");
                        XPathNavigator facilityNode = location.SelectSingleNode("facilityName");
                        XPathNavigator htmlNode = location.SelectSingleNode("contactHTML");

                        // Because the node will always exist, there's no need to check for nulls.
                        string city = cityNode.Value;
                        string state = stateNode.Value;
                        string country = countryNode.Value;
                        string facilityName = facilityNode.Value;
                        string facilityRef = facilityNode.GetAttribute("siteRef", string.Empty);
                        string html = htmlNode.Value;
                        string contactKey = location.GetAttribute("contactKey", string.Empty);

                        AddContactInfoHTML(html, facilityRef, facilityName, city, state, country, contactKey);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Rendering Error: Rendering protocol contact info HTML failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);
                }
            }
        }

        /// <summary>
        /// Method to add a study site to the protocol contact info.
        /// </summary>
        /// <param name="contactHTML"></param>
        /// <param name="siteRef"></param>
          public void AddContactInfoHTML(string contactHTML, string siteRef, string siteName, string city, string state, string country, string contactKey)
          {
            try
            {
                int orgID = 0; 
                if (siteRef.Trim().Length > 0)
                    orgID = Int32.Parse(CDRHelper.ExtractCDRID(siteRef.Trim()));
                int key = Int32.Parse(contactKey);
                
                              
                foreach (ProtocolContactInfo contact in this._protocolDocument.ContactInfoList)
                {
                    if ((contact.OrganizationID == orgID || 
                        (contact.OrganizationName.Trim() == siteName.Trim() && contact.State == state && contact.City == city && contact.Country == country) ||
                        contact.OrganizationRole == ((int)OrganizationRoleType.Person).ToString()) && 
                        contact.ContactInfoKey == key && !contact.IsLeadOrg)
                    {
                        contact.Html = contactHTML;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Adding study contact info HTML failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);
            }

        }

        #endregion Study/Trial Sites

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Method to render the protocol document.
        /// </summary>
        /// <param name="document"></param>
        public override void Render(Document document)
        {
            try
            {
                if (document is ProtocolDocument)
                {
                    this._protocolDocument = (ProtocolDocument)document;

                    RenderCTGovProtocol();
                }
                else
                {
                    throw new Exception("Rendering Error: Document passed to Render(Document document) is not a protocol document. Document CDRID=" + _protocolDocument.DocumentID.ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Render data from Protocol Document failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);
            }

        }

        #endregion
    }
}
