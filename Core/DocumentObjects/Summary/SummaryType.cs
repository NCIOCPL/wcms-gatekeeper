using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Represents the summary type.
    /// </summary>
    [Serializable]
    public enum SummaryType
    {
        /// <summary>
        /// Patient (general public) summary.
        /// </summary>
        Patient = 1,

        /// <summary>
        /// Health professional summary.
        /// </summary>
        HealthProfessional = 2,
    }
}
