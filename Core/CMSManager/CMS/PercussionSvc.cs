using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GKManagers.CMSManager.PercussionWebSvc;
using System.Configuration;
using GKManagers.CMSManager.Configuration;
namespace GKManagers.CMSManager.CMS
{
    [Obsolete("This class has been supplanted by CMSController.  Do not use PercussionSvc.", true)]
    internal class PercussionSvc
    {
        /**
         * The Rhythmyx session, initialized by login()}. 
         */
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


        /**
         * The loader properties, read from the file 'Loader.xml'.
         */
        public Dictionary<string, string> m_props;

        /**
         * The Content Item data to be uploaded; read from the file 'DataFile.xml' 
         */
        private List<Dictionary<string, string>> m_itemData;


        /**
         * The property name of the protocol of the server connection.
         */
        public static String PROTOCOL = "Protocol";

        /**
         * The property name of the host of the server connection.
         */
        public static String HOST = "Host";

        /**
         * The property name of the port of the server connection.
         */
        public static String PORT = "Port";

        /**
         * The property name of the name of the login user.
         */
        public static String USER_NAME = "Username";

        /**
         * The property name of the password of the login user.
         */
        public static String PASSWORD = "Password";

        /**
         * The property name of the name of the login Community.
         */
        public static String COMMUNITY = "Community";

        /**
         * The property name of the name of the Content Type of the Content Items to
         * be uploaded. 
         */
        public static String CONTENT_TYPE = "ContentType";

        /**
         * The property name of the target Folder path in Rhythmyx.
         */

        public static String TARGET_FOLDER = "TargetFolder";

        public static String APPEND_TARGET_FOLDER = "AppendTargetFolder";

        /**
         * The property name of the name of the data file.
         */
        public static String DATA_FILE = "DataFile";

        public PercussionSvc()
        {
            // Load the connection information
            Dictionary<string, string> props = new Dictionary<string, string>();
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig/connectionInfo");


            props.Add(PercussionSvc.PROTOCOL, percussionConfig.Protocol.Value);
            props.Add(PercussionSvc.HOST, percussionConfig.Host.Value);
            props.Add(PercussionSvc.PORT, percussionConfig.Port.Value);

            props.Add(PercussionSvc.USER_NAME, percussionConfig.UserName.Value);
            props.Add(PercussionSvc.PASSWORD, percussionConfig.Password.Value);
            props.Add(PercussionSvc.COMMUNITY, percussionConfig.Community.Value);
            props.Add(PercussionSvc.APPEND_TARGET_FOLDER, percussionConfig.AppendTargetFolder.Value);


            //props.Add(PercussionSvc.CONTENT_TYPE, "pdqDrugInfoSummary");

            m_props = props;

        }

        public void Login()
        {
            PSWSUtils.SetConnectionInfo(m_props[PROTOCOL], m_props[HOST],
                Int16.Parse(m_props[PORT]));

            m_secService = PSWSUtils.GetSecurityService();
            m_rxSession = PSWSUtils.Login(m_secService, m_props[USER_NAME],
                  m_props[PASSWORD], m_props[COMMUNITY], null);

            m_contService = PSWSUtils.GetContentService(m_secService.CookieContainer,
                m_secService.PSAuthenticationHeaderValue);
            m_sysService = PSWSUtils.GetSystemService(m_secService.CookieContainer,
                m_secService.PSAuthenticationHeaderValue);
        }


        public void Logout()
        {
            PSWSUtils.Logout(m_secService, m_rxSession);
        }


        public void CreateTargetFolder(string targetFolder)
        {
            PSFolder[] folders = PSWSUtils.AddFolderTree(m_contService, targetFolder);

        }


        public void UploadContentItem(List<Dictionary<string, string>> m_itemData)
        {
            // get the items in the target folder
            PSItemSummary[] curItems = PSWSUtils.FindFolderChildren(m_contService,
                m_props[TARGET_FOLDER]);

            foreach (Dictionary<string, string> itemFields in m_itemData)
            {
                {
                    CreateItem(itemFields);
                }
            }

        }

        PSItemSummary GetItem(PSItemSummary[] curItems, String sysTitle)
        {
            foreach (PSItemSummary item in curItems)
            {
                if (item.name.ToLower() == sysTitle.ToLower())
                    return item;
            }
            return null;

        }

        private void UpdateItem(long id, Dictionary<string, string> fields)
        {
            PSItemStatus status = PSWSUtils.PrepareForEdit(m_contService, id);

            PSItem item = PSWSUtils.LoadItem(m_contService, id);
            SetItemFields(item, fields);
            PSWSUtils.SaveItem(m_contService, item);

            PSWSUtils.ReleaseFromEdit(m_contService, status);

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

        private void CreateItem(Dictionary<string, string> fields)
        {
            PSItem item = PSWSUtils.CreateItem(m_contService, m_props[CONTENT_TYPE]);

            SetItemFields(item, fields);

            long id = PSWSUtils.SaveItem(m_contService, item);
            PSWSUtils.CheckinItem(m_contService, id);
            //uncomment after fixing the save bug
            //PSWSUtils.TransitionItem(m_sysService, id, "DirecttoPublic");

            // Attach the Content Item to the Target folder
            String path = m_props[TARGET_FOLDER];
            PSWSUtils.AddFolderChildren(m_contService, path, new long[] { id });

        }

        public static void TransitionItem(systemSOAP systemSvc, long id,
            string trigger)
        {
            TransitionItemsRequest req = new TransitionItemsRequest();
            req.Id = new long[] { id };
            req.Transition = trigger;
            systemSvc.TransitionItems(req);
        }

        /// <summary>
        /// Searches for a list of workflow objects.
        /// </summary>
        /// <param name="workflowName">Name of the workflow(s) to be loaded, wildcards allowed.</param>
        /// <returns>Zero or more workflows with names matching the specified name.</returns>
        public PSWorkflow[] LoadWorkflow(string workflowName)
        {
            PSWorkflow[] workflows;

            LoadWorkflowsRequest request = new LoadWorkflowsRequest();
            request.Name = workflowName;
            workflows = m_sysService.LoadWorkflows(request);

            return workflows;
        }

    }
}
