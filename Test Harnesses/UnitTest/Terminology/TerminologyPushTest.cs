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

namespace GateKeeper.UnitTest.Terminology
{
    [TestFixture]
    public class TerminologyPushTest
    {
        #region Unit Tests

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void TerminologyPush37779()
        {
            // Set up
            try
            {
                int documentID = 37779;

                // Test Protocol saving
                TerminologyDocument term = new TerminologyDocument();
                term.DocumentID = documentID;
                using (TerminologyQuery terminologyQuery = new TerminologyQuery())
                {
                    terminologyQuery.PushDocumentToPreview(term, "ychen");
                    terminologyQuery.PushDocumentToLive(term, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyPush41919()
        {
            // Set up
            try
            {
                int documentID = 41919;

                // Test Protocol saving
                TerminologyDocument term = new TerminologyDocument();
                term.DocumentID = documentID;
                using (TerminologyQuery terminologyQuery = new TerminologyQuery())
                {
                    terminologyQuery.PushDocumentToPreview(term, "ychen");
                    terminologyQuery.PushDocumentToLive(term, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyPush43236()
        {
            // Set up
            try
            {
                int documentID = 43236;

                // Test Protocol saving
                TerminologyDocument term = new TerminologyDocument();
                term.DocumentID = documentID;
                using (TerminologyQuery terminologyQuery = new TerminologyQuery())
                {
                    terminologyQuery.PushDocumentToPreview(term, "ychen");
                    terminologyQuery.PushDocumentToLive(term, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyPush39347()
        {
            // Set up
            try
            {
                int documentID = 39347;

                // Test Protocol saving
                TerminologyDocument term = new TerminologyDocument();
                term.DocumentID = documentID;
                using (TerminologyQuery terminologyQuery = new TerminologyQuery())
                {
                    terminologyQuery.PushDocumentToPreview(term, "ychen");
                    terminologyQuery.PushDocumentToLive(term, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyPush37900()
        {
            // Set up
            try
            {
                int documentID = 37900;

                // Test Protocol saving
                TerminologyDocument term = new TerminologyDocument();
                term.DocumentID = documentID;
                using (TerminologyQuery terminologyQuery = new TerminologyQuery())
                {
                    terminologyQuery.PushDocumentToPreview(term, "ychen");
                    terminologyQuery.PushDocumentToLive(term, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyPush39069()
        {
            // Set up
            try
            {
                int documentID = 39069;

                // Test Protocol saving
                TerminologyDocument term = new TerminologyDocument();
                term.DocumentID = documentID;
                using (TerminologyQuery terminologyQuery = new TerminologyQuery())
                {
                    terminologyQuery.PushDocumentToPreview(term, "ychen");
                    terminologyQuery.PushDocumentToLive(term, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing terminology document failed.", e);
            }
        }

        #endregion
    }
}
