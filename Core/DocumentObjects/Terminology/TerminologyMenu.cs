using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Terminology
{
    /// <summary>
    /// Terminology menu.
    /// </summary>
    [Serializable]
    public class TerminologyMenu
    {
        #region Fields

        // The menu type is default to 1.  So far there is only one type in database.  Maybe expand in the future
        private int _menuType = 1;
        private string _displayName = string.Empty;
        private string _sortName = string.Empty;
        private List<int> _menuParentIDList = new List<int>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Menu type name.
        /// </summary>
        /// <example>Clinical Trials--CancerType</example>
        public int MenuType
        {
            get { return _menuType; }
            set { _menuType = value; }
        }

        /// <summary>
        /// Menu display name
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// Sort name.
        /// </summary>
        public string SortName
        {
            get { return _sortName; }
            set { _sortName = value; }
        }

        /// <summary>
        /// A list of Parent IDs 
        /// </summary>
        public List<int> MenuParentIDList
        {
            get { return _menuParentIDList; }
        }


        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="sortName"></param>
        public TerminologyMenu(string displayName, string sortName)
        {
            this._displayName = displayName;
            this._sortName = sortName;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the Menu object.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" MenuType = {0} DisplayName = {1} SortName = {2} ",
                this.MenuType, this.DisplayName, this.SortName));

            sb.Append("MenuParentIDs = \n");
            foreach (int menuParentID in _menuParentIDList)
            {
                sb.Append(string.Format("{0}\n", menuParentID));
            }


            return sb.ToString();
        }

        #endregion
    }
}
