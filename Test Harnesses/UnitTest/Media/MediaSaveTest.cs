using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;
using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;


namespace GateKeeper.UnitTest.Media
{
    [TestFixture]
    public class MediaSaveTest
    {
        #region Fields

        Utilities.Timers.HiPerfTimer perfTimer = new Utilities.Timers.HiPerfTimer();
        DocumentXPathManager xPathManager = new DocumentXPathManager();

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
        /// 
        [Test]
        public void TestMediaByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 415590;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(101130, cdrID);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(requestData.DocumentDataString);
                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);

                MediaDocument media = new MediaDocument();
                MediaExtractor.Extract(xmlDoc, media, 466541, xPathManager);

                MediaRenderer render = new MediaRenderer();
                render.Render(media);

                // Test query
                using (MediaQuery mediaQuery = new MediaQuery())
                {
                    if (mediaQuery.SaveDocument(media, "ychen"))
                        Console.WriteLine("Saving media complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from media document failed.", e);
            }
        }

        [Test]
        public void MediaTest466541()
        {
            // Set up
            try
            {
                XmlDocument mediaXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Media466541.xml"))
                {
                    mediaXml.PreserveWhitespace = true;
                    mediaXml.LoadXml(srBuffer.ReadToEnd());
                }

                MediaDocument media = new MediaDocument();
                MediaExtractor.Extract(mediaXml, media, 466541, xPathManager);

                MediaRenderer render = new MediaRenderer();
                render.Render(media);

                // Test query
                using (MediaQuery mediaQuery = new MediaQuery())
                {
                    if (mediaQuery.SaveDocument(media, "ychen"))
                        Console.WriteLine("Saving media complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from media document failed.", e);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void MediaTest415511()
        {
            // Set up
            try
            {
                XmlDocument mediaXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Media415511.xml"))
                {
                    mediaXml.PreserveWhitespace = true;
                    mediaXml.LoadXml(srBuffer.ReadToEnd());
                }

                MediaDocument media = new MediaDocument();
                MediaExtractor.Extract(mediaXml, media, 415511, xPathManager);

                MediaRenderer render = new MediaRenderer();
                render.Render(media);

                // Test query
                using (MediaQuery mediaQuery = new MediaQuery())
                {
                    if (mediaQuery.SaveDocument(media, "ychen"))
                        Console.WriteLine("Saving media complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from media document failed.", e);
            }
        }


        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void MediaTest415499()
        {
            // Set up
            try
            {
                XmlDocument mediaXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Media415499.xml"))
                {
                    mediaXml.PreserveWhitespace = true;
                    mediaXml.LoadXml(srBuffer.ReadToEnd());
                }

                MediaDocument media = new MediaDocument();
                MediaExtractor.Extract(mediaXml, media, 415499, xPathManager);

                MediaRenderer render = new MediaRenderer();
                render.Render(media);

                // Test query
                using (MediaQuery mediaQuery = new MediaQuery())
                {
                    if (mediaQuery.SaveDocument(media, "ychen"))
                        Console.WriteLine("Saving media complete.");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from media document failed.", e);
            }
        }

        #endregion

        #region Private method
        private void Message(string s)
        { Console.WriteLine(s); }
        #endregion

    }
}
