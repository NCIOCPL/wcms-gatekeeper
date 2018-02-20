using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using NUnit.Framework;

using GateKeeper.DocumentObjects.Summary;
using GKManagers.Preprocessors;

namespace GateKeeper.UnitTest.DocumentObjects.Summary
{
    /// <summary>
    /// Tests for the Summary Preprocessor's RewriteSummaryRefAttributes() method.
    /// </summary>
    [TestFixture]
    class SummaryPreprocessorRewriteSummaryRefAttributes
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

        ISplitDataManager SplitData;

        /// <summary>
        /// Set up and initialize a SplitDataManager to match the assumed pilot summary.
        /// </summary>
        [TestFixtureSetUp]
        public void Setup()
        {
            SplitData = SplitDataManager.CreateFromString(@"
[
	{
        ""cdrid"": ""1"",
        ""url"": ""/general-information-page"",
        ""page-sections"": [""_1"", ""_2"", ""_3"", ""_AboutThis_1""],
		""general-sections"": [""_1"", ""_2""],
		""linked-sections"": [""_1"", ""_2"", ""_100"", ""_201"", ""_202"", ""_203""],
        ""long-title"": ""n/a"",
		""short-title"": ""n/a"",
		""long-description"": ""n/a"",
		""meta-keywords"": ""n/a""
	},
	{
        ""cdrid"": ""2"",
        ""url"": ""/general-information-page-2"",
        ""page-sections"": [""_1"", ""_2"", ""_3"", ""_AboutThis_1""],
		""general-sections"": [""_1"", ""_2""],
		""linked-sections"": [""_1"", ""_2"", ""_100"", ""_201"", ""_202"", ""_203""],
        ""long-title"": ""n/a"",
		""short-title"": ""n/a"",
		""long-description"": ""n/a"",
		""meta-keywords"": ""n/a""
	},
	{
        ""cdrid"": ""3"",
        ""url"": ""/general-information-page-3"",
        ""page-sections"": [""_1"", ""_2"", ""_3"", ""_AboutThis_1""],
		""general-sections"": [""_1"", ""_2""],
		""linked-sections"": [""_1"", ""_2"", ""_100"", ""_201"", ""_202"", ""_203""],
        ""long-title"": ""n/a"",
		""short-title"": ""n/a"",
		""long-description"": ""n/a"",
		""meta-keywords"": ""n/a""
	}
]");
        }


        /// <summary>
        /// Test for correct handling of a reference to a piloted summary (Reference to the actual summary, not a section).
        /// </summary>
        [Test]
        public void ReferenceToPilotSummary()
        {
            // Contains a reference to a summary in the pilot.
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000010"" LegacyPDQID=""12345"">
    <SummarySection id=""_1"">
        <Para id=""_100""><SummaryRef href=""CDR0000000001"" url=""/path/to/original/url"">Reference to Pilot Summary</SummaryRef></Para>
        <Para id=""_200""><SummaryRef href=""CDR0000000002"" url=""/path/to/non-pilot/summary"">Reference to Non-pilot Summary</SummaryRef></Para>
    </SummarySection>
    <SummarySection id = ""_2"">
        <Para id = ""_201"">Sub - section 2.1</Para>
        <Para id = ""_202"">Sub - section 2.2</Para>
        <Para id = ""_203"">Sub - section 2.3</Para>
    </SummarySection>
    <SummarySection id = ""_AboutThis_1"">
        <Title>About This PDQ Summary</Title >
    </SummarySection>
</Summary>
");

            SummaryPreprocessor processor = new SummaryPreprocessor();
            processor.RewriteSummaryRefAttributes(summary, SplitData);

            XmlNode testElement = summary.SelectSingleNode("//SummaryRef[@href='CDR0000000001']");
            string testUrl = testElement.Attributes["url"].Value;
            Assert.AreEqual("/general-information-page", testUrl);
        }

        /// <summary>
        /// Test for correct handling of a reference to a non-piloted summary (Reference to the actual summary, not a section).
        /// </summary>
        [Test]
        public void ReferenceToNonpilotSummary()
        {
            // Contains a reference to a summary which is not in the pilot.
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000010"" LegacyPDQID=""12345"">
    <SummarySection id=""_1"">
        <Para id=""_100""><SummaryRef href=""CDR0000000001"" url=""/path/to/original/url"">Reference to Pilot Summary</SummaryRef></Para>
        <Para id=""_200""><SummaryRef href=""CDR0000000002"" url=""/path/to/non-pilot/summary"">Reference to Non-pilot Summary</SummaryRef></Para>
    </SummarySection>
    <SummarySection id = ""_2"">
        <Para id = ""_201"">Sub - section 2.1</Para>
        <Para id = ""_202"">Sub - section 2.2</Para>
        <Para id = ""_203"">Sub - section 2.3</Para>
    </SummarySection>
    <SummarySection id = ""_AboutThis_1"">
        <Title>About This PDQ Summary</Title >
    </SummarySection>
</Summary>
");

            SummaryPreprocessor processor = new SummaryPreprocessor();
            processor.RewriteSummaryRefAttributes(summary, SplitData);

            XmlNode testElement = summary.SelectSingleNode("//SummaryRef[@href='CDR0000000002']");
            string testUrl = testElement.Attributes["url"].Value;
            Assert.AreEqual("/path/to/non-pilot/summary", testUrl);
        }

        /// <summary>
        /// Test for correct handling of a reference to a section within a piloted summary.
        /// </summary>
        [Test]
        public void ReferenceToPilotSummarySection()
        {
            // Contains a reference to a summary in the pilot.
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000010"" LegacyPDQID=""12345"">
    <SummarySection id=""_1"">
        <Para id=""_100""><SummaryRef href=""CDR0000000001#_1"" url=""/path/to/original/url-1"">Reference to 1st Pilot Summary</SummaryRef></Para>
        <Para id=""_200""><SummaryRef href=""CDR0000000002#_2"" url=""/path/to/original/url-2"">Reference to 2nd Pilot Summary</SummaryRef></Para>
    </SummarySection>
    <SummarySection id = ""_2"">
        <Para id = ""_201"">Sub - section 2.1</Para>
        <Para id = ""_202""><SummaryRef href=""CDR0000000003#_3"" url=""/path/to/original/url-3"">Reference to 3rd Pilot Summary</SummaryRef></Para>
        <Para id = ""_203"">Sub - section 2.3</Para>
    </SummarySection>
    <SummarySection id = ""_AboutThis_1"">
        <Title>About This PDQ Summary</Title >
    </SummarySection>
</Summary>
");

            SummaryPreprocessor processor = new SummaryPreprocessor();
            processor.RewriteSummaryRefAttributes(summary, SplitData);

            for (int i = 1; i <= 3; i++)
            {
                string xpath = String.Format("//SummaryRef[@href='CDR000000000{0}']", i);
                string expected = String.Format("/general-information-page-{0}", i);

                XmlNode testElement = summary.SelectSingleNode(xpath);
                string testUrl = testElement.Attributes["url"].Value;
                Assert.AreEqual(expected, testUrl);
            }
        }

        /// <summary>
        /// Test for correct handling of a reference to a section within a non-piloted summary.
        /// </summary>
        [Test]
        public void ReferenceToNonpilotSummarySection()
        {
            // Contains a reference to a summary in the pilot.
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000010"" LegacyPDQID=""12345"">
    <SummarySection id=""_1"">
        <Para id=""_100""><SummaryRef href=""CDR0000000010#_1"" url=""/path/to/original/url-1"">Reference to 1st Pilot Summary</SummaryRef></Para>
        <Para id=""_200""><SummaryRef href=""CDR0000000020#_2"" url=""/path/to/original/url-2"">Reference to 2nd Pilot Summary</SummaryRef></Para>
    </SummarySection>
    <SummarySection id = ""_2"">
        <Para id = ""_201"">Sub - section 2.1</Para>
        <Para id = ""_202""><SummaryRef href=""CDR0000000030#_3"" url=""/path/to/original/url-3"">Reference to 3rd Pilot Summary</SummaryRef></Para>
        <Para id = ""_203"">Sub - section 2.3</Para>
    </SummarySection>
    <SummarySection id = ""_AboutThis_1"">
        <Title>About This PDQ Summary</Title >
    </SummarySection>
</Summary>
");

            SummaryPreprocessor processor = new SummaryPreprocessor();
            processor.RewriteSummaryRefAttributes(summary, SplitData);

            for (int i = 1; i <= 3; i++)
            {
                string xpath = String.Format("//SummaryRef[@href='CDR00000000{0}0']", i);
                string expected = String.Format("/path/to/original/url--{0}", i);

                XmlNode testElement = summary.SelectSingleNode(xpath);
                string testUrl = testElement.Attributes["url"].Value;
                Assert.AreEqual(expected, testUrl);
            }
        }

        /// <summary>
        /// Test for correct handling when there is no summary reference.
        /// </summary>
        [Test]
        public void NoSummaryReference()
        {
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(@"
<Summary id=""CDR000000050"" LegacyPDQID=""12345"">
    <SummarySection id=""_1"">
        <Para id=""_100"">Sub - section 1.1</Para>
        <Para id=""_200"">Sub - section 1.2</Para>
    </SummarySection>
    <SummarySection id = ""_2"">
        <Para id = ""_201"">Sub - section 2.1</Para>
        <Para id = ""_202"">Sub - section 2.2</Para>
        <Para id = ""_203"">Sub - section 2.3</Para>
    </SummarySection>
    <SummarySection id = ""_AboutThis_1"">
        <Title>About This PDQ Summary</ Title >
    </SummarySection>
</Summary>
");
            string oldXml = summary.OuterXml;
            SummaryPreprocessor processor = new SummaryPreprocessor();
            processor.RewriteSummaryRefAttributes(summary, SplitData);
            string newXml = summary.OuterXml;

            Assert.AreEqual(oldXml, newXml);
        }
    }
}
