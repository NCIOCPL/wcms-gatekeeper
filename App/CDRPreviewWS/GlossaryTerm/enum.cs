using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCI.Web.Dictionary
{
    /// <summary>
    /// Enumeration of the known dictionary types.
    /// </summary>
    public enum DictionaryType
    {
        // We don't know what dictionary this is.  Error condition.
        Unknown = 0,

        // Dictionary of Cancer Terms
        term = 1,

        // Drug Dictionary
        drug = 2,

        // Dictionary of Genetics Terms
        genetic = 3
    }

    /// <summary>
    /// Allowed search types
    /// </summary>
    public enum SearchType
    {
        Unknown = 0,

        /// <summary>
        /// Search for terms beginning with
        /// </summary>
        Begins = 1,

        /// <summary>
        /// Search for terms containing
        /// </summary>
        Contains = 2,

        /// <summary>
        /// Search for terms beginning with, followed by terms containing
        /// </summary>
        Magic = 3
    }

    public enum AudienceType
    {
        Unknown = 0,
        Patient = 1,
        HealthProfessional = 2
    }
}
