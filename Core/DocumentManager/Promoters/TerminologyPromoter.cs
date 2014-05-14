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
using GateKeeper.DocumentObjects.Terminology;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Glossary tem promoter class
    /// </summary>
    class TerminologyPromoter : DocumentPromoterBase
    {
        #region Public methods
        public TerminologyPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Method to extract/render/save terminology document into staging database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToStaging(DocumentXPathManager xPathManager,
            HistoryEntryWriter warningWriter,
                               HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote terminology document to the staging database.");

            TerminologyDocument term = new TerminologyDocument();
            term.WarningWriter = warningWriter;
            term.InformationWriter = informationWriter;
            term.VersionNumber = DataBlock.CdrVersion;
            term.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Extract terminology data
                term.VersionNumber = DataBlock.CdrVersion;
                TerminologyExtractor extractor = new TerminologyExtractor();
                extractor.Extract(DataBlock.DocumentData, term, xPathManager);

                // Rendering terminology data
                TerminologyRenderer gtRender = new TerminologyRenderer();
                gtRender.Render(term);

                // Save terminology data into database
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    termQuery.SaveDocument(term, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove terminology data from database
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    termQuery.DeleteDocument(term, ContentDatabase.Staging, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid terminology request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            term = null;

            informationWriter("Promoting terminology document to the staging database succeeded.");
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
            informationWriter("Start to promote terminology document to the preview database.");

            TerminologyDocument term = new TerminologyDocument();
            term.WarningWriter = warningWriter;
            term.InformationWriter = informationWriter;
            term.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push terminology document to the preview database
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    termQuery.PushDocumentToPreview(term, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove terminology data from database
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    termQuery.DeleteDocument(term, ContentDatabase.Preview, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid terminology request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting terminology document to the preview database succeeded.");
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
            informationWriter("Start to promote terminology document to the live database.");

            TerminologyDocument term = new TerminologyDocument();
            term.WarningWriter = warningWriter;
            term.InformationWriter = informationWriter;
            term.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push terminology document to the live database
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    termQuery.PushDocumentToLive(term, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove terminology data from database
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    termQuery.DeleteDocument(term, ContentDatabase.Live, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid terminology request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting terminology document to the live database succeeded.");
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
            informationWriter("Start to promote terminology document to the preview and live database in one step.");

            this.PromoteToPreview(xPathManager, warningWriter, informationWriter);
            this.PromoteToLive(xPathManager, warningWriter, informationWriter);

            informationWriter("Promoting terminology document to the preview and live database succeeded.");
        }

        #endregion
    }
}
