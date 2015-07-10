using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Configuration;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.DocumentObjects.Media;

namespace GateKeeper.ContentRendering
{
    /// <summary>
    /// Class to render summary document type.
    /// </summary>
    public class SummaryRenderer : DocumentRenderer
    {

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SummaryRenderer()
        {
            string xslPath = ConfigurationManager.AppSettings["Summary"];
            try
            {
                base.LoadTransform(new System.IO.FileInfo(xslPath));
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Loading summary XSL file " + xslPath + " failed.", e);
            }
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Method to pre-render the summary document.
        /// </summary>
        /// <param name="document"></param>
        public override void Render(Document document, TargetedDevice targetedDevice)
        {
            try
            {
                base.Render(document, targetedDevice);

                SummaryDocument summary = (SummaryDocument)document;
                XPathNavigator xNav = summary.PostRenderXml.CreateNavigator();

                foreach (SummarySection section in summary.SectionList)
                {
                    XPathNavigator sectionNav = null;
                    //top level section have <section id="_section_1" and subsections <section id="_1"
                    string expression = string.Format(".//section[@id='_section_{0}']|.//section[@id='_{0}']", section.SectionID);
                    sectionNav = xNav.SelectSingleNode(expression);

                    if (sectionNav != null)
                    {
                        //set the section title 
                        //this takes care of special tags like GeneName being part of the title
                        XPathNavigator titleNav = null;
                        string titleExpression = string.Format(".//h2[@id='_{0}_toc']|.//h3[@id='_{0}_toc']|.//h4[@id='_{0}_toc']|.//h5[@id='_{0}_toc']|.//h6[@id='_{0}_toc']", section.SectionID);
                        titleNav = sectionNav.SelectSingleNode(titleExpression);
                        if (titleNav != null)
                        {
                            section.Title = titleNav.InnerXml;
                            //top level section titles come from the Percussion template for Desktop summaries
                            //the tag can be deleted from the HTML
                            if (section.IsTopLevel)
                                titleNav.DeleteSelf();
                        }

                        // Save the results of the transform into the Html property
                        //Taking care of the spaces around GlossaryTermRefLink
                        string html = sectionNav.OuterXml;
                        if (sectionNav.OuterXml.Contains("GlossaryTermRefs"))
                        {
                            string glossaryTermTag = "definition";
                            BuildGlossaryTermRefLink(ref html, glossaryTermTag);
                        }
                        section.Html.LoadXml(html);
                    }
                    else
                    {
                        throw new Exception("Rendering Error: Cannot find section ID = " + section.SectionID);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Render data from summary document failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }

        /// <summary>
        /// Taking care of the spaces around GlossaryTermRefLink
        /// </summary>
        public void BuildGlossaryTermRefLink(ref string html, string tag)
        {
            string startTag = "<a class=\"" + tag + "\"";
            string endTag = "</a>";
            int startIndex = html.IndexOf(startTag, 0);
            string sectionHTML = html;
            string collectHTML = string.Empty;
            string partC = string.Empty;
            while (startIndex >= 0)
            {
                string partA = sectionHTML.Substring(0, startIndex);
                string left = sectionHTML.Substring(startIndex);
                int endIndex = left.IndexOf(endTag) + endTag.Length;
                string partB = left.Substring(0, endIndex);
                partC = left.Substring(endIndex);

                // Combine
                // Do not add extra space after the GlossaryTermRef if following sign
                // is after the SummaryRef )}].,:;? " with )}].,:;? or space after it, ' with )]}.,:;? or space after it.
                if (Regex.IsMatch(partA.Trim(), "^[).,:;!?}]|^]|^\"[).,:;!?}\\s]|^\'[).,:;!?}\\s]|^\"]|^\']") || collectHTML.Length == 0)
                    collectHTML += partA.Trim();
                else
                    collectHTML += " " + partA.Trim();

                // Do not add extra space before the GlossaryTermRef if following sign is lead before the link: ({[ or open ' "
                if (Regex.IsMatch(collectHTML, "[({[\\-/]$|[({[\\-\\s]\'$|[({[\\-\\s]\"$"))
                    collectHTML += partB;
                else
                    collectHTML += " " + partB;

                sectionHTML = partC.Trim();
                startIndex = sectionHTML.IndexOf(startTag, 0);
            }
            html = collectHTML + partC;
        }


        #endregion Public Methods
    }
}
