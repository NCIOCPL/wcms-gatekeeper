using System;
using System.Configuration;


namespace GKManagers.CMSManager.Configuration
{

    /// <summary>
    /// Controls serialization of the DrugInfoSummaryConfig configuration section.
    /// </summary>
    public class DrugInfoSummaryConfig : ConfigurationSection
    {
        // Slot ID to use for the related pages box.
        [ConfigurationProperty("relatedPagesSlotID")]
        public RelatedPagesSlotIDElement RelatedPagesSlotID
        {
            get { return (RelatedPagesSlotIDElement)this["relatedPagesSlotID"]; }
            set { this["relatedPagesSlotID"] = value; }
        }

        // Snippet template to use for the related pages box.
        [ConfigurationProperty("relatedPagesSnippetTemplateID")]
        public RelatedPagesSnippetTemplateIDElement RelatedPagesSnippetTemplateID
        {
            get { return (RelatedPagesSnippetTemplateIDElement)this["relatedPagesSnippetTemplateID"]; }
            set { this["relatedPagesSnippetTemplateID"] = value; }
        }

        // Snippet template to use for the related pages box.
        [ConfigurationProperty("relatedPagesContentID")]
        public RelatedPagesContentIDElement RelatedPagesContentID
        {
            get { return (RelatedPagesContentIDElement)this["relatedPagesContentID"]; }
            set { this["relatedPagesContentID"] = value; }
        }
    }
}
