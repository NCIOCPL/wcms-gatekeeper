using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers.CMSManager.CMS
{
    // Collection of object names.  (Avoids spelling and casing problems.)

    internal class WorkFlowStateNames
    {
        public const string Staging = "Staging";
        public const string Preview = "Preview";
        public const string Live = "Live";
        public const string UpdateStaging = "UpdateStaging";
        public const string UpdateStaging = "UpdatePreview";
    }
}
