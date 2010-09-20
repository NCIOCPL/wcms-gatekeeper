using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using GateKeeper.DocumentObjects;
using GKManagers.CMS.PercussionWebSvc;
using GKManagers.CMS.Configuration;
using System.Configuration;
using System.Collections;
namespace GKManagers.CMS
{
    public class CMSManager
    {
        public void Store(GateKeeper.DocumentObjects.DrugInfoSummary.DrugInfoSummaryDocument DocType)
        {
            Percussion percussionLoader = new Percussion();
            try
            {
                GKManagers.CMS.Configuration.PercussionConfig cfg = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig/connectionInfo");
                //Login to Percussion.
                percussionLoader.Login();
                //Create Target Directory
                percussionLoader.CreateTargetFolder(DocType.PrettyURL);
                //Upload Content Item
                percussionLoader.UploadContentItem(GetFields(DocType));

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
            fields.Add("sys_title", "1");
            fields.Add("pretty_url_name", "first");
            fields.Add("long_title", "firstLong");
            fields.Add("short_title", "FirstShort");
            fields.Add("cdrid", "1");

            itemFields.Add(fields);
            return itemFields;
        }

    }
}
