using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;

namespace GKManagers.CMSManager.Configuration
{
    public class PDQPermanentLinkElement : ConfigurationElement
    {
        [ConfigurationProperty("value", DefaultValue = "pdqPermanentLink", IsRequired = false)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
        }
    }
}
