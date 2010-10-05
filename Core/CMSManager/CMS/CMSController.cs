using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;

using GKManagers.CMSManager.Configuration;
using GKManagers.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSManager.CMS
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

        // These four fields represent the interface to Percussion.  They are initialized by the
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
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CMSController"/> class.
        /// </summary>
        public CMSController()
        {
            // Percussion system login and any other needed intitialization goes here.
            // The login ID and password are loaded from the application's configuration file.
            Login();
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            siteRootPath = percussionConfig.ConnectionInfo.SiteRootPath.Value;

        }


        /// <summary>
        /// Login to the Percussion session, set up services.
        /// </summary>
        private void Login()
        {
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

            PSWSUtils.SetConnectionInfo(percussionConfig.ConnectionInfo.Protocol.Value, percussionConfig.ConnectionInfo.Host.Value,
                Int16.Parse(percussionConfig.ConnectionInfo.Port.Value));

            _securityService = PSWSUtils.GetSecurityService();
            _percussionSession = PSWSUtils.Login(_securityService, percussionConfig.ConnectionInfo.UserName.Value,
                  percussionConfig.ConnectionInfo.Password.Value, percussionConfig.ConnectionInfo.Community.Value, null);

            _contentService = PSWSUtils.GetContentService(_securityService.CookieContainer,
                _securityService.PSAuthenticationHeaderValue);
            _systemService = PSWSUtils.GetSystemService(_securityService.CookieContainer,
                _securityService.PSAuthenticationHeaderValue);
        }



        /// <summary>
        /// Creates a new content item.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="fieldCollections">The field collections.</param>
        /// <param name="targetFolder">The target folder.</param>
        /// <returns> A list of id's for the items created</returns>
        public List<long> CreateContentItemList(List<CreateContentItem> contentItems)
        {
            List<long> idList = new List<long>();
            long id;
            foreach (CreateContentItem cmi in contentItems)
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
        private long CreateItem(string contentType, Dictionary<string, string> fields, string targetFolder)
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

        /// <summary>
        /// Updates the content item list.
        /// </summary>
        /// <param name="contentItems">A collection of UpdateContentItem.</param>
        /// <returns>List of IDs of all the content items updated </returns>
        public List<long> UpdateContentItemList(List<UpdateContentItem> contentItems)
        {
            List<long> idUpdList = new List<long>();
            long idUpd;
            foreach (UpdateContentItem cmi in contentItems)
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
            PSItemStatus status = PSWSUtils.PrepareForEdit(_contentService, id);
            PSItem item = new PSItem();
            PSItemFolders psf = new PSItemFolders();
            psf.path = siteRootPath + targetFolder;
            item.Folders = new PSItemFolders[] { psf };

            PSItem[] returnList = PSWSUtils.LoadItems(_contentService, new long[]{id});
            item = returnList[0];

            SetItemFields(item, fields);
            long idUpd = PSWSUtils.SaveItem(_contentService, item);
            PSWSUtils.ReleaseFromEdit(_contentService, status);
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
                string nameValue;
                if (fields.TryGetValue(srcField.name, out nameValue))
                {
                    PSFieldValue value = new PSFieldValue();
                    value.RawData = nameValue;
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
            FolderManager folderMgr = new FolderManager(_contentService);
            return folderMgr.GuaranteeFolder(siteRootPath + folderPath);
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
            PSAaRelationship[] relationships = PSWSUtils.GetRelationships(_contentService, filter);

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
    }
}
