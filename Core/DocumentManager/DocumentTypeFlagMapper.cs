using System;

using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Provides a mechanism for performing operations on values.
    /// </summary>
    class DocumentTypeFlagUtil
    {
        public static DocumentTypeFlag MapDocumentType(CDRDocumentType docType)
        {
            switch (docType)
            {
                case CDRDocumentType.GlossaryTerm:
                    return DocumentTypeFlag.GlossaryTerm;

                case CDRDocumentType.Term:
                    return DocumentTypeFlag.Term;

                case CDRDocumentType.InScopeProtocol:
                    return DocumentTypeFlag.InScopeProtocol;

                case CDRDocumentType.Summary:
                    return DocumentTypeFlag.Summary;

                case CDRDocumentType.Organization:
                    return DocumentTypeFlag.Organization;

                case CDRDocumentType.MiscellaneousDocument:
                    return DocumentTypeFlag.MiscellaneousDocument;

                case CDRDocumentType.OutOfScopeProtocol:
                    return DocumentTypeFlag.OutOfScopeProtocol;

                case CDRDocumentType.PoliticalSubUnit:
                    return DocumentTypeFlag.PoliticalSubUnit;

                case CDRDocumentType.GENETICSPROFESSIONAL:
                    return DocumentTypeFlag.GeneticsProfessional;

                case CDRDocumentType.Protocol:
                    return DocumentTypeFlag.Protocol;

                case CDRDocumentType.CTGovProtocol:
                    return DocumentTypeFlag.CTGovProtocol;

                case CDRDocumentType.Media:
                    return DocumentTypeFlag.Media;

                case CDRDocumentType.DrugInformationSummary:
                    return DocumentTypeFlag.DrugInformationSummary;

                default:
                    {
                        string format = "Encountered an unknown document type {0}.";
                        string message = string.Format(format, docType);
                        DocMgrLogBuilder.Instance.CreateError(typeof(DocumentTypeFlagUtil), "MapDocumentType", message);
                        throw new Exception(message);
                    }
            }
        }

        public static bool FlagsOverlap(DocumentTypeFlag flag1, DocumentTypeFlag flag2)
        {
            return (flag1 == flag2) ||
                ((flag1 & flag2) != DocumentTypeFlag.Empty);
        }
    }
}
