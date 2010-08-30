using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects
{
    /// <summary>
    /// Represents supported document languages.
    /// </summary>
    [Serializable]
    public enum Language
    {
        /// <summary>
        /// English language document or text.
        /// </summary>
        English = 1,

        /// <summary>
        /// Spanish language document or text.
        /// </summary>
        Spanish = 2,

        /// <summary>
        /// Language is unsupported.
        /// </summary>
        NotSupported = 9999,
    }
}
