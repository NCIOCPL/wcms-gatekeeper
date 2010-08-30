using System;
using System.Collections.Generic;
using System.Text;

using NCI.Logging;

namespace GateKeeper.Common
{
    /// <summary>
    /// Wrapper class for event logging.  This class is not used directly, instead derive a
    /// module-specific subclass which passes the module name in the constuctor.  The module name
    /// should generally be WebService, WebAdmin, ProcessManager or Framework (i.e. The business
    /// logic layer). This allows for standardization of module names while also reducing the
    /// amount of typing for each logging call.
    /// 
    /// For example, the BatchLogBuilder has a constructor implemented as
    /// 
    ///     public BatchLogBuilder()
    ///         : base("BatchManager")
    ///     {
    ///     }
    /// 
    /// </summary>
    public abstract class LogBuilder
    {
        // The name of the overall application
        private const string _applicationName = "GateKeeper";

        private string _moduleName;

        /// <summary>
        /// Creates an instance of the LogBuilder object.  This constructor is intended to be called
        /// via the base() operator in the constructor of a derived class.
        /// </summary>
        /// <param name="module">A string, generally one of WebService, WebAdmin, ProcessManager or Framework</param>
        protected LogBuilder(string moduleName)
        {
            _moduleName = moduleName;
        }

        /// <summary>
        /// Creates a Critical entry in the event log.  This allows for distinct "special" processing
        /// above that performed for Error entries.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        public void CreateCritical(Type classType, string method, string message)
        {
            Create(_moduleName, classType, method, message, NCIErrorLevel.Critical);
        }

        /// <summary>
        /// Creates a Critical entry in the event log.  This allows for distinct "special" processing
        /// above that performed for Error entries.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        public void CreateCritical(Type classType, string method,
            string message, Exception ex)
        {
            Create(_moduleName, classType, method, message, ex, NCIErrorLevel.Critical);
        }

        /// <summary>
        /// Creates a Critical entry in the event log.  This allows for distinct "special" processing
        /// above that performed for Error entries.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        public void CreateCritical(Type classType, string method, Exception ex)
        {
            Create(classType, method, ex, NCIErrorLevel.Critical);
        }

        /// <summary>
        /// Creates an Error entry in the event log.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        public void CreateError(Type classType, string method, string message)
        {
            Create(_moduleName, classType, method, message, NCIErrorLevel.Error);
        }

        /// <summary>
        /// Creates an Error entry in the event log.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        public void CreateError(Type classType, string method, string message, Exception ex)
        {
            Create(_moduleName, classType, method, message, ex, NCIErrorLevel.Error);
        }

        /// <summary>
        /// Creates an Error entry in the event log.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        public void CreateError(Type classType, string method, Exception ex)
        {
            Create(classType, method, ex, NCIErrorLevel.Error);
        }

        /// <summary>
        /// Creates a Warning entry in the event log.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        public void CreateWarning(Type classType, string method, string message)
        {
            Create(_moduleName, classType, method, message, NCIErrorLevel.Warning);
        }

        /// <summary>
        /// Creates a Warning entry in the event log.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        public void CreateWarning(Type classType, string method, string message, Exception ex)
        {
            Create(_moduleName, classType, method, message, ex, NCIErrorLevel.Warning);
        }

        /// <summary>
        /// Creates a Warning entry in the event log.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        public void CreateWarning(Type classType, string method, Exception ex)
        {
            Create(classType, method, ex, NCIErrorLevel.Warning);
        }

        /// <summary>
        /// Creates an Information entry in the event log.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        public void CreateInformation(Type classType, string method, string message)
        {
            Create(_moduleName, classType, method, message, NCIErrorLevel.Info);
        }

        /// <summary>
        /// Creates an Information entry in the event log.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        public void CreateInformation(Type classType, string method, string message, Exception ex)
        {
            Create(_moduleName, classType, method, message, ex, NCIErrorLevel.Info);
        }

        /// <summary>
        /// Creates an Information entry in the event log.
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        public void CreateInformation(Type classType, string method, Exception ex)
        {
            Create(classType, method, ex, NCIErrorLevel.Info);
        }

        /// <summary>
        /// Creates the actual event log entry.  This method is not intended to be called directly; instead,
        /// call one of the Create_event methods. 
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        /// <param name="level">Enumerated value representing the specific type of log entry to create.</param>
        private void Create(string module, Type classType, string method, string message, NCIErrorLevel level)
        {
            try
            {
                string facility = BuildFacilityString(classType, method);
                Logger.LogError(facility, message, level);
            }
            catch (Exception)
            {
                // Don't let logging cause an event that needs to be logged.
            }
        }

        /// <summary>
        /// Creates the actual event log entry.  This method is not intended to be called directly; instead,
        /// call one of the Create_event methods. 
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="message">A plain-text description of the event being logged.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        /// <param name="level">Enumerated value representing the specific type of log entry to create.</param>
        private void Create(string module, Type classType, string method, string message, Exception ex, NCIErrorLevel level)
        {
            try
            {
                string facility = BuildFacilityString(classType, method);
                Logger.LogError(facility, message, level, ex);
            }
            catch (Exception)
            {
                // Don't let logging cause an event that needs to be logged.
            }
        }

        /// <summary>
        /// Creates the actual event log entry.  This method is not intended to be called directly; instead,
        /// call one of the Create_event methods. 
        /// </summary>
        /// <param name="classType">The Type object describing the class/instance where the logged
        /// event occured.  The Type object may be obtained by calling the GetType() method on this
        /// or in a static method, by using the typeof() operator.</param>
        /// <param name="method">The name of the method where the logged event occured.</param>
        /// <param name="ex">The exception which created a loggable event.</param>
        /// <param name="level">Enumerated value representing the specific type of log entry to create.</param>
        private void Create(Type classType, string method, Exception ex, NCIErrorLevel level)
        {
            try
            {
                string facility = BuildFacilityString(classType, method);
                Logger.LogError(facility, level, ex);
            }
            catch (Exception)
            {
                // Don't let logging cause an event that needs to be logged.
            }
        }

        /// <summary>
        /// Internal method to format the log system's facility string.
        /// </summary>
        /// <param name="classType">The Type object for the class where the loggable event takes place.</param>
        /// <param name="method">Name of the method where the loggable event takes place.</param>
        /// <returns></returns>
        private string BuildFacilityString(Type classType, string method)
        {
            string facilityFormat = "{0}:{1}:{2}.{3}";
            string facility = string.Format(facilityFormat,
                _applicationName, _moduleName, classType.FullName, method);

            return facility;
        }
    }
}
