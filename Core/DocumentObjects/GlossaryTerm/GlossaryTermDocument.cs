using System;
using System.Collections.Generic;
using System.Text;
using GateKeeper.Common;
using System.Xml;
using System.Xml.Serialization;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// Class to represent a glossary term document.
    /// </summary>
    [XmlRoot("GlossaryTerm")]
    public class GlossaryTermDocument : Document, IXmlSerializable
    {
        const String NAME_ELEMENT = "TermName";
        const String ENGLISH_DEFINITION_ELEMENT = "TermDefinition";

        const String SPANISH_NAME_ELEMENT = "SpanishTermName";
        const String SPANISH_DEFINITION_ELEMENT = "SpanishTermDefinition";

        private String _termName;
        private String _spanishTermName;


        /// <summary>
        /// Default constructor.
        /// </summary>
        public GlossaryTermDocument()
            : base()
        {
            this.DocumentType = DocumentType.GlossaryTerm;
        }

        #region IXmlSerializable Members

        /// <summary>
        /// Provides an implementation of GetSchema() in order to fulfill the
        /// IXmlSerializable contract. A return of NULL is the defacto standard
        /// implementation and there are other, better, and *supported* ways to 
        /// return a Schema if one is ever needed.
        /// </summary>
        /// <returns></returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToAttribute("id");
            if (reader.ReadAttributeValue())
            {
                //this.ID = reader.ReadContentAsString();
            }


            while (reader.Read())
            {
                if (reader.Name == NAME_ELEMENT)
                {
                    _termName = reader.ReadString();
                }
                else if (reader.Name == SPANISH_NAME_ELEMENT)
                {
                    _spanishTermName = reader.ReadString();
                }
                else if (reader.Name == ENGLISH_DEFINITION_ELEMENT)
                {
                    // Deserialize a TermDefinition and mark it as English.
                    // This feels slightly kludgy -- XmlSerializer.Deserialize() process the current
                    // element and its children and then advances to the next element.
                    // The Read() at the top of the loop also advances to the next element.
                    // This can result in records being skipped.  Rather than allow that,
                    // we create a new reader for just the current element and deserialize
                    // from that, leaving the original reader in place.

                    //XmlSerializer engSerializer = new XmlSerializer(typeof(EnglishTermDefinition));
                    //XmlReader subTree = reader.ReadSubtree();
                    //Definition def = (Definition)engSerializer.Deserialize(subTree);
                    //if (def != null)
                    //    englishDefinitionMetadata.Add(def);
                }
                else if (reader.Name == SPANISH_DEFINITION_ELEMENT)
                {
                    // Deserialize a TermDefinition and mark it as Spanish.
                    //XmlSerializer spanSerializer = new XmlSerializer(typeof(SpanishTermDefinition));
                    //Definition def = (Definition)spanSerializer.Deserialize(reader);
                    //if (def != null)
                    //    spanishDefinitionMetadata.Add(def);
                }
            }
        }

        /// <summary>
        /// Provides an WriteXml() in order to implement IXmlSerializable, however this
        /// method is deliberately not implemented as the CDR is responsible for creating the
        /// XML and GateKeeper is only actually interested in deserializing it.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
