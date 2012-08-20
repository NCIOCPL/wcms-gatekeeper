using System;
using System.Xml;

using NUnit.Framework;

using GateKeeper.Common;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects.Summary;

namespace GateKeeper.UnitTest.Extract
{
    class PermanentLink
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


        [TestCase("PermanentLink-None.xml")]
        public void NoReferenceToPermaTargs(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.PermanentLinkList.Count == 0);
        }

        [TestCase("PermanentLink-One.xml")]
        public void OnePermaTarg(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.PermanentLinkList.Count == 1);
            Assert.IsTrue(summaryDoc.PermanentLinkList[0].ID.Equals("0"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[0].Title.Equals("Only"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[0].SectionID.Equals("_1"));
        }

        [TestCase("PermanentLink-Three.xml")]
        public void ThreePermaTargs(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.PermanentLinkList.Count == 3);
            Assert.IsTrue(summaryDoc.PermanentLinkList[0].ID.Equals("1"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[1].ID.Equals("2"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[2].ID.Equals("3"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[0].Title.Equals("First"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[0].SectionID.Equals("_1"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[1].SectionID.Equals("_2"));
        }

        [TestCase("PermanentLink-EmptyLink.xml")]
        public void EmptyPermaTarg(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.PermanentLinkList.Count == 0);
        }

        [TestCase("PermanentLink-EmptyList.xml")]
        public void EmptyListOfPermaTargs(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.PermanentLinkList.Count == 0);
        }

        [TestCase("PermanentLink-Floater.xml")]
        public void FloatingPermaTargsShouldBeIgnored(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.PermanentLinkList.Count == 1);
        }

        [TestCase("PermanentLink-WithOtherTags.xml")]
        public void OtherTagsInEncasingList(string filename)
        {
            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);
            Assert.IsTrue(summaryDoc.PermanentLinkList.Count == 1);
        }

        [TestCase("PermanentLink-BadSectionID.xml")]
        public void ThrowsExceptionIfSectionLinkingToDoesntExist(string filename)
        {
            bool caughtException = false;
            try
            {
                SummaryDocument summaryDoc = new SummaryDocument();
                RunExtract(filename, summaryDoc);
            }
            catch (Exception ex)
            {
                caughtException = true;
            }
            Assert.IsTrue(caughtException);
        }

        /*
         * This test is actually being removed due to a removed functionality.
         * Please keep until 6.5 DTD is released.
         * 
        [TestCase("PermanentLink-Prefix.xml")]
        public void MakeSureSectionIDIsCorrect(string filename)
        {
            // Tests to make sure the prefix _ gets removed
            // But that that is the only _ removed

            SummaryDocument summaryDoc = new SummaryDocument();
            RunExtract(filename, summaryDoc);

            Assert.IsTrue(summaryDoc.PermanentLinkList[0].SectionID.Equals("1"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[1].SectionID.Equals("2"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[2].SectionID.Equals("AboutPDQ_2"));
            Assert.IsTrue(summaryDoc.PermanentLinkList[3].SectionID.Equals("AboutPDQ_6"));
        }
       */

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
