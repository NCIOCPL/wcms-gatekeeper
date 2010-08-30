using System;
using System.Collections.Generic;
using System.Text;

using GateKeeper.Common;

namespace GateKeeper
{
    public class WebServiceLogBuilder : LogBuilder
    {
        public WebServiceLogBuilder()
            : base("WebService")
        {
        }

        public static WebServiceLogBuilder Instance
        {
            get { return new WebServiceLogBuilder(); }
        }
    }
}
