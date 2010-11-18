using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Finds the ID of the content item representing the first content item
        /// in the Page Slot of a previously saved Cancer Information Summary.
        /// </summary>
        /// <param name="root">The ID of the content item forming the summary document's root.</param>
        /// <returns>PercussionGuid of the </returns>
        public PercussionGuid FindFirstPage(PercussionGuid root)
        {
            PercussionGuid first = null;

            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(root, SummaryPageSlot);
            if (pageIDs.Length > 0)
                first = pageIDs[0];

            return first;
        }

        public void FindPageContainingSection(PercussionGuid root, string sectionID, out int pageNumber, out PercussionGuid containingItem)
        {
            // Force the out parameters to have values.
            pageNumber = int.MinValue;
            containingItem = null;

            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(root, SummaryPageSlot);
            PSItem[] pageItems = CMSController.LoadContentItems(pageIDs);

            for (int i = 0; i < pageItems.Length; i++)
            {
                if (sectionID == PSItemUtils.GetFieldValue(pageItems[i], "top_sectionid"))
                {
                    pageNumber = i;
                    containingItem = pageIDs[i];
                }

            }
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
