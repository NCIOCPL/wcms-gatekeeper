using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDRPreviewWS.GlossaryTerm
{
    /// <summary>
    /// Enumeration of the known dictionary types.
    /// </summary>
    public enum DictionaryType
    {
        // We don't know what dictionary this is.  Error condition.
        Unknown = 0,

        // This term should show up, it just does not belong to any specific dictionary
        NotSet = 1, 

        // Dictionary of Cancer Terms
        term = 2,

        // Drug Dictionary
        drug = 3,

        // Dictionary of Genetics Terms
        genetic = 4
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
