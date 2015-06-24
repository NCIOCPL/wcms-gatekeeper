using System;
using System.Collections.Generic;
using System.Text;
using GateKeeper.Common;
using System.Xml;
using System.Xml.Serialization;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// Class to maintain the processing context of a glossary term document
    /// </summary>
    public class GlossaryTermDocument : Document 
    {
        GlossaryTermMetadata TermData { get; set; }


        /// <summary>
        /// Default constructor.
        /// </summary>
        public GlossaryTermDocument()
            : base()
        {
            this.DocumentType = DocumentType.GlossaryTerm;
        }

    }
}
