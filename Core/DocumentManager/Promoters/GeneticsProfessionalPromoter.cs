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
using GateKeeper.DocumentObjects.GeneticsProfessional;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Glossary tem promoter class
    /// </summary>
    class GeneticsProfessionalPromoter : DocumentPromoterBase
    {
        #region Public methods
        public GeneticsProfessionalPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Method to extract/render/save genetics professional document into staging database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToStaging(DocumentXPathManager xPathManager,
            HistoryEntryWriter warningWriter,
                               HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote genetics professional document to the staging database.");

            GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
            genProf.WarningWriter = warningWriter;
            genProf.InformationWriter = informationWriter;
            genProf.VersionNumber = DataBlock.CdrVersion;
            genProf.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Extract genetics professional data
                genProf.VersionNumber = DataBlock.CdrVersion;
                GeneticsProfessionalExtractor extract = new GeneticsProfessionalExtractor();
                extract.Extract(DataBlock.DocumentData, genProf, xPathManager);

                // Save genetics professional data into database
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.SaveDocument(genProf, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove genetics professional data from database
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Staging, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid genetics professional request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            genProf = null;

            informationWriter("Promoting genetics professional document to the staging database succeeded.");
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
            informationWriter("Start to promote genetics professional document to the preview database.");

            GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
            genProf.WarningWriter = warningWriter;
            genProf.InformationWriter = informationWriter;
            genProf.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push genetics professional document to the preview database
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.PushDocumentToPreview(genProf, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove genetics professional data from database
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Preview, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid genetics professional request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting genetics professional document to the preview database succeeded.");
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
            informationWriter("Start to promote genetics professional document to the live database.");

            GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
            genProf.WarningWriter = warningWriter;
            genProf.InformationWriter = informationWriter;
            genProf.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push genetics professional document to the live database
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.PushDocumentToLive(genProf, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove genetics professional data from database
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Live, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid genetics professional request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting genetics professional document to the live database succeeded.");
        }

        #endregion
    }
}
