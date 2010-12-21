using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

using GateKeeper.DocumentObjects.Summary;


namespace GKManagers.CMSDocumentProcessing
{
    class CancerInfoSummarySectionFinder
    {
        #region Constants

        const string SummaryPageSlot = "pdqCancerInformationSummaryPageSlot";

        #endregion

        private CMSController CMSController { get; set; }

        public CancerInfoSummarySectionFinder(CMSController controller)
        {
            CMSController = controller;
        }

        /// <summary>
        /// Finds the ID of the content item representing the first page content item
        /// in a previously saved Cancer Information Summary.
        /// </summary>
        /// <param name="root">The ID of the content item forming the summary document's root.</param>
        /// <returns>PercussionGuid of the first page</returns>
        public PercussionGuid FindFirstPage(PercussionGuid root)
        {
            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(root, SummaryPageSlot);
            return FindFirstPage(pageIDs);
        }

        /// <summary>
        /// Finds the ID of the content item representing the first page content item
        /// in a previously saved Cancer Information Summary.
        /// </summary>
        /// <param name="pageIDs">The list of IDs for document's individual pages.
        /// Assumed to be in page order.</param>
        /// <returns>PercussionGuid of the first page</returns>
        public PercussionGuid FindFirstPage(PercussionGuid[] pageIDs)
        {
            PercussionGuid first = null;

            if (pageIDs.Length > 0)
                first = pageIDs[0];

            return first;
        }


        public void FindPageContainingSection(PercussionGuid root, string sectionID, out int pageNumber, out PercussionGuid containingItem)
        {
            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(root, SummaryPageSlot);
            if (pageIDs.Length <= 0)
                throw new EmptySlotException(string.Format("Slot unexpectedly empty for content item {0}.", root));

            FindPageContainingSection(pageIDs, sectionID, out pageNumber, out containingItem);
        }

        public void FindPageContainingSection(PercussionGuid[] pageIDs, string sectionID, out int pageNumber, out PercussionGuid containingItem)
        {
            // Force the out parameters to have values.
            pageNumber = int.MinValue;
            containingItem = null;

            PSItem[] pageItems = CMSController.LoadContentItems(pageIDs);

            for (int i = 0; i < pageItems.Length; i++)
            {
                string foundValue = PSItemUtils.GetFieldValue(pageItems[i], "top_sectionid");
                if (!string.IsNullOrEmpty(foundValue) && foundValue == sectionID)
                {
                    pageNumber = i;
                    containingItem = pageIDs[i];
                    break;
                }

                string[] fieldValues = PSItemUtils.GetChildFieldValues(pageItems[i], "contained_sections", "section_id");
                if (fieldValues != null && fieldValues.Length > 0 && fieldValues.Contains(sectionID))
                {
                    pageNumber = i;
                    containingItem = pageIDs[i];
                    break;
                }

                // A "section number" can actually identify any block-level (?) element in the
                // page HTML.  So, finally we check there.
                string bodyField = PSItemUtils.GetFieldValue(pageItems[i], "bodyfield");
                XmlDocument bodyHtml = new XmlDocument();
                bodyHtml.LoadXml(bodyField);
                XPathNavigator xNav = bodyHtml.CreateNavigator();
                XPathNavigator node = xNav.SelectSingleNode(string.Format("//a[@name='Section{0}']", sectionID));
                if (node != null)
                {
                    pageNumber = i;
                    containingItem = pageIDs[i];
                    break;
                }
            }

            // If a page number was found, adjust pageNumber to reflect natural numbers (1, 2, 3)
            // instead of zero-based.
            if (pageNumber != int.MinValue)
                pageNumber++;
        }

        public int FindInternalPageContainingSection(SummaryDocument summary, string sectionID)
        {
            int pageNumber = int.MinValue;
            int pageCounter = 0;

            foreach (SummarySection section in summary.SectionList)
            {
                // Top level sections denote individual pages.  The first page is page 1 (natural numbers)
                // so the counter is incremented at the top of the loop.
                if (section.IsTopLevel)
                    pageCounter++;

                if (section.RawSectionID == sectionID)
                {
                    pageNumber = pageCounter;
                    break;
                }

                // A "section number" can actually identify any block-level (?) element in the
                // section HTML.  So next we look there.
                XPathNavigator xNav = section.Html.CreateNavigator();
                XPathNavigator node = xNav.SelectSingleNode(string.Format("//a[@name='Section{0}']", sectionID));
                if (node != null)
                {
                    pageNumber = pageCounter;
                    break;
                }
            }

            return pageNumber;
        }
    }
}
