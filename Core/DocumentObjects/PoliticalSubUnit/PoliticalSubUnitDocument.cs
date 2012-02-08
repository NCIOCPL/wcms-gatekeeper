using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.PoliticalSubUnit
{
    /// <summary>
    /// Class to represent a state or political sub unit.
    /// </summary>
    [Serializable]
    public class PoliticalSubUnitDocument : Document
    {
        #region Fields

        private string _countryId = string.Empty;
        private string _fullName = string.Empty;
        private string _shortName = string.Empty;
        private string _countryName = string.Empty;

        #endregion

        #region Constructors

        public PoliticalSubUnitDocument()
            : base()
        {
            this.DocumentType = DocumentType.PoliticalSubUnit;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// State full name.
        /// </summary>
        public string CountryId
        {
            get { return _countryId; }
            internal set { _countryId = value; }
        }

        /// <summary>
        /// State full name.
        /// </summary>
        public string FullName
        {
            get { return _fullName; }
            internal set { _fullName = value; }
        }

        /// <summary>
        /// Stat short name.
        /// </summary>
        public string ShortName
        {
            get { return _shortName; }
            internal set { _shortName = value; }
        }

        /// <summary>
        /// Country name.
        /// </summary>
        public string CountryName
        {
            get { return _countryName; }
            internal set { _countryName = value; }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the politicial sub unit document.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" FullName = {0} ShortName = {1} CountryName = {2}",
                this.FullName, this.ShortName, this.CountryName));

            return sb.ToString();
        }

        #endregion
    }
}
