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
        private MediaLinkCollection _mediaLinkList = new MediaLinkCollection();
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
        public GlossaryTermTranslation(string termName, string pronounciation, Language language, List<GlossaryTermDefinition> definitionList, string html)
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
        public MediaLinkCollection MediaLinkList
        {
            get { return _mediaLinkList; }
        }

        public List<GlossaryTermDefinition> DefinitionList
        {
            get { return _definitionList; }
            set { _definitionList = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the rendered markup for the translation's audio MediaLink object.
        /// </summary>
        /// <returns>Markup to render the audio MediaLink, if it exists.  Otherwise returns String.empty.</returns>
        public String GetAudioMarkup()
        {
            String markup;
            MediaLink audioLink = null;

            // For a given language, the pronunciation is the same, regardless of
            // audience, so just find the first audio link for the language, without
            // requiring an audience to be specified.
            foreach (AudienceType audience in MediaLinkList.Audiences)
            {
                foreach (MediaLink ml in MediaLinkList[audience])
                {
                    if (ml.Language == this.Language && ml.Type.Contains("audio"))
                    {
                        audioLink = ml;
                        break;
                    }
                }
            }

            markup = (audioLink == null) ? String.Empty : audioLink.Html;

            return markup;
        }

        /// <summary>
        /// Retrieves the markup for this translation's image MediaLink objects.
        /// </summary>
        /// <param name="audience">The intended audience for the images.</param>
        /// <returns>If any image MediaLinks exist, the markup to render the whole set is returned.
        /// If no image MediaLink exists, String.empty is returned.</returns>
        public String GetImageMarkup(AudienceType audience)
        {
            String markup = String.Empty;

            foreach (MediaLink ml in MediaLinkList[audience])
            {
                if (ml.Language == this.Language && (string.IsNullOrEmpty(ml.Type) || ml.Type.Contains("image")))
                {
                    markup += ml.Html;
                }
            }

            return markup;
        }

        /// <summary>
        /// Retrieves the CDRID of the images associated with this translation.
        /// </summary>
        /// <param name="audience">The intended audience for the images.</param>
        /// <returns>A List containing the CDRIDs of the iamges.  If no images are associated with the translation, an empty 
        /// list is returned.</returns>
        public List<int> GetImageIDColl(AudienceType audience)
        {
            List<int> cdridCollection = new List<int>();

            foreach (MediaLink ml in MediaLinkList[audience])
            {
                if (ml.Language == this.Language && (string.IsNullOrEmpty(ml.Type) || ml.Type.Contains("image")))
                {
                    cdridCollection.Add(ml.MediaDocumentId);
                }
            }

            return cdridCollection;
        }

        /// <summary>
        /// Retrieves the captions of the images associated with this translation.
        /// </summary>
        /// <param name="audience">The intended audience for the images.</param>
        /// <returns>A List containing the captions of the iamges.  If no images are associated with the translation, an empty 
        /// list is returned.</returns>
        public List<String> GetImageCaptionColl(AudienceType audience)
        {
            List<String> captionCollection = new List<String>();

            foreach (MediaLink ml in MediaLinkList[audience])
            {
                if (ml.Language == this.Language && (string.IsNullOrEmpty(ml.Type) || ml.Type.Contains("image")))
                {
                    captionCollection.Add(ml.Caption);
                }
            }

            return captionCollection;
        }

        #endregion
    }
}
