using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Terminology
{
    public class TermSemanticType
    {
        #region Fields
        private string _name = string.Empty;
        private int _id = 0;
        #endregion

        
        #region Public Properties

        /// <summary>
        /// Gets, sets semantic type name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        /// <summary>
        /// Gets, sets semantic type ID.
        /// </summary>
        public int ID
        {
            get { return _id; }
            internal set { _id = value; }
        }
        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="sectionID"></param>
        /// <param name="html"></param>
        /// <param name="protocolSectionType"></param>
        public TermSemanticType(string name, int id)
        {
            this._name = name;
            this._id = id;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a string representation of the ProtocolSection object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" SemanticTypeName = {0} \n",  this.Name));
            sb.Append(string.Format(" SemanticTypeID = {0}\n", this.ID));
            return sb.ToString();
        }

        #endregion Public Methods

    }
}
