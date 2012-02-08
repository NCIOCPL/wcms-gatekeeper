using System.Configuration;

namespace GKManagers.Configuration
{
    /// <summary>
    /// Reusable class for any named configuration element with a single value.
    /// </summary>
    public class NamedSingleValueElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }
    }
}
