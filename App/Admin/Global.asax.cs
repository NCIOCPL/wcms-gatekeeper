using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using GateKeeperAdmin.Common;

namespace GateKeeperAdmin
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception objErr = Server.GetLastError();

            if (objErr != null)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(Global), "Global.Application_Error: ", objErr);
            }
            Response.Redirect("~/Error.aspx");
        }
    }
}