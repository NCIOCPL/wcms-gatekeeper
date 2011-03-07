using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Represents types of summary relations.
    /// </summary>
    [Serializable]
    public enum SummaryRelationType
    {
        /// <summary>
        /// This summary is a patient version of the related summary.
        /// </summary>
        PatientVersionOf = 1,

        /// <summary>
        /// This summary is a spanish translation of the related summary.
        /// </summary>
        SpanishTranslationOf = 2,
    }
}
