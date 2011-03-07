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
        public PDQDrugInfoSummaryElement PDQDrugInfoSummary
        {
            get
            {
                return (PDQDrugInfoSummaryElement)base["pdqDrugInfoSummary"];
            }

        }

        [ConfigurationProperty("pdqCancerInfoSummary")]
        public PDQCancerInfoSummaryElement PDQCancerInfoSummary
        {
            get
            {
                return (PDQCancerInfoSummaryElement)base["pdqCancerInfoSummary"];
            }

        }

        [ConfigurationProperty("pdqCancerInfoSummaryLink")]
        public PDQCancerInfoSummaryLinkElement PDQCancerInfoSummaryLink
        {
            get
            {
                return (PDQCancerInfoSummaryLinkElement)base["pdqCancerInfoSummaryLink"];
            }

        }

        [ConfigurationProperty("pdqCancerInfoSummaryPage")]
        public PDQCancerInfoSummaryPageElement PDQCancerInfoSummaryPage
        {
            get
            {
                return (PDQCancerInfoSummaryPageElement)base["pdqCancerInfoSummaryPage"];
            }

        }

        [ConfigurationProperty("pdqMediaLink")]
        public PDQMediaLinkElement PDQMediaLink
        {
            get
            {
                return (PDQMediaLinkElement)base["pdqMediaLink"];
            }

        }

        [ConfigurationProperty("pdqTableSection")]
        public PDQTableSectionElement PDQTableSection
        {
            get
            {
                return (PDQTableSectionElement)base["pdqTableSection"];
            }

        }
    }
}
