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
                case 3:
                    MesTextLbl.Text = "You should be logged in as Administrator to perform this task or your login session has timed out.";
                    break;
                case 4:
                    MesTextLbl.Text = "Document reprocessing cannot be scheduled, because copyRequest did not return DataReceived status.";
                    break;
                case 5:
                    MesTextLbl.Text = "Document reprocessing cannot schedule the batch.";
                    break;
                default:
                    {
                        MesTextLbl.Text = "Unexpected errors occurred. Our technicians have been notified and are working to correct the situation.";
                    }
                    break;
            }

        }
    }
}
