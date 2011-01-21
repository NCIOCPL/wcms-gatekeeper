using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Services.Protocols;
using System.Xml;

using NCI.WCM.CMSManager.PercussionWebSvc;
using NCI.WCM.CMSManager;

namespace NCI.WCM.CMSManager.CMS
{
    /// <summary>
    /// Utility methods for communicating with the Percussion CMS.
    /// </summary>
    internal static class PSWSUtils
    {
        /*
           Don't even *THINK* about adding any static fields to this class.
        */


        #region Constants

        const string errorNamespace = "urn:www.percussion.com/6.0.0/faults";
        const string errorNamespacePrefix = "ns1";
        const string errorResultPath = "//ns1:PSErrorResultsFault";
        const string folderNotFoundErrorPath = "//ns1:PSError[@code='43']";

        #endregion


        #region Content Service Methods

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
        /// Finds the content items residing in a folder.
        /// </summary>
        /// <param name="contentSvc">proxy of the content service</param>
        /// <param name="folderPath">The folder path.</param>
        /// <returns></returns>
        public static PSItemSummary[] FindFolderChildren(contentSOAP contentSvc,
            string folderPath)
        {
            PSItemSummary[] folderResults;

            try
            {
                FindFolderChildrenRequest req = new FindFolderChildrenRequest();
                req.Folder = new FolderRef();
                req.Folder.Path = folderPath;
                folderResults = contentSvc.FindFolderChildren(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in FindFolderChildren.", ex);
            }

            return folderResults;
        }

        /// <summary>
        /// Finds the content items residing in a folder.
        /// </summary>
        /// <param name="contentSvc">proxy of the content service</param>
        /// <param name="folderID">ID fo the folder to check.</param>
        /// <returns></returns>
        public static PSItemSummary[] FindFolderChildren(contentSOAP contentSvc, int folderID)
        {
            PSItemSummary[] folderResults;

            try
            {
                FindFolderChildrenRequest req = new FindFolderChildrenRequest();
                req.Folder = new FolderRef();
                req.Folder.Id = folderID;
                folderResults = contentSvc.FindFolderChildren(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in FindFolderChildren.", ex);
            }

            return folderResults;
        }

        public static PSContentTypeSummary[] LoadContentTypes(contentSOAP contentSvc)
        {
            PSContentTypeSummary[] contentData;

            try
            {
                LoadContentTypesRequest req = new LoadContentTypesRequest();
                req.Name = null;
                contentData = contentSvc.LoadContentTypes(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion error in PSContentTypeSummary", ex);
            }
            return contentData;
        }

        public static PSFolder[] LoadFolders(contentSOAP contentSvc, string folderPath)
        {
            PSFolder[] returnItems;

            // Loading a non-existant folder results in a SoapException,
            // so we have to do some extra work to handle that possiblity.
            try
            {
                LoadFoldersRequest req = new LoadFoldersRequest();
                req.Path = new string[] { folderPath };
                returnItems = contentSvc.LoadFolders(req);
            }
            catch (SoapException ex)
            {
                if (ConfirmFolderNotFoundError(ex.Detail))
                {
                    returnItems = null;
                }
                else
                {
                    // OK, that wasn't the error we expected.
                    throw;
                }
            }

            return returnItems;
        }

        /// <summary>
        /// Parses the detail node of a SoapException to verify that the
        /// error which occured really was a Folder Not Found error.
        /// </summary>
        /// <param name="errorDetail">Detail property from a SoapException.</param>
        /// <returns>true or false depending on whether the error was
        /// the Folder Not Found.</returns>
        private static bool ConfirmFolderNotFoundError(XmlNode errorDetail)
        {
            bool errorConfirmed = false;

            XmlNameTable names = new NameTable();
            XmlNamespaceManager nameMgr = new XmlNamespaceManager(names);
            nameMgr.AddNamespace(errorNamespacePrefix, errorNamespace);
            XmlNode error = errorDetail.SelectSingleNode(errorResultPath, nameMgr);
            if (error != null)
            {
                XmlNode errorCode = error.SelectSingleNode(folderNotFoundErrorPath, nameMgr);
                if (errorCode != null)
                {
                    errorConfirmed = true;
                }
            }

            return errorConfirmed;
        }

        /// <summary>
        /// Creates a Content Item of the specified Content Type.
        /// </summary>
        /// <param name="contentSvc">proxy of the content service</param>
        /// <param name="contentType">Type of the content item(druginfosummary,pdqsummary,...).</param>
        /// <returns>An empty content item with fields defined according to contentType.</returns>
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

        public static PSChildEntry[] CreateChildEntries(contentSOAP contentSvc, long parentItemID, string fieldSetName, int count)
        {
            PSChildEntry[] entrylist;

            try
            {
                CreateChildEntriesRequest req = new CreateChildEntriesRequest();
                req.Id = parentItemID;
                req.Name = fieldSetName;
                req.Count = count;
                entrylist = contentSvc.CreateChildEntries(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion error in CreateChildEntries", ex);
            }

            return entrylist;
        }

        public static void SaveChildEntries(contentSOAP contentSvc, long parentID, string fieldSetName, PSChildEntry[] childEntries)
        {
            try
            {
                SaveChildEntriesRequest req = new SaveChildEntriesRequest();

                req.Id = parentID;
                req.Name = fieldSetName;
                req.PSChildEntry = childEntries;

                contentSvc.SaveChildEntries(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion error in SaveChildEntries", ex);
            }
        }

        /// <summary>
        /// Checkin the specified Content Item.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service</param>
        /// <param name="id">The ID of the Content Item to be checked in.</param>
        public static void CheckinItem(contentSOAP contentSvc, long id)
        {
            CheckInItemList(contentSvc, new long[] { id });
        }

        public static void CheckInItemList(contentSOAP contentSvc, long[] idList)
        {
            CheckinItemsRequest req = new CheckinItemsRequest();
            try
            {
                req.Id = idList;
                contentSvc.CheckinItems(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in CheckinItem.", ex);
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
        /// Saves a Content Item to the repository.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="item">The Content Item to be saved.</param>
        /// <returns></returns>
        [Obsolete("Use SaveItem(contentSOAP contentSvc, PSItem[] item)")]
        public static long SaveItem(contentSOAP contentSvc, PSItem item)
        {
            //TODO: Refactor SaveItem() and its callers to receive an array of PSItem objects.
            PSItem[] itemArray = new PSItem[] { item };
            return SaveItem(contentSvc, itemArray)[0];
        }

        /// <summary>
        /// Saves a collection of Content Items to the repository.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="item">An array of Content Items to be saved.</param>
        /// <returns></returns>
        public static long[] SaveItem(contentSOAP contentSvc, PSItem[] item)
        {
            SaveItemsResponse response;

            try
            {
                SaveItemsRequest req = new SaveItemsRequest();
                req.PSItem = item;
                response = contentSvc.SaveItems(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in SaveItem.", ex);
            }

            return response.Ids;
        }

        /// <summary>
        /// Prepares the specified Content Item for Edit.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="idList">Array ofContent Item IDs to be prepared for editing.</param>
        /// <returns>An array of PSItemStatus objects reflecting the checkout state of each
        /// item in idList. The list is in the same order as the values in idList.</returns>
        public static PSItemStatus[] PrepareForEdit(contentSOAP contentSvc, long[] idList)
        {
            try
            {
                return contentSvc.PrepareForEdit( idList );
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in PrepareForEdit.", ex);
            }

        }

        /// <summary>
        /// Release the specified Content Item from Edit
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="status">Collection of status objects for the Content Items to be released for edit.</param>
        public static void ReleaseFromEdit(contentSOAP contentSvc, PSItemStatus[] status)
        {
            try
            {
                ReleaseFromEditRequest req = new ReleaseFromEditRequest();
                req.PSItemStatus = status;
                contentSvc.ReleaseFromEdit(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in ReleaseFromEdit.", ex);
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
                if (idList.Length > 0)
                {
                    req.Id = idList;
                    req.IncludeFolderPath = true;
                    req.IncludeChildren = true;
                    items = contentSvc.LoadItems(req);
                }
                else
                {
                    items = new PSItem[] { };
                }
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in LoadItem.", ex);
            }

            return items;
        }

        /// <summary>
        /// Deletes a list of content items.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="itemsToDelete">array of content item ids to be deleted.</param>
        public static void DeleteItem(contentSOAP contentSvc, long[] itemsToDelete)
        {
            try
            {
                if (itemsToDelete.Length > 0)
                {
                    contentSvc.DeleteItems(itemsToDelete);
                }
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
        /// Deletes a collection of folders. Requires admin permissions in order to purge
        /// folder contents.
        /// </summary>
        /// <param name="contentSvc">The proxy of the content service.</param>
        /// <param name="idList">List of folder IDs to delete</param>
        /// <param name="purgeItems">If true, folder contents are purged instead of being removed.
        /// (Requires admin permssion.)</param>
        public static void DeleteFolders(contentSOAP contentSvc, long[] idList, bool purgeItems)
        {
            try
            {
                DeleteFoldersRequest req = new DeleteFoldersRequest();
                req.Id = idList;
                req.PurgItems = purgeItems;
                contentSvc.DeleteFolders(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in DeleteFolders.", ex);
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
        public static PSAaRelationship[] FindRelationships(contentSOAP contentSvc, PSAaRelationshipFilter filter)
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
                throw new CMSSoapException("Percussion Error in FindRelationships.", ex);
            }

        }

        /// <summary>
        /// Creates relationships between a parent object and a collection of child objects using a named
        /// slot and snippet template.
        /// </summary>
        /// <param name="contentSvc">Instance of the Percussion content service.</param>
        /// <param name="parentItemID">ID of the parent content item.</param>
        /// <param name="childItemIDList">Array of child item IDs.</param>
        /// <param name="slotName">Name of the slot which will contain the child items.</param>
        /// <param name="snippetTemplateName">Name of the snippet template to use when rendering
        /// the child items.</param>
        /// <returns>An array of PSAaRelationship objects representing the created relationships.
        /// The array is never null or empty</returns>
        /// <remarks>Before relationships may be created, the parent content item must be placed in an
        /// editable condition by calling PrepareForEdit.</remarks>
        public static PSAaRelationship[] CreateActiveAssemblyRelationships(contentSOAP contentSvc,
            long parentItemID, long[] childItemIDList, string slotName, string snippetTemplateName)
        {
            PSAaRelationship[] results = null;
            try
            {
                AddContentRelationsRequest req = new AddContentRelationsRequest();
                req.Id = parentItemID;
                req.RelatedId = childItemIDList;
                req.Slot = slotName;
                req.Template = snippetTemplateName;


                results = contentSvc.AddContentRelations(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion error in CreateActiveAssemblyRelationships().", ex);
            }

            return results;
        }

        public static PSAaRelationship[] CreateActiveAssemblyRelationships(contentSOAP contentSvc,
            long parentItemID, long[] childItemIDList, string slotName, string snippetTemplateName, int index)
        {
            PSAaRelationship[] results = null;
            try
            {
                AddContentRelationsRequest req = new AddContentRelationsRequest();
                req.Id = parentItemID;
                req.RelatedId = childItemIDList;
                req.Slot = slotName;
                req.Template = snippetTemplateName;
                req.Index = index;


                results = contentSvc.AddContentRelations(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion error in CreateActiveAssemblyRelationships().", ex);
            }

            return results;
        }


        public static void DeleteActiveAssemblyRelationships(contentSOAP contentSvc, long[] relationshipIDList)
        {
            try
            {
                contentSvc.DeleteContentRelations(relationshipIDList);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion error in DeleteActiveAssemblyRelationships().", ex);
            }
        }

        public static PSSearchResults[] FindItemByFieldValues(contentSOAP contentSvc, string contentType, Dictionary<string,string> fieldCriteria)
        {
            return FindItemByFieldValues(contentSvc, contentType, null, fieldCriteria);
        }

        public static PSSearchResults[] FindItemByFieldValues(contentSOAP contentSvc, string contentType, string searchPath, Dictionary<string, string> fieldCriteria)
        {
            PSSearchResults[] results;

            // The contentSvc.FindItems() method will throw a low-level error if the folder doesn't exist.
            // For our purposes, if caller specifies that the item must exist in a non-existant
            // path, then the item doesn't exist.

            // Before doing the other work, require either searchPath is empty, or else it contains
            // a valid path.  (If searchPath is null/empty, the second test won't be performed.)
            if (string.IsNullOrEmpty(searchPath) || FolderExists(contentSvc, searchPath))
            {
                FindItemsRequest req = new FindItemsRequest();

                // Basic set up.
                req.PSSearch = new PSSearch();
                req.PSSearch.useExternalSearchEngine = false;
                req.PSSearch.useDbCaseSensitivity = false;
                req.PSSearch.PSSearchParams = new PSSearchParams();

                // Search for specific content types.
                if (!string.IsNullOrEmpty(contentType))
                {
                    req.PSSearch.PSSearchParams.ContentType = contentType;
                }

                // Search in path
                if (!string.IsNullOrEmpty(searchPath))
                {
                    req.PSSearch.PSSearchParams.FolderFilter = new PSSearchParamsFolderFilter();
                    req.PSSearch.PSSearchParams.FolderFilter.includeSubFolders = true;
                    req.PSSearch.PSSearchParams.FolderFilter.Value = searchPath;
                }

                // Search for fields
                if (fieldCriteria != null && fieldCriteria.Count > 0)
                {
                    req.PSSearch.PSSearchParams.Parameter = new PSSearchField[fieldCriteria.Count];
                    int offset = 0;
                    foreach (KeyValuePair<string, string> pair in fieldCriteria)
                    {
                        req.PSSearch.PSSearchParams.Parameter[offset] = new PSSearchField();
                        req.PSSearch.PSSearchParams.Parameter[offset].name = pair.Key;
                        req.PSSearch.PSSearchParams.Parameter[offset].Value = pair.Value;
                        req.PSSearch.PSSearchParams.Parameter[offset].Operator = operatorTypes.equal;
                    }
                }

                try
                {
                    results = contentSvc.FindItems(req);
                }
                catch (SoapException ex)
                {
                    throw new CMSSoapException("Percussion error in FindItemByFieldValues.", ex);
                }
            }
            else
            {
                results = new PSSearchResults[] { };
            }

            return results;
        }

        public static bool FolderExists(contentSOAP contentSvc, string folderPath)
        {
            bool found = false;

            PSFolder[] folders = PSWSUtils.LoadFolders(contentSvc, folderPath);
            if (folders != null && folders.Length > 0)
                found = true;

            return found;

        }

        #endregion

        #region System Service Methods

        public static PSRelationship CreateRelationship(systemSOAP systemSvc,
            long parentItemID, long childItemID, string relationshipType)
        {
            PSRelationship relationship = null;

            try
            {
                CreateRelationshipRequest req = new CreateRelationshipRequest();
                req.Name = relationshipType;
                req.OwnerId = parentItemID;
                req.DependentId = childItemID;

                CreateRelationshipResponse result = systemSvc.CreateRelationship(req);

                if (result != null)
                    relationship = result.PSRelationship;
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion error in CreateRelationship().", ex);
            }

            return relationship;
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

        #endregion

        #region Assembly Service Methods

        /// <summary>
        /// Creates and intialize a proxy of the Percussion service used for retriving
        /// slots and templates.
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
        public static assemblySOAP GetAssemblyService(string protocol, string host, string port, CookieContainer cookie, PSAuthenticationHeader authHeader)
        {
            assemblySOAP assemblySvc = new assemblySOAP();

            assemblySvc.Url = RewriteServiceUrl(assemblySvc.Url, protocol, host, port);
            assemblySvc.CookieContainer = cookie;
            assemblySvc.PSAuthenticationHeaderValue = authHeader;

            return assemblySvc;
        }

        public static PSAssemblyTemplate[] LoadAssemblyTemplates(assemblySOAP assemblySvc)
        {
            PSAssemblyTemplate[] templateData;

            try
            {
                LoadAssemblyTemplatesRequest req = new LoadAssemblyTemplatesRequest();
                templateData = assemblySvc.LoadAssemblyTemplates(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion error in LoadAssemblyTemplates", ex);
            }


            return templateData;
        }

        public static PSTemplateSlot[] LoadSlots(assemblySOAP assemblySvc)
        {
            PSTemplateSlot[] slotData;

            try
            {
                LoadSlotsRequest req = new LoadSlotsRequest();
                slotData = assemblySvc.LoadSlots(req);
            }
            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion error in LoadSlots", ex);
            }


            return slotData;
        }

        #endregion

        #region Security Service Methods

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
        /// Login to a Percussion sesession with the specified credentials and associated parameters.
        /// </summary>
        /// <param name="securitySvc">the proxy of the security service</param>
        /// <param name="user">The login user.</param>
        /// <param name="password">The login password.</param>
        /// <param name="community">The name of the Community into which to log the user</param>
        /// <param name="locale">The name of the Locale into which to log the user</param>
        /// <returns></returns>
        public static PSLogin Login(securitySOAP securitySvc, string user, string password, string community, string locale)
        {
            LoginRequest loginReq = new LoginRequest();
            PSLogin loginContext = null;

            try
            {
                loginReq.Username = user;
                loginReq.Password = password;
                loginReq.Community = community;
                loginReq.LocaleCode = locale;

                // Setting the authentication header to maintain Rhythmyx session
                LoginResponse loginResp = securitySvc.Login(loginReq);
                loginContext = loginResp.PSLogin;
                securitySvc.PSAuthenticationHeaderValue = new PSAuthenticationHeader();
                securitySvc.PSAuthenticationHeaderValue.Session = loginContext.sessionId;
            }

            catch (SoapException ex)
            {
                throw new CMSSoapException("Percussion Error in Login.", ex);
            }

            return loginContext;
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

        #endregion


        #region Utility Methods

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

        #endregion

    }
}
