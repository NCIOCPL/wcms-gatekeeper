using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GateKeeper.DocumentObjects.DrugInfoSummary
{
    /// <summary>
    /// Class to represent drug information summary documents.
    /// </summary>
    [Serializable]
    public class DrugInfoSummaryDocument : Document
    {
        #region Fields

        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _prettyURL = string.Empty;
        private int _terminologyLink = 0;
        private Guid _nciViewID = Guid.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// Document title.
        /// </summary>
        public string Title
        {
            get { return _title; }
            internal set { _title = value; }
        }

        /// <summary>
        /// Document description.
        /// </summary>
        public string Description
        {
            get { return _description; }
            internal set { _description = value; }
        }

        /// <summary>
        /// Pretty URL for the document.
        /// </summary>
        public string PrettyURL
        {
            get { return _prettyURL; }
            internal set { _prettyURL = value; }
        }

        /// <summary>
        /// Document id of the related drug terminolog document.
        /// </summary>
        public int TerminologyLink
        {
            get { return _terminologyLink; }
            internal set { _terminologyLink = value; }
        }

        /// <summary>
        /// NCI View identifier.
        /// </summary>
        public Guid NciViewID
        {
            get { return _nciViewID; }
            internal set { _nciViewID = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the drug info summary document.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" Title = {0} Description = {1} PrettyURL = {2} TerminologyLink = {3} NciViewID = {4} ", 
                this.Title, this.Description, this.PrettyURL, this.TerminologyLink, this.NciViewID));

            return sb.ToString();
        }

        #endregion
    }
}
