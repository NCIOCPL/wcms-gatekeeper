using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Protocol
{
    /// <summary>
    /// Class to represent protocol intervention/modality.
    /// </summary>
    [Serializable]
    public class ProtocolModality
    {
        #region Fields

        private int _modalityID = 0;
        private string _type = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="modalityID"></param>
        /// <param name="type"></param>
        public ProtocolModality(int modalityID, string type)
        {
            this._modalityID = modalityID;
            this._type = type;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Modality identifier.
        /// </summary>
        public int ModalityID
        {
            get { return _modalityID; }
            set { _modalityID = value; }
        }
        
        /// <summary>
        /// Modality type.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String that represents the Modality.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} ModalityID = {1} Type = {2}", base.ToString(), ModalityID, Type);
        }

        #endregion
    }
}
