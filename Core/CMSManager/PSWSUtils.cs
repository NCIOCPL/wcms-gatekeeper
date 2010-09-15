using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Services.Protocols;
using GKManagers.CMS.PercussionWebSvc;
namespace GKManagers.CMS
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
    }
}
