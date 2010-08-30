using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using NUnit.Framework;
using GateKeeper.Common;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.ContentRendering;

namespace GateKeeper.UnitTest.GlossaryTerm
{
    /// <summary>
    /// Glossary Term extraction unit test.
    /// </summary>
    [TestFixture]
    public class GlossaryPushTest
    {
        #region Unit Tests
        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void TestGlossaryTermPushByCDRID()
        {
            // Set up
            try
            {
                int documentID = 335128;
                // Test query
                GlossaryTermDocument gt = new GlossaryTermDocument();
                gt.DocumentID = documentID;
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.PushDocumentToPreview(gt, "ychen");
                    GTQuery.PushDocumentToLive(gt, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Push data from glossary term document failed.", e);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void GlossaryTermPushEnglish()
        {
            // Set up
            try
            {
                int documentID = 350231;
                // Test query
                GlossaryTermDocument gt = new GlossaryTermDocument();
                gt.DocumentID = documentID;
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.PushDocumentToPreview(gt, "ychen");
                    GTQuery.PushDocumentToLive(gt, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Push data from glossary term document failed.", e);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void GlossaryTermPushPron()
        {
            // Set up
            try
            {
                int documentID = 322891;
                // Test query
                GlossaryTermDocument gt = new GlossaryTermDocument();
                gt.DocumentID = documentID;
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.PushDocumentToPreview(gt, "ychen");
                    GTQuery.PushDocumentToLive(gt, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Push data from glossary term document failed.", e);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void GlossaryTermPushMediaAndMultLang()
        {
            // Set up
            try
            {
                int documentID = 304687;
                // Test query
                GlossaryTermDocument gt = new GlossaryTermDocument();
                gt.DocumentID = documentID;
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.PushDocumentToPreview(gt, "ychen");
                    GTQuery.PushDocumentToLive(gt, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Push data from glossary term document failed.", e);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void GlossaryTermPushSpanish()
        {
            // Set up
            try
            {
                int documentID = 335158;
                // Test query
                GlossaryTermDocument gt = new GlossaryTermDocument();
                gt.DocumentID = documentID;
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    GTQuery.PushDocumentToPreview(gt, "ychen");
                    GTQuery.PushDocumentToLive(gt, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Push data from glossary term document failed.", e);
            }
        }


        #endregion
    }

}
