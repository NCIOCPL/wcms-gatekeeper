using System;
using System.Collections.Generic;
using System.Text;

using GateKeeper.Common;

namespace GKManagers
{
    class BatchLogBuilder : LogBuilder
    {
        public BatchLogBuilder()
            : base("BatchManager")
        {
        }

        public static BatchLogBuilder Instance
        {
            get { return new BatchLogBuilder(); }
        }
    }
}
