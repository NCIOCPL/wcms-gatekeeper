using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects
{
    /// <summary>
    /// Represents the intended audience for a document.
    /// </summary>
    [Serializable]
    public enum AudienceType
    {
        /// <summary>
        /// Document is intended for health professionals.
        /// </summary>
        HealthProfessional = 1,

        /// <summary>
        /// Document is intended for patients.
        /// </summary>
        Patient = 2,
    }
}
