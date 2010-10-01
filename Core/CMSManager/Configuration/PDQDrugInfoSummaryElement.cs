using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;

namespace GKManagers.CMSManager.Configuration
{
    public class PDQDrugInfoSummaryElement : ConfigurationElement
    {
        [ConfigurationProperty("value",DefaultValue="pdqDrugInfoSummary", IsRequired = false)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
        }
    }
}
