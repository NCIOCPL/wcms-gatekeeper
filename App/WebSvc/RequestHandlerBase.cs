using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;

using GateKeeper.Common;
using GKManagers;
using GKManagers.BusinessObjects;

namespace GateKeeper
{
    public abstract class RequestHandlerBase
    {
        abstract protected void AddResponseBodyDetails(ref XmlDocument response);

        private XmlNamespaceManager namespaceManager = null;

        protected XmlNamespaceManager cgNamespace
        {
            get { return namespaceManager; }
        }

        protected RequestHandlerBase()
        {
            namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("cg", "http://www.cancer.gov/webservices/");
        }

        protected XmlDocument CreateResponseSkeleton()
        {
            /// The details of the response body are specific to a particular handler,
            /// however the high-level container is shared across all handlers.
            XmlDocument body = new XmlDocument();
            XmlNode root;
            XmlNode node;

            body.AppendChild(body.CreateElement("Response"));
            root = body.DocumentElement;

            node = body.CreateElement("ResponseType");
            root.AppendChild(node);

            node = body.CreateElement("ResponseMessage");
            root.AppendChild(node);

            /// AddResponseBodyDetails() must be implemented in each derived class
            /// to create the specific handler's details
            AddResponseBodyDetails(ref body);

            return body;
        }

        protected void SetResponseType(ref XmlDocument response,
                                            string responseType,
                                            string responseMessage)
        {
            XmlNode node;

            node = response.SelectSingleNode("//ResponseType");
            node.AppendChild(response.CreateTextNode(responseType));

            node = response.SelectSingleNode("//ResponseMessage");
            node.AppendChild(response.CreateTextNode(responseMessage));
        }

        virtual protected void CheckSystemReadiness(out bool isReady, out string reason)
        {
            isReady = true;
            reason = "";

            try
            {
                string message;

                // Readiness check:  Is the system accepting requests? (System disabled control setting)
                string systemStatus = ConfigurationManager.AppSettings["WebServiceDisabled"];
                if (systemStatus == "Y")
                    throw new Exception("System configuration set to not accept requests at this time.");

                // Check database connectivity
                RequestManager.CheckDatabaseStatus(out isReady, out message);
                if (!isReady)
                {
                    WebServiceLogBuilder.Instance.CreateCritical(this.GetType(), "CheckSystemReadiness", message);
                    throw new Exception(message);
                }

                // Readiness check:  Is the DTD file in place?
                string dtdLocation = ConfigurationManager.AppSettings["DTDLocation"];
                if (!File.Exists(dtdLocation))
                {
                    message = string.Format("DTD file not found at {0}.", dtdLocation);
                    WebServiceLogBuilder.Instance.CreateCritical(this.GetType(), "CheckSystemReadiness", message);
                    throw new Exception(message);
                }

                // FUTURE: Readiness check:  Is the message queue in place?
            }
            catch (Exception ex)
            {
                isReady = false;
                reason = ex.Message;
            }
        }

        /// <summary>
        /// Retrieve the publication type (Hotfix|Export|Remove|Full Load) from a request message body.
        /// This method is only intended for use when handling a transmission phase for which the
        /// message body includes a pubType node as a child of the PubEvent node.  (Status check,
        /// Begin transmission and End transmission.)
        /// </summary>
        /// <param name="message">Request tranmission message</param>
        /// <returns>The request's publication type.</returns>
        virtual protected RequestPublicationType GetRequestPublicationType(XmlNode message)
        {
            RequestPublicationType pubType;
            XmlNode node;
            string nodeValue;
            int index;

            // Get the pubType node.
            node = message.SelectSingleNode("//cg:pubType/text()", cgNamespace);
            if (node == null)
                throw new InvalidRequestException("Missing node /PubEvent/pubType");

            /// Because we use an enumerated value, strip out any spaces so the value can be
            /// parsed by CLR functions.
            nodeValue = node.Value;
            while ((index = nodeValue.IndexOf(' ')) > -1 && nodeValue.Length > 0)
                nodeValue = nodeValue.Remove(index, 1);

            pubType = ConvertEnum<RequestPublicationType>.Convert(nodeValue);

            return pubType;
        }

        virtual protected RequestTargetType GetRequestTargetType(XmlNode message)
        {
            RequestTargetType target;
            XmlNode node;

            node = message.SelectSingleNode("//cg:pubTarget", cgNamespace);

            if (node != null && node.InnerText.Length > 0)
            {
                target = ConvertEnum<RequestTargetType>.Convert(node.InnerText);
            }
            else
            {
                // If no Target is specified, default to GateKeeper.
                target = RequestTargetType.GateKeeper;
            }

            return target;
        }
    }
}
