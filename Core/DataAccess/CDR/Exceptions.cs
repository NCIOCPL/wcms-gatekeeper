using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Base exception for exceptions related to the Extract operation.
    /// </summary>
    [global::System.Serializable]
    public abstract class ExtractException : Exception
    {
        public ExtractException() { }
        public ExtractException(string message) : base(message) { }
        public ExtractException(string message, Exception inner) : base(message, inner) { }
        protected ExtractException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Exception thrown when a specific XML element is required but not present.
    /// </summary>
    [global::System.Serializable]
    public class MissingElementException : ExtractException
    {
        public MissingElementException() { }
        public MissingElementException(string message) : base(message) { }
        public MissingElementException(string message, Exception inner) : base(message, inner) { }
        protected MissingElementException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
