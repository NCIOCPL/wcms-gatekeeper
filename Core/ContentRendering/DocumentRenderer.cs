#define DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Security.Permissions;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;

namespace GateKeeper.ContentRendering
{
    /// <summary>
    /// Base class for document rendering.
    /// </summary>
    /// 
    [XmlInclude(typeof(GeneticsProfessionalRenderer))]
    [XmlInclude(typeof(GlossaryTermRenderer))]
    [XmlInclude(typeof(MediaRenderer))]
    [XmlInclude(typeof(ProtocolRenderer))]
    [XmlInclude(typeof(SummaryRenderer))]
    [XmlInclude(typeof(TerminologyRenderer))]
    public class DocumentRenderer
    {
        #region Fields

        private XslCompiledTransform _transform = new XslCompiledTransform();

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Transform to use for rendering.
        /// </summary>
        protected XslCompiledTransform Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Common document type rendering implementation.
        /// </summary>
        /// <param name="document"></param>
        public virtual void Render(Document document)
        {
            // TODO: Convert all document renderers to use the 
            // TargetedDevice override directly.
            Render(document, TargetedDevice.screen);
        }

        /// <summary>
        /// Common document type rendering implementation.
        /// </summary>
        /// <param name="document"></param>
        public virtual void Render(Document document, TargetedDevice outputDevice)
        {
            // Maps to the $targetedDevice parameter available in the XSL transform.
            // See: ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/wd_xml/html/fe60aaa0-ae43-4b1c-9be1-426af66ba757.htm
            XsltArgumentList renderParameters = new XsltArgumentList();
            renderParameters.AddParam("targetedDevice", string.Empty, outputDevice.ToString());

            this.Render(document, renderParameters);
        }

        /// <summary>
        /// Performs a document XSL transformation using an arbitrary set of parameters.
        /// </summary>
        /// <param name="document">The GateKeeper document object which contains the XML to be rendered.</param>
        /// <param name="renderParameters">A list of parameter names amd values.</param>
        public virtual void Render(Document document, XsltArgumentList renderParameters)
        {

            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);

            this._transform.Transform(document.Xml.CreateNavigator(), renderParameters, sw);
               
            document.Html = sb.ToString();
#if DEBUG
            string path = "C:\\temp\\Output\\";
            if (Directory.Exists(path))
            {
                string fileName = path + document.DocumentType.ToString() + document.DocumentID.ToString() + ".xml";
                using (StreamWriter writer = File.CreateText(fileName))
                {
                    writer.Write(document.Html);
                    writer.Close();
                }
            }
#endif
            //OCEPROJECT 3101 - make sure space is preserved when there are two elements next to each other
            document.PostRenderXml.PreserveWhitespace = true;
            document.PostRenderXml.LoadXml(sb.ToString());
        }

        /// <summary>
        /// Common rendering code.  This is the routine which does the actual rendering.
        /// </summary>
        /// <param name="navigator"></param>
        /// <param name="output"></param>
        private void Render(XPathNavigator navigator, XsltArgumentList parameters, System.IO.TextWriter output)
        {
            this._transform.Transform(navigator, parameters, output);
        }



        /// <summary>
        /// Loads the XSL pointed to by the file path into the Transform.
        /// </summary>
        /// <param name="fileInfo"></param>
        protected void LoadTransform(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                FileIOPermission ioPermission =
                    new FileIOPermission(FileIOPermissionAccess.Read, fileInfo.FullName);
                ioPermission.Demand();

                _transform.Load(fileInfo.FullName);
            }
        }

        #endregion
    }
}
