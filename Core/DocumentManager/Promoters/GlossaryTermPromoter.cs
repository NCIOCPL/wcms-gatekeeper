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
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Glossary tem promoter class
    /// </summary>
    class GlossaryTermPromoter : DocumentPromoterBase
    {
        #region Public methods
        public GlossaryTermPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Method to extract/render/save glossary term document into staging database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
         protected override void PromoteToStaging(DocumentXPathManager xPathManager,
             HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote glossary term document to the staging database.");

            GlossaryTermDocument glossaryTerm = new GlossaryTermDocument();
            glossaryTerm.WarningWriter = warningWriter;
            glossaryTerm.InformationWriter = informationWriter;
            glossaryTerm.VersionNumber = DataBlock.CdrVersion;
            glossaryTerm.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Extract glossary term data
                glossaryTerm.VersionNumber = DataBlock.CdrVersion;
                GlossaryTermExtractor extract = new GlossaryTermExtractor();
                extract.Extract(DataBlock.DocumentData, glossaryTerm, xPathManager);

                // Rendering glossary term data
                GlossaryTermRenderer gtRender = new GlossaryTermRenderer();
                gtRender.Render(glossaryTerm);

                // Save glossary term data into database
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.SaveDocument(glossaryTerm, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove glossary term data from database
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.DeleteDocument(glossaryTerm, ContentDatabase.Staging, UserName);
                }
            }
            else
            {
               // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid glossary term request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            glossaryTerm = null;
            
            informationWriter("Promoting glossary term document to the staging database succeeded.");
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
            informationWriter("Start to promote glossary term document to the preview database.");

            GlossaryTermDocument glossaryTerm = new GlossaryTermDocument();
            glossaryTerm.WarningWriter = warningWriter;
            glossaryTerm.InformationWriter = informationWriter;
            glossaryTerm.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push glossary term document to the preview database
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.PushDocumentToPreview(glossaryTerm, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove glossary term data from database
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.DeleteDocument(glossaryTerm, ContentDatabase.Preview, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid glossary term request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting glossary term document to the preview database succeeded.");
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
            informationWriter("Start to promote glossary term document to the live database.");

            GlossaryTermDocument glossaryTerm = new GlossaryTermDocument();
            glossaryTerm.WarningWriter = warningWriter;
            glossaryTerm.InformationWriter = informationWriter;
            glossaryTerm.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push glossary term document to the live database
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.PushDocumentToLive(glossaryTerm, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove glossary term data from database
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.DeleteDocument(glossaryTerm, ContentDatabase.Live, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid glossary term request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting glossary term document to the live database succeeded.");
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
            informationWriter("Start to promote glossary term document to the preview and live database in one step.");

            this.PromoteToPreview(xPathManager, warningWriter, informationWriter);
            this.PromoteToLive(xPathManager, warningWriter, informationWriter);

            informationWriter("Promoting glossary term document to the preview and live database succeeded.");
        }

        #endregion
    }
}
