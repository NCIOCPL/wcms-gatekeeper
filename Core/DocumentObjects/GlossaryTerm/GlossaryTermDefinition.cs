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
    public class EnglishTermDefinition : GlossaryTermDefinition { }

    /// <summary>
    /// Spanish-specific subclass GlossaryTermDefinition.
    /// This class is only used for serialization.
    /// </summary>
    [XmlRoot("SpanishTermDefinition")]
    public class SpanishTermDefinition : GlossaryTermDefinition { }

    /// <summary>
    /// Business object to represent the metadata for a definition extracted from a GlossaryTerm document.
    /// This class is used for both the English and Spanish terms as the structures are identical
    /// between the languages.
    /// </summary>
    abstract public class GlossaryTermDefinition
    {
        /// <summary>
        /// Which specific dictionary is this definition intended for?
        ///     Cancer.gov - Dictionary of Cancer Terms
        ///     Genetics - Dictionary of Genetics Terms
        /// </summary>
        public String Dictionary { get; set; }

        /// <summary>
        /// What audience is the definition for?
        ///     Patient - Patients and caregivers
        ///     Health professional - Doctors and other health professionals.
        /// </summary>
        public String Audience { get; set; }
    }
}
