using System;
using System.Collections.Generic;
using System.Configuration;


namespace GKManagers.CMSManager.Configuration
{
    public class TransactionChunkSizeElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = false, DefaultValue = 25)]
        public int Value
        {
            get { return (int)this["value"]; }
        }
    }
}
