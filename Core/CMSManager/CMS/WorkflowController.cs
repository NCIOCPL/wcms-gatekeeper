using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GKManagers.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSManager.CMS
{
    /// <summary>
    /// Controls a content item's progress through the CancerGov_PDQ_Workflow.
    /// All updates to the CancerGov_PDQ_Workflow must, must, must be reflected
    /// in this set of state and transition IDs.
    /// </summary>
    internal class WorkflowMapper
    {
        public long StagingStateID { get; private set; }
        public long PreviewStateID { get; private set; }
        public long LiveStateID { get; private set; }
        public long UpdateStagingStateID { get; private set; }
        public long UpdatePreviewStateID { get; private set; }

        public long TransitionIDStagingToPreview { get; private set; }
        public long TransitionIDPreviewToLive { get; private set; }
        public long TransitionIDLiveToUpdateStaging { get; private set; }
        public long TransitionIDUpdateStagingToUpdatePreview { get; private set; }
        public long TransitionIDUpdatePreviewToLive { get; private set; }

        public WorkflowMapper(PSWorkflow[] workflows)
        {
            if (workflows.Length != 1)
                throw new CMSWorkflowException(string.Format("Error initializing WorkflowController. Expected 1 workflow, found {0}.", workflows.Length));

            PSWorkflow theWorkflow = workflows[0];

            // Get workflow State IDs.
            foreach (PSState item in theWorkflow.States)
            {
                switch (item.name.ToLower())
                {
                    case WorkFlowNames.StateName.Staging:
                        MapStagingIDs(item);
                        break;
                    case WorkFlowNames.StateName.Preview:
                        MapPreviewIDs(item);
                        break;
                    case WorkFlowNames.StateName.Live:
                        MapLiveIDs(item);
                        break;
                    case WorkFlowNames.StateName.UpdateStaging:
                        MapUpdateStagingIDs(item);
                        break;
                    case WorkFlowNames.StateName.UpdatePreview:
                        MapUpdatePreviewIDs(item);
                        break;
                    default:
                        // We don't know what this state is.
                        break;
                }
            }
        }

        private void MapStagingIDs(PSState workflowState)
        {
            StagingStateID = workflowState.id;
            foreach (PSTransition item in workflowState.Transitions)
            {
                switch (item.label.ToLower())
                {
                    case WorkFlowNames.TransitionName.Staging.ToPreview:
                        TransitionIDStagingToPreview = item.id;
                        break;
                    default:
                        break;
                }
            }
        }

        private void MapPreviewIDs(PSState workflowState)
        {
            PreviewStateID = workflowState.id;
            foreach (PSTransition item in workflowState.Transitions)
            {
                switch (item.label.ToLower())
                {
                    case WorkFlowNames.TransitionName.Preview.ToLive:
                        TransitionIDPreviewToLive = item.id;
                        break;
                    default:
                        break;
                }
            }
        }

        private void MapLiveIDs(PSState workflowState)
        {
            LiveStateID = workflowState.id;
            foreach (PSTransition item in workflowState.Transitions)
            {
                switch (item.label.ToLower())
                {
                    case WorkFlowNames.TransitionName.Live.ToStaging:
                        TransitionIDLiveToUpdateStaging = item.id;
                        break;
                    default:
                        break;
                }
            }
        }

        private void MapUpdateStagingIDs(PSState workflowState)
        {
            UpdateStagingStateID = workflowState.id;
            foreach (PSTransition item in workflowState.Transitions)
            {
                switch (item.label.ToLower())
                {
                    case WorkFlowNames.TransitionName.UpdateStaging.ToPreview:
                        TransitionIDStagingToPreview = item.id;
                        break;
                    default:
                        break;
                }
            }
        }

        private void MapUpdatePreviewIDs(PSState workflowState)
        {
            UpdatePreviewStateID = workflowState.id;
            foreach (PSTransition item in workflowState.Transitions)
            {
                switch (item.label.ToLower())
                {
                    case WorkFlowNames.TransitionName.UpdatePreview.ToLive:
                        TransitionIDUpdatePreviewToLive = item.id;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
