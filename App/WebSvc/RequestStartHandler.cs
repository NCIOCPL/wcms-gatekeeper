using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;

using GateKeeper.Common;
using GKManagers;
using GKManagers.BusinessObjects;

/*
 * The RequestStartHandler class responds to the initial request of a data transmission,
 * distinguished by the message body containing a docCount field with an expected document
 * count and the root node being named "PubEvent": 
 * 
 * The expected message body is:
 *  <PubEvent>
 *      <pubType>Hotfix|Export|Remove|Full Load</pubType>
 *      <pubTarget>Staging|Preview|Live</pubTarget>
 *      <description>Job description</description>
 *      <docCount>Expected document count or "Ignore"</docCount>
 *      <lastJobID>lastJobID</lastJobID>
 *  </PubEvent>
 * 
 * The response body is:
 *  <Response>
 *      <ResponseType>OK|Not Ready|Error</ResponseType>
 *      <ResponseMessage>message text</ResponseMessage>
 *      <PubEventResponse>
 *          <pubType>echoed from input</pubType>
 *          <lastJobID>lastKnownJobID</lastJobID>
 *      </PubEventResponse>
 *  </Response>
 * 
 */
namespace GateKeeper
{
    public class RequestStartHandler : RequestHandlerBase, IRequestHandler
    {
        private string _requestSource = null;
        private string _requestID = null;

        // Message parts
        private RequestPublicationType _pubType = RequestPublicationType.Invalid;
        private RequestTargetType _pubTarget = RequestTargetType.Invalid;
        private string _description;
        private string _lastJobID = null;

        #region Constructor

        public RequestStartHandler(string source, string requestID, XmlNode message)
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

            /// Retrieve publication target (Staging|Preview|Live)
            _pubTarget = GetRequestTargetType(message);

            node = message.SelectSingleNode("//cg:description/text()", cgNamespace);
            if (node == null)
                throw new InvalidRequestException("Missing node /PubEvent/description");
            _description = node.Value;

            node = message.SelectSingleNode("//cg:lastJobID/text()", cgNamespace);
            if (node == null)
                throw new InvalidRequestException("Missing node /PubEvent/lastJobID");
            _lastJobID = node.Value;
        }

        #endregion

        #region Methods

        public XmlDocument ProcessRequest()
        {
            XmlDocument response = CreateResponseSkeleton();

            try
            {
                bool systemIsReady = true;
                string notReadyReason;
                CheckSystemReadiness(out systemIsReady, out notReadyReason);

                if (systemIsReady)
                {
                    string currentDtdVersion = ConfigurationManager.AppSettings["DTDVersion"];

                    if (String.IsNullOrEmpty(currentDtdVersion))
                    {
                        currentDtdVersion = String.Empty; // Prevent crashes
                        WebServiceLogBuilder.Instance.CreateWarning(this.GetType(),
                            "ProcessRequest", "DTDVersion not set; extra DTD validation may occur.");
                    }

                    /// Check that the value reported as the job's lastJobID matches the last
                    /// value known by the GateKeeper system.  In the case of a mismatch, an
                    /// exception is thrown and reported in the catch block.
                    VerifyLastJobID();

                    /// Check for a maximum allowed promotion level (used for testing purposes to prevent
                    /// jobs from being automatically sent to the live system).
                    string maxPromotionLevel = ConfigurationManager.AppSettings["ForcedMaximumPromotionLevel"];
                    if (!String.IsNullOrEmpty(maxPromotionLevel))
                    {
                        RequestTargetType maxTarget = ConvertEnum<RequestTargetType>.Convert(maxPromotionLevel);
                        if (_pubTarget > maxTarget)
                        {
                            WebServiceLogBuilder.Instance.CreateInformation(this.GetType(),
                                "ProcessRequest",
                                string.Format("Overriding requested publication target {0} with {1}.",
                                    _pubTarget.ToString(), maxTarget.ToString()));
                            _pubTarget = maxTarget;
                        }
                    }

                    Request request = new Request(_requestID, _requestSource, _pubType, _pubTarget,
                        _description, "XML", currentDtdVersion, "GateKeeper");
                    RequestManager.CreateNewRequest(ref request);

                    SetResponseType(ref response, "OK", "Ready to Accept Data");
                }
                else
                {
                    SetResponseType(ref response, "Not Ready", notReadyReason);
                }
            }
            catch (Exception ex)
            {
                WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "ProcessRequest", ex);
                SetResponseType(ref response, "Error", ExceptionHelper.RetreiveInnermostMessage(ex));
            }

            return response;
        }

        /// <summary>
        /// Verifies that the last known job ID matches the system's idea of the last job ID.
        /// </summary>
        private void VerifyLastJobID()
        {
            string fmt = "lastJobID '{0}' does not match for request '{1}'. Last known job id is: '{2}'.";
            string message;

            string lastKnownJobID = RequestManager.GetMostRecentExternalID(_requestSource);
            if (lastKnownJobID != _lastJobID)
            {
                message = string.Format(fmt, _lastJobID, _requestID, lastKnownJobID);
                throw new Exception(message);
            }
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

            node = response.CreateElement("lastJobID");
            node.AppendChild(response.CreateTextNode(RequestManager.GetMostRecentExternalID(_requestSource)));
            root.AppendChild(node);
        }

        #endregion
    }
}
