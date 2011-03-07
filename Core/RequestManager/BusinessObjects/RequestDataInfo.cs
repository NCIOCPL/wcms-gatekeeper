using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GKManagers;

namespace GKManagers.BusinessObjects
{
    [Serializable]
    public class RequestDataInfo
    {
        private RequestDataActionType _actionType = RequestDataActionType.Invalid;
        private RequestDataStatusType _status = RequestDataStatusType.Invalid;
        private RequestDataDependentStatusType _dependencyStatus = RequestDataDependentStatusType.Invalid;
        private RequestDataLocationType _location = RequestDataLocationType.Invalid;

        private int _requestDataID;
        private int _requestID;
        private int _packetNumber;
        private CDRDocumentType _cdrDocType;
        private int _cdrID;
        private string _cdrVersion;
        private DateTime _receivedDate;
        private int _groupID;

        public RequestDataInfo(int packetNumber, RequestDataActionType actionType, int cdrID,
            string cdrVersion, CDRDocumentType cdrDocType, RequestDataLocationType location, int groupID)
        {
            if (!Enum.IsDefined(typeof(RequestDataActionType), actionType))
                throw ExceptionBuilder.InvalidValue("actionType", actionType);
            if (!Enum.IsDefined(typeof(CDRDocumentType), cdrDocType))
                throw ExceptionBuilder.InvalidValue("cdrDocType", cdrDocType);
            if (!Enum.IsDefined(typeof(RequestDataLocationType), location))
                throw ExceptionBuilder.InvalidValue("location", location);

            _packetNumber = packetNumber;
            _actionType = actionType;
            _cdrID = cdrID;
            _cdrVersion = cdrVersion;
            _cdrDocType = cdrDocType;
            _location = location;
            _groupID = groupID;

            _requestDataID = -1;
        }

        public RequestDataInfo()
        {
            _packetNumber = -1;
            _actionType = RequestDataActionType.Invalid;
            _cdrID = -1;

            _requestDataID = -1;
        }

        #region Properties

        public int RequestDataID
        {
            get { return _requestDataID; }
            set { _requestDataID = value; }
        }

        public int RequestID
        {
            get { return _requestID; }
            set { _requestID = value; }
        }

        public int PacketNumber
        {
            get { return _packetNumber; }
            set { _packetNumber = value; }
        }

        public RequestDataActionType ActionType
        {
            get { return _actionType; }
            set { _actionType = value; }
        }

        /// <summary>
        /// CDRDocument type (based on the DataSet table)
        /// </summary>
        public CDRDocumentType CDRDocType
        {
            get { return _cdrDocType; }
            set { _cdrDocType = value; }
        }

        public DocumentType GKDocType
        {
            get { return DocumentTypeConverter.CdrToGK(_cdrDocType); }
            set { _cdrDocType = DocumentTypeConverter.GKToCdr(value); }
        }

        public int CdrID
        {
            get { return _cdrID; }
            set { _cdrID = value; }
        }

        public string CdrVersion
        {
            get { return _cdrVersion; }
            set { _cdrVersion = value; }
        }

        [XmlIgnore]
        public DateTime ReceivedDate
        {
            get { return _receivedDate; }
            set { _receivedDate = value; }
        }

        public RequestDataStatusType Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public RequestDataDependentStatusType DependencyStatus
        {
            get { return _dependencyStatus; }
            set { _dependencyStatus = value; }
        }

        public RequestDataLocationType Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public int GroupID
        {
            get { return _groupID; }
            set { _groupID = value; }
        }

        #endregion

    }
}
