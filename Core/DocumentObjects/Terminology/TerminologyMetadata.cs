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
    public class TerminologyMetadata
    {       
        /// <summary>
        /// The source Term document's CDRID.
        /// </summary>
        [XmlIgnore()]
        public int ID { get; private set; }

        /// <summary>
        /// The source Term document's CDRID.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public string CDRID
        {
            get { return ID.ToString(); }
            set
            {
                ID = CDRHelper.ExtractCDRIDAsInt(value);                
            }
        }

        /// <summary>
        /// The term name (PreferredName), as extracted from the original Terminology document.
        /// </summary>
        [XmlElement(ElementName = "PreferredName")]
        public String TermName { get; set; }

        /// <summary>
        /// A collection of metadata for each of the definitions contained in the original
        /// GlosaryTerm document. Use with the EnglishTermName and SpanishTermName to create
        /// a unique combination of termname, language, audience and dictionary.
        /// </summary>
        [XmlElement(ElementName = "Definition")]
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
        [XmlElement(ElementName = "SemanticType")]
        public String DictionaryKludge
        {
            get { return this.Dictionary.ToString(); }
            set
            {
                _dictionary = value;
                if (_dictionary.Equals("drug/agent", StringComparison.InvariantCultureIgnoreCase))
                    this.Dictionary = DictionaryType.Drug;
                else
                    throw new ArgumentException(String.Format("Expected 'Drug/agent' but found '{0}'.", _dictionary));

            }
        }

        /// <summary>
        /// What dictionary is the definition intended for?
        ///     Drug/agent - Drug dictionary.
        /// </summary>
        [XmlIgnore()]
        public DictionaryType Dictionary { get; private set; }

        /// <summary>
        /// What dictionary is the definition intended for?
        ///     Drug/agent - Drug dictionary.
        /// </summary>
        [XmlIgnore()]
        public bool HasDefinition { get { return (Definition != null && (Enum.IsDefined(typeof(AudienceType), Definition.Audience))); } }
             
        
    }

}
