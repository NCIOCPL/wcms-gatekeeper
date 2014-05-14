using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// A simplified GlossaryTerm document for handling the document post-processing.
    /// </summary>
    public class GlossaryTermSimple
    {
        List<GlossaryTermSimpleDefinition> _definitions = new List<GlossaryTermSimpleDefinition>();

        /// <summary>
        /// The term's CDRID
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// The english version of the term.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// The Spanish term name, if one exists.
        /// </summary>
        public String SpanishName { get; private set; }

        /// <summary>
        /// English pronunciation key.
        /// </summary>
        public String Pronunciation { get; private set; }

        /// <summary>
        /// Simple List of GlossaryTermSimpleDefinition objects associated with the term.
        /// </summary>
        public List<GlossaryTermSimpleDefinition> DefinitionList
        {
            get
            {
                return _definitions;
            }
        }

        /// <summary>
        /// Reports whether the term has a spanish component.
        /// </summary>
        public bool HasSpanishTerm
        {
            get { return !String.IsNullOrEmpty(SpanishName); }
        }

        /// <summary>
        /// True if the glossary term has a definition with health professionals
        /// as the target audience.
        /// </summary>
        public bool HasHealthProfessionalDefinition
        {
            get
            {
                bool hasHealthProf = false;

                // Search the definition list.  Technically, this search is O(n).  Storing the list of definitions
                // in a Dictionary would allow for O(1), but since the list will have no more than three entries, it's really
                // not worth the effort.
                foreach (GlossaryTermSimpleDefinition def in DefinitionList)
                {
                    if (def.Audience == AudienceType.HealthProfessional)
                    {
                        hasHealthProf = true;
                        break;
                    }
                }

                return hasHealthProf;
            }
        }

        /// <summary>
        /// True if the glossary term has a definition with patients
        /// as the target audience.
        /// </summary>
        public bool HasPatientDefinition
        {
            get
            {
                bool hasPatient = false;

                // Search the definition list.  Technically, this search is O(n).  Storing the list of definitions
                // in a Dictionary would allow for O(1), but since the list will have no more than three entries, it's really
                // not worth the effort.
                foreach (GlossaryTermSimpleDefinition def in DefinitionList)
                {
                    if (def.Audience == AudienceType.Patient)
                    {
                        hasPatient = true;
                        break;
                    }
                }

                return hasPatient;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The term's CDRID</param>
        /// <param name="term">The term's English name</param>
        /// <param name="spanishTerm">The term's Spanish name</param>
        /// <param name="pronunciation">Pronunciation key.</param>
        public GlossaryTermSimple(int id, String term, String spanishTerm, String pronunciation)
        {
            ID = id;
            Name = term;
            SpanishName = spanishTerm;
            Pronunciation = pronunciation;
        }
    }
}
