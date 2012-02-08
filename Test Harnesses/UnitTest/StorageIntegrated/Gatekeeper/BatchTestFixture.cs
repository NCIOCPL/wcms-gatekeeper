using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;

using GKManagers;
using GKManagers.BusinessObjects;


namespace GateKeeper.UnitTest.StorageIntegrated.Gatekeeper
{
    [TestFixture, Explicit]
    public class BatchTestFixture
    {
        #region Fields

        string _externalID1;
        string _externalID2;

        int _requestID1;
        int _requestID2;

        XmlDocument _requestDocXML1 = new XmlDocument();
        XmlDocument _requestDocXML2 = new XmlDocument();
        XmlDocument _requestDocXML3 = new XmlDocument();
        XmlDocument _requestDocXML4 = new XmlDocument();

        List<XmlDocument> _requestDocList;
        List<CDRDocumentType> _requestDocTypeList;
        List<int> _requestCdridList;

        XmlDocument _requestSummaryDocXML1 = new XmlDocument();

        List<XmlDocument> _summaryDocList;
        List<int> _summaryCdridList;

        Random _rng = new Random();

        const string _testUserID = "Test User";
        const string _requestSource = "NUNIT";

        #endregion

        #region Helpers

        private string CreateRequestName(string source)
        {
            int newID;
            string oldID = RequestManager.GetMostRecentExternalID(source);
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
            try
            {
                // Many tests in this fixture require the system to be in a processing state.
                _orignalRunState = RequestManager.GetGateKeeperSystemStatus();
                RequestManager.StartGateKeeperSystem();

                _requestDocList = new List<XmlDocument>();
                _requestCdridList = new List<int>();
                _summaryDocList = new List<XmlDocument>();
                _summaryCdridList = new List<int>();

                _requestDocTypeList = new List<CDRDocumentType>();
                _requestDocTypeList.Add(CDRDocumentType.GENETICSPROFESSIONAL);
                _requestDocTypeList.Add(CDRDocumentType.GlossaryTerm);
                _requestDocTypeList.Add(CDRDocumentType.Media);
                _requestDocTypeList.Add(CDRDocumentType.Term);

                _requestCdridList.Add(10);
                _requestCdridList.Add(350231);
                _requestCdridList.Add(20);
                _requestCdridList.Add(37779);

                _summaryCdridList.Add(62787);

                #region CreateDocumentXML

                // CDRID = 10
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GENETICSPROFESSIONAL.xml"))
                {
                    this._requestDocXML1.PreserveWhitespace = true;
                    this._requestDocXML1.LoadXml(srBuffer.ReadToEnd());
                }
                _requestDocList.Add(_requestDocXML1);

                // CDRID = CDR0000350231
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GlossaryTerm.xml"))
                {
                    this._requestDocXML2.PreserveWhitespace = true;
                    this._requestDocXML2.LoadXml(srBuffer.ReadToEnd());
                }
                _requestDocList.Add(_requestDocXML2);

                // CDRID = not embedded.
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Media.xml"))
                {
                    this._requestDocXML3.PreserveWhitespace = true;
                    this._requestDocXML3.LoadXml(srBuffer.ReadToEnd());
                }
                _requestDocList.Add(_requestDocXML3);

                // CDRID = CDR0000037779
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Terminology.xml"))
                {
                    this._requestDocXML4.PreserveWhitespace = true;
                    this._requestDocXML4.LoadXml(srBuffer.ReadToEnd());
                }
                _requestDocList.Add(_requestDocXML4);

                // CDRID = CDR0000062787
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/SummaryEnglishHPWithReference62787.xml"))
                {
                    this._requestSummaryDocXML1.PreserveWhitespace = true;
                    this._requestSummaryDocXML1.LoadXml(srBuffer.ReadToEnd());
                }
                _summaryDocList.Add(_requestSummaryDocXML1);


                #endregion

                // Build Request1
                _externalID1 = CreateRequestName(_requestSource);
                Request req1 = new Request(_externalID1, _requestSource, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Request for Batch testing via NUNIT", 
                    "XML", "1.0", _testUserID);
                RequestManager.CreateNewRequest(ref req1);
                for (int i = 0; i < _requestDocList.Count; ++i)
                {
                    RequestData data = RequestDataFactory.Create(_requestDocTypeList[i], (i + 1),
                        RequestDataActionType.Export, _requestCdridList[i], "1.0", 
                        RequestDataLocationType.Staging, 1, _requestDocList[i].OuterXml);
                    RequestManager.InsertRequestData(_externalID1, _requestSource, _testUserID, ref data);
                }
                RequestManager.CompleteRequest(_externalID1, _requestSource, _testUserID, _requestDocList.Count);
                _requestID1 = req1.RequestID;

                // Build Request2
                _externalID2 = CreateRequestName(_requestSource);
                Request req2 = new Request(_externalID2, _requestSource, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Request for Batch testing via NUNIT", 
                    "XML", "1.0", _testUserID);
                RequestManager.CreateNewRequest(ref req2);
                for (int i = 0; i < _summaryDocList.Count; ++i)
                {
                    RequestData data = RequestDataFactory.Create(CDRDocumentType.Summary, (i + 1),
                        RequestDataActionType.Export, _summaryCdridList[i], "1.0", 
                        RequestDataLocationType.Staging, 3, _summaryDocList[i].OuterXml);
                    RequestManager.InsertRequestData(_externalID2, _requestSource, _testUserID, ref data);
                }
                RequestManager.CompleteRequest(_externalID2, _requestSource, _testUserID, _summaryDocList.Count);
                _requestID2 = req2.RequestID;
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
        public void CreateNewBatch1()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CreateNewBatch()...");

                Batch batch1 = new Batch("Test for CreateNewBatch1", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);
                List<int> requestDataIDs = 
                    RequestManager.LoadRequestDataIDList(_requestID1);

                bool succeeded = 
                    BatchManager.CreateNewBatch(ref batch1, requestDataIDs.ToArray());
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch1.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

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
        public void CreateNewBatch2()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CreateNewBatch()...");

                Batch batch1 = new Batch("Test for CreateNewBatch2", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);

                bool succeeded = BatchManager.CreateNewBatch(ref batch1, _requestID2);
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch1.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

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
        public void LoadBatch()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadBatch()...");

                Batch batch1 = new Batch("Test for LoadBatch", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);
                List<int> requestDataIDs =
                    RequestManager.LoadRequestDataIDList(_requestID1);

                bool succeeded =
                    BatchManager.CreateNewBatch(ref batch1, requestDataIDs.ToArray());
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch1.BatchID > 0, "CreateNewBatch did not return a valid batchID.");


                Batch batch2 = BatchManager.LoadBatch(batch1.BatchID);
                Assert.IsNotNull(batch2, "Batch not loaded.");
                Assert.AreEqual(batch1.BatchID, batch2.BatchID, "Batch ID not equal");
                Assert.AreEqual(batch1.Status, batch2.Status, "Status not equal");
                Assert.AreEqual(batch1.Actions.Count, batch2.Actions.Count, "Different numbers of Actions.");
                for (int i = 0; i < batch1.Actions.Count; ++i)
                {
                    Assert.AreEqual(batch1.Actions[i], batch2.Actions[i],
                        "Difference in Action %d.  %s vs %s",
                        i, batch1.Actions[i].ToString(), batch2.Actions[i].ToString());
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
        public void CancelBatch1()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CancelBatch1()...");

                Batch batch1 = new Batch("Test for CancelBatch1", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);
                List<int> requestDataIDs =
                    RequestManager.LoadRequestDataIDList(_requestID1);

                bool succeeded =
                    BatchManager.CreateNewBatch(ref batch1, requestDataIDs.ToArray());
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch1.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

                succeeded = BatchManager.CancelBatch(ref batch1, _testUserID);
                Assert.IsTrue(succeeded, "CancelBatch() failed.");
                Assert.AreEqual(BatchStatusType.Cancelled, batch1.Status,
                    "batch1 not marked as Cancelled.");

                Batch batch2 = BatchManager.LoadBatch(batch1.BatchID);
                Assert.IsNotNull(batch2, "Batch not loaded.");
                Assert.AreEqual(BatchStatusType.Cancelled, batch2.Status,
                    "batch2 not marked as Cancelled.");

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
        public void CancelBatch2()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CancelBatch2()...");

                Batch batch1 = new Batch("Test for CancelBatch2", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);
                List<int> requestDataIDs =
                    RequestManager.LoadRequestDataIDList(_requestID1);

                bool succeeded =
                    BatchManager.CreateNewBatch(ref batch1, requestDataIDs.ToArray());
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch1.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

                succeeded = BatchManager.CancelBatch(ref batch1, _testUserID);
                Assert.IsTrue(succeeded, "CancelBatch() failed.");
                Assert.AreEqual(BatchStatusType.Cancelled, batch1.Status,
                    "batch1 not marked as Cancelled.");

                Batch batch2 = BatchManager.LoadBatch(batch1.BatchID);
                Assert.IsNotNull(batch2, "Batch not loaded.");
                Assert.AreEqual(BatchStatusType.Cancelled, batch2.Status,
                    "batch2 not marked as Cancelled.");

                List<int> batchIDs = BatchManager.LoadActiveBatchList();
                Assert.IsNotNull(batchIDs, "LoadActiveBatchList failed to return a list.");

                // Verify the batch is not listed.
                Assert.IsFalse(batchIDs.Contains(batch1.BatchID), "Batch1 is in the list and shouldn't be.");

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
        public void LoadBatchList()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadActiveBatchList()...");

                Batch batch1 = new Batch("Test #1 for LoadActiveBatchList", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);
                List<int> requestDataIDs =
                    RequestManager.LoadRequestDataIDList(_requestID1);
                bool succeeded =
                    BatchManager.CreateNewBatch(ref batch1, requestDataIDs.ToArray());
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch1.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

                Batch batch2 = new Batch("Test #2 for LoadActiveBatchList", _testUserID,
                    ProcessActionType.PromoteToPreview, ProcessActionType.PromoteToLive);
                succeeded =
                    BatchManager.CreateNewBatch(ref batch2, _requestID2);
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch2.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

                Batch batch3 = new Batch("Test #3 for LoadActiveBatchList", _testUserID,
                    ProcessActionType.PromoteToLive, ProcessActionType.PromoteToLive);
                succeeded =
                    BatchManager.CreateNewBatch(ref batch3, _requestID1);
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch3.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

                Batch batch4 = new Batch("Test #4 for LoadActiveBatchList", _testUserID,
                    ProcessActionType.PromoteToLive, ProcessActionType.PromoteToLive);
                succeeded =
                    BatchManager.CreateNewBatch(ref batch4, _requestID2);
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch4.BatchID > 0, "CreateNewBatch did not return a valid batchID.");


                // A) Call LoadActiveBatchList() -- Retrieves all batches.
                List<int> batchIDs = BatchManager.LoadActiveBatchList();
                Assert.IsNotNull(batchIDs, "LoadActiveBatchList failed to return a list.");
                Assert.IsNotEmpty(batchIDs, "LoadActiveBatchList returned an empty list.");

                // B) Verify all batches are present.
                Console.WriteLine("Initial Load");
                Assert.IsTrue(batchIDs.Contains(batch1.BatchID), "Batch ID 1 is missing from the list.");
                Assert.IsTrue(batchIDs.Contains(batch2.BatchID), "Batch ID 2 is missing from the list.");
                Assert.IsTrue(batchIDs.Contains(batch3.BatchID), "Batch ID 3 is missing from the list.");
                Assert.IsTrue(batchIDs.Contains(batch4.BatchID), "Batch ID 4 is missing from the list.");

                // C)  Leave batch 1 as is.
                //    Cancel batch 2
                //      Mark batch 3 failed
                //      Mark batch 4 complete
                BatchManager.CancelBatch(ref batch2, _testUserID);
                BatchManager.CompleteBatch(batch3, _testUserID, true);
                BatchManager.CompleteBatch(batch4, _testUserID);
                
                // Check what's present for the Active Batch List
                batchIDs = BatchManager.LoadActiveBatchList();
                Console.WriteLine("LoadActiveBatchList");
                Assert.IsTrue(batchIDs.Contains(batch1.BatchID), "Batch 1 is missing from the list.");
                Assert.IsFalse(batchIDs.Contains(batch2.BatchID), "Batch 2 should not be present.");
                Assert.IsFalse(batchIDs.Contains(batch3.BatchID), "Batch 3 should not be present.");
                Assert.IsFalse(batchIDs.Contains(batch4.BatchID), "Batch 4 should not be present.");

                // Check what's present for the Failed Batch List
                Console.WriteLine("LoadFailedBatchList");
                batchIDs = BatchManager.LoadFailedBatchList();
                Assert.IsFalse(batchIDs.Contains(batch1.BatchID), "Batch 1 should not be present.");
                Assert.IsFalse(batchIDs.Contains(batch2.BatchID), "Batch 2 should not be present.");
                Assert.IsTrue(batchIDs.Contains(batch3.BatchID), "Batch 3 is missing from the list.");
                Assert.IsFalse(batchIDs.Contains(batch4.BatchID), "Batch 4 should not be present.");

                // Check what's present for the Complete Batch List
                Console.WriteLine("LoadCompleteBatchList");
                batchIDs = BatchManager.LoadCompleteBatchList();
                Assert.IsTrue(batchIDs.Contains(batch1.BatchID), "Batch 1 is missing from the list.");
                Assert.IsFalse(batchIDs.Contains(batch2.BatchID), "Batch 2 should not be present.");
                Assert.IsTrue(batchIDs.Contains(batch3.BatchID), "Batch 3 is missing from the list.");
                Assert.IsFalse(batchIDs.Contains(batch4.BatchID), "Batch 4 should not be present.");

                // Clean up: Set batch back to a non-error condition to avoid inconsistent data.
                BatchManager.CompleteBatch(batch1, _testUserID, false);
                BatchManager.CompleteBatch(batch2, _testUserID, false);
                BatchManager.CompleteBatch(batch3, _testUserID, false);
                BatchManager.CompleteBatch(batch4, _testUserID, false);

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
        public void CompleteBatch()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CompleteBatch()...");

                Batch batch1 = new Batch("Test #1 for CompleteBatch", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);
                List<int> requestDataIDs =
                    RequestManager.LoadRequestDataIDList(_requestID1);
                bool succeeded =
                    BatchManager.CreateNewBatch(ref batch1, requestDataIDs.ToArray());
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch1.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

                Batch batch2 = new Batch("Test #2 for CompleteBatch", _testUserID,
                    ProcessActionType.PromoteToPreview, ProcessActionType.PromoteToLive);
                succeeded =
                    BatchManager.CreateNewBatch(ref batch2, _requestID2);
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch2.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

                BatchManager.CompleteBatch(batch1, _testUserID);
                Assert.AreEqual(BatchStatusType.Complete, batch1.Status, "Batch 1 not marked complete.");

                BatchManager.CompleteBatch(batch2, _testUserID, true);
                Assert.AreEqual(BatchStatusType.CompleteWithErrors, batch2.Status,
                    "Batch 2 not marked complete with errors.");

                List<int> batchList = BatchManager.LoadCompleteBatchList();
                Assert.IsFalse(batchList.Contains(batch1.BatchID), "Batch 1 not removed from queue.");
                Assert.IsTrue(batchList.Contains(batch2.BatchID), "Batch 2 incorrectly removed from queue.");

                Batch batch3 = BatchManager.LoadBatch(batch1.BatchID);
                Assert.AreEqual(BatchStatusType.Complete, batch3.Status, "Batch 3 not marked complete.");

                Batch batch4 = BatchManager.LoadBatch(batch2.BatchID);
                Assert.AreEqual(BatchStatusType.CompleteWithErrors, batch4.Status,
                    "Batch 4 not marked complete with errors.");

                // Clean up: Set batches to a non-error condition to avoid inconsistent data.
                BatchManager.CompleteBatch(batch1, _testUserID, false);
                BatchManager.CompleteBatch(batch2, _testUserID, false);

                
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
        public void ReviewBatch()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.ReviewBatch()...");

                Batch batch1 = new Batch("Test for ReviewBatch", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);
                List<int> requestDataIDs =
                    RequestManager.LoadRequestDataIDList(_requestID1);
                bool succeeded =
                    BatchManager.CreateNewBatch(ref batch1, requestDataIDs.ToArray());
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch1.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

                BatchManager.CompleteBatch(batch1, _testUserID, true);

                Batch batch2 = BatchManager.LoadBatch(batch1.BatchID);
                Assert.AreEqual(BatchStatusType.CompleteWithErrors, batch2.Status,
                    "Batch 2 not marked complete-with-errors.");

                BatchManager.ReviewBatch(ref batch2, _testUserID);
                Batch batch3 = BatchManager.LoadBatch(batch2.BatchID);
                Assert.AreEqual(BatchStatusType.Reviewed, batch3.Status, "batch3 not reviewed.");

                Assert.AreEqual(batch1.BatchID, batch2.BatchID, "Batch ID changed between 1 & 2!");
                Assert.AreEqual(batch2.BatchID, batch3.BatchID, "Batch ID changed between 2 & 3!");

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
        public void CancelRunningBatch()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CancelRunningBatch()...");

                Batch batch1 = new Batch("Test for CancelRunningBatch", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToPreview);
                List<int> requestDataIDs =
                    RequestManager.LoadRequestDataIDList(_requestID1);

                bool succeeded =
                    BatchManager.CreateNewBatch(ref batch1, requestDataIDs.ToArray());
                Assert.IsTrue(succeeded, "CreateNewBatch() failed.");
                Assert.IsTrue(batch1.BatchID > 0, "CreateNewBatch did not return a valid batchID.");

                /// This may not be the same batch created above because not all tests clean up
                /// after themselves.  They'll only be the same when the batch queue is clean.
                /// Any batch returned by StartNextBatch() is guaranteed to have its status set
                /// to "Processing".
                int newBatchID = BatchManager.StartNextBatch();
                Batch batch2 = BatchManager.LoadBatch(newBatchID);

                succeeded = BatchManager.CancelBatch(ref batch2, _testUserID);
                Assert.IsFalse(succeeded, "CancelBatch() allowed a batch to be cancelled after it was already marked for processing.");

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
        public void ResetBatchDocumentStatus()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.LoadRequestDataByID()...");

                // Create a request.
                Request request1 = new Request(CreateRequestName(_requestSource), _requestSource, RequestPublicationType.Export, 
                    RequestTargetType.Live, "Test -- LoadRequestDataByCDRID", "XML", "1.234", _testUserID);
                bool success = RequestManager.CreateNewRequest(ref request1);
                Assert.IsTrue(success, "CreateNewRequest failed.");

                // Add three documents to the request.
                RequestData document1 = RequestDataFactory.Create(CDRDocumentType.GENETICSPROFESSIONAL,
                    1, RequestDataActionType.Export, 10, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, _requestSource,
                    _testUserID, ref document1);
                Assert.IsTrue(success, "InsertRequestData - document1");

                RequestData document2 = RequestDataFactory.Create(CDRDocumentType.GlossaryTerm,
                    2, RequestDataActionType.Export, 350231, "1.0", RequestDataLocationType.GateKeeper, 1,
                    _requestDocXML2.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, _requestSource,
                    _testUserID, ref document2);
                Assert.IsTrue(success, "InsertRequestData - document2");

                RequestData document3 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    3, RequestDataActionType.Export, 62787, "1.0", RequestDataLocationType.GateKeeper, 2,
                    _requestSummaryDocXML1.OuterXml);
                success = RequestManager.InsertRequestData(request1.ExternalRequestID, _requestSource,
                    _testUserID, ref document3);
                Assert.IsTrue(success, "InsertRequestData - document3");

                // Complete the request.
                success = RequestManager.CompleteRequest(request1.ExternalRequestID, _requestSource, _testUserID, 3);
                Assert.IsTrue(success, "GKManagers.CompleteRequest failed");

                // Place all three documents into a batch.
                Batch batch1 = new Batch("Test for ResetBatchDocumentStatus()", _testUserID,
                    ProcessActionType.PromoteToStaging, ProcessActionType.PromoteToLive);
                BatchManager.CreateNewBatch(ref batch1, request1.RequestID);


                // Document 1 & 2 belong to group 1
                // Document 3 belongs to group 2

                // Simulate promotion error.
                RequestManager.MarkDocumentWithErrors(document1.RequestDataID);

                // The big test
                BatchManager.ResetBatchDocumentStatus(batch1.BatchID);

                RequestData testDoc1;
                RequestData testDoc2;
                RequestData testDoc3;

                testDoc1 = RequestManager.LoadRequestDataByID(document1.RequestDataID);
                testDoc2 = RequestManager.LoadRequestDataByID(document2.RequestDataID);
                testDoc3 = RequestManager.LoadRequestDataByID(document3.RequestDataID);

                Assert.AreEqual(RequestDataStatusType.OK, testDoc1.Status, "Test1 not set OK.");
                Assert.AreEqual(RequestDataDependentStatusType.OK, testDoc1.DependencyStatus,
                    "Test1 dependent status not set OK.");

                Assert.AreEqual(RequestDataStatusType.OK, testDoc2.Status, "Test2 not set OK.");
                Assert.AreEqual(RequestDataDependentStatusType.OK, testDoc2.DependencyStatus,
                    "Test2dependent status not set OK.");

                Assert.AreEqual(RequestDataStatusType.OK, testDoc3.Status, "Test3 not set OK.");
                Assert.AreEqual(RequestDataDependentStatusType.OK, testDoc3.DependencyStatus,
                    "Test3 dependent status not set OK.");

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
        public void StartNextBatch()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing RequestTextFixture.CreateNewBatch()...");

                int batchID1 = BatchManager.StartNextBatch();
                Batch batch1 = BatchManager.LoadBatch(batchID1);
                BatchManager.CompleteBatch(batch1, _testUserID);

                int batchID2 = BatchManager.StartNextBatch();

                // Stop the system.  StartNextBatch() should return -1 to signify that
                // no batch is available.
                SystemStatusType oldStatus = RequestManager.GetGateKeeperSystemStatus();
                RequestManager.StopGateKeeperSystem();

                int noBatchID = BatchManager.StartNextBatch();
                Assert.IsTrue(noBatchID < 0, "A batch was found while the system was stopped.");

                if (oldStatus == SystemStatusType.Stopped)
                    RequestManager.StopGateKeeperSystem();
                else
                    RequestManager.StartGateKeeperSystem();

                Assert.AreNotEqual(batchID1, batchID2, "Same batch loaded twice.");

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
