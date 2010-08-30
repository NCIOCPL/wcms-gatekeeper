using System;
using System.Collections.Generic;
using System.Text;

using GateKeeper.Common;

namespace GKManagers
{
    class RequestMgrLogBuilder : LogBuilder
    {
        public RequestMgrLogBuilder()
            : base("RequestManager")
        {
        }

        public static RequestMgrLogBuilder Instance
        {
            get { return new RequestMgrLogBuilder(); }
        }
    }
}
