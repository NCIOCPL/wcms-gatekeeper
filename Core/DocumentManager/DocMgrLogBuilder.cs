using System;
using System.Collections.Generic;
using System.Text;

using GateKeeper.Common;

namespace GKManagers
{
    class DocMgrLogBuilder : LogBuilder
    {
        public DocMgrLogBuilder()
            : base("DocumentManager")
        {
        }

        public static DocMgrLogBuilder Instance
        {
            get { return new DocMgrLogBuilder(); }
        }
    }
}
