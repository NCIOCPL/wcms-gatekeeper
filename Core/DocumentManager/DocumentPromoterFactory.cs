using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using GateKeeper.DataAccess.GateKeeper;

using GKManagers.BusinessObjects;

namespace GKManagers
{
    public class DocumentPromoterFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataBlock">The request data object containing the document to be promoted.</param>
        /// <param name="batchID">Batch ID that this document is part of</param>
        /// <param name="action">The specific promotion action to take (PromoteToStaging, etc.)</param>
        /// <param name="userName">Identity of the user responsible for scheduling the batch.</param>
        /// <returns></returns>
        public static DocumentPromoterBase Create(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
        {
            if (dataBlock == null)
                throw new ArgumentNullException("dataBlock");

            DocumentPromoterBase promoter = null;

            // TODO: Add the remainder of the promoter objects.

            switch (dataBlock.CDRDocType)
            {
                case CDRDocumentType.GlossaryTerm:
                    promoter = new GlossaryTermPromoter(dataBlock, batchID, action, userName);
                    break;
                case CDRDocumentType.Term:
                    promoter = new TerminologyPromoter(dataBlock, batchID, action, userName);
                    break;
                case CDRDocumentType.Summary:
                    promoter = new SummaryPromoter(dataBlock, batchID, action, userName);
                    break;
                case CDRDocumentType.DrugInformationSummary:
                    promoter = new DrugInfoSummaryPromoter(dataBlock, batchID, action, userName);
                    break;
                case CDRDocumentType.GENETICSPROFESSIONAL:
                    promoter = new GeneticsProfessionalPromoter(dataBlock, batchID, action, userName);
                    break;
                case CDRDocumentType.CTGovProtocol:
                    promoter = new CTGovProtocolPromoter(dataBlock, batchID, action, userName);
                    break;
                case CDRDocumentType.Media:
                    promoter = new MediaPromoter(dataBlock, batchID, action, userName);
                    break;
                case CDRDocumentType.Organization:
                    promoter = new OrganizationPromoter(dataBlock, batchID, action, userName);
                    break;
                case CDRDocumentType.PoliticalSubUnit:
                    promoter = new PoliticalSubUnitPromoter(dataBlock, batchID, action, userName);
                    break;
                 default:
                     DocMgrLogBuilder.Instance.CreateError(typeof(DocumentPromoterFactory), "Create",
                         string.Format("Encountered the unsupported document type: {0}.", dataBlock.CDRDocType));
                     throw new NotImplementedException(string.Format("Encountered the unknown document type: {0}.",
                            dataBlock.CDRDocType));
                     // promoter = new SpecificDocumentPromoter(dataBlock, batchID, action, userName);
            }

            return promoter;
        }
    }
}
