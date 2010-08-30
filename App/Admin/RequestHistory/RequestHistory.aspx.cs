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

namespace GateKeeperAdmin.RequestHistory
{
    public partial class RequestHistory : BasePage
    {
        private RequestHistoryFilter filter = new RequestHistoryFilter();        

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                filter.SortColumn = ReqHistorySortColumnType.RequestIdColumn;
                filter.SortOrder = SortOrderType.Descending;
                SetActiveNavTab((int)AdminNavTabs.AdminNavTabType.ReqHistTab);
                PopulateFilterDropDowns();                
                SetLinkButtons();
                PopulateMonthDropDown();
                GetRequestData();
            }

        }

        private void SetFilter()
        {
            try
            {
                int destType = Strings.ToInt(PubDestinationDropDown.SelectedValue);
                int status = Strings.ToInt(RequestStatusDropDown.SelectedValue);
                int month = Strings.ToInt(NumMonthDropDown.SelectedValue);
                int cdrid = Strings.ToInt(CdrIdTextBox.Text);

                filter.RequestStatus = (RequestStatusType)status;
                filter.PublishingDestination = (RequestDataLocationType)destType;
                filter.NumberOfMonth = month;
                filter.CdrID = cdrid;

            }
            catch(Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(RequestHistory), "SetFilter", ex); 
            }
        }

        private void GetRequestData()
        {
                SetFilter();
                DataSet results = null;
                try
                {
                    results = RequestManager.LoadRequests(filter, 1, 0);
                    ReqRepeater.DataSource = results;
                    ReqRepeater.DataBind();
                }
                catch(Exception ex)
                {
                    GKAdminLogBuilder.Instance.CreateError(typeof(RequestHistory), "GetRequestData", "Failed in GetRequestData() on LoadRequests", ex); 
                }
                finally
                {
                    if (results != null)
                        results.Dispose();
                }

        }

        #region setup controls
        private void PopulateFilterDropDowns()
        {
            CommonUI.PopulateDropDownFromEnum(PubDestinationDropDown, typeof(RequestDataLocationType), true);
            CommonUI.PopulateDropDownFromEnum(RequestStatusDropDown, typeof(RequestStatusType), true);
        }

        private void PopulateMonthDropDown()
        {
            for (int i=1;  i<13; i++ ){
                NumMonthDropDown.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }
            NumMonthDropDown.Items.Add(new ListItem("24", "24"));
            NumMonthDropDown.Items.Add(new ListItem("all", "0"));
        }

        private void ClearFilterDropDowns()
        {
            RequestStatusDropDown.ClearSelection();
            PubDestinationDropDown.ClearSelection();
            CdrIdTextBox.Text = "";
        }

        private void SetLinkButtons()
        {
            ReqDateLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            ReqDateLinkBtn.CommandName = ((int)ReqHistorySortColumnType.RequestDateColumn).ToString();

            ReqIdLinkBtn.CommandArgument = ((int)SortOrderType.Descending).ToString();
            ReqIdLinkBtn.CommandName = ((int)ReqHistorySortColumnType.RequestIdColumn).ToString();

            ExternalReqIdLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            ExternalReqIdLinkBtn.CommandName = ((int)ReqHistorySortColumnType.ExternalRequestIdColumn).ToString();

            ReqStatusLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            ReqStatusLinkBtn.CommandName = ((int)ReqHistorySortColumnType.RequestStatusColumn).ToString();

            PubDestinationLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            PubDestinationLinkBtn.CommandName = ((int)ReqHistorySortColumnType.PublishingDestinationColumn).ToString();

        }

        #endregion 

        #region Events

        protected void DropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetRequestData();
        }

        protected void ClearFiltersButton_Click(object sender, ImageClickEventArgs e)
        {
            ClearFilterDropDowns();
            GetRequestData();
        }

        protected void LinkBtn_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton lnkBtn = (LinkButton)sender;
                //change order 
                SortOrderType newSortOrder = ChangeSortOrder((SortOrderType)Strings.ToInt(lnkBtn.CommandArgument));
                lnkBtn.CommandArgument = ((int)newSortOrder).ToString();

                filter.SortColumn = (ReqHistorySortColumnType)Strings.ToInt(lnkBtn.CommandName);
                filter.SortOrder = newSortOrder;
                GetRequestData();

            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(RequestHistory), "LinkBtn_Click()", ex); 
            }
        }

        private SortOrderType ChangeSortOrder(SortOrderType so)
        {
            if (so == SortOrderType.Ascending)
                return SortOrderType.Descending;
            else
                return SortOrderType.Ascending;
        }

        protected void ActionGoButton_Click(object sender, ImageClickEventArgs e)
        {
            GetRequestData();
        }

        protected void CDRGoButton_Click(object sender, ImageClickEventArgs e)
        {
            //make sure that CDR ID is a valid integer 
            //it could be blank if the user is clearing the cdrid value 
            //refresh the results for that too
            int cdrid = Strings.ToInt(CdrIdTextBox.Text);
            if (cdrid > 0 || CdrIdTextBox.Text.Trim().Length == 0)
            {
                GetRequestData();
            }
            else
            {
                ReqRepeater.DataSource = null;
                ReqRepeater.DataBind();
            }
        }

        #endregion 
    }


}
