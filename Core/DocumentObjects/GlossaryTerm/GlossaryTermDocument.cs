using System;
using System.Collections.Generic;
using System.Text;
using GateKeeper.Common;
using System.Xml;
using System.Xml.Serialization;

using GateKeeper.DocumentObjects.Dictionary;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// Class to maintain the processing context of a glossary term document
    /// </summary>
    public class GlossaryTermDocument : Document 
    {
        /// <summary>
        /// The collection of dictionary items created from the document XML.
        /// </summary>
        public List<GeneralDictionaryEntry> Dictionary { get; private set; }

        /// <summary>
        /// The collection of aliases for the term.
        /// (GlossaryTerm documents don't include an alias element, so this collection is always empty.
        /// </summary>
        public List<TermAlias> AliasList { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GlossaryTermDocument()
            : base()
        {
            this.DocumentType = DocumentType.GlossaryTerm;
            this.Dictionary = new List<GeneralDictionaryEntry>();
            this.AliasList = new List<TermAlias>();
        }

    }
}
