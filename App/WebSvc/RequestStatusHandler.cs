using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

using GateKeeper.Common;
using GKManagers;
using GKManagers.BusinessObjects;

namespace GateKeeper
{
    class RequestStatusHandler: IRequestHandler
    {
        private enum StatusReportType
        {
            Invalid = -1,
            Summary = 0,
            DocumentLocation = 1,
            SingleDocument = 2
        };

        private string _source;
        private string _requestID;
        private StatusReportType _statusType;

        public RequestStatusHandler(string source, string requestID, string statusType)
        {
            string message;

            // Validate and retrieve source.
            if (String.IsNullOrEmpty(source ))
                throw new ArgumentException("'source' must not be empty.");
            _source = source;

            // Validate and retrieve status type.
            if (String.IsNullOrEmpty( statusType ))
                throw new ArgumentException("'statusType' must not be empty.");
            _statusType = ConvertEnum<StatusReportType>.Convert(statusType, StatusReportType.Invalid);
            if(_statusType == StatusReportType.Invalid)
            {
                string format = "Unrecognized statusType: {0}";
                message = string.Format(format, statusType);
                WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "RequestStatusHandler", message);
                throw new InvalidRequestException(message);
            }

            // Validate requestID.
            if (_statusType == StatusReportType.SingleDocument || _statusType == StatusReportType.Summary)
            {
                // RequestID is only required for SingleDocument and Summary reports.
                if (String.IsNullOrEmpty(requestID))
                {
                    message = "RequestID is required";
                    WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "RequestStatusHandler", message);
                    throw new InvalidRequestException(message);
                }

                int result = -1;
                if (!int.TryParse(requestID, out result))
                {
                    message = "RequestID should be an integer";
                    WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "RequestStatusHandler", message);
                    throw new InvalidRequestException(message);
                }
            }
            _requestID = requestID;
        }     


        public XmlDocument ProcessRequest()
        {
            XmlDocument xdoc = null;

            switch (_statusType)
            {
                case StatusReportType.Summary:
                    {
                        SqlParameter reqID = new SqlParameter("@requestID", SqlDbType.Int);
                        reqID.Value = int.Parse(_requestID);
                        xdoc = GetStatus("usp_getRequestStatusSummary", reqID);
                        WebServiceLogBuilder.Instance.CreateInformation(this.GetType(), "Process Request", "Request Id: " + _requestID); 
                    }
                    break;

                case StatusReportType.DocumentLocation:
                    {
                        List<RequestLocationExternalIds> locations = RequestManager.LoadAllDocumentLocationsExternalId();
                        xdoc = BuildAllDocumentsReport(locations);

                        //SqlParameter cdrID = new SqlParameter("@cdrID", SqlDbType.Int);
                        //cdrID.Value = 0;
                        //xdoc = GetStatus("usp_getRequestStatusDocumentHistory", cdrID);
                    }
                    break;

                case StatusReportType.SingleDocument:
                    {
                        int cdrID = int.Parse(_requestID);
                        List<RequestLocationExternalIds> locations = RequestManager.LoadSingleDocumentLocationExternalId(cdrID);
                        xdoc = BuildSingleDocumentReport(locations);
                    }
                    break;
                default:
                    xdoc =  SetResponseType(string.Format("Error - Unknown status type '{0}'.", _statusType ));
                    break;
            }

            return  xdoc;
        }

        protected XmlDocument SetResponseType(string detailedMessage)
        {
            XmlDocument response = new XmlDocument();
            XmlNode node;

            response.AppendChild(response.CreateElement("Response"));

            node = response.CreateElement("DetailedMessage");
            node.AppendChild(response.CreateTextNode(detailedMessage));

            return response;
        }

        private XmlDocument GetStatus(string sprocName, SqlParameter id)
        {
            XmlDocument response = new XmlDocument();
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["GateKeeper"].ToString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(sprocName, connection);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();

                try
                {
                    if (id != null)
                    {
                        cmd.Parameters.Add(id);
                    }

                    XmlReader reader = cmd.ExecuteXmlReader();
                    while (reader.Read())
                    {
                        response.Load(reader);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    string format = "Error -- {0}";
                    string message = string.Format(format, ex.Message);
                    string detailedErrorMessage = string.Format("Stored Proc: {0}. Sql Param Name: {1}. Sql Param Value: {2}", sprocName, id != null ? id.ToString() : "(NULL)", !System.DBNull.Value.Equals(id) ? id.Value.ToString() : "(NULL)");
                    WebServiceLogBuilder.Instance.CreateError(this.GetType(), "GetStatus", detailedErrorMessage, ex);
                    response = SetResponseType(message);
                }
            }
            return response;
        }

        private XmlDocument BuildSingleDocumentReport(List<RequestLocationExternalIds> locations)
        {
            XmlDocument report = new XmlDocument();

            report.AppendChild(report.CreateElement("Response"));

            XmlElement messageBody = report.CreateElement("detailedMessage");
            report.DocumentElement.AppendChild(messageBody);

            BuildDocumentList(messageBody, locations);

            return report;
        }

        private XmlDocument BuildAllDocumentsReport(List<RequestLocationExternalIds> locations)
        {
            XmlDocument report = new XmlDocument();

            report.AppendChild(report.CreateElement("Response"));

            if (locations != null)
            {
                // Add document count
                int docCount = locations.Count;
                XmlElement countNode = report.CreateElement("docCount");
                countNode.AppendChild(report.CreateTextNode(docCount.ToString()));
                report.DocumentElement.AppendChild(countNode);

                // Add document list.
                XmlElement messageBody = report.CreateElement("detailedMessage");
                report.DocumentElement.AppendChild(messageBody);
                BuildDocumentList(messageBody, locations);
            }

            return report;
        }

        private void BuildDocumentList(XmlElement parentNode, List<RequestLocationExternalIds> docInfo)
        {
            XmlDocument owner = parentNode.OwnerDocument;   // For creating nodes.

            XmlElement root = owner.CreateElement("documentList");
            XmlElement node;

            foreach (RequestLocationExternalIds doc in docInfo)
            {
                node = owner.CreateElement("document");

                AddAttribute(node, "cdrid", doc.CdrId.ToString());
                AddAttribute(node, "gatekeeper", FormatExternalId(doc.GKReqId));
                AddAttribute(node, "gatekeeperDateTime", FormatDate(doc.GKDate));
                AddAttribute(node, "preview", FormatExternalId(doc.PreviewReqId));
                AddAttribute(node, "previewDateTime", FormatDate(doc.PreviewDate));
                AddAttribute(node, "live", FormatExternalId(doc.LiveReqId));
                AddAttribute(node, "liveDateTime", FormatDate(doc.LiveDate));

                root.AppendChild(node);
            }

            parentNode.AppendChild(root);
        }

        private void AddAttribute(XmlElement node, string name, string value)
        {
            XmlAttribute attribute = node.OwnerDocument.CreateAttribute(name);
            attribute.Value = value;
            node.Attributes.Append(attribute);
        }

        private string FormatExternalId(string externalId)
        {
            string result = externalId;
            if (string.IsNullOrEmpty(result))
                result = "Not Present";
            return result;
        }

        private string FormatDate(DateTime date)
        {
            string result = "Not Present";
            if (date != DateTime.MinValue)
            {
                result = date.ToString("s");
            }
            return result;
        }
    }
}
