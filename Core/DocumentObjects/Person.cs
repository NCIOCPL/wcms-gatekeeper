using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects
{
    /// <summary>
    /// Class to represent person documents.
    /// NOTE: Not used at this point.
    /// </summary>
    [Serializable]
    public class Person : Document
    {
        #region Fields

        private string _givenName = string.Empty;
        private string _surName = string.Empty;
        private string _professionalSuffix = string.Empty;

        #endregion

        #region Public Properties 

        /// <summary>
        /// First name.
        /// </summary>
        public string GivenName
        {
            get { return _givenName; }
            set { _givenName = value; }
        }
        
        /// <summary>
        /// Last name.
        /// </summary>
        public string SurName
        {
            get { return _surName; }
            set { _surName = value; }
        }

        /// <summary>
        /// Suffix (MD,...etc).
        /// </summary>
        public string ProfessionalSuffix
        {
            get { return _professionalSuffix; }
            set { _professionalSuffix = value; }
        }

        #endregion
    }
}
