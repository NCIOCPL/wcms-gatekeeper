﻿using System;
using System.Runtime.Serialization;

namespace CDRPreviewWS.GlossaryTerm.BusinessObjects
{
    [DataContract()]
    public class Alias
    {
        /// <summary>
        /// The other name
        /// </summary>
        [DataMember(Name = "name")]
        public String Name { get; set; }

        /// <summary>
        /// The type other name
        /// </summary>
        [DataMember(Name = "type")]
        public String Type { get; set; }
    }
}