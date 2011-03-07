using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using GateKeeper.Common;
using GateKeeper.Common.XPathKeys;
using GateKeeper.Logging;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Protocol;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Class to extract a CTGovProtocol object from XML.
    /// </summary>
    public class CTGovProtocolExtractor : ProtocolExtractorBase
    {
        #region Field
        private int _documentID;
        #endregion

        #region Private Methods

        public CTGovProtocolExtractor(DocumentXPathManager xPath)
            : base(xPath)
        {
        }

        /// <summary>
        /// Extract misc metadata.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="ctgovProtocol"></param>
        private void ExtractMetadata(XPathNavigator xNav, ProtocolDocument ctgovProtocol, ProtocolExtractor protExtractor)
        {
            string path = XPathManager.GetXPath(ProtocolXPath.StudyCategory);
            try
            {

                XPathNodeIterator studyCategoryIter = xNav.Select(path);
                while (studyCategoryIter.MoveNext())
                {
                    ctgovProtocol.StudyCategoryList.Add(studyCategoryIter.Current.Value.Trim());
                }

                // Extract title information
                path = XPathManager.GetXPath(ProtocolXPath.CTBriefTitle);
                ProtocolTitle briefTitle = ExtractTitle(xNav, path);
                briefTitle.AudienceType = "BriefTitle";
                ctgovProtocol.HealthProfessionalTitle = briefTitle.Title;
                ctgovProtocol.PatientTitle = briefTitle.Title;

                path = XPathManager.GetXPath(ProtocolXPath.CTOfficialTitle);
                ProtocolTitle officialTitle = ExtractTitle(xNav, path);
                officialTitle.AudienceType = "OfficialTitle";

                path = XPathManager.GetXPath(ProtocolXPath.Phase);
                XPathNodeIterator phaseIter = xNav.Select(path);
                while (phaseIter.MoveNext())
                {
                    ctgovProtocol.ProtocolPhaseList.Add(protExtractor.IntPhase(phaseIter.Current.Value.Trim()));
                }

                // Extract protocol special categories
                path = XPathManager.GetXPath(ProtocolXPath.SpecialCategory);
                XPathNodeIterator specialCategoryIter = xNav.Select(path);
                while (specialCategoryIter.MoveNext())
                {
                    string specialCategoryValue = specialCategoryIter.Current.Value.Trim();
                    ctgovProtocol.SpecialCategoryList.Add(specialCategoryValue);
                    // Check if the protocol is NIH clinical center trial
                    if (specialCategoryValue.Equals("NIH Clinical Center trial"))
                        ctgovProtocol.IsNIHClinicalTrial = 1;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed. Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract title.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="basePath"></param>
        private ProtocolTitle ExtractTitle(XPathNavigator xNav, string basePath)
        {

           ProtocolTitle protocolTitle = new ProtocolTitle();

           XPathNavigator protocolTitleElement = xNav.SelectSingleNode(basePath);

           if (protocolTitleElement != null)
           {
               protocolTitle.Title = protocolTitleElement.Value;
               protocolTitle.AudienceType = DocumentHelper.GetAttribute(protocolTitleElement, XPathManager.GetXPath(ProtocolXPath.TitleAudience));
           }
           return protocolTitle;
        }

        /// <summary>
        /// Extracts the IDStrings and types for alternate protocol IDs.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="alternateIDList"></param>
        /// <remarks>Note: Due to specialized logic (for AlternateProtocolID.Type), <br/>
        /// ProtocolExtractHelper.ExtractAlternateIDs() was not used for CTGov Protocol</remarks>
        private void ExtractAlternateIDs(XPathNavigator xNav, List<AlternateProtocolID> alternateIDList)
        {
            string path = XPathManager.GetXPath(ProtocolXPath.CTOrgStudyID);
            try{
                XPathNodeIterator alternateIDIter = xNav.Select(path);

                while (alternateIDIter.MoveNext())
                {
                    AlternateProtocolID altProtocolID = new AlternateProtocolID();
                    altProtocolID.IdString = alternateIDIter.Current.Value.Trim();
                    altProtocolID.Type = "Primary";
                    alternateIDList.Add(altProtocolID);
                }

                path = XPathManager.GetXPath(ProtocolXPath.CTSecondaryID);
                alternateIDIter = xNav.Select(path);

                while (alternateIDIter.MoveNext())
                {
                    AlternateProtocolID altProtocolID = new AlternateProtocolID();
                    altProtocolID.IdString = alternateIDIter.Current.Value.Trim();
                    altProtocolID.Type = "Secondary";
                    alternateIDList.Add(altProtocolID);
                }

                path = XPathManager.GetXPath(ProtocolXPath.CTNCIID);
                alternateIDIter = xNav.Select(path);

                while (alternateIDIter.MoveNext())
                {
                    AlternateProtocolID altProtocolID = new AlternateProtocolID();
                    altProtocolID.IdString = alternateIDIter.Current.Value.Trim();
                    altProtocolID.Type = "NCTID";
                    alternateIDList.Add(altProtocolID);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed. Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Parse CTGov Protocol site contact information.
        /// </summary>
        /// <param name="contactNav"></param>
        /// <param name="facility"></param>
        /// <param name="contactKey"></param>
        /// <param name="protExtractor"></param>
        /// <returns></returns>

        private ProtocolContactInfo ExtractSiteContact(XPathNavigator contactNav, ProtocolContactInfo facility, ref int contactKey, ProtocolExtractor protExtractor)
        {
            ProtocolContactInfo ci = new ProtocolContactInfo();
            string path = XPathManager.GetXPath(ProtocolXPath.CTGivenName);
           
            try{
                // Set organization information parsed from the ProtocolLeadOrg node parsed above:
                ci.OrganizationID = facility.OrganizationID;
                ci.OrganizationName = facility.OrganizationName;
                ci.OrganizationRole = facility.OrganizationRole;
                ci.City = facility.City;
                ci.State = facility.State;
                ci.StateID = facility.StateID;
                ci.Country = facility.Country;
                ci.PostalCodeZip = facility.PostalCodeZip;
                
                int tempID = 0;
                string tempIDString = DocumentHelper.GetAttribute(contactNav, XPathManager.GetXPath(ProtocolXPath.SitePersonRef));
                if (tempIDString.Length > 0)
                {
                    if (Int32.TryParse(CDRHelper.ExtractCDRID(tempIDString), out tempID))
                    {
                        ci.PersonID = tempID;
                    }
                    else
                    {
                        throw new Exception("Extraction Error: CTGovProtocol site contact attribute " + XPathManager.GetXPath(ProtocolXPath.SitePersonRef) + " should be a valid CDR ID. CurrentValue=" + tempIDString + ". Document CDRID=" + _documentID.ToString());
                    }
                }

                ci.PersonGivenName = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTSurName);
                ci.PersonSurName = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTSuffix);
                ci.PersonProfessionalSuffix = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTRole);
                ci.PersonRole = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTPhone);
                ci.PhoneNumber = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTPhoneExt);
                ci.PhoneExtension = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                // Unique id to keep track of each contact info record
                ci.ContactInfoKey = contactKey++;
                // also create it in the attribute for passing the value into render transformation
                contactNav.CreateAttribute("", "site", "", ci.ContactInfoKey.ToString());
                ci.Xml.LoadXml(contactNav.OuterXml);
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed. Document CDRID=" + _documentID.ToString(), e);
            }

            return ci;
        }


        /// <summary>
        /// Parse CTGov Protocol Lead Sponsor contact information.
        /// </summary>
        /// <param name="contactNav"></param>
        /// <param name="organizationID"></param>
        /// <param name="organizationName"></param>
        /// <param name="organizationRole"></param>
        /// <param name="protExtractor></param>
        /// <returns></returns>
        /// <example>
        ///    <OverallContact>
        ///      <GivenName>Erlinda</GivenName>
        ///      <MiddleInitial>M.</MiddleInitial>
        ///      <SurName>Gordon</SurName>
        ///      <ProfessionalSuffix>M.D.</ProfessionalSuffix>
        ///      <Phone>323-442-2527</Phone>
        ///      <Email>FAKEEMAIL@FAKEEMAIL.FAKEEMAIL</Email>
        ///    </OverallContact>
        /// </example>
        private ProtocolContactInfo ExtractLeadContact(XPathNavigator contactNav, int organizationID, string organizationName, string organizationRole, ProtocolExtractor protExtractor)
        {
            ProtocolContactInfo ci = new ProtocolContactInfo();
            string path = XPathManager.GetXPath(ProtocolXPath.CTGivenName);
            try {
                 // Set organization information parsed from the ProtocolLeadOrg node parsed above:
                ci.OrganizationID = organizationID;
                ci.OrganizationName = organizationName;
                ci.OrganizationRole = organizationRole;
                ci.IsLeadOrg = true;

                int tempID = 0;
                string tempIDString = DocumentHelper.GetAttribute(contactNav, XPathManager.GetXPath(ProtocolXPath.LeadOrgRef));
                if (tempIDString.Length > 0)
                {
                    if (Int32.TryParse(CDRHelper.ExtractCDRID(tempIDString), out tempID))
                    {
                        ci.PersonID = tempID;
                    }
                    else
                    {
                        throw new Exception("Extraction Error: CTGovProtocol lead contact attribute " + XPathManager.GetXPath(ProtocolXPath.LeadOrgRef) + " should be a valid CDR ID. CurrentValue=" + tempIDString + ". Document CDRID=" + _documentID.ToString());
                    }
                }

                ci.PersonGivenName = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTSurName);
                ci.PersonSurName = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTSuffix);
                ci.PersonProfessionalSuffix = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTRole);
                ci.PersonRole = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTPhone);
                ci.PhoneNumber = DocumentHelper.GetXmlDocumentValue(contactNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.CTPhoneExt);
                ci.PhoneExtension = DocumentHelper.GetXmlDocumentValue(contactNav, path);
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed. Document CDRID=" + _documentID.ToString(), e);
            }
            return ci;
        }


        /// <summary>
        /// Extract lead organization(s).
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="contactInfoList"></param>
        /// <param name="basePath"></param>
        /// <param name="protExtractor></param>
        /// <example>
        /// <Sponsors>
        ///    <PDQSponsorship>Other</PDQSponsorship>
        ///    <LeadSponsor ref="CDR0000346479">Erlinda M. Gordon, MD</LeadSponsor>
        ///    <Collaborator ref="CDR0000339937">Epeius Biotechnologies</Collaborator>
        ///    <Collaborator ref="CDR0000030646">USC/Norris Comprehensive Cancer Center and Hospital</Collaborator>
        ///    <OverallContact>
        ///      <GivenName>Erlinda</GivenName>
        ///      <MiddleInitial>M.</MiddleInitial>
        ///      <SurName>Gordon</SurName>
        ///      <ProfessionalSuffix>M.D.</ProfessionalSuffix>
        ///      <Phone>323-442-2527</Phone>
        ///      <Email>FAKEEMAIL@FAKEEMAIL.FAKEEMAIL</Email>
        ///    </OverallContact>
        ///    <OverallContactBackup>
        ///      <GivenName>Frederick</GivenName>
        ///      <MiddleInitial>L.</MiddleInitial>
        ///      <SurName>Hall</SurName>
        ///      <ProfessionalSuffix>Ph.D.</ProfessionalSuffix>
        ///      <Phone>323-442-1548</Phone>
        ///      <Email>FAKEEMAIL@FAKEEMAIL.FAKEEMAIL</Email>
        ///    </OverallContactBackup>
        ///  </Sponsors>
        /// </example>
        private void ExtractLeadSponsor(XPathNavigator xNav, List<ProtocolContactInfo> contactInfoList, string basePath, ProtocolExtractor protExtractor)
        {
            string path = XPathManager.GetXPath(ProtocolXPath.CTLeadSponsor);
            try {
                XPathNavigator sponsorsNav = xNav.SelectSingleNode(basePath);
                if (sponsorsNav != null)
                {
                    int organizationID = 0;
                    string tempOrganizationID = string.Empty;
                    string organizationName = string.Empty;
                    bool hasContact = false;

                    // Parse Lead Org record(s)...
                    XPathNavigator leadSponsorNav = sponsorsNav.SelectSingleNode(path);
                    if (leadSponsorNav != null)
                    {
                        organizationName = leadSponsorNav.Value.Trim();
                        tempOrganizationID = CDRHelper.ExtractCDRID(DocumentHelper.GetAttribute(leadSponsorNav, XPathManager.GetXPath(ProtocolXPath.LeadOrgRef)));
                        if (!Int32.TryParse(CDRHelper.ExtractCDRID(tempOrganizationID), out organizationID) && tempOrganizationID.Length > 0)
                        {
                            throw new Exception("Extraction Error: CTGovProtocol lead sponsor attribute " + XPathManager.GetXPath(ProtocolXPath.LeadOrgRef) + " should be a CDRID. CurrentValue=" + tempOrganizationID + ". Document CDRID=" + _documentID.ToString());
                        }
                    }

                     // Parse ProtPerson records...
                    path = XPathManager.GetXPath(ProtocolXPath.CTOverallContact);
                    XPathNodeIterator overallContactIter = sponsorsNav.Select(path);
                    while (overallContactIter.MoveNext())
                    {
                        contactInfoList.Add(ExtractLeadContact(overallContactIter.Current, organizationID, organizationName, ((int)OrganizationRoleType.Primary).ToString(), protExtractor));
                        hasContact = true;
                    }

                    // Parse ProtPerson records...
                    path = XPathManager.GetXPath(ProtocolXPath.CTOverallBackup);
                    XPathNodeIterator overallContactBackupIter = sponsorsNav.Select(path);
                    while (overallContactBackupIter.MoveNext())
                    {
                        contactInfoList.Add(ExtractLeadContact(overallContactBackupIter.Current, organizationID, organizationName, ((int)OrganizationRoleType.Primary).ToString(), protExtractor));
                        hasContact = true;
                    }

                    // Parse ProtPerson records...
                    path = XPathManager.GetXPath(ProtocolXPath.CTOfficial);
                    XPathNodeIterator overallOfficialIter = sponsorsNav.Select(path);
                    while (overallOfficialIter.MoveNext())
                    {
                        contactInfoList.Add(ExtractLeadContact(overallOfficialIter.Current, organizationID, organizationName, ((int)OrganizationRoleType.Primary).ToString(), protExtractor));
                        hasContact = true;
                    }

                    // If no detailed person contact information available for this lead org,
                    // add the lead org info into the contact list
                    if (!hasContact && organizationName.Trim() != string.Empty)
                    {
                        ProtocolContactInfo leadCi = new ProtocolContactInfo();
                        leadCi.OrganizationID = organizationID;
                        leadCi.OrganizationName = organizationName;
                        leadCi.OrganizationRole = ((int)OrganizationRoleType.Primary).ToString();
                        leadCi.IsLeadOrg = true;
                        contactInfoList.Add(leadCi);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed. Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract protocol sites.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="basePath"></param>
        private ProtocolContactInfo ExtractFacility(XPathNavigator xNav, ref int contactKey,  string basePath, ProtocolExtractor protExtractor)
        {
            ProtocolContactInfo facilityCi = new ProtocolContactInfo();
            string path = XPathManager.GetXPath(ProtocolXPath.CTFacilityName);
            try {
                XPathNavigator facilityNav = xNav.SelectSingleNode(basePath);
                if (facilityNav != null)
                {
                    facilityCi.OrganizationName = DocumentHelper.GetXmlDocumentValue(facilityNav, path);
                    facilityCi.OrganizationRole = "3";
                    int organizationID = 0;
                    string tempOrganizationID = DocumentHelper.GetAttribute(facilityNav, path, XPathManager.GetXPath(ProtocolXPath.SiteRef));
                    if (tempOrganizationID.Length > 0)
                    {
                        if (Int32.TryParse(CDRHelper.ExtractCDRID(tempOrganizationID), out organizationID))
                        {
                            facilityCi.OrganizationID = organizationID;
                        }
                        else
                        {
                            throw new Exception("Extraction Error: CTGov protocol " + path  + "/@" + XPathManager.GetXPath(ProtocolXPath.SiteRef) + " should be a valid CDRID. CurrentValue=" + tempOrganizationID + ". Document CDRID=" + _documentID.ToString());
                        }
                    }


                    path = XPathManager.GetXPath(ProtocolXPath.CTState);
                    string tempStateID = DocumentHelper.GetAttribute(facilityNav, path, XPathManager.GetXPath(ProtocolXPath.StateRef));
                    int stateID = 0;
                    if (tempStateID.Length > 0)
                    {
                        if (Int32.TryParse(CDRHelper.ExtractCDRID(tempStateID), out stateID))
                        {
                            facilityCi.StateID = stateID;
                        }
                        else
                        {
                            throw new Exception("Extraction Error: CTGov protocol " + path + "/@" + XPathManager.GetXPath(ProtocolXPath.StateRef) + " should be a CDRID. CurrentValue=" + tempOrganizationID + ". Document CDRID=" + _documentID.ToString());
                        }
                    }

                    facilityCi.State = DocumentHelper.GetXmlDocumentValue(facilityNav, path);
                    path = XPathManager.GetXPath(ProtocolXPath.CTCity);
                    facilityCi.City = DocumentHelper.GetXmlDocumentValue(facilityNav, path);
                    path = XPathManager.GetXPath(ProtocolXPath.CTCountry);
                    facilityCi.Country = DocumentHelper.GetXmlDocumentValue(facilityNav, path);
                    path = XPathManager.GetXPath(ProtocolXPath.CTZip);
                    facilityCi.PostalCodeZip = DocumentHelper.GetXmlDocumentValue(facilityNav, path);
                    // This is to handle the case that there is no contact under a facility, then the facility will become the point of contact
                    // to save into the ProtocolTrialSite and ProtocolContactInfoHTML table
                    facilityCi.ContactInfoKey = contactKey++;
                    facilityNav.CreateAttribute("", "site", "", facilityCi.ContactInfoKey.ToString());
                    facilityCi.Xml.LoadXml(facilityNav.OuterXml);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed. Document CDRID=" + _documentID.ToString(), e);
            }

            return facilityCi;
        }

        /// <summary>
        /// Extracts metadata for location(s).
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="contactInfoList"></param>
        /// <param name="basePath"></param>
        /// <example>
        /// <Location>
        ///    <Facility>
        ///      <FacilityName ref="CDR0000030646">USC/Norris Comprehensive Cancer Center and Hospital</FacilityName>
        ///      <PostalAddress>
        ///        <City>Los Angeles</City>
        ///        <PoliticalSubUnitName ref="CDR0000043861">California</PoliticalSubUnitName>
        ///        <CountryName ref="CDR0000043753">U.S.A.</CountryName>
        ///        <PostalCode_ZIP>90089</PostalCode_ZIP>
        ///        <PostalCodePosition>after PoliticalSubUnit_State</PostalCodePosition>
        ///      </PostalAddress>
        ///    </Facility>
        ///    <Status>Active</Status>
        ///    <CTGovContact>
        ///      <GivenName>Claire</GivenName>
        ///      <SurName>Hughlett</SurName>
        ///      <ProfessionalSuffix>R.N.</ProfessionalSuffix>
        ///      <Phone>323-865-0460</Phone>
        ///      <Email>FAKEEMAIL@FAKEEMAIL.FAKEEMAIL</Email>
        ///    </CTGovContact>
        ///    <Investigator ref="CDR0000017657">
        ///      <GivenName>Heinz-Josef</GivenName>
        ///      <SurName>Lenz</SurName>
        ///      <ProfessionalSuffix>M.D.</ProfessionalSuffix>
        ///      <Role>Principal Investigator</Role>
        ///    </Investigator>
        ///  </Location>
        /// </example>
        private void ExtractLocations(XPathNavigator xNav, List<ProtocolContactInfo> contactInfoList, string basePath, ref int contactKey, ProtocolExtractor protExtractor)
        {
            string path = basePath;
            try {
                XPathNodeIterator locationsIter = xNav.Select(basePath);

                while (locationsIter.MoveNext())
                {
                    path = XPathManager.GetXPath(ProtocolXPath.CTFacility);
                    ProtocolContactInfo facilityCi = ExtractFacility(locationsIter.Current, ref contactKey, path, protExtractor);
                    bool facilityContact = false;

                    path = XPathManager.GetXPath(ProtocolXPath.CTGovContact);
                    XPathNodeIterator contactIter = locationsIter.Current.Select(path);
                    while (contactIter.MoveNext())
                    {
                        contactInfoList.Add(ExtractSiteContact(contactIter.Current, facilityCi, ref contactKey, protExtractor));
                        facilityContact = true;
                    }

                    path = XPathManager.GetXPath(ProtocolXPath.CTGovContactBackup);
                    XPathNodeIterator contactBackupIter = locationsIter.Current.Select(path);
                    while (contactBackupIter.MoveNext())
                    {
                        contactInfoList.Add(ExtractSiteContact(contactBackupIter.Current, facilityCi, ref contactKey, protExtractor));
                        facilityContact = true;
                    }

                    path = XPathManager.GetXPath(ProtocolXPath.CTInvestigator);
                    XPathNodeIterator investigatorIter = locationsIter.Current.Select(path);
                    while (investigatorIter.MoveNext())
                    {
                        contactInfoList.Add(ExtractSiteContact(investigatorIter.Current, facilityCi, ref contactKey, protExtractor));
                        facilityContact = true;
                    }

                    // If no detailed person contact information available for this facility,
                    // add the facility info into the contact list
                    if (!facilityContact && facilityCi.OrganizationName.Trim().Length > 0)
                    {
                        contactInfoList.Add(facilityCi);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed. Document CDRID=" + _documentID.ToString(), e);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Modifies the document XML, so that subsequent processing is based on
        /// ideal input.
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PrepareXml(XmlDocument xmlDoc)
        {
            // TODO: Add code to "prepare" xml (fix problems with data) 
        }

        /// <summary>
        /// Main extract method.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="ctgovProtocol"></param>
        public void Extract(XmlDocument xmlDoc, ProtocolDocument ctgovProtocol)
        {
           try {
                ctgovProtocol.ProtocolType = ProtocolType.CTGov;
                ctgovProtocol.IsCTProtocol = 1;
                ctgovProtocol.DocumentType = DocumentType.CTGovProtocol;

                XPathNavigator xNav = xmlDoc.CreateNavigator();

                // Extract the CDR ID...
                xNav.MoveToFirstChild();
                if (CDRHelper.ExtractCDRID(xNav, XPathManager.GetXPath(CommonXPath.CDRID), out _documentID))
                {
                    ctgovProtocol.DocumentID = _documentID;
                    ctgovProtocol.ProtocolID = _documentID;
                }
                else
                {
                    throw new Exception("Extraction Error: Failed to extract document (CDR) ID in CTGov protocol document!");
                }

                DocumentHelper.CopyXml(xmlDoc, ctgovProtocol);

                // Extract misc metadata
                ProtocolExtractor protExtractor = new ProtocolExtractor(XPathManager);
                ExtractMetadata(xNav, ctgovProtocol, protExtractor);

                // Extract lead sponsors and locations
                int contactKey = 0;
                ExtractLeadSponsor(xNav, ctgovProtocol.ContactInfoList, XPathManager.GetXPath(ProtocolXPath.CTSponsor), protExtractor);
                ExtractLocations(xNav, ctgovProtocol.ContactInfoList, XPathManager.GetXPath(ProtocolXPath.CTLocation), ref contactKey, protExtractor);

                // Extract protocol alternate IDs
                ExtractAlternateIDs(xNav, ctgovProtocol.AlternateIDList);

                // Extract protocol Pretty URL IDs
                ExtractProtocolPrettyUrlIDs(xNav, ctgovProtocol);

                // Use common Protocol helper functions to parse common metadata

                // Extract sponsors 
                 protExtractor.ExtractSponsors(xNav, ctgovProtocol,  XPathManager.GetXPath(ProtocolXPath.CTPDQSponsor));

                // Extract 
                protExtractor.ExtractEligibility(xNav, ctgovProtocol, XPathManager.GetXPath(ProtocolXPath.Eligibility));

                // Extract protocol status

                protExtractor.ExtractStatus(xNav, ctgovProtocol, XPathManager.GetXPath(ProtocolXPath.CTGovStatus));

                // Extract type of cancer
                protExtractor.ExtractTypeOfCancer(xNav, ctgovProtocol.TypeOfCancerList, XPathManager.GetXPath(ProtocolXPath.CancerType1));
                protExtractor.ExtractTypeOfCancer(xNav, ctgovProtocol.TypeOfCancerList, XPathManager.GetXPath(ProtocolXPath.CancerType2));
                protExtractor.ExtractTypeOfCancer(xNav, ctgovProtocol.TypeOfCancerList, XPathManager.GetXPath(ProtocolXPath.CancerType3));
                protExtractor.ExtractTypeOfCancer(xNav, ctgovProtocol.TypeOfCancerList, XPathManager.GetXPath(ProtocolXPath.CancerType4));

                // Extract modalities
                protExtractor.ExtractModality(xNav, ctgovProtocol.ModalityList, XPathManager.GetXPath(ProtocolXPath.ModalityType1));
                protExtractor.ExtractModality(xNav, ctgovProtocol.ModalityList, XPathManager.GetXPath(ProtocolXPath.ModalityType2));
                protExtractor.ExtractModality(xNav, ctgovProtocol.ModalityList, XPathManager.GetXPath(ProtocolXPath.ModalityType3));

                // Extract Drugs
                protExtractor.ExtractDrug(xNav, ctgovProtocol.DrugList, XPathManager.GetXPath(ProtocolXPath.Durg));

                // Extract protocol term section
                string CDRID = xNav.GetAttribute(XPathManager.GetXPath(CommonXPath.CDRID), string.Empty);
                protExtractor.ExtractTermSection(xmlDoc, ctgovProtocol.ProtocolSectionList, CDRID, ProtocolSectionType.CTGovTerms);

                // Handle modified and published dates
                DocumentHelper.ExtractDates(xNav, ctgovProtocol, XPathManager.GetXPath(ProtocolXPath.CTLastModifiedDate), XPathManager.GetXPath(CommonXPath.FirstPublishedDate));

                // Save the modified xml into the business object
                ctgovProtocol.Xml.LoadXml(xNav.OuterXml);
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting CTGov protocol XML document failed. Document CDRID=" + _documentID.ToString(), e);
            }
        }

        #endregion
    }
}
