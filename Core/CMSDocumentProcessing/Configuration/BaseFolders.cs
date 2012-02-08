using System.Configuration;


namespace GKManagers.CMSDocumentProcessing.Configuration
{
    /// <summary>
    /// Class for accessing site base paths.
    /// </summary>
    public class BaseFolders : ConfigurationElement
    {
        /// <summary>
        /// Gets the mobile site's base folder.
        /// </summary>
        /// <value>CMS folder corresponding to the base of a mobile site.</value>
        [ConfigurationProperty("mobileSiteBase", IsRequired = false)]
        public string MobileSiteBase
        {
            get { return (string)base["mobileSiteBase"]; }
        }

        /// <summary>
        /// Gets the desktop site's base folder.
        /// </summary>
        /// <value>CMS folder corresponding to the base of a desktop site.</value>
        [ConfigurationProperty("desktopSiteBase", IsRequired = true)]
        public string DesktopSiteBase
        {
            get { return (string)base["desktopSiteBase"]; }
        }
    }
}
