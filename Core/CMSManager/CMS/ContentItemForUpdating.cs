using System;
using System.Collections.Generic;
using System.Linq;

namespace NCI.WCM.CMSManager.CMS
{
    // TODO: Merge ContentItemForCreating and ContentItemForUpdating into a single type.
    // See notes in ContentItemForCreating.
    public class ContentItemForUpdating
    {
        //Constructor for updating  content Item
        public ContentItemForUpdating(long id, FieldSet fields)
        {
            ID = id;
            Fields = fields;
        }

        public long ID {get;private set;}
        public FieldSet Fields { get; private set; }
    }
}
