using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using NUnit.Framework;
using GateKeeper.Common;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GeneticsProfessional;
using GateKeeper.ContentRendering;

namespace GateKeeper.UnitTest.GeneticsProfessional
{
    [TestFixture]
    public class GenProfDeleteTest
    {
        [Test]
        public void GeneticProfessionalDelete249()
        {
            // Set up
            try
            {
                int documentID = 30010249;
                // Test saving
                GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
                genProf.DocumentID = documentID;
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Staging, "ychen");
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Preview, "ychen");
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing data to preview database failed.", e);
            }
        }

        [Test]
        public void GeneticProfessionalDelete355()
        {
            // Set up
            try
            {
                int documentID = 30010355;
                // Test saving
                GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
                genProf.DocumentID = documentID;
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Staging, "ychen");
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Preview, "ychen");
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing data to preview database failed.", e);
            }
        }

        [Test]
        public void GeneticProfessionalDelete173()
        {
            // Set up
            try
            {
                int documentID = 30010173;
                // Test saving
                GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
                genProf.DocumentID = documentID;

                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Staging, "ychen");
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Preview, "ychen");
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing data to preview database failed.", e);
            }
        }

        [Test]
        public void GeneticProfessionalDelete007()
        {
            // Set up
            try
            {
                int documentID = 30010007;
                // Test saving
                GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
                genProf.DocumentID = documentID;
                using (GenProfQuery genProfQuery = new GenProfQuery())
                {
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Staging, "ychen");
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Preview, "ychen");
                    genProfQuery.DeleteDocument(genProf, ContentDatabase.Live, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Deleteing data to preview database failed.", e);
            }
        }
    }
}
