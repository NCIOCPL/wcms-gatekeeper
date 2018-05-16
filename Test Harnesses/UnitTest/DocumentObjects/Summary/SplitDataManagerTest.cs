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
		""comment"": ""Split data for first summary"",

        ""cdrid"": ""1"",
        ""url"": ""Not sure what goes here"",
        ""page-sections"": [""_1"", ""_2"", ""_AboutThis_1""],
		""general-sections"": [""_1""],
		""linked-sections"": [""_1"", ""_90""],
		""long-title"": ""The long title for the pilot page"",
		""short-title"": ""The short title for the summary's pilot page"",
		""long-description"": ""The pilot page's long description"",
		""meta-keywords"": ""keyword1 keyword2""
	},
	{
		""comment"": ""Split data for second summary"",

        ""cdrid"": ""2"",
        ""url"": ""Not sure what goes here"",
        ""page-sections"": [""_3"", ""_4"", ""_AboutThis_1""],
		""general-sections"": [""_3""],
		""linked-sections"": [""_3"", ""_90""],
		""long-title"": ""The long title for the pilot page"",
		""short-title"": ""The short title for the summary's pilot page"",
		""long-description"": ""The pilot page's long description"",
		""meta-keywords"": ""keyword3 keyword4""
	}
]    
");

        }

        /// <summary>
        /// Tests that Summaries defined in the split data are found correctly.
        /// </summary>
        [TestCase(1)]
        [TestCase(2)]
        public void SummaryIsFound(int summaryID)
        {
            // Summary ID 1 should be found
            Assert.IsTrue(MatchingSplitData.SummaryIsSplit(summaryID));
        }

        /// <summary>
        /// Tests that Summaries which do NOT appear in the split data reported correctly.
        /// </summary>
        [TestCase(10)]
        [TestCase(20)]
        public void SummaryNotFound(int summaryID)
        {
            // Summary 2 shouldn't be found
            Assert.IsFalse(MatchingSplitData.SummaryIsSplit(summaryID));
        }

        /// <summary>
        /// Verify that SplitData is returned from the GetSplitData method.
        /// </summary>
        /// <param name="summaryID"></param>
        [TestCase(1)]
        [TestCase(2)]
        public void SplitDataIsFound(int summaryID)
        {
            SplitData data = MatchingSplitData.GetSplitData(summaryID);
            Assert.IsNotNull(data);
        }

        /// <summary>
        /// Verify the *correct* SplitData object is returned from GetSplitData.
        /// </summary>
        [Test]
        public void CorrectSplitDataIsFound()
        {
            SplitData data = MatchingSplitData.GetSplitData(1);

            // CDRID
            Assert.AreEqual(1, data.CdrId);

            // Page sections.
            Assert.AreEqual(3, data.PageSections.Length);
            Assert.AreEqual("_2", data.PageSections[1]);

            // Sections embedded in a page.
            Assert.AreEqual(1, data.GeneralSections.Length);
            Assert.AreEqual("_1", data.GeneralSections[0]);

            // Link targets
            Assert.AreEqual(2, data.LinkedSections.Length);
            Assert.AreEqual("_90", data.LinkedSections[1]);

            Assert.AreEqual("The long title for the pilot page", data.LongTitle);
            Assert.AreEqual("The short title for the summary's pilot page", data.ShortTitle);
            Assert.AreEqual("The pilot page's long description", data.LongDescription);
            Assert.AreEqual("keyword1 keyword2", data.MetaKeywords);
        }

        /// <summary>
        /// Verify that non-existant SplitData is correctly reported.
        /// </summary>
        /// <param name="summaryID"></param>
        [TestCase(10)]
        [TestCase(20)]
        public void SplitDataIsNotFound(int summaryID)
        {
            SplitData data = MatchingSplitData.GetSplitData(summaryID);
            Assert.IsNull(data);
        }


        /// <summary>
        /// Verify that references to a section in a summary's general information section are
        /// correctly identified.
        /// </summary>
        /// <param name="summaryID">ID of a summary which is potentially part of the split pilot.</param>
        /// <param name="sectionID">ID of a summary section.</param>
        [TestCase(1, "_1")]
        [TestCase(1, "_90")]
        public void SummaryReferenceToGeneralInformationDetected(int summaryID, string sectionID)
        {
            SplitDataManager splitData = SplitDataManager.CreateFromString(@"
[
	{
		""comment"": ""Split data for first summary"",

        ""cdrid"": ""1"",
        ""url"": ""n/a"",
        ""page-sections"": [""_1"", ""_2"", ""_AboutThis_1""],
		""general-sections"": [""_1""],
		""linked-sections"": [""_1"", ""_90""],
		""long-title"": ""n/a"",
		""short-title"": ""n/a"",
		""long-description"": ""n/a"",
		""meta-keywords"": ""n/a""
	}
]    
");
            bool result = splitData.ReferenceIsForGeneralSection(summaryID, sectionID);
            Assert.IsTrue(result);
        }


        /// <summary>
        /// Verify that references to a section in which is not part of a summary's general information section are
        /// correctly identified.
        /// </summary>
        /// <param name="summaryID">ID of a summary which is potentially part of the split pilot.</param>
        /// <param name="sectionID">ID of a summary section.</param>
        [TestCase(1, "_2")] // Piloted summary, Top-level section, not part of general information.
        [TestCase(7, "_1")] // Non-pilot summary.
        public void SummaryReferenceToNonGeneralSectionIgnored(int summaryID, string sectionID)
        {
            SplitDataManager splitData = SplitDataManager.CreateFromString(@"
[
	{
		""comment"": ""Split data for first summary"",

        ""cdrid"": ""1"",
        ""url"": ""n/a"",
        ""page-sections"": [""_1"", ""_2"", ""_AboutThis_1""],
		""general-sections"": [""_1""],
		""linked-sections"": [""_1"", ""_90""],
		""long-title"": ""n/a"",
		""short-title"": ""n/a"",
		""long-description"": ""n/a"",
		""meta-keywords"": ""n/a""
	}
]    
");
            bool result = splitData.ReferenceIsForGeneralSection(summaryID, sectionID);
            Assert.IsFalse(result);
        }



        /// <summary>
        /// Verify that sections for a summary's general information pages are
        /// correctly identified.
        /// </summary>
        /// <param name="summaryID">ID of a summary which is potentially part of the split pilot.</param>
        /// <param name="sectionID">ID of a summary section.</param>
        [TestCase(1, "_1")]
        public void GeneralInformationPagesDetected(int summaryID, string sectionID)
        {
            SplitDataManager splitData = SplitDataManager.CreateFromString(@"
[
	{
		""comment"": ""Split data for first summary"",

        ""cdrid"": ""1"",
        ""url"": ""n/a"",
        ""page-sections"": [""_1"", ""_2"", ""_AboutThis_1""],
		""general-sections"": [""_1""],
		""linked-sections"": [""_1"", ""_90""],
		""long-title"": ""n/a"",
		""short-title"": ""n/a"",
		""long-description"": ""n/a"",
		""meta-keywords"": ""n/a""
	}
]    
");
            bool result = splitData.SectionIsAGeneralInformationPage(summaryID, sectionID);
            Assert.IsTrue(result);
        }


        /// <summary>
        /// Verify that sections which are not top-level general information sections/pages are
        /// correctly identified.
        /// </summary>
        /// <param name="summaryID">ID of a summary which is potentially part of the split pilot.</param>
        /// <param name="sectionID">ID of a summary section.</param>
        [TestCase(1, "_2")] // Piloted summary, Top-level section, not part of general information.
        [TestCase(1, "_90")] // Piloted summary, general information section, but *not* a general information *page*.
        [TestCase(7, "_1")] // Non-pilot summary.
        public void NonGeneralInformationPageSectionIdentified(int summaryID, string sectionID)
        {
            SplitDataManager splitData = SplitDataManager.CreateFromString(@"
[
	{
		""comment"": ""Split data for first summary"",

        ""cdrid"": ""1"",
        ""url"": ""n/a"",
        ""page-sections"": [""_1"", ""_2"", ""_AboutThis_1""],
		""general-sections"": [""_1""],
		""linked-sections"": [""_1"", ""_90""],
		""long-title"": ""n/a"",
		""short-title"": ""n/a"",
		""long-description"": ""n/a"",
		""meta-keywords"": ""n/a""
	}
]    
");
            bool result = splitData.SectionIsAGeneralInformationPage(summaryID, sectionID);
            Assert.IsFalse(result);
        }

    }
}
