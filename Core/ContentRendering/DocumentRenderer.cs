#define DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Security.Permissions;
using GateKeeper.DocumentObjects;

namespace GateKeeper.ContentRendering
{
    /// <summary>
    /// Base class for document rendering.
    /// </summary>
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
            StringBuilder sb = new StringBuilder();

            System.IO.StringWriter sw = new System.IO.StringWriter(sb);

            this.Render(document.Xml.CreateNavigator(), sw);

            document.Html = sb.ToString();
#if DEBUG
            string path = "C:\\temp\\Output\\";
            if (Directory.Exists(path))
            {
                string fileName = path + document.DocumentType.ToString() + document.DocumentID.ToString() + ".xml";
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.LoadXml(document.Html);
                //Save the document to a file.
                doc.Save(fileName);
            }
#endif
            //Prasad: To resolve issue 738.
            document.PostRenderXml.PreserveWhitespace = true;
            document.PostRenderXml.LoadXml(sb.ToString());
        }

        /// <summary>
        /// Common rendering code.
        /// </summary>
        /// <param name="navigator"></param>
        /// <param name="output"></param>
        protected void Render(XPathNavigator navigator, System.IO.TextWriter output)
        {
            this._transform.Transform(navigator, null, output);
        }

        /// <summary>
        /// Common rendering code.
        /// </summary>
        /// <param name="navigator"></param>
        /// <param name="output"></param>
        protected void Render(XPathNavigator navigator, System.Xml.XmlWriter output)
        {
            this._transform.Transform(navigator, null, output);
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
