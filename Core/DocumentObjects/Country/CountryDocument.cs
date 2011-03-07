using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Country
{
    /// <summary>
    /// Organization document object.
    /// </summary>
    [Serializable]
    public class CountryDocument : Document
    {
        #region Fields

        private string _fullName = string.Empty;
        private string _shortName = string.Empty;
        private string _continent = string.Empty;
        private string _postalCodePosition = string.Empty;
        #endregion

        #region Public Properties

        /// <summary>
        /// Official country name.
        /// </summary>
        public string FullName
        {
            get { return _fullName; }
            internal set { _fullName = value; }
        }

        /// <summary>
        /// Country short names.
        /// </summary>
        public string ShortName
        {
            get { return _shortName; }
            internal set { _shortName = value; }
        }

        /// <summary>
        /// Continent that the country belongs.
        /// </summary>
        public string Continent
        {
            get { return _continent; }
            internal set { _continent = value; }
        }

        /// <summary>
        /// Country's postal code position.
        /// </summary>
        public string PostalCodePosition
        {
            get { return _postalCodePosition; }
            internal set { _postalCodePosition = value; }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the organization document.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format("Country full name = {0}\n", this.FullName));
            sb.Append(string.Format("ShortName = {0}\n", this.ShortName));
            sb.Append(string.Format("Continent = {0}\n", this.Continent));
            sb.Append(string.Format("PostCodePosition = {0}\n", this.PostalCodePosition));

            return sb.ToString();
        }

        #endregion
    }
}
