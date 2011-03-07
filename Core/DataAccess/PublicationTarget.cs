using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DataAccess
{
    /// <summary>
    /// Indicates which publication target has been specified for an operation.
    /// </summary>
    public enum PublicationTarget
    {
        /// <summary>
        /// GateKeeper database.
        /// </summary>
        GateKeeper = 1,

        /// <summary>
        /// CDRStaging database.
        /// </summary>
        Staging = 2,

        /// <summary>
        /// CDRPreview (CancerGov preview) database.
        /// </summary>
        Preview = 3,

        /// <summary>
        /// CDRLive (CancerGov live/production) database.
        /// </summary>
        Live = 4,
    }
}
