using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GateKeeper.DocumentObjects.Protocol
{
    /// <summary>
    /// Enumeration to represent protocol section types.
    /// </summary>
    [Serializable]
    public enum ProtocolSectionType
    {
        PatientAbstract = 1,
        Objectives = 2,
        EntryCriteria = 3,
        ExpectedEnrollment = 4,  
        Outline = 5,
        PublishedResults = 6,
        Terms = 7,
        HPDisclaimer = 8,
        LeadOrgs = 9,
        PDisclaimer = 10,
        LastMod = 11,
        CTGovBriefSummary = 12,
        CTGovDisclaimer = 13,
        CTGovLeadOrgs = 14,
        CTGovFooter = 15,
        CTGovDetailedDescription = 16,
        CTGovEntryCriteria = 17,
        CTGovTerms = 18,
        SpecialCategory = 19,
        PatientProtocolRelatedLinks = 20,
        HPProtocolRelatedLinks = 21,
        Outcomes = 22,
        RelatedPublications = 23,
        RegistryInformation = 24,
    }

    /// <summary>
    /// Represents a section of a protocol document.
    /// </summary>
    [Serializable]
    public class ProtocolSection
    {
        #region Fields

        private int _sectionID = 0;
        private XmlDocument _html = new XmlDocument();
        private XmlDocument _xml = new XmlDocument();
        private ProtocolSectionType _protocolSectionType;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets, sets pre-rendered HTML.
        /// </summary>
        public XmlDocument Html
        {
            get { return _html; }
            internal set { _html = value; }
        }

        /// <summary>
        /// Gets, sets section identifier.
        /// </summary>
        public int SectionID
        {
            get { return _sectionID; }
            internal set { _sectionID = value; }
        }

        /// <summary>
        /// Gets, sets raw document XML.
        /// </summary>
        public XmlDocument Xml
        {
            get { return _xml; }
            internal set { _xml = value; }
        }

        /// <summary>
        /// Gets, sets protocol section type.
        /// </summary>
        public ProtocolSectionType ProtocolSectionType
        {
            get { return _protocolSectionType; }
            internal set { _protocolSectionType = value; }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="sectionID"></param>
        /// <param name="html"></param>
        /// <param name="protocolSectionType"></param>
        public ProtocolSection(int sectionID, string html, ProtocolSectionType protocolSectionType)
        {
            this._sectionID = sectionID;
            this._html.LoadXml(html);
            //this.xml = xml;
            this._protocolSectionType = protocolSectionType;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a string representation of the ProtocolSection object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" SectionID = {0} \n",  this.SectionID));
            sb.Append(string.Format("Xml = {0}\n", this.Xml));
            sb.Append(string.Format("Html = {0}\n", this.Html));

            return sb.ToString();
        }

        #endregion Public Methods
    }
}
