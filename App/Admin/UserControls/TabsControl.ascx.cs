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

namespace GateKeeperAdmin.UserControls
{
    public partial class TabsControl : System.Web.UI.UserControl
    {
        public int ActiveTabIndex
        {
            set { SetActiveTab(value); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void SetActiveTab(int tabId)
        {
            ClearTabs();
            switch (tabId)
            {
                case 1:
                    HomeTab.Attributes.Add("class", "on");
                    break;
                case 2:
                    ReqHistTab.Attributes.Add("class", "on");
                    break;
                case 3:
                    ActivitiesTab.Attributes.Add("class", "on");
                    break;
                case 4:
                    ReportsTab.Attributes.Add("class", "on");
                    break;
                case 5:
                    AdminTab.Attributes.Add("class", "on");
                    break;
                default :
                    break;
            }

        }

        public void ClearTabs()
        {
            HomeTab.Attributes.Remove("class");
            ReqHistTab.Attributes.Remove("class");
            ActivitiesTab.Attributes.Remove("class");
            ReportsTab.Attributes.Remove("class");
            AdminTab.Attributes.Remove("class");
        }
    }
}