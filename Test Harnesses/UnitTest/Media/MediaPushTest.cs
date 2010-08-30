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


namespace GateKeeper.UnitTest.Media
{
    [TestFixture]
    public class MediaPushTest
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

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void MediaTestPush466541()
        {
            // Set up
            try
            {
                int documentID = 466541;
                // Test query
                MediaDocument media = new MediaDocument();
                media.DocumentID = documentID;
                using (MediaQuery mediaQuery = new MediaQuery())
                {
                   mediaQuery.PushDocumentToPreview(media, "ychen");
                }

                using (MediaQuery mediaQuery = new MediaQuery())
                {
                    mediaQuery.PushDocumentToLive(media, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing media document failed.", e);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void MediaTestPush415511()
        {
            // Set up
            try
            {
                int documentID = 415511;
                // Test query
                MediaDocument media = new MediaDocument();
                media.DocumentID = documentID;
                // Test query
                using (MediaQuery mediaQuery = new MediaQuery())
                {
                    mediaQuery.PushDocumentToPreview(media, "ychen");
                }

                using (MediaQuery mediaQuery = new MediaQuery())
                {
                    mediaQuery.PushDocumentToLive(media, "ychen");
                }

            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing media document failed.", e);
            }
        }


        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void MediaTestPush415499()
        {
            // Set up
            try
            {
                int documentID = 415499;
                // Test query
                MediaDocument media = new MediaDocument();
                media.DocumentID = documentID;
                using (MediaQuery mediaQuery = new MediaQuery())
                {
                    mediaQuery.PushDocumentToPreview(media, "ychen");
                }

                using (MediaQuery mediaQuery = new MediaQuery())
                {
                    mediaQuery.PushDocumentToLive(media, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing media document failed.", e);
            }
        }

        #endregion
    }
}
