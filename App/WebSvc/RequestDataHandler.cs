using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using GateKeeper.Common;
using GKManagers;
using GKManagers.BusinessObjects;

/*
 * The RequestDataHandler class responds to the initial request of a data transmission,
 * distinguished by the message body containing a docCount field with an expected document
 * count and the root node being named "PubEvent": 
 * 
 * The expected message body is:
 *  <PubData>
 *      <docNum>packet Number</docNum>
 *      <transactionType>Export|Remove</transactionType>
 *      <CDRDoc Type="docType" ID="docID" Version="version" Group="groupNumber">
 *          Document XML Data
 *      </CDRDoc>
 *  </PubData>
 * 
 * The response body is:
 *  <Response>
 *      <ResponseType>OK|Not Ready|Error</ResponseType>
 *      <ResponseMessage>message text</ResponseMessage>
 *      <PubDataResponse>
 *          <docNum>packet Number</docNum>
 *      </PubDataResponse>
 *  </Response>
 * 
 */
namespace GateKeeper
{
    public class RequestDataHandler : RequestHandlerBase, IRequestHandler
    {
        private string _requestSource = null;
        private string _requestID = null;

        // Message parts
        private int _packetNumber;
        private RequestDataActionType _actionType;
        private CDRDocumentType _docType;
        private int _cdrID;
        private string _cdrVersion;
        private int _groupNumber;
        private string _documentData;

        #region Constructor

        public RequestDataHandler(string source, string requestID, XmlNode message)
            : base()
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (requestID == null)
                throw new ArgumentNullException("requestID");
            if (message == null)
                throw new ArgumentNullException("message");

            _requestSource = source;
            _requestID = requestID;

            XmlNode node;
            XmlNode child;

            if (message.Name != "PubData")
                throw new InvalidRequestException("Document element must be PubData.");

            // Packet Number
            node = message.SelectSingleNode("cg:docNum/text()", cgNamespace);
            if (node == null)
                throw new InvalidRequestException("Missing node /PubData/docNum");
            _packetNumber = int.Parse(node.Value);
            if (_packetNumber < 1)
                throw new InvalidRequestException("Value for node /PubData/docNum must be greater than 0.");

            // Transaction type
            node = message.SelectSingleNode("cg:transactionType/text()", cgNamespace);
            if (node == null)
                throw new InvalidRequestException("Missing node /PubData/transactionType");
            _actionType = ConvertEnum<RequestDataActionType>.Convert(node.Value);
            if (_actionType != RequestDataActionType.Export &&
                _actionType != RequestDataActionType.Remove)
            {
                string exceptionText;
                exceptionText = string.Format("Unknown action type ({0}).", _actionType.ToString());
                throw new InvalidRequestException(exceptionText);
            }

            // Collect attributes from CDRDoc node.
            node = message.SelectSingleNode("cg:CDRDoc", cgNamespace);
            if (node == null)
                throw new InvalidRequestException("Missing node /PubData/CDRDoc");

            // Document Type
            child = node.Attributes["Type"];
            if (child == null)
                throw new InvalidRequestException("Missing attribute /PubData/CDRDoc/@Type");
            // Conver document type errors to be Invalid request instead of system errors.
            try
            {
                _docType = ConvertEnum<CDRDocumentType>.Convert(child.Value);
            }
            catch (Exception ex)
            {
                throw new InvalidRequestException(ex.Message);
            }

            // CDR ID
            child = node.Attributes["ID"];
            if (child == null)
                throw new InvalidRequestException("Missing attribute /PubData/CDRDoc/@ID");
            /// Remove leading non-digit characters from the CDRID.
            {
                string cdrID = child.Value;
                cdrID = Regex.Replace(cdrID, "^CDR(0*)", "", RegexOptions.Compiled);
                _cdrID = int.Parse(cdrID);
            }

            // CDR Version Number
            child = node.Attributes["Version"];
            if (child == null)
                throw new InvalidRequestException("Missing attribute /PubData/CDRDoc/@Version");
            _cdrVersion = child.Value;
            // Group Number
            child = node.Attributes["Group"];
            if (child == null)
                throw new InvalidRequestException("Missing attribute /PubData/CDRDoc/@Group");
            _groupNumber = int.Parse(child.Value);

            // Document Data
            _documentData = node.InnerXml;
            bool documentIsEmpty = (_documentData == null || _documentData.Length == 0);
            if (_actionType == RequestDataActionType.Export && documentIsEmpty ||
                _actionType == RequestDataActionType.Remove && !documentIsEmpty)
            {
                throw new InvalidRequestException("Document data must be present for Export and absent for Remove.");
            }
            // If applicable, remove the cancer.gov namespace from the document.
            if (_documentData != null)
            {
                string namespaceURI = @" xmlns=""" + cgNamespace.LookupNamespace("cg") + @"""";
                RegexOptions options = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;
                _documentData = Regex.Replace(_documentData, namespaceURI, "", options);
            }

            // Don't keep large object references any longer than needed.
            message = null;
        }

        #endregion

        #region Methods

        public XmlDocument ProcessRequest()
        {
            XmlDocument response = CreateResponseSkeleton();

            try
            {
                RequestData packet = RequestDataFactory.Create(_docType, _packetNumber, _actionType,
                    _cdrID, _cdrVersion, RequestDataLocationType.GateKeeper, _groupNumber, _documentData);

                if (RequestManager.InsertRequestData(_requestID, _requestSource, "GateKeeper", ref packet))
                    SetResponseType(ref response, "OK", "Packet OK");
                else
                    SetResponseType(ref response, "Error", "Could not insert document.");

                // Remove reference to packet and its internals.
                packet = null;
            }
            catch (Exception ex)
            {
                WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "ProcessRequest", ex);
                SetResponseType(ref response, "Error", ExceptionHelper.RetreiveInnermostMessage(ex));
            }

            return response;
        }

        override protected void AddResponseBodyDetails(ref XmlDocument response)
        {
            XmlNode root = response.DocumentElement;
            XmlNode node;

            // Start a new sub-tree.
            node = response.CreateElement("PubDataResponse");
            root.AppendChild(node);

            // Point to base of sub-tree.
            root = node;

            // Echo back the packet number
            node = response.CreateElement("docNum");
            node.AppendChild(response.CreateTextNode(_packetNumber.ToString()));
            root.AppendChild(node);

            // Don't keep references to potentially large objects around longer than needed.
            root = null;
        }

        #endregion
    }
}
