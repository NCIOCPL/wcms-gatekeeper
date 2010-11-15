using System;
using System.Collections.Generic;
using System.Linq;

namespace NCI.WCM.CMSManager.CMS
{
    // TODO: Merge ContentItemForCreating and ContentItemForUpdating into a single type.
    // Derive specific update types with the content type names.
    // Having separate types for create vs. update was a bad call on my part.
    public class ContentItemForCreating
    {
        //constructor for creating new content item
        public ContentItemForCreating(string contentType, FieldSet fields, string targetFolder)
        {
            ContentType = contentType;
            TargetFolder = targetFolder;
            Fields = fields;
        }

        public string ContentType { get; private set; }
        public string TargetFolder { get; private set; }

        public FieldSet Fields { get; private set; }


        List<ChildFieldSet> _childFieldList;
        public List<ChildFieldSet> ChildFieldList
        {
            get
            {
                if (_childFieldList == null)
                    _childFieldList = new List<ChildFieldSet>();
                return _childFieldList;
            }
        }
    }
}