// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Admin
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    /// <summary>
    /// Notification Service Configuration Helper
    /// </summary>
    internal class NotificationServiceConfigurationHelper
    {
        /// <summary>
        /// Singleton instance property
        /// </summary>
        private static volatile NotificationServiceConfigurationHelper instance;

        /// <summary>
        /// NotificationService Configuration property
        /// </summary>
        private System.Configuration.Configuration notificationServiceConfiguration;

        /// <summary>
        /// Lock Object used for synchronization
        /// </summary>
        private static readonly object LockObject = new Object();

        /// <summary>
        /// Prevents a default instance of the NotificationServiceConfigurationHelper class from being created.
        /// </summary>
        private NotificationServiceConfigurationHelper()
        {
        }

        /// <summary>
        /// Gets the Singleton Instance of Notification Service Configuration Helper
        /// </summary>
        public static NotificationServiceConfigurationHelper SingletonInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (LockObject)
                    {
                        if (instance == null)
                        {
                            instance = new NotificationServiceConfigurationHelper();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Opens the Notification Service Configuration file for edit
        /// </summary>
        /// <param name="pathOfExe">Path of the exe</param>
        /// <returns>Returns true if the file is opened else false.</returns>
        internal bool OpenNotificationServiceConfigurationFile(string pathOfExe)
        {
            try
            {
                System.IO.File.Exists(pathOfExe);
                this.notificationServiceConfiguration = ConfigurationManager.OpenExeConfiguration(pathOfExe);
                return true;
            }
            catch (System.Exception exception)
            {
                throw new FaultException(exception.ToString());
                //throw new FaultException<Exception>(exception, exception.Message);
            }
        }

        /// <summary>
        /// Saves the Notification Service Configuration file and closes it.
        /// </summary>
        /// <returns>Returns true if the file is closed else false.</returns>
        internal bool CloseNotificationServiceConfigurationFile()
        {
            try
            {
                this.notificationServiceConfiguration.Save(ConfigurationSaveMode.Modified, true);
                return true;
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.PivotGalleryConfigurationFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Notification Service Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Sets the Batch Size 
        /// </summary>
        /// <param name="batchSize">Batch size</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateBatchSize(int batchSize)
        {
            if (batchSize < 5 && batchSize > 500)
            {
                throw new FaultException(new FaultReason(Properties.Messages.ValueOutOfRange));
            }

            try
            {
                this.notificationServiceConfiguration.AppSettings.Settings["BatchSize"].Value = batchSize.ToString();
                return true;
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.NotificationServiceConfigFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Notification Service Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Sets the Time out value
        /// </summary>
        /// <param name="timeOut">Time out</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateTimeOut(int timeOut)
        {
            if (timeOut < 30000)
            {
                throw new FaultException(new FaultReason(Properties.Messages.TimeoutOutOfRange));
            }

            try
            {
                this.notificationServiceConfiguration.AppSettings.Settings["TimeOut"].Value = timeOut.ToString();
                return true;
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.NotificationServiceConfigFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Notification Service Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Gets the batch size
        /// </summary>
        /// <returns>Returns string representation of the batch size</returns>
        internal int GetBatchSize()
        {
            try
            {
                string batchSize = this.notificationServiceConfiguration.AppSettings.Settings["BatchSize"].Value;
                return Convert.ToInt32(batchSize);
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.NotificationServiceConfigFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Notification Service Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Gets the Time out value
        /// </summary>
        /// <returns>Returns string representaion of the time out value</returns>
        internal int GetTimeOut()
        {
            try
            {
                string timeOut = this.notificationServiceConfiguration.AppSettings.Settings["TimeOut"].Value;
                return Convert.ToInt32(timeOut);
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.NotificationServiceConfigFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Notification Service Configuration File is not Opened."));
            }
        }
    }
}
