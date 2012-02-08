using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.CMS;

namespace GKManagers.CMSDocumentProcessing
{
    class SummaryPageInfo
    {
        public int PageNumber { get; private set; }
        public string BasePath { get; private set; }
        public PercussionGuid ContentItemID { get; private set; }

        public SummaryPageInfo(int pageNumber, string basePath, PercussionGuid contentItemID)
        {
            PageNumber = pageNumber;
            BasePath = basePath;
            ContentItemID = contentItemID;
        }
    }
}
