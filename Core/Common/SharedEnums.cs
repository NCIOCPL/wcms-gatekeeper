using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.Common
{
    /// <summary>
    /// Used to denote different targeted device platforms.
    /// The same enum names are mirrored between the CDR, Extract
    /// and rendering code.
    /// </summary>
    [Flags]
    public enum TargetedDevice
    {
        screen = 0, // Desktop web browser.
        mobile = 1, // Mobile device
        ebook = 4
    }
}
