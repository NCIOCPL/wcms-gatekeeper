using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using GateKeeper.Common;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// An IXmlSerializable representation of a GlossaryTerm document.  This class is used by
    /// GlossaryTermExtractor to perform the bulk of the logic for retrieving data from a GlossaryTerm
    /// document in a readily available format.
    /// </summary>
    [XmlRoot("GlossaryTerm")]
    public class GlossaryTermMetadata : IXmlSerializable
    {
        const String NAME_ELEMENT = "TermName";
        const String ENGLISH_DEFINITION_ELEMENT = "TermDefinition";

        const String SPANISH_NAME_ELEMENT = "SpanishTermName";
        const String SPANISH_DEFINITION_ELEMENT = "SpanishTermDefinition";

        /// <summary>
        /// The source GlossaryTerm document's CDRID.
        /// </summary>
        public String ID { get; private set; }

        /// <summary>
        /// The English name for term, as extracted from the original GlossaryTerm document.
        /// </summary>
        [XmlIgnore()]
        public String EnglishTermName { get; private set; }

        /// <summary>
        /// The Spanish name for term, as extracted from the original GlossaryTerm document.
        /// </summary>
        [XmlIgnore()]
        public String SpanishTermName { get; private set; }

        /// <summary>
        /// A collection of metadata for each of the definitions contained in the original
        /// GlosaryTerm document. Use with the EnglishTermName and SpanishTermName to create
        /// a unique combination of termname, language, audience and dictionary.
        /// </summary>
        [XmlIgnore()]
        public ReadOnlyCollection<GlossaryTermDefinition> DefinitionList
        {
            get { return new ReadOnlyCollection<GlossaryTermDefinition>(definitionMetadata); }
        }

        private List<GlossaryTermDefinition> definitionMetadata = new List<GlossaryTermDefinition>();

        
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


        /// <summary>
        /// Implement ReadXml for the IXmlSerializable contract. This method is the only part
        /// of IXmlSerializable which receives an actual implementation.
        /// </summary>
        /// <param name="reader"></param>
        /// <remarks>
        /// The GlossaryTerm data structure GateKeeper receives from the CDR doesn't resemble the
        /// structure we want to work with for the dictionaries. Implementing custom deserialization
        /// via IXmlSerializable allows us to take the individual elements we're interested in and store the
        /// results in memory in a structure more suited to the front end's uses.
        /// </remarks>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            // Grab the ID.
            reader.MoveToAttribute("id");
            if (reader.ReadAttributeValue())
            {
                this.ID = CDRHelper.ExtractCDRID(reader.ReadContentAsString());
            }


            while (reader.Read())
            {
                if (reader.Name == NAME_ELEMENT)
                {
                    EnglishTermName = reader.ReadString();
                }
                else if (reader.Name == SPANISH_NAME_ELEMENT)
                {
                    SpanishTermName = reader.ReadString();
                }
                else if (reader.Name == ENGLISH_DEFINITION_ELEMENT)
                {
                    // Use a nested serializer to extract English definitions instead of
                    // having to work directly with an XmlReader and advancing through the various
                    // components.
                    XmlSerializer engSerializer = new XmlSerializer(typeof(EnglishTermDefinition));
                    XmlReader subTree = reader.ReadSubtree();
                    GlossaryTermDefinition def = (GlossaryTermDefinition)engSerializer.Deserialize(subTree);
                    if (def != null)
                        definitionMetadata.Add(def);
                }
                else if (reader.Name == SPANISH_DEFINITION_ELEMENT)
                {
                    // Use a nested serializer to extract Spanish definitions instead of
                    // having to work directly with an XmlReader and advancing through the various
                    // components.
                    XmlSerializer spanSerializer = new XmlSerializer(typeof(SpanishTermDefinition));
                    XmlReader subTree = reader.ReadSubtree();
                    GlossaryTermDefinition def = (GlossaryTermDefinition)spanSerializer.Deserialize(subTree);
                    if (def != null)
                        definitionMetadata.Add(def);
                }
            }
        }

        /// <summary>
        /// Provides a WriteXml() in order to implement IXmlSerializable, however this
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
