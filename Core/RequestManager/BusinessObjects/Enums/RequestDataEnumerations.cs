
namespace GKManagers.BusinessObjects
{
    public enum RequestDataStatusType
    {
        Invalid = -1,

        OK = 1,
        Error = 2,
        Warning = 3
    }

    public enum RequestDataDependentStatusType
    {
        Invalid = -1,

        OK = 1,
        Error = 2
    }

    public enum RequestDataLocationType
    {
        Invalid = -1,

        GateKeeper = 1,
        Staging = 2,
        Preview = 3,
        Live = 4
    }

    public enum RequestDataActionType
    {
        Invalid = -1,

        Export = 1,
        Remove = 2
    }

    public enum CDRDocumentType
    {
        Invalid = -1,   // Not a real document type.

        /// Values defined in the dataSetID table.  Any changes to this enum
        /// MUST reflect the DocumentType table.
        GlossaryTerm = 1,
        Term = 4,
        InScopeProtocol = 6,
        Summary = 7,
        Organization = 8, 
        MiscellaneousDocument = 9,
        OutOfScopeProtocol = 10,
        PoliticalSubUnit = 11,
        GENETICSPROFESSIONAL = 12,
        Protocol = 13,
        CTGovProtocol = 14,
        Media = 15,
        DrugInformationSummary = 16
    }

    public enum RequestDataHistoryType
    {
        Invalid = -1,

        Information = 1,
        Warning = 2,
        Error = 3,
        Debug = 4   // Debug information
    }
}
