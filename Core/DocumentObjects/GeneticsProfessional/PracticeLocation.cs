using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.GeneticsProfessional
{
    /// <summary>
    /// Genetics professional practice location.
    /// </summary>
    [Serializable]
    public class PracticeLocation
    {
        #region Fields

        private string _city = string.Empty;
        private string _state = string.Empty;
        private string _postalCode = string.Empty;
        private string _country = string.Empty;

        #endregion Fields

        #region Public Properties

        /// <summary>
        /// City name.
        /// </summary>
        public string City
        {
            get { return _city; }
            set { _city = value; }
        }

        /// <summary>
        /// State name.
        /// </summary>
        public string State
        {
            get { return _state; }
            set { _state = value; }
        }

        /// <summary>
        /// Zip code.
        /// </summary>
        public string PostalCode
        {
            get { return _postalCode; }
            set { _postalCode = value; }
        }

        /// <summary>
        /// Country name.
        /// </summary>
        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="postalCode"></param>
        /// <param name="country"></param>
        public PracticeLocation(string city, string state, string postalCode, string country)
        {
            this._city = city;
            this._state = state;
            this._postalCode = postalCode;
            this._country = country;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the PracticeLocation object.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(string.Format(" City = {0} State = {1} PostalCode = {2} Country = {3}", 
                this.City, this.State, this.PostalCode, this.Country));

            return sb.ToString();
        }

        #endregion
    }
}
