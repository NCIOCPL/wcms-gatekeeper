using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using NUnit.Framework;
using PubPreviewTest.PubPreviewWS;
using GateKeeper.DocumentObjects;
namespace GateKeeper.UnitTest.GlossaryTerm
{
    [TestFixture]
    public class SummaryTest
    {
        [TestCase("Summary62872.xml")]
        public void callPubPreview(string requestDataXMLFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(@"./XMLData/" + requestDataXMLFile);
            CDRPreview pubPreviewWs = new CDRPreview();
            string html = pubPreviewWs.ReturnXML((XmlNode)xmlDoc, PreviewTypes.Summary.ToString());
            html += "";
        }
    }
}
