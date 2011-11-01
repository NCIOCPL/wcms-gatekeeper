using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;

namespace GKManagers.CMSManager.Configuration
{
    /// <summary>
    /// This class contains config Setting specifically for CDR Preview
    /// </summary>
    public class PreviewSettings : ConfigurationElement
    {
        /// <summary>
        /// The id of the publish preview context. The publish preview context id will 
        /// be different in each environment. So this is a configurable value.
        /// </summary>
        [ConfigurationProperty("publishPreviewContextId", IsRequired = true)]
        public ConfigValue PublishPreviewContextId
        {
            get
            {
                return (ConfigValue)base["publishPreviewContextId"];
            }
        }

        /// <summary>
        /// Gets the Item Filter value associated with publish preview.
        /// </summary>
        [ConfigurationProperty("itemFilter", IsRequired = true)]
        public ConfigValue ItemFilter
        {
            get
            {
                return (ConfigValue)base["itemFilter"];
            }
        }


        [ConfigurationProperty("pdqDrugInfoSummaryTemplateName", IsRequired = true)]
        public ConfigValue PDQDrugInfoSummaryTemplateName
        {
            get
            {
                return (ConfigValue)base["pdqDrugInfoSummaryTemplateName"];
            }
        }

        [ConfigurationProperty("pdqCancerSummaryTemplateName", IsRequired = true)]
        public ConfigValue PDQCancerSummaryTemplateName
        {
            get
            {
                return (ConfigValue)base["pdqCancerSummaryTemplateName"];
            }
        }

        [ConfigurationProperty("pdqImageTemplateName", IsRequired = true)]
        public ConfigValue PDQImageTemplateName
        {
            get
            {
                return (ConfigValue)base["pdqImageTemplateName"];
            }
        }

        [ConfigurationProperty("previewAudioFilePath", IsRequired = true)]
        public ConfigValue PreviewAudioFilePath
        {
            get
            {
                return (ConfigValue)base["previewAudioFilePath"];
            }
        }

        [ConfigurationProperty("previewImageContentLocation", IsRequired = true)]
        public ConfigValue PreviewImageContentLocation
        {
            get
            {
                return (ConfigValue)base["previewImageContentLocation"];
            }
        }

        [ConfigurationProperty("frameHtmlPage", IsRequired = true)]
        public ConfigValue FrameHtmlPage
        {
            get
            {
                return (ConfigValue)base["frameHtmlPage"];
            }
        }
        
    }
}
