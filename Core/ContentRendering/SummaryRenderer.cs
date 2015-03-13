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
                    string expression = string.Format(".//section[contains(@id,'_section_{0}')]|.//section[@id='_{0}']", section.SectionID);
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
                            if (section.IsTopLevel && targetedDevice != TargetedDevice.mobile)
                                titleNav.DeleteSelf();
                        }

                        // Save the results of the transform into the Html property
                        section.Html.LoadXml(sectionNav.OuterXml);
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
                
        #endregion Public Methods
    }
}
