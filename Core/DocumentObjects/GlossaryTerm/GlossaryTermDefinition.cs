using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    public class GlossaryTermDefinition
    {
        #region Fields
        private string _text = string.Empty;
        private string _html = string.Empty;
        private List<AudienceType> _audienceTypeList;
        private List<string> _dictionaryNameList;
        #endregion

        #region Constructors
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="definitionTextList"></param>
        /// <param name="audienceTypeList"></param>
        /// <param name="dictionaryNameList"></param>
        public GlossaryTermDefinition(string text, List<AudienceType> audienceTypeList, List<string> dictionaryNameList)
        {
            this._text = text;
            this._audienceTypeList = audienceTypeList;
            this._dictionaryNameList = dictionaryNameList;
        }

        /// <summary>
        /// Class constructor with HTML field
        /// </summary>
        /// <param name="text"></param>
        /// <param name="html"></param>
        /// <param name="definitionTextList"></param>
        /// <param name="audienceTypeList"></param>
        /// <param name="dictionaryNameList"></param>
        public GlossaryTermDefinition(string html, string text, List<AudienceType> audienceTypeList, List<string> dictionaryNameList)
        {
            this._html = html;
            this._text = text;
            this._audienceTypeList = audienceTypeList;
            this._dictionaryNameList = dictionaryNameList;
        }


        #endregion

        #region Public Properties
        /// <summary>
        /// Gets, sets glossary term definition text.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// Gets, sets glossary term definition html.
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

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
       #endregion

       #region Public Methods

       /// <summary>
       /// Returns a System.String that represents the GlossaryTermDefinition.
       /// </summary>
       /// <returns></returns>
       public override string ToString()
       {
           StringBuilder sb = new StringBuilder(base.ToString());

           sb.Append(string.Format(" Text = {0} Html = {1}\n",
               this.Text, this.Html));

           sb.Append("AudienceTypeList: \n");
           foreach (AudienceType audienceType in this.AudienceTypeList)
           {
               sb.Append(string.Format("AudienceType = {0}\n", audienceType));
           }

           sb.Append("DictionaryNameList: \n");
           foreach (string definitionText in this.DictionaryNameList)
           {
               sb.Append(string.Format("DefinitionText = {0}\n", _dictionaryNameList));
           }
           return sb.ToString();
       }

       #endregion
    }
}
