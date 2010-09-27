using System;
using System.Configuration;

namespace GKManagers.CMSManager.Configuration
{
    /// <summary>
    /// Implements the RelatedPagesSlotIDElement which specifies the
    /// slot ID to use for the related pages box.
    /// </summary>
    public class RelatedPagesSlotIDElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get { return (String)this["value"]; }
            set { this["value"] = value; }
        }
    }
}
