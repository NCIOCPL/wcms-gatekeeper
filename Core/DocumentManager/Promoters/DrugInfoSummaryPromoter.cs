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
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Glossary tem promoter class
    /// </summary>
    class DrugInfoSummaryPromoter: DocumentPromoterBase
    {
        #region Public methods
        public DrugInfoSummaryPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Method to extract/render/save drug info summary document into staging database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToStaging(DocumentXPathManager xPathManager,
                               HistoryEntryWriter warningWriter,
            HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote drug info summary document to the staging database.");

            DrugInfoSummaryDocument drugInfoSummary = new DrugInfoSummaryDocument();
            drugInfoSummary.WarningWriter = warningWriter;
            drugInfoSummary.InformationWriter = informationWriter;
            drugInfoSummary.DocumentID = DataBlock.CdrID;
            drugInfoSummary.VersionNumber = DataBlock.CdrVersion;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Extract drug info summary data
                drugInfoSummary.VersionNumber = DataBlock.CdrVersion;
                DrugInfoSummaryExtractor extractor = new DrugInfoSummaryExtractor();
                extractor.Extract(DataBlock.DocumentData, drugInfoSummary, xPathManager);

                // Rendering drug info summary data
                DrugInfoSummaryRenderer drugRender = new DrugInfoSummaryRenderer();
                drugRender.Render(drugInfoSummary);

                // Save drug info summary data into database
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.SaveDocument(drugInfoSummary, UserName);
                    //call store(doctype object);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove drug info summary data from database
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.DeleteDocument(drugInfoSummary, ContentDatabase.Staging, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid drug info summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            drugInfoSummary = null;

            informationWriter("Promoting drug info summary document to the staging database succeeded.");
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
            informationWriter("Start to promote drug info summary document to the preview database.");

            DrugInfoSummaryDocument drugInfoSummary = new DrugInfoSummaryDocument();
            drugInfoSummary.WarningWriter = warningWriter;
            drugInfoSummary.InformationWriter = informationWriter;
            drugInfoSummary.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push drug info summary document to the preview database
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.PushDocumentToPreview(drugInfoSummary, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove drug info summary data from database
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.DeleteDocument(drugInfoSummary, ContentDatabase.Preview, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid drug info summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting drug info summary document to the preview database succeeded.");
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
            informationWriter("Start to promote drug info summary document to the live database.");

            DrugInfoSummaryDocument drugInfoSummary = new DrugInfoSummaryDocument();
            drugInfoSummary.WarningWriter = warningWriter;
            drugInfoSummary.InformationWriter = informationWriter;
            drugInfoSummary.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push drug info summary document to the live database
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.PushDocumentToLive(drugInfoSummary, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove drug info summary data from database
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.DeleteDocument(drugInfoSummary, ContentDatabase.Live, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid drug info summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting drug info summary document to the live database succeeded.");
        }

        #endregion
    }
}
