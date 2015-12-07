using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GateKeeper.DocumentObjects.Terminology
{
    /// <summary>
    /// English-specific subclass GlossaryTermDefinition.
    /// This class is only used for serialization.
    /// </summary>
    [XmlRoot("Definition")]
    public class TerminologyDefinition
    {

        /// <summary>
        /// Infrastructure.  This property is not intended to be used outside serialization.
        /// </summary>
        /// <remarks>
        /// The Audience element in a TermDefinition or SpanishTermDefinition structure is constrained by the
        /// CDR to be either "Patient" or "Health Professional."  The latter value contains a space, which
        /// prevents it from being deserialized into an enumerated value. We thus end up with the workaround of
        /// using a String property as a wrapper for the "real" Audience property.
        /// </remarks>
        [XmlElement(ElementName = "DefinitionType")]
        public String AudienceKludge
        {
            get { return this.Audience.ToString(); }
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
