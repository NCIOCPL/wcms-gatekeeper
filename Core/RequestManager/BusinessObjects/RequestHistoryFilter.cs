using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.BusinessObjects
{
    public enum ReqHistorySortColumnType
    {
        NotSet = -1,
        RequestDateColumn = 11,
        RequestIdColumn = 12,
        ExternalRequestIdColumn = 13,
        RequestStatusColumn = 14,
        PublishingDestinationColumn = 15,
        SourceColumn = 16
    }

    public class RequestHistoryFilter
    {

        private RequestStatusType _requestStatus = RequestStatusType.Invalid;
        private RequestDataLocationType _publishingDestination = RequestDataLocationType.Invalid;
        private int _cdrID = 0;
        private int _numberOfMonth = 0;
        private ReqHistorySortColumnType _sortColumn = ReqHistorySortColumnType.NotSet;
        private SortOrderType _sortOrder = SortOrderType.NotSet;

        public int GetSelectedColumnSortHash()
        {
            return (int)_sortColumn*10 + (int)_sortOrder;
        }

        #region properties

        public RequestStatusType RequestStatus
        {
            get { return _requestStatus; }
            set { _requestStatus = value; }
        }

        public RequestDataLocationType PublishingDestination
        {
            get { return _publishingDestination; }
            set { _publishingDestination = value; }
        }

        public ReqHistorySortColumnType SortColumn
        {
            get { return _sortColumn; }
            set { _sortColumn = value; }
        }

        public SortOrderType SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }

        public int CdrID
        {
            get { return _cdrID; }
            set { _cdrID = value; }
        }

        public int NumberOfMonth
        {
            get { return _numberOfMonth; }
            set { _numberOfMonth = value; }
        }

        #endregion 
    }
}
