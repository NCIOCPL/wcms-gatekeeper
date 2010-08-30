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
using GKManagers;
using GKManagers.BusinessObjects;
using GateKeeper.DocumentObjects;
using GateKeeper.Common;
using GateKeeperAdmin.Common;
using System.Collections.Generic;


namespace GateKeeperAdmin.RequestHistory
{
    public partial class Popup : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int nReqID = Strings.ToInt(Request.QueryString["reqid"]);
            if (nReqID < 0)
                return;
            try
            {
                rptTypeCount.DataSource = RequestManager.LoadRequestCounts(nReqID);
                rptTypeCount.DataBind();
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(Popup), "Failed in Page_Load()", ex);
            }
        }
    }


}
