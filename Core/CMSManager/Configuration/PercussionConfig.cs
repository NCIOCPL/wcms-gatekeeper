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
        // Create a "font" element.
        [ConfigurationProperty("protocol")]
        public ProtocolElement Protocol
        {
            get
            {
                return (ProtocolElement)this["protocol"];
            }
            set
            { this["protocol"] = value; }
        }

        [ConfigurationProperty("host")]
        public HostElement Host
        {
            get
            {
                return (HostElement)this["host"];
            }
            set
            { this["host"] = value; }
        }

        [ConfigurationProperty("port")]
        public PortElement Port
        {
            get
            {
                return (PortElement)this["port"];
            }
            set
            { this["port"] = value; }
        }

        [ConfigurationProperty("username")]
        public UserNameElement UserName
        {
            get
            {
                return (UserNameElement)this["username"];
            }
            set
            { this["username"] = value; }
        }
        
        [ConfigurationProperty("password")]
        public PasswordElement Password
        {
            get
            {
                return (PasswordElement)this["password"];
            }
            set
            { this["password"] = value; }
        }

        [ConfigurationProperty("community")]
        public CommunityElement Community
        {
            get
            {
                return (CommunityElement)this["community"];
            }
            set
            { this["community"] = value; }
        }

        [ConfigurationProperty("appendtargetfolder")]
        public AppendTargetFolderElement AppendTargetFolder
        {
            get
            {
                return (AppendTargetFolderElement)this["appendtargetfolder"];
            }
            set
            { this["appendtargetfolder"] = value; }
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


    public class AppendTargetFolderElement : ConfigurationElement
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
}
