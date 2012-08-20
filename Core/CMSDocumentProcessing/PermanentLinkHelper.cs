using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GateKeeper.Common;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.CMSDocumentProcessing.Configuration;

using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

using GKManagers.CMSDocumentProcessing;

using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Media;
using GKManagers.CMSManager.Configuration;

using NCI.WCM.CMSManager;

namespace GKManagers.CMSDocumentProcessing
{
    public class PermanentLinkHelper
    {

        #region Permanent Link Constants

        // LinkID, TargetID, & LongTitle are used for translating existing Permanent Link values into
        // GateKeeper format (PermanentLink Class).
        // All PercussionFields are used in the CreateFieldValueMapPDQPermanentLink
        //
        // List of Percussion Fields is alphabetical
        private const string PercussionFieldLinkID = "link_id";
        private const string PercussionFieldLongTitle = "long_title";
        private const string PercussionFieldShortTitle = "short_title";
        private const string PercussionFieldSystemTitle = "sys_title";
        private const string PercussionFieldTargetID = "target_id";
        private const string PercussionFieldTargetURL = "target_url";

        // Set of ints that define the maximum values that can be used in Percussion Fields
        // This is due to a limit set by Percussion and an error will be thrown if it is exceeded
        // Values used in CreateFieldValueMapPDQPermanentLink
        //
        // List of Percussion Fields is alphabetical
        private const int PercussionFieldMaxLongTitle = 240;
        private const int PercussionFieldMaxShortTitle = 90;
        private const int PercussionFieldMaxSystemTitle = 240;


        // URL Constants are for creating/updating the URL string that is assigned to each PermanentLink
        // And then translated into a usable URL for Percussion and the Web
        private const string URLConstantPage = "Page";
        private const string URLConstantSection = "#Section";

        #endregion



        #region Constructor Set Required Values

        // Percussion control object, shared among all Document Processor types
        // derived from DocumentProcessorCommon
        // Passed in by CancerInfoSummaryProcessors (And code copied from)
        protected readonly CMSController CMSController;

        // PercussionConfig type
        private readonly PercussionConfig PercussionConfig;
        // The particular PercussionConfig type that we need
        private readonly string PermanentLinkContentType;


        #endregion



        #region Sorted Links
        // Must be set in the constructor and then called separately in various functions 
        // as a document progresses through it's creation, update, or deletion.

        // List of links after they're marked for Create, Update, or Delete
        private List<PermanentLink> toCreatePermanentLinks = new List<PermanentLink>();
        private List<PermanentLink> toUpdatePermanentLinks = new List<PermanentLink>();
        private List<PermanentLink> toDeletePermanentLinks = new List<PermanentLink>();

        #endregion




        #region Constructors

        /// <summary>
        /// This Constructor is used when Updating an existing document in Staging mode.
        /// This Constructor is also used when Creating a new Summary document.
        /// </summary>
        /// <param name="cmsController">Uses the same CMSController as other CISProcessors.</param>
        /// <param name="incomingPermanentLinks">List of the incoming Permanent Links that will be needed to 
        /// renconciled with discovered old Permanent Links so we know which links will be marked for Create, 
        /// Update, or Delete.</param>
        /// <param name="summaryPath">Folder path in which we can find the summary's main existence. 
        /// Down to Patient / Health Professional level. Where the Permanent Links are located.</param>
        /// <remarks>A brand new Summary document can be handled the same way, as far as this Processor is 
        /// concerned, as a Summary that's being updated, but has no exisiting Permanent Links.</remarks>
        public PermanentLinkHelper(CMSController cmsController, List<PermanentLink> incomingPermanentLinks, string summaryPath)
        {
            // Set the variables that are required for the processor from incoming values

            // Set the PermanentLinkContentType
            // This is copied from CancerInfoSummaryCommon
            PercussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            PermanentLinkContentType = PercussionConfig.ContentType.PDQPermanentLink.Value;

            // Throw errors if nothing is set
            CMSController = cmsController;
            if (CMSController == null)
            {
                throw new NullReferenceException("CMSController is not defined.");
            }
            
            // Setup is required to reconcile the (possible) incoming Permanent Links with all of the
            // (possible) existing Permanent Links
            // A Deep Copy of the incoming Permanent Links is passed to prevent any modification
            PermanentLinkSetup(summaryPath, new List<PermanentLink>(incomingPermanentLinks));
        }

        /// <summary>
        /// This Constructor is used when Updating an existing document in Preview and Live modes. 
        /// This instance only worries about detecting Permanent Links in Percussion.
        /// </summary>
        /// <param name="cmsController">Uses the same CMSController as other CISProcessors.</param>
        /// <param name="summaryPath">Only the location in which to look for the Permanent Links 
        /// is necessary.</param>
        /// <remarks>Preview & Live only need existing links, so there is no need for any function here
        /// other than LocateExistingPermanentLinks().</remarks>
        public PermanentLinkHelper(CMSController cmsController, string summaryPath)
        {
            // Set the PermanentLinkContentType
            // This is copied from CancerInfoSummaryCommon
            PercussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");
            PermanentLinkContentType = PercussionConfig.ContentType.PDQPermanentLink.Value;

            // Set the variables that are required for the processor from incoming values
            // Throw errors if nothing is set
            CMSController = cmsController;
            if (CMSController == null)
            {
                throw new NullReferenceException("CMSController is not defined.");
            }
  

            // Locate is only needed for this type as it just really is detecting old guids in the folder
            LocateExistingPermanentLinks(summaryPath);
        }

        #endregion


        #region Existing Guids

        // The existing Guids are frequently used
        private PercussionGuid[] _existingGuids;
        /// <summary>
        /// Returns the Old Guids if needed by a function outside of this processor.
        /// Will return a Array of size 0 instead of a null function for safety reasons.
        /// </summary>
        /// <remarks>Required for LocateExistingLinks in the CancerInfoSummaryCommon, which helps 
        /// promote the (old) guids from stage to stage.</remarks>
        public PercussionGuid[] GetOldGuids
        {
            get
            {
                if (_existingGuids == null)
                    return new PercussionGuid[0];
                else
                    return _existingGuids;
            }

            private set
            {
                _existingGuids = value;
            }
        }

        #endregion



        /// <summary>
        /// Locates existing Permanent Links and reconciles that with incoming Permanent Links in order to 
        /// set the correct Create, Update, and Delete lists attached to this Processor.
        /// </summary>
        /// <param name="incomingPermanentLinks">Incoming Links to compare against Existing Links.</param>
        /// <param name="summaryPath">Where should we search for Existing Links?</param>
        private void PermanentLinkSetup(string summaryPath, List<PermanentLink> incomingPermanentLinks)
        {
            // First locate any possibly existing Permanent Links
            PercussionGuid[] guidsForExistingPermanentLinks = LocateExistingPermanentLinks(summaryPath);

            // If there were existing Permanent Link Guids found, then there are existing Permanent Links
            // that must be processed to a GateKeeper comparable format
            if (guidsForExistingPermanentLinks.Length > 0)
            {
                // Sets GateKeeper recognized instances (PermanentLink Class) using the Guids
                // that are stored in _oldGuids
                List<PermanentLink> existingPermanentLinks = SetExistingPermanentLinks(guidsForExistingPermanentLinks);

                // Reconcile the list of Incoming Permanent Links, defined in the constructor from
                // the Extract class VERSUS the Existing Permanent Links as discovered by
                // LocateExistingPermanentLinks and set by SetExistingPermanentLinks
                // Produces the list of Permanent Links to be Created, to be Updated, & to be Deleted
                SortPermanentLinks(incomingPermanentLinks, existingPermanentLinks);
            }
            // If no existing Guids were found, then this is either an entirely new summary or a summary
            // that is being updated and has completely new links
            else
            {
                // A completely new set of links does not need to worry about reconciling an old list
                // with a new list as all incoming links are marked for creation
                AllNewPermanentLinksSetup(incomingPermanentLinks);
            }
        }



        /// <summary>
        /// This handles the case of an entirely new Summary Document that is being created. Or, just 
        /// a set of Permanent Links where no Permanent Links exist currently.
        /// </summary>
        /// <param name="incomingPermanentLinks">Links that are incoming should be marked for creation.</param>
        private void AllNewPermanentLinksSetup(List<PermanentLink> incomingPermanentLinks)
        {
            // All incoming links are for creation
            // No links are going to be updated or deleted as there are no existing links
            toCreatePermanentLinks = incomingPermanentLinks;
        }



        /// <summary>
        /// Finds exisiting Permanent Links in Percussion using the stored SummaryPath.
        /// </summary>
        /// <param name="summaryPath">Where should we look for Existing Links?</param>
        /// <returns>The PercussionGuids of the Permanent Links we found.</returns>
        /// <remarks>Used to take a string as an item path.
        /// Originally returned a list of exisiting Percussion identifying Guids is returned.</remarks>
        private PercussionGuid[] LocateExistingPermanentLinks(string summaryPath)
        {
            // Create a link instance so it's easy to add found instances
            List<PercussionGuid> oldPermanentLinkGuids = new List<PercussionGuid>();

            // If the item path is null, then the content item has no path in *this* site.
            // Therefore, there is no summary link to search for.
            if (!string.IsNullOrEmpty(summaryPath))
            {
                PSItemSummary[] folderKids = CMSController.FindFolderChildren(summaryPath);
                foreach (PSItemSummary kid in folderKids)
                {
                    if (kid.ContentType != null)
                    {
                        if (kid.ContentType.name.Equals(PermanentLinkContentType))
                        {
                            oldPermanentLinkGuids.Add(new PercussionGuid(kid.id));
                        }
                    }
                }
            }
            else
            {
                throw new NullReferenceException("Summary Path was empty and therefore cannot find any children.");
            }
            
            // Update the Existing Guids values so that it may be called other places
            _existingGuids = oldPermanentLinkGuids.ToArray();

            // Return array for evaluation in the Link Setup
            return oldPermanentLinkGuids.ToArray();
        }



        /// <summary>
        /// Turns the existing Permanent Links into something that GateKeeper can recognize.
        /// </summary>
        /// <param name="existingGuids">Existing Guids to be turned into GateKeeper Permanent Links.</param>
        /// <returns>List of the Existing Permanent Links in a GateKeeper recognized format.</returns>
        /// <remarks>Used to return existing Permanent Links in GateKeeper recognized form.
        /// Used to take Set of Percussion identified Guids; Now pulls existing stored values.</remarks>
        private List<PermanentLink> SetExistingPermanentLinks(PercussionGuid[] existingGuids)
        {
            List<PermanentLink> existingPermanentLinks = new List<PermanentLink>();

            // This is the information that is currently stored in Percussion and we would like easily
            // readable in GateKeeper format (PermanentLink Class)
            string linkID, targetID, targetTitle;
            PercussionGuid guid;

            // Load all of the Guids as previously identified so that we may extract this information
            PSItem[] loadedOldPermanentLinks = CMSController.LoadContentItems(existingGuids);
            // Placeholder for loaded item
            PSItem loadedItem = null;

            // Loop through all Percussion loaded Permanent Link items
            for (int i = 0; i < loadedOldPermanentLinks.Length; i++)
            {
                // Set the placeholder
                loadedItem = loadedOldPermanentLinks[i];

                // If something was actually loaded, set all the values and add it to the list of existing links
                if (loadedItem != null)
                {
                    // Grab the values for the PermanentLink specific Percussion fields
                    linkID = PSItemUtils.GetFieldValue(loadedItem, PercussionFieldLinkID);
                    targetID = PSItemUtils.GetFieldValue(loadedItem, PercussionFieldTargetID);

                    // Use the Percussion field "long_tile" to set the title as it has the highest limit
                    // for characters so you most likely get the best/longest value
                    targetTitle = PSItemUtils.GetFieldValue(loadedItem, PercussionFieldLongTitle);

                    // Guid is not stored as one of the Percussion fields, but is a by product
                    guid = existingGuids[i];

                    // Internally add this to the set of existing Permanent Links in GateKeeper format (Permanent Link Class)
                    existingPermanentLinks.Add(new PermanentLink(linkID, targetID, targetTitle, guid.Guid));
                }
            }
            return existingPermanentLinks;
        }



        /// <summary>
        /// Reconciles the list of incoming Permanent Links with existing Permanent Links in order to 
        /// discover which Permanent Links should be marked for Create vs Update vs Delete.
        /// </summary>
        /// <param name="existingEvaluationList">Existing Permanent Links</param>
        /// <param name="incomingEvaluationList">Incoming Permanent Links</param>
        public void SortPermanentLinks(List<PermanentLink> incomingEvaluationList, List<PermanentLink> existingEvaluationList)
        {
            // Examine the incoming list of Permanent Links
            foreach (PermanentLink link in incomingEvaluationList)
            {
                // If we cannot find this link in the list of existing links, then it must be new
                if (existingEvaluationList.IndexOf(link) < 0)
                {
                    toCreatePermanentLinks.Add(link);
                }
                // If we found this link in the list of existing links, then we're updating it
                else
                {
                    // Before we add the link to the marked for update list, let's set it's Guid
                    // It's Guid is only set in the Existing Link Set, so we must get it's Guid from there
                    // We cannot simply add the entire link from the Existing Link Set because the link
                    // instance from the Incoming Link Set has the potentially new/updated values for a
                    // title, sectionID, etc
                    PermanentLink temp = existingEvaluationList.Find(item => item.ID.Equals(link.ID));
                    link.Guid = temp.Guid;
                    toUpdatePermanentLinks.Add(link);
                    
                    // Since this link has been processed and marked for updating, let's remove it from
                    // consideration in the Existing link set evaluation
                    existingEvaluationList.Remove(link);
                }
            }
            
            // Examine the remaining list of existing Permanent Links
            // Since items marked for Update have already been removed, everything remaining should be
            // marked for deletion
            foreach (PermanentLink oldPermanentLink in existingEvaluationList)
            {
                toDeletePermanentLinks.Add(oldPermanentLink);
            }

            /////
            // No need to return anything as lists are getting modified and stored already
            /////
        }



        /// <summary>
        /// Updates/Instantiates the URLs for Permanent Links that have been marked as To Create or as To Update. 
        /// This must be called after sections have been placed on the correct pages in order to get 
        /// the proper URL.
        /// </summary>
        /// <param name="newPageIDs">The newest page IDs used to find where sections are located.</param>
        /// <remarks>Must be public as it is called outside of this PermanentLinkProcessor in other
        /// Processors once the New Page/Section IDs have been determined.</remarks>
        public void SetURLs(PercussionGuid[] newPageIDs, List<SummarySection> topLevelSections)
        {
            // Use the same finder on both sets of links
            CancerInfoSummarySectionFinder finder = new StandardSummarySectionFinder(CMSController);

            // Convert the Top Level Sections into a String of IDs so they may be more easily compared
            //List<String> topLevelSectionIDs = topLevelSections.ConvertAll(new Converter<SummarySection, String>(SummarySection.SectionByID));
            HashSet<String> topLevelSectionIDs = new HashSet<String>(topLevelSections.ConvertAll(new Converter<SummarySection, String>(SummarySection.SectionByID)));


            // Same process for determing URLs for brand new links as well as the old links
            // So call the same function on each list
            SetURLs(finder, newPageIDs, topLevelSectionIDs, toCreatePermanentLinks);
            SetURLs(finder, newPageIDs, topLevelSectionIDs, toUpdatePermanentLinks);
        }



        /// <summary>
        /// Updates the URLS for Permanent Links given a list.
        /// </summary>
        /// <param name="finder">SectionFinder object that will be used to find pages.</param>
        /// <param name="newPageIDs">The newest page IDs used to find where sections are located.</param>
        /// <param name="permanentLinks">The list of Permanent Links that will have its URLs modified.</param>
        /// <remarks>This function was created as a helper function to the public UpdateURLs as the 
        /// same functionality was required for two different lists.</remarks>
        private void SetURLs(CancerInfoSummarySectionFinder finder, PercussionGuid[] newPageIDs, HashSet<String> topLevelSectionIDs, List<PermanentLink> permanentLinks)
        {
            SummaryPageInfo temporaryInfo;
            string newURL;
            foreach (PermanentLink link in permanentLinks)
            {
                // First find the page containing this Section ID
                temporaryInfo = finder.FindPageContainingSection(newPageIDs, link.SectionID);

                // Current URL schema is "Page<#>#Section<_#>
                // Constant are defined at the top of the function with other Permanent Link Constants
                // If it is a top level section, it should only be Page<#>
                newURL = URLConstantPage + temporaryInfo.PageNumber;
                // If it is deeper than a top level section, it should be Page<#>#Section<#>
                if (!topLevelSectionIDs.Contains(link.SectionID))
                {
                    newURL += URLConstantSection + link.SectionID;
                }

                // Assign the new URL
                // No need to check for errors because FindPageContainingSection will throw an error
                // if the Section ID cannot be found.
                link.Url = newURL;
            }
        }



        /// <summary>
        /// Creates the set of Fields that are required in Percussion. This must be used to create new 
        /// Permanent Links in Percussion and to update values for links as well.
        /// </summary>
        /// <param name="linkID">Permanent Link identifying number.</param>
        /// <param name="targetID">Permanent Link's target section.</param>
        /// <param name="targetURL">URL to Permanent Link's target section. Should be an updated value 
        /// after the new page IDs have been discovered.</param>
        /// <param name="title">Title of the Permanent Link. Most likely the title of the section it links
        /// to as well based on 6.5 Design & discussions.</param>
        /// <returns>Set of Fields that Percussion requires with GateKeeper stored values.</returns>
        private FieldSet CreateFieldValueMapPDQPermanentLink(string linkID, string targetID, string targetURL, string title)
        {
            // Collection of fields that will be used and added
            FieldSet fields = new FieldSet();

            //ToDo: Add a checker for link other max's? Necessary?

            // Percussion Field Constants and Percussion Field Max Constants are defined at the beginning
            // of the Processor under PermanentLinkConstants
            fields.Add(PercussionFieldSystemTitle, title.Substring(0, Math.Min(title.Length, PercussionFieldMaxSystemTitle)));
            fields.Add(PercussionFieldLinkID, linkID);
            fields.Add(PercussionFieldTargetID, targetID);
            fields.Add(PercussionFieldTargetURL, targetURL);
            fields.Add(PercussionFieldLongTitle, title.Substring(0, Math.Min(title.Length, PercussionFieldMaxLongTitle)));
            fields.Add(PercussionFieldShortTitle, title.Substring(0, Math.Min(title.Length, PercussionFieldMaxShortTitle)));

            return fields;
        }



        /// <summary>
        /// Creates Permanent Links in Percussion under the given folder path based off
        /// what is stored in the Permanent Link Processor. Returns (long) guids for
        /// rollback list.
        /// </summary>
        /// <param name="creationPath">Indicates the folder path that the Permanent Links
        /// should be created in.</param>
        /// <returns>Returns the Guids of the created Permanent Links in long formats. 
        /// This return is needed for the Rollback List.</returns>
        public List<long> CreatePermanentLinks(string creationPath)
        {
            // List of content items that we will create
            List<ContentItemForCreating> listOfContentForCreating = new List<ContentItemForCreating>();

            // Placeholder for a content item that we will create
            ContentItemForCreating cmi;
            // Placeholder for the temporary PermanentLink
            PermanentLink newPermanentLink;

            // A list of Guids that corresponds to the list of content we are creating
            // Needed for the return value
            List<long> guidList = new List<long>();

            // For every Permanent Link that is marked for creation,
            for (int i = 0; i < toCreatePermanentLinks.Count; i++)
            {
                // Set the placeholder for the Permanent Link
                newPermanentLink = toCreatePermanentLinks[i];

                // Create the content item that will be updated and set the placeholder for it
                cmi = new ContentItemForCreating(PermanentLinkContentType,
                    CreateFieldValueMapPDQPermanentLink(newPermanentLink.ID, newPermanentLink.SectionID, newPermanentLink.Url, newPermanentLink.Title), creationPath);

                // Add the new values to our list
                listOfContentForCreating.Add(cmi);
            }

            // Create all of the values and store the new Guids in a list
            guidList = CMSController.CreateContentItemList(listOfContentForCreating);

            // For every Permanent Link that we created, update its corresponding Guid in the creation marked list
            for (int i = 0; i < toCreatePermanentLinks.Count; i++)
            {
                toCreatePermanentLinks[i].Guid = guidList[i];
            }

            return guidList;
        }



        /// <summary>
        /// Updates the Permanent Links in Percussion, which have been marked for an update 
        /// in the Permanent Link Processor. 
        /// </summary>
        /// <remarks>Must be public as it is called from outside this Processor. Is called outside the
        /// try/catch loop in Updating an existing Summary.</remarks>
        public void UpdatePermanentLinks()
        {
            // Placeholder for the populated fieldSet we get for each link
            FieldSet updatedFields;

            // List of all the items in their updated state
            List<ContentItemForUpdating> contentForUpdatingList = new List<ContentItemForUpdating>();

            // For every link marked for updating
            foreach (PermanentLink link in toUpdatePermanentLinks)
            {
                // Update the placeholder with the FieldSet from CreateFieldValueMapPDQPermanentLink
                updatedFields = CreateFieldValueMapPDQPermanentLink(link.ID, link.SectionID, link.Url, link.Title);
                
                // Add the updated item to the list of content to be changed/updated
                contentForUpdatingList.Add(new ContentItemForUpdating(link.Guid, updatedFields));
            }

            // Update every piece of content
            CMSController.UpdateContentItemList(contentForUpdatingList);
        }



        /// <summary>
        /// Figures out the Guids of what Permanent Links are marked for deletion as stored in this 
        /// Permanent Link Processor.
        /// </summary>
        /// <returns>Guids of Permanent Links that are marked for deletion.</returns>
        /// <remarks>Must be public so it can be called for Verify in other Processors.</remarks>
        public PercussionGuid[] DetectToDeletePermanentLinkRelationships()
        {
            // First get all of the identifying Guids (in long format) that are in the list of links
            // that are marked for deletion
            long[] toDeleteLongs = Array.ConvertAll(toDeletePermanentLinks.ToArray(), linkToDelete => linkToDelete.Guid);

            // Convert all of these identifying Guids into PercussionGuid format
            PercussionGuid[] toDeleteGuids = Array.ConvertAll(toDeleteLongs, linkToDelete => new PercussionGuid(linkToDelete));

            // Return these guids so that they may be evaluated
            return toDeleteGuids;
        }



        /// <summary>
        /// Deletes the items from Percussion which are marked for deletion in the Permanent Link Processor.
        /// </summary>
        /// <remarks>
        /// No return value necessary because once they're cleared for deletion, we don't care. 
        /// No parameters necessary because the links marked for deletion are already stored in this 
        /// Processor and guids for each should already be identified.
        /// </remarks>
        public void DeletePermanentLinks()
        {
            // ToDo: Can probably store to delete guids somewhere...

            PercussionGuid[] toDeleteGuids = DetectToDeletePermanentLinkRelationships();
            CMSController.DeleteItemList(toDeleteGuids);
        }



    }
}
