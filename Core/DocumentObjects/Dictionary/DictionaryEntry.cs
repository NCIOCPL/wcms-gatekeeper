using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DocumentObjects.Dictionary
{
    // Data and metadata for storing a dictionary entry extracted from
    // either a GlossaryTerm or Terminology document.  This class holds the
    // data, the indivdual document extractors classes are responsible for
    // populating it.
    public class DictionaryEntry
    {
        int TermID { get; set; }
        String TermName { get; set; }
        String DictionaryName { get; set; }
        String Language { get; set; }
        String Audience { get; set; }
        String Object { get; set; }
    }
}
