using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Protocol
{
    /// <summary>
    /// Type of cancer (drives protocol search hierarchy).
    /// </summary>
    [Serializable]
    public class TypeOfCancer
    {
        #region Fields

        private string _typeOfCancerName = string.Empty;
        private int _typeOfCancerID = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeOfCancerName"></param>
        /// <param name="typeOfCancerID"></param>
        public TypeOfCancer(int typeOfCancerID, string typeOfCancerName)
        {
            this._typeOfCancerID = typeOfCancerID;
            this._typeOfCancerName = typeOfCancerName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Type of cancer name.
        /// </summary>
        public string TypeOfCancerName
        {
            get { return _typeOfCancerName; }
            internal set { _typeOfCancerName = value; }
        }

        /// <summary>
        /// Type of cancer identifier.
        /// </summary>
        public int TypeOfCancerID
        {
            get { return _typeOfCancerID; }
            internal set { _typeOfCancerID = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String that represents the TypeOfCancer.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} TypeOfCancerID = {1} TypeOfCancerName = {2}", base.ToString(), TypeOfCancerID, TypeOfCancerName);
        }

        #endregion
    }
}
