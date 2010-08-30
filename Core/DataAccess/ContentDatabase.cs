using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DataAccess
{
    /// <summary>
    /// Enumerations to represent the 4 gatekeeper/CancerGov environments.
    /// </summary>
    public enum ContentDatabase
    {
        /// <summary>
        /// GateKeeper database (documents are received here).
        /// </summary>
        GateKeeper,

        /// <summary>
        /// Documents are extracted and rendered in this environment.
        /// </summary>
        Staging,

        /// <summary>
        /// Processed CDR document QC environment.
        /// </summary>
        Preview,

        /// <summary>
        /// Live production environment.
        /// </summary>
        Live,

        /// <summary>
        /// CancerGov QC environment.
        /// </summary>
        CancerGovStaging,

        /// <summary>
        /// CancerGov live production environment.
        /// </summary>
        CancerGov,
    }
}
