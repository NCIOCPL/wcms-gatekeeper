using System;
using System.Collections.Generic;
using System.Text;

using GateKeeper.DocumentObjects;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Utility class to convert between values of CDRDocumentType and the internal DocumentType.
    /// These conversions are based on the values returned by the query.  Any changes to the 
    /// conversion MUST match these values.
    /// 
    ///     select ds.name, dtm.dataSetID, dtm.DocumentTypeID, dt.name
    ///     from dbo.DocumentType dt, dbo.DocumentTypeMap dtm, dbo.DataSet ds
    ///     where dt.documentTypeID = dtm.documentTypeID and
    ///          dtm.dataSetID = ds.dataSetID
    ///     order by dtm.DocumentTypeID
    /// 
    /// </summary>
    public static class DocumentTypeConverter
    {
        public static DocumentType CdrToGK(CDRDocumentType docType)
        {
            DocumentType newType;

            switch (docType)
            {
                case CDRDocumentType.Term:
                    newType = DocumentType.Terminology;
                    break;
                case CDRDocumentType.InScopeProtocol:
                case CDRDocumentType.OutOfScopeProtocol:
                case CDRDocumentType.Protocol:
                    newType = DocumentType.Protocol;
                    break;
                case CDRDocumentType.Summary:
                    newType = DocumentType.Summary;
                    break;
                case CDRDocumentType.Organization:
                    newType = DocumentType.Organization;
                    break;
                case CDRDocumentType.PoliticalSubUnit:
                    newType = DocumentType.PoliticalSubUnit;
                    break;
                case CDRDocumentType.DrugInformationSummary:
                    newType = DocumentType.DrugInfoSummary;
                    break;
                case CDRDocumentType.GENETICSPROFESSIONAL:
                    newType = DocumentType.GENETICSPROFESSIONAL;
                    break;
                case CDRDocumentType.GlossaryTerm:
                    newType = DocumentType.GlossaryTerm;
                    break;
                case CDRDocumentType.CTGovProtocol:
                    newType = DocumentType.CTGovProtocol;
                    break;
                case CDRDocumentType.Media:
                    newType = DocumentType.Media;
                    break;
                case CDRDocumentType.Invalid:
                    newType = DocumentType.Invalid;
                    break;

                default:
                    string fmt = "Encountered unknown CDRDocumentType value {0}.";
                    string msg = string.Format(fmt, docType.ToString());
                    RequestMgrLogBuilder.Instance.CreateError(typeof(DocumentTypeConverter),
                        "CdrToGK", msg);
                    newType = DocumentType.Invalid;
                    break;
            }

            return newType;
        }

        public static CDRDocumentType GKToCdr(DocumentType docType)
        {
            CDRDocumentType newType;

            switch (docType)
            {
                case DocumentType.Terminology:
                    newType = CDRDocumentType.Term;
                    break;
                case DocumentType.Protocol:
                    newType = CDRDocumentType.Protocol;
                    break;
                case DocumentType.Summary:
                    newType = CDRDocumentType.Summary;
                    break;
                case DocumentType.Organization:
                    newType = CDRDocumentType.Organization;
                    break;
                case DocumentType.PoliticalSubUnit:
                    newType = CDRDocumentType.PoliticalSubUnit;
                    break;
                case DocumentType.DrugInfoSummary:
                    newType = CDRDocumentType.DrugInformationSummary;
                    break;
                case DocumentType.GENETICSPROFESSIONAL:
                    newType = CDRDocumentType.GENETICSPROFESSIONAL;
                    break;
                case DocumentType.GlossaryTerm:
                    newType = CDRDocumentType.GlossaryTerm;
                    break;
                case DocumentType.CTGovProtocol:
                    newType = CDRDocumentType.CTGovProtocol;
                    break;
                case DocumentType.Media:
                    newType = CDRDocumentType.Media;
                    break;
                case DocumentType.Invalid:
                    newType = CDRDocumentType.Invalid;
                    break;

                default:
                    string fmt = "Encountered unknown DocumentType value {0}.";
                    string msg = string.Format(fmt, docType.ToString());
                    RequestMgrLogBuilder.Instance.CreateError(typeof(DocumentTypeConverter),
                        "GKToCdr", msg);
                    newType = CDRDocumentType.Invalid;
                    break;
            }

            return newType;
        }
    }
}
