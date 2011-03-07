using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using GateKeeper.Common;

namespace GateKeeperAdmin
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int msg = Strings.ToInt(Request.QueryString["msg"]);
            switch (msg)
            {
                case 1:
                    MesTextLbl.Text = "You must select Push To Preview step when selecting Push To Staging.";
                    break;
                case 2:
                    MesTextLbl.Text = "Can not delete a batch because it's being Processed";
                    break;
                default:
                    MesTextLbl.Text = "Unexpected errors occurred. Our technicians have been notified and are working to correct the situation.";
                    break;
            }

        }
    }
}
