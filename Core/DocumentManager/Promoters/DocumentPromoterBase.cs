using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using GateKeeper.Common;
using GateKeeper.DataAccess.GateKeeper;

using GKManagers.BusinessObjects;

namespace GKManagers
{
    abstract class DocumentPromoterBase
    {
        private RequestData _dataBlock;     // Document to promote.
        private int _batchID;               // What batch is this promotion for.
        private ProcessActionType _promotionAction; // What promotion to perform. (Export/Remove)
        private string _userName;    // Who is performing the promotion.

        private bool _promotionWasSuccessful = false;

        public DocumentPromoterBase(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
        {
            _promotionAction = action;
            _dataBlock = dataBlock;
            _batchID = batchID;
            _userName = userName;
        }

        #region Properties

        public ProcessActionType PromotionAction
        {
            get { return _promotionAction; }
        }

        public bool PromotionWasSuccessful
        {
            get { return _promotionWasSuccessful; }
        }

        protected RequestData DataBlock
        {
            get { return _dataBlock; }
            set { _dataBlock = value; }
        }

        protected string UserName
        {
            get { return _userName; }
        }

        #endregion

        /* The PromoteToxxxxxxxx methods must be implemented in the individual document
         * type promoter classes which are derived from DocumentPromoterBase. */
        protected abstract void PromoteToStaging(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter);

        protected abstract void PromoteToPreview(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter);

        protected abstract void PromoteToLive(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter);

        public void Promote(DocumentXPathManager xPathManager)
        {
            try
            {
                VerifyDocumentType(_dataBlock);
                DispatchAndPromote(xPathManager);
                /// Was promotion successful?
                ///     Yes: Continue
                ///     No: An exception has already occured, go to the catch block.
                _promotionWasSuccessful = true;
                WriteHistoryInformationEntry(string.Format("{0} successful.", _promotionAction.ToString()));
            }
            catch (Exception ex)
            {
                // Mark Document as having errors.
                // Mark Group as having Dependency errors (same call)
                string failureMessage = string.Format("{0} failed with errors.\n{1}",
                    _promotionAction.ToString(),
                    ExceptionHelper.RetrieveMessage(ex));
                WriteHistoryErrorEntry(failureMessage);
                WriteHistoryDebugEntry(ex.ToString());
                RequestManager.MarkDocumentWithErrors(_dataBlock.RequestDataID);

                _promotionWasSuccessful = false;

                // Do not rethrow the error.  Document errors end here.
            }
        }

        #region Private Methods

        // Dispatch to Promotion based on desired action.
        private void DispatchAndPromote(DocumentXPathManager xPathManager)
        {
            switch (_promotionAction)
            {
                case ProcessActionType.PromoteToStaging:
                    WriteHistoryInformationEntry("Promoting to Staging.");
                    PromoteToStaging(xPathManager, WriteHistoryWarningEntry, WriteHistoryInformationEntry);
                    _dataBlock.Location = RequestDataLocationType.Staging;
                    RequestManager.SaveDocumentLocation(_dataBlock);
                    break;

                case ProcessActionType.PromoteToPreview:
                    WriteHistoryInformationEntry("Promoting to Preview.");
                    PromoteToPreview(xPathManager, WriteHistoryWarningEntry, WriteHistoryInformationEntry);
                    _dataBlock.Location = RequestDataLocationType.Preview;
                    RequestManager.SaveDocumentLocation(_dataBlock);
                    break;

                case ProcessActionType.PromoteToLive:
                    WriteHistoryInformationEntry("Promoting to Live.");
                    PromoteToLive(xPathManager, WriteHistoryWarningEntry, WriteHistoryInformationEntry);
                    _dataBlock.Location = RequestDataLocationType.Live;
                    RequestManager.SaveDocumentLocation(_dataBlock);
                    break;

                default:
                    DocMgrLogBuilder.Instance.CreateError(typeof(DocumentPromoterBase), "DispatchAndPromote",
                        string.Format("Unknown promotion action: {0}", _promotionAction));
                    break;
            }
        }

        private void VerifyDocumentType(RequestData docData)
        {
            // The document object is null for Remove requests.
            if (docData.ActionType != RequestDataActionType.Remove)
            {
                string format = "Document {0} is identified as CDR type {1} but is really type {2}.";
                string message;
                string markedDocumentType = docData.CDRDocType.ToString();
                string realDocumentType = docData.DocumentData.DocumentElement.Name;
                if (markedDocumentType != realDocumentType)
                {
                    message = string.Format(format, docData.CdrID, markedDocumentType, realDocumentType);
                    DocMgrLogBuilder.Instance.CreateError(typeof(DocumentPromoterBase), "VerifyDocumentType", message);
                    throw new Exception(message);
                }
            }
        }

        private void WriteHistoryInformationEntry(string description)
        {
            RequestManager.AddRequestDataHistoryEntry(_dataBlock.RequestID, _dataBlock.RequestDataID,
                _batchID, description, RequestDataHistoryType.Information);
        }

        private void WriteHistoryWarningEntry(string description)
        {
            RequestManager.MarkDocumentWithWarnings(_dataBlock.RequestDataID);
            RequestManager.AddRequestDataHistoryEntry(_dataBlock.RequestID, _dataBlock.RequestDataID,
                _batchID, description, RequestDataHistoryType.Warning);
        }

        private void WriteHistoryErrorEntry(string description)
        {
            RequestManager.AddRequestDataHistoryEntry(_dataBlock.RequestID, _dataBlock.RequestDataID,
                _batchID, description, RequestDataHistoryType.Error);
        }

        private void WriteHistoryDebugEntry(string description)
        {
            RequestManager.AddRequestDataHistoryEntry(_dataBlock.RequestID, _dataBlock.RequestDataID,
                _batchID, description, RequestDataHistoryType.Debug);
        }

        #endregion
    }
}
