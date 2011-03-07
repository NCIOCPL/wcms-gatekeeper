using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;

namespace NCI.WCM.CMSManager
{
    /// <summary>
    /// Absract base class for all exceptions thrown by objects in the NCI.WCM.CMSManager namespace.
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
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when an unexpected document type
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

    /// <summary>
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when an error occurs
    /// in workflow processing.
    /// </summary>
    [global::System.Serializable]
    public class CMSWorkflowException : CMSException
    {
        public CMSWorkflowException() { }
        public CMSWorkflowException(string message) : base(message) { }
        public CMSWorkflowException(string message, Exception inner) : base(message, inner) { }
        protected CMSWorkflowException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when an error occurs
    /// in determining an item's workflow state.
    /// </summary>
    [global::System.Serializable]
    public class CMSWorkflowStateInferenceException : CMSException
    {
        public CMSWorkflowStateInferenceException() { }
        public CMSWorkflowStateInferenceException(string message) : base(message) { }
        public CMSWorkflowStateInferenceException(string message, Exception inner) : base(message, inner) { }
        protected CMSWorkflowStateInferenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when an Soap error occurs.
    /// </summary>
    [global::System.Serializable]
    public class CMSSoapException : CMSException
    {
        public CMSSoapException(string message, SoapException inner)
            : base(message + "\n\n" + inner.Detail.InnerXml.ToString(),inner) { }
        //public CMSSoapException(string message, SoapException inner)
        //    : base(message + "\n\n" + inner.ToString()) { }


    }
    
    /// <summary>
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when an attempt is made
    /// to delete content items which are the target of relationships owned by other
    /// content items.
    /// </summary>
    [global::System.Serializable]
    public class CMSCannotDeleteException : CMSException
    {
        public CMSCannotDeleteException() { }
        public CMSCannotDeleteException(string message) : base(message) { }
        public CMSCannotDeleteException(string message, Exception inner) : base(message, inner) { }
        protected CMSCannotDeleteException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
    
    /// <summary>
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when the CMS returns an
    /// unexpected result when performing an operation.
    /// </summary>
    [global::System.Serializable]
    public class CMSOperationalException : CMSException
    {
        public CMSOperationalException() { }
        public CMSOperationalException(string message) : base(message) { }
        public CMSOperationalException(string message, Exception inner) : base(message, inner) { }
        protected CMSOperationalException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when the TemplateNameManager
    /// is unable to locate an expected template.
    /// </summary>
    [global::System.Serializable]
    public class CMSMissingTemplateException : CMSException
    {
        public CMSMissingTemplateException() { }
        public CMSMissingTemplateException(string message) : base(message) { }
        public CMSMissingTemplateException(string message, Exception inner) : base(message, inner) { }
        protected CMSMissingTemplateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when the ContentTypeManager
    /// is unable to locate an expected content type.
    /// </summary>
    [global::System.Serializable]
    public class CMSMissingContentTypeException : CMSException
    {
        public CMSMissingContentTypeException() { }
        public CMSMissingContentTypeException(string message) : base(message) { }
        public CMSMissingContentTypeException(string message, Exception inner) : base(message, inner) { }
        protected CMSMissingContentTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when the SlotManager
    /// is unable to locate an expected slot.
    /// </summary>
    [global::System.Serializable]
    public class CMSMissingSlotException : CMSException
    {
        public CMSMissingSlotException() { }
        public CMSMissingSlotException(string message) : base(message) { }
        public CMSMissingSlotException(string message, Exception inner) : base(message, inner) { }
        protected CMSMissingSlotException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Thrown by methods in the NCI.WCM.CMSManager namespace when an attempt is made to
    /// set a content item field which does not exist.
    /// </summary>
    [global::System.Serializable]
    public class CMSInvalidFieldnameException : CMSException
    {
        public CMSInvalidFieldnameException() { }
        public CMSInvalidFieldnameException(string message) : base(message) { }
        public CMSInvalidFieldnameException(string message, Exception inner) : base(message, inner) { }
        protected CMSInvalidFieldnameException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
