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
using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;

namespace GateKeeper.UnitTest.StorageIntegrated.Organization
{
    [TestFixture, Explicit]
    public class OrganizationSaveTest
    {
        #region Fields

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
        public void TestOrganizationByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 35945;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(25, cdrID);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(requestData.DocumentDataString);

                OrganizationDocument org = new OrganizationDocument();
                OrganizationExtractor.Extract(xmlDoc, org, xPathManager);

                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.SaveDocument(org, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from organization document failed.", e);

            }
        }

        [Test]
        public void OrgSave346204()
        {
            // Set up
            try
            {
                XmlDocument orgXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Organization346204.xml"))
                {
                    orgXml.PreserveWhitespace = true;
                    orgXml.LoadXml(srBuffer.ReadToEnd());
                }
                OrganizationDocument org = new OrganizationDocument();
                OrganizationExtractor.Extract(orgXml, org, xPathManager);

                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.SaveDocument(org, "ychen");
                }
              }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from organization document failed.", e);

            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgSave346415()
        {
            // Set up
            try
            {
                // Set up
                try
                {
                    XmlDocument orgXml = new XmlDocument();
                    using (StreamReader srBuffer = new StreamReader(@"./XMLData/Organization346415.xml"))
                    {
                        orgXml.PreserveWhitespace = true;
                        orgXml.LoadXml(srBuffer.ReadToEnd());
                    }
                    OrganizationDocument org = new OrganizationDocument();
                    OrganizationExtractor.Extract(orgXml, org, xPathManager);

                    using (OrganizationQuery oQuery = new OrganizationQuery())
                    {
                        oQuery.SaveDocument(org, "ychen");
                    }
                }
                catch (NUnit.Framework.AssertionException assertEx)
                {
                    Console.WriteLine("Assertion failed: " + assertEx.Message);
                    throw assertEx;
                }
                catch (Exception e)
                {
                    throw new Exception("Testing Error: Saving data from organization document failed.", e);

                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from organization document failed.", e);
            }
        }

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgSave27218()
        {
             // Set up
             try
             {
                 XmlDocument orgXml = new XmlDocument();
                 using (StreamReader srBuffer = new StreamReader(@"./XMLData/Organization27218.xml"))
                 {
                     orgXml.PreserveWhitespace = true;
                     orgXml.LoadXml(srBuffer.ReadToEnd());
                 }
                 OrganizationDocument org = new OrganizationDocument();
                 OrganizationExtractor.Extract(orgXml, org, xPathManager);
                 
                 using (OrganizationQuery oQuery = new OrganizationQuery())
                 {
                     oQuery.SaveDocument(org, "ychen");
                 }
             }
             catch (NUnit.Framework.AssertionException assertEx)
             {
                 Console.WriteLine("Assertion failed: " + assertEx.Message);
                 throw assertEx;
             }
             catch (Exception e)
             {
                 throw new Exception("Testing Error: Saving data from organization document failed.", e);
             }
        }


        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void OrgSave27271()
        {
            // Set up
            try
            {
                XmlDocument orgXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Organization27271.xml"))
                {
                    orgXml.PreserveWhitespace = true;
                    orgXml.LoadXml(srBuffer.ReadToEnd());
                }
                OrganizationDocument org = new OrganizationDocument();
                OrganizationExtractor.Extract(orgXml, org, xPathManager);

                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.SaveDocument(org, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from organization document failed.", e);
            }
        }

                /// <summary>
        /// Unit test for multiple Short Names .
        /// </summary>
        [Test]
        public void OrgSaveMultipleShortNames34983()
        {
            // Set up
            try
            {
                XmlDocument orgXml = new XmlDocument();
                using (StreamReader srBuffer = new StreamReader(@"./XMLData/Organization34983.xml"))
                {
                    orgXml.PreserveWhitespace = true;
                    orgXml.LoadXml(srBuffer.ReadToEnd());
                }
                OrganizationDocument org = new OrganizationDocument();
                OrganizationExtractor.Extract(orgXml, org, xPathManager);

                using (OrganizationQuery oQuery = new OrganizationQuery())
                {
                    oQuery.SaveDocument(org, "ychen");
                }
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from organization document failed.", e);
            }
        }


        #endregion
    }
}
