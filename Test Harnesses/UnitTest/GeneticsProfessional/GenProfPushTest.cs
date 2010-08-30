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
using GateKeeper.DocumentObjects.GeneticsProfessional;
using GateKeeper.ContentRendering;

namespace GateKeeper.UnitTest.GeneticsProfessional
{
    [TestFixture]
    public class GenProfPushTest
    {
        [Test]
        public void GeneticProfessionalPush249()
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
                    genProfQuery.PushDocumentToPreview(genProf, "ychen");
                    genProfQuery.PushDocumentToLive(genProf, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing data to preview database failed.", e);
            }
        }

        [Test]
        public void GeneticProfessionalPush355()
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
                    genProfQuery.PushDocumentToPreview(genProf, "ychen");
                    genProfQuery.PushDocumentToLive(genProf, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing data to preview database failed.", e);
            }
        }

        [Test]
        public void GeneticProfessionalPush173()
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
                    genProfQuery.PushDocumentToPreview(genProf, "ychen");
                    genProfQuery.PushDocumentToLive(genProf, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing data to preview database failed.", e);
            }
        }

        [Test]
        public void GeneticProfessionalPush007()
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
                    genProfQuery.PushDocumentToPreview(genProf, "ychen");
                    genProfQuery.PushDocumentToLive(genProf, "ychen");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Testing Error: Pushing data to preview database failed.", e);
            }
        }
    }
}
