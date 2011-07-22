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

namespace GateKeeper.UnitTest.GlossaryTerm
{
    [TestFixture]
    public class GTRelInfoTest
    {
        [TestCase("GT46332.xml", "PromoteToStaging")]
        [TestCase("GT46583.xml", "PromoteToStaging")]
        [TestCase("GT512821.xml", "PromoteToStaging")] 
        [TestCase("GT572363.xml", "PromoteToStaging")]
        [TestCase("GT663502.xml", "PromoteToStaging")]
        public void ToStaging(string requestDataXMLFile, string promotionAction)
        {
            GateKeeper.UnitTest.Common.TesterHelper.promote(requestDataXMLFile, promotionAction );
        }

        [TestCase("GT46332.xml", "PromoteToPreview")]
        [TestCase("GT46583.xml", "PromoteToPreview")]
        [TestCase("GT512821.xml", "PromoteToPreview")]
        [TestCase("GT572363.xml", "PromoteToPreview")]
        [TestCase("GT663502.xml", "PromoteToPreview")]
        public void ToPreview(string requestDataXMLFile, string promotionAction)
        {
            GateKeeper.UnitTest.Common.TesterHelper.promote(requestDataXMLFile, promotionAction);
        }

        [TestCase("GT46332.xml", "PromoteToLive")]
        [TestCase("GT46583.xml", "PromoteToLive")]
        [TestCase("GT512821.xml", "PromoteToLive")]
        [TestCase("GT572363.xml", "PromoteToLive")]
        [TestCase("GT663502.xml", "PromoteToLive")]
        public void ToLive(string requestDataXMLFile, string promotionAction)
        {
            GateKeeper.UnitTest.Common.TesterHelper.promote(requestDataXMLFile, promotionAction);
        }

    }
}
