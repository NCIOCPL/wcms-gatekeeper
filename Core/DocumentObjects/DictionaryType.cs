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
        /// Dictionary hasn't been properly assigned.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Dictionary of Cancer Terms.
        /// </summary>
        Term = 1,

        /// <summary>
        /// Genetics Dictionary
        /// </summary>
        Genetic = 2,

        /// <summary>
        /// Drug dictionary.
        /// </summary>
        Drug = 3
    }
}
