using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects.PoliticalSubUnit;
using GateKeeper.DataAccess.CancerGov;

namespace GateKeeper.UnitTest.StorageIntegrated.PoliticalSubUnit
{
    [TestFixture, Explicit]
    public class PoliticalSubUnitDeleteTest
    {
        #region Unit Tests
        [Test]
        public void PoliticialSubUnitDelete43856()
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
                    polQuery.DeleteDocument(po, ContentDatabase.Staging,  "ychen");
                    polQuery.DeleteDocument(po, ContentDatabase.Preview, "ychen");
                    polQuery.DeleteDocument(po, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing PoliticialSubUnit data from database failed.", e);
            }

        }

        [Test]
        public void PoliticialSubUnitDelete43870()
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
                    polQuery.DeleteDocument(po, ContentDatabase.Staging, "ychen");
                    polQuery.DeleteDocument(po, ContentDatabase.Preview, "ychen");
                    polQuery.DeleteDocument(po, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing PoliticialSubUnit data from database failed.", e);
            }

        }

        [Test]
        public void PoliticialSubUnitDelete43920()
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
                    polQuery.DeleteDocument(po, ContentDatabase.Staging, "ychen");
                    polQuery.DeleteDocument(po, ContentDatabase.Preview, "ychen");
                    polQuery.DeleteDocument(po, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing PoliticialSubUnit data from database failed.", e);
            }

        }

        #endregion
    }
}
