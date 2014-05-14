using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// Business object to store one of the definitions associated with a GlossaryTermDefinition.
    /// </summary>
    public class GlossaryTermDefinition
    {
        #region Fields

        private List<AudienceType> _audienceTypeList;
        private List<string> _dictionaryNameList;
        private List<RelatedInformationLink> _relatedInformationList = new List<RelatedInformationLink>();

        #endregion

        #region Constructors

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="audienceTypeList"></param>
        /// <param name="dictionaryNameList"></param>
        public GlossaryTermDefinition(string text, List<AudienceType> audienceTypeList, List<string> dictionaryNameList)
        {
            this.Text = text;
            this._audienceTypeList = audienceTypeList;
            this._dictionaryNameList = dictionaryNameList;
        }

        /// <summary>
        /// Class constructor with HTML field
        /// </summary>
        /// <param name="text"></param>
        /// <param name="html"></param>
        /// <param name="audienceTypeList"></param>
        /// <param name="dictionaryNameList"></param>
        public GlossaryTermDefinition(string html, string text, List<AudienceType> audienceTypeList, List<string> dictionaryNameList)
        {
            this.Html = html;
            this.Text = text;
            this._audienceTypeList = audienceTypeList;
            this._dictionaryNameList = dictionaryNameList;
        }


        #endregion

        #region Public Properties

        /// <summary>
        /// Gets, sets glossary term definition text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets, sets glossary term definition html.
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// Gets, sets glossary term definition audience type list.
        /// </summary>
        public List<AudienceType> AudienceTypeList
        {
            get { return _audienceTypeList; }
            set { _audienceTypeList = value; }
        }

        /// <summary>
        /// Gets, sets glossary term definition dictionary name list.
        /// </summary>
        public List<string> DictionaryNameList
        {
            get { return _dictionaryNameList; }
            set { _dictionaryNameList = value; }
        }

        /// <summary>
        /// A list of links to information related to this definition.
        /// </summary>
        public List<RelatedInformationLink> RelatedInformationList
        {
            get { return _relatedInformationList; }
            set { _relatedInformationList = value; }
        }

        /// <summary>
        /// The rendered HTML for this definition's related links.
        /// </summary>
        public string RelatedInformationHTML { get; set; }

        #endregion
    }
}
