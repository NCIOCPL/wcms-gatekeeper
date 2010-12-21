using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers.CMSDocumentProcessing
{
    [global::System.Serializable]
    public abstract class DocumentProcessingException : Exception
    {
        public DocumentProcessingException() { }
        public DocumentProcessingException(string message) : base(message) { }
        public DocumentProcessingException(string message, Exception inner) : base(message, inner) { }
        protected DocumentProcessingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class DocumentExistsException : DocumentProcessingException
    {
        public DocumentExistsException() { }
        public DocumentExistsException(string message) : base(message) { }
        public DocumentExistsException(string message, Exception inner) : base(message, inner) { }
        protected DocumentExistsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class EmptySlotException : DocumentProcessingException
    {
        public EmptySlotException() { }
        public EmptySlotException(string message) : base(message) { }
        public EmptySlotException(string message, Exception inner) : base(message, inner) { }
        protected EmptySlotException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
