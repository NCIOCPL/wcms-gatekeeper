using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// Definitions for a glossary term document which has already been processed through GateKeeper.
    /// </summary>
    public class GlossaryTermSimpleDefinition
    {
        /// <summary>
        /// The target audience.
        /// </summary>
        public AudienceType Audience { get; private set; }

        /// <summary>
        /// Definition's language.
        /// </summary>
        public Language Language { get; private set; }

        /// <summary>
        /// Plain text of the definition.
        /// </summary>
        public String DefinitionText { get; private set; }

        /// <summary>
        /// HTML markup of the definition.
        /// </summary>
        public String DefinitionHtml { get; private set; }

        /// <summary>
        /// HTML markup for any images associated with the definition.
        /// </summary>
        public String MediaHtml { get; private set; }

        /// <summary>
        /// HTML markup for any audio pronunciation associated with the definition.
        /// </summary>
        public String AudioHtml { get; private set; }

        /// <summary>
        /// HTML mark up for any related links accompanying the definition.
        /// </summary>
        public String RelatedLinksHtml { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="audience">The target audience.</param>
        /// <param name="language">Definition's language.</param>
        /// <param name="definitionText">Plain text</param>
        /// <param name="definitionHtml">HTML markup</param>
        /// <param name="mediaHtml">HTML markup for any images</param>
        /// <param name="audioHtml">HTML markup for any audio pronunciation</param>
        /// <param name="relatedLinksHtml">HTML mark up for any related links</param>
        public GlossaryTermSimpleDefinition(AudienceType audience, Language language,
            String definitionText, String definitionHtml, String mediaHtml, String audioHtml, String relatedLinksHtml)
        {
            Audience = audience;
            Language = language;
            DefinitionText = definitionText;
            DefinitionHtml = definitionHtml;
            MediaHtml = mediaHtml;
            AudioHtml = audioHtml;
            RelatedLinksHtml = relatedLinksHtml;
        }
    }
}
