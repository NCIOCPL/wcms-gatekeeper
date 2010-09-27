using System;
using System.Configuration;

namespace GKManagers.CMSManager.Configuration
{
    /// <summary>
    /// Implements the RelatedPagesContentID element identifying the
    /// snippet template to use for the related pages box.
    /// </summary>
    public class RelatedPagesContentIDElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get { return (String)this["value"]; }
            set { this["value"] = value; }
        }
    }
}
