using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;

namespace GKManagers.CMSManager.Configuration
{
    public class PercussionConfig : ConfigurationSection
    {
        [ConfigurationProperty("connectionInfo")]
        public ConnectionInfo ConnectionInfo
        {
            get
            {
                return (ConnectionInfo)base["connectionInfo"];
            }

        }

        [ConfigurationProperty("contentTypes", IsRequired = false)]
        public ContentType ContentType
        {
            get
            {
                return (ContentType)base["contentTypes"];
            }

        }

        [ConfigurationProperty("previewRepublishEditionList", IsRequired = true)]
        public RepublishEditionListElement PreviewRepublishEditionList
        {
            get { return (RepublishEditionListElement)base["previewRepublishEditionList"]; }
        }

        [ConfigurationProperty("liveRepublishEditionList", IsRequired = true)]
        public RepublishEditionListElement LiveRepublishEditionList
        {
            get { return (RepublishEditionListElement)base["liveRepublishEditionList"]; }
        }

        [ConfigurationProperty("navonPublicTransitionName")]
        public NavonPublicTransitionElement NavonPublicTransition
        {
            get { return (NavonPublicTransitionElement)base["navonPublicTransitionName"]; }
        }
    }

    public class ProtocolElement : ConfigurationElement
    {
        [ConfigurationProperty("value", DefaultValue = "http", IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

    }


    public class HostElement : ConfigurationElement
    {
        [ConfigurationProperty("value", DefaultValue = "http", IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

    }


    public class PortElement : ConfigurationElement
    {
        [ConfigurationProperty("value",IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

    }

    public class UserNameElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

    }

    public class PasswordElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

    }


    public class CommunityElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

    }


    public class SiteRootPathElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

    }

    public class TimeoutElement : ConfigurationElement
    {
        [ConfigurationProperty("value", DefaultValue = 100000, IsRequired = false)]
        public int Value
        {
            get
            {
                return (int)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

    }

}
