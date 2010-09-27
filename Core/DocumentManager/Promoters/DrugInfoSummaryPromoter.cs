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

using GKManagers.CMSManager.DocumentProcessing;

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

                // Save drug info summary data into the Percussion CMS.
                using (DrugInfoSummaryProcessor processor = new DrugInfoSummaryProcessor(warningWriter, informationWriter))
                {
                    processor.ProcessDocument(drugInfoSummary);
                }

            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove from Staging does nothing.  This is by design.
                // Attempting to remove the document at this stage would
                // remove it from all stages.
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
                // Promote the document to Preview into the Percussion CMS.
                using (DrugInfoSummaryProcessor processor = new DrugInfoSummaryProcessor(warningWriter, informationWriter))
                {
                    processor.PromoteToPreview(drugInfoSummary.DocumentID);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove from Preview does nothing.  This is by design.
                // Attempting to remove the document at this stage would
                // remove it from all stages.
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
                // Promote the document to Live into the Percussion CMS.
                using (DrugInfoSummaryProcessor processor = new DrugInfoSummaryProcessor(warningWriter, informationWriter))
                {
                    processor.PromoteToLive(drugInfoSummary.DocumentID);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove from Preview does nothing at this point.
                throw new NotImplementedException("Sorry Bilal and Sharon, remove is *next* week!");
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
