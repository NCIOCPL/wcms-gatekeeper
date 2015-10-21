using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.DocumentObjects.Media;
using GKManagers.CMSManager.Configuration;

using NCI.WCM.CMSManager;
using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

// TODO: Move all the CreateXXXXXX content type methods into a single factory object.
// As written, CancerInfoSummaryProcessorStandard has way too many concerns, with too many methods
// that aren't directly related to putting things into the CMS.
// Take the document as an argument on the constructor, expose properties with collections
// of strongly typed meta content types with a single base class

namespace GKManagers.CMSDocumentProcessing
{
    /// <summary>
    /// Abstract base class containing common code for processing Cancer Information Summaries.
    /// </summary>
    public abstract class CancerInfoSummaryProcessorCommon : DocumentProcessorCommon, IDocumentProcessor, IDisposable
    {
        public PercussionConfig PercussionConfig { get; private set; }

        #region Constants

        //changed this length from 64 to 100 to make sure it matches short_title length
        //in percussion
        const int ShortTitleLength = 100;

        //The meta keywords in Percussion
        const int MetaKeywordsLength = 255;

        protected const string SummaryRefSlot = "pdqCancerInformationSummaryRef";

        protected const string SummarySectionSnippetTemplate = "pdqSnCancerInformationSummaryPage";
        protected const string MobileSummarySectionSnippetTemplate = "pdqMSnCancerInformationSummaryPage";
                
        protected const string PatientAudience = "Patients";
        protected const string HealthProfAudience = "HealthProfessional";

        const string NavonType = "rffNavon";
        const string NavonCommunity = "CancerGov";
        const string NavOnLandingPageSlot = "rffNavLandingPage";
        const string NavOnSnippetTemplate = "cgvSnTitleLink";

        #endregion

        #region Runtime Constants

        // Yeah, it's a funny name for the region. These values are loaded at runtime,
        // but aren't allowed to change during the run.

        // Contain the names of the ContentTypes used to represent Cancer Info Summaries in the CMS.
        // Set in the constructor.
        readonly protected string CancerInfoSummaryContentType;
        readonly protected string CancerInfoSummaryPageContentType;
        readonly protected string CancerInfoSummaryLinkContentType;
        readonly protected string MediaLinkContentType;
        readonly protected string TableSectionContentType;
        readonly protected string PermanentLinkContentType;

        readonly protected string[] SummaryContentTypes;

        #endregion

        #region Internal Delegates

        protected delegate string SubDocumentIDDelegate<SubDocumentType>(SubDocumentType subDocument);
        protected delegate void ItemTransitionDelegate(PercussionGuid[] itemList);

        #endregion

        #region Protected Members

        public const string StandardPageSlotName = "pdqCancerInformationSummaryPageSlot";
        public const string MobilePageSlotName = "pdqCancerInformationSummaryMobilePageSlot";

        protected abstract string SummaryPageSlot { get; }
        protected abstract string PageSnippetTemplateName { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CancerInfoSummaryProcessorCommon"/> class.
        /// </summary>
        /// <param name="warningWriter">HistoryEntryWriter for output of warning messages.</param>
        /// <param name="informationWriter">HistoryEntryWriter for output of informational messages.</param>
        public CancerInfoSummaryProcessorCommon(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        {
            PercussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            CancerInfoSummaryContentType = PercussionConfig.ContentType.PDQCancerInfoSummary.Value;
            CancerInfoSummaryPageContentType = PercussionConfig.ContentType.PDQCancerInfoSummaryPage.Value;
            CancerInfoSummaryLinkContentType = PercussionConfig.ContentType.PDQCancerInfoSummaryLink.Value;
            MediaLinkContentType = PercussionConfig.ContentType.PDQMediaLink.Value;
            TableSectionContentType = PercussionConfig.ContentType.PDQTableSection.Value;
            PermanentLinkContentType = PercussionConfig.ContentType.PDQPermanentLink.Value;

            SummaryContentTypes = new String[] {CancerInfoSummaryContentType, CancerInfoSummaryPageContentType,
                                           CancerInfoSummaryLinkContentType, MediaLinkContentType,
                                           TableSectionContentType, PermanentLinkContentType};
        }

        #region IDocumentProcessor Members

        /// <summary>
        /// Main entry point for processing a Cancer Information Summary (formerly just "Summary")
        /// object which is to be managed in the CMS.
        /// </summary>
        /// <param name="documentObject">Reference to a SummaryDocument object.</param>
        public override void ProcessDocument(Document documentObject)
        {
            ProcessDocument(documentObject, CMSController.SiteRootPath);
        }

        /// <summary>
        /// Main entry point for processing a Cancer Information Summary (formerly just "Summary")
        /// object which is to be managed in the CMS.
        /// </summary>
        /// <param name="documentObject">Reference to a SummaryDocument object.</param>
        /// <param name="sitePath">The base path for the site where the document is to be stored.</param>
        public override void ProcessDocument(Document documentObject, string sitePath)
        {
            // Call the internal private method for processing the document.
            processDocumentInternal(documentObject, sitePath);
        }

        /// <summary>
        /// Move the specified Summary document from Staging to Preview.
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public override void PromoteToPreview(int documentID)
        {
            PromoteToPreview(documentID, null);
        }

        public override void PromoteToPreview(int documentID, string sitePath)
        {
            // Allow override of the path where the operation will take place.
            if (!string.IsNullOrEmpty(sitePath))
                CMSController.SiteRootPath = sitePath;

            PercussionGuid summaryRootID = GetCdrDocumentID(CancerInfoSummaryContentType, documentID);
           
            // If Permanent Links exist, we need to worry about performing transitions
            // this will return null for mobile and then potentially populated with values on the desktop
            // as PermanentLink content items should not exist on Mobile
            PercussionGuid[] permanentLinkIDs = LocateExistingPermanentLinks(summaryRootID);

            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(summaryRootID, SummaryPageSlot);

            PerformTransition(TransitionItemsToPreview, summaryRootID, permanentLinkIDs, pageIDs);
        }

        /// <summary>
        /// Move the specified DrugInfoSummary document from Preview to Live.
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public override void PromoteToLive(int documentID)
        {
            PromoteToLive(documentID, null);
        }

        public override void PromoteToLive(int documentID, string sitePath)
        {
            // Allow override of the path where the operation will take place.
            if (!string.IsNullOrEmpty(sitePath))
                CMSController.SiteRootPath = sitePath;

            PercussionGuid summaryRootID = GetCdrDocumentID(CancerInfoSummaryContentType, documentID);
                      

            // If Permanent Links exist, we need to worry about performing transitions
            // this will return null for mobile and then potentially populated with values on the desktop
            // as PermanentLink content items should not exist on Mobile
            PercussionGuid[] permanentLinkIDs = LocateExistingPermanentLinks(summaryRootID);

            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(summaryRootID, SummaryPageSlot);

            PerformTransition(TransitionItemsToLive, summaryRootID, permanentLinkIDs, pageIDs);
        }

        /// <summary>
        /// Move the specified Summary document from Staging to Live and skip Preview Step
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public override void PromoteToLiveFast(int documentID)
        {
            PromoteToLiveFast(documentID, null);
        }

        public override void PromoteToLiveFast(int documentID, string sitePath)
        {
            LogDetailedStep("Cancer Info Summary Promote To Live Fast");

            // Allow override of the path where the operation will take place.
            if (!string.IsNullOrEmpty(sitePath))
                CMSController.SiteRootPath = sitePath;

            PercussionGuid summaryRootID = GetCdrDocumentID(CancerInfoSummaryContentType, documentID);
           
            // If Permanent Links exist, we need to worry about performing transitions
            // this will return null for mobile and then potentially populated with values on the desktop
            // as PermanentLink content items should not exist on Mobile
            PercussionGuid[] permanentLinkIDs = LocateExistingPermanentLinks(summaryRootID);

            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(summaryRootID, SummaryPageSlot);

            PerformTransition(TransitionItemsToLiveFast, summaryRootID, permanentLinkIDs, pageIDs);

        }

        protected void PerformTransition(ItemTransitionDelegate transitionMethod,
            PercussionGuid summaryRootID,
            PercussionGuid[] permanentLinkIDs,
            PercussionGuid[] pageIDs)
        {
            LogDetailedStep("Begin workflow transition.");

            // Root item and Link must move independently of each other and everything else.
            if (summaryRootID != null) transitionMethod(new PercussionGuid[] { summaryRootID });
            
            if (permanentLinkIDs != null)
            {
                PerformSeparateTransitions(transitionMethod, permanentLinkIDs);
            }

            // Pages and subPages are created new on each update and will therefore be in
            // a different state than the root and link items.
            if (pageIDs != null)
            {
                PercussionGuid[] pageCollection = CMSController.BuildGuidArray(pageIDs);
                if (pageCollection.Length > 0) transitionMethod(pageCollection);
            }
            LogDetailedStep("End workflow transition.");
        }

        /// <summary>
        /// If there are multiple states for the same type of content, it will not transition properly 
        /// with PerformTransitions. So, must use this function, which separates the content items by current
        /// workflow position and then updates accordingly.
        /// </summary>
        /// <param name="transitionMethod">Abstracted method to promote items through the workflow.</param>
        /// <param name="contentGuids">The Guids are needed to identify which content items will be transferred.</param>
        protected void PerformSeparateTransitions(ItemTransitionDelegate transitionMethod, PercussionGuid[] contentGuids)
        {
            // If there are no content guids, there is no need to transition them
            // And null will break a foreach
            if (contentGuids != null)
            {

                // Transition each ID separately as only things with the same state can be transferred
                // as a group
                foreach (PercussionGuid id in contentGuids)
                {
                    transitionMethod(new PercussionGuid[] { id });
                }
            }
        }

        /// <summary>
        /// Creates a new Summary document.
        /// </summary>
        /// <param name="document">SummaryDocument object containing the summary to be stored in the CMS.</param>
        /// <param name="sitePath">BasePath for the site where the content structure is to be stored.</param>
        /// <returns>Percussion ID for the root content item.</returns>
        protected abstract PercussionGuid CreateNewCancerInformationSummary(SummaryDocument document, string sitePath);

        /// <summary>
        /// Performs the shared logic for updating an existing Summary document.
        /// </summary>
        /// <param name="summary">The summary to update.</param>
        /// <param name="summaryRootID">ID of the summary's root object.</param>
        /// <param name="sitePath">BasePath for the site where the content structure is to be stored.</param>
        protected virtual void UpdateCancerInformationSummary(SummaryDocument summary, PercussionGuid summaryRootID, string sitePath)
        {
            if (string.IsNullOrEmpty(sitePath))
                throw new ArgumentNullException("sitePath");

            LogDetailedStep("Begin gathering content items.");
                       
            // Lookup desktop pages
            PercussionGuid[] desktopPageIDs = CMSController.SearchForItemsInSlot(summaryRootID, StandardPageSlotName);

            
            LogDetailedStep("End gathering content items.");

            LogDetailedStep("Begin gathering incoming relationships.");

            // Determine which pages in this summary are dependent items in other summaries.
            PSAaRelationship[] desktopPageRelationships = FindIncomingPageRelationships(summaryRootID, desktopPageIDs);
           
            LogDetailedStep("End gathering incoming relationships.");

            List<PSAaRelationship> allPageRelationships = new List<PSAaRelationship>();
            allPageRelationships.AddRange(desktopPageRelationships);
           

            LogDetailedStep("Begin Permanent Link evaluation.");
            PSItem[] summaryRootItem = CMSController.LoadContentItems(new PercussionGuid[] { summaryRootID });
            //get the existing item path for the Permanent Link Helper 
            //this will ensure no errors are thrown if the URL changes when updating summaries
            string existingItemPath = CMSController.GetPathInSite(summaryRootItem[0]);
            PermanentLinkHelper PermanentLinkData = new PermanentLinkHelper(CMSController, summary.PermanentLinkList, existingItemPath);
            
            PercussionGuid[] permanentLinkGuids = PermanentLinkData.GetOldGuids;
            PSAaRelationship[] incomingPermanentLinkRelationships = new PSAaRelationship[0];
            if (permanentLinkGuids.Length > 0)
            {
                incomingPermanentLinkRelationships = FindExternalRelationships(permanentLinkGuids);
            }
            LogDetailedStep("End Permanent Link evaluation.");

            // Is the summary in a state suitable for updating?
            VerifyDocumentMayBeUpdated(summary, summaryRootID, PermanentLinkData, incomingPermanentLinkRelationships, allPageRelationships.ToArray());

            PerformUpdate(summary, summaryRootID, PermanentLinkData,
                desktopPageIDs, desktopPageRelationships,
                sitePath);
        }

        /// <summary>
        /// Device-specific logic for summary updates.
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="summaryRootID"></param>
        /// <param name="sitePath"></param>
        protected abstract void PerformUpdate(SummaryDocument summary, PercussionGuid summaryRootID,
            PermanentLinkHelper permanentLinkData,
            PercussionGuid[] desktopPageIDs, PSAaRelationship[] incomingDesktopPageRelationships,
            string sitePath);


        /// <summary>
        /// Performs checks to verify that the summary document is in a condition suitable for updating.
        /// This is primarily a "system integrity" test to validate the assumption that no page-level
        /// content items are directly referenced by content items not managed by GateKeeper.
        /// Throws CannotUpdateException if the document may not be updated.
        /// </summary>
        /// <param name="summary">The updated summary.</param>
        /// 
        /// <param name="summaryRoot">PercussionGuid of the composite document's root CancerInfoSummary content item.</param>
        /// <param name="summaryLink">PercussionGuid of the composite document's root CancerInfoSummaryLink content item.</param>
        /// <param name="incomingPageRelationships">Array of Percussion Relationship structures for items linking
        /// to the individual summary pages.</param>
        protected void VerifyDocumentMayBeUpdated(SummaryDocument summary, PercussionGuid summaryRoot, PermanentLinkHelper PermanentLinkData, PSAaRelationship[] incomingPermanentLinkRelationships, PSAaRelationship[] incomingPageRelationships)
        {
            List<string> errorList = new List<string>();

            // Cache to prevent loading items repeatedly
            ItemCache itemStore = new ItemCache(CMSController);


            // Inspect incoming relationships to Permanent Links
            if (incomingPermanentLinkRelationships.Length > 0)
            {
                // At this time, we are choosing to not allow relationships to links that are going to be deleted
                PercussionGuid[] toBeDeletedPermanentLinkGuids = PermanentLinkData.DetectToDeletePermanentLinkRelationships();

                foreach (PSAaRelationship relationship in incomingPermanentLinkRelationships)
                {
                    PercussionGuid dependentID = new PercussionGuid(relationship.dependentId);

                    if (toBeDeletedPermanentLinkGuids.Contains(dependentID))
                    {
                        PercussionGuid parentID = new PercussionGuid(relationship.ownerId);
                        PSItem parentItem = itemStore.LoadContentItem(parentID);

                        errorList.Add(CISErrorBuilder.BuildPermanentLinkError(dependentID, summary.DocumentID, parentItem));
                    }

                }
            }
                       
            // If any errors occured, errorList will have a non-zero count.
            // In this case, we report the error by throwing CannotUpdateException
            if (errorList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                errorList.ForEach(message =>
                {
                    if (sb.Length > 0)
                        sb.Append(",\n");
                    sb.Append(message);
                });

                throw new CannotUpdateException(sb.ToString());
            }
        }


        protected void RemoveOldPages(PercussionGuid[] oldPageIDs)
        {
            CMSController.DeleteItemList(oldPageIDs);
        }
                
        //OCEPROJECT-1765 - Remove summary link dependency
        /// <summary>
        /// Determines the set of active assembly relationships which come from other
        /// content items.
        /// </summary>
        /// <param name="internalIdentifers">A collection of the content IDs which are
        /// considered to be part of the content item.  The first item in the array
        /// is assumed to be the root CancerInformationSummary object.</param>
        /// <returns></returns>
        protected PSAaRelationship[] FindExternalRelationships(PercussionGuid[] internalIdentifers)
        {
            // The first item in the collection is always root.
            PercussionGuid rootItem = internalIdentifers[0];
            
            /// Find all the incoming relationships.  (Need to include inline links!)
            PSAaRelationship[] incomingRelationship =
                CMSController.FindIncomingActiveAssemblyRelationships(internalIdentifers);

            List<PSAaRelationship> candidateRelationships = new List<PSAaRelationship>(incomingRelationship);

            /// Filter out any incoming relationships we can identify.
            candidateRelationships.RemoveAll(relationship =>
            {
                PercussionGuid ownerID = new PercussionGuid(relationship.ownerId);
                return internalIdentifers.Contains(ownerID); // owner is part of the document.
            });

            // Any remaining relationships are external
            return candidateRelationships.ToArray();
        }

        protected PSAaRelationship[] FindIncomingPageRelationships(PercussionGuid summaryRootID,
            PercussionGuid[] oldPageIDs)
        {
            PercussionGuid[] currentIDs = CMSController.BuildGuidArray(summaryRootID, oldPageIDs);
            PSAaRelationship[] externalRelationships = FindExternalRelationships(currentIDs);

            // FindExternalRelationships finds all CISummary relationships external to the items specified.
            // We also want to ignore references from the SummaryLink, the main Summary item, and any
            // relationships with the main summary item as the dependent (only references to individual pages
            // need to be updated and this is completely dependent on the assumption that there will never a
            // link from an external item directly to a page.
            List<PSAaRelationship> relationshipList = new List<PSAaRelationship>(externalRelationships);
            relationshipList.RemoveAll(relationship =>
            {
                PercussionGuid ownerID = new PercussionGuid(relationship.ownerId);
                PercussionGuid dependent = new PercussionGuid(relationship.dependentId);
                return ownerID == summaryRootID     // Filter out links from root
                    || dependent == summaryRootID; // Filter out relationships which own the root.
            });

            // Anything left at this point is a relationship to a CancerInfoSummaryPage object.
            // NOTE: This assumes the only objects which can reference a CancerInfoSummaryPage
            // are CancerInfoSummary objects and other CancerInfoSummaryPage objects.
            return relationshipList.ToArray();
        }

        /// <summary>
        /// Helper method to verify that an existing content items belongs to a folder path, any folder path.
        /// Throws FolderAssociationException if item is not associated with a folder.
        /// </summary>
        /// <param name="item">The content item to be checked for a path.</param>
        protected void VerifyItemHasPath(PSItem item)
        {
            VerifyItemHasPath(item, null);
        }

        protected void VerifyItemHasPath(PSItem item, int? documentID)
        {
            if (item == null)
                throw new ArgumentNullException("Argument 'item' must not be null.");

            if (item.Folders == null || item.Folders.Length == 0)
            {
                string message;
                if (documentID == null)
                    message = string.Format("Content item {0} has no path.", new PercussionGuid(item.id));
                else
                    message = string.Format("Content item {0} has no path, CDRID = {1}", new PercussionGuid(item.id), documentID);

                throw new FolderAssociationException(message);
            }
        }

        protected void UpdateDocumentURL(string targetURL, PercussionGuid summaryRootItemID,
            PercussionGuid[] summaryComponentIDList)
        {
            string newPath = GetTargetFolder(targetURL);
            PSItem[] keyItems = CMSController.LoadContentItems(new PercussionGuid[] { summaryRootItemID });
            string oldPath = CMSController.GetPathInSite(keyItems[0]);  // Root item.

            if (!newPath.Equals(oldPath, StringComparison.InvariantCultureIgnoreCase))
            {
                //Remove the summary from the landing page slot of the old NavOn
                DeleteNavOnRelationship(summaryRootItemID, oldPath);

                // Move the CancerInformationSummary and all its components
                CMSController.GuaranteeFolder(newPath, FolderManager.NavonAction.MakePublic);
                CMSController.MoveContentItemFolder(oldPath, newPath, CMSController.BuildGuidArray(summaryRootItemID, summaryComponentIDList));

            }
        }


        protected SectionToCmsIDMap BuildItemIDMap<SubDocumentType>(IList<SubDocumentType> subDocumentList,
            SubDocumentIDDelegate<SubDocumentType> IdValueAccessor,
            List<long> idList)
        {
            SectionToCmsIDMap itemIDMap = new SectionToCmsIDMap();

            int sectionCount = subDocumentList.Count();
            for (int i = 0; i < sectionCount; i++)
            {
                itemIDMap.AddSection(IdValueAccessor(subDocumentList[i]), idList[i]);
            }

            return itemIDMap;
        }     
        
        /// <summary>
        /// Creates active assembly relationships between a list of content items and
        /// the content items they refer to.
        /// </summary>
        /// <param name="pageIDList"></param>
        /// <param name="referencedItems"></param>
        /// <returns></returns>
        protected PSAaRelationship[] CreateExternalSummaryRelationships(long[] pageIDList,
            List<List<PercussionGuid>> referencedItems)
        {
            LogDetailedStep("Begin CreateExternalSummaryRelationships.");

            List<PSAaRelationship> relationshipList = new List<PSAaRelationship>();

            int itemCount = pageIDList.Length;
            for (int i = 0; i < itemCount; i++)
            {
                PercussionGuid[] guidList = referencedItems[i].ToArray();

                // If the list entry for a given page contains references to other
                // Cancer info summaries, then create AA relationships to them.
                if (guidList.Length > 0)
                {
                    PSAaRelationship[] relationships =
                        CMSController.CreateActiveAssemblyRelationships(pageIDList[i],
                            Array.ConvertAll(guidList, percGuid => (long)percGuid.ID),
                            SummaryRefSlot, PageSnippetTemplateName);
                    relationshipList.AddRange(relationships);
                }
            }

            LogDetailedStep("End CreateExternalSummaryRelationships.");

            return relationshipList.ToArray();
        }
       

        /// <summary>
        /// Verifies that the English version of a document exists before attempting
        /// to create an alternage language version.
        /// </summary>
        /// <param name="summary">Reference to a CancerInformationSummary object.</param>
        protected virtual void VerifyEnglishLanguageVersion(SummaryDocument summary)
        {
            // No need to validate if the document is in English.
            if (summary.Language == Language.Spanish)
            {
                // If this SummaryRelation did not exist, we would not know the document was in Spanish.
                SummaryRelation relationship = summary.RelationList.Find(relation => relation.RelationType == SummaryRelationType.SpanishTranslationOf);
                PercussionGuid englishItem = GetCdrDocumentID(CancerInfoSummaryContentType, relationship.RelatedSummaryID);
                if (englishItem == null)
                    throw new EnglishVersionNotFoundException(
                        string.Format("Document {0} must exist before its translation may be added.", relationship.RelatedSummaryID));
            }
        }

        /// <summary>
        /// Creates a Translation relationship between the Spanish and English versions of
        /// the CancerInformationSummary document.
        /// </summary>
        /// <param name="summary">Reference to a CancerInformationSummary object.</param>
        /// <param name="rootID">The CMSID for the root element of the CancerInformationSummary's
        /// composite content item.</param>
        protected virtual void LinkToAlternateLanguageVersion(SummaryDocument summary, PercussionGuid rootID)
        {
            // We only need to set up a translation relationship from Spanish to English,
            // not the other way around.  CDR business rules dicatate that the English version
            // must exist before the translation can be created.
            if (summary.Language == Language.Spanish)
            {
                // If this relationship did not exist, we would not know the document was in Spanish.
                SummaryRelation relationship = summary.RelationList.Find(relation => relation.RelationType == SummaryRelationType.SpanishTranslationOf);

                PercussionGuid englishItem = GetCdrDocumentID(CancerInfoSummaryContentType, relationship.RelatedSummaryID);
                if (englishItem == null)
                    throw new EnglishVersionNotFoundException(string.Format("Document {0} must exist before its translation may be added.", relationship.RelatedSummaryID));

                CMSController.CreateRelationship(englishItem.ID, rootID.ID, CMSController.TranslationRelationshipType);
            }
        }

        /// <summary>
        /// Deletes the content items representing the speicified Cancer Information Summary document.
        /// </summary>
        /// <param name="documentID">The document ID.</param>
        public override void DeleteContentItem(int documentID)
        {
            DeleteContentItem(documentID, null);
        }

        /// <summary>
        /// This method purges all content item and also the containg folder of the root content item.
        /// This id should be a root item id. This ensures all the content item related to \
        /// the summary will be deleted.
        /// </summary>
        /// <param name="itemGuid">The root item guid of the summary content item.</param>
        public virtual void DeleteContentItem(PercussionGuid itemGuid)
        {
            // A null rootItem means the document has already been deleted.
            // No further work is required.
            if (itemGuid != null)
            {
                // Get access to the PSFolderItem object for removing the folder and its content.
                PSItem[] rootItem = CMSController.LoadContentItems(new long[] { itemGuid.ID });

                // Get path from the first item of the the PSFolderItem of the rootItem folder collection.
                string folderPath = CMSController.GetPathInSite(rootItem[0]);

                // Purge all the contents inside in the folder and remove  the folder.
                CMSController.DeleteFolder(folderPath, true);
            }
        }

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
                PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(rootItem, SummaryPageSlot);
                PermanentLinkHelper PermanentLinkData = new PermanentLinkHelper(CMSController, sitePath);
                PercussionGuid[] permanentLinks = PermanentLinkData.DetectToDeletePermanentLinkRelationships();

                // Create a list of all content IDs making up the document.
                // It is important for verification that rootItem always be first.
                PercussionGuid[] fullIDList = CMSController.BuildGuidArray(rootItem, pageIDs);

                VerifyDocumentMayBeDeleted(fullIDList.ToArray(), permanentLinks);

                CMSController.DeleteItemList(fullIDList);
            }
        }

        /// <summary>
        /// Verifies that a document object has no incoming refernces. Throws CMSCannotDeleteException
        /// if the document is the target of any incoming relationships.
        /// </summary>
        /// <param name="documentCmsIdentifier">The document's ID in the CMS.</param>
        protected void VerifyDocumentMayBeDeleted(PercussionGuid[] summaryIdentifers, PercussionGuid[] permanentLinks)
        {
            // TODO: Modify this to call FindExternalRelationships().

            // The first item in the collection is always root.
            PercussionGuid rootItem = summaryIdentifers[0];

            // Inspect incoming relationships to Permanent Links
            PSAaRelationship[] incomingPermanentLinkRelationships = new PSAaRelationship[0];
            if (permanentLinks.Length > 0)
            {
                incomingPermanentLinkRelationships = FindExternalRelationships(permanentLinks);
            }
            if (incomingPermanentLinkRelationships.Length > 0)
            {
                // If there are any relationships to any Permanent Link that will be deleted, throw an error
                throw new Exception("A Permanent Link cannot be removed while something is linking to it.");
            }
        }

        /// <summary>
        /// Locates all PermanentLink content items within the root's folder.
        /// </summary>
        /// <param name="rootItemID">Needed to find the folder in which you're search for content.</param>
        /// <returns>The CMS identifers for the embedded content items.</returns>
        protected virtual PercussionGuid[] LocateExistingPermanentLinks(PercussionGuid rootItemID)
        {
            PSItem[] rootItem = CMSController.LoadContentItems(new long[] { rootItemID.ID });
            VerifyItemHasPath(rootItem[0]);
            string summaryPath = CMSController.GetPathInSite(rootItem[0]);
            PermanentLinkHelper PermanentLinkData = new PermanentLinkHelper(CMSController, summaryPath);
            return PermanentLinkData.GetOldGuids;
        }
              
        #endregion

        #region Public Members
        /// <summary>
        /// This method is identical to ProcessDocument except it takes in ref argument which
        /// contains the CMS content item identifier when the method returns. 
        /// </summary>
        /// <param name="documentObject">The sumamry document object</param>
        /// <param name="contentItemGuid">The percussionguid of the content item</param>
        public void ProcessDocument(Document documentObject, ref PercussionGuid contentItemGuid)
        {
            contentItemGuid = processDocumentInternal(documentObject, null);
        }

        public string GetLanguageCode(Language language)
        {
            string languageCode;

            switch (language)
            {
                case Language.Spanish:
                    languageCode = LanguageSpanish;
                    break;
                case Language.English:
                default:
                    languageCode = LanguageEnglish;
                    break;
            }

            return languageCode;
        }
        #endregion

        #region Protected Members
        /// <summary>
        /// And internal private method which the identity of the content item created or being updated.
        /// </summary>
        /// <param name="documentObject">The summary document instance</param>
        /// <returns>The percussion guid of the content item</returns>
        private PercussionGuid processDocumentInternal(Document documentObject, string sitePath)
        {
            VerifyRequiredDocumentType(documentObject, DocumentType.Summary);

            SummaryDocument document = documentObject as SummaryDocument;

            InformationWriter(string.Format("Begin Percussion processing for document CDRID = {0}.", document.DocumentID));

            // Are we updating an existing document? Or saving a new one?
            PercussionGuid identifier = GetCdrDocumentID(CancerInfoSummaryContentType, document.DocumentID);

            // No mapping found, this is a new item.
            if (identifier == null)
            {
                InformationWriter(string.Format("Create new content item for document CDRID = {0}.", document.DocumentID));
                identifier = CreateNewCancerInformationSummary(document, sitePath);
            }
            else
            {
                InformationWriter(string.Format("Update existing content item for document CDRID = {0}.", document.DocumentID));
                UpdateCancerInformationSummary(document, identifier, sitePath);
            }

            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));

            return identifier;
        }

        protected List<ContentItemForCreating> CreatePDQCancerInfoSummaryPage(SummaryDocument document, string creationPath)
        {
            // Content items may be re-created during an update, therefore we must pass the create path from the caller.

            List<ContentItemForCreating> contentItemList = new List<ContentItemForCreating>();

            ChildFieldSet subsectionList = null;

            for (int i = 0; i <= document.SectionList.Count - 1; i++)
            {
                if (document.SectionList[i].IsTopLevel == true)
                {
                    FieldSet mainFields = CreateFieldValueMapPDQCancerInfoSummaryPage(document, document.SectionList[i]);
                    subsectionList = new ChildFieldSet("contained_sections");
                    ContentItemForCreating contentItem = new ContentItemForCreating(CancerInfoSummaryPageContentType, mainFields, creationPath);
                    contentItem.ChildFieldList.Add(subsectionList);

                    contentItemList.Add(contentItem);
                }
                else
                {
                    // This is somewhat hacky, but it takes advantange of the fact that subsectionList is a reference
                    // and still refers to the object that was added to the current instance of ContentItemForCreating.
                    // It is not logically possible for a subsection to appear outside a top-level section.
                    FieldSet subsection = new FieldSet();
                    subsection.Add("section_id", document.SectionList[i].RawSectionID);
                    subsectionList.Fields.Add(subsection);
                }
            }

            return contentItemList;
        }

        protected FieldSet CreateFieldValueMapPDQCancerInfoSummaryPage(SummaryDocument summary, SummarySection summarySection)
        {
            FieldSet fields = new FieldSet();
            string html = summarySection.Html.OuterXml;

            fields.Add("bodyfield", html);

            fields.Add("table_of_contents", (string.IsNullOrEmpty(summarySection.TOC) ? null : summarySection.TOC));

            string longTitle = summarySection.Title;
            fields.Add("long_title", longTitle);

            int shortLength = Math.Min(ShortTitleLength, longTitle.Length);
            fields.Add("short_title", longTitle.Substring(0, shortLength));

            fields.Add("sys_title", EscapeSystemTitle(summarySection.Title));

            // HACK: This relies on Percussion not setting anything else in the login session.
            fields.Add("sys_lang", GetLanguageCode(summary.Language));
            fields.Add("top_sectionid", summarySection.RawSectionID);

            return fields;
        }

        protected List<ContentItemForCreating> CreatePDQCancerInfoSummary(SummaryDocument document, string creationPath)
        {
            // Content items may be re-created during an update, therefore we must pass the create path from the caller.

            List<ContentItemForCreating> contentItemList = new List<ContentItemForCreating>();

            ContentItemForCreating contentItem = new ContentItemForCreating(CancerInfoSummaryContentType, CreateFieldValueMapPDQCancerInfoSummary(document), creationPath);
            //contentItemList.Add(contentItem);

            ChildFieldSet subsectionList = null;

            for (int i = 0; i <= document.SectionList.Count - 1; i++)
            {
                if (document.SectionList[i].IsTopLevel == true)
                {
                    subsectionList = new ChildFieldSet("contained_sections");
                    contentItem.ChildFieldList.Add(subsectionList);

                    //add row per section that has a device
                    foreach (SummarySectionDeviceType device in document.SectionList[i].IncludedDeviceTypes)
                    {
                        FieldSet subsection = new FieldSet();
                        subsection.Add("section_id", document.SectionList[i].RawSectionID);
                        string html = document.SectionList[i].Html.OuterXml;

                        subsection.Add("bodyfield", html);

                        subsection.Add("section_title", document.SectionList[i].Title);

                        subsection.Add("display_device", device.ToString());

                        subsectionList.Fields.Add(subsection);

                    }

                }

            }
            contentItemList.Add(contentItem);
            return contentItemList;
        }

        protected ContentItemForUpdating UpdateSummaryChildTables(SummaryDocument document, ContentItemForUpdating contentItem)
        {
            //delete the child fields before adding new ones
            CMSController.DeleteChildItems(new PercussionGuid(contentItem.ID));

            //add new child fields
            ChildFieldSet subsectionList = null;

            for (int i = 0; i <= document.SectionList.Count - 1; i++)
            {
                if (document.SectionList[i].IsTopLevel == true)
                {
                    subsectionList = new ChildFieldSet("contained_sections");
                    contentItem.ChildFieldList.Add(subsectionList);

                    //add one row per top level section that has a device
                    if (document.SectionList[i].IncludedDeviceTypes.Count > 0)
                    {
                        foreach (SummarySectionDeviceType device in document.SectionList[i].IncludedDeviceTypes)
                        {
                            FieldSet subsection = new FieldSet();
                            subsection.Add("section_id", document.SectionList[i].RawSectionID);
                            string html = document.SectionList[i].Html.OuterXml;

                            subsection.Add("bodyfield", html);

                            subsection.Add("section_title", document.SectionList[i].Title);

                            subsection.Add("display_device", device.ToString());

                            subsectionList.Fields.Add(subsection);

                        }
                    }
                    else
                    {
                        FieldSet subsection = new FieldSet();
                        subsection.Add("section_id", document.SectionList[i].RawSectionID);
                        string html = document.SectionList[i].Html.OuterXml;

                        subsection.Add("bodyfield", html);

                        subsection.Add("section_title", document.SectionList[i].Title);

                        //top-level section visible on all devices
                        subsection.Add("display_device", SummarySectionDeviceType.all.ToString());

                        subsectionList.Fields.Add(subsection);
                    }
                }

            }
            return contentItem;
        }

        protected FieldSet CreateFieldValueMapPDQCancerInfoSummary(SummaryDocument summary)
        {
            FieldSet fields = new FieldSet();
            string prettyURLName = GetSummaryPrettyUrlName(summary.BasePrettyURL);

            // Explicitly set pretty_url_name to null so the CI Summary will be
            // the folder's default document.
            fields.Add("pretty_url_name", null);

            fields.Add("long_title", summary.Title);

            //OCEPROJECT - 1147
            //Update and save the short title field as opposed to truncating the long title
            fields.Add("short_title", summary.ShortTitle);

            fields.Add("long_description", summary.Description);
            fields.Add("short_description", string.Empty);
            fields.Add("date_next_review", "1/1/2100");

            if (summary.LastModifiedDate == DateTime.MinValue)
                fields.Add("date_last_modified", null);
            else
                fields.Add("date_last_modified", summary.LastModifiedDate.ToString());

            if (summary.FirstPublishedDate == DateTime.MinValue)
            {
                fields.Add("date_first_published", null);
            }
            else
            {
                fields.Add("date_first_published", summary.FirstPublishedDate.ToString());
            }

            fields.Add("date_display_mode", "2");

            fields.Add("cdrid", summary.DocumentID.ToString());
            fields.Add("summary_type", summary.Type);

            // Guaranteed by CDR to be (exact text) either "Patients" or "Health professionals".
            fields.Add("audience", summary.AudienceType);          

            fields.Add("sys_title", EscapeSystemTitle(summary.Title));

            //OCE Project 199 - if the keywords exist save them to the meta keywords field in Percussion as a comma separated list 
            if (summary.SummaryKeyWords != null && summary.SummaryKeyWords.Count > 0)
            {               
                string keywordList = string.Join(",", summary.SummaryKeyWords.ToArray());
                //OCEPROJECT-3696 - The meta keywords field needs to be truncated to 255 characters
                //The meta keywords character limit is 255 in Percussion. 
                //The system needs to truncate the keywords coming in from the CDR to conform to that limit to avoid errrors.
                if (keywordList.Length > 255)
                {
                    //truncate the string to 255
                    keywordList = keywordList.Substring(0, MetaKeywordsLength);
                    //truncate to the nearest whole word
                    keywordList = keywordList.Substring(0, keywordList.LastIndexOf(","));
                }

                fields.Add("meta_keywords", keywordList);

            }

            // HACK: This relies on Percussion not setting anything else in the login session.
            fields.Add("sys_lang", GetLanguageCode(summary.Language));

            return fields;
        }

        protected FieldSet CreateFieldValueMapNavOn(SummaryDocument summary)
        {
            FieldSet fields = new FieldSet();

            fields.Add("nav_title", summary.NavLabel);

            fields.Add("sys_lang", GetLanguageCode(summary.Language));

            return fields;
        }

        /// <summary>
        /// Updates the Navon which resides in the same folder as the summary root node.
        /// </summary>
        /// <param name="document">SummaryDocument object containing information about the
        /// summary being processed.</param>
        /// <param name="summaryRootItemID">Guid of document's Percussion content item.</param>
        /// <param name="path">The folder path containing the navon to be updated.</param>
        protected virtual void UpdateNavOn(SummaryDocument document, PercussionGuid summaryRootItemID, string path)
        {

            List<ContentItemForUpdating> contentItemList = new List<ContentItemForUpdating>();

            PercussionGuid[] searchList =
                    CMSController.SearchForContentItems(NavonType, path, null);

            //create a new instance of the CMSController with the CancerGov community
            CMSController CMSControllerForNavon = new CMSController(NavonCommunity);

            if (searchList != null && searchList.Length > 0)
            {
                //Move to Editing
                TransitionNavonToEditing(CMSControllerForNavon, searchList);

                ContentItemForUpdating contentItem = new ContentItemForUpdating(searchList[0].ID, CreateFieldValueMapNavOn(document));
                contentItemList.Add(contentItem);
                //update the Nav Label field on the nav on
                CMSControllerForNavon.UpdateContentItemList(contentItemList);

                //find the relationship between the summary and the navon
                PSAaRelationship[] incomingRelationship = CMSControllerForNavon.FindIncomingActiveAssemblyRelationships(new PercussionGuid[] { summaryRootItemID }, NavOnLandingPageSlot, NavOnSnippetTemplate);
                if (incomingRelationship != null && incomingRelationship.Length <= 0)
                {
                    //add the summary to the Nav Landing Page slot
                    //add the relationship only if it does not exist
                    CMSControllerForNavon.CreateActiveAssemblyRelationships(searchList[0].ID, new long[] { summaryRootItemID.ID }, NavOnLandingPageSlot, NavOnSnippetTemplate);
                }

                //Move to Public
                TransitionNavonToPublic(CMSControllerForNavon, searchList);

            }
        }

        //deletes the relationship of the summary with the old nav on 
        protected void DeleteNavOnRelationship(PercussionGuid summaryRootItemID, string path)
        {
            PercussionGuid[] searchList =
                   CMSController.SearchForContentItems(NavonType, path, null);

            //create a new instance of the CMSController with the CancerGov community
            CMSController CMSControllerForNavon = new CMSController(NavonCommunity);

            if (searchList != null && searchList.Length > 0)
            {
                //Move to Editing
                TransitionNavonToEditing(CMSControllerForNavon, searchList);

                //find the relationship between the summary and the navon
                PSAaRelationship[] incomingRelationship = CMSControllerForNavon.FindIncomingActiveAssemblyRelationships(new PercussionGuid[] { summaryRootItemID }, NavOnLandingPageSlot, NavOnSnippetTemplate);
                if (incomingRelationship != null && incomingRelationship.Length > 0)
                {
                    //delete the relationship before adding it
                    CMSControllerForNavon.DeleteActiveAssemblyRelationships(incomingRelationship, false, new PercussionGuid[] { summaryRootItemID }, NavOnLandingPageSlot, NavOnSnippetTemplate);
                }

                //Move to Public
                TransitionNavonToPublic(CMSControllerForNavon, searchList);
            }

        }
              
        /// <summary>
        /// Receives a Cancer Information Summary's pretty URL and converts it into
        /// a path relative to the base of the site folder structure, ommitting only
        /// the //Sites/sitename portion.  If the URL begins with a protocol and host,
        /// they are removed.
        /// </summary>
        /// <param name="prettyUrl">The Cancer Information Summary's online URL.</param>
        /// <returns>Path relative to the //Sites/sitename folder.</returns>
        protected string GetTargetFolder(string prettyUrl)
        {
            string targetUrl;

            // If present, remove the host name and protocol
            // TODO: Refactor to allow protocols other than http.
            if (prettyUrl.ToLower().StartsWith("http"))
            {
                //Remove hostname and protocol.
                System.Uri URL = new Uri(prettyUrl);
                targetUrl = URL.AbsolutePath;
            }
            else
            {
                targetUrl = prettyUrl;
            }

            // Trim any trailing slash for consistency.
            if (targetUrl.EndsWith("/") && targetUrl.Length > 1)
                targetUrl = targetUrl.Substring(0, targetUrl.Length - 1);

            return targetUrl;
        }

        /// <summary>
        /// Receives a Cancer Information Summary's pretty URL and converts it to
        /// the document's parent folder.  If the folder specified has no parent,
        /// the root folder is returned.
        /// </summary>
        /// <param name="prettyUrl">The Cancer Information Summary's online URL.</param>
        /// <returns>Path relative to the //Sites/sitename folder.</returns>
        protected string GetParentFolder(string prettyUrl)
        {
            const string separator = "/";
            string folder = GetTargetFolder(prettyUrl);

            // GetTargetFolder() handles any trailing slash for us.
            // Default to the folder value in order to handle path of /.
            string parent = folder;

            // More than a single separator means there's something to work with.
            if (!folder.Equals(separator))
            {
                int index = folder.LastIndexOf(separator);
                if (index > 0)
                    parent = folder.Substring(0, index);
            }

            return parent;
        }

        protected string GetSummaryPrettyUrlName(string prettyUrl)
        {
            string prettyURLName = prettyUrl.Substring(prettyUrl.LastIndexOf('/') + 1);
            return prettyURLName;
        }

        protected string SummarySectionIDAccessor(SummarySection section)
        {
            return section.RawSectionID;
        }

        #endregion

        protected void LogDetailedStep(string message)
        {
#if DEBUG
            InformationWriter(message);
#endif
        }
    }
}