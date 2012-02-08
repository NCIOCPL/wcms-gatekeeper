using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects.PoliticalSubUnit;
using GateKeeper.DataAccess.CancerGov;

namespace GateKeeper.UnitTest.StorageIntegrated.PoliticalSubUnit
{
    [TestFixture, Explicit]
    public class PoliticalSubUnitPushTest
    {
        

        #region Unit Tests

 
        [Test]
        public void PoliticialSubUnitPush43856()
        {
            // Set up
            try
            {
                int documentID = 43856;
                PoliticalSubUnitDocument po = new PoliticalSubUnitDocument();
                po.DocumentID = documentID;
                // Test saving
                using (PoliticalSubUnitQuery polQuery = new PoliticalSubUnitQuery())
                {
                    polQuery.PushDocumentToPreview(po, "ychen");
                    polQuery.PushDocumentToLive(po, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing PoliticialSubUnit data to database failed.", e);
            }

        }

        [Test]
        public void PoliticialSubUnitPush43870()
        {
            // Set up
            try
            {
                int documentID = 43870;
                // Test saving
                PoliticalSubUnitDocument po = new PoliticalSubUnitDocument();
                po.DocumentID = documentID;
                using (PoliticalSubUnitQuery polQuery = new PoliticalSubUnitQuery())
                {
                    polQuery.PushDocumentToPreview(po, "ychen");
                    polQuery.PushDocumentToLive(po, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing PoliticialSubUnit data to database failed.", e);
            }

        }

        [Test]
        public void PoliticialSubUnitPush43920()
        {
            // Set up
            try
            {
                int documentID = 43920;
                // Test saving
                PoliticalSubUnitDocument po = new PoliticalSubUnitDocument();
                po.DocumentID = documentID;
                using (PoliticalSubUnitQuery polQuery = new PoliticalSubUnitQuery())
                {
                    polQuery.PushDocumentToPreview(po, "ychen");
                    polQuery.PushDocumentToLive(po, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing PoliticialSubUnit data to database failed.", e);
            }

        }

        #endregion
    }
}
