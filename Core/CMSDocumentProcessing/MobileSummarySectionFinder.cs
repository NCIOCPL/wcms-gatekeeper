using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

using GateKeeper.DocumentObjects.Summary;
using GKManagers.CMSDocumentProcessing.Configuration;

namespace GKManagers.CMSDocumentProcessing
{
    public class MobileSummarySectionFinder : CancerInfoSummarySectionFinder
    {
        protected override string SummaryPageSlot { get { return CancerInfoSummaryProcessorCommon.MobilePageSlotName; } }

        public MobileSummarySectionFinder(CMSController controller)
            : base(controller)
        {
        }

        /// <summary>
        /// Finds the ID of the content item representing the first page content item
        /// in a previously saved Cancer Information Summary.
        /// Overrides
        /// </summary>
        /// <param name="root">The ID of the content item forming the summary document's root.</param>
        /// <returns>PercussionGuid of the first page</returns>
        internal override SummaryPageInfo FindFirstPage(PercussionGuid root)
        {
            SummaryPageInfo pageInfo = null;

            CMSProcessingSection config = CMSProcessingSection.Instance;
            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(root, CancerInfoSummaryProcessorCommon.MobilePageSlotName);
            bool isMobileReference = true;

            string siteBase = config.BaseFolders.MobileSiteBase;

            if (pageIDs.Length < 1)
            {
                pageIDs = CMSController.SearchForItemsInSlot(root, CancerInfoSummaryProcessorCommon.StandardPageSlotName);
                siteBase = config.BaseFolders.DesktopSiteBase;
                isMobileReference = false;
            }

            PercussionGuid first = null;

            if (pageIDs.Length > 0)
            {
                first = pageIDs[0];

                // Load the actual page item.
                PSItem item = CMSController.LoadContentItems(new PercussionGuid[] { first })[0];

                string pagePath = CMSController.GetPathInSite(item, siteBase);

                pageInfo = new SummaryPageInfo(1, pagePath, first, isMobileReference);
            }

            return pageInfo;
        }

        internal override SummaryPageInfo FindPageContainingSection(PercussionGuid root, string sectionID)
        {
            CMSProcessingSection config = CMSProcessingSection.Instance;
            string siteurl = config.BaseFolders.MobileSiteBase;
            bool isMobileReference = true;

            // Look for pages in the mobile version first.
            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(root, CancerInfoSummaryProcessorCommon.MobilePageSlotName);

            // If no pages were found, look at the desktop version next.
            if (pageIDs.Length <= 0)
            {
                pageIDs = CMSController.SearchForItemsInSlot(root, CancerInfoSummaryProcessorCommon.StandardPageSlotName);
                siteurl = config.BaseFolders.DesktopSiteBase;
                isMobileReference = false;
            }

            if (pageIDs.Length <= 0)
                throw new EmptySlotException(string.Format("Slot unexpectedly empty for content item {0}.", root));

            return FindPageContainingSection(pageIDs, sectionID, siteurl, isMobileReference);
        }

        internal SummaryPageInfo FindPageContainingSection(PercussionGuid[] pageIDs, string sectionID, string siteUrl, bool isMobileReference)
        {
            // Force the out parameters to have values.
           int pageNumber = int.MinValue;
           PercussionGuid containingItem = null;

            PSItem[] pageItems = CMSController.LoadContentItems(pageIDs);

            // All the page items reside in the same folder.
           string itemFolder = CMSController.GetPathInSite(pageItems[0], siteUrl);

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

            return new SummaryPageInfo(pageNumber, itemFolder, containingItem, isMobileReference);
        }

    }
}
