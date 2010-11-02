using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Services.Protocols;

using GKManagers.CMSManager.Configuration;
using NCI.WCM.CMSManager.PercussionWebSvc;

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

        #region Percussion Fields

        // These fields represent the interface to Percussion.  They are initialized by the
        // CMSController constructor. These fields are used by all CMSController methods which
        // need to communicate with the Percussion system.

        // The Percussion session, initialized by login() in the constructor. 
        string _percussionSession;

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

        private string siteRootPath = string.Empty;

        #endregion

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
                PSWSUtils.Logout(_securityService, _percussionSession);
                _percussionSession = null;
                _securityService = null;
                _contentService = null;
                _systemService = null;
                _assemblyService = null;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the CMSController class.
        /// </summary>
        public CMSController()
        {
            // Percussion system login and any other needed intitialization goes here.
            // The login ID and password are loaded from the application's configuration file.
            Login();
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            siteRootPath = percussionConfig.ConnectionInfo.SiteRootPath.Value;
        }

        public TemplateNameManager TemplateNameManager
        {
            /*
             * At a glance, lazy-loading like this seems dangerous. But the TemplateNameManager
             * property can only be accessed from an instance of CMSController. Because the constructor
             * calls Login, the _assemblyService can be used safely.  The only danger is if someone
             * tries using the property after CMController has been disposed, but that requires a high
             * degree of not knowing what you're doing.
             */
            get { return new TemplateNameManager(_assemblyService); }
        }

        public FolderManager FolderManager
        {
            get { return new FolderManager(_contentService); }
        }

        /// <summary>
        /// Login to the Percussion session, set up services.
        /// </summary>
        private void Login()
        {
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

            string protocol = percussionConfig.ConnectionInfo.Protocol.Value;
            string host = percussionConfig.ConnectionInfo.Host.Value;
            string port = percussionConfig.ConnectionInfo.Port.Value;

            _securityService = PSWSUtils.GetSecurityService(protocol, host, port);
            _percussionSession = PSWSUtils.Login(_securityService, percussionConfig.ConnectionInfo.UserName.Value,
                  percussionConfig.ConnectionInfo.Password.Value, percussionConfig.ConnectionInfo.Community.Value, null);

            _contentService = PSWSUtils.GetContentService(protocol, host, port, _securityService.CookieContainer,
                _securityService.PSAuthenticationHeaderValue);
            _systemService = PSWSUtils.GetSystemService(protocol, host, port, _securityService.CookieContainer,
                _securityService.PSAuthenticationHeaderValue);
            _assemblyService = PSWSUtils.GetAssemblyService(protocol, host, port, _securityService.CookieContainer,
                _securityService.PSAuthenticationHeaderValue);
        }

        /// <summary>
        /// Creates the content items in the list.
        /// </summary>
        /// <param name="contentItems">The content items list.</param>
        /// <returns>List of Id's for the items created</returns>
        public List<long> CreateContentItemList(List<ContentItemForCreating> contentItems)
        {
            List<long> idList = new List<long>();
            long id;
            foreach (ContentItemForCreating cmi in contentItems)
            {
                {
                    id = CreateItem(cmi.ContentType, cmi.Fields, cmi.TargetFolder);
                    idList.Add(id);
                }
            }

            return idList;
        }

        /// <summary>
        /// Creates a single item in percussion.
        /// </summary>
        /// <param name="contentType">Type of the content like druginfosummary,cancerinfosummary.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="targetFolder">The target folder in percussion.</param>
        /// <returns>Id for the the created item</returns>
        [Obsolete("This method will be merged with CreateContentItemList().")]
        public long CreateItem(string contentType, Dictionary<string, string> fields, string targetFolder)
        {
            PSItem item = PSWSUtils.CreateItem(_contentService, contentType);

            // Attach item to a folder
            PSFolder folder = GuaranteeFolder(targetFolder);

            PSItemFolders psf = new PSItemFolders();
            psf.path = folder.path;

            item.Folders = new PSItemFolders[] { psf };


            SetItemFields(item, fields);

            long id = PSWSUtils.SaveItem(_contentService, item);
            PSWSUtils.CheckinItem(_contentService, id);
            return id;

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
            List<long> idUpdList = new List<long>();
            long idUpd;
            foreach (ContentItemForUpdating cmi in contentItems)
            {
                idUpd = UpdateItem(cmi.ID, cmi.Fields, cmi.TargetFolder);
                idUpdList.Add(idUpd);
            }
            return idUpdList;
        }

        /// <summary>
        /// Updates a single content item.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="targetFolder">The target folder.</param>
        /// <returns></returns>
        private long UpdateItem(long id, Dictionary<string, string> fields, string targetFolder)
        {
            PSItemStatus[] checkOutStatus = PSWSUtils.PrepareForEdit(_contentService, new long[] { id });
            PSItem item = new PSItem();
            PSItemFolders psf = new PSItemFolders();
            psf.path = siteRootPath + targetFolder;
            item.Folders = new PSItemFolders[] { psf };

            PSItem[] returnList = PSWSUtils.LoadItems(_contentService, new long[]{id});
            item = returnList[0];

            SetItemFields(item, fields);
            long idUpd = PSWSUtils.SaveItem(_contentService, item);
            PSWSUtils.ReleaseFromEdit(_contentService, checkOutStatus);
            return idUpd;
        }

        /// <summary>
        /// Sets the content item fields.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="fields">The fields.</param>
        private void SetItemFields(PSItem item, Dictionary<string, string> fields)
        {
            foreach (PSField srcField in item.Fields)
            {
                string fieldValue;
                if (fields.TryGetValue(srcField.name, out fieldValue))
                {
                    PSFieldValue value = new PSFieldValue();
                    value.RawData = fieldValue;
                    srcField.PSFieldValue = new PSFieldValue[] { value };
                }
            }
        }


        /// <summary>
        /// Deletes the specified content item.
        /// </summary>
        /// <param name="itemID">ID of the content item to be deleted.</param>
        public void DeleteItem(long itemID)
        {
            PSWSUtils.DeleteItem(_contentService, new long[]{itemID});
        }

        public void DeleteItemList(PercussionGuid[] itemList)
        {
            int length = itemList.Length;
            long[] rawIDs = new long[length];
            for (int i = 0; i < length; i++)
            {
                rawIDs[i] = itemList[i].ID;
            }

            PSWSUtils.DeleteItem(_contentService, rawIDs);
        }

        /// <summary>
        /// Moves a content item from one folder to another.
        /// </summary>
        /// <param name="sourcePath">The source folder path.</param>
        /// <param name="targetPath">The target folder path.</param>
        /// <param name="id">The id.</param>
        public void MoveContentItemFolder(string sourcePath, string targetPath, long[] id)
        {
            sourcePath = siteRootPath + sourcePath;
            targetPath = siteRootPath + targetPath;

            PSWSUtils.MoveFolderChildren(_contentService, targetPath, sourcePath, id);
        }

        /// <summary>
        /// GuaranteeFolder that a folder exists, creating it if it doesn't
        /// already exist.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>A PSFolder object containing details of the folder.</returns>
        /// <remarks>The folder path argument will have the path to the site root
        /// prepended before the attempt is made to create it.</remarks>
        public PSFolder GuaranteeFolder(string folderPath)
        {
            return FolderManager.GuaranteeFolder(siteRootPath + folderPath);
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
        /// Retrieves a list of content items identified by the values in itemIDList.
        /// </summary>
        /// <param name="itemIDList">An array of content item ID values.</param>
        /// <returns>An array of content items in the same order as the values
        /// in itemIDList</returns>
        public PSItem[] LoadContentItems(long[] itemIDList)
        {
            return PSWSUtils.LoadItems(_contentService, itemIDList);
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
        public PSAaRelationship[] CreateRelationships(long parentItemID, long[] childItemIDList, string slotName, string snippetTemplateName)
        {
            PSAaRelationship[] relationships = null;

            PSItemStatus[] parentCheckoutStatus = PSWSUtils.PrepareForEdit(_contentService, new long[]{parentItemID});
            if (!parentCheckoutStatus[0].didCheckout)
                throw new CMSOperationalException(string.Format("Unable to perform a checkout for item with CMS content item {0}.", parentItemID));

            relationships = PSWSUtils.CreateRelationships(_contentService, parentItemID, childItemIDList, slotName, snippetTemplateName);

            PSWSUtils.ReleaseFromEdit(_contentService, parentCheckoutStatus);

            return relationships;
        }

        /// <summary>
        /// Strips the leading //Sites/sitename portion from the first path
        /// a content item resides in.
        /// </summary>
        /// <param name="item">A content item</param>
        /// <returns>The path relative to the site's base, or null if no path is available.</returns>
        public string GetPathInSite(PSItem item)
        {
            if (item == null || item.Folders == null || item.Folders.Length == 0)
                return null;

            PSItemFolders pathFolder = Array.Find(item.Folders, folder => (string.IsNullOrEmpty(folder.path)));
            if (pathFolder == null)
                return null;

            string path = pathFolder.path;
            if (path.StartsWith(siteRootPath, StringComparison.InvariantCultureIgnoreCase))
                return path.Substring(siteRootPath.Length);
            else
                return path;
        }

        public enum CMSPublishingTarget
        {
            CDRStaging = 0, // For completeness, not really useful.
            CDRPreview = 1,
            CDRLive = 2
        }


        public void StartPublishing(CMSPublishingTarget target)
        {
            // Preview and Live are the only CMS publishing editions we would run.
            if (target == CMSPublishingTarget.CDRPreview || target == CMSPublishingTarget.CDRLive)
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
            else
                textValue = percussionConfig.LiveRepublishEditionList.Value.Trim();

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
        /// <param name="path">Base path in which to search for content items.
        /// (Must begin with /, must not include the //Sites/sitename component.)</param>
        /// <param name="fieldCriteria">Optional list of name/value pairs identifying the fields
        /// and values to search for</param>
        /// <returns>An array containing zero or more content item ID values.  The array may
        /// be empty, but is never null.</returns>
        public PercussionGuid[] SearchForContentItems(string contentType, string path, Dictionary<string, string> fieldCriteria)
        {
            PercussionGuid[] contentIdList;

            string searchPath;

            if (!string.IsNullOrEmpty(path))
                searchPath = siteRootPath + path;
            else
                searchPath = null;


            PSSearchResults[] searchResults = PSWSUtils.FindItemByFieldValues(_contentService, contentType, searchPath, fieldCriteria);
            contentIdList = new PercussionGuid[searchResults.Length];
            for (int i = 0; i < searchResults.Length; i++)
            {
                // FindItemByFieldValues always returns the sys_contentid field, so it's safe to assume this expression will work.
                long value = long.Parse(Array.Find(searchResults[i].Fields, field => field.name.Equals("sys_contentid")).Value);
                contentIdList[i] = new PercussionGuid(value);
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

    }
}
