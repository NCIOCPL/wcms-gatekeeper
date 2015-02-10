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
        public bool IsMobileReference { get; private set; }

        public SummaryPageInfo(int pageNumber, string basePath, PercussionGuid contentItemID)
            : this(pageNumber, basePath, contentItemID, false)
        {
        }

        public SummaryPageInfo(int pageNumber, string basePath, PercussionGuid contentItemID, bool isMobileReference)
        {
            PageNumber = pageNumber;
            BasePath = basePath;
            ContentItemID = contentItemID;
            IsMobileReference = isMobileReference;
        }

        public string GetReferenceUrl(string sectionID)
        {
                string url;

                //For Devon Rex - Remove page numbers

                // Page numbers are natural numbers (1, 2, 3...), not zero-based.
                //if (PageNumber > 1 && !IsMobileReference)
                //{
                //    if (string.IsNullOrEmpty(sectionID))
                //        url = string.Format("{0}/Page{1}", BasePath, PageNumber);
                //    else
                //        //removed the word section from the url
                //        //as sections are represented using their ids
                //        url = string.Format("{0}/Page{1}#{2}", BasePath, PageNumber, sectionID);
                //}
                //else
                //{
                    if (string.IsNullOrEmpty(sectionID))
                        url = string.Format("{0}", BasePath);
                    else
                        //removed the word section from the url
                        //as sections are represented using their ids
                        url = string.Format("{0}/#{1}", BasePath, sectionID);
                //}

                return url;

        }
    }
}
