using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;

namespace GKManagers.CMSManager.Configuration
{
    public class ConnectionInfo : ConfigurationElement
    {

        [ConfigurationProperty("protocol")]
        public ProtocolElement Protocol
        {
            get
            {
                return (ProtocolElement)base["protocol"];
            }
       }

        [ConfigurationProperty("host")]
        public HostElement Host
        {
            get
            {
                return (HostElement)base["host"];
            }
 
        }

        [ConfigurationProperty("port")]
        public PortElement Port
        {
            get
            {
                return (PortElement)base["port"];
            }
            
        }

        [ConfigurationProperty("username")]
        public UserNameElement UserName
        {
            get
            {
                return (UserNameElement)base["username"];
            }
            
        }

        [ConfigurationProperty("password")]
        public PasswordElement Password
        {
            get
            {
                return (PasswordElement)base["password"];
            }
            
        }

        [ConfigurationProperty("community")]
        public CommunityElement Community
        {
            get
            {
                return (CommunityElement)base["community"];
            }
            
        }

        [ConfigurationProperty("siteRootPath")]
        public SiteRootPathElement SiteRootPath
        {
            get
            {
                return (SiteRootPathElement)base["siteRootPath"];
            }
            
        }

        [ConfigurationProperty("timeout")]
        public TimeoutElement Timeout
        {
            get
            {
                return (TimeoutElement)base["timeout"];
            }

        }

    }
}
