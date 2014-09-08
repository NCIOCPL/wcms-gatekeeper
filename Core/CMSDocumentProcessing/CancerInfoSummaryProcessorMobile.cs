using System;
using System.Collections.Generic;

using GateKeeper.Common;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.CMSDocumentProcessing.Configuration;
using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSDocumentProcessing
{
    public class CancerInfoSummaryProcessorMobile
        : CancerInfoSummaryProcessorCommon
    {
        #region Protected Members

        protected override string SummaryPageSlot { get { return MobilePageSlotName; } }
        protected override string PageSnippetTemplateName { get { return MobileSummarySectionSnippetTemplate; } }
        protected override string MediaLinkSnippetTemplate { get { return MobileMediaLinkSnippetTemplate; } }
        #endregion


        public CancerInfoSummaryProcessorMobile(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        {
        }

        /// <summary>
        /// Creates a new Summary document.
        /// </summary>
        /// <param name="document">SummaryDocument object containing the summary to be stored in the CMS.</param>
        /// <param name="sitePath">BasePath for the site where the content structure is to be stored.</param>
        /// <returns>Percussion ID for the root content item.</returns>
        protected override PercussionGuid CreateNewCancerInformationSummary(SummaryDocument document, string sitePath)
        {
            // Summaries must be placed on the desktop before mobile. Allowing this assumption means
            // this method is never called.  The assumption is enforced in part through the ProcessorPool
            // object which always returns the screen ProcessingTarget first.
            throw new NotImplementedException("Summaries must be created on the desktop site before mobile.");
        }

        /// <summary>
        /// Updates an existing Summary document.
        /// </summary>
        /// <param name="summary">The summary to update.</param>
        /// <param name="summaryRootID">ID of the summary's root object.</param>
        /// <param name="sitePath">BasePath for the site where the content structure is to be stored.</param>
        protected override void PerformUpdate(SummaryDocument summary, PercussionGuid summaryRootID, PercussionGuid summaryLinkID, PermanentLinkHelper permanentLinkData,
            PercussionGuid[] desktopPageIDs, PercussionGuid[] desktopSubItemIDs, PSAaRelationship[] incomingDesktopPageRelationships,
            PercussionGuid[] mobilePageIDs, PercussionGuid[] mobileSubItemIDs, PSAaRelationship[] incomingMobilePageRelationships,
            string sitePath)
        {
            if (string.IsNullOrEmpty(sitePath))
                throw new ArgumentNullException("sitePath");

            // SummaryPageSlot points to the mobile page slot.  If there are no mobile pages, we're creating the
            // mobile edition for the first time.
            if (mobilePageIDs.Length == 0)
            {
                // Create Mobile for the first time.
                CreateMobilePageStructure(summary, summaryRootID, summaryLinkID, sitePath);
            }
            else
            {
                UpdateMobilePageStructure(summary, summaryLinkID, summaryRootID,
                    mobilePageIDs, mobileSubItemIDs, incomingMobilePageRelationships, sitePath);
            }
        }

        private void CreateMobilePageStructure(SummaryDocument document, 
            PercussionGuid summaryRoot, 
            PercussionGuid summaryLink, 
            string sitePath)
        {
            // For undoing failed attempts.
            List<PercussionGuid> rollbackList = new List<PercussionGuid>();

            // Get the paths
            string createPath = GetTargetFolder(document.BaseMobileURL);
            string rootItemPrettyUrlName = GetSummaryPrettyUrlName(document.BaseMobileURL);
            string desktopFolder = GetTargetFolder(document.BasePrettyURL);

            string originalSitePath = CMSController.SiteRootPath;

            try
            {
                // Move existing document components to staging.
                // Pages and sub-pages don't yet exist.
                PerformTransition(TransitionItemsToStaging, summaryRoot, summaryLink, new PercussionGuid[0], null, null);

                CMSController.SiteRootPath = sitePath;

                // Content structure components.
                long[] summaryPageIDList;

                // Create the folder where the content items are to be created and set the Navon to public.
                CMSController.GuaranteeFolder(createPath, FolderManager.NavonAction.MakePublic);

                // Find the list of content items referenced by table sections.
                // After the table sections are created, these are used to create relationships.
                //List<List<PercussionGuid>> tableSectionReferencedItems =
                //    ResolveSectionSummaryReferences(document, document.TableSectionList, new MobileSummarySectionFinder(CMSController));

                // Create the sub-pages
                //List<long> tableIDs;
                //List<long> mediaLinkIDs;
                //CreateSubPages(document, createPath, rollbackList, out tableIDs, out mediaLinkIDs);

                // Create relationships from this summary's tables to other Cancer Information Summary Objects.
                //PSAaRelationship[] tableExternalRelationships =
                //    CreateExternalSummaryRelationships(tableIDs.ToArray(), tableSectionReferencedItems);

                // When creating new summaries, resolve the summmary references after the summary pages are created.
                // Find the list of content items referenced by the summary sections.
                // After the page items are created, these are used to create relationships.
                List<List<PercussionGuid>> pageSectionReferencedSumamries = ResolveSectionSummaryReferences(document, document.TopLevelSectionList, new MobileSummarySectionFinder(CMSController));

                //Create Cancer Info Summary Page items
                List<ContentItemForCreating> summaryPageList = CreatePDQCancerInfoSummaryPage(document, createPath);
                List<long> pageRawIDList = CMSController.CreateContentItemList(summaryPageList);
                summaryPageIDList = pageRawIDList.ToArray();
                rollbackList.AddRange(CMSController.BuildGuidArray(pageRawIDList));

                // Add summary pages to the mobile page slot.
                PSAaRelationship[] relationships = CMSController.CreateActiveAssemblyRelationships(summaryRoot.ID, summaryPageIDList, SummaryPageSlot, MobileSummarySectionSnippetTemplate);

                // Create relationships to other Cancer Information Summary Objects.
                PSAaRelationship[] pageExternalRelationships = CreateExternalSummaryRelationships(summaryPageIDList, pageSectionReferencedSumamries);

                // Link to alternate language version.
                // This step is not needed. The relationship was created when the desktop version was created
                // and the desktop version is always created before the mobile one.
                // LinkToAlternateLanguageVersion(document, summaryRoot);

                // Mobile doesn't update the SummaryLink content item.

                // Update (but don't replace) the CancerInformationSummary object.
                ContentItemForUpdating summaryItem = new ContentItemForUpdating(summaryRoot.ID, CreateFieldValueMapPDQCancerInfoSummary(document));
                List<ContentItemForUpdating> itemsToUpdate = new List<ContentItemForUpdating>(new ContentItemForUpdating[] { summaryItem });
                List<long> updatedItemIDs = CMSController.UpdateContentItemList(itemsToUpdate, null, new List<string>(new string[] { createPath }));
            }
            catch (Exception)
            {
                // If an error occurs while updating attempt to cleanup.
                CMSController.DeleteItemList(rollbackList.ToArray());

                // Restore original site path.
                CMSController.SiteRootPath = originalSitePath;

                throw;
            }

            // Restore original site path.
            CMSController.SiteRootPath = originalSitePath;
        }

        private void UpdateMobilePageStructure(SummaryDocument summary,
            PercussionGuid summaryLink, PercussionGuid summaryRoot,
            PercussionGuid[] oldpageIDs, PercussionGuid[] oldSubItems, PSAaRelationship[] incomingPageRelationships,
            string sitePath)
        {
            // For undoing failed updates.
            List<PercussionGuid> rollbackList = new List<PercussionGuid>();

            string originalSitePath = CMSController.SiteRootPath;
            CMSController.SiteRootPath = sitePath;

            // Does the item have a path?  Any path at all....
            PSItem[] summaryRootItem = CMSController.LoadContentItems(new PercussionGuid[] { summaryRoot });
            VerifyItemHasPath(summaryRootItem[0], summary.DocumentID);

            string existingItemPath = CMSController.GetPathInSite(summaryRootItem[0]);

            string newPath = GetTargetFolder(summary.BaseMobileURL);
            string prettyUrlName = GetSummaryPrettyUrlName(summary.BaseMobileURL);

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
            //List<long> tableIDs;
            //List<long> mediaLinkIDs;
            long[] newSummaryPageIDList;

            try
            {
                // Move the entire composite document to staging.
                PerformTransition(TransitionItemsToStaging, summaryRoot, summaryLink, new PercussionGuid[0], oldpageIDs, oldSubItems);

                // Create the new folder, but don't publish the navon.  This is deliberate.
                tempFolder = CMSController.GuaranteeFolder(temporaryPath, FolderManager.NavonAction.None);

                LogDetailedStep("Begin sub-page setup.");

                // Find the list of content items referenced by table sections.
                // After the table sections are created, these are used to create relationships.
                //List<List<PercussionGuid>> tableSectionReferencedItems =
                //    ResolveSectionSummaryReferences(summary, summary.TableSectionList, new MobileSummarySectionFinder(CMSController));

                // Create the new sub-page items in a temporary location.
                //CreateSubPages(summary, temporaryPath, rollbackList, out tableIDs, out mediaLinkIDs);

                // Create relationships from this summary's tables to other Cancer Information Summary Objects.
                //PSAaRelationship[] tableExternalRelationships =
                //    CreateExternalSummaryRelationships(tableIDs.ToArray(), tableSectionReferencedItems);

                // Find the list of content items referenced by the summary sections.
                // After the page items are created, these are used to create relationships.
                List<List<PercussionGuid>> pageSectionReferencedItems =
                    ResolveSectionSummaryReferences(summary, summary.TopLevelSectionList, new MobileSummarySectionFinder(CMSController));

                LogDetailedStep("End sub-page setup.");


                // Create new Cancer Info Summary Page items

                LogDetailedStep("Begin page creation.");


                List<long> idList;
                List<ContentItemForCreating> summaryPageList = CreatePDQCancerInfoSummaryPage(summary, temporaryPath);
                idList = CMSController.CreateContentItemList(summaryPageList);
                newSummaryPageIDList = idList.ToArray();
                PercussionGuid[] newPageIDs = Array.ConvertAll(newSummaryPageIDList, pageID => new PercussionGuid(pageID));
                rollbackList.AddRange(newPageIDs);

                LogDetailedStep("End page creation.");

                LogDetailedStep("Begin Relationship updates.");

                UpdateIncomingSummaryReferences(summary.DocumentID, summaryRoot, summaryLink, oldpageIDs, newPageIDs, incomingPageRelationships, new MobileSummarySectionFinder(CMSController));

                // Add new cancer information summary pages into the page slot.
                PSAaRelationship[] relationships = CMSController.CreateActiveAssemblyRelationships(summaryRoot.ID, newSummaryPageIDList, SummaryPageSlot, MobileSummarySectionSnippetTemplate);

                // Create relationships from this summary's pages to other Cancer Information Summary Objects.
                PSAaRelationship[] pageExternalRelationships = CreateExternalSummaryRelationships(newSummaryPageIDList, pageSectionReferencedItems);

                LogDetailedStep("End Relationship updates.");

                // Update (but don't replace) the CancerInformationSummary and CancerInformationSummaryLink objects.
                ContentItemForUpdating summaryItem = new ContentItemForUpdating(summaryRoot.ID, CreateFieldValueMapPDQCancerInfoSummary(summary));
                ContentItemForUpdating summaryLinkItem = new ContentItemForUpdating(summaryLink.ID, CreateFieldValueMapPDQCancerInfoSummaryLink(summary));
                List<ContentItemForUpdating> itemsToUpdate = new List<ContentItemForUpdating>(new ContentItemForUpdating[] { summaryItem, summaryLinkItem });
                List<long> updatedItemIDs = CMSController.UpdateContentItemList(itemsToUpdate);
            }
            catch (Exception)
            {
                // In the event of an error while updating attempt to cleanup.
                CMSController.DeleteItemList(rollbackList.ToArray());
                if (tempFolder != null)
                    CMSController.DeleteFolders(new PSFolder[] { tempFolder });

                // Restore original site path.
                CMSController.SiteRootPath = originalSitePath;

                throw;
            }

            // Remove the old pages, table sections and medialink items.
            // Assumes that there are never any non-summary links to individual pages.
            // No links from other summaries to table sections and media links.
            RemoveOldPages(oldpageIDs, oldSubItems);

            //// Move the new items into the main folder.
            PercussionGuid[] componentIDs = CMSController.BuildGuidArray(/*tableIDs, mediaLinkIDs, */newSummaryPageIDList);
            CMSController.MoveContentItemFolder(temporaryPath, existingItemPath, componentIDs);
            CMSController.DeleteFolders(new PSFolder[] { tempFolder });

            // Handle a potential change of URL.
            UpdateDocumentURL(summary.BaseMobileURL, summaryRoot, summaryLink, componentIDs);

            // Restore original site path.
            CMSController.SiteRootPath = originalSitePath;
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
            string referenceUrl;

            Uri baseUrl = new Uri(summary.BaseMobileURL, UriKind.RelativeOrAbsolute);

            if (baseUrl.IsAbsoluteUri)
                referenceUrl = BuildSummaryRefUrl(baseUrl.AbsolutePath, pageNumber, sectionID);
            else
                referenceUrl = BuildSummaryRefUrl(summary.BaseMobileURL.Trim(), pageNumber, sectionID);

            return referenceUrl;
        }

        /// <summary>
        /// Build the URL to resolve a reference between two sections of the same summary
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <param name="pageNumber">The page number.  (Not used on mobile)</param>
        /// <param name="sectionID">The section ID.</param>
        /// <returns></returns>
        protected override string BuildInternalSummaryRefURL(SummaryDocument summary, int pageNumber, string sectionID)
        {
            string referenceUrl;

            Uri baseUrl = new Uri(summary.BaseMobileURL, UriKind.RelativeOrAbsolute);

            if (baseUrl.IsAbsoluteUri)
                referenceUrl = BuildSummaryRefUrl(baseUrl.AbsolutePath, sectionID);
            else
                referenceUrl = BuildSummaryRefUrl(summary.BaseMobileURL.Trim(), sectionID);

            return referenceUrl;
        }

        /// <summary>
        /// Searches the CMS repository for the specified CDR Document.
        /// 
        /// This version overrides the default implementation and searches the
        /// mobile site followed by the desktop site.
        /// </summary>
        /// <param name="contentType">The CMS content type of the document being searched for.</param>
        /// <param name="cdrID">The document's CDR ID</param>
        /// <returns>
        /// If found, a Percussion GUID value is returned which identifies the document.
        /// A null return means no matching document was located.
        /// </returns>
        public override PercussionGuid GetCdrDocumentID(string contentType, int cdrID)
        {
            CMSProcessingSection config = CMSProcessingSection.Instance;
            PercussionGuid docContentId;

            docContentId = base.GetCdrDocumentID(contentType, config.BaseFolders.MobileSiteBase, null, cdrID);

            if(docContentId == null)
                docContentId = base.GetCdrDocumentID(contentType, config.BaseFolders.DesktopSiteBase, null, cdrID);

            return docContentId;
        }

        /// <summary>
        /// Locates all PermanentLink content items within the root's folder.
        /// </summary>
        /// <param name="rootItemID">Needed to find the folder in which you're search for content.</param>
        /// <returns>The CMS identifers for the embedded content items.</returns>
        protected override PercussionGuid[] LocateExistingPermanentLinks(PercussionGuid rootItemID)
        {
            return new PercussionGuid[0];
        }
    }
}
