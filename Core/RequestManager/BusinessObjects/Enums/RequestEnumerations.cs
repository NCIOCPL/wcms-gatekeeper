
namespace GKManagers.BusinessObjects
{
    public enum RequestStatusType
    {
        Invalid = -1,

        Receiving = 1,
        DataReceived = 2,
        Aborted = 3
    }

    public enum RequestPublicationType
    {
        Invalid = -1,

        Hotfix = 1,
        Remove = 2,
        FullLoad = 3,
        Export = 4,
        Import = 5,
        Reload = 6
    }

    public enum RequestTargetType
    {
        Invalid = -1,

        GateKeeper = 1,
        Preview = 2,
        Live = 3
    }

    public enum SystemStatusType
    {
        Invalid = -1,

        Normal = 1,
        Stopped = 2
    }
}
