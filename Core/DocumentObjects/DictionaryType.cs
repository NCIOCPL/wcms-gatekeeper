using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DocumentObjects
{
    /// <summary>
    /// Represents a dictionary entry's target dictionary.
    /// </summary>
    [Serializable]
    public enum DictionaryType
    {
        /// <summary>
        /// Dictionary is set, but not to a value we recognize.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The source document didn't specify a dictionary.
        /// Distinct from "A dictioary was assigned, but we don't know what it is."
        /// </summary>
        NotSet = 1,

        /// <summary>
        /// Dictionary of Cancer Terms.
        /// </summary>
        Term = 2,

        /// <summary>
        /// Genetics Dictionary
        /// </summary>
        Genetic = 3,

        /// <summary>
        /// Drug dictionary.
        /// </summary>
        Drug = 4
    }
}
