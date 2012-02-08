using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Terminology;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.ContentRendering;
using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;

namespace GateKeeper.UnitTest.StorageIntegrated.Terminology
{
    [TestFixture, Explicit]
    public class TerminologySaveTest
    {
        #region Fields

        Utilities.Timers.HiPerfTimer perfTimer = new Utilities.Timers.HiPerfTimer();
        List<double> durationList = new List<double>();
        DocumentXPathManager xPathManager = new DocumentXPathManager();

        #endregion

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
            UTHelper.CalculateAverage(durationList);
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
        public void TestTerminologyByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 510285;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(101011, cdrID);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(requestData.DocumentDataString); 
                TerminologyDocument term = new TerminologyDocument();
                TerminologyExtractor extract = new TerminologyExtractor();
                extract.Extract(xmlDoc, term, xPathManager);
                TerminologyRenderer render = new TerminologyRenderer();
                render.Render(term);

                // Test terminology saving
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    if (termQuery.SaveDocument(term, "ychen"))
                        Console.WriteLine("Saving Terminology complete.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

        [Test]
        public void TestTerminologyDictionary37779()
        {
            // Set up
            try
            {
                this.perfTimer.Start();
                XmlDocument terminologyXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/TerminologyDic37779.xml"))
                {
                    terminologyXml.PreserveWhitespace = true;
                    terminologyXml.LoadXml(srBuffer.ReadToEnd());
                }
                TerminologyDocument term = new TerminologyDocument();
                TerminologyExtractor extractor = new TerminologyExtractor();
                extractor.Extract(terminologyXml, term, xPathManager);
                TerminologyRenderer render = new TerminologyRenderer();
                render.Render(term);

                // Test terminology saving

                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    if (termQuery.SaveDocument(term, "ychen"))
                        Console.WriteLine("Saving Terminology complete.");
                }
                
                this.perfTimer.Stop();

                Console.WriteLine("Testing TestTerminologyDictionary complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
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
        public void TestTeminologyDicTest41919()
        {
            // Set up
            try
            {

                XmlDocument terminologyXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/TerminologyMenu41919.xml"))
                {
                    terminologyXml.PreserveWhitespace = true;
                    terminologyXml.LoadXml(srBuffer.ReadToEnd());
                }

                TerminologyDocument term = new TerminologyDocument();
                TerminologyExtractor extractor = new TerminologyExtractor();
                extractor.Extract(terminologyXml, term, xPathManager);
                TerminologyRenderer render = new TerminologyRenderer();
                render.Render(term);

                // Test terminology saving
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    if (termQuery.SaveDocument(term, "ychen"))
                        Console.WriteLine("Saving Terminology complete.");
                }

                this.perfTimer.Stop();

                Console.WriteLine("Testing TestTerminologyDictionary complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);

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
        public void TestTeminologyDefinition43236()
        {
            // Set up
            try
            {
                XmlDocument terminologyXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/TerminologyDefinition43236.xml"))
                {
                    terminologyXml.PreserveWhitespace = true;
                    terminologyXml.LoadXml(srBuffer.ReadToEnd());
                }

                TerminologyDocument term = new TerminologyDocument();
                TerminologyExtractor extractor = new TerminologyExtractor();
                extractor.Extract(terminologyXml, term, xPathManager);
                TerminologyRenderer render = new TerminologyRenderer();
                render.Render(term);

                // Test terminology saving
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    if (termQuery.SaveDocument(term, "ychen"))
                        Console.WriteLine("Saving Terminology complete.");
                }

                this.perfTimer.Stop();

                Console.WriteLine("Testing TestTerminologyDictionary complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
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
        public void TestTeminologyMenuGrandpa39347()
        {
            // Set up
            try
            {
                XmlDocument terminologyXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/TerminologyMenuGrandpa39347.xml"))
                {
                    terminologyXml.PreserveWhitespace = true;
                    terminologyXml.LoadXml(srBuffer.ReadToEnd());
                }

                TerminologyDocument term = new TerminologyDocument();
                TerminologyExtractor extractor = new TerminologyExtractor();
                extractor.Extract(terminologyXml, term, xPathManager);
                TerminologyRenderer render = new TerminologyRenderer();
                render.Render(term);

                // Test terminology saving
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    if (termQuery.SaveDocument(term, "ychen"))
                        Console.WriteLine("Saving Terminology complete.");
                }

                this.perfTimer.Stop();

                Console.WriteLine("Testing TestTerminologyDictionary complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
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
        public void TestTeminologyMenuParent37900()
        {
            // Set up
            try
            {

                XmlDocument terminologyXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/TerminologyMenuParent37900.xml"))
                {
                    terminologyXml.PreserveWhitespace = true;
                    terminologyXml.LoadXml(srBuffer.ReadToEnd());
                }

                TerminologyDocument term = new TerminologyDocument();
                TerminologyExtractor extractor = new TerminologyExtractor();
                extractor.Extract(terminologyXml, term, xPathManager);
                TerminologyRenderer render = new TerminologyRenderer();
                render.Render(term);

                // Test terminology saving
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    if (termQuery.SaveDocument(term, "ychen"))
                        Console.WriteLine("Saving terminology complete.");
                }

                this.perfTimer.Stop();

                Console.WriteLine("Testing TestTerminologyDictionary complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);

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
        public void TestTeminologyMenuChild39069()
        {
            // Set up
            try
            {

                XmlDocument terminologyXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/TerminologyMenuChild39069.xml"))
                {
                    terminologyXml.PreserveWhitespace = true;
                    terminologyXml.LoadXml(srBuffer.ReadToEnd());
                }

                TerminologyDocument term = new TerminologyDocument();
                TerminologyExtractor extractor = new TerminologyExtractor();
                extractor.Extract(terminologyXml, term, xPathManager);
                TerminologyRenderer render = new TerminologyRenderer();
                render.Render(term);

                // Test terminology saving
                using (TerminologyQuery termQuery = new TerminologyQuery())
                {
                    if (termQuery.SaveDocument(term, "ychen"))
                        Console.WriteLine("Saving terminology complete.");
                }

                this.perfTimer.Stop();

                Console.WriteLine("Testing TestTerminologyDictionary complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);

            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from terminology document failed.", e);
            }
        }

        #endregion
    }
}
