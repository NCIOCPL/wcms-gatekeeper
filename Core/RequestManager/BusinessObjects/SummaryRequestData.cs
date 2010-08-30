using System;

namespace GKManagers.BusinessObjects
{
    public class SummaryRequestData : RequestData
    {

        public SummaryRequestData(int packetNumber, RequestDataActionType actionType, int cdrID,
            string cdrVersion, CDRDocumentType cdrDocType, RequestDataLocationType location,
            int groupID, string documentData)
            :
            base(packetNumber, actionType, cdrID, cdrVersion, cdrDocType, location, groupID, documentData)
        {
            // Class-specific intitialization goes here.
        }

    }
}
