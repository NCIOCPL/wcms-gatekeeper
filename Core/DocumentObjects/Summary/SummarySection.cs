using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Class to represent a section of a summary document.
    /// </summary>
    [Serializable]
    public class SummarySection
    {
        #region Fields

        private Guid _summarySectionID = Guid.Empty;
        private Guid _parentSummarySectionID = Guid.Empty;
        private string _rawSectionID = string.Empty;
        private XmlDocument _xml = null;
        private XmlDocument _html = null;
        private XmlDocument _standaloneHtml = null; // HACK: See StandaloneHTML property.
        private string _title = string.Empty;
        private string _text = string.Empty;
        private string _toc = string.Empty;
        private bool _isTopLevel = false;
        private string _prettyUrl = string.Empty;
        private bool _isTableSection = false;
        private int _level = 0;
        private int _priority = 0;
        private SummarySectionType _sectionType = SummarySectionType.SummarySection;
        private List<string> _linkableNodeRawIDList = new List<string>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Internal document identifier for the section.
        /// 
        /// Legacy code included some hackishness with removing the first character
        /// from the ID.  The getter for this property is preserved in order to minimize
        /// the impact on existing code, however the *unmodified* ID value must be
        /// set via RawSectionID.
        /// </summary>
        /// <example>Typically of the form: "51"</example>
        public string SectionID
        {
            get
            {
                if (string.IsNullOrEmpty(_rawSectionID))
                    return string.Empty;
                else
                    return _rawSectionID.Substring(1);
            }
        }

        /// <summary>
        /// Document identifier for the section.  RawSectionID contains the ID exactly
        /// as it appears in the document's XML source.
        /// 
        /// The RawSectionID is a workaround for some legacy hackishness where ID values
        /// were created by removing the first character from the section's actual ID value.
        /// Legacy code continues to use the SectionID property which continues to remove
        /// the leading character.  The RawSectionID is used wherever the original, unaltered
        /// value is needed.
        /// </summary>
        public string RawSectionID
        {
            get { return _rawSectionID; }
            internal set { _rawSectionID = value; }
        }

        /// <summary>
        /// Unique identifier for the summary section.
        /// </summary>
        public Guid SummarySectionID
        {
            get { return _summarySectionID; }
            internal set { _summarySectionID = value; }
        }

        /// <summary>
        /// Parent summary section ID.
        /// </summary>
        public Guid ParentSummarySectionID
        {
            get { return _parentSummarySectionID; }
            internal set { _parentSummarySectionID = value; }
        }

        /// <summary>
        /// List of raw IDs for the various document nodes which can be
        /// linked to from other documents.
        /// </summary>
        public List<string> LinkableNodeRawIDList
        {
            get { return _linkableNodeRawIDList; }
        }

        /// <summary>
        /// Indicates the type of the section.
        /// </summary>
        public SummarySectionType SectionType
        {
            get { return _sectionType; }
            internal set { _sectionType = value; }
        }

        /// <summary>
        /// Section title.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        /// <summary>
        /// Table of contents for the section.
        /// </summary>
        public string TOC
        {
            get { return _toc; }
            set { _toc = value; }
        }

        /// <summary>
        /// Pre-rendered HTML document.
        /// </summary>
        public XmlDocument Html
        {
            get { return _html; }
        }

        /// <summary>
        /// Pre-rendered HTML document for the standalone version of
        /// a section.  Only used for TableSections.
        /// 
        /// This is a tremendous HACK.  In a proper design, TableSection would
        /// be derived from SummarySection and only TableSection would have
        /// the additonal property.
        /// </summary>
        public XmlDocument StandaloneHTML
        {
            get
            {
                if (_standaloneHtml == null)
                {
                    _standaloneHtml = new XmlDocument();
                    _standaloneHtml.PreserveWhitespace = true;
                }
                return _standaloneHtml;
            }
        }

        /// <summary>
        /// Pretty URL for the section.
        /// </summary>
        public string PrettyUrl
        {
            get { return _prettyUrl; }
            internal set { _prettyUrl = value; }
        }

        /// <summary>
        /// Indicates that this is a top level section.
        /// </summary>
        public bool IsTopLevel
        {
            get { return _isTopLevel; }
            internal set { _isTopLevel = value; }
        }

        /// <summary>
        /// XML for the summary section.
        /// </summary>
        public System.Xml.XmlDocument Xml
        {
            get { return _xml; }
        }

        /// <summary>
        /// Text value of section.
        /// </summary>
        public string Text
        {
            get { return _text; }
            internal set { _text = value; }
        }

        /// <summary>
        /// Indicates if this summary section is a table section.
        /// </summary>
        /// <remarks>
        /// - For each table in a given summary section a 
        /// table "enlarge" is created and inserted in the 
        /// "SummarySection" table (the HTML column stores the 
        /// pre-rendered HTML for the table enlarge)
        /// 
        /// - A ViewObject is also created for the Table section 
        /// in the SummarySection table
        /// </remarks>
        public bool IsTableSection
        {
            get { return _isTableSection; }
            internal set { _isTableSection = value; }
        }

        /// <summary>
        /// Gets, sets section level.
        /// </summary>
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;

                if (_level == 1)
                {
                    this._isTopLevel = true;
                }
                else
                {
                    this._isTopLevel = false;
                }
            }
        }

        /// <summary>
        /// Page display order of the section.
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SummarySection()
        {
            this._html = new XmlDocument();
            this._html.PreserveWhitespace = true;

            this._xml = new XmlDocument();
            this._xml.PreserveWhitespace = true;
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Returns a System.String that represents the SummarySection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(string.Format("\nSectionID = {0} SummarySectionID = {1} TOC = {2} IsTableSection = {3} ",
                this.SectionID, this.SummarySectionID, this.TOC, this.IsTableSection));

            sb.Append(string.Format("Level = {0} SectionType = {1} ",
                this.Level, this.SectionType.ToString()));

            sb.Append(string.Format("IsTopLevel = {0} PrettyUrl = {1} Text = {2} Html = {3}",
                this.IsTopLevel, this.PrettyUrl, this.Text, this.Html.OuterXml));

            return sb.ToString();
        }

        #endregion
    }
}
