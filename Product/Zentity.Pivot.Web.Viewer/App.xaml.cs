// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Pivot.Web.Viewer
{
    using System;
    using System.Windows;

    /// <summary>
    /// Code behind of App.xaml
    /// Inherits the Application class
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the App class
        /// </summary>
        public App()
        {
            this.Startup += this.Application_Startup;            
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        /// <summary>
        /// Application_Startup event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Startup event</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.RootVisual = new PivotPage();
        }

        /// <summary>
        /// Application_UnhandledException event handler
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">Event data for Unhandled exception</param>
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { this.ReportErrorToDOM(e); });
            }
        }

        /// <summary>
        /// Report Error to Html document
        /// </summary>
        /// <param name="e">Event data for Unhandled exception</param>
        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
