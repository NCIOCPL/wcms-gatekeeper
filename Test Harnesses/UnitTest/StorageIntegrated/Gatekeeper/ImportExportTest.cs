using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using NUnit.Framework;

using GateKeeper.Common;
using GKManagers.BusinessObjects;
using GKManagers;


namespace GateKeeper.UnitTest.StorageIntegrated.Gatekeeper
{

    [TestFixture, Explicit]
    public class ImportExportTest
    {
        const string TEST_USER_ID = "Test User";
        const string REQUEST_SOURCE = "NUNIT";
        XmlDocument _requestDocXML1 = new XmlDocument();
        
        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void InsertRequestData()
        {
             bool result= false;
            string externalID ="128500947";
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing ImportExportTest.InsertRequestData() for Request 104458 ...");

                using (StreamReader srBuffer = new StreamReader(@"./XMLData/ImportExportTest.xml"))
                {
                    this._requestDocXML1.PreserveWhitespace = true;
                    this._requestDocXML1.LoadXml(srBuffer.ReadToEnd());
                }

                try
                {
                    RequestData data1 = RequestDataFactory.Create(CDRDocumentType.Summary,
                    2, RequestDataActionType.Export, 156762, "1.0", RequestDataLocationType.GateKeeper, 411, 
                   _requestDocXML1.OuterXml);
                        //Set data property
                    data1.Status = RequestDataStatusType.OK;
                    data1.DependencyStatus = RequestDataDependentStatusType.OK;
                    data1.Location = RequestDataLocationType.GateKeeper;

                    result = RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref data1);
                    
                    Assert.IsTrue(result);
                    if (!result)
                    {
                        RequestManager.AbortRequest(externalID, REQUEST_SOURCE, TEST_USER_ID,
                            "Import aborted due to problem in inserting request data.");
                        Assert.IsFalse(result, "InsertRequestData failed for data 1.");
                    }

                    RequestData data = RequestDataFactory.Create(CDRDocumentType.Summary,
                    2, RequestDataActionType.Export, 156762, "1.0", RequestDataLocationType.GateKeeper, 411, 
                    string.Empty);
                        //Set data property
                    data.Status = RequestDataStatusType.OK;
                    data.DependencyStatus = RequestDataDependentStatusType.OK;
                    data.Location = RequestDataLocationType.GateKeeper;

                    result = RequestManager.InsertRequestData(externalID, REQUEST_SOURCE, TEST_USER_ID, ref data);
                    Console.WriteLine(result.ToString());
                    Assert.IsTrue(result);
                    
                    if (!result)
                    {
                        RequestManager.AbortRequest(externalID, REQUEST_SOURCE, TEST_USER_ID,
                            "Import aborted due to problem in inserting request data.");
                        Assert.IsFalse(result, "InsertRequestData failed for data 2.");
                    }
                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    RequestManager.AbortRequest(externalID, REQUEST_SOURCE, TEST_USER_ID,
                                "Import aborted due to problem in inserting request data. Details: " + ex.Message);
                    Assert.IsFalse(result, "InsertRequestData failed.");
                }

          
                Console.WriteLine("========================================");
            }
            catch (AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed: Encountered {0} exception. Details: {1}", ex.Message, ex.ToString());
                Assert.Fail("Test failed: Encountered {0} exception.", ex.Message);
            }
        }

    }
}
