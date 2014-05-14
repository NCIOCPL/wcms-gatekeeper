using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers.CMSDocumentProcessing
{
   
    /// <summary>
    /// List of all the possible workflow states
    /// These numbers should map to workflow states, however we do not use the numbers directly
    /// as they may change between Percussion environments
    /// </summary>
    public enum WorkflowState
    {
        Invalid = 0,
        CDRStaging = 1,
        CDRPreview = 2,
        CDRLive = 3,
        CDRStagingUpdate = 4,
        CDRPreviewUpdate = 5
    }

    /// <summary>
    /// List of all the possible transitions between workflow states
    /// These numbers should map to workflow transition ids, however we do not use the numbers directly
    /// as they may change between Percussion environments
    /// </summary>
    public enum WorkflowTransition
    {
        Invalid = 0,

        // From CDRStaging
        PromoteToCDRPreviewNew = 1,
        PromoteToCDRLiveFastNew = 9,

        // From CDRPreview
        PromoteToCDRLiveNew = 2,
        RevertToCDRStagingNew = 3,

        // From CDRLive
        Update = 4,

        // From CDRStagingUpdate
        PromoteToCDRPreviewUpdate = 5,
        PromoteToCDRLiveFastUpdate = 10,

        // From CDRPreviewUpdate
        PromoteToCDRLiveUpdate = 6,
        RevertToCDRStagingUpdate = 7
    }
}
