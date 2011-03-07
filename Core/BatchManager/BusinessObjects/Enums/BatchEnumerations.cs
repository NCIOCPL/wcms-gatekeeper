using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.BusinessObjects
{
    public enum BatchStatusType
    {
        Invalid = -1,

        Queued = 1,
        Processing = 2,
        Cancelled = 3,
        Complete = 4,
        CompleteWithErrors = 5,
        Reviewed = 6
    }

    public enum ProcessActionType
    {
        Invalid = -1,

        PromoteToStaging = 1,
        PromoteToPreview = 2,
        PromoteToLive = 3
    }

    enum BatchListFilterType
    {
        Invalid = -1,

        RetrieveAll = 0,
        RetrieveActive = 1,
        RetrieveError = 2
    }
}
