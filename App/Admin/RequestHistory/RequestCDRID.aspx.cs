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
using System.Collections.Generic;

using GateKeeper.Common;
using GKManagers;
using GKManagers.BusinessObjects;
using GateKeeperAdmin.Common;


namespace GateKeeperAdmin.RequestHistory
{
    public partial class RequestCDRID : BasePage
    {
        private int _requestID = -1;
        private int _cdrID = -1;
        private int _reqDataID = -1;

        private RequestData _reqData = null;
       

        protected void Page_Load(object sender, EventArgs e)
        {
            SetActiveNavTab((int)AdminNavTabs.AdminNavTabType.ReqHistTab);
            _requestID = Strings.ToInt(Request.QueryString["ReqId"]);
            _reqDataID = Strings.ToInt(Request.QueryString["ReqDataId"]);

            ShowHideActionsArea();

            if (!Page.IsPostBack)
            {
                GetRequestData();
            }

        }

        //show scheduling actions only for administrators only, on test environments, when config setting is set 
        //request must be in DataReceived status
        private void ShowHideActionsArea(){
            try
            {
                ScheduleActionsDiv.Visible = false;
                string allowActions = ConfigurationManager.AppSettings["AllowSchedulingOfOneDoc"];
                int allow = Strings.ToInt(allowActions);

                if ((allow == 1) && IsUserAdmin() && _requestID > 0)
                {
                    Request req = RequestManager.LoadRequestByID(_requestID);
                    if (req.Status == RequestStatusType.DataReceived)
                        ScheduleActionsDiv.Visible = true;
                }                 
            }
            catch (Exception ex)
            {                
                GKAdminLogBuilder.Instance.CreateError(typeof(RequestCDRID), "Failed in ShowHideActionsArea()", ex); 
            }
        }

        //schedule a batch for this cdrid 
        private void ScheduleBatch()
        {
            bool bPushStagingPreview = PushToStagingPreviewChkbx.Checked;            
            bool bPushLive = PushToLiveChkbx.Checked;
            int startAction = -1;
            int endAction = -1;
            bool bSucceeded = false;

            if (bPushStagingPreview)
            {
                startAction = (int)ProcessActionType.PromoteToStaging;
                endAction = (int)ProcessActionType.PromoteToPreview;
            }
            if (bPushLive)
            {
                if (startAction < 0)
                    startAction = (int)ProcessActionType.PromoteToLive;
                endAction = (int)ProcessActionType.PromoteToLive;
            }

            if (startAction < 0 || endAction <0)  //none of the check boxes were selected 
            {
                //should not get here, JavaScript function makes sure at least one action is set 
            }
            else
            {
                //schedule batch
                try
                {
                    string userName = GetUserName();
                    string batchName = Strings.Clean(BatchNameTxtBox.Text);
                    if (batchName != null && batchName.Length>0 && userName != null){ 
                        Batch bat = new Batch(batchName, userName, (ProcessActionType)startAction, (ProcessActionType)endAction);
                        int[] reqIds = new int[1];
                        reqIds[0] = _reqDataID;
                        bSucceeded  = BatchManager.CreateNewBatch(ref bat, reqIds);
                    }
                }
                catch(Exception ex)
                {
                   bSucceeded = false;
                   GKAdminLogBuilder.Instance.CreateError(typeof(RequestCDRID), "Failed in ScheduleBatch()", ex); 
                }

                if (bSucceeded)
                    Response.Redirect("../ThankYou.aspx?msg=1");
                else
                    Response.Redirect("../Error.aspx");
            }
        }

        //show data for this cdrid for gatekeeper, staging, preview and live databases
        private void GetRequestData(){
             
            if (_reqDataID > 0){
                try
                {
                    _reqData = RequestManager.LoadRequestDataByID(_reqDataID);
                    //_reqData = RequestManager.LoadRequestDataByCdrid(_requestID, _cdrID);

                    RequestDataIdLbl.Text = _reqDataID.ToString();
                    ActionTypeLbl.Text = _reqData.ActionType.ToString();
                    LocationLbl.Text = _reqData.Location.ToString();
                    PacketNumLbl.Text = _reqData.PacketNumber.ToString();
                    CDRIDLbl.Text = _reqData.CdrID.ToString();
                    CDRVersionLbl.Text = _reqData.CdrVersion;
                    DateLbl.Text = _reqData.ReceivedDate.ToString();
                    StatusLbl.Text = Enum.GetName(typeof(RequestDataStatusType), _reqData.Status);
                    DependencyStatusLbl.Text = Enum.GetName(typeof(RequestDataDependentStatusType), _reqData.DependencyStatus);
                    GroupIdLbl.Text = _reqData.GroupID.ToString();
                    DocTypeLbl.Text = Enum.GetName(typeof(CDRDocumentType), _reqData.CDRDocType);
                    _cdrID = _reqData.CdrID;

                    Dictionary<RequestDataLocationType, RequestDataInfo> locationRequests = RequestManager.LoadRequestDataListForCDRLocations(_cdrID);

                    List<RequestLocationInternalIds> locations =
                        RequestManager.LoadRequestLocationInternalIds(0, _cdrID);

                    RequestDataInfo rd = locationRequests[RequestDataLocationType.GateKeeper];
                    GateKeeperBox.DocumentInfo = rd;
                    GateKeeperBox.RequestDataIsPresent = locations[0].IsPresentInGateKeeper;
                    GateKeeperBox.RequestDataHasBeenRemoved = locations[0].IsRemovedFromGateKeeper;

                    rd = locationRequests[RequestDataLocationType.Staging];
                    StagingBox.DocumentInfo = rd;
                    StagingBox.RequestDataIsPresent = locations[0].IsPresentInStaging;
                    StagingBox.RequestDataHasBeenRemoved = locations[0].IsRemovedFromStaging;

                    rd = locationRequests[RequestDataLocationType.Preview];
                    PreviewBox.DocumentInfo = rd;
                    PreviewBox.RequestDataIsPresent = locations[0].IsPresentInPreview;
                    PreviewBox.RequestDataHasBeenRemoved = locations[0].IsRemovedFromPreview;

                    rd = locationRequests[RequestDataLocationType.Live];
                    LiveBox.DocumentInfo = rd;
                    LiveBox.RequestDataIsPresent = locations[0].IsPresentInLive;
                    LiveBox.RequestDataHasBeenRemoved = locations[0].IsRemovedFromLive;
                }
                catch (Exception ex)
                {
                    GKAdminLogBuilder.Instance.CreateError(typeof(RequestCDRID), "Failed in GetRequestData()", ex);
                }
            }
        }

        protected void GoButton_Click(object sender, ImageClickEventArgs e)
        {
            ScheduleBatch();
        }

        #region Properties
        protected int RequestID
        {
            get { return _requestID; }
        }

        protected int CdrID
        {
            get { return _cdrID; }
        }

        public int ReqDataID
        {
            get { return _reqDataID; }
            set { _reqDataID = value; }
        }
        #endregion
    }
}
