using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCI.WCM.CMSManager.CMS
{
    public class ContentTypeToTemplateInfo
    {
        public string ContentTypeName { get; private set; }
        public PercussionGuid ContentTypeID { get; private set; }

        public string TemplateName { get; private set; }
        public PercussionGuid TemplateID { get; private set; }

        public ContentTypeToTemplateInfo(string contentTypeName, PercussionGuid contentTypeID,
                  string templateName, PercussionGuid templateID)
        {
            ContentTypeName = contentTypeName;
            ContentTypeID = contentTypeID;
            TemplateName=templateName;
            TemplateID = templateID;
        }
    }
}