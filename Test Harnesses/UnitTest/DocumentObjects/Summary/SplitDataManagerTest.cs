using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using NUnit.Framework;

using GateKeeper.Common;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.Preprocessors;
using GateKeeper.DocumentObjects;

namespace GateKeeper.UnitTest.DocumentObjects.Summary
{
    /// <summary>
    /// Tests for the SummaryPreprocessor.Validate method
    /// </summary>
    [TestFixture]
    class SplitDataManagerTest
    {
        SplitDataManager MatchingSplitData;

        [TestFixtureSetUp]
        public void Setup()
        {
            MatchingSplitData = SplitDataManager.CreateFromString(@"
[
	{
		""comment"": ""Split manager which matches VALID_SUMMARY"",

        ""cdrid"": ""1"",
        ""url"": ""Not sure what goes here"",
        ""page-sections"": [""_1"", ""_2"", ""_AboutThis_1""],
		""general-sections"": [""_1""],
		""linked-sections"": [""_1"", ""_90""],
		""long-title"": ""The long title for the pilot page"",
		""short-title"": ""The short title for the summary's pilot page"",
		""long-description"": ""The pilot page's long description"",
		""meta-keywords"": ""keyword1 keyword2""
	}
]    
");

        }

        [Test]
        public void SummaryIsFound()
        {
            // Summary ID 1 should be found
            Assert.IsTrue(MatchingSplitData.SummaryIsSplit(1));
        }

        [Test]
        public void SummaryNotFound()
        {
            // Summary 2 shouldn't be found
            Assert.IsFalse(MatchingSplitData.SummaryIsSplit(2));
        }

    }
}
