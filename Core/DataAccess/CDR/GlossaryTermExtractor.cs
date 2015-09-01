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
using System.Xml.XPath;
using GateKeeper.DataAccess.CancerGov;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Class to extract GlossaryTerm object from XML.
    /// </summary>
    public class GlossaryTermExtractor
    {
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

                GeneralDictionaryEntry[] dictionary = GetDictionary(extractData);
                glossaryTermDocument.Dictionary.AddRange(dictionary);

                // Hack to rewrite related Glosssary Term elements.
                RewriteRelatedGlossaryTerms(xmlDoc, document);

                // Copy the the XML document into the Glossary Term business object because that's
                // where all the rest of the code expects to find it.
                DocumentHelper.CopyXml(xmlDoc, glossaryTermDocument);
            }
            catch (Exception e)
            {
                throw new ExtractionException("Extraction Error: Failed to extract glossary term document", e);
            }
        }

        /// <summary>
        /// Hack to rewrite the XML document's references to related Glossary Terms so they can be
        /// rendered more correctly in the JSON. (NOTE: The XML is rewritten based on the languages
        /// and audiences of any related terms. It is possible that related term references will be
        /// written for language and audiences that the current document knows nothing about.)
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="document"></param>
        private void RewriteRelatedGlossaryTerms(XmlDocument xmlDoc, Document gtDocument)
        {
            XPathNavigator nav = xmlDoc.CreateNavigator();
            XPathNodeIterator relatedTerms = nav.Select("//RelatedInformation/RelatedGlossaryTermRef");

            // Template for writing new RelatedGlossaryTermRef elements.
            string TermElementFmt = "<RelatedGlossaryTermRef href=\"{0}\" UseWith=\"{1}\" audience=\"{2}\">{3}</RelatedGlossaryTermRef>";

            List<XPathNavigator> oldEntries = new List<XPathNavigator>();

            foreach (XPathNavigator term in relatedTerms)
            {
                // Keep a list of pre-rewrite entries for eventual clean-up.
                oldEntries.Add(term);

                // Get related Term ID.
                string idAsString = DocumentHelper.GetAttribute(term, "href");
                int relatedID;
                if (!Int32.TryParse(CDRHelper.ExtractCDRID(idAsString), out relatedID))
                    throw new Exception("Failed to parse" + term.Name + " document id from value: " + idAsString);

                // Fetch the referenced glossary term so we can determine audiences, languages, etc.
                GlossaryTermSimple relatedTerm = getRelatedGlossaryTerm(relatedID, gtDocument.WarningWriter);

                if (relatedTerm == null)
                {
                    gtDocument.WarningWriter(String.Format("RewriteRelatedGlossaryTerms: Unable to fetch related term '{0}'.", relatedID));
                    continue;
                }


                // Insert new nodes for specific languages and audiences *before* the current node.
                // (Inserting after would cause them to be picked up as next in the list, repeating
                // the loop endlessly.)
                if (relatedTerm.HasPatientDefinition)
                {
                    // We assume the related term has an English version.
                    term.InsertBefore(String.Format(TermElementFmt, idAsString, "en", AudienceType.Patient, relatedTerm.Name));
                    if(relatedTerm.HasSpanishTerm)
                        term.InsertBefore(String.Format(TermElementFmt, idAsString, "es", AudienceType.Patient, relatedTerm.SpanishName));
                }

                if (relatedTerm.HasHealthProfessionalDefinition)
                {
                    // We assume the related term has an English version.
                    // Currently, no dicitonary uses Spanish with a Health Professional definition.
                    term.InsertBefore(String.Format(TermElementFmt, idAsString, "en", AudienceType.HealthProfessional, relatedTerm.Name));
                }

                // Remove the orignal element.
                //term.DeleteSelf();
            }

            // Remove the original entries from the XML.  (Deleting them earlier would invalidate the iterator.)
            oldEntries.ForEach(entry => { entry.DeleteSelf(); });
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
                entry.ApiVersion = "v1";
                
                dictionary.Add(entry);
            }

            return dictionary.ToArray();
        }


        private GlossaryTermSimple getRelatedGlossaryTerm(int documentId, HistoryEntryWriter logWriter)
        {
            GlossaryTermSimple term = null;
            try
            {
                GlossaryTermQuery query = new GlossaryTermQuery();
                term = query.GetGlossaryTerm(documentId);
            }
            catch (Exception ex)
            {
                // Swallow the exception.  Not finding the related term shouldn't be fatal.
                logWriter("HistoryEntryWriter.getRelatedGlossaryTerm(). Error retrieveing documentId = " + documentId + ". " + ex.ToString());
            }
            return term;
        }
    
    }
}
