using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Terminology
{
    /// <summary>
    /// Terminology synonym.
    /// </summary>
    [Serializable]
    public class TerminologyOtherName
    {
        #region Fields

        private string _name = string.Empty;
        private string _type = string.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// Term name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        
        /// <summary>
        /// Term type.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public TerminologyOtherName(string name, string type)
        {
            this._name = name;
            this._type = type;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the TerminologyOtherName object.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format("Name = {0} Type = {1}", this.Name, this.Type));

            return sb.ToString();
        }

        #endregion
    }
}
