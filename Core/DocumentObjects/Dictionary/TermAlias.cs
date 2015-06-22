using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DocumentObjects.Dictionary
{
    // Class representing a single alias for a given dictionary term
    public class TermAlias
    {
        public String AlternateName { get; set; }
        public String NameType { get; set; }
        public String Language { get; set; }
    }
}
