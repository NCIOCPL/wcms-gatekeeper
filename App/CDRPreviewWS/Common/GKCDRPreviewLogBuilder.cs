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

namespace CDRPreviewWS.Common
{
    public class GKCDRPreviewLogBuilder : LogBuilder
    {
        public GKCDRPreviewLogBuilder()
            : base("GateKeeperCDRPreview")
        {
        }

        public static GKCDRPreviewLogBuilder Instance
        {
            get { return new GKCDRPreviewLogBuilder(); }
        }
    }
}
