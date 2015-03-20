using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Web.Services.Protocols;
using System.Text;
using System.Diagnostics;

using GKManagers.CMSManager.Configuration;
using NCI.WCM.CMSManager.PercussionWebSvc;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace NCI.WCM.CMSManager.CMS
{
    /// <summary>
    /// Delegate definition for determining an element's workflow state based on the list
    /// of transitions currently allowed.
    /// </summary>
    /// <param name="transitionNames">An array of strings representing the triggers for the
    /// transitions allowed from the current state.</param>
    /// <returns></returns>
    public delegate object WorkflowStateInfererDelegate(string[] transitionNames);

    /// <summary>
    /// This class is the sole means by which any code in the GateKeeper system may interact with Percussion.
    /// It manages the single login session used for all interations, and performs all needed operations.
    /// </summary>
    public class CMSController : IDisposable
    {
        #region Public Constants

        public const string TranslationRelationshipType = "Translation";

        #endregion


        #region Percussion Fields

        // These fields represent the interface to Percussion.  They are initialized by the
        // CMSController constructor. These fields are used by all CMSController methods which
        // need to communicate with the Percussion system.

        /*
         * Describes the current Percussion login session, initialized by login() in the constructor.
         */
        PSLogin _loginSessionContext;

        /**
         * The security service instance; used to perform operations defined in
         * the security services. It is initialized by login().
         */
        securitySOAP _securityService;

        /**
        * The content service instance; used to perform operations defined in
        * the content services. It is initialized by login().
        */
        contentSOAP _contentService;

        /**
         * The system service instance; used to perform operations defined in
         * the system service. It is initialized by login().
         */
        systemSOAP _systemService;

        /*
         * The assembly service instance; used to retrieve lists of slots
         * and templates. It is initialized by login().
         */
        assemblySOAP _assemblyService;


        #endregion

        #region CMSController Session Fields.

        // These fields are used to store information about the CMS.
        // In order to avoid stateful behavior, no fields are defined to maintain
        // the state of content items (or similar entities) between calls to
        // the controller's public methods.

        private Dictionary<string, PercussionGuid> communityCollection = new Dictionary<string, PercussionGuid>();

        #endregion

        #region CMSController Session Properties

        /// <summary>
        /// Allows the site root path to be overridden.  This value defaults to a value
        /// loaded from the siteRoot key in the Percussion configuration section.
        /// </summary>
        public string SiteRootPath { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the CMSController class.
        /// </summary>
        public CMSController()
            : this(null)
        {
            // Initialization logic is shared with and implemented by CMSController(string communityName).
        }

        /// <summary>
        /// Initializes a new instance of the CMSController class.
        /// </summary>
        public CMSController(string communityName)
        {
            // HACK: There should really be a mechanism for a single CMSController to address multiple communities
            // without having to switch on the fly.  (Maintain multiple logins?)

            // Percussion system login and any other needed intitialization goes here.
            // The login ID and password are loaded from the application's configuration file.
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

            if (percussionConfig == null)
                throw new CMSInitializationException("Unable to load Percussion Configuration section.");

            string community;
            if (string.IsNullOrEmpty(communityName))
                community = percussionConfig.ConnectionInfo.Community.Value;
            else
                community = communityName;

            string username = percussionConfig.ConnectionInfo.UserName.Value;
            string password = percussionConfig.ConnectionInfo.Password.Value;

            string host = percussionConfig.ConnectionInfo.Host.Value;
            string port = percussionConfig.ConnectionInfo.Port.Value;
            string protocol = percussionConfig.ConnectionInfo.Protocol.Value;

            int timeout = percussionConfig.ConnectionInfo.Timeout.Value;  //100000; // percussionConfig.Timeout;

            Login(username, password, community, host, port, protocol, timeout);
            SiteRootPath = percussionConfig.ConnectionInfo.SiteRootPath.Value;
        }


        #region Disposable Pattern Members

        ~CMSController()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Free managed resources.
            if (disposing)
            {
                //The _loginSessionContext may be null under certain circumstances, so
                //we must check.
                if (_loginSessionContext != null)
                    PSWSUtils.Logout(_securityService, _loginSessionContext.sessionId);

                _securityService = null;
                _contentService = null;
                _systemService = null;
                _assemblyService = null;
                _loginSessionContext = null;

                GC.SuppressFinalize(this);
            }
        }

        #endregion

        public TemplateNameManager TemplateNameManager
        {
            /*
             * At a glance, lazy-loading like this seems likely to load repeatedly. But the TemplateNameManager
             * property can only be accessed from an instance of CMSController. Because the constructor
             * calls Login, the _contentService can be used safely.  The only danger is if someone
             * tries using the property after CMController has been disposed, but that requires a high
             * degree of not knowing what you're doing.
             */
            get { return new TemplateNameManager(_assemblyService); }
        }

        public ContentTypeManager ContentTypeManager
        {
            get { return new ContentTypeManager(_contentService); }
        }

        public SlotManager SlotManager
        {
            get { return new SlotManager(_assemblyService, ContentTypeManager, TemplateNameManager); }
        }

        public FolderManager FolderManager
        {
            get { return new FolderManager(_contentService, _systemService); }
        }

        public Dictionary<string, PercussionGuid> Community
        {
            get { return communityCollection; }
        }

        /// <summary>
        /// Login to the Percussion session, set up services.
        /// </summary>
        private void Login(string username, string password, string community, string host, string port, string protocol, int timeout)
        {

            _securityService = PSWSUtils.GetSecurityService(protocol, host, port);
            _loginSessionContext = PSWSUtils.Login(_securityService, username, password, community, null);

            _contentService = PSWSUtils.GetContentService(protocol, host, port, _securityService.CookieContainer,
                _securityService.PSAuthenticationHeaderValue);
            _systemService = PSWSUtils.GetSystemService(protocol, host, port, _securityService.CookieContainer,
                _securityService.PSAuthenticationHeaderValue);
            _assemblyService = PSWSUtils.GetAssemblyService(protocol, host, port, _securityService.CookieContainer,
                _securityService.PSAuthenticationHeaderValue);

            _securityService.Timeout = timeout;
            _contentService.Timeout = timeout;
            _systemService.Timeout = timeout;
            _assemblyService.Timeout = timeout;

            foreach (PSCommunity item in _loginSessionContext.Communities)
            {
                communityCollection.Add(item.name, new PercussionGuid(item.id));
            }
        }

        /// <summary>
        /// Creates the content items in the list.
        /// </summary>
        /// <param name="contentItems">The content items list.</param>
        /// <returns>List of Id's for the items created</returns>
        public List<long> CreateContentItemList(List<ContentItemForCreating> contentItems)
        {
            return CreateContentItemList(contentItems, null);
        }

        /// <summary>
        /// Creates the content items in the list.
        /// </summary>
        /// <param name="contentItems">The content items list.</param>
        /// <returns>List of Id's for the items created</returns>
        public List<long> CreateContentItemList(List<ContentItemForCreating> contentItems,
            Action<string> errorHandler)
        {
            List<long> idList = new List<long>();

            try
            {
                foreach (ContentItemForCreating cmi in contentItems)
                {
                    {
                        long id = CreateItem(cmi.ContentType, cmi.Fields, cmi.ChildFieldList, cmi.TargetFolder, errorHandler);
                        idList.Add(id);
                    }
                }
            }
            catch (Exception ex)
            {
                // If an error occurs, rollback item creation.
                DeleteItemList(idList.ToArray());
                throw;
            }

            return idList;
        }

        /// <summary>
        /// Creates a single item in the CMS.
        /// </summary>
        /// <param name="contentType">Type of the content like druginfosummary,cancerinfosummary.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="childFieldList">Collection of ChildFieldSet objects.  May be null if the content
        /// item has no child fieldsets.</param>
        /// <param name="targetFolder">The target folder in percussion.</param>
        /// <param name="invalidFieldnameHandler">Method accepting a single string parameter to call
        /// when a fieldname is not valid.  If invalidFieldnameHandler is null, invalid fieldnames
        /// will throw CMSInvalidFieldnameException with the invalid name.</param>
        /// <returns>Id for the the created item</returns>
        public long CreateItem(string contentType, Dictionary<string, string> newFieldValues, IEnumerable<ChildFieldSet> childFieldList, string targetFolder, Action<string> invalidFieldnameHandler)
        {
            // TODO: Merge this with the version of CreateContentItemList which accepts invalidFieldnameHandler.


            PSItem item = PSWSUtils.CreateItem(_contentService, contentType);

            // Attach item to a folder
            PSFolder folder = GuaranteeFolder(targetFolder);

            PSItemFolders psf = new PSItemFolders();
            psf.path = folder.path;

            item.Folders = new PSItemFolders[] { psf };

            if (invalidFieldnameHandler == null)
            {
                // If no error handler supplied, then catch invalid field names here.
                List<string> invalidFields = new List<string>();
                MergeFieldValues(item.Fields, newFieldValues, fieldName => { invalidFields.Add(fieldName); });
                if (invalidFields.Count != 0)
                {
                    throw new
                        CMSInvalidFieldnameException(string.Format("Invalid field names specified: {0}",
                        string.Join(", ", invalidFields.ToArray())));
                }
            }
            else
            {
                // Pass the user-supplied handler
                MergeFieldValues(item.Fields, newFieldValues, invalidFieldnameHandler);
            }

            long id = PSWSUtils.SaveItem(_contentService, item);

            // The base content item must be created before any child fields may be added.
            if (childFieldList != null)
            {
                CreateChildItems(new PercussionGuid(id), childFieldList, invalidFieldnameHandler);
            }

            PSWSUtils.CheckinItem(_contentService, id);
            return id;

        }


        private void CreateChildItems(PercussionGuid parentItemID,
            IEnumerable<ChildFieldSet> childFieldList,
            Action<string> invalidFieldnameHandler)
        {
            foreach (ChildFieldSet childField in childFieldList)
            {
                int entryCount = childField.Fields.Count;

                if (entryCount > 0)
                {
                    PSChildEntry[] itemChildren =
                        PSWSUtils.CreateChildEntries(_contentService, parentItemID.ID, childField.Name, entryCount);
                    for (int i = 0; i < entryCount; i++)
                    {
                        MergeFieldValues(itemChildren[i].PSField, childField.Fields[i], invalidFieldnameHandler);
                    }
                    PSWSUtils.SaveChildEntries(_contentService, parentItemID.ID, childField.Name, itemChildren);
                }
            }
        }


        public void CheckInItems(PercussionGuid[] itemIDList)
        {
            int length = itemIDList.Length;
            long[] rawIDs = new long[length];
            for (int i = 0; i < length; i++)
            {
                rawIDs[i] = itemIDList[i].ID;
            }

            PSWSUtils.CheckInItemList(_contentService, rawIDs);
        }


        /// <summary>
        /// Updates the content item list.
        /// </summary>
        /// <param name="contentItems">A collection of UpdateContentItem.</param>
        /// <returns>List of IDs of all the content items updated </returns>
        public List<long> UpdateContentItemList(List<ContentItemForUpdating> contentItems)
        {
            return UpdateContentItemList(contentItems, null, null, null);
        }

        /// <summary>
        /// Updates the content item list.
        /// </summary>
        /// <param name="contentItems">A collection of UpdateContentItem.</param>
        /// <param name="replacementPathCollection">Updated collection of paths the item should be appear in.
        /// This value replaces the existing collection of locations for the items.
        /// If null, the items' location is not updated.</param>
        /// <param name="additionalPathCollection">Collection of additional paths where the items should appear.
        /// Specify null for no additional paths.</param>
        /// <returns>
        /// List of IDs of all the content items updated
        /// </returns>
        public List<long> UpdateContentItemList(List<ContentItemForUpdating> contentItems,
            List<string> replacementPathCollection,
            List<string>additionalPathCollection)
        {
            return UpdateContentItemList(contentItems, null, replacementPathCollection, additionalPathCollection);
        }

        /// <summary>
        /// Updates the content item list.
        /// </summary>
        /// <param name="contentItems">A collection of UpdateContentItem.</param>
        /// <param name="invalidFieldnameHandler">Method accepting a single string parameter to call
        /// when a fieldname is not valid.  If invalidFieldnameHandler is null, invalid fieldnames
        /// will throw CMSInvalidFieldnameException with the invalid name.</param>
        /// <returns>List of IDs of all the content items updated </returns>
        public List<long> UpdateContentItemList(List<ContentItemForUpdating> contentItems,
            Action<string> invalidFieldnameHandler)
        {
            return UpdateContentItemList(contentItems, invalidFieldnameHandler, null, null);
        }

        /// <summary>
        /// Updates the content item list.
        /// </summary>
        /// <param name="contentItems">A collection of UpdateContentItem.</param>
        /// <param name="invalidFieldnameHandler">Method accepting a single string parameter to call
        /// when a fieldname is not valid.  If invalidFieldnameHandler is null, invalid fieldnames
        /// will throw CMSInvalidFieldnameException with the invalid name.</param>
        /// <param name="replacementPathCollection">Updated collection of paths the item should be appear in.
        /// This value replaces the existing collection of locations for the items.
        /// If null, the items' location is not updated.</param>
        /// <param name="additionalPathCollection">Collection of additional paths where the items should appear.
        /// Specify null for no additional paths.</param>
        /// <returns>
        /// List of IDs of all the content items updated
        /// </returns>
        public List<long> UpdateContentItemList(List<ContentItemForUpdating> contentItems,
            Action<string> invalidFieldnameHandler,
            List<string> replacementPathCollection,
            List<string> additionalPathCollection)
        {
            List<long> idUpdList = new List<long>();
            long idUpd;
            foreach (ContentItemForUpdating cmi in contentItems)
            {
                idUpd = UpdateItem(cmi.ID, cmi.Fields, invalidFieldnameHandler, replacementPathCollection, additionalPathCollection);
                idUpdList.Add(idUpd);
            }
            return idUpdList;
        }

        /// <summary>
        /// Updates a single content item.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="newFieldValues">The new field values.</param>
        /// <param name="invalidFieldnameHandler">Method accepting a single string parameter to call
        /// when a fieldname is not valid.  If invalidFieldnameHandler is null, invalid fieldnames
        /// will throw CMSInvalidFieldnameException with the invalid name.</param>
        /// <param name="replacementPathCollection">Updated collection of paths the item should be appear in.
        /// This value replaces the existing collection of locations for the items.
        /// If null, the items' location is not updated.</param>
        /// <param name="additionalPathCollection">Collection of additional paths where the items should appear.
        /// Specify null for no additional paths.</param>
        /// <returns>
        /// The ID of the content time which has been updated.
        /// </returns>
        private long UpdateItem(long id,
            Dictionary<string, string> newFieldValues,
            Action<string> invalidFieldnameHandler,
            List<string> replacementPathCollection,
            List<string> additionalPathCollection)
        {
            // TODO: Merge this with the version of UpdateContentItemList which accepts invalidFieldnameHandler.

            PSItemStatus[] checkOutStatus = PSWSUtils.PrepareForEdit(_contentService, new long[] { id });

            PSItem[] returnList = PSWSUtils.LoadItems(_contentService, new long[] { id });
            PSItem item = returnList[0];

            if (invalidFieldnameHandler == null)
            {
                // If no error handler supplied, then catch invalid field names here.
                List<string> invalidFields = new List<string>();
                MergeFieldValues(item.Fields, newFieldValues, fieldName => { invalidFields.Add(fieldName); });
                if (invalidFields.Count != 0)
                {
                    // Undo checkout before throwing error.
                    PSWSUtils.ReleaseFromEdit(_contentService, checkOutStatus);

                    throw new
                        CMSInvalidFieldnameException(string.Format("Invalid field names specified: {0}",
                           string.Join(", ", invalidFields.ToArray())));
                }
            }
            else
            {
                // Pass the user-supplied handler
                MergeFieldValues(item.Fields, newFieldValues, invalidFieldnameHandler);
            }

            // TODO: Add logic to update child fields.


            // Replace the item's list of paths.
            if (replacementPathCollection != null)
            {
                item.Folders = PSItemFoldersBuilder.Build(SiteRootPath, replacementPathCollection);
            }

            // Update the item's list of paths.
            if (additionalPathCollection != null)
            {
                item.Folders = PSItemFoldersBuilder.Build(SiteRootPath, item.Folders, additionalPathCollection);
            }

            long idUpd = PSWSUtils.SaveItem(_contentService, item);
            PSWSUtils.ReleaseFromEdit(_contentService, checkOutStatus);
            return idUpd;
        }

        /// <summary>
        /// Merge a collection of field name/values pairs into the list of fields
        /// contained in a content item.
        /// </summary>
        /// <param name="item">The item to store the values in.</param>
        /// <param name="fieldValueList">Collection of field name/values pairs.</param>
        /// <param name="invalidFieldnameHandler">Method accepting a single string parameter to call
        /// when a fieldname is not valid.</param>
        /// <remarks>If any of the fields named in fieldValueList don't exist in item.Fields, the field
        /// name is reported via invalidFieldnameHandler. If invalidFieldnameHandler is null, invalid
        /// field names result in CMSInvalidFieldnameException being thrown with the invalid name.</remarks>
        private void MergeFieldValues(PSField[] itemFieldSet,
            Dictionary<string, string> fieldValueList,
            Action<string> invalidFieldnameHandler)
        {
            // If the named field doesn't exist in item.Fields, check for the presence of
            // invalidFieldnameHandler.  If invalidFieldnameHandler is non-null, call it with
            // the field name.  Otherwise, throw CMSInvalidFieldnameException.

            // Scan through the item's fields collection and look for field names.
            foreach (KeyValuePair<string, string> kvp in fieldValueList)
            {
                string targetName = kvp.Key;
                string fieldValue = kvp.Value;

                PSField itemField = Array.Find(itemFieldSet, field => { return field.name == targetName; });
                if (itemField != null)
                {
                    PSFieldValue value = new PSFieldValue();
                    value.RawData = fieldValue;
                    if (targetName == "date_display_mode")
                    {
                        PSFieldValue v1 = new PSFieldValue();
                        PSFieldValue v2 = new PSFieldValue();
                        PSFieldValue v3 = new PSFieldValue();
                        switch (fieldValue)
                        {
                            case "3":
                                v1.RawData = "1";
                                v2.RawData = "2";
                                itemField.PSFieldValue = new PSFieldValue[] { v1, v2 };
                                break;
                            case "5":
                                v1.RawData = "1";
                                v2.RawData = "4";
                                itemField.PSFieldValue = new PSFieldValue[] { v1, v2 };
                                break;
                            case "6":
                                v1.RawData = "2";
                                v2.RawData = "4";
                                itemField.PSFieldValue = new PSFieldValue[] { v1, v2 };
                                break;
                            case "7":
                                v1.RawData = "1";
                                v2.RawData = "2";
                                v3.RawData = "4";
                                itemField.PSFieldValue = new PSFieldValue[] { v1, v2, v3 };
                                break;
                            default:
                                itemField.PSFieldValue = new PSFieldValue[] { value };
                                break;
                        }
                    }
                    else
                        itemField.PSFieldValue = new PSFieldValue[] { value };
                }
                else
                {
                    // If the named field doesn't exist in item.Fields, either handle the error, or throw CMSInvalidFieldnameException.
                    if (invalidFieldnameHandler != null)
                        invalidFieldnameHandler(targetName);
                    else
                        throw new CMSInvalidFieldnameException(targetName);
                }
            }
        }


        /// <summary>
        /// Deletes the specified content items.
        /// </summary>
        /// <param name="itemID">ID of the content item to be deleted.</param>
        public void DeleteItem(long itemID)
        {
            DeleteItemList(new long[] { itemID });
        }

        /// <summary>
        /// Deletes the specified content items.
        /// </summary>
        /// <param name="itemList">Array of content item IDs to be deleted.</param>
        public void DeleteItemList(PercussionGuid[] itemList)
        {
            long[] rawIDs = Array.ConvertAll(itemList, item => (long)item.ID);
            if (rawIDs.Length > 0)
            {
                DeleteItemList(rawIDs);
            }
        }

        /// <summary>
        /// Deletes the specified content items.
        /// </summary>
        /// <param name="itemID">ID of the content item to be deleted.</param>
        private void DeleteItemList(long[] itemList)
        {
            PSWSUtils.DeleteItem(_contentService, itemList);
        }

        /// <summary>
        /// Moves a colletion of content items from one folder to another.
        /// </summary>
        /// <param name="sourcePath">The source folder path.</param>
        /// <param name="targetPath">The target folder path.</param>
        /// <param name="idcoll">The collection of tems to move.</param>
        public void MoveContentItemFolder(string sourcePath, string targetPath, PercussionGuid[] idcoll)
        {
            long[] arr = Array.ConvertAll(idcoll, id => (long)id.ID);
            MoveContentItemFolder(sourcePath, targetPath, arr);
        }

        /// <summary>
        /// Moves a colletion of content items from one folder to another.
        /// </summary>
        /// <param name="sourcePath">The source folder path.</param>
        /// <param name="targetPath">The target folder path.</param>
        /// <param name="idcoll">The collection of tems to move.</param>
        public void MoveContentItemFolder(string sourcePath, string targetPath, long[] idcoll)
        {
            sourcePath = SiteRootPath + sourcePath;
            targetPath = SiteRootPath + targetPath;

            PSWSUtils.MoveFolderChildren(_contentService, targetPath, sourcePath, idcoll);
        }

        /// <summary>
        /// GuaranteeFolder that a folder exists, creating it if it doesn't
        /// already exist.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>A PSFolder object containing details of the folder.</returns>
        /// <remarks>The folder path argument will have the path to the site root
        /// prepended before the attempt is made to create it.</remarks>
        [Obsolete("Use GuaranteeFolder(string folderPath, NavonAction navonAction) instead.")]
        public PSFolder GuaranteeFolder(string folderPath)
        {
            return FolderManager.GuaranteeFolder(SiteRootPath + folderPath, FolderManager.NavonAction.None);
        }

        /// <summary>
        /// GuaranteeFolder that a folder exists, creating it if it doesn't
        /// already exist.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>A PSFolder object containing details of the folder.</returns>
        /// <remarks>The folder path argument will have the path to the site root
        /// prepended before the attempt is made to create it.</remarks>
        public PSFolder GuaranteeFolder(string folderPath, FolderManager.NavonAction navonAction)
        {
            return FolderManager.GuaranteeFolder(SiteRootPath + folderPath, navonAction);
        }

        /// <summary>
        /// GuaranteeFolder that a folder exists, creating it if it doesn't
        /// already exist.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="makeNavonPublic">Set true to immediately make the Navon public (default behavior),
        /// or false to leave the Navon in its default state.</param>
        /// <returns>A PSFolder object containing details of the folder.</returns>
        /// <remarks>The folder path argument will have the path to the site root
        /// prepended before the attempt is made to create it.</remarks>
        public PSFolder GuaranteeFolder(string folderPath, bool makeNavonPublic)
        {
            return FolderManager.GuaranteeFolder(SiteRootPath + folderPath, makeNavonPublic);
        }

        /// <summary>
        /// Delete folder from the CMS.
        /// </summary>
        /// <param name="folderPath">Path of the folder to be deleted.</param>
        /// <param name="purge">if set to <c>true</c> [purge].</param>
        public void DeleteFolder(string folderPath, bool purge)
        {
            PSFolder folderInfo = FolderManager.GetExistingFolder(SiteRootPath + folderPath);
            DeleteFolders(new PSFolder[] { folderInfo }, purge);
        }

        /// <summary>
        /// Deletes a collection of folders. Any items contained in the folder are
        /// removed from the content tree, but are not purged.
        /// </summary>
        /// <param name="folderInfo"></param>
        public void DeleteFolders(PSFolder[] folderInfo)
        {
            DeleteFolders(folderInfo, false);
        }

        /// <summary>
        /// Deletes a collection of folders. Any items contained in the folder are
        /// removed from the content tree, but are not purged.
        /// </summary>
        /// <param name="folderInfo">The folder info.</param>
        /// <param name="purge">if set to <c>true</c> [purge].</param>
        public void DeleteFolders(PSFolder[] folderInfo, bool purge)
        {
            long[] folderIDs = Array.ConvertAll(folderInfo, detail => detail.id);
            PSWSUtils.DeleteFolders(_contentService, folderIDs, purge);
        }


        /// <summary>
        /// Finds the content items contained in a folder.
        /// </summary>
        /// <param name="path">Path to the folder being investigated.</param>
        /// <returns>A collection of zero or more objects describing the folder's contents.</returns>
        public PSItemSummary[] FindFolderChildren(string path)
        {
            return PSWSUtils.FindFolderChildren(_contentService, SiteRootPath + path);
        }

        /// <summary>
        /// Checks whether a CMS folder path exists.
        /// </summary>
        /// <param name="path">A site-relative folder path. (/CancerTopics, not //sites/CancerGov/CancerTopics)</param>
        /// <returns>True if the folder exists, false otherwise.</returns>
        public bool FolderExists(string path)
        {
            return PSWSUtils.FolderExists(_contentService, SiteRootPath + path);
        }

        /// <summary>
        /// Retrieves the shared workflow state of a list of content items.
        /// </summary>
        /// <param name="itemIDs">An array of content item IDs.</param>
        /// <param name="inferState">Delegate for a method which is able to determine
        /// a state name from the list of transitions it makes available.</param>
        /// <returns></returns>
        /// <remarks>All items must be maintained in the same state.</remarks>
        public object GetWorkflowState(long[] itemIDs, WorkflowStateInfererDelegate inferState)
        {
            string[] transitionNames = PSWSUtils.GetTransitions(_systemService, itemIDs);
            return inferState(transitionNames);
        }

        /// <summary>
        /// Moves the designated content items to another state in the workflow by
        /// performing the named transition.
        /// </summary>
        /// <param name="idList">A list of content items.</param>
        /// <param name="triggerName">The unique trigger name associated with a
        /// workflow transition.</param>
        /// <remarks>All content items must belong the same workflow and be
        /// in the same state.</remarks>
        public void PerformWorkflowTransition(long[] idList, string triggerName)
        {
            PSWSUtils.TransitionItems(_systemService, idList, triggerName);
        }

        /// <summary>
        /// Moves the designated content items to another state in the workflow by
        /// performing the named transition.
        /// </summary>
        /// <param name="idList">A list of content items.</param>
        /// <param name="triggerName">The unique trigger name associated with a
        /// workflow transition.</param>
        /// <remarks>All content items must belong the same workflow and be
        /// in the same state.</remarks>
        public void PerformWorkflowTransition(PercussionGuid[] idList, string triggerName)
        {
            long[] idLongVals = Array.ConvertAll(idList, id => (long)id.ID);
            PerformWorkflowTransition(idLongVals, triggerName);
        }


        /// <summary>
        /// Retrieves a list of content items which own relationships to the content
        /// item identified by itemID.
        /// </summary>
        /// <param name="itemID">The ID of a content item which is to be examined
        /// for incoming relationships.</param>
        /// <returns>An array of PSItem objects defining content items which have
        /// relationships to the item identified by itemID. If no items have relationships,
        /// the array will be empty, but is never null.</returns>
        public PSItem[] LoadLinkingContentItems(long itemID)
        {
            PSItem[] returnList = new PSItem[] { };

            // Check for any relationships.
            PSAaRelationshipFilter filter = new PSAaRelationshipFilter();
            filter.Dependent = new long[1] { itemID };

            // Only return relationships for the most recent revision.
            filter.limitToEditOrCurrentOwnerRevision = true;

            PSAaRelationship[] relationships = PSWSUtils.FindRelationships(_contentService, filter);

            // If incoming relationships exist, load the relevant content items.
            if (relationships.Length > 0)
            {
                int relCount = relationships.Length;
                long[] ownerIDs = new long[relCount];
                for (int i = 0; i < relationships.Length; i++)
                {
                    ownerIDs[i] = relationships[i].ownerId;
                }

                returnList = PSWSUtils.LoadItems(_contentService, ownerIDs);
            }

            return returnList;
        }


        /// <summary>
        /// Finds the active assembly relationships which a collection of content items depend on.
        /// </summary>
        /// <param name="IDList">An array of PercusionGuid objects to be checked
        /// for incoming active assembly relationships. Required.</param>
        /// <returns>An array of zero or more PSAaRelationship objects having one or move
        /// items from IDList as a dependent.</returns>
        public PSAaRelationship[] FindIncomingActiveAssemblyRelationships(PercussionGuid[] IDList)
        {
            return FindIncomingActiveAssemblyRelationships(IDList, null, null);
        }

        /// <summary>
        /// Finds the active assembly relationships which a collection of content items depend on.
        /// </summary>
        /// <param name="dependentIDList">An array of PercusionGuid objects to be checked
        /// for incoming active assembly relationships. Required.</param>
        /// <param name="slotName">If specified, restricts the result set to relationships which
        /// use the named slot.</param>
        /// <param name="templateName">If specified, restricts the result set to relationships which
        /// use the named snippet template.</param>
        /// <returns>An array of zero or more PSAaRelationship objects having one or move
        /// items from IDList as a dependent.</returns>
        public PSAaRelationship[] FindIncomingActiveAssemblyRelationships(PercussionGuid[] dependentIDList, string slotName, string templateName)
        {
            long[] idArray = Array.ConvertAll(dependentIDList, guid => (long)guid.ID);
            return FindIncomingActiveAssemblyRelationships(idArray, slotName, templateName);
        }

        /// <summary>
        /// Finds the active assembly relationships which a collection of content items depend on.
        /// </summary>
        /// <param name="IDList">An array of long values identifiying Percussion content items to be checked
        /// for incoming active assembly relationships. Required.</param>
        /// <param name="slotName">If specified, restricts the result set to relationships which
        /// use the named slot.</param>
        /// <param name="templateName">If specified, restricts the result set to relationships which
        /// use the named snippet template.</param>
        /// <returns>An array of zero or more PSAaRelationship objects having one or move
        /// items from IDList as a dependent.</returns>
        private PSAaRelationship[] FindIncomingActiveAssemblyRelationships(long[] IDList, string slotName, string templateName)
        {
            PSAaRelationshipFilter filter = new PSAaRelationshipFilter();
            filter.Dependent = IDList;

            // Only return relationships for the most recent revision.
            filter.limitToEditOrCurrentOwnerRevision = true;

            if (!string.IsNullOrEmpty(slotName))
            {
                filter.slot = slotName;
            }

            if (!string.IsNullOrEmpty(templateName))
            {
                filter.template = templateName;
            }

            return PSWSUtils.FindRelationships(_contentService, filter);
        }

        /// <summary>
        /// Retrieves a list of content items identified by the values in itemIDList.
        /// </summary>
        /// <param name="itemIDList">An array of content item ID values.</param>
        /// <returns>An array of content items in the same order as the values
        /// in itemIDList</returns>
        public PSItem[] LoadContentItems(PercussionGuid[] itemIDList)
        {
            long[] idList = Array.ConvertAll(itemIDList, item => (long)item.ID);
            return PSWSUtils.LoadItems(_contentService, idList);
        }

        /// <summary>
        /// Retrieves a list of content items identified by the values in itemIDList.
        /// </summary>
        /// <param name="itemIDList">An array of content item ID values.</param>
        /// <returns>An array of content items in the same order as the values
        /// in itemIDList</returns>
        public PSItem[] LoadContentItems(long[] itemIDList)
        {
            return PSWSUtils.LoadItems(_contentService, itemIDList);
        }

        public PercussionGuid[] SaveContentItems(PSItem[] itemList)
        {
            long[] returnIDs = PSWSUtils.SaveItem(_contentService, itemList);
            return Array.ConvertAll(returnIDs, id => new PercussionGuid(id));
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
        public PSAaRelationship[] CreateActiveAssemblyRelationships(long parentItemID, long[] childItemIDList, string slotName, string snippetTemplateName)
        {
            PSAaRelationship[] relationships;

            PSItemStatus[] parentCheckoutStatus = PSWSUtils.PrepareForEdit(_contentService, new long[] { parentItemID });
            if (!parentCheckoutStatus[0].didCheckout)
                throw new CMSOperationalException(string.Format("Unable to perform a checkout for item with CMS content item {0}.", parentItemID));

            relationships = PSWSUtils.CreateActiveAssemblyRelationships(_contentService, parentItemID, childItemIDList, slotName, snippetTemplateName);

            PSWSUtils.ReleaseFromEdit(_contentService, parentCheckoutStatus);

            return relationships;
        }

        public PSAaRelationship[] CreateActiveAssemblyRelationships(long parentItemID, long[] childItemIDList, string slotName, string snippetTemplateName, int index)
        {
            PSAaRelationship[] relationships;

            PSItemStatus[] parentCheckoutStatus = PSWSUtils.PrepareForEdit(_contentService, new long[] { parentItemID });
            if (!parentCheckoutStatus[0].didCheckout)
                throw new CMSOperationalException(string.Format("Unable to perform a checkout for item with CMS content item {0}.", parentItemID));

            relationships = PSWSUtils.CreateActiveAssemblyRelationships(_contentService, parentItemID, childItemIDList, slotName, snippetTemplateName, index);

            PSWSUtils.ReleaseFromEdit(_contentService, parentCheckoutStatus);

            return relationships;
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
        public PSAaRelationship[] CreateActiveAssemblyRelationships(PercussionGuid parentItemID, PercussionGuid[] childItemIDList, string slotName, string snippetTemplateName)
        {
            PSAaRelationship[] relationships;

            long parentItemIDAsLong = parentItemID.ID;
            long[] childItemIDListAsLong = Array.ConvertAll(childItemIDList, childId => (long)childId.ID);

            PSItemStatus[] parentCheckoutStatus = PSWSUtils.PrepareForEdit(_contentService, new long[] { parentItemIDAsLong });
            if (!parentCheckoutStatus[0].didCheckout)
                throw new CMSOperationalException(string.Format("Unable to perform a checkout for item with CMS content item {0}.", parentItemID));

            relationships = PSWSUtils.CreateActiveAssemblyRelationships(_contentService, parentItemIDAsLong, childItemIDListAsLong, slotName, snippetTemplateName);

            PSWSUtils.ReleaseFromEdit(_contentService, parentCheckoutStatus);

            return relationships;
        }

        public void DeleteActiveAssemblyRelationships(PSAaRelationship[] relationships, bool alreadyInEditingState, PercussionGuid[] idList, string slotName, string templateName)
        {
            PSItemStatus[] parentCheckoutStatus = new PSItemStatus[] { };
            
            if (!alreadyInEditingState)
            {
                //edit the relationship and checkout the parent
                long[] parentItems = (from relationship in relationships
                                      select (long)PercussionGuid.GetID(relationship.ownerId)).Distinct().ToArray();
                parentCheckoutStatus = PSWSUtils.PrepareForEdit(_contentService, parentItems);
            }

            //do another find of the same relationship 
            //so that the checked out item will be returned and hence will be the the 
            //edit revision 
            PSAaRelationship[] incomingRelationship = FindIncomingActiveAssemblyRelationships(idList, slotName, templateName);

            if (incomingRelationship != null && incomingRelationship.Length > 0)
            {
                long[] relationshipIDs = Array.ConvertAll(incomingRelationship, relationship => (long)((ulong)relationship.id | 0xffffff0000000000));
                PSWSUtils.DeleteActiveAssemblyRelationships(_contentService, relationshipIDs);
            }
            
            if (!alreadyInEditingState)
            {
                PSWSUtils.ReleaseFromEdit(_contentService, parentCheckoutStatus);
            }
        }

        public PSRelationship CreateRelationship(long parentItemID, long childItemID, string relationshipType)
        {
            PSRelationship relationship = null;

            PSItemStatus[] parentCheckoutStatus = PSWSUtils.PrepareForEdit(_contentService, new long[] { parentItemID });
            if (!parentCheckoutStatus[0].didCheckout)
                throw new CMSOperationalException(string.Format("Unable to perform a checkout for item with CMS content item {0}.", parentItemID));

            relationship = PSWSUtils.CreateRelationship(_systemService, parentItemID, childItemID, relationshipType);

            PSWSUtils.ReleaseFromEdit(_contentService, parentCheckoutStatus);

            return relationship;
        }

        /// <summary>
        /// Finds the first path in the current site which a content item resides in.
        /// </summary>
        /// <param name="item">A content item</param>
        /// <returns>The path relative to the current site's base, or null if no path is available.</returns>
        public string GetPathInSite(PSItem item)
        {
            return GetPathInSite(item, SiteRootPath);
        }

        /// <summary>
        /// Finds the first path, under the specified site path, which a content item resides in.
        /// </summary>
        /// <param name="item">A content item</param>
        /// <param name="siteBasePath">The site base path.</param>
        /// <returns>
        /// The path relative to the current site's base, or null if no path is available.
        /// </returns>
        public string GetPathInSite(PSItem item, string siteBasePath)
        {
            if (item == null || item.Folders == null || item.Folders.Length == 0)
                return null;

            PSItemFolders pathFolder = Array.Find(item.Folders, folder =>{
                return (!string.IsNullOrEmpty(folder.path)
                    && folder.path.StartsWith(siteBasePath));
            });
            if (pathFolder == null)
                return null;

            string path = pathFolder.path;
            if (path.StartsWith(siteBasePath, StringComparison.InvariantCultureIgnoreCase))
                return path.Substring(siteBasePath.Length);
            else
                return path;
        }

        public enum CMSPublishingTarget
        {
            CDRStaging = 0, // For completeness, not really useful.
            CDRPreview = 1,
            CDRLive = 2,
            CDRLiveFast = 3
        }


        public void StartPublishing(CMSPublishingTarget target)
        {
            // Preview and Live are the only CMS publishing editions we would run.
            if (target == CMSPublishingTarget.CDRPreview || target == CMSPublishingTarget.CDRLive || target == CMSPublishingTarget.CDRLiveFast)
            {
                // Server communication information.
                PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
                string protocol = percussionConfig.ConnectionInfo.Protocol.Value;
                string host = percussionConfig.ConnectionInfo.Host.Value;
                string port = percussionConfig.ConnectionInfo.Port.Value;
                string publishingUrlFormat =
                    "{0}://{1}:{2}/Rhythmyx/sys_pubHandler/publisher.htm?editionid={3}&PUBAction=publish";

                string[] editionList = GetPublishingEditionList(target);

                Array.ForEach(editionList, edition =>
                {
                    string activationUrl = string.Format(publishingUrlFormat, protocol, host, port, edition);
                    WebRequest request = WebRequest.Create(activationUrl);
                    WebResponse response = request.GetResponse();            
                    
                });

            }
        }

        private string[] GetPublishingEditionList(CMSPublishingTarget target)
        {
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            string[] editionList;
            string textValue;

            if (target == CMSPublishingTarget.CDRPreview)
                textValue = percussionConfig.PreviewRepublishEditionList.Value.Trim();
            else if (target == CMSPublishingTarget.CDRLive)
                textValue = percussionConfig.LiveRepublishEditionList.Value.Trim();
            else
                textValue = percussionConfig.LiveFastRepublishEditionList.Value.Trim();

            if (!string.IsNullOrEmpty(textValue))
                editionList = textValue.Split(new char[] { ',' });
            else
                editionList = new string[] { };

            return editionList;
        }


        /// <summary>
        /// Peforms a search of the CMS repository for content items via a the CMS database search
        /// engine (as opposed to the full text search engine).
        /// Search criteria must include a content type, and may optionally include a list of
        /// field/values pairs.
        /// </summary>
        /// <param name="contentType">String naming the content type for limiting the search.</param>
        /// <param name="fieldCriteria">Optional list of name/value pairs identifying the fields
        /// and values to search for</param>
        /// <returns>An array containing zero or more content item ID values.</returns>
        public PercussionGuid[] SearchForContentItems(string contentType, Dictionary<string, string> fieldCriteria)
        {
            return SearchForContentItems(contentType, null, fieldCriteria);
        }

        /// <summary>
        /// Peforms a search of the CMS repository for content items via a the CMS database search
        /// engine (as opposed to the full text search engine).
        /// Search criteria must include a content type, and may optionally include a list of
        /// field/values pairs.
        /// </summary>
        /// <param name="contentType">String naming the content type for limiting the search.</param>
        /// <param name="path">Site sub path in which to search for content items.
        /// (Must begin with /, must not include the //Sites/sitename component.)</param>
        /// <param name="fieldCriteria">Optional list of name/value pairs identifying the fields
        /// and values to search for</param>
        /// <returns>An array containing zero or more content item ID values.  The array may
        /// be empty, but is never null.</returns>
        public PercussionGuid[] SearchForContentItems(string contentType, string path, Dictionary<string, string> fieldCriteria)
        {
            return SearchForContentItems(contentType, null, path, fieldCriteria);
        }

        /// <summary>
        /// Peforms a search of the CMS repository for content items via a the CMS database search
        /// engine (as opposed to the full text search engine).
        /// Search criteria must include a content type, and may optionally include a list of
        /// field/values pairs.
        /// </summary>
        /// <param name="contentType">String naming the content type for limiting the search.</param>
        /// <param name="siteBasePath">Path to the site's base folder. Use null for current/default site.</param>
        /// <param name="path">Site sub path in which to search for content items.
        /// (Must begin with /, must not include the //Sites/sitename component.)</param>
        /// <param name="fieldCriteria">Optional list of name/value pairs identifying the fields
        /// and values to search for</param>
        /// <returns>
        /// An array containing zero or more content item ID values.  The array may
        /// be empty, but is never null.
        /// </returns>
        public PercussionGuid[] SearchForContentItems(string contentType, string siteBasePath, string path, Dictionary<string, string> fieldCriteria)
        {
            PercussionGuid[] contentIdList;

            string searchPath;
            string siteBase;

            // Allow override of default site root.
            if (string.IsNullOrEmpty(siteBasePath))
                siteBase = SiteRootPath;
            else
                siteBase = siteBasePath;

            // Allow path within site to be specified.
            if (string.IsNullOrEmpty(path))
                searchPath = siteBase;
            else
                searchPath = siteBase + path;


            PSSearchResults[] searchResults = PSWSUtils.FindItemByFieldValues(_contentService, contentType, searchPath, fieldCriteria);
            contentIdList = new PercussionGuid[searchResults.Length];
            for (int i = 0; i < searchResults.Length; i++)
            {
                contentIdList[i] = new PercussionGuid(searchResults[i].id);
            }

            return contentIdList;
        }

        /// <summary>
        /// Searches for items stored in a specific slot.
        /// </summary>
        /// <param name="owner">ID of the relationship owner.</param>
        /// <param name="slotname">Name of the slot to look in.</param>
        /// <returns>Array of PercussionGuid objects which reside in the slot.</returns>
        public PercussionGuid[] SearchForItemsInSlot(PercussionGuid owner, string slotname)
        {
            PercussionGuid[] returnList = new PercussionGuid[] { };

            // Check for any relationships.
            PSAaRelationshipFilter filter = new PSAaRelationshipFilter();

            // Only return relationships for the most recent revision.
            filter.limitToEditOrCurrentOwnerRevision = true;

            // Was an owner specified?
            if (owner != null)
            {
                filter.Owner = owner.ID;
            }

            // Slot name if specified
            if (!string.IsNullOrEmpty(slotname))
            {
                filter.slot = slotname;
            }

            PSAaRelationship[] relationships = PSWSUtils.FindRelationships(_contentService, filter);

            // If relationships exist, load the relevant content items.
            if (relationships.Length > 0)
            {
                int relCount = relationships.Length;
                returnList = new PercussionGuid[relCount];
                for (int i = 0; i < relCount; i++)
                {
                    returnList[i] = new PercussionGuid(relationships[i].dependentId);
                }
            }

            return returnList;
        }

        public PercussionGuid[] SearchForContentItemsByDependent(PercussionGuid dependent, string slotname, string templateName)
        {
            PercussionGuid[] returnList = new PercussionGuid[] { };

            // Check for any relationships.
            PSAaRelationshipFilter filter = new PSAaRelationshipFilter();

            // Only return relationships for the most recent revision.
            filter.limitToEditOrCurrentOwnerRevision = true;

            if (dependent != null)
            {
                filter.Dependent = new long[] { dependent.ID };
            }
            
            if (!string.IsNullOrEmpty(slotname))
            {
                filter.slot = slotname;
            }

            if (!string.IsNullOrEmpty(templateName))
                filter.template = templateName;
            
            PSAaRelationship[] relationships = PSWSUtils.FindRelationships(_contentService, filter);

            if (relationships.Length > 0)
            {
                int relCount = relationships.Length;
                returnList = new PercussionGuid[relCount];
                for (int i = 0; i < relCount; i++)
                {                    
                    returnList[i] = new PercussionGuid(relationships[i].ownerId);
                }
            }

            return returnList;
        }

        public PSItemStatus[] CheckOutForEditing(PercussionGuid[] guidList)
        {
            long[] idList = Array.ConvertAll(guidList, guid => (long)guid.ID);
            List<PSItemStatus> statusList = new List<PSItemStatus>();

            // Check out no more than maxChunkSize items at once.
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            int maxChunkSize = percussionConfig.TransactionChunkSize.Value;

            int itemCount = idList.Length;
            int loopCount = (itemCount / maxChunkSize);
            int remainder = itemCount % maxChunkSize;

            int subsetSize;
            int first;

            for (int i = 0; i <= loopCount; i++)
            {
                // Check items out in groups no larger than maxChunkSize.
                subsetSize = (i < loopCount) ? maxChunkSize : remainder;
                first = i * maxChunkSize;

                try
                {
                    // As items are checked out, add them to statusList for eventual return.
                    long[] listSubset = new long[subsetSize];
                    Array.ConstrainedCopy(idList, first, listSubset, 0, subsetSize);
                    PSItemStatus[] result = PSWSUtils.PrepareForEdit(_contentService, listSubset);
                    statusList.AddRange(result);
                }
                catch (Exception ex)
                {
                    // Percussion loops through items during a checkout and in the event of an error
                    // may leave some of them checked out.  We need to release those items.
                    //
                    // The statusList provides a means of rolling back the items from successful
                    // chunks.
                    //
                    // We don't have the necessary information to rollback the items from the
                    // chunk which failed.  We could check them back in, but that doesn't address
                    // the workflow state, and Percussion is OK with allowing the same user to check
                    // an item out twice.  On the plus side, the search mechanism seems to be slightly
                    // more reliable when searching for items by the checkoutusername field, so that
                    // should make it easier to deal with any needed manual fixes.

                    // Release items one at a time. This is more expensive than a chunked call,
                    // but if the chunk size has already resulted in an error has occured,
                    // this is more likely to complete safely.
                    statusList.ForEach(
                        item => PSWSUtils.ReleaseFromEdit(_contentService, new PSItemStatus[] { item })
                    );

                    throw new CMSOperationalException("CheckOutForEditing failed. Items rolled back.", ex);
                }
            }


            return statusList.ToArray();
        }

        public void ReleaseFromEditing(PSItemStatus[] statusList)
        {
            // Check in no more than maxChunkSize items at once.
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            int maxChunkSize = percussionConfig.TransactionChunkSize.Value;

            int itemCount = statusList.Length;
            int loopCount = (itemCount / maxChunkSize);
            int remainder = itemCount % maxChunkSize;

            int subsetSize;
            int first;

            for (int i = 0; i <= loopCount; i++)
            {
                // Release items in groups no larger than maxChunkSize.
                subsetSize = (i < loopCount) ? maxChunkSize : remainder;
                first = i * maxChunkSize;

                try
                {
                    // As items are checked out, add them to statusList for eventual return.
                    PSItemStatus[] listSubset = new PSItemStatus[subsetSize];
                    Array.ConstrainedCopy(statusList, first, listSubset, 0, subsetSize);
                    PSWSUtils.ReleaseFromEdit(_contentService, listSubset);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public string Render(PercussionGuid contentID, string templateName, string contextId, string siteId, string itemFilter)
        {
            string renderdHtml = string.Empty;

            PercussionGuid templateID = TemplateNameManager[templateName];

            Uri serverUri = new Uri(_systemService.Url);
            string cookies = _systemService.CookieContainer.GetCookieHeader(serverUri);

            // Percussion system login and any other needed intitialization goes here.
            // The login ID and password are loaded from the application's configuration file.
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            string host = percussionConfig.ConnectionInfo.Host.Value;
            string port = percussionConfig.ConnectionInfo.Port.Value;
            string protocol = percussionConfig.ConnectionInfo.Protocol.Value;

            string srcAddress = "http://156.40.134.66:9925/Rhythmyx/assembler/render";
            int pathStart = srcAddress.IndexOf("/Rhythmyx/");
            string baseUrl = string.Format("{0}://{1}:{2}{3}", protocol, host, port, srcAddress.Substring(pathStart));

            string url = string.Format("{0}?sys_contentid={1}&sys_context={2}&sys_siteid={3}&sys_itemfilter={4}&sys_template={5}",
                                        baseUrl, contentID.ID, contextId, siteId, itemFilter, templateID.ID);

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("cookie", cookies);
                Byte[] pageData = client.DownloadData(url);
                renderdHtml = Encoding.UTF8.GetString(pageData);
                if (string.IsNullOrEmpty(renderdHtml))
                    throw new Exception("CMS Preview did not render html content");
            }


            return renderdHtml;
        }

        #region Static Utility Methods

        /// <summary>
        /// Creates an array of PercussionGuid objects from a list of objects containing
        /// individual or collections of long values or PercussionGuid objects.
        /// </summary>
        /// <param name="potentialGuids">PercussionGuid objects. May be individual long values or
        /// PercussionGuid objects, or collections which implement IEnumerable for long or PercussionGuid.</param>
        /// <returns></returns>
        public static PercussionGuid[] BuildGuidArray(params Object[] potentialGuids)
        {
            List<PercussionGuid> guidList = new List<PercussionGuid>();

            foreach (object item in potentialGuids)
            {
                // skip the empties.
                if (item == null)
                    continue;

                if (item is PercussionGuid)
                    guidList.Add(item as PercussionGuid);
                else if (item is IEnumerable<PercussionGuid>)
                    guidList.AddRange(item as IEnumerable<PercussionGuid>);
                else if (item is long)
                    guidList.Add(new PercussionGuid((long)item));
                else if (item is IEnumerable<long>)
                {
                    IEnumerable<long> eTemp = item as IEnumerable<long>;
                    guidList.AddRange(eTemp.Select(id => new PercussionGuid(id)));
                }
                else
                    throw new ArgumentException("Arguments must be of type PercussionGuid, long, an IEnumerable<> collection of PercussionGuid or long.");
            }

            return guidList.ToArray();
        }

        #endregion

    }
}
