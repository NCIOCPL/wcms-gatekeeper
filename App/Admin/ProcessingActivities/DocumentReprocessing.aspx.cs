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
using System.Linq;

using GKManagers;
using GKManagers.BusinessObjects;
using GateKeeper.DocumentObjects;
using GateKeeper.Common;
using GateKeeperAdmin.Common;

namespace GateKeeperAdmin.ProcessingActivities
{
    public partial class DocumentReprocessing : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsUserAdmin())
                Response.Redirect("~/home.aspx");

            //set active tab 
            SetActiveNavTab((int)AdminNavTabs.AdminNavTabType.AdminTab);

            if (!IsPostBack)
            {
                CommonUI.PopulateDropDownFromEnum(DocTypeDropDown, typeof(DocumentType), true);
                pnlResults.Visible = false;
            }

            Page.ClientScript.RegisterExpandoAttribute(cvIsConfirmed.ClientID, "RB1", chkReallyWant.ClientID, false);
            Page.ClientScript.RegisterExpandoAttribute(cvIsConfirmed2.ClientID, "RB1", chkReallyWant.ClientID, false);

            // Enables ui elements that can be used by user logged in as admin.
            // to Schedule requests
            bool adminOperationVisible = IsUserAdmin();
            btnScheduleSelected.Visible = adminOperationVisible;
            btnScheduleAll.Visible = adminOperationVisible;
            chkReallyWant.Visible = adminOperationVisible;
        }

        protected void DropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reset confirmation checkbox.
            chkReallyWant.Checked = false;

            // no doctype selection in the interface.
            if (DocTypeDropDown.SelectedValue == "-1")
                pnlResults.Visible = false;
            else
                LoadAndDisplayRequestDocuments();
        }

        /// <summary>
        /// Handle Click evnt 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected void ibtnGo_Click(object sender, ImageClickEventArgs e)
        {
            // no doctype selection in the interface.
            if (DocTypeDropDown.SelectedValue == "-1")
                pnlResults.Visible = false;
            else
                LoadAndDisplayRequestDocuments();
        }

        protected void btnScheduleSelected_Click(object sender, ImageClickEventArgs e)
        {
            if (IsUserAdmin())
            {
                int totalDocCount = 0;
                //schedule selected documents for processing 
                List<int> requestDataIdList = GetSelectedCheckboxesRequestId(ref totalDocCount);
                if (requestDataIdList.Count > 0)
                {
                    // Process the selected requested ids.
                    ReProcessDocuments(requestDataIdList, DocTypeDropDown.SelectedValue);
                }
            }
            else
                Response.Redirect("../Error.aspx?msg=3");
        }

        /// <summary>
        /// This event is fired when the Schedule All button is clicked. This
        /// method creates a list of GateKeeperRequestDataID values for all 
        /// documents matching the Request ID. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnScheduleAll_Click(object sender, ImageClickEventArgs e)
        {
            if (IsUserAdmin())
            {
                int totalDocCount = 0;
                //schedule selected documents for processing 
                List<int> requestDataIdList = GetAllRequestIds(ref totalDocCount);
                if (requestDataIdList.Count > 0)
                {
                    // Process the selected requested ids.
                    ReProcessDocuments(requestDataIdList, DocTypeDropDown.SelectedValue);
                }
            }
            else
                Response.Redirect("../Error.aspx?msg=3");
        }

        /// <summary>
        /// This method calls copy request which creates a new request and 
        /// makes new copies of the requestdata item after that is success it schedules
        /// a new batch using the newly created request data.
        /// </summary>
        /// <param name="requestDataIdList">The existing requestdata list</param>
        private void ReProcessDocuments(List<int> requestDataIdList, string documentType)
        {
            int[] requestDataIds = requestDataIdList.ToArray();
            int requestId = 0;
            string source = string.Empty;
            string externalRequestId = string.Empty;

            // Make copies of the request from existing request data.
            RequestStatusType requestStatusType = RequestManager.CopyRequest(GetUserName(), requestDataIds, out requestId, DTDVersion);
            if (requestStatusType == RequestStatusType.DataReceived)
            {
                // Once the request data for reprocessing is created, schedule a new batch.
                ScheduleBatch(documentType, requestId);
            }
            else
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(DocumentReprocessing), "Failed in ReProcessDocuments", "CopyRequest returned " + requestStatusType.ToString());
                Response.Redirect("../Error.aspx?msg=4");
            }

        }

        protected void SelectAll_Click(object sender, EventArgs e)
        {
            CommonUI.SelectOrClearAllCheckboxes(rptRequestsWithCurrentDTD, "ChkBox", true);
        }

        protected void ClearAll_Click(object sender, EventArgs e)
        {
            CommonUI.SelectOrClearAllCheckboxes(rptRequestsWithCurrentDTD, "ChkBox", false);
        }

        protected string DTDVersion
        {
            get { return ConfigurationManager.AppSettings["DTDVersion"]; }
        }

        private void LoadAndDisplayRequestDocuments()
        {
            try
            {
                // not doctype selection in the interface.
                if (DocTypeDropDown.SelectedValue == "-1")
                {
                    return;
                }

                // Read all the requests for this document type 
                List<RequestLocationInternalIds> arrLocation = GetLocations(DocTypeDropDown.SelectedValue, 0);

                // match all the request for the current DTD.
                var currentDTDVersionRequests =
                from reqLocation in arrLocation
                where reqLocation.LiveReqId != -1
                && reqLocation.ActionType == RequestDataActionType.Export
                && reqLocation.Status == RequestStatusType.DataReceived
                orderby reqLocation.LiveReqId
                select new
                {
                    CompleteReceivedTime = reqLocation.CompleteReceivedTime,
                    RequestID = reqLocation.LiveReqId,
                    ExternalRequestID = reqLocation.ExternalRequestId,
                    RequestDataID = reqLocation.LiveRequestDataID,
                    GateKeeperDTDVersion = reqLocation.LiveDTDVersion,
                    CdrId = reqLocation.CdrId,
                    Title = reqLocation.Title,
                    GroupId = reqLocation.GroupId
                };

                rptRequestsWithCurrentDTD.DataSource = currentDTDVersionRequests;
                rptRequestsWithCurrentDTD.DataBind();

                pnlResults.Visible = true;

            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(DocumentReprocessing), "Failed in ibtnGo_Click()", ex);
            }
        }

        private List<RequestLocationInternalIds> GetLocations(string documentType, int CdrID)
        {
            int docType = Strings.ToInt(documentType);
            //Convert DocType suitable for st proc excepting All and Empty DropDown List
            if (docType > 0)
                docType = (int)DocumentTypeConverter.GKToCdr((DocumentType)docType);

            List<RequestLocationInternalIds> arrLocation = new List<RequestLocationInternalIds>();
            try
            {
                if (docType != -1)
                    arrLocation = RequestManager.LoadRequestLocationInternalIds(docType, CdrID);
                else
                    GKAdminLogBuilder.Instance.CreateError(typeof(DocumentReprocessing), "GetLocations", string.Format("Invalid document type {0}", docType));
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(DocumentReprocessing), "Failed in GetLocations(int, int)", ex);
            }

            return arrLocation;
        }

        /// <summary>
        /// Returns all the request data ids 
        /// </summary>
        /// <param name="countOfSelectedDocs">The total number of items selected</param>
        /// <returns>The list containing the reqeuest data ids.</returns>
        private List<int> GetAllRequestIds(ref int countOfSelectedDocs)
        {
            countOfSelectedDocs = 0;
            List<string> selectedGroupsList = new List<string>();
            List<int> selectedRequstedDataIdList = new List<int>();

            foreach (RepeaterItem ri in rptRequestsWithCurrentDTD.Items)
            {
                string groupIdSelected = ((Label)ri.FindControl("GroupIdLbl")).Text;
                string requestId = ((Label)ri.FindControl("RequestIdLbl")).Text;
                int requestDataId = Strings.ToInt(((Label)ri.FindControl("RequestDataIdLbl")).Text);
                selectedRequstedDataIdList.Add(requestDataId);
            }

            return selectedRequstedDataIdList;
        }

        /// <summary>
        /// Returns all the request data item that were selected by the user. 
        /// </summary>
        /// <param name="countOfSelectedDocs">The total number of items selected</param>
        /// <returns>The list containing the reqeuest data ids.</returns>
        private List<int> GetSelectedCheckboxesRequestId(ref int countOfSelectedDocs)
        {
            countOfSelectedDocs = 0;
            List<string> selectedGroupsList = new List<string>();
            List<int> selectedRequstedDataIdList = new List<int>();

            List<RepeaterItem> uncheckedForGroups = new List<RepeaterItem>();

            foreach (RepeaterItem ri in rptRequestsWithCurrentDTD.Items)
            {
                string groupIdSelected = ((Label)ri.FindControl("GroupIdLbl")).Text;
                string requestId = ((Label)ri.FindControl("RequestIdLbl")).Text;
                int requestDataId = Strings.ToInt(((Label)ri.FindControl("RequestDataIdLbl")).Text);

                if (((CheckBox)ri.FindControl("ChkBox")).Checked)
                {
                    countOfSelectedDocs++; 
                    selectedRequstedDataIdList.Add(requestDataId);
                }
            }

            return selectedRequstedDataIdList;
        }



        /// <summary>
        /// 1.Create a new Batch object with the current username, a start action of 
        /// ProcessActionType.PromoteToStaging, an end action of ProcessActionType.PromoteToLive and a 
        /// batch name of "Reprocess all documents of type {0}, documents from request {1}." 
        /// Where {0} is the selected document type and {1} is the Request ID. 
        /// 2.Schedule the batch by calling BatchManager.CreateNewBatch() with the Batch object and array of request data IDs. 
        /// </summary>
        /// <param name="documentType">The </param>
        /// <param name="reqDataIds"></param>
        private void ScheduleBatch(string documentType, int requestId)
        {
            bool bSucceeded = false;
            try
            {
                string userName = GetUserName();
                string batchName = string.Format("Reprocess documents of type {0}", documentType);
                if (batchName != null && batchName.Length > 0 && userName != null)
                {
                    Batch bat = new Batch(batchName, userName, ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToLive);
                    bSucceeded = BatchManager.CreateNewBatch(ref bat, requestId);
                }
            }
            catch (Exception ex)
            {
                bSucceeded = false;
                GKAdminLogBuilder.Instance.CreateError(typeof(DocumentReprocessing), "Failed in ScheduleBatch()", ex);
            }

            if (bSucceeded)
                Response.Redirect("../ThankYou.aspx?msg=1");
            else
                Response.Redirect("../Error.aspx?msg=5");

        }
    }
}
