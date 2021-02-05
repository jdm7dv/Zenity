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
    /// Host Configuration Helper
    /// </summary>
    internal class HostConfigurationHelper
    {
        /// <summary>
        /// Singleton instance property
        /// </summary>
        private static volatile HostConfigurationHelper instance;

        /// <summary>
        /// Pivto config section property
        /// </summary>
        private static PivotConfigurationSection pivotConfigSection;

        /// <summary>
        /// Host Configuration property
        /// </summary>
        private System.Configuration.Configuration hostConfiguration;

        /// <summary>
        /// Lock Object used for synchronization
        /// </summary>
        private static readonly object LockObject = new Object();

        /// <summary>
        /// Prevents a default instance of the <see cref="HostConfigurationHelper "/> class from being created.
        /// </summary>
        private HostConfigurationHelper()
        {
        }

        /// <summary>
        /// Gets the Singleton Instance of Host Configuration Helper
        /// </summary>
        public static HostConfigurationHelper SingletonInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (LockObject)
                    {
                        if (instance == null)
                        {
                            instance = new HostConfigurationHelper();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Opens the Host configuration file for edit
        /// </summary>
        /// <returns>Returns true if the file is opened else false.</returns>
        internal bool OpenHostConfigurationFile()
        {
            try
            {
                string collectionCreatorExePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Zentity.Pivot.CollectionCreator.exe");
                this.hostConfiguration = System.Configuration.ConfigurationManager.OpenExeConfiguration(collectionCreatorExePath);
                HostConfigurationHelper.pivotConfigSection = (PivotConfigurationSection)this.hostConfiguration.GetSection(PivotConfigurationSection.Instance.SectionInformation.Name);
                PivotConfigurationSection.Instance = HostConfigurationHelper.pivotConfigSection;
                return true;
            }
            catch (System.Exception exception)
            {
                throw new FaultException(exception.ToString());
                //throw new FaultException<Exception>(exception, exception.Message);
            }
        }

        /// <summary>
        /// Saves the Host configuration file and closes it.
        /// </summary>
        /// <returns>Returns true if the file is closed else false.</returns>
        internal bool CloseHostConfigurationFile()
        {
            try
            {
                pivotConfigSection.SectionInformation.ForceSave = true;
                this.hostConfiguration.Save(System.Configuration.ConfigurationSaveMode.Full);
                return true;
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.HostConfigFileNotOpened));
                //throw new FaultException<Exception>(exception, new FaultReason("Invalid operation exception. The Host Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Sets the location of the default html template location
        /// </summary>
        /// <param name="location">File location of the default deep zoom</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateDefaultDeepZoomElementTemplateLocation(string location)
        {
            if (HostConfigurationHelper.pivotConfigSection != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.DefaultDeepZoom != null)
            {
                if (!string.IsNullOrEmpty(location) && !string.IsNullOrWhiteSpace(location))
                {
                    if (System.IO.File.Exists(location) || location.Equals("default"))
                    {
                        HostConfigurationHelper.pivotConfigSection.GlobalSettings.DefaultDeepZoom.TemplateLocation = location;
                        return true;
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.LocationNotAccessible));
                        //throw new FaultException<System.IO.DirectoryNotFoundException>(new System.IO.DirectoryNotFoundException("The location specified could not be found or does not have permissions."));
                    }
                }
                else
                {
                    throw new FaultException(Properties.Messages.ArgumentsNullOrEmpty);
                    //throw new FaultException<ArgumentNullException>(new ArgumentNullException("location", "One or more arguments passed to this method is null or empty."));
                }
            }
            else
            {
                throw new FaultException(new FaultReason(Properties.Messages.HostConfigFileNotOpened));
                //throw new FaultException<Exception>(new Exception(), new FaultReason("Invalid operation exception. The Host Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Sets the location of the default deep zoom image location
        /// </summary>
        /// <param name="location">File location of the default deep zoom image location</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateDefaultDeepZoomElementImageLocation(string location)
        {
            if (HostConfigurationHelper.pivotConfigSection != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.DefaultDeepZoom != null)
            {
                if (!string.IsNullOrEmpty(location) && !string.IsNullOrWhiteSpace(location))
                {
                    if (System.IO.File.Exists(location))
                    {
                        HostConfigurationHelper.pivotConfigSection.GlobalSettings.DefaultDeepZoom.ImageLocation = location;
                        return true;
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.LocationNotAccessible));
                        //throw new FaultException<System.IO.DirectoryNotFoundException>(new System.IO.DirectoryNotFoundException("The location specified could not be found or does not have permissions."));
                    }
                }
                else
                {
                    throw new FaultException(Properties.Messages.ArgumentsNullOrEmpty);
                    //throw new FaultException<ArgumentNullException>(new ArgumentNullException("location", "One or more arguments passed to this method is null or empty."));
                }
            }
            else
            {
                throw new FaultException(new FaultReason(Properties.Messages.HostConfigFileNotOpened));
                //throw new FaultException<Exception>(new Exception(), new FaultReason("Invalid operation exception. The Host Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Sets the type of Web Capture to be used.
        /// </summary>
        /// <param name="type">Assembly and Class of the WebCapture</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateWebCaptureElement(string type)
        {
            if (HostConfigurationHelper.pivotConfigSection != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.WebCapture != null)
            {
                if (!string.IsNullOrEmpty(type) && !string.IsNullOrWhiteSpace(type))
                {
                    HostConfigurationHelper.pivotConfigSection.GlobalSettings.WebCapture.Type = type;
                    return true;
                }
                else
                {
                    throw new FaultException(Properties.Messages.ArgumentsNullOrEmpty);
                    //throw new FaultException<ArgumentNullException>(new ArgumentNullException("type", "The arguments passed to this method is null or empty."));
                }
            }
            else
            {
                throw new FaultException(new FaultReason(Properties.Messages.HostConfigFileNotOpened));
                //throw new FaultException<Exception>(new Exception(), new FaultReason("Invalid operation exception. The Host Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Sets the Output To setting to the type specified
        /// </summary>
        /// <param name="type">Output Type</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateOutputToElment(OutputType type)
        {
            if (HostConfigurationHelper.pivotConfigSection != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.OutputTo != null)
            {
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.OutputTo.Type = type;
                return true;
            }
            else
            {
                throw new FaultException(new FaultReason(Properties.Messages.HostConfigFileNotOpened));
                //throw new FaultException<Exception>(new Exception(), new FaultReason("Invalid operation exception. The Host Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Sets the Uri Format to the specified format
        /// </summary>
        /// <param name="format">Uri format</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateUriFormatElment(UriFormatType format)
        {
            if (HostConfigurationHelper.pivotConfigSection != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.UriFormat != null)
            {
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.UriFormat.Format = format;
                return true;
            }
            else
            {
                throw new FaultException(new FaultReason(Properties.Messages.HostConfigFileNotOpened));
                //throw new FaultException<Exception>(new Exception(), new FaultReason("Invalid operation exception. The Host Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Sets the Base uri for the generation of related collections links
        /// </summary>
        /// <param name="uri">Base uri.</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateBaseUriElment(string uri)
        {
            if (HostConfigurationHelper.pivotConfigSection != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.BaseUri != null)
            {
                if (!string.IsNullOrEmpty(uri) && !string.IsNullOrWhiteSpace(uri))
                {
                    try
                    {
                        new Uri(uri);
                        HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.BaseUri.Uri = uri;
                        return true;
                    }
                    catch (UriFormatException)
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.UriFormatError));
                        //throw new FaultException<UriFormatException>(new UriFormatException("The Uri specified is not of the correct format."));
                    }
                }
                else
                {
                    HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.BaseUri.Uri = uri;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the Output Folder where the Cxml's are to be generated
        /// </summary>
        /// <param name="location">Folder location</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateOutputFolderElment(string location)
        {
            if (HostConfigurationHelper.pivotConfigSection != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.OutputFolder != null)
            {
                if (!string.IsNullOrEmpty(location) && !string.IsNullOrWhiteSpace(location))
                {
                    if (System.IO.Directory.Exists(location))
                    {
                        HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.OutputFolder.Location = location;
                        return true;
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.LocationNotAccessible));
                        //throw new FaultException<System.IO.DirectoryNotFoundException>(new System.IO.DirectoryNotFoundException(), new FaultReason("The location specified could not be found or does not have permissions."));
                    }
                }
                else
                {
                    throw new FaultException(new FaultReason(Properties.Messages.ArgumentsNullOrEmpty));
                    //throw new FaultException<ArgumentNullException>(new ArgumentNullException("location"), new FaultReason("The arguments passed to this method is null or empty."));
                }
            }
            else
            {
                throw new FaultException(new FaultReason(Properties.Messages.HostConfigFileNotOpened));
                //throw new FaultException<Exception>(new Exception(), new FaultReason("Invalid operation exception. The Host Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Sets the Split Size of the Collections
        /// </summary>
        /// <param name="value">Split size</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateSplitSizeElment(int value)
        {
            if (HostConfigurationHelper.pivotConfigSection != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.SplitSize != null)
            {
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.SplitSize.Value = value;
                return true;
            }
            else
            {
                throw new FaultException(new FaultReason(Properties.Messages.HostConfigFileNotOpened));
                //throw new FaultException<Exception>(new Exception(), new FaultReason("Invalid operation exception. The Host Configuration File is not Opened."));
            }
        }

        /// <summary>
        /// Enables or Disables the generation of related collections
        /// </summary>
        /// <param name="value">Boolean value</param>
        /// <returns>Returns true if setting is changed else false.</returns>
        internal bool UpdateGenerateRelatedCollectionsElment(bool value)
        {
            if (HostConfigurationHelper.pivotConfigSection != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings != null &&
               HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings != null &&
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.GenerateRelatedCollections != null)
            {
                HostConfigurationHelper.pivotConfigSection.GlobalSettings.OutputSettings.GenerateRelatedCollections.Value = value;
                return true;
            }
            else
            {
                throw new FaultException(new FaultReason(Properties.Messages.HostConfigFileNotOpened));
                //throw new FaultException<Exception>(new Exception(), new FaultReason("Invalid operation exception. The Host Configuration File is not Opened."));
            }
        }
    }
}
