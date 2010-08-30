using System;
using System.Collections.Generic;
using System.Text;
using GateKeeper.Common;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// Class to represent a glossary term document.
    /// </summary>
    [Serializable]
    public class GlossaryTermDocument : Document
    {
        #region Fields

        private Dictionary<Language, GlossaryTermTranslation> _glossaryTermTranslationMap = new Dictionary<Language, GlossaryTermTranslation>();

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GlossaryTermDocument()
        {
            this.DocumentType = DocumentType.GlossaryTerm;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets, sets a map of different translations of the glossary term.
        /// </summary>
        public Dictionary<Language, GlossaryTermTranslation> GlossaryTermTranslationMap
        {
            get { return _glossaryTermTranslationMap; }
            internal set { _glossaryTermTranslationMap = value; }
        }
       #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String that represents the GlossaryTermDocument.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append("GlossaryTermList = \n");
            foreach (Language language in this.GlossaryTermTranslationMap.Keys)
            {
                sb.Append(string.Format("{0} => {1} \n", language.ToString(), 
                    GlossaryTermTranslationMap[language].ToString()));
            }

            return sb.ToString();
        }

       #endregion
    }
}
