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

        protected const string SummaryLinkSnippetTemplate = "pdqSnCancerInformationSummaryItemLink";

        protected const string PatientVersionLinkSlot = "pdqCancerInformationSummaryPatient";
        protected const string HealthProfVersionLinkSlot = "pdqCancerInformationSummaryHealthProf";
        protected const string SummaryRefSlot = "pdqCancerInformationSummaryRef";


        protected const string AudienceLinkSnippetTemplate = "pdqSnCancerInformationSummaryItemLink";
        protected const string AudienceTabSnippetTemplate = "pdqSnCancerInformationSummaryItemAudienceTab";

        protected const string SummarySectionSnippetTemplate = "pdqSnCancerInformationSummaryPage";
        protected const string MobileSummarySectionSnippetTemplate = "pdqMSnCancerInformationSummaryPage";

        protected const string TableSectionSnippetTemplate = "pdqSnTableSection";

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
            PercussionGuid summaryLinkID = LocateExistingSummaryLink(summaryRootID);

            // If Permanent Links exist, we need to worry about performing transitions
            // this will return null for mobile and then potentially populated with values on the desktop
            // as PermanentLink content items should not exist on Mobile
            PercussionGuid[] permanentLinkIDs = LocateExistingPermanentLinks(summaryRootID);

            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(summaryRootID, SummaryPageSlot);
            
            PerformTransition(TransitionItemsToPreview, summaryRootID, summaryLinkID, permanentLinkIDs, pageIDs);
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
            PercussionGuid summaryLinkID = LocateExistingSummaryLink(summaryRootID);

            // If Permanent Links exist, we need to worry about performing transitions
            // this will return null for mobile and then potentially populated with values on the desktop
            // as PermanentLink content items should not exist on Mobile
            PercussionGuid[] permanentLinkIDs = LocateExistingPermanentLinks(summaryRootID);

            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(summaryRootID, SummaryPageSlot);
            
            PerformTransition(TransitionItemsToLive, summaryRootID, summaryLinkID, permanentLinkIDs, pageIDs);
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
            PercussionGuid summaryLinkID = LocateExistingSummaryLink(summaryRootID);

            // If Permanent Links exist, we need to worry about performing transitions
            // this will return null for mobile and then potentially populated with values on the desktop
            // as PermanentLink content items should not exist on Mobile
            PercussionGuid[] permanentLinkIDs = LocateExistingPermanentLinks(summaryRootID);

            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(summaryRootID, SummaryPageSlot);
            
            PerformTransition(TransitionItemsToLiveFast, summaryRootID, summaryLinkID, permanentLinkIDs, pageIDs);
                        
        }

        protected void PerformTransition(ItemTransitionDelegate transitionMethod,
            PercussionGuid summaryRootID,
            PercussionGuid summaryLinkID,
            PercussionGuid[] permanentLinkIDs,
            PercussionGuid[] pageIDs)
        {
            LogDetailedStep("Begin workflow transition.");

            // Root item and Link must move independently of each other and everything else.
            if (summaryRootID != null) transitionMethod(new PercussionGuid[] { summaryRootID });
            if (summaryLinkID != null) transitionMethod(new PercussionGuid[] { summaryLinkID });
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

            // Retrieve IDs for the summary's components.
            PercussionGuid summaryLinkID;
            try
            {
                summaryLinkID = LocateExistingSummaryLink(summaryRootID);
                string slotName = "";
                slotName = summary.AudienceType == PatientAudience ? slotName = PatientVersionLinkSlot : slotName = HealthProfVersionLinkSlot;
                PercussionGuid[] foundIDs = CMSController.SearchForContentItemsByDependent(summaryRootID, slotName, SummaryLinkSnippetTemplate);
                if (foundIDs != null && foundIDs.Length > 0)
                {
                    //there should be just one SummaryLinkItem that will be returned
                    summaryLinkID = foundIDs[0];
                }
            }
            catch (FolderAssociationException ex)
            {
                throw new FolderAssociationException(string.Format("Content item {0} has no path, CDRID = {1}.",
                    summaryRootID, summary.DocumentID), ex);
            }

            // Lookup desktop pages
            PercussionGuid[] desktopPageIDs = CMSController.SearchForItemsInSlot(summaryRootID, StandardPageSlotName);
            
            // Lookup mobile pages
            PercussionGuid[] mobilePageIDs = CMSController.SearchForItemsInSlot(summaryRootID, MobilePageSlotName);
            
            LogDetailedStep("End gathering content items.");

            LogDetailedStep("Begin gathering incoming relationships.");

            // Determine which pages in this summary are dependent items in other summaries.
            PSAaRelationship[] desktopPageRelationships = FindIncomingPageRelationships(summaryRootID, summaryLinkID, desktopPageIDs);
            PSAaRelationship[] mobilePageRelationships = FindIncomingPageRelationships(summaryRootID, summaryLinkID, mobilePageIDs);

            LogDetailedStep("End gathering incoming relationships.");

            List<PSAaRelationship> allPageRelationships = new List<PSAaRelationship>();
            allPageRelationships.AddRange(desktopPageRelationships);
            allPageRelationships.AddRange(mobilePageRelationships);

            LogDetailedStep("Begin Permanent Link evaluation.");
            PSItem[] summaryRootItem = CMSController.LoadContentItems(new PercussionGuid[] { summaryRootID });
            //get the existing item path for the Permanent Link Helper 
            //this will ensure no errors are thrown if the URL changes when updating summaries
            string existingItemPath = CMSController.GetPathInSite(summaryRootItem[0]);
            PermanentLinkHelper PermanentLinkData = new PermanentLinkHelper(CMSController, summary.PermanentLinkList, existingItemPath);
            //PercussionGuid[] toDeletePermanentLinkGuids = PermanetLinkData.DetectToDeletePermanentLinkRelationships();
            PercussionGuid[] permanentLinkGuids = PermanentLinkData.GetOldGuids;
            PSAaRelationship[] incomingPermanentLinkRelationships = new PSAaRelationship[0];
            if (permanentLinkGuids.Length > 0)
            {
                incomingPermanentLinkRelationships = FindExternalRelationships(permanentLinkGuids);
            }
            LogDetailedStep("End Permanent Link evaluation.");

            // Is the summary in a state suitable for updating?
            VerifyDocumentMayBeUpdated(summary, summaryRootID, summaryLinkID, PermanentLinkData, incomingPermanentLinkRelationships, allPageRelationships.ToArray());

            PerformUpdate(summary, summaryRootID, summaryLinkID, PermanentLinkData,
                desktopPageIDs, desktopPageRelationships,
                mobilePageIDs, mobilePageRelationships,
                sitePath);
        }

        /// <summary>
        /// Device-specific logic for summary updates.
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="summaryRootID"></param>
        /// <param name="sitePath"></param>
        protected abstract void PerformUpdate(SummaryDocument summary, PercussionGuid summaryRootID,
            PercussionGuid summaryLinkID, PermanentLinkHelper permanentLinkData,
            PercussionGuid[] desktopPageIDs, PSAaRelationship[] incomingDesktopPageRelationships,
            PercussionGuid[] mobilePageIDs, PSAaRelationship[] incomingMobilePageRelationships,
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
        protected void VerifyDocumentMayBeUpdated(SummaryDocument summary, PercussionGuid summaryRoot, PercussionGuid summaryLink, PermanentLinkHelper PermanentLinkData, PSAaRelationship[] incomingPermanentLinkRelationships, PSAaRelationship[] incomingPageRelationships)
        {
            List<string> errorList = new List<string>();

            // Rollup list of link targets in this summary.
            HashSet<string> sectionIDList = new HashSet<string>();
            foreach (SummarySection section in summary.SectionList)
            {
                if (!sectionIDList.Contains(section.RawSectionID))
                    sectionIDList.Add(section.RawSectionID);
                section.LinkableNodeRawIDList.ForEach(sectionID =>
                {
                    if (!sectionIDList.Contains(sectionID))
                        sectionIDList.Add(sectionID);
                });
            }


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


            // Inspect items owning relationships to the pages.
            foreach (PSAaRelationship relationship in incomingPageRelationships)
            {
                PercussionGuid parentID = new PercussionGuid(relationship.ownerId);
                PercussionGuid dependentID = new PercussionGuid(relationship.dependentId);

                PSItem parentItem = itemStore.LoadContentItem(parentID);

                // Check for links from non-PDQ content types.
                if (!SummaryContentTypes.Contains(parentItem.contentType))
                {
                    errorList.Add(CISErrorBuilder.BuildNonPDQReferenceMessage(dependentID, summary.DocumentID, parentItem));
                    continue;
                }

                string sourceBodyField = String.Empty;

                // Table section use Inline Table & FullSizeTable
                if (parentItem.contentType == TableSectionContentType)
                {
                    // Load the content of the page containing the link.
                    sourceBodyField += "<tableSection>";
                    sourceBodyField += PSItemUtils.GetFieldValue(parentItem, "inline_table");
                    sourceBodyField += PSItemUtils.GetFieldValue(parentItem, "fullsize_table");
                    sourceBodyField += "</tableSection>";
                }
                else
                {
                    // Load the body of the page containing the link.
                    sourceBodyField = PSItemUtils.GetFieldValue(parentItem, "bodyfield");
                }
                XmlDocument body = new XmlDocument();
                body.LoadXml(sourceBodyField);

                // Find all summary references contained in the source document.
                XmlNodeList nodeList = body.SelectNodes("//a[@inlinetype='SummaryRef']");
                foreach (XmlNode node in nodeList)
                {
                    // Find the section number
                    XmlAttributeCollection attributeList = node.Attributes;
                    XmlAttribute referenceAttribute = attributeList["objectid"];

                    // Create a dummy reference.  (We're not overly concerned with path for this test.)
                    SummaryReference reference = new SummaryReference(referenceAttribute.Value, "/");

                    // If the reference doesn't refer to this document,
                    // OR, if the reference isn't to a particular section number,
                    // then skip it.
                    if (reference.CdrID != summary.DocumentID
                        || !reference.IsSectionReference)
                        continue;

                    if (!sectionIDList.Contains(reference.SectionID))
                    {
                        errorList.Add(CISErrorBuilder.BuildMissingReferenceMessage(summary.DocumentID, reference.SectionID, parentItem));
                        continue;
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

        /// <summary>
        /// Modifies summary references from other Cancer Info Summaries to link to the document's
        /// new pages instead of the old ones.
        /// </summary>
        /// <param name="targetCDRID">The CDR ID of the document currently being processed.  IOW,
        /// the one being referred to (not the one to be updated).</param>
        /// <param name="summaryRootID">PercussionGuid of the composite document's root CancerInfoSummary content item.</param>
        /// <param name="summaryLinkID">PercussionGuid of the composite document's root CancerInfoSummaryLink content item.</param>
        /// <param name="oldPageIDs">Array of PercussionGuids for the old pages.</param>
        /// <param name="newPageIDs">Array of PercussionGuids for the new pages.</param>
        /// <param name="incomingRelationships">Array of Percussion Relationship structures for items linking
        /// to the summary which is being updated.</param>
        protected void UpdateIncomingSummaryReferences(int targetCDRID,
            PercussionGuid summaryRootID,
            PercussionGuid summaryLinkID,
            PercussionGuid[] oldPageIDs,
            PercussionGuid[] newPageIDs,
            PSAaRelationship[] incomingRelationships,
            CancerInfoSummarySectionFinder finder)
        {
            if (incomingRelationships.Length > 0)
            {
                LogDetailedStep("Begin UpdateIncomingSummaryReferences.");

                ItemCache itemStore = new ItemCache(CMSController);
                PercussionGuid[] combinedList = CMSController.BuildGuidArray(oldPageIDs, newPageIDs, summaryRootID);

                // Check out all relationship owners. We know they will be modified.
                PercussionGuid[] itemsToCheckout = // Filter out duplicate item IDs.
                    (from idValue in
                         (from relationship in incomingRelationships select relationship.ownerId).Distinct()
                     select new PercussionGuid(idValue)).ToArray();

                PSItemStatus[] checkedOutPageStatus = CMSController.CheckOutForEditing(itemsToCheckout);

                List<KeyValuePair<PercussionGuid, PercussionGuid>> relationshipPairs
                    = new List<KeyValuePair<PercussionGuid, PercussionGuid>>();

                try
                {

                    itemStore.Preload(combinedList);
                    PSItem rootItem = itemStore.LoadContentItem(summaryRootID);
                    string basePath = CMSController.GetPathInSite(rootItem);

                    foreach (PSAaRelationship individual in incomingRelationships)
                    {
                        PercussionGuid sourceID = new PercussionGuid(individual.ownerId);
                        PercussionGuid oldTargetID = new PercussionGuid(individual.dependentId);
                        PSItem oldTargetItem = itemStore.LoadContentItem(oldTargetID);
                        PSItem sourceItem = itemStore.LoadContentItem(sourceID);

                        string sourceBodyField = String.Empty;

                        // Table section use Inline Table & FullSizeTable
                        if (sourceItem.contentType == TableSectionContentType)
                        {
                            // Load the content of the page containing the link.
                            sourceBodyField += "<tableSection>";
                            sourceBodyField += PSItemUtils.GetFieldValue(sourceItem, "inline_table");
                            sourceBodyField += PSItemUtils.GetFieldValue(sourceItem, "fullsize_table");
                            sourceBodyField += "</tableSection>";
                        }
                        else
                        {
                            // Load the body of the page containing the link.
                            sourceBodyField = PSItemUtils.GetFieldValue(sourceItem, "bodyfield");
                        }

                        XmlDocument body = new XmlDocument();
                        body.LoadXml(sourceBodyField);

                        // Find all summary references contained in the source document.
                        XmlNodeList nodeList = body.SelectNodes("//a[@inlinetype='SummaryRef']");
                        foreach (XmlNode node in nodeList)
                        {
                            // Find the section number
                            XmlAttributeCollection attributeList = node.Attributes;
                            XmlAttribute referenceAttribute = attributeList["objectid"];

                            SummaryReference reference = new SummaryReference(referenceAttribute.Value, basePath);

                            // If the reference doesn't refer to this document, then skip it.
                            if (reference.CdrID != targetCDRID)
                                continue;

                            // Link is to a specific document fragment.
                            if (reference.IsSectionReference)
                            {
                                // Find the page number in the new list of sections.
                                SummaryPageInfo referencedPage = finder.FindPageContainingSection(newPageIDs, reference.SectionID);

                                // Rebuild the link.
                                string url = BuildSummaryRefUrl(referencedPage.BasePath, referencedPage.PageNumber, reference.SectionID);
                                XmlAttribute href = attributeList["href"];
                                href.Value = url;

                                // Add the item to the list of references.
                                relationshipPairs.Add(new KeyValuePair<PercussionGuid, PercussionGuid>(sourceID, referencedPage.ContentItemID));
                            }
                            else
                            {
                                // Links to the summary without a fragment. (page 1)

                                // Find the page number in the new list of sections.
                                SummaryPageInfo referencedPage = finder.FindFirstPage(newPageIDs, CMSController.SiteRootPath);

                                // Add the item to the list of references.
                                relationshipPairs.Add(new KeyValuePair<PercussionGuid, PercussionGuid>(sourceID, referencedPage.ContentItemID));

                                // Rebuild the link. (URL may have changed)
                                XmlAttribute href = attributeList["href"];
                                href.Value = reference.Url;
                            }
                        }

                        // Save the updated HTML.
                        CMSController.SaveContentItems(new PSItem[] { sourceItem });
                        itemStore.RemoveContentItem(sourceID); // Delete cached copy
                    }
                }
                finally
                {
                    CMSController.ReleaseFromEditing(checkedOutPageStatus.ToArray());
                }

                // At this point, we've created one relationship pair for each reference to a section. Some
                // Summary pages may contain references to multiple sections contained in a single page,
                // so we need to filter out the duplicates.
                List<KeyValuePair<PercussionGuid, PercussionGuid>> filteredPairs = new List<KeyValuePair<PercussionGuid, PercussionGuid>>();
                filteredPairs.AddRange(relationshipPairs.Distinct());

                // Create the new relationships.  (Saved for last because CreateActiveAssemblyRelationships
                // will Lock/Release the involved content items and that would conflict with the locks
                // needed for the updates.
                filteredPairs.ForEach(
                    kvp =>
                    {
                        CMSController.CreateActiveAssemblyRelationships(kvp.Key,
                            new PercussionGuid[] { kvp.Value }, SummaryRefSlot, PageSnippetTemplateName);
                    });

                LogDetailedStep("End UpdateIncomingSummaryReferences.");
            }
        }


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
            PercussionGuid[] alternateAudiences = LocateAlternateAudienceVersions(rootItem);

            /// Find all the incoming relationships.  (Need to include inline links!)
            PSAaRelationship[] incomingRelationship =
                CMSController.FindIncomingActiveAssemblyRelationships(internalIdentifers);

            List<PSAaRelationship> candidateRelationships = new List<PSAaRelationship>(incomingRelationship);

            /// Filter out any incoming relationships we can identify.
            candidateRelationships.RemoveAll(relationship =>
            {
                PercussionGuid ownerID = new PercussionGuid(relationship.ownerId);
                return internalIdentifers.Contains(ownerID) // owner is part of the document.
                    || alternateAudiences.Contains(ownerID);// owner is alternate audience version.
            });

            // Any remaining relationships are external
            return candidateRelationships.ToArray();
        }

        protected PSAaRelationship[] FindIncomingPageRelationships(PercussionGuid summaryRootID, PercussionGuid summaryLinkID,
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
                return ownerID == summaryRootID     // Filter out links from root and summary link
                    || ownerID == summaryLinkID
                    || dependent == summaryRootID  // Filter out relationships which own the root or link.
                    || dependent == summaryLinkID;
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

        protected virtual void UpdateDocumentURL(string targetURL, PercussionGuid summaryRootItemID,
            PercussionGuid summaryLinkItemID, PercussionGuid[] summaryComponentIDList)
        {
            string newPath = GetTargetFolder(targetURL);
            PSItem[] keyItems = CMSController.LoadContentItems(new PercussionGuid[] { summaryRootItemID, summaryLinkItemID });
            string oldPath = CMSController.GetPathInSite(keyItems[0]);  // Root item.

            if (!newPath.Equals(oldPath, StringComparison.InvariantCultureIgnoreCase))
            {
                // Move the CancerInformationSummary and all its components. The link item is moved separately.
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
        /// Replaces SummaryRef placeholder tags with links to the individual summary sections.
        /// The added links will point to the pretty URL of the top-level section, and optionally
        /// end with a document fragment identifier.
        /// </summary>
        /// <param name="summary">A list of objects corresponding 1:1 to the collection of top-level sections.
        /// Each object in the list is a (possibly empty) collection of PercussionGuid objects refererenced by
        /// the corresponding top-leavel section.</param>
        /// <param name="sectionList">A collection of sections within the sumary to scan for references.</param>
        /// <returns></returns>
        protected List<List<PercussionGuid>> ResolveSectionSummaryReferences(SummaryDocument summary,
            IEnumerable<SummarySection> sectionList,
            CancerInfoSummarySectionFinder finder)
        {
            List<List<PercussionGuid>> listOfLists = new List<List<PercussionGuid>>();

            LogDetailedStep("Begin ResolveSectionSummaryReferences.");

            foreach (SummarySection section in sectionList)
            {
                // Every section has a unique list of referenced items.  If no items are referenced,
                // the list is empty, and this is OK.
                List<PercussionGuid> referencedContentItems = new List<PercussionGuid>();
                listOfLists.Add(referencedContentItems);

                // HACK: Look for summary references in both the main section HTML, and in the 
                // (for tables only) standalone section.
                foreach (XmlDocument html in new XmlDocument[] { section.Html, section.StandaloneHTML })
                {
                    if (html != null)  // Only table sections will have a StandaloneHTML
                    {
                        XmlNodeList nodeList = html.SelectNodes("//a[@inlinetype='SummaryRef']");

                        foreach (XmlNode node in nodeList)
                        {
                            XmlAttributeCollection attributeList = node.Attributes;

                            XmlAttribute reference = attributeList["objectid"];
                            XmlAttribute attrib;

                            if (summary.SummaryReferenceMap.ContainsKey(reference.Value))
                            {
                                SummaryReference details = summary.SummaryReferenceMap[reference.Value];
                                PercussionGuid referencedItemRootID = GetCdrDocumentID(CancerInfoSummaryContentType, details.CdrID);

                                if (referencedItemRootID == null)
                                {
                                    WarningWriter(string.Format("Unable to find Summary document with CDRID={0}.", details.CdrID));
                                    continue;
                                }

                                // Self-reference.
                                if (summary.DocumentID == details.CdrID)
                                {
                                    // There's no need to create a reference list entry.  Those are only used for
                                    // detecting and updating external links.  An internal link isn't a problem
                                    // for those purposes.
                                    int pageNumber = finder.FindInternalPageContainingSection(summary, details.SectionID);
                                    string url = BuildInternalSummaryRefURL(summary, pageNumber, details.SectionID);
                                    attrib = html.CreateAttribute("href");
                                    attrib.Value = url;
                                    attributeList.Append(attrib);
                                }
                                else if (details.IsSectionReference)
                                {
                                    // Link to an external summary with a document fragment.

                                    SummaryPageInfo referencedPage = finder.FindPageContainingSection(referencedItemRootID, details.SectionID);

                                    if (referencedPage.ContentItemID == null)
                                    {
                                        string message =
                                            string.Format("Unable to resolve section {0} in document with CDRID={1}.",
                                            details.SectionID, details.CdrID);
                                        WarningWriter(message);
                                        continue;
                                    }

                                    // Add the item to the list.
                                    referencedContentItems.Add(referencedPage.ContentItemID);

                                    // Build the link.
                                    string url = referencedPage.GetReferenceUrl(details.SectionID);
                                    attrib = html.CreateAttribute("href");
                                    attrib.Value = url;
                                    attributeList.Append(attrib);
                                }
                                else
                                {
                                    // Link to a summary without a fragment.

                                    // Add the item to the list.
                                    SummaryPageInfo referencedPage = finder.FindFirstPage(referencedItemRootID);

                                    if (referencedPage == null)
                                    {
                                        WarningWriter(string.Format("Unable to find Summary document with CDRID={0}.", details.CdrID));
                                        continue;
                                    }


                                    referencedContentItems.Add(referencedPage.ContentItemID);

                                    // Build the link.
                                    attrib = html.CreateAttribute("href");
                                    attrib.Value = referencedPage.GetReferenceUrl(null);
                                    attributeList.Append(attrib);
                                }
                            }
                        }
                    }
                }
            }

            LogDetailedStep("End ResolveSectionSummaryReferences.");

            return listOfLists;
        }

        /// <summary>
        /// Builds the URL to resolve an external summary reference.
        /// </summary>
        /// <param name="itemFolder">The base URL.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="sectionID">The section ID.</param>
        /// <returns></returns>
        protected string BuildSummaryRefUrl(string baseUrl, int pageNumber, string sectionID)
        {
            string url;

            // Page numbers are natural numbers (1, 2, 3...), not zero-based.
            if (pageNumber > 0)
            {
                if (string.IsNullOrEmpty(sectionID))
                    url = string.Format("{0}/Page{1}", baseUrl, pageNumber);
                else
                    //removed the word section from the url
                    //as sections are represented using their ids
                    url = string.Format("{0}/Page{1}#{2}", baseUrl, pageNumber, sectionID);
            }
            else
            {
                url = BuildSummaryRefUrl(baseUrl, sectionID);
            }

            return url;
        }

        /// <summary>
        /// Builds the URL to a summary without using page number.
        /// (Link to page 1, or within a mobile summary.)
        /// </summary>
        /// <param name="itemFolder">The base URL.</param>
        /// <param name="sectionID">The section ID.</param>
        /// <returns></returns>
        protected string BuildSummaryRefUrl(string baseUrl, string sectionID)
        {
            string url;

            if (string.IsNullOrEmpty(sectionID))
                url = string.Format("{0}", baseUrl);
            else
                //removed the word section from the url
                //as sections are represented using their ids
                url = string.Format("{0}/#{1}", baseUrl, sectionID);

            return url;
        }

        /// <summary>
        /// Builds the URL to resolve a reference between two summaries.
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="sectionID">The section ID.</param>
        /// <returns></returns>
        protected abstract string BuildSummaryRefUrl(SummaryDocument summary, int pageNumber, string sectionID);

        /// <summary>
        /// Build the URL to resolve a reference between two sections of the same summary
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="sectionID">The section ID.</param>
        /// <returns></returns>
        protected abstract string BuildInternalSummaryRefURL(SummaryDocument summary, int pageNumber, string sectionID);

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
        /// Creates a relationship between the specified Cancer Information Summary document 
        /// and its alternate audience version, if one exists.
        /// </summary>
        /// <param name="documentID">Identifier for the content item to connect with its alternate version.</param>
        /// <param name="audienceType">Audience type value for the current content item.</param>
        protected void LinkRootItemToAlternateAudienceVersion(PercussionGuid documentID, string audienceType)
        {
            // 1. What is the parent of this item? (Type is Cancer Information Summary Link)
            PercussionGuid summaryLinkID = FindSummaryLink(documentID);

            // Set up slot names and "other audience" search critera.
            string theirSlotName;   // Slot to search for alternate version.
            string mySlotName;      // Slot to store this item in.
            string otherAudience;
            if (audienceType.Equals(PatientAudience, StringComparison.InvariantCultureIgnoreCase))
            {
                mySlotName = PatientVersionLinkSlot;
                theirSlotName = HealthProfVersionLinkSlot;
                otherAudience = HealthProfAudience;
            }
            else
            {
                mySlotName = HealthProfVersionLinkSlot;
                theirSlotName = PatientVersionLinkSlot;
                otherAudience = PatientAudience;
            }

            // 3. Search the link item for child items in the other audience type's slot.
            PercussionGuid otherAudienceVersion = FindAudienceVersion(summaryLinkID, otherAudience);

            // 4. If the slot in the summary link contains a content item, it must be of the opposite audience type.
            if (otherAudienceVersion != null)
            {
                // Link from this item to the alternate version.
                CMSController.CreateActiveAssemblyRelationships(documentID.ID, new long[] { otherAudienceVersion.ID }, theirSlotName, AudienceTabSnippetTemplate);

                // Link from the alternate version back to this one.
                // TODO: Delete any existing relationships in that slot.
                CMSController.CreateActiveAssemblyRelationships(otherAudienceVersion.ID, new long[] { documentID.ID }, mySlotName, AudienceTabSnippetTemplate);
            }
        }

        protected void LinkTableSectionsToAlternateAudienceVersion()
        {
        }

        /// <summary>
        /// Finds the SummaryLink content item for a CancerInfoSummary root item.
        /// </summary>
        /// <param name="summaryRootID">Content ID of the CancerInfoSummary root item.</param>
        /// <returns>The PercussionGuid of the parent SummaryLink node.</returns>
        protected PercussionGuid FindSummaryLink(PercussionGuid summaryRootID)
        {
            // 1. What is the parent of this item? (Type is Cancer Information Summary Link)
            PSItem[] parentItems = CMSController.LoadLinkingContentItems(summaryRootID.ID);
            PSItem summaryLink =
                Array.Find(parentItems, item => item.contentType == CancerInfoSummaryLinkContentType);
            if (summaryLink == null)
                throw new CMSOperationalException(string.Format("Cannot locate CancerInfoSummaryLink for content item {0}", summaryRootID.ToString()));

            return new PercussionGuid(summaryLink.id);
        }

        /// <summary>
        /// Locates a specific audience version of a summary.
        /// </summary>
        /// <param name="summaryLink">ID of the summary's parent SummaryLink node.</param>
        /// <param name="audienceType">Audience type to load.</param>
        /// <returns>The ID of the root Summary content item for the specified audience.
        /// Null if no qualifying Summary version is found.</returns>
        protected PercussionGuid FindAudienceVersion(PercussionGuid summaryLink, string audienceType)
        {
            PercussionGuid summaryRoot = null;

            string containingSlot;

            // Determine which slot to use.
            if (audienceType.Equals(PatientAudience, StringComparison.InvariantCultureIgnoreCase))
                containingSlot = PatientVersionLinkSlot;
            else if (audienceType.Equals(HealthProfAudience, StringComparison.InvariantCultureIgnoreCase))
                containingSlot = HealthProfVersionLinkSlot;
            else
                throw new ArgumentException(string.Format("Argument audienceType must be {0} or {1}.", PatientAudience, HealthProfAudience));

            // Look for a summary item in the slot.
            PercussionGuid[] searchResults = CMSController.SearchForItemsInSlot(summaryLink, containingSlot);
            if (searchResults.Length > 0)
            {
                summaryRoot = searchResults[0];
            }

            return summaryRoot;
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
                PercussionGuid summaryLink = LocateExistingSummaryLink(rootItem);
                PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(rootItem, SummaryPageSlot);
                //PercussionGuid[] subItems = LocateMediaLinksAndTableSections(pageIDs); // Table sections and MediaLinks.
                PermanentLinkHelper PermanentLinkData = new PermanentLinkHelper(CMSController, sitePath);
                PercussionGuid[] permanentLinks = PermanentLinkData.DetectToDeletePermanentLinkRelationships();

                // Create a list of all content IDs making up the document.
                // It is important for verification that rootItem always be first.
                PercussionGuid[] fullIDList = CMSController.BuildGuidArray(rootItem, pageIDs/*, subItems*/, summaryLink);

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

            /// Find all the incoming relationships.  (Need to include inline links!)
            PSAaRelationship[] incomingRelationship =
                CMSController.FindIncomingActiveAssemblyRelationships(summaryIdentifers);
            List<PercussionGuid> externalOwners = new List<PercussionGuid>();

            /// Eliminate the internal relationships.
            Array.ForEach(incomingRelationship, relationship =>
            {
                // If the owner of this relationship isn't one of the objects making up the document,
                // then add the relationship's owner to the list of external owners.
                PercussionGuid owner = new PercussionGuid(relationship.ownerId);
                if (Array.Find(summaryIdentifers, guid => guid.ID == owner.ID) == null)
                    externalOwners.Add(owner);
            });

            // Find the ID of any alternate audience versions and eliminate from the list.
            PercussionGuid[] alternateAudiences = LocateAlternateAudienceVersions(rootItem);
            externalOwners.RemoveAll(owner => alternateAudiences.Contains(owner));

            /// If there are any external relationship owners, don't allow the delete.
            if (externalOwners.Count > 0)
            {
                PSItem[] itemsWithLinks = CMSController.LoadContentItems(externalOwners.ToArray());
                StringBuilder sb = new StringBuilder();
                sb.Append("Document is referenced by:\n");
                foreach (PSItem item in itemsWithLinks)
                {
                    sb.AppendFormat("Content item: {0}\n", new PercussionGuid(item.id).ID);
                    string prettyUrlName = PSItemUtils.GetFieldValue(item, "pretty_url_name");
                    Array.ForEach(item.Folders, folder =>
                    {
                        sb.AppendFormat("\t{0}/{1}\n", folder.path, prettyUrlName);
                    });
                }

                throw new CMSCannotDeleteException(sb.ToString());
            }
        }

        /// <summary>
        /// Locates a Cancer Information Summary document's corresponding summary link item.
        /// </summary>
        /// <param name="rootItemID">The CMS ID of a CancerInformationSummary object.</param>
        /// <returns>The CMS ID of the matching CancerInformationSummaryLink object</returns>
        protected PercussionGuid LocateExistingSummaryLink(PercussionGuid rootItemID)
        {
            // TODO: Rewrite this to find all incoming Active Assembly relationships
            // which use the Patient or HealtProfessional slots with the link template.

            PercussionGuid summaryLinkID = null;

            PSItem[] rootItem = CMSController.LoadContentItems(new long[] { rootItemID.ID });
            VerifyItemHasPath(rootItem[0]);

            string itemPath = CMSController.GetPathInSite(rootItem[0]);

            // If the item path is null, then the content item has no path in *this* site.
            // Therefore, there is no summary link to search for.
            if (!string.IsNullOrEmpty(itemPath))
            {
                string linkPath = GetParentFolder(itemPath);
                PercussionGuid[] searchList =
                    CMSController.SearchForContentItems(CancerInfoSummaryLinkContentType, linkPath, null);

                // There may not be a summary link (e.g. mobile site). Let the caller decide how to handle that.
                if (searchList.Length > 0)
                {
                    // There should only be one item found. Verify that it's the one we want.
                    // Is the root ID in the patient slot?
                    PercussionGuid[] foundIDs = CMSController.SearchForItemsInSlot(searchList[0], PatientVersionLinkSlot);
                    if (foundIDs != null && foundIDs.Length > 0 && foundIDs[0].Equals(rootItemID))
                    {
                        summaryLinkID = searchList[0];
                    }
                    else
                    {
                        // Is the root ID in the health professional slot?
                        foundIDs = CMSController.SearchForItemsInSlot(searchList[0], HealthProfVersionLinkSlot);
                        if (foundIDs != null && foundIDs.Length > 0 && foundIDs[0].ID == rootItemID.ID)
                            summaryLinkID = searchList[0];
                    }
                }
            }

            return summaryLinkID;
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
                
        /// <summary>
        /// Searches for all content items which refer to the root item as their alternate
        /// audience version. In theory, this should be a 1:1 relationship, but a full
        /// set of values is returned so that errors don't prevent deletion.
        /// </summary>
        /// <param name="rootItemID">ID of the CancerInformationSummary item to be
        /// checked for alternate audiences.</param>
        /// <returns>A non-null, possibly empty array of item IDs which identify rootItemID
        /// as their alternate audience version.</returns>
        protected PercussionGuid[] LocateAlternateAudienceVersions(PercussionGuid rootItemID)
        {
            List<PercussionGuid> foundOwners = new List<PercussionGuid>();

            PercussionGuid[] rootIDArray = new PercussionGuid[] { rootItemID };
            string[] slots = { PatientVersionLinkSlot, HealthProfVersionLinkSlot };

            PSAaRelationship[] relationships;
            Array.ForEach(slots, slotname =>
            {
                // Find all relationships using the alternate audience slots in conjunction
                // with the template for audience tabs.
                relationships =
                    CMSController.FindIncomingActiveAssemblyRelationships(rootIDArray,
                    slotname, AudienceTabSnippetTemplate);
                Array.ForEach(relationships, rel => foundOwners.Add(new PercussionGuid(rel.ownerId)));
            });

            return foundOwners.ToArray();
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
            contentItemList.Add(contentItem);

            return contentItemList;
        }

        protected FieldSet CreateFieldValueMapPDQCancerInfoSummary(SummaryDocument summary)
        {
            FieldSet fields = new FieldSet();
            string prettyURLName = GetSummaryPrettyUrlName(summary.BasePrettyURL);

            string TOC = "";

            // Explicitly set pretty_url_name to null so the CI Summary will be
            // the folder's default document.
            fields.Add("pretty_url_name", null);

            fields.Add("long_title", summary.Title);

            //if (summary.Title.Length > ShortTitleLength)
            //    fields.Add("short_title", summary.Title.Substring(0, ShortTitleLength));
            //else
            //    fields.Add("short_title", summary.Title);

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

            fields.Add("table_of_contents", TOC);

            fields.Add("sys_title", EscapeSystemTitle(summary.Title));

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

        protected void UpdateNavOn(SummaryDocument document, PercussionGuid summaryRootItemID, string path)
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

        protected List<ContentItemForCreating> CreatePDQCancerInfoSummaryLink(SummaryDocument document, string creationPath)
        {
            // Content items may be re-created during an update, therefore we must pass the create path from the caller.

            List<ContentItemForCreating> contentItemList = new List<ContentItemForCreating>();

            ContentItemForCreating contentItem =
                new ContentItemForCreating(CancerInfoSummaryLinkContentType, CreateFieldValueMapPDQCancerInfoSummaryLink(document), creationPath);
            contentItemList.Add(contentItem);

            return contentItemList;
        }

        protected FieldSet CreateFieldValueMapPDQCancerInfoSummaryLink(SummaryDocument summary)
        {
            FieldSet fields = new FieldSet();

            fields.Add("sys_title", summary.ShortTitle);
            fields.Add("long_title", summary.Title);
            fields.Add("short_title", summary.ShortTitle);
            fields.Add("long_description", summary.Description);

            // HACK: This relies on Percussion not setting anything else in the login session.
            fields.Add("sys_lang", GetLanguageCode(summary.Language));

            return fields;

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