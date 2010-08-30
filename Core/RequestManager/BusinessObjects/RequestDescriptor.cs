using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.BusinessObjects
{
    class RequestDescriptor
    {

        private int _requestID = -1;
        private string _externalRequestID;
        private string _source;
        private string _description;

        private RequestPublicationType _requestPublicationType = RequestPublicationType.Invalid;
        private RequestTargetType _publicationTarget = RequestTargetType.Invalid;
        private RequestStatusType _status = RequestStatusType.Invalid;
        private string _initiateDate;

        private int _actualDocCount = -1;
        private int _exportDocCount = -1;
        private int _removeDocCount = -1;
        private int _previewDocCount = -1;
        private int _liveDocCount = -1;
        private int _errorCount = -1;
        private int _warningCount = -1; 

        public RequestDescriptor()
        {
        }

        public RequestDescriptor(int requestId, string externalRequestID, string source, string description, 
            RequestPublicationType requestPublicationType,
            RequestTargetType publicationTarget, RequestStatusType status, 
            string dataType, string initiateDate, 
            int actualDocCount, int exportDocCount, int removeDocCount, int previewDocCount, 
            int liveDocCount, int errorCount, int warningCount)
        {
            _requestID = requestId;
            _externalRequestID = externalRequestID;
            _source = source;
            _description = description; 
            _requestPublicationType = requestPublicationType;
            _publicationTarget = publicationTarget;
            _status = status;
            _initiateDate = initiateDate;


            _actualDocCount = actualDocCount;
            _exportDocCount = exportDocCount;
            _removeDocCount = removeDocCount;
            _previewDocCount = previewDocCount;
            _liveDocCount = liveDocCount;
            _errorCount = errorCount;
            _warningCount = warningCount; 

        }
    }
}
