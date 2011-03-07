using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;

using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;


namespace GateKeeper.UnitTest.Gatekeeper
{
    [TestFixture]
    public class RequestTestFixture
    {
        #region Fields

        XmlDocument _requestDocXML1 = new XmlDocument();
        XmlDocument _requestDocXML2 = new XmlDocument();
        XmlDocument _requestDocXML3 = new XmlDocument();
        XmlDocument _requestDocXML4 = new XmlDocument();

        int _requestCdrid1 = 10;
        int _requestCdrid2 = 350231;
        int _requestCdrid3 = 30;
        int _requestCdrid4 = 37779;

        XmlDocument requestSummaryDocXML1 = new XmlDocument();
        int _requestSummaryCdrid1 = 62787;

        Random _rng = new Random();

        const string TEST_USER_ID = "Test User";
        const string REQUEST_SOURCE = "NUNIT";

        #endregion

        #region Helpers

        private string CreateRequestName(string requestSource)
        {
            int newID;
            string oldID = RequestManager.GetMostRecentExternalID(requestSource);
            newID = int.Parse(oldID) + 1;
            return newID.ToString();
        }

        #endregion

        #region Setup and Teardown

        // Preserve the system's processing state.
        SystemStatusType _orignalRunState;

        /// <summary>
        /// Setup run once for all Unit Tests.
        /// </summary>
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            // Many tests in this fixture require the system to be in a processing state.
            _orignalRunState = RequestManager.GetGateKeeperSystemStatus();
            RequestManager.StartGateKeeperSystem();

            try
            {
                // CDRID = 10
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GENETICSPROFESSIONAL.xml"))
                {
                    this._requestDocXML1.PreserveWhitespace = true;
                    this._requestDocXML1.LoadXml(srBuffer.ReadToEnd());
                }

                // CDRID = CDR0000350231
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GlossaryTerm.xml"))
                {
                    this._requestDocXML2.PreserveWhitespace = true;
                    this._requestDocXML2.LoadXml(srBuffer.ReadToEnd());
                }

                // CDRID = not embedded.
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Media.xml"))
                {
                    this._requestDocXML3.PreserveWhitespace = true;
                    this._requestDocXML3.LoadXml(srBuffer.ReadToEnd());
                }

                // CDRID = CDR0000037779
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Terminology.xml"))
                {
                    this._requestDocXML4.PreserveWhitespace = true;
                    this._requestDocXML4.LoadXml(srBuffer.ReadToEnd());
                }

                // CDRID = CDR0000062787
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/SummaryEnglishHPWithReference62787.xml"))
                {
                    this.requestSummaryDocXML1.PreserveWhitespace = true;
                    this.requestSummaryDocXML1.LoadXml(srBuffer.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception loading test XML file: " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Teardown run once for all Unit Tests.
        /// </summary>
        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            // Restore the system's original processing state.
            if (_orignalRunState == SystemStatusType.Normal)
                RequestManager.StartGateKeeperSystem();
            else
                RequestManager.StopGateKeeperSystem();
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
        public void RequestDataObject()
        {

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CreateNewRequest()...");

                string xmlInput1 = _requestDocXML1.OuterXml;
                string xmlInput2 = _requestDocXML2.OuterXml;

                int packetNumber = 1;
                RequestDataActionType actionType = RequestDataActionType.Export;
                int cdrID = _requestCdrid1;
                string cdrVersion = "4.56";
                CDRDocumentType docType = CDRDocumentType.GENETICSPROFESSIONAL;
                RequestDataLocationType location = RequestDataLocationType.Preview;
                int groupID = 81;

                RequestData data = RequestDataFactory.Create(docType, packetNumber, actionType,
                    cdrID, cdrVersion, location, groupID, xmlInput1);

                Assert.AreEqual(packetNumber, data.PacketNumber, "PacketNumber failed.");
                Assert.AreEqual(actionType, data.ActionType, "ActionType failed.");
                Assert.AreEqual(cdrID, data.CdrID, "CdrID failed");
                Assert.AreEqual(docType, data.CDRDocType, "CdrDocumentType failed.");
                Assert.AreEqual(location, data.Location, "Location failed.");
                Assert.AreEqual(groupID, data.GroupID, "GroupID failed.");

                Assert.AreEqual(xmlInput1, data.DocumentDataString,
                    "DocumentDataString did not match constructor input.");

                data.DocumentDataString = xmlInput2;
                Assert.AreEqual(xmlInput2, data.DocumentData.OuterXml,
                    "DocumentData.OuterXml did not match DocumentDataString");

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
        public void CreateNewRequest()
        {

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CreateNewRequest()...");

                string requestName = CreateRequestName(REQUEST_SOURCE);

                Request request1 = new Request(requestName, REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test Request Document", "XML", "1.234", TEST_USER_ID);

                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");
                Assert.IsTrue(request1.RequestID > 0,
                    "CreateNewRequest did not modify request1 to contain a valid requestID.");
                Assert.IsTrue(request1.Status == RequestStatusType.Receiving,
                    "CreateNewRequest did not set the new request to Receiving.");

                RequestManager.AbortRequest(requestName, REQUEST_SOURCE, TEST_USER_ID);

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
        public void CreateNewRequestTwice()
        {

            string externalRequestID = CreateRequestName(REQUEST_SOURCE);

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CreateNewRequestTwice()...");

                Request request1 = new Request(externalRequestID, REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test Request Document", "XML", "1.234", TEST_USER_ID);

                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");
                Assert.IsTrue(request1.Status == RequestStatusType.Receiving,
                    "CreateNewRequest did not set the new request to Receiving.");

                RequestManager.AbortRequest(externalRequestID, REQUEST_SOURCE, TEST_USER_ID);

                success = RequestManager.CreateNewRequest(ref request1);

                // Execution should not reach this point.  (An exception should be thrown instead.)
                Assert.Fail("CreateNewRequest did not diagnose the attempt to reuse an external ID.");

                Console.WriteLine("========================================");
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                string originalMessage = ExceptionHelper.RetreiveInnermostMessage(ex);

                // Check whether this is the error we're testing for.
                if (originalMessage.IndexOf("Error (-2):") != 0)
                {
                    Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());

                    RequestManager.AbortRequest(externalRequestID, REQUEST_SOURCE, TEST_USER_ID);

                    Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
                }
                else
                    Console.WriteLine("========================================");
            }
        }


        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void CompleteRequest()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CreateNewRequest()...");

                string externalID = CreateRequestName(REQUEST_SOURCE);

                Request request1 = new Request(externalID, REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test Request Document", "XML", "1.234", TEST_USER_ID);

                // Create a request
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");
                Assert.IsTrue(request1.RequestID > 0,
                    "CreateNewRequest did not modify request1 to contain a valid requestID.");
                Assert.AreEqual(request1.Status, RequestStatusType.Receiving,
                    "CreateNewRequest did not set the new request to Receiving.");

                // Add a document
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML1.OuterXml);
                success =
                    RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document1);

                // Mark the request as completed (expect 5 docs even though only 1 was provided)
                success = RequestManager.CompleteRequest(externalID, REQUEST_SOURCE, "NUNIT Tester", 5);
                Assert.IsTrue(success, "CompleteRequest failed");

                Request request2 = RequestManager.LoadRequestByID(request1.RequestID);

                Assert.AreEqual(request2.Status, RequestStatusType.DataReceived);

                Assert.AreEqual(5, request2.ExpectedDocCount, "Incorrect ExpectedDocCount");
                Assert.AreEqual(1, request2.ActualDocCount, "Incorrect ActualDocCount");

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
        public void CompleteRequestTwice()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CompleteRequestTwice()...");

                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test Request Document", "XML", "1.234", TEST_USER_ID);

                // Create a Request.
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");
                Assert.IsTrue(request1.RequestID > 0,
                    "CreateNewRequest did not modify request1 to contain a valid requestID.");
                Assert.AreEqual(request1.Status, RequestStatusType.Receiving,
                    "CreateNewRequest did not set the new request to Receiving.");

                // Mark it complete
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, "NUNIT Tester", 0);
                Assert.IsTrue(success, "CompleteRequest failed on first close attempt.");

                Request request2 = RequestManager.LoadRequestByID(request1.RequestID);
                Assert.AreEqual(request2.Status, RequestStatusType.DataReceived);

                // Complete the test a second time.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, "NUNIT Tester", 0);
                Assert.IsTrue(success, "CompleteRequest reported an error on the second close attempt.");

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
        public void InsertRequestData()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.InsertRequestData1()...");

                string externalID = CreateRequestName(REQUEST_SOURCE);

                Request request1 = new Request(externalID, REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test Request Document", "XML", "1.234", TEST_USER_ID);

                bool isCreated = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(isCreated, "CreateNewRequest failed.");

                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML1.OuterXml);

                bool succeeded =
                    RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document1);
                Assert.IsTrue(succeeded, "InsertRequestData failed.");
                Assert.IsTrue(request1.RequestID > 0,
                    "InsertRequestData did not modify document1 to contain a valid requestDataID.");

                RequestManager.AbortRequest(externalID, REQUEST_SOURCE, TEST_USER_ID);

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
        public void InsertIntoClosedRequest()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.InsertRequestData1()...");

                string externalID = CreateRequestName(REQUEST_SOURCE);

                Request request1 = new Request(externalID, REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test Request Document", "XML", "1.234", TEST_USER_ID);

                bool isCreated = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(isCreated, "CreateNewRequest failed.");

                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML1.OuterXml);

                bool succeeded =
                    RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document1);
                Assert.IsTrue(succeeded, "Insert into open request failed.");

                RequestManager.CompleteRequest(externalID, REQUEST_SOURCE, TEST_USER_ID, 1);

                // Attempt to insert a document after the request has been completed.
                try
                {
                    RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                        2, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                        _requestDocXML1.OuterXml);

                    // This should throw an exception.
                    succeeded = RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document2);
                    Assert.Fail("Successful insert into closed request.");
                }
                catch (NUnit.Framework.AssertionException)
                {
                    // Don't discard the NUnit exception.
                    throw;
                }
                catch (Exception)
                {
                    // Do discard the expected exception.
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
        public void LoadRequestByID()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadRequestByID()...");

                string externalRequestID = CreateRequestName(REQUEST_SOURCE);
                RequestPublicationType pubType = RequestPublicationType.Export;
                RequestTargetType pubTarget = RequestTargetType.Live;
                string description = "Test Request Document";
                string dataType = "XML";
                string dtdVersion = "1.234";

                Request request1 = new Request(externalRequestID, REQUEST_SOURCE, pubType, 
                    pubTarget, description, dataType, dtdVersion, TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);

                Assert.IsTrue(success, "CreateNewRequest() failed.");
                Assert.IsTrue(request1.RequestID > 0, "Request ID not set.");

                Request request2 = RequestManager.LoadRequestByID(request1.RequestID);

                Assert.AreEqual(request1.RequestID, request2.RequestID, "RequestID failed.");
                Assert.AreEqual(externalRequestID, request2.ExternalRequestID, "External ID failed");
                Assert.AreEqual(REQUEST_SOURCE, request2.Source, "Source failed");
                Assert.AreEqual(pubType, request2.RequestPublicationType, "RequestPublicationType failed.");
                Assert.AreEqual(pubTarget, request2.PublicationTarget, "PublicationTarget failed.");
                Assert.AreEqual(description, request2.Description, "Description failed.");
                Assert.AreEqual(dataType, request2.DataType, "DataType failed.");
                Assert.AreEqual(dtdVersion, request2.DtdVersion, "DtdVersion failed.");
                Assert.AreEqual(TEST_USER_ID, request2.UpdateUserID, "UpdateUserID failed.");

                RequestManager.AbortRequest(externalRequestID, REQUEST_SOURCE, TEST_USER_ID);

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
        public void LoadRequestByExternalID()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadRequestByID()...");

                string externalRequestID = CreateRequestName(REQUEST_SOURCE);
                RequestPublicationType pubType = RequestPublicationType.Export;
                RequestTargetType pubTarget = RequestTargetType.Live;
                string description = "Test Request Document";
                string dataType = "XML";
                string dtdVersion = "1.234";

                Request request1 = new Request(externalRequestID, REQUEST_SOURCE, pubType, 
                    pubTarget, description, dataType, dtdVersion, TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);

                Assert.IsTrue(success, "CreateNewRequest() failed.");
                Assert.IsTrue(request1.RequestID > 0, "Request ID not set.");

                Request request2 = RequestManager.LoadRequestByExternalID(externalRequestID, REQUEST_SOURCE);

                Assert.AreEqual(request1.RequestID, request2.RequestID, "RequestID failed.");
                Assert.AreEqual(externalRequestID, request2.ExternalRequestID, "External ID failed");
                Assert.AreEqual(REQUEST_SOURCE, request2.Source, "Source failed");
                Assert.AreEqual(pubType, request2.RequestPublicationType, "RequestPublicationType failed.");
                Assert.AreEqual(pubTarget, request2.PublicationTarget, "PublicationTarget failed.");
                Assert.AreEqual(description, request2.Description, "Description failed.");
                Assert.AreEqual(dataType, request2.DataType, "DataType failed.");
                Assert.AreEqual(dtdVersion, request2.DtdVersion, "DtdVersion failed.");
                Assert.AreEqual(TEST_USER_ID, request2.UpdateUserID, "UpdateUserID failed.");

                RequestManager.AbortRequest(externalRequestID, REQUEST_SOURCE, TEST_USER_ID);

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
        public void LoadRequestDataByCDRID()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadRequestDataByCDRID()...");

                int targetCDRID = 30;

                string externalRequestID = CreateRequestName(REQUEST_SOURCE);

                Request request1 = new Request(externalRequestID, REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- LoadRequestDataByCDRID", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");


                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML1.OuterXml);
                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML2.OuterXml);
                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Media,
                    3, RequestDataActionType.Export, _requestCdrid3, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML3.OuterXml);
                RequestData document4 = RequestDataFactory.Create(CDRDocumentType.Term,
                    4, RequestDataActionType.Export, _requestCdrid4, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML4.OuterXml);
                RequestData document5 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    5, RequestDataActionType.Export, _requestSummaryCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    requestSummaryDocXML1.OuterXml);

                // Add documents to the request.
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document4);
                Assert.IsTrue(success, "InsertRequestData - document4");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document5);
                Assert.IsTrue(success, "InsertRequestData - document5");

                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 5);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                RequestData requestData1 =
                    RequestManager.LoadRequestDataByCdrid(request1.RequestID, targetCDRID);
                Assert.IsNotNull(requestData1);

                Assert.AreEqual(document3.RequestDataID, requestData1.RequestDataID);
                Assert.AreEqual(document3.DocumentDataString, requestData1.DocumentDataString);

                Request request2 = RequestManager.LoadRequestByID(request1.RequestID);
                Assert.AreEqual(5, request2.ExpectedDocCount, "Incorrect Exepcted Document Count");
                Assert.AreEqual(5, request2.ActualDocCount, "Incorrect Actual Document Count");

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
        public void LoadRequestDataByID()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadRequestDataByID()...");

                string externalRequestID = CreateRequestName(REQUEST_SOURCE);

                Request request1 = new Request(externalRequestID, REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- LoadRequestDataByCDRID", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");


                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML1.OuterXml);
                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML2.OuterXml);
                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Media,
                    3, RequestDataActionType.Export, _requestCdrid3, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML3.OuterXml);
                RequestData document4 = RequestDataFactory.Create(CDRDocumentType.Term,
                    4, RequestDataActionType.Export, _requestCdrid4, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML4.OuterXml);
                RequestData document5 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    5, RequestDataActionType.Export, _requestSummaryCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    requestSummaryDocXML1.OuterXml);

                // Add documents to the request.
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document4);
                Assert.IsTrue(success, "InsertRequestData - document4");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document5);
                Assert.IsTrue(success, "InsertRequestData - document5");

                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 5);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");


                RequestData requestData1 =
                    RequestManager.LoadRequestDataByID(document3.RequestDataID);
                Assert.IsNotNull(requestData1);

                Assert.AreEqual(document3.RequestDataID, requestData1.RequestDataID);
                Assert.AreEqual(document3.DocumentDataString, requestData1.DocumentDataString);
                Assert.AreEqual(document3.CdrID, requestData1.CdrID);

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
        public void LoadRequestDataInfo()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadRequestDataByID()...");

                string externalRequestID = CreateRequestName(REQUEST_SOURCE);

                Request request1 = new Request(externalRequestID, REQUEST_SOURCE, RequestPublicationType.Export,
                    RequestTargetType.Live, "Test -- LoadRequestDataByCDRID", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");


                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML1.OuterXml);
                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML2.OuterXml);
                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Media,
                    3, RequestDataActionType.Export, _requestCdrid3, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML3.OuterXml);

                // Add documents to the request.
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 5);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");


                RequestDataInfo requestDataInfo =
                    RequestManager.LoadRequestDataInfo(document3.RequestDataID);
                Assert.IsNotNull(requestDataInfo);

                Assert.AreEqual(document3.RequestDataID, requestDataInfo.RequestDataID, "RequestDataID");
                Assert.AreEqual(document3.CdrID, requestDataInfo.CdrID, "CdrID");
                Assert.AreEqual(document3.GKDocType, requestDataInfo.GKDocType, "GKDocType");

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
        public void LoadRequestDataIDList()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadRequestDataByID()...");

                string externalRequestID = CreateRequestName(REQUEST_SOURCE);

                // Create a request.
                Request request1 = new Request(externalRequestID, REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- LoadRequestDataByCDRID", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add three documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    _requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    3, RequestDataActionType.Export, _requestCdrid3, "1.0", RequestDataLocationType.GateKeeper, 1, 
                    requestSummaryDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 3);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                // Load the list of RequestDataIDs
                List<int> requestDataIDs =
                    RequestManager.LoadRequestDataIDList(request1.RequestID);
                Assert.IsNotNull(requestDataIDs);
                Assert.AreEqual(3, requestDataIDs.Count, "LoadRequestDataIDList returned the wrong number of IDs.");

                Assert.IsTrue(requestDataIDs.Contains(document1.RequestDataID));
                Assert.IsTrue(requestDataIDs.Contains(document2.RequestDataID));
                Assert.IsTrue(requestDataIDs.Contains(document3.RequestDataID));

                Assert.IsFalse(requestDataIDs.Contains(0));

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
        public void MarkDocumentWithErrors()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.MarkDocumentWithErrors()...");

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export,
                    RequestTargetType.Live, "MarkDocumentWithWarnings", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add three documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    3, RequestDataActionType.Export, _requestCdrid3, "1.0", RequestDataLocationType.GateKeeper, 2,
                    requestSummaryDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 3);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                // Document 1 & 2 belong to group 1
                // Document 3 belongs to group 2

                RequestManager.MarkDocumentWithErrors(document1.RequestDataID);

                /// Expected Results:
                /// testDoc1 - Status == Error
                /// testDoc2 - DependencyStatus == Error
                /// testDoc3 - OK

                RequestData testDoc1;
                RequestData testDoc2;
                RequestData testDoc3;

                testDoc1 = RequestManager.LoadRequestDataByID(document1.RequestDataID);
                testDoc2 = RequestManager.LoadRequestDataByID(document2.RequestDataID);
                testDoc3 = RequestManager.LoadRequestDataByID(document3.RequestDataID);

                Assert.AreEqual(RequestDataStatusType.Error, testDoc1.Status,
                    "testDoc1 is not marked as Error.");
                Assert.AreEqual(RequestDataDependentStatusType.Error, testDoc2.DependencyStatus,
                    "testDoc2 is not marked as Dependency Error.");
                Assert.AreEqual(RequestDataStatusType.OK, testDoc3.Status,
                    "testDoc3 is marked as Error.");
                Assert.AreEqual(RequestDataDependentStatusType.OK, testDoc3.DependencyStatus,
                    "testDoc3 is marked as Dependency Error.");

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
        public void MarkDocumentWithWarnings()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.MarkDocumentWithWarnings()...");

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export,
                    RequestTargetType.Live, "MarkDocumentWithWarnings", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add two documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 2,
                    _requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 2);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                int docId1 = document1.RequestDataID;
                int docId2 = document2.RequestDataID;

                // Verify basic warning.
                RequestManager.MarkDocumentWithWarnings(docId1);
                RequestData testDoc = RequestManager.LoadRequestDataByID(docId1);
                Assert.AreEqual(RequestDataStatusType.Warning, testDoc.Status, "RequestData.Status");

                // Verify warning status does not override error.
                RequestManager.MarkDocumentWithErrors(docId2);
                RequestManager.MarkDocumentWithWarnings(docId2);
                testDoc = RequestManager.LoadRequestDataByID(docId2);
                Assert.AreEqual(RequestDataStatusType.Error, testDoc.Status, "RequestData.Status -- Warning should not overwrite Error.");

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
        public void Validation()
        {

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.Validation()...");

                string xmlInput1 = _requestDocXML2.OuterXml;

                int packetNumber = 1;
                RequestDataActionType actionType = RequestDataActionType.Export;
                int cdrID = _requestCdrid2;
                string cdrVersion = "4.56";
                CDRDocumentType docType = CDRDocumentType.GlossaryTerm;
                RequestDataLocationType location = RequestDataLocationType.Preview;
                int groupID = 81;

                RequestData data = RequestDataFactory.Create(docType, packetNumber, actionType,
                    cdrID, cdrVersion, location, groupID, xmlInput1);

                string result = RequestManager.ValidateRequestData(data);
                Assert.IsNull(result, "Validation Failed. Expected null, got: " + result);

                // Build a new RequestData object, with an intentionally wrong document type.
                docType = CDRDocumentType.GENETICSPROFESSIONAL;

                data = RequestDataFactory.Create(docType, packetNumber, actionType,
                    cdrID, cdrVersion, location, groupID, xmlInput1);
                result = RequestManager.ValidateRequestData(data);
                Assert.IsNotNull(result, "Validation should have failed.");


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
        /// Attempt to promote a document with validation errors.
        /// </summary>
        [Test]
        public void ValidationOnInsert()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.ValidationOnInsert()...");

                RequestData document;

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export,
                    RequestTargetType.Live, "Test -- ValidationOnInsert", "XML", "Invalid DTD version", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add one document to the request with the correct document type.
                document = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    1, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document);
                Assert.IsTrue(success, "InsertRequestData - document 1");

                // Add one document to the request with an incorrect document type.
                document = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML1.OuterXml);

                try
                {
                    success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                        TEST_USER_ID, ref document);
                    Assert.Fail("InsertRequestData failed to diagnose a DTD validation failure.");
                }
                catch
                {
                    // An exception is the expected result.
                }

                // Aborrt the request.
                success = RequestManager.AbortRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID);
                Assert.IsTrue(success, "GKManagers.AbortRequest failed");


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
        /// Test basic database connectivity.
        /// </summary>
        [Test]
        public void DatabaseCheck()
        {

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.DatabaseCheck()...");

                bool ready;
                string message;
                RequestManager.CheckDatabaseStatus(out ready, out message);

                Assert.IsTrue(ready && string.IsNullOrEmpty(message),
                    string.Format("Database not ready. {0}", message));

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
        /// Test basic database connectivity.
        /// </summary>
        [Test]
        public void GatekeeperSystemStatus()
        {

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.DatabaseCheck()...");

                SystemStatusType originalState;

                // Preserve original state
                originalState = RequestManager.GetGateKeeperSystemStatus();

                RequestManager.StartGateKeeperSystem();
                Assert.AreEqual(SystemStatusType.Normal, RequestManager.GetGateKeeperSystemStatus(),
                        "Failed to start GateKeeper system.");

                RequestManager.StopGateKeeperSystem();
                Assert.AreEqual(SystemStatusType.Stopped, RequestManager.GetGateKeeperSystemStatus(),
                        "Failed to stop GateKeeper system.");

                RequestManager.StartGateKeeperSystem();
                Assert.AreEqual(SystemStatusType.Normal, RequestManager.GetGateKeeperSystemStatus(),
                        "Failed to restart GateKeeper system.");

                RequestManager.StopGateKeeperSystem();
                Assert.AreEqual(SystemStatusType.Stopped, RequestManager.GetGateKeeperSystemStatus(),
                        "Failed to re-stop GateKeeper system.");

                // Reset original state
                if (originalState == SystemStatusType.Normal)
                    RequestManager.StartGateKeeperSystem();
                else
                    RequestManager.StopGateKeeperSystem();

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
        /// Test basic database connectivity.
        /// </summary>
        [Test]
        public void GetMostRecentExternalID()
        {

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.GetMostRecentExternalID()...");

                string externalID = CreateRequestName(REQUEST_SOURCE);
                string reportedID;

                // Create a new request.
                Request request1 = new Request(externalID, REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- GetMostRecentExternalID", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);

                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML1.OuterXml);

                RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document1);

                // Although it's not complete, the mew request should be considered the most recent.
                reportedID = RequestManager.GetMostRecentExternalID(REQUEST_SOURCE);
                Assert.AreEqual(externalID, reportedID,
                    "The new request should be considered most recent request.");

                RequestManager.CompleteRequest(externalID, REQUEST_SOURCE, TEST_USER_ID, 1);

                // The request is complete and should therefore be most recent.
                reportedID = RequestManager.GetMostRecentExternalID(REQUEST_SOURCE);
                Assert.AreEqual(externalID, reportedID,
                    "Current request should be considered most recent.");


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
        public void RequestStatus()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CreateNewRequest()...");

                string externalID = CreateRequestName(REQUEST_SOURCE);

                Request request1 = new Request(externalID, REQUEST_SOURCE, RequestPublicationType.Export,
                    RequestTargetType.GateKeeper, "Test Request Document", "XML", "1.234", TEST_USER_ID);

                RequestStatusType testStatus = RequestStatusType.Invalid;

                // Create a request
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Check status.
                testStatus = RequestManager.GetRequestStatus(request1.RequestID);
                Assert.AreEqual(RequestStatusType.Receiving, testStatus, "Request Status after creation.");

                // Add a document
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML1.OuterXml);
                success =
                    RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document1);

                // Check status.
                testStatus = RequestManager.GetRequestStatus(request1.RequestID);
                Assert.AreEqual(RequestStatusType.Receiving, testStatus, "Request Status after adding data.");
                testStatus = RequestManager.GetRequestStatusFromDocumentID(document1.RequestDataID);
                Assert.AreEqual(RequestStatusType.Receiving, testStatus, "Request Status after adding data. (FromDocumentID)");

                // Mark the request as completed
                success = RequestManager.CompleteRequest(externalID, REQUEST_SOURCE, "NUNIT Tester", 1);
                Assert.IsTrue(success, "CompleteRequest failed");

                // Check status
                testStatus = RequestManager.GetRequestStatus(request1.RequestID);
                Assert.AreEqual(RequestStatusType.DataReceived, testStatus, "Request Status after adding data.");
                testStatus = RequestManager.GetRequestStatusFromDocumentID(document1.RequestDataID);
                Assert.AreEqual(RequestStatusType.DataReceived, testStatus, "Request Status after adding data. (FromDocumentID)");

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
        public void DuplicateRequestData()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.DuplicateRequestData()...");

                string externalID = CreateRequestName(REQUEST_SOURCE);

                Request request1 = new Request(externalID, REQUEST_SOURCE, RequestPublicationType.Export,
                    RequestTargetType.Live, "Test DuplicateRequestData", "XML", "1.234", TEST_USER_ID);

                bool isCreated = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(isCreated, "CreateNewRequest failed.");

                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML1.OuterXml);
                bool succeeded =
                    RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document1);
                Assert.IsTrue(succeeded, "InsertRequestData failed.");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 2,
                    _requestDocXML2.OuterXml);
                succeeded = RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document2);
                Assert.IsTrue(succeeded, "InsertRequestData failed.");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Media,
                    3, RequestDataActionType.Export, _requestCdrid3, "1.0", RequestDataLocationType.GateKeeper, 3,
                    _requestDocXML3.OuterXml);
                succeeded = RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document3);
                Assert.IsTrue(succeeded, "InsertRequestData failed.");

                try
                {
                    // Attempt to insert a duplicate record.
                    succeeded = RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref document3);
                    Assert.Fail("InsertRequestData failed to catch duplicate record.");
                }
                catch
                {   // This block intentionally blank.  Correct behavior is an exception.
                }


                RequestManager.AbortRequest(externalID, REQUEST_SOURCE, TEST_USER_ID);

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
        /// Verify that requests created with a publication type of FullLoad have their promotion
        /// target forced to GateKeeper.
        /// </summary>
        [Test]
        public void FullLoad()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CreateNewRequest()...");

                string requestName = CreateRequestName(REQUEST_SOURCE);

                /// Create a new request object with publication type set to FullLoad and
                /// target to Live.
                Request request1 = new Request(requestName, REQUEST_SOURCE, RequestPublicationType.FullLoad,
                    RequestTargetType.Live, "Test Request Document", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Verify that the publication target was forced to GateKeeper.
                Request testRequest = RequestManager.LoadRequestByExternalID(requestName, REQUEST_SOURCE);
                Assert.AreEqual(RequestTargetType.GateKeeper, testRequest.PublicationTarget,
                    "PublicationTarget not forced to GateKeeper.");

                // Clean up
                RequestManager.AbortRequest(requestName, REQUEST_SOURCE, TEST_USER_ID);

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
