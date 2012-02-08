using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using GateKeeper.Common;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;

namespace GateKeeper.UnitTest.StorageIntegrated.Summary
{
    [TestFixture, Explicit]
    public class SummaryPushTest
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

        [Test]
        public void TestPushSummaryPushByCDRID()
        {
            // Set up
            try
            {
                int documentID = 527424;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToPreview(summary, "ychen");
                    summaryQuery.PushDocumentToLive(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }


        [Test]
        public void SummaryPush258017Preview()
        {
            // Set up
            try
            {
                int documentID = 62979;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToPreview(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

        [Test]
        public void SummaryPush258017Live()
        {
            // Set up
            try
            {
                int documentID = 258017;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToLive(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }


        [Test]
        public void SummaryPush62787Preview()
        {
            // Set up
            try
            {
                int documentID = 62787;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToPreview(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

        [Test]
        public void SummaryPush62787Live()
        {
            // Set up
            try
            {
                int documentID = 62787;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToLive(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

        [Test]
        public void SummaryPush62955Preview()
        {
            // Set up
            try
            {
                int documentID = 62955;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToPreview(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

        [Test]
        public void SummaryPush62955Live()
        {
            // Set up
            try
            {
                int documentID = 62955;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToLive(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

        [Test]
        public void SummaryPush256762Preview()
        {
            // Set up
            try
            {
                int documentID = 256762;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToPreview(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

        [Test]
        public void SummaryPush256762Live()
        {
            // Set up
            try
            {
                int documentID = 256762;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToLive(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }


        [Test]
        public void SummaryPush256668Preview()
        {
            // Set up
            try
            {
                int documentID = 256668;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToPreview(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }
        [Test]
        public void SummaryPush256668Live()
        {
            // Set up
            try
            {
                int documentID = 256668;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToLive(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

        [Test]
        public void SummaryPush257999Preview()
        {
            // Set up
            try
            {
                int documentID = 257999;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToPreview(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

        [Test]
        public void SummaryPush257999Live()
        {
            // Set up
            try
            {
                int documentID = 257999;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToLive(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }


        [Test]
        public void SummaryPush299612Preview()
        {
            // Set up
            try
            {
                int documentID = 299612;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToPreview(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

        [Test]
        public void SummaryPush299612Live()
        {
            // Set up
            try
            {
                int documentID = 299612;
                // Test Summary saving
                SummaryDocument summary = new SummaryDocument();
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.PushDocumentToLive(summary, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing summary document failed.", e);
            }
        }

       #endregion
    }
}
