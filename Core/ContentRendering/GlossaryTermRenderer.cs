using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.Logging;

namespace GateKeeper.ContentRendering
{
    public class GlossaryTermRenderer : DocumentRenderer
    {
        /// <summary>
        /// Maps the document type specific XSL to the base class.
        /// </summary>
        /// 
        #region Fields

        private string _language;

        #endregion

        #region Constructors
        public GlossaryTermRenderer()
        {}
        #endregion

        #region Public Methods
        /// <summary>
        /// Render Media Link and Definition text into HTML format
        /// </summary>
        /// <param name="GlossaryTermDocument"></param>
        public override void Render(Document glossaryDoc)
        {
            try
            {
                GlossaryTermDocument GTDocument = (GlossaryTermDocument)glossaryDoc;

                // Render Media Link HTML
                foreach (Language lang in GTDocument.GlossaryTermTranslationMap.Keys)
                {
                    GlossaryTermTranslation trans = GTDocument.GlossaryTermTranslationMap[lang];

                    // Render media link HTML
                    foreach (MediaLink ml in trans.MediaLinkList)
                    {
                        if (ml.Language == lang)
                        {
                            ml.Html = DocumentRenderHelper.ProcessMediaLink(ml, true, false);
                        }
                    }

                    // Render Definition HTML
                    //DocumentRenderHelper.TextLanguage = lang.ToString();
                    _language = lang.ToString();
                    foreach (GlossaryTermDefinition gtDef in trans.DefinitionList)
                    {
                        string definition = gtDef.Text;
                        if (definition.Length >= 0)
                        {
                            Regex expr = new Regex("<(ExternalRef|LOERef|ProtocolRef|GlossaryTermRef)\\s+(?:h|x)ref=\"(.+?)\">(.+?)</.+?>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            definition = expr.Replace(definition, ReferenceStringReplace);
                           // definition = definition.Replace("'", "''");
                        }
                        gtDef.Html = definition;

                        // Take out the xml tags from the definition text
                        string text = gtDef.Text;
                        RemoveXMLTags(ref text);
                        gtDef.Text = text;
                   }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Rendering data from glossary term document failed. Document CDRID=" + glossaryDoc.DocumentID.ToString(), e);
            }
        }

       #endregion

        #region Private methods

        // <summary>
        /// This method is called to remove tags in the definition text
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        protected void RemoveXMLTags(ref string htmlText)
        {
            // Remove <></> tag
            int begin = htmlText.IndexOf("<");
            while (begin >= 0)
            {
                int end = htmlText.IndexOf(">");
                string tag = htmlText.Substring(begin, end - begin + 1);
                htmlText = htmlText.Replace(tag, string.Empty);
                begin = htmlText.IndexOf("<");
            }

            // Replace xml special chars
            htmlText = htmlText.Replace("&amp;", "&");
            htmlText = htmlText.Replace("&lt; ", "<");
            htmlText = htmlText.Replace("&gt; ", ">");
            htmlText = htmlText.Replace("&quot;", "\"");
            htmlText = htmlText.Replace("&apos;", "'");
        }

        /// <summary>
        /// Replace the reference with reference hyperlink
        /// </summary>
        /// <param name="match"></param>
        public string ReferenceStringReplace(Match match)
        {
            string input = match.Groups[0].Value;
            string type = match.Groups[1].Value;
            string reference = match.Groups[2].Value;
            string text = match.Groups[3].Value;
            string cdrID = Regex.Replace(reference, "^CDR0+", "", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

            switch (type)
            {
                case "ExternalRef": return "<a href=\"" + reference + "\" target=\"new\">" + text + "</a>"; break;
                case "LOERef": return "<a href=\"/templates/db_alpha.aspx?cdrid=" + cdrID + "&version=healthprofessional&language=" + _language + "\" onclick=\"javascript:popWindow('defbyid','" + reference + "'); return(false);\">" + text + "</a>"; break;
                case "ProtocolRef": return "<a href=\"/search/viewclinicaltrials.aspx?cdrid=" + cdrID + "&version=patient\" target=\"new\">" + text + "</a>"; break;
                /*
                case "SummaryRef": 
                    GetSummaryLinkInfo(cdrID, ref viewID, ref audience);
                    return "<a href=\"/templates/doc.aspx?viewid=" + viewID + "&version=" + (audience == "Patients" ? "0" : "1") + "\" target=\"new\">" + text + "</a>"; 
                    break;
                */
                case "GlossaryTermRef": return "<a href=\"/templates/db_alpha.aspx?cdrid=" + cdrID + "\" onclick=\"javascript:popWindow('defbyid','" + reference + "'); return(false);\">" + text + "</a>"; break;
                default: break;
            }

            return input;
        }

        #endregion

    }
}
