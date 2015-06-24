using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

using GateKeeper.Common;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.DocumentObjects.Summary;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Class to extract GlossaryTerm object from XML.
    /// </summary>
    public class GlossaryTermExtractor
    {
        #region Fields
        private int _documentID = 0;
        private DocumentXPathManager xPathManager;
        #endregion

        
        /// <summary>
        /// Modifies the document XML, so that subsequent processing is based on
        /// ideal input.
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PrepareXml(XmlDocument xmlDoc)
        {
            // TODO: Add code to "prepare" xml (fix problems with data) 
        }

        /// <summary>
        /// Main extraction method.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="glossaryTermDocument"></param>
        public void Extract(XmlDocument xmlDoc, Document document, DocumentXPathManager xPath)
        {
            GlossaryTermDocument glossaryTermDocument = document as GlossaryTermDocument;
            if (glossaryTermDocument == null)
            {
                string message = "Expected document of type GlossaryTermDocument, found {0}.";
                throw new DocumentTypeMismatchException(string.Format(message, document.GetType().Name));
            }


            try
            {
                // Use XML deserialization to extract the meta data.

                XmlSerializer serializer = new XmlSerializer(typeof(GlossaryTermMetadata));

                // Use XML Serialization to parse the dictionary entry and extract the term details.
                // Note that this approach is new as of the Feline release (Summer 2015) and is significantly
                // different from the legacy extract mechanisms used elsewhere in this codebase.
                GlossaryTermMetadata extractData;
                using (TextReader reader = new StringReader(xmlDoc.OuterXml))
                {
                    extractData = (GlossaryTermMetadata)serializer.Deserialize(reader);
                }
                //XPathNavigator xNav = xmlDoc.CreateNavigator();

                //// Extract the CDR ID...
                //xNav.MoveToFirstChild();
                //if (CDRHelper.ExtractCDRID(xNav, xPath.GetXPath(CommonXPath.CDRID), out _documentID))
                //{
                //    glossaryTermDocument.DocumentID = _documentID;
                //}
                //else
                //{
                //    throw new Exception("Extraction Error: Failed to extract document (CDR) ID in glossary term document!");
                //}

                //DocumentHelper.CopyXml(xmlDoc, glossaryTermDocument);

                // Get Glossary Term XPath
                //xPathManager = xPath;

                //ExtractEnglishTerm(xNav, glossaryTermDocument);
                //ExtractSpanishTerm(xNav, glossaryTermDocument);
                //ExtractMediaLinks(xNav, glossaryTermDocument);
                //ExtractRelatedInformation(xNav, glossaryTermDocument);

                //// Handle modified and published dates
                //DocumentHelper.ExtractDates(xNav, glossaryTermDocument, xPathManager.GetXPath(CommonXPath.LastModifiedDate), xPathManager.GetXPath(CommonXPath.FirstPublishedDate));
            }
            catch (Exception e)
            {
                throw new ExtractionException("Extraction Error: Failed to extract glossary term document", e);
            }
        }

    }
}
