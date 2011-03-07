using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects
{
    /// <summary>
    /// Enumerations to represent the document types from CDR Preview.
    /// </summary>
    public enum PreviewTypes
    {
        /// <summary>
        /// No preview type is defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// Summary document.
        /// </summary>
        Summary,

        /// <summary>
        /// Drug Information Summary document.
        /// </summary>
        DrugInfoSummary,

        /// <summary>
        /// Protocol Health Professional document.
        /// </summary>
        Protocol_HP,

        /// <summary>
        /// Protocol Patient document.
        /// </summary>
        Protocol_Patient,

        /// <summary>
        /// CT Gov Protocol document.
        /// </summary>
        CTGovProtocol,

        /// <summary>
        /// Glossary Term document.
        /// </summary>
        GlossaryTerm,

        /// <summary>
        /// Genetics Professional document.
        /// </summary>
        GeneticsProfessional
    }
}
