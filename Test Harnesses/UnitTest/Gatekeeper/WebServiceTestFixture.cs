using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;

using GKManagers.BusinessObjects;
using GKManagers;

using GKService = GateKeeper.UnitTest.GateKeeperWS.GateKeeper;


namespace GateKeeper.UnitTest.Gatekeeper
{
    [TestFixture]
    public class WebServiceTestFixture
    {
        #region Fields

        XmlDocument _requestDocXML1 = new XmlDocument();
        XmlDocument _requestDocXML2 = new XmlDocument();
        XmlDocument _requestDocXML3 = new XmlDocument();
        XmlDocument _requestDocXML4 = new XmlDocument();
        XmlDocument _requestDocXML5 = new XmlDocument();


        int _requestCdrid1 = 10;
        int _requestCdrid2 = 35023;
        int _requestCdrid3 = 30;
        //int _requestCdrid4 = 37779;

        XmlNamespaceManager _cgNamespace = null;

        string _sourceName = "NUNIT";


        #endregion

        #region Helpers

        private string CreateRequestName(string requestSource)
        {
            int newID;
            string oldID = RequestManager.GetMostRecentExternalID(requestSource);
            newID = int.Parse(oldID) + 1;
            return newID.ToString();
        }

        private string CreateStatusCheckBody( string pubType, string target)
        {
            string fmt = @"<PubEvent xmlns=""http://www.cancer.gov/webservices/""><pubType>{0}</pubType><pubTarget>{1}</pubTarget></PubEvent>";
            return string.Format(fmt, pubType, target);
        }

        private string CreateRequestStartMessageBody(RequestPublicationType pubType, 
            RequestTargetType target, string description, string lastJobID)
        {
            return CreateRequestStartMessageBody(pubType, target.ToString(), description, lastJobID);
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
            CDRDocumentType docType, int cdrID, string cdrVersion,int groupNumber, string documentXML)
        {
            string format = @"<PubData xmlns=""http://www.cancer.gov/webservices/""><docNum>{0}</docNum><transactionType>{1}</transactionType><CDRDoc Type=""{2}"" ID=""{3}"" Version=""{4}"" Group=""{5}"">{6}</CDRDoc></PubData>";

            if (documentXML == null)
                documentXML = "";

            return string.Format(format, packetNumber, actionType.ToString(), docType.ToString(),
                    cdrID, cdrVersion, groupNumber, documentXML);
        }

        private GKService GetGateKeeperService()
        {
            GKService service = new GKService();

            string location = ConfigurationManager.AppSettings["GateKeeperURL"];
            if (location != null && location.Length > 0)
                service.Url = location;

            return service;
        }

        #endregion

        #region Setup and Teardown

        /// <summary>
        /// Setup run once for all Unit Tests.
        /// </summary>
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            try
            {
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GENETICSPROFESSIONAL.xml"))
                {
                    this._requestDocXML1.PreserveWhitespace = true;
                    this._requestDocXML1.LoadXml(srBuffer.ReadToEnd());
                }

                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GlossaryTerm.xml"))
                {
                    this._requestDocXML2.PreserveWhitespace = true;
                    this._requestDocXML2.LoadXml(srBuffer.ReadToEnd());
                }

                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Media.xml"))
                {
                    this._requestDocXML3.PreserveWhitespace = true;
                    this._requestDocXML3.LoadXml(srBuffer.ReadToEnd());
                }

                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Terminology.xml"))
                {
                    this._requestDocXML4.PreserveWhitespace = true;
                    this._requestDocXML4.LoadXml(srBuffer.ReadToEnd());
                }

                using (StreamReader srBuffer = new StreamReader(@"./XMLData/SummaryEnglishHPWithReference62787.xml"))
                {
                    this._requestDocXML5.PreserveWhitespace = true;
                    this._requestDocXML5.LoadXml(srBuffer.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception loading test XML file: " + ex.Message);
                throw ex;
            }

            _cgNamespace = new XmlNamespaceManager(new NameTable());
            _cgNamespace.AddNamespace("cg", "http://www.cancer.gov/webservices/");

        }

        /// <summary>
        /// Teardown run once for all Unit Tests.
        /// </summary>
        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
        }

        /// <summary>
        /// Setup run for each Unit Test.
        /// </summary>
        [SetUp]
        public void UnitTestSetUp()
        {

        }

        /// <summary>
        /// Teardown run for each Unit Test.
        /// </summary>
        [TearDown]
        public void UnitTearDown()
        {

        }

        #endregion

        #region Unit Tests

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void StatusCheck()
        {

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.StatusCheck()...");

                GKService service = GetGateKeeperService();                

                XmlNode response;
                XmlText responseType;
                XmlText responseMessage;
                XmlText jobID;
                XmlText pubType;
                XmlDocument message = new XmlDocument();

                string[] pubTypeList = {RequestPublicationType.Export.ToString(),
                                        RequestPublicationType.FullLoad.ToString(),
                                        RequestPublicationType.Hotfix.ToString(),
                                        RequestPublicationType.Reload.ToString(),
                                        RequestPublicationType.Remove.ToString()};

                string[] targetList = { RequestTargetType.GateKeeper.ToString(),
                                        RequestTargetType.Live.ToString(),
                                        RequestTargetType.Preview.ToString(),
                                        RequestTargetType.GateKeeper.ToString(),
                                        RequestTargetType.Live.ToString()};

                Assert.AreEqual(pubTypeList.Length, targetList.Length, "List of pub types must be same size as list of targets.");

                string lastJobID = RequestManager.GetMostRecentExternalID(_sourceName);
                string messageBody;

                for (int index = 0; index < pubTypeList.Length; ++index)
                {
                    messageBody = CreateStatusCheckBody(pubTypeList[index], targetList[index]);
                    message.LoadXml(messageBody);

                    response = service.Request(_sourceName, "Status Check", message);
                    responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                    responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                    Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                    // Last job ID
                    jobID = (XmlText)response.SelectSingleNode("//PubEventResponse/lastJobID/text()");
                    Assert.IsNotNull(jobID, "lastJobID node not found");
                    Assert.AreEqual(lastJobID, jobID.Value, "Last Job ID should match");

                    // Publication type.
                    pubType = (XmlText)response.SelectSingleNode("//PubEventResponse/pubType/text()");
                    Assert.IsNotNull(pubType);
                    Assert.AreEqual(pubTypeList[index], pubType.Value, "/PubEvent/pubType value not echoed.");
                }
    
                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void BeginDataTransmission()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.GateKeeper;
            string description = "Test for BeginDataTransmission()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.BeginDataTransmission()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Test with a valid last known job id.
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                Request request1 = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.IsTrue(request1.RequestID != Request.InvalidRequestID, "Invalid Request ID");
                Assert.AreEqual(externalID, request1.ExternalRequestID, "ExternalRequestID");
                Assert.AreEqual(pubType, request1.RequestPublicationType, "RequestPublicationType");
                Assert.AreEqual(target, request1.PublicationTarget, "PublicationTarget");
                Assert.AreEqual(description, request1.Description, "Description");
                Assert.AreEqual(Request.IgnoreDocumentCount, request1.ExpectedDocCount, "ExpectedDocCount");
                Assert.AreEqual(RequestStatusType.Receiving, request1.Status, "Status");
                Assert.AreEqual(_sourceName, request1.Source, "Request Source");

                // Abort the job so we don't have an open request blocking new ones.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OpenWithRequestAlreadyOpen()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.GateKeeper;
            string description = "Test for OpenWithRequestAlreadyOpen()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.OpenWithRequestAlreadyOpen()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Test with a valid last known job id.
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Abort the job so we don't have an open request blocking new ones.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void AbortDataTransmission()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.GateKeeper;
            string description = "Test for AbortDataTransmission()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.AbortDataTransmission()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Test with a valid last known job id.
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Abort the job so we don't have an open request blocking new ones.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                Request request1 = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestStatusType.Aborted, request1.Status, "Status");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void EndDataTransmission()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for EndDataTransmission()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.EndDataTransmission()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 0, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                XmlText node = (XmlText)response.SelectSingleNode("PubEventResponse/docCount/text()");
                Assert.IsNotNull(node, "Node /Response/PubEventResponse/docCount missing or empty");
                Assert.AreEqual("0/0", node.Value, "Actual Count/Max Packet Number.");

                Request request1 = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestStatusType.Aborted, request1.Status, "Status");
                Assert.AreEqual(0, request1.ActualDocCount, "ActualDocCount");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void SendData()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for SendData()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.SendData()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                Request request1 = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.IsTrue(request1.RequestID != Request.InvalidRequestID, "Invalid Request ID");
                Assert.AreEqual(externalID, request1.ExternalRequestID, "ExternalRequestID");
                Assert.AreEqual(pubType, request1.RequestPublicationType, "RequestPublicationType");
                Assert.AreEqual(target, request1.PublicationTarget, "PublicationTarget");
                Assert.AreEqual(description, request1.Description, "Description");
                Assert.AreEqual(Request.IgnoreDocumentCount, request1.ExpectedDocCount, "ExpectedDocCount");
                Assert.AreEqual(RequestStatusType.Receiving, request1.Status, "Status");
                Assert.AreEqual(_sourceName, request1.Source, "Request Source");


                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Check actual packet count, max packet number.
                XmlText node = (XmlText)response.SelectSingleNode("PubEventResponse/docCount/text()");
                Assert.IsNotNull(node, "Node /Response/PubEventResponse/docCount missing or empty");
                Assert.AreEqual("1/1", node.Value, "Actual Count/Max Packet Number.");

                request1 = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestStatusType.DataReceived, request1.Status, "Status");
                Assert.AreEqual(1, request1.ActualDocCount, "ActualDocCount");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void CompleteTwice()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for SendData()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.SendData()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, "First Completion: " + responseMessage.Value);

                // Send the end signal a second time..
                messageBody = CreateRequestEndMessageBody(pubType, 1, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, "Second Completion: " + responseMessage.Value);


                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        [Ignore("Not Implemented")]
        public void FullLoadStart()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.FullLoad;
            RequestTargetType target = RequestTargetType.GateKeeper;
            string description = "Test for SendData()";

            XmlText responseType;
            XmlText responseMessage;
            XmlText node;

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.SendData()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                node = (XmlText)message.SelectSingleNode("//cg:pubType/text()", _cgNamespace);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                Request request1 = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(pubType, request1.RequestPublicationType, "RequestPublicationType");
                Assert.AreEqual(target, request1.PublicationTarget, "PublicationTarget");


                // Send abort.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                request1 = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestStatusType.Aborted, request1.Status, "Status");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void DroppedPacket()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for DroppedPacket()";
            int expectedDocCount = 4;   // Set expectation for four packets.

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.DroppedPacket()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Only send packet #3 (out of an expected 4).
                messageBody = CreateRequestDataMessageBody(3, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, expectedDocCount, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                // Verify report of only 1 actual document, max packet # is 3.
                XmlText node = (XmlText)response.SelectSingleNode("PubEventResponse/docCount/text()");
                Assert.IsNotNull(node, "Node /Response/PubEventResponse/docCount missing or empty");
                Assert.AreEqual("1/3", node.Value, "Actual Count/Max Packet Number.");

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);
                Assert.AreEqual("Incorrect packet count.", responseMessage.Value, "Response message.");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }
        
        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void BadSendOrder()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for BadSendOrder()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.BadSendOrder()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);
                
                // Send an "extra" request data item
                messageBody = CreateRequestDataMessageBody(2, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void InvalidRequestType()
        {

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.InvalidRequestType()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = "<PubEvent><pubType>InvalidRequestType</pubType><pubTarget>Staging</pubTarget><lastJobID>{0}</lastJobID></PubEvent>";
                messageBody = string.Format(messageBody, RequestManager.GetMostRecentExternalID(_sourceName));

                message.LoadXml(messageBody);

                XmlNode response = service.Request(_sourceName, "InvalidRequestType", message);
                Assert.IsNotNull(response, "NULL response from service.Request() call.");

                XmlNode node = response.SelectSingleNode("//ResponseType");
                Assert.IsNotNull(node, "Node not found //ResponseType");
                Assert.IsNotNull(node.InnerText, "Response type empty.");
                Assert.AreNotEqual("OK", node.InnerText, "Error note dedtci=");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// </summary>
        [Test]
        public void AbortCompletedTransmission()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for AbortCompletedTransmission()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.AbortCompletedTransmission()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Attempt to Abort the request even though it's already been completed
                messageBody = CreateRequestEndMessageBody(pubType, 0, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                /// Verify that the attempt failed.
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                // Completed transmission should remain completed.
                Request testRequest = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestStatusType.DataReceived, testRequest.Status,
                    "Request status should remain DataComplete");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// </summary>
        [Test]
        public void CompleteAbortedTransmission()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for CompleteAbortedTransmission()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.CompleteAbortedTransmission()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Abort the request
                messageBody = CreateRequestEndMessageBody(pubType, 0, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Attempt to complete the request even though it's been aborted
                messageBody = CreateRequestEndMessageBody(pubType, 1, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                // Aborted transmission should remain aborted.
                Request testRequest = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestStatusType.Aborted, testRequest.Status,
                    "Request status should remain Aborted");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// After a transmission has been closed, no further packets should be accepted.
        /// </summary>
        [Test]
        public void SendAfterTransmissionClose()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.GateKeeper;
            string description = "Test for SendAfterTransmissionClose()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.SendAfterTransmissionClose()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                XmlText node = (XmlText)response.SelectSingleNode("PubEventResponse/docCount/text()");
                Assert.IsNotNull(node, "Node /Response/PubEventResponse/docCount missing or empty");
                Assert.AreEqual("1/1", node.Value, "Actual Count/Max Packet Number.");

                /// The transmission has been closed.  No further packets should be accepted.
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GlossaryTerm, _requestCdrid2, "5", 1, _requestDocXML2.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                Request testRequest = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestStatusType.DataReceived, testRequest.Status,
                    "Request status should remain DataComplete");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// After a transmission has been aborted, no further packets should be accepted.
        /// </summary>
        [Test]
        public void SendAfterTransmissionAborted()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.GateKeeper;
            string description = "Test for SendAfterTransmissionAborted()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.SendAfterTransmissionAborted()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Abort the request.
                messageBody = CreateRequestEndMessageBody(pubType, 0, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                /// The transmission has been closed.  No further packets should be accepted.
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GlossaryTerm, _requestCdrid2, "5", 1, _requestDocXML2.OuterXml);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                Request testRequest = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestStatusType.Aborted, testRequest.Status,
                    "Request status should remain Aborted");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void ExternalRequestID()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.GateKeeper;
            string description = "Test for ExternalRequestID()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.ExternalRequestID()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Test with a valid last known job id.
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                Request request1 = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.IsTrue(request1.RequestID != Request.InvalidRequestID, "Invalid Request ID");
                Assert.AreEqual(externalID, request1.ExternalRequestID, "ExternalRequestID");
                Assert.AreEqual(pubType, request1.RequestPublicationType, "RequestPublicationType");
                Assert.AreEqual(target, request1.PublicationTarget, "PublicationTarget");
                Assert.AreEqual(description, request1.Description, "Description");
                Assert.AreEqual(Request.IgnoreDocumentCount, request1.ExpectedDocCount, "ExpectedDocCount");
                Assert.AreEqual(RequestStatusType.Receiving, request1.Status, "Status");
                Assert.AreEqual(_sourceName, request1.Source, "Request Source");

                // Abort the job so we don't have an open request blocking new ones.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// ExternalRequestID must be unique for a given data source.
        /// </summary>
        [Test]
        public void DuplicateExternalRequestID()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.GateKeeper;
            string description = "Test for DuplicateExternalRequestID()";

            try
            {
                XmlNode response;
                XmlText responseType;
                XmlText responseMessage;

                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.DuplicateExternalRequestID()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Test with a new job id.
                response = service.Request(_sourceName, externalID, message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Abort the job so we don't have an open request blocking new ones.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                // Make an attempt at re-using the same job id. This should not be allowed.
                messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// No publication target, should default to GateKeeper target.
        /// </summary>
        [Test]
        public void EmptyTarget()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            string description = "Test for SendData()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.SendData()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                // Create request without specifying the publication target.
                string messageBody = CreateRequestStartMessageBody(pubType, "", description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Verify that the target is GateKeeper
                Request request1 = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestTargetType.GateKeeper, request1.PublicationTarget,
                    "Wrong PublicationTarget -- Check default for the web service.");


                // Abort the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 0, "Abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void InvalidLastJobID()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.GateKeeper;
            string description = "Test for InvalidLastJobID()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.InvalidLastJobID()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    "Bogus Last Job ID");
                message.LoadXml(messageBody);

                // Test with an invalid last known job id.
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);


                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void MissingPublicationTarget()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Live;
            string description = "Test for MissingPublicationTarget()";

            string messageBody;
            XmlNode node, child;
            Request testRequest;
            XmlNode response;
            XmlText responseType;
            XmlText responseMessage;

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.MissingPublicationTarget()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                // Create job
                messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Blank the Target
                node = message.SelectSingleNode("//cg:pubTarget/text()", _cgNamespace);
                node.Value = null;

                // Send
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Verify the Target is GateKeeper
                testRequest = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestTargetType.GateKeeper, testRequest.PublicationTarget,
                        "Publication Target is empty string");

                // Abort the job so we don't have an open request blocking new ones.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);


                /// Repeat the test, this time outright removing the publication target.
                // Create job
                externalID = CreateRequestName(_sourceName);
                messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Delete the pubTarget node
                child = message.SelectSingleNode("//cg:pubTarget", _cgNamespace);
                node = child.ParentNode;
                node.RemoveChild(child);

                // Send
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Verify the Target is GateKeeper
                testRequest = RequestManager.LoadRequestByExternalID(externalID, _sourceName);
                Assert.AreEqual(RequestTargetType.GateKeeper, testRequest.PublicationTarget,
                        "Publication Target not present");

                // Abort the job so we don't have an open request blocking new ones.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);


                // Create Status check with empty target node.
                messageBody = CreateStatusCheckBody(RequestPublicationType.Reload.ToString(),"");
                message.LoadXml(messageBody);

                response = service.Request(_sourceName, "Status Check", message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, 
                    "Status check -- Publication target is empty. " + responseMessage.Value);

                /// Repeat the test, this time outright removing the publication target.
                child = message.SelectSingleNode("//cg:pubTarget", _cgNamespace);
                node = child.ParentNode;
                node.RemoveChild(child);

                response = service.Request(_sourceName, "Status Check", message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value,
                    "Status check -- Publication target is missing. " + responseMessage.Value);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Send a non-remove data packet without the document.
        /// </summary>
        [Test]
        public void EmptyDocument()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for SendData()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.SendData()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, null);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);


                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void RemoveOneDocument()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Remove;
            RequestTargetType target = RequestTargetType.Live;
            string description = "Test for RemoveOneDocument()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.RemoveOneDocument()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Remove,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, null);
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Check actual packet count, max packet number.
                XmlText node = (XmlText)response.SelectSingleNode("PubEventResponse/docCount/text()");
                Assert.IsNotNull(node, "Node /Response/PubEventResponse/docCount missing or empty");
                Assert.AreEqual("1/1", node.Value, "Actual Count/Max Packet Number.");


                Request testRequest = RequestManager.LoadRequestByExternalID(externalID, _sourceName);

                List<int> documentIDs = RequestManager.LoadRequestDataIDList(testRequest.RequestID);
                Assert.AreEqual(1, documentIDs.Count,
                    "testRequest.RequestDatas.Length - Wrong number of entries.");

                RequestData testData = RequestManager.LoadRequestDataByID(documentIDs[0]);
                Assert.AreEqual(RequestDataActionType.Remove, testData.ActionType, "ActionType");
                Assert.AreEqual(_requestCdrid1, testData.CdrID, "CdrID");
                Assert.AreEqual(CDRDocumentType.GENETICSPROFESSIONAL, testData.CDRDocType, "DocType");
                Assert.IsEmpty(testData.DocumentDataString, "DocumentDataString");

                Console.WriteLine("RequestID: {0}", testRequest.RequestID);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void SendDuplicatePackets()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for SendDuplicatePackets()";

            XmlNode response;
            XmlText responseType;
            XmlText responseMessage;

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.SendDuplicatePackets()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                response = service.Request(_sourceName, externalID, message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);

                response = service.Request(_sourceName, externalID, message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Send second data item.
                messageBody = CreateRequestDataMessageBody(2, RequestDataActionType.Export,
                    CDRDocumentType.GlossaryTerm, _requestCdrid2, "5", 2, _requestDocXML2.OuterXml);
                message.LoadXml(messageBody);

                response = service.Request(_sourceName, externalID, message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Duplicate the packet ID with a new document..
                messageBody = CreateRequestDataMessageBody(2, RequestDataActionType.Export,
                    CDRDocumentType.GlossaryTerm, _requestCdrid3, "5", 2, _requestDocXML2.OuterXml);
                message.LoadXml(messageBody);

                response = service.Request(_sourceName, externalID, message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, "Duplicate Packet # " + responseMessage.Value);

                // Duplicate the CDRID by sending the second data item again with a new packet number.
                messageBody = CreateRequestDataMessageBody(3, RequestDataActionType.Export,
                    CDRDocumentType.GlossaryTerm, _requestCdrid2, "5", 2, _requestDocXML2.OuterXml);
                message.LoadXml(messageBody);

                response = service.Request(_sourceName, externalID, message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, "Duplicate CDRID " + responseMessage.Value);

                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Send packets for a job which hasn't been created yet.
        /// </summary>
        [Test]
        public void SendToUnstartedJob()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            //RequestTargetType target = RequestTargetType.Preview;
            //string description = "Test for SendToUnstartedJob()";

            string messageBody;
            XmlNode response;
            XmlText responseType;
            XmlText responseMessage;

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.SendToUnstartedJob()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                //messageBody = CreateRequestStartMessageBody(pubType, target, description,
                //    RequestManager.GetMostRecentExternalID(_sourceName));
                //message.LoadXml(messageBody);

                //// Start a request
                //response = service.Request(_sourceName, externalID, message);
                //responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                //responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                //Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);

                response = service.Request(_sourceName, externalID, message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                // Send second data item.
                messageBody = CreateRequestDataMessageBody(2, RequestDataActionType.Export,
                    CDRDocumentType.GlossaryTerm, _requestCdrid2, "5", 2, _requestDocXML2.OuterXml);
                message.LoadXml(messageBody);

                response = service.Request(_sourceName, externalID, message);
                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 2, "complete");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void InvalidDocumentType()
        {
            string externalID = CreateRequestName(_sourceName);
            RequestPublicationType pubType = RequestPublicationType.Export;
            RequestTargetType target = RequestTargetType.Preview;
            string description = "Test for InvalidDocumentType()";

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing WebServiceTestFixture.InvalidDocumentType()...");

                GKService service = GetGateKeeperService();
                XmlDocument message = new XmlDocument();

                string messageBody = CreateRequestStartMessageBody(pubType, target, description,
                    RequestManager.GetMostRecentExternalID(_sourceName));
                message.LoadXml(messageBody);

                // Start a request
                XmlNode response = service.Request(_sourceName, externalID, message);
                XmlText responseType;
                XmlText responseMessage;

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);


                // Send request data
                messageBody = CreateRequestDataMessageBody(1, RequestDataActionType.Export,
                    CDRDocumentType.GENETICSPROFESSIONAL, _requestCdrid1, "5", 1, _requestDocXML1.OuterXml);
                message.LoadXml(messageBody);

                // Change the document type to an invalid value.
                XmlNode node = message.SelectSingleNode("//cg:CDRDoc", _cgNamespace);
                node.Attributes["Type"].Value = "INVALID DOCUMENT TYPE";

                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("Error", responseType.Value, responseMessage.Value);


                // Send the end request.
                messageBody = CreateRequestEndMessageBody(pubType, 1, "abort");
                message.LoadXml(messageBody);
                response = service.Request(_sourceName, externalID, message);

                responseType = (XmlText)response.SelectSingleNode("//ResponseType/text()");
                responseMessage = (XmlText)response.SelectSingleNode("//ResponseMessage/text()");
                Assert.AreEqual("OK", responseType.Value, responseMessage.Value);

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        #endregion
    }
}
