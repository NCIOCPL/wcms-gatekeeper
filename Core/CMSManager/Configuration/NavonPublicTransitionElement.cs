﻿using System;
using System.Collections.Generic;
using System.Configuration;

namespace GKManagers.CMSManager.Configuration
{
    public class NavonPublicTransitionElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = false, DefaultValue = "DirecttoPublic")]
        public String Value
        {
            get { return (String)this["value"]; }
        }
    }
}
