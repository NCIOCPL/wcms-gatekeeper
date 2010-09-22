using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GKManagers.CMSManager.PercussionWebSvc;
using GKManagers.CMSManager.Configuration;
using System.Web.Services.Protocols;

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
        string m_rxSession;

        /**
         * The security service instance; used to perform operations defined in
         * the security services. It is initialized by login().
         */
        securitySOAP m_secService;

        /**
         * The content service instance; used to perform operations defined in
         * the content services. It is initialized by login().
         */
        contentSOAP m_contService;

        /**
         * The system service instance; used to perform operations defined in
         * the system service. It is initialized by login().
         */
        systemSOAP m_sysService;

        #endregion

        #region CMSController Session Fields.

        // These fields are used to store information about the CMS.
        // In order to avoid stateful behavior, no fields are defined to maintain
        // the state of content items (or similar entities) between calls to
        // the controller's public methods.

        private string siteRootPath = string.Empty;

        #endregion

        #region Disposable Pattern Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Free managed resources only.
            if (disposing)
            {
                PSWSUtils.Logout(m_secService, m_rxSession);
                m_rxSession = null;
                m_secService = null;
                m_contService = null;
                m_sysService = null;
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
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig/connectionInfo");
            siteRootPath = percussionConfig.SiteRootPath.Value;

        }

        /// These are methods we may still need.

        ///     LoadContentItem - Loads an existing content item. (Does this need to be a content ID? Can it be a path?)
        ///     ContentItemExists - Boolean -- true if an item exists, false otherwise.  Needs to be able to detect
        ///                         based on a path and pretty_url_name field. Otherwise, we need to keep this information
        ///                         in a GateKeeper-owned database.
        ///     CreatePath (Based on a string containing the path. Is a site name needed?)



        /// <summary>
        /// Login to the Percussion session, set up services.
        /// </summary>
        private void Login()
        {
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig/connectionInfo");

            PSWSUtils.SetConnectionInfo(percussionConfig.Protocol.Value, percussionConfig.Host.Value,
                Int16.Parse(percussionConfig.Port.Value));

            m_secService = PSWSUtils.GetSecurityService();
            m_rxSession = PSWSUtils.Login(m_secService, percussionConfig.UserName.Value,
                  percussionConfig.Password.Value, percussionConfig.Community.Value, null);

            m_contService = PSWSUtils.GetContentService(m_secService.CookieContainer,
                m_secService.PSAuthenticationHeaderValue);
            m_sysService = PSWSUtils.GetSystemService(m_secService.CookieContainer,
                m_secService.PSAuthenticationHeaderValue);
        }



        /// <summary>
        /// Creates a new content item.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="fieldCollections">The field collections.</param>
        /// <param name="targetFolder">The target folder.</param>
        /// <returns> A list of id's for the items created</returns>
        public List<long> CreateContentItemList(string contentType, List<CreateContentItem> contentItems)
        {
            List<long> idList = new List<long>();
            long id;
            foreach (CreateContentItem cmi in contentItems)
            {
                {
                    id = CreateItem(contentType, cmi.Fields, cmi.TargetFolder);
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
            PSItem item = PSWSUtils.CreateItem(m_contService, contentType);

            //Attach to a folder

            PSItemFolders psf = new PSItemFolders();

            psf.path = siteRootPath + targetFolder;

            item.Folders = new PSItemFolders[] { psf };


            SetItemFields(item, fields);

            long id = PSWSUtils.SaveItem(m_contService, item);
            PSWSUtils.CheckinItem(m_contService, id);
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
            PSItemStatus status = PSWSUtils.PrepareForEdit(m_contService, id);
            PSItem item = new PSItem();
            PSItemFolders psf = new PSItemFolders();
            psf.path = siteRootPath + targetFolder;
            item.Folders = new PSItemFolders[] { psf };

            item = PSWSUtils.LoadItem(m_contService, id);

            SetItemFields(item, fields);
            long idUpd = PSWSUtils.SaveItem(m_contService, item);

            PSWSUtils.ReleaseFromEdit(m_contService, status);
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
        /// Deletes the content item.
        /// </summary>
        /// <param name="IDs">Array of Ids.</param>
        public void DeleteItem(long[] IDs)
        {
            PSWSUtils.DeleteItem(m_contService, IDs);
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
            string[] transitionNames = PSWSUtils.GetTransitions(m_sysService, itemIDs);
            return inferState(transitionNames);
        }

        public void PerformWorkflowTransition(long[] idList, string triggerName)
        {
            PSWSUtils.TransitionItems(m_sysService, idList, triggerName);
        }
    }
}
