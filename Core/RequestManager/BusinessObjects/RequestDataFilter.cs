using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.BusinessObjects
{

    public enum ReqDataSortColumnType
    {
        NotSet = -1,
        PacketNumberColumn = 11,
        GroupIdColumn = 12,
        CdrIdColumn = 13,
        ActionTypeColumn = 14,
        DocTypeColumn = 15,
        StatusColumn = 16,
        DependencyStatusColumn = 17,
        LocationColumn = 18
    }

    public enum SortOrderType
    {
        NotSet = -1,
        Ascending = 1,
        Descending = 2
    }

    public class RequestDataFilter
    {
        private RequestDataActionType _actionType = RequestDataActionType.Invalid;
        private CDRDocumentType _docTypeId = CDRDocumentType.Invalid;
        private RequestDataStatusType _docStatus = RequestDataStatusType.Invalid;
        private RequestDataDependentStatusType _dependencyStatus = RequestDataDependentStatusType.Invalid;
        private RequestDataLocationType _requestDataLocation = RequestDataLocationType.Invalid;
        private ReqDataSortColumnType _sortColumn = ReqDataSortColumnType.NotSet;
        private SortOrderType _sortOrder = SortOrderType.NotSet;

        public int GetSelectedColumnSortHash()
        {
            return (int)_sortColumn*10 + (int)_sortOrder;
        }

        #region Properties 
        public RequestDataActionType ActionType
        {
            get { return _actionType; }
            set { _actionType = value; }
        }  

        public CDRDocumentType DocTypeId
        {
            get { return _docTypeId; }
            set { _docTypeId = value; }
        }


        public RequestDataStatusType DocStatus
        {
            get { return _docStatus; }
            set { _docStatus = value; }
        }
 

        public RequestDataDependentStatusType DependencyStatus
        {
            get { return _dependencyStatus; }
            set { _dependencyStatus = value; }
        }


        public RequestDataLocationType RequestDataLocation
        {
            get { return _requestDataLocation; }
            set { _requestDataLocation = value; }
        }


        public ReqDataSortColumnType SortColumn
        {
            get { return _sortColumn; }
            set { _sortColumn = value; }
        }


        public SortOrderType SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }

        #endregion 

    }
}
