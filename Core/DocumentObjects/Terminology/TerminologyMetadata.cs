using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GateKeeper.DocumentObjects.Dictionary;
using System.Collections.ObjectModel;
using System.Xml;
using GateKeeper.Common;

namespace GateKeeper.DocumentObjects.Terminology
{
    /// <summary>
    /// TerminologyExtractor class to perform the bulk of the logic for retrieving data from a Drug Dictionary Term
    /// document in a readily available format.
    /// </summary>
    [XmlRoot("Term")]
    public class TerminologyMetadata : IXmlSerializable
    {
        const String DEFINITION_ELEMENT = "Definition";
        const String SEMANTIC_TYPE_ELEMENT = "SemanticType";
        const String TERM_NAME_ELEMENT = "PreferredName";

        /// <summary>
        /// The source Term document's CDRID.
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// The term name (PreferredName), as extracted from the original Terminology document.
        /// </summary>
        [XmlIgnore()]
        public String TermName { get; set; }

        /// <summary>
        /// A collection of metadata for each of the definitions contained in the original
        /// GlosaryTerm document. Use with the EnglishTermName and SpanishTermName to create
        /// a unique combination of termname, language, audience and dictionary.
        /// </summary>
        [XmlIgnore()]
        public TerminologyDefinition Definition { get; set; }
        
        
        /// <summary>
        ///  //For Terminology documents the language is always set to English
        /// </summary>
        [XmlIgnore()]
        public Language Language { get { return Language.English; } }


        // Storage for the SEMANTIC_TYPE_ELEMENT.
        private String _dictionary;
        
        /// <summary>
        /// What dictionary is the definition intended for?
        ///     Drug/agent - Drug dictionary.
        /// </summary>
        [XmlIgnore()]
        public DictionaryType Dictionary { get; private set; }
               

        /// <summary>
        /// What audience is the definition for?
        ///     Patient - Patients and caregivers
        ///     Health professional - Doctors and other health professionals.
        /// </summary>
        [XmlIgnore()]
        public AudienceType Audience { get; set; }

        /// <summary>
        /// Logic for deserializing the TerminologyDefinition 
        /// </summary>
        /// <param name="reader">XmlReader object, already pointing to the beginning of the structure.</param>
        /// <param name="termSubType">Type object for the specific TerminologyDefinition subclass.</param>
        /// <returns>A TerminologyDefinition object containing the deserialized data.</returns>
        private TerminologyDefinition ReadTermDefinition(XmlReader reader, Type termSubType)
        {
            // Use a new serializer to extract definitions instead of fiddling around with an
            // XmlReader and figuring out how to advance through the various components.
            XmlSerializer engSerializer = new XmlSerializer(termSubType);
            XmlReader subTree = reader.ReadSubtree();
            TerminologyDefinition def = (TerminologyDefinition)engSerializer.Deserialize(subTree);

            return def;
        }

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
        /// Provides a WriteXml() in order to implement IXmlSerializable, however this
        /// method is deliberately not implemented as the CDR is responsible for creating the
        /// XML and GateKeeper is only actually interested in deserializing it.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Implement ReadXml for the IXmlSerializable contract. This method is the only part
        /// of IXmlSerializable which receives an actual implementation.
        /// </summary>
        /// <param name="reader"></param>
        /// <remarks>
        /// The Term data structure GateKeeper receives from the CDR doesn't resemble the
        /// structure we want to work with for the dictionaries. Implementing custom deserialization
        /// via IXmlSerializable allows us to take the individual elements we're interested in and store the
        /// results in memory in a structure more suited to the front end's uses.
        /// 
        /// This doesn't deserialize straight to a collection of DictionaryEntry objects, but it does get
        /// to the point where it's relatively straightforward to use the data to create them.
        /// </remarks>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            // Grab the ID.
            reader.MoveToAttribute("id");
            if (reader.ReadAttributeValue())
            {
                this.ID = CDRHelper.ExtractCDRIDAsInt(reader.ReadContentAsString());
            }

            // Find the top-level nodes want to keep, ignore everything else.
            while (reader.Read())
            {

                switch (reader.Name)
                {
                    // Found an instance of the definition element
                    case DEFINITION_ELEMENT:
                        {
                            TerminologyDefinition def = ReadTermDefinition(reader, typeof(TerminologyDefinition));
                            if (def != null)
                                Definition = def;

                        }
                        break;

                    // Type of dictionary 
                    case SEMANTIC_TYPE_ELEMENT:
                        {
                            _dictionary = reader.ReadString(); 
                            if (_dictionary.Equals("drug/agent", StringComparison.InvariantCultureIgnoreCase)) 
                                this.Dictionary = DictionaryType.Drug;
                            else
                                throw new ArgumentException(String.Format("Expected 'Drug/agent' but found '{0}'.", _dictionary));

                        }
                        break;

                    // The term name
                    case TERM_NAME_ELEMENT:
                        TermName = reader.ReadString();
                        break;

                    // Ignore any other elements.
                    default:
                        break;
                }

            }
        
        }
        
    }

}
