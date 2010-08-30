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
    public partial class BatchHistory : System.Web.UI.Page
    {
        private int _batchId = -1;
        private int _requestId = -1;


        protected void Page_Load(object sender, EventArgs e)
        {
            _requestId = Strings.ToInt(Request.QueryString["reqid"]);
            if (!Page.IsPostBack)
            {
                _batchId = Strings.ToInt(Request.QueryString["batchid"]);
                if (_batchId < 0)
                    return;
                RequestIdLbl.Text = _batchId.ToString();
                GetBatchData();
            }
            else
            {
                _batchId = Strings.ToInt(Request.QueryString["batchid"]);
            }

        }

        /// <summary>
        /// Extract data from database and bind them
        /// </summary>
        /// <returns></returns>
        private void GetBatchData()
        {
            if (_batchId < 0)
                return;
            DataSet ds = null;
            try
            {
                ds = RequestManager.LoadBatchHistoryDS(_batchId);
                rptBatchAction.DataSource = ds.Tables[0];
                rptBatchHistory.DataSource = ds.Tables[1];
                rptBatchHistory.DataBind();
                rptBatchAction.DataBind();
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(BatchHistory), "Failed in GetBatchData()", ex);
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
            }
        }

        /// <summary>
        /// Handle Data Bound event and substitute an empty string with "-"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public void rptBatchAction_ItemDataBound(Object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label label = (Label)e.Item.FindControl("lblComplDateItem");
                if(label != null &&  label.Text == String.Empty)
                {
                    label.Text = "-";
                    return;
                }
                label = (Label)e.Item.FindControl("lblComplDateItemAlt");
                if(label != null &&  label.Text == String.Empty)
                {
                    label.Text = "-";
                }
            }
        }

        #region Properties
        public int RequestId
        {
            get { return _requestId; }
            set { _requestId = value; }
        }
        #endregion
    }
}
