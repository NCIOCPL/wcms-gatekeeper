using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;

//using GateKeeper.ProcessManager;

using GKManagers;
using GKManagers.BusinessObjects;


namespace GateKeeper.UnitTest.Gatekeeper
{
    [TestFixture]
    public class IntegrationTestFixture
    {
        #region Fields

        XmlDocument requestDocXML1 = new XmlDocument();
        XmlDocument requestDocXML2 = new XmlDocument();
        //XmlDocument requestDocXML3 = new XmlDocument();
        //XmlDocument requestDocXML4 = new XmlDocument();

        XmlDocument requestSummaryDocXML1 = new XmlDocument();
        XmlDocument requestSummaryDocXML2 = new XmlDocument();

        int _requestCdrid1 = 10;
        int _requestCdrid2 = 35023;
        //int _requestCdrid3 = 30;
        //int _requestCdrid4 = 37779;

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
                    this.requestDocXML1.PreserveWhitespace = true;
                    this.requestDocXML1.LoadXml(srBuffer.ReadToEnd());
                }

                // CDRID = CDR0000350231
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GlossaryTerm.xml"))
                {
                    this.requestDocXML2.PreserveWhitespace = true;
                    this.requestDocXML2.LoadXml(srBuffer.ReadToEnd());
                }

                // CDRID = not embedded.
                //using (StreamReader srBuffer = new StreamReader(@"./XMLData/Media.xml"))
                //{
                //    this.requestDocXML3.PreserveWhitespace = true;
                //    this.requestDocXML3.LoadXml(srBuffer.ReadToEnd());
                //}

                // CDRID = CDR0000037779
                //using (StreamReader srBuffer = new StreamReader(@"./XMLData/Terminology.xml"))
                //{
                //    this.requestDocXML4.PreserveWhitespace = true;
                //    this.requestDocXML4.LoadXml(srBuffer.ReadToEnd());
                //}

                // CDRID = CDR0000062787
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/SummaryEnglishHPWithReference62787.xml"))
                {
                    this.requestSummaryDocXML1.PreserveWhitespace = true;
                    this.requestSummaryDocXML1.LoadXml(srBuffer.ReadToEnd());
                }

                // CDRID = CDR000065504
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolWithMultiplePersonsInOneSite65504.xml"))
                {
                    this.requestSummaryDocXML2.PreserveWhitespace = true;
                    this.requestSummaryDocXML2.LoadXml(srBuffer.ReadToEnd());
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
        public void AddRequestDataHistoryEntry()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadRequestDataByID()...");

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- LoadRequestDataByCDRID", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add three documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    3, RequestDataActionType.Export, _requestSummaryCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestSummaryDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 3);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");


                // Create a batch, walk through steps, add events.
                Batch batch1 = new Batch("Batch test for AddRequestDataHistoryEntry()", TEST_USER_ID,
                    ProcessActionType.PromoteToPreview, ProcessActionType.PromoteToPreview);
                success = BatchManager.CreateNewBatch(ref batch1, request1.RequestID);
                Assert.IsTrue(success, "Failure creating batch.");

                // Mark with a non-processing status.
                BatchManager.CompleteBatch(batch1, TEST_USER_ID, true);

                success = RequestManager.AddRequestDataHistoryEntry(request1.RequestID,
                    document1.RequestDataID, batch1.BatchID,
                    String.Format("Batch 1 Marked for Processing {0}", batch1.BatchID),
                    RequestDataHistoryType.Information);
                Assert.IsTrue(success, "AddRequestDataHistoryEntry() failed.");

                // Clean up: Set batch back to a non-error condition to avoid inconsistent data.
                BatchManager.CompleteBatch(batch1, TEST_USER_ID, false);

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
        /// Attempt to promote from GateKeeper to Preview without going through Staging.
        /// </summary>
        [Test]
        public void BadPromotion()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.BadPromotion()...");

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- LoadRequestDataByCDRID", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add three documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    3, RequestDataActionType.Export, _requestSummaryCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestSummaryDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 3);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");


                // BATCH 1 -- Attempts to promote to Preview for a request which is only at Gatekeeper.
                Batch batch1 = new Batch("Batch test for BadPromotion()", TEST_USER_ID,
                    ProcessActionType.PromoteToPreview, ProcessActionType.PromoteToPreview);
                success = BatchManager.CreateNewBatch(ref batch1, request1.RequestID);
                Assert.IsTrue(success, "Failure creating batch.");

                // Submit batch to DocumentManager for processing.
                DocumentManager.PromoteBatch(batch1.BatchID, TEST_USER_ID);

                Batch batchResult = BatchManager.LoadBatch(batch1.BatchID);
                Assert.AreEqual(batch1.BatchID, batchResult.BatchID, "Wrong batchID loaded.");
                Assert.AreEqual(BatchStatusType.CompleteWithErrors, batchResult.Status,
                    "Batch was expected to fail but did not.");

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
        /// Attempt to promote from GateKeeper to Staging.
        /// </summary>
        [Test]
        public void PromoteGatekeeperToLive()
        {
            // This test manages its batch IDs and cannot tolerate an instance of ProcessManager
            // running the same test.
            SystemStatusType originalSystemStatus;
            originalSystemStatus = RequestManager.GetGateKeeperSystemStatus();
            RequestManager.StopGateKeeperSystem();

            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.PromoteGatekeeperToLive()...");

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export,
                    RequestTargetType.Live, "Test -- PromoteGatekeeperToLive", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add three documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                //RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Summary,
                //    3, RequestDataActionType.Export, _requestSummaryCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                //    requestSummaryDocXML1.OuterXml);
                //success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                //    TEST_USER_ID, ref document3);
                //Assert.IsTrue(success, "InsertRequestData - document3");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Protocol,
                    3, RequestDataActionType.Export, 517312, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestSummaryDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 3);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");


                // Submit a valid batch.
                Batch batch1 = new Batch("Batch test for PromoteGatekeeperToStaging()", TEST_USER_ID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToLive);
                success = BatchManager.CreateNewBatch(ref batch1, request1.RequestID);
                Assert.IsTrue(success, "Failure creating batch.");

                // Submit batch to DocumentManager for processing.
                DocumentManager.PromoteBatch(batch1.BatchID, TEST_USER_ID);

                Batch batchResult = BatchManager.LoadBatch(batch1.BatchID);
                Assert.AreEqual(batch1.BatchID, batchResult.BatchID, "Wrong batchID loaded.");
                Assert.AreEqual(BatchStatusType.Complete, batchResult.Status,
                    "Batch not marked complete.");

                // Was Doc3 promoted?
                //RequestData testDocument = RequestManager.LoadRequestDataByID(document3.RequestDataID);
                //Assert.AreEqual(RequestDataLocationType.Live, testDocument.Location, "Document version differs.");

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
            finally
            {
                if (originalSystemStatus == SystemStatusType.Normal)
                    RequestManager.StartGateKeeperSystem();
                else
                    RequestManager.StopGateKeeperSystem();
            }
        }

        [Test]
        public void PromoteGatekeeperToLiveForReindexFullTextProtocol()
        {
            // This test manages its batch IDs and cannot tolerate an instance of ProcessManager
            // running the same test.
            SystemStatusType originalSystemStatus;
            originalSystemStatus = RequestManager.GetGateKeeperSystemStatus();
            RequestManager.StopGateKeeperSystem();

            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.PromoteGatekeeperToLive()...");

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export,
                    RequestTargetType.Live, "Test -- PromoteGatekeeperToLive", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add three documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Protocol,
                    3, RequestDataActionType.Export, 65504, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestSummaryDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 3);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");


                // Submit a valid batch.
                Batch batch1 = new Batch("Batch test for PromoteGatekeeperToStaging()", TEST_USER_ID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToLive);
                success = BatchManager.CreateNewBatch(ref batch1, request1.RequestID);
                Assert.IsTrue(success, "Failure creating batch.");

                // Submit batch to DocumentManager for processing.
                DocumentManager.PromoteBatch(batch1.BatchID, TEST_USER_ID);

                Batch batchResult = BatchManager.LoadBatch(batch1.BatchID);
                Assert.AreEqual(batch1.BatchID, batchResult.BatchID, "Wrong batchID loaded.");
                Assert.AreEqual(BatchStatusType.Complete, batchResult.Status,
                    "Batch not marked complete.");

                // Was Doc3 promoted?
                //RequestData testDocument = RequestManager.LoadRequestDataByID(document3.RequestDataID);
                //Assert.AreEqual(RequestDataLocationType.Live, testDocument.Location, "Document version differs.");

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
            finally
            {
                if (originalSystemStatus == SystemStatusType.Normal)
                    RequestManager.StartGateKeeperSystem();
                else
                    RequestManager.StopGateKeeperSystem();
            }
        }

        /// <summary>
        /// Attempt to promote from GateKeeper to Staging.
        /// </summary>
        [Test]
        public void DocumentLocation()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.DocumentLocation()...");

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- DocumentLocation", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add three documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    3, RequestDataActionType.Export, _requestSummaryCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestSummaryDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 3);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                // Submit a batch promoting all three documents to Live status.
                Batch batch1 = new Batch("DocumentLocation() - batch 1", TEST_USER_ID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToLive);
                success = BatchManager.CreateNewBatch(ref batch1, request1.RequestID);
                Assert.IsTrue(success, "Failure creating batch.");
                DocumentManager.PromoteBatch(batch1.BatchID, TEST_USER_ID);


                // Create a second request, containing an "updated" version of document 2 (CDRID 25)
                Request request2 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- DocumentLocation (request 2)", "XML", "1.234", TEST_USER_ID);
                success = RequestManager.CreateNewRequest(ref request2);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                RequestData document2a = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "2.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request2.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2a");

                // Complete the request.
                success = RequestManager.CompleteRequest(request2.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 1);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                // Promote the document to preview.
                Batch batch2 = new Batch("DocumentLocation() - batch 2", TEST_USER_ID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);
                success = BatchManager.CreateNewBatch(ref batch2, request2.RequestID);
                Assert.IsTrue(success, "Failure creating batch.");
                DocumentManager.PromoteBatch(batch2.BatchID, TEST_USER_ID);


                // The test we've all been waiting for.
                int[] idList = new int[4];
                idList[0] = document1.RequestDataID;
                idList[1] = document2.RequestDataID;
                idList[2] = document2a.RequestDataID;
                idList[3] = document3.RequestDataID;

                DocumentVersionMap versionMap = RequestManager.LoadDocumentLocationMap(request1.RequestID);

                // Check for lame-brain programming errors.
                Assert.AreEqual(document2.CdrID, document2a.CdrID, "Documents 2 and 2a have different CDRIDs.");
                Assert.AreNotEqual(document2.RequestDataID, document2a.RequestDataID,
                    "Documents 2 and 2a have the same RequestDataID.");

                // The things we're testing for.
                Assert.IsTrue(versionMap.Contains(document1.CdrID), "Document 1 not in map 2.");
                Assert.IsTrue(versionMap.Contains(document2.CdrID), "Document 2 not in map 2.");
                Assert.IsTrue(versionMap.Contains(document2a.CdrID), "Document 2a not in map 2.");
                Assert.IsTrue(versionMap.Contains(document3.CdrID), "Document 3 not in map 2.");

                // Check for non-existant document
                Assert.IsFalse(versionMap.Contains(1), "Non-existant document found.");

                int requestID1 = request1.RequestID;
                int requestID2 = request2.RequestID;

                // Doc1, Doc2 & Doc3 promoted to Live.
                // Doc2a (later edition of Doc2) promoted to Preview.

                Assert.IsTrue(versionMap.MatchStagingVersion(document2.CdrID, requestID1) < 0,
                    "Staging: Request 1 not diagnosed as older.");
                Assert.IsTrue(versionMap.MatchPreviewVersion(document2.CdrID, requestID1) < 0,
                    "Preview: Request 1 not diagnosed as older.");
                Assert.IsTrue(versionMap.MatchLiveVersion(document2.CdrID, requestID1) == 0,
                    "Live: Request 1 not diagnosed as same version.");

                Assert.IsTrue(versionMap.MatchStagingVersion(document2.CdrID, requestID2) == 0,
                    "Staging: Request 2 not diagnosed as same version.");
                Assert.IsTrue(versionMap.MatchPreviewVersion(document2.CdrID, requestID2) == 0,
                    "Preview: Request 2 not diagnosed as same version.");
                Assert.IsTrue(versionMap.MatchLiveVersion(document2.CdrID, requestID2) > 0,
                    "Live: Request 2 not diagnosed as newer.");

                Assert.IsTrue(versionMap.MatchStagingVersion(document1.CdrID, requestID1) == 0,
                    "Staging: Request 1 not diagnosed as same version.");
                Assert.IsTrue(versionMap.MatchPreviewVersion(document1.CdrID, requestID1) == 0,
                    "Preview: Request 1 not diagnosed as same version.");
                Assert.IsTrue(versionMap.MatchLiveVersion(document1.CdrID, requestID1) == 0,
                    "Live: Request 1 not diagnosed as same version.");


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
        /// Verify document status and dependency map.
        /// </summary>
        [Test]
        public void DocumentStatus()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.DocumentLocation()...");

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- DocumentLocation", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add three documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, _requestCdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, _requestCdrid2, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    3, RequestDataActionType.Export, _requestSummaryCdrid1, "1.0", RequestDataLocationType.GateKeeper, 2,
                    requestSummaryDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 3);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                /// Group 1:  Documents 1 & 2
                /// Group 2:  Document 3

                // Fail document 1, this should cause document 2 to have failed dependency status.
                // Document 3 should be unaffected.
                RequestManager.MarkDocumentWithErrors(document1.RequestDataID);

                DocumentStatusMap statusMap = RequestManager.LoadDocumentStatusMap(request1.RequestID);

                Assert.IsNotNull(statusMap, "LoadDocumentStatusMap returned null.");

                Assert.IsTrue(statusMap.Contains(document1.RequestDataID), "Document 1 not found.");
                Assert.IsTrue(statusMap.Contains(document2.RequestDataID), "Document 2 not found.");
                Assert.IsTrue(statusMap.Contains(document3.RequestDataID), "Document 3 not found.");

                Assert.IsFalse(statusMap.Contains(8), "Imaginary document was found.");


                Assert.AreEqual(RequestDataStatusType.Error,
                    statusMap.CheckDocumentStatus(document1.RequestDataID),
                    "Document 1 not flagged as failed");
                Assert.AreEqual(RequestDataStatusType.OK,
                    statusMap.CheckDocumentStatus(document2.RequestDataID),
                    "Document 2 flagged as failed");
                Assert.AreEqual(RequestDataStatusType.OK,
                    statusMap.CheckDocumentStatus(document3.RequestDataID),
                    "Document 3 flagged as failed");

                Assert.AreEqual(RequestDataDependentStatusType.OK,
                    statusMap.CheckDependencyStatus(document1.RequestDataID),
                    "Document 1 flagged as dependent failed");
                Assert.AreEqual(RequestDataDependentStatusType.Error,
                    statusMap.CheckDependencyStatus(document2.RequestDataID),
                    "Document 2 not flagged as dependent failed");
                Assert.AreEqual(RequestDataDependentStatusType.OK,
                    statusMap.CheckDependencyStatus(document3.RequestDataID),
                    "Document 3 flagged as dependent failed");

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
        /// Remove a document (Promote an empty document).
        /// </summary>
        [Test]
        public void RemoveDocument()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.DocumentLocation()...");

                int cdrid1 = _requestCdrid2;

                // Create a request.
                Request request1 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- DocumentLocation", "XML", "1.234", TEST_USER_ID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add a document to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    1, RequestDataActionType.Export, cdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1);
                Assert.IsTrue(success, "RemoveDocument - document1");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 1);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                // Submit a batch promoting the document to Live status.
                Batch batch1 = new Batch("RemoveDocument() - batch 1", TEST_USER_ID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToLive);
                success = BatchManager.CreateNewBatch(ref batch1, request1.RequestID);
                Assert.IsTrue(success, "Failure creating batch.");
                DocumentManager.PromoteBatch(batch1.BatchID, TEST_USER_ID);


                // New request to remove the same CDRID.
                Request request2 = new Request(CreateRequestName(REQUEST_SOURCE), REQUEST_SOURCE, RequestPublicationType.Remove, 
                    RequestTargetType.Live, "Test -- RemoveDocument", "XML", "1.234", TEST_USER_ID);
                success = RequestManager.CreateNewRequest(ref request2);
                Assert.IsTrue(success, "CreateNewRequest failed on second call.");

                // Add a document to the request.
                RequestData document1a = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    1, RequestDataActionType.Remove, cdrid1, "1.0", RequestDataLocationType.GateKeeper, 1,
                    null);
                success = RequestManager.InsertRequestData(request2.ExternalRequestID, REQUEST_SOURCE,
                    TEST_USER_ID, ref document1a);
                Assert.IsTrue(success, "InsertRequestData - document1a");

                // Complete the request.
                success = RequestManager.CompleteRequest(request2.ExternalRequestID, REQUEST_SOURCE, TEST_USER_ID, 1);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                // Submit a batch promoting the document to Live status.
                Batch batch2 = new Batch("RemoveDocument() - batch 2", TEST_USER_ID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToLive);
                success = BatchManager.CreateNewBatch(ref batch2, request2.RequestID);
                Assert.IsTrue(success, "Failure creating batch.");
                DocumentManager.PromoteBatch(batch2.BatchID, TEST_USER_ID);


                // The big test.  Did it work?
                Batch batchTest = BatchManager.LoadBatch(batch2.BatchID);
                Assert.AreEqual(batch2.BatchID, batchTest.BatchID, "Different batch ID loaded for comparison");
                Assert.AreEqual(BatchStatusType.Complete, batchTest.Status, "Removal not succesful.");

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

        [Test, Explicit]
        public void SimulateProcessManager()
        {
            int currentBatchID = -1;
            string userName = REQUEST_SOURCE;

            while ((currentBatchID = BatchManager.StartNextBatch()) > 0)
            {
                try
                {
                    GKManagers.DocumentManager.PromoteBatch(currentBatchID, userName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            }
        }

        #endregion
    }
}
