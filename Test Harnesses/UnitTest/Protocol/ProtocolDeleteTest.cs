using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using NUnit.Framework;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Protocol;
using GateKeeper.ContentRendering;


namespace GateKeeper.UnitTest.Protocol
{
    [TestFixture]
    public class ProtocolDeleteTest
    {
        #region Fields

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
        public void ProtocolbyCDRID()
        {
            // Set up
            try
            {

               // int documentID = 504047;
                int documentID = 482277;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                   // protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    //protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete363917()
        {
            // Set up
            try
            {

                int documentID = 363917;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete491296()
        {
            // Set up
            try
            {

                int documentID = 491296;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete65504()
        {
            // Set up
            try
            {

                int documentID = 65504;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete67377()
        {
            // Set up
            try
            {

                int documentID = 67377;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete349374()
        {
            // Set up
            try
            {

                int documentID = 349374;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete65755()
        {
            // Set up
            try
            {

                int documentID = 65755;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete64184()
        {
            // Set up
            try
            {

                int documentID = 64184;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete446178()
        {
            // Set up
            try
            {

                int documentID = 446178;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete360607()
        {
            // Set up
            try
            {

                int documentID = 360607;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolDelete467954()
        {
            // Set up
            try
            {

                int documentID = 467954;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Staging, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Preview, "ychen");
                    protocolQuery.DeleteDocument(protocol, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }


        #endregion
    }
}
