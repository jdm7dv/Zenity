// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    /// <summary>
    /// Global that is across the service.
    /// </summary>
    public static class Globals
    {
        #region Public Constants

        /// <summary>
        /// Min threads per core to do the processing.
        /// </summary>
        public const int MinThreadsPerCore = 2;

        /// <summary>
        /// Max threads per core to do the processing.
        /// </summary>
        public const int MaxThreadsPerCore = 30;

        /// <summary>
        /// Maximum retries before any operation is cancelled.
        /// </summary>
        public const int MaxRetryValue = 2;

        /// <summary>
        /// Invalid range of characters as per W3 standards. http://www.w3.org/TR/2004/REC-xml-20040204/#charsets
        /// </summary>
        public const string InvalidCharacterRangeBase64 = "KD88IVtcdUQ4MDAtXHVEQkZGXSlbXHVEQzAwLVx1REZGRl18W1x1RDgwMC1cdURCRkZdKD8hW1x1REMwMC1cdURGRkZdKXxbXHgwMC1ceDA4XHgwQlx4MENceDBFLVx4MUZceDdGLVx4OUZcdUZFRkZcdUZGRkVcdUZGRkZd";

        #endregion

        #region Static Constructor

        /// <summary>
        /// Initializes static members of the <see cref="Globals"/> class.
        /// </summary>
        static Globals()
        {
            ThreadPool.SetMaxThreads(MaxThreadsPerCore, MaxThreadsPerCore);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => TraceMessage(TraceEventType.Error, e.ExceptionObject.ToString(), Properties.Messages.UnhandledException);
            InvalidCharacterRange = Encoding.UTF8.GetString(Convert.FromBase64String(InvalidCharacterRangeBase64));
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Logs message to the Log file.
        /// </summary>
        /// <param name="severity">The type of trace event</param>
        /// <param name="message">The main body of the log message.</param>
        /// <param name="title">The title of the log message</param>
        public static void TraceMessage(TraceEventType severity, string message, string title)
        {
            StringBuilder customMessage = new StringBuilder();
            try
            {
                StackTrace stackTrace = new StackTrace();
                StackFrame stackFrame = stackTrace.GetFrame(1);
                MethodBase callingMethod = stackFrame.GetMethod();

                if (severity == TraceEventType.Error)
                {
                    customMessage.AppendLine("Parameter Information");
                    foreach (ParameterInfo paramInfo in callingMethod.GetParameters())
                    {
                        customMessage.AppendFormat("\t{0} : {1}", paramInfo.Name, paramInfo.ParameterType.Name);
                        customMessage.AppendLine();
                    }
                }

                title = string.Format("{0} :: {1} - {2}", severity, callingMethod.Name, title);
            }
            catch
            {
            }
            
            customMessage.AppendLine();
            customMessage.Append(message);

            LogEntry zentityLogEntry = new LogEntry { Severity = severity, Title = title, Message = customMessage.ToString() };
            Logger.Write(zentityLogEntry);
        }

        /// <summary>
        /// Cleanups the invalid XML characters from the source text.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <returns>Cleaned up text</returns>
        public static string CleanupInvalidXmlCharacters(string sourceText)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(sourceText, InvalidCharacterRange, string.Empty);
        }

        #endregion

        #region Private Fields

        private static readonly string InvalidCharacterRange;

        #endregion
    }

    /// <summary>
    /// Return codes for the executable
    /// </summary>
    public enum ReturnCode
    {
        /// <summary>
        /// Return code for successful operation
        /// </summary>
        Success = 0,

        /// <summary>
        /// Return code for an invalid argument / switch entered by the user.
        /// </summary>
        Invalid = -100,

        /// <summary>
        /// Return code for an error situation.
        /// </summary>
        Error = -200
    }
}
