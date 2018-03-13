using System;
using System.Xml;

using NUnit.Framework;

using GateKeeper.DocumentObjects.Summary;
using GKManagers.Preprocessors;
using GateKeeper.DocumentObjects;

namespace GateKeeper.UnitTest.DocumentObjects.Summary
{
    /// <summary>
    /// Tests for the SummaryPreprocessor ValidateSummaryRefs() method.
    /// </summary>
    [TestFixture]
    class SummaryPreprocessorValidateSummaryRefs
    {
        /// <summary>
        /// Test where all the sections identified in the linked-sections list are
        /// the actual top-level sections identified as general information pages.
        /// </summary>
        [Test]
        public void IdentifiesTopLevelGISection()
        {
            SplitData data = new SplitData()
            {
                CdrId = 1,
                Url = "n/a",
                PageSections = new string[] { "_1", "_2", "_3", "_AboutThis_1" },
                GeneralSections = new string[] { "_1", "_2"},
                LinkedSections = new string[] { "_1", "_2"},
                LongTitle = "n/a",
                ShortTitle = "n/a",
                LongDescription = "n/a",
                MetaKeywords = "n/a"
            };

            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
        <Summary id=""CDR000000001"""" LegacyPDQID=""""1278"""">
            <SummarySection id=""_1"">
                <Para id = ""_100"" >Sub-section 1.100 </Para>
                <Para id = ""_200"" >Sub-section 1.200 </Para>
            </SummarySection>
            <SummarySection id=""_2"">
                <Para id = ""_201"" >Sub-section 2.201 </Para>
                <Para id = ""_202"" >Sub-section 2.202 </Para>
                <Para id = ""_203"" >Sub-section 2.203 </Para>
            </SummarySection>
            <SummarySection id=""_3"">
                <Para id = ""_301"" >Sub-section 3.301 </Para>
                <Para id = ""_302"" >Sub-section 3.302 </Para>
                <Para id = ""_303"" >Sub-section 3.303 </Para>
            </SummarySection>
            <SummarySection id=""_AboutThis_1"">
                <Title>About This PDQ Summary</Title>
            </SummarySection>
        </Summary>
");

            Assert.DoesNotThrow(() => {
                SummaryPreprocessor processor = new SummaryPreprocessor();
                processor.ValidateSummaryRefs(summary, data);
            });
            
        }

        /// <summary>
        /// Verify validation failure when a section in the linked-sections list is
        /// a top-level section, but is NOT identified as general information pages.
        /// </summary>
        [Test]
        public void MisidentifiesTopLevelGISection()
        {
            SplitData data = new SplitData()
            {
                CdrId = 1,
                Url = "n/a",
                PageSections = new string[] { "_1", "_2", "_3", "_AboutThis_1" },
                GeneralSections = new string[] { "_1", "_2" },
                LinkedSections = new string[] { "_3" },
                LongTitle = "n/a",
                ShortTitle = "n/a",
                LongDescription = "n/a",
                MetaKeywords = "n/a"
            };

            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
        <Summary id=""CDR000000001"""" LegacyPDQID=""""1278"""">
            <SummarySection id=""_1"">
                <Para id = ""_100"" >Sub-section 1.100 </Para>
                <Para id = ""_200"" >Sub-section 1.200 </Para>
            </SummarySection>
            <SummarySection id=""_2"">
                <Para id = ""_201"" >Sub-section 2.201 </Para>
                <Para id = ""_202"" >Sub-section 2.202 </Para>
                <Para id = ""_203"" >Sub-section 2.203 </Para>
            </SummarySection>
            <SummarySection id=""_3"">
                <Para id = ""_301"" >Sub-section 3.301 </Para>
                <Para id = ""_302"" >Sub-section 3.302 </Para>
                <Para id = ""_303"" >Sub-section 3.303 </Para>
            </SummarySection>
            <SummarySection id=""_AboutThis_1"">
                <Title>About This PDQ Summary</Title>
            </SummarySection>
        </Summary>
");

            Assert.Throws<ValidationException>(() => {
                SummaryPreprocessor processor = new SummaryPreprocessor();
                processor.ValidateSummaryRefs(summary, data);
            });

        }

        /// <summary>
        /// Test where all the sections identified in the linked-sections list are
        /// internal to a top-level sections identified as general information pages.
        /// </summary>
        [Test]
        public void IdentifiesInternalGISection()
        {
            SplitData data = new SplitData()
            {
                CdrId = 1,
                Url = "n/a",
                PageSections = new string[] { "_1", "_2", "_3", "_AboutThis_1" },
                GeneralSections = new string[] { "_1", "_2" },
                LinkedSections = new string[] { "_100", "_202", "_203" },
                LongTitle = "n/a",
                ShortTitle = "n/a",
                LongDescription = "n/a",
                MetaKeywords = "n/a"
            };

            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
        <Summary id=""CDR000000001"""" LegacyPDQID=""""1278"""">
            <SummarySection id=""_1"">
                <Para id = ""_100"" >Sub-section 1.100 </Para>
                <Para id = ""_200"" >Sub-section 1.200 </Para>
            </SummarySection>
            <SummarySection id=""_2"">
                <Para id = ""_201"" >Sub-section 2.201 </Para>
                <Para id = ""_202"" >Sub-section 2.202 </Para>
                <Para id = ""_203"" >Sub-section 2.203 </Para>
            </SummarySection>
            <SummarySection id=""_3"">
                <Para id = ""_301"" >Sub-section 3.301 </Para>
                <Para id = ""_302"" >Sub-section 3.302 </Para>
                <Para id = ""_303"" >Sub-section 3.303 </Para>
            </SummarySection>
            <SummarySection id=""_AboutThis_1"">
                <Title>About This PDQ Summary</Title>
            </SummarySection>
        </Summary>
");

            Assert.DoesNotThrow(() => {
                SummaryPreprocessor processor = new SummaryPreprocessor();
                processor.ValidateSummaryRefs(summary, data);
            });

        }

        /// <summary>
        /// Verify validation failure when a section in the linked-sections list is
        /// an internal section, but is NOT internal to a section identified as a general information page.
        /// </summary>
        [Test]
        public void MisidentifiesInternalGISection()
        {
            SplitData data = new SplitData()
            {
                CdrId = 1,
                Url = "n/a",
                PageSections = new string[] { "_1", "_2", "_3", "_AboutThis_1" },
                GeneralSections = new string[] { "_1", "_2" },
                LinkedSections = new string[] { "_302" },
                LongTitle = "n/a",
                ShortTitle = "n/a",
                LongDescription = "n/a",
                MetaKeywords = "n/a"
            };

            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
        <Summary id=""CDR000000001"""" LegacyPDQID=""""1278"""">
            <SummarySection id=""_1"">
                <Para id = ""_100"" >Sub-section 1.100 </Para>
                <Para id = ""_200"" >Sub-section 1.200 </Para>
            </SummarySection>
            <SummarySection id=""_2"">
                <Para id = ""_201"" >Sub-section 2.201 </Para>
                <Para id = ""_202"" >Sub-section 2.202 </Para>
                <Para id = ""_203"" >Sub-section 2.203 </Para>
            </SummarySection>
            <SummarySection id=""_3"">
                <Para id = ""_301"" >Sub-section 3.301 </Para>
                <Para id = ""_302"" >Sub-section 3.302 </Para>
                <Para id = ""_303"" >Sub-section 3.303 </Para>
            </SummarySection>
            <SummarySection id=""_AboutThis_1"">
                <Title>About This PDQ Summary</Title>
            </SummarySection>
        </Summary>
");

            Assert.Throws<ValidationException>(() => {
                SummaryPreprocessor processor = new SummaryPreprocessor();
                processor.ValidateSummaryRefs(summary, data);
            });

        }

    }
}
