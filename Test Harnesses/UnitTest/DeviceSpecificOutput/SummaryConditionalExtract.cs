using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using GateKeeper.Common;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.BusinessObjects;

namespace GateKeeper.UnitTest.DeviceSpecificOutput
{
    [TestFixture]
    public class SummaryConditionalExtract
    {
        #region Reusable pieces
        DocumentXPathManager _xPathManager;
        HistoryEntryWriter _informationWriter = delegate(string message) { Console.Write(message); };
        HistoryEntryWriter _warningWriter = delegate(string message) { Console.Write(message); };

        [TestFixtureSetUp()]
        public void Init()
        {
            _xPathManager =  new DocumentXPathManager();
        }

        #endregion

        /// <summary>
        /// Check for the presence of sections.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="device"></param>
        /// <param name="rawSectionIDs"></param>
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, new string[] { "_134", "_472", "_GetMore_3", "_211", "_AboutPDQ_14" })]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, new string[] { "_1", "_28", "_134", "_472" })]
        public void ExpectedSections(string filename, TargetedDevice device, string[] rawSectionIDs)
        {
            SummaryDocument document = RunExtract(filename, device);

            string msg = "Couldn't find section {0}.";
            Array.ForEach(rawSectionIDs, sectionID =>
            {
                Assert.IsNotNull(document.TopLevelSectionList.Find(section => section.RawSectionID == sectionID), string.Format(msg, sectionID));
            });
        }

        /// <summary>
        /// Check for the absence of sections.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="device">The device.</param>
        /// <param name="rawSectionIDs">The raw section I ds.</param>
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_1")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_28")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_102")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_102")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_GetMore_3")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_211")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_AboutPDQ_14")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_257" )]
        public void UnexpectedSections(string filename, TargetedDevice device, string rawSectionID)
        {
            SummaryDocument document = RunExtract(filename, device);

            string msg = "Unexpectedly found section {0}.";
            Assert.IsNull(document.SectionList.Find(section => section.RawSectionID == rawSectionID), string.Format(msg, rawSectionID));
        }

        /// <summary>
        /// Test for conditional inclusion of tables.
        /// </summary>
        /// <param name="filename">Data file.</param>
        /// <param name="device">The targeted device.</param>
        /// <param name="topSection">The top section containing the tables.</param>
        /// <param name="expectedTables">The expected tables.</param>
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_1_Table_No_attributes")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_2_Table_Include_Screen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_4_Table_Include_ScreenMobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_6_Table_Exclude_Mobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_10_Table_ExcludeNothing")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_1_Table_No_attributes")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_3_Table_Include_Mobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_4_Table_Include_ScreenMobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_5_Table_Exclude_Screen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_10_Table_ExcludeNothing")]
        public void ExpectedTopLevelTables(string filename, TargetedDevice device, string topSection, string expectedTable)
        {
            SummaryDocument document = RunExtract(filename, device);
            string msg = "Couldn't find table {0}.";

            Assert.IsNotNull(document.TableSectionList.Find(table => table.RawSectionID == expectedTable), string.Format(msg, expectedTable));
        }

        /// <summary>
        /// Test for the presence of tables which should be absent.
        /// </summary>
        /// <param name="filename">Data file.</param>
        /// <param name="device">The targeted device.</param>
        /// <param name="topSection">The top section containing the tables.</param>
        /// <param name="expectedTables">The expected tables.</param>
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_3_Table_Include_Mobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_5_Table_Exclude_Screen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_7_Table_Exclude_ScreenMobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_8_Table_Empty_Attributes")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_472", "_9_Table_IncludeNothing")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_2_Table_Include_Screen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_6_Table_Exclude_Mobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_7_Table_Exclude_ScreenMobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_8_Table_Empty_Attributes")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_472", "_9_Table_IncludeNothing")]
        public void UnexpectedTopLevelTables(string filename, TargetedDevice device, string topSection, string expectedTable)
        {
            SummaryDocument document = RunExtract(filename, device);
            string msg = "Unexpectedly found table {0}";
            Assert.IsNull(document.TableSectionList.Find(table => table.RawSectionID == expectedTable), string.Format(msg, expectedTable));
        }

        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_1_MediaLink_No_Attributes")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_2_MediaLink_Include_Screen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_4_MediaLink_Include_MobileScreen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_7_MediaLink_Exclude_Mobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_9_MediaLink_Exclude_Nothing")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_1_MediaLink_No_Attributes")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_3_MediaLink_Include_Mobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_4_MediaLink_Include_MobileScreen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_6_MediaLink_Exclude_Screen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_9_MediaLink_Exclude_Nothing")]
        public void ExpectedMediaLinks(string filename, TargetedDevice device, string expectedMediaLink)
        {
            SummaryDocument document = RunExtract(filename, device);
            string msg = "Couldn't find media link {0}.";
            Assert.IsNotNull(document.Level5SectionList.Find(mediaLinkSection => mediaLinkSection.RawSectionID == expectedMediaLink), string.Format(msg, expectedMediaLink));
        }


        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_3_MediaLink_Include_Mobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_5_MediaLink_Include_Nothing")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_6_MediaLink_Exclude_Screen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_8_MediaLink_Exclude_MobileScreen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen, "_10_MediaLink_EmptyAttributes")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_2_MediaLink_Include_Screen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_5_MediaLink_Include_Nothing")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_7_MediaLink_Exclude_Mobile")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_8_MediaLink_Exclude_MobileScreen")]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile, "_10_MediaLink_EmptyAttributes")]
        public void UnexpectedMediaLinks(string filename, TargetedDevice device, string expectedMediaLink)
        {
            SummaryDocument document = RunExtract(filename, device);
            string msg = "Unexpectedly found media link {0}.";
            Assert.IsNull(document.Level5SectionList.Find(mediaLinkSection => mediaLinkSection.RawSectionID == expectedMediaLink), string.Format(msg, expectedMediaLink));
        }


        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.screen)]
        [TestCase("Summary-Conditional-Extract.xml", TargetedDevice.mobile)]
        public void Render(string filename, TargetedDevice device)
        {
            Assert.DoesNotThrow(delegate { SummaryDocument summary = RunExtract(filename, device); });
        }

        private SummaryDocument RunExtract(string filename, TargetedDevice target)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(@"./XMLData/" + filename);

            SummaryDocument document = new SummaryDocument();

            document.InformationWriter = _informationWriter;
            document.WarningWriter = _warningWriter;
            SummaryExtractor extractor = new SummaryExtractor();

            extractor.Extract(xml, document, _xPathManager, target);

            return document;
        }


    }
}
