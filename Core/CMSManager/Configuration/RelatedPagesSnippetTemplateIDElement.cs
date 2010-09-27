using System;
using System.Configuration;

namespace GKManagers.CMSManager.Configuration
{
    /// <summary>
    /// Implements the RelatedPagesSnippetTemplateID element specifying the
    /// snippet template to use for the related pages box.
    /// </summary>
    public class RelatedPagesSnippetTemplateIDElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get { return (String)this["value"]; }
            set { this["value"] = value; }
        }
    }
}
