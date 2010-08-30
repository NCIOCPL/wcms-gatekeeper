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

namespace GateKeeperAdmin.Administrator
{
    public partial class EventLogViewer : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetActiveNavTab((int)AdminNavTabs.AdminNavTabType.AdminTab);
            if (!IsPostBack)
            {
                string strLogNames = ConfigurationManager.AppSettings["EventLogNames"];
                strLogNames = "Select a name of log," + strLogNames;
                string[] arrLogNames = strLogNames.Split(new char[] {','});
                for(int i = 1; i < arrLogNames.Length; i++)
                {
                    arrLogNames[i] = arrLogNames[i].Trim();
                }
                dropLogNames.DataSource = arrLogNames;
                dropLogNames.DataBind();
                elvViewer.Visible = false;
            }
        }

        protected void dropLogNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((DropDownList)sender).SelectedItem.Text.Trim() != String.Empty && ((DropDownList)sender).SelectedIndex != 0)
            {
                elvViewer.Log = ((DropDownList)sender).SelectedItem.Text;
                elvViewer.Visible = true;
            }
            else
                elvViewer.Visible = false;
        }
    }
}
