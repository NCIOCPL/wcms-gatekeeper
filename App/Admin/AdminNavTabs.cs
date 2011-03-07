using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace GateKeeperAdmin
{
    public class AdminNavTabs
    {
        public enum AdminNavTabType
        {
            HomeTab = 1,
            ReqHistTab = 2,
            ActivitiesTab = 3,
            ReportsTab = 4,
            AdminTab = 5
        }
    }
}
