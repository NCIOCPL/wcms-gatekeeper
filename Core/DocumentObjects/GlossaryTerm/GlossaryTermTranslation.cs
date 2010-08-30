using System;
using System.Collections.Generic;
using System.Text;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Media;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// Class to represent a glossary term (typically represents 
    /// different translations of a glossary term).
    /// </summary>
    [Serializable]
    public class GlossaryTermTranslation
    {
        #region Fields

        private Language _language = Language.English;
        private string _html = string.Empty;
        private string _pronounciation = string.Empty;
        private string _termName = string.Empty;
        private List<MediaLink> _mediaLinkList = new List<MediaLink>();
        private List<GlossaryTermDefinition> _definitionList = new List<GlossaryTermDefinition>();

       #endregion


        #region Constructors

        /// <summary>
        /// Class constructor (takes html parameter).
        /// </summary>
        /// <param name="termName"></param>
        /// <param name="pronounciation"></param> 
        /// <param name="language"></param>
        /// <param name="definitionList"></param>
         /// <param name="html"></param>
        public GlossaryTermTranslation(string termName, string pronounciation, Language language,List<GlossaryTermDefinition> definitionList, string html)
        {
            this._termName = termName;
            this._pronounciation = pronounciation;
            this._language = language;
            this._definitionList = definitionList;
            this._html = html;
        }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="termName"></param>
        /// <param name="pronounciation"></param>
        /// <param name="language"></param>
        /// <param name="definitionList"></param>
        public GlossaryTermTranslation(string termName, string pronounciation, Language language, List<GlossaryTermDefinition> definitionList)
        {
            this._termName = termName;
            this._pronounciation = pronounciation;
            this._language = language;
            this._definitionList = definitionList;
         }

         #endregion

        #region Public Properties

        /// <summary>
        /// Gets, sets glossary term name.
        /// </summary>
        public string TermName
        {
            get { return _termName; }
            internal set { _termName = value; }
        }

        /// <summary>
        /// Gets, sets Language translation of the term.
        /// </summary>
        public Language Language
        {
            get { return _language; }
            internal set { _language = value; }
        }

        /// <summary>
        /// Gets, sets Pre-rendered HTML.
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        /// <summary>
        /// Gets, sets Pronounciation.
        /// </summary>
        public string Pronounciation
        {
            get { return _pronounciation; }
            set { _pronounciation = value; }
        }

        /// <summary>
        /// Gets, sets a collection of media links (usually links to image files).
        /// </summary>
        public List<MediaLink> MediaLinkList
        {
            get { return _mediaLinkList; }
            internal set { _mediaLinkList = value; }
        }

        public List<GlossaryTermDefinition> DefinitionList
        {
            get { return _definitionList; }
            set { _definitionList = value; }
        }
        #endregion

          #region Public Methods

        /// <summary>
        /// Returns a System.String that represents the GlossaryTermTranslation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" TermName = {0} Language = {1} Html = {2}\n",
                this.TermName, this.Language, this.Html));

            sb.Append("Media links: \n");
            foreach (MediaLink link in this.MediaLinkList)
            {
                sb.Append(link.ToString());
            }

            sb.Append("Definitions: \n");
           // foreach (GlossaryTermDefinition def in this.DefinitionList)
           // {
           //     sb.Append(def.ToString());
           // }

            return sb.ToString();
        }

        #endregion

       

     }
}
