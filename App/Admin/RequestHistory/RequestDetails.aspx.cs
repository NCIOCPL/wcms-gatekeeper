using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
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
    public partial class RequestDetails : BasePage
    {
        private List<int> SelCheckBoxes = new List<int>();
        private int _requestID = 0;
        private int _batchID = 0;
        private bool _isBatchScreen = false;

        private RequestDataFilter filter = new RequestDataFilter();

        protected void Page_Load(object sender, EventArgs e)
        {

            _requestID = Strings.ToInt(Request.QueryString["ReqId"]);
            _batchID = Strings.ToInt(Request.QueryString["BatchId"]);
            if (_batchID > 0)
            {
                IsBatchScreen = true;
                BatchIdLbl.Text = " |  Batch Id: " + _batchID.ToString();
            }
            ShowHideActionsArea();
            if (!Page.IsPostBack)
            {
                filter.SortColumn = ReqDataSortColumnType.CdrIdColumn;
                filter.SortOrder = SortOrderType.Ascending;
                RequestIdLbl.Text = _requestID.ToString();
                RequestIdLbl1.Text = _requestID.ToString();
                SetActiveNavTab((int)AdminNavTabs.AdminNavTabType.ReqHistTab);
                PopulateFilterDropDowns();
                SetLinkButtons();   
                GetRequestData();
                BuildPopupLink();
            }
        }

        void Page_PreRender(object sender, EventArgs e)
        {
            if (ConfirmationDiv.Visible == true)
                pnlInfo.Visible = false;
            else
                pnlInfo.Visible = true;
        }

        #region setup controls 
        private void PopulateFilterDropDowns()
        {
            CommonUI.PopulateDropDownFromEnum(RequestTypeDropDown, typeof(RequestDataActionType), true);
            CommonUI.PopulateDropDownFromEnum(DocTypeDropDown, typeof(DocumentType), true);
            CommonUI.PopulateDropDownFromEnum(StatusDropDown, typeof(RequestDataStatusType), true);
            CommonUI.PopulateDropDownFromEnum(DepStatusDropDown, typeof(RequestDataDependentStatusType), true);
            CommonUI.PopulateDropDownFromEnum(LocationDropDown, typeof(RequestDataLocationType), true);
        }

        private void ClearFilterDropDowns()
        {
            RequestTypeDropDown.ClearSelection();
            DocTypeDropDown.ClearSelection();
            StatusDropDown.ClearSelection();
            DepStatusDropDown.ClearSelection();
            LocationDropDown.ClearSelection();
        }

        private void SetLinkButtons()
        {
            CdrIdLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            CdrIdLinkBtn.CommandName = ((int)ReqDataSortColumnType.CdrIdColumn).ToString();

            PacketNumLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            PacketNumLinkBtn.CommandName = ((int)ReqDataSortColumnType.PacketNumberColumn).ToString();

            GroupIdLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            GroupIdLinkBtn.CommandName = ((int)ReqDataSortColumnType.GroupIdColumn).ToString();

            DocTypeLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            DocTypeLinkBtn.CommandName = ((int)ReqDataSortColumnType.DocTypeColumn).ToString();

            DocStatusLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            DocStatusLinkBtn.CommandName = ((int)ReqDataSortColumnType.StatusColumn).ToString();

            DepStatusLinkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            DepStatusLinkBtn.CommandName = ((int)ReqDataSortColumnType.DependencyStatusColumn).ToString();

            LocationLnkBtn.CommandArgument = ((int)SortOrderType.Ascending).ToString();
            LocationLnkBtn.CommandName = ((int)ReqDataSortColumnType.LocationColumn).ToString();
        }


        private void ShowHideActionsArea()
        {
            bool bShowActions = false;

            //do not show actions for requests that are not in DataReceived status 
            if (!_isBatchScreen){
                if (_requestID > 0)
                {
                    try
                    {
                        Request req = RequestManager.LoadRequestByID(_requestID);
                        if (req.Status == RequestStatusType.DataReceived)
                            bShowActions = true;
                    }
                    catch(Exception ex)
                    {
                        GKAdminLogBuilder.Instance.CreateError(typeof(RequestDetails), "Failed in ShowHideActionsArea()", ex); 
                    }
                }
            }
            //show actions for promotion only for administrators and operators 
            //only if showing request details, not batch details             
            if  (((IsUserAdmin() || IsUserOperator())) && !_isBatchScreen && bShowActions)
            {
                ActionsDiv.Visible = true;
                NoActionsDiv.Visible = false;
            }
            else
            {
                ActionsDiv.Visible = false;
                NoActionsDiv.Visible = true;
            }
        }

        #endregion 

        #region checkboxes
        private List<int> GetSelectedCheckboxes(ref int countOfSelectedDocs)
        {
            countOfSelectedDocs = 0;
            List<int> selectedGroupsList = new List<int>(); 
            foreach(RepeaterItem ri in ReqRepeater.Items){
                if (((CheckBox)ri.FindControl("ChkBox")).Checked){
                    countOfSelectedDocs++;
                    int groupIdSelected = Strings.ToInt(((Label)ri.FindControl("GroupIdLbl")).Text);
                    if (!selectedGroupsList.Contains(groupIdSelected)){
                        selectedGroupsList.Add(groupIdSelected);
                    }
                }
            }
            return selectedGroupsList;
        }

         private List<int> GetSelectedCheckboxesRequestId()
        {
            List<int> selectedGroupsList = new List<int>();
            foreach (RepeaterItem ri in ReqRepeater.Items)
            {
                if (((CheckBox)ri.FindControl("ChkBox")).Checked)
                {
                        int id = Strings.ToInt(((Label)ri.FindControl("RequestDataIdLbl")).Text);
                        selectedGroupsList.Add(id);
                }
            }
            return selectedGroupsList;
        }

        #endregion 

        private void SetFilter()
        {
            try {
                int reqType = Strings.ToInt(RequestTypeDropDown.SelectedValue);
                int docType = Strings.ToInt(DocTypeDropDown.SelectedValue);
                int status = Strings.ToInt(StatusDropDown.SelectedValue);
                int depStatus = Strings.ToInt(DepStatusDropDown.SelectedValue);
                int location = Strings.ToInt(LocationDropDown.SelectedValue);
                CDRDocumentType cdrDocType = DocumentTypeConverter.GKToCdr((DocumentType)docType);

                filter.ActionType = (RequestDataActionType)reqType;
                filter.DocTypeId = cdrDocType;
                filter.DocStatus = (RequestDataStatusType)status;
                filter.DependencyStatus = (RequestDataDependentStatusType)depStatus;
                filter.RequestDataLocation = (RequestDataLocationType)location;
            }catch(Exception ex){
                GKAdminLogBuilder.Instance.CreateError(typeof(RequestDetails), "Failed in SetFilter()", ex); 
            }
        }

        private List<int> GetRequestDataIdList()
        {
            List<int> reqDataIdList = new List<int>();
            try
            {
                foreach (RepeaterItem ri in RepeaterConfirm.Items)
                {

                    int reqDataId = Strings.ToInt(((Label)ri.FindControl("RequestDataIdLbl")).Text);
                    reqDataIdList.Add(reqDataId);
                }
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(RequestDetails), "Failed in GetRequestDataIdList()", ex); 
            }
            return reqDataIdList;
        }

        private void GetRequestData()
        {
            if (_requestID > 0 || _batchID > 0)
            {
                SetFilter();
                try
                {
                    int totalRequestDataCount = 0;

                    List<RequestData> arr = null;
                    if (_batchID > 0)
                    {
                        arr = RequestManager.LoadRequestDataListByBatchID(_batchID, filter, 1, 0, ref totalRequestDataCount);
                    }
                    else
                    {
                        arr = RequestManager.LoadRequestDataListByReqID(_requestID, filter, 1, 0, ref totalRequestDataCount);
                    }
                    ReqRepeater.DataSource = arr;
                    ReqRepeater.DataBind();
                    RequestDocCount.Text = totalRequestDataCount.ToString();
                    lblNumEntries.Text = arr.Count.ToString();
                }
                catch (Exception ex)
                {
                    GKAdminLogBuilder.Instance.CreateError(typeof(RequestDetails), "Failed in GetRequestData()", ex); 
                }
            }

        }
        private void ScheduleBatch(string batchName, int requestID, ProcessActionType startAction, ProcessActionType endAction, List<int> reqDataIdList)
        {
            bool bSucceeded = true;
            try
            {
                if (IsUserAdmin()|| IsUserOperator())
                {
                    string userName = GetUserName();

                    Batch bat = new Batch(batchName, userName, startAction, endAction);

                    if (reqDataIdList.Count == 0)
                        bSucceeded = BatchManager.CreateNewBatch(ref bat, requestID);
                    else
                        bSucceeded = BatchManager.CreateNewBatch(ref bat, reqDataIdList.ToArray());
                }
            }
            catch(Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(RequestDetails), "Failed in ScheduleBatch()", ex);                
            }
            if (bSucceeded)
                Response.Redirect("../ThankYou.aspx?msg=1");
            else
                Response.Redirect("../Error.aspx");
        }

        private void GetStartEndAction(ref ProcessActionType startAction, ref ProcessActionType endAction)
        {
          switch (ActionDropDown.SelectedIndex)
            {
                case 1: // Promote to Preview.
                    startAction = ProcessActionType.PromoteToStaging;
                    endAction = ProcessActionType.PromoteToPreview;
                    break;
                case 2: // Promote to Live.
                    startAction = ProcessActionType.PromoteToStaging;
                    endAction = ProcessActionType.PromoteToLive;
                    break;
                case 3: // Copy from Preview to Live.
                    startAction = ProcessActionType.PromoteToLive;
                    endAction = ProcessActionType.PromoteToLive;
                    break;
                default:
                    break;
            }
        }

        private void BuildPopupLink()
        {
            try
            {
                if (_requestID < 0)
                {
                    aPopup.HRef = String.Empty;
                    return;
                }
                string strQS = aPopup.ResolveUrl("~/RequestHistory/Popup.aspx?reqid=") + _requestID.ToString();
                aPopup.Attributes["onclick"] = "window.open('" + strQS + "', '', 'scrollbars=yes,resizable=yes,width=600,height=400'); return false;";
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(BatchHistory), "Failed in GetBatchData()", ex);
            }
        }

        #region Events 
        protected void ScheduleAllButton_Click(object sender, ImageClickEventArgs e)
        {

            //schedule all documents in tequest for for processing 

            ProcessActionType startAction = ProcessActionType.Invalid;
            ProcessActionType endAction = ProcessActionType.Invalid;
            string batchName = Strings.Clean(BatchNameTxt.Text);

            //if an action is selected and batch name entered 
            if (ActionDropDown.SelectedIndex > 0 && batchName != null && batchName.Length > 0)
            {
                GetStartEndAction(ref startAction, ref endAction);
                ScheduleBatch(batchName, _requestID, startAction, endAction, new List<int>());
            }
        }
            
        protected void ActionGoButton_Click(object sender, ImageClickEventArgs e)
        {
            //schedule selected documents for processing 
            int requestDocCount = Strings.ToInt(RequestDocCount.Text);
            int totalDocCount = 0;
            ProcessActionType startAction = ProcessActionType.Invalid;
            ProcessActionType endAction = ProcessActionType.Invalid;
            string batchName = Strings.Clean(BatchNameTxt.Text);

            //if an action is selected and batch name entered 
            if (ActionDropDown.SelectedIndex > 0 && batchName!= null && batchName.Length >0)
            {
                List<int> groupList = GetSelectedCheckboxes(ref totalDocCount);
                if (groupList.Count > 0)
                {
                    if (totalDocCount == requestDocCount)
                    {
                        //if the whole request is selected - schdule it 
                        GetStartEndAction(ref startAction, ref endAction);
                        ScheduleBatch(batchName,_requestID, startAction, endAction, new List<int>());
                    }
                    else
                    {
                        //get a list of all requestData objects given selected groups 
                        //draw the confirmation screen 
                        ConfirmationDiv.Visible = true;
                        ScheduleDiv.Visible = false;
                        ActionLabel.Text = ActionDropDown.SelectedItem.ToString();
                        BatchNameLbl.Text = BatchNameTxt.Text;

                        //get a list of all requestdata for selected groups 
                        List<RequestData> arr = RequestManager.LoadRequestDataListByReqIDGroups(_requestID, groupList);
                        RepeaterConfirm.DataSource = arr;
                        RepeaterConfirm.DataBind();
                    }
                }
            }

        }

        protected void ScheduleGoButton_Click(object sender, ImageClickEventArgs e)
        {
            //schedule batch for selected set of requestdata objects for a given request id 
            List<int> reqDataIdList = GetRequestDataIdList();
            //make sure batch name is not empty 
            string batchName = Strings.Clean(BatchNameLbl.Text);
            ProcessActionType startAction = ProcessActionType.Invalid;
            ProcessActionType endAction = ProcessActionType.Invalid;


            if (reqDataIdList.Count > 0 && batchName != null && batchName.Length >0)
            {
                GetStartEndAction(ref startAction, ref endAction);
                ScheduleBatch(batchName, _requestID, startAction, endAction, reqDataIdList);
            }
        }

        protected void DropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetRequestData();
        }

        protected void ClearFiltersButton_Click(object sender, ImageClickEventArgs e)
        {
            ClearFilterDropDowns();
            GetRequestData();
        }

        protected void ExportButton_Click(object sender, ImageClickEventArgs e)
        {
            bool bSucceeded = true;
            try
            {
                //export selected set of requestdata objects and an associated request            
                List<int> reqDataIdList = GetSelectedCheckboxesRequestId();
                if (reqDataIdList.Count > 0)
                {
                    RequestManager.ExportRequest(reqDataIdList);
                }
            }
            catch (Exception ex)
            {
                bSucceeded = false;
                GKAdminLogBuilder.Instance.CreateError(typeof(RequestDetails), "Failed in ExportButton_Click()", ex); 
            }
            if (!bSucceeded)
            {
                Response.Redirect("../Error.aspx");
            }
        }

        protected void LinkBtn_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton lnkBtn = (LinkButton)sender;
                //change order 
                SortOrderType newSortOrder = ChangeSortOrder((SortOrderType)Strings.ToInt(lnkBtn.CommandArgument));
                lnkBtn.CommandArgument = ((int)newSortOrder).ToString();

                filter.SortColumn = (ReqDataSortColumnType)Strings.ToInt(lnkBtn.CommandName);
                filter.SortOrder = newSortOrder;
                SelCheckBoxes = GetSelectedCheckboxesRequestId();
                GetRequestData();

            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(RequestDetails), "Failed in LinkBtn_Click()", ex); 
            }
        }

        private SortOrderType ChangeSortOrder(SortOrderType so)
        {
            if (so == SortOrderType.Ascending)
                return SortOrderType.Descending;
            else
                return SortOrderType.Ascending;
        }

        protected void SelectAll_Click(object sender, EventArgs e)
        {
            CommonUI.SelectOrClearAllCheckboxes(ReqRepeater, "ChkBox", true);
        }

        protected void ClearAll_Click(object sender, EventArgs e)
        {
            CommonUI.SelectOrClearAllCheckboxes(ReqRepeater, "ChkBox", false);
        }

        /// <summary>
        /// Handle Data Bound event to provide CheckBox persistence
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected void ReqRepeater_ItemDataBound(Object sender, RepeaterItemEventArgs e)
        {
            if (!IsPostBack || e.Item.DataItem == null)
                return;
            int nRequestDataID;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                nRequestDataID = ((RequestData)(e.Item.DataItem)).RequestDataID;
                if (SelCheckBoxes.Contains(nRequestDataID))
                {
                    CheckBox checkBox = (CheckBox)e.Item.FindControl("ChkBox");
                    checkBox.Checked = true;
                }
            }
        }

        #endregion 

        #region Properties 

        public int RequestID
        {
            get { return _requestID; }
            set { _requestID = value; }
        }

        public int BatchID
        {
            get { return _batchID; }
            set { _batchID = value; }
        }

        public bool IsBatchScreen
        {
            get { return _isBatchScreen; }
            set { _isBatchScreen = value; }
        }

        #endregion 

    }
}
