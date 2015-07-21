using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// English-specific subclass GlossaryTermDefinition.
    /// This class is only used for serialization.
    /// </summary>
    [XmlRoot("TermDefinition")]
    public class EnglishTermDefinition : GlossaryTermDefinition
    {
        /// <summary>
        /// Override of GlossaryTermDefinition.Language to report English.
        /// </summary>
        public override Language Language { get { return Language.English; } }
    }

    /// <summary>
    /// Spanish-specific subclass GlossaryTermDefinition.
    /// This class is only used for serialization.
    /// </summary>
    [XmlRoot("SpanishTermDefinition")]
    public class SpanishTermDefinition : GlossaryTermDefinition
    {
        /// <summary>
        /// Override of GlossaryTermDefinition.Language to report Spanish.
        /// </summary>
        public override Language Language { get { return Language.Spanish; } }
    }

    /// <summary>
    /// Business object to represent the metadata for a definition extracted from a GlossaryTerm document.
    /// This class is used for both the English and Spanish terms as the structures are identical
    /// between the languages.
    /// </summary>
    abstract public class GlossaryTermDefinition
    {
        /// <summary>
        /// What language is this definition in?
        ///     Language.English
        ///     Language.Spanish
        /// This member must be overriden in the subclass with a read-only property returning
        /// the relevant language.
        /// </summary>
        public abstract Language Language { get; }

        /// <summary>
        /// Which specific dictionary is this definition intended for?
        ///     Cancer.gov - Dictionary of Cancer Terms
        ///     Genetics - Dictionary of Genetics Terms
        /// </summary>
        /// <remarks>
        /// The Dictionary element in a TermDefinition or SpanishTermDefinition structure is constrained by the
        /// CDR to be either "Cancer.Gov" (Dictionary of Cancer Terms) or "Genetics."  The period in "Cancer.gov"
        /// prevents direct serialization, so we use a workaround in the setter.
        /// </remarks>
        [XmlElement(ElementName = "Dictionary")]
        public String DictionaryKludge
        {
            get { return _dictionary; }
            set
            {
                _dictionary = value;
                if (_dictionary.Equals("cancer.gov", StringComparison.InvariantCultureIgnoreCase))
                    this.Dictionary = DictionaryType.Term;
                else if (_dictionary.Equals("genetics", StringComparison.InvariantCultureIgnoreCase))
                    this.Dictionary = DictionaryType.Genetic;
                else
                    throw new ArgumentException(String.Format("Expected 'Cancer.gov' or 'Genetics' but found '{0}'.", value));
            }
        }
        // Storage for the DictionaryKludge property.
        private String _dictionary;


        /// <summary>
        /// What dictionary is the definition intended for?
        ///     Cancer.gov - Dictionary of Cancer Terms.
        ///     Genetics - Genetics dictionary.
        /// </summary>
        /// <remarks>
        /// This property is set in the deserialization of the DictionaryKludge property.
        /// See that property's remarks for additional detail.
        /// </remarks>
        [XmlIgnore()]
        public DictionaryType Dictionary { get; private set; }


        /// <summary>
        /// Infrastructure.  This property is not intended to be used outside serialization.
        /// </summary>
        /// <remarks>
        /// The Audience element in a TermDefinition or SpanishTermDefinition structure is constrained by the
        /// CDR to be either "Patient" or "Health Professional."  The latter value contains a space, which
        /// prevents it from being deserialized into an enumerated value. We thus end up with the workaround of
        /// using a String property as a wrapper for the "real" Audience property.
        /// </remarks>
        [XmlElement(ElementName = "Audience")]
        public String AudienceKludge
        {
            get {return this.Audience.ToString();}
            set
            {
                if (value.Equals("patient", StringComparison.InvariantCultureIgnoreCase))
                    this.Audience = AudienceType.Patient;
                else if (value.Equals("health professional", StringComparison.InvariantCultureIgnoreCase))
                    this.Audience = AudienceType.HealthProfessional;
                else
                    throw new ArgumentException(String.Format("Expected 'patient' or 'health professional' but found '{0}'.", value));
            }
        }

        /// <summary>
        /// What audience is the definition for?
        ///     Patient - Patients and caregivers
        ///     Health professional - Doctors and other health professionals.
        /// </summary>
        /// <remarks>
        /// This property is set in the deserialization of the AudienceKludge property.
        /// See that property's remarks for additional detail.
        /// </remarks>
        [XmlIgnore()]
        public AudienceType Audience { get; set; }
    }
}
