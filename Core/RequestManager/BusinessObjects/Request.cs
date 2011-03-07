using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using GateKeeper.Common;

namespace GKManagers.BusinessObjects
{
    [Serializable]      
    public class Request
    {
        public const int InvalidRequestID = -1;
        public const int IgnoreDocumentCount = -1;

        private int _requestID = Request.InvalidRequestID;
        private RequestPublicationType _requestPublicationType = RequestPublicationType.Invalid;
        private int _expectedDocCount = Request.IgnoreDocumentCount;
        private int _actualDocCount = -1;
        private int _maxPacketNumber = -1;
        private RequestTargetType _publicationTarget = RequestTargetType.Invalid;
        private RequestStatusType _status = RequestStatusType.Invalid;

        private string _externalRequestID;
        private string _description;
        private string _source;
        private string _dtdVersion;
        private string _dataType;
        private DateTime _initiateDate = DateTime.MinValue;
        private DateTime _completeReceivedTime = DateTime.MinValue;
        private DateTime _updateDate = DateTime.MinValue;
        private string _updateUserID;

        //Collection
        private RequestData[] _requestDataArray;
        
        #region Properties


        public int RequestID
        {
            get { return _requestID; }
            set { _requestID = value; }
        }
        
        [XmlIgnore]
        public string ExternalRequestID
        {
            get { return _externalRequestID; }
            set { _externalRequestID = value; }
        }
        
        [XmlIgnore]
        public RequestPublicationType RequestPublicationType
        {
            get { return _requestPublicationType; }
            set { _requestPublicationType = value; }
        }
        
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        
        [XmlIgnore]
        public RequestStatusType Status
        {
            get { return _status; }
            set { _status = value; }
        }
        
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }
        
        public string DtdVersion
        {
            get { return _dtdVersion; }
            set { _dtdVersion = value; }
        }
        
        [XmlIgnore]
        public int ExpectedDocCount
        {
            get { return _expectedDocCount; }
            set { _expectedDocCount = value; }
        }
        
        [XmlIgnore]
        public int ActualDocCount
        {
            get { return _actualDocCount; }
            set { _actualDocCount = value; }
        }
        
        public string DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }
        
        [XmlIgnore]
        public DateTime InitiateDate
        {
            get { return _initiateDate; }
            set { _initiateDate = value; }
        }
        
        [XmlIgnore]
        public DateTime CompleteReceivedTime
        {
            get { return _completeReceivedTime; }
            set { _completeReceivedTime = value; }
        }
        
        [XmlIgnore]
        public RequestTargetType PublicationTarget
        {
            get { return _publicationTarget; }
            set { _publicationTarget = value; }
        }
        
        [XmlIgnore]
        public DateTime UpdateDate
        {
            get { return _updateDate; }
            set { _updateDate = value; }
        }
        
        [XmlIgnore]
        public string UpdateUserID
        {
            get { return _updateUserID; }
            set { _updateUserID = value; }
        }
        
        public int MaxPacketNumber
        {
            get { return _maxPacketNumber; }
            set { _maxPacketNumber = value; }
        }
        
        public RequestData[] RequestDatas
        {
            get { return _requestDataArray; }
            set { _requestDataArray = value; }
        }

        #endregion


        public Request()
        {
        }

        public Request(string externalRequestID, string source, RequestPublicationType requestPublicationType, 
            RequestTargetType publicationTarget, string description, 
            string dataType, string dtdVersion, string userID)
        {
            if (!Enum.IsDefined(typeof(RequestPublicationType), requestPublicationType))
            {
                RequestMgrLogBuilder.Instance.CreateError(this.GetType(), "(constructor)",
                    string.Format("Invalid requestPublicationType value: {0}.", requestPublicationType.ToString()));
                throw ExceptionBuilder.InvalidValue("requestPublicationType", requestPublicationType);
            }
            if (!Enum.IsDefined(typeof(RequestTargetType), publicationTarget))
            {
                RequestMgrLogBuilder.Instance.CreateError(this.GetType(), "(constructor)",
                    string.Format("Invalid publicationTarget value: {0}.", publicationTarget.ToString()));
                throw ExceptionBuilder.InvalidValue("publicationTarget", publicationTarget);
            }

            _externalRequestID = externalRequestID;
            _source = source;
            _requestPublicationType = requestPublicationType;
            _publicationTarget = publicationTarget;
            _description = description;
            _dataType = dataType;
            _dtdVersion = dtdVersion;
            _updateUserID = userID;
        }


    }
}
