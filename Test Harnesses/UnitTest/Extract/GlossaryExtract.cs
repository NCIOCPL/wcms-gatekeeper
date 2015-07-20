using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using GateKeeper.Common;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Dictionary;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GKManagers.BusinessObjects;

namespace GateKeeper.UnitTest.Extract
{
    [TestFixture]
    public class GlossaryExtract
    {
        #region Reusable pieces
        HistoryEntryWriter _informationWriter = delegate(string message) { Console.Write(message); };
        HistoryEntryWriter _warningWriter = delegate(string message) { Console.Write(message); };

        [TestFixtureSetUp()]
        public void Init()
        {
        }

        #endregion

        /// <summary>
        /// Verify that GlossaryTermExtractor fails for everything except actual GlossaryTerm documents.
        /// </summary>
        /// <param name="filename">Data file to load</param>
        /// <param name="doctype">Document type to try extracting.</param>
        [ExpectedException(typeof(DocumentTypeMismatchException))]
        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.DrugInfoSummary.DrugInfoSummaryDocument))]
        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.GeneticsProfessional.GeneticsProfessionalDocument))]
        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.Media.MediaDocument))]
        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.Organization.OrganizationDocument))]
        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.PoliticalSubUnit.PoliticalSubUnitDocument))]
        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.Protocol.ProtocolDocument))]
        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.Terminology.TerminologyDocument))]
        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.Summary.SummaryDocument))]
        public void WrongDocumentType(string filename, Type doctype)
        {
            RunExtract(filename, doctype);
        }

        /// <summary>
        /// Verify that GlossaryTermExtractor is able to load GlossaryTerm documents.
        /// </summary>
        /// <param name="filename">GlossaryTerm data file to load</param>
        [TestCase("Term-SingleDefinition-English.xml")]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml")]
        [TestCase("Term-MultiDefinition-English.xml")]
        [TestCase("Term-EnglishAndSpanish.xml")]
        public void CorrectDocumentType(string filename)
        {
            RunExtract(filename);
        }

        /// <summary>
        /// Verify that documents are extracted with the correct number of dictionary entries per-language.
        /// </summary>
        /// <param name="filename">GlossaryTerm data file to load</param>
        /// <param name="targetLanguage">The language to check for</param>
        /// <param name="expectedCount">The number of documents expected in the targeted language.</param>
        [TestCase("Term-SingleDefinition-English.xml", Language.English, 1)]
        [TestCase("Term-SingleDefinition-English.xml", Language.Spanish, 0)]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml", Language.English, 1)]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml", Language.Spanish, 0)]
        [TestCase("Term-EnglishAndSpanish.xml", Language.English, 1)]
        [TestCase("Term-EnglishAndSpanish.xml", Language.Spanish, 1)]
        [TestCase("Term-MultiDefinition-English.xml", Language.English, 2)]
        [TestCase("Term-MultiDefinition-English.xml", Language.Spanish, 0)]
        [TestCase("Term-MultiDefinition-Eng-and-Spanish.xml", Language.English, 2)]
        [TestCase("Term-MultiDefinition-Eng-and-Spanish.xml", Language.Spanish, 2)]
        public void DictionaryEntryCount(string filename, Language targetLanguage, int expectedCount)
        {
            GlossaryTermDocument document = new GlossaryTermDocument();

            RunExtract(filename, document);

            // Find the number of dictionary entries targeting English.
            int count = document.Dictionary.Count(entry => { return entry.Language == targetLanguage; });
            Assert.AreEqual(expectedCount, count, "Number of dictionary entries..");
        }


        /// <summary>
        /// Shared extract code for tests which only require that extract not throw exceptions.
        /// </summary>
        /// <param name="filename">XML data file containing a PDQ document.</param>
        private void RunExtract(string filename)
        {
            RunExtract(filename, typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument));
        }

        /// <summary>
        /// Shared extract code for tests which only require that extract not throw exceptions.
        /// </summary>
        /// <param name="filename">XML data file containing a PDQ document.</param>
        /// <param name="doctype">The document type to create.</param>
        private void RunExtract(string filename, Type doctype)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(@"./XMLData/GlossaryTerm/" + filename);

            // This is how you construct an object when you don't know its
            // actual type until runtime.
            ConstructorInfo constructorInfo = doctype.GetConstructor(new Type[] { });
            Document document = (Document)constructorInfo.Invoke(null);

            document.InformationWriter = _informationWriter;
            document.WarningWriter = _warningWriter;
            GlossaryTermExtractor extractor = new GlossaryTermExtractor();

            // GlossaryTermExtractor doesn't use the DocumentXPathManager argument.
            // The argument only exists for backwards compatability with a generic signature.
            extractor.Extract(xml, document, null);
        }

        /// <summary>
        /// Shared extract code for tests which use the GlossaryTermDocument object.
        /// </summary>
        /// <param name="filename">XML data file containing a PDQ document.</param>
        /// <param name="document">An instantiated GlossaryTermDocument.</param>
        private void RunExtract(string filename, GlossaryTermDocument document)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(@"./XMLData/GlossaryTerm/" + filename);

            document.InformationWriter = _informationWriter;
            document.WarningWriter = _warningWriter;
            GlossaryTermExtractor extractor = new GlossaryTermExtractor();

            // GlossaryTermExtractor doesn't use the DocumentXPathManager argument.
            // The argument only exists for backwards compatability with a generic signature.
            extractor.Extract(xml, document, null);
        }
    }
}
