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


namespace GateKeeper.UnitTest.StorageIntegrated.Gatekeeper
{
    [TestFixture, Explicit]
    public class IntegrationTestFixture
    {
        #region Fields

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
            // SimulateProcessManager requires the system to be in a processing state.
            _orignalRunState = RequestManager.GetGateKeeperSystemStatus();
            RequestManager.StartGateKeeperSystem();

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
