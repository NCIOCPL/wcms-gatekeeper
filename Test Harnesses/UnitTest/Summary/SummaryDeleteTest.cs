using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using GateKeeper.Common;
using NUnit.Framework;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;


namespace GateKeeper.UnitTest.Summary
{
    [TestFixture]
    public class SummaryDeleteTest
    {
        #region Fields

        Utilities.Timers.HiPerfTimer perfTimer = new Utilities.Timers.HiPerfTimer();
        XmlDocument summaryXmlWithTables = new XmlDocument();

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

        

        #region Utility Methods
       #endregion

        #region Unit Tests

        /// <summary>
        /// Unit test.
        /// </summary>

        [Test]
        public void TestDeleteSummaryByCDRID()
        {
            // Set up
            try
            {
                int documentID = 527424;
                // Test Summary saving
                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                summary.DocumentID = documentID;
                using (SummaryQuery summaryQuery = new SummaryQuery())
                {
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Staging, "ychen");
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, "ychen");
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }


        [Test]
        public void SummaryDelete258017Staging()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Staging, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete258017Preview()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete258017Live()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete62787Staging()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Staging, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete62787Preview()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }


        [Test]
        public void SummaryDelete62787Live()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete62955Staging()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Staging, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete62955Preview()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete62955Live()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete256762Staging()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Staging, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete256762Preview()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete256762Live()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete256668Staging()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Staging, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete256668Preview()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete256668Live()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete257999Staging()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Staging, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete257999Preview()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete257999Live()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }


        [Test]
        public void SummaryDelete299612Staging()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Staging, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete299612Preview()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Preview, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        [Test]
        public void SummaryDelete299612Live()
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
                    summaryQuery.DeleteDocument(summary, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting summary document failed.", e);
            }
        }

        #endregion

        #region Private method
        private void Message(string s)
        { Console.WriteLine(s); }
        #endregion
    }
}
