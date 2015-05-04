using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using GateKeeper.Common;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Protocol;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Class to extract summary object from XML.
    /// </summary>
    public class ProtocolExtractor : ProtocolExtractorBase
    {
        #region Fields
            private int _documentID;
            private int _contactCount;
         #endregion

          #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// 
        public ProtocolExtractor(DocumentXPathManager xPath)
            : base(xPath)
        {
        }

        #endregion

         #region Private Methods

        /// <summary>
        /// Extracts all supported protocol metadata.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="protocol"></param>
        private void ExtractMetadata(XPathNavigator xNav, ProtocolDocument protocol)
        {
            string path = XPathManager.GetXPath(ProtocolXPath.StudyCategory);
            try
            {
                // Extract protocol study category
                XPathNodeIterator studyIter = xNav.Select(path);
                while (studyIter.MoveNext())
                {
                    protocol.StudyCategoryList.Add(studyIter.Current.Value.Trim());
                }

                // Extract protocol phase
                path = XPathManager.GetXPath(ProtocolXPath.Phase);
                XPathNodeIterator phaseIter = xNav.Select(path);
                while (phaseIter.MoveNext())
                {
                    protocol.ProtocolPhaseList.Add(IntPhase(phaseIter.Current.Value.Trim()));
                }

                // Extract protocol special categories
                path = XPathManager.GetXPath(ProtocolXPath.SpecialCategory);
                XPathNodeIterator specialCategoryIter = xNav.Select(path);
                while (specialCategoryIter.MoveNext())
                {
                    string specialCategoryValue = specialCategoryIter.Current.Value.Trim();
                    protocol.SpecialCategoryList.Add(specialCategoryValue);
                    // Check if the protocol is NIH clinical center trial
                    if (specialCategoryValue.Equals("NIH Clinical Center trial"))
                        protocol.IsNIHClinicalTrial = 1;
                }

                // Extract title information
                ExtractTitle(xNav, protocol, XPathManager.GetXPath(ProtocolXPath.Title), XPathManager.GetXPath(ProtocolXPath.PDQTitle));
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extracts the IDStrings and types for alternate protocol IDs.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="alternateIDList"></param>
        /// <param name="basePath"></param>
        private void ExtractAlternateIDs(XPathNavigator xNav, List<AlternateProtocolID> alternateIDList, string basePath)
        {
            string path = basePath;
            try{
                XPathNodeIterator alternateIDIter = xNav.Select(basePath);

                while (alternateIDIter.MoveNext())
                {
                    AlternateProtocolID altProtocolID = new AlternateProtocolID();
                    path = XPathManager.GetXPath(ProtocolXPath.IDString);
                    if (alternateIDIter.Current.SelectSingleNode(path) != null)
                        altProtocolID.IdString = alternateIDIter.Current.SelectSingleNode(path).Value.Trim();

                    path = XPathManager.GetXPath(ProtocolXPath.IDType);
                    if (alternateIDIter.Current.SelectSingleNode(path) != null)
                        altProtocolID.Type = alternateIDIter.Current.SelectSingleNode(path).Value.Trim();
                    else
                        altProtocolID.Type = "Primary";

                    alternateIDList.Add(altProtocolID);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Common routine to parse protocol person (ProtPerson element).
        /// </summary>
        /// <param name="protPersonNav"></param>
        /// <param name="organizationID"></param>
        /// <param name="organizationName"></param>
        /// <param name="organizationRole"></param>
        /// <param name="isLeadOrg"></param>
        /// <returns></returns>
        private ProtocolContactInfo ExtractProtPerson(XPathNavigator protPersonNav, int organizationID, string organizationName, string organizationRole, bool isLeadOrg)
        {
            ProtocolContactInfo ci = new ProtocolContactInfo();
            string path = XPathManager.GetXPath(ProtocolXPath.GivenName);
            try{
               ci.Xml.LoadXml(protPersonNav.OuterXml);

                // Set organization information parsed from the ProtocolLeadOrg node parsed above:
                ci.OrganizationName = organizationName;
                ci.OrganizationRole = organizationRole;
                ci.IsLeadOrg = isLeadOrg;

                // Person ID is an optional field in DTD, this data could be missing in xml document
                int tempID = 0;
                string tempIDString = DocumentHelper.GetAttribute(protPersonNav, XPathManager.GetXPath(ProtocolXPath.SitePersonRef));
                if (tempIDString.Length > 0)
                {
                    if (Int32.TryParse(CDRHelper.ExtractCDRID(tempIDString), out tempID))
                    {
                        ci.PersonID = tempID;
                        tempID = 0;
                    }
                    else
                    {
                        throw new Exception("Extraction Error: " + XPathManager.GetXPath(ProtocolXPath.SitePerson) + "/@" + XPathManager.GetXPath(ProtocolXPath.SitePersonRef) + " should be a CDRID. CurrentValue=" + tempIDString + ". Document CDRID= " + _documentID.ToString());
                    }
                }

                ci.PersonGivenName = DocumentHelper.GetXmlDocumentValue(protPersonNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.SurName);
                ci.PersonSurName = DocumentHelper.GetXmlDocumentValue(protPersonNav,path);
                path = XPathManager.GetXPath(ProtocolXPath.ProfSuffix);
                ci.PersonProfessionalSuffix = DocumentHelper.GetXmlDocumentValue(protPersonNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.Role);
                ci.PersonRole = DocumentHelper.GetXmlDocumentValue(protPersonNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.City);
                ci.City = DocumentHelper.GetXmlDocumentValue(protPersonNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.State);
                ci.State = DocumentHelper.GetXmlDocumentValue(protPersonNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.StateID);
                tempIDString = DocumentHelper.GetAttribute(protPersonNav.SelectSingleNode(path), XPathManager.GetXPath(ProtocolXPath.StateRef));
                if (tempIDString.Length > 0)
                {
                    if (Int32.TryParse(CDRHelper.ExtractCDRID(tempIDString), out tempID))
                    {
                        ci.StateID = tempID;
                        tempID = 0;
                    }
                    else
                    {
                        throw new Exception("Extraction Error: " + path + "/@" + XPathManager.GetXPath(ProtocolXPath.SitePersonRef) + " should be a CDRID. CurrentValue=" + tempIDString + ". Document CDRID= " + _documentID.ToString());
                    }
                }
                path = XPathManager.GetXPath(ProtocolXPath.Country);
                ci.Country = DocumentHelper.GetXmlDocumentValue(protPersonNav, path);
                path = XPathManager.GetXPath(ProtocolXPath.ZipCode);
                ci.PostalCodeZip = DocumentHelper.GetXmlDocumentValue(protPersonNav, path);
                // Unique id to keep track of each contact info record
                ci.ContactInfoKey = _contactCount++;
                // also create it in the attribute for passing the value into render transformation
                protPersonNav.CreateAttribute("", "site", "", ci.ContactInfoKey.ToString());
                ci.Xml.LoadXml(protPersonNav.OuterXml);
           
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
            return ci;
        }

        /// <summary>
        /// Extract lead organization(s).
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="contactInfoList"></param>
        /// <param name="basePath"></param>
        private void ExtractLeadOrg(XPathNavigator xNav, List<ProtocolContactInfo> contactInfoList, string basePath)
        {
            string path = basePath;
            try{
                XPathNodeIterator leadOrgIter = xNav.Select(basePath);
                while (leadOrgIter.MoveNext())
                {
                    int leadOrgID = 0;
                    string tempLeadOrgID = string.Empty;
                    string leadOrgName = string.Empty;

                    // Parse Lead Org record(s)...
                    path = XPathManager.GetXPath(ProtocolXPath.LeadOrgName);
                    XPathNavigator leadOrgNameNav = leadOrgIter.Current.SelectSingleNode(path);
                    if (leadOrgNameNav != null)
                    {
                        leadOrgName = leadOrgNameNav.Value.Trim();
                        tempLeadOrgID = CDRHelper.ExtractCDRID(DocumentHelper.GetAttribute(leadOrgNameNav, XPathManager.GetXPath(ProtocolXPath.LeadOrgRef)));
                        if (tempLeadOrgID.Length > 0)
                        {
                            if (!Int32.TryParse(tempLeadOrgID, out leadOrgID))
                            {
                                throw new Exception("Extraction Error: " + path + "/@" + XPathManager.GetXPath(ProtocolXPath.LeadOrgRef) + " should be a CDRID. CurrentValue=" + tempLeadOrgID + ". Document CDRID= " + _documentID.ToString());
                            }
                        }
                    }
                    //ToDo: We should consider this for future design to make it a int in database, 
                    // so we can define enum here instead of hard coding string numbers.
                    path = XPathManager.GetXPath(ProtocolXPath.LeadOrgRole);
                    string leadOrgRole = DocumentHelper.GetXmlDocumentValue(leadOrgIter.Current, path);
                    if (leadOrgRole.ToUpper() == "PRIMARY")
                        leadOrgRole = "1";
                    else if (leadOrgRole.ToUpper() == "SECONDARY")
                        leadOrgRole = "2";

                    // Parse ProtPerson records...
                    path = XPathManager.GetXPath(ProtocolXPath.Person);
                    XPathNodeIterator protPersonIter = leadOrgIter.Current.Select(path);
                    while (protPersonIter.MoveNext())
                    {
                        contactInfoList.Add(ExtractProtPerson(protPersonIter.Current, leadOrgID, leadOrgName, leadOrgRole, true));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract protocol sites.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="contactInfoList"></param>
        /// <param name="basePath"></param>
        private void ExtractProtocolSites(XPathNavigator xNav, List<ProtocolContactInfo> contactInfoList, string basePath)
        {
            string path = basePath;
            try{
                XPathNodeIterator protocolSiteIter = xNav.Select(basePath);

                while (protocolSiteIter.MoveNext())
                {
                    path = XPathManager.GetXPath(ProtocolXPath.SiteName);
                    string organizationName = DocumentHelper.GetXmlDocumentValue(protocolSiteIter.Current, path);
                    string organizationRole = DocumentHelper.GetAttribute(protocolSiteIter.Current, "sitetype");
                    // Convert the organization role to string number for database storage purpose
                    int role = 0;
                    if (organizationRole.ToUpper() == "PRIMARY")
                        role = (int)OrganizationRoleType.Primary;
                    else if (organizationRole.ToUpper() == "SECONDARY")
                        role = (int)OrganizationRoleType.Secondary;
                    else if (organizationRole.ToUpper() == "ORGANIZATION")
                        role = (int)OrganizationRoleType.Organization;
                    else if (organizationRole.ToUpper() == "PERSON")
                        role = (int)OrganizationRoleType.Person;
                    organizationRole = role.ToString();


                    int organizationID = 0;
                    string tempOrganizationID = DocumentHelper.GetAttribute(protocolSiteIter.Current, XPathManager.GetXPath(ProtocolXPath.SiteRef));
                    if (tempOrganizationID.Length > 0)
                    {
                        if (!Int32.TryParse(CDRHelper.ExtractCDRID(tempOrganizationID), out organizationID))
                        {
                            throw new Exception("Extraction Error: " + XPathManager.GetXPath(ProtocolXPath.Site) + "/@" + XPathManager.GetXPath(ProtocolXPath.SiteRef) + " should be a CDRID. CurrentValue=" + tempOrganizationID + ". Document CDRID= " + _documentID.ToString());
                        }
                    }

                    // Parse ProtPerson records...
                    path = XPathManager.GetXPath(ProtocolXPath.SitePerson);
                    XPathNodeIterator protPersonIter = protocolSiteIter.Current.Select(path);
                    while (protPersonIter.MoveNext())
                    {
                        contactInfoList.Add(ExtractProtPerson(protPersonIter.Current, organizationID, organizationName, organizationRole, false));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract protocol title(s).
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="protocol"></param>
        /// <param name="basePath"></param>
        /// <param name="pdqPath"></param>
        private void ExtractTitle(XPathNavigator xNav, ProtocolDocument protocol, string basePath, string pdqPath)
        {
            try{
                XPathNodeIterator protocolTitleIter = xNav.Select(basePath);
                // For tracking error
                basePath = pdqPath;
                if (protocolTitleIter.Count == 0)
                    protocolTitleIter = xNav.Select(pdqPath);

                while (protocolTitleIter.MoveNext())
                {
                    string temp = DocumentHelper.GetAttribute(protocolTitleIter.Current, XPathManager.GetXPath(ProtocolXPath.TitleAudience));
                    if (temp == "Professional")
                        protocol.HealthProfessionalTitle = protocolTitleIter.Current.Value;
                    else if (temp == "Patient")
                    {
                        if (!protocolTitleIter.Current.Value.Trim().Equals("No Patient Title") &&
                            !protocolTitleIter.Current.Value.Trim().Equals(string.Empty))
                            protocol.PatientTitle = protocolTitleIter.Current.Value;
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + basePath + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
         }
        #endregion

        #region Protected Methods

        // NOTE: These internal methods are called by the CTGovExtractHelper class...

        /// <summary>
        /// Extract all possible protocol drugs.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="drugList"></param>
        /// <param name="basePath"></param>
        internal void ExtractDrug(XPathNavigator xNav, List<ProtocolDrug> drugList, string basePath)
        {
            try{
                XPathNodeIterator nodeIter = xNav.Select(basePath);
                while (nodeIter.MoveNext())
                {
                    string tempDrugID = DocumentHelper.GetAttribute(nodeIter.Current, XPathManager.GetXPath(ProtocolXPath.DrugRef));
                    string tempDrugName = nodeIter.Current.Value.Trim();
                    int drugID = 0;
                    if (tempDrugID.Length > 0)
                    {
                        if (Int32.TryParse(CDRHelper.ExtractCDRID(tempDrugID), out drugID))
                        {
                            // Filter out the drug that it's ID has already been in the list
                            bool exist = false;
                            foreach (ProtocolDrug pd in drugList)
                            {
                                if (pd.DrugID == drugID)
                                {
                                    // TODO: WARNING - give out warnings if it there is redudent ids
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist)
                                drugList.Add(new ProtocolDrug(drugID, tempDrugName));
                        }
                        else
                        {
                            throw new Exception("Extraction Error: " + basePath + "/@" + XPathManager.GetXPath(ProtocolXPath.DrugRef) + " should be a CDRID. CurrentValue=" + tempDrugID + ". Document CDRID= " + _documentID.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + basePath + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract all the possible protocol modalities.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="modalityList"></param>
        /// <param name="basePath"></param>
        internal void ExtractModality(XPathNavigator xNav, List<ProtocolModality> modalityList, string basePath)
        {
            try
            {
                XPathNodeIterator modalityIter = xNav.Select(basePath);
                while (modalityIter.MoveNext())
                {
                    string tempInterventionTypeID = DocumentHelper.GetAttribute(modalityIter.Current, XPathManager.GetXPath(ProtocolXPath.ModalityRef));
                    int interventionTypeID = 0;
                    if (tempInterventionTypeID.Length > 0)
                    {
                        if (Int32.TryParse(CDRHelper.ExtractCDRID(tempInterventionTypeID), out interventionTypeID))
                        {
                            bool exist = false;
                            // Avoid to add redundent id into the list
                            foreach (ProtocolModality pm in modalityList)
                            {
                                if (pm.ModalityID == interventionTypeID)
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist)
                                modalityList.Add(new ProtocolModality(interventionTypeID, modalityIter.Current.Value.Trim()));
                        }
                        else
                        {
                            throw new Exception("Extraction Error: " + basePath + "/@" + XPathManager.GetXPath(ProtocolXPath.ModalityRef) + " should be a CDRID. CurrentValue=" + tempInterventionTypeID + ". Document CDRID= " + _documentID.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + basePath + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extracts age eligibility attributes.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="protocol"></param>
        /// <param name="basePath"></param>
        internal void ExtractEligibility(XPathNavigator xNav, ProtocolDocument protocol, string basePath)
        {
            string path = basePath;
            try{
                XPathNavigator eligibilityNode = xNav.SelectSingleNode(basePath);
                path = XPathManager.GetXPath(ProtocolXPath.AgeRange);
                protocol.AgeRange = DocumentHelper.GetXmlDocumentValue(eligibilityNode, path);
                int lowAge = 0;
                path = XPathManager.GetXPath(ProtocolXPath.LowAge);
                string tempLowAge = DocumentHelper.GetXmlDocumentValue(eligibilityNode, path);
                if (tempLowAge.Length > 0)
                {
                    if (Int32.TryParse(tempLowAge, out lowAge))
                    {
                        protocol.LowAge = lowAge;
                    }
                    else
                    {
                        throw new Exception("Extraction Error: " + path + " should be an integer. CurrentValue=" + tempLowAge + ". Document CDRID= " + _documentID.ToString());
                    }
                }
                int highAge = 0;
                path = XPathManager.GetXPath(ProtocolXPath.HighAge);
                string tempHighAge = DocumentHelper.GetXmlDocumentValue(eligibilityNode, path);
                if (tempHighAge.Length > 0)
                {
                    if (Int32.TryParse(tempHighAge, out highAge))
                    {
                        protocol.HighAge = highAge;
                    }
                    else
                    {
                        throw new Exception("Extraction Error: " + path + " should be an integer. CurrentValue=" + tempLowAge + ". Document CDRID= " + _documentID.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract all the possible types of cancer for a protocol.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="typeOfCancerList"></param>
        /// <param name="basePath"></param>
        internal void ExtractTypeOfCancer(XPathNavigator xNav, List<TypeOfCancer> typeOfCancerList, string basePath)
        {
            try{
                XPathNodeIterator specificDiagnosisIter = xNav.Select(basePath);
                while (specificDiagnosisIter.MoveNext())
                {
                    string tempDiagnosisID = DocumentHelper.GetAttribute(specificDiagnosisIter.Current, XPathManager.GetXPath(ProtocolXPath.CancerTypeRef));
                    int specificDiagnosisID = 0;
                    if (tempDiagnosisID.Length > 0)
                    {
                        if (Int32.TryParse(CDRHelper.ExtractCDRID(tempDiagnosisID), out specificDiagnosisID))
                        {
                            bool newID = true;
                            // Avoid to save redudent ids
                             foreach (TypeOfCancer cancer in typeOfCancerList)
                            {
                                if (cancer.TypeOfCancerID == specificDiagnosisID)
                                {
                                    // TODO: WARNING - give out warnings if it there is redudent ids
                                    newID = false;
                                    break;
                                }
                            }
                            if (newID)
                                typeOfCancerList.Add(new TypeOfCancer(specificDiagnosisID, specificDiagnosisIter.Current.Value.Trim()));
                        }
                        else
                        {
                            throw new Exception("Extraction Error: " + basePath + "/@" + XPathManager.GetXPath(ProtocolXPath.CancerTypeRef) + " should be an integer. CurrentValue=" + tempDiagnosisID + ". Document CDRID= " + _documentID.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + basePath + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract protocol status.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="protocol"></param>
        /// <param name="basePath"></param>
        internal void ExtractStatus(XPathNavigator xNav, ProtocolDocument protocol, string basePath)
        {
            try
            {
                protocol.Status = DocumentHelper.GetXmlDocumentValue(xNav, basePath);
                protocol.IsActive = 1;
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + basePath + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }

        }

        /// <summary>
        /// This method is used to convert Phase into corresponding integer for saving into database
        /// It is shared by ProtocolExtractor and CTGovProtocolExtractor
        /// </summary>
        /// <param name="phase"></param>
        internal int IntPhase(string phase)
        {

            if (phase.Equals("Phase I"))
                return (int)PhaseType.PhaseI;
            else if (phase.Equals("Phase II"))
                return (int)PhaseType.PhaseII;
            else if (phase.Equals("Phase III"))
                return (int)PhaseType.PhaseIII;
            else if (phase.Equals("Phase IV"))
                return (int)PhaseType.PhaseIV;
            else if (phase.Equals("Phase V"))
                return (int)PhaseType.PhaseV;
            else
            {
                return (int)PhaseType.NoPhase;
            }
        }

        /// <summary>
        /// Get Term info from different parent nodes and combine them into HTML format
        /// Note: The logic of this code is borrowed from the old GateKeeper code.
        /// In the future we need to clean up this code to make it more robust.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="sectionList"></param>
        /// <param name="CDRID"></param>
        /// <param name="protType"></param>
        internal void ExtractTermSection(XmlDocument xdProtocol, List<ProtocolSection> sectionList, string CDRID, ProtocolSectionType protType)
        {
            try{
                string strHPTerms = "<a name=\"Terms:" + CDRID + "\">\n<h4>TERMS</h4>\n";
			    string strName, strValue;
			    StringBuilder sbFragment = new StringBuilder();;
			    bool bHasValues = false;

			    sbFragment.Append("<h5>Eligible Diagnoses/Conditions Studied</h5>\n");
			    sbFragment.Append("<table class=\"table-default\">\n");

			    if (xdProtocol.GetElementsByTagName("Diagnosis").Count > 0)
                {
                    //CTRedesign - remove nowrap attribute and add with to 40% and 60%
                   // sbFragment.Append("<tr><td nowrap=\"true\"><Span class=\"Protocol-Term-Table-Heading\">Diagnosis</Span></td><td nowrap=\"true\"><Span class=\"Protocol-Term-Table-Heading\">Parent Diagnosis</Span></td></tr>\n");
				   
				    sbFragment.Append("<tr><th>Diagnosis</th><th>Parent Diagnosis</th></tr>\n");
				    foreach (XmlNode xnTmp in xdProtocol.GetElementsByTagName("Diagnosis")) 
				    {
					    bHasValues = true;
                        strName = "";
					    strValue = "";
					    sbFragment.Append("<tr>\n");
					    foreach (XmlNode xnTmp2 in xnTmp.ChildNodes) 
					    {
						    // debug Console.WriteLine("looking at: " + xnTmp2.Name + "/" + xnTmp2.Value);
						    switch (xnTmp2.Name) 
						    {
							    case "SpecificDiagnosis": strName = xnTmp2.InnerXml; break;
							    case "DiagnosisParent": strValue += ", " + xnTmp2.InnerXml; break;
							    default: break;
						    }
					    }
					    if (strValue.Length > 2) 
						    strValue = strValue.Substring(2);
					    if (strName.Length == 0) 
						    strName = "n/a";
					    if (strValue.Length == 0) 
						    strValue = "n/a";

                        //CTRedesign -- remove nowrap
					    //sbFragment.Append("<td valign=\"top\" nowrap=\"true\">" + strName + "</td>\n");
                        //sbFragment.Append("<td valign=\"top\" width=\"100%\">" + strValue + "</td>\n");
                        sbFragment.Append("<td>" + strName + "</td>\n");
                        sbFragment.Append("<td>" + strValue + "</td>\n");
					    sbFragment.Append("</tr>\n");
				    }
			    }

			    if (xdProtocol.GetElementsByTagName("StudyCondition").Count > 0) 
			    {
				    sbFragment.Append("<tr><th>Condition</th><th>Parent Condition</th></tr>\n");
				    foreach (XmlNode xnTmp in xdProtocol.GetElementsByTagName("StudyCondition")) 
				    {
					    bHasValues = true;
					    strName = "";
					    strValue = "";
					    sbFragment.Append("<tr>\n");
					    foreach (XmlNode xnTmp2 in xnTmp.ChildNodes) 
					    {
						    switch (xnTmp2.Name) 
						    {
							    case "SpecificCondition": strName = xnTmp2.InnerXml; break;
							    case "ConditionParent": strValue += ", " + xnTmp2.InnerXml; break;
							    default: break;
						    }
					    }
					    if (strValue.Length > 2) 
						    strValue = strValue.Substring(2);
					    if (strName.Trim().Length == 0) 
						    strName = "n/a";
                        if (strValue.Trim().Length == 0) 
						    strValue = "n/a";
					    sbFragment.Append("<td>" + strName + "</td>\n");
					    sbFragment.Append("<td>" + strValue + "</td>\n");
					    sbFragment.Append("</tr>\n");
				    }
			    }

			    sbFragment.Append("</table>\n");
			    if (bHasValues) 
			    {
				    strHPTerms += sbFragment.ToString();
			    }

			    sbFragment = new StringBuilder();
			    sbFragment.Append("<h5>Interventions</h5>\n");
			    sbFragment.Append("<table class=\"table-default\">\n");
                sbFragment.Append("<tr><th>Intervention Type</th><th>Intervention Name</th></tr>\n");
			    bHasValues = false;
			    foreach (XmlNode xnTmp in xdProtocol.GetElementsByTagName("Intervention")) 
			    {
				    bHasValues = true;
				    strName = "";
				    strValue = "";
				    sbFragment.Append("<tr>\n");
				    foreach (XmlNode xnTmp2 in xnTmp.ChildNodes) 
				    {
					    switch (xnTmp2.Name) 
					    {
						    case "InterventionType": strName = xnTmp2.InnerXml; break;
						    case "InterventionNameLink": strValue += ", " + xnTmp2.InnerXml; break;
						    default: break;
					    }
				    }
				    if (strValue.Length > 2) 
					    strValue = strValue.Substring(2);
				    if (strName == "") 
					    strName = "n/a";
				    if (strValue == "") 
					    strValue = "n/a";
				    sbFragment.Append("<td>" + strName + "</td>\n");
				    sbFragment.Append("<td>" + strValue + "</td>\n");
				    sbFragment.Append("</tr>\n");
			    }
			    sbFragment.Append("</table>\n");
			    if (bHasValues) 
			    {
				    strHPTerms += sbFragment.ToString();
			    }

			    strHPTerms += "\n</a>";

                sectionList.Add(new ProtocolSection(0, strHPTerms, protType));
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting term section failed.  Document CDRID=" + _documentID.ToString(), e);
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
        /// Extracts the protocol metadata from the input XML document.
        /// </summary>
        /// <param name="xmlDoc">Protocol XML</param>
        /// <param name="protocol">Protocol document object</param>
        public void Extract(XmlDocument xmlDoc, ProtocolDocument protocol)
        {
            try
            {
                XPathNavigator xNav = xmlDoc.CreateNavigator();

                // Extract the CDR ID...
                xNav.MoveToFirstChild();
                if (CDRHelper.ExtractCDRID(xNav, XPathManager.GetXPath(CommonXPath.CDRID), out _documentID))
                {
                    protocol.DocumentID = _documentID;
                    protocol.ProtocolID = _documentID;
                }
                else
                {
                    throw new Exception("Extraction Error: Failed to extract document (CDR) ID in protocol document!");
                }

                DocumentHelper.CopyXml(xmlDoc, protocol);
                
                // Extract misc metadata...
                ExtractMetadata(xNav, protocol);

                // Extract status
                ExtractStatus(xNav, protocol, XPathManager.GetXPath(ProtocolXPath.Status));

                // Extract age eligibility
                ExtractEligibility(xNav, protocol, XPathManager.GetXPath(ProtocolXPath.Eligibility));

                // Extract alternate IDs
                ExtractAlternateIDs(xNav, protocol.AlternateIDList, XPathManager.GetXPath(ProtocolXPath.AlternateID));
                ExtractAlternateIDs(xNav, protocol.AlternateIDList, XPathManager.GetXPath(ProtocolXPath.PrimaryID));

                // Extract protocol Pretty URL IDs
                ExtractProtocolPrettyUrlIDs(xNav, protocol);

                // Extract types of cancer 
                ExtractTypeOfCancer(xNav, protocol.TypeOfCancerList, XPathManager.GetXPath(ProtocolXPath.CancerType1));
                ExtractTypeOfCancer(xNav, protocol.TypeOfCancerList, XPathManager.GetXPath(ProtocolXPath.CancerType2));
                ExtractTypeOfCancer(xNav, protocol.TypeOfCancerList, XPathManager.GetXPath(ProtocolXPath.CancerType3));
                ExtractTypeOfCancer(xNav, protocol.TypeOfCancerList, XPathManager.GetXPath(ProtocolXPath.CancerType4));

                // Extract modalities
                ExtractModality(xNav, protocol.ModalityList, XPathManager.GetXPath(ProtocolXPath.ModalityType1));
                ExtractModality(xNav, protocol.ModalityList, XPathManager.GetXPath(ProtocolXPath.ModalityType2));
                ExtractModality(xNav, protocol.ModalityList, XPathManager.GetXPath(ProtocolXPath.ModalityType3));

                // Extract Drugs
                ExtractDrug(xNav, protocol.DrugList, XPathManager.GetXPath(ProtocolXPath.Durg));

                // Extract lead orgs 
                ExtractLeadOrg(xNav, protocol.ContactInfoList, XPathManager.GetXPath(ProtocolXPath.LeadOrg));

                // Extract protocol sites
                ExtractProtocolSites(xNav, protocol.ContactInfoList, XPathManager.GetXPath(ProtocolXPath.Site));

                // Extract protocol term section
                string CDRID = xNav.GetAttribute("id", string.Empty);
                ExtractTermSection(xmlDoc, protocol.ProtocolSectionList, CDRID, ProtocolSectionType.Terms);

                // Handle modified and published dates
                DocumentHelper.ExtractDates(xNav, protocol, XPathManager.GetXPath(CommonXPath.LastModifiedDate), XPathManager.GetXPath(CommonXPath.FirstPublishedDate));

                // Reload the xml document since there are some changes such as added attribute site to keep track of each contact info
               protocol.Xml.LoadXml(xNav.OuterXml);

                // Determine if this is a new protocol 
                // If the document is published within 30 days, it is a new protocol.
               if (protocol.FirstPublishedDate != null)
               {
                   DateTime now = DateTime.Now;
                   DateTime comparedDate = protocol.FirstPublishedDate.AddDays(30);
                   if (comparedDate.CompareTo(now) > 0)
                       protocol.IsNew = 1;
               }
            }
            catch (Exception e)
            {
               throw new Exception("Extraction Error: Failed to extract protocol document", e);
            }
        }

        #endregion
    }
}
