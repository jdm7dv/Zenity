// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Collections;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;

namespace Zentity.Services.Windows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Threading;
    using Pivot;

    /// <summary>
    /// Class of type EventArgs used to propogate the
    /// informative Messages through notfication event.
    /// </summary>
    public class NotifyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NotifyEventArgs(string message)
        {
            this.Message = message;
        }
    }

    /// <summary>
    /// Notify message delegate.
    /// </summary>
    public delegate void NotifyMessageDelegate(object sender, NotifyEventArgs args);

    /// <summary>
    /// Worker class
    /// </summary>
    internal class Worker : IDisposable
    {
        private readonly BackgroundWorker workerThread;
        private SqlConnection sqlCon;
        private SqlCommand sqlCmd;
        private readonly string sqlCmdStr = "Zentity.Core.FetchMessage";
        private readonly string sqlConStr;
        private readonly string queueName;
        private readonly int timeOutValue;
        private readonly int batchSize;
        private bool disposed;
        private PublishingServiceClient publishingProxy;
        internal static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Event to notify whenever a message is received.
        /// </summary>
        public event NotifyMessageDelegate NotifyMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="sqlConnectionString">sql connection string.</param>
        /// <param name="queueName">Sql broker queue to connect.</param>
        /// <param name="batchSize">Count of messages to be fetched in a single call.</param>
        /// <param name="timeOutValue">The time out value.</param>
        internal Worker(string sqlConnectionString, string queueName, int batchSize, int timeOutValue)
        {
            this.sqlConStr = sqlConnectionString;
            this.queueName = queueName;
            this.batchSize = batchSize <= 0 ? 1 : batchSize;
            this.timeOutValue = timeOutValue;
            this.sqlCmdStr = string.Format(this.sqlCmdStr, this.batchSize, queueName);
            this.publishingProxy = new PublishingServiceClient("WSHttpBinding_IPublishingService");
            
            // Start a background worker to fetch broker data from the DB.
            this.workerThread = new BackgroundWorker();
            this.workerThread.WorkerSupportsCancellation = true;
            this.workerThread.DoWork += this.WorkerThreadDoWork;
        }

        /// <summary>
        ///  Method to start the worker.
        /// </summary>
        internal void Start()
        {
            if (this.workerThread != null)
            {
                this.workerThread.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Method to stop the worker.
        /// </summary>
        internal void Stop()
        {
            if (this.workerThread != null)
            {
                this.workerThread.CancelAsync();
            }
        }

        /// <summary>
        /// Event handler for DoWork event of BackgroundWorker thread.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">DoWorkEventArgs args.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification="This string is not set by user input.")]
        private void WorkerThreadDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!(string.IsNullOrWhiteSpace(sqlConStr) && string.IsNullOrWhiteSpace(queueName)))
                {
                    while (!e.Cancel)
                    {
                        string brokerMessage;
                        using (sqlCon = new SqlConnection(sqlConStr))
                        {
                            sqlCon.Open();

                            // Call the fetch stored proc.
                            using (sqlCmd = new SqlCommand())
                            {
                                sqlCmd.Connection = sqlCon;
                                sqlCmd.CommandTimeout = sqlCon.ConnectionTimeout;
                                sqlCmd.CommandText = this.sqlCmdStr;
                                sqlCmd.CommandType = CommandType.StoredProcedure;
                                
                                // Initialize the parameters for the Stored Procedure
                                SqlParameter param1 = sqlCmd.Parameters.Add(new SqlParameter("@Msg", SqlDbType.NVarChar, -1));
                                param1.Direction = ParameterDirection.Output;
                                SqlParameter param2 = sqlCmd.Parameters.Add(new SqlParameter("@BatchSize", SqlDbType.Int));
                                param2.Value = this.batchSize;
                                SqlParameter param3 = sqlCmd.Parameters.Add(new SqlParameter("@Timeout", SqlDbType.BigInt));
                                param3.Value = 5000;

                                // Fire the sql command on the server and raise events.
                                brokerMessage = this.Listen();
                                this.OnNotifyMessage(brokerMessage);
                            }
                        }

                        // If the message is empty then there are no messages in the broker queue. 
                        // The Notification service will go into sleep mode for the timeout value
                        if (string.IsNullOrWhiteSpace(brokerMessage))
                        {
                            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, string.Format(TraceMessages.BrokerQueueExhausted, timeOutValue));
                            Thread.Sleep(timeOutValue);

                            // Ping the publishing service. Check for running publish operations.
                            // If publishing is running then dont proceed to SQL Server. Otherwise get new messages from broker queue.
                            while (true)
                            {
                                try
                                {
                                    List<PublishStatus> publishStatusTracker = this.publishingProxy.GetAllPublishingStatus();
                                    if (publishStatusTracker != null)
                                    {
                                        var statusList = from item in publishStatusTracker
                                                         where (item.CurrentStage != PublishStage.Completed &&
                                                                item.CurrentStage != PublishStage.AbortedOnError &&
                                                                item.CurrentStage != PublishStage.AbortedOnDemand)
                                                         select item;
                                        if (statusList.Count() == 0)
                                        {
                                            break;
                                        }
                                        
                                        Globals.TraceMessage(TraceEventType.Verbose, string.Empty, string.Format(TraceMessages.PublishingServiceBusyGoingToSleep, RetryInterval.TotalSeconds));
                                    }
                                    Thread.Sleep(RetryInterval);
                                }
                                catch (TimeoutException ex)
                                {
                                    Globals.TraceMessage(TraceEventType.Error, ex.Message, string.Format(TraceMessages.PublishingServiceEndpointUnavailableAfterTimeout));
                                }
                                catch (EndpointNotFoundException ex)
                                {
                                    Globals.TraceMessage(TraceEventType.Error, ex.Message, string.Format(TraceMessages.PublishingServiceEndpointUnavailableWithSuggestion, RetryInterval.Minutes));
                                    this.publishingProxy = RecreatePublishingProxy(this.publishingProxy);
                                }
                                catch (Exception ex)
                                {
                                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), string.Format(TraceMessages.ExceptionThrownByPublishingService, RetryInterval.Minutes));
                                    this.publishingProxy = RecreatePublishingProxy(this.publishingProxy);
                                }
                            }
                        }
                    }
                }
            }
           
            catch (SqlException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), TraceMessages.ExceptionCallingStoredProcedure);
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), TraceMessages.ExceptionInWorkerThread);
            }
        }

        /// <summary>
        /// Method fires the NotifyMessage event.
        /// </summary>
        /// <param name="message">Message from the sql broker queue.</param>
        private void OnNotifyMessage(string message)
        {
            if (this.NotifyMessage != null)
            {
                Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(TraceMessages.ReceivedMessagesFromSqlBrokerQueue,message.Length));
                NotifyMessage(this, new NotifyEventArgs(message));
            }
        }

        /// <summary>
        /// This Method listens to the sql broker queue for messages.
        /// it executes 'Core.FetchMessage' sql proc, which has a blocking call on sql server.
        /// and keeps the current service in blocked state untill given number of messages are got.
        /// </summary>
        /// <returns>Messages from the sql broker queue.</returns>
        private string Listen()
        {
            string message = string.Empty;
            try
            {
                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, string.Format(TraceMessages.FetchingMessagesFromSqlBrokerQueue, sqlCmd.CommandText));
                sqlCmd.ExecuteNonQuery();
                message = sqlCmd.Parameters["@Msg"].Value.ToString();
                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, TraceMessages.FetchedMessagesFromSqlBroker);
            }
            catch (SqlException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), TraceMessages.ExceptionCallingStoredProcedure);
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), TraceMessages.ExceptionInWorkerThread);
            }

            return message;
        }

        /// <summary>
        /// Recreates the publishing proxy after loading the settings againg from the config file
        /// </summary>
        /// <param name="publishServiceProxy">The publish service proxy.</param>
        /// <returns>A newly created publishing proxy client</returns>
        internal static PublishingServiceClient RecreatePublishingProxy(PublishingServiceClient publishServiceProxy)
        {
            try
            {
                // Close the communication channel
                if (publishServiceProxy.State != CommunicationState.Closed)
                    publishServiceProxy.Close();

                // Wait for retry interval before retrying.
                Thread.Sleep(RetryInterval);

                // Reload the configuration for the service
                ConfigurationManager.RefreshSection("system.serviceModel/client");
                ClientSection clientSection = ((ClientSection) ConfigurationManager.GetSection("system.serviceModel/client"));
                return new PublishingServiceClient("WSHttpBinding_IPublishingService", clientSection.Endpoints[0].Address.AbsoluteUri);
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), TraceMessages.ExceptionCreatingPublishingServiceProxy);
            }

            return new PublishingServiceClient("WSHttpBinding_IPublishingService");
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose the publisher.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.workerThread.Dispose();
                    this.publishingProxy.Close();
                }
            }
            else
            {
                this.disposed = true;
            }
        }

        #endregion
    }
}
