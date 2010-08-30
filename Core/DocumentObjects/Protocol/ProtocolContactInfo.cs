using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GateKeeper.DocumentObjects.Protocol
{
    /// <summary>
    /// Class to represent contact information.
    /// </summary>
    [Serializable]
    public class ProtocolContactInfo
    {
        #region Fields

        private int _protocolContactInfoID = 0;
        private int _organizationID = 0;
        private int _personID = 0;
        private string _organizationName = string.Empty;
        private string _personGivenName = string.Empty;
        private string _personSurName = string.Empty;
        private string _personProfessionalSuffix = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _phoneExtension = string.Empty;
        private string _personRole = string.Empty;
        private string _organizationRole = string.Empty;
        private string _city = string.Empty;
        private string _state = string.Empty;
        private int _stateID = 0;
        private string _country = string.Empty;
        private string _postalCodeZip = string.Empty;
        private bool _isLeadOrg = false;
        // This is created to unique track each contact info the content will be organizationid - contact#
        private int _contactInfoKey = 0;

        private string _html = string.Empty;
        private XmlDocument _xml = new XmlDocument();

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor. 
        /// </summary>
        public ProtocolContactInfo()
        { }

        #endregion

        #region Public Properties

        /// <summary>
        /// 
        /// </summary>
        public int ProtocolContactInfoID
        {
            get { return _protocolContactInfoID; }
            set { _protocolContactInfoID = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int OrganizationID
        {
            get { return _organizationID; }
            set { _organizationID = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string OrganizationName
        {
            get { return _organizationName; }
            set { _organizationName = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PersonGivenName
        {
            get { return _personGivenName; }
            set { _personGivenName = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string PersonSurName
        {
            get { return _personSurName; }
            set { _personSurName = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string PersonProfessionalSuffix
        {
            get { return _personProfessionalSuffix; }
            set { _personProfessionalSuffix = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { _phoneNumber = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PhoneExtension
        {
            get { return _phoneExtension; }
            set { _phoneExtension = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string PersonRole
        {
            get { return _personRole; }
            set { _personRole = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string OrganizationRole
        {
            get { return _organizationRole; }
            set { _organizationRole = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public int PersonID
        {
            get { return _personID; }
            set { _personID = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string City
        {
            get { return _city; }
            set { _city = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string State
        {
            get { return _state; }
            set { _state = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int StateID
        {
            get { return _stateID; }
            set { _stateID = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PostalCodeZip
        {
            get { return _postalCodeZip; }
            set { _postalCodeZip = value; }
        }

        /// <summary>
        /// Pre-rendered contact info.
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        /// <summary>
        /// Source XML block.
        /// </summary>
        public XmlDocument Xml
        {
            get { return _xml; }
            set { _xml = value; }
        }

        /// <summary>
        /// Flag if the contact info is for lead organization.
        /// </summary>
        public bool IsLeadOrg
        {
            get { return _isLeadOrg; }
            set { _isLeadOrg = value; }
        }

        /// <summary>
        /// Key to track unique contact info
        /// </summary>
        public int ContactInfoKey
        {
            get { return _contactInfoKey; }
            set { _contactInfoKey = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns System.String representation of ContactInfo.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" ContactInfoID = {0} OrganizationID = {1} OrganizationName = {2} OrganizationRole = {3}\n",
                this.ProtocolContactInfoID, this.OrganizationID, this.OrganizationName, this.OrganizationRole));

            sb.Append(string.Format("PersonGivenName = {0} PersonSurName = {1} PersonProfessionalSuffix = {2} ",
                this.PersonGivenName, this.PersonSurName, this.PersonProfessionalSuffix));

            sb.Append(string.Format("PersonRole = {0} PhoneNumber = {1} PhoneExtension = {2} StateID = {3} State = {4} PostalCodeZip = {5} \n",
                this.PersonRole, this.PhoneNumber, this.PhoneExtension, this.StateID, this.State, this.PostalCodeZip));

            sb.Append(string.Format("City = {0} Country = {1}",
                this.City, this.Country));

            return sb.ToString();
        }

        #endregion
    }
}
