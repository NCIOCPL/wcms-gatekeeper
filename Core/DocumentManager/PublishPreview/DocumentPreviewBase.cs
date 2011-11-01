using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using GateKeeper.Common;
using GateKeeper.DataAccess.GateKeeper;

using GKManagers.BusinessObjects;

namespace GKPreviews
{
    /// <summary>
    /// A abstract base class for pre and post processing of documents before being sent 
    /// to the specific document processor. 
    /// </summary>
    public abstract class DocumentPreviewBase
    {
        private XmlDocument _documentData;     // Document data to preview.
        private string _userName;    // Who is performing the action.
        DocumentXPathManager _xDocPathManager = new DocumentXPathManager();

        public DocumentPreviewBase(XmlDocument documentData, string userName)
        {
            _documentData = documentData;
            _userName = userName;
        }

        #region Protected Properties
        /// <summary>
        /// Gets or Sets the document data.
        /// </summary>
        protected XmlDocument DocumentData
        {
            get { return _documentData; }
            set { _documentData = value; }
        }

        /// <summary>
        /// Get an instance of DocumentXPathManager
        /// </summary>
        protected DocumentXPathManager DocXPathManager
        {
            get { return _xDocPathManager; }
        }

        protected string UserName
        {
            get { return _userName; }
        }

        #endregion

        /// <summary>
        /// All subclass should implement this method.
        /// </summary>
        /// <returns>A string which cotains the HTML used for preview</returns>
        protected abstract void ProcessPreview(ref string contentHtml, ref string headerContent);

        /// <summary>
        /// This is a main entry method for producing the html rendered by
        /// CMS.
        /// </summary>
        /// <returns></returns>
        public void Preview(ref string contentHtml, ref string headerContent)
        {
            try
            {
                ProcessPreview(ref contentHtml, ref headerContent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Protected Methods
        protected void WriteHistoryInformationEntry(string description)
        {
        }

        protected void WriteHistoryWarningEntry(string description)
        {
        }

        protected void WriteHistoryErrorEntry(string description)
        {
        }

        protected void WriteHistoryDebugEntry(string description)
        {
        }

        #endregion
    }
}
