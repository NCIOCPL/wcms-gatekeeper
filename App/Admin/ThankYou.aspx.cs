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
    public partial class ThankYou : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            int msg = Strings.ToInt(Request.QueryString["msg"]);
            switch(msg){
                case 1:
                    MesTextLbl.Text = "<b>Your batch has been sucessfully scheduled.</b>";
                    break;
                default:
                    break;
            }
        }
    }
}
