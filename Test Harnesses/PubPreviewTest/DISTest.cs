using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using NUnit.Framework;
using PubPreviewTest.PubPreviewWS;
using GateKeeper.DocumentObjects;
namespace GateKeeper.UnitTest.DISTest
{
    [TestFixture]
    public class DISTest
    {
        [TestCase("DrugInfoSummary487488.xml")]
        public void callPubPreview(string requestDataXMLFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(@"./XMLData/" + requestDataXMLFile);
            CDRPreview pubPreviewWs = new CDRPreview();
            pubPreviewWs.Url = "http://gatekeeperca.cancer.gov/CDRPreviewWS/CDRPreview.asmx";
            string html = pubPreviewWs.ReturnXML((XmlNode)xmlDoc, PreviewTypes.DrugInfoSummary.ToString());
            html += "";
        }
    }
}
