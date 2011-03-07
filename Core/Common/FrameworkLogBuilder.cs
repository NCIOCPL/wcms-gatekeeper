using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.Common
{
    /// <summary>
    /// A log builder for use in code shared between multiple Manager objects.
    /// </summary>
    public class FrameworkLogBuilder : LogBuilder
    {
        public FrameworkLogBuilder()
            : base("Framework")
        {
        }

        public static FrameworkLogBuilder Instance
        {
            get { return new FrameworkLogBuilder(); }
        }
    }
}
