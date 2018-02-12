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
    class SummaryPreprocessorValidateTest
    {
        HistoryEntryWriter fakeInfoWriter = delegate (string message) { Console.Write(message); };
        HistoryEntryWriter fakeWarningWriter = delegate (string message) { Console.Write(message); };

        // Skeleton of a valid summary.
        const string VALID_SUMMARY = @"
<Summary id=""CDR000000001"" LegacyPDQID=""1278"">
    <SummarySection id=""_1"">
        <Para id = ""_90"" >Sub-section</Para>
    </SummarySection>
    <SummarySection id=""_2"">
        <Para id = ""_91"" >Sub-section 2</Para>
    </SummarySection>
    <SummarySection id=""_AboutThis_1"">
        <Title>About This PDQ Summary</Title>
    </SummarySection>
</Summary>
";


        // Not actually a summary.
        const string GLOSSARY_TERM = @"
<GlossaryTerm id=""CDR000000001"" LegacyPDQID=""1278"">
</GlossaryTerm>
";

        MockSplitDataManager MatchingSplitData; // Split manager which matches VALID_SUMMARY
        MockSplitDataManager UnmatchedSplitData;// Split manager which does NOT matches VALID_SUMMARY
        MockSplitDataManager incorrectGISplitData;// Split manager with a non-existant section identified as General Information.

        [TestFixtureSetUp]
        public void Setup()
        {
            // Split manager which matches VALID_SUMMARY
            MatchingSplitData = new MockSplitDataManager();
            MatchingSplitData.splitConfigs = new List<SplitData>();
            MatchingSplitData.splitConfigs.Add(
                new SplitData()
                {
                    CdrId = 1,
                    PageSections = new string[] { "_1", "_2", "_AboutThis_1" },
                    GeneralSections = new string[] { "_1" },
                    LinkedSections= new string[] { "_1", "_90" },
                    LongDescription = "Long description",
                    LongTitle = "Long title",
                    ShortTitle = "Short title",
                    MetaKeywords = "word1 word2",
                    Url = "Not sure what goes here"
                }
            );

            // Split manager which does NOT matches VALID_SUMMARY
            UnmatchedSplitData = new MockSplitDataManager();
            UnmatchedSplitData.splitConfigs = new List<SplitData>();
            UnmatchedSplitData.splitConfigs.Add(
                new SplitData()
                {
                    CdrId = 2,
                    PageSections = new string[] { "_1", "_2", "_AboutThis_1" },
                    GeneralSections = new string[] { "_1" },
                    LinkedSections = new string[] { "_1", "_90" },
                    LongDescription = "Long description",
                    LongTitle = "Long title",
                    ShortTitle = "Short title",
                    MetaKeywords = "word1 word2",
                    Url = "Not sure what goes here"
                }
            );

            // Split manager with a non-existant section identified as General Information.
            incorrectGISplitData = new MockSplitDataManager();
            incorrectGISplitData.splitConfigs = new List<SplitData>();
            incorrectGISplitData.splitConfigs.Add(
                new SplitData()
                {
                    CdrId = 2,
                    PageSections = new string[] { "_1", "_2", "_AboutThis_1" },
                    GeneralSections = new string[] { "_7" }, // Non-existant
                    LinkedSections = new string[] { "_1", "_90" },
                    LongDescription = "Long description",
                    LongTitle = "Long title",
                    ShortTitle = "Short title",
                    MetaKeywords = "word1 word2",
                    Url = "Not sure what goes here"
                }
            );

        }


        /// <summary>
        /// Test that sumamry with a valid root element is allowed.
        /// </summary>
        [Test]
        public void ValidSummaryRoot()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(VALID_SUMMARY);
            ISplitDataManager splitMgr = new MockSplitDataManager();
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.DoesNotThrow(() => { processor.Validate(doc, splitMgr); });
        }

        /// <summary>
        /// Test that sumamry with an invalid root element fails.
        /// </summary>
        [Test]
        public void FailInvalidSummaryRoot()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(GLOSSARY_TERM);
            ISplitDataManager splitMgr = new MockSplitDataManager();
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.Throws<ValidationException>(() => { processor.Validate(doc, splitMgr); });
        }

        /// <summary>
        /// Test that split data that matches the summary is accepted.
        /// </summary>
        [Test]
        public void ValidSplit()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(VALID_SUMMARY);
            ISplitDataManager splitMgr = MatchingSplitData;
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.DoesNotThrow(() => { processor.Validate(doc, splitMgr); });
        }

        /// <summary>
        /// Test that a summary which doesn't exist in the split data is allowed.
        /// </summary>
        [Test]
        public void UnmatchedSplit()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(VALID_SUMMARY);
            ISplitDataManager splitMgr = UnmatchedSplitData;
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.DoesNotThrow(() => { processor.Validate(doc, splitMgr); });
        }


        /// <summary>
        /// Test that processing fails for split data with a non-existant section identified as General Information.
        /// </summary>
        [Test]
        public void FailNonExistantGISection()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(VALID_SUMMARY);
            ISplitDataManager splitMgr = UnmatchedSplitData;
            SummaryPreprocessor processor = new SummaryPreprocessor();
            Assert.Throws<ValidationException>(() => { processor.Validate(doc, splitMgr); });
        }
    }
}
