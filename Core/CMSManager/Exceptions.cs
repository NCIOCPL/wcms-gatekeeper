using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers.CMSManager
{
    /// <summary>
    /// Absract base class for all exceptions thrown by objects in the CMSManager namespace.
    /// </summary>
    [global::System.Serializable]
    public abstract class CMSException : Exception
    {
        public CMSException() { }
        public CMSException(string message) : base(message) { }
        public CMSException(string message, Exception inner) : base(message, inner) { }
        protected CMSException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown by methods in the GKManagers.CMSManager namespace when an unexpected document type
    /// is encountered during processing.
    /// </summary>
    [global::System.Serializable]
    public class CMSManagerIncorrectDocumentTypeException : CMSException
    {
        public CMSManagerIncorrectDocumentTypeException() { }
        public CMSManagerIncorrectDocumentTypeException(string message) : base(message) { }
        public CMSManagerIncorrectDocumentTypeException(string message, Exception inner) : base(message, inner) { }
        protected CMSManagerIncorrectDocumentTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
