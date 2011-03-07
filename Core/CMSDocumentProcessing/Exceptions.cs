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

    /// <summary>
    /// Thrown by methods in the GKManagers.CMSDocumentProcessing namespace when an error occurs
    /// with a content item's association to a folder.
    /// </summary>
    [global::System.Serializable]
    public class FolderAssociationException : DocumentProcessingException
    {
        public FolderAssociationException() { }
        public FolderAssociationException(string message) : base(message) { }
        public FolderAssociationException(string message, Exception inner) : base(message, inner) { }
        protected FolderAssociationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown by methods in the GKManagers.CMSDocumentProcessing namespace when a document's
    /// English version of a document doesn't exist prior to creating an alternate language
    /// version.
    /// </summary>
    [global::System.Serializable]
    public class EnglishVersionNotFoundException : DocumentProcessingException
    {
        public EnglishVersionNotFoundException() { }
        public EnglishVersionNotFoundException(string message) : base(message) { }
        public EnglishVersionNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected EnglishVersionNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
