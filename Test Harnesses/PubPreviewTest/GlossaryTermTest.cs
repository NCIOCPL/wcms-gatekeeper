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
    public class GTRelInfoTest
    {
        [TestCase("GT46332.xml")]
        //[TestCase("GT46583.xml", "PromoteToStaging")]
        //[TestCase("GT512821.xml", "PromoteToStaging")]
        //[TestCase("GT572363.xml", "PromoteToStaging")]
        //[TestCase("GT663502.xml", "PromoteToStaging")]
        public void callPubPreview(string requestDataXMLFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(@"./XMLData/" + requestDataXMLFile);
            CDRPreview pubPreviewWs = new CDRPreview();
            string html = pubPreviewWs.ReturnXML((XmlNode)xmlDoc, PreviewTypes.GlossaryTerm.ToString());
            html += "";
        }
    }
}
