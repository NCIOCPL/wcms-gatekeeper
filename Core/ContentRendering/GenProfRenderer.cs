using System;
using System.Collections.Generic;
using System.Configuration;
using GateKeeper.DocumentObjects;

namespace GateKeeper.ContentRendering
{
    public class GeneticsProfessionalRenderer : DocumentRenderer
    {
        /// <summary>
        /// Class constructor. Maps the Xsl to the base class.
        /// NOTE: Currently not needed (document is rendered by CancerGov front-end).
        /// </summary>
        public GeneticsProfessionalRenderer()
        {
            string xslPath = ConfigurationManager.AppSettings["GeneticsProfessional"];
            try
            {
                base.LoadTransform(new System.IO.FileInfo(xslPath));
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Loading drug information summary XSL file " + xslPath + " failed.", e);
            }
        }
    
        public override void Render(Document document)
        {
            base.Render(document);

            // Strip off the GeneticsProfessional tag.
            string html = document.Html;
            int index = html.IndexOf("<GeneticsProfessional");
            index = html.IndexOf('>', index) + 1;
            html = html.Substring(index);
            html = html.Replace("</GeneticsProfessional>", string.Empty);
            document.Html = html;
        }
    }
}
