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
        protected override string MediaLinkSnippetTemplate { get { return StandardMediaLinkSnippetTemplate; } }
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
                long[] summaryPageIDList;
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

                // Find the list of content items referenced by table sections.
                // After the table sections are created, these are used to create relationships.
                List<List<PercussionGuid>> tableSectionReferencedItems =
                    ResolveSectionSummaryReferences(document, document.TableSectionList, new StandardSummarySectionFinder(CMSController));

                // Create the sub-pages
                List<long> tableIDs;
                List<long> mediaLinkIDs;
                CreateSubPages(document, createPath, rollbackList, out tableIDs, out mediaLinkIDs);

                // Create relationships from this summary's tables to other Cancer Information Summary Objects.
                PSAaRelationship[] tableExternalRelationships =
                    CreateExternalSummaryRelationships(tableIDs.ToArray(), tableSectionReferencedItems);

                // When creating new summaries, resolve the summmary references after the summary pages are created.
                // Find the list of content items referenced by the summary sections.
                // After the page items are created, these are used to create relationships.
                List<List<PercussionGuid>> pageSectionReferencedSumamries = ResolveSectionSummaryReferences(document, document.TopLevelSectionList, new StandardSummarySectionFinder(CMSController));
                //ResolveSummaryReferences(document);


                //Create Cancer Info Summary Page items
                List<ContentItemForCreating> summaryPageList = CreatePDQCancerInfoSummaryPage(document, createPath);
                List<long> pageRawIDList = CMSController.CreateContentItemList(summaryPageList);
                summaryPageIDList = pageRawIDList.ToArray();
                rollbackList.AddRange(CMSController.BuildGuidArray(pageRawIDList));

                // Add summary pages into the page slot.
                PSAaRelationship[] relationships = CMSController.CreateActiveAssemblyRelationships(summaryRoot.ID, summaryPageIDList, SummaryPageSlot, SummarySectionSnippetTemplate);

                // Create relationships to other Cancer Information Summary Objects.
                PSAaRelationship[] pageExternalRelationships = CreateExternalSummaryRelationships(summaryPageIDList, pageSectionReferencedSumamries);

                //  Search for a CISLink in the parent folder.
                string parentPath = GetParentFolder(document.BasePrettyURL);
                PercussionGuid[] searchList =
                    CMSController.SearchForContentItems(CancerInfoSummaryLinkContentType, parentPath, null);

                // TODO: Turn AudienceType into an enum during Extract.
                string slotName;
                if (document.AudienceType.Equals(PatientAudience, StringComparison.InvariantCultureIgnoreCase))
                    slotName = PatientVersionLinkSlot;
                else
                    slotName = HealthProfVersionLinkSlot;

                PercussionGuid summaryLink;
                if (searchList.Length == 0)
                {
                    // If a summary link doesn't exist, create a new one.
                    string linkItemPath = GetParentFolder(document.BasePrettyURL);
                    List<ContentItemForCreating> summaryLinkList = CreatePDQCancerInfoSummaryLink(document, linkItemPath);
                    List<long> linkRawIDList = CMSController.CreateContentItemList(summaryLinkList);
                    summaryLink = new PercussionGuid(linkRawIDList[0]);
                    rollbackList.Add(summaryLink);
                    CMSController.CreateActiveAssemblyRelationships(summaryLink.ID, new long[] { summaryRoot.ID }, slotName, AudienceLinkSnippetTemplate);
                }
                else
                {
                    // If the summary link does exist, add this summary to the appropriate slot.
                    summaryLink = searchList[0];
                    CMSController.CreateActiveAssemblyRelationships(summaryLink.ID, new long[] { summaryRoot.ID }, slotName, AudienceLinkSnippetTemplate);
                }

                // Create All New Permanent Links (for a New Summary)
                PermanentLinkHelper PermanentLinkData = new PermanentLinkHelper(CMSController, document.PermanentLinkList, createPath);
                PercussionGuid[] newPageIDs = Array.ConvertAll(summaryPageIDList, newPageID => new PercussionGuid(newPageID));
                PermanentLinkData.SetURLs(newPageIDs, document.TopLevelSectionList);
                List<long> permanentLinkIDs = PermanentLinkData.CreatePermanentLinks(createPath);
                rollbackList.AddRange(CMSController.BuildGuidArray(permanentLinkIDs));

                // Find the Patient or Health Professional version and create a relationship.
                LinkRootItemToAlternateAudienceVersion(summaryRoot, document.AudienceType);

                LinkToAlternateLanguageVersion(document, summaryRoot);

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
        protected override void PerformUpdate(SummaryDocument summary, PercussionGuid summaryRootID, PercussionGuid summaryLinkID, PermanentLinkHelper PermanentLinkData,
            PercussionGuid[] oldPageIDs, PercussionGuid[] oldSubItems, PSAaRelationship[] incomingPageRelationships,
            PercussionGuid[] mobilePageIDs, PercussionGuid[] mobileSubItemIDs, PSAaRelationship[] incomingMobilePageRelationships,
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
            List<long> tableIDs;
            List<long> mediaLinkIDs;
            long[] newSummaryPageIDList;
            List<long> permanentLinkIDs;

            try
            {
                // Move the entire composite document to staging.
                // This step is not required when creating items since creation takes place in staging.
                PerformTransition(TransitionItemsToStaging, summaryRootID, summaryLinkID, PermanentLinkData.GetOldGuids, oldPageIDs, oldSubItems);

                // Create the new folder, but don't publish the navon.  This is deliberate.
                tempFolder = CMSController.GuaranteeFolder(temporaryPath, FolderManager.NavonAction.None);

                LogDetailedStep("Begin sub-page setup.");

                // Find the list of content items referenced by table sections.
                // After the table sections are created, these are used to create relationships.
                List<List<PercussionGuid>> tableSectionReferencedItems =
                    ResolveSectionSummaryReferences(summary, summary.TableSectionList, new StandardSummarySectionFinder(CMSController));

                // Create the new sub-page items in a temporary location.
                CreateSubPages(summary, temporaryPath, rollbackList, out tableIDs, out mediaLinkIDs);

                // Create relationships from this summary's tables to other Cancer Information Summary Objects.
                PSAaRelationship[] tableExternalRelationships =
                    CreateExternalSummaryRelationships(tableIDs.ToArray(), tableSectionReferencedItems);

                // Find the list of content items referenced by the summary sections.
                // After the page items are created, these are used to create relationships.
                List<List<PercussionGuid>> pageSectionReferencedItems =
                    ResolveSectionSummaryReferences(summary, summary.TopLevelSectionList, new StandardSummarySectionFinder(CMSController));

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

                //UpdateIncomingSummaryReferences(summary.DocumentID, summaryRootID, summaryLinkID, oldPageIDs, newPageIDs, incomingPageRelationships, new StandardSummarySectionFinder(CMSController));
                UpdateIncomingSummaryReferences(summary.DocumentID, summaryRootID, summaryLinkID, oldPageIDs, newPageIDs, incomingPageRelationships, new StandardSummarySectionFinder(CMSController));

                // Add new cancer information summary pages into the page slot.
                PSAaRelationship[] relationships = CMSController.CreateActiveAssemblyRelationships(summaryRootID.ID, newSummaryPageIDList, SummaryPageSlot, SummarySectionSnippetTemplate);

                // Create relationships from this summary's pages to other Cancer Information Summary Objects.
                PSAaRelationship[] pageExternalRelationships = CreateExternalSummaryRelationships(newSummaryPageIDList, pageSectionReferencedItems);

                LogDetailedStep("End Relationship updates.");


                // Update URLs for new and updating Permanent Links
                // This step must go down here because the new page ids must be used to find the right
                // sections, otherwise the URLs will update with old section/page locations. And, this
                // step must go before the Permanent Links are created or updated so that the correct 
                // values are used.
                LogDetailedStep("Begin Permanent Link URL updates.");
                PermanentLinkData.SetURLs(newPageIDs, summary.TopLevelSectionList);
                LogDetailedStep("End Permanent Link URL updates.");

                // Create the new Permanent Links in Percussion
                // Add the returned guids to the rollback list just in case there's a problem.
                LogDetailedStep("Begin Permanent Link creation.");
                permanentLinkIDs = PermanentLinkData.CreatePermanentLinks(temporaryPath);
                rollbackList.AddRange(CMSController.BuildGuidArray(permanentLinkIDs));
                LogDetailedStep("End Permanent Link creation.");

                // Update (but don't replace) the CancerInformationSummary and CancerInformationSummaryLink objects.
                ContentItemForUpdating summaryItem = new ContentItemForUpdating(summaryRootID.ID, CreateFieldValueMapPDQCancerInfoSummary(summary));
                ContentItemForUpdating summaryLinkItem = new ContentItemForUpdating(summaryLinkID.ID, CreateFieldValueMapPDQCancerInfoSummaryLink(summary));
                List<ContentItemForUpdating> itemsToUpdate = new List<ContentItemForUpdating>(new ContentItemForUpdating[] { summaryItem, summaryLinkItem });
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

            // Remove the old pages, table sections and medialink items.
            // Assumes that there are never any non-summary links to individual pages.
            // No links from other summaries to table sections and media links.
            RemoveOldPages(oldPageIDs, oldSubItems);

            // Move the new items into the main folder.
            PercussionGuid[] componentIDs = CMSController.BuildGuidArray(tableIDs, mediaLinkIDs, newSummaryPageIDList, permanentLinkIDs);
            CMSController.MoveContentItemFolder(temporaryPath, existingItemPath, componentIDs);
            CMSController.DeleteFolders(new PSFolder[] { tempFolder });

            // Handle a potential change of URL.
            UpdateDocumentURL(summary.BasePrettyURL, summaryRootID, summaryLinkID, componentIDs);

            // Permanent Links Updates and Deletion must go outside of the try / catch block. This is 
            // because these changes cannot be rolled back, so we must ensure that there will be no 
            // errors encountered at this point.
            LogDetailedStep("Begin Permanent Link updates and deletion.");
            PermanentLinkData.UpdatePermanentLinks();
            PermanentLinkData.DeletePermanentLinks();
            LogDetailedStep("End Permanent Link updates and deletion.");
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
                PercussionGuid summaryLink = LocateExistingSummaryLink(rootItem);
                PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(rootItem, SummaryPageSlot);
                PercussionGuid[] subItems = LocateMediaLinksAndTableSections(pageIDs); // Table sections and MediaLinks.
                PermanentLinkHelper PermanentLinkData = new PermanentLinkHelper(CMSController, sitePath);
                PercussionGuid[] permanentLinks = PermanentLinkData.DetectToDeletePermanentLinkRelationships();

                // Create a list of all content IDs making up the document.
                // It is important for verification that rootItem always be first.
                PercussionGuid[] fullIDList = CMSController.BuildGuidArray(rootItem, pageIDs, subItems, summaryLink);

                VerifyDocumentMayBeDeleted(fullIDList.ToArray(), permanentLinks);

                // Additional check on mobile version content item for dependency before the 
                // the standard version can be deleted.
                PercussionGuid mobileRootItem = rootItem;

                if (mobileRootItem != null)
                {
                    PercussionGuid[] mobilePageIDs = CMSController.SearchForItemsInSlot(mobileRootItem, MobilePageSlotName);
                    PercussionGuid[] mobileSubItems = LocateMediaLinksAndTableSections(mobilePageIDs); // Table sections and MediaLinks.

                    // Create a list of all content IDs making up the document.
                    // It is important for verification that rootItem always be first.
                    PercussionGuid[] mobileFullIDList = CMSController.BuildGuidArray(mobileRootItem, mobilePageIDs, mobileSubItems, summaryLink);

                    VerifyDocumentMayBeDeleted(mobileFullIDList.ToArray(), new PercussionGuid[0]);

                    mobileFullIDList = CMSController.BuildGuidArray(mobilePageIDs, mobileSubItems);

                    CMSController.DeleteItemList(mobileFullIDList);
                }

                CMSController.DeleteItemList(fullIDList);
            }
        }
    }
}
