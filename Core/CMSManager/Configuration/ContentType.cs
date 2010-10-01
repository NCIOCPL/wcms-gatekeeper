using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;

namespace GKManagers.CMSManager.Configuration
{
    public class ContentType : ConfigurationElement
    {
        [ConfigurationProperty("pdqDrugInfoSummary")]
        public ValueElement PDQDrugInfoSummary
        {
            get
            {
                return (ValueElement)base["pdqDrugInfoSummary"];
            }

        }

        [ConfigurationProperty("pdqCancerInfoSummary")]
        public ValueElement PDQCancerInfoSummary
        {
            get
            {
                return (ValueElement)base["pdqCancerInfoSummary"];
            }

        }

        [ConfigurationProperty("pdqCancerInfoSummaryLink")]
        public ValueElement PDQCancerInfoSummaryLink
        {
            get
            {
                return (ValueElement)base["pdqCancerInfoSummaryLink"];
            }

        }

        [ConfigurationProperty("pdqCancerInfoSummaryPage")]
        public ValueElement PDQCancerInfoSummaryPage
        {
            get
            {
                return (ValueElement)base["pdqCancerInfoSummaryPage"];
            }

        }

        [ConfigurationProperty("pdqMediaLink")]
        public ValueElement PDQMediaLink
        {
            get
            {
                return (ValueElement)base["pdqMediaLink"];
            }

        }

        [ConfigurationProperty("pdqTableSection")]
        public ValueElement PDQTableSection
        {
            get
            {
                return (ValueElement)base["pdqTableSection"];
            }

        }
    }
}
