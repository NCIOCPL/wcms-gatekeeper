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

namespace GateKeeperAdmin
{
    public partial class BasePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void SetActiveNavTab(int tabID)
        {
            AdminToolMasterPage masterPage = (AdminToolMasterPage)this.Master;
            masterPage.SetActiveNavigationTab(tabID);
        }


        public string GetUserName()
        {
            string name = null;
            try{
                MembershipUser user = Membership.GetUser();
                name = user.UserName;
            }catch(Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(BasePage), "Failed in GetUserName()", ex); 
            }
            return name;
        }

        public bool IsUserAdmin()
        {
            bool isAdmin = false;
            try
            {
                string adminGroup = ConfigurationManager.AppSettings["AdminUserGroup"];
                isAdmin = User.IsInRole(adminGroup);
            }
            catch(Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(BasePage), "Failed in IsUserAdmin()", ex); 

            }
            return isAdmin;
        }

        public bool IsUserOperator()
        {
            bool isOperator = false;
            try
            {
                string operatorGroup = ConfigurationManager.AppSettings["OperatorUserGroup"];
                isOperator = User.IsInRole(operatorGroup);
            }
            catch(Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(BasePage), "Failed in IsUserOperator()", ex); 

            }
            return isOperator;
        }
    }
}
