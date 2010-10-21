using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
namespace NCI.WCM.CMSManager.CMS
{
    public class ContentItemForUpdating
    {
        //Constructor for updating  content Item
        public ContentItemForUpdating(long id,Dictionary<string,string> fields,string targetFolder)
        {
            ID = id;
            Fields = fields;
            TargetFolder = targetFolder;
        }

        public long ID {get;private set;}
        public Dictionary<string, string> Fields { get; private set; }
        public string TargetFolder { get; private set; }

    }
}
