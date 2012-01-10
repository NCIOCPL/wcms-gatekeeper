using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using NUnit.Framework;
using PubPreviewTest.PubPreviewWS;
using GateKeeper.DocumentObjects;


namespace PubPreviewTest
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
            pubPreviewWs.Url = "http://gatekeeperred.cancer.gov/CDRPreviewWS/CDRPreview.asmx";
            string html = pubPreviewWs.ReturnXML((XmlNode)xmlDoc, PreviewTypes.Summary.ToString());
            html += "";
            Assert.IsFalse(html.Contains("CDRPreview web service error"));
        }
    }
}
