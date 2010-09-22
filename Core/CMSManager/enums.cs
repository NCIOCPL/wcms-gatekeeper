using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers.CMSManager
{
    public enum WorkflowState
    {
        Invalid = 0,
        Staging = 1,
        Preview = 2,
        Live = 3,
        UpdateStaging = 4,
        UpdatePreview = 5
    }

    public enum WorkflowTransition
    {
        Invalid = 0,

        // From Staging
        PromoteToPreviewNew = 1,

        // From Preview
        PromoteToLiveNew = 2,
        RevertToStagingNew = 3,

        // From Live
        Update = 4,

        // From UpdateStaging
        PromoteToPreviewUpdate = 5,

        // From UpdatePreview
        PromoteToLiveUpdate = 6,
        RevertToStagingUpdate = 7
    }
}
