using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

using GateKeeper.ContentRendering;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.DrugInfoSummary;

namespace GateKeeper.UnitTest.StorageIntegrated.DrugInfoSummary
{
    [TestFixture, Explicit]
    public class DrugInfoSummaryPushTest
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
        public void PushDrugByCDRID()
        {
            // Set up
            try
            {

                int documentID = 487488;

                // Test Summary saving
                DrugInfoSummaryDocument drug = new DrugInfoSummaryDocument();
                drug.DocumentID = documentID;
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.PushDocumentToPreview(drug, "ychen");
                    drugQuery.PushDocumentToLive(drug, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing drug information summary document failed.", e);
            }
        }

        [Test]
        public void DrugPushTo487488()
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
                    drugQuery.PushDocumentToPreview(drug, "ychen");
                    drugQuery.PushDocumentToLive(drug, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing drug information summary document failed.", e);
            }
        }

        [Test]
        public void DrugPushTo487511()
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
                    drugQuery.PushDocumentToPreview(drug, "ychen");
                    drugQuery.PushDocumentToLive(drug, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing drug information summary document failed.", e);
            }
        }

        [Test]
        public void DrugPushTo502187()
        {
            // Set up
            try
            {

                int documentID = 492038;

                // Test Summary saving
                DrugInfoSummaryDocument drug = new DrugInfoSummaryDocument();
                drug.DocumentID = documentID;
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    drugQuery.PushDocumentToPreview(drug, "ychen");
                    drugQuery.PushDocumentToLive(drug, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing drug information summary document failed.", e);
            }
        }
        #endregion
     }

}
