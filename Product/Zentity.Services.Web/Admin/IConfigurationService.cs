// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Admin
{
    using System;
    using System.ServiceModel;
    using Zentity.Services.Configuration.Pivot;

    /// <summary>
    /// IConfigurationService
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IConfigurationService
    {
        #region Open Configuration File

        /// <summary>
        /// Opens the Host configuration file for edit
        /// </summary>
        /// <returns>Returns true if the file is opened else false.</returns>
        [OperationContract]
        bool OpenHostConfigurationFile();

        /// <summary>
        /// Opens the Notification Service Configuration file for edit
        /// </summary>
        /// <param name="pathOfExe">Path of the exe</param>
        /// <returns>Returns true if the file is opened else false.</returns>
        [OperationContract]
        bool OpenNotificationServiceConfigurationFile(string pathOfExe);

        /// <summary>
        /// Opens the Pivot Gallery Configuration file for edit
        /// </summary>
        /// <param name="path">Path of the config file</param>
        /// <param name="zentityPivotGallerySite">Pivot Gallery Site Name</param>
        /// <param name="remoteServer">Remote Server Name, null if localhost</param>
        /// <returns>Returns true if the file is opened else false.</returns>
        [OperationContract]
        bool OpenPivotGalleryConfigurationFile(string path, string zentityPivotGallerySite, string remoteServer);

        #endregion

        #region Close Configuration File

        /// <summary>
        /// Saves the Host configuration file and closes it.
        /// </summary>
        /// <returns>Returns true if the file is closed else false.</returns>
        [OperationContract]
        bool SaveHostConfigurationFile();

        /// <summary>
        /// Saves the Notification Service Configuration file and closes it.
        /// </summary>
        /// <returns>Returns true if the file is closed else false.</returns>
        [OperationContract]
        bool SaveNotificationServiceConfigurationFile();
        
        /// <summary>
        /// Saves the Pivot Gallery Configuration file and closes it.
        /// </summary>
        /// <returns>Returns true if the file is closed else false.</returns>
        [OperationContract]
        bool SavePivotGalleryConfigurationFile();

        #endregion

        #region ServiceHost Configuration

        /// <summary>
        /// Sets the location of the default deep zoom html template location
        /// </summary>
        /// <param name="location">File location of the default html template location or "default"</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        [FaultContract(typeof(System.IO.DirectoryNotFoundException))]
        bool SetDefaultDeepZoomTemplateLocation(string location);

        /// <summary>
        /// Sets the location of the default deep zoom image location
        /// </summary>
        /// <param name="location">File location of the default deep zoom image</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        [FaultContract(typeof(System.IO.DirectoryNotFoundException))]
        bool SetDefaultDeepZoomImageLocation(string location);

        /// <summary>
        /// Sets the type of Web Capture to be used.
        /// </summary>
        /// <param name="type">Assembly and Class of the WebCapture</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        bool SetWebCaptureProviderType(string type);

        /// <summary>
        /// Sets the Output To setting to the type specified
        /// </summary>
        /// <param name="type">Output Type</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        bool SetOutputToType(OutputType type);

        /// <summary>
        /// Sets the Uri Format to the specified format
        /// </summary>
        /// <param name="format">Uri format</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        bool SetUriFormatType(UriFormatType format);

        /// <summary>
        /// Sets the Base uri for the generation of related collections links
        /// </summary>
        /// <param name="uri">Base uri.</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        bool SetBaseUri(string uri);

        /// <summary>
        /// Sets the Output Folder where the Cxml's are to be generated
        /// </summary>
        /// <param name="location">Folder location</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        bool SetOutputFolderLocation(string location);

        /// <summary>
        /// Sets the Split Size of the Collections
        /// </summary>
        /// <param name="value">Split size</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        bool SetSplitSize(int value);

        /// <summary>
        /// Enables or Disables the generation of related collections
        /// </summary>
        /// <param name="enable">Boolean value</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        bool EnableRelatedCollections(bool enable);

        /// <summary>
        /// Gets the location where the default deep zoom Html template is located.
        /// </summary>
        /// <returns>Returns string representation of the location.</returns>
        [OperationContract]
        string GetDefaultDeepZoomTemplateLocation();

        /// <summary>
        /// Gets the location where the default deep zoom image is located.
        /// </summary>
        /// <returns>Returns string representation of the location.</returns>
        [OperationContract]
        string GetDefaultDeepZoomImageLocation();

        /// <summary>
        /// Gets the WebCapture type
        /// </summary>
        /// <returns>Returns string representation of the type.</returns>
        [OperationContract]
        string GetWebCaptureProviderType();

        /// <summary>
        /// Gets the Output to type
        /// </summary>
        /// <returns>Returns type</returns>
        [OperationContract]
        OutputType GetOutputToType();

        /// <summary>
        /// Gets the Uri format type
        /// </summary>
        /// <returns>Returns type</returns>
        [OperationContract]
        UriFormatType GetUriFormatType();

        /// <summary>
        /// Gets the Base Uri
        /// </summary>
        /// <returns>Returns the string representaion of the base uri</returns>
        [OperationContract]
        string GetBaseUri();

        /// <summary>
        /// Gets the Ouput folder location where the cxml's are to be generated
        /// </summary>
        /// <returns>Returns the Output folder location</returns>
        [OperationContract]
        string GetOutputFolderLocation();

        /// <summary>
        /// Gets the Split size value
        /// </summary>
        /// <returns>Retuns the split size</returns>
        [OperationContract]
        int GetSplitSize();

        /// <summary>
        /// Gets value indicating whether related collections are to be generated or not
        /// </summary>
        /// <returns>Boolean value</returns>
        [OperationContract]
        bool IsRelatedCollectionsEnabled(); 

        #endregion

        #region Notification Service Configuration

        /// <summary>
        /// Sets the Batch Size 
        /// </summary>
        /// <param name="batchSize">Batch size</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        bool SetBatchSize(int batchSize);

        /// <summary>
        /// Sets the Time out value
        /// </summary>
        /// <param name="timeOut">Time out</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        bool SetTimeOut(int timeOut);

        /// <summary>
        /// Gets the batch size
        /// </summary>
        /// <returns>Returns string representation of the batch size</returns>
        [OperationContract]
        int GetBatchSize();

        /// <summary>
        /// Gets the Time out value
        /// </summary>
        /// <returns>Returns string representaion of the time out value</returns>
        [OperationContract]
        int GetTimeOut();

        #endregion

        #region Pivot Gallery Configuration

        /// <summary>
        /// Sets the Collection file path
        /// </summary>
        /// <param name="collectionFilePath">Collection file Path</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        bool SetCollectionFilePath(string collectionFilePath);

        /// <summary>
        /// Sets the path prefix
        /// </summary>
        /// <param name="pathPrefix">Path Prefix</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        [OperationContract]
        bool SetPathPrefix(string pathPrefix);

        /// <summary>
        /// Gets the collection file path
        /// </summary>
        /// <returns>Returns string represenation of the Collection file path</returns>
        [OperationContract]
        string GetCollectionFilePath();

        /// <summary>
        /// Gets the path prefix
        /// </summary>
        /// <returns>Path Prefix</returns>
        [OperationContract]
        string GetPathPrefix();

        #endregion
    }
}
