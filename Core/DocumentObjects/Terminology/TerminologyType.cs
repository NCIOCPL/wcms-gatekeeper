using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Terminology
{
    /// <summary>
    /// Terminology type.
    /// </summary>
    [Serializable]
    public enum TerminologyType
    {
        /// <summary>
        /// NCI Drug dictionary entry.
        /// </summary>
        Drug = 1,

        /// <summary>
        /// Menu hierarchy (used in protocol search).
        /// </summary>
        Menu = 2,

        /// <summary>
        /// Any other values that the Semantic Type may have e.g. Neoplasm diagnosis.
        /// </summary>
        Other = 3
    }
}
