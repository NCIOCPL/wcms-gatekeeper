using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using GateKeeper.Common;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.BusinessObjects;
using GKManagers;

namespace GateKeeper.UnitTest.StorageIntegrated.Summary
{
    [TestFixture, Explicit]
    public class SummarySaveTest
    {
        #region Fields

        Utilities.Timers.HiPerfTimer perfTimer = new Utilities.Timers.HiPerfTimer();
        XmlDocument summaryXmlWithTables = new XmlDocument();
        DocumentXPathManager xPathManager = new DocumentXPathManager();

        #endregion

        #region Setup and Teardown

        /// <summary>
        /// Setup run once for all Unit Tests.
        /// </summary>
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {

        }

        /// <summary>
        /// Teardown run once for all Unit Tests.
        /// </summary>
        [TestFixtureTearDown]
        public void FixtureTearDown()
        {

        }

        /// <summary>
        /// Setup run for each Unit Test.
        /// </summary>
        [SetUp]
        public void UnitTestSetUp()
        {

        }

        /// <summary>
        /// Teardown run for each Unit Test.
        /// </summary>
        [TearDown]
        public void UnitTearDown()
        {

        }

        #endregion

        #region Utility Methods

         private static void PrintDetails(SummaryDocument summary)
        {
            Console.WriteLine("Dumping Summary Sections to " + System.Environment.CurrentDirectory);
            //Console.WriteLine("Summary Sections:");
            foreach (SummarySection section in summary.SectionList)
            {
                Console.WriteLine("Section id = " + section.SectionID);
                Console.WriteLine("Section title = " + section.Title);
                Console.WriteLine("Table section = " + section.IsTableSection);
                Console.WriteLine("Top level = " + section.IsTopLevel);
                Console.WriteLine("TOC = " + section.TOC);

                using (StreamWriter sw = new StreamWriter("Render\\RenderOutput" + section.SectionID + ".html"))
                {
                    using (XmlWriter xwriter = XmlWriter.Create(sw))
                    {
                        section.Html.WriteContentTo(xwriter);
                    }
                }
            }

            foreach (SummarySection section in summary.Level4SectionList)
            {
                Console.WriteLine("Section id = " + section.SectionID);
                Console.WriteLine("Section title = " + section.Title);
                Console.WriteLine("Table section = " + section.IsTableSection);
                Console.WriteLine("Top level = " + section.IsTopLevel);
                Console.WriteLine("Level = " + section.Level);
                Console.WriteLine("TOC = " + section.TOC);

                //Console.WriteLine("HTML = " + section.Html.OuterXml);

                using (StreamWriter sw = new StreamWriter("Render\\RenderOutput" + section.SectionID + ".html"))
                {
                    using (XmlWriter xwriter = XmlWriter.Create(sw))
                    {
                        section.Html.WriteContentTo(xwriter);
                    }
                }
            }

            foreach (SummarySection section in summary.Level5SectionList)
            {
                Console.WriteLine("Section id = " + section.SectionID);
                Console.WriteLine("Section title = " + section.Title);
                Console.WriteLine("Table section = " + section.IsTableSection);
                Console.WriteLine("Top level = " + section.IsTopLevel);
                Console.WriteLine("Level = " + section.Level);
                Console.WriteLine("TOC = " + section.TOC);

                //Console.WriteLine("HTML = " + section.Html.OuterXml);

                using (StreamWriter sw = new StreamWriter("Render\\RenderOutput" + section.SectionID + ".html"))
                {
                    using (XmlWriter xwriter = XmlWriter.Create(sw))
                    {
                        section.Html.WriteContentTo(xwriter);
                    }
                }
            }

            foreach (SummarySection tableSection in summary.TableSectionList)
            {
                Console.WriteLine("Section id = " + tableSection.SectionID);
                Console.WriteLine("Section title = " + tableSection.Title);
                Console.WriteLine("Table section = " + tableSection.IsTableSection);
                Console.WriteLine("Top level = " + tableSection.IsTopLevel);
                Console.WriteLine("Level = " + tableSection.Level);
                Console.WriteLine("TOC = " + tableSection.TOC);

                //Console.WriteLine("HTML = " + section.Html.OuterXml);

                using (StreamWriter sw = new StreamWriter("Render\\RenderOutput" + tableSection.SectionID + ".html"))
                {
                    using (XmlWriter xwriter = XmlWriter.Create(sw))
                    {
                        tableSection.Html.WriteContentTo(xwriter);
                    }
                }
            }
        }

        private static void PrintShort(SummaryDocument summary)
        {
            Console.WriteLine("SectionList count = " + summary.SectionList.Count);
            Console.WriteLine("Level4SectionList count = " + summary.Level4SectionList.Count);
            Console.WriteLine("Level5SectionList count = " + summary.Level5SectionList.Count);
            Console.WriteLine("TableSectionList count = " + summary.TableSectionList.Count);

            Console.WriteLine("Total count = " + (summary.SectionList.Count +
                summary.Level4SectionList.Count +
                summary.Level5SectionList.Count +
                summary.TableSectionList.Count));
        }

        /// <summary>
        /// Utility to read in an XML file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string ReadXml(string fileName)
        {
            string xml = string.Empty;

            using (StreamReader sr = new StreamReader(fileName))
            {
                xml = sr.ReadToEnd();
            }

            return xml;
        }

        /// <summary>
        /// Writes top-level summary sections to disk.
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="directoryName"></param>
        private void WriteSummary(SummaryDocument summary, string directoryName)
        {
            foreach (SummarySection section in summary.SectionList)
            {
                if (section.IsTopLevel)
                {
                    using (StreamWriter sw = new StreamWriter(string.Format("{0}/{1}.htm", directoryName, section.SectionID)))
                    {
                        sw.WriteLine(section.Html.OuterXml);
                    }
                }
            }
        }

        #endregion

        #region Unit Tests

        /// <summary>
        /// Unit test.
        /// </summary>
        /// 
        /// This unit test pass in cdrid to get xml 
        /// 
        [Test]
        public void TestSummaryByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 62675;
                int requestID = 621;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(requestID, cdrID);
                XmlDocument summaryDoc = new XmlDocument();
                summaryDoc.PreserveWhitespace = true;
                summaryDoc.LoadXml(requestData.DocumentDataString);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                SummaryExtractor extractor = new SummaryExtractor();
                extractor.Extract(summaryDoc, summary, xPathManager, TargetedDevice.screen);
                SummaryRenderer render = new SummaryRenderer();
                render.Render(summary);

                // Test Summary saving
                using (SummaryQuery sumQuery = new SummaryQuery())
                {
                    if (sumQuery.SaveDocument(summary, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

                perfTimer.Stop();
                Console.WriteLine("Extraction/Rendering/Saving complete; Duration = " + perfTimer.Duration);
                Console.WriteLine("========================================");
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from summary document failed.", e);
            }
        }


        [Test]
        public void TestSet1SummaryEnglishHPWithRefAndTables62787()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing SummaryDataAccessTest.TestDataAccess()...");

                string xml = ReadXml(@"./XMLData/SummaryEnglishHPWithReference62787.xml");
               
                perfTimer.Start();
                XmlDocument summaryDoc = new XmlDocument();
                summaryDoc.PreserveWhitespace = true;
                summaryDoc.LoadXml(xml);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                SummaryExtractor extractor = new SummaryExtractor();
                extractor.Extract(summaryDoc, summary, xPathManager, TargetedDevice.screen);
                SummaryRenderer render = new SummaryRenderer();
                render.Render(summary);

               // Test Summary saving
                using (SummaryQuery sumQuery = new SummaryQuery())
                {
                    if (sumQuery.SaveDocument(summary, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

                perfTimer.Stop();
                Console.WriteLine("Extraction/Rendering/Saving complete; Duration = " + perfTimer.Duration);
                Console.WriteLine("========================================");
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from summary document failed.", e);
            }
        }

        [Test]
        public void TestSet1SummaryEnglishPatientWithMedia62955()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing SummaryDataAccessTest.TestDataAccess()...");

                string xml = ReadXml(@"./XMLData/SummaryPatientWithMeida62955.xml");
                perfTimer.Start();
                XmlDocument summaryDoc = new XmlDocument();
                summaryDoc.PreserveWhitespace = true;
                summaryDoc.LoadXml(xml);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                SummaryExtractor extractor = new SummaryExtractor();
                extractor.Extract(summaryDoc, summary, xPathManager, TargetedDevice.screen); ;
                SummaryRenderer render = new SummaryRenderer();
                render.Render(summary);

                // Test Summary saving
                using (SummaryQuery sumQuery = new SummaryQuery())
                {
                    if (sumQuery.SaveDocument(summary, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

                perfTimer.Stop();
                Console.WriteLine("Extraction/Rendering/Saving complete; Duration = " + perfTimer.Duration);
                Console.WriteLine("========================================");
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from summary document failed.", e);
            }
        }


        [Test]
        public void TestSet1SummarySpanishPatient256762()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing SummaryDataAccessTest.TestDataAccess()...");

                string xml = ReadXml(@"./XMLData/SummaryPatientSpanish256762.xml");
                perfTimer.Start();
                XmlDocument summaryDoc = new XmlDocument();
                summaryDoc.PreserveWhitespace = true;
                summaryDoc.LoadXml(xml);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                SummaryExtractor extractor = new SummaryExtractor();
                extractor.Extract(summaryDoc, summary, xPathManager, TargetedDevice.screen); ;
                SummaryRenderer render = new SummaryRenderer();
                render.Render(summary);

                // Test Summary saving
                using (SummaryQuery sumQuery = new SummaryQuery())
                {
                    if (sumQuery.SaveDocument(summary, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

                perfTimer.Stop();
                Console.WriteLine("Extraction/Rendering/Saving complete; Duration = " + perfTimer.Duration);
                Console.WriteLine("========================================");
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from summary document failed.", e);
            }
        }

        [Test]
        public void TestSet1SummarySpanishHPWithMedia256668()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing SummaryDataAccessTest.TestDataAccess()...");

                string xml = ReadXml(@"./XMLData/SummaryHPSpanish256668.xml");
                perfTimer.Start();
                XmlDocument summaryDoc = new XmlDocument();
                summaryDoc.PreserveWhitespace = true;
                summaryDoc.LoadXml(xml);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                SummaryExtractor extractor = new SummaryExtractor();
                extractor.Extract(summaryDoc, summary, xPathManager, TargetedDevice.screen); ;
                SummaryRenderer render = new SummaryRenderer();
                render.Render(summary);

                // Test Summary saving
                using (SummaryQuery sumQuery = new SummaryQuery())
                {
                    if (sumQuery.SaveDocument(summary, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

                perfTimer.Stop();
                Console.WriteLine("Extraction/Rendering/Saving complete; Duration = " + perfTimer.Duration);
                Console.WriteLine("========================================");
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from summary document failed.", e);
            }
        }

        [Test]
        public void TestSummaryPatientEnglish258017()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing SummaryDataAccessTest.TestDataAccess()...");

                string xml = ReadXml(@"./XMLData/SummaryPatientVersion258017.xml");
                perfTimer.Start();
                XmlDocument summaryDoc = new XmlDocument();
                summaryDoc.PreserveWhitespace = true;
                summaryDoc.LoadXml(xml);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                SummaryExtractor extractor = new SummaryExtractor();
                extractor.Extract(summaryDoc, summary, xPathManager, TargetedDevice.screen); ;
                SummaryRenderer render = new SummaryRenderer();
                render.Render(summary);

                // Test Summary saving
                using (SummaryQuery sumQuery = new SummaryQuery())
                {
                    if (sumQuery.SaveDocument(summary, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

                perfTimer.Stop();
                Console.WriteLine("Extraction/Rendering/Saving complete; Duration = " + perfTimer.Duration);
                Console.WriteLine("========================================");
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from summary document failed.", e);
            }
        }

        [Test]
        public void TestSummaryWithReference257999()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing SummaryDataAccessTest.TestDataAccess()...");

                string xml = ReadXml(@"./XMLData/SummarySectionReference257999.xml");
                perfTimer.Start();
                XmlDocument summaryDoc = new XmlDocument();
                summaryDoc.PreserveWhitespace = true;
                summaryDoc.LoadXml(xml);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                SummaryExtractor extractor = new SummaryExtractor();
                extractor.Extract(summaryDoc, summary, xPathManager, TargetedDevice.screen); ;
                SummaryRenderer render = new SummaryRenderer();
                render.Render(summary);

                // Test Summary saving
                using (SummaryQuery sumQuery = new SummaryQuery())
                {
                    if (sumQuery.SaveDocument(summary, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

                perfTimer.Stop();
                Console.WriteLine("Extraction/Rendering/Saving complete; Duration = " + perfTimer.Duration);
                Console.WriteLine("========================================");
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from summary document failed.", e);
            }
        }

        [Test]
        public void TestSummaryWithTable299612()
        {
            // Set up
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing SummaryDataAccessTest.TestDataAccess()...");

                string xml = ReadXml(@"./XMLData/SummaryWithTable299612.xml");
                perfTimer.Start();
                XmlDocument summaryDoc = new XmlDocument();
                summaryDoc.PreserveWhitespace = true;
                summaryDoc.LoadXml(xml);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                SummaryExtractor extractor = new SummaryExtractor();
                extractor.Extract(summaryDoc, summary, xPathManager, TargetedDevice.screen); ;
                SummaryRenderer render = new SummaryRenderer();
                render.Render(summary);

                // Test Summary saving
                using (SummaryQuery sumQuery = new SummaryQuery())
                {
                    if (sumQuery.SaveDocument(summary, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }

                perfTimer.Stop();
                Console.WriteLine("Extraction/Rendering/Saving complete; Duration = " + perfTimer.Duration);
                Console.WriteLine("========================================");
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from summary document failed.", e);
            }
        }


        [Test]
        public void TestSummarySummaryRef62863()
        {
            // Set up
            try
            {
                string xml = ReadXml(@"./XMLData/SummarySummaryRefLink62863.xml");
                perfTimer.Start();
                XmlDocument summaryDoc = new XmlDocument();
                summaryDoc.PreserveWhitespace = true;
                summaryDoc.LoadXml(xml);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                SummaryDocument summary = new SummaryDocument();
                summary.WarningWriter = warningWriter;
                summary.InformationWriter = informationWriter;
                SummaryExtractor extractor=new SummaryExtractor();
                extractor.Extract(summaryDoc, summary, xPathManager, TargetedDevice.screen); ;
                SummaryRenderer render = new SummaryRenderer();
                render.Render(summary);

                // Test Summary saving
                using (SummaryQuery sumQuery = new SummaryQuery())
                {
                    if (sumQuery.SaveDocument(summary, "ychen"))
                        Console.WriteLine("Saving Summary complete.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from summary document failed.", e);
            }
        }

        #region Private method
        private void Message(string s)
        { Console.WriteLine(s); }
        #endregion



        #endregion
    }
}
