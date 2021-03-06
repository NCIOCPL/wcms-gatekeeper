using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

using GateKeeper.Common;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
using GKManagers.BusinessObjects;

using NCI.WCM.CMSManager.CMS;
using GKManagers.DataAccess;
using GateKeeper.DocumentObjects.Summary;

namespace GKManagers
{
    public class DocumentManager
    {
        #region Delegates
        private delegate int VersionComparer(int cdrid, int requestID);
        private delegate bool ExistenceCheck(int cdrid);
        private delegate bool GroupLocation(int groupid);
        #endregion

        #region Fields
        
        private static bool _processingIsAllowed = true;

        #endregion

        #region Constants

        // Shortcut for the multiple document types which are maintained in the CMS.
        const DocumentTypeFlag CmsManagedTypes =
            DocumentTypeFlag.DrugInformationSummary | DocumentTypeFlag.Summary | DocumentTypeFlag.Media;

        #endregion

        #region Properties

        public static bool ProcessingIsAllowed
        {
            get { return DocumentManager._processingIsAllowed; }
            set { DocumentManager._processingIsAllowed = value; }
        }

        #endregion

        /// <summary>
        /// Oversees the promotion of an individual batch through the associated
        /// list of batch actions.
        /// </summary>
        /// <param name="batchID">Unique identifier for the batch to be promoted.</param>
        public static void PromoteBatch(int batchID, string userName)
        {
            // Used to get threads back in sync.
            //ManualResetEvent[] synchronizer = null;
            //PromotionWorker[] workerThreads = null;

            // Used for tracking the collected document types in the batch.
            DocumentTypeTracker documentTypeTracker = null;

            if (batchID < 1)
                throw new ArgumentException("batchID");
            if (userName == null || userName.Length == 0)
                throw new ArgumentNullException("userName");
            
            /// 1. Retrieve the list of RequestData items associated with the batch.
            Batch currentBatch = null;
            currentBatch = BatchManager.LoadBatch(batchID);

            BatchManager.AddBatchHistoryEntry(batchID, currentBatch.UserName, "Begin processing");
            BatchManager.ResetBatchDocumentStatus(batchID);

            // 2. For each action in the Batch Actions list.
            //     For each RequestData item:
            List<ProcessActionType> actionList = currentBatch.Actions;
            List<int> requestDataIDList = currentBatch.RequestDataIDs;

            DocumentXPathManager xPathManager = new DocumentXPathManager();
            bool promotionWasSuccessful = true;

            DocumentVersionMap locationMap = null;
            DocumentStatusMap statusMap = null;
            RequestDataInfo docData = null;
            Request theRequest = null;
            bool validationRequired = true;

            // Tracks whether the first pass through the innermost loop has been executed.
            bool isFirstPass = true;
            object suppressSetting = ConfigurationManager.AppSettings["suppressMultiThreading"];
            bool suppressMultiThreading = (suppressSetting != null) ? Strings.ToBoolean(suppressSetting) : false;

            // Load Summary split metadata object.
            string splitDataFile = ConfigurationManager.AppSettings["summary-split-file-location"];
            using (SplitDataManager splitData = SplitDataManager.Create(splitDataFile))
            {

                try
                {
                    foreach (ProcessActionType action in actionList)
                    {
                        // Determine whether multi-threading is allowed in this pass.
                        // Only promotion to staging is ever multi-threaded. Everything else is multi-threaded.
                        bool useMultiThreading;
                        if (action == ProcessActionType.PromoteToStaging && !suppressMultiThreading)
                            useMultiThreading = true;
                        else
                            useMultiThreading = false;

                        // Get a new type tracker for each promotion action.
                        documentTypeTracker = new DocumentTypeTracker();

                        BatchManager.AddBatchHistoryEntry(batchID, currentBatch.UserName,
                            string.Format("Start promotion to {0}", ActionToLocation(action)));

                        SynchronizedIDQueue queueForRegularDocs = new SynchronizedIDQueue();
                        SynchronizedIDQueue queueForCmsDocs = new SynchronizedIDQueue();
                        List<int> delayedSummaryIDList = new List<int>();
                        List<int> mediaDocumentIDList = new List<int>();

                        for (int docIndex = 0; docIndex < requestDataIDList.Count; ++docIndex)
                        {
                            /// The _processingIsAllowed flag goes to false if an attempt is made 
                            /// to shut the system down during a series of promotions.
                            if (!_processingIsAllowed)
                            {
                                BatchManager.AddBatchHistoryEntry(batchID, userName,
                                    "DocumentManager: Promotion System is being shut down.");
                                throw new Exception("DocumentManager: Promotion Halted.");
                            }

                            docData = RequestManager.LoadRequestDataInfo(requestDataIDList[docIndex]);

                            /// We need to load the first RequestData object before we can load the location map
                            /// or the request object.  These operations only occur on the first pass through the
                            /// innermost loop.
                            if (isFirstPass)
                            {
                                locationMap = RequestManager.LoadDocumentLocationMap(docData.RequestID);
                                theRequest = RequestManager.LoadRequestByID(docData.RequestID);
                                validationRequired = MustValidate(theRequest.DtdVersion);
                                isFirstPass = false;
                            }

                            // Keep track of what type of document is being promoted.
                            documentTypeTracker.AddDocumentType(docData.CDRDocType);

                            // Status map must be reloaded for each iteration through the list of
                            // promotion levels.
                            if (statusMap == null)
                                statusMap = RequestManager.LoadDocumentStatusMap(docData.RequestID);

                            // Is it OK to promote the document?
                            if (IsDocumentOKToPromote(action, batchID, docData, locationMap, statusMap))
                            {
                                DocumentTypeFlag docTypeFlag = DocumentTypeFlagUtil.MapDocumentType(docData.CDRDocType);

                                // Summary translations need to be scheduled after their
                                // original versions. But do not check for existence of translation when
                                // the document is being remove, since during remove there is no translation
                                // language information due to the absence of document data. 
                                if (docData.ActionType != RequestDataActionType.Remove &&
                                     docTypeFlag == DocumentTypeFlag.Summary &&
                                     SummaryIsATranslation(docData))
                                {
                                    delayedSummaryIDList.Add(requestDataIDList[docIndex]);
                                }
                                // Media documents require special scheduling so they go in ahead of
                                // anything which might reference them.
                                else if (docTypeFlag == DocumentTypeFlag.Media)
                                {
                                    mediaDocumentIDList.Add(requestDataIDList[docIndex]);
                                }
                                else if (useMultiThreading)
                                {
                                    // Place documents in the appropriate queue.
                                    if (DocumentTypeFlagUtil.FlagsOverlap(docTypeFlag, CmsManagedTypes))
                                        queueForCmsDocs.Enqueue(requestDataIDList[docIndex]);
                                    else
                                        queueForRegularDocs.Enqueue(requestDataIDList[docIndex]);
                                }
                                else
                                {
                                    // For single threading, use one queue.
                                    queueForRegularDocs.Enqueue(requestDataIDList[docIndex]);
                                }
                            }
                            else
                            {
                                // Errors are logged and the document is marked as having errors
                                // during the call to IsDocumentOKToPromote.
                                promotionWasSuccessful = false;
                            }
                        }

                        // Merge delayed summaries back into the appropriate queue.
                        if (useMultiThreading)
                        {
                            // All of the document types with scheduling depependencies are
                            // controlled by the CMS.

                            // Rebuild the list of IDs.  First the media documents,
                            // Second the items without scheduling dependencies.
                            // Third the summary translations.
                            List<int> tempList = new List<int>();
                            tempList.AddRange(mediaDocumentIDList);
                            tempList.AddRange(queueForCmsDocs.ToArray());
                            tempList.AddRange(delayedSummaryIDList);

                            // Reload the CMS queue.
                            queueForCmsDocs.Clear();
                            tempList.ForEach(queueItem => queueForCmsDocs.Enqueue(queueItem));
                        }
                        else
                        {
                            // All of the document types with scheduling depependencies are
                            // controlled by the CMS, but because this is single threaded,
                            // they all end up in the regular item queue.  We still
                            // have to resort them though.

                            //delayedSummaryIDList.ForEach(summaryID => queueForRegularDocs.Enqueue(summaryID));
                            // Rebuild the list of IDs.  First the media documents,
                            // Second the items without scheduling dependencies.
                            // Third the summary translations.
                            List<int> tempList = new List<int>();
                            tempList.AddRange(mediaDocumentIDList);
                            tempList.AddRange(queueForRegularDocs.ToArray());
                            tempList.AddRange(delayedSummaryIDList);

                            // Reload the CMS queue.
                            queueForRegularDocs.Clear();
                            tempList.ForEach(queueItem => queueForRegularDocs.Enqueue(queueItem));
                        }


                        // Run the individual promotions.
                        PromoteQueuedDocuments(batchID, action, validationRequired, currentBatch.UserName, xPathManager,
                            locationMap, statusMap, useMultiThreading, queueForRegularDocs, queueForCmsDocs,
                            out promotionWasSuccessful);

                        // Update the document location map's group list to reflect promotions.
                        locationMap.RefreshGroupPresence();

                        // Update caches
                        UpdateDocumentCache(documentTypeTracker, action, currentBatch);

                        LaunchCMSPublishing(documentTypeTracker, action, currentBatch);

                        BatchManager.AddBatchHistoryEntry(batchID, currentBatch.UserName,
                            string.Format("Finish promotion to {0}", ActionToLocation(action)));
                        BatchManager.CompletePromotionStep(batchID, action);

                        // Status map must be reloaded for each promotion level.
                        statusMap = null;
                    }
                }
                // Catch exceptions at the batch level.  If this catch is ever triggered, something
                // is very wrong.
                catch (Exception ex)
                {
                    promotionWasSuccessful = false;
                    DocMgrLogBuilder.Instance.CreateError(typeof(DocumentManager), "PromoteBatch", ex);
                    BatchManager.AddBatchHistoryEntry(currentBatch.BatchID, currentBatch.UserName,
                        string.Format("An error occurred during processing. Message text: {0} Stack: {1}",
                            ExceptionHelper.RetrieveMessage(ex), ex.StackTrace));
                }
            }



            // 3. Was the entire batch promoted successfully?
            if (promotionWasSuccessful)
            {
                // Mark batch complete and remove from queue.
                BatchManager.CompleteBatch(currentBatch, currentBatch.UserName);

                // Run routine to clean up the unused modality 
                BatchManager.CleanupUnusedModality(currentBatch.BatchID, currentBatch.UserName, actionList);
            }
            else
            {
                // Mark as complete with errors.
                String fmt = "Batch ID {0} ('{1}') was completed with errors at {2} on {3}.";
                String message = String.Format(fmt, currentBatch.BatchID, currentBatch.BatchName,
                    DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString());

                BatchManager.CompleteBatch(currentBatch, currentBatch.UserName, true);
                DocMgrLogBuilder.Instance.CreateInformation(typeof(DocumentManager),
                    "PromototionCompleteWithErrors", message);
            }
        }

        private static void PromoteQueuedDocuments(int batchID, ProcessActionType action, bool validationRequired,
            string userName, DocumentXPathManager xPathManager, DocumentVersionMap locationMap,
            DocumentStatusMap statusMap, bool allowMultiThreading, SynchronizedIDQueue queueForRegularDocs,
            SynchronizedIDQueue queueForCmsDocs, out bool promotionWasSuccessful)
        {
            // Determine number of threads to allow.
            int workerThreadCount = Strings.ToInt(ConfigurationManager.AppSettings["MaxWorkerThreads"], 4);

            SetThreadPoolSize();

            if (allowMultiThreading)
            {
                // For multi-threading, require at least one thread per queue.
                if (workerThreadCount < 2)
                    workerThreadCount = 2;
            }
            else
            {
                // Single threaded only gets one thread.
                workerThreadCount = 1;
            }

            PromotionWorker[] workerThreads = new PromotionWorker[workerThreadCount];

            // Allocate synchronizers as non-signalled (blocking).
            ManualResetEvent[] synchronizers = new ManualResetEvent[workerThreadCount];
            for (int i = 0; i < workerThreadCount; i++)
                synchronizers[i] = new ManualResetEvent(false);

            if (allowMultiThreading)
            {
                // We are guaranteed to have at least two threads in this case.
                // First worker thread always uses the CMS queue.
                workerThreads[0] = new PromotionWorker(synchronizers[0], queueForCmsDocs, batchID, action,
                    validationRequired, userName, xPathManager, locationMap, statusMap);
                for (int i = 1; i < workerThreadCount; i++)
                {
                    workerThreads[i] = new PromotionWorker(synchronizers[i], queueForRegularDocs, batchID, action,
                        validationRequired, userName, xPathManager, locationMap, statusMap);
                }
            }
            else
            {   
                // Single-threaded
                workerThreads[0] = new PromotionWorker(synchronizers[0], queueForRegularDocs, batchID, action,
                    validationRequired, userName, xPathManager, locationMap, statusMap);
            }

            // Start all worker threads
            Array.ForEach(workerThreads, thread => ThreadPool.QueueUserWorkItem(thread.PromotionCallback));

            WaitForAllThreads(synchronizers);

            // Promotion success is determined by ANDing the success status of
            // all the worker threads.
            bool allSuccessful = true;  // out parameters aren't allowed in lambdas, so we need a temp.
            Array.ForEach(workerThreads, thread => allSuccessful &= thread.AllPromotionsSucceeded);
            promotionWasSuccessful = allSuccessful;
        }

        /// <summary>
        /// Determines whether processing of a Summary document must be delayed.
        /// </summary>
        /// <param name="docData">Meta-data for the RequestData object representing the sumamry.</param>
        /// <returns>True if the Summary </returns>
        /// <remarks>Translations of Summary documents contain references to the English versions,
        /// but not the other way around.  As a result, these summaries must be processed after
        /// all English versions.
        /// 
        /// Having this logic here completely violates the abstraction of "We're just moving documents."
        /// </remarks>
        private static bool SummaryIsATranslation(RequestDataInfo docData)
        {
            RequestData data = RequestManager.LoadRequestDataByID(docData.RequestDataID);
            XmlDocument summaryDoc = data.DocumentData;
            XPathNavigator xNav = summaryDoc.CreateNavigator();
            Language lang = DocumentHelper.DetermineLanguageString(DocumentHelper.GetXmlDocumentValue(xNav, "/Summary/SummaryMetaData/SummaryLanguage"));

            if (lang == Language.English)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Waits for a set of ManualResetEvents to all acheive signalled status.
        /// This method is *NOT* safe for use with threads which wait on shared resources.
        /// Fortunately, our threads
        /// </summary>
        /// <param name="synchronizer"></param>
        private static void WaitForAllThreads(ManualResetEvent[] synchronizer)
        {
            if (synchronizer == null)
                throw new ArgumentNullException("synchronizer");

            foreach (ManualResetEvent synch in synchronizer)
            {
                synch.WaitOne();
            }

        }

        /// <summary>
        /// Clears the protocol cache and refreshes the pretty URL list based on
        /// the promotion stage which has just been completed.
        /// </summary>
        /// <param name="documentTypeTracker">Reference to a DocumentTypeTracker object
        /// containing a list of document types which have been successfully promoted
        /// to the current stage.</param>
        /// <param name="action">A ProcessActionType value which has been performed.
        /// Used to determine which environment should be updated.</param>
        /// <param name="batchInfo">Batch object containing metadata for the
        /// promotion batch.</param>
        private static void UpdateDocumentCache(DocumentTypeTracker documentTypeTracker,
                ProcessActionType action, Batch batchInfo)
        {
            // Caches only exists on Preview & Live. There's nothing to do on Staging.
            if (action == ProcessActionType.PromoteToPreview ||
                action == ProcessActionType.PromoteToLive ||
                action == ProcessActionType.PromoteToLiveFast)
            {
                // Shortcut for multiple types.
                DocumentTypeFlag protocolCacheTypes = DocumentTypeFlag.Protocol | DocumentTypeFlag.CTGovProtocol;

                // Which database does the cache live on?
                ContentDatabase contentDB;
                using (ProtocolQuery query = new ProtocolQuery())
                {
                    // Clear the protocol cache if any protocol documents were promoted.
                    if (documentTypeTracker.Contains(protocolCacheTypes))
                    {
                        if (action == ProcessActionType.PromoteToPreview)
                            contentDB = ContentDatabase.Preview;
                        else
                            contentDB = ContentDatabase.Live;

                        BatchManager.AddBatchHistoryEntry(batchInfo.BatchID, batchInfo.UserName,
                            string.Format("Clearing search cache on {0} database.", contentDB));
                        query.ClearSearchCache(contentDB);

                        BatchManager.AddBatchHistoryEntry(batchInfo.BatchID, batchInfo.UserName,
                            string.Format("Starting protocol re-indexing on {0} database.", contentDB));
                        //Reindex protocol full text after clearing cache
                        query.ReindexProtocolFullText(contentDB);
                    }
                }
            }
        }

        /// <summary>
        /// Triggers the CMS to publish documents which have been updated.
        /// </summary>
        /// <param name="documentTypeTracker">Reference to a DocumentTypeTracker object
        /// containing a list of document types which have been successfully updated.</param>
        /// <param name="action">The ProcessActionType which has been performed in
        /// updating the documents in the DocumentTypeTracker object.
        /// Used to determine which environment should be updated.</param>
        /// <param name="batchInfo">Batch object containing metadata for the promotion batch.</param>
        private static void LaunchCMSPublishing(DocumentTypeTracker documentTypeTracker,
                ProcessActionType action, Batch batchInfo)
        {

            // Publishing only takes place for Preview & Live. There's nothing to do on Staging.
            if ((action == ProcessActionType.PromoteToPreview || action == ProcessActionType.PromoteToLive || action == ProcessActionType.PromoteToLiveFast)
                && documentTypeTracker.Contains(CmsManagedTypes))
            {
                CMSController controller = new CMSController();

                List<CMSController.CMSPublishingTarget> target = new List<CMSController.CMSPublishingTarget>();

                if (action == ProcessActionType.PromoteToPreview)
                {
                    target.Add(CMSController.CMSPublishingTarget.CDRPreview); 
                }
                else if (action == ProcessActionType.PromoteToLive)
                {
                    target.Add(CMSController.CMSPublishingTarget.CDRLive);
                }
                else if (action == ProcessActionType.PromoteToLiveFast)
                {
                    target.Add(CMSController.CMSPublishingTarget.CDRLiveFast);
                }
                foreach (CMSController.CMSPublishingTarget targetItem in target)
                {
                    BatchManager.AddBatchHistoryEntry(batchInfo.BatchID, batchInfo.UserName,
                        string.Format("Begin CMS publishing for stage {0}: {1}", action, targetItem));
                    controller.StartPublishing(targetItem);
                }
            }
        }

        /// <summary>
        /// Encapsulates the logic for checking whether the appropriate versions of a document
        /// are in the source and destination locations for a given promotion step.
        /// </summary>
        /// <param name="docData">The document being checked</param>
        /// <param name="promotionAction">promote to staging, promote to preview, promote to live</param>
        /// <param name="locationMap">the last location this document version was in.</param>
        /// <returns></returns>
        private static bool IsOkToPromote(RequestDataInfo docData, int batchID,
                                            ProcessActionType promotionAction, 
                                            DocumentVersionMap locationMap)
        {
            bool isOK = true;

            VersionComparer compareSource;          // Delegate for comparing what's in the source location.
            VersionComparer compareDestination;     // Delegate for comparing what's in the destination location.
            ExistenceCheck previousVersionExists;
            GroupLocation groupIsPresent;       // Delegate for checking groups.

            // For reporting errors.
            string format = "", message;

            /// In order to be promoted to a level, the document's current location must be
            /// no more than one level below the target.  
            switch (promotionAction)
            {
                case ProcessActionType.PromoteToStaging:
                    // It's always OK to promote to staging as long as version that's 
                    // already there is older.
                    compareSource = null;   // There is no source prior to staging.
                    compareDestination = locationMap.MatchStagingVersion;
                    previousVersionExists = null;   // Always present in GateKeeper
                    groupIsPresent = locationMap.GroupIsPresentInGateKeeper;
                    if (docData.Location < RequestDataLocationType.GateKeeper)
                    {
                        DocMgrLogBuilder.Instance.CreateError(typeof(DocumentManager),
                            "IsOkToPromote", "Document Location cannot be prior to GateKeeper.");
                        format = "{0} failed. The document was not found in Gatekeeper!";
                        isOK = false;
                    }
                    break;

                case ProcessActionType.PromoteToPreview:
                    compareSource = locationMap.MatchStagingVersion;
                    compareDestination = locationMap.MatchPreviewVersion;
                    previousVersionExists = locationMap.StagingVersionExists;
                    groupIsPresent = locationMap.GroupIsPresentInStaging;
                    if (docData.Location < RequestDataLocationType.Staging)
                    {
                        format = "{0} failed. The document was not found in Staging.";
                        isOK = false;
                    }
                    break;

                case ProcessActionType.PromoteToLive:
                    compareSource = locationMap.MatchPreviewVersion;
                    compareDestination = locationMap.MatchLiveVersion;
                    previousVersionExists = locationMap.PreviewVersionExists;
                    groupIsPresent = locationMap.GroupIsPresentInPreview;
                    if (docData.Location < RequestDataLocationType.Preview)
                    {
                        format = "{0} failed. The document was not found in Preview.";
                        isOK = false;
                    }
                    break;

                case ProcessActionType.PromoteToLiveFast:
                    compareSource = locationMap.MatchStagingVersion;
                    compareDestination = locationMap.MatchLiveVersion;
                    previousVersionExists = locationMap.StagingVersionExists;
                    groupIsPresent = locationMap.GroupIsPresentInStaging;
                    if (docData.Location < RequestDataLocationType.Staging)
                    {
                        format = "{0} failed. The document was not found in Staging.";
                        isOK = false;
                    }
                    break;

                default:
                    {
                        format = "Unknown promotion action: {0}.";
                        message = string.Format(format, promotionAction.ToString());
                        DocMgrLogBuilder.Instance.CreateError(typeof(DocumentManager),
                            "IsOkToPromote", message);
                        throw new Exception( message);
                    }
            }

            if (!isOK)
            {
                message = string.Format(format, promotionAction.ToString());
                RequestManager.AddRequestDataHistoryEntry(docData.RequestID, docData.RequestDataID,
                    batchID, message, RequestDataHistoryType.Error);
                RequestManager.MarkDocumentWithErrors(docData.RequestDataID);
            }

            bool matchesSource = true;
            bool matchesDest = true;
            switch (docData.ActionType)
            {
                case RequestDataActionType.Export:
                    // This document's version must be the same as what's in the source location.
                    if (compareSource != null && // Always available in Gatekeeper DB
                        compareSource(docData.CdrID, docData.RequestID) != 0)
                        matchesSource = false;

                    // This document's version must be the same or newer than what's currently
                    // in the target location.
                    if (compareDestination(docData.CdrID, docData.RequestID) < 0)
                        matchesDest = false;
                    break;

                case RequestDataActionType.Remove:
                    {
                        // The document version must be the same as what's in the source,
                        // or else the document must not exist in the source.
                        bool differentVersion = (compareSource != null) && // always in Gatekeeper DB.
                            (compareSource(docData.CdrID, docData.RequestID) != 0);
                        bool doesExist = (previousVersionExists == null) || // always in Gatekeeper DB.
                            previousVersionExists(docData.CdrID);

                        if (doesExist && differentVersion)
                            matchesSource = false;

                        // This document's version must be the same or newer than what's currently
                        // in the target location.
                        if (compareDestination(docData.CdrID, docData.RequestID) < 0)
                            matchesDest = false;
                    }
                    break;

                default:
                    throw new Exception("Unknown document action: " + docData.ActionType.ToString());
            }

            if (isOK) // Avoid multiple diagnostics.
            {
                if (!matchesSource)
                {
                    format = "{0} failed. The document is not available in the source location.";
                    message = string.Format(format, promotionAction.ToString());
                    RequestManager.AddRequestDataHistoryEntry(docData.RequestID, docData.RequestDataID,
                        batchID, message, RequestDataHistoryType.Error);
                    RequestManager.MarkDocumentWithErrors(docData.RequestDataID);
                    isOK = false;
                }

                if (!matchesDest)
                {
                    format = "{0} failed. A newer version exists in the target location.";
                    message = string.Format(format, promotionAction.ToString());
                    RequestManager.AddRequestDataHistoryEntry(docData.RequestID, docData.RequestDataID,
                        batchID, message, RequestDataHistoryType.Error);
                    RequestManager.MarkDocumentWithErrors(docData.RequestDataID);
                    isOK = false;
                }
            }

            if (isOK) // Avoid multiple diagnostics.
            {
                if (!groupIsPresent(docData.GroupID))
                {
                    // Write an error message to the history, but don't mark the document
                    // with an error as the problem may actually be with another member of the group.
                    // The failing document will have been marked separately.
                    format = "{0} failed. Some members of the dependency group do not appear in the source location.";
                    message = string.Format(format, promotionAction.ToString());
                    RequestManager.AddRequestDataHistoryEntry(docData.RequestID, docData.RequestDataID,
                        batchID, message, RequestDataHistoryType.Error);
                    isOK = false;
                }
            }

            return (isOK && matchesSource && matchesDest);
        }

        #region Private Methods

        /// <summary>
        /// Encapsulates the logic for checking whether it's OK to promote a document,
        /// based on previous promotion status. (Both the individual document and its
        /// dependency group.)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="docData"></param>
        /// <param name="locationMap"></param>
        /// <param name="statusMap"></param>
        /// <returns></returns>
        private static bool IsDocumentOKToPromote(ProcessActionType action, int batchID,
            RequestDataInfo docData, DocumentVersionMap locationMap, DocumentStatusMap statusMap)
        {
            bool documentIsOKToPromote = true;

            // Check document status from previous iterations.
            if (!DocumentStatusIsOK(docData.RequestDataID, statusMap))
            {
                // Record why we're not promoting, but no need to mark it a second time.
                documentIsOKToPromote = false;

                string format = "{0} failed for Cdrid {1} because of errors in a previous promotion.";
                string message = string.Format(format, action.ToString(), docData.CdrID);
                RequestManager.AddRequestDataHistoryEntry(docData.RequestID, docData.RequestDataID, batchID,
                    message, RequestDataHistoryType.Error);
            }

            // Check the document's dependency status
            else if (!DocumentDependencyStatusIsOK(docData.RequestDataID, statusMap))
            {
                // Record why we're not promoting, but this document has no error of its own.
                documentIsOKToPromote = false;

                string format = "{0} failed for Cdrid {1} because of errors in a document in the same dependency group.";
                string message = string.Format(format, action.ToString(), docData.CdrID);
                RequestManager.AddRequestDataHistoryEntry(docData.RequestID, docData.RequestDataID, batchID,
                    message, RequestDataHistoryType.Error);
            }

            // Check that promotion is OK it terms of the versions being in the right place.
            // History entry is recorded in IsOkToPromote().
            if (documentIsOKToPromote &&
                !IsOkToPromote(docData, batchID, action, locationMap))
            {
                documentIsOKToPromote = false;
            }

            return documentIsOKToPromote;
        }

        /// <summary>
        /// For convenience, encapsulate the check for document status.
        /// </summary>
        /// <param name="docData"></param>
        /// <returns></returns>
        private static bool DocumentStatusIsOK(int requestDataID, DocumentStatusMap statusMap)
        {
            RequestDataStatusType status = statusMap.CheckDocumentStatus(requestDataID);

            return (status == RequestDataStatusType.OK ||
                    status == RequestDataStatusType.Warning);
        }

        /// <summary>
        /// For convenience, encapsulate the check for document dependency status.
        /// </summary>
        /// <param name="docData"></param>
        /// <returns></returns>
        private static bool DocumentDependencyStatusIsOK(int requestDataID, DocumentStatusMap statusMap)
        {
            RequestDataDependentStatusType status = statusMap.CheckDependencyStatus(requestDataID);

            return status == RequestDataDependentStatusType.OK;
        }

        private static void SetThreadPoolSize()
        {
            // Maximum number of thread pool threads, not the maximum number of entries that
            // can be queued.
            string maxWorkerValue;
            string maxIOValue;
            int maxWorkerThreads = -1;
            int maxIOThreads = -1;

            try
            {
                maxWorkerValue = ConfigurationManager.AppSettings["MaxWorkerThreads"];
                maxIOValue = ConfigurationManager.AppSettings["MaxIOCompletionThreads"];

                maxWorkerThreads = int.Parse(maxWorkerValue);
                maxIOThreads = int.Parse(maxIOValue);
            }
            catch (Exception ex)
            {
                DocMgrLogBuilder.Instance.CreateWarning(typeof(DocumentManager), "SetThreadPoolSize",
                    "Error reading thread pool size.  Using maximum.", ex);
                ThreadPool.GetAvailableThreads(out maxWorkerThreads, out maxIOThreads);
            }

            ThreadPool.SetMaxThreads(maxWorkerThreads, maxIOThreads);
        }

        /// <summary>
        /// The decision of whether promotion requires documents to be validated is based on
        /// the current DTD version.  If the current version matches the one used to create
        /// the request, then there's no need to validate again.
        /// </summary>
        /// <param name="requestDtdVersion"></param>
        /// <returns></returns>
        private static bool MustValidate(string requestDtdVersion)
        {
            bool validationNeeded = true;
            string currentDtdVersion = ConfigurationManager.AppSettings["DTDVersion"];

            if (currentDtdVersion != null &&
                currentDtdVersion == requestDtdVersion)
                validationNeeded = false;

            return validationNeeded;
        }


        private static string ActionToLocation(ProcessActionType action)
        {
            string location;

            switch (action)
            {
                case ProcessActionType.PromoteToLive:
                case ProcessActionType.PromoteToLiveFast:
                    location = "Live";
                    break;

                case ProcessActionType.PromoteToPreview:
                    location = "Preview";
                    break;

                case ProcessActionType.PromoteToStaging:
                    location = "Staging";
                    break;

                case ProcessActionType.Invalid:
                    DocMgrLogBuilder.Instance.CreateError(typeof(DocumentManager), "ActionToLocation",
                        @"""Invalid"" is not a valid promotion action.");
                    location = "Invalid Location(!!)";
                    break;

                default:
                    location = string.Format("Unknown promotion action: {0}.", action);
                    DocMgrLogBuilder.Instance.CreateError(typeof(DocumentManager), "ActionToLocation",
                        string.Format(@"""{0}"" is not a valid promotion action.", action));
                    break;
            }

            return location;
        }

        #endregion
    }
}
