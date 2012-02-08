using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using GateKeeper.Common;
using GateKeeper.DataAccess.CDR;
using GateKeeper.ContentRendering;
using GateKeeper.DataAccess.DataAccessWrappers;
using GKManagers.Processors;

namespace GateKeeper.UnitTest.ModularProcessing
{
    [TestFixture]
    public class SummaryProcessingTargets
    {
        #region XML Structures

        const string SummaryTargetWithoutDevice =
@"<ProcessingTarget  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	<DocumentExtractor xsi:type=""SummaryExtractor"" />
    <DocumentRenderer xsi:type=""SummaryRenderer"" />
    <DocumentDataAccess xsi:type=""SummaryStandardDataAccessWrapper"" />
</ProcessingTarget>";

        string SummaryTargetWithAttribute =
@"<ProcessingTarget {0}  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	<DocumentExtractor xsi:type=""SummaryExtractor"" />
    <DocumentRenderer xsi:type=""SummaryRenderer"" />
    <DocumentDataAccess xsi:type=""SummaryStandardDataAccessWrapper"" />
</ProcessingTarget>";

        #endregion

        [Test]
        public void DefaultDevice()
        {
            ProcessingTarget target = DeserializeTarget(SummaryTargetWithoutDevice);
            Assert.AreEqual(TargetedDevice.screen, target.TargetedDevice);
        }

        [TestCase(TargetedDevice.ebook)]
        [TestCase(TargetedDevice.screen)]
        [TestCase(TargetedDevice.mobile)]
        public void SpecificDevice(TargetedDevice expectedDevice)
        {
            string attribute = string.Format(@"TargetedDevice=""{0}""", expectedDevice);

            string xml = string.Format(SummaryTargetWithAttribute, attribute);
            ProcessingTarget target = DeserializeTarget(xml);
            Assert.AreEqual(expectedDevice, target.TargetedDevice);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestCase("18")]
        [TestCase("chicken")]
        public void InvalidDevice(string device)
        {
            string attribute = string.Format(@"TargetedDevice=""{0}""", device);

            string xml = string.Format(SummaryTargetWithAttribute, attribute);
            ProcessingTarget target = DeserializeTarget(xml);
        }

        [Test]
        public void SummaryComponents()
        {
            ProcessingTarget target = DeserializeTarget(SummaryTargetWithoutDevice);

            Assert.AreEqual(typeof(SummaryExtractor), target.DocumentExtractor.GetType());
            Assert.AreEqual(typeof(SummaryRenderer), target.DocumentRenderer.GetType());
            Assert.AreEqual(typeof(SummaryStandardDataAccessWrapper), target.DocumentDataAccess.GetType());
        }

        private ProcessingTarget DeserializeTarget(string xml)
        {
            ProcessingTarget target;

            XmlSerializer serializer = new XmlSerializer(typeof(ProcessingTarget));
            using (TextReader reader = new StringReader(xml))
            {
                target = (ProcessingTarget)serializer.Deserialize(reader);
            }

            return target;
        }
    }
}
