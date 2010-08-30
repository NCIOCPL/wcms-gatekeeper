using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;

using GateKeeper.Common;
using GKManagers;
using GKManagers.BusinessObjects;

namespace GateKeeper
{
    /*
     * The RequestEndHandler class responds to the initial request of a data transmission,
     * distinguished by the message body containing a docCount field with an expected document
     * count and the root node being named "PubEvent": 
     * 
     * The expected message body is:
     *  <PubEvent>
     *      <pubType>Hotfix|Export|Remove|Full Load</pubType>
     *      <docCount>"complete"</docCount>
     *  </PubEvent>
     * 
     * The response body is:
     *  <Response>
     *      <ResponseType>OK|Error</ResponseType>
     *      <ResponseMessage>message text</ResponseMessage>
     *      <PubEventResponse>
     *          <pubType>echoed from input</pubType>
     *          <docCount>actualCount/maxPacket</docCount>
     *      </PubEventResponse>
     *  </Response>
     * 
     */
    public class RequestEndHandler : RequestHandlerBase, IRequestHandler
    {
        private string _requestSource = null;
        private string _requestID = null;

        // Message parts
        private RequestPublicationType _pubType = RequestPublicationType.Invalid;
        private int _expectedDocCount;
        private string _status;

        #region Constructor

        public RequestEndHandler(string source, string requestID, XmlNode message)
            : base()
        {
            XmlNode node;

            if (source == null)
                throw new ArgumentNullException("source");
            if (requestID == null)
                throw new ArgumentNullException("requestID");
            if (message == null)
                throw new ArgumentNullException("message");

            _requestSource = source;
            _requestID = requestID;

            if (message.Name != "PubEvent")
                throw new InvalidRequestException("Document element must be PubEvent.");

            /// Retrieve the publication type (Hotfix|Export|Remove|Full Load).
            _pubType = GetRequestPublicationType(message);

            string docCount;
            node = message.SelectSingleNode("//cg:docCount/text()", cgNamespace);
            if (node == null)
                throw new InvalidRequestException("Missing node /PubEvent/docCount");
            docCount = node.Value;
            _expectedDocCount = int.Parse(docCount);

            node = message.SelectSingleNode("//cg:status/text()", cgNamespace);
            if (node == null)
                throw new InvalidRequestException("Missing node /PubEvent/status");
            _status = (string)node.Value;
        }

        #endregion

        #region Methods

        private delegate XmlDocument ProcessRequestDelegate();

        public XmlDocument ProcessRequest()
        {
            ProcessRequestDelegate processRequestMethod;

            if (string.Compare(_status, "complete", true) == 0)
                processRequestMethod = ProcessCloseRequest;
            else if (string.Compare(_status, "abort", true) == 0)
                processRequestMethod = ProcessAbortRequest;
            else
            {
                string format = "Unknown request type '{0}'.";
                string message = string.Format(format, _status);
                WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "ProcessRequest", message);
                throw new InvalidRequestException(message);
            }

            return processRequestMethod();
        }

        private XmlDocument ProcessAbortRequest()
        {
            XmlDocument response = CreateResponseSkeleton();

            try
            {
                RequestManager.AbortRequest(_requestID, _requestSource, _requestSource,
                    string.Format("Request aborted by {0}.", _requestSource));
                Request theRequest = RequestManager.LoadRequestByExternalID(_requestID, _requestSource);

                SetDocumentCount(ref response, theRequest.ActualDocCount, theRequest.MaxPacketNumber);

                // Because the request is being aborted, there's no need to check the number of packets.
                SetResponseType(ref response, "OK", "Data Stream Aborted.");
            }
            catch (Exception ex)
            {
                // TODO: Log the full error info.
                WebServiceLogBuilder.Instance.CreateError(this.GetType(), "ProcessAbortRequest", ex);
                SetResponseType(ref response, "Error", ExceptionHelper.RetreiveInnermostMessage(ex));
            }

            return response;
        }

        private XmlDocument ProcessCloseRequest()
        {
            XmlDocument response = CreateResponseSkeleton();

            try
            {
                Request theRequest = RequestManager.LoadRequestByExternalID(_requestID, _requestSource);

                SetDocumentCount(ref response, theRequest.ActualDocCount, theRequest.MaxPacketNumber);

                // Check whether the document count matches.
                if (_expectedDocCount == theRequest.ActualDocCount &&
                    _expectedDocCount > 0)
                {
                    RequestManager.CompleteRequest(_requestID, _requestSource, _requestSource, _expectedDocCount);

                    // Reload the request with updated status, etc.
                    theRequest = RequestManager.LoadRequestByExternalID(_requestID, _requestSource);

                    SetResponseType(ref response, "OK", "Data Stream Complete.");
                    ScheduleBatch(theRequest);
                }
                else if (_expectedDocCount < 1)
                {
                    RequestManager.AbortRequest(_requestID, _requestSource, _requestSource, "Aborted - No packets received.");
                    SetResponseType(ref response, "Error", "The expected document count must be greater than zero.");
                }
                else
                {
                    RequestManager.AbortRequest(_requestID, _requestSource, _requestSource, "Aborted - Incorrect number of packets received.");
                    SetResponseType(ref response, "Error", "Incorrect packet count.");
                }
            }
            catch (Exception ex)
            {
                // TODO: Log the full error info.
                WebServiceLogBuilder.Instance.CreateError(this.GetType(), "ProcessCloseRequest", ex);
                SetResponseType(ref response, "Error", ExceptionHelper.RetreiveInnermostMessage(ex));
            }

            return response;
        }

        override protected void AddResponseBodyDetails(ref XmlDocument response)
        {
            XmlNode root = response.DocumentElement;
            XmlNode node;

            // Start a new sub-tree.
            node = response.CreateElement("PubEventResponse");
            root.AppendChild(node);

            // Point to base of sub-tree.
            root = node;

            // Echo back the pubType, lastJobID, and expected document counts.
            node = response.CreateElement("pubType");
            node.AppendChild(response.CreateTextNode(_pubType.ToString()));
            root.AppendChild(node);

            node = response.CreateElement("docCount");
            root.AppendChild(node);
        }

        #endregion

        #region Helper Methods

        private void SetDocumentCount(ref XmlDocument response, int actualCount, int maxPacket)
        {
            string docText = string.Format("{0}/{1}", actualCount, maxPacket);

            XmlNode node = response.SelectSingleNode("//PubEventResponse/docCount");
            if (node != null)
                node.AppendChild(response.CreateTextNode(docText));
            else
                // docCount node should have been created in AddResponseBodyDetails().
                throw new Exception("Node //PubEventResponse/docCount is missing.");
        }

        private void ScheduleBatch(Request theRequest)
        {
            /// If the request's publication target is GateKeeper, it's assumed that scheduling
            /// will be done manually.  Otherwise, a batch is submitted automatically.

            if (theRequest.PublicationTarget != RequestTargetType.GateKeeper)
            {
                ProcessActionType startAction = ProcessActionType.PromoteToStaging;
                ProcessActionType finalAction = GetFinalAction(theRequest.PublicationTarget);
                string batchName = GetBatchName(theRequest);

                Batch autoBatch = new Batch(batchName, "GateKeeper", startAction, finalAction);
                BatchManager.CreateNewBatch(ref autoBatch, theRequest.RequestID);
            }
        }

        private ProcessActionType GetFinalAction(RequestTargetType target)
        {
            ProcessActionType finalAction;

            if (target == RequestTargetType.Preview)
                finalAction = ProcessActionType.PromoteToPreview;
            else if (target == RequestTargetType.Live)
                finalAction = ProcessActionType.PromoteToLive;
            else
            {
                string message = string.Format("Unknown publication target: {0}", target.ToString());
                WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "GetFinalAction", message);
                throw new Exception(message);
            }

            return finalAction;
        }

        private string GetBatchName(Request theRequest)
        {
            string nameFormat = "Automatic batch for CDR request {0}";
            return string.Format(nameFormat, theRequest.ExternalRequestID);
        }

        #endregion
    }
}
