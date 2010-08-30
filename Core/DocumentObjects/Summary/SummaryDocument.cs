using System;
using System.Collections.Generic;
using System.Text;
using GateKeeper.DocumentObjects;

namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Class to represent a summary document.
    /// </summary>
    [Serializable]
    public class SummaryDocument : Document
    {
        #region Fields
        private string _basePrettyURL = string.Empty;
        private string _prettyURL = string.Empty;
        private string _title = string.Empty;
        private string _shortTitle = string.Empty;
        private string _description = string.Empty;
        private List<SummaryRelation> _relationList = new List<SummaryRelation>();
        private string _audienceType = string.Empty;
        private Language _language = Language.English;
        private string _type = string.Empty;
        private List<SummarySection> _sectionList = new List<SummarySection>();
        private List<SummarySection> _level4SectionList = new List<SummarySection>();
        private List<SummarySection> _level5SectionList = new List<SummarySection>();
        private List<SummarySection> _tableSectionList = new List<SummarySection>();
        private string _oldDocumentID = string.Empty;
        // Pretty URL map for internal document fragments.
        private Dictionary<string, string> _prettyUrlMap = new Dictionary<string, string>();
        private int _replacementForID = 0;
        private Guid _relatedDocumentGUID = Guid.Empty;
        private Guid _otherLanguageDocumentGUID = Guid.Empty;
        private Guid _replacementforDocumentGUID = Guid.Empty;


        /// <summary>
        /// Summaries/SummarySections that this summary references (may also be internal references).
        /// </summary>
        /// <example>"CDR000062727#_2" => "cancertopics/pdq/treatment/breast/patient#page2" <br/>
        /// "CDR000062727" => "cancertopics/pdq/treatment/breast/patient" ...etc
        /// </example>
        // TODO: Move externalPrettyUrlReferenceMap to base class?
        private Dictionary<string, string> _prettyUrlReferenceMap = new Dictionary<string, string>();
        private bool _isActive = true;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SummaryDocument()
        {
            this.DocumentType = DocumentType.Summary;
        }

        /// <summary>
        /// Class constructor with parameters.
        /// </summary>
        /// <param name="summaryID"></param>
        /// <param name="summaryGuid"></param>
        /// <param name="title"></param>
        /// <param name="audience"></param>
        /// <param name="language"></param>
        /// <param name="isActive"></param>
        public SummaryDocument(int summaryID, string title, string audience, Language language, bool isActive)
        {
            this.DocumentType = DocumentType.Summary;
            this.DocumentID = summaryID;
            this._title = title;
            this._audienceType = audience;
            this._language = language;
            this._type = _type.ToString();
            this._isActive = isActive;
        }

        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets, sets ID of the summary document that this summary replaces.
        /// </summary>
        public int ReplacementForID
        {
            get { return _replacementForID; }
            internal set { _replacementForID = value; }
        }

        /// <summary>
        /// Gets, sets base Pretty URL (from which all page pretty URLs will be derived).
        /// </summary>
        public string BasePrettyURL
        {
            get { return _basePrettyURL; }
            internal set { _basePrettyURL = value; }
        }

        /// <summary>
        /// Gets, sets base Pretty URL (from which all page pretty URLs will be derived).
        /// </summary>
        public string PrettyURL
        {
            get { return _prettyURL; }
            internal set { _prettyURL = value; }
        }


        /// <summary>
        /// Pretty URL map for internal document fragments.
        /// </summary>
        public Dictionary<string, string> PrettyUrlMap
        {
            get { return _prettyUrlMap; }
            internal set { _prettyUrlMap = value; }
        }

        /// <summary>
        /// Gets, sets summary title.
        /// </summary>
        public string Title
        {
            get { return _title; }
            internal set { _title = value; }
        }

        /// <summary>
        /// Gets, sets summary short title.
        /// </summary>
        public string ShortTitle
        {
            get { return _shortTitle; }
            internal set { _shortTitle = value; }
        }

        /// <summary>
        /// Gets, sets summary description.
        /// </summary>
        public string Description
        {
            get { return _description; }
            internal set { _description = value; }
        }

        /// <summary>
        /// Summaries/SummarySections that this summary references (may also be internal references).
        /// </summary>
        /// <example>"CDR000062727#_2" => "cancertopics/pdq/treatment/breast/patient#page2" <br/>
        /// "CDR000062727" => "cancertopics/pdq/treatment/breast/patient" ...etc
        /// </example>
        public Dictionary<string, string> PrettyUrlReferenceMap
        {
            get { return _prettyUrlReferenceMap; }
            internal set { _prettyUrlReferenceMap = value; }
        }

        /// <summary>
        /// Gets, sets representation of the relationship for this summary 
        /// (patient version of, translation of...etc).
        /// </summary>
        public List<SummaryRelation> RelationList
        {
            get { return _relationList; }
            internal set { _relationList = value; }
        }

        /// <summary>
        /// Gets, sets intended audience of the summary.
        /// </summary>
        public string AudienceType
        {
            get { return _audienceType; }
            internal set { _audienceType = value; }
        }

        /// <summary>
        /// Gets, sets language of the summary.
        /// </summary>
        public Language Language
        {
            get { return _language; }
            internal set { _language = value; }
        }

        /// <summary>
        /// Gets, sets summary type metadata.
        /// </summary>
        public string Type
        {
            get { return _type; }
            internal set { _type = value; }
        }

        /// <summary>
        /// Gets, sets Collection of sections.
        /// </summary>
        public List<SummarySection> SectionList
        {
            get { return _sectionList; }
            internal set { _sectionList = value; }
        }

        /// <summary>
        /// Level 4 sections.
        /// </summary>
        public List<SummarySection> Level4SectionList
        {
            get { return _level4SectionList; }
            internal set { _level4SectionList = value; }
        }

        /// <summary>
        /// Level 5 sections.
        /// </summary>
        public List<SummarySection> Level5SectionList
        {
            get { return _level5SectionList; }
            internal set { _level5SectionList = value; }
        }

        /// <summary>
        /// Contains all table enlarge sections.
        /// </summary>
        public List<SummarySection> TableSectionList
        {
            get { return _tableSectionList; }
            internal set { _tableSectionList = value; }
        }

        /// <summary>
        /// Gets, sets summary ID of the document this summary replaces.
        /// Note: May be empty string if this is not a replace.
        /// </summary>
        public string OldDocumentID
        {
            get { return _oldDocumentID; }
            internal set { _oldDocumentID = value; }
        }

        /// <summary>
        /// Indicates that the document is active in the document table.
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            internal set { _isActive = value; }
        }

        public Guid RelatedDocumentGUID
        {
            get { return _relatedDocumentGUID; }
            internal set { _relatedDocumentGUID = value; }
        }

        public Guid OtherLanguageDocumentGUID
        {
            get { return _otherLanguageDocumentGUID; }
            internal set { _otherLanguageDocumentGUID = value; }
        }

        public Guid ReplacementforDocumentGUID
        {
            get { return _replacementforDocumentGUID; }
            internal set { _replacementforDocumentGUID = value; }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Adds a "level 5" section. 
        /// Note: This is called from the ContentRendering code 
        /// (Level 5 sections are parsed from the pre-rendered document).
        /// </summary>
        /// <param name="sectionID"></param>
        /// <param name="sectionTitle"></param>
        /// <param name="parentSectionID"></param>
        /// <param name="sectionType"></param>
        public void AddLevel5Section(string sectionID, string sectionTitle, Guid parentSectionID, SummarySectionType sectionType, int priority)
        {
            // HACK: This code is called from the ContentRendering code. This 
            // should be an extraction task.
            SummarySection level5Section = new SummarySection();
            level5Section.SummarySectionID = Guid.NewGuid();
            level5Section.SectionID = sectionID;
            level5Section.Title = sectionTitle;
            level5Section.ParentSummarySectionID = parentSectionID;
            level5Section.Level = 5;
            level5Section.SectionType = sectionType;
            level5Section.Priority = priority;

            this.Level5SectionList.Add(level5Section);
        }

        /// <summary>
        /// Adds the section to the appropriate collection of sections.
        /// </summary>
        /// <param name="section"></param>
        public void AddSection(SummarySection section)
        {
            if (section.IsTableSection)
            {
                this._tableSectionList.Add(section);
            }
            else
            {
                switch (section.Level)
                {
                    case 4:
                        this._level4SectionList.Add(section);
                        break;
                    case 5:
                        this._level5SectionList.Add(section);
                        break;
                    default:
                        this._sectionList.Add(section);
                        break;
                }
            }
        }

        /// <summary>
        /// Finds the summary section by ID (Guid).
        /// </summary>
        /// <param name="sectionID"></param>
        /// <returns></returns>
        public SummarySection FindSection(Guid sectionID)
        {
            SummarySection findSection = 
                this._sectionList.Find(
                delegate(SummarySection section) 
                {
                    if (section.SummarySectionID == sectionID) 
                        return true; 
                    else 
                        return false; 
                });

            return findSection;
        }

        /// <summary>
        /// Returns a System.String that represents the SummaryDocument.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(string.Format(" DocumentID = {0} Title = {1} Type = {2} Description = {3} BasePrettyURL = {4} AudienceType = {5} ", 
                this.DocumentID, this.Title, this.Type, this.Description, this.BasePrettyURL, this.AudienceType));

            sb.Append(string.Format("Language = {0} ReplacementForID = {1} ", Language, ReplacementForID));

            sb.Append("\nPrettyURLMap = \n");
            foreach (string key in this.PrettyUrlMap.Keys)
            {
                sb.Append(string.Format("{0} => {1} \n", key, this.PrettyUrlMap[key]));
            }

            sb.Append("PrettyUrlReferenceMap = \n");
            foreach (string key in this.PrettyUrlReferenceMap.Keys)
            {
                sb.Append(string.Format("{0} => {1} \n", key, this.PrettyUrlReferenceMap[key]));
            }

            sb.Append("SectionList = \n");
            foreach (SummarySection summarySection in SectionList)
            {
                sb.Append("---------------\n");
                sb.Append(string.Format("{0}\n", summarySection.ToString()));
            }

            sb.Append("RelationList = \n");
            foreach (SummaryRelation summaryRelation in this.RelationList)
            {
                sb.Append("---------------\n");
                sb.Append(string.Format("{0}\n", summaryRelation.ToString()));
            }

            return sb.ToString();
        }

        #endregion Public Methods
    }
}
