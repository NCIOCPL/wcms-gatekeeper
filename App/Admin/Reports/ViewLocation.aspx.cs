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
using System.Collections.Generic;
using GateKeeper.DocumentObjects;

namespace GateKeeperAdmin.Reports
{
    public partial class ViewLocation : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetActiveNavTab((int)AdminNavTabs.AdminNavTabType.AdminTab);
            if (!IsPostBack)
            {
                dropDocType.Items.Add(new ListItem("", "-1"));
                dropDocType.Items.Add(new ListItem("All", "0"));
                CommonUI.PopulateDropDownFromEnum(dropDocType, typeof(DocumentType), false);
            }
        }

        private void GetLocations(int DocType, int CdrID)
        {
            List<RequestLocationInternalIds> arrLocation = new List<RequestLocationInternalIds>();
            try
            {
                if(DocType != -1)
                    arrLocation = RequestManager.LoadRequestLocationInternalIds(DocType, CdrID);
                rptLocation.DataSource = arrLocation;
                rptLocation.DataBind();
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(ViewLocation), "Failed in GetLocations(int, int)", ex);
            }
        }

        protected void dropDocType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int docType = Strings.ToInt(dropDocType.SelectedValue);
            //Convert DocType suitable for st proc excepting All and Empty DropDown List
            if(docType > 0)
                docType = (int)DocumentTypeConverter.GKToCdr((DocumentType)docType);
            GetLocations(docType, 0);
            txtCdrId.Text = String.Empty;
        }

        protected void ibtnGo_Click(object sender, ImageClickEventArgs e)
        {
            int CdrId = Strings.ToInt(txtCdrId.Text);
            if (!String.IsNullOrEmpty(txtCdrId.Text) && CdrId != -1)
            {
                GetLocations(0, CdrId);
                dropDocType.SelectedValue = "0";
            }
        }

     }
}
