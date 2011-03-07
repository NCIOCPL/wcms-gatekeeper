using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;
using GateKeeper.DocumentObjects;
using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;

namespace GateKeeper.UnitTest.DrugInfoSummary
{
    [TestFixture]
    public class DrugInfoSummarySaveTest
    {
        #region Fields
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
        public void TestDrugByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 492038;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(111010, cdrID);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(requestData.DocumentDataString);
                DrugInfoSummaryDocument drugDoc = new DrugInfoSummaryDocument();
                DrugInfoSummaryExtractor extractor = new DrugInfoSummaryExtractor();
                extractor.Extract(xmlDoc, drugDoc, xPathManager);
                DrugInfoSummaryRenderer render = new DrugInfoSummaryRenderer();
                render.Render(drugDoc);

                // Test Summary saving
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    if (drugQuery.SaveDocument(drugDoc, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from drug information summary document failed.", e);
            }
        }

        [Test]
        public void Drug487488()
        {
            // Set up
            try
            {
                string xml = ReadXml(@"./XMLData/DrugInfoSummary487488.xml");

                DrugInfoSummaryDocument drugDoc = new DrugInfoSummaryDocument();
                XmlDocument drugXML = new XmlDocument();
                drugXML.PreserveWhitespace = true;
                drugXML.LoadXml(xml);
                DrugInfoSummaryExtractor extractor = new DrugInfoSummaryExtractor();
                extractor.Extract(drugXML, drugDoc, xPathManager);
                DrugInfoSummaryRenderer render = new DrugInfoSummaryRenderer();
                render.Render(drugDoc);

                // Test Summary saving
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    if (drugQuery.SaveDocument(drugDoc, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from drug information summary document failed.", e);
            }
        }

        [Test]
        public void Drug502187()
        {
            // Set up
            try
            {
                string xml = ReadXml(@"./XMLData/DrugInfoSummary502187.xml");

                DrugInfoSummaryDocument drugDoc = new DrugInfoSummaryDocument();
                XmlDocument drugXML = new XmlDocument();
                drugXML.PreserveWhitespace = true;
                drugXML.LoadXml(xml);
                DrugInfoSummaryExtractor extractor = new DrugInfoSummaryExtractor();
                extractor.Extract(drugXML, drugDoc, xPathManager);
                DrugInfoSummaryRenderer render = new DrugInfoSummaryRenderer();
                render.Render(drugDoc);

                // Test Summary saving
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    if (drugQuery.SaveDocument(drugDoc, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from drug information summary document failed.", e);
            }
        }

        [Test]
        public void Drug487511()
        {
            // Set up
            try
            {
                string xml = ReadXml(@"./XMLData/DrugInfoSummary487511.xml");

                DrugInfoSummaryDocument drugDoc = new DrugInfoSummaryDocument();
                XmlDocument drugXML = new XmlDocument();
                drugXML.PreserveWhitespace = true;
                drugXML.LoadXml(xml);
                DrugInfoSummaryExtractor extractor = new DrugInfoSummaryExtractor();
                extractor.Extract(drugXML, drugDoc, xPathManager);
                DrugInfoSummaryRenderer render = new DrugInfoSummaryRenderer();
                render.Render(drugDoc);

                // Test Summary saving
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    if (drugQuery.SaveDocument(drugDoc, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from drug information summary document failed.", e);
            }
        }

        [Test]
        public void Drug495323()
        {
            // Set up
            try
            {
                string xml = ReadXml(@"./XMLData/DrugInfoSummary495323.xml");

                DrugInfoSummaryDocument drugDoc = new DrugInfoSummaryDocument();
                XmlDocument drugXML = new XmlDocument();
                drugXML.PreserveWhitespace = true;
                drugXML.LoadXml(xml);
                DrugInfoSummaryExtractor extractor = new DrugInfoSummaryExtractor();
                extractor.Extract(drugXML, drugDoc, xPathManager);
                DrugInfoSummaryRenderer render = new DrugInfoSummaryRenderer();
                render.Render(drugDoc);

                // Test Summary saving
                using (DrugInfoSummaryQuery drugQuery = new DrugInfoSummaryQuery())
                {
                    if (drugQuery.SaveDocument(drugDoc, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from drug information summary document failed.", e);
            }
        }

        #endregion
        #region
        /// <summary>
        /// Utility to read in an XML file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string ReadXml(string fileName)
        {
            string xml = string.Empty;

            using (StreamReader sr = new StreamReader(fileName))
            {
                xml = sr.ReadToEnd();
            }

            return xml;
        }
        #endregion
    }
     
}
