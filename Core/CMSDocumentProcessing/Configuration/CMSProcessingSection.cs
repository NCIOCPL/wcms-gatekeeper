using System;
using System.Configuration;


namespace GKManagers.CMSDocumentProcessing.Configuration
{
    public class CMSProcessingSection : ConfigurationSection
    {
        /// <summary>
        /// Loads the CMSProcessingSection section.
        /// </summary>
        public static CMSProcessingSection Instance
        {
            get
            {
                CMSProcessingSection config
                    = (CMSProcessingSection)ConfigurationManager.GetSection("CMSProcessing");
                return config;
            }
        }

        [ConfigurationProperty("BaseFolders", IsRequired = true)]
        public BaseFolders BaseFolders
        {
            get { return (BaseFolders)base["BaseFolders"]; }
        }
    }
}
