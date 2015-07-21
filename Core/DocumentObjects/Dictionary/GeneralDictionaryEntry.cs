using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DocumentObjects.Dictionary
{
    /// <summary>
    /// Data and metadata for storing a dictionary entry extracted from
    /// either a GlossaryTerm or Terminology document.  This class holds the
    /// data, the indivdual document extractors classes are responsible for
    /// populating it.
    /// </summary>
    public class GeneralDictionaryEntry
    {
        /// <summary>
        /// The document's CDRID
        /// </summary>
        public int TermID { get; set; }

        /// <summary>
        /// The term name in the object's language.
        /// </summary>
        public String TermName { get; set; }

        /// <summary>
        /// The particular dictionary the row belongs to (glossary, genetics, drugs)
        /// </summary>
        public DictionaryType Dictionary { get; set; }

        /// <summary>
        /// The item's language
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// The particular audience a media link is intended for. (Patient, HealthProfessionals)
        /// </summary>
        public AudienceType Audience { get; set; }

        /// <summary>
        /// The rendered JSON corresponding to the object.
        /// (Why isn't this named JSON? Because We're rendering JSON today, but five years from now might
        /// be using something else.  So the name is generic to reflect the use versus a specific
        /// data format.)
        /// </summary>
        public String Object { get; set; }
    }
}
