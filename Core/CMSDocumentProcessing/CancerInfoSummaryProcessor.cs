using System;
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


namespace GKManagers.CMSDocumentProcessing
{
    public class CancerInfoSummaryProcessor : DocumentProcessorCommon, IDocumentProcessor, IDisposable
    {
        private PercussionConfig PercussionConfig;

        #region Constants

        const string PatientVersionLinkSlot = "pdqCancerInformationSummaryPatient";
        const string HealthProfVersionLinkSlot = "pdqCancerInformationSummaryHealthProf";
        const string SummaryPageSlot = "pdqCancerInformationSummaryPageSlot";
        const string InlineSlot = "sys_inline_variant";

        const string AudienceLinkSnippetTemplate = "pdqSnCancerInformationSummaryItemLink";
        const string AudienceTabSnippetTemplate = "pdqSnCancerInformationSummaryItemAudienceTab";

        const string languageEnglish = "en-us";
        const string languageSpanish = "es-us";

        // TODO: Seriously, the AudienceType string needs to be changed to an Enum.
        const string PatientAudience = "Patients";

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
                UpdateCancerInformationSummary(document);
            }


            InformationWriter(string.Format("Percussion processing completed for document CDRID = {0}.", document.DocumentID));
        }


        private void CreateNewCancerInformationSummary(SummaryDocument document)
        {
            List<long> idList;

            PercussionGuid summaryRoot;
            long[] summaryPageIDList;

            // TODO:  Move all this to a new CreateEmbeddedContentItems or similar method

            // Create the embeddable content items first so we can get their contentIDs.

            // Create TableSection content items.
            List<ContentItemForCreating> tableItemList = new List<ContentItemForCreating>();
            CreatePDQTableSections(document, tableItemList);
            List<long> tableItemIDs = CMSController.CreateContentItemList(tableItemList);
            SectionToCmsIDMap tableIDMap = BuildItemIDMap(document.TableSectionList, tableItemIDs);
            ResolveInlineSlots(document.SectionList, tableIDMap, "pdqSnTableSection");

            // Create MediaLink content items.
            List<ContentItemForCreating> mediaLinkItemList = new List<ContentItemForCreating>();
            CreatePDQMediaLink(document, mediaLinkItemList);
            List<long> mediaLinkItemIDs = CMSController.CreateContentItemList(mediaLinkItemList);
            SectionToCmsIDMap mediaLinkIDMap = BuildItemIDMap(document.MediaLinkSectionList, mediaLinkItemIDs);
            ResolveInlineSlots(document.SectionList, mediaLinkIDMap, "pdqSnMediaLink");


            // END TODO.


            // Create the Cancer Info Summary item.
            List<ContentItemForCreating> rootList = new List<ContentItemForCreating>();
            CreatePDQCancerInfoSummary(document, rootList);
            summaryRoot = new PercussionGuid(CMSController.CreateContentItemList(rootList)[0]);


            //Create Cancer Info Summary Page items
            List<ContentItemForCreating> summaryPageList = new List<ContentItemForCreating>();
            CreatePDQCancerInfoSummaryPage(document, summaryPageList);
            idList = CMSController.CreateContentItemList(summaryPageList);
            summaryPageIDList = idList.ToArray();

            // Add summary pages into the page slot.
            PSAaRelationship[] relationships = CMSController.CreateActiveAssemblyRelationships(summaryRoot.ID, summaryPageIDList, SummaryPageSlot, "pdqSnCancerInformationSummaryPage");


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
            if (searchList.Length==0)
            {
                // If a summary link doesn't exist, create a new one.
                List<ContentItemForCreating> summaryLinkList = new List<ContentItemForCreating>();
                CreatePDQCancerInfoSummaryLink(document, summaryLinkList);
                idList = CMSController.CreateContentItemList(summaryLinkList);
                summaryLink = new PercussionGuid(idList[0]);
                CMSController.CreateActiveAssemblyRelationships(summaryLink.ID, new long[] { summaryRoot.ID }, slotName, AudienceLinkSnippetTemplate);
            }
            else
            {
                // If the summary link does exist, add this summary to the appropriate slot.
                summaryLink = searchList[0];
                CMSController.CreateActiveAssemblyRelationships(summaryLink.ID, new long[] { summaryRoot.ID }, slotName, AudienceLinkSnippetTemplate);
            }

            // Find the Patient or Health Professional version and create a relationship.
            LinkToAlternateAudienceVersion(summaryRoot, document.AudienceType);

            LinkToAlternateLanguageVersion(document, summaryRoot);
        }

        private void UpdateCancerInformationSummary(SummaryDocument document)
        {
            // Stop until we're ready to work on update.
            throw new NotImplementedException("UpdateCancerInformationSummary");

            PercussionGuid documentCmsID = new PercussionGuid();

            //Update Content Items
            List<long> idList;

            List<ContentItemForUpdating> contentItemsListToUpdate = new List<ContentItemForUpdating>();
            long contentID;
            // Add pdqCancerInfoSummary content item to the contentItemsListToUpdate 
            ContentItemForUpdating updateContentItem = new ContentItemForUpdating(documentCmsID.ID, CreateFieldValueMapPDQCancerInfoSummary(document), GetTargetFolder(document.PrettyURL));
            contentItemsListToUpdate.Add(updateContentItem);


            //Add pdqCancerInfoSummaryLink content item to the contentItemsListToUpdate

            //Get the ID for the content item to be updated.
            contentID = GetpdqCancerInfoSummaryLinkID(document);

            updateContentItem = new ContentItemForUpdating(contentID, CreateFieldValueMapPDQCancerInfoSummaryLink(document), GetTargetFolder(document.PrettyURL));
            contentItemsListToUpdate.Add(updateContentItem);

            //Add pdqTableSections content item to the contentItemsListToUpdate
            GetPDQTableSectionsToUpdate(document, contentItemsListToUpdate);

            //Add pdqCancerInfoSummaryPages content item to the contentItemsListToUpdate
            GetPDQCancerInfoSummaryPagesToUpdate(document, contentItemsListToUpdate);


            InformationWriter(string.Format("Updating document CDRID = {0} in Percussion system.", document.DocumentID));

            //Update all the content Item in one operation
            idList = CMSController.UpdateContentItemList(contentItemsListToUpdate);

            //Check if the pdqCancerInfoSummary Pretty URL changed if yes then move the content item to the new folder in percussion.
            string prettyURL = GetTargetFolder(document.BasePrettyURL);
            //if (mappingInfo.PrettyURL != prettyURL)
            //{
            long[] id = idList.ToArray();
            CMSController.GuaranteeFolder(prettyURL);
            //CMSController.MoveContentItemFolder(mappingInfo.PrettyURL, prettyURL, id);

            ////Delete existing mapping for the CDRID.
            //mapManager.DeleteCdrIDMapping(document.DocumentID);

            //// Save the mapping between the CDR and CMS IDs.
            //mappingInfo = new CMSIDMapping(document.DocumentID, idList[0], document.PrettyURL);
            //mapManager.InsertCdrIDMapping(mappingInfo);

            //}

        }

        private SectionToCmsIDMap BuildItemIDMap(IList<SummarySection> sectionList, List<long> idList)
        {
            SectionToCmsIDMap itemIDMap = new SectionToCmsIDMap();

            int sectionCount = sectionList.Count();
            for (int i = 0; i < sectionCount; i++)
            {
                itemIDMap.AddSection(sectionList[i].RawSectionID, idList[i]);
            }

            return itemIDMap;
        }

        private SectionToCmsIDMap BuildItemIDMap(IList<MediaLink> mediaLinkList, List<long> idList)
        {
            SectionToCmsIDMap itemIDMap = new SectionToCmsIDMap();

            int mediaCount = mediaLinkList.Count();
            for (int i = 0; i < mediaCount; i++)
            {
                itemIDMap.AddSection(mediaLinkList[i].Reference, idList[i]);
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

                    XmlAttribute reference = attributeList["objectId"];
                    XmlAttribute attrib;

                    if (itemIDMap.ContainsSectionKey(reference.Value))
                    {
                        string target = reference.Value;
                        long dependent = PSItemUtils.GetID(itemIDMap[target]);

                        attrib = html.CreateAttribute("sys_dependentid");
                        attrib.Value = dependent.ToString();
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("contenteditable");
                        attrib.Value = "false";
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("rxinlineslot");
                        attrib.Value = "105";
                        attributeList.Append(attrib);

                        attrib = html.CreateAttribute("sys_dependentvariantid");
                        attrib.Value = snippetTemplate.ID.ToString();
                        attributeList.Append(attrib);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified Cancer Information Summary document is a Health Professional
        /// or Patient version, attempts to locate the alternate one, and if it exists, creates a relationship
        /// with it.
        /// </summary>
        /// <param name="documentID">Identifier for the content item to connect with its alternate version.</param>
        /// <param name="audienceType">Audience type value for the current content item.</param>
        private void LinkToAlternateAudienceVersion(PercussionGuid documentID, string audienceType)
        {
            // 1. What is the parent of this item? (Type is Cancer Information Summary Link)
            PSItem[] parentItems = CMSController.LoadLinkingContentItems(documentID.ID);
            PSItem summaryLink =
                Array.Find(parentItems, item => item.contentType == CancerInfoSummaryLinkContentType);
            if (summaryLink == null)
                throw new CMSOperationalException(string.Format("Cannot locate CancerInfoSummaryLink for content item {0}", documentID.ToString()));
            
            PercussionGuid summaryLinkID= new PercussionGuid(summaryLink.id);
            

            // 2. Search the link item for child items in the other audience type's slot.
            string theirSlotName;   // Slot to search for alternate version.
            string mySlotName;      // Slot to store this item in.
            if (audienceType.Equals(PatientAudience, StringComparison.InvariantCultureIgnoreCase))
            {
                mySlotName = PatientVersionLinkSlot;
                theirSlotName = HealthProfVersionLinkSlot;
            }
            else
            {
                mySlotName = HealthProfVersionLinkSlot;
                theirSlotName = PatientVersionLinkSlot;
            }

            PercussionGuid[] otherAudienceVersion =
                CMSController.SearchForItemsInSlot(summaryLinkID, theirSlotName);

            // 3. If the slot contains a content item, it must be of the opposite audience type.
            if (otherAudienceVersion.Length > 0)
            {
                // Link from this item to the alternate version.
                CMSController.CreateActiveAssemblyRelationships(documentID.ID, new long[] { otherAudienceVersion[0].ID }, theirSlotName, AudienceTabSnippetTemplate);

                // Link from the alternate version back to this one.
                // TODO: Delete any existing relationships in that slot.
                CMSController.CreateActiveAssemblyRelationships(otherAudienceVersion[0].ID, new long[] { documentID.ID }, mySlotName, AudienceTabSnippetTemplate);
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

                PercussionGuid englishItem = GetCdrDocumentID(CancerInfoSummaryContentType, relationship.RelatedSummaryID);
                if (englishItem == null)
                    throw new CMSOperationalException(string.Format("Document {0} must exist before its translation may be added.", relationship.RelatedSummaryID));

                CMSController.CreateRelationship(rootID.ID, englishItem.ID, CMSController.TranslationRelationshipType);
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
                PercussionGuid summaryLink = LocateSummaryLink(rootItem);
                PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(rootItem, SummaryPageSlot);
                PercussionGuid[] subItems = LocateMediaLinksAndTableSections(pageIDs); // Table sections and MediaLinks.

                // Creat a list of all content IDs making up the document.
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
        private PercussionGuid LocateSummaryLink(PercussionGuid rootItemID)
        {
            // TODO: Rewrite this to find all incoming Active Assembly relationships
            // which use the Patient or HealtProfessional slots with the link template.

            PercussionGuid summaryLinkID = null;

            PSItem[] rootItem = CMSController.LoadContentItems(new long[] { rootItemID.ID });
            string linkPath = GetParentFolder(CMSController.GetPathInSite(rootItem[0]));
            PercussionGuid[] searchList =
                CMSController.SearchForContentItems(CancerInfoSummaryLinkContentType, linkPath, null);

            // It would be very weird to not find a summary link. Let the caller decide how to handle that.
            if (searchList != null)
            {
                // There should only be one item found. Verify that it's the one we want.
                // Is the root ID in the patient slot?
                PercussionGuid[] foundIDs = CMSController.SearchForItemsInSlot(searchList[0], PatientVersionLinkSlot);
                if (foundIDs != null && foundIDs.Length > 0 && foundIDs[0] == rootItemID)
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
            Array.ForEach(pageIDList, pageID =>
            {
                PercussionGuid[] items = CMSController.SearchForItemsInSlot(pageID, InlineSlot);
                subItems.AddRange(items);
            });

            return subItems.ToArray();
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

            PercussionGuid[] rootIDArray = new PercussionGuid[]{rootItemID};
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

        private void CreatePDQCancerInfoSummaryPage(SummaryDocument document, List<ContentItemForCreating> contentItemList)
        {
            int i;

            for (i = 0; i <= document.SectionList.Count - 1; i++)
            {
                if (document.SectionList[i].IsTopLevel == true)
                {
                    ContentItemForCreating contentItem = new ContentItemForCreating(CreateFieldValueMapPDQCancerInfoSummaryPage(document, document.SectionList[i]), GetTargetFolder(document.BasePrettyURL), CancerInfoSummaryPageContentType);
                    contentItemList.Add(contentItem);
                }

            }

        }

        private Dictionary<string, string> CreateFieldValueMapPDQCancerInfoSummaryPage(SummaryDocument summary, SummarySection summarySection)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string html = summarySection.Html.OuterXml;

            string prettyURLName = summarySection.PrettyUrl.Substring(summarySection.PrettyUrl.LastIndexOf('/') + 1);
            if (summarySection.Html.OuterXml.Contains("<SummaryRef"))
            {
                BuildSummaryRefLink(ref html, 0);
            }

            // TODO: Move Summary-GlossaryTermRef Extract/Render out of the data access layer!
            if (summarySection.Html.OuterXml.Contains("Summary-GlossaryTermRef"))
            {
                string glossaryTermTag = "Summary-GlossaryTermRef";
                BuildGlossaryTermRefLink(ref html, glossaryTermTag);
            }

            // TODO: Cleanup MediaHTML and TableSectionXML before we get this far.
            fields.Add("bodyfield", html.Replace("<MediaHTML>", string.Empty).Replace("</MediaHTML>", string.Empty).Replace("<TableSectionXML>", string.Empty).Replace("</TableSectionXML>", string.Empty));
            fields.Add("long_title", summarySection.Title);
            fields.Add("sys_title", summarySection.Title);

            // HACK: This relies on Percussion not setting anything else in the login session.
            fields.Add("sys_lang", GetLanguageCode(summary.Language));

            return fields;
        }


        private void CreatePDQTableSections(SummaryDocument document, List<ContentItemForCreating> contentItemList)
        {
            int i;


            for (i = 0; i <= document.TableSectionList.Count - 1; i++)
            {
                ContentItemForCreating contentItem = new ContentItemForCreating(CreateFieldValueMapPDQTableSection(document, document.TableSectionList[i]), GetTargetFolder(document.BasePrettyURL), TableSectionContentType);
                contentItemList.Add(contentItem);
            }

        }

        private Dictionary<string, string>  CreateFieldValueMapPDQTableSection(SummaryDocument summary, SummarySection tableSection)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string prettyURLName = tableSection.PrettyUrl.Substring(tableSection.PrettyUrl.LastIndexOf('/') + 1);

            fields.Add("pretty_url_name", prettyURLName);

            // The long_title field is required, but the DTD for table sections doesn't require them.
            // If there is no title, fill it in, but make it hidden.
            if (string.IsNullOrEmpty(tableSection.Title))
            {
                fields.Add("long_title", "Section: " + tableSection.RawSectionID);
                fields.Add("showpagetitle", "false");
            }
            else
            {
                fields.Add("long_title", tableSection.Title);
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



        private void CreatePDQCancerInfoSummary(SummaryDocument document, List<ContentItemForCreating> contentItemList)
        {
            ContentItemForCreating contentItem = new ContentItemForCreating(CreateFieldValueMapPDQCancerInfoSummary(document), GetTargetFolder(document.BasePrettyURL), CancerInfoSummaryContentType);
            contentItemList.Add(contentItem);
        }

        private Dictionary<string, string> CreateFieldValueMapPDQCancerInfoSummary(SummaryDocument summary)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            string prettyURLName = summary.BasePrettyURL.Substring(summary.BasePrettyURL.LastIndexOf('/') + 1);

            string TOC = BuildTableOfContents(summary);

            fields.Add("pretty_url_name", prettyURLName);
            fields.Add("long_title", summary.Title);

            if (summary.Title.Length > 64)
                fields.Add("short_title", summary.Title.Substring(1, 64));
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


        private void CreatePDQCancerInfoSummaryLink(SummaryDocument document, List<ContentItemForCreating> contentItemList)
        {
            ContentItemForCreating contentItem =
                new ContentItemForCreating(CreateFieldValueMapPDQCancerInfoSummaryLink(document),
                    GetParentFolder(document.BasePrettyURL), CancerInfoSummaryLinkContentType);
            contentItemList.Add(contentItem);
        }

        private Dictionary<string, string> CreateFieldValueMapPDQCancerInfoSummaryLink(SummaryDocument summary)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();

            fields.Add("sys_title", summary.ShortTitle);
            fields.Add("long_title", summary.Title);
            fields.Add("short_title", summary.ShortTitle);
            fields.Add("long_description", summary.Description);

            // HACK: This relies on Percussion not setting anything else in the login session.
            fields.Add("sys_lang", GetLanguageCode(summary.Language));

            return fields;

        }

        private void CreatePDQMediaLink(SummaryDocument document, List<ContentItemForCreating> contentItemList)
        {
            int i;

            for (i = 0; i <= document.MediaLinkSectionList.Count - 1; i++)
            {
                if (document.MediaLinkSectionList[i] != null)
                {
                    ContentItemForCreating contentItem = new ContentItemForCreating(CreateFieldValueMapPDQMediaLink(document, document.MediaLinkSectionList[i], i + 1), GetTargetFolder(document.BasePrettyURL), MediaLinkContentType);
                    contentItemList.Add(contentItem);
                }

            }

        }

        private Dictionary<string, string> CreateFieldValueMapPDQMediaLink(SummaryDocument summary, MediaLink mediaLink, int listOffset)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            fields.Add("inline_image_url", mediaLink.InlineImageUrl);
            fields.Add("popup_image_url", mediaLink.PopupImageUrl);
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

        private long GetpdqCancerInfoSummaryLinkID(SummaryDocument document)
        {
            throw new NotImplementedException("GetpdqCancerInfoSummaryLinkID");
            //long contentid;
            //contentid = CMSController.GetItemID(GetTargetFolder(document.BasePrettyURL), document.Title);
            //return contentid;
        }


        private void GetPDQTableSectionsToUpdate(SummaryDocument document, List<ContentItemForUpdating> contentItemsListToUpdate)
        {
            throw new NotImplementedException("GetPDQTableSectionsToUpdate");

            //int i;
            //long contentid;

            //for (i = 0; i <= document.TableSectionList.Count - 1; i++)
            //{
            //    string prettyURLName = document.TableSectionList[i].PrettyUrl.Substring(document.TableSectionList[i].PrettyUrl.LastIndexOf('/') + 1);
            //    contentid = CMSController.GetItemID(GetTargetFolder(document.TableSectionList[i].PrettyUrl), prettyURLName);
            //    ContentItemForUpdating updateContentItem = new ContentItemForUpdating(contentid, CreateFieldValueMapPDQTableSection(document.TableSectionList[i]), GetTargetFolder(document.PrettyURL));
            //    contentItemsListToUpdate.Add(updateContentItem);
            //}

        }

        private void GetPDQCancerInfoSummaryPagesToUpdate(SummaryDocument document, List<ContentItemForUpdating> contentItemsListToUpdate)
        {
            throw new NotImplementedException("GetPDQTableSectionsToUpdate");

            //int i;
            //long contentid;

            //for (i = 0; i <= document.SectionList.Count - 1; i++)
            //{
            //    string prettyURLName = document.SectionList[i].PrettyUrl.Substring(document.SectionList[i].PrettyUrl.LastIndexOf('/') + 1);
            //    contentid = CMSController.GetItemID(GetTargetFolder(document.SectionList[i].PrettyUrl), prettyURLName);
            //    ContentItemForUpdating updateContentItem = new ContentItemForUpdating(contentid, CreateFieldValueMapPDQTableSection(document.SectionList[i]), GetTargetFolder(document.PrettyURL));
            //    contentItemsListToUpdate.Add(updateContentItem);

            //}

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



        private void BuildSummaryRefLink(ref string html, int isGlossary)
        {
            string startTag = "<SummaryRef";
            string endTag = "</SummaryRef>";
            int startIndex = html.IndexOf(startTag, 0);
            string sectionHTML = html;
            while (startIndex >= 0)
            {
                // Devide the whole piece of string into three parts: a= first part; b = "<summaryref href="CDR0012342" url="/cander_topic/...HP/>..</summaryref>"; c = last part
                int endIndex = sectionHTML.IndexOf(endTag) + endTag.Length;
                string partA = sectionHTML.Substring(0, startIndex);
                string partB = sectionHTML.Substring(startIndex, endIndex - startIndex);
                string partC = sectionHTML.Substring(endIndex);

                // Process partB
                // Get the href, url, text between the tag
                XmlDocument refDoc = new XmlDocument();
                refDoc.LoadXml(partB);
                XPathNavigator xNav = refDoc.CreateNavigator();
                XPathNavigator link = xNav.SelectSingleNode("/SummaryRef");
                string text = link.InnerXml;
                string href = link.GetAttribute("href", string.Empty);
                string url = link.GetAttribute("url", string.Empty).Trim();
                if (url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1);
                }

                // The following code is preserved just in case in the future we need to support
                // prettyURL links in CDRPreview web service.
                // Get prettyurl server if the PrettyURLController is not reside on the same server
                // This is used for CDRPreview web service, GateKeeper should not have this setting.
                // string prettyURLServer = ConfigurationManager.AppSettings["PrettyUrlServer"];
                //if (prettyURLServer != null && prettyURLServer.Trim().Length > 0)
                //    url = prettyURLServer + url;

                // Get the section ID in href
                int index = href.IndexOf("#");
                string sectionID = string.Empty;
                string prettyURL = url;
                if (index > 0)
                {
                    sectionID = href.Substring(index + 2);
                    prettyURL = url + "/" + sectionID + ".cdr#Section_" + sectionID;
                }

                //Create new link string
                if (prettyURL.Trim().Length > 0)
                {
                    // The click on the summary link in the GlossaryTerm will open a new browser for summary document
                    if (isGlossary == 1)
                        partB = "<a class=\"SummaryRef\" href=\"" + prettyURL + "\" target=\"new\">" + text + "</a>";
                    else
                        partB = "<a class=\"SummaryRef\" href=\"" + prettyURL + "\">" + text + "</a>";
                }
                else
                {
                    throw new Exception("Retrieving SummaryRef url failed. SummaryRef=" + partB + ".");
                }

                // Combine
                // Do not add extra space before the SummaryRef if following sign is lead before the link: ({[ or open ' "
                if (Regex.IsMatch(partA.Trim(), "[({[/]$|[({[\\s]\'$|[({[\\s]\"$"))
                    sectionHTML = partA.Trim() + partB;
                else
                    sectionHTML = partA.Trim() + " " + partB;

                // Do not add extra space after the SummaryRef if following sign
                // is after the SummaryRef )}].,:;? " with )}].,:;? or space after it, ' with )]}.,:;? or space after it.
                if (Regex.IsMatch(partC.Trim(), "^[).,:;!?}]|^]|^\"[).,:;!?}\\s]|^\'[).,:;!?}\\s]|^\"]|^\']"))
                    sectionHTML += partC.Trim();
                else
                    sectionHTML += " " + partC.Trim();

                startIndex = sectionHTML.IndexOf(startTag, 0);
            }
            html = sectionHTML;
        }

        // <summary>
        /// Taking care of the spaces around GlossaryTermRefLink
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        public void BuildGlossaryTermRefLink(ref string html, string tag)
        {
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
                    languageCode= languageSpanish;
                    break;
                case Language.English:
                default:
                    languageCode= languageEnglish;
                    break;
            }

            return languageCode;
        }
    }
}
