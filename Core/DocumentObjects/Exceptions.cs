using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DocumentObjects
{
    [Serializable]
    public class MediaLinkSizeException : Exception
    {
        public MediaLinkSizeException() { }
        public MediaLinkSizeException(string message) : base(message) { }
        public MediaLinkSizeException(string message, Exception inner) : base(message, inner) { }
        protected MediaLinkSizeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class UnexpectedExtractedValueException : Exception
    {
        public UnexpectedExtractedValueException() { }
        public UnexpectedExtractedValueException(string message) : base(message) { }
        public UnexpectedExtractedValueException(string message, Exception inner) : base(message, inner) { }
        protected UnexpectedExtractedValueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }


    /// <summary>
    /// Thrown when a document cannot be validated.
    /// </summary>
    [Serializable]
    public class ValidationException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ValidationException() { }

        /// <summary>
        /// Constructor for specifying the exception message.
        /// </summary>
        /// <param name="message">Message describing why the exception was thrown.</param>
        public ValidationException(string message) : base(message) { }

        /// <summary>
        /// Constructor for specifying the exception message and the exception which caused it.
        /// </summary>
        /// <param name="message">Message describing why the exception was thrown.</param>
        /// <param name="inner">Exception which caused the validation exception to be thrown.</param>
        public ValidationException(string message, Exception inner) : base(message, inner) { }
    }
}
