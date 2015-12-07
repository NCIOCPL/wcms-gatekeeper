using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Summary section device type.
    /// This enumeration decides on which device a particular section will be visible
    /// </summary>
    public enum SummarySectionDeviceType
    {
        /// <summary>
        /// Summary section visible on Desktop only
        /// </summary>
        screen,

        /// <summary>
        /// Summary section visible on Mobile only
        /// </summary>
        mobile,

        /// <summary>
        /// Summary section visible on Syndication only
        /// </summary>
        syndication,

        /// <summary>
        /// Summary section visible on All devices
        /// </summary>
        all,

        /// <summary>
        /// Summary section hidden on All devices
        /// </summary>
        none
    }
}
