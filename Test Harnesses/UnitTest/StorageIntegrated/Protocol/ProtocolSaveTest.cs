using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Protocol;
using GateKeeper.ContentRendering;

using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;

namespace GateKeeper.UnitTest.StorageIntegrated.Protocol
{
    [TestFixture, Explicit]
    public class ProtocolSaveTest
    {
        #region Fields

        Utilities.Timers.HiPerfTimer perfTimer = new Utilities.Timers.HiPerfTimer();
        DocumentXPathManager xPathManager = new DocumentXPathManager();
        List<double> durationList = new List<double>();

       #endregion

        #region Setup and Teardown

        [TestFixtureSetUp]
        public void SetUp()
        {
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            UTHelper.CalculateAverage(durationList);
        }

        [TearDown]
        public void UnitTearDown()
        {
            Console.WriteLine("UT tear down...");
        }

        #endregion

        #region Utility Functions

        #endregion

        #region Unit Tests

        /// <summary>
        /// Test multiple sites
        /// </summary>
        /// 
        [Test]
        public void TestProtocolByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 66727; // 350052;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(658, cdrID);
                XmlDocument protocolDoc = new XmlDocument();
                protocolDoc.PreserveWhitespace = true;
                protocolDoc.LoadXml(requestData.DocumentDataString);

                ProtocolDocument protocol = new ProtocolDocument();
                ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
                pe.Extract(protocolDoc, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test protocol saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }

                Console.WriteLine("Saving complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

        [Test]
        public void TestCTGovByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 650807; // 360607;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(658, cdrID);
                XmlDocument protocolDoc = new XmlDocument();
                protocolDoc.PreserveWhitespace = true;
                protocolDoc.LoadXml(requestData.DocumentDataString);

                ProtocolDocument protocol = new ProtocolDocument();
                CTGovProtocolExtractor extractor = new CTGovProtocolExtractor(xPathManager);
                extractor.Extract(protocolDoc, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }

            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }


        [Test]
        public void TestProtocolWithMultipleSites491296()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");

                perfTimer.Start();

                XmlDocument protocolXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolWithMultipleSites491296.xml"))
                {
                    protocolXml.PreserveWhitespace = true;
                    protocolXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
                pe.Extract(protocolXml, protocol);
                perfTimer.Stop();

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test protocol saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }

                Console.WriteLine("Saving complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
 
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

        [Test]
        public void TestProtocol517312()
        {
            // Set up
            try
            {
                Console.WriteLine("=============== start =================");

                perfTimer.Start();

                XmlDocument protocolXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Protocol517312.xml"))
                {
                    protocolXml.PreserveWhitespace = true;
                    protocolXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
                pe.Extract(protocolXml, protocol);
                perfTimer.Stop();

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test protocol saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }

                Console.WriteLine("Saving complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);

            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

        [Test]
        public void TestProtocolCTProtocol349876()
        {
            // Set up
            try
            {
                Console.WriteLine("=============== start =================");

                perfTimer.Start();

                XmlDocument protocolXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/CTProtocol349876.xml"))
                {
                    protocolXml.PreserveWhitespace = true;
                    protocolXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                CTGovProtocolExtractor extractor = new CTGovProtocolExtractor(xPathManager);
                extractor.Extract(protocolXml, protocol);

                perfTimer.Stop();

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test protocol saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }

                Console.WriteLine("Saving complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);

            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

    
        /// <summary>
        /// Test multiple persons under one site
        /// </summary>
         [Test]
        public void TestProtocolWithMultiplePersons65504()
        {
            // Set up
            try
            {
                 Console.WriteLine("========================================");
                 XmlDocument protocolXml = new XmlDocument();
                 using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolWithMultiplePersonsInOneSite65504.xml"))
                 {
                     protocolXml.PreserveWhitespace = true;
                     protocolXml.LoadXml(srBuffer.ReadToEnd());
                 }

                perfTimer.Start();
                ProtocolDocument protocol = new ProtocolDocument();
                ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
                pe.Extract(protocolXml, protocol);
                perfTimer.Stop();

                Console.WriteLine("Extraction complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
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
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

         /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestProtocolSpecialCategory67377()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");

                XmlDocument protocolXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolWithSpecialCategory67377.xml"))
                {
                    protocolXml.PreserveWhitespace = true;
                    protocolXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
                pe.Extract(protocolXml, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestProtocolStudyCondition349374()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");

                XmlDocument protocolXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolWithStudyCondition349374.xml"))
                {
                    protocolXml.PreserveWhitespace = true;
                    protocolXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
                pe.Extract(protocolXml, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestProtocolClosed65755()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");

                XmlDocument protocolXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolClosed65755.xml"))
                {
                    protocolXml.PreserveWhitespace = true;
                    protocolXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
                pe.Extract(protocolXml, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestProtocolWithMultipleLeadOrg64184()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");

                XmlDocument protocolXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolWithMultipleLeadOrg64184.xml"))
                {
                    protocolXml.PreserveWhitespace = true;
                    protocolXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
                pe.Extract(protocolXml, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestProtocolWithDuplicateContactNULLPerson446178()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");

                XmlDocument protocolXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolWithDuplicateContactNullPerson446178.xml"))
                {
                    protocolXml.PreserveWhitespace = true;
                    protocolXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
                pe.Extract(protocolXml, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestCTGovWithOverallContact360607()
        {
            // Set up
            try
            {
                 XmlDocument protocoXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolCTGovWithOverallContact360607.xml"))
                {
                    protocoXml.PreserveWhitespace = true;
                    protocoXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                CTGovProtocolExtractor extractor = new CTGovProtocolExtractor(xPathManager);
                extractor.Extract(protocoXml, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }

         }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestCTGovWithOverallOfficial363917()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                XmlDocument protocoXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolCTGovWithOverallOfficial363917.xml"))
                {
                    protocoXml.PreserveWhitespace = true;
                    protocoXml.LoadXml(srBuffer.ReadToEnd());
                }

                ProtocolDocument protocol = new ProtocolDocument();
                CTGovProtocolExtractor extractor = new CTGovProtocolExtractor(xPathManager);
                extractor.Extract(protocoXml, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving protocol complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestCTGovProtocolWithMultipleSites467954()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                XmlDocument protocoXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ProtocolCTGovWtihMultipleSites467954.xml"))
                {
                    protocoXml.PreserveWhitespace = true;
                    protocoXml.LoadXml(srBuffer.ReadToEnd());
                }

                perfTimer.Start();
                ProtocolDocument protocol = new ProtocolDocument();
                CTGovProtocolExtractor extractor = new CTGovProtocolExtractor(xPathManager);
                extractor.Extract(protocoXml, protocol);

                ProtocolRenderer render = new ProtocolRenderer();
                render.Render(protocol);

                // Test Summary saving
                using (ProtocolQuery protQuery = new ProtocolQuery())
                {
                    if (protQuery.SaveDocument(protocol, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

                perfTimer.Stop();

                Console.WriteLine("Extraction complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);

            }
            catch (Exception e)
            {
                throw new Exception("Testing Error:Saving data from protocol document failed.", e);
            }
        }

       #endregion
    }
}
