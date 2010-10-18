using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Services.Protocols;
using GKManagers.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSManager.CMS
{
    /// <summary>
    /// Utility methods for communicating with the Percussion CMS.
    /// </summary>
    public static class PSWSUtils
    {
        /*
           Don't even *THINK* about adding any static fields to this class.
        */


        /// <summary>
        /// Creates and intialize a proxy of the Percussion service for maintaining
        /// login sessions.
        /// </summary>
        /// <param name="protocol">Communications protocol to use when connecting to
        /// the Percussion server.  Should be either HTTP or HTTPS.</param>
        /// <param name="host">Host name or IP address of the Percussion server.</param>
        /// <param name="port">Port number to use when connecting to the Percussion server.</param>
        /// <returns>An initialized proxy for the Percussion security service.</returns>
        public static securitySOAP GetSecurityService(string protocol, string host, string port)
        {
           securitySOAP securitySvc = new securitySOAP();

            securitySvc.Url = RewriteServiceUrl(securitySvc.Url, protocol, host, port);

               // create a cookie object to maintain JSESSION
               CookieContainer cookie = new CookieContainer();
               securitySvc.CookieContainer = cookie;

            return securitySvc;
        }


        /// <summary>
        /// Rewrites a Percussion service URL with a new protocol, host and port number.
        /// </summary>
        /// <param name="srcAddress">The source address with the 
        ///     the connection information (protocol, host and port)</param>
        /// <param name="protocol">Communications protocol to use when connecting to
        ///     the Percussion server.  Should be either HTTP or HTTPS.</param>
        /// <param name="host">Host name or IP address of the Percussion server.</param>
        /// <param name="port">Port number to use when connecting to the Percussion server.</param>
        /// <returns>The modified service URL.</returns>
        private static String RewriteServiceUrl(String srcAddress, string protocol, string host, string port)
        {
            int pathStart = srcAddress.IndexOf("/Rhythmyx/");
            return string.Format("{0}://{1}:{2}{3}",
                protocol, host, port, srcAddress.Substring(pathStart));
        }


        /// <summary>
        /// Login to a Percussion sesession with the specified credentials and associated parameters.
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
        /// Creates and intialize a proxy of the Percussion service used for manipulating
        /// content items and relationships.
        /// </summary>
        /// <param name="protocol">Communications protocol to use when connecting to
        ///     the Percussion server.  Should be either HTTP or HTTPS.</param>
        /// <param name="host">Host name or IP address of the Percussion server.</param>
        /// <param name="port">Port number to use when connecting to the Percussion server.</param>
        /// <param name="cookie">The cookie container for maintaining the session for all
        ///     webservice requests.</param>
        /// <param name="authHeader">The authentication header for maintaining the Rhythmyx session
        ///     for all webservice requests.</param>
        /// <returns>An initialized proxy for the Percussion content service.</returns>
        public static contentSOAP GetContentService(string protocol, string host, string port, CookieContainer cookie, PSAuthenticationHeader authHeader)
        {
            contentSOAP contentSvc = new contentSOAP();

            contentSvc.Url = RewriteServiceUrl(contentSvc.Url, protocol, host, port);
            contentSvc.CookieContainer = cookie;
            contentSvc.PSAuthenticationHeaderValue = authHeader;

            return contentSvc;
        }

        /// <summary>
        /// Creates and intialize a proxy of the Percussion service used for manipulating
        /// the overall system (folders, list of relationships, etc).
        /// </summary>
        /// <param name="protocol">Communications protocol to use when connecting to
        ///     the Percussion server.  Should be either HTTP or HTTPS.</param>
        /// <param name="host">Host name or IP address of the Percussion server.</param>
        /// <param name="port">Port number to use when connecting to the Percussion server.</param>
        /// <param name="cookie">The cookie container for maintaining the session for all
        ///     webservice requests.</param>
        /// <param name="authHeader">The authentication header for maintaining the Rhythmyx session
        ///     for all webservice requests.</param>
        /// <returns>An initialized proxy for the Percussion system service.</returns>
        public static systemSOAP GetSystemService(string protocol, string host, string port, CookieContainer cookie, PSAuthenticationHeader authHeader)
        {
            systemSOAP systemSvc = new systemSOAP();

            systemSvc.Url = RewriteServiceUrl(systemSvc.Url, protocol, host, port);
            systemSvc.CookieContainer = cookie;
            systemSvc.PSAuthenticationHeaderValue = authHeader;

            return systemSvc;
        }

        /// <summary>
        /// Logs out from the Rhythmyx session.
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
            //TODO: Refactor SaveItem() and its callers to receive an array of PSItem objects.
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
            //TODO: Refactor PrepareForEdit() to receive an array of long values.
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
        /// Retrieves an array of PSItem objects representing the content items
        /// specified in the idList argument.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="id">The ID of the Content Item to be loaded.</param>
        /// <returns>An array of PSItem objects, listed in the same order as the
        /// entries in idList. Never null or empty.</returns>
        /// <remarks>The returned items are not editable.</remarks>
        public static PSItem[] LoadItems(contentSOAP contentSvc, long[] idList)
        {
            LoadItemsRequest req = new LoadItemsRequest();
            PSItem[] items;
            try
            {
                req.Id = idList;
                req.IncludeFolderPath = true;
                items = contentSvc.LoadItems(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in LoadItem.", ex);
            }

            return items;
        }

        /// <summary>
        /// Release the specified Content Item from Edit
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="status">The status of the Content Item to be released for edit.</param>
        public static void ReleaseFromEdit(contentSOAP contentSvc, PSItemStatus status)
        {
            // TODO: Refactor ReleaseFromEdit() to receive an array of PSItemStatus objects.
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
        /// Moves a collection of content items from one folder to another.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="targetPath">The target folder path.</param>
        /// <param name="sourcePath">The source folder path.</param>
        /// <param name="id">The content item ids to be moved</param>
        /// <remarks>All items being moved must have the same source and target paths.</remarks>
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
        /// Retrieves relationships via the Percussion Content service.
        /// By default, all defined active asesmbly relationships are returned. The list
        /// may be filtered by assigning values to the various properties
        /// of the PSAaRelationshipFilter object.
        /// </summary>
        /// <param name="contentSvc">Instance of the Percussion content service.</param>
        /// <param name="filter">An instance of PSAaRelationshipFilter specifying
        /// criteria to use when filtering the list of relationsships.</param>
        /// <returns>An array of active asesmbly relationship objects. Never null,
        /// but may be empty.</returns>
        public static PSAaRelationship[] GetRelationships(contentSOAP contentSvc, PSAaRelationshipFilter filter)
        {
            try
            {
                LoadContentRelationsRequest req = new LoadContentRelationsRequest();
                req.loadReferenceInfo = true;
                req.PSAaRelationshipFilter = filter;
                return contentSvc.LoadContentRelations(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in GetRelationships.", ex);
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
