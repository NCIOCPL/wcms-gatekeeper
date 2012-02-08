using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using NUnit.Framework;
using GateKeeper.Common;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;
using GKManagers;

namespace GateKeeper.UnitTest.StorageIntegrated.GlossaryTerm
{
    /// <summary>
    /// Glossary Term extraction unit test.
    /// </summary>
    [TestFixture, Explicit]
    public class GlossaryTermDeleteTest
    {
        #region Fields
        Utilities.Timers.HiPerfTimer perfTimer = new Utilities.Timers.HiPerfTimer();
        List<double> durationList = new List<double>();
        DocumentXPathManager xPathManager = new DocumentXPathManager();

        #endregion

        #region Unit Tests

        /// <summary>
        /// Unit test.
        /// </summary>
        [Test]
        public void TestGlossaryTermByCDRID()
        {
            // Set up
            try
            {
                int cdrID = 335128;
                RequestData requestData = RequestManager.LoadRequestDataByCdrid(101332, cdrID);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(requestData.DocumentDataString);

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                GlossaryTermDocument glossaryTerm = new GlossaryTermDocument();
                glossaryTerm.WarningWriter = warningWriter;
                glossaryTerm.InformationWriter = informationWriter;
                GlossaryTermExtractor extract = new GlossaryTermExtractor();
                extract.Extract(xmlDoc, glossaryTerm, xPathManager);

                // Test Rendering
                GlossaryTermRenderer gtRender = new GlossaryTermRenderer();
                gtRender.Render(glossaryTerm);
               
                // Test query
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    if (GTQuery.SaveDocument(glossaryTerm, "ychen"))
                        Console.WriteLine("Saving Glossary Term complete.");
                }

                this.perfTimer.Stop();
                Console.WriteLine("Testing TestGlossaryTermEnglish complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from glossary term document failed.", e);
            }
        }

        [Test]
        public void GlossaryTermSaveSpanish335158()
        {
            // Set up
            try
            {
                XmlDocument glossaryTermWithSpanishXml = new XmlDocument();
                Console.WriteLine("========================================");

                perfTimer.Start();
                using (StreamReader srBuffer2 = new StreamReader(@"./XMLData/GlossaryTermWithSpanish.xml"))
                {
                    glossaryTermWithSpanishXml.PreserveWhitespace = true;
                    glossaryTermWithSpanishXml.LoadXml(srBuffer2.ReadToEnd());
                }

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                GlossaryTermDocument glossaryTerm = new GlossaryTermDocument();
                glossaryTerm.WarningWriter = warningWriter;
                glossaryTerm.InformationWriter = informationWriter;
                GlossaryTermExtractor extractor = new GlossaryTermExtractor();
                extractor.Extract(glossaryTermWithSpanishXml, glossaryTerm, xPathManager);

                // Test Rendering
                GlossaryTermRenderer gtRender = new GlossaryTermRenderer();
                gtRender.Render(glossaryTerm);

                // Test query
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    if (GTQuery.SaveDocument(glossaryTerm, "ychen"))
                        Console.WriteLine("Saving Glossary Term complete.");
                }

                this.perfTimer.Stop();
                Console.WriteLine("Testing TestGlossaryTermSpanish complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from glossary term document failed.", e);
            }
        }

        [Test]
        public void GlossaryTermSavePronounciation322891()
        {
            // Set up
            try
            {
                XmlDocument glossaryTermWithPronounciationXml = new XmlDocument();
                Console.WriteLine("========================================");

                perfTimer.Start();
                using (StreamReader srBuffer3 = new StreamReader(@"./XMLData/GlossaryTermWithPronounciation.xml"))
                {
                    glossaryTermWithPronounciationXml.PreserveWhitespace = true;
                    glossaryTermWithPronounciationXml.LoadXml(srBuffer3.ReadToEnd());
                }

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                GlossaryTermDocument glossaryTerm = new GlossaryTermDocument();
                glossaryTerm.WarningWriter = warningWriter;
                glossaryTerm.InformationWriter = informationWriter;
                DocumentXPathManager xPathManager = new DocumentXPathManager();
                GlossaryTermExtractor extractor=new GlossaryTermExtractor();
                extractor.Extract(glossaryTermWithPronounciationXml, glossaryTerm, xPathManager);
  
                // Test Rendering
                GlossaryTermRenderer gtRender = new GlossaryTermRenderer();
                gtRender.Render(glossaryTerm);

                // Test query
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    if (GTQuery.SaveDocument(glossaryTerm, "ychen"))
                        Console.WriteLine("Saving Glossary Term complete.");
                }

                this.perfTimer.Stop();
                Console.WriteLine("Testing TestGlossaryTermPronounciation complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from glossary term document failed.", e);
            }
        }

        [Test]
        public void GlossaryTermSaveWithMediaLink304687()
        {
            // Set up
            try
            {
                XmlDocument glossaryTermWithMediaLinkXml = new XmlDocument();

                Console.WriteLine("========================================");

                perfTimer.Start();
                using (StreamReader srBuffer4 = new StreamReader(@"./XMLData/GlossaryTermWithMediaAndMutiLang.xml"))
                {
                    glossaryTermWithMediaLinkXml.PreserveWhitespace = true;
                    glossaryTermWithMediaLinkXml.LoadXml(srBuffer4.ReadToEnd());
                }

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                GlossaryTermDocument glossaryTerm = new GlossaryTermDocument();
                glossaryTerm.WarningWriter = warningWriter;
                glossaryTerm.InformationWriter = informationWriter;
                DocumentXPathManager xPathManager = new DocumentXPathManager();
                GlossaryTermExtractor extractor=new GlossaryTermExtractor();
                extractor.Extract(glossaryTermWithMediaLinkXml, glossaryTerm, xPathManager);

                // Test Rendering
                GlossaryTermRenderer gtRender = new GlossaryTermRenderer();
                gtRender.Render(glossaryTerm);

                // Test query
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    if (GTQuery.SaveDocument(glossaryTerm, "ychen"))
                        Console.WriteLine("Saving Glossary Term complete.");
                }

                this.perfTimer.Stop();
                Console.WriteLine("Testing TestGlossaryTermWithMediaLink complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from glossary term document failed.", e);
            }
        }

        [Test]
        public void GlossaryTermSaveWithMediaLink322891()
        {
            // Set up
            try
            {
                XmlDocument glossaryTermWithMediaLinkXml = new XmlDocument();

                Console.WriteLine("========================================");

                perfTimer.Start();
                using (StreamReader srBuffer4 = new StreamReader(@"./XMLData/GlossaryTermWithMedia.xml"))
                {
                    glossaryTermWithMediaLinkXml.PreserveWhitespace = true;
                    glossaryTermWithMediaLinkXml.LoadXml(srBuffer4.ReadToEnd());
                }

                HistoryEntryWriter warningWriter = new HistoryEntryWriter(Message);
                HistoryEntryWriter informationWriter = new HistoryEntryWriter(Message);
                GlossaryTermDocument glossaryTerm = new GlossaryTermDocument();
                glossaryTerm.WarningWriter = warningWriter;
                glossaryTerm.InformationWriter = informationWriter;
                DocumentXPathManager xPathManager = new DocumentXPathManager();
                GlossaryTermExtractor extractor= new GlossaryTermExtractor();
                extractor.Extract(glossaryTermWithMediaLinkXml, glossaryTerm, xPathManager);

                // Test Rendering
                GlossaryTermRenderer gtRender = new GlossaryTermRenderer();
                gtRender.Render(glossaryTerm);

                // Test query
                using (GlossaryTermQuery GTQuery = new GlossaryTermQuery())
                {
                    if (GTQuery.SaveDocument(glossaryTerm, "ychen"))
                        Console.WriteLine("Saving Glossary Term complete.");
                }

                this.perfTimer.Stop();
                Console.WriteLine("Testing TestGlossaryTermWithMediaLink complete; Duration = " + perfTimer.Duration);
                durationList.Add(perfTimer.Duration);
            }
            catch (NUnit.Framework.AssertionException assertEx)
            {
                Console.WriteLine("Assertion failed: " + assertEx.Message);
                throw assertEx;
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Saving data from glossary term document failed.", e);
            }
        }

       #endregion

        #region Private method
        private void Message(string s)
        { Console.WriteLine(s); }
        #endregion
    }
}
