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

        [TestFixtureSetUp]
        public void Setup()
        {
            SplitData = SplitDataManager.CreateFromString(@"
[
	{
		""comment"": ""Split manager which matches VALID_SUMMARY"",

        ""cdrid"": ""1"",
        ""url"": ""n/a"",
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

    }
}
