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
    public abstract class CancerInfoSummarySectionFinder
    {
        protected CMSController CMSController { get; set; }

        protected abstract string SummaryPageSlot { get; }

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
        internal virtual SummaryPageInfo FindFirstPage(PercussionGuid root)
        {
            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(root, SummaryPageSlot);
            return FindFirstPage(pageIDs, CMSController.SiteRootPath);
        }

        /// <summary>
        /// Finds the ID of the content item representing the first page content item
        /// in a previously saved Cancer Information Summary.
        /// </summary>
        /// <param name="pageIDs">The list of IDs for document's individual pages.
        /// Assumed to be in page order.</param>
        /// <returns>PercussionGuid of the first page</returns>
        internal SummaryPageInfo FindFirstPage(PercussionGuid[] pageIDs, string siteFolder)
        {
            PercussionGuid first = null;
            SummaryPageInfo pageInfo = null;

            if (pageIDs.Length > 0)
            {
                first = pageIDs[0];

                // Load the actual page item.
                PSItem item = CMSController.LoadContentItems(new PercussionGuid[] { first })[0];

                string pagePath = CMSController.GetPathInSite(item, siteFolder);

                pageInfo = new SummaryPageInfo(1, pagePath, first);
            }

            return pageInfo;
        }


        internal abstract SummaryPageInfo FindPageContainingSection(PercussionGuid root, string sectionID);


        internal SummaryPageInfo FindPageContainingSection(PercussionGuid[] pageIDs, string sectionID)
        {
            // Force the out parameters to have values.
            int pageNumber = int.MinValue;
            PercussionGuid containingItem = null;

            SummaryPageInfo pageInfo = null;

            PSItem[] pageItems = CMSController.LoadContentItems(pageIDs);

            // All the page items reside in the same folder.
            string baseUrl = CMSController.GetPathInSite(pageItems[0]);

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

            if (pageNumber == int.MinValue)
            {
                String fmt = "Unable to locate section {0}.";
                throw new CannotUpdateException(string.Format(fmt, sectionID));
            }

            // Adjust pageNumber to reflect natural numbers (1, 2, 3)
            // instead of zero-based.
            pageNumber++;

            pageInfo = new SummaryPageInfo(pageNumber, baseUrl, containingItem);

            return pageInfo;
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
