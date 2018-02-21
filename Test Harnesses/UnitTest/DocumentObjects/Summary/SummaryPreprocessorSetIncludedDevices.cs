using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

using GateKeeper.DocumentObjects.Summary;
using GKManagers.Preprocessors;

namespace GateKeeper.UnitTest.DocumentObjects.Summary
{
    /// <summary>
    /// Tests for the Summary Preprocessor's SetIncludedDevices() method.
    /// </summary>
    [TestFixture]
    class SummaryPreprocessorSetIncludedDevices
    {
        /*
    These tests assume the following (very simplified) summary is participating in the pilot
    and perform tests against the corresponding split data.

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

        const string PILOT_SUMMARY_XML = @"
<Summary id=""CDR000000001"" LegacyPDQID=""1278"">
    <SummarySection id=""_1"">
        <Para id = ""_100"" >Sub-section 1.100 </Para>
        <Para id = ""_200"" >Sub-section 1.200 </Para>
    </SummarySection>
    <SummarySection id=""_2"" IncludedDevices=""screen"">
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
";

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
        ""url"": ""/general-information-page-1"",
        ""page-sections"": [""_1"", ""_2"", ""_3"", ""_AboutThis_1""],
		""general-sections"": [""_1"", ""_2""],
		""linked-sections"": [""_1"", ""_2"", ""_100"", ""_201"", ""_202"", ""_203""],
        ""long-title"": ""n/a"",
		""short-title"": ""n/a"",
		""long-description"": ""n/a"",
		""meta-keywords"": ""n/a""
	}]");
        }


        /// <summary>
        /// Verify that the devices "general" and "syndication" are added to top-level
        /// sections on the general-sections list.
        /// </summary>
        [TestCase ("/Summary/SummarySection[id=\"_1\"")] // Section has no existing IncludedDevices
        [TestCase ("/Summary/SummarySection[id=\"_2\"")] // Section has an existing IncludedDevices which we will deliberately overwrite.
        public void DevicesAddedOnGeneralSections(string targetPage)
        {
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(PILOT_SUMMARY_XML);

            SummaryPreprocessor processor = new SummaryPreprocessor();
            processor.SetIncludedDevices(summary, SplitData);

            XmlNode testElement = summary.SelectSingleNode(targetPage);
            string testValue = testElement.Attributes["IncludedDevices"].Value;
            string[] devices = testValue.Split(' ');

            bool generalFound = Array.Exists(devices, device => device == "general");
            Assert.IsTrue(generalFound, "Device 'general' not found.");
            bool syndicationFound = Array.Exists(devices, device => device == "syndication");
            Assert.IsTrue(syndicationFound, "Device 'syndication' not found.");
        }

        /// <summary>
        /// Verify that no devices are added to top-level sections which are not on general-sections list.
        /// </summary>
        [TestCase("/Summary/SummarySection[id=\"_3\"")] // Section has no existing IncludedDevices
        [TestCase("/Summary/SummarySection[id=\"_AboutThis_1\"")] // Section has an existing IncludedDevices which we will deliberately overwrite.
        public void DevicesNotAddedToTreatmentSections(string targetPage)
        {
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(PILOT_SUMMARY_XML);

            SummaryPreprocessor processor = new SummaryPreprocessor();
            processor.SetIncludedDevices(summary, SplitData);

            XmlNode testElement = summary.SelectSingleNode(targetPage);
            Assert.IsNull(testElement.Attributes["IncludedDevices"], "The 'IncludedDevices' attribute should not be found.");
        }

        /// <summary>
        /// For a piloted summary, verify that all top-level sections are marked for syndication
        /// </summary>
        [Test]
        public void AllPagesMarkedForSyndication()
        {
            XmlDocument summary = new XmlDocument();
            summary.LoadXml(PILOT_SUMMARY_XML);

            SummaryPreprocessor processor = new SummaryPreprocessor();
            processor.SetIncludedDevices(summary, SplitData);

            XPathNavigator xNav = summary.CreateNavigator();
            XPathNodeIterator nodeList = xNav.Select("/Summary/SummarySection");

            foreach (XPathNavigator node in nodeList)
            {
                string attributeValue = node.GetAttribute("IncludedDevices", "");
                string[] devices = attributeValue.Split(' ');
                bool syndicationFound = Array.Exists(devices, device => device.Equals(SummarySectionDeviceType.syndication.ToString()));
                Assert.IsTrue(syndicationFound, "Device 'syndication' not found.");
            }
        }

        /// <summary>
        /// Verify that no changes are made when the summary isn't part of the pilot.
        /// </summary>
        [Test]
        public void NoChangesForNonpilotSummary()
        {
            XmlDocument nonPilotSummary = new XmlDocument();
            nonPilotSummary.LoadXml(@"
<Summary id=""CDR000000001"" LegacyPDQID=""1278"">
    <SummarySection id=""_1"">
        <Para id = ""_100"" >Sub-section 1.100 </Para>
        <Para id = ""_200"" >Sub-section 1.200 </Para>
    </SummarySection>
    <SummarySection id=""_2"" IncludedDevices=""screen"">
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

            string originalXml = nonPilotSummary.OuterXml;
            SummaryPreprocessor processor = new SummaryPreprocessor();
            processor.SetIncludedDevices(nonPilotSummary, SplitData);

            string unchangedXml = nonPilotSummary.OuterXml;
            Assert.AreEqual(originalXml, unchangedXml, "Processing should not change the XML.");
        }
    }
}
