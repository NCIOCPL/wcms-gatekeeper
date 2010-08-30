using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
using GateKeeper.Logging;

namespace GateKeeper.UnitTest.Logging
{
    [TestFixture]
    public class LoggingTestFixture
    {
        #region Fields

        Utilities.Timers.HiPerfTimer perfTimer = new Utilities.Timers.HiPerfTimer();

        #endregion

        #region Setup and Teardown

        /// <summary>
        /// Setup run once for all Unit Tests.
        /// </summary>
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {

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
        public void TestWriteLogMessage()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing LoggingTestFixture.TestWriteLogMessage()...");

                LogManager.WriteLogMessage("Simple UT message");

                LogManager.WriteLogMessage("Test information log category", LogManager.GeneralLogCategory);

                LogManager.WriteLogMessage("Test error log category", LogManager.ErrorLogCategory);

                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties.Add("DocumentID", "9999");
                properties.Add("ExceptionType", "SqlException");

                LogManager.WriteLogMessage("Test general log category", "Does this work?", LogManager.GeneralLogCategory,
                    TraceEventType.Information, properties);

                LogManager.WriteLogMessage("Test critical log category", LogManager.CriticalLogCategory);

                LogManager.WriteLogMessage("Test warning log category", LogManager.WarningLogCategory);

                LogManager.WriteLogMessage("Test general log category and information trace event type", "UT message...", LogManager.GeneralLogCategory,
                    TraceEventType.Information);

                LogManager.WriteLogMessage("Test critical log category with critical trace type", "UT message", LogManager.CriticalLogCategory,
                    TraceEventType.Critical);

                Console.WriteLine("No exceptions generated; Examine log syncs for details...");

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
