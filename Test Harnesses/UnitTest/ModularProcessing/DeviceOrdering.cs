using System;
using System.IO;
using System.Xml.Serialization;

using NUnit.Framework;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GKManagers.Processors;

namespace GateKeeper.UnitTest.ModularProcessing
{
    /// <summary>
    /// Test the assumption that the TargetedDevice enum will always put screen before mobile
    /// and that Processor Pool will always put screen first, regardless of order in the deserialized
    /// configuration.
    /// </summary>
    [TestFixture]
    public class DeviceOrdering
    {
        [TestCase(TargetedDevice.mobile)]
        [TestCase(TargetedDevice.ebook)]
        public void EnumOrdering(TargetedDevice device)
        {
            Assert.IsTrue(TargetedDevice.screen < device);
        }

        [Test]
        public void DeserializedOrder()
        {
            // Deliberately put screen at the end of the list of processors.
            #region declare string PoolXML....
            string PoolXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <ProcessorPool xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                  <Processors>

                    <Processor>
                      <Type>Summary</Type>
                      <ProcessingTargets>

                        <ProcessingTarget TargetedDevice=""mobile"">
                          <DocumentExtractor xsi:type=""SummaryExtractor"" />
                          <DocumentRenderer xsi:type=""SummaryRenderer"" />
                          <DocumentDataAccess xsi:type=""SummaryMobileDataAccessWrapper"" />
                        </ProcessingTarget>

                        <ProcessingTarget TargetedDevice=""ebook"">
                          <DocumentExtractor xsi:type=""SummaryExtractor"" />
                          <DocumentRenderer xsi:type=""SummaryRenderer"" />
                          <DocumentDataAccess xsi:type=""SummaryStandardDataAccessWrapper"" />
                        </ProcessingTarget>

                        <ProcessingTarget TargetedDevice=""screen"">
                          <DocumentExtractor xsi:type=""SummaryExtractor"" />
                          <DocumentRenderer xsi:type=""SummaryRenderer"" />
                          <DocumentDataAccess xsi:type=""SummaryStandardDataAccessWrapper"" SitePath=""//Sites/MobileCancerGov"" />
                        </ProcessingTarget>

                      </ProcessingTargets>
                    </Processor>

                  </Processors>
                </ProcessorPool>";
            #endregion

            // Deserialize PoolXML
            ProcessorPool pool;
            XmlSerializer serializer = new XmlSerializer(typeof(ProcessorPool));
            using (TextReader reader = new StringReader(PoolXML))
            {
                pool = (ProcessorPool)serializer.Deserialize(reader);
            }

            // Verify screen is the first target returned.
            ProcessingTarget[] targets = pool.GetProcessingTargets(DocumentType.Summary);
            Assert.IsTrue(targets[0].TargetedDevice == TargetedDevice.screen);
        }
    }
}
