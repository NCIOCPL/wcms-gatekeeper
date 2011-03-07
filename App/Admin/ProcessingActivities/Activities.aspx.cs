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

namespace GateKeeperAdmin.ProcessingActivities
{
    public partial class Activities : BasePage 
    {
        private bool bActiveTab = true;

        protected void Page_Load(object sender, EventArgs e)
        {   
            //set active tab 
            SetActiveNavTab((int)AdminNavTabs.AdminNavTabType.ActivitiesTab);

            DeactivateButton.Attributes.Add("onclick", "return confirm_deactivate();");

            //show activate or deactivate button 

            ActivateButton.Visible = false;
            DeactivateButton.Visible = false;
            //only show activate / deactivate buttons for administrators, not operators 
            SystemStatusType systemStatus = RequestManager.GetGateKeeperSystemStatus();
            if (IsUserAdmin())
            {
                if (systemStatus == SystemStatusType.Stopped)
                {
                    ActivateButton.Visible = true;
                }
                else
                {
                    DeactivateButton.Visible = true;
                }
            }

            //show action buttons for admin users only
            if (IsUserAdmin() || IsUserOperator())
                ActionsDiv.Visible = true;
            else
                ActionsDiv.Visible = false;

            // Regardless of Admin permissions, show/hide system status message.
            if (systemStatus == SystemStatusType.Stopped)
                StatusDiv.Visible = true;
            else
                StatusDiv.Visible = false;

            //show processing tab or failed tab 
            int activeTabIndx = Strings.ToInt(Request.QueryString["tab"]);
            if (activeTabIndx > 0)
            {
                if (activeTabIndx == 1)
                    //active batches tab
                    bActiveTab = true;
                else
                    //failed batches tab
                    bActiveTab = false;
            }
            if (!Page.IsPostBack)
            {
                LoadBatches();
            }
        }

        //load processing queue/failed batches from db and display them 
        private void LoadBatches(){
            DataSet batches = null;
            try
            {
                if (bActiveTab)
                {
                    ActiveTabDiv.Visible = true;
                    FailedTabDiv.Visible = false;
                    batches = BatchManager.LoadActiveBatchDetailsList();
                    int cntFailed = BatchManager.GetFailedBatchProcessQueueCount();
                    ShowErrorImages(cntFailed > 0);
                }
                else
                {
                    ActiveTabDiv.Visible = false;
                    FailedTabDiv.Visible = true;
                    batches = BatchManager.LoadFailedBatchDetailsList();
                    ShowErrorImages(batches != null && batches.Tables[0].Rows.Count > 0);
                }
                if (batches != null)
                {
                    BatchRepeater.DataSource = batches;
                    BatchRepeater.DataBind();
                }
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(typeof(Activities), "Failed to load batches in LoadBatches()", ex); 
            }
            finally
            {
                if(batches != null)
                    batches.Dispose();
            }
        }

        //show/hide exclamation mark image on Failed Batches tabs if there are failed batches 
        private void ShowErrorImages(bool bShow){
            if (bShow){
                ErrorImage1.Visible = true;
                ErrorImage2.Visible = true;
            }
            else
            {
                ErrorImage1.Visible = false;
                ErrorImage2.Visible = false;
            }
        }

        //get a list of selected batches 
        private List<int> GetSelectedCheckboxes(ref int countOfSelectedDocs)
        {
            countOfSelectedDocs = 0;
            List<int> selectedGroupsList = new List<int>();
            foreach (RepeaterItem ri in BatchRepeater.Items)
            {
                if (((CheckBox)ri.FindControl("ChkBox")).Checked)
                {
                    countOfSelectedDocs++;
                    int idSelected = Strings.ToInt(((Label)ri.FindControl("BatchIdLbl")).Text);
                    if (!selectedGroupsList.Contains(idSelected))
                    {
                        selectedGroupsList.Add(idSelected);
                    }
                }
            }
            return selectedGroupsList;
        }

        #region Events 
        protected void ActivateButton_Click(object sender, ImageClickEventArgs e)
        {
            //allow the Gatekeeper Process Manager to pick up new batches for processing  
            RequestManager.StartGateKeeperSystem();
            ActivateButton.Visible = false;
            DeactivateButton.Visible = true;
            StatusDiv.Visible = false;
        }


        protected void DeactivateButton_Click(object sender, ImageClickEventArgs e)
        {
            //stop the Gatekeeper Process Manager 
            //no new bathches should be picked up for processing
            RequestManager.StopGateKeeperSystem();
            ActivateButton.Visible = true;
            DeactivateButton.Visible = false;
            StatusDiv.Visible = true;
        }

        protected void ClearButton_Click(object sender, ImageClickEventArgs e)
        {
            //delete selected batches from the queue if they are not in Processing Status 
            int cnt = 0;
            List<int> batchIdList = GetSelectedCheckboxes(ref cnt);
            if (cnt > 0)
            {
                foreach (int i in batchIdList)
                {
                    try
                    {
                        string userName = GetUserName();
                        bool bSucceded = false;
                        Batch b = BatchManager.LoadBatch(i);
                        if (b.Status == BatchStatusType.CompleteWithErrors)
                            bSucceded = BatchManager.ReviewBatch(ref b, userName);
                        else if (b.Status == BatchStatusType.Queued)
                            bSucceded = BatchManager.CancelBatch(ref b, userName);
                        if (!bSucceded)
                        {
                            Response.Redirect("../Error.aspx?msg=2"); 
                        }
                    }
                    catch (Exception ex)
                    {
                        GKAdminLogBuilder.Instance.CreateError(typeof(Activities), "Error in ClearButton_Click()", ex);
                    }
                }
                LoadBatches();
            }
        }
        
        protected void DropDown_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void SelectAll_Click(object sender, EventArgs e)
        {
            CommonUI.SelectOrClearAllCheckboxes(BatchRepeater, "ChkBox", true);
        }

        protected void ClearAll_Click(object sender, EventArgs e)
        {
            CommonUI.SelectOrClearAllCheckboxes(BatchRepeater, "ChkBox", false);
        }

        #endregion 

    }
}
