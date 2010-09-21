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
    /// This class is the sole means by which any code in the GateKeeper system may interact with Percussion.
    /// It manages the single login session used for all interations, and performs all needed operations.
    /// </summary>
    public class CMSController
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

        private WorkflowMapper m_workflowMap;

        #endregion


        public CMSController()
        {
            // Percussion system login and any other needed intitialization goes here.
            // The login ID and password are loaded from the application's configuration file.
            Login();
            InitializeWorkflowController();
        }

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


        ////Set Content Item
        //public void SetContentType(string contentType)
        //{
        //    percCMS.m_props.Add(PercussionSvc.CONTENT_TYPE, "pdqDrugInfoSummary");

        //}
        
        //Create Target Directory
        //public void CreateTargetDirectory(string targetFolder)
        //{
        //    percCMS.m_props.Add(PercussionSvc.TARGET_FOLDER, percCMS.m_props["AppendTargetFolder"].ToString() + targetFolder);
        //    percCMS.CreateTargetFolder(percCMS.m_props["TargetFolder"].ToString());
            
        //}

        // A few methods which are definitely needed:
        //     CreateContentItem - Creates a blank content item. Based on a string containing the Content Type.

        public void CreateContentItem(string contentType,
            List<Dictionary<string, string>> fieldCollections,
            string targetFolder)
        {
            try
            {
                // get the items in the target folder
                PSItemSummary[] curItems = PSWSUtils.FindFolderChildren(m_contService, targetFolder);

                foreach (Dictionary<string, string> itemFields in fieldCollections)
                {
                    {
                        CreateItem(contentType, itemFields, targetFolder);
                    }
                }
            }
            catch (SoapException e)
            {

            }
        }

        private void CreateItem(string contentType, Dictionary<string, string> fields, string targetFolder)
        {
            PSItem item = PSWSUtils.CreateItem(m_contService, contentType);

            SetItemFields(item, fields);

            long id = PSWSUtils.SaveItem(m_contService, item);
            PSWSUtils.CheckinItem(m_contService, id);
            //uncomment after fixing the save bug
            //PSWSUtils.TransitionItem(m_sysService, id, "DirecttoPublic");

            // Attach the Content Item to the Target folder
            String path = contentType;
            PSWSUtils.AddFolderChildren(m_contService, path, new long[] { id });

        }


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


        ///     LoadContentItem - Loads an existing content item. (Does this need to be a content ID? Can it be a path?)
        ///     ContentItemExists - Boolean -- true if an item exists, false otherwise.  Needs to be able to detect
        ///                         based on a path and pretty_url_name field. Otherwise, we need to keep this information
        ///                         in a GateKeeper-owned database.
        ///     CreatePath (Based on a string containing the path. Is a site name needed?)


        /// <summary>
        /// Initialize the map
        /// </summary>
        private void InitializeWorkflowController()
        {
            PSWorkflow[] workflows;
            
            LoadWorkflowsRequest request = new LoadWorkflowsRequest();
            request.Name = WorkFlowNames.WorkFlow;
            workflows = m_sysService.LoadWorkflows(request);

            m_workflowMap = new WorkflowMapper(workflows);
        }
    }
}
