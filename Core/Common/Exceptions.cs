using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.Common
{
    /// <summary>
    /// General base class for all GateKeeper exceptions.
    /// </summary>
    [global::System.Serializable]
    public class GateKeeperException : Exception
    {
        public GateKeeperException() { }
        public GateKeeperException(string message) : base(message) { }
        public GateKeeperException(string message, Exception inner) : base(message, inner) { }
        protected GateKeeperException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown when processing is expecting one type of document, but encounters another.
    /// Used when converting from a more general document type to a more specific one.
    /// </summary>
    [global::System.Serializable]
    public class DocumentTypeMismatchException : GateKeeperException
    {
        public DocumentTypeMismatchException() { }
        public DocumentTypeMismatchException(string message) : base(message) { }
        public DocumentTypeMismatchException(string message, Exception inner) : base(message, inner) { }
        protected DocumentTypeMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown when an error is found in the processing configuration.
    /// </summary>
    [global::System.Serializable]
    public class ProcessingConfigurationException : GateKeeperException
    {
        public ProcessingConfigurationException() { }
        public ProcessingConfigurationException(string message) : base(message) { }
        public ProcessingConfigurationException(string message, Exception inner) : base(message, inner) { }
        protected ProcessingConfigurationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown when an error occurs during the metadata extraction process.
    /// </summary>
    [global::System.Serializable]
    public class ExtractionException : Exception
    {
        public ExtractionException() { }
        public ExtractionException(string message) : base(message) { }
        public ExtractionException(string message, Exception inner) : base(message, inner) { }
        protected ExtractionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
