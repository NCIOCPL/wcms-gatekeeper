using System;
using System.Collections.Generic;
using System.Text;

using GKManagers.BusinessObjects;

namespace GKManagers
{
    class DocumentTypeTracker
    {
        private DocumentTypeFlag _documentTypes = DocumentTypeFlag.Empty;

        public DocumentTypeTracker()
        {
        }

        public void AddDocumentType(CDRDocumentType docType)
        {
            switch(docType)
            {
                case CDRDocumentType.GlossaryTerm:
                    _documentTypes |= DocumentTypeFlag.GlossaryTerm;
                    break;

                case CDRDocumentType.Term:
                    _documentTypes |= DocumentTypeFlag.Term;
                    break;

                case CDRDocumentType.InScopeProtocol:
                    _documentTypes |= DocumentTypeFlag.InScopeProtocol;
                    break;

                case CDRDocumentType.Summary:
                    _documentTypes |= DocumentTypeFlag.Summary;
                    break;

                case CDRDocumentType.Organization:
                    _documentTypes |= DocumentTypeFlag.Organization;
                    break;

                case CDRDocumentType.MiscellaneousDocument:
                    _documentTypes |= DocumentTypeFlag.MiscellaneousDocument;
                    break;

                case CDRDocumentType.OutOfScopeProtocol:
                    _documentTypes |= DocumentTypeFlag.OutOfScopeProtocol;
                    break;

                case CDRDocumentType.PoliticalSubUnit:
                    _documentTypes |= DocumentTypeFlag.PoliticalSubUnit;
                    break;

                case CDRDocumentType.GENETICSPROFESSIONAL:
                    _documentTypes |= DocumentTypeFlag.GeneticsProfessional;
                    break;

                case CDRDocumentType.Protocol:
                    _documentTypes |= DocumentTypeFlag.Protocol;
                    break;

                case CDRDocumentType.CTGovProtocol:
                    _documentTypes |= DocumentTypeFlag.CTGovProtocol;
                    break;

                case CDRDocumentType.Media:
                    _documentTypes |= DocumentTypeFlag.Media;
                    break;

                case CDRDocumentType.DrugInformationSummary:
                    _documentTypes |= DocumentTypeFlag.DrugInformationSummary;
                    break;

                default:
                    {
                        string format = "Encountered an unknown document type {0}.";
                        string message = string.Format(format, docType);
                        DocMgrLogBuilder.Instance.CreateError(this.GetType(), "AddDocumentType", message);
                        throw new Exception(message);
                    }
            }
        }
    
        public bool Contains(DocumentTypeFlag types)
        {
            bool result = false;

            if ((_documentTypes & types) != DocumentTypeFlag.Empty)
                result = true;

            return result;
        }
    }
}
