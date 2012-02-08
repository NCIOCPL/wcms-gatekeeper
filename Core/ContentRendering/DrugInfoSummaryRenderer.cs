using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using GateKeeper.DocumentObjects;
using GateKeeper.Common;
using GateKeeper.DocumentObjects.DrugInfoSummary;

namespace GateKeeper.ContentRendering
{
    public class DrugInfoSummaryRenderer : DocumentRenderer
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DrugInfoSummaryRenderer()
        {
            string xslPath = ConfigurationManager.AppSettings["DrugInfoSummary"];
            try
            {
                base.LoadTransform(new System.IO.FileInfo(xslPath));
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Loading drug information summary XSL file " + xslPath + " failed.", e );
            }
        }

        /// <summary>
        /// Method to pre-render the drug information summary html field.
        /// </summary>
        /// <param name="document"></param>
        public override void Render(Document document)
        {
            try
            {
                base.Render(document);
                // Strip off the opening and closing DrugInformationSummary tags.
                // Tricky bit -- the opening tag will include a namespace, so we have
                // to find the end of the tag separately from the beginning.
                string html = document.Html;
                int index = html.IndexOf("<DrugInformationSummary");
                index = html.IndexOf('>', index) + 1;

                html = html.Substring(index);
                html = html.Replace("</DrugInformationSummary>", string.Empty);
                document.Html = html;
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Render data from drug information summary document failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }
    }
}
