using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GKManagers;

namespace GKManagers.BusinessObjects
{
    [Serializable]
    public class RequestData : RequestDataInfo
    {
        private string _documentDataString = string.Empty;

        public RequestData(int packetNumber, RequestDataActionType actionType, int cdrID,
            string cdrVersion, CDRDocumentType cdrDocType, RequestDataLocationType location, int groupID,
            string documentData)
            : base(packetNumber, actionType, cdrID, cdrVersion, cdrDocType, location, groupID)
        {
            // Only allow documentData to be null/empty on remove requests.
            if (actionType != RequestDataActionType.Remove && String.IsNullOrEmpty(documentData))
                throw new ArgumentException("Argument documentData may only be empty for Remove requests.");

            // For a remove document, force the data string to be empty instead of null;
            if (actionType != RequestDataActionType.Remove)
                _documentDataString = documentData;
            else
                _documentDataString = string.Empty;
        }

        public RequestData()
            : base()
        {
        }

        #region Properties

        [XmlElement(ElementName = "XML")]
        public XmlDocument DocumentData
        {
            get
            {
                if (!String.IsNullOrEmpty(_documentDataString))
                {
                    XmlDocument document = new XmlDocument();
                    document.PreserveWhitespace = true;
                    document.LoadXml(_documentDataString);
                    return document;
                }
                else
                    return null;
            }

            set
            {
                if (value != null)
                    _documentDataString = value.OuterXml;
                else
                    _documentDataString = string.Empty;
            }
        }

        [XmlIgnore]
        public string DocumentDataString
        {
            get { return _documentDataString; }
            set
            {
                if (value != null)
                    _documentDataString = value;
                else
                    _documentDataString = string.Empty;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validates the document against the specified DTD.
        /// </summary>
        /// <param name="dtdPath">The filename and path for the DTD</param>
        /// <returns>null if valid, a list of validation messages otherwise</returns>
        public virtual string Validate(string dtdPath)
        {
            string tmp_message = null;

            if (this.ActionType == RequestDataActionType.Export && !String.IsNullOrEmpty(this._documentDataString))
            {

                string xml_string = this._documentDataString;
                xml_string = Regex.Replace(xml_string, "xmlns=\"(.+?)\"", "", RegexOptions.Singleline | RegexOptions.Compiled);
                xml_string = "<!DOCTYPE " + this.CDRDocType.ToString()
                    + " SYSTEM \"" + dtdPath + "\">\n" + xml_string;
                StringReader xml_sr = new StringReader(xml_string);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ProhibitDtd = false;
                settings.ValidationType = ValidationType.DTD;

                // Tricky bit.  Instead of setting up a separate function as an event handler,
                // we use an anonymous delegate in order to store the results in a local variable.
                settings.ValidationEventHandler +=
                    delegate(object sender, ValidationEventArgs args)
                    {
                        // When validation errors occure, they are passed into this method
                        // and appended to tmp_message.
                        tmp_message += args.Message;
                    };

                try
                {
                    using (XmlReader reader = XmlReader.Create(xml_sr, settings))
                    {
                        while (reader.Read()) ;
                    }
                }
                catch (Exception ex)
                {
                    // If an exception occurs, then something is wrong with the XML.
                    // Therefore it's not valid, so record the message.
                    tmp_message += ex.Message;
                }
                finally
                {
                    // Remove references to likely large strings.
                    xml_string = null;
                    xml_sr = null;
                }

                // At this point, tmp_message will contain any validation messages.
            }
            else if (this.ActionType == RequestDataActionType.Remove)
            {
                // No need to validate when a document is being removed.
                tmp_message = null;
            }
            else
            {
                string format = "Invalid validation attempt. Action type is {0} and document data {1}.";
                string msgDataCondition;
                if (String.IsNullOrEmpty(this._documentDataString))
                    msgDataCondition = "is empty";
                else
                    msgDataCondition = "is not empty";
                string message = string.Format(format, this.ActionType.ToString(), msgDataCondition);
                RequestMgrLogBuilder.Instance.CreateError(this.GetType(), "Validate", message);
                throw new InvalidOperationException(message);
            }

            return tmp_message;
        }

        #endregion
    }
}
