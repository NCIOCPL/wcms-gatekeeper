using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace NCI.Web.Dictionary.BusinessObjects
{
    //This particular class needs Data Contract since the data from the database has different names than these
    [DataContract()]
    public class ImageReference
    {
        /// <summary>
        /// Filename attribute
        /// </summary>
        [DataMember(Name = "ref")]
        public string Filename { get; set; }

        /// <summary>
        /// The Alt Text
        /// </summary>
        [DataMember(Name = "alt")]
        public string AltText { get; set; }


        /// <summary>
        /// Caption text
        /// </summary>
        [DataMember(Name = "caption")]
        public string Caption { get; set; }
    }
}