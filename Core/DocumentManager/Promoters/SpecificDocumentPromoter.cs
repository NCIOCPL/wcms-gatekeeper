using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using GateKeeper.Common;
using GateKeeper.DataAccess.GateKeeper;

using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Sample class demonstrating the minimum required functions needed to promote a
    /// specific document type
    /// </summary>
    class SpecificDocumentPromoter : DocumentPromoterBase
    {
        public SpecificDocumentPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
            DocMgrLogBuilder.Instance.CreateCritical(typeof(SpecificDocumentPromoter), "(constructor)",
                "Sample class SpecificDocumentPromoter is not intended for use in production.");
            throw new NotImplementedException("DO NOT USE SpecificDocumentPromoter.  WRITE A NEW CLASS!!!");
        }

        // TODO: Implement the per document-type document promoters.
        protected override void PromoteToStaging(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter)
        {
            /// Implementation for promoting a specific document type from
            /// GateKeeper to Staging
            DocMgrLogBuilder.Instance.CreateCritical(typeof(SpecificDocumentPromoter), "PromoteToStaging",
                "Sample class SpecificDocumentPromoter is not intended for use in production.");
            informationWriter("PromoteToStaging()");
            throw new NotImplementedException("DO NOT USE SpecificDocumentPromoter.  WRITE A NEW CLASS!!!");
        }

        protected override void PromoteToPreview(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter)
        {
            /// Implementation for promoting a specific document type from
            /// Staging to Preview
            DocMgrLogBuilder.Instance.CreateCritical(typeof(SpecificDocumentPromoter), "PromoteToPreview",
                "Sample class SpecificDocumentPromoter is not intended for use in production.");
            informationWriter("PromoteToPreview()");
            throw new NotImplementedException("DO NOT USE SpecificDocumentPromoter.  WRITE A NEW CLASS!!!");
        }

        protected override void PromoteToLive(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter)
        {
            /// Implementation for promoting a specific document type from
            /// Preview to Live
            DocMgrLogBuilder.Instance.CreateCritical(typeof(SpecificDocumentPromoter), "PromoteToLive",
                "Sample class SpecificDocumentPromoter is not intended for use in production.");
            informationWriter("PromoteToLive()");
            throw new NotImplementedException("DO NOT USE SpecificDocumentPromoter.  WRITE A NEW CLASS!!!");
        }

        protected override void PromoteToLiveFast(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter)
        {
            /// Implementation for promoting a specific document type from
            /// Preview to Live
            DocMgrLogBuilder.Instance.CreateCritical(typeof(SpecificDocumentPromoter), "PromoteToLiveFast",
                "Sample class SpecificDocumentPromoter is not intended for use in production.");
            informationWriter("PromoteToLive()");
            throw new NotImplementedException("DO NOT USE SpecificDocumentPromoter.  WRITE A NEW CLASS!!!");
        }
    }
}
