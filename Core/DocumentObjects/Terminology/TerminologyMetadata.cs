using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GateKeeper.DocumentObjects.Dictionary;
using System.Collections.ObjectModel;

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
        public int ID { get; set; }

        /// <summary>
        /// The term name, as extracted from the original Terminology document.
        /// </summary>
        [XmlElement(ElementName = "PreferredName")]
        public String TermName { get; set; }

        /// <summary>
        /// The definition, as extracted from the original Terminology document.
        /// </summary>
        [XmlElement(ElementName = "DefinitionText")]
        public String Definition { get; set; }

        /// <summary>
        ///  //For Terminology documents the language is always set to English
        /// </summary>
        [XmlIgnore()]
        public Language Language { get { return Language.English; } }


        /// <summary>
        /// Which specific dictionary is this definition intended for?
        ///     Drug/agent - Drug Dictionary
        /// </summary>
        /// <remarks>
        /// The SemanticType element in a Terminology document is constrained by the
        /// CDR to be "Drug/agent"  
        /// </remarks>
        [XmlElement(ElementName = "SemanticType")]
        public String DictionaryKludge
        {
            get { return _dictionary; }
            set
            {
                _dictionary = value;
                if (_dictionary.Equals("drug/agent", StringComparison.InvariantCultureIgnoreCase))
                    this.Dictionary = DictionaryType.Drug;
                else
                    throw new ArgumentException(String.Format("Expected 'Drug/agent' but found '{0}'.", value));
            }
        }
        // Storage for the DictionaryKludge property.
        private String _dictionary;


        /// <summary>
        /// What dictionary is the definition intended for?
        ///     Drug/agent - Drug dictionary.
        /// </summary>
        /// <remarks>
        /// This property is set in the deserialization of the DictionaryKludge property.
        /// See that property's remarks for additional detail.
        /// </remarks>
        [XmlIgnore()]
        public DictionaryType Dictionary { get; private set; }


       

        /// <summary>
        /// What audience is the definition for?
        ///     Patient - Patients and caregivers
        ///     Health professional - Doctors and other health professionals.
        /// </summary>
        [XmlIgnore()]
        public AudienceType Audience { get; set; }
        
    }

}
