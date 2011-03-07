using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace GateKeeper.Logging
{
    /// <summary>
    /// Class to manage application logging.
    /// </summary>
    /// <remarks>This class does not rely on the app.config for logging configuration</remarks>
    public class LogManager
    {
        #region Fields

        private static LogWriter _logWriter; // Instance is created in static ctor which is thread safe
        private static CategoryFilter _categoryFilter;

        #endregion Fields

        #region Constants

        // Event Log Source name.
        const string AppEventLogName = "GateKeeper";
        const string GateKeeperEventLogSource = "GateKeeperWS";
        const string ProcessManagerEventLogSource = "ProcessManager";

        /// <summary>
        /// Name of the category filter.
        /// </summary>
        public const string CategoryFilterName = "CategoryFilter";

        /// <summary>
        /// Critical/fatal error log message category..
        /// </summary>
        public const string CriticalLogCategory = "Critical";

        /// <summary>
        /// Error log message category..
        /// </summary>
        public const string ErrorLogCategory = "Error";

        /// <summary>
        /// Warning log message category..
        /// </summary>
        public const string WarningLogCategory = "Warning";

        /// <summary>
        /// General (common) log message category.
        /// </summary>
        public const string GeneralLogCategory = "General";

        /// <summary>
        /// Verbose log message category.
        /// </summary>
        public const string VerboseLogCategory = "Verbose";

        /// <summary>
        /// Trace level log message category (generates the most output).
        /// </summary>
        public const string TraceLogCategory = "Trace";

        #endregion

        #region Enumerations

        /// <summary>
        /// Represents processing statuses.
        /// </summary>
        public enum ProcessStatus
        {
            Ready = 1,
            Processing = 2,
            Error = 3,
            Done = 4,
            Cancelled = 5,
        }

        #endregion Enumerations

        #region Public Static Properties

        /// <summary>
        /// Gets, sets log category filter.
        /// </summary>
        public static CategoryFilter CategoryFilter
        {
            get { return LogManager._categoryFilter; }
            set
            {
                lock (typeof(LogManager))
                {
                    LogManager._categoryFilter = value;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Static class constructor.
        /// </summary>
        /// <remarks>Static ctor is thread safe according to ECMA standard section 9.5.3</remarks>
        static LogManager()
        {
            CreateEventSource();
            _logWriter = CreateLogWriter();
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Creates a LogWriter without an app.config.
        /// </summary>
        /// <returns></returns>
        /// <see cref="http://geekswithblogs.net/akraus1/archive/2006/02/16/69784.aspx"/>
        private static LogWriter CreateLogWriter()
        {
            // This is our message template for any Sink you add below in our case the Windows Event Log
            TextFormatter formatter = new TextFormatter("Timestamp: {timestamp}{newline}" +
                                                        "Message: {message}{newline}" +
                                                        "Category: {category}{newline}" +
                                                        "Severity: {severity}{newline}" +
                                                        "Process Id: {processId}{newline}" +
                                                        "Process Name: {processName}{newline}" +
                                                        "Win32 Thread Id: {win32ThreadId}{newline}" +
                                                        "Thread Name: {threadName}{newline}" +
                                                        "Extended Properties: {dictionary({key} - {value}{newline})}");

            // TODO: Add other sources beside event log...(database, flatfile, MSMQ?)

            //FlatFileTraceListener logFileListener =
            //    new FlatFileTraceListener(@"c:\messages.log",
            //                           "----------",
            //                           "----------",
            //                           formatter);

            LogSource emptyTraceSource = new LogSource("none");

            LogSource criticalTraceSource = new LogSource(CriticalLogCategory, SourceLevels.Critical);
            LogSource errorsTraceSource = new LogSource(ErrorLogCategory, SourceLevels.Error);
            LogSource warningTraceSource = new LogSource(WarningLogCategory, SourceLevels.Warning);
            LogSource generalTraceSource = new LogSource(GeneralLogCategory, SourceLevels.Information);
            LogSource verboseTraceSource = new LogSource(VerboseLogCategory, SourceLevels.Verbose);
            LogSource traceTraceSource = new LogSource(TraceLogCategory, SourceLevels.ActivityTracing);

            // Create Listeners
            criticalTraceSource.Listeners.Add(new FormattedEventLogTraceListener(GetEventLogSource(), formatter));
            errorsTraceSource.Listeners.Add(new FormattedEventLogTraceListener(GetEventLogSource(), formatter));
            warningTraceSource.Listeners.Add(new FormattedEventLogTraceListener(GetEventLogSource(), formatter));
            generalTraceSource.Listeners.Add(new FormattedEventLogTraceListener(GetEventLogSource(), formatter));
            verboseTraceSource.Listeners.Add(new FormattedEventLogTraceListener(GetEventLogSource(), formatter));
            traceTraceSource.Listeners.Add(new FormattedEventLogTraceListener(GetEventLogSource(), formatter));

            IDictionary<string, LogSource> traceSources = new Dictionary<string, LogSource>();
            traceSources.Add(CriticalLogCategory, criticalTraceSource);
            traceSources.Add(ErrorLogCategory, errorsTraceSource);
            traceSources.Add(WarningLogCategory, warningTraceSource);
            traceSources.Add(GeneralLogCategory, generalTraceSource);
            traceSources.Add(VerboseLogCategory, verboseTraceSource);

            // Setup default category filter (don't log verbose and trace by default)
            _categoryFilter = new CategoryFilter(CategoryFilterName, 
                new string[] { VerboseLogCategory, TraceLogCategory }, CategoryFilterMode.AllowAllExceptDenied);

            List<ILogFilter> filterList = new List<ILogFilter>();
            filterList.Add(_categoryFilter);

            return new LogWriter(filterList,           // ICollection<ILogFilter> filters
                                 traceSources,         // IDictionary<string, LogSource> traceSources
                                 emptyTraceSource,     // LogSource allEventsTraceSource
                                 emptyTraceSource,     // LogSource notProcessedTraceSource
                                 errorsTraceSource,    // LogSource errorsTraceSource
                                 GeneralLogCategory,   // string defaultCategory
                                 false,                // bool tracingEnabled
                                 false);               // bool logWarningsWhenNoCategoriesMatch
        }

        /// <summary>
        /// This method will read an event log source 
        /// from the application config and return it to the caller.
        /// </summary>
        /// <returns></returns>
        private static string GetEventLogSource()
        {
            string eventLogSource = LogManager.ProcessManagerEventLogSource;

            string tempEventLogSource = System.Configuration.ConfigurationManager.AppSettings["EventLogSource"];
            if (tempEventLogSource != null && tempEventLogSource.Length > 0)
            {
                eventLogSource = tempEventLogSource;
            }

            return eventLogSource;
        }

        /// <summary>
        /// Determines the log message priority based on the event type.
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns>Integer priority value (1 through 10)</returns>
        private static int DeterminePriority(TraceEventType eventType)
        {
            int priority = 0;

            switch (eventType)
            {
                case TraceEventType.Critical:
                    priority = 1;
                    break;
                case TraceEventType.Error:
                    priority = 2;
                    break;
                case TraceEventType.Information:
                    priority = 3;
                    break;
                case TraceEventType.Warning:
                    priority = 4;
                    break;
                case TraceEventType.Verbose:
                    priority = 10;
                    break;
                default:
                    priority = 3;
                    break;
            }

            return priority;
        }

        /// <summary>
        /// Determines the log message priority based on the event type.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns>TraceEventType corresponding to the category name</returns>
        private static TraceEventType DetermineTraceEventType(string categoryName)
        {
            TraceEventType type = TraceEventType.Information;

            switch (categoryName)
            {
                case LogManager.CriticalLogCategory:
                    type = TraceEventType.Critical;
                    break;
                case LogManager.ErrorLogCategory:
                    type = TraceEventType.Error;
                    break;
                case LogManager.WarningLogCategory:
                    type = TraceEventType.Warning;
                    break;
                case LogManager.GeneralLogCategory:
                    type = TraceEventType.Information;
                    break;
                case LogManager.VerboseLogCategory:
                    type = TraceEventType.Verbose;
                    break;
                case LogManager.TraceLogCategory:
                    type = TraceEventType.Verbose;
                    break;
            }

            return type;
        }

        /// <summary>
        /// Creates the appropriate event sources.
        /// </summary>
        private static void CreateEventSource()
        {
            string eventLogName = GetEventLogSource();

            if (!EventLog.Exists(AppEventLogName))
            {
                // Create the source, if it does not already exist.
                if (EventLog.SourceExists(eventLogName))
                {
                    EventLog.DeleteEventSource(eventLogName);
                }

                EventSourceCreationData eventSourceData =
                    new EventSourceCreationData(eventLogName, AppEventLogName);

                EventLog.CreateEventSource(eventSourceData);
            }
        }

        #endregion Private Static Methods

        #region Public Static Methods

        #region Message Formatting

        /// <summary>
        /// Method to format a general log message.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="message"></param>
        /// <param name="documentID"></param>
        /// <param name="messageDetails"></param>
        /// <returns></returns>
        public static string FormatLogMessage(string methodName, string message, int documentID, string messageDetails)
        {
            StringBuilder sb = new StringBuilder(methodName);
            sb.Append(": ");
            sb.Append(message);
            sb.Append(";");
            if (documentID > 0)
            {
                sb.Append("DocumentID = ");
                sb.Append(documentID);
                sb.Append(";");
            }
            sb.Append("Details = ");
            sb.Append(messageDetails);

            return sb.ToString();
        }

        /// <summary>
        /// Method to format a SQL exception.
        /// </summary>
        /// <param name="sqlEx"></param>
        /// <returns></returns>
        public static string FormatSqlException(SqlException sqlEx)
        {
            StringBuilder errorDetails = new StringBuilder("SqlException details: ");
            errorDetails.Append(sqlEx.Message);
            errorDetails.Append(";");
            for (int i = 0; i < sqlEx.Errors.Count; i++)
            {
                errorDetails.Append("Index #" + i + "\n" +
                    "Message: " + sqlEx.Errors[i].Message + "\n" +
                    "LineNumber: " + sqlEx.Errors[i].LineNumber + "\n" +
                    "Source: " + sqlEx.Errors[i].Source + "\n" +
                    "Procedure: " + sqlEx.Errors[i].Procedure + "\n");
            }

            return errorDetails.ToString();
        }

        #endregion Message Formatting

        #region Logging 

        /// <summary>
        /// Message logging method.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLogMessage(string message)
        {
            WriteLogMessage(message, "Log Entry", GeneralLogCategory, TraceEventType.Information, null);
        }

        /// <summary>
        /// Message logging method with the ability to support 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventType"></param>
        public static void WriteLogMessage(string message, string categoryName)
        {
            WriteLogMessage(message, "Log Entry", categoryName, DetermineTraceEventType(categoryName), null);
        }

        /// <summary>
        /// Message logging method.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="categoryName"></param>
        /// <param name="eventType"></param>
        /// <param name="eventID"></param>
        public static void WriteLogMessage(string message, string title, string categoryName, TraceEventType eventType)
        {
            WriteLogMessage(message, "Log Entry", categoryName, eventType, null);
        }

        /// <summary>
        /// Message logging method.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="categoryName"></param>
        /// <param name="eventType"></param>
        /// <param name="properties">Dictionary of extended properties</param>
        public static void WriteLogMessage(string message, string title, string categoryName, TraceEventType eventType, Dictionary<string, object> properties)
        {
            _logWriter.Write(
                new LogEntry(message, categoryName, DeterminePriority(eventType), 0, eventType, title, properties));

            // If we go with the app.config based logging use this:
            //Logger.Write(message, "General", DeterminePriority(eventType), eventID, eventType);
        }

        #endregion Logging

        #region Status

        /// <summary>
        /// Method to write document/processing statuses to the GateKeeper database.
        /// </summary>
        /// <param name="documentID"></param>
        /// <param name="message"></param>
        /// <param name="status"></param>
        public static void WriteStatusMessage(int documentID, string message, ProcessStatus status)
        {
            // TODO: May need request id (or requestdataid...etc)

            // UNDONE: Stored procedure usp_WriteStatusMessage

            // UNDONE: implement status message method...
        }

        #endregion Status

        #endregion Public Static Methods
    }
}
