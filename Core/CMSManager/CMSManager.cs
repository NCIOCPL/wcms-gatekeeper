using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using GateKeeper.DocumentObjects;
using GKManagers.CMS.PercussionWebSvc;
namespace GKManagers.CMS
{
    public class CMSManager
    {
        public void Store(GateKeeper.DocumentObjects.DrugInfoSummary.DrugInfoSummaryDocument DocType)
        {
            PercussionLoader percussionLoader = new PercussionLoader();
            try
            {
                //Login to Percussion.
                percussionLoader.Login();
                //Create Target Directory
                percussionLoader.CreateTargetFolder(DocType.PrettyURL);
                //Upload Content Item
                percussionLoader.UploadDrungInfoContentItem(GetFields(DocType));

                //Logout of percussion
                percussionLoader.Logout();
            }
            catch (SoapException ex)
            {

            }

            
        }

        private List<Dictionary<string, string>> GetFields(GateKeeper.DocumentObjects.DrugInfoSummary.DrugInfoSummaryDocument DocType)
        {
            List<Dictionary<string, string>> itemFields = new List<Dictionary<string, string>>();
            Dictionary<string, string> fields = new Dictionary<string, string>();
            fields.Add("sys_title", "5559");
            fields.Add("pretty_url_name", "learnblair");
            fields.Add("field2", "45599");
            itemFields.Add(fields);
            return itemFields;
        }

    }
}
