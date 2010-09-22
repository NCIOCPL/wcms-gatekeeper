using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Services.Protocols;
using GKManagers.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSManager.CMS
{
    public class PSWSUtils
    {

        private static string ms_protocol = "http";
        private static string ms_host = "156.40.134.66";
        private static int ms_port = 9922;
        public static void SetConnectionInfo(string protocol, string host, int port)
        {

            ms_protocol = protocol;
            ms_host = host;
            ms_port = port;
        }

        public static securitySOAP GetSecurityService()
        {
            securitySOAP securitySvc = new securitySOAP();
            securitySvc.Url = GetNewAddress(securitySvc.Url);

            // create a cookie object to maintain JSESSION
            CookieContainer cookie = new CookieContainer();
            securitySvc.CookieContainer = cookie;

            return securitySvc;
        }


        private static String GetNewAddress(String srcAddress)
        {
            int pathStart = srcAddress.IndexOf("/Rhythmyx/");
            return ms_protocol + "://" + ms_host + ":" + ms_port +
                srcAddress.Substring(pathStart);
        }


        public static string Login(securitySOAP securitySvc, string user,string password, string community, string locale)
        {
            LoginRequest loginReq = new LoginRequest();
            loginReq.Username = user;
            loginReq.Password = password;
            loginReq.Community = community;
            loginReq.LocaleCode = locale;

            // Setting the authentication header to maintain Rhythmyx session
            LoginResponse loginResp = securitySvc.Login(loginReq);
            string rxSession = loginResp.PSLogin.sessionId;
            securitySvc.PSAuthenticationHeaderValue = new PSAuthenticationHeader();
            securitySvc.PSAuthenticationHeaderValue.Session = rxSession;

            return rxSession;
        }


        public static contentSOAP GetContentService(CookieContainer cookie,PSAuthenticationHeader authHeader)
        {
            contentSOAP contentSvc = new contentSOAP();
            contentSvc.Url = GetNewAddress(contentSvc.Url);

            contentSvc.CookieContainer = cookie;
            contentSvc.PSAuthenticationHeaderValue = authHeader;

            return contentSvc;
        }

        public static systemSOAP GetSystemService(CookieContainer cookie,PSAuthenticationHeader authHeader)
        {
            systemSOAP systemSvc = new systemSOAP();
            systemSvc.Url = GetNewAddress(systemSvc.Url);

            systemSvc.CookieContainer = cookie;
            systemSvc.PSAuthenticationHeaderValue = authHeader;

            return systemSvc;
        }

        public static void Logout(securitySOAP securitySvc, String rxSession)
        {
            LogoutRequest logoutReq = new LogoutRequest();
            logoutReq.SessionId = rxSession;
            securitySvc.Logout(logoutReq);
        }

        public static PSFolder[] AddFolderTree(contentSOAP contentSvc,
            string folderPath)
        {
            AddFolderTreeRequest req = new AddFolderTreeRequest();
            req.Path = folderPath;
            return contentSvc.AddFolderTree(req);
        }

        public static PSItemSummary[] FindFolderChildren(contentSOAP contentSvc,
            string folderPath)
        {
            FindFolderChildrenRequest req = new FindFolderChildrenRequest();
            req.Folder = new FolderRef();
            req.Folder.Path = folderPath;
            return contentSvc.FindFolderChildren(req);
        }

        public static PSItem CreateItem(contentSOAP contentSvc, string contentType)
        {
            CreateItemsRequest request = new CreateItemsRequest();
            request.ContentType = contentType;
            request.Count = 1;
            PSItem[] items = contentSvc.CreateItems(request);
            return items[0];
        }

        public static void CheckinItem(contentSOAP contentSvc, long id)
        {
            CheckinItemsRequest req = new CheckinItemsRequest();
            req.Id = new long[] { id };
            contentSvc.CheckinItems(req);
        }

        public static void TransitionItem(systemSOAP systemSvc, long id,
            string trigger)
        {
            TransitionItemsRequest req = new TransitionItemsRequest();
            req.Id = new long[] { id };
            req.Transition = trigger;
            systemSvc.TransitionItems(req);
        }
        public static void AddFolderChildren(contentSOAP contentSvc,
            string folderPath, long[] childIds)
        {
            AddFolderChildrenRequest req = new AddFolderChildrenRequest();
            req.ChildIds = childIds;
            req.Parent = new FolderRef();
            req.Parent.Path = folderPath;
            contentSvc.AddFolderChildren(req);
        }
        public static long SaveItem(contentSOAP contentSvc, PSItem item)
        {
            SaveItemsRequest req = new SaveItemsRequest();
            req.PSItem = new PSItem[] { item };
            SaveItemsResponse response = contentSvc.SaveItems(req);

            return response.Ids[0];
        }
        public static PSItemStatus PrepareForEdit(contentSOAP contentSvc, long id)
        {
            return contentSvc.PrepareForEdit(new long[] { id })[0];
        }

        public static PSItem LoadItem(contentSOAP contentSvc, long id)
        {
            LoadItemsRequest req = new LoadItemsRequest();
            req.Id = new long[] { id };
            req.IncludeBinary = true;
            req.IncludeFolderPath = true;
            PSItem[] items = contentSvc.LoadItems(req);
            return items[0];
        }
        public static void ReleaseFromEdit(contentSOAP contentSvc, PSItemStatus status)
        {
            ReleaseFromEditRequest req = new ReleaseFromEditRequest();
            req.PSItemStatus = new PSItemStatus[] { status };
            contentSvc.ReleaseFromEdit(req);
        }
    }
}
