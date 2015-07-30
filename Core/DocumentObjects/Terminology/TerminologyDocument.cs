using System;
using System.Collections.Generic;
using System.Text;

using GateKeeper.DocumentObjects.Dictionary;

namespace GateKeeper.DocumentObjects.Terminology
{
    /// <summary>
    /// Class to represent a terminology document.
    /// </summary>
    [Serializable]
    public class TerminologyDocument : Document
    {
        #region Fields

        private string _preferredName = string.Empty;
        private string _termTypeName = string.Empty;
        private string _definitionText = string.Empty;
        private int     _parentTermID = 0;
        private AudienceType _definitionAudience = AudienceType.Patient;
        private TerminologyType _termType = TerminologyType.Drug;
        private List<TerminologyOtherName> _otherNames = new List<TerminologyOtherName>();
        private List<TermSemanticType> _semanticTypes = new List<TermSemanticType>();
        private List<TerminologyMenu> _menus = new List<TerminologyMenu>();

        #endregion

        #region Constructors

        public TerminologyDocument()
            : base()
        {
            this.DocumentType = DocumentType.Terminology;
            this.Dictionary = new List<GeneralDictionaryEntry>();
            this.TermAliasList = new List<TermAlias>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Audience type for the term document.
        /// </summary>
        public AudienceType DefinitionAudience
        {
            get { return _definitionAudience; }
            set { _definitionAudience = value; }
        }

        /// <summary>
        /// The dictionary entry that comes from a TerminologyDocument.
        /// This element is only set when the term's SemanticTypes collection
        /// includes a value of "drug/agent."
        /// </summary>
        public List<GeneralDictionaryEntry> Dictionary { get; private set; }

        /// <summary>
        /// The term alias that comes from a TerminologyDocument.
        /// This element is only set when the term's SemanticTypes collection
        /// includes a value of "drug/agent."
        /// </summary>
        public List<TermAlias> TermAliasList { get; private set; }

        /// <summary>
        /// Text of the definition.
        /// </summary>
        public string DefinitionText
        {
            get { return _definitionText; }
            set { _definitionText = value; }
        }

        /// <summary>
        /// Term preferred name.
        /// </summary>
        public string PreferredName
        {
            get { return _preferredName; }
            set { _preferredName = value; }
        }

        /// <summary>
        /// List of synonyms.
        /// </summary>
        public System.Collections.Generic.List<TerminologyOtherName> OtherNames
        {
            get { return _otherNames; }
            set { _otherNames = value; }
        }

        /// <summary>
        /// List of menus.
        /// </summary>
        public List<TerminologyMenu> Menus
        {
            get { return _menus; }
            set { _menus = value; }
        }

        /// <summary>
        /// Term type.
        /// </summary>
        public string TermTypeName
        {
            get { return _termTypeName; }
            set { _termTypeName = value; }
        }

        /// <summary>
        /// Term type.
        /// </summary>
        public TerminologyType TermType
        {
            get { return _termType; }
            set { _termType = value; }
        }

        /// <summary>
        /// Parent term doc id.
        /// </summary>
        public int ParentTermID
        {
            get { return _parentTermID; }
            set { _parentTermID = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<TermSemanticType> SemanticTypes
        {
            get { return _semanticTypes; }
            set { _semanticTypes = value; }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the 
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" PreferredName = {0} TermTypeName = {1} ParentTermID = {2} DefinitionText = {3}\n",
                this.PreferredName, this.TermTypeName, this.ParentTermID, this.DefinitionText));

            sb.Append(string.Format("TermType = {0}\n", this.TermType));

            sb.Append("OtherNames = \n");
            foreach (TerminologyOtherName otherName in this.OtherNames)
            {
                sb.Append(string.Format("OtherName = {0}\n", otherName.ToString()));
            }

            sb.Append("SemanticTypes = \n");
            foreach (TermSemanticType semanticType in this.SemanticTypes)
            {
                sb.Append(string.Format("SemanticType = {0}\n", semanticType.ToString()));
            }

            sb.Append("MenuList = \n");
            foreach (TerminologyMenu menu in this.Menus)
            {
                sb.Append(string.Format("Menu = {0}\n", menu.ToString()));
            }

            return sb.ToString();
        }

        #endregion
    }
}
