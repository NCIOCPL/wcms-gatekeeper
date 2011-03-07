using System;
using System.Collections;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Schema;

namespace GateKeeper
{
    /// <summary>
    /// This is the only interface from which the sources of data, such as CDR shall be able to push data to us.
    /// </summary>
    [WebService(Namespace = "http://www.cancer.gov/webservices/")]
    public class GateKeeper : System.Web.Services.WebService
    {

        // empty Constructor
        public GateKeeper()
        {
            //CODEGEN: This call is required by the ASP.NET Web Services Designer
            InitializeComponent();

        }

        #region Component Designer generated code

        //Required by the Web Services Designer 
        private IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// this SOAP web method processes requests to push data to CancerGov
        /// </summary>
        /// <param name="source">CDR, etc</param>
        /// <param name="requestID">Initiate Request, jobID</param>
        /// <param name="message">XML formated request</param>
        /// <returns>XML response</returns>
        [WebMethod(Description = "Request Processor")]
        //[TraceExtension(Filename="C:\\webresources\\gatekeeper\\trace.log")]
        public XmlDocument Request(string source, string requestID, XmlNode message)
        {
            XmlDocument response;
            IRequestHandler requestHandler;

            try
            {
                if (String.IsNullOrEmpty(source))
                    throw new InvalidRequestException("'source' may not be empty.");
                if (String.IsNullOrEmpty(requestID))
                    throw new InvalidRequestException("'requestID' may not be empty");
                if (message == null)
                    throw new InvalidRequestException("'message' may not be empty");

                requestHandler = HandlerFactory.CreateHandler(source, requestID, message);
                response = requestHandler.ProcessRequest();
            }
            catch (InvalidRequestException ex)
            {
                response = BuildErrorResponse("Error", ex.Message /*"Invalid Request"*/);
                string error = string.Format("Error: '{0}'\r\nRequest: {1}.", ex.ToString(), message.OuterXml);
                WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "Request", error, ex);
            }
            catch (Exception ex)
            {
                response = BuildErrorResponse("System Error", ex.ToString());
                string error = string.Format("Error: '{0}'\r\nRequest: {1}.", ex.Message, message.OuterXml);
                WebServiceLogBuilder.Instance.CreateError(this.GetType(), "Request", error, ex);
            }
            finally
            {
                // Remove reference to the requestHandler and any potentially large
                // objects it references.
                requestHandler = null;
            }

            return response;
        }


        /// <summary>
        /// A SOAP web method for retrieving information about requests.
        /// </summary>
        /// <param name="source">CDR, etc</param>
        /// <param name="requestID">jobiD</param>
        /// <param name="statusType">Summary, RequestLog, ProcessLog, or FullLog</param>
        /// <returns>an XML response of the status and/or log for this request</returns>
        [WebMethod(Description = "Request Status Report")]
        public XmlDocument RequestStatus(string source, string requestID, string statusType)
        {
            XmlDocument response = new XmlDocument();
            try
            {
                RequestStatusHandler requestHandler = new RequestStatusHandler(source, requestID, statusType);
                response = requestHandler.ProcessRequest();
            }
            catch (InvalidRequestException ex)
            {
                string message = ex.ToString(); // "Invalid Request";
                response = BuildErrorResponse("Error", message);
                WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "Request", message, ex);
            }
            catch (Exception ex)
            {
                string message = ex.ToString();
                response = BuildErrorResponse("System Error", message);
                WebServiceLogBuilder.Instance.CreateError(this.GetType(), "Request", message, ex);
            }

            return response;
        }

        protected XmlDocument BuildErrorResponse(string errorType, string message)
        {
            XmlDocument response = new XmlDocument();
            XmlNode node;
            XmlNode root;

            /// Building up the document with an XmlWriter would be less code, but building the
            /// nodes by hand saves the processing overhead of later re-parsing the document.
            response.AppendChild(response.CreateElement("Response"));
            root = response.DocumentElement;

            node = response.CreateElement("ResponseType");
            node.AppendChild(response.CreateTextNode(errorType));
            root.AppendChild(node);

            node = response.CreateElement("ResponseMessage");
            node.AppendChild(response.CreateTextNode(message));
            root.AppendChild(node);

            // TODO: Figure out what to do about a response body for errors.
            // Idea #1 --> Add an XmlNode "Response Body" parameter containing
            // as much information as possible.
            // Idea #2 --> Return the entire original request message body.

            return response;
        }
    }
}

