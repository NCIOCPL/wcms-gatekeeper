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

        
        #region Fields

        // This is the one and only instance of the interface to Percussion.  It gets created
        // when the CMSController is created. Login occurs when the CMSController constructor
        // is run.  This instance is used by all CMSController methods which need to communicate
        // with the Percussion system.
        private readonly PercussionSvc percCMS = new PercussionSvc();

        private WorkflowMapper workflowController;

        #endregion

        public CMSController()
        {
            // Percussion system login and any other needed intitialization goes here.
            // The login ID and password must be loaded from the application's configuration file.
            percCMS.Login();
            InitializeWorkflowController();
        }

        //Set Content Item
        public void SetContentType(string contentType)
        {
            percCMS.m_props.Add(PercussionSvc.CONTENT_TYPE, "pdqDrugInfoSummary");

        }
        
        //Create Target Directory
        public void CreateTargetDirectory(string targetFolder)
        {
            percCMS.m_props.Add(PercussionSvc.TARGET_FOLDER, percCMS.m_props["AppendTargetFolder"].ToString() + targetFolder);
            percCMS.CreateTargetFolder(percCMS.m_props["TargetFolder"].ToString());
            
        }

        /// A few methods which are definitely needed:
        ///     CreateContentItem - Creates a blank content item. Based on a string containing the Content Type.

        public void CreateContentItem(List<Dictionary<string, string>> m_itemData)
        {
            try
            {
                percCMS.UploadContentItem(m_itemData);
            }
            catch (SoapException e)
            {

            }
        }
        
        
        
        
        
        ///     LoadContentItem - Loads an existing content item. (Does this need to be a content ID? Can it be a path?)
        ///     ContentItemExists - Boolean -- true if an item exists, false otherwise.  Needs to be able to detect
        ///                         based on a path and pretty_url_name field. Otherwise, we need to keep this information
        ///                         in a GateKeeper-owned database.
        ///     CreatePath (Based on a string containing the path. Is a site name needed?)


        /// <summary>
        /// Retrieves a list of workflow objects.
        /// </summary>
        /// <param name="workflowName">Name of the workflow(s) to be loaded, wildcards allowed.</param>
        /// <returns>Zero or more workflows with names matching the specified name.</returns>
        public void InitializeWorkflowController()
        {
            PSWorkflow[] workflows = percCMS.LoadWorkflow(WorkFlowNames.WorkFlow);
            workflowController = new WorkflowMapper(workflows);
        }
    }
}
