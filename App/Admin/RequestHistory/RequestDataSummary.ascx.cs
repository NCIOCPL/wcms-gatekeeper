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

using GKManagers.BusinessObjects;

namespace GateKeeperAdmin.RequestHistory
{
    public partial class RequestDataSummary : System.Web.UI.UserControl
    {
        private RequestDataInfo _documentInfo;
        private bool _requestDataIsPresent;
        private bool _requestDataHasBeenRemoved;

        public RequestDataInfo DocumentInfo
        {
            get { return _documentInfo; }
            set { _documentInfo = value; }
        }

        /// <summary>
        /// Controls the label appearing at the top of the summary.
        /// </summary>
        public String SectionLabel
        {
            get { return sectionLabel.Text; }
            set { sectionLabel.Text = value; }
        }

        /// <summary>
        /// Designates whether a document is present for the promotion level.
        /// If a document is not present, then it either has not been promoted to the
        /// level, or it has been promoted but subsquently removed.
        /// </summary>
        public bool RequestDataIsPresent
        {
            get { return _requestDataIsPresent; }
            set { _requestDataIsPresent = value; }
        }

        /// <summary>
        /// RequestDataHasBeenRemoved is a refinement of RequestDataIsPresent.
        /// If the document is not present, RequestDataHasBeenRemoved is used to
        /// clarify whether it has been removed or was never promoted.
        /// </summary>
        public bool RequestDataHasBeenRemoved
        {
            get { return _requestDataHasBeenRemoved; }
            set { _requestDataHasBeenRemoved = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (_requestDataIsPresent && _documentInfo != null)
                {
                    Cdrid.Text = _documentInfo.CdrID.ToString();
                    RequestIdLabel.Text = "<a href=\"RequestDetails.aspx?reqid=" + _documentInfo.RequestID.ToString() + "\">" + _documentInfo.RequestID.ToString() + "</a>";
                    DateLabel.Text = _documentInfo.ReceivedDate.ToString();
                    CDRVersionLabel.Text = _documentInfo.CdrVersion;
                    DocumentHistoryLink.NavigateUrl = "RequestDataHistory.aspx?reqDataID=" + _documentInfo.RequestDataID.ToString() + "&reqid=" + _documentInfo.RequestID.ToString();
                    DocumentHistoryLink.Visible = true;
                    XMLLink.Visible = true;
                    if (_documentInfo.ActionType == RequestDataActionType.Export)
                    {
                        XMLLink.Text = "<a href=\"ViewXML.aspx?reqDataID=" + _documentInfo.RequestDataID.ToString() + "\">XML</a>";
                    }
                    else
                    {
                        XMLLink.Text = "(Removed)";
                    }
                }
                else if (_requestDataHasBeenRemoved)
                {
                    XMLLink.Visible = true;
                    XMLLink.Text = "(Removed)";
                }
                else
                {
                    RequestIdLabel.Text = "(Not Present)";
                }
            }
        }
    }
}