using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DocumentObjects.Organization;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;

namespace GateKeeper.UnitTest.StorageIntegrated.Organization
{
    [TestFixture, Explicit]
    public class OrganizationDeleteTest
    {
        #region Fields

        #endregion

        #region Unit Tests

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgDelete346204()
        {
            // Set up
            try
            {
                int documentID = 152501;
                OrganizationDocument org = new OrganizationDocument();
                org.DocumentID = documentID;
                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.DeleteDocument(org, ContentDatabase.Staging, "ychen");
                    oQuery.DeleteDocument(org, ContentDatabase.Preview, "ychen");
                    oQuery.DeleteDocument(org, ContentDatabase.Live, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting data from organization document failed.", e);

            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgDelete346415()
        {
            // Set up
            try
            {
                int documentID = 346415;
                OrganizationDocument org = new OrganizationDocument();
                org.DocumentID = documentID;
                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.DeleteDocument(org, ContentDatabase.Staging, "ychen");
                    oQuery.DeleteDocument(org, ContentDatabase.Preview, "ychen");
                    oQuery.DeleteDocument(org, ContentDatabase.Live, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting data from organization document failed.", e);

            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgDelete27218()
        {
            // Set up
            try
            {
                int documentID = 27218;
                OrganizationDocument org = new OrganizationDocument();
                org.DocumentID = documentID;
                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.DeleteDocument(org, ContentDatabase.Staging, "ychen");
                    oQuery.DeleteDocument(org, ContentDatabase.Preview, "ychen");
                    oQuery.DeleteDocument(org, ContentDatabase.Live, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting data from organization document failed.", e);

            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgDelete27271()
        {
            // Set up
            try
            {
                int documentID = 27271;
                OrganizationDocument org = new OrganizationDocument();
                org.DocumentID = documentID;
                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.DeleteDocument(org, ContentDatabase.Staging, "ychen");
                    oQuery.DeleteDocument(org, ContentDatabase.Preview, "ychen");
                    oQuery.DeleteDocument(org, ContentDatabase.Live, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleting data from organization document failed.", e);

            }
        }
        #endregion
    }
}
