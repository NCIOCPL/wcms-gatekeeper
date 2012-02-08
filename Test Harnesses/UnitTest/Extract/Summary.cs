using System;
using System.Xml;

using NUnit.Framework;

using GateKeeper.Common;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects.Summary;

namespace GateKeeper.UnitTest.Extract
{
    [TestFixture]
    public class SummaryExtract
    {
        #region Reusable pieces

        DocumentXPathManager _xPathManager;
        HistoryEntryWriter _historyWriter = delegate(string message) { Console.Write(message); };

        [TestFixtureSetUp()]
        public void Init()
        {
            _xPathManager = new DocumentXPathManager();
        }

        #endregion

        [TestCase("Summary-BreastPatient-NoMobile-62955.xml")]
        [TestCase("Summary-BreastPatient-Mobile-62955.xml")]
        public void DefaultDevice(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.ValidOutputDevices.Contains(TargetedDevice.screen), string.Format("ValidOutputDevices does not contain {0}.", TargetedDevice.screen));
        }

        [TestCase("Summary-BreastPatient-NoMobile-62955.xml")]
        public void DefaultDeviceOnly(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.ValidOutputDevices.Contains(TargetedDevice.screen), string.Format("ValidOutputDevices does not contain {0}.", TargetedDevice.screen));
            Assert.IsFalse(summaryDoc.ValidOutputDevices.Contains(TargetedDevice.mobile), string.Format("ValidOutputDevices unexpectedly contains {0}.", TargetedDevice.mobile));
        }

        [TestCase("Summary-BreastPatient-Mobile-62955.xml")]
        public void MobileDevice(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.ValidOutputDevices.Contains(TargetedDevice.screen), string.Format("ValidOutputDevices does not contain {0}.", TargetedDevice.screen));
            Assert.IsTrue(summaryDoc.ValidOutputDevices.Contains(TargetedDevice.mobile), string.Format("ValidOutputDevices does not contain {0}.", TargetedDevice.mobile));
        }

        private void RunExtract(string filename, SummaryDocument summaryDoc)
        {
            XmlDocument document = new XmlDocument();
            document.Load(@"./XMLData/" + filename);

            summaryDoc.InformationWriter = _historyWriter;
            summaryDoc.WarningWriter = _historyWriter;

            SummaryExtractor extractor = new SummaryExtractor();
            extractor.Extract(document, summaryDoc, _xPathManager, TargetedDevice.screen);
        }
    }
}
