using System;

namespace GKManagers.BusinessObjects
{
    public static class RequestDataFactory
    {
        static public RequestData Create(CDRDocumentType docType, int packetNumber, 
            RequestDataActionType actionType, int cdrID, string cdrVersion, 
            RequestDataLocationType location, int groupID, string documentData)
        {
            RequestData theObject = null;

            if (docType == CDRDocumentType.Summary)
            {
                theObject = 
                    new SummaryRequestData(packetNumber, actionType, cdrID, cdrVersion, docType,
                        location, groupID, documentData);
            }
            else
            {
                theObject =
                    new RequestData(packetNumber, actionType, cdrID, cdrVersion, docType,
                        location, groupID, documentData);
            }

            return theObject;
        }
    }
}
