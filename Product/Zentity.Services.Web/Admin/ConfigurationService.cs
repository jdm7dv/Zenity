// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Admin
{
    using System.Security.Permissions;
    using System.ServiceModel;
    using Configuration.Pivot;

    /// <summary>
    /// Configuration Service
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ConfigurationService : IConfigurationService
    {
        #region Private ReadOnly Data Members

        /// <summary>
        /// Host Configuration Helper
        /// </summary>
        private HostConfigurationHelper hostConfigurationHelper;

        /// <summary>
        /// Notification Service Configuration Helper
        /// </summary>
        private NotificationServiceConfigurationHelper notificationServiceConfigurationHelper;

        /// <summary>
        /// Pivot Gallery Configuration Helper
        /// </summary>
        private PivotGalleryConfigurationHelper pivotGalleryConfigurationHelper;

        #endregion

        #region Open Configuration File

        /// <summary>
        /// Opens the Host configuration file for edit
        /// </summary>
        /// <returns>Returns true if the file is opened else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public bool OpenHostConfigurationFile()
        {
            this.hostConfigurationHelper = HostConfigurationHelper.SingletonInstance;
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.OpenHostConfigurationFile();
            }

            return false;
        }

        /// <summary>
        /// Opens the Notification Service Configuration file for edit
        /// </summary>
        /// <param name="pathOfExe">Path of Exe</param>
        /// <returns>Returns true if the file is opened else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public bool OpenNotificationServiceConfigurationFile(string pathOfExe)
        {
            this.notificationServiceConfigurationHelper = NotificationServiceConfigurationHelper.SingletonInstance;
            if (this.notificationServiceConfigurationHelper != null)
            {
                return this.notificationServiceConfigurationHelper.OpenNotificationServiceConfigurationFile(pathOfExe);
            }

            return false;
        }

        /// <summary>
        /// Opens the Pivot Gallery Configuration file for edit
        /// </summary>
        /// <param name="path">Path of the config file</param>
        /// <param name="zentityPivotGallerySite">Pivot Gallery Site Name</param>
        /// <param name="remoteServer">Remote Server Name, null if localhost</param>
        /// <returns>Returns true if the file is opened else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public bool OpenPivotGalleryConfigurationFile(string path, string zentityPivotGallerySite, string remoteServer)
        {
            this.pivotGalleryConfigurationHelper = PivotGalleryConfigurationHelper.SingletonInstance;
            if (this.pivotGalleryConfigurationHelper != null)
            {
                return this.pivotGalleryConfigurationHelper.OpenPivotGalleryConfigurationFile(path, zentityPivotGallerySite, remoteServer);
            }

            return false;
        }

        #endregion

        #region Close Configuration File

        /// <summary>
        /// Saves the Host configuration file and closes it.
        /// </summary>
        /// <returns>Returns true if the file is closed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SaveHostConfigurationFile()
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.CloseHostConfigurationFile();
            }

            return false;
        }

        /// <summary>
        /// Saves the Notification Service Configuration file and closes it.
        /// </summary>
        /// <returns>Returns true if the file is closed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SaveNotificationServiceConfigurationFile()
        {
            if (this.notificationServiceConfigurationHelper != null)
            {
                return this.notificationServiceConfigurationHelper.CloseNotificationServiceConfigurationFile();
            }

            return false;
        }

        /// <summary>
        /// Saves the Pivot Gallery Configuration file and closes it.
        /// </summary>
        /// <returns>Returns true if the file is closed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SavePivotGalleryConfigurationFile()
        {
            if (this.pivotGalleryConfigurationHelper != null)
            {
                return this.pivotGalleryConfigurationHelper.ClosePivotGalleryConfigurationFile();
            }

            return false;
        }

        #endregion

        #region Configure Host Configuration

        /// <summary>
        /// Sets the location of the default deep zoom html template location
        /// </summary>
        /// <param name="location">File location of the default html template location or "default"</param>
        /// <returns>
        /// Returns true if setting is changed else false.
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetDefaultDeepZoomTemplateLocation(string location)
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.UpdateDefaultDeepZoomElementTemplateLocation(location);
            }

            return false;
        }

        /// <summary>
        /// Sets the location of the default deep zoom image location
        /// </summary>
        /// <param name="location">File location of the default deep zoom image</param>
        /// <returns>
        /// Returns true if setting is changed else false.
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetDefaultDeepZoomImageLocation(string location)
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.UpdateDefaultDeepZoomElementImageLocation(location);
            }

            return false;
        }
        
        /// <summary>
        /// Sets the type of Web Capture to be used.
        /// </summary>
        /// <param name="type">Assembly and Class of the WebCapture</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetWebCaptureProviderType(string type)
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.UpdateWebCaptureElement(type);
            }

            return false;
        }

        /// <summary>
        /// Sets the Output To setting to the type specified
        /// </summary>
        /// <param name="type">Output Type</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetOutputToType(OutputType type)
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.UpdateOutputToElment(type);
            }

            return false;
        }

        /// <summary>
        /// Sets the Uri Format to the specified format
        /// </summary>
        /// <param name="format">Uri format</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetUriFormatType(UriFormatType format)
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.UpdateUriFormatElment(format);
            }

            return false;
        }

        /// <summary>
        /// Sets the Base uri for the generation of related collections links
        /// </summary>
        /// <param name="uri">Base uri.</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetBaseUri(string uri)
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.UpdateBaseUriElment(uri);
            }

            return false;
        }

        /// <summary>
        /// Sets the Output Folder where the Cxml's are to be generated
        /// </summary>
        /// <param name="location">Folder location</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetOutputFolderLocation(string location)
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.UpdateOutputFolderElment(location);
            }

            return false;
        }

        /// <summary>
        /// Sets the Split Size of the Collections
        /// </summary>
        /// <param name="value">Split size</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetSplitSize(int value)
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.UpdateSplitSizeElment(value);
            }

            return false;
        }

        /// <summary>
        /// Enables or Disables the generation of related collections
        /// </summary>
        /// <param name="enable">Boolean value</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool EnableRelatedCollections(bool enable)
        {
            if (this.hostConfigurationHelper != null)
            {
                return this.hostConfigurationHelper.UpdateGenerateRelatedCollectionsElment(enable);
            }

            return false;
        }

        /// <summary>
        /// Gets the location where the default deep zoom Html template is located.
        /// </summary>
        /// <returns>
        /// Returns string representation of the location.
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public string GetDefaultDeepZoomTemplateLocation()
        {
            if (PivotConfigurationSection.Instance != null &&
               PivotConfigurationSection.Instance.GlobalSettings != null &&
               PivotConfigurationSection.Instance.GlobalSettings.DefaultDeepZoom != null)
            {
                return PivotConfigurationSection.Instance.GlobalSettings.DefaultDeepZoom.TemplateLocation;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the location where the default deep zoom image is located.
        /// </summary>
        /// <returns>
        /// Returns string representation of the location.
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public string GetDefaultDeepZoomImageLocation()
        {
            if (PivotConfigurationSection.Instance != null &&
               PivotConfigurationSection.Instance.GlobalSettings != null &&
               PivotConfigurationSection.Instance.GlobalSettings.DefaultDeepZoom != null)
            {
                return PivotConfigurationSection.Instance.GlobalSettings.DefaultDeepZoom.ImageLocation;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the WebCapture type
        /// </summary>
        /// <returns>Returns string representation of the type.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public string GetWebCaptureProviderType()
        {
            if (PivotConfigurationSection.Instance != null &&
              PivotConfigurationSection.Instance.GlobalSettings != null &&
              PivotConfigurationSection.Instance.GlobalSettings.WebCapture != null)
            {
                return PivotConfigurationSection.Instance.GlobalSettings.WebCapture.Type;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the Output to type
        /// </summary>
        /// <returns>Returns type</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public OutputType GetOutputToType()
        {
            if (PivotConfigurationSection.Instance != null &&
              PivotConfigurationSection.Instance.GlobalSettings != null &&
              PivotConfigurationSection.Instance.GlobalSettings.OutputSettings != null &&
              PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.OutputTo != null)
            {
                return PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.OutputTo.Type;
            }

            return OutputType.FileSystem;
        }

        /// <summary>
        /// Gets the Uri format type
        /// </summary>
        /// <returns>Returns type</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public UriFormatType GetUriFormatType()
        {
            if (PivotConfigurationSection.Instance != null &&
              PivotConfigurationSection.Instance.GlobalSettings != null &&
              PivotConfigurationSection.Instance.GlobalSettings.OutputSettings != null &&
              PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.UriFormat != null)
            {
                return PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.UriFormat.Format;
            }

            return UriFormatType.Relative;
        }

        /// <summary>
        /// Gets the Base Uri
        /// </summary>
        /// <returns>Returns the string representaion of the base uri</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public string GetBaseUri()
        {
            if (PivotConfigurationSection.Instance != null &&
             PivotConfigurationSection.Instance.GlobalSettings != null &&
             PivotConfigurationSection.Instance.GlobalSettings.OutputSettings != null &&
             PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.BaseUri != null)
            {
                return PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.BaseUri.Uri;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the Ouput folder location where the cxml's are to be generated
        /// </summary>
        /// <returns>Returns the string representation of the output folder location</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public string GetOutputFolderLocation()
        {
            if (PivotConfigurationSection.Instance != null &&
                PivotConfigurationSection.Instance.GlobalSettings != null &&
                PivotConfigurationSection.Instance.GlobalSettings.OutputSettings != null &&
                PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.OutputFolder != null)
            {
                return PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.OutputFolder.Location;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the Split size value
        /// </summary>
        /// <returns>Retuns the split size</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public int GetSplitSize()
        {
            if (PivotConfigurationSection.Instance != null &&
                PivotConfigurationSection.Instance.GlobalSettings != null &&
                PivotConfigurationSection.Instance.GlobalSettings.OutputSettings != null &&
                PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.SplitSize != null)
            {
                return PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.SplitSize.Value;
            }

            return 0;
        }

        /// <summary>
        /// Gets value indicating whether related collections are to be generated or not
        /// </summary>
        /// <returns>Boolean value</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public bool IsRelatedCollectionsEnabled()
        {
            if (PivotConfigurationSection.Instance != null &&
                PivotConfigurationSection.Instance.GlobalSettings != null &&
                PivotConfigurationSection.Instance.GlobalSettings.OutputSettings != null &&
                PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.GenerateRelatedCollections != null)
            {
                return PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.GenerateRelatedCollections.Value;
            }

            return false;
        }

        #endregion

        #region Configure Notification Service Configuraion

        /// <summary>
        /// Sets the Batch Size 
        /// </summary>
        /// <param name="batchSize">Batch size</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetBatchSize(int batchSize)
        {
            if (this.notificationServiceConfigurationHelper != null)
            {
                return this.notificationServiceConfigurationHelper.UpdateBatchSize(batchSize);
            }

            return false;
        }

        /// <summary>
        /// Sets the Time out value
        /// </summary>
        /// <param name="timeOut">Time out</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetTimeOut(int timeOut)
        {
            if (this.notificationServiceConfigurationHelper != null)
            {
                return this.notificationServiceConfigurationHelper.UpdateTimeOut(timeOut);
            }

            return false;
        }

        /// <summary>
        /// Gets the batch size
        /// </summary>
        /// <returns>Returns string representation of the batch size</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public int GetBatchSize()
        {
            if (this.notificationServiceConfigurationHelper != null)
            {
                return this.notificationServiceConfigurationHelper.GetBatchSize();
            }

            return 0;
        }

        /// <summary>
        /// Gets the Time out value
        /// </summary>
        /// <returns>Returns string representaion of the time out value</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public int GetTimeOut()
        {
            if (this.notificationServiceConfigurationHelper != null)
            {
                return this.notificationServiceConfigurationHelper.GetTimeOut();
            }

            return 0;
        }

        #endregion

        #region Configure Pivot Gallery Configuration

        /// <summary>
        /// Sets the Collection file path
        /// </summary>
        /// <param name="collectionFilePath">Collection file path</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetCollectionFilePath(string collectionFilePath)
        {
            if (this.pivotGalleryConfigurationHelper != null)
            {
                return this.pivotGalleryConfigurationHelper.UpdateCollectionFilePath(collectionFilePath);
            }

            return false;
        }

        /// <summary>
        /// Sets the path prefix
        /// </summary>
        /// <param name="pathPrefix">Path Prefix</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool SetPathPrefix(string pathPrefix)
        {
            if (this.pivotGalleryConfigurationHelper != null)
            {
                return this.pivotGalleryConfigurationHelper.UpdatePathPrefix(pathPrefix);
            }

            return false;
        }

        /// <summary>
        /// Gets the collection file path
        /// </summary>
        /// <returns>Returns string represenation of the Collection file path</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public string GetCollectionFilePath()
        {
            if (this.pivotGalleryConfigurationHelper != null)
            {
                return this.pivotGalleryConfigurationHelper.GetCollectionFilePath();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the path prefix
        /// </summary>
        /// <returns>Path Prefix</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public string GetPathPrefix()
        {
            if (this.pivotGalleryConfigurationHelper != null)
            {
                return this.pivotGalleryConfigurationHelper.GetPathPrefix();
            }

            return string.Empty;
        }

        #endregion
    }
}
