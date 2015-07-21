using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Xsl;

using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Dictionary;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.DocumentObjects.Media;

namespace GateKeeper.ContentRendering
{
    /// <summary>
    /// Drives the rendering logic specific to a given document type.
    /// </summary>
    public class GlossaryTermRenderer : DocumentRenderer
    {
        private const String TARGET_LANGUAGE = "targetLanguage";
        private const String TARGET_AUDIENCE = "targetAudience";
        private const String TARGET_DICTIONARY = "targetDictionary";

        public GlossaryTermRenderer()
        {
            string xslPath = ConfigurationManager.AppSettings["GlossaryTerm"];
            try
            {
                base.LoadTransform(new System.IO.FileInfo(xslPath));
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Loading summary XSL file " + xslPath + " failed.", e);
            }
        }


        #region Public Methods
        /// <summary>
        /// Render Media Link and Definition text into HTML format
        /// </summary>
        /// <param name="GlossaryTermDocument"></param>
        public override void Render(Document glossaryDoc)
        {
            try
            {
                GlossaryTermDocument gtDocument = (GlossaryTermDocument)glossaryDoc;

                XsltArgumentList renderParams = new XsltArgumentList();

                foreach (GeneralDictionaryEntry entry in gtDocument.Dictionary)
                {
                    // Clear values from any previous iteration.
                    renderParams.Clear();

                    Language language = entry.Language;
                    AudienceType audience = entry.Audience;
                    DictionaryType dictionary = entry.Dictionary;

                    renderParams.AddParam(TARGET_LANGUAGE, string.Empty, language.ToString());
                    renderParams.AddParam(TARGET_AUDIENCE, string.Empty, audience.ToString());
                    renderParams.AddParam(TARGET_DICTIONARY, string.Empty, dictionary.ToString());

                    Render(glossaryDoc, renderParams);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Rendering data from glossary term document failed. Document CDRID=" + glossaryDoc.DocumentID.ToString(), e);
            }
        }

       #endregion

    }
}
