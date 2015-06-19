using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using GateKeeper.Common;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.CMSDocumentProcessing.Configuration;

using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;


namespace GKManagers.CMSDocumentProcessing
{
    public class CancerInfoSummaryProcessorStandard
        : CancerInfoSummaryProcessorCommon
    {
        #region Protected Members

        protected override string SummaryPageSlot { get { return StandardPageSlotName; } }
        protected override string PageSnippetTemplateName { get { return SummarySectionSnippetTemplate; } }

        #endregion


        public CancerInfoSummaryProcessorStandard(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        {
        }

        /// <summary>
        /// Creates a new Summary document.
        /// </summary>
        /// <param name="document">SummaryDocument object containing the summary to be stored in the CMS.</param>
        /// <param name="sitePath">BasePath for the site.  In the Standard implementation, the sitePath specified
        /// in the PercussionConfig configuration section is used instead.</param>
        /// <returns>Percussion ID for the root content item.</returns>
        protected override PercussionGuid CreateNewCancerInformationSummary(SummaryDocument document, string sitePath)
        {
            /// The sitePath argument is not used in the Standard processor.  The argument exists to fulfill the
            /// contract of the signatu

            // List of items to delete if unable to create the entire document.
            List<PercussionGuid> rollbackList = new List<PercussionGuid>();

            try
            {
                PercussionGuid summaryRoot;

                string createPath = GetTargetFolder(document.BasePrettyURL);
                string rootItemPrettyUrlName = GetSummaryPrettyUrlName(document.BasePrettyURL);

                if (!PrettyUrlIsAvailable(createPath, rootItemPrettyUrlName))
                {
                    throw new DocumentExistsException(string.Format("Another document already exists at path {0}/{1}.",
                        createPath, rootItemPrettyUrlName));
                }

                VerifyEnglishLanguageVersion(document);

                // Create the folder where the content items are to be created and set the Navon to public.
                CMSController.GuaranteeFolder(createPath, FolderManager.NavonAction.MakePublic);

                // Create the Cancer Info Summary item.
                List<ContentItemForCreating> rootList = CreatePDQCancerInfoSummary(document, createPath);
                summaryRoot = new PercussionGuid(CMSController.CreateContentItemList(rootList)[0]);
                rollbackList.Add(summaryRoot);
                               
                // Create All New Permanent Links (for a New Summary)
                PermanentLinkHelper PermanentLinkData = new PermanentLinkHelper(CMSController, document.PermanentLinkList, createPath);
                PermanentLinkData.SetURLs(/*newPageIDs,*/ document.TopLevelSectionList);
                List<long> permanentLinkIDs = PermanentLinkData.CreatePermanentLinks(createPath);
                rollbackList.AddRange(CMSController.BuildGuidArray(permanentLinkIDs));
                                
                LinkToAlternateLanguageVersion(document, summaryRoot);

                //update the Nav Label field and add the summary to the landing page slot of the nav on
                UpdateNavOn(document, summaryRoot, GetTargetFolder(document.BasePrettyURL));

                return summaryRoot;
            }
            catch (Exception)
            {
                if (rollbackList.Count > 0)
                {
                    CMSController.DeleteItemList(rollbackList.ToArray());
                }
                throw;
            }
        }

        /// <summary>
        /// Updates an existing Summary document.
        /// </summary>
        /// <param name="summary">The summary to update.</param>
        /// <param name="summaryRootID">ID of the summary's root object.</param>
        /// <param name="sitePath">BasePath for the site where the content structure is to be stored.</param>
        protected override void PerformUpdate(SummaryDocument summary, PercussionGuid summaryRootID/*, PercussionGuid summaryLinkID*/, PermanentLinkHelper PermanentLinkData,
            PercussionGuid[] oldPageIDs, PSAaRelationship[] incomingPageRelationships,
            //PercussionGuid[] mobilePageIDs, PSAaRelationship[] incomingMobilePageRelationships,
            string sitePath)
        {
            // For undoing failed updates.
            List<PercussionGuid> rollbackList = new List<PercussionGuid>();

            // Does the item have a path?  Any path at all....
            PSItem[] summaryRootItem = CMSController.LoadContentItems(new PercussionGuid[] { summaryRootID });
            VerifyItemHasPath(summaryRootItem[0], summary.DocumentID);

            string existingItemPath = CMSController.GetPathInSite(summaryRootItem[0]);

            string newPath = GetTargetFolder(summary.BasePrettyURL);
            string prettyUrlName = GetSummaryPrettyUrlName(summary.BasePrettyURL);

            if (!PrettyUrlIsSameDocument(summary.DocumentID, existingItemPath, prettyUrlName))
            {
                throw new DocumentExistsException(string.Format("Another document already exists at path {0}/{1}.", existingItemPath, prettyUrlName));
            }

            if (!newPath.Equals(existingItemPath, StringComparison.InvariantCultureIgnoreCase)
                && PrettyUrlPathIsOccupied(newPath))
            {
                throw new DocumentExistsException(string.Format("Path {0} is already in use.", newPath));
            }

            // Temporary location for creating new content items without deleting the old.
            string temporaryPath = string.Format("{0}/_UPD-{1}", existingItemPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            PSFolder tempFolder = null;

            // Raw content IDs for new content items.
            List<long> permanentLinkIDs;

            try
            {
                // Move the entire composite document to staging.
                // This step is not required when creating items since creation takes place in staging.
                PerformTransition(TransitionItemsToStaging, summaryRootID, /*summaryLinkID,*/ PermanentLinkData.GetOldGuids, oldPageIDs/*, oldSubItems*/);

                // Create the new folder, but don't publish the navon.  This is deliberate.
                tempFolder = CMSController.GuaranteeFolder(temporaryPath, FolderManager.NavonAction.None);

                // Update URLs for new and updating Permanent Links
                // This step must go down here because the new page ids must be used to find the right
                // sections, otherwise the URLs will update with old section/page locations. And, this
                // step must go before the Permanent Links are created or updated so that the correct 
                // values are used.
                LogDetailedStep("Begin Permanent Link URL updates.");
                PermanentLinkData.SetURLs(summary.TopLevelSectionList);
                LogDetailedStep("End Permanent Link URL updates.");

                // Create the new Permanent Links in Percussion
                // Add the returned guids to the rollback list just in case there's a problem.
                LogDetailedStep("Begin Permanent Link creation.");
                permanentLinkIDs = PermanentLinkData.CreatePermanentLinks(temporaryPath);
                rollbackList.AddRange(CMSController.BuildGuidArray(permanentLinkIDs));
                LogDetailedStep("End Permanent Link creation.");



                // Update (but don't replace) the CancerInformationSummary and CancerInformationSummaryLink objects.
                ContentItemForUpdating summaryItem = new ContentItemForUpdating(summaryRootID.ID, CreateFieldValueMapPDQCancerInfoSummary(summary));
                //update items in the child table i.e. the summary sections
                summaryItem = UpdateSummaryChildTables(summary, summaryItem);
                                
                List<ContentItemForUpdating> itemsToUpdate = new List<ContentItemForUpdating>(new ContentItemForUpdating[] { summaryItem });
                List<long> updatedItemIDs = CMSController.UpdateContentItemList(itemsToUpdate);
            }
            catch (Exception)
            {
                // If an error occurs while updating attempt to cleanup.
                CMSController.DeleteItemList(rollbackList.ToArray());
                if (tempFolder != null)
                    CMSController.DeleteFolders(new PSFolder[] { tempFolder });

                throw;
            }

            // Remove the old pages.
            // Assumes that there are never any non-summary links to individual pages.
            if (oldPageIDs != null && oldPageIDs.Length > 0)
                RemoveOldPages(oldPageIDs);

            // Permanent Links Updates and Deletion must go outside of the try / catch block. This is 
            // because these changes cannot be rolled back, so we must ensure that there will be no 
            // errors encountered at this point.
            LogDetailedStep("Begin Permanent Link updates and deletion.");
            PermanentLinkData.UpdatePermanentLinks();
            PermanentLinkData.DeletePermanentLinks();
            LogDetailedStep("End Permanent Link updates and deletion.");

            // Move the new items into the main folder.
            PercussionGuid[] componentIDs = CMSController.BuildGuidArray(permanentLinkIDs);
            if (componentIDs.Length > 0)
                CMSController.MoveContentItemFolder(temporaryPath, existingItemPath, componentIDs);
            CMSController.DeleteFolders(new PSFolder[] { tempFolder });

            //Add the PermanentLinks that are marked as 'Update' to the list of PermanentLinkIds that were created
            permanentLinkIDs.AddRange(Array.ConvertAll(PermanentLinkData.GetOldGuids, guid => (long)guid.ID));
            componentIDs = CMSController.BuildGuidArray(permanentLinkIDs);

            // Handle a potential change of URL.
            UpdateDocumentURL(summary.BasePrettyURL, summaryRootID,/* summaryLinkID,*/ componentIDs);

            //update the Nav Label field and add the summary to the landing page slot of the nav on
            UpdateNavOn(summary, summaryRootID, GetTargetFolder(summary.BasePrettyURL));
        }

        /// <summary>
        /// Builds the URL to resolve a reference between two summaries.
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="sectionID">The section ID.</param>
        /// <returns></returns>
        protected override string BuildSummaryRefUrl(SummaryDocument summary, int pageNumber, string sectionID)
        {
            Uri baseUrl = new Uri(summary.BasePrettyURL, UriKind.RelativeOrAbsolute);
            return BuildSummaryRefUrl(baseUrl.AbsolutePath, pageNumber, sectionID);
        }

        /// <summary>
        /// Build the URL to resolve a reference between two sections of the same summary
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="sectionID">The section ID.</param>
        /// <returns></returns>
        protected override string BuildInternalSummaryRefURL(SummaryDocument summary, int pageNumber, string sectionID)
        {
            // On desktop, references within the same summary have the same construction logic as
            // links to other summaries.
            return BuildSummaryRefUrl(summary, pageNumber, sectionID);
        }

        /// <summary>
        /// The base class implementation deletes only the content items specified.
        /// This delete operation is going to remove the standard(desktop) version and the mobile version as well.
        /// A check is made if mobile content also has any dependency, if there are dependency the delete operation
        /// is not done. 
        /// </summary>
        /// <param name="documentID">The summary document identifier.</param>
        /// <param name="sitePath">The site path.</param>
        public override void DeleteContentItem(int documentID, string sitePath)
        {
            // Allow override of the path where the operation will take place.
            if (!string.IsNullOrEmpty(sitePath))
                CMSController.SiteRootPath = sitePath;

            // Retrieve IDs for the summary and all its components. They will be needed
            // for both delete and for delete validation.
            PercussionGuid rootItem = GetCdrDocumentID(CancerInfoSummaryContentType, documentID);

            // A null rootItem means the document has already been deleted.
            // No further work is required.
            if (rootItem != null)
            {
                //PercussionGuid summaryLink = LocateExistingSummaryLink(rootItem);
                PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(rootItem, SummaryPageSlot);
                PermanentLinkHelper PermanentLinkData = new PermanentLinkHelper(CMSController, sitePath);
                PercussionGuid[] permanentLinks = PermanentLinkData.DetectToDeletePermanentLinkRelationships();

                // Create a list of all content IDs making up the document.
                // It is important for verification that rootItem always be first.
                PercussionGuid[] fullIDList = CMSController.BuildGuidArray(rootItem, pageIDs/*, subItems, summaryLink*/);

                VerifyDocumentMayBeDeleted(fullIDList.ToArray(), permanentLinks);

                // Additional check on mobile version content item for dependency before the 
                // the standard version can be deleted.
                PercussionGuid mobileRootItem = rootItem;

                if (mobileRootItem != null)
                {
                    PercussionGuid[] mobilePageIDs = CMSController.SearchForItemsInSlot(mobileRootItem, MobilePageSlotName);

                    // Create a list of all content IDs making up the document.
                    // It is important for verification that rootItem always be first.
                    PercussionGuid[] mobileFullIDList = CMSController.BuildGuidArray(mobileRootItem, mobilePageIDs/*, mobileSubItems, summaryLink*/);

                    VerifyDocumentMayBeDeleted(mobileFullIDList.ToArray(), new PercussionGuid[0]);

                    mobileFullIDList = CMSController.BuildGuidArray(mobilePageIDs);

                    CMSController.DeleteItemList(mobileFullIDList);
                }

                CMSController.DeleteItemList(fullIDList);
            }
        }
                
        
    }
}
