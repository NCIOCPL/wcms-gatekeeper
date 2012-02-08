using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using GateKeeper.Common;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Helper class to extract MediaDocuments from source XML.
    /// </summary>
    public static class MediaExtractor
    {
        #region Public Static Methods

        /// <summary>
        /// Modifies the document XML, so that subsequent processing is based on
        /// ideal input.
        /// </summary>
        /// <param name="xmlDoc"></param>
        public static void PrepareXml(XmlDocument xmlDoc)
        {
            // TODO: Add code to "prepare" xml (fix problems with data) 
        }

        /// <summary>
        /// Extract the media document from the XML input.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="media"></param>
        /// <param name="documentID">Media XML documents do not contain document IDs so this must be passed in</param>
        public static void Extract(XmlDocument xmlDoc, MediaDocument media, int documentID, DocumentXPathManager xPathManager )
        {
            try
            {
                // The media document XML does not contain the CDR ID...
                media.DocumentID = documentID;
                DocumentHelper.CopyXml(xmlDoc, media);

                // Assign document type
                media.DocumentType = DocumentType.Media;

                XPathNavigator docNav = xmlDoc.CreateNavigator();
                if (docNav.MoveToFirstChild())
                {
                    string strBase64String = docNav.Value;

                    //MemoryStream imgMS = new MemoryStream(System.Convert.FromBase64String(strBase64String));
                    //media.ImageMedia = System.Drawing.Image.FromStream(imgMS);
                    media.EncodedData = strBase64String;
                    if (docNav.HasAttributes)
                    {
                        media.MimeType = docNav.GetAttribute(xPathManager.GetXPath(MediaXPath.Type), string.Empty);
                        media.Size = docNav.GetAttribute(xPathManager.GetXPath(MediaXPath.Size), string.Empty);
                        media.EncodingType = docNav.GetAttribute(xPathManager.GetXPath(MediaXPath.Encoding), string.Empty);
                    }
                }
                else
                {
                    throw new Exception("Extraction Error: Unable to locate the image element in media document.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Failed to extract media document", e);
            }
        }

        #endregion Public Static Methods
    }
}
