using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Protocol
{
    /// <summary>
    /// Represents the type of protocol.
    /// </summary>
    [Serializable]
    public enum ProtocolType
    {
        /// <summary>
        /// Protocol registered with ClinicalTrials.gov
        /// </summary>
        CTGov = 1,

        /// <summary>
        /// Internal protocol document.
        /// </summary>
        Protocol = 2,
    }
}
