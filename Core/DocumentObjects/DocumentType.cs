using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects
{
    /// <summary>
    /// Enumeration to represent supported document types.
    /// </summary>
    [Serializable]
    public enum DocumentType
    {
        /// <summary>
        /// Explicitly invalid value
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// Drug information summary document type.
        /// </summary>
        DrugInfoSummary = 9,

        /// <summary>
        /// Genetics professional document type.
        /// </summary>
        GENETICSPROFESSIONAL = 10,

        /// <summary>
        /// Glossary term document type.
        /// </summary>
        GlossaryTerm = 26,

        /// <summary>
        /// Media document type.
        /// </summary>
        Media = 29,

        /// <summary>
        /// Organization document type.
        /// </summary>
        Organization = 7,

        /// <summary>
        /// Protocol document type.
        /// </summary>
        Protocol = 5,

        /// <summary>
        /// CTGovProtocol document type.
        /// </summary>
        CTGovProtocol = 28,


        /// <summary>
        /// Summary document type.
        /// </summary>
        Summary = 6,


        /// <summary>
        /// Terminology document type.
        /// </summary>
        Terminology = 3,


        /// <summary>
        /// PoliticalSubUnit document type.
        /// </summary>
        PoliticalSubUnit = 8,
    }
}
