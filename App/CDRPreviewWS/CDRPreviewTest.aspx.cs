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
using System.Xml;
using GKManagers.BusinessObjects;
using GKManagers;

namespace CDRPreviewWS
{
    public class CDRPreviewTest : System.Web.UI.Page
    {
        protected System.Web.UI.WebControls.TextBox requestID;
        protected System.Web.UI.WebControls.TextBox cdrID;
        protected System.Web.UI.WebControls.Button btnPreview;
        protected System.Web.UI.WebControls.DropDownList dropDownList;
        protected string result;
        protected string cgovHtmlUrl=string.Empty;

         #region Web Form
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        private void InitializeComponent()
        {
        }
        #endregion


        protected void btnPreview_Click(object sender, EventArgs e)
        {
            int request = Int32.Parse(this.requestID.Text);
            int cdr = Int32.Parse(this.cdrID.Text);
            string documentType = dropDownList.SelectedValue;

            //RequestData requestData = RequestManager.LoadRequestDataByCdrid(request, cdr);
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.PreserveWhitespace = true;
            //xmlDoc.LoadXml(requestData.DocumentDataString);
            //CDRPreview previewSvc = new CDRPreview();
            //string html = previewSvc.ReturnXML((XmlNode)xmlDoc, documentType);
            //this.result = html;

            cgovHtmlUrl = string.Format("<iframe name=\"previewFrame\" Id=\"previewFrame\" onLoad='adjustMyFrameHeight();' width=\"100%\"  border=0 frameborder=0 scrolling=no src=CGovHtml.aspx?requestID={0}&cdrID={1}&documentType={2}></iframe>", request.ToString(), cdr.ToString(), documentType);
        }


    }
}
