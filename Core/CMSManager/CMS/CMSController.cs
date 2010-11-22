using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
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
                PSWSUtils.Logout(_securityService, _loginSessionContext.sessionId);
                _securityService = null;
                _contentService = null;
                _systemService = null;
                _assemblyService = null;
                _loginSessionContext = null;
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
            _loginSessionContext= PSWSUtils.Login(_securityService, percussionConfig.ConnectionInfo.UserName.Value,
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
            long id;
            foreach (ContentItemForCreating cmi in contentItems)
            {
                {
                    id = CreateItem(cmi.ContentType, cmi.Fields, cmi.ChildFieldList, cmi.TargetFolder, errorHandler);
                    idList.Add(id);
                }
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
            return UpdateContentItemList(contentItems, null);
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
            List<long> idUpdList = new List<long>();
            long idUpd;
            foreach (ContentItemForUpdating cmi in contentItems)
            {
                idUpd = UpdateItem(cmi.ID, cmi.Fields, invalidFieldnameHandler);
                idUpdList.Add(idUpd);
            }
            return idUpdList;
        }

        /// <summary>
        /// Updates a single content item.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="invalidFieldnameHandler">Method accepting a single string parameter to call
        /// when a fieldname is not valid.  If invalidFieldnameHandler is null, invalid fieldnames
        /// will throw CMSInvalidFieldnameException with the invalid name.</param>
        /// <returns>The ID of the content time which has been updated.</returns>
        private long UpdateItem(long id,
            Dictionary<string, string> newFieldValues,
            Action<string> invalidFieldnameHandler)
        {
            // TODO: Merge this with the version of UpdateContentItemList which accepts invalidFieldnameHandler.

            PSItemStatus[] checkOutStatus = PSWSUtils.PrepareForEdit(_contentService, new long[] { id });

            PSItem[] returnList = PSWSUtils.LoadItems(_contentService, new long[]{id});
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
            foreach (KeyValuePair<string,string> kvp in fieldValueList)
            {
                string targetName = kvp.Key;
                string fieldValue = kvp.Value;

                PSField itemField = Array.Find(itemFieldSet, field => { return field.name == targetName; });
                if (itemField != null)
                {
                    PSFieldValue value = new PSFieldValue();
                    value.RawData = fieldValue;
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
        /// Deletes the specified content item.
        /// </summary>
        /// <param name="itemID">ID of the content item to be deleted.</param>
        public void DeleteItem(long itemID)
        {
            PSWSUtils.DeleteItem(_contentService, new long[]{itemID});
        }

        /// <summary>
        /// Deletes the specified content item.
        /// </summary>
        /// <param name="itemList">Array of content item IDs to be deleted.</param>
        public void DeleteItemList(PercussionGuid[] itemList)
        {
            long[] rawIDs = Array.ConvertAll(itemList, item => (long)item.ID);
            PSWSUtils.DeleteItem(_contentService, rawIDs);
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
            sourcePath = siteRootPath + sourcePath;
            targetPath = siteRootPath + targetPath;

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
        public PSFolder GuaranteeFolder(string folderPath)
        {
            return FolderManager.GuaranteeFolder(siteRootPath + folderPath);
        }

        /// <summary>
        /// Deletes a collection of folders. Any items contained in the folder are
        /// removed from the content tree, but are not purged.
        /// </summary>
        /// <param name="folderInfo"></param>
        public void DeleteFolders(PSFolder[] folderInfo)
        {
            long[] folderIDs = Array.ConvertAll(folderInfo, detail => detail.id);
            PSWSUtils.DeleteFolders(_contentService, folderIDs, false);
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
        /// <param name="IDList">An array of PercusionGuid objects to be checked
        /// for incoming active assembly relationships. Required.</param>
        /// <param name="slotName">If specified, restricts the result set to relationships which
        /// use the named slot.</param>
        /// <param name="templateName">If specified, restricts the result set to relationships which
        /// use the named snippet template.</param>
        /// <returns>An array of zero or more PSAaRelationship objects having one or move
        /// items from IDList as a dependent.</returns>
        public PSAaRelationship[] FindIncomingActiveAssemblyRelationships(PercussionGuid[] IDList, string slotName, string templateName)
        {
            PSAaRelationshipFilter filter = new PSAaRelationshipFilter();
            filter.Dependent = Array.ConvertAll(IDList, guid => (long)guid.ID);

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
            PSAaRelationship[] relationships = null;

            PSItemStatus[] parentCheckoutStatus = PSWSUtils.PrepareForEdit(_contentService, new long[]{parentItemID});
            if (!parentCheckoutStatus[0].didCheckout)
                throw new CMSOperationalException(string.Format("Unable to perform a checkout for item with CMS content item {0}.", parentItemID));

            relationships = PSWSUtils.CreateActiveAssemblyRelationships(_contentService, parentItemID, childItemIDList, slotName, snippetTemplateName);

            PSWSUtils.ReleaseFromEdit(_contentService, parentCheckoutStatus);

            return relationships;
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
        /// Strips the leading //Sites/sitename portion from the first path
        /// a content item resides in.
        /// </summary>
        /// <param name="item">A content item</param>
        /// <returns>The path relative to the site's base, or null if no path is available.</returns>
        public string GetPathInSite(PSItem item)
        {
            if (item == null || item.Folders == null || item.Folders.Length == 0)
                return null;

            PSItemFolders pathFolder = Array.Find(item.Folders, folder => (!string.IsNullOrEmpty(folder.path)));
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
