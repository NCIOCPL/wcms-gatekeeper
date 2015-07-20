using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GlossaryTerm;

namespace GateKeeper.UnitTest.DocumentObjects.GlossaryTerm
{
    [TestFixture]
    class GlossaryTermMetadataExtract
    {
        /// <summary>
        /// Verify that the english term name deserializes.
        /// </summary>
        /// <param name="filename">GlossaryTerm data file to load</param>
        [TestCase("Term-SingleDefinition-English.xml", 45693)]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml", 45693)]
        [TestCase("Term-EnglishAndSpanish.xml", 45693)]
        [TestCase("Term-MultiDefinition-English.xml", 45693)]
        public void DeserializeTermID(string filename, int expectedTermID)
        {
            GlossaryTermMetadata data = Deserilaize(filename);
            Assert.AreEqual(data.ID, expectedTermID);
        }


        /// <summary>
        /// Verify that the english term name deserializes.
        /// </summary>
        /// <param name="filename">GlossaryTerm data file to load</param>
        [TestCase("Term-SingleDefinition-English.xml")]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml")]
        [TestCase("Term-EnglishAndSpanish.xml")]
        public void DeserializeEnglishTermName(string filename)
        {
            GlossaryTermMetadata data = Deserilaize(filename);
            Assert.IsNotNullOrEmpty(data.EnglishTermName, "extractData.EnglishTermName");
            Assert.AreEqual(data.EnglishTermName, "Definition");
        }

        /// <summary>
        /// Verify that the spanish term name deserializes.
        /// </summary>
        /// <param name="filename">GlossaryTerm data file to load</param>
        [TestCase("Term-EnglishAndSpanish.xml")]
        public void DeserializeSpanishTermName(string filename)
        {
            GlossaryTermMetadata data = Deserilaize(filename);
            Assert.IsNotNullOrEmpty(data.SpanishTermName);
            Assert.AreEqual(data.SpanishTermName, "Definición");
        }

        /// <summary>
        /// Verify that Extract works correctly for documents which contain more than one english definition.
        /// </summary>
        /// <param name="filename">GlossaryTerm data file to load</param>
        [TestCase("Term-MultiDefinition-English.xml")]
        public void DeserializeMultipleEnglishPatientTerm(string filename)
        {
            GlossaryTermMetadata data = Deserilaize(filename);

            int count = data.DefinitionList.Count(definition => { return definition.Language == Language.English; });
            Assert.Greater(count, 1, "Number of English definitions greater than one.");
        }


        /// <summary>
        /// Verify that Extract works correctly for documents which contain exactly one english definition.
        /// </summary>
        /// <param name="filename">GlossaryTerm data file to load</param>
        [TestCase("Term-SingleDefinition-English.xml")]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml")]
        [TestCase("Term-EnglishAndSpanish.xml")]
        public void DeserializeSingleEnglishPatientDefinition(string filename)
        {
            GlossaryTermMetadata data = Deserilaize(filename);
            int count = data.DefinitionList.Count(definition => { return definition.Language == Language.English; });
            Assert.AreEqual(count, 1, "Number of English definitions is exactly one.");
        }

        /// <summary>
        /// Verify that Extract works correctly for documents which contain exactly one spanish definition.
        /// </summary>
        /// <param name="filename">GlossaryTerm data file to load</param>
        [TestCase("Term-EnglishAndSpanish.xml")]
        public void DeserializeSingleSpanishPatientDefinition(string filename)
        {
            GlossaryTermMetadata data = Deserilaize(filename);
            int count = data.DefinitionList.Count(definition => { return definition.Language == Language.Spanish; });
            Assert.AreEqual(count, 1, "Number of Spanish definitions is exactly one.");
        }

        /// <summary>
        /// GlossaryTerm XML must always contain at least one English definition.  Verify that at least one is always found.
        /// </summary>
        /// <param name="filename"></param>
        [TestCase("Term-SingleDefinition-English.xml")]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml")]
        [TestCase("Term-EnglishAndSpanish.xml")]
        [TestCase("Term-MultiDefinition-English.xml")]
        public void AlwaysDeserializeEnglishDefinition(string filename)
        {
            GlossaryTermMetadata data = Deserilaize(filename);
            int count = data.DefinitionList.Count(definition => { return definition.Language == Language.English; });
            Assert.Greater(count, 0, "No English definitions.");
        }

        /// <summary>
        /// Test that no Spanish definitions are found when the source document
        /// doesn't contain any.
        /// </summary>
        /// <param name="filename"></param>
        [TestCase("Term-SingleDefinition-English.xml")]
        [TestCase("Term-SingleDefinition-NoPronunciation-English.xml")]
        public void FailToDeserializeSpanishPatientDefinition(string filename)
        {
            GlossaryTermMetadata data = Deserilaize(filename);
            int count = data.DefinitionList.Count(definition => { return definition.Language == Language.Spanish; });
            Assert.AreEqual(count, 0, "Non-existant Spanish definitions.");
        }

        /// <summary>
        /// Helper method to do the actual deserialization we're testing 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private GlossaryTermMetadata Deserilaize(string filename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@"./XMLData/GlossaryTerm/" + filename);

            XmlSerializer serializer = new XmlSerializer(typeof(GlossaryTermMetadata));
                GlossaryTermMetadata extractData;
            using (TextReader reader = new StringReader(xmlDoc.OuterXml))
            {
                extractData = (GlossaryTermMetadata)serializer.Deserialize(reader);
            }

            return extractData;
        }
    }
}
