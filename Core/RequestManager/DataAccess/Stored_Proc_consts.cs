using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.DataAccess
{
    static class StoredProcNames
    {
        private const string _spGetRequestStatusFromDocumentID = "usp_GetRequestStatusFromDocumentID";
        private const string _spGetRequestStatus = "usp_GetRequestStatus";
        private const string _spGetRequestDataIdentifiers = "usp_GetRequestDataIdentifiers";
        private const string _spGetGatekeeperControlValue = "usp_GetGatekeeperControlValue";
        private const string _spSetGatekeeperControlValue = "usp_SetGatekeeperControlValue";
        private const string _spGetMostRecentExternalID = "usp_GetMostRecentExternalID";
        private const string _spGetDocumentStatusMap = "usp_GetDocumentStatusMap";
        private const string _spMarkDocumentWithErrors = "usp_MarkDocumentWithErrors";
        private const string _spMarkDocumentWithWarnings = "usp_MarkDocumentWithWarnings";
        private const string _spGetDocumentLocationMap = "usp_GetDocumentLocationMap";
        private const string _spSaveDocumentLocation = "usp_SaveDocumentLocation";
        private const string _spAddRequestDataHistoryEntry = "usp_AddRequestDataHistoryEntry";
        private const string _spLoadRequestDataIDList = "usp_GetRequestDataIDList";
        private const string _spLoadRequestDataByID = "usp_GetRequestDataByID";
        private const string _spLoadRequestDataInfo = "usp_GetRequestDataInfo";
        private const string _spLoadRequestDataByCdrid = "usp_GetRequestDataByCdrid";
        private const string _spLoadRequestByID = "usp_GetRequestByID";
        private const string _spLoadRequestByExternalID = "usp_GetRequestByExternalID";
        private const string _spCreateNewRequest = "usp_CreateNewRequest";
        private const string _spInsertRequestData = "usp_InsertRequestData";
        private const string _spAbortRequest = "usp_AbortRequest";
        private const string _spCompleteRequest = "usp_CompleteRequest";
        private const string _spSearchRequestData = "usp_SearchRequestData";
        private const string _spSearchRequest = "usp_SearchRequest";
        private const string _spGetRequestDataByGroupID = "usp_getRequestDataByGroupID";
        private const string _spGetDataHistory = "usp_GetRequestDataHistory";
        private const string _spGetBatchHistory = "usp_GetBatchHistory";
        private const string _spGetBatchAction = "usp_GetBatchAction";
        private const string _spGetRequestBatchHistory = "usp_GetRequestBatchHistory";
        private const string _usp_GetRequestDataForCDRLocations = "usp_GetRequestDataForCDRLocations";
        private const string _usp_GetRequestDataByDataIDList = "usp_GetRequestDataByDataIDList";
        private const string _spGetReports = "usp_GetReports";
        private const string _spGetRequestCounts = "usp_GetRequestCounts";
        private const string _spGetRequestLocation = "usp_GetRequestLocation";
        private const string _spGetRequestLocation_External = "usp_GetRequestLocation_External";
        private const string _spGetRequestPublicationType = "usp_GetRequestPublicationType";
        private const string _spGenerateComplementaryRemoveRequest = "usp_GenerateComplementaryRemoveRequest";
        private const string _spAddRequestComment = "usp_AddRequestComment";
        private const string _spAddRequestCommentExternalID = "usp_AddRequestCommentExternalID"; 

        public static string SP_GET_REQUEST_STATUS_FROM_DOCUMENT_ID
        {
            get { return _spGetRequestStatusFromDocumentID; }
        }

        public static string SP_GET_REQUEST_STATUS
        {
            get { return _spGetRequestStatus; }
        }

        public static string SP_GET_REQUEST_DATA_BY_DATA_ID_LIST
        {
            get { return _usp_GetRequestDataByDataIDList; }
        }

        public static string SP_GET_REQUEST_DATA_FOR_CDR_LOCATIONS
        {
            get { return _usp_GetRequestDataForCDRLocations; }
        }

        public static string SP_GET_REQUEST_DATA_IDENTIFIERS
        {
            get { return _spGetRequestDataIdentifiers; }
        }

        public static string SP_GET_GATEKEEPER_CONTROL_VALUE
        {
            get { return _spGetGatekeeperControlValue; }
        }

        public static string SP_SET_GATEKEEPER_CONTROL_VALUE
        {
            get { return _spSetGatekeeperControlValue; }
        }

        public static string SP_GET_MOST_RECENT_EXTERNAL_ID
        {
            get { return _spGetMostRecentExternalID; }
        }

        public static string SP_GET_REQUEST_DOCUMENT_STATUS_MAP
        {
            get { return _spGetDocumentStatusMap; }
        }

        public static string SP_MARK_DOCUMENT_WITH_ERRORS
        {
            get { return _spMarkDocumentWithErrors; }
        }

        public static string SP_MARK_DOCUMENT_WITH_WARNINGS
        {
            get { return _spMarkDocumentWithWarnings; }
        }

        public static string SP_GET_DOCUMENT_LOCATION_MAP
        {
            get { return _spGetDocumentLocationMap; }
        }

        public static string SP_SAVE_DOCUMENT_LOCATION
        {
            get { return _spSaveDocumentLocation; }
        }

        public static string SP_ADD_REQUEST_DATA_HISTORY_ENTRY
        {
            get { return _spAddRequestDataHistoryEntry; }
        }

        public static string SP_LOAD_REQUEST_DATA_ID_LIST
        {
            get { return _spLoadRequestDataIDList; }
        }

        public static string SP_LOAD_REQUEST_DATA_BY_ID
        {
            get { return _spLoadRequestDataByID; }
        }

        public static string SP_LOAD_REQUEST_DATA_INFO
        {
            get { return _spLoadRequestDataInfo; }
        }

        public static string SP_LOAD_REQUEST_DATA_BY_CDRID
        {
            get { return _spLoadRequestDataByCdrid; }
        }

        public static string SP_ABORT_REQUEST
        {
            get { return _spAbortRequest; }
        }

        public static string SP_COMPLETE_REQUEST
        {
            get { return _spCompleteRequest; }
        }

        public static string SP_INSERT_REQUEST_DATA
        {
            get { return _spInsertRequestData; }
        }

        public static string SP_CREATE_NEW_REQUEST
        {
            get { return _spCreateNewRequest; }
        }

        public static string SP_LOAD_REQUEST_BY_EXTERNAL_ID
        {
            get { return _spLoadRequestByExternalID; }
        }

        public static string SP_LOAD_REQUEST_BY_ID
        {
            get { return _spLoadRequestByID; }
        }

        public static string SP_SEARCH_REQUEST_DATA
        {
            get { return _spSearchRequestData; }
        }

        public static string SP_SEARCH_REQUEST
        {
            get { return _spSearchRequest; }
        }

        public static string SP_GET_REQUEST_DATA_BY_GROUP_ID
        {
            get { return _spGetRequestDataByGroupID; }
        }

        public static string SP_GET_DATA_HISTORY
        {
            get { return _spGetDataHistory; }
        }

        public static string SP_GET_BATCH_HISTORY
        {
            get { return _spGetBatchHistory; }
        }

        public static string SP_GET_BATCH_ACTION
        {
            get { return _spGetBatchAction; }
        }

        public static string SP_GET_REQUEST_BATCH_HISTORY
        {
            get { return _spGetRequestBatchHistory; }
        }

        public static string SP_GET_REPORTS
        {
            get { return _spGetReports; }
        }

        public static string SP_GET_REQUEST_COUNTS
        {
            get { return _spGetRequestCounts; }
        }

        public static string SP_GET_REQUEST_LOCATION
        {
            get { return _spGetRequestLocation; }
        }

        public static string SP_GET_REQUEST_LOCATION_EXTERNAL_IDS
        {
            get { return _spGetRequestLocation_External; }
        }

        public static string SP_GET_REQUEST_PUBLICATION_TYPE
        {
            get { return _spGetRequestPublicationType; }
        }

        public static string SP_GENERATE_COMPLEMENTARY_REMOVE_REQUEST
        {
            get { return _spGenerateComplementaryRemoveRequest; }
        }

        public static string SP_ADD_REQUEST_COMMENT
        {
            get { return _spAddRequestComment; }
        }

        public static string SP_ADD_REQUEST_COMMENT_EXTERNAL_ID
        {
            get { return _spAddRequestCommentExternalID; }
        }
    }
}
