using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers.CMSManager.CMS
{
    // Collection of object names.  (Avoids spelling and casing problems.)

    /// <summary>
    /// Hierarchy of names used for controlling the Workflow
    /// </summary>
    internal struct WorkFlowNames
    {
        public const string WorkFlow = "CancerGov_PDQ_Workflow";

        public struct StateName
        {
            public const string Staging = "staging";
            public const string Preview = "preview";
            public const string Live = "live";
            public const string UpdateStaging = "updatestaging";
            public const string UpdatePreview = "updatepreview";
        }

        /// <summary>
        /// Hierarchy of transitions for each state.
        /// </summary>
        public struct TransitionName
        {
            public struct Staging
            {
                public const string ToPreview = "promotetopreviewnew";
            }

            public struct Preview
            {
                public const string ToLive = "promotetolivenew";
            }

            public struct Live
            {
                public const string ToStaging = "update";
            }

            public struct UpdateStaging
            {
                public const string ToPreview = "promotetopreviewupdate";
            }

            public struct UpdatePreview
            {
                public const string ToLive = "promotetoliveupdate";
            }
        }
    }
}
