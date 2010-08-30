using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Protocol
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ProtocolTitle
    {
        #region Fields

        private string _title = string.Empty;
        private string _audienceType = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ProtocolTitle()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="audienceType"></param>
        public ProtocolTitle(string title, string audienceType)
        {
            this._title = title;
            this._audienceType = audienceType;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Protocol title.
        /// </summary>
        public string Title
        {
            get { return _title; }
            internal set { _title = value; }
        }

        /// <summary>
        /// Audience type.
        /// </summary>
        public string AudienceType
        {
            get { return _audienceType; }
            internal set { _audienceType = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the ProtocolTitle.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" Title = {0} AudienceType = {1}", this.Title, this.AudienceType));

            return sb.ToString();
        }

        #endregion
    }
}
