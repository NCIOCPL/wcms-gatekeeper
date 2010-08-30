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


namespace GateKeeper.UnitTest.Protocol
{
    [TestFixture]
    public class ProtocolPushTest
    {
        #region Fields

        #endregion

        #region Setup and Teardown

        [TestFixtureSetUp]
        public void SetUp()
        {
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
        }

        [TearDown]
        public void UnitTearDown()
        {
        }

        #endregion

        #region Utility Functions

        #endregion

        #region Unit Tests
        [Test]
        public void ProtocolPushByCDRID()
        {
            // Set up
            try
            {

                int documentID = 422341;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush363917()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush491296()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }


        [Test]
        public void ProtocolPush517312()
        {
            // Set up
            try
            {

                int documentID = 517312;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }


        [Test]
        public void ProtocolPush349876()
        {
            // Set up
            try
            {

                int documentID = 349876;

                // Test Protocol saving
                ProtocolDocument protocol = new ProtocolDocument();
                protocol.DocumentID = documentID;
                using (ProtocolQuery protocolQuery = new ProtocolQuery())
                {
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush65504()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush367377()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush349374()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush65755()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush64184()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush446178()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush360607()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting protocol document failed.", e);
            }
        }

        [Test]
        public void ProtocolPush467954()
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
                    protocolQuery.PushDocumentToPreview(protocol, "ychen");
                    protocolQuery.PushDocumentToLive(protocol, "ychen");

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
