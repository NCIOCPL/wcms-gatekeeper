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

        public RelatedInformationLink(string name, string url, Language language)
        {
            Name = name;
            Url = url;
            Language = language;
        }
    }
}
