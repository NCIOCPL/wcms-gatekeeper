using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Protocol
{
    /// <summary>
    /// Class to represent protocol alternate IDs.
    /// </summary>
    [Serializable]
    public class AlternateProtocolID
    {
        #region Fields

        private string _idString = string.Empty;
        private string _type = "Primary";
        
        #endregion

        #region Public Properties

        /// <summary>
        /// Alternate ID string.
        /// </summary>
        public string IdString
        {
            get { return _idString; }
            set { _idString = value; }
        }

        /// <summary>
        /// Alternate ID type.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String that represents the AlternateProtocolID.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} IDString = {1} Type = {2}", base.ToString(), IdString, Type);
        }

        #endregion
    }
}
