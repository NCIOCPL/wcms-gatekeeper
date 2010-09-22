using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
namespace GKManagers.CMSManager.CMS
{
    public class CreateContentItem
    {
        //constructor for creating new content item
        public CreateContentItem(Dictionary<string, string> fields, string targetFolder)
        {            
            Fields = fields;
            TargetFolder = targetFolder;
        }

        public Dictionary<string, string> Fields { get; private set; }
        public string TargetFolder { get; private set; }

    }
}
