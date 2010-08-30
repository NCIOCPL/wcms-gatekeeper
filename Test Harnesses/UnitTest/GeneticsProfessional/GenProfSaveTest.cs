using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DocumentObjects.GeneticsProfessional;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;
using GateKeeper.DocumentObjects;
using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;

namespace GateKeeper.UnitTest.GeneticsProfessional
{
    [TestFixture]
    public class GenProfSaveTest
    {
        #region Fields

        Utilities.Timers.HiPerfTimer perfTimer = new Utilities.Timers.HiPerfTimer();
        XmlDocument geneticsProfessionalXml = new XmlDocument();
        XmlDocument geneticsProfessional2Xml = new XmlDocument();
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
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GENETICSPROFESSIONAL.xml"))
                {
                    this.geneticsProfessionalXml.PreserveWhitespace = true;
                    this.geneticsProfessionalXml.LoadXml(srBuffer.ReadToEnd());
                }

                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GeneticsProfessional2.xml"))
                {
                    this.geneticsProfessional2Xml.PreserveWhitespace = true;
                    this.geneticsProfessional2Xml.LoadXml(srBuffer.ReadToEnd());
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
        public void TestGeneticProfessionalByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 30010007;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(111016, cdrID);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(requestData.DocumentDataString);

                GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
                GeneticsProfessionalExtractor extract = new GeneticsProfessionalExtractor();
                extract.Extract(xmlDoc, genProf, xPathManager);

                // Test saving
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    if (genProfQuery.SaveDocument(genProf, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from genetic professional summary document failed.", e);
            }
        }

        [Test]
        public void GeneticProfessional355()
        {
            // Set up
            try
            {
                XmlDocument geneticsProfessionalXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GenProf30010355.xml"))
                {
                    this.geneticsProfessionalXml.PreserveWhitespace = true;
                    this.geneticsProfessionalXml.LoadXml(srBuffer.ReadToEnd());
                }

                GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
                GeneticsProfessionalExtractor extractor = new GeneticsProfessionalExtractor();
                extractor.Extract(this.geneticsProfessionalXml, genProf, xPathManager);

                // Test saving
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    if (genProfQuery.SaveDocument(genProf, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from genetic professional summary document failed.", e);
            }
        }

        [Test]
        public void GeneticProfessional173()
        {
            // Set up
            try
            {
                XmlDocument geneticsProfessionalXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GenProf30010173.xml"))
                {
                    this.geneticsProfessionalXml.PreserveWhitespace = true;
                    this.geneticsProfessionalXml.LoadXml(srBuffer.ReadToEnd());
                }

                GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
                GeneticsProfessionalExtractor extractor = new GeneticsProfessionalExtractor();
                extractor.Extract(this.geneticsProfessionalXml, genProf, xPathManager);

                // Test saving
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    if (genProfQuery.SaveDocument(genProf, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from genetic professional summary document failed.", e);
            }

        }

        [Test]
        public void GeneticProfessional007()
        {
            // Set up
            try
            {
                XmlDocument geneticsProfessionalXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GenProf30010007.xml"))
                {
                    this.geneticsProfessionalXml.PreserveWhitespace = true;
                    this.geneticsProfessionalXml.LoadXml(srBuffer.ReadToEnd());
                }

                GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
                GeneticsProfessionalExtractor extractor = new GeneticsProfessionalExtractor();
                extractor.Extract(this.geneticsProfessionalXml, genProf, xPathManager);

                // Test saving
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    if (genProfQuery.SaveDocument(genProf, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from genetic professional summary document failed.", e);
            }

        }

        [Test]
        public void GeneticProfessional249()
        {
            // Set up
            try
            {
                XmlDocument geneticsProfessionalXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/GenProf30010249.xml"))
                {
                    this.geneticsProfessionalXml.PreserveWhitespace = true;
                    this.geneticsProfessionalXml.LoadXml(srBuffer.ReadToEnd());
                }

                GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
                GeneticsProfessionalExtractor extractor = new GeneticsProfessionalExtractor();
                extractor.Extract(this.geneticsProfessionalXml, genProf, xPathManager);

                // Test saving
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    if (genProfQuery.SaveDocument(genProf, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from genetic professional summary document failed.", e);
            }

        }

        #endregion
    }
}
