// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    public static class Globals
    {
        #region Static Constructor

        /// <summary>
        /// Initializes the <see cref="Globals"/> class.
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
        /// <param name="title"></param>
        public static void TraceMessage(TraceEventType severity, string message, string title)
        {
            StringBuilder sbCustomMessage = new StringBuilder();
            try
            {
                StackTrace stackTrace = new StackTrace();
                StackFrame stackFrame = stackTrace.GetFrame(1);
                MethodBase callingMethod = stackFrame.GetMethod();

                if (severity == TraceEventType.Error)
                {
                    sbCustomMessage.AppendLine("Parameter Information");
                    foreach (ParameterInfo paramInfo in callingMethod.GetParameters())
                    {
                        sbCustomMessage.AppendFormat("\t{0} : {1}", paramInfo.Name, paramInfo.ParameterType.Name);
                        sbCustomMessage.AppendLine();
                    }
                }

                title = string.Format("{0} :: {1} - {2}", severity, callingMethod.Name, title);
            }
            catch
            {
            }

            sbCustomMessage.AppendLine();
            sbCustomMessage.Append(message);

            LogEntry zentityLogEntry = new LogEntry
            {
                Severity = severity,
                Title = title,
                Message = sbCustomMessage.ToString()
            };

            Logger.Write(zentityLogEntry);
        }

        #endregion
    }
}