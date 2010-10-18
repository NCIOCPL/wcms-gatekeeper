using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
namespace GKManagers.CMSManager.CMS
{
    public class ContentItemForCreating
    {
        //constructor for creating new content item
        public ContentItemForCreating(Dictionary<string, string> fields, string targetFolder,string contentType)
        {            
            Fields = fields;
            TargetFolder = targetFolder;
            ContentType = contentType;
        }

        public Dictionary<string, string> Fields { get; private set; }
        public string TargetFolder { get; private set; }
        public string ContentType { get; private set; }
    }
}
