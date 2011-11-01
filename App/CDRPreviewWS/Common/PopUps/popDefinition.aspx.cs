using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace CDRPreviewWS.Common.PopUps
{
    public partial class popDefinition : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string redirectUrl = GetServerURL() + Request.Url.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
            redirectUrl = redirectUrl.Replace("/CDRPreviewWS", "");
            Response.Redirect(redirectUrl);
        }

        /// <summary>
        /// Retrieve web service server url from configuration file
        /// </summary>
        /// <param></param>
        /// <returns>Return server url</returns>
        private string GetServerURL()
        {
            return ConfigurationManager.AppSettings["ServerURL"];
        }
    }
}
