using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GKManagers.CMSManager.CMS;

namespace GKManagers.CMSManager
{
    /// <summary>
    /// Abstract base class to implement the functionality shared between
    /// all DocumentProcessors.
    /// </summary>
    public abstract class DocumentProcessorCommon : IDisposable
    {
        #region Properties

        // Percussion control object, shared among all Document Processor types
        // derived from DocumentProcessorCommon
        protected CMSController CMSController{get;private set;}

        /// <summary>
        /// Delegate for writing warnings about potential problems encountered during document processing.
        /// Generally used for non-fatal errors.  Errors which cannot handles are reported by throwing
        /// an exception from the CMSException family. (See Exceptions.cs)
        /// </summary>
        protected HistoryEntryWriter WarningWriter { get; private set; }

        /// <summary>
        /// Delegate for writing informational messages about document processing.
        /// Generally used for reporting progress in the processings steps.
        /// (e.g. "Creating Content Item", "Saving Content Item", "Promoting Content Item to Preview.", etc.)
        /// </summary>
        protected HistoryEntryWriter InformationWriter { get; private set; }

        #endregion

        #region Disposable Pattern Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Free managed resources only.
            if (disposing)
            {
                CMSController.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="percLoader">The instance of PercussionLoader which will be used by
        /// the concrete DocumentProcessor to manipulate the Percussion CMS.</param>
        public DocumentProcessorCommon(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
        {
            this.CMSController = new CMSController();
            WarningWriter = warningWriter;
            InformationWriter = informationWriter;
        }

        /// <summary>
        /// Verifies that a document object contains an expected document type.
        /// </summary>
        /// <param name="pdqDocument">A document object to be tested for its concrete document type</param>
        /// <param name="expectedDocType">Enumerated value representing the expected document type.</param>
        protected void VerifyRequiredDocumentType(Document pdqDocument, DocumentType expectedDocType)
        {
            if (pdqDocument.DocumentType!= expectedDocType)
            {
                throw new CMSManagerIncorrectDocumentTypeException(string.Format("Incorrect DocumentType encountered. Expected {0}, found {1}.",
                    expectedDocType, pdqDocument.DocumentType));
            }
        }

        protected void TransitionItemsToStaging(long[] idList)
        {
            // GetWorkflowState() is guaranteed to return a valid state.
            WorkflowState oldState = GetWorkflowState(idList);

            // If we're already in Staging, there's nothing to do.
            if (oldState != WorkflowState.Staging && oldState != WorkflowState.UpdateStaging)
            {
                WorkflowTransition transition = WorkflowTransition.Invalid;

                // Perform the necessary transition to move the items to the appropriate
                // editable state (Staging or UpdateStaging).
                switch (oldState)
                {
                    case WorkflowState.Preview:
                        transition = WorkflowTransition.RevertToStagingNew;
                        break;
                    case WorkflowState.Live:
                        transition = WorkflowTransition.Update;
                        break;
                    case WorkflowState.UpdatePreview:
                        transition = WorkflowTransition.RevertToStagingUpdate;
                        break;
                }

                CMSController.PerformWorkflowTransition(idList, transition.ToString());
            }
        }

        protected WorkflowState GetWorkflowState(long[] idList)
        {
            object state = CMSController.GetWorkflowState(idList, InferWorkflowState);

            if (state == null ||
                !state.GetType().Equals(typeof(WorkflowState)))
                throw new CMSWorkflowStateInferenceException("Unable to determine workflow state.");

            return (WorkflowState)state;
        }

        /// <summary>
        /// Infers a workflow state from the list of transitions which can be made from it.
        /// </summary>
        /// <param name="transitionNameList">A list of transition trigger names.</param>
        /// <returns>A boxed value containing a workflow state.</returns>
        protected object InferWorkflowState(IEnumerable<string> transitionNameList)
        {
            WorkflowState state = WorkflowState.Invalid;
            WorkflowTransition transition = WorkflowTransition.Invalid;
            bool found = false;
            string lastTransitionName = string.Empty;

            if (transitionNameList == null || transitionNameList.Count() == 0)
                throw new CMSWorkflowStateInferenceException("Transition List is null or empty.");

            foreach (string name in transitionNameList)
            {
                if (found)
                    break;

                lastTransitionName = name;

                // Convert the name to an enum for easy testing (avoids problems with
                // spelling and casing).
                transition = ConvertEnum<WorkflowTransition>.Convert(name);
                switch (transition)
                {
                    // Staging
                    case WorkflowTransition.PromoteToPreviewNew:
                        state = WorkflowState.Staging;
                        found = true;
                        break;
                    // Preview
                    case WorkflowTransition.PromoteToLiveNew:
                    case WorkflowTransition.RevertToStagingNew:
                        state = WorkflowState.Preview;
                        found = true;
                        break;
                    // Live
                    case WorkflowTransition.Update:
                        state = WorkflowState.Live;
                        found = true;
                        break;
                    // Update Staging
                    case WorkflowTransition.PromoteToPreviewUpdate:
                        state = WorkflowState.UpdateStaging;
                        found = true;
                        break;
                    // Update Preview
                    case WorkflowTransition.PromoteToLiveUpdate:
                    case WorkflowTransition.RevertToStagingUpdate:
                        state = WorkflowState.UpdatePreview;
                        found = true;
                        break;
                    // Unknown.
                    case WorkflowTransition.Invalid:
                    default:
                        break;
                }
            }

            if (!found || state==WorkflowState.Invalid)
            {
                throw new
                    CMSWorkflowStateInferenceException(
                        string.Format("Unable to infer workflow state from transition name {0}", lastTransitionName));
            }

            return (object)state;
        }
    }
}
