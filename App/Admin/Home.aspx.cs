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

namespace GateKeeperAdmin.www
{
    public partial class Home : BasePage 
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ShowHideAdminBox();
        }

        private void ShowHideAdminBox()
        {
            try
            {

                string adminGroup = ConfigurationManager.AppSettings["AdminUserGroup"];

                if (IsUserAdmin())
                {
                    AdminBoxDiv.Visible = true;
                }
                else
                {
                    AdminBoxDiv.Visible = false;
                }
            }
            catch(Exception ex)
            {                
                AdminBoxDiv.Visible = false;
                GKAdminLogBuilder.Instance.CreateError(typeof(Home), "Failed in ShowHideAdminBox()", ex); 
            }
        }
    }
}
