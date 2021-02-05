// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Data
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    /// <summary>
    /// Global that is across the service.
    /// </summary>
    public static class Globals
    {
        #region Static Constructor

        /// <summary>
        /// Initializes static members of the <see cref="Globals"/> class.
        /// </summary>
        static Globals()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => TraceMessage(TraceEventType.Error, e.ExceptionObject.ToString(), "Unhandled Exception");
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Logs message to the Log file.
        /// </summary>
        /// <param name="severity">The typeof Trace Message</param>
        /// <param name="message">The Message to log.</param>
        /// <param name="title">The title.</param>
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

            LogEntry zentityLogEntry = new LogEntry
                                                {
                                                    Severity = severity,
                                                    Title = title,
                                                    Message = customMessage.ToString()
                                                };

            Logger.Write(zentityLogEntry);
        }

        #endregion
    }
}
