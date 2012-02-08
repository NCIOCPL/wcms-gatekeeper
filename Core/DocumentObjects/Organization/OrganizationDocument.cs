using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Organization
{
    /// <summary>
    /// Organization document object.
    /// </summary>
    [Serializable]
    public class OrganizationDocument : Document
    {
        #region Fields

        private string _officialName = string.Empty;
        private List<string> _shortNames = new List<string>();
        private List<string> _alternateNames = new List<string>();
        #endregion

        #region Constructors

        public OrganizationDocument()
            : base()
        {
            this.DocumentType = DocumentType.Organization;
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Official organization name.
        /// </summary>
        public string OfficialName
        {
            get { return _officialName; }
            internal set { _officialName = value; }
        }

        /// <summary>
        /// Collection of organization short names.
        /// </summary>
        public List<string> ShortNames
        {
            get { return _shortNames; }
            internal set { _shortNames = value; }
        }

        /// <summary>
        /// Collection of organization alternate names.
        /// </summary>
        public List<string> AlternateNames
        {
            get { return _alternateNames; }
            internal set { _alternateNames = value; }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the organization document.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format("Official name = {0}\n", this.OfficialName));

            sb.Append("ShortNames = \n");
            foreach (string shortName in this.ShortNames)
            {
                sb.Append(string.Format("ShortName = {0}\n", shortName));
            }

            sb.Append("AlternateNames = \n");
            foreach (string altName in this.AlternateNames)
            {
                sb.Append(string.Format("AlternateName = {0}\n", altName));
            }

             return sb.ToString();
        }

        #endregion
    }
}
