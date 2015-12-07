using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DocumentObjects
{
    [global::System.Serializable]
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

    [global::System.Serializable]
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
}
