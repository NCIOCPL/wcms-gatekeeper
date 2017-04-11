using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CDRPreviewWS.GlossaryTerm.BusinessObjects
{
    /// <summary>
    /// Contains the metadata representing a video.
    /// </summary>
    [DataContract()]
    public class VideoReference
    {
        [DataMember(Name = "unique_id")]
        public string UniqueID {get;set;}

        [DataMember(Name = "template")]
        public string Template { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "caption")]
        public string Caption { get; set; }
    }
}
