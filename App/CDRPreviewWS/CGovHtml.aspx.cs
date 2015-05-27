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
using GKManagers.CMSManager.Configuration;
using System.Globalization;
using System.Collections.Generic;
using System.Xml.XPath;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DocumentObjects;

namespace CDRPreviewWS
{
    public partial class CGovHtml : System.Web.UI.Page
    {
        protected string result = string.Empty;
        protected string contentHeader = string.Empty;
        protected string serverUrl = string.Empty;
        protected string currentHost = string.Empty;
        //set English as the default language
        protected string currentLanguage = "en";
    
        protected void Page_Load(object sender, EventArgs e)
        {
            serverUrl = ConfigurationManager.AppSettings["ServerURL"];
            currentHost = Request.Url.GetLeftPart(UriPartial.Authority);
            if (!string.IsNullOrEmpty(this.Request.Params["requestID"]) &&
                !string.IsNullOrEmpty(this.Request.Params["cdrID"]) &&
                !string.IsNullOrEmpty(this.Request.Params["documentType"]))
            {
                int request = Int32.Parse(this.Request.Params["requestID"]);
                int cdr = Int32.Parse(this.Request.Params["cdrID"]);
                string documentType = this.Request.Params["documentType"];

                RequestData requestData = RequestManager.LoadRequestDataByCdrid(request, cdr);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(requestData.DocumentDataString);

                CDRPreview previewSvc = new CDRPreview();
                currentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                if (previewSvc.GetDocumentType(documentType) == GateKeeper.DocumentObjects.PreviewTypes.Summary)
                {
                    XPathNavigator xNav = xmlDoc.CreateNavigator();
                    xNav.MoveToFirstChild();
                    DocumentXPathManager xPathManager = new DocumentXPathManager();
                    Language summaryLanguage;
                    string path = xPathManager.GetXPath(SummaryXPath.Lang);
                    summaryLanguage = DocumentHelper.DetermineLanguageString(DocumentHelper.GetXmlDocumentValue(xNav, path));

                    if (summaryLanguage == Language.Spanish)
                        currentLanguage = "es";

                }

                this.result = previewSvc.ReturnHTML((XmlNode)xmlDoc, documentType, ref contentHeader);                


            }
            else
            {
                result = "CONTENT HTML";
                contentHeader = "HEADER HTML";
            }
        }
             
    }
}
