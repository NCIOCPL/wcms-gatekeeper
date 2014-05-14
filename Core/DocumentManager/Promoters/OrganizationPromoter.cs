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
using GateKeeper.DocumentObjects.Organization;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Glossary tem promoter class
    /// </summary>
    class OrganizationPromoter: DocumentPromoterBase
    {
        #region Public methods
        public OrganizationPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Method to extract/render/save organization document into staging database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToStaging(DocumentXPathManager xPathManager,
            HistoryEntryWriter warningWriter,
                               HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote organization document to the staging database.");

            OrganizationDocument organization = new OrganizationDocument();
            organization.WarningWriter = warningWriter;
            organization.InformationWriter = informationWriter;
            organization.VersionNumber = DataBlock.CdrVersion;
            organization.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Extract organization data
                organization.VersionNumber = DataBlock.CdrVersion;
                OrganizationExtractor.Extract(DataBlock.DocumentData, organization, xPathManager);

                // Save organization data into database
                using (OrganizationQuery orgQuery = new OrganizationQuery())
                {
                    orgQuery.SaveDocument(organization, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove organization data from database
                using (OrganizationQuery orgQuery = new OrganizationQuery())
                {
                    orgQuery.DeleteDocument(organization, ContentDatabase.Staging, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid organization request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            organization = null;

            informationWriter("Promoting organization document to the staging database succeeded.");
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
            informationWriter("Start to promote organization document to the preview database.");

            OrganizationDocument organization = new OrganizationDocument();
            organization.WarningWriter = warningWriter;
            organization.InformationWriter = informationWriter;
            organization.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push organization document to the preview database
                using (OrganizationQuery orgQuery = new OrganizationQuery())
                {
                    orgQuery.PushDocumentToPreview(organization, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove organization data from database
                using (OrganizationQuery orgQuery = new OrganizationQuery())
                {
                    orgQuery.DeleteDocument(organization, ContentDatabase.Preview, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid organization request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting organization document to the preview database succeeded.");
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
            informationWriter("Start to promote organization document to the live database.");

            OrganizationDocument organization = new OrganizationDocument();
            organization.WarningWriter = warningWriter;
            organization.InformationWriter = informationWriter;
            organization.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push organization document to the live database
                using (OrganizationQuery orgQuery = new OrganizationQuery())
                {
                    orgQuery.PushDocumentToLive(organization, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove organization data from database
                using (OrganizationQuery orgQuery = new OrganizationQuery())
                {
                    orgQuery.DeleteDocument(organization, ContentDatabase.Live, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid organization request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting organization document to the live database succeeded.");
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
            informationWriter("Start to promote organization document to the preview and live database in one step.");

            this.PromoteToPreview(xPathManager, warningWriter, informationWriter);
            this.PromoteToLive(xPathManager, warningWriter, informationWriter);

            informationWriter("Promoting organization document to the preview and live database succeeded.");
        }

        #endregion
    }
}
