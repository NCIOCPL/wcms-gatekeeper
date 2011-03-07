using System;
using System.Collections.Generic;
using System.Text;
using GateKeeper.Common;

namespace GateKeeper.DocumentObjects
{
    /// <summary>
    /// Document base class.
    /// </summary>
    public class Document
    {
        #region Fields

        private int _legacyPdqID = 0;
        private int _documentID = 0;
        private string _versionNumber = string.Empty;
        private DocumentType _documentType;
        private System.Xml.XmlDocument _xml = new System.Xml.XmlDocument();
        private System.Xml.XmlDocument _postRenderXml = new System.Xml.XmlDocument();
        private string _html = string.Empty;
        private DateTime _lastModifiedDate = DateTime.Now;
        private DateTime _firstPublishedDate = DateTime.MinValue;
        private DateTime _receivedDate = DateTime.MinValue;
        private HistoryEntryWriter _warningWriter;
        private HistoryEntryWriter _informationWriter;
        private Guid _documentGUID = Guid.NewGuid();

        #endregion

        #region Public Properties

        /// <summary>
        /// Document GUID.
        /// </summary>
        public Guid GUID
        {
            get { return _documentGUID; }
            internal set { _documentGUID = value; }
        }

        /// <summary>
        /// Xml after the XSL transform is applied.
        /// </summary>
        public System.Xml.XmlDocument PostRenderXml
        {
            get { return _postRenderXml; }
            set { _postRenderXml = value; }
        }

        /// <summary>
        /// Legacy PDQ ID.
        /// </summary>
        public int LegacyPdqID
        {
            get { return _legacyPdqID; }
            internal set { _legacyPdqID = value; }
        }

        /// <summary>
        /// Document identifier.
        /// </summary>
        public int DocumentID
        {
            get { return _documentID; }
            set { _documentID = value; }
        }

        /// <summary>
        /// Represents the version of the document.
        /// </summary>
        /// <remarks>Comprised of the CDRVersion.RequestID</remarks>
        /// <example>CDRVersion = 1 and RequestID = 858 => 1.858</example>
        public string VersionNumber
        {
            // TODO: Finalize the version number
            get { return _versionNumber; }
            set { _versionNumber = value; }
        }

        /// <summary>
        /// Document XML.
        /// </summary>
        public System.Xml.XmlDocument Xml
        {
            get { return _xml; }
            set { _xml = value; }
        }

        /// <summary>
        /// Document type.
        /// </summary>
        public DocumentType DocumentType
        {
            get { return _documentType; }
            internal set { _documentType = value; }
        }

        /// <summary>
        /// Document HTML representation.
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        /// <summary>
        /// Last modified date from external CMS (CDR).
        /// </summary>
        public DateTime LastModifiedDate
        {
            get { return _lastModifiedDate; }
            internal set { _lastModifiedDate = value; }
        }

        /// <summary>
        /// First published date from external CMS (CDR).
        /// </summary>
        public DateTime FirstPublishedDate
        {
            get { return _firstPublishedDate; }
            internal set { _firstPublishedDate = value; }
        }

        /// <summary>
        /// Document received data from DocumentManager.
        /// </summary>
        public DateTime ReceivedDate
        {
            get { return _receivedDate; }
            set { _receivedDate = value; }
        }

        public HistoryEntryWriter WarningWriter
        {
            get { return _warningWriter; }
            set { _warningWriter = value; }
        }

        public HistoryEntryWriter InformationWriter
        {
            get { return _informationWriter; }
            set { _informationWriter = value; }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the Document.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(
                string.Format(" DocumentID = {0} LegacyPdqID = {1} DocumentType = {2} VersionNumber = {3} FirstPublishedDate = {3} LastModifiedDate = {4} ",
                DocumentID, LegacyPdqID, DocumentType, VersionNumber, FirstPublishedDate, LastModifiedDate));

            return sb.ToString();
        }

        #endregion
    }
}
