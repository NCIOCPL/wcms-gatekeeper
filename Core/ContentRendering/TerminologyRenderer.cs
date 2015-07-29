using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Xsl;

using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Dictionary;
using GateKeeper.DocumentObjects.Terminology;


namespace GateKeeper.ContentRendering
{
    public class TerminologyRenderer : DocumentRenderer
    {
        private const String TARGET_LANGUAGE = "targetLanguage";
        private const String TARGET_AUDIENCE = "targetAudience";
        private const String TARGET_DICTIONARY = "targetDictionary";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TerminologyRenderer()
        {
            string xslPath = ConfigurationManager.AppSettings["Terminology"];
            try
            {
                base.LoadTransform(new System.IO.FileInfo(xslPath));
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Loading summary XSL file " + xslPath + " failed.", e);
            }
        }

        /// <summary>
        /// Render Media Link and Definition text into HTML format
        /// This logic is coming from the old gatekeeper's extract store procedure
        /// </summary>
        /// <param name="GlossaryTermDocument"></param>
        public override void Render(Document terminologyDoc)
        {
            try
            {
                TerminologyDocument termDoc = (TerminologyDocument)terminologyDoc;

                // Legacy Render
                // Replace the ExternalRef with link
                string htmlText = termDoc.Html;
                htmlText = htmlText.Replace("<ExternalRef", "<a class=\"navigation-dark-red\"");
                htmlText = htmlText.Replace("xref", "href");
                htmlText = htmlText.Replace("</ExternalRef>", "</a>");
                termDoc.Html = htmlText;

                // New dictionary render.
                XsltArgumentList renderParams = new XsltArgumentList();
                String renderedText;

                foreach (GeneralDictionaryEntry entry in termDoc.DictionaryEntry)
                {
                    // Clear values from any previous iteration.
                    renderParams.Clear();

                    Language language = entry.Language;
                    AudienceType audience = entry.Audience;
                    DictionaryType dictionary = entry.Dictionary;

                    renderParams.AddParam(TARGET_LANGUAGE, string.Empty, language.ToString());
                    renderParams.AddParam(TARGET_AUDIENCE, string.Empty, audience.ToString());
                    renderParams.AddParam(TARGET_DICTIONARY, string.Empty, dictionary.ToString());

                    renderedText = RenderToText(termDoc, renderParams);
                    entry.Object = renderedText;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Render data from terminology document failed. Document CDRID=" + terminologyDoc.DocumentID.ToString(), e);
            }
        }

    }
}

