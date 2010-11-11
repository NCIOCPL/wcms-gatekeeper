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
        public ContentItemForUpdating(long id, Dictionary<string, string> fields)
        {
            ID = id;
            Fields = fields;
        }

        public long ID {get;private set;}
        public Dictionary<string, string> Fields { get; private set; }
    }
}
