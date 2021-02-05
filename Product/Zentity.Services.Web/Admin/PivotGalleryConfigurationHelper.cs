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
    using System.Web.Configuration;

    /// <summary>
    /// Pivot Gallery Configuration Helper
    /// </summary>
    internal class PivotGalleryConfigurationHelper
    {
        /// <summary>
        /// Singleton instance property
        /// </summary>
        private static volatile PivotGalleryConfigurationHelper instance;

        /// <summary>
        /// PivotGallery Configuration property
        /// </summary>
        private Configuration pivotGalleryConfiguration;

        /// <summary>
        /// Lock Object used for synchronization
        /// </summary>
        private static readonly object LockObject = new Object();

        /// <summary>
        /// Prevents a default instance of the PivotGalleryConfigurationHelper class from being created.
        /// </summary>
        private PivotGalleryConfigurationHelper()
        {
        }

        /// <summary>
        /// Gets the Singleton Instance of PivotGallery Configuration Helper
        /// </summary>
        public static PivotGalleryConfigurationHelper SingletonInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (LockObject)
                    {
                        if (instance == null)
                        {
                            instance = new PivotGalleryConfigurationHelper();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Opens the Pivot Gallery Configuration file for edit
        /// </summary>
        /// <param name="path">Path of the config file</param>
        /// <param name="zentityPivotGallerySite">Pivot Gallery Site Name</param>
        /// <param name="remoteServer">Remote Server Name, null if localhost</param>
        /// <returns>Returns true if the file is opened else false.</returns>
        internal bool OpenPivotGalleryConfigurationFile(string path, string zentityPivotGallerySite, string remoteServer)
        {
            try
            {
                this.pivotGalleryConfiguration = WebConfigurationManager.OpenWebConfiguration(path, zentityPivotGallerySite, null, remoteServer);
                return true;
            }
            catch (Exception exception)
            {
                throw new FaultException(exception.ToString());
                //throw new FaultException<Exception>(exception, exception.Message);
            }
        }

        /// <summary>
        /// Saves the Pivot Gallery Configuration file and closes it.
        /// </summary>
        /// <returns>Returns true if the file is closed else false.</returns>
        internal bool ClosePivotGalleryConfigurationFile()
        {
            try
            {
                this.pivotGalleryConfiguration.Save(ConfigurationSaveMode.Modified);
                return true;
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.PivotGalleryConfigurationFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("The Pivot Gallery Configuration File is not opened."));
            }
        }

        /// <summary>
        /// Sets the Collection file path
        /// </summary>
        /// <param name="collectionFilePath">Collection file Path</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateCollectionFilePath(string collectionFilePath)
        {
            try
            {
                WebContext context = null;
                if (this.pivotGalleryConfiguration.EvaluationContext != null)
                {
                    context = (WebContext)this.pivotGalleryConfiguration.EvaluationContext.HostingContext; 
                }
                if (context == null)
                {
                    return false;
                }

                this.UpdateConfigFile(this.GetCollectionFilePath(), collectionFilePath);
                this.pivotGalleryConfiguration.AppSettings.Settings["CollectionFilePath"].Value = collectionFilePath;
                string pathPrefix = this.GetPathPrefix();

                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.FileName = Environment.SystemDirectory + @"\inetsrv\appcmd.exe";
                    string input = "set vdir /vdir.name:" + "\"" + context.Site + context.ApplicationPath + "/" + pathPrefix + "\"" + " /physicalPath:" + "\"" + collectionFilePath +
                                   "\"";
                    process.StartInfo.Arguments = input;
                    process.Start();
                    process.WaitForExit();
                }

                return true;
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.PivotGalleryConfigurationFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Pivot Gallery Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Copies the Web.config file to the new Collections file path
        /// </summary>
        /// <param name="oldCollectionsFilePath">Old Collections file path present in the config file.</param>
        /// <param name="newCollectionsFilePath">New Collections file path provided by the user.</param>
        /// <returns>Returns true if copy succeeds else false.</returns>
        private bool UpdateConfigFile(string oldCollectionsFilePath, string newCollectionsFilePath)
        {
            try
            {
                if (System.IO.Directory.Exists(oldCollectionsFilePath) && System.IO.Directory.Exists(newCollectionsFilePath))
                {
                    if (System.IO.File.Exists(System.IO.Path.Combine(oldCollectionsFilePath, "Web.config")))
                    {
                        System.IO.File.Copy(System.IO.Path.Combine(oldCollectionsFilePath, "Web.config"), System.IO.Path.Combine(newCollectionsFilePath, "Web.config"), true);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Sets the path prefix
        /// </summary>
        /// <param name="pathPrefix">Path Prefix</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdatePathPrefix(string pathPrefix)
        {
            try
            {
                this.pivotGalleryConfiguration.AppSettings.Settings["PathPrefix"].Value = pathPrefix;
                return true;
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.PivotGalleryConfigurationFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Pivot Gallery Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Gets the collection file path
        /// </summary>
        /// <returns>Returns string represenation of the Collection file path</returns>
        internal string GetCollectionFilePath()
        {
            try
            {
                return this.pivotGalleryConfiguration.AppSettings.Settings["CollectionFilePath"].Value;
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.PivotGalleryConfigurationFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Notification Service Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Gets the path prefix
        /// </summary>
        /// <returns>Path Prefix</returns>
        internal string GetPathPrefix()
        {
            try
            {
                return this.pivotGalleryConfiguration.AppSettings.Settings["PathPrefix"].Value;
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.PivotGalleryConfigurationFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Notification Service Configuration File is not Opened."));
            }
        }
    }
}
