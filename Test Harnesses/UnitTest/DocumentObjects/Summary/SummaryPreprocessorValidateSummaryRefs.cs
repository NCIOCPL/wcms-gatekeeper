using System;
using System.Xml;

using NUnit.Framework;

using GateKeeper.DocumentObjects.Summary;
using GKManagers.Preprocessors;
using GateKeeper.DocumentObjects;

namespace GateKeeper.UnitTest.DocumentObjects.Summary
{
    /// <summary>
    /// Tests for the SummaryPreprocessor ValidateOutgoingSummaryRefs() method.
    /// </summary>
    [TestFixture]
    class SummaryPreprocessorValidateSummaryRefs
    {
        /*
            These tests assume the following (very simplified) summary is participating in the pilot.

            Page sections (top sections) are: _1, _2, _3, _AboutThis_1
            Sections in the pilot are: _1, _2
            Sections which are *reported* to be linked from elsewhere are: _1, _2, _100, _201, _202, _203

            The tests will use summaries with varying SummaryRef elements.

        <Summary id="CDR000000001" LegacyPDQID="1278">
            <SummarySection id="_1">
                <Para id = "_100" >Sub-section 1.100 </Para>
                <Para id = "_200" >Sub-section 1.200 </Para>
            </SummarySection>
            <SummarySection id="_2">
                <Para id = "_201" >Sub-section 2.201 </Para>
                <Para id = "_202" >Sub-section 2.202 </Para>
                <Para id = "_203" >Sub-section 2.203 </Para>
            </SummarySection>
            <SummarySection id="_3">
                <Para id = "_301" >Sub-section 3.301 </Para>
                <Para id = "_302" >Sub-section 3.302 </Para>
                <Para id = "_303" >Sub-section 3.303 </Para>
            </SummarySection>
            <SummarySection id="_AboutThis_1">
                <Title>About This PDQ Summary</Title>
            </SummarySection>
        </Summary>

          */

        MockSplitDataManager splitData;

        [TestFixtureSetUp]
        public void Setup()
        {
            splitData = new MockSplitDataManager()
            {
                MockSplitData = new SplitData()
                {
                    CdrId = 1,
                    Url = "n/a",
                    PageSections = new string[] {"_1", "_2", "_3", "_AboutThis_1"},
                    GeneralSections = new string[] {"_1", "_2"},
                    LinkedSections = new string[] {"_1", "_2", "_100", "_201", "_202", "_203"},
                    LongTitle = "n/a",
                    ShortTitle = "n/a",
                    LongDescription = "n/a",
                    MetaKeywords = "n/a"
                }
            };
        }

        /// <summary>
        /// Check that summary is successfully validated when all SummaryRef sections appear in the linked section list.
        /// </summary>
        [Test]
        public void SummaryRefsAreInLinkedCollection()
        {
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000002"" LegacyPDQID=""1279"">
    <SummarySection id=""_1"">
        <Para id = ""_100"">Paragraph 1.1 <SummaryRef href=""CDR0000000001#_1"" url=""/whatever/1"">SummaryRef 1</SummaryRef></Para>
        <Para id = ""_200"">Paragraph 1.2 <SummaryRef href=""CDR0000000001#_2"" url=""/whatever/2"">SummaryRef 2</SummaryRef></Para>
    </SummarySection>
    <SummarySection id=""_2"">
        <Para id = ""_300"">Paragraph 1.3 <SummaryRef href=""CDR0000000001#_202"" url=""/whatever/3"">SummaryRef 3</SummaryRef></Para>
    </SummarySection>
    <SummarySection id=""_AboutThis_1"">
        <Title>About This PDQ Summary</Title>
        <Para id = ""_400"">Paragraph 1.4 <SummaryRef href=""CDR0000000001#_203"" url=""/whatever/4"">SummaryRef 4</SummaryRef></Para>
    </SummarySection>
</summary >
");
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.DoesNotThrow(() => { processor.ValidateOutgoingSummaryRefs(summary, splitData); });
        }

        /// <summary>
        /// Check that summary does not validate when a SummaryRef is not in the section list.
        /// </summary>
        [Test]
        public void SummaryRefsAreMissingFromLinkedCollection()
        {
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000002"" LegacyPDQID=""1279"">
    <SummarySection id=""_1"">
        <Para id = ""_100"">Paragraph 1.1 <SummaryRef href=""CDR0000000001#_1"" url=""/whatever/1"">ValidRef 1</SummaryRef></Para>
        <Para id = ""_200"">Paragraph 1.2 <SummaryRef href=""CDR0000000001#_2"" url=""/whatever/2"">ValidRef 2</SummaryRef></Para>
    </SummarySection>
    <SummarySection id=""_2"">
        <Para id = ""_300"">Paragraph 1.3 <SummaryRef href=""CDR0000000001#_7"" url=""/whatever/3"">INVALID REF!!!!!</SummaryRef></Para>
    </SummarySection>
    <SummarySection id=""_AboutThis_1"">
        <Title>About This PDQ Summary</Title>
        <Para id = ""_400"">Paragraph 1.4 <SummaryRef href=""CDR0000000001#_203"" url=""/whatever/4"">SummaryRef 4</SummaryRef></Para>
    </SummarySection>
</summary >
");
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.Throws<ValidationException>(() => { processor.ValidateOutgoingSummaryRefs(summary, splitData); });
        }

        /// <summary>
        /// Check that summary is successfully validated when the SummaryRef doesn't include a section.
        /// (Reference to the pilot summary itself.)
        /// </summary>
        [Test]
        public void SummaryRefToSummaryOnly()
        {
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000002"" LegacyPDQID=""1279"">
    <SummarySection id=""_1"">
        <Para id = ""_100"">Paragraph 1.1 <SummaryRef href=""CDR0000000001"" url=""/whatever/1"">SummaryRef 1</SummaryRef></Para>
        <Para id = ""_200"">Paragraph 1.2 <SummaryRef href=""CDR0000000001"" url=""/whatever/2"">SummaryRef 2</SummaryRef></Para>
    </SummarySection>
    <SummarySection id=""_2"">
        <Para id = ""_300"">Paragraph 1.3 <SummaryRef href=""CDR0000000001"" url=""/whatever/3"">SummaryRef 3</SummaryRef></Para>
    </SummarySection>
    <SummarySection id=""_AboutThis_1"">
        <Title>About This PDQ Summary</Title>
        <Para id = ""_400"">Paragraph 1.4 <SummaryRef href=""CDR0000000001"" url=""/whatever/4"">SummaryRef 4</SummaryRef></Para>
    </SummarySection>
</summary >
");
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.DoesNotThrow(() => { processor.ValidateOutgoingSummaryRefs(summary, splitData); });
        }

        /// <summary>
        /// Check that summary is successfully validated when it has no references to piloted summaries.
        /// </summary>
        [Test]
        public void SummaryRefToNonpilotSummaries()
        {
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000002"" LegacyPDQID=""1279"">
    <SummarySection id=""_1"">
        <Para id = ""_100"">Paragraph 1.1 <SummaryRef href=""CDR0000000010#_1"" url=""/whatever/1"">SummaryRef 1</SummaryRef></Para>
        <Para id = ""_200"">Paragraph 1.2 <SummaryRef href=""CDR0000000010#_2"" url=""/whatever/2"">SummaryRef 2</SummaryRef></Para>
    </SummarySection>
    <SummarySection id=""_2"">
        <Para id = ""_300"">Paragraph 1.3 <SummaryRef href=""CDR0000000010#_202"" url=""/whatever/3"">SummaryRef 3</SummaryRef></Para>
    </SummarySection>
    <SummarySection id=""_AboutThis_1"">
        <Title>About This PDQ Summary</Title>
        <Para id = ""_400"">Paragraph 1.4 <SummaryRef href=""CDR0000000010#_203"" url=""/whatever/4"">SummaryRef 4</SummaryRef></Para>
    </SummarySection>
</summary >
");
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.DoesNotThrow(() => { processor.ValidateOutgoingSummaryRefs(summary, splitData); });
        }

        /// <summary>
        /// Check that summary is successfully validated when it has no summary refs.
        /// </summary>
        [Test]
        public void SummaryWithoutSummaryRefs()
        {
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000002"" LegacyPDQID=""1279"">
    <SummarySection id=""_1"">
        <Para id = ""_100"">Paragraph 1.1</Para>
        <Para id = ""_200"">Paragraph 1.2</Para>
    </SummarySection>
    <SummarySection id=""_2"">
        <Para id = ""_300"">Paragraph 1.3</Para>
    </SummarySection>
    <SummarySection id=""_AboutThis_1"">
        <Title>About This PDQ Summary</Title>
        <Para id = ""_400"">Paragraph 1.4 </Para>
    </SummarySection>
</summary >
");
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.DoesNotThrow(() => { processor.ValidateOutgoingSummaryRefs(summary, splitData); });
        }

    }
}
