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
using GateKeeperAdmin.Common;

namespace GateKeeperAdmin.WebControls
{
    public partial class LoginControl : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                MembershipUser user = Membership.GetUser();

                if (user != null)
                {
                    LoginDiv.Visible = false;
                    LoggedinDiv.Visible = true;
                }
                else
                {
                    LoginDiv.Visible = true;
                    LoggedinDiv.Visible = false;
                }
            }
            catch(Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(LoginControl), "Failed for Membership.GetUser in Page_Load()", ex); 
            }
        }                
    }
}