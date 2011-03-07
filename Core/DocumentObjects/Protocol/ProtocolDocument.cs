using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Protocol
{
    /// <summary>
    /// Class to represent protocol documents.
    /// </summary>
    [Serializable]
    public class ProtocolDocument : Document
    {
        #region Fields

        private int _protocolID = 0;
        private string _healthProfessionalTitle = string.Empty;
        private string _patientTitle = string.Empty;
        private List<AlternateProtocolID> _alternateIDList = new List<AlternateProtocolID>();
        private AudienceType _audienceType = AudienceType.HealthProfessional;
        private int _lowAge = 0;
        private int _highAge = 0;
        private string _ageRange = string.Empty;
        private int _isActive = 0;
        private List<string> _sponsorList = new List<string>();
        private string _sponsorText = string.Empty;
        private List<TypeOfCancer> _typeOfCancerList = new List<TypeOfCancer>();
        private List<string> _studyCategoryList = new List<string>();
        private List<ProtocolModality> _modalityList = new List<ProtocolModality>();
        private List<int> _protocolPhaseList = new List<int>();
        private List<ProtocolContactInfo> _contactInfoList = new List<ProtocolContactInfo>();
        private List<ProtocolDrug> _drugList = new List<ProtocolDrug>();
        private int _isNIHClinicalTrial = 0;
        private List<string> _specialCategoryList = new List<string>();
        private ProtocolType _protocolType = ProtocolType.Protocol;
        private List<ProtocolSection> _protocolSectionList = new List<ProtocolSection>();
        private string _status = string.Empty;
        private int _isNew = 0;
        private int _isCTProtocol = 0;
        private System.Xml.XmlDocument _patienXML = new System.Xml.XmlDocument();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets, sets health professional protocol title.
        /// </summary>
        public string HealthProfessionalTitle
        {
            get { return _healthProfessionalTitle; }
            internal set { _healthProfessionalTitle = value; }
        }

        /// <summary>
        /// Gets, sets patient protocol title.
        /// </summary>
        public string PatientTitle
        {
            get { return _patientTitle; }
            internal set { _patientTitle = value; }
        }

        /// <summary>
        /// Protocol identifier.
        /// </summary>
        public int ProtocolID
        {
            get { return _protocolID; }
            internal set { _protocolID = value; }
        }

        /// <summary>
        /// Gets, sets list of protocol alternate IDs.
        /// </summary>
        public List<AlternateProtocolID> AlternateIDList
        {
            get { return _alternateIDList; }
            internal set { _alternateIDList = value; }
        }

        /// <summary>
        /// Gets, sets protocol audience type.
        /// </summary>
        public AudienceType AudienceType
        {
            get { return _audienceType; }
            internal set { _audienceType = value; }
        }

        /// <summary>
        /// Gets, sets minimum age for participation 
        /// in the protocol.
        /// </summary>
        public int LowAge
        {
            get { return _lowAge; }
            internal set { _lowAge = value; }
        }

        /// <summary>
        /// Maximum age for participation in the protocol.
        /// </summary>
        public int HighAge
        {
            get { return _highAge; }
            internal set { _highAge = value; }
        }

        /// <summary>
        /// Gets, sets description of the age range.
        /// </summary>
        public string AgeRange
        {
            get { return _ageRange; }
            internal set { _ageRange = value; }
        }

        /// <summary>
        /// Gets, sets flag that indiciates is active.
        /// </summary>
        public int IsActive
        {
            get { return _isActive; }
            internal set { _isActive = value; }
        }

        /// <summary>
        /// Gets, sets collection of protocol sponsors.
        /// </summary>
        public List<string> SponsorList
        {
            get { return _sponsorList; }
            internal set { _sponsorList = value; }
        }

        /// <summary>
        /// Gets, sets formatted string of SponsorList contents.
        /// </summary>
        public string SponsorText
        {
            get { return _sponsorText; }
            internal set { _sponsorText = value; }
        }

        /// <summary>
        /// Gets, sets type of cancer (subject of protocol).
        /// </summary>
        public List<TypeOfCancer> TypeOfCancerList
        {
            get { return _typeOfCancerList; }
            internal set { _typeOfCancerList = value; }
        }

        /// <summary>
        /// Gets, sets protocol study category.
        /// </summary>
        public List<string> StudyCategoryList
        {
            get { return _studyCategoryList; }
            internal set { _studyCategoryList = value; }
        }

        /// <summary>
        /// Gets, sets type of therapy being tested 
        /// under the protocol.
        /// </summary>
        public List<ProtocolModality> ModalityList
        {
            get { return _modalityList; }
            set { _modalityList = value; }
        }

        /// <summary>
        /// Gets, sets phases of the protocol.
        /// </summary>
        public List<int> ProtocolPhaseList
        {
            get { return _protocolPhaseList; }
            internal set { _protocolPhaseList = value; }
        }

        /// <summary>
        /// Gets, sets protocol contact info 
        /// (lead orgs, protocol sites...etc).
        /// </summary>
        public List<ProtocolContactInfo> ContactInfoList
        {
            get { return _contactInfoList; }
            internal set { _contactInfoList = value; }
        }

        /// <summary>
        /// Gets, sets collection of drugs being tested 
        /// under the protocol.
        /// </summary>
        public List<ProtocolDrug> DrugList
        {
            get { return _drugList; }
            internal set { _drugList = value; }
        }

        /// <summary>
        /// Gets, sets flag to indicate if this protocol 
        /// being carried out by the NIH.
        /// </summary>
        public int IsNIHClinicalTrial
        {
            get { return _isNIHClinicalTrial; }
            internal set { _isNIHClinicalTrial = value; }
        }

        /// <summary>
        /// Gets, sets the collection of special categories.
        /// </summary>
        public List<string> SpecialCategoryList
        {
            get { return _specialCategoryList; }
            internal set { _specialCategoryList = value; }
        }

        /// <summary>
        /// Type of protocol (treatment, supportive care...etc).
        /// </summary>
        public ProtocolType ProtocolType
        {
            get { return _protocolType; }
            internal set { _protocolType = value; }
        }

        /// <summary>
        /// Gets, sets the collection of document sections.
        /// </summary>
        public List<ProtocolSection> ProtocolSectionList
        {
            get { return _protocolSectionList; }
            internal set { _protocolSectionList = value; }
        }

        /// <summary>
        /// Gets, sets status of the Protocol.
        /// </summary>
        public string Status
        {
            get { return _status; }
            internal set { _status = value; }
        }

        /// <summary>
        /// Gets, sets flag that indicates if the protocol has 
        /// been published in the last 30 days or not.
        /// </summary>
        public int IsNew
        {
            get { return _isNew; }
            internal set { _isNew = value; }
        }


        /// <summary>
        /// Gets, sets flag that indicates if the protocol is a CTGovProtocol
        /// </summary>
        public int IsCTProtocol
        {
            get { return _isCTProtocol; }
            internal set { _isCTProtocol = value; }
        }

        /// <summary>
        /// Document XML.
        /// </summary>
        public System.Xml.XmlDocument PatientXML
        {
            get { return _patienXML; }
            set { _patienXML = value; }
        }

        /// <summary>
        /// Gets, sets the URL version of the primary ID used for
        /// the Protocol PrettyURL.
        /// </summary>
        public string PrimaryProtocolUrlID { get; set; }

        /// <summary>
        /// Gets, sets the URL version of the secondary ID used for
        /// the Protocol PrettyURL.
        /// </summary>
        public string SecondaryProtocolUrlID { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ProtocolDocument()
        {
            this.DocumentType = DocumentType.Protocol;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returnes a System.String representation of the ProtocolDocument.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(string.Format(" ProtocolID = {0} AudienceType = {1} Status = {2} ", 
                ProtocolID, AudienceType, Status));

            sb.Append("StudyCategoryList = \n");
            foreach (string study in StudyCategoryList)
            {
                sb.Append(string.Format("{0}\n", study));
            }


            sb.Append(string.Format("IsActive = {0} IsNew = {1} \n", IsActive, IsNew));

            sb.Append("TypeOfCancerList = \n");
            foreach (TypeOfCancer typeOfCancer in TypeOfCancerList)
            {
                sb.Append(string.Format("{0}\n", typeOfCancer.ToString()));
            }

            sb.Append("AlternateIDList = \n");
            foreach (AlternateProtocolID altProtocolID in AlternateIDList)
            {
                sb.Append(string.Format("{0}\n", altProtocolID.ToString()));
            }

            sb.Append("DrugList = \n");
            foreach (ProtocolDrug drug in DrugList)
            {
                sb.Append(string.Format("{0}\n", drug.ToString()));
            }

            sb.Append("ModalityList = \n");
            foreach (ProtocolModality modality in ModalityList)
            {
                sb.Append(string.Format("{0}\n", modality.ToString()));
            }

            sb.Append("ProtocolPhaseList = \n");
            foreach (int phase in ProtocolPhaseList)
            {
                sb.Append(string.Format("{0}\n", phase.ToString()));
            }

            sb.Append("SponsorList = \n");
            foreach (string sponsor in SponsorList)
            {
                sb.Append(string.Format("{0}\n", sponsor));
            }

            sb.Append("SpecialCategoryList = \n");
            foreach (string specialCategory in SpecialCategoryList)
            {
                sb.Append(string.Format("{0}\n", specialCategory));
            }

            sb.Append("ContactInfoList = \n");
            foreach (ProtocolContactInfo ci in ContactInfoList)
            {
                sb.Append(string.Format("{0}\n", ci.ToString()));
            }

            return sb.ToString();
        }

        #endregion
    }
}
