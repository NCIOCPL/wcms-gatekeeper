using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    [DataContract()]
    public class RelatedItems
    {
        public RelatedItems()
        {
            // Guarantee that the arrays are never null.
            Term = new RelatedTerm[] { };
            External = new RelatedExternalLink[] { };
            Summary = new RelatedSummary[] { };
            DrugSummary = new RelatedDrugSummary[] { };
        }

        /// <summary>
        /// Possibly empty list of relatedTerm structures
        /// </summary>
        [DataMember(Name = "term")]
        public RelatedTerm[] Term { get; set; }

        /// <summary>
        /// Possibly empty list of externalLink structures
        /// </summary>
        [DataMember(Name = "external")]
        public RelatedExternalLink[] External { get; set; }

        /// <summary>
        /// Possibly empty list of summaryRef structures
        /// </summary>
        [DataMember(Name = "summary")]
        public RelatedSummary[] Summary { get; set; }

        /// <summary>
        /// Possibly empty list of drugSummaryRef structures
        /// </summary>
        [DataMember(Name = "drug_summary")]
        public RelatedDrugSummary[] DrugSummary { get; set; }
    }
}
