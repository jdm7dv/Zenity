// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Windows
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceProcess;
    using System.Security.Permissions;

    /// <summary>
    /// Window service to listen the queue and fetch the messages.
    /// </summary>
    public partial class QueueListener : ServiceBase 
    {
        private const int DefaultBatchSize = 500;
        private const int DefaultTimeOut = 30000;
        private readonly Worker queueWorker;
        private readonly MessagePublisher messagePublisher;

        /// <summary>
        /// Initializes a new instance of the QueueListener class.
        /// </summary>
        public QueueListener()
        {
            InitializeComponent();

            try
            {
                Globals.TraceMessage(TraceEventType.Information, string.Empty, "Zentity Notification Service - Starting initialization.");
                
                string sqlConStr = Globals.ZentityConnectionString;
                string queueName = ConfigurationManager.AppSettings["ZentityQueue"];
                int batchSize, timeOut;

                if (!int.TryParse(ConfigurationManager.AppSettings["BatchSize"], NumberStyles.Integer, CultureInfo.InvariantCulture, out batchSize) || batchSize < 1)
                {
                    batchSize = DefaultBatchSize;
                }

                if (!int.TryParse(ConfigurationManager.AppSettings["Timeout"], NumberStyles.Integer, CultureInfo.InvariantCulture, out timeOut) || timeOut < 1)
                {
                    timeOut = DefaultTimeOut;
                }
                else if (timeOut < DefaultTimeOut)
                {
                    timeOut = DefaultTimeOut;
                }

                if (!(string.IsNullOrWhiteSpace(sqlConStr) && string.IsNullOrWhiteSpace(queueName)))
                {
                    this.queueWorker = new Worker(sqlConStr, queueName, batchSize, timeOut);
                    this.queueWorker.NotifyMessage += this.QueueWorkerNotifyMessage;
                }

                this.messagePublisher = new MessagePublisher();

                Globals.TraceMessage(TraceEventType.Information, string.Empty, "Zentity Notification Service - Completed initialization.");
            }
            catch (ConfigurationErrorsException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Error in application configuration. Please check the config file and correct the errors.");
            }
            catch (ArgumentException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Error applying configuration values. Please check the config file and correct the errors.");
            }
        }

        /// <summary>
        /// Starts this instance. Only used for interactive debug purposes.
        /// </summary>
        public void Start()
        {
            this.OnStart(null);
        }

        /// <summary>
        /// Event handler to handle NotifyMessage event of Worker class.
        /// </summary>
        /// <param name="sender">Instance of Worker class.</param>
        /// <param name="args">Notify event arguments</param>
        private void QueueWorkerNotifyMessage(object sender, NotifyEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.Message))
            {
                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, "Pushing the broker messages to internal message queue.");
                this.messagePublisher.PublishMessage(args.Message);
            }
        }

        /// <summary>
        /// Executes when Start command is sent to service.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected override void OnStart(string[] args)
        {
            this.queueWorker.Start();
        }

        /// <summary>
        /// Executes when Stop command is sent to service.
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void OnStop()
        {
            this.queueWorker.NotifyMessage -= this.QueueWorkerNotifyMessage;
            this.queueWorker.Stop();
            this.messagePublisher.OnClose();
        }

    }
}
