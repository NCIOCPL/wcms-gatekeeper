using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DocumentObjects.Organization;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;

namespace GateKeeper.UnitTest.Organization
{
    [TestFixture]
    public class OrganizationPushTest
    {
        #region Fields

        #endregion

        #region Unit Tests

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgPush346204()
        {
            // Set up
            try
            {
                int documentID = 152501;
                OrganizationDocument org = new OrganizationDocument();
                org.DocumentID = documentID;
                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.PushDocumentToPreview(org, "ychen");
                    oQuery.PushDocumentToLive(org, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing data from organization document failed.", e);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgPush346415()
        {
            // Set up
            try
            {
                int documentID = 346415;
                OrganizationDocument org = new OrganizationDocument();
                org.DocumentID = documentID;
                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.PushDocumentToPreview(org, "ychen");
                    oQuery.PushDocumentToLive(org, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing data from organization document failed.", e);

            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgPush27218()
        {
            // Set up
            try
            {
                int documentID = 27218;
                OrganizationDocument org = new OrganizationDocument();
                org.DocumentID = documentID;
                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.PushDocumentToPreview(org, "ychen");
                    oQuery.PushDocumentToLive(org, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing data from organization document failed.", e);

            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgPush27271()
        {
            // Set up
            try
            {
                int documentID = 27271;
                OrganizationDocument org = new OrganizationDocument();
                org.DocumentID = documentID;
                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.PushDocumentToPreview(org, "ychen");
                    oQuery.PushDocumentToLive(org, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing data from organization document failed.", e);

            }
        }
        #endregion
    }
}
