using System;
using System.Collections.Generic;
using System.Text;

using GateKeeper.Common;

namespace GateKeeper.ProcessManager
{
    class ProcessMgrLogBuilder : LogBuilder
    {
        public ProcessMgrLogBuilder()
            : base("ProcessManagerService")
        {
        }

        public static ProcessMgrLogBuilder Instance
        {
            get { return new ProcessMgrLogBuilder(); }
        }
    }
}
