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
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            this.Load += new System.EventHandler(this.Page_Load);
        }
        #endregion


        protected void btnPreview_Click(object sender, EventArgs e)
        {
            int request = Int32.Parse(this.requestID.Text);
            int cdr = Int32.Parse(this.cdrID.Text);
            RequestData requestData = RequestManager.LoadRequestDataByCdrid(request, cdr);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(requestData.DocumentDataString);

            CDRPreview previewSvc = new CDRPreview();
            string html = previewSvc.ReturnXML((XmlNode)xmlDoc, dropDownList.SelectedValue);

            this.result = html;

        }


    }
}
