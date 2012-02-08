using System;
using System.Configuration;

namespace GKManagers.Configuration
{
    /// <summary>
    /// Configuration section for setting up doucment processing.
    /// </summary>
    public class DocumentProcessingSection : ConfigurationSection
    {
        /// <summary>
        /// Loads the DocumentProcessing section.
        /// </summary>
        public static DocumentProcessingSection Instance
        {
            get
            {
                DocumentProcessingSection config
                    = (DocumentProcessingSection)ConfigurationManager.GetSection("DocumentProcessing");
                return config;
            }
        }

        [ConfigurationProperty("processingConfigurationFile", IsRequired = true)]
        public NamedSingleValueElement ProcessingConfigurationFile
        {
            get { return (NamedSingleValueElement)base["processingConfigurationFile"]; }
        }
    }
}
