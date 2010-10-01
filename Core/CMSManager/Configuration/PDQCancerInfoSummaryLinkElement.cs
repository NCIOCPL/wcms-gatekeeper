﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;
namespace GKManagers.CMSManager.Configuration
{
    public class PDQCancerInfoSummaryLinkElement : ConfigurationElement
    {
        [ConfigurationProperty("value", DefaultValue = "pdqCancerInfoSummaryLink", IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
        }
    }
}
