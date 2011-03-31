﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.DocumentObjects.Media;
using GKManagers.CMSManager.Configuration;

using NCI.WCM.CMSManager;
using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

// TODO: Move all the CreateXXXXXX content type methods into a single factory object.
// As written, CancerInfoSummaryProcessor has way too many concerns, with too many methods
// that aren't directly related to putting things into the CMS.
// Take the document as an argument on the constructor, expose properties with collections
// of strongly typed meta content types with a single base class

namespace GKManagers.CMSDocumentProcessing
{
    public class CancerInfoSummaryProcessor : DocumentProcessorCommon, IDocumentProcessor, IDisposable
    {
        private PercussionConfig PercussionConfig;

        #region Constants

        const int ShortTitleLength = 64;

        const string PatientVersionLinkSlot = "pdqCancerInformationSummaryPatient";
        const string HealthProfVersionLinkSlot = "pdqCancerInformationSummaryHealthProf";
        const string SummaryPageSlot = "pdqCancerInformationSummaryPageSlot";
        const string SummaryRefSlot = "pdqCancerInformationSummaryRef";
        const string InlineSlot = "sys_inline_variant";

        const string InlineLinkSlotID = "103";
        const string InlinImageSlotID = "104";
        const string InlineTemplateSlotID = "105";

        const string AudienceLinkSnippetTemplate = "pdqSnCancerInformationSummaryItemLink";
        const string AudienceTabSnippetTemplate = "pdqSnCancerInformationSummaryItemAudienceTab";
        const string MediaLinkSnippetTemplate = "pdqSnMediaLink";
        const string SummarySectionSnippetTemplate = "pdqSnCancerInformationSummaryPage";
        const string TableSectionSnippetTemplate = "pdqSnTableSection";

        // TODO: Seriously, the AudienceType string needs to be changed to an Enum.
        const string PatientAudience = "Patients";
        const string HealthProfAudience = "HealthProfessional";

        #endregion

        #region Runtime Constants

        // Yeah, it's a funny name for the region. These values are loaded at runtime,
        // but aren't allowed to change during the run.

        // Contain the names of the ContentTypes used to represent Cancer Info Sumamries in the CMS.
        // Set in the constructor.
        readonly private string CancerInfoSummaryContentType;
        readonly private string CancerInfoSummaryPageContentType;
        readonly private string CancerInfoSummaryLinkContentType;
        readonly private string MediaLinkContentType;
        readonly private string TableSectionContentType;

        readonly private string[] SummaryContentTypes;

        #endregion


        #region Internal Delegates

        private delegate string SubDocumentIDDelegate<SubDocumentType>(SubDocumentType subDocument);
        private delegate void ItemTransitionDelegate(PercussionGuid[] itemList);

        #endregion


        public CancerInfoSummaryProcessor(HistoryEntryWriter warningWriter, HistoryEntryWriter informationWriter)
            : base(warningWriter, informationWriter)
        {
            PercussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            CancerInfoSummaryContentType = PercussionConfig.ContentType.PDQCancerInfoSummary.Value;
            CancerInfoSummaryPageContentType = PercussionConfig.ContentType.PDQCancerInfoSummaryPage.Value;
            CancerInfoSummaryLinkContentType = PercussionConfig.ContentType.PDQCancerInfoSummaryLink.Value;
            MediaLinkContentType = PercussionConfig.ContentType.PDQMediaLink.Value;
            TableSectionContentType = PercussionConfig.ContentType.PDQTableSection.Value;

            SummaryContentTypes = new String[] {CancerInfoSummaryContentType, CancerInfoSummaryPageContentType,
                                           CancerInfoSummaryLinkContentType, MediaLinkContentType,
                                           TableSectionContentType};
        }

        #region IDocumentProcessor Members

        /// <summary>
        /// Main entry point for processing a Cancer Information Summary (formerly just "Summary")
        /// object which is to be managed in the CMS.
        /// </summary>
        /// <param name="documentObject"></param>
        public void ProcessDocument(Document documentObject)
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
                CreateNewCancerInformationSummary(document);
            }
            else
            {
                InformationWriter(string.Format("Update existing content item for document CDRID = {0}.", document.DocumentID));
                UpdateCancerInformationSummary(document, identifier);
            }


            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
        }

        /// <summary>
        /// Move the specified DrugInfoSummary document from Staging to Preview.
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public void PromoteToPreview(int documentID)
        {
            PercussionGuid summaryRootID = GetCdrDocumentID(CancerInfoSummaryContentType, documentID);

            PercussionGuid summaryLinkID = LocateExistingSummaryLink(summaryRootID);

            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(summaryRootID, SummaryPageSlot);
            PercussionGuid[] subItems = LocateMediaLinksAndTableSections(pageIDs); // Table sections and MediaLinks.

            PerformTransition(TransitionItemsToPreview, summaryRootID, summaryLinkID, pageIDs, subItems);
        }

        /// <summary>
        /// Move the specified DrugInfoSummary document from Preview to Live.
        /// </summary>
        /// <param name="documentID">The document's CDR ID.</param>
        public void PromoteToLive(int documentID)
        {
            PercussionGuid summaryRootID = GetCdrDocumentID(CancerInfoSummaryContentType, documentID);
            PercussionGuid summaryLinkID = LocateExistingSummaryLink(summaryRootID);
            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(summaryRootID, SummaryPageSlot);
            PercussionGuid[] subItems = LocateMediaLinksAndTableSections(pageIDs); // Table sections and MediaLinks.

            PerformTransition(TransitionItemsToLive, summaryRootID, summaryLinkID, pageIDs, subItems);
        }

        private void PerformTransition(ItemTransitionDelegate transitionMethod,
            PercussionGuid summaryRootID,
            PercussionGuid summaryLinkID,
            PercussionGuid[] pageIDs,
            PercussionGuid[] subItems)
        {
            // Root item and Link must move independently of each other and everything else.
            transitionMethod(new PercussionGuid[] { summaryRootID });
            transitionMethod(new PercussionGuid[] { summaryLinkID });

            // Pages and subPages are created new on each update and will therefore be in
            // a different state than the root and link items.
            PercussionGuid[] pageCollection = CMSController.BuildGuidArray(pageIDs, subItems);
            transitionMethod(pageCollection);
        }


        private void CreateNewCancerInformationSummary(SummaryDocument document)
        {
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

                // Create the sub-pages
                List<long> tableIDs;
                List<long> mediaLinkIDs;
                CreateSubPages(document, createPath, rollbackList, out tableIDs, out mediaLinkIDs);

                // Find the list of content items referenced by the summary sections.
                // After the page items are created, these are used to create relationships.
                List<List<PercussionGuid>> referencedSumamries = ResolveSummaryReferences(document);

                // Create the Cancer Info Summary item.
                List<ContentItemForCreating> rootList = CreatePDQCancerInfoSummary(document, createPath);
                summaryRoot = new PercussionGuid(CMSController.CreateContentItemList(rootList)[0]);
                rollbackList.Add(summaryRoot);


                //Create Cancer Info Summary Page items
                List<ContentItemForCreating> summaryPageList = CreatePDQCancerInfoSummaryPage(document, createPath);
                List<long> pageRawIDList = CMSController.CreateContentItemList(summaryPageList);
                summaryPageIDList = pageRawIDList.ToArray();
                rollbackList.AddRange(CMSController.BuildGuidArray(pageRawIDList));

                // Add summary pages into the page slot.
                PSAaRelationship[] relationships = CMSController.CreateActiveAssemblyRelationships(summaryRoot.ID, summaryPageIDList, SummaryPageSlot, SummarySectionSnippetTemplate);

                // Create relationships to other Cancer Information Summary Objects.
                PSAaRelationship[] externalRelationships = CreateExternalSummaryRelationships(summaryPageIDList, referencedSumamries);

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

                // Find the Patient or Health Professional version and create a relationship.
                LinkRootItemToAlternateAudienceVersion(summaryRoot, document.AudienceType);

                LinkToAlternateLanguageVersion(document, summaryRoot);
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

        private void UpdateCancerInformationSummary(SummaryDocument summary, PercussionGuid summaryRootID)
        {
            // For undoing failed updates.
            List<PercussionGuid> rollbackList = new List<PercussionGuid>();

            // Retrieve IDs for the summary's components.
            PercussionGuid summaryLinkID;
            try
            {
                summaryLinkID = LocateExistingSummaryLink(summaryRootID);
            }
            catch (FolderAssociationException ex)
            {
                throw new FolderAssociationException(string.Format("Content item {0} has no path, CDRID = {1}.",
                    summaryRootID, summary.DocumentID), ex);
            }

            PercussionGuid[] oldPageIDs = CMSController.SearchForItemsInSlot(summaryRootID, SummaryPageSlot);
            PercussionGuid[] oldSubItems = LocateMediaLinksAndTableSections(oldPageIDs); // Table sections and MediaLinks.

            // Determine which pages in this summary are dependent items in other summaries.
            PSAaRelationship[] incomingPageRelationships = FindIncomingPageRelationships(summaryRootID, summaryLinkID, oldPageIDs);

            // Doublecheck paths.
            PSItem[] summaryRootItem = CMSController.LoadContentItems(new PercussionGuid[] { summaryRootID });
            VerifyItemHasPath(summaryRootItem[0], summary.DocumentID);

            string existingItemPath = CMSController.GetPathInSite(summaryRootItem[0]);

            // Is the summary in a state suitable for updating?
            VerifyDocumentMayBeUpdated(summary, existingItemPath, summaryRootID, summaryLinkID, oldPageIDs, incomingPageRelationships);
            
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

            try
            {
                // Move the entire composite document to staging.
                // This step is not required when creating items since creation takes place in staging.
                PerformTransition(TransitionItemsToStaging, summaryRootID, summaryLinkID, oldPageIDs, oldSubItems);

                // Create the new folder, but don't publish the navon.  This is deliberate.
                tempFolder = CMSController.GuaranteeFolder(temporaryPath, FolderManager.NavonAction.None);

                // Create the new sub-page items in a temporary location.
                CreateSubPages(summary, temporaryPath, rollbackList, out tableIDs, out mediaLinkIDs);

                // Find the list of content items referenced by the summary sections.
                // After the page items are created, these are used to create relationships.
                List<List<PercussionGuid>> referencedItems = ResolveSummaryReferences(summary);

                // Create new Cancer Info Summary Page items
                List<long> idList;
                List<ContentItemForCreating> summaryPageList = CreatePDQCancerInfoSummaryPage(summary, temporaryPath);
                idList = CMSController.CreateContentItemList(summaryPageList);
                newSummaryPageIDList = idList.ToArray();
                PercussionGuid[] newPageIDs = Array.ConvertAll(newSummaryPageIDList, pageID => new PercussionGuid(pageID));
                rollbackList.AddRange(newPageIDs);

                UpdateIncomingSummaryReferences(summary.DocumentID, summaryRootID, summaryLinkID, oldPageIDs, newPageIDs, incomingPageRelationships);

                // Add new cancer information summary pages into the page slot.
                PSAaRelationship[] relationships = CMSController.CreateActiveAssemblyRelationships(summaryRootID.ID, newSummaryPageIDList, SummaryPageSlot, SummarySectionSnippetTemplate);

                // Create relationships to other Cancer Information Summary Objects.
                PSAaRelationship[] externalRelationships = CreateExternalSummaryRelationships(newSummaryPageIDList, referencedItems);

                // Update (but don't replace) the CancerInformationSummary and CancerInformationSummaryLink objects.
                ContentItemForUpdating summaryItem = new ContentItemForUpdating(summaryRootID.ID, CreateFieldValueMapPDQCancerInfoSummary(summary));
                ContentItemForUpdating summaryLinkItem = new ContentItemForUpdating(summaryLinkID.ID, CreateFieldValueMapPDQCancerInfoSummaryLink(summary));
                List<ContentItemForUpdating> itemsToUpdate = new List<ContentItemForUpdating>(new ContentItemForUpdating[] { summaryItem, summaryLinkItem });
                List<long> updatedItemIDs = CMSController.UpdateContentItemList(itemsToUpdate);
            }
            catch (Exception)
            {
                // In the event of an error while updating attempt to cleanup.
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
            PercussionGuid[] componentIDs = CMSController.BuildGuidArray(tableIDs, mediaLinkIDs, newSummaryPageIDList);
            CMSController.MoveContentItemFolder(temporaryPath, existingItemPath, componentIDs);
            CMSController.DeleteFolders(new PSFolder[] { tempFolder });

            // Handle a potential change of URL.
            UpdateDocumentURL(summary.BasePrettyURL, summaryRootID, summaryLinkID, componentIDs);
        }


        /// <summary>
        /// Performs checks to verify that the summary document is in a condition suitable for updating.
        /// Throws CannotUpdateException if the document may not be updated.
        /// </summary>
        /// <param name="summary">The updated summary.</param>
        /// <param name="summaryPath">Path to the summary's main folder.</param>
        /// <param name="summaryRoot">PercussionGuid of the composite document's root CancerInfoSummary content item.</param>
        /// <param name="summaryLink">PercussionGuid of the composite document's root CancerInfoSummaryLink content item.</param>
        /// <param name="summaryPages">Array of PercussionGuids representing the current set of pages.</param>
        /// <param name="incomingPageRelationships">Array of Percussion Relationship structures for items linking
        /// to the individual summary pages.</param>
        protected void VerifyDocumentMayBeUpdated(SummaryDocument summary,
            string summaryPath,
            PercussionGuid summaryRoot,
            PercussionGuid summaryLink,
            PercussionGuid[] summaryPages,
            PSAaRelationship[] incomingPageRelationships)
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

                // Load the body of the page containing the link.
                string sourceBodyField = PSItemUtils.GetFieldValue(parentItem, "bodyfield");
                XmlDocument body = new XmlDocument();
                body.LoadXml(sourceBodyField);

                // Find all summary references contained in the source document.
                XmlNodeList nodeList = body.SelectNodes("//a[@inlinetype='SummaryRef']");
                foreach (XmlNode node in nodeList)
                {
                    // Find the section number
                    XmlAttributeCollection attributeList = node.Attributes;
                    XmlAttribute referenceAttribute = attributeList["objectid"];

                    SummaryReference reference = new SummaryReference(referenceAttribute.Value, summaryPath);

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
 

        private void RemoveOldPages(PercussionGuid[] oldPageIDs, PercussionGuid[] oldSubItems)
        {
            PercussionGuid[] combinedIDList = CMSController.BuildGuidArray(oldPageIDs, oldSubItems);

            CMSController.DeleteItemList(combinedIDList);
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
        private void UpdateIncomingSummaryReferences(int targetCDRID,
            PercussionGuid summaryRootID, PercussionGuid summaryLinkID, PercussionGuid[] oldPageIDs, PercussionGuid[] newPageIDs,
            PSAaRelationship[] incomingRelationships)
        {

            if (incomingRelationships.Length > 0)
            {
                ItemCache itemStore = new ItemCache(CMSController);
                PercussionGuid[] combinedList = CMSController.BuildGuidArray(oldPageIDs, newPageIDs, summaryRootID);

                // Check out all relationship owners. We know they will be modified.
                PercussionGuid[] itemsToCheckout = // Filter out duplicate item IDs.
                    (from idValue in
                         (from relationship in incomingRelationships select relationship.ownerId).Distinct()
                     select new PercussionGuid(idValue)).ToArray();

                PSItemStatus[] checkedOutPageStatus = CMSController.CheckOutForEditing(itemsToCheckout);

                CancerInfoSummarySectionFinder finder = new CancerInfoSummarySectionFinder(CMSController);
                itemStore.Preload(combinedList);
                PSItem rootItem = itemStore.LoadContentItem(summaryRootID);
                string basePath = CMSController.GetPathInSite(rootItem);

                List<KeyValuePair<PercussionGuid, PercussionGuid>> relationshipPairs
                                    = new List<KeyValuePair<PercussionGuid, PercussionGuid>>();

                foreach (PSAaRelationship individual in incomingRelationships)
                {
                    PercussionGuid sourceID = new PercussionGuid(individual.ownerId);
                    PercussionGuid oldTargetID = new PercussionGuid(individual.dependentId);
                    PSItem oldTargetItem = itemStore.LoadContentItem(oldTargetID);
                    PSItem sourceItem = itemStore.LoadContentItem(sourceID);

                    string sourceBodyField = PSItemUtils.GetFieldValue(sourceItem, "bodyfield");
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
                            int pageNumber;
                            PercussionGuid referencedItemID;

                            // Find the page number in the new list of sections.
                            finder.FindPageContainingSection(newPageIDs, reference.SectionID, out pageNumber, out referencedItemID);

                            // Rebuild the link.
                            string url = BuildSummaryRefUrl(reference.Url, pageNumber, reference.SectionID);
                            XmlAttribute href = attributeList["href"];
                            href.Value = url;

                            // Add the item to the list of references.
                            relationshipPairs.Add(new KeyValuePair<PercussionGuid, PercussionGuid>(sourceID, referencedItemID));
                        }
                        else
                        {
                            // Links to the summary without a fragment. (page 1)

                            // Find the page number in the new list of sections.
                            PercussionGuid referencedItemID = finder.FindFirstPage(newPageIDs);

                            // Add the item to the list of references.
                            relationshipPairs.Add(new KeyValuePair<PercussionGuid, PercussionGuid>(sourceID, referencedItemID));

                            // Rebuild the link. (URL may have changed)
                            XmlAttribute href = attributeList["href"];
                            href.Value = reference.Url;
                        }
                    }

                    // Save the updated HTML.
                    CMSController.SaveContentItems(new PSItem[] { sourceItem });
                    itemStore.RemoveContentItem(sourceID); // Delete cached copy
                }

                CMSController.ReleaseFromEditing(checkedOutPageStatus.ToArray());

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
                            new PercussionGuid[] { kvp.Value }, SummaryRefSlot, SummarySectionSnippetTemplate);
                    });
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
        private PSAaRelationship[] FindExternalRelationships(PercussionGuid[] internalIdentifers)
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

        private PSAaRelationship[] FindIncomingPageRelationships(PercussionGuid summaryRootID, PercussionGuid summaryLinkID,
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

        private void CreateSubPages(SummaryDocument summary, string createPath,
            List<PercussionGuid> rollbackList,
            out List<long> tableIDs, out List<long> mediaLinkIDs)
        {
            // Create the embeddable content items and resolve the item references.

            // Create the MediaLinks
            List<ContentItemForCreating> medialLinkList = CreatePDQMediaLink(summary, createPath);
            mediaLinkIDs = CMSController.CreateContentItemList(medialLinkList);
            rollbackList.AddRange(CMSController.BuildGuidArray(mediaLinkIDs));

            // Resolve MediaLink inline slots within the pages.
            SectionToCmsIDMap mediaLinkIDMap = BuildItemIDMap(summary.MediaLinkSectionList, MediaLinkIDAccessor, mediaLinkIDs);
            ResolveInlineSlots(summary.SectionList, mediaLinkIDMap, MediaLinkSnippetTemplate);

            // Resolve MediaLink references within table sections.
            ResolveTableSectionMediaLinkInlineSlots(summary.TableSectionList, mediaLinkIDMap, MediaLinkSnippetTemplate);

            // Create the Table sections
            List<ContentItemForCreating> tableList = CreatePDQTableSections(summary, createPath);
            tableIDs = CMSController.CreateContentItemList(tableList);
            rollbackList.AddRange(CMSController.BuildGuidArray(tableIDs));

            // Resolve Table inline slots within the pages.
            SectionToCmsIDMap tableIDMap = BuildItemIDMap(summary.TableSectionList, SummarySectionIDAccessor, tableIDs);
            ResolveInlineSlots(summary.SectionList, tableIDMap, TableSectionSnippetTemplate);
        }

        /// <summary>
        /// Helper method to verify that an existing content items belongs to a folder path, any folder path.
        /// Throws FolderAssociationException if item is not associated with a folder.
        /// </summary>
        /// <param name="item">The content item to be checked for a path.</param>
        private void VerifyItemHasPath(PSItem item)
        {
            VerifyItemHasPath(item, null);
        }

        private void VerifyItemHasPath(PSItem item, int? documentID)
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

        private void UpdateDocumentURL(string targetURL, PercussionGuid summaryRootItemID,
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

                oldPath = CMSController.GetPathInSite(keyItems[1]); // Link item.
                newPath = GetParentFolder(targetURL);
                CMSController.MoveContentItemFolder(oldPath, newPath, new PercussionGuid[] { summaryLinkItemID });
            }
        }


        private SectionToCmsIDMap BuildItemIDMap<SubDocumentType>(IList<SubDocumentType> subDocumentList,
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


        private void ResolveInlineSlots(List<SummarySection> sectionList, SectionToCmsIDMap itemIDMap, string templateName)
        {
            PercussionGuid snippetTemplate = CMSController.TemplateNameManager[templateName];

            foreach (SummarySection section in sectionList.Where(item => item.IsTopLevel))
            {
                XmlDocument html = section.Html;
                XmlNodeList nodeList = html.SelectNodes("//div[@inlinetype='rxvariant']");

                foreach (XmlNode node in nodeList)
                {
                    XmlAttributeCollection attributeList = node.Attributes;

                    XmlAttribute reference = attributeList["objectid"];
                    XmlAttribute attrib;

                    if (itemIDMap.ContainsSectionKey(reference.Value))
                    {
                        string target = reference.Value;
                        long dependent = itemIDMap[target].ID;

                        attrib = html.CreateAttribute("sys_dependentid");
                        attrib.Value = dependent.ToString();
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("contenteditable");
                        attrib.Value = "false";
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("rxinlineslot");
                        attrib.Value = InlineTemplateSlotID;
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("sys_dependentvariantid");
                        attrib.Value = snippetTemplate.ID.ToString();
                        attributeList.Append(attrib);
                    }
                }
            }
        }

        /// <summary>
        /// Resolves medialink inline slots contained within table sections
        /// </summary>
        /// <param name="tableSectionList"></param>
        /// <param name="mediaLinkIDMap"></param>
        /// <param name="templateName"></param>
        private void ResolveTableSectionMediaLinkInlineSlots(List<SummarySection> tableSectionList, SectionToCmsIDMap mediaLinkIDMap, string templateName)
        {
            PercussionGuid snippetTemplate = CMSController.TemplateNameManager[templateName];

            // NOTE: Unlike ResolveInlineSlots for SummarySection lists, this has no where clause.
            foreach (SummarySection section in tableSectionList)
            {
                XmlDocument html = section.Html;
                XmlNodeList nodeList = html.SelectNodes("//div[@inlinetype='rxvariant']");

                foreach (XmlNode node in nodeList)
                {
                    XmlAttributeCollection attributeList = node.Attributes;

                    XmlAttribute reference = attributeList["objectid"];
                    XmlAttribute attrib;

                    if (mediaLinkIDMap.ContainsSectionKey(reference.Value))
                    {
                        string target = reference.Value;
                        long dependent = mediaLinkIDMap[target].ID;

                        attrib = html.CreateAttribute("sys_dependentid");
                        attrib.Value = dependent.ToString();
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("contenteditable");
                        attrib.Value = "false";
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("rxinlineslot");
                        attrib.Value = InlineTemplateSlotID;
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("sys_dependentvariantid");
                        attrib.Value = snippetTemplate.ID.ToString();
                        attributeList.Append(attrib);
                    }
                }

                // Lather, rinse and repeast for the full-size HTML.
                html = section.StandaloneHTML;
                nodeList = html.SelectNodes("//div[@inlinetype='rxvariant']");

                foreach (XmlNode node in nodeList)
                {
                    XmlAttributeCollection attributeList = node.Attributes;

                    XmlAttribute reference = attributeList["objectid"];
                    XmlAttribute attrib;

                    if (mediaLinkIDMap.ContainsSectionKey(reference.Value))
                    {
                        string target = reference.Value;
                        long dependent = mediaLinkIDMap[target].ID;

                        attrib = html.CreateAttribute("sys_dependentid");
                        attrib.Value = dependent.ToString();
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("contenteditable");
                        attrib.Value = "false";
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("rxinlineslot");
                        attrib.Value = InlineTemplateSlotID;
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("sys_dependentvariantid");
                        attrib.Value = snippetTemplate.ID.ToString();
                        attributeList.Append(attrib);
                    }
                }
            }
        }

        /// <summary>
        /// Replaces SummaryRef placeholder tags with links to the individual summary sections.
        /// The added links will point to the pretty URL of the top-level section, and optionally
        /// end with a document fragment identifier.
        /// </summary>
        /// <param name="summary">A list of objects corresponding 1:1 to the collection of top-level sections.
        /// Each object in the list is a (possibly empty) collection of PercussionGuid objects refererenced by
        /// the corresponding top-leavel section.</param>
        private List<List<PercussionGuid>> ResolveSummaryReferences(SummaryDocument summary)
        {
            List<List<PercussionGuid>> listOfLists = new List<List<PercussionGuid>>();

            CancerInfoSummarySectionFinder finder = new CancerInfoSummarySectionFinder(CMSController);

            foreach (SummarySection section in summary.SectionList.Where(item => item.IsTopLevel))
            {
                // Every section has a unique list of referenced items.  If no items are referenced,
                // the list is empty, and this is OK.
                List<PercussionGuid> referencedContentItems = new List<PercussionGuid>();
                listOfLists.Add(referencedContentItems);

                XmlDocument html = section.Html;
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
                            string url = BuildSummaryRefUrl(details.Url, pageNumber, details.SectionID);
                            attrib = html.CreateAttribute("href");
                            attrib.Value = url;
                            attributeList.Append(attrib);
                        }
                        else if (details.IsSectionReference)
                        {
                            // Link to an external summary with a document fragment.
                            int pageNumber;
                            PercussionGuid referencedItemID;
                            finder.FindPageContainingSection(referencedItemRootID, details.SectionID, out pageNumber, out referencedItemID);

                            if (referencedItemID == null)
                            {
                                string message =
                                    string.Format("Unable to resolve section {0} in document with CDRID={1}.",
                                    details.SectionID, details.CdrID);
                                WarningWriter(message);
                                continue;
                            }
                            
                            // Add the item to the list.
                            referencedContentItems.Add(referencedItemID);

                            // Build the link.
                            string url = BuildSummaryRefUrl(details.Url, pageNumber, details.SectionID);
                            attrib = html.CreateAttribute("href");
                            attrib.Value = url;
                            attributeList.Append(attrib);
                        }
                        else
                        {
                            // Link to a summary without a fragment.

                            // Add the item to the list.
                            PercussionGuid referencedItemID = finder.FindFirstPage(referencedItemRootID);

                            if (referencedItemID == null)
                            {
                                WarningWriter(string.Format("Unable to find Summary document with CDRID={0}.", details.CdrID));
                                continue;
                            }


                            referencedContentItems.Add(referencedItemID);

                            // Build the link.
                            attrib = html.CreateAttribute("href");
                            attrib.Value = details.Url;
                            attributeList.Append(attrib);
                        }
                    }
                }
            }

            return listOfLists;
        }

        private string BuildSummaryRefUrl(string baseUrl, int pageNumber, string sectionID)
        {
            string url;

            // Page numbers are natural numbers (1, 2, 3...), not zero-based.
            if (pageNumber > 1)
                url = string.Format("{0}/Page{1}#Section{2}", baseUrl, pageNumber, sectionID);
            else
                url = string.Format("{0}/#Section{1}", baseUrl, sectionID);

            return url;
        }

        /// <summary>
        /// Creates active assembly relationships between a list of content items and
        /// the content items they refer to.
        /// </summary>
        /// <param name="pageIDList"></param>
        /// <param name="referencedItems"></param>
        /// <returns></returns>
        private PSAaRelationship[] CreateExternalSummaryRelationships(long[] pageIDList,
            List<List<PercussionGuid>> referencedItems)
        {
            int itemCount = pageIDList.Length;
            for (int i = 0; i < itemCount; i++)
            {
                PercussionGuid[] guidList = referencedItems[i].ToArray();

                // If the list entry for a given page contains references to other
                // Cancer info summaries, then create AA relationships to them.
                if (guidList.Length > 0)
                {
                    CMSController.CreateActiveAssemblyRelationships(pageIDList[i],
                        Array.ConvertAll(guidList, percGuid => (long)percGuid.ID),
                        SummaryRefSlot, SummarySectionSnippetTemplate);
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a relationship between the specified Cancer Information Summary document 
        /// and its alternate audience version, if one exists.
        /// </summary>
        /// <param name="documentID">Identifier for the content item to connect with its alternate version.</param>
        /// <param name="audienceType">Audience type value for the current content item.</param>
        private void LinkRootItemToAlternateAudienceVersion(PercussionGuid documentID, string audienceType)
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

        private void LinkTableSectionsToAlternateAudienceVersion()
        {
        }

        /// <summary>
        /// Finds the SummaryLink content item for a CancerInfoSummary root item.
        /// </summary>
        /// <param name="summaryRootID">Content ID of the CancerInfoSummary root item.</param>
        /// <returns>The PercussionGuid of the parent SummaryLink node.</returns>
        private PercussionGuid FindSummaryLink(PercussionGuid summaryRootID)
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
        private PercussionGuid FindAudienceVersion(PercussionGuid summaryLink, string audienceType)
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
            if (searchResults.Length>0)
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
        private void VerifyEnglishLanguageVersion(SummaryDocument summary)
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
        private void LinkToAlternateLanguageVersion(SummaryDocument summary, PercussionGuid rootID)
        {
            // We only need to set up a translation relationship from Spanish to English,
            // not the other way around.  CDR business rules dicatate that the English version
            // must exist before the translation can be created.
            if (summary.Language == Language.Spanish)
            {
                // If this relationship did not exist, we would not know the document was in Spanish.
                SummaryRelation relationship = summary.RelationList.Find(relation => relation.RelationType == SummaryRelationType.SpanishTranslationOf);

                // TODO: Make this relationship 1:1.

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
        public void DeleteContentItem(int documentID)
        {
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

                // Create a list of all content IDs making up the document.
                // It is important for verification that rootItem always be first.
                PercussionGuid[] fullIDList = CMSController.BuildGuidArray(rootItem, pageIDs, subItems, summaryLink);

                VerifyDocumentMayBeDeleted(fullIDList.ToArray());

                CMSController.DeleteItemList(fullIDList);
            }
        }

        /// <summary>
        /// Verifies that a document object has no incoming refernces. Throws CMSCannotDeleteException
        /// if the document is the target of any incoming relationships.
        /// </summary>
        /// <param name="documentCmsIdentifier">The document's ID in the CMS.</param>
        protected void VerifyDocumentMayBeDeleted(PercussionGuid[] summaryIdentifers)
        {
            // TODO: Modify this to call FindExternalRelationships().

            // The first item in the collection is always root.
            PercussionGuid rootItem = summaryIdentifers[0];

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
        private PercussionGuid LocateExistingSummaryLink(PercussionGuid rootItemID)
        {
            // TODO: Rewrite this to find all incoming Active Assembly relationships
            // which use the Patient or HealtProfessional slots with the link template.

            PercussionGuid summaryLinkID = null;

            PSItem[] rootItem = CMSController.LoadContentItems(new long[] { rootItemID.ID });
            VerifyItemHasPath(rootItem[0]);

            string linkPath = GetParentFolder(CMSController.GetPathInSite(rootItem[0]));
            PercussionGuid[] searchList =
                CMSController.SearchForContentItems(CancerInfoSummaryLinkContentType, linkPath, null);

            // It would be very weird to not find a summary link. Let the caller decide how to handle that.
            if (searchList != null)
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

            return summaryLinkID;
        }

        /// <summary>
        /// Locates all content items embedded in inline slots within a set of
        /// CancerInformationSummaryPage objects.
        /// </summary>
        /// <param name="pageIDList">An array of pages to search for items in inline slots.</param>
        /// <returns>The CMS identifers for the embedded content items.</returns>
        private PercussionGuid[] LocateMediaLinksAndTableSections(PercussionGuid[] pageIDList)
        {
            List<PercussionGuid> subItems = new List<PercussionGuid>();

            // Locate media links and tables as immediate children of the pages.
            Array.ForEach(pageIDList, pageID =>
            {
                PercussionGuid[] items = CMSController.SearchForItemsInSlot(pageID, InlineSlot);
                subItems.AddRange(items);
            });


            // Locate sub-items as childlren of each other.
            // E.g. a TableSection contains a MediaLink.
            HashSet<PercussionGuid> lowerItems = new HashSet<PercussionGuid>();
            subItems.ForEach(subItem =>
            {
                PercussionGuid[] items = CMSController.SearchForItemsInSlot(subItem, InlineSlot);
                Array.ForEach(items, item =>
                {
                    if (!lowerItems.Contains(item))
                        lowerItems.Add(item);
                });
            });

            subItems.AddRange(lowerItems);

            return subItems.Distinct().ToArray();
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
        private PercussionGuid[] LocateAlternateAudienceVersions(PercussionGuid rootItemID)
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

        private List<ContentItemForCreating> CreatePDQCancerInfoSummaryPage(SummaryDocument document, string creationPath)
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

        private FieldSet CreateFieldValueMapPDQCancerInfoSummaryPage(SummaryDocument summary, SummarySection summarySection)
        {
            FieldSet fields = new FieldSet();
            string html = summarySection.Html.OuterXml;

            // TODO: Move Summary-GlossaryTermRef Extract/Render out of the data access layer!
            // This kind of manipulation particularly shouldn't happen in the
            // routine that creates field/value pairs!
            if (summarySection.Html.OuterXml.Contains("Summary-GlossaryTermRef"))
            {
                string glossaryTermTag = "Summary-GlossaryTermRef";
                BuildGlossaryTermRefLink(ref html, glossaryTermTag);
            }

            fields.Add("bodyfield", html);

            // TODO: Implement table of contents.
            fields.Add("table_of_contents", (string.IsNullOrEmpty(summarySection.TOC) ? null : summarySection.TOC));

            string longTitle = summarySection.Title;
            fields.Add("long_title", longTitle);

            int shortLength = Math.Min(ShortTitleLength, longTitle.Length);
            fields.Add("short_title", longTitle.Substring(0, shortLength));
            
            fields.Add("sys_title", summarySection.Title);

            // HACK: This relies on Percussion not setting anything else in the login session.
            fields.Add("sys_lang", GetLanguageCode(summary.Language));
            fields.Add("top_sectionid", summarySection.RawSectionID);

            return fields;
        }

        private List<ContentItemForCreating> CreatePDQTableSections(SummaryDocument document, string creationPath)
        {
            // Content items may be re-created during an update, therefore we must pass the create path from the caller.

            List<ContentItemForCreating> contentItemList = new List<ContentItemForCreating>();
            int i;

            for (i = 0; i <= document.TableSectionList.Count - 1; i++)
            {
                ContentItemForCreating contentItem = new ContentItemForCreating(TableSectionContentType, CreateFieldValueMapPDQTableSection(document, document.TableSectionList[i]), creationPath);
                contentItemList.Add(contentItem);
            }

            return contentItemList;
        }

        private FieldSet CreateFieldValueMapPDQTableSection(SummaryDocument summary, SummarySection tableSection)
        {
            FieldSet fields = new FieldSet();
            string prettyURLName = tableSection.PrettyUrl.Substring(tableSection.PrettyUrl.LastIndexOf('/') + 1);

            fields.Add("pretty_url_name", prettyURLName);

            string summaryTitle = summary.Title;
            fields.Add("long_title", summaryTitle);

            // Short title is a maximum of 64 characters long,
            // Need to allow three characters for the " - " and the length of prettyURLName.
            int decorationLength = Math.Min(3 + prettyURLName.Length, ShortTitleLength);
            int shortLength = Math.Min(ShortTitleLength, summaryTitle.Length);
            string shortTitleLead;
            if ((shortLength + decorationLength) > ShortTitleLength)
            {
                int copyLength = ShortTitleLength - decorationLength;
                shortTitleLead = summaryTitle.Substring(0, copyLength);
            }
            else
                shortTitleLead = summaryTitle;
            fields.Add("short_title", string.Format("{0} - {1}", shortTitleLead, prettyURLName));

            if (!string.IsNullOrEmpty(tableSection.Title))
            {
                fields.Add("table_title", tableSection.Title);
            }

            fields.Add("inline_table", tableSection.Html.OuterXml);
            fields.Add("fullsize_table", tableSection.StandaloneHTML.OuterXml);

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

            fields.Add("sys_title", prettyURLName);

            // HACK: This relies on Percussion not setting anything else in the login session.
            fields.Add("sys_lang", GetLanguageCode(summary.Language));


            return fields;
        }



        private List<ContentItemForCreating> CreatePDQCancerInfoSummary(SummaryDocument document, string creationPath)
        {
            // Content items may be re-created during an update, therefore we must pass the create path from the caller.

            List<ContentItemForCreating> contentItemList = new List<ContentItemForCreating>();

            ContentItemForCreating contentItem = new ContentItemForCreating(CancerInfoSummaryContentType, CreateFieldValueMapPDQCancerInfoSummary(document), creationPath);
            contentItemList.Add(contentItem);

            return contentItemList;
        }

        private FieldSet CreateFieldValueMapPDQCancerInfoSummary(SummaryDocument summary)
        {
            FieldSet fields = new FieldSet();
            string prettyURLName = GetSummaryPrettyUrlName(summary.BasePrettyURL);

            string TOC = BuildTableOfContents(summary);

            // Explicitly set pretty_url_name to null so the CI Summary will be
            // the folder's default document.
            fields.Add("pretty_url_name", null);

            fields.Add("long_title", summary.Title);

            if (summary.Title.Length > ShortTitleLength)
                fields.Add("short_title", summary.Title.Substring(0, ShortTitleLength));
            else
                fields.Add("short_title", summary.Title);

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

            fields.Add("sys_title", summary.Title);

            // HACK: This relies on Percussion not setting anything else in the login session.
            fields.Add("sys_lang", GetLanguageCode(summary.Language));

            return fields;
        }

        private string BuildTableOfContents(SummaryDocument summary)
        {
            // TODO:  Replace BuildTableOfContents() with something a bit less hackish.
            // At the very least, it would be nice to drop the table and hard-coded
            // spacer URL.

            StringBuilder sb = new StringBuilder();

            int lastNestingLevel = 1;

            // The Table of Contents consists of section titles for the top three levels.
            summary.SectionList.ForEach(section =>
            {
                if (section.Level <= 3 &&
                    !string.IsNullOrEmpty(section.Title))
                {
                    if (section.Level > lastNestingLevel)
                        sb.Append("<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" width=\"100%\"><tr><td><img src=\"http://www.cancer.gov/images/spacer.gif\" border=\"0\" width=\"30\" height=\"1\" alt=\"\"></td><td width=\"100%\">");
                    else if (section.Level < lastNestingLevel)
                        sb.Append("</td></tr></table>");

                    sb.AppendFormat("<a href=\"#Section{0}\">{1}</a><br />", section.RawSectionID, section.Title);

                    lastNestingLevel = section.Level;
                }

            });

            // Clean up any lingering tables.
            if (lastNestingLevel > 2)
                sb.Append("</td></tr></table>");
            if (lastNestingLevel > 1)
                sb.Append("</td></tr></table>");

            return sb.ToString();
        }


        private List<ContentItemForCreating> CreatePDQCancerInfoSummaryLink(SummaryDocument document, string creationPath)
        {
            // Content items may be re-created during an update, therefore we must pass the create path from the caller.

            List<ContentItemForCreating> contentItemList = new List<ContentItemForCreating>();

            ContentItemForCreating contentItem =
                new ContentItemForCreating(CancerInfoSummaryLinkContentType, CreateFieldValueMapPDQCancerInfoSummaryLink(document), creationPath);
            contentItemList.Add(contentItem);

            return contentItemList;
        }

        private FieldSet CreateFieldValueMapPDQCancerInfoSummaryLink(SummaryDocument summary)
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

        private List<ContentItemForCreating> CreatePDQMediaLink(SummaryDocument document, string creationPath)
        {
            // Content items may be re-created during an update, therefore we must pass the create path from the caller.

            List<ContentItemForCreating> contentItemList = new List<ContentItemForCreating>();

            for (int i = 0; i < document.MediaLinkSectionList.Count; i++)
            {
                if (document.MediaLinkSectionList[i] != null)
                {
                    ContentItemForCreating contentItem =
                        new ContentItemForCreating(MediaLinkContentType, CreateFieldValueMapPDQMediaLink(document, document.MediaLinkSectionList[i], i + 1), creationPath);
                    contentItemList.Add(contentItem);
                }
            }

            return contentItemList;
        }

        private FieldSet CreateFieldValueMapPDQMediaLink(SummaryDocument summary, MediaLink mediaLink, int listOffset)
        {
            FieldSet fields = new FieldSet();
            fields.Add("inline_image_url", mediaLink.InlineImageUrl);
            fields.Add("inline_image_width", mediaLink.InlineImageWidth.ToString());

            fields.Add("popup_image_url", mediaLink.PopupImageUrl);
            fields.Add("popup_image_width", mediaLink.PopupImageWidth.ToString());

            if (string.IsNullOrEmpty(mediaLink.Caption))
            {
                fields.Add("caption_text", null);
            }
            else
            {
                fields.Add("caption_text", mediaLink.Caption);
            }
            fields.Add("long_description", mediaLink.Alt);
            fields.Add("pretty_url_name", "image" + listOffset);
            fields.Add("section_id", mediaLink.Id);
            fields.Add("sys_title", "image" + listOffset);

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
        private string GetTargetFolder(string prettyUrl)
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
        private string GetParentFolder(string prettyUrl)
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

        private string GetSummaryPrettyUrlName(string prettyUrl)
        {
            string prettyURLName = prettyUrl.Substring(prettyUrl.LastIndexOf('/') + 1);
            return prettyURLName;
        }


        // <summary>
        /// Taking care of the spaces around GlossaryTermRefLink
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        public void BuildGlossaryTermRefLink(ref string html, string tag)
        {
            // TODO:  This doesn't belong in the data layer!!!!
            string startTag = "<a Class=\"" + tag + "\"";
            string endTag = "</a>";
            int startIndex = html.IndexOf(startTag, 0);
            string sectionHTML = html;
            string collectHTML = string.Empty;
            string partC = string.Empty;
            while (startIndex >= 0)
            {
                string partA = sectionHTML.Substring(0, startIndex);
                string left = sectionHTML.Substring(startIndex);
                int endIndex = left.IndexOf(endTag) + endTag.Length;
                string partB = left.Substring(0, endIndex);
                partC = left.Substring(endIndex);

                // Combine
                // Do not add extra space after the GlossaryTermRef if following sign
                // is after the SummaryRef )}].,:;? " with )}].,:;? or space after it, ' with )]}.,:;? or space after it.
                if (Regex.IsMatch(partA.Trim(), "^[).,:;!?}]|^]|^\"[).,:;!?}\\s]|^\'[).,:;!?}\\s]|^\"]|^\']") || collectHTML.Length == 0)
                    collectHTML += partA.Trim();
                else
                    collectHTML += " " + partA.Trim();

                // Do not add extra space before the GlossaryTermRef if following sign is lead before the link: ({[ or open ' "
                if (Regex.IsMatch(collectHTML, "[({[/]$|[({[\\s]\'$|[({[\\s]\"$"))
                    collectHTML += partB;
                else
                    collectHTML += " " + partB;

                sectionHTML = partC.Trim();
                startIndex = sectionHTML.IndexOf(startTag, 0);
            }
            html = collectHTML + partC;
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

        private string SummarySectionIDAccessor(SummarySection section)
        {
            return section.RawSectionID;
        }

        private string MediaLinkIDAccessor(MediaLink link)
        {
            return link.Id;
        }
    }
}