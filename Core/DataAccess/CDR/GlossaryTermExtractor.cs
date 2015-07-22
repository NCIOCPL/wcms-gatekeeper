using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using GateKeeper.Common;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Dictionary;
using GateKeeper.DocumentObjects.GlossaryTerm;

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
                // Copy the the XML into the Glossary Term business object
                DocumentHelper.CopyXml(xmlDoc, glossaryTermDocument);


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

                GeneralDictionaryEntry[] dictionary = GetDictionary(extractData);
                glossaryTermDocument.Dictionary.AddRange(dictionary);
            }
            catch (Exception e)
            {
                throw new ExtractionException("Extraction Error: Failed to extract glossary term document", e);
            }
        }

        /// <summary>
        /// Creates a collection of DictionaryEntry objects from the information contained in GlossaryTermMetadata object.
        /// </summary>
        /// <param name="metadata">A GlossaryTermMetadata created from a GlossaryTerm XML document.</param>
        /// <returns></returns>
        private GeneralDictionaryEntry[] GetDictionary(GlossaryTermMetadata metadata)
        {
            List<GeneralDictionaryEntry> dictionary = new List<GeneralDictionaryEntry>();

            foreach (GlossaryTermDefinition item in metadata.DefinitionList)
            {
                GeneralDictionaryEntry entry = new GeneralDictionaryEntry();
                entry.TermID = metadata.ID;
                entry.TermName = metadata.GetTermName(item.Language);
                entry.Dictionary = item.Dictionary;
                entry.Language = item.Language;
                entry.Audience = item.Audience;
                entry.ApiVersion = "v1.0";
                
                dictionary.Add(entry);
            }

            return dictionary.ToArray();
        }
    }
}
