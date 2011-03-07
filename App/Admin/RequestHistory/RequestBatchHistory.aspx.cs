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
    public partial class RequestBatchHistory : System.Web.UI.Page
    {
        private int _reqID = -1;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                _reqID = Strings.ToInt(Request.QueryString["reqid"]);
                if (_reqID < 0)
                {
                    //idDataTabled.Visible = false;
                    return;
                }
                RequestIdLbl.Text = _reqID.ToString();
                GetReqBatchHistoryData();
            }
            else
            {
                _reqID = Strings.ToInt(Request.QueryString["reqid"]);
            }

        }

        /// <summary>
        /// Extract dat from database and bind them
        /// </summary>
        /// <returns></returns>
        private void GetReqBatchHistoryData()
        {
            try
            {
                rptRequestBatchHistory.DataSource = RequestManager.LoadReqBatchHistoryData(_reqID);
                rptRequestBatchHistory.DataBind();
            }
            catch (Exception ex)
            {

                GKAdminLogBuilder.Instance.CreateError(typeof(RequestBatchHistory), "Failed in GetReqBatchHistoryData()", ex); 
            }
        }

        #region Properties
        public int ReqID
        {
            get { return _reqID; }
            set { _reqID = value; }
        }
        #endregion
    }
}
