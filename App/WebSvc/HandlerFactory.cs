using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GateKeeper
{
    static class HandlerFactory
    {
        public static IRequestHandler CreateHandler(string source, string requestID, XmlNode message)
        {
            if (String.IsNullOrEmpty(source))
                throw new InvalidRequestException("'source' may not be empty.");
            if (String.IsNullOrEmpty( requestID ))
                throw new InvalidRequestException("'requestID' may not be empty");
            if (message == null)
                throw new InvalidRequestException("'message' may not be empty");

            IRequestHandler handler = null;
            string baseElementName = message.Name;

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("cg", "http://www.cancer.gov/webservices/");

            XmlNode docCount = message.SelectSingleNode("cg:docCount/text()", namespaceManager);
            XmlNode packetNumber = message.SelectSingleNode("cg:docNum/text()", namespaceManager);
            XmlNode docNode = message.SelectSingleNode("cg:CDRDoc", namespaceManager);
            XmlNode status = message.SelectSingleNode("cg:status", namespaceManager);

            if (baseElementName == "PubEvent" && string.Compare(requestID, "Status Check", true) == 0)
            {
                handler = new StatusCheckHandler(source, requestID, message);
            }
            else if (baseElementName == "PubEvent" && docCount == null && docNode == null )
            {
                handler = new RequestStartHandler(source, requestID, message);
            }
            else if (baseElementName == "PubEvent" && docCount != null && status != null && docNode == null)
            {
                handler = new RequestEndHandler(source, requestID, message);
            }
            else if (baseElementName == "PubData" &&
                    packetNumber != null && docNode != null)
            {
                handler = new RequestDataHandler(source, requestID, message);
            }
            else
            {
                string format = "RequestID: {0}\r\nSource: {1}\r\n Unrecognized request structure. {2}";
                string msg = string.Format(format, requestID, source, message.OuterXml);
                WebServiceLogBuilder.Instance.CreateError(typeof(HandlerFactory), "CreateHandler", msg);
                throw new InvalidRequestException(msg);
            }

            return handler;
        }
    }
}
