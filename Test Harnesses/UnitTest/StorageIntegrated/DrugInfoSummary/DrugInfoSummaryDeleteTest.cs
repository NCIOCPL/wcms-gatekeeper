using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;
using GateKeeper.DocumentObjects;
using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;


namespace GateKeeper.UnitTest.StorageIntegrated.DrugInfoSummary
{
    [TestFixture, Explicit]
    public class DrugInfoSummaryDeleteTest
    {
        #region Setup and Teardown

        /// <summary>
        /// Setup run once for all Unit Tests.
        /// </summary>
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            try
            {
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
        public void TestDrugDeleteByCDRID()
        {
            // Set up
            try
            {

                int documentID = 8;

                // Test Summary saving
                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                DrugInfoSummaryDocument drug = new DrugInfoSummaryDocument();
                drug.WarningWriter = warningWriter;
                drug.InformationWriter = informationWriter;
                drug.DocumentID = documentID;
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.DeleteDocument(drug, ContentDatabase.Staging, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Preview, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing drug information summary document failed.", e);
            }
        }

        [Test]
        public void DrugDelete495323()
        {
            // Set up
            try
            {

                int documentID = 495323;

                // Test Summary saving
                DrugInfoSummaryDocument drug = new DrugInfoSummaryDocument();
                drug.DocumentID = documentID;
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.DeleteDocument(drug, ContentDatabase.Staging, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Preview, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing drug information summary document failed.", e);
            }
        }

        [Test]
        public void DrugDelete487488()
        {
            // Set up
            try
            {

                int documentID = 487489;

                // Test Summary saving
                DrugInfoSummaryDocument drug = new DrugInfoSummaryDocument();
                drug.DocumentID = documentID;
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.DeleteDocument(drug, ContentDatabase.Staging, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Preview, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing drug information summary document failed.", e);
            }
        }

        [Test]
        public void DrugDelete487511()
        {
            // Set up
            try
            {

                int documentID = 487511;

                // Test Summary saving
                DrugInfoSummaryDocument drug = new DrugInfoSummaryDocument();
                drug.DocumentID = documentID;
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.DeleteDocument(drug, ContentDatabase.Staging, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Preview, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing drug information summary document failed.", e);
            }
        }

        [Test]
        public void DrugDelete502187()
        {
            // Set up
            try
            {

                int documentID = 502187;

                // Test Summary saving
                DrugInfoSummaryDocument drug = new DrugInfoSummaryDocument();
                drug.DocumentID = documentID;
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.DeleteDocument(drug, ContentDatabase.Staging, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Preview, "ychen");
                    drugQuery.DeleteDocument(drug, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing drug information summary document failed.", e);
            }
        }
        #endregion

        #region Private method
        private void Message(string s)
        { Console.WriteLine(s); }
        #endregion
    }

}
