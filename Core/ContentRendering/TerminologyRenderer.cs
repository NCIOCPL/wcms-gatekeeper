using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Terminology;


namespace GateKeeper.ContentRendering
{
    public class TerminologyRenderer : DocumentRenderer
    {
        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TerminologyRenderer(){ }
        #endregion

        #region Public Methods

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
                // Replace the ExternalRef with link
                string htmlText = termDoc.Html;
                htmlText = htmlText.Replace("<ExternalRef", "<a class=\"navigation-dark-red\"");
                htmlText = htmlText.Replace("xref", "href");
                htmlText = htmlText.Replace("</ExternalRef>", "</a>");
                termDoc.Html = htmlText;
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Render data from terminology document failed. Document CDRID=" + terminologyDoc.DocumentID.ToString(), e);
            }
        }

        #endregion

    }
}

