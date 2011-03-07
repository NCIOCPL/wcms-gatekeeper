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

using System.IO;
using System.Xml;
using System.Xml.Serialization;


using GKManagers;
using GKManagers.BusinessObjects;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CancerGov;
using GateKeeperAdmin.Common;

namespace GateKeeperAdmin.Administrator
{
    public partial class ResetCache : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Check Permission. If not permitted, redirect. 
            if (!IsUserAdmin())
            {
                Response.Redirect("../Home.aspx");
                return;
            }

            bool isImportVisible = false;
            
            if (ConfigurationManager.AppSettings["AllowImport"] != null)
                isImportVisible = (int.Parse(ConfigurationManager.AppSettings["AllowImport"].ToString()) ==1? true : false);

            this.ImportDoc.Visible = isImportVisible;

            if (!IsPostBack)
            {
                this.lblStatus.Text =string.Empty;
                GetOpenRequests();
            }

            //Only for Export Testing
            
           // if (Request.QueryString["RequestID"] != null)
           //     TestExport();
        }

        protected void btnImport_Click(object sender, EventArgs e)
        {
            this.lblStatus.Text = string.Empty;

            if (this.FileUploadImport.FileName.Length==0)
            {
                this.lblStatus.Text ="Please select a file.";
                return;
            }

            if (!this.FileUploadImport.FileName.EndsWith(".xml"))
            {
                this.lblStatus.Text ="An xml file is required.";
                return;
            }

            if (this.FileUploadImport.PostedFile.ContentLength==0)
            {
                this.lblStatus.Text ="The selected file is empty.";
                return;
            }
            try
            {
                int requestID;
                RequestStatusType type = RequestManager.ImportRequest(this.FileUploadImport.PostedFile.InputStream, "GateKeeper Import", this.Page.User.Identity.Name, out requestID);
                 this.lblStatus.Text = "Import Request: ID --" + requestID.ToString() +"  status: " +type.ToString();
            }
            catch (Exception ex)
            {
                this.lblStatus.Text += ex.Message +"  "+ ex.InnerException.Message;
            }
        }


        protected void btnResetLiveCache_Click(object sender, EventArgs e)
        {
            ClearCache(ContentDatabase.Live);
        }

        protected void btnResetPreviewCache_Click(object sender, EventArgs e)
        {
            ClearCache(ContentDatabase.Preview);
        }

        protected void btnAbort_Click(object sender, EventArgs e)
        {
            if (sender != null && ((Button)sender).CommandArgument != null)
            {
                AbortRequest(((Button)sender).CommandArgument);
                GetOpenRequests();
            }
        }
             
        private void ClearCache(ContentDatabase db)
        {
            this.lblStatus.Text = string.Empty;
          
            try
            {
                ProtocolQuery query = new ProtocolQuery();
                query.ClearSearchCache(db);
                this.lblStatus.Text = "Succeeded in clearing Search Cache for " + db.ToString();
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = ex.Message;
            }
        }

        private void TestExport()
        {
                //TEst export function
            int requestID = 100011;
 
            if (Request.QueryString["RequestID"] != null)
                requestID = int.Parse(Request.QueryString["RequestID"].ToString());

            try
            {
                RequestDataFilter filter = new RequestDataFilter();
                filter.ActionType= RequestDataActionType.Export;
                //filter.DependencyStatus = RequestDataDependentStatusType.Error;
                //filter.DocStatus = RequestDataStatusType.Error;
                //filter.DocTypeId = CDRDocumentType.GlossaryTerm;
                filter.RequestDataLocation = RequestDataLocationType.Live;

                List<int> ids = new List<int>();
                ids.Add(1714582);
                ids.Add(1714583);
                ids.Add(1714584);
                ids.Add(1714585);
                ids.Add(1714586);
                RequestManager.ExportRequest(ids);
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = ex.Message;
            }
        }

        private void GetOpenRequests()
        {
            RequestHistoryFilter filter = new RequestHistoryFilter();
            filter.RequestStatus = RequestStatusType.Receiving;

            DataSet results = null;
            try
            {
                results = RequestManager.LoadRequests(filter, 1, 0);
                ReqRepeater.DataSource = results;
                ReqRepeater.DataBind();
            }
            catch (Exception ex)
            {
                GKAdminLogBuilder.Instance.CreateError(this.GetType(), "GetOpenRequests", "Error loading list of open requests.", ex);
            }
            finally
            {
                if (results != null)
                    results.Dispose();
            }

        }

        private void AbortRequest(string requestIdentifiers)
        {
            try
            {
                string[] idParts = requestIdentifiers.Split(new Char[] { '|' });
                string userID = this.Page.User.Identity.Name;
                string message = string.Format(@"Request ""{0}"" from Source ""{1}"" aborted by user ""{1}"".",
                    idParts[0], idParts[1], userID);
                RequestManager.AbortRequest(idParts[0], idParts[1], userID, message);
            }
            catch(Exception ex)
            {
                // Log the error and swallow the exception.
                GKAdminLogBuilder.Instance.CreateError(this.GetType(), "AbortRequest", ex);
            }
        }

    }
}
