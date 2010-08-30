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
using GateKeeper.Common;
using GateKeeperAdmin.Common;

namespace GateKeeperAdmin.RequestHistory
{
    public partial class ViewXML : System.Web.UI.Page
    {
        String _xml;

        public String XML
        {
            get { return _xml; }
        }

        //show xml document in the browser 
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                RequestData rd = null;
                int _reqDataID = Strings.ToInt(Request.QueryString["reqDataId"]);
                if (_reqDataID>0)
                    rd = RequestManager.LoadRequestDataByID(_reqDataID);
                if (rd != null && rd.DocumentData != null)
                {
                    _xml = rd.DocumentDataString;
                }
                else
                {
                    if (rd.ActionType == RequestDataActionType.Remove)
                    {
                        _xml = "<INFO>NO XML is available for REMOVE Request Type</INFO>";
                    }
                    else
                    {
                        _xml = "<INFO>XML file is missing</INFO>";
                    }
                }
                // Set the page's content type to XML files
                Response.ContentType = "text/xml";
            }
            catch(Exception ex)
            {
                _xml = "<Error>Errors loading XML data</Error>";
                GKAdminLogBuilder.Instance.CreateError(typeof(ViewXML), "Failed in Page_Load() - LoadRequestDataByID", ex); 
            }

        }
    }
}
