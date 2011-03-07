using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers.CMSDocumentProcessing
{
    public enum WorkflowState
    {
        Invalid = 0,
        CDRStaging = 1,
        CDRPreview = 2,
        CDRLive = 3,
        CDRStagingUpdate = 4,
        CDRPreviewUpdate = 5
    }

    public enum WorkflowTransition
    {
        Invalid = 0,

        // From CDRStaging
        PromoteToCDRPreviewNew = 1,

        // From CDRPreview
        PromoteToCDRLiveNew = 2,
        RevertToCDRStagingNew = 3,

        // From CDRLive
        Update = 4,

        // From CDRStagingUpdate
        PromoteToCDRPreviewUpdate = 5,

        // From CDRPreviewUpdate
        PromoteToCDRLiveUpdate = 6,
        RevertToCDRStagingUpdate = 7
    }
}
