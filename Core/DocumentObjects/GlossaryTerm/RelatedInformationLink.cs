using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// Class which represents the RelatedInformation Links
    /// </summary>
    [Serializable]
    public class RelatedInformationLink
    {
        /// <summary>
        /// Enumerated list of document types a related link might reference.
        /// </summary>
        public enum RelatedLinkType
        {
            /// <summary>
            /// Unknown document type.
            /// </summary>
            Undefined = 0,
            /// <summary>
            /// External (unmanaged)
            /// </summary>
            External = 1,
            /// <summary>
            /// Cancer Summary
            /// </summary>
            Summary = 2,
            /// <summary>
            /// Drug info Summary
            /// </summary>
            DrugInfoSummary = 4,
            /// <summary>
            /// A glossary term.
            /// </summary>
            GlossaryTerm = 8
        }

        /// <summary>
        /// Notes the type of document the related link references.
        /// </summary>
        public RelatedLinkType LinkType { get; set; }

        /// <summary>
        /// Name of the relatedinformation link
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The url of the related information link 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Pre-rendered HTML.
        /// </summary>
        public string Html
        { get; set; }

        /// <summary>
        /// Property which identifies the language this relatedinformation link will be used
        /// with
        /// </summary>
        public Language Language { get; set; }

        public RelatedInformationLink()
        { }

        public RelatedInformationLink(string name, string url, Language language, RelatedLinkType linkType)
        {
            Name = name;
            Url = url;
            Language = language;
            LinkType = linkType;
        }
    }
}
