using System;
using System.Configuration;
using System.IO;
using System.Security.Principal;
using System.Web;

namespace GateKeeperAdmin.Authenticate
{
    public class GKAuthenticate : IHttpModule
    {
        //These are the extensions that site minder does not care about and ignores.
        String[] IgnoreExtensions
        {
            get
            {
                // Retrieve the preferred list of types to ignore from the config appSettings
                // section.  This is meant to be a comma-separated list, so separate on those dividers.
                String[] extensionList;
                String[] defaults = { ".class", ".gif", ".jpg", ".jpeg", ".png", ".fcc", ".scc", ".sfcc", ".ccc", ".ntc" };

                string textList = ConfigurationManager.AppSettings["AuthIgnoreFileTypes"];
                if (!string.IsNullOrEmpty(textList))
                {
                    Char[] separator = { ',' };
                    extensionList = textList.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                    extensionList = defaults;

                return extensionList;
            }
        }


        private void AuthenticateRequest(object sender, EventArgs e)
        {
            // Get the application and request objects.
            HttpApplication application = sender as HttpApplication;
            HttpContext context = application.Context;
            HttpRequest request = context.Request;

            try
            {

                if (DoesRequestExtRequireAuth(request))
                {
                    // Check for SSO username and domain override in the config file.
                    string userName = ConfigurationManager.AppSettings["Override_SSO_Username"];
                    if (String.IsNullOrEmpty(userName))
                        userName = request.ServerVariables["HTTP_SM_USER"];

                    //We do not need the auth domain for this really... well I do not think so unless,
                    //NIH is going to start allowing other domains to connect

                    if (!String.IsNullOrEmpty(userName))
                    {
                        // Create a new identity object.
                        IIdentity identity = new GenericIdentity(userName, "SimpleSSOModule");
                        // Create a new principal object and assign it to the current context.
                        context.User = new GenericPrincipal(identity, new string[] {"gkadmin"});
                    }
                    else
                    {
                        // Do Nothing???
                        // throw new Exception("SM_USER header is blank!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to authenticate request.", ex);
            }
        }

        private bool DoesRequestExtRequireAuth(HttpRequest request)
        {
            bool rtnVal = true;

            string ext = Path.GetExtension(request.Path);

            if (!string.IsNullOrEmpty(ext))
            {
                String[] ignoreList = IgnoreExtensions;
                // If the IgnoreList contains the extension, then authentication is not needed.
                // Really, what we want is an If-NOT-Exist method...
                rtnVal = !Array.Exists(ignoreList, item => { return string.Compare(ext, item, true) == 0; });
            }

            return rtnVal;
        }


        #region IHttpModule Members

        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += new EventHandler(AuthenticateRequest);
        }

        public void Dispose()
        {
            // If we need to dispose of anything, here's where we do it.
        }

        #endregion
    }
}