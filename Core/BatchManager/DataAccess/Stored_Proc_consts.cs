using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.DataAccess
{
    static class StoredProcNames
    {
        private const string _spStartNextBatch = "usp_StartNextBatch";
        private const string _spResetBatchDocumentStatus = "usp_ResetBatchDocumentStatus";
        private const string _spAddBatchHistoryEntry = "usp_AddBatchHistoryEntry";
        private const string _spGetBatchList = "usp_GetBatchList";
        private const string _spGetBatchDetailsList = "usp_GetBatchDetailsList";
        private const string _spRemoveBatchFromQueue = "usp_RemoveBatchFromQueue";
        private const string _spCancelBatch = "usp_CancelBatch";
        private const string _spCompleteBatchWithErrors = "usp_CompleteBatchWithErrors";
        private const string _spGetBatch = "usp_GetBatch";
        private const string _spInsertBatchInQueue = "usp_InsertBatchInQueue";
        private const string _spAddRequestDataToBatch = "usp_AddRequestDataToBatch";
        private const string _spAddRequestToBatch = "usp_AddRequestToBatch";
        private const string _spCreateNewBatch = "usp_CreateNewBatch";
        private const string _spGetFailedBatchProcessQueueCount = "usp_GetFailedBatchProcessQueueCount";
        private const string _spUpdateBatchActions = "usp_UpdateBatchActions";
        private const string _spDeleteOldModality = "usp_DeleteOldModality"; 

        public static string SP_GET_FAILED_BATCH_COUNT
        {
            get { return _spGetFailedBatchProcessQueueCount; }
        }

        public static string SP_START_NEXT_BATCH
        {
            get { return _spStartNextBatch; }
        }

        public static string SP_RESET_BATCH_DOCUMENT_STATUS
        {
            get { return _spResetBatchDocumentStatus; }
        }

        public static string SP_ADD_BATCH_HISTORY_ENTRY
        {
            get { return _spAddBatchHistoryEntry; }
        }

        public static string SP_GET_BATCH_LIST
        {
            get { return _spGetBatchList; }
        }

        public static string SP_GET_BATCH_DETAILS_LIST
        {
            get { return _spGetBatchDetailsList; }
        }

        public static string SP_REMOVE_BATCH_FROM_QUEUE
        {
            get { return _spRemoveBatchFromQueue; }
        }

        public static string SP_CANCEL_BATCH
        {
            get { return _spCancelBatch; }
        }

        public static string SP_COMPLETE_BATCH_WITH_ERRORS
        {
            get { return _spCompleteBatchWithErrors; }
        } 

        public static string SP_GET_BATCH
        {
            get { return _spGetBatch; }
        } 

        public static string SP_INSERT_BATCH_IN_QUEUE
        {
          get { return _spInsertBatchInQueue; }  
        } 

        public static string SP_ADD_REQUEST_TO_BATCH
        {
            get { return _spAddRequestToBatch; }
        }

        public static string SP_ADD_REQUESTDATA_TO_BATCH
        {
            get { return _spAddRequestDataToBatch; }
        }

        public static string SP_CREATE_NEW_BATCH
        {
            get { return _spCreateNewBatch; }
        }

        public static string SP_UPDATE_BATCH_ACTIONS
        {
            get { return _spUpdateBatchActions; }
        }

        public static string SP_DELETE_UNUSED_MODALITY
        {
            get { return _spDeleteOldModality; }
        }
    }
}
