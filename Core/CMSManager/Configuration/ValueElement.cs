using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;

namespace GKManagers.CMSManager.Configuration
{
    public class ValueElement : ConfigurationElement
    {
        [ConfigurationProperty("value")]
        public string Value
        {
            get
            {   
                return (string)base["value"];
            }
        }
    }
}
