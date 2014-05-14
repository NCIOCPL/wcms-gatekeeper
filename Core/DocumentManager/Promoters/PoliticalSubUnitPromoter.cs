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
using GateKeeper.DocumentObjects.PoliticalSubUnit;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Glossary tem promoter class
    /// </summary>
    class PoliticalSubUnitPromoter : DocumentPromoterBase
    {
        #region Public methods
        public PoliticalSubUnitPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Method to extract/render/save political sub unit document into staging database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToStaging(DocumentXPathManager xPathManager,
            HistoryEntryWriter warningWriter,
                               HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote political sub unit document to the staging database.");

            PoliticalSubUnitDocument subUnit = new PoliticalSubUnitDocument();
            subUnit.WarningWriter = warningWriter;
            subUnit.InformationWriter = informationWriter;
            subUnit.VersionNumber = DataBlock.CdrVersion;
            subUnit.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Extract political sub unit data
                subUnit.VersionNumber = DataBlock.CdrVersion;
                PoliticalSubUnitExtractor.Extract(DataBlock.DocumentData, subUnit, xPathManager);

                // Save political sub unit data into database
                using (PoliticalSubUnitQuery subUnitQuery = new PoliticalSubUnitQuery())
                {
                    subUnitQuery.SaveDocument(subUnit, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove political sub unit data from database
                using (PoliticalSubUnitQuery subUnitQuery = new PoliticalSubUnitQuery())
                {
                    subUnitQuery.DeleteDocument(subUnit, ContentDatabase.Staging, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid political sub unit request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            subUnit = null;

            informationWriter("Promoting political sub unit document to the staging database succeeded.");
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
            informationWriter("Start to promote political sub unit document to the preview database.");

            PoliticalSubUnitDocument subUnit = new PoliticalSubUnitDocument();
            subUnit.WarningWriter = warningWriter;
            subUnit.InformationWriter = informationWriter;
            subUnit.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push political sub unit document to the preview database
                using (PoliticalSubUnitQuery subUnitQuery = new PoliticalSubUnitQuery())
                {
                    subUnitQuery.PushDocumentToPreview(subUnit, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove political sub unit data from database
                using (PoliticalSubUnitQuery subUnitQuery = new PoliticalSubUnitQuery())
                {
                    subUnitQuery.DeleteDocument(subUnit, ContentDatabase.Preview, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid political sub unit request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting political sub unit document to the preview database succeeded.");
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
            informationWriter("Start to promote political sub unit document to the live database.");

            PoliticalSubUnitDocument subUnit = new PoliticalSubUnitDocument();
            subUnit.WarningWriter = warningWriter;
            subUnit.InformationWriter = informationWriter;
            subUnit.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push political sub unit document to the live database
                using (PoliticalSubUnitQuery subUnitQuery = new PoliticalSubUnitQuery())
                {
                    subUnitQuery.PushDocumentToLive(subUnit, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove political sub unit data from database
                using (PoliticalSubUnitQuery subUnitQuery = new PoliticalSubUnitQuery())
                {
                    subUnitQuery.DeleteDocument(subUnit, ContentDatabase.Live, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid political sub unit request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting political sub unit document to the live database succeeded.");
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
            informationWriter("Start to promote political sub unit document to the preview and live database in one step.");

            this.PromoteToPreview(xPathManager, warningWriter, informationWriter);
            this.PromoteToLive(xPathManager, warningWriter, informationWriter);

            informationWriter("Promoting political sub unit document to the preview and live database succeeded.");
        }

        #endregion
    }
}
