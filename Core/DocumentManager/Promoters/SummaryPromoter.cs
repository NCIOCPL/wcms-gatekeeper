using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using GateKeeper.Common;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.DataAccessWrappers;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.ContentRendering;
using GKManagers.Preprocessors;
using GKManagers.Processors;
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

            // Apply any pre-processing changes to the XML
            SummaryPreprocessor preprocessor = new SummaryPreprocessor();
            preprocessor.Preprocess( DataBlock.DocumentData );

            ProcessorPool pool = ProcessorLoader.Load();
            ProcessingTarget[] processingTargets = pool.GetProcessingTargets(DocumentType.Summary);

            foreach (ProcessingTarget target in processingTargets)
            {
                informationWriter(string.Format("Processing for device: {0}.", target.TargetedDevice.ToString()));

                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                summary.VersionNumber = DataBlock.CdrVersion;
                summary.DocumentID = DataBlock.CdrID;
                if (DataBlock.ActionType == RequestDataActionType.Export)
                {
                    // Extract summary data
                    target.DocumentExtractor.Extract(DataBlock.DocumentData, summary, xPathManager, target.TargetedDevice);

                    // Only render and save if the Summary supports the current target.
                    if (summary.ValidOutputDevices.Contains(target.TargetedDevice))
                    {
                        // Rendering summary data
                        target.DocumentRenderer.Render(summary, target.TargetedDevice);

                    }
                    // Save summary data
                    target.DocumentDataAccess.SaveDocument(summary, UserName, warningWriter, informationWriter);

                }
                else if (DataBlock.ActionType == RequestDataActionType.Remove)
                {
                    target.DocumentDataAccess.DeleteDocument(summary, UserName, warningWriter, informationWriter);
                }
                else
                {
                    // There should never be any invalid request.
                    throw new Exception("Promoter Error: Invalid summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
                }

            }
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

            ProcessorPool pool = ProcessorLoader.Load();
            ProcessingTarget[] processingTargets = pool.GetProcessingTargets(DocumentType.Summary);

            foreach (ProcessingTarget target in processingTargets)
            {
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                summary.DocumentID = DataBlock.CdrID;
                if (DataBlock.ActionType == RequestDataActionType.Export)
                {
                    target.DocumentDataAccess.PromoteToPreview(summary, UserName, warningWriter, informationWriter);
                }
                else if (DataBlock.ActionType == RequestDataActionType.Remove)
                {
                    target.DocumentDataAccess.RemoveFromPreview(summary, UserName, warningWriter, informationWriter);
                }
                else
                {
                    // There should never be any invalid request.
                    throw new Exception("Promoter Error: Invalid summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
                }
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

            ProcessorPool pool = ProcessorLoader.Load();
            ProcessingTarget[] processingTargets = pool.GetProcessingTargets(DocumentType.Summary);

            foreach (ProcessingTarget target in processingTargets)
            {
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                summary.DocumentID = DataBlock.CdrID;
                if (DataBlock.ActionType == RequestDataActionType.Export)
                {
                    target.DocumentDataAccess.PromoteToLive(summary, UserName, warningWriter, informationWriter);
                }
                else if (DataBlock.ActionType == RequestDataActionType.Remove)
                {
                    target.DocumentDataAccess.RemoveFromLive(summary, UserName, warningWriter, informationWriter);
                }
                else
                {
                    // There should never be any invalid request.
                    throw new Exception("Promoter Error: Invalid summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
                }
            }

            informationWriter("Promoting summary document to the live database succeeded.");
        }

        /// <summary>
        /// Method to call query class to push document to the preview and live database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToLiveFast(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter)
        {

            informationWriter("Start to promote summary document to the live database using PromoteToLiveFast.");

            ProcessorPool pool = ProcessorLoader.Load();
            ProcessingTarget[] processingTargets = pool.GetProcessingTargets(DocumentType.Summary);

            foreach (ProcessingTarget target in processingTargets)
            {
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                summary.DocumentID = DataBlock.CdrID;
                if (DataBlock.ActionType == RequestDataActionType.Export)
                {
                    target.DocumentDataAccess.PromoteToLiveFast(summary, UserName, warningWriter, informationWriter);

                }
                else if (DataBlock.ActionType == RequestDataActionType.Remove)
                {
                    //remove the document from Preview and Live
                    target.DocumentDataAccess.RemoveFromPreview(summary, UserName, warningWriter, informationWriter);
                    target.DocumentDataAccess.RemoveFromLive(summary, UserName, warningWriter, informationWriter);

                }
                else
                {
                    // There should never be any invalid request.
                    throw new Exception("Promoter Error: Invalid summary request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
                }
            }

            informationWriter("Promoting summary document to the live database succeeded using PromoteToLiveFast.");

        }

        #endregion
    }
}
