using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Class to represent a summary relation.
    /// </summary>
    [Serializable]
    public class SummaryRelation
    {
        #region Fields

        private int _relatedSummaryID = 0;
        private SummaryRelationType _relationType = SummaryRelationType.PatientVersionOf;

        #endregion

        #region Public Properties

        /// <summary>
        /// Type of relation.
        /// </summary>
        public SummaryRelationType RelationType
        {
            get { return _relationType; }
            internal set { _relationType = value; }
        }

        /// <summary>
        /// ID or related summary.
        /// </summary>
        public int RelatedSummaryID
        {
            get { return _relatedSummaryID; }
            internal set { _relatedSummaryID = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="relatedSummaryID"></param>
        /// <param name="summaryRelationType"></param>
        public SummaryRelation(int relatedSummaryID, SummaryRelationType summaryRelationType)
        {
            this._relatedSummaryID = relatedSummaryID;
            this._relationType = summaryRelationType;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the SummaryRelation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} RelatedSummaryID = {1} RelationType = {2}", 
                base.ToString(), this.RelatedSummaryID, this.RelationType);
        }

        #endregion
    }
}
