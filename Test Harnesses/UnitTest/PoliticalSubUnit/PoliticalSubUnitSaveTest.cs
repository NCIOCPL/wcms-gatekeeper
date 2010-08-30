using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects.PoliticalSubUnit;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;

namespace GateKeeper.UnitTest.PoliticalSubUnit
{
    [TestFixture]
    public class PoliticalSubUnitSaveTest
    {
        #region Fields

        Utilities.Timers.HiPerfTimer perfTimer = new Utilities.Timers.HiPerfTimer();
        XmlDocument polSubXml = new XmlDocument();
        DocumentXPathManager xPathManager = new DocumentXPathManager();


        #endregion
        #region Private field

         /// <summary>
        /// Setup run once for all Unit Tests.
        /// </summary>
//        [TestFixtureSetUp]
        private void FixtureSetUp(string xmlFile)
        {
            try
            {
                using (StreamReader sr = new StreamReader(xmlFile))
                {
                    this.polSubXml.PreserveWhitespace = true;
                    this.polSubXml.LoadXml(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private void MakeExtraction()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Testing PoliticalSubUnitTestFixture.Extract()...");

            perfTimer.Start();
            PoliticalSubUnitDocument polSub = new PoliticalSubUnitDocument();
            PoliticalSubUnitExtractor.Extract(this.polSubXml, polSub, xPathManager);
            perfTimer.Stop();

            // Test saving
            using (PoliticalSubUnitQuery polQuery = new PoliticalSubUnitQuery())
            {
                if (polQuery.SaveDocument(polSub, "ychen"))
                    Console.WriteLine("Saving Summary complete.");
            }
        }
        #endregion

       #region Setup and Teardown

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
        /// 
        [Test]
        public void TestPoliticalSubUnitByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 510926;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(25, cdrID);
                polSubXml.PreserveWhitespace = true;
                polSubXml.LoadXml(requestData.DocumentDataString);

                MakeExtraction();
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from PoliticalSubUnit document failed.", e);
            }
        }

        [Test]
        public void PoliticalSubUnit43856()
        {
            // Set up
            try
            {
                FixtureSetUp(@"./XMLData/PoliticalSubUnit43856.xml");
                MakeExtraction();
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from PoliticalSubUnit document failed.", e);
            }
        }

        [Test]
        public void PoliticalSubUnit43870()
        {
            // Set up
            try
            {
                FixtureSetUp(@"./XMLData/PoliticalSubUnit43870.xml");
                MakeExtraction();
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from PoliticalSubUnit document failed.", e);
            }
        }

        [Test]
        public void PoliticalSubUnit43920()
        {
            // Set up
            try
            {
                FixtureSetUp(@"./XMLData/PoliticalSubUnit43920.xml");
                MakeExtraction();
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from PoliticalSubUnit document failed.", e);
            }
        }
 
        #endregion
    }
}
