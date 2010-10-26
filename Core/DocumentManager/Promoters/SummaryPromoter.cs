using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using GateKeeper.Common;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;

using GKManagers.CMSDocumentProcessing;

namespace GKManagers
{
    /// <summary>
    /// Glossary tem promoter class
    /// </summary>
    class SummaryPromoter: DocumentPromoterBase
    {
        #region Public methods
        public SummaryPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Method to extract/render/save summary document into staging database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToStaging(DocumentXPathManager xPathManager,
            HistoryEntryWriter warningWriter,
                               HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote summary document to the staging database.");

            SummaryDocument summary = new SummaryDocument();
            summary.WarningWriter = warningWriter;
            summary.InformationWriter = informationWriter;
            summary.VersionNumber = DataBlock.CdrVersion;
            summary.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Extract summary data
                summary.VersionNumber = DataBlock.CdrVersion;
                XmlDocument summaryDoc = DataBlock.DocumentData;
                SummaryExtractor extract = new SummaryExtractor();
                extract.PrepareXml(summaryDoc, xPathManager);
                extract.Extract(summaryDoc, summary);

                // Rendering summary data
                SummaryRenderer summaryRender = new SummaryRenderer();
                summaryRender.Render(summary);

                // Save summary data into the Percussion CMS.
                using (CancerInfoSummaryProcessor processor = new CancerInfoSummaryProcessor(warningWriter, informationWriter))
                {
                    processor.ProcessDocument(summary);
                }


                // Save summary data into database
                // Don't delete this until after refactoring the render code.
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.SaveDocument(summary, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove summary data from database
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Staging, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            summary = null;

            informationWriter("Promoting summary document to the staging database succeeded.");
        }

        /// <summary>
        /// Method to call query class to push document to the preview database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToPreview(DocumentXPathManager xPathManager,
                               HistoryEntryWriter warningWriter,
                               HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote summary document to the preview database.");

            SummaryDocument summary = new SummaryDocument();
            summary.WarningWriter = warningWriter;
            summary.InformationWriter = informationWriter;
            summary.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push summary document to the preview database
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToPreview(summary, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove summary data from database
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting summary document to the preview database succeeded.");
        }

        /// <summary>
        /// Method to call query class to push document to the live database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToLive(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote summary document to the live database.");

            SummaryDocument summary = new SummaryDocument();
            summary.WarningWriter = warningWriter;
            summary.InformationWriter = informationWriter;
            summary.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push summary document to the live database
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToLive(summary, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove summary data from database
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Live, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting summary document to the live database succeeded.");
        }

        #endregion
    }
}
