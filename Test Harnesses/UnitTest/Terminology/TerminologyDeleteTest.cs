using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Terminology;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.ContentRendering;

namespace GateKeeper.UnitTest.Terminology
{
    [TestFixture]
    public class TerminologyDeleteTest
    {
        #region Unit Tests

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void TerminologyDelete37779()
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
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Staging, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Preview, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyDelete41919()
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
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Staging, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Preview, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyDelete43236()
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
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Staging, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Preview, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyDelete39347()
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
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Staging, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Preview, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyDelete37900()
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
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Staging, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Preview, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing terminology document failed.", e);
            }
        }

        [Test]
        public void TerminologyDelete39069()
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
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Staging, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Preview, "ychen");
                    terminologyQuery.DeleteDocument(term, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing terminology document failed.", e);
            }
        }

        #endregion
    }
}
