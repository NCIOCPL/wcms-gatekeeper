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
using GKManagers;
using GateKeeperAdmin.Common;
using GKManagers.BusinessObjects;

namespace GateKeeperAdmin.RequestHistory
{
    public partial class RequestDataHistory : System.Web.UI.Page
    {
        private int _reqDataID = -1;
        private int _reqID = -1;
        protected void Page_Load(object sender, EventArgs e)
        {
            _reqDataID = Strings.ToInt(Request.QueryString["reqdataid"]);
            _reqID = Strings.ToInt(Request.QueryString["reqid"]);
            if (!Page.IsPostBack)
            {               
                if (_reqDataID < 0)
                {
                    idDataTabled.Visible = false;
                    return;
                }
                RequestIdLbl.Text = _reqDataID.ToString();
                CommonUI.PopulateDropDownFromEnum(dropHistoryType, typeof(RequestDataHistoryType), true);
                dropHistoryType.Items.Remove(dropHistoryType.Items.FindByText("Debug"));
                GetHistoryData(false);
            }
        }

        /// <summary>
        /// Extract data from database and bind them showing or not showing the header
        /// </summary>
        /// <returns></returns>
        private void GetHistoryData(bool bShowAlwaysHeader)
        {
            if (_reqDataID < 0)
                return;
            DataSet ds = null;
            try
            {
                ds = RequestManager.LoadDataHistoryDS(_reqDataID, dropHistoryType.SelectedItem.Text.ToLower(), chkDebug.Checked);
                if (ds.Tables[0].Rows.Count < 1 && !bShowAlwaysHeader)
                {
                    idDataTabled.Visible = false;
                    return;
                }
                rptDataHistory.DataSource = ds;
                rptDataHistory.DataBind();
            }         
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(RequestDataHistory), "Failed in GetHistoryData()", ex); 
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
            }
        }

        /// <summary>
        /// Handle Index Changed event when the user select the type of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected void dropHistoryType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((DropDownList)sender).SelectedItem.Text.ToLower() != "error" && ((DropDownList)sender).SelectedItem.Text != String.Empty)
            {
                chkDebug.Enabled = false;
                chkDebug.Checked = false;
            }
            else
                chkDebug.Enabled = true;

            GetHistoryData(true);
        }

        protected void chkDebug_CheckedChanged(object sender, EventArgs e)
        {
            GetHistoryData(true);
        }

        #region Properties
        protected int ReqID
        {
            get { return _reqID; }
            set { _reqID = value; }
        }

        protected int ReqDataID
        {
            get { return _reqDataID; }
            set { _reqDataID = value; }
        }
        #endregion
    }
}
