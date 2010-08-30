using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using GKManagers;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using GateKeeperAdmin.Common;
using GateKeeper.Common;


namespace GateKeeperAdmin.Reports
{
    public partial class ViewReports : BasePage
    {
        //This is a list of reports as an array
        private string[] arrReports = { "Check Summary Load", "Check Protocol Load", "Check Terminology Load", "Check Glossary Load", 
            "Check PoliticalSubUnit Load", "Check Organization Load", "Check Genetics Professioal Load", "Check Document Table and MetaData Tables"};

        protected new void Page_Load(object sender, EventArgs e)
        {
            SetActiveNavTab((int)AdminNavTabs.AdminNavTabType.ReportsTab);
            if (!IsPostBack)
            {
                pnlTitle.Visible = false;
                dropViewReports.DataSource = arrReports;
                dropViewReports.DataBind();
            }

        }

        /// <summary>
        /// Handle Click evnt to Extract a chosen report
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected void ibtnGo_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                int nSel = dropViewReports.SelectedIndex + 1;
                GetReports(nSel);
                //Make title visible
                pnlTitle.Visible = true;
                lblTitle.Text = dropViewReports.Text;
                //Erase "Check " from the title of the report
                if (lblTitle.Text.ToLower().StartsWith("check "))
                    lblTitle.Text = lblTitle.Text.Remove(0, 6);
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(ViewReports), "Failed in ibtnGo_Click()", ex);
            }
        }

        /// <summary>
        /// Extract a report from database by number
        /// </summary>
        /// <param name="nNumber">The number of a report</param>
        /// <returns></returns>
        private void GetReports(int nNumber)
        {
            DataSet ds = null;
            try
            {
                ds = RequestManager.GetReports(nNumber);
                rptViewReports.DataSource = ds;
                rptViewReports.DataBind();
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(ViewReports), "Failed in GetReports()", ex);
            }
            finally
            {
                if(ds != null)
                    ds.Dispose();
            }
        }

        /// <summary>
        /// Handle Data Bound event to eliminate dbo before Table name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected void rptViewReports_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label label = (Label)e.Item.FindControl("lblTableName");
                if (label != null && Strings.Clean(label.Text) != null && label.Text.ToLower().IndexOf("dbo.") > -1)
                {
                    label.Text = label.Text.Remove(label.Text.ToLower().IndexOf("dbo."), 4);
                }
            }
        }
    }
}
