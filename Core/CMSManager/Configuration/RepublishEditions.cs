using System;
using System.Collections.Generic;
using System.Configuration;

namespace GKManagers.CMSManager.Configuration
{
    public class RepublishEditionListElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get { return (String)this["value"]; }
        }
    }
}