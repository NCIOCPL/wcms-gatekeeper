using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

using GateKeeper.DocumentObjects.Summary;

namespace GKManagers.CMSDocumentProcessing
{
    public class StandardSummarySectionFinder : CancerInfoSummarySectionFinder
    {
        protected override string SummaryPageSlot { get { return "pdqCancerInformationSummaryPageSlot"; } }

        public StandardSummarySectionFinder(CMSController controller)
            : base(controller)
        {
        }


        internal override SummaryPageInfo FindPageContainingSection(PercussionGuid root, string sectionID)
        {
            PercussionGuid[] pageIDs = CMSController.SearchForItemsInSlot(root, SummaryPageSlot);

            if (pageIDs.Length <= 0)
                throw new EmptySlotException(string.Format("Slot unexpectedly empty for content item {0}.", root));

            return FindPageContainingSection(pageIDs, sectionID);
        }
    }
}
