using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using GKService = GateKeeperTest.localhost.GateKeeper;

using GKManagers;
using GKManagers.BusinessObjects;

namespace GateKeeperTest
{
    public partial class WebServicePage : Form
    {
        private enum MyEnum
        {
            
        }

        const string REQUEST_SOURCE = "Web Service Test";

        GKService _webService;// = GetGateKeeperService()
        Request _webRequest;

        public WebServicePage()
        {
            InitializeComponent();

            try
            {
                urlEntryField.Text = ConfigurationManager.AppSettings["GateKeeperURL"];
                InitializeWebService();

                SetupDocumentList();
                SetupPublicationType();
                SetupPublicationTarget();

                sourceField.Text = REQUEST_SOURCE;
                lastJobID.Text = GetLastJobID();
                requestGroup.Enabled = true;
            }
            catch (Exception ex)
            {
                requestGroup.Enabled = false;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
            }
        }

        #region Form Initialization

        private void SetupDocumentList()
        {
            documentList.View = View.Details;
            documentList.Columns.Add("CDRID", -2, HorizontalAlignment.Left);
            documentList.Columns.Add("CDR Doc Type", -2, HorizontalAlignment.Left);
            documentList.Columns.Add("Action", -2, HorizontalAlignment.Left);
            documentList.Columns.Add("Data", -2, HorizontalAlignment.Left);
        }

        private void SetupPublicationType()
        {
            RequestPublicationType[] types = {
                        RequestPublicationType.Export,
                        RequestPublicationType.Hotfix,
                        RequestPublicationType.FullLoad};

            foreach (RequestPublicationType item in types)
            {
                publishTypeCombo.Items.Add(item);
            }
        }

        private void SetupPublicationTarget()
        {
            RequestTargetType[] targets ={
                RequestTargetType.GateKeeper,
                RequestTargetType.Preview,
                RequestTargetType.Live};

            TargetSelection.Items.Add("");
            foreach (RequestTargetType item in targets)
            {
                TargetSelection.Items.Add(item);
            }
        }

        #endregion

        private void initWebService_Click(object sender, EventArgs e)
        {
            try
            {
                InitializeWebService();
                lastJobID.Text = GetLastJobID();
                requestGroup.Enabled = true;
                MessageBox.Show("Updated");
            }
            catch (Exception ex)
            {
                requestGroup.Enabled = false;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
            }
        }

        private void StatusCheck_Click(object sender, EventArgs e)
        {
            string status, text;
            string message;

            InitializeWebService();
            GetStatusCheck(out status, out text);
            message = string.Format("Status: {0}\n\n  Text: {1}", status, text);
            MessageBox.Show(message, "Status Check", MessageBoxButtons.OK);
        }

        private void InitializeWebService()
        {
            string newUrl = urlEntryField.Text;

            if (!string.IsNullOrEmpty(newUrl))
            {
                _webService = new GKService();
                _webService.Url = newUrl;
            }
            else
            {
                _webService = null;
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog open = new OpenFileDialog();

                if (open.ShowDialog() == DialogResult.OK)
                {
                    _webRequest = (Request)RequestManager.DeserializeObject(open.OpenFile(), typeof(Request));
                    _webRequest.RequestPublicationType = RequestPublicationType.Import;
                    ShowRequestInformation(_webRequest);
                }

            }
            catch (Exception ex)
            {
                requestGroup.Enabled = false;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
            }
        }

        private void ShowRequestInformation(Request data)
        {
            descriptionText.Text = data.Description;

            documentList.Items.Clear();
            foreach (RequestData item in data.RequestDatas)
	        {
                string[] values = {item.CdrID.ToString(),
                    item.CDRDocType.ToString(),
                    item.ActionType.ToString(), item.DocumentDataString};
                ListViewItem insert = new ListViewItem(values);
                documentList.Items.Add(insert);
        	}
        }

        private void TransmitButton_Click(object sender, EventArgs e)
        {
            // TODO: Update from fields
            int lastJob, nextJob;

            try
            {
                ValidateInputs();

                lastJob = int.Parse(lastJobID.Text);

                if (newJobIDOverride.Checked)
                    nextJob = int.Parse(newJobIdField.Text);
                else
                    nextJob = lastJob + 1;

                _webRequest.Description = descriptionText.Text;
                _webRequest.RequestPublicationType = (RequestPublicationType)publishTypeCombo.SelectedItem;

                if (TargetSelection.SelectedItem != null && TargetSelection.SelectedItem != "")
                    _webRequest.PublicationTarget = (RequestTargetType)TargetSelection.SelectedItem;
                else
                    _webRequest.PublicationTarget = RequestTargetType.Invalid;

                TransmitRequest(_webRequest, lastJob, nextJob);

                MessageBox.Show("Transmission Succeeded.");
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Error", MessageBoxButtons.OK);
            }
            finally
            {
                lastJobID.Text = GetLastJobID();
                newJobIdField.Text = "";
            }
        }

        private void publishTypeCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox pubTypeCombo = sender as ComboBox;
            if ((RequestPublicationType)publishTypeCombo.SelectedItem == RequestPublicationType.FullLoad)
            {
                string message = "CAUTION: Sending a request of type FullLoad will result in a Remove request\nfor all documents not included in the request.\n\nAre you sure you want to do this?";
                if(MessageBox.Show(message, "CAUTION", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                    == DialogResult.Cancel)
                {
                    pubTypeCombo.SelectedItem=RequestPublicationType.Export;
                }
            }
        }

        private void JobIDOverride_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox jobOverride = sender as CheckBox;

            lastJobID.Enabled = jobOverride.Checked;

            // If the override was just cleared, reset the last job ID.
            if (!jobOverride.Checked)
            {
                lastJobID.Text = GetLastJobID();
            }
        }

        private void newJobIDOverride_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox jobOverride = sender as CheckBox;

            newJobIdField.Enabled = jobOverride.Checked;

            // If the override was just cleared, reset the next job ID.
            if (!jobOverride.Checked)
            {
                newJobIdField.Text = string.Empty;
            }
        }

        private void ValidateInputs()
        {
            if (_webRequest == null)
            {
                browseButton.Focus();
                throw new Exception("You must load a request file.");
            }

            if (String.IsNullOrEmpty(descriptionText.Text))
            {
                descriptionText.Focus();
                throw new Exception("You must supply a description for this request.");
            }

            if (publishTypeCombo.SelectedIndex < 0)
            {
                publishTypeCombo.Focus();
                throw new Exception("You must select a publication type.");
            }

            string textValue;
            int temp;
            textValue = lastJobID.Text;
            if (string.IsNullOrEmpty(textValue) ||
                !int.TryParse(textValue, out temp) ||
                temp < 0)
            {
                lastJobID.Focus();
                throw new Exception("Last Job ID must be a non-negative integer value.");
            }

            if (newJobIDOverride.Checked)
            {
                textValue = newJobIdField.Text;
                if (string.IsNullOrEmpty(textValue) ||
                    !int.TryParse(textValue, out temp) ||
                    temp < 0)
                {
                    newJobIdField.Focus();
                    throw new Exception("New Job ID must be a non-negative integer value.");
                }
            }
        }

        #region Web Service Calls

        private string CreateStatusCheckBody(string pubType, string target)
        {
            string fmt = @"<PubEvent xmlns=""http://www.cancer.gov/webservices/""><pubType>{0}</pubType><pubTarget>{1}</pubTarget></PubEvent>";
            return string.Format(fmt, pubType, target);
        }

        private string CreateRequestStartMessageBody(RequestPublicationType pubType,
            RequestTargetType target, string description, string lastJobID)
        {
            string targetString;

            if (target == RequestTargetType.Invalid)
                targetString = "";
            else
                targetString = target.ToString();

            return CreateRequestStartMessageBody(pubType, targetString, description, lastJobID);
        }

        private string CreateRequestStartMessageBody(RequestPublicationType pubType,
            string target, string description, string lastJobID)
        {
            string format = @"<PubEvent xmlns=""http://www.cancer.gov/webservices/""><pubType>{0}</pubType><pubTarget>{1}</pubTarget><description>{2}</description><lastJobID>{3}</lastJobID></PubEvent>";

            return string.Format(format, pubType.ToString(), target.ToString(), description,
                lastJobID);
        }

        private string CreateRequestEndMessageBody(RequestPublicationType pubType, int expectedCount, string status)
        {
            string format = @"<PubEvent xmlns=""http://www.cancer.gov/webservices/""><pubType>{0}</pubType><docCount>{1}</docCount><status>{2}</status></PubEvent>";
            return string.Format(format, pubType.ToString(), expectedCount, status);
        }

        private string CreateRequestDataMessageBody(int packetNumber, RequestDataActionType actionType,
            CDRDocumentType docType, int cdrID, string cdrVersion, int groupNumber, string documentXML)
        {
            string format = @"<PubData xmlns=""http://www.cancer.gov/webservices/""><docNum>{0}</docNum><transactionType>{1}</transactionType><CDRDoc Type=""{2}"" ID=""{3}"" Version=""{4}"" Group=""{5}"">{6}</CDRDoc></PubData>";

            if (documentXML == null)
                documentXML = "";

            return string.Format(format, packetNumber, actionType.ToString(), docType.ToString(),
                    cdrID, cdrVersion, groupNumber, documentXML);
        }

        private string GetLastJobID()
        {
            string messageBody = CreateStatusCheckBody(RequestPublicationType.Export.ToString(),
                                                    RequestTargetType.Live.ToString());
            XmlDocument message = new XmlDocument();
            message.LoadXml(messageBody);
            XmlNode response = _webService.Request(REQUEST_SOURCE, "Status Check", message);
            XmlText responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
            XmlText responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
            XmlText jobID;

            if (string.Compare(responseType.Value, "OK", true) != 0)
                throw new Exception(responseMessage.Value);

            jobID = (XmlText)response.SelectSingleNode("//PubEventResponse/lastJobID/text()");

            return jobID.Value;
        }

        private void GetStatusCheck(out string status, out string statusText)
        {
            string messageBody = CreateStatusCheckBody(RequestPublicationType.Export.ToString(),
                                                    RequestTargetType.Live.ToString());
            XmlDocument message = new XmlDocument();
            message.LoadXml(messageBody);
            XmlNode response = _webService.Request(REQUEST_SOURCE, "Status Check", message);
            XmlText responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
            XmlText responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");

            status = responseType.Value;
            statusText = responseMessage.Value;
        }

        private void TransmitRequest(Request data, int lastJob, int nextJob)
        {
            string messageBody;
            XmlDocument message = new XmlDocument();

            RequestData document;

            messageBody = CreateRequestStartMessageBody(data.RequestPublicationType,
                data.PublicationTarget, data.Description, lastJob.ToString());
            message.LoadXml(messageBody);
            TransmitRequestPart(nextJob, message);

            for (int i = 0; i < data.RequestDatas.Length; i++)
            {
                document = data.RequestDatas[i];
                messageBody = CreateRequestDataMessageBody(i + 1, document.ActionType, document.CDRDocType,
                    document.CdrID, document.CdrVersion, document.GroupID, document.DocumentDataString);
                message.LoadXml(messageBody);
                TransmitRequestPart(nextJob, message);
            }

            messageBody = CreateRequestEndMessageBody(data.RequestPublicationType, data.RequestDatas.Length, "complete");
            message.LoadXml(messageBody);
            TransmitRequestPart(nextJob, message);

        }

        private void TransmitRequestPart(int jobId, XmlNode message)
        {
            XmlNode response;
            XmlText status, statusText;

            response = _webService.Request(REQUEST_SOURCE, jobId.ToString(), message);
            status = (XmlText)response.SelectSingleNode("//ResponseType/text()");
            statusText = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");

            if (string.Compare(status.Value, "OK", true) != 0)
                throw new Exception(statusText.Value);
        }

        #endregion
    }
}