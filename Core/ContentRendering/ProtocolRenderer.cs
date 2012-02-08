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

        #region Patient

        /// <summary>
        /// Main method for rendering/parsing the patient version XSL output.
        /// </summary>
        private void RenderPatientVersion()
        {
            try
            {
                string xslPath = ConfigurationManager.AppSettings["ProtocolPatient"];
                FileInfo xslFile = new FileInfo(xslPath);
                if (!xslFile.Exists)
                {
                    throw new Exception("Rendering Error: XSLT file for rendering protocol patient version does not exist. Relateive path = " + xslPath + ".");
                }

                base.LoadTransform(xslFile);
                base.Render(this._protocolDocument);
                // Perform post XSL processing...
                XPathNavigator xNav = this._protocolDocument.PostRenderXml.CreateNavigator();
                // Parse main sections 
                ParsePatientSections(xNav);
                // Handle StudySites
                ParseStudySites(xNav.SelectSingleNode("//a[@name = 'SitesAndContacts']"));
                this._protocolDocument.PatientXML.LoadXml(xNav.OuterXml);
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Render protocol patient version failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Method to parse the Patient sections from the XSL output.
        /// </summary>
        /// <param name="xNav"></param>
        /// <remarks>Note: Each HTML/XML fragment extracted by this routine 
        /// corresponds to a section in the final document.</remarks>
        private void ParsePatientSections(XPathNavigator xNav)
        {
            try
            {
                // LastMod
                XPathNavigator lastModNav = xNav.SelectSingleNode("//P");
                if (lastModNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, lastModNav.OuterXml, ProtocolSectionType.LastMod));
                }

                // SpecialCategory 
                XPathNavigator specialCategoryNav = xNav.SelectSingleNode("//a[starts-with(@name, 'SpecialCategory:')]");
                if (specialCategoryNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, specialCategoryNav.OuterXml, ProtocolSectionType.SpecialCategory));
                }

                // PatientAbstract
                XPathNavigator patientAbstractNav = xNav.SelectSingleNode("//a[starts-with(@name, 'TrialDescription:')]");
                string patientAbstract = patientAbstractNav.OuterXml;

                if (patientAbstractNav != null && 
                    (patientAbstractNav.InnerXml.Trim().Length > patientAbstractNav.SelectSingleNode("//a[starts-with(@name, 'TrialDescription_')]").OuterXml.Trim().Length))

                {

                    XPathNavigator patientDisclaimerNav = patientAbstractNav.SelectSingleNode("//a[@name = 'Disclaimer']");
                    if (patientDisclaimerNav != null)
                    {
                        //TODO: REMOVE - This replacement is done for string comparison purpose
                        string html = patientDisclaimerNav.InnerXml;
                        patientDisclaimerNav.InnerXml = html.Replace("<a name=\"Section_ProtPatientDisclaimer_4\" />", "<a name=\"Section_ProtPatientDisclaimer_4\"></a>");
                        // Add patient disclaimer to section list
                        this._protocolDocument.ProtocolSectionList.Add(new ProtocolSection(0, patientDisclaimerNav.OuterXml, ProtocolSectionType.PDisclaimer));
                        // Remove patient disclaimer in the patient abstract node 
                        patientDisclaimerNav.DeleteSelf();
                    }
                    // Add patient abstract to section
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, patientAbstractNav.OuterXml, ProtocolSectionType.PatientAbstract));

                    // Add patient disclaimer back for CDRPreview purpose
                    patientAbstractNav.OuterXml = patientAbstract;
                }

                 // ProtocolRelatedLinks
                XPathNavigator protocolRelatedLinksNav = xNav.SelectSingleNode("//a[starts-with(@name,  'ProtocolRelatedLinks:')]");
                if (protocolRelatedLinksNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, protocolRelatedLinksNav.OuterXml, ProtocolSectionType.PatientProtocolRelatedLinks));
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Parse protocol patient section failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);
            }
        }

        #endregion Patient

        #region Health Professional

        /// <summary>
        /// Main method for rendering/parsing the health professional (HP) version XSL output.
        /// </summary>
        private void RenderHPVersion()
        {
            try
            {
                string xslPath = ConfigurationManager.AppSettings["ProtocolHP"];
                FileInfo xslFile = new FileInfo(xslPath);
                if (!xslFile.Exists)
                {
                    throw new Exception("Rendering Error: XSLT file for rendering protocol HP version does not exist. Relateive path = " + xslPath + ".");
                }
                
                base.LoadTransform(xslFile);
                base.Render(this._protocolDocument);
                // Perform post XSL processing...
                XPathNavigator xNav = this._protocolDocument.PostRenderXml.CreateNavigator();
                ParseHPSections(xNav);
                // Handle StudySites
                ParseStudySites(xNav.SelectSingleNode("//a[@name = 'SitesAndContacts']"));
                this._protocolDocument.Xml.LoadXml(xNav.OuterXml);
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Render protocol HP version failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);

            }
        }

        /// <summary>
        /// Method to parse the HP sections from the XSL output.
        /// </summary>
        /// <param name="xNav"></param>
        /// <remarks>Note: Each HTML/XML fragment extracted by this routine 
        /// corresponds to a section in the final document.</remarks>
        private void ParseHPSections(XPathNavigator xNav)
        {
            try{
                // HPDisclaimer
                XPathNavigator disclaimerNav = xNav.SelectSingleNode("//a[@name='Disclaimer']");
                if (disclaimerNav != null)
                {
                    // TODO: REMOVE - This need to be removed after string comparison. 
                    string html = disclaimerNav.InnerXml;
                    disclaimerNav.InnerXml = html.Replace("<a name=\"Section_ProtHPDisclaimer_5\" />", "<a name=\"Section_ProtHPDisclaimer_5\"></a>");
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, disclaimerNav.OuterXml, ProtocolSectionType.HPDisclaimer));
                }

                // HPObjectives
                XPathNavigator objectivesNav = xNav.SelectSingleNode("//a[starts-with(@name, 'Objectives:')]");
                if (objectivesNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, objectivesNav.OuterXml, ProtocolSectionType.Objectives));
                }

                // HPEntryCriteria
                XPathNavigator entryCriteriaNav = xNav.SelectSingleNode("//a[starts-with(@name, 'EntryCriteria:')]");
                if (entryCriteriaNav != null)
                {
                   this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, entryCriteriaNav.OuterXml, ProtocolSectionType.EntryCriteria));
                }

                // HPExpectedEnrollment
                XPathNavigator expectedEnrollmentNav = xNav.SelectSingleNode("//a[starts-with(@name, 'ExpectedEnrollment:')]");
                if (expectedEnrollmentNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, expectedEnrollmentNav.OuterXml, ProtocolSectionType.ExpectedEnrollment));
                }

                // HPOutline
                XPathNavigator outlineNav = xNav.SelectSingleNode("//a[starts-with(@name, 'Outline:')]");
                if (outlineNav != null)
                {
                   this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, outlineNav.OuterXml, ProtocolSectionType.Outline));
                }

                // HPPubResults
                XPathNavigator publishedResultsNav = xNav.SelectSingleNode("//a[starts-with(@name, 'PublishedResults:')]");
                if (publishedResultsNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, publishedResultsNav.OuterXml, ProtocolSectionType.PublishedResults));
                }

                // HPLeadOrgs
                XPathNavigator leadOrgsNav = xNav.SelectSingleNode("//a[starts-with(@name, 'LeadOrgs:')]");
                if (leadOrgsNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, leadOrgsNav.OuterXml, ProtocolSectionType.LeadOrgs));
                }

                // HPProtocolRelatedLinks
                XPathNavigator protocolRelatedLinksNav = xNav.SelectSingleNode("//a[starts-with(@name, 'ProtocolRelatedLinks:')]");
                if (protocolRelatedLinksNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, protocolRelatedLinksNav.OuterXml, ProtocolSectionType.HPProtocolRelatedLinks));
                }
                // Outcomes
                XPathNavigator outcomesNav = xNav.SelectSingleNode("//a[starts-with(@name, 'Outcomes:')]");
                if (outcomesNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, outcomesNav.OuterXml, ProtocolSectionType.Outcomes));
                }

                // RegistryInfo 
                XPathNavigator registryInfoNav = xNav.SelectSingleNode("//a[starts-with(@name, 'RegistryInfo:')]");
                if (registryInfoNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, registryInfoNav.OuterXml, ProtocolSectionType.RegistryInformation));
                }

                // RelatedPublications 
                XPathNavigator relatedPublicationsNav = xNav.SelectSingleNode("//a[starts-with(@name, 'RelatedPublications:')]");
                if (relatedPublicationsNav != null)
                {
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, relatedPublicationsNav.OuterXml, ProtocolSectionType.RelatedPublications));
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Parse protocol HP section failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);
            }
        }

        #endregion Health Professional

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
                    html = html.Replace("<a name=\"Section\" />", "<a name=\"Section\"></a>");

                    summaryNav.InnerXml = "<a name=\"Objectives_" + strCDRID + "\"></a>" + html;
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
                        tempHtml.Replace("http://www.nlm.nih.gov/contacts/custserv-email.html", "<a href=\"http://www.nlm.nih.gov/contacts/custserv-email.html\">http://www.nlm.nih.gov/contacts/custserv-email.html</a>"),
                        ProtocolSectionType.CTGovDisclaimer));
                }

                // LeadOrgs
                XPathNavigator leadOrgsNav = xNav.SelectSingleNode("//a[@name='LeadOrgs']");
                if (leadOrgsNav != null)
                {
                    leadOrgsNav.InnerXml = "<a name=\"LeadOrgs_" + strCDRID + "\"></a>" + leadOrgsNav.InnerXml;
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
                    html = html.Replace("<a name=\"Section\" />", "<a name=\"Section\"></a>");

                    detailedDescNav.InnerXml = "<a name=\"Outline_" + strCDRID + "\"></a>" + html;
                    this._protocolDocument.ProtocolSectionList.Add(
                        new ProtocolSection(0, detailedDescNav.OuterXml,ProtocolSectionType.CTGovDetailedDescription));
                }

                // Entry criteria
                XPathNavigator entryCriteriaNav = xNav.SelectSingleNode("//a[@name='EntryCriteria']");
                if (entryCriteriaNav != null)
                {
                    //TODO: REMOVE - This replacement is done for string comparison purpose
                    string html = entryCriteriaNav.InnerXml;
                    html = html.Replace("<a name=\"Section\" />", "<a name=\"Section\"></a>");

                    entryCriteriaNav.InnerXml = "<a name=\"EntryCriteria_" + strCDRID + "\"></a>" + html;
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
            try
            {
                if (studySitesNav != null)
                {
                    XPathNavigator tableNav = studySitesNav.SelectSingleNode("P/table");
                    XmlNode studySiteNode = ((IHasXmlNode)tableNav).GetNode();
                    StringBuilder siteHtmlBuffer = new StringBuilder();
                    string city = string.Empty;
                    string newCity = string.Empty;
                    string state = string.Empty;
                    string newState = string.Empty;
                    string country = string.Empty;
                    string newCountry = string.Empty;
                    string siteName = string.Empty;
                    string newSiteName = string.Empty;
                    string siteRef = string.Empty;
                    string personRef = string.Empty;
                    // Unique key to hold the person's contact info
                    string contactKey = string.Empty;
                    bool addSpace = true;
                    bool bCity = false;
                    bool bState = false;
                    bool bCountry = false;

                    foreach (XmlNode xnTableRow in studySiteNode.ChildNodes)
                    {
                        switch (xnTableRow.Attributes[0].Name.ToLower())
                        {
                            case "space":
                                if (addSpace)
                                {
                                    siteHtmlBuffer.Append("<tr>");
                                    siteHtmlBuffer.Append(xnTableRow.InnerXml);
                                    siteHtmlBuffer.Append("</tr>");
                                }
                                else
                                {
                                    xnTableRow.RemoveAll();
                                }
                                break;
                            case "country":
                                addSpace = false;
                                newCountry = xnTableRow.ChildNodes[0].InnerText;
                                if ((newCountry.Trim().Length > 0) && (newCountry != country))
                                {
                                    country = newCountry;
                                    bCountry = true;
                                    addSpace = true;
                                }
                                else
                                {
                                    xnTableRow.RemoveAll();
                                }
                                break;
                            case "state":
                                addSpace = false;
                                newState = xnTableRow.ChildNodes[0].InnerText;
                                if (newState.Trim() == string.Empty && xnTableRow.ChildNodes[1] != null)
                                    newState = xnTableRow.ChildNodes[1].InnerText;
                                if (newState != null && newState.Trim().Length > 0 && newState != state)
                                {
                                    state = newState;
                                    bState = true;
                                    addSpace = true;
                                }
                                else
                                {
                                    xnTableRow.RemoveAll();
                                }
                                break;
                            case "city":
                                addSpace = false;
                                newCity = xnTableRow.ChildNodes[1].InnerText;
                                if ((newCity.Trim().Length > 0) && (newCity != city))
                                {
                                    city = newCity;
                                    bCity = true;
                                    addSpace = true;
                                }
                                else
                                {
                                    xnTableRow.RemoveAll();
                                }
                                break;
                            case "facility":
                                addSpace = false;
                                newSiteName = xnTableRow.ChildNodes[1].InnerText.Trim();
                                if  (newSiteName != siteName|| bCity || bState || bCountry ) 
                                {
                                    if (xnTableRow.Attributes.Count > 0)
                                    {
                                        siteRef = xnTableRow.Attributes[1].Value;
                                    }
                                    else
                                    {
                                        siteRef = string.Empty;
                                    }
                                    siteName = newSiteName;
                                    siteHtmlBuffer.Append("<tr>");
                                    siteHtmlBuffer.Append(xnTableRow.InnerXml);
                                    siteHtmlBuffer.Append("</tr>\n");
                                    addSpace = true;
                                }
                                else
                                {
                                    xnTableRow.RemoveAll();
                                }
                                break;
                            case "name":
                                personRef = xnTableRow.Attributes[1].Value;
                                siteHtmlBuffer.Append("<tr>");
                                siteHtmlBuffer.Append(xnTableRow.InnerXml);
                                siteHtmlBuffer.Append("</tr>\n");
                                addSpace = true;
                                break;
                            case "email":
                                siteHtmlBuffer.Append("<tr>");
                                siteHtmlBuffer.Append(xnTableRow.InnerXml);
                                siteHtmlBuffer.Append("</tr>\n");
                                addSpace = true;
                                break;
                            case "contact":
                                if (siteHtmlBuffer.Length > 0)
                                {
                                    contactKey = xnTableRow.Attributes[0].Value;
                                    AddContactInfoHTML(siteHtmlBuffer.ToString(), siteRef, siteName, city, state, country, contactKey);
                                    siteHtmlBuffer.Remove(0, siteHtmlBuffer.Length);
                                    contactKey = string.Empty;
                                }
                                bCountry = false;
                                bState = false;
                                bCity = false;
                                xnTableRow.RemoveAll();
                                break;
                            default:
                                // Ignore unknown node 
                                break;
                        } // End switch
                    } // End foreach
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Rendering protocol contact info HTML failed. Document CDRID=" + _protocolDocument.DocumentID.ToString(), e);
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

                    if (this._protocolDocument.ProtocolType == ProtocolType.Protocol)
                    {
                        // The patient version and HP version of the protocol are both 
                        // generated from the same source document. The pertinent sections 
                        // are parsed from both versions and save to the ProtocolSection table 
                        // for the same ProtocolID and the CG front-end pulls the appropriate
                        // set of sections from the table for display based on the 
                        // version (Patient vs. HP) requested by the user.
                        RenderPatientVersion();
                        RenderHPVersion();

                    }
                    else
                    {
                        // Only one XSL is used to generate both the patient and HP versions
                        // of the CTGov protocols.
                       RenderCTGovProtocol();
                    }
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
