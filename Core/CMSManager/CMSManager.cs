using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GateKeeper.DocumentObjects;
using GKManagers.CMS.PercussionWebSvc;
namespace GKManagers.CMS
{
    public class CMSManager
    {
        public void Store(GateKeeper.DocumentObjects.DrugInfoSummary.DrugInfoSummaryDocument DocType)
        {
            PercussionLoader percussionLoader = new PercussionLoader();
            
            //Login to Percussion.
            percussionLoader.Login();
            //Create Target Directory


            //Logout of percussion
            percussionLoader.Logout();
            
        }

 

    }
}
