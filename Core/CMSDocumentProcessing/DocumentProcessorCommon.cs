using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using NCI.WCM.CMSManager;
using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSDocumentProcessing
{
    /// <summary>
    /// Abstract base class to implement the functionality shared between
    /// all DocumentProcessors.
    /// </summary>
    public abstract class DocumentProcessorCommon : IDocumentProcessor, IDisposable
    {
        #region Constants

        const string NavonType = "rffNavon";
        const string WorkflowTransitionForNavonRevise = "Quick Edit";//8, //TriggerName = Quick Edit = Move to state 'Editing'
        const string WorkflowTransitionForNavonPreview = "Preview";//4, //TriggerName = Preview = Move to state 'Staging (P)'
        const string WorkflowTransitionForNavonReview = "Resubmit for Review";// 17, //TriggerName = Resubmit for Review = Move to state 'Review (P)'
        const string WorkflowTransitionForNavonApprove = "Reapprove";//12 //TriggerName = Reapprove = Move to state 'Public'

        protected string LanguageEnglish { get { return "en-us"; } }
        protected string LanguageSpanish { get { return "es-us"; } }

        protected const string InlineSlot = "sys_inline_variant";
        protected const string InlineLinkSlotID = "103";
        protected const string InlinImageSlotID = "104";
        protected const string InlineTemplateSlotID = "105";
        #endregion

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

        ~DocumentProcessorCommon()
        {
            Dispose(false);
        }

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
                GC.SuppressFinalize(this);
            }
        }

        #endregion

        #region IDocumentProcessor methods

        /// These methods are not intended to be called from derived classes.  They are
        /// implemented in the DocumentProcessorCommon class to allow subclasses to
        /// fullfil the IDocumentProcessor contract without requiring the sitePath
        /// logic to be implemented in all classes.
        /// 
        /// The intent is that as more document types begin to be supported on the mobile
        /// platform, these default implementations can be removed.

        public virtual void ProcessDocument(Document documentObject)
        {
            throw new NotImplementedException();
        }

        public virtual void ProcessDocument(Document documentObject, string sitePath)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteContentItem(int documentCdrID, string sitePath)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteContentItem(int documentCdrID)
        {
            throw new NotImplementedException();
        }

        public virtual void PromoteToPreview(int documentCdrID, string sitePath)
        {
            throw new NotImplementedException();
        }

        public virtual void PromoteToPreview(int documentCdrID)
        {
            throw new NotImplementedException();
        }

        public virtual void PromoteToLive(int documentCdrID, string sitePath)
        {
            throw new NotImplementedException();
        }

        public virtual void PromoteToLive(int documentCdrID)
        {
            throw new NotImplementedException();
        }

        public virtual void PromoteToLiveFast(int documentCdrID, string sitePath)
        {
            throw new NotImplementedException();
        }

        public virtual void PromoteToLiveFast(int documentCdrID)
        {
            throw new NotImplementedException();
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
        /// Verifies that a document object contains an expected document type.  Throws 
        /// CMSManagerIncorrectDocumentTypeException if the value of the object's DocumentType
        /// property does not match expectedDocType.
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


        /// <summary>
        /// Searches the CMS repository for the specified CDR Document.
        /// </summary>
        /// <param name="contentType">The CMS content type of the document being searched for.</param>
        /// <param name="path">The path to search.</param>
        /// <param name="cdrID">The document's CDR ID</param>
        /// <returns>If found, a Percussion GUID value is returned which identifies the document.
        /// A null return means no matching document was located.</returns>
        public virtual PercussionGuid GetCdrDocumentID(string contentType, string path, int cdrID)
        {
            return GetCdrDocumentID(contentType, null, path, cdrID);
        }

        /// <summary>
        /// Searches the CMS repository for the specified CDR Document.
        /// </summary>
        /// <param name="contentType">The CMS content type of the document being searched for.</param>
        /// <param name="siteBase">Path to the site's base folder.  Specify null for current/default site.</param>
        /// <param name="path">The path to search.</param>
        /// <param name="cdrID">The document's CDR ID</param>
        /// <returns>
        /// If found, a Percussion GUID value is returned which identifies the document.
        /// A null return means no matching document was located.
        /// </returns>
        public virtual PercussionGuid GetCdrDocumentID(string contentType, string siteBase, string path, int cdrID)
        {
            PercussionGuid[] searchResults;
            PercussionGuid foundItem;

            Dictionary<string, string> fieldCriteria = new Dictionary<string, string>();
            fieldCriteria.Add("cdrid", cdrID.ToString("d"));
            searchResults = CMSController.SearchForContentItems(contentType, siteBase, path, fieldCriteria);

            if (searchResults.Count() > 1)
            {
                // Something is wrong; the CDRID is supposed to be unique!
                throw new CMSOperationalException(string.Format("Found multiple content items with CDRID {0}.", cdrID));
            }
            else if (searchResults.Count() < 1)
            {
                // No results found.
                foundItem = null;
            }
            else
            {
                // Found exactly 1 item.
                foundItem = searchResults[0];
            }

            return foundItem;

        }

        public PSItem GetContentItem(string contentType, string path, int cdrID)
        {
            PercussionGuid identifier = GetCdrDocumentID( contentType, path, cdrID);
            if (identifier != null)
            {
               PSItem[] item = null;
               item = CMSController.LoadContentItems(new PercussionGuid[] { identifier });
               return item[0];
            }

            return null;
        }


        /// <summary>
        /// Searches the CMS repository for the specified CDR Document.
        /// </summary>
        /// <param name="contentType">The CMS content type of the document being searched for.</param>
        /// <param name="cdrID">The document's CDR ID</param>
        /// <returns>If found, a Percussion GUID value is returned which identifies the document.
        /// A null return means no matching document was located.</returns>
        public virtual PercussionGuid GetCdrDocumentID(string contentType, int cdrID)
        {
            return GetCdrDocumentID(contentType, null, cdrID);
        }


        /// <summary>
        /// Moves the content items identified by idList to the Staging state.
        /// </summary>
        /// <param name="idList">An array of ID values identifying content items to
        /// be moved</param>
        protected void TransitionItemsToStaging(PercussionGuid[] idList)
        {
            // GetWorkflowState() is guaranteed to return a valid state.
            WorkflowState oldState = GetWorkflowState(idList);

            // If we're already in Staging, there's nothing to do.
            if (oldState != WorkflowState.CDRStaging && oldState != WorkflowState.CDRStagingUpdate)
            {
                WorkflowTransition transition = WorkflowTransition.Invalid;

                // Perform the necessary transition to move the items to the appropriate
                // editable state (CDRStaging or CDRStagingUpdate).
                switch (oldState)
                {
                    case WorkflowState.CDRPreview:
                        transition = WorkflowTransition.RevertToCDRStagingNew;
                        break;
                    case WorkflowState.CDRLive:
                        transition = WorkflowTransition.Update;
                        break;
                    case WorkflowState.CDRPreviewUpdate:
                        transition = WorkflowTransition.RevertToCDRStagingUpdate;
                        break;
                }

                CMSController.PerformWorkflowTransition(idList, transition.ToString());
            }
        }

        /// <summary>
        /// Moves the content items identified by idList to the Preview state.
        /// </summary>
        /// <param name="idList">An array of ID values identifying content items to
        /// be moved</param>
        protected void TransitionItemsToPreview(PercussionGuid[] idList)
        {
            // GetWorkflowState() is guaranteed to return a valid state.
            WorkflowState oldState = GetWorkflowState(idList);

            // If we're already in Preview, there's nothing to do.
            // Likewise, if we're already in Live, then we've already been through preview.
            if (oldState != WorkflowState.CDRPreview && oldState != WorkflowState.CDRPreviewUpdate
                && oldState!= WorkflowState.CDRLive)
            {
                WorkflowTransition transition = WorkflowTransition.Invalid;

                // Perform the necessary transition to move the items to the appropriate
                // editable state (CDRStaging or CDRStagingUpdate).
                switch (oldState)
                {
                    case WorkflowState.CDRStaging:
                        transition = WorkflowTransition.PromoteToCDRPreviewNew;
                        break;
                    case WorkflowState.CDRStagingUpdate:
                        transition = WorkflowTransition.PromoteToCDRPreviewUpdate;
                        break;
                }

                CMSController.PerformWorkflowTransition(idList, transition.ToString());
            }
        }

        /// <summary>
        /// Moves the content items identified by idList to the Live state.
        /// </summary>
        /// <param name="idList">An array of ID values identifying content items to
        /// be moved</param>
        /// <remarks>Throws CMSWorkflowException if the targeted items are in the
        /// CDRStaging or CDRStagingUpdate states as these are not valid states from
        /// which to move to CDRLive.</remarks>
        protected void TransitionItemsToLive(PercussionGuid[] idList)
        {
            // GetWorkflowState() is guaranteed to return a valid state.
            WorkflowState oldState = GetWorkflowState(idList);

            // If we're already in Live, there's nothing to do.
            if (oldState != WorkflowState.CDRLive)
            {
                WorkflowTransition transition = WorkflowTransition.Invalid;

                switch (oldState)
                {
                    case WorkflowState.CDRStaging:
                    case WorkflowState.CDRStagingUpdate:
                        throw new CMSWorkflowException("Illegal attempt to move directly from CDRStaging to CDRLive.");
                    case WorkflowState.CDRPreview:
                        transition = WorkflowTransition.PromoteToCDRLiveNew;
                        break;
                    case WorkflowState.CDRPreviewUpdate:
                        transition = WorkflowTransition.PromoteToCDRLiveUpdate;
                        break;
                }

                CMSController.PerformWorkflowTransition(idList, transition.ToString());
            }
        }

        /// <summary>
        /// Moves the content items identified by idList to the Live state.
        /// </summary>
        /// <param name="idList">An array of ID values identifying content items to
        /// be moved</param>
        /// <remarks>If the targeted items are in the
        /// CDRStaging or CDRStagingUpdate are valid workflow states.</remarks>
        protected void TransitionItemsToLiveFast(PercussionGuid[] idList)
        {
            // GetWorkflowState() is guaranteed to return a valid state.
            WorkflowState oldState = GetWorkflowState(idList);

            // If we're already in Live, there's nothing to do.
            if (oldState != WorkflowState.CDRLive)
            {
                WorkflowTransition transition = WorkflowTransition.Invalid;

                switch (oldState)
                {
                    case WorkflowState.CDRStaging:
                        transition = WorkflowTransition.PromoteToCDRLiveFastNew;//valid workflow transition when moving documents directly from staging to live
                        break;
                    case WorkflowState.CDRStagingUpdate:
                        transition = WorkflowTransition.PromoteToCDRLiveFastUpdate;//valid workflow transition when moving documents directly from staging to live
                        break;
                    case WorkflowState.CDRPreview:
                        transition = WorkflowTransition.PromoteToCDRLiveNew;
                        break;
                    case WorkflowState.CDRPreviewUpdate:
                        transition = WorkflowTransition.PromoteToCDRLiveUpdate;
                        break;
                }
                CMSController.PerformWorkflowTransition(idList, transition.ToString());
            }
        }

        /// <summary>
        /// Retrieves the workflow state of the content items identifed by idList.
        /// </summary>
        /// <param name="idList">An array of content items for which the workflow state
        /// is to be determined.</param>
        /// <returns>A WorkflowState value representing the items' current workflow state.</returns>
        /// <remarks>Throws CMSWorkflowStateInferenceException if a workflow state cannot be
        /// determined. (e.g. If the designated items do not have a shared workflow state,
        /// or if the items belong to an unexpected workflow.  CancerGov_PDQ_Workflow is assumed.</remarks>
        protected WorkflowState GetWorkflowState(PercussionGuid[] idGuids)
        {
            long[] idList = Array.ConvertAll(idGuids, id => (long)id.ID);

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
        /// <remarks>The workflow states and transitions searched for are assumed to
        /// be from CancerGov_PDQ_Workflow which includes the states: CDRStaging, 
        /// CDRPreview, CDRLive, CDRStagingUpdate and CDRPreviewUpdate.</remarks>
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
                    // CDRStaging
                    case WorkflowTransition.PromoteToCDRPreviewNew:
                    case WorkflowTransition.PromoteToCDRLiveFastNew://new workflow transition to promote from staging to live & skip the preview step
                        state = WorkflowState.CDRStaging;
                        found = true;
                        break;
                    // CDRPreview
                    case WorkflowTransition.PromoteToCDRLiveNew:
                    case WorkflowTransition.RevertToCDRStagingNew:
                        state = WorkflowState.CDRPreview;
                        found = true;
                        break;
                    // CDRLive
                    case WorkflowTransition.Update:
                        state = WorkflowState.CDRLive;
                        found = true;
                        break;
                    // CDRStaging Update
                    case WorkflowTransition.PromoteToCDRPreviewUpdate:
                    case WorkflowTransition.PromoteToCDRLiveFastUpdate://new workflow transition to promote from staging to live & skip the preview step
                        state = WorkflowState.CDRStagingUpdate;
                        found = true;
                        break;
                    // CDRPreview Update
                    case WorkflowTransition.PromoteToCDRLiveUpdate:
                    case WorkflowTransition.RevertToCDRStagingUpdate:
                        state = WorkflowState.CDRPreviewUpdate;
                        found = true;
                        break;
                    // Unknown.
                    case WorkflowTransition.Invalid:
                    default:
                        throw new CMSWorkflowStateInferenceException(
                            string.Format("Unknown workflow transition name {0}.", name));
                }
            }

            if (!found || state==WorkflowState.Invalid)
            {
                throw new
                    CMSWorkflowStateInferenceException(
                        string.Format("Unable to infer workflow state from transition name {0}.", lastTransitionName));
            }

            return (object)state;
        }

        protected void TransitionNavonToEditing(CMSController CMSControllerForNavOn, PercussionGuid[] idList)
        {
            //Move to Editing
            CMSControllerForNavOn.PerformWorkflowTransition(idList, WorkflowTransitionForNavonRevise);
        }

        protected void TransitionNavonToPublic(CMSController CMSControllerForNavOn, PercussionGuid[] idList)
        {
            //Move to Staging(P)
            CMSControllerForNavOn.PerformWorkflowTransition(idList, WorkflowTransitionForNavonPreview);

            //Move to Review(P)
            CMSControllerForNavOn.PerformWorkflowTransition(idList, WorkflowTransitionForNavonReview);

            //Move to Public
            CMSControllerForNavOn.PerformWorkflowTransition(idList, WorkflowTransitionForNavonApprove);

        }

        /// <summary>
        /// Checks whether any content item exists at location path with its
        /// pretty_url_name field set to prettyUrlName.
        /// </summary>
        /// <param name="path">Path to check for content items</param>
        /// <param name="prettyUrlName">pretty_url_name field value to check.</param>
        /// <returns>true if no item matches the parameters.</returns>
        protected bool PrettyUrlIsAvailable(string path, string prettyUrlName)
        {
            return PrettyUrlIsAvailable(null, path, prettyUrlName);
        }

        /// <summary>
        /// Checks whether any content item exists at location path with its
        /// pretty_url_name field set to prettyUrlName.
        /// </summary>
        /// <param name="path">Path to check for content items</param>
        /// <param name="prettyUrlName">pretty_url_name field value to check.</param>
        /// <param name="sitePath">Overrides the default site folder.</param>
        /// <returns>true if no item matches the parameters.</returns>
        protected bool PrettyUrlIsAvailable(string sitePath, string path, string prettyUrlName)
        {
            bool urlAvailable;

            PercussionGuid[] searchResults;

            Dictionary<string, string> fieldCriteria = new Dictionary<string, string>();
            fieldCriteria.Add("pretty_url_name", prettyUrlName);

            // Allow the configured site path to be overriden.
            string defaultPath = CMSController.SiteRootPath;
            if (!string.IsNullOrEmpty(sitePath))
                CMSController.SiteRootPath = sitePath;

            searchResults = CMSController.SearchForContentItems(null, path, fieldCriteria);

            if (searchResults.Length > 0)
            {
                // Results were found, but because Percussion matches tokens within the name,
                // we need to check to see whether there are any *exact* matches.
                urlAvailable = true; // Assume no exact matches.
                PSItem[] conflictingItems = CMSController.LoadContentItems(searchResults);
                foreach (PSItem item in conflictingItems)
                {
                    // Compare values
                    string fieldValue = PSItemUtils.GetFieldValue(item.Fields, "pretty_url_name");
                    if (fieldValue.Equals(prettyUrlName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // An exact match was found.
                        urlAvailable = false;
                        break;
                    }
                }
            }
            else
            {
                // No matching results found
                urlAvailable = true;
            }


            // Restore default site path.
            CMSController.SiteRootPath = defaultPath;

            return urlAvailable;
        }

        /// <summary>
        /// Verifies that the CDR document at the location path is the one described
        /// by the given criteria.
        /// </summary>
        /// <param name="cdrID">Expected document CDRID.</param>
        /// <param name="path">Path to check for content items.</param>
        /// <param name="prettyUrlName">pretty_url_name field value to check.</param>
        /// <returns>true if the item is a match</returns>
        protected bool PrettyUrlIsSameDocument(int cdrID, string path, string prettyUrlName)
        {
            PercussionGuid[] searchResults;

            Dictionary<string, string> fieldCriteria = new Dictionary<string, string>();
            fieldCriteria.Add("cdrid", cdrID.ToString());
            searchResults = CMSController.SearchForContentItems(null, path, fieldCriteria);

            return searchResults.Length == 1;
        }

        /// <summary>
        /// Determines whether the named path contains anything other than a Navon.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>False if the folder is empty. (Navons are ignored.)</returns>
        protected bool PrettyUrlPathIsOccupied(string path)
        {
            bool isPathOccupied = false;

            bool folderExists = CMSController.FolderExists(path);
            if (folderExists)
            {
                PSItemSummary[] items = CMSController.FindFolderChildren(path);
                if (items.Length == 1) 
                {
                    PSItem[] contentItem = CMSController.LoadContentItems(new PercussionGuid[] { new PercussionGuid(items[0].id) });
                    // Check whether it's a Navon.
                    if (contentItem[0].contentType.Equals(NavonType, StringComparison.InvariantCultureIgnoreCase))
                        isPathOccupied = false;
                    else
                        //if there is only one item and it is anything other than a Navon the path is occupied
                        isPathOccupied = true;
                }
                else if (items.Length > 1)
                {
                    isPathOccupied = true;
                }
            }

            return isPathOccupied;
        }

        protected String EscapeSystemTitle(String title)
        {
            while (title.IndexOf('/') > -1)
            {
                int offset= title.IndexOf('/');
                title = title.Substring(0, offset) + "," + title.Substring(offset + 1);
            }

            return title;
        }
    }
}
