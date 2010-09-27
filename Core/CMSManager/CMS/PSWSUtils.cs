﻿using System;
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

        /// <summary>
        ///     Creates a proxy of the security service.
        /// </summary>
        /// <returns>
        ///     the created proxy of the security service; 
        /// </returns>
        public static securitySOAP GetSecurityService()
        {
           securitySOAP securitySvc = new securitySOAP();
           try
           {
               securitySvc.Url = GetNewAddress(securitySvc.Url);

               // create a cookie object to maintain JSESSION
               CookieContainer cookie = new CookieContainer();
               securitySvc.CookieContainer = cookie;
           }
           catch (SoapException ex)
           {
               throw new CMSSoapException("Percussion Error in GetSecurityService.", ex);
           }

            return securitySvc;
        }


        /// <summary>
        /// Creates a new address from the specified source address.
        /// </summary>
        /// <param name="srcAddress">The source address with the 
        ///     the connection information (protocol, host and port)</param>
        /// <returns></returns>
        private static String GetNewAddress(String srcAddress)
        {
            int pathStart;

            try
            {
                pathStart = srcAddress.IndexOf("/Rhythmyx/");
            }

            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in GetNewAddress.", ex);
            }

            return ms_protocol + "://" + ms_host + ":" + ms_port +
                srcAddress.Substring(pathStart);
        }


        /// <summary>
        /// Login with the specified credentials and associated parameters.
        /// </summary>
        /// <param name="securitySvc">the proxy of the security service</param>
        /// <param name="user">The login user.</param>
        /// <param name="password">The login password.</param>
        /// <param name="community">The name of the Community into which to log the user</param>
        /// <param name="locale">The name of the Locale into which to log the user</param>
        /// <returns></returns>
        public static string Login(securitySOAP securitySvc, string user,string password, string community, string locale)
        {
            LoginRequest loginReq = new LoginRequest();
            string rxSession;
            try
            {
                loginReq.Username = user;
                loginReq.Password = password;
                loginReq.Community = community;
                loginReq.LocaleCode = locale;

                // Setting the authentication header to maintain Rhythmyx session
                LoginResponse loginResp = securitySvc.Login(loginReq);
                rxSession = loginResp.PSLogin.sessionId;
                securitySvc.PSAuthenticationHeaderValue = new PSAuthenticationHeader();
                securitySvc.PSAuthenticationHeaderValue.Session = rxSession;
            }

            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in Login.", ex);
            }
            
            return rxSession;
        }


        /// <summary>
        /// Creates a proxy of the content service
        /// </summary>
        /// <param name="cookie">The cookie container for maintaining the session for all
        ///     webservice requests.</param>
        /// <param name="authHeader">The authentication header for maintaining the Rhythmyx session
        ///     for all webservice requests.</param>
        /// <returns></returns>
        public static contentSOAP GetContentService(CookieContainer cookie,PSAuthenticationHeader authHeader)
        {
            contentSOAP contentSvc = new contentSOAP();
            try
            {
                contentSvc.Url = GetNewAddress(contentSvc.Url);

                contentSvc.CookieContainer = cookie;
                contentSvc.PSAuthenticationHeaderValue = authHeader;
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in GetContentService.", ex);
            }
            
            return contentSvc;
        }

        /// <summary>
        /// Creates a proxy of the system service.
        /// </summary>
        /// <param name="cookie">The cookie container for maintaining the session for all
        ///     webservice requests.</param>
        /// <param name="authHeader">The authentication header for maintaining the Rhythmyx session
        ///     for all webservice requests.</param>
        /// <returns></returns>
        public static systemSOAP GetSystemService(CookieContainer cookie,PSAuthenticationHeader authHeader)
        {
            systemSOAP systemSvc = new systemSOAP();
            try
            {
                systemSvc.Url = GetNewAddress(systemSvc.Url);

                systemSvc.CookieContainer = cookie;
                systemSvc.PSAuthenticationHeaderValue = authHeader;
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in GetSystemService.", ex);
            }

            return systemSvc;
        }

        /// <summary>
        /// Logs out the specified Rhythmyx session.
        /// </summary>
        /// <param name="securitySvc">The security proxy.</param>
        /// <param name="rxSession">The Rhythmyx session.</param>
        public static void Logout(securitySOAP securitySvc, String rxSession)
        {
            LogoutRequest logoutReq = new LogoutRequest();
            try
            {
                logoutReq.SessionId = rxSession;
                securitySvc.Logout(logoutReq);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in Logout.", ex);
            }

        }

        /// <summary>
        /// Creates Folders for the specified Folder path.  Any Folders specified in 
        /// the path that do not exist will be created; No action is taken on any  
        /// existing Folders.
        /// </summary>
        /// <param name="contentSvc">proxy of the content service</param>
        /// <param name="folderPath">The folder path.</param>
        /// <returns></returns>
        public static PSFolder[] AddFolderTree(contentSOAP contentSvc,
            string folderPath)
        {
            AddFolderTreeRequest req = new AddFolderTreeRequest();
            try
            {
                req.Path = folderPath;
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in AddFolderTree.", ex);
            }

            return contentSvc.AddFolderTree(req);
        }

        /// <summary>
        /// Finds the folder children.
        /// </summary>
        /// <param name="contentSvc">proxy of the content service</param>
        /// <param name="folderPath">The folder path.</param>
        /// <returns></returns>
        public static PSItemSummary[] FindFolderChildren(contentSOAP contentSvc,
            string folderPath)
        {
            FindFolderChildrenRequest req = new FindFolderChildrenRequest();
            try
            {
                req.Folder = new FolderRef();
                req.Folder.Path = folderPath;
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in FindFolderChildren.", ex);
            }

            return contentSvc.FindFolderChildren(req);
        }

        /// <summary>
        /// Creates a Content Item of the specified Content Type.
        /// </summary>
        /// <param name="contentSvc">proxy of the content service</param>
        /// <param name="contentType">Type of the content item(druginfosummary,pdqsummary,...).</param>
        /// <returns></returns>
        public static PSItem CreateItem(contentSOAP contentSvc, string contentType)
        {
            CreateItemsRequest request = new CreateItemsRequest();
            PSItem[] items;
            try
            {
                request.ContentType = contentType;
                request.Count = 1;
                items = contentSvc.CreateItems(request);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in CreateItem.", ex);
            }

            return items[0];
        }

        /// <summary>
        /// Checkin the specified Content Item.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service</param>
        /// <param name="id">The ID of the Content Item to be checked in.</param>
        public static void CheckinItem(contentSOAP contentSvc, long id)
        {
            CheckinItemsRequest req = new CheckinItemsRequest();
            try
            {
                req.Id = new long[] { id };
                contentSvc.CheckinItems(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in CheckinItem.", ex);
            }

        }

        /// <summary>
        /// Transitions the content items to cdrlive,cdrstaging,...
        /// </summary>
        /// <param name="systemSvc">The proxy of the content service.</param>
        /// <param name="idList">List of ids for the transition</param>
        /// <param name="trigger">The trigger.</param>
        public static void TransitionItems(systemSOAP systemSvc, long[] idList,
            string trigger)
        {
            TransitionItemsRequest req = new TransitionItemsRequest();
            try
            {
                req.Id = idList;
                req.Transition = trigger;
                systemSvc.TransitionItems(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in TransitionItems.", ex);
            }

        }
        /// <summary>
        /// Associates the specified Content Items with the specified Folder.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="childIds">The IDs of the objects to be associated with the Folder specified</param>
        public static void AddFolderChildren(contentSOAP contentSvc,
            string folderPath, long[] childIds)
        {
            AddFolderChildrenRequest req = new AddFolderChildrenRequest();
            try
            {
                req.ChildIds = childIds;
                req.Parent = new FolderRef();
                req.Parent.Path = folderPath;
                contentSvc.AddFolderChildren(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in AddFolderChildren.", ex);
            }

        }
        /// <summary>
        /// Saves the specified Content Item to the repository.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="item">The Content Item to be saved.</param>
        /// <returns></returns>
        public static long SaveItem(contentSOAP contentSvc, PSItem item)
        {
            SaveItemsResponse response = null;

            try
            {
                SaveItemsRequest req = new SaveItemsRequest();
                req.PSItem = new PSItem[] { item };
                response = contentSvc.SaveItems(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in SaveItem.", ex);
            }

            return response.Ids[0];
        }
        /// <summary>
        /// Prepares the specified Content Item for Edit.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="id">The ID of the Content Item to be prepared for editing.</param>
        /// <returns></returns>
        public static PSItemStatus PrepareForEdit(contentSOAP contentSvc, long id)
        {
            try
            {
                return contentSvc.PrepareForEdit(new long[] { id })[0];
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in PrepareForEdit.", ex);
            }

        }

        /// <summary>
        /// Loads the specified Content Item.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="id">The ID of the Content Item to be loaded.</param>
        /// <returns></returns>
        public static PSItem LoadItem(contentSOAP contentSvc, long id)
        {
            LoadItemsRequest req = new LoadItemsRequest();
            PSItem[] items;
            try
            {
                req.Id = new long[] { id };
                req.IncludeBinary = true;
                req.IncludeFolderPath = true;
                items = contentSvc.LoadItems(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in LoadItem.", ex);
            }

            return items[0];
        }
        /// <summary>
        /// Release the specified Content Item from Edit
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="status">The status of the Content Item to be released for edit.</param>
        public static void ReleaseFromEdit(contentSOAP contentSvc, PSItemStatus status)
        {
            ReleaseFromEditRequest req = new ReleaseFromEditRequest();
            try
            {
                req.PSItemStatus = new PSItemStatus[] { status };
                contentSvc.ReleaseFromEdit(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in ReleaseFromEdit.", ex);
            }

        }

        /// <summary>
        /// Deletes a content item.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="DeleteItemRequest">array of content item ids to be deleted.</param>
        public static void DeleteItem(contentSOAP contentSvc, long[] DeleteItemId)
        {
            try
            {
                contentSvc.DeleteItems(DeleteItemId);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in DeleteItem.", ex);
            }

        }



        /// <summary>
        /// Moves a content item from one folder to another.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="targetPath">The target folder path.</param>
        /// <param name="sourcePath">The source folder path.</param>
        /// <param name="id">The content item ids to be moved</param>
        public static void MoveFolderChildren(contentSOAP contentSvc, string targetPath,string sourcePath,long[] id)
        {
            MoveFolderChildrenRequest moveFolder = new MoveFolderChildrenRequest();
            FolderRef folderRefSource = new FolderRef();
            FolderRef folderRefTarget = new FolderRef();

            try
            {
                folderRefSource.Path = sourcePath;
                folderRefTarget.Path = targetPath;

                moveFolder.Source = folderRefSource;
                moveFolder.Target = folderRefTarget;
                moveFolder.ChildId = id;

                contentSvc.MoveFolderChildren(moveFolder);
            }

            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in MoveFolderChildren.", ex);
            }

            
        }


        /// <summary>
        /// Retrieve the list of workflow transitions allowed for a list of content items.
        /// </summary>
        /// <param name="systemSvc">The system service.</param>
        /// <param name="itemList">A list of content items.</param>
        /// <returns>A list of the names for transitions which are allowed for
        /// all content items specified in itemList.</returns>
        /// <remarks>Only returns the tranitions which are common to all
        /// items in the list</remarks>
        public static string[] GetTransitions(systemSOAP systemSvc, long[] itemList)
        {
            GetAllowedTransitionsResponse response;
            try
            {
                response = systemSvc.GetAllowedTransitions(itemList);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in GetTransitions.", ex);
            }

            return response.Transition;
        }
    }
}
