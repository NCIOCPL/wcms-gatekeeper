using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using GateKeeper.Common;
using GKManagers;
using GKManagers.BusinessObjects;

/*
 * The StatusCheckHandler class responds to "Is the system ready?" requests
 * (Marked by the requestID argument containing the value "Initiate Request".
 * 
 * The expected message body is:
 *  <PubEvent>
 *      <pubType>Hotfix|Export|Remove|Reload|Full Load</pubType>
 *      <pubTarget>Staging|Preview|Live</pubTarget>
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
    public class StatusCheckHandler : RequestHandlerBase, IRequestHandler
    {
        private string _requestSource = null;
        private string _requestID = null;

        // Message parts
        private RequestPublicationType _pubType = RequestPublicationType.Invalid;
        private RequestTargetType _pubTarget = RequestTargetType.Invalid;

        public StatusCheckHandler(string source, string requestID, XmlNode message) :
            base()
        {
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
        }

        public XmlDocument ProcessRequest()
        {
            XmlDocument response = CreateResponseSkeleton();
            XmlNode node;

            bool systemIsReady = true;
            string notReadyReason;
            CheckSystemReadiness(out systemIsReady, out notReadyReason);
            if (!systemIsReady)
            {
                SetResponseType(ref response, "Not Ready", notReadyReason);
            }

            if (systemIsReady)
            {
                // Report the last-known job ID
                string lastKnownExternID = RequestManager.GetMostRecentExternalID(_requestSource);
                node = response.SelectSingleNode("//PubEventResponse/lastJobID");
                node.AppendChild(response.CreateTextNode(lastKnownExternID));

                //  Report success
                SetResponseType(ref response, "OK", "Ready to Accept Data");
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

            /// Echo back the publication event type.
            /// (Last job ID is dependent on the database readiness check.)
            node = response.CreateElement("pubType");
            node.AppendChild(response.CreateTextNode(_pubType.ToString()));
            root.AppendChild(node);

            node = response.CreateElement("lastJobID");
            root.AppendChild(node);
        }
    }
}
