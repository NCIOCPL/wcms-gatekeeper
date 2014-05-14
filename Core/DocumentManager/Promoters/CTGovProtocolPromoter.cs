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
using GateKeeper.DocumentObjects.Protocol;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Glossary tem promoter class
    /// </summary>
    class CTGovProtocolPromoter : DocumentPromoterBase
    {
        #region Public methods
        public CTGovProtocolPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Method to extract/render/save protocol document into staging database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToStaging(DocumentXPathManager xPathManager,
                               HistoryEntryWriter warningWriter,
                               HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote protocol document to the staging database.");

            ProtocolDocument protocol = new ProtocolDocument();
            protocol.WarningWriter = warningWriter;
            protocol.InformationWriter = informationWriter;
            protocol.VersionNumber = DataBlock.CdrVersion;
            protocol.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Extract protocol data
                protocol.VersionNumber = DataBlock.CdrVersion;
                protocol.ReceivedDate = DataBlock.ReceivedDate;
                CTGovProtocolExtractor extractor = new CTGovProtocolExtractor(xPathManager);
                extractor.Extract(DataBlock.DocumentData, protocol);

                // Rendering protocol data
                ProtocolRenderer protocolRender = new ProtocolRenderer();
                protocolRender.Render(protocol);

                // Save protocol data into database
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.SaveDocument(protocol, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove protocol data from database
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid protocol request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            protocol = null;

            informationWriter("Promoting protocol document to the staging database succeeded.");
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
            informationWriter("Start to promote protocol document to the preview database.");

            ProtocolDocument protocol = new ProtocolDocument();
            protocol.WarningWriter = warningWriter;
            protocol.InformationWriter = informationWriter;
            protocol.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push protocol document to the preview database
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.PushDocumentToPreview(protocol, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove protocol data from database
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid protocol request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting protocol document to the preview database succeeded.");
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
            informationWriter("Start to promote protocol document to the live database.");

            ProtocolDocument protocol = new ProtocolDocument();
            protocol.WarningWriter = warningWriter;
            protocol.InformationWriter = informationWriter;
            protocol.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push protocol document to the live database
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.PushDocumentToLive(protocol, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove protocol data from database
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid protocol request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting protocol document to the live database succeeded.");
        }

        /// <summary>
        /// Method to call query class to push document to the preview and live database in one step.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToLiveFast(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote protocol document to the preview and live database in one step.");

            this.PromoteToPreview(xPathManager, warningWriter, informationWriter);
            this.PromoteToLive(xPathManager, warningWriter, informationWriter);

            informationWriter("Promoting protocol document to the preview and live database succeeded.");
        }

        #endregion
    }
}
