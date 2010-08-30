using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Protocol
{
    /// <summary>
    /// Class to represent protocol drug.
    /// </summary>
    [Serializable]
    public class ProtocolDrug
    {
        #region Fields

        private string _name = string.Empty;
        private string _searchName = string.Empty;
        private int _drugID = 0;
        private int _parentRef = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ProtocolDrug()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="drugID"></param>
        /// <param name="drugName"></param>
        public ProtocolDrug(int drugID, string drugName)
        {
            this._drugID = drugID;
            this._name = drugName;
            this._searchName = CleanDrugName(_name);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 
        /// </summary>
        public int ParentRef
        {
            get { return _parentRef; }
            set { _parentRef = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public int DrugID
        {
            get { return _drugID; }
            set { _drugID = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;

                _searchName = CleanDrugName(_name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string SearchName
        {
            get { return _searchName; }
            set { _searchName = value; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Utility function to create the search name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string CleanDrugName(string name)
        {
            string tempDrugName = name.Replace(" ", string.Empty);
            tempDrugName = tempDrugName.Replace("-", string.Empty);
            return tempDrugName.Replace(",", string.Empty).ToUpper();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String that represents the Drug.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} DrugID = {1} Name = {2} SearchName = {3} ParentRef = {4}", base.ToString(), DrugID, Name, SearchName, ParentRef);
        }

        #endregion
    }
}
