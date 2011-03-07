using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using GateKeeper.Common;

namespace GateKeeperAdmin.Common
{
    public class GKAdminLogBuilder : LogBuilder
    {
        public GKAdminLogBuilder(): base("GatekeeperAdmin")
        {
        }

        public static GKAdminLogBuilder Instance
        {
            get { return new GKAdminLogBuilder(); }
        }
    }
}
