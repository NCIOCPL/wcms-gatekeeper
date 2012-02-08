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

namespace GateKeeper.UnitTest.ModularProcessing
{
    [TestFixture]
    public class SummaryExtract
    {
        #region Reusable pieces
        DocumentXPathManager _xPathManager;
        HistoryEntryWriter _informationWriter = delegate(string message) { Console.Write(message); };
        HistoryEntryWriter _warningWriter = delegate(string message) { Console.Write(message); };

        [TestFixtureSetUp()]
        public void Init()
        {
            _xPathManager =  new DocumentXPathManager();
        }

        #endregion

        [ExpectedException(typeof(DocumentTypeMismatchException))]
        [TestCase("Summary-BreastPatient-62955.xml", typeof(GateKeeper.DocumentObjects.DrugInfoSummary.DrugInfoSummaryDocument))]
        [TestCase("Summary-BreastPatient-62955.xml", typeof(GateKeeper.DocumentObjects.GeneticsProfessional.GeneticsProfessionalDocument))]
        [TestCase("Summary-BreastPatient-62955.xml", typeof(GateKeeper.DocumentObjects.GlossaryTerm.GlossaryTermDocument))]
        [TestCase("Summary-BreastPatient-62955.xml", typeof(GateKeeper.DocumentObjects.Media.MediaDocument))]
        [TestCase("Summary-BreastPatient-62955.xml", typeof(GateKeeper.DocumentObjects.Organization.OrganizationDocument))]
        [TestCase("Summary-BreastPatient-62955.xml", typeof(GateKeeper.DocumentObjects.PoliticalSubUnit.PoliticalSubUnitDocument))]
        [TestCase("Summary-BreastPatient-62955.xml", typeof(GateKeeper.DocumentObjects.Protocol.ProtocolDocument))]
        [TestCase("Summary-BreastPatient-62955.xml", typeof(GateKeeper.DocumentObjects.Terminology.TerminologyDocument))]
        public void WrongDocumentType(string filename, Type doctype)
        {
            RunExtract(filename, doctype);
        }

        [TestCase("Summary-BreastPatient-62955.xml", typeof(GateKeeper.DocumentObjects.Summary.SummaryDocument))]
        public void CorrectDocumentType(string filename, Type doctype)
        {
            RunExtract(filename, doctype);
        }

        private void RunExtract(string filename, Type doctype)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(@"./XMLData/" + filename);

            // This is how you construct an object when you don't know its
            // actual type until runtime.
            ConstructorInfo constructorInfo = doctype.GetConstructor(new Type[] { });
            Document document = (Document)constructorInfo.Invoke(null);

            document.InformationWriter = _informationWriter;
            document.WarningWriter = _warningWriter;
            SummaryExtractor extractor = new SummaryExtractor();

            extractor.Extract(xml, document, _xPathManager, TargetedDevice.screen);
        }


    }
}
