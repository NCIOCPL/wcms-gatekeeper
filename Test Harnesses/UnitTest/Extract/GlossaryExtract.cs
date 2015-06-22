using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using GateKeeper.Common;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
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

        // Verify that GlossaryTermExtractor fails for everything except actual GlossaryTerm documents.
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

        [TestCase("GlossaryTerm\\Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument))]
        public void CorrectDocumentType(string filename, Type doctype)
        {
            RunExtract(filename, doctype);
        }

        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument))]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml", typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument))]
        [TestCase("Term-EnglishAndSpanish.xml", typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument))]
        public void LoadSingleEnglishPatientTerm(string filename, Type doctype)
        {
            RunExtract(filename, doctype);
            throw new NotImplementedException();
        }

        [TestCase("Term-MultiDefinition-English.xml", typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument))]
        public void LoadMultipleEnglishPatientTerm(string filename, Type doctype)
        {
            RunExtract(filename, doctype);
            throw new NotImplementedException();
        }

        [TestCase("Term-EnglishAndSpanish.xml", typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument))]
        public void LoadSingleSpanishPatientTerm(string filename, Type doctype)
        {
            RunExtract(filename, doctype);
            throw new NotImplementedException();
        }

        [TestCase("Term-SingleDefinition-English.xml", typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument))]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml", typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument))]
        public void FailToLoadSpanishPatientTerm(string filename, Type doctype)
        {
            RunExtract(filename, doctype);
            throw new NotImplementedException();
        }


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

    }
}
