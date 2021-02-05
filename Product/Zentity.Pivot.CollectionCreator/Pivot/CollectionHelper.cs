// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Pivot
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.DeepZoomTools;
    using Zentity.CollectionCreator.PublishingProgressService;
    using Zentity.Core;
    using Zentity.Pivot.Imaging;
    using Zentity.Services.Configuration.Pivot;
    using Zentity.Services.External;
    using Zentity.Services.Web;
    using Zentity.Services.Web.Pivot;
    using DeepZoomClasses = Zentity.DeepZoom;
    using StringMessages = Zentity.CollectionCreator.Properties.Messages;
    using StringResources = Zentity.CollectionCreator.Properties.Resources;
    using System.Globalization;

    internal class CollectionHelper : IDisposable, IPublishingProgressServiceCallback
    {
        #region Private ReadOnly Data Member

        /// <summary>
        /// A global instance for Parallel extensions options.
        /// </summary>
        private readonly ParallelOptions parallelLoopOptions;

        /// <summary>
        /// Holds a list of DZI image paths for the CollectionCreator class.
        /// </summary>
        private readonly Dictionary<Guid, string> deepZoomSourceImageList;

        /// <summary>
        /// Holds a list of collection items being handled by this CollectionHelper instance
        /// </summary>
        private readonly List<Item> collectionItemList;

        /// <summary>
        /// List containing the ResourceTypes for which cxml needs to be generated.
        /// </summary>
        private readonly List<string> relatedCollectionList;

        /// <summary>
        /// Holds the cancellation token source object for cancellation of operation
        /// </summary>
        private readonly CancellationTokenSource cancellationToken;

        /// <summary>
        /// Holds the publishing service client
        /// </summary>
        private PublishingProgressServiceClient progressServiceClient;

        #endregion

        #region Public Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionHelper"/> class.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        public CollectionHelper(Guid instanceId)
        {
            // Creating an instance id for the collection helper instance
            this.InstanceId = instanceId;

            // Creation of working folder inside the system temp folder
            string destFolder = Path.Combine(Path.GetTempPath(), instanceId.ToString());
            this.WorkingFolder = destFolder + Path.DirectorySeparatorChar;

            // Rename the current temp folder and push for delete on a separate thread
            List<string> foldersToDelete = new List<string>();
            Guid toDeleteGuid = Guid.NewGuid();
            string toDeleteFolderName = Path.Combine(Path.GetTempPath(), toDeleteGuid.ToString());
            if (Directory.Exists(destFolder))
            {
                Directory.Move(destFolder, toDeleteFolderName + "_todelete");
                foldersToDelete.Add(toDeleteFolderName + "_todelete");
            }
            Directory.CreateDirectory(destFolder);

            string deepZoomFolder = Path.Combine(destFolder, StringResources.DeepZoomImageFolderName);
            Directory.CreateDirectory(deepZoomFolder);
            this.DeepZoomFolder = deepZoomFolder + Path.DirectorySeparatorChar;

            // Creation of the temporary image folder.
            destFolder = Path.Combine(Path.GetTempPath(), instanceId + "_Images");
            this.ImagesFolder = destFolder + Path.DirectorySeparatorChar;
            if (Directory.Exists(destFolder))
            {
                Directory.Move(destFolder, toDeleteFolderName + "_images_todelete");
                foldersToDelete.Add(toDeleteFolderName + "_images_todelete");
            }
            Directory.CreateDirectory(destFolder);

            if (foldersToDelete.Count > 0)
            {
                Thread deleteFolders = new Thread(DeleteFolderOnThread) {IsBackground = true};
                deleteFolders.Start(foldersToDelete);
            }

            // Initializing variables and collections
            this.CurrentLevel = 1;
            this.CurrentStage = PublishStage.NotStarted;
            this.cancellationToken = new CancellationTokenSource();
            this.collectionItemList = new List<Item>();
            this.relatedCollectionList = new List<string>();
            this.deepZoomSourceImageList = new Dictionary<Guid, string>();

            // Initialize the Publishing Progress Service client
            try
            {
                InstanceContext instanceContext = new InstanceContext(this);
                this.progressServiceClient = new PublishingProgressServiceClient(instanceContext);
                this.progressServiceClient.ReportStart(this.InstanceId, CreatorApplication.CollectionCreator, Process.GetCurrentProcess().Id);
                this.IsProgressServiceReachable = true;
            }
            catch (TimeoutException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                this.IsProgressServiceReachable = false;
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }

            // Determining the threads per core from the config file.
            int threadsPerCore = Globals.MinThreadsPerCore;
            if (PivotConfigurationSection.Instance.GlobalSettings != null)
            {
                if (PivotConfigurationSection.Instance.GlobalSettings.ThreadsPerCore < Globals.MinThreadsPerCore)
                    threadsPerCore = Globals.MinThreadsPerCore;
                else if (PivotConfigurationSection.Instance.GlobalSettings.ThreadsPerCore > Globals.MaxThreadsPerCore)
                    threadsPerCore = Globals.MaxThreadsPerCore;
                else
                    threadsPerCore = PivotConfigurationSection.Instance.GlobalSettings.ThreadsPerCore;
            }

            this.parallelLoopOptions = new ParallelOptions {MaxDegreeOfParallelism = threadsPerCore, CancellationToken = this.cancellationToken.Token};

            // Check and copy the default deepzoom image to the temporary location.
            if (PivotConfigurationSection.Instance.GlobalSettings != null)
            {
                if (!string.IsNullOrWhiteSpace(PivotConfigurationSection.Instance.GlobalSettings.DefaultDeepZoom.ImageLocation))
                {
                    string defaultDeepZoomPath = Path.Combine(Utilities.AssemblyLocation, PivotConfigurationSection.Instance.GlobalSettings.DefaultDeepZoom.ImageLocation);
                    if (System.IO.File.Exists(defaultDeepZoomPath))
                    {
                        CopyFolder(Path.GetDirectoryName(defaultDeepZoomPath), this.DeepZoomFolder);
                        this.DefaultDeepZoomPath = Path.Combine(this.DeepZoomFolder, Path.GetFileName(defaultDeepZoomPath));
                    }
                }

            }
        }

        /// <summary>
        /// Initializes a new instance of the CollectionHelper class
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="dataModelNamespace">Data Model Namespace</param>
        /// <param name="resouceTypeName">Resource Type Name</param>
        public CollectionHelper(Guid instanceId, string dataModelNamespace, string resouceTypeName)
            : this(instanceId)
        {
            this.ResourceTypeName = resouceTypeName;
            this.DataModelNamespace = dataModelNamespace;
            this.ResourceTypeFullName = string.Format(StringResources.ResourceTypeFullNameFormat, dataModelNamespace, resouceTypeName);
            this.ResourceTypeOutputFolder = Path.Combine(PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.OutputFolder.Location, this.ResourceTypeFullName);
            Globals.TraceMessage(TraceEventType.Information, string.Empty, "Started Zentity Pivot Collection Creator - Instance ID : " + this.InstanceId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionHelper"/> class.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="dataModelNamespace">Data Model Namespace</param>
        /// <param name="resouceTypeName">Resource Type Name</param>
        /// <param name="level">The Desired Generation level.</param>
        public CollectionHelper(Guid instanceId, string dataModelNamespace, string resouceTypeName, int level)
            : this(instanceId, dataModelNamespace, resouceTypeName)
        {
            this.CurrentLevel = level;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether the progress service is reachable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the progress service is reachable.; otherwise, <c>false</c>.
        /// </value>
        public bool IsProgressServiceReachable
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current stage.
        /// </summary>
        /// <value>The current stage.</value>
        public PublishStage CurrentStage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the WorkingFolder Path
        /// </summary>
        public string WorkingFolder
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ImagesFolder Path
        /// </summary>
        public string ImagesFolder
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the DeepZoomFolder Path inside WorkingFolder
        /// </summary>
        public string DeepZoomFolder
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the resource type output folder.
        /// </summary>
        /// <value>The resource type output folder.</value>
        public string ResourceTypeOutputFolder
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the default deep zoom path.
        /// </summary>
        /// <value>The default deep zoom path.</value>
        public string DefaultDeepZoomPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ResourceTypeFullName = DataModelNameSpace + ResourceType
        /// </summary>
        public string ResourceTypeFullName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ResourceType Name
        /// </summary>
        public string ResourceTypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the DataModelNamespace
        /// </summary>
        public string DataModelNamespace
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the level upto which the Cxml generation is required.
        /// </summary>
        public int CurrentLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the instance id.
        /// </summary>
        /// <value>The instance id.</value>
        public Guid InstanceId
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the collection.
        /// </summary>
        /// <param name="resourceChangeMessageList">The resource change message list.</param>
        public void UpdateCollection(IEnumerable<ResourceChangeMessage> resourceChangeMessageList)
        {
            try
            {
                // Argument Validations
                if (resourceChangeMessageList == null || resourceChangeMessageList.Count() == 0)
                {
                    throw new ArgumentNullException("resourceChangeMessageList");
                }

                Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "Update of Pivot Collection for Resource Type was started : " + this.ResourceTypeFullName);
                this.ReportProgress(PublishStage.Initiating, default(ProgressCounter));

                // Cancel the operation if it has been requested for.
                if (this.cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(this.cancellationToken.Token);
                }

                // Get the ResourceType settings from the Configuration Service
                ConfigurationService configService = new ConfigurationService();
                ResourceTypeSetting resourceTypeSetting = configService.GetResourceTypeConfiguration(this.DataModelNamespace, this.ResourceTypeName);
                if (resourceTypeSetting == null || resourceTypeSetting.Facets == null || resourceTypeSetting.Facets.Length == 0)
                {
                    throw new ArgumentNullException("resourceTypeSetting", StringMessages.NullResourceTypeSettingMessage);
                }

                // If the ResourceTypeSetting for the resource type has the disable collection creation set to true, then abort the operation.
                if (resourceTypeSetting.DisableCollectionCreation)
                {
                    throw new InvalidOperationException("Collection creation is disabled in the configuration for the resource type : " + this.ResourceTypeFullName);
                }

                // Cancel the operation if it has been requested for.
                if (this.cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(this.cancellationToken.Token);
                }

                // Check for existing collection exists and initialize for update operation
                if (this.UpdateCollectionInitialization())
                {
                    // Cancel the operation if it has been requested for.
                    if (this.cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(this.cancellationToken.Token);
                    }

                    // Deserialize the collection from the existing .cxml files
                    Collection finalCollection = this.DeserializeCollection(this.WorkingFolder);
                    if (finalCollection == null)
                    {
                        throw new ArgumentNullException("finalCollection");
                    }

                    this.collectionItemList.Clear();
                    this.collectionItemList.AddRange(finalCollection.Items[0].Item);
                    
                    // Process the resource change message list and update the collection
                    IEnumerable<Resource> resourceTypeItems = this.UpdateCollectionProcessResourceChangeMessages(resourceChangeMessageList, resourceTypeSetting);

                    // Create the images for Pivot Collection Items
                    if (resourceTypeItems.Count() > 0)
                    {
                        this.CreateCollectionItemImages(resourceChangeMessageList, resourceTypeItems, resourceTypeSetting);
                    }

                    // Create the DeepZoom images for the created images
                    Dictionary<Guid, string> modifiedImageList = deepZoomSourceImageList.Where(imgItem => !imgItem.Value.EndsWith(StringResources.DeepZoomImageFileExtension)).ToDictionary(pairItem => pairItem.Key, pairItem => pairItem.Value);
                    this.CreateDeepZoomImages(modifiedImageList);

                    if (this.collectionItemList.Count > 0)
                    {
                        // Create the DeepZoom collection for the DeepZoom images created
                        this.CreateDeepZoomCollection(this.collectionItemList);

                        // Update the facet categories if there are change in the resource type configurations.
                        this.UpdateCollectionFacetCategories(finalCollection.FacetCategories, resourceTypeSetting);

                        // Split the Final Collection into parts and serialize them into .cxml files
                        this.SplitFinalCollection(this.collectionItemList, finalCollection.FacetCategories);
                    }

                    // Copy the Web.Config file to the Output Folder to disable caching
                    CopyWebConfig(this.ResourceTypeOutputFolder);

                    // Delete the existing Pivot Collection
                    this.ReportProgress(PublishStage.DeletingExistingCollection, default(ProgressCounter));
                    DeleteFolder(this.ResourceTypeOutputFolder, true);

                    if (this.collectionItemList.Count > 0)
                    {
                        // Copy the new collection to the output folder
                        this.ReportProgress(PublishStage.CopyingNewCollection, default(ProgressCounter));
                        CopyFolder(this.WorkingFolder, this.ResourceTypeOutputFolder, true, true);
                    }

                    // Perform cleanup and mark the operation as completed
                    this.Cleanup();

                    Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "Update of Pivot Collection for Resource Type was completed : " + this.ResourceTypeFullName);
                }
                else
                {
                    Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(StringMessages.UpdateCollectionCreateCollectionMessage, this.ResourceTypeFullName, CultureInfo.InvariantCulture));
                    this.CreateCollection();
                }
            }
            catch (OperationCanceledException)
            {
                this.ReportProgress(PublishStage.AbortedOnDemand, default(ProgressCounter));
                throw;
            }
            catch
            {
                this.ReportProgress(PublishStage.AbortedOnError, default(ProgressCounter));
                throw;
            }
        }

        /// <summary>
        /// Creates the collection.
        /// </summary>
        public void CreateCollection()
        {
            try
            {
                Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "Publishing of Pivot Collection for Resource Type was started : " + this.ResourceTypeFullName);
                this.ReportProgress(PublishStage.Initiating, default(ProgressCounter));

                // Cancel the operation if it has been requested for.
                if (this.cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(this.cancellationToken.Token);
                }

                // Get the ResourceType settings from the Configuration Service
                ConfigurationService configService = new ConfigurationService();
                ResourceTypeSetting resourceTypeSetting = configService.GetResourceTypeConfiguration(this.DataModelNamespace, this.ResourceTypeName);
                if (resourceTypeSetting == null || resourceTypeSetting.Facets == null || resourceTypeSetting.Facets.Length == 0)
                {
                    throw new ArgumentNullException("resourceTypeSetting", StringMessages.NullResourceTypeSettingMessage);
                }

                // If the ResourceTypeSetting for the resource type has the disable collection creation set to true, then abort the operation.
                if (resourceTypeSetting.DisableCollectionCreation)
                {
                    throw new InvalidOperationException("Collection creation is disabled in the configuration for the resource type : " + this.ResourceTypeFullName);
                }

                // Cancel the operation if it has been requested for.
                if (this.cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(this.cancellationToken.Token);
                }

                // Get the ResourceType list for the current Data Model from the Zentity DB
                IEnumerable<ResourceType> resourceTypes = Utilities.GetResourceTypes(this.DataModelNamespace);
                if (resourceTypes == null)
                {
                    throw new ArgumentNullException("resourceTypes", StringMessages.NullResourceTypesFaultReason);
                }

                // Check if the current ResourceType is present in the list of ResourceTypes from Zentity DB
                ResourceType specificResourceType = resourceTypes.Where(resType => resType.Name.Equals(this.ResourceTypeName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (specificResourceType == null)
                {
                    throw new ArgumentNullException("specificResourceType", string.Format("{0} : {1}", StringMessages.NullResourceTypeInDBMessage, this.ResourceTypeName));
                }

                // Cancel the operation if it has been requested for.
                if (this.cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(this.cancellationToken.Token);
                }

                // If the current ResourceType is found, then fetch the Resource items for this ResourceType
                this.ReportProgress(PublishStage.FetchingResourceItems, default(ProgressCounter));
                IEnumerable<Resource> resourceTypeItems = this.GetResourceTypeItems(specificResourceType);
                if (resourceTypeItems == null || resourceTypeItems.Count() == 0)
                {
                    throw new ArgumentNullException("resourceTypeItems", string.Format("{0} : {1}", StringMessages.NullResourcesInDBMessage, this.ResourceTypeFullName));
                }

                // Cancel the operation if it has been requested for.
                if (this.cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(this.cancellationToken.Token);
                }

                // Generate the Facet Categories for the current ResourceType
                FacetCategories facetCategories = CreateCollectionFacetCategories(resourceTypeSetting);
                if (facetCategories == null)
                {
                    throw new ArgumentNullException("facetCategories", StringMessages.NullFacetCategoriesMessage);
                }

                // Cancel the operation if it has been requested for.
                if (this.cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(this.cancellationToken.Token);
                }

                // Create the Pivot Collection Items
                this.CreateCollectionItems(resourceTypeItems, resourceTypeSetting);

                // Publish the Intermediate collection
                this.PublishIntermediateCollection(facetCategories);

                // Create the images for Pivot Collection Items
                this.CreateCollectionItemImages(resourceTypeItems, resourceTypeSetting);

                // Create the DeepZoom images for the created images
                this.CreateDeepZoomImages(this.deepZoomSourceImageList);

                // Create the DeepZoom collection for the DeepZoom images created
                this.CreateDeepZoomCollection(this.collectionItemList);

                // Split the Final Collection into parts and serialize them into .cxml files
                this.SplitFinalCollection(this.collectionItemList, facetCategories);

                // Copy the Web.Config file to the Output Folder to disable caching
                CopyWebConfig(this.ResourceTypeOutputFolder);

                // Delete the existing Pivot Collection
                this.ReportProgress(PublishStage.DeletingExistingCollection, default(ProgressCounter));
                DeleteFolder(this.ResourceTypeOutputFolder, true);

                // Copy the new collection to the output folder
                this.ReportProgress(PublishStage.CopyingNewCollection, default(ProgressCounter));
                CopyFolder(this.WorkingFolder, this.ResourceTypeOutputFolder, true, true);

                // Perform cleanup and mark the operation as completed
                this.Cleanup();

                Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "Publishing of Pivot Collection for Resource Type was completed : " + this.ResourceTypeFullName);
            }
            catch (OperationCanceledException)
            {
                this.ReportProgress(PublishStage.AbortedOnDemand, default(ProgressCounter));
                throw;
            }
            catch
            {
                this.ReportProgress(PublishStage.AbortedOnError, default(ProgressCounter));
                throw;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reports the progress to publishing service.
        /// </summary>
        /// <param name="newStage">The new stage.</param>
        /// <param name="counter">The counter.</param>
        private void ReportProgress(PublishStage newStage, ProgressCounter counter)
        {
            this.CurrentStage = newStage;

            try
            {
                if (this.IsProgressServiceReachable && this.progressServiceClient.State != CommunicationState.Opened)
                {
                    InstanceContext instanceContext = new InstanceContext(this);
                    this.progressServiceClient = new PublishingProgressServiceClient(instanceContext);
                }

                if (this.IsProgressServiceReachable)
                {
                    this.progressServiceClient.ReportProgress(this.InstanceId, newStage, counter);
                }
            }
            catch (TimeoutException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                this.IsProgressServiceReachable = false;
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }
        }

        /// <summary>
        /// Ges the Resource Type Items for a specifig ResourceType
        /// </summary>
        /// <param name="resourceType">Resource Type</param>
        /// <returns>Resource enumeration</returns>
        private IEnumerable<Resource> GetResourceTypeItems(ResourceType resourceType)
        {
            // Argument Validations
            if (resourceType == null)
            {
                throw new ArgumentNullException("resourceType");
            }

            KeyValuePair<string, Assembly> resourceTypeAssembly = AssemblyCache.Current.GetItemContainingNamespace(this.DataModelNamespace);
            if (resourceTypeAssembly.Equals(default(KeyValuePair<string, Assembly>)))
            {
                Globals.TraceMessage(TraceEventType.Error, string.Empty, string.Format(CultureInfo.InvariantCulture, "The namespace {0} was not found in any assemblies loaded in the cache.", this.DataModelNamespace));
                return null;
            }

            Type typeofResourceType = resourceTypeAssembly.Value.GetType(resourceType.FullName);
            Type resourceCollectionType = typeof(System.Data.Objects.ObjectQuery<Resource>);

            MethodInfo ofTypeMethod = resourceCollectionType.GetMethod(StringResources.MethodName_OfType);
            if (ofTypeMethod != null && typeofResourceType != null)
            {
                Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "Fetching resources from Zentity Database for " + this.ResourceTypeFullName);

                using (ZentityContext zentityContext = Utilities.CreateZentityContext())
                {
                    zentityContext.Resources.MergeOption = System.Data.Objects.MergeOption.NoTracking;
                    MethodInfo genericOfTypeMethod = ofTypeMethod.MakeGenericMethod(new[] { typeofResourceType });
                    if (genericOfTypeMethod != null)
                    {
                        var resourceItemList = genericOfTypeMethod.Invoke(zentityContext.Resources, null) as IEnumerable<Resource>;

                        if (resourceItemList != null)
                        {
                            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "Fetched resources from Zentity Database for " + this.ResourceTypeFullName + ". Resource Count = " + resourceItemList.Count());
                            return resourceItemList.ToList();
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the Resource Type Item for a specific Resource Type and Guid
        /// </summary>
        /// <param name="resourceType">Resource Type</param>
        /// <param name="resourceId">Globally Unique Identifier</param>
        /// <returns>Returns the specific Resource</returns>
        private Resource GetResourceTypeItem(ResourceType resourceType, Guid resourceId)
        {
            // Argument Validations
            if (resourceType == null || resourceId.Equals(default(Guid)))
            {
                return null;
            }

            KeyValuePair<string, Assembly> resourceTypeAssembly = AssemblyCache.Current.GetItemContainingNamespace(this.DataModelNamespace);
            if (resourceTypeAssembly.Equals(default(KeyValuePair<string, Assembly>)))
            {
                Globals.TraceMessage(TraceEventType.Error, string.Empty, string.Format(CultureInfo.InvariantCulture, "The namespace {0} was not found in any assemblies loaded in the cache.", this.DataModelNamespace));
                return null;
            }

            Type tagType = resourceTypeAssembly.Value.GetType(resourceType.FullName);
            Type resourceCollectionType = typeof(System.Data.Objects.ObjectQuery<Resource>);

            MethodInfo ofTypeMethod = resourceCollectionType.GetMethod(StringResources.MethodName_OfType);
            if (ofTypeMethod != null && tagType != null)
            {
                using (ZentityContext zentityContext = Utilities.CreateZentityContext())
                {
                    zentityContext.Resources.MergeOption = System.Data.Objects.MergeOption.NoTracking;
                    return zentityContext.Resources.Where(resource => resource.Id == resourceId).FirstOrDefault();
                }
            }

            return null;
        }

        /// <summary>
        /// Perform initialization tasks before the Update Collection operation.
        /// </summary>
        /// <returns>[true] if the initialization was successful by copying the existing collection otherwise [false].</returns>
        private bool UpdateCollectionInitialization()
        {
            if (Directory.Exists(this.ResourceTypeOutputFolder))
            {
                DirectoryInfo resourceTypeOutputDirInfo = new DirectoryInfo(this.ResourceTypeOutputFolder);
                var pivotCollectionFiles = resourceTypeOutputDirInfo.GetFiles(StringResources.CxmlExtension);
                if (pivotCollectionFiles.Length > 0)
                {
                    this.ReportProgress(PublishStage.CopyingExistingCollection, default(ProgressCounter));
                    Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", string.Format(StringMessages.UpdateCollectionCopyToTempMessage, this.ResourceTypeFullName, CultureInfo.InvariantCulture));

                    try
                    {
                        CopyFolder(this.ResourceTypeOutputFolder, this.WorkingFolder, true, true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                        throw;
                    }
                }
            }

            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", string.Format(CultureInfo.InvariantCulture, "Existing collection of resource type \'{0}\' was not found.", this.ResourceTypeFullName));
            return false;
        }

        /// <summary>
        /// Process the resource change message list and update the pivot collection items.
        /// </summary>
        /// <param name="resourceChangeMessageList">The resource change message list.</param>
        /// <param name="resourceTypeSetting">The resource type setting.</param>
        /// <returns>The collection of Resources been feched for the change message collection</returns>
        private IEnumerable<Resource> UpdateCollectionProcessResourceChangeMessages(IEnumerable<ResourceChangeMessage> resourceChangeMessageList, ResourceTypeSetting resourceTypeSetting)
        {
            // Get the ResourceType list for the current Data Model from the Zentity DB
            IEnumerable<ResourceType> resourceTypes = Utilities.GetResourceTypes(this.DataModelNamespace);
            if (resourceTypes == null)
            {
                throw new ArgumentNullException("resourceTypes", StringMessages.NullResourceTypesFaultReason);
            }

            // Check if the current ResourceType is present in the list of ResourceTypes from Zentity DB
            ResourceType specificResourceType = resourceTypes.Where(resType => resType.Name.Equals(this.ResourceTypeName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (specificResourceType == null)
            {
                throw new ArgumentNullException("specificResourceType", string.Format(CultureInfo.CurrentCulture, "{0} : {1}", StringMessages.NullResourceTypeInDBMessage, this.ResourceTypeName));
            }

            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "Processing resource change message list. Change Message Count : " + resourceChangeMessageList.Count());

            ProgressCounter resourceItemsCounter = new ProgressCounter { Total = resourceChangeMessageList.Count() };
            this.ReportProgress(PublishStage.ProcessingResourceItems, resourceItemsCounter);
            List<Resource> resourceTypeItems = new List<Resource>();

            #region Parallel ForEach

            try
            {
                Parallel.ForEach(resourceChangeMessageList.OrderBy(changeMessage => changeMessage.ChangeType), this.parallelLoopOptions, (resourceChangeMessage) =>
                {
                    // Cancel the operation if it has been requested for.
                    if (this.cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(this.cancellationToken.Token);
                    }

                    if (resourceChangeMessage.ChangeType == ResourceChangeType.Deleted)
                    {
                        this.DeleteCollectionItem(resourceChangeMessage);
                    }
                    else
                    {
                        Resource resourceItem = this.GetResourceTypeItem(specificResourceType, resourceChangeMessage.ResourceId);
                        resourceTypeItems.Add(resourceItem);
                        if (resourceItem != null)
                        {
                            this.DeleteCollectionItem(resourceChangeMessage);
                            this.UpdateCollectionItem(resourceItem, resourceTypeSetting, resourceChangeMessage);
                        }
                    }

                    resourceItemsCounter.Completed++;
                    this.ReportProgress(PublishStage.ProcessingResourceItems, resourceItemsCounter);
                    Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", string.Format(CultureInfo.InvariantCulture, "Items Processed for {0} : {1} - {2} - {3}", this.ResourceTypeFullName, resourceItemsCounter.Completed, resourceChangeMessage.ChangeType, resourceChangeMessage.ResourceId));
                });
            }
            catch (OperationCanceledException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Publishing has been aborted by user for resource type  : " + this.ResourceTypeFullName);
                throw;
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Exception occurred in parallelization of tasks. Publishing has been aborted.");
                throw;
            }

            #endregion

            return resourceTypeItems;
        }

        /// <summary>
        /// Update the facet categories for all the items in the collection
        /// </summary>
        /// <param name="facetCategories">The facet categories.</param>
        /// <param name="resourceTypeSetting">The resource type setting.</param>
        private void UpdateCollectionFacetCategories(FacetCategories facetCategories, ResourceTypeSetting resourceTypeSetting)
        {
            IQueryable<FacetCategory> oldFacetCategories = facetCategories.FacetCategory.AsQueryable();
            IQueryable<FacetCategory> newFacetCategories = CreateCollectionFacetCategories(resourceTypeSetting).FacetCategory.AsQueryable();
            if (oldFacetCategories != null && newFacetCategories != null)
            {
                IQueryable<FacetCategory> oldExceptNew = oldFacetCategories.Except(newFacetCategories, new FacetCategoryComparer()).AsQueryable();
                IQueryable<FacetCategory> newExceptOld = newFacetCategories.Except(oldFacetCategories, new FacetCategoryComparer()).AsQueryable();
                int a = oldExceptNew.Count();
                int b = newExceptOld.Count();

                if (a == b)
                {
                    return;
                }
                if (b > a)
                {
                    // Get the ResourceType list for the current Data Model from the Zentity DB
                    IEnumerable<ResourceType> resourceTypes = Utilities.GetResourceTypes(this.DataModelNamespace);
                    if (resourceTypes == null)
                    {
                        throw new ArgumentNullException("resourceTypes", StringMessages.NullResourceTypesFaultReason);
                    }

                    // Check if the current ResourceType is present in the list of ResourceTypes from Zentity DB
                    ResourceType specificResourceType = resourceTypes.Where(resType => resType.Name.Equals(this.ResourceTypeName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (specificResourceType == null)
                    {
                        throw new ArgumentNullException("specificResourceType", string.Format(CultureInfo.CurrentCulture, "{0} : {1}", StringMessages.NullResourceTypeInDBMessage, this.ResourceTypeName));
                    }

                    facetCategories.FacetCategory = newFacetCategories.ToArray();
                    foreach (Item item in this.collectionItemList)
                    {
                        Resource resource = this.GetResourceTypeItem(specificResourceType, new Guid(item.Id));
                        if (resource != null)
                        {
                            item.Facets.Facet = this.CreateCollectionItemFacets(resource, resourceTypeSetting.Facets).ToArray();
                        }
                    }

                    return;
                }
                if (a > b)
                {
                    IEnumerable<string> toRemoveFacetPropertyName = oldExceptNew.Select(f => f.Name);
                    facetCategories.FacetCategory = newFacetCategories.ToArray();
                    foreach (Item item in this.collectionItemList)
                    {
                        List<Facet> currentFacetList = item.Facets.Facet.ToList();
                        currentFacetList.RemoveAll(f => toRemoveFacetPropertyName.Contains(f.Name));
                        item.Facets.Facet = currentFacetList.ToArray();
                    }

                    return;
                }
            }

            return;
        }

        /// <summary>
        /// Updates the collection item.
        /// </summary>
        /// <param name="resourceItem">The resource item.</param>
        /// <param name="resourceTypeSetting">The resource type setting.</param>
        /// <param name="resourceChangeMessage">The resource change message.</param>
        private void UpdateCollectionItem(Resource resourceItem, ResourceTypeSetting resourceTypeSetting, ResourceChangeMessage resourceChangeMessage)
        {
            // Argument Validations
            if (resourceItem == null || resourceTypeSetting == null || resourceChangeMessage == null)
            {
                return;
            }

            Item newItem = this.CreateCollectionItem(resourceItem, resourceTypeSetting);
            if (newItem != null)
            {
                lock (this.collectionItemList)
                {
                    this.collectionItemList.RemoveAll(item => item.Id.Equals(newItem.Id, StringComparison.OrdinalIgnoreCase));
                    this.collectionItemList.Add(newItem);
                }
            }
        }

        /// <summary>
        /// Deletes the collection item.
        /// </summary>
        /// <param name="resourceChangeMessage">The resource change message.</param>
        private void DeleteCollectionItem(ResourceChangeMessage resourceChangeMessage)
        {
            // Argument Validations
            if (this.collectionItemList.Count == 0)
            {
                return;
            }

            // Remove the collection item from the collection item list
            lock (this.collectionItemList)
            {
                this.collectionItemList.RemoveAll(item => item.Id.Equals(resourceChangeMessage.ResourceId.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            lock (this.deepZoomSourceImageList)
            {
                var deepZoomImageItem = this.deepZoomSourceImageList.Where(item => item.Key == resourceChangeMessage.ResourceId).FirstOrDefault();
                if (deepZoomImageItem.Key != default(Guid))
                {
                    try
                    {
                        if (System.IO.File.Exists(deepZoomImageItem.Value))
                        {
                            System.IO.File.Delete(deepZoomImageItem.Value);
                        }

                        string tileImagesFolder = Path.Combine(this.DeepZoomFolder, Path.GetFileNameWithoutExtension(deepZoomImageItem.Value) + "_files");
                        if (Directory.Exists(tileImagesFolder))
                        {
                            Directory.Delete(tileImagesFolder, true);
                        }
                    }
                    finally
                    {
                        this.deepZoomSourceImageList.Remove(deepZoomImageItem.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the Collection Items
        /// </summary>
        /// <param name="resourceTypeItems">Resource Type Items</param>
        /// <param name="resourceTypeSetting">Resource Type Setting</param>
        private void CreateCollectionItems(IEnumerable<Resource> resourceTypeItems, ResourceTypeSetting resourceTypeSetting)
        {
            // Argument Validations
            if (resourceTypeItems == null || resourceTypeItems.Count() == 0)
            {
                throw new ArgumentNullException("resourceTypeItems");
            }

            // Argument Validations
            if (resourceTypeSetting == null || resourceTypeSetting.Facets == null || resourceTypeSetting.Facets.Length == 0)
            {
                throw new ArgumentNullException("resourceTypeSetting");
            }

            #region Parallel ForEach

            try
            {
                this.collectionItemList.Clear();
                ProgressCounter resourceItemsCounter = new ProgressCounter { Total = resourceTypeItems.Count() };
                this.ReportProgress(PublishStage.ProcessingResourceItems, resourceItemsCounter);
                Parallel.ForEach(resourceTypeItems, this.parallelLoopOptions, (resourceItem) =>
                {
                    this.parallelLoopOptions.CancellationToken.ThrowIfCancellationRequested();
                    Item newItem = this.CreateCollectionItem(resourceItem, resourceTypeSetting);

                    if (newItem != null)
                    {
                        lock (this.collectionItemList)
                        {
                            this.collectionItemList.Add(newItem);
                            resourceItemsCounter.Completed++;
                            this.ReportProgress(PublishStage.ProcessingResourceItems, resourceItemsCounter);
                        }

                        Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", string.Format("Items Processed (without image) for {0} : {1}", this.ResourceTypeFullName, resourceItemsCounter.Completed));
                    }
                });
            }
            catch (OperationCanceledException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Publishing has been aborted by user for resource type  : " + this.ResourceTypeFullName);
                throw;
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Exception occurred in parallelization of tasks. Publishing has been aborted.");
                throw;
            }

            #endregion
        }

        /// <summary>
        /// Creates an individual item given a resource and resourceTypeSetting
        /// </summary>
        /// <param name="resource">Individual resource</param>
        /// <param name="resourceTypeSetting">Resource Type Setting</param>
        /// <returns>Returns the item created or else null.</returns>
        private Item CreateCollectionItem(Resource resource, ResourceTypeSetting resourceTypeSetting)
        {
            // Argument Validations
            if (resource == null)
            {
                return null;
            }

            Item collectionItem = new Item
            {
                Id = resource.Id.ToString(),
                Name = resource.Title,
                Description = resource.Description,
                Type = this.ResourceTypeFullName
            };

            if (!string.IsNullOrWhiteSpace(resource.Uri))
            {
                collectionItem.Href = resource.Uri;
            }

            if (resourceTypeSetting.Link != null && !string.IsNullOrWhiteSpace(resourceTypeSetting.Link.PropertyName))
            {
                object propertyValue = GetResourcePropertyValue(resource, resourceTypeSetting.Link.PropertyName, false, null);
                if (propertyValue != null)
                {
                    collectionItem.Href = propertyValue.ToString();
                }
            }

            // Create Facets and assign the respective values.
            List<Facet> facetList = this.CreateCollectionItemFacets(resource, resourceTypeSetting.Facets);
            if (facetList != null)
            {
                Facets facets = new Facets { Facet = facetList.ToArray() };
                collectionItem.Facets = facets;
            }
            else
            {
                return null;
            }

            return collectionItem;
        }

        /// <summary>
        /// Creates the Collection Items
        /// </summary>
        /// <param name="resourceTypeItems">Resource Type Items</param>
        /// <param name="resourceTypeSetting">Resource Type Setting</param>
        private void CreateCollectionItemImages(IEnumerable<Resource> resourceTypeItems, ResourceTypeSetting resourceTypeSetting)
        {
            // Argument Validations
            if (resourceTypeItems == null || resourceTypeItems.Count() == 0)
            {
                return;
            }

            // Argument Validations
            if (resourceTypeSetting == null || resourceTypeSetting.Facets == null || resourceTypeSetting.Facets.Length == 0)
            {
                throw new ArgumentNullException("resourceTypeSetting");
            }

            #region Parallel ForEach

            try
            {
                this.deepZoomSourceImageList.Clear();
                ProgressCounter imageCounter = new ProgressCounter { Total = resourceTypeItems.Count() };
                this.ReportProgress(PublishStage.CreatingImages, imageCounter);
                Parallel.ForEach(resourceTypeItems, this.parallelLoopOptions, (resource) =>
                {
                    this.parallelLoopOptions.CancellationToken.ThrowIfCancellationRequested();
                    var collectionItem = this.collectionItemList.Where(item => item.Id.Equals(resource.Id.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (collectionItem != default(Item))
                    {
                        string imageSource = this.CreateCollectionItemSourceImage(collectionItem, resource, resourceTypeSetting);
                        if (!string.IsNullOrWhiteSpace(imageSource))
                        {
                            collectionItem.Img = string.Empty;
                            imageCounter.Completed++;
                            this.ReportProgress(PublishStage.CreatingImages, imageCounter);
                            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", string.Format("Images generated for collection item {0} : {1}", this.ResourceTypeFullName, imageCounter.Completed));
                        }
                        else
                        {
                            collectionItem.Img = "-";
                        }
                    }
                });
            }
            catch (OperationCanceledException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Publishing has been aborted by user for resource type  : " + this.ResourceTypeFullName);
                throw;
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Exception occurred in parallelization of tasks. Publishing has been aborted.");
                throw;
            }

            #endregion
        }

        /// <summary>
        /// Creates the Collection Items
        /// </summary>
        /// <param name="resourceChangeMessages">The resource change messages which are added or updated.</param>
        /// <param name="resourceTypeItems">The resource type items.</param>
        /// <param name="resourceTypeSetting">Resource Type Setting</param>
        private void CreateCollectionItemImages(IEnumerable<ResourceChangeMessage> resourceChangeMessages, IEnumerable<Resource> resourceTypeItems, ResourceTypeSetting resourceTypeSetting)
        {
            // Argument Validations
            if (resourceChangeMessages == null || resourceChangeMessages.Count() == 0 ||
                resourceTypeItems == null || resourceTypeItems.Count() == 0)
            {
                return;
            }

            // Argument Validations
            if (resourceTypeSetting == null || resourceTypeSetting.Facets == null || resourceTypeSetting.Facets.Length == 0)
            {
                throw new ArgumentNullException("resourceTypeSetting");
            }

            #region Parallel ForEach

            try
            {
                ProgressCounter imageCounter = new ProgressCounter { Total = resourceChangeMessages.Count() };
                this.ReportProgress(PublishStage.CreatingImages, imageCounter);
                Parallel.ForEach(resourceChangeMessages, this.parallelLoopOptions, (changeMessage) =>
                {
                    this.parallelLoopOptions.CancellationToken.ThrowIfCancellationRequested();
                    var collectionItem = this.collectionItemList.Where(item => item.Id.Equals(changeMessage.ResourceId.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    var resourceItem = resourceTypeItems.Where(item => item.Id == changeMessage.ResourceId).FirstOrDefault();
                    if (collectionItem != default(Item) && resourceItem != default(Resource))
                    {
                        string imageSource = this.CreateCollectionItemSourceImage(collectionItem, resourceItem, resourceTypeSetting);
                        if (!string.IsNullOrWhiteSpace(imageSource))
                        {
                            collectionItem.Img = string.Empty;
                            imageCounter.Completed++;
                            this.ReportProgress(PublishStage.CreatingImages, imageCounter);
                            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", string.Format("Images generated for collection item {0} : {1}", this.ResourceTypeFullName, imageCounter.Completed));
                        }
                        else
                        {
                            collectionItem.Img = "-";
                        }
                    }
                });
            }
            catch (OperationCanceledException ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Publishing has been aborted by user for resource type  : " + this.ResourceTypeFullName);
                throw;
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Exception occurred in parallelization of tasks. Publishing has been aborted.");
                throw;
            }

            #endregion
        }

        /// <summary>
        /// Creates the facet list for a given resource an pivotConfigFacets
        /// </summary>
        /// <param name="resource">Individual Resource</param>
        /// <param name="pivotConfigFacets">Pivot Config Facets</param>
        /// <returns>Returns a list of Facet if created or else null.</returns>
        private List<Facet> CreateCollectionItemFacets(Resource resource, IEnumerable<Services.Configuration.Pivot.Facet> pivotConfigFacets)
        {
            List<Facet> collectionItemFacets = new List<Facet>();

            foreach (var configFacetItem in pivotConfigFacets)
            {
                Facet newFacet = new Facet { Name = configFacetItem.DisplayName };

                #region Switch Case for Facet Type

                switch (configFacetItem.DataType)
                {
                    case FacetDataType.String:
                        {
                            object propertyValue = configFacetItem.PropertyName.Equals(typeof(ResourceType).Name, StringComparison.OrdinalIgnoreCase)
                                                   ? resource.GetType().FullName 
                                                   : GetResourcePropertyValue(resource, configFacetItem.PropertyName, false, null);
                            if (propertyValue != null)
                            {
                                List<StringType> stringTypeList = new List<StringType>();
                                if (string.IsNullOrWhiteSpace(configFacetItem.Delimiter))
                                {
                                    if (!string.IsNullOrWhiteSpace(propertyValue.ToString()))
                                        stringTypeList.Add(new StringType { Value = propertyValue.ToString().Trim() });
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(propertyValue.ToString()))
                                    {
                                        string[] delimitedStrings = propertyValue.ToString().Split(new[] { configFacetItem.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                                        stringTypeList.AddRange(from delimitedString in delimitedStrings
                                                                where !string.IsNullOrWhiteSpace(delimitedString)
                                                                select new StringType
                                                                {
                                                                    Value = delimitedString.Trim()
                                                                });
                                    }
                                }

                                if (stringTypeList.Count > 0)
                                {
                                    newFacet.String = stringTypeList.ToArray();
                                    collectionItemFacets.Add(newFacet);
                                }
                            }

                            break;
                        }

                    case FacetDataType.LongString:
                        {
                            object propertyValue = GetResourcePropertyValue(resource, configFacetItem.PropertyName, false, null);
                            if (propertyValue != null)
                            {
                                StringType longStringType = new StringType { Value = (string) propertyValue };

                                List<StringType> longStringTypeList = new List<StringType> { longStringType };
                                if (!string.IsNullOrWhiteSpace(longStringType.Value))
                                {
                                    newFacet.LongString = longStringTypeList.ToArray();
                                    collectionItemFacets.Add(newFacet);
                                }
                            }

                            break;
                        }

                    case FacetDataType.Number:
                        {
                            object propertyValue = GetResourcePropertyValue(resource, configFacetItem.PropertyName, false, null);
                            if (propertyValue != null)
                            {
                                List<NumberType> numberTypeList = new List<NumberType>();
                                if (string.IsNullOrWhiteSpace(configFacetItem.Delimiter))
                                {
                                    if (!string.IsNullOrWhiteSpace(propertyValue.ToString()))
                                        numberTypeList.Add(new NumberType
                                        {
                                            Value = Convert.ToDecimal(propertyValue, CultureInfo.CurrentCulture)
                                        });
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(propertyValue.ToString()))
                                    {
                                        string[] delimitedStrings = propertyValue.ToString().Split(new[] { configFacetItem.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                                        try
                                        {
                                            numberTypeList.AddRange(from delimitedString in delimitedStrings
                                                                    where !string.IsNullOrWhiteSpace(delimitedString)
                                                                    select new NumberType
                                                                    {
                                                                        Value = Convert.ToDecimal(delimitedString, CultureInfo.CurrentCulture)
                                                                    });
                                        }
                                        catch { }
                                    }
                                }

                                if (numberTypeList.Count > 0)
                                {
                                    newFacet.Number = numberTypeList.ToArray();
                                    collectionItemFacets.Add(newFacet);
                                }
                            }

                            break;
                        }

                    case FacetDataType.DateTime:
                        {
                            object propertyValue = GetResourcePropertyValue(resource, configFacetItem.PropertyName, false, null);
                            if (propertyValue != null && propertyValue is DateTime)
                            {
                                DateTimeType dateTimeType = new DateTimeType { Value = (DateTime) propertyValue };

                                List<DateTimeType> dateTimeTypeList = new List<DateTimeType> { dateTimeType };
                                newFacet.DateTime = dateTimeTypeList.ToArray();
                                collectionItemFacets.Add(newFacet);
                            }

                            break;
                        }

                    case FacetDataType.Link:
                        {
                            using (ZentityContext zentityContext = Utilities.CreateZentityContext())
                            {
                                object propertyValue = GetResourcePropertyValue(resource, configFacetItem.PropertyName, true, zentityContext);
                                List<LinkType> linkTypeList = new List<LinkType>();
                                if (propertyValue == null)
                                {
                                    if (resource.EntityState != System.Data.EntityState.Detached)
                                        zentityContext.Detach(resource);
                                    break;
                                }

                                if (propertyValue is IEnumerable<Core.File>)
                                {
                                    // No action for Files collection
                                    if (resource.EntityState != System.Data.EntityState.Detached)
                                        zentityContext.Detach(resource);
                                    break;
                                }

                                if (propertyValue is IEnumerable<Resource>)
                                {
                                    IEnumerable<Resource> relatedCollection = propertyValue as IEnumerable<Resource>;
                                    foreach (Resource relatedResource in relatedCollection)
                                    {
                                        LinkType linkType = new LinkType
                                                                {
                                                                    Name = string.IsNullOrWhiteSpace(relatedResource.Title)
                                                                               ? relatedResource.Id.ToString()
                                                                               : relatedResource.Title
                                                                };

                                        string href = GetRelativeOrAbsolutePath(relatedResource.GetType().FullName,
                                                                                relatedResource.GetType().Name,
                                                                                relatedResource.Id);
                                        if (!string.IsNullOrWhiteSpace(href))
                                        {
                                            linkType.Href = href;
                                        }

                                        linkTypeList.Add(linkType);

                                        if (!this.relatedCollectionList.Contains(relatedResource.GetType().Name))
                                        {
                                            this.relatedCollectionList.Add(relatedResource.GetType().Name);
                                        }
                                    }
                                }
                                else if (propertyValue is Resource)
                                {
                                    Resource propertyResource = propertyValue as Resource;
                                    if (!string.IsNullOrWhiteSpace(propertyResource.Title) &&
                                        !string.IsNullOrWhiteSpace(propertyResource.Uri))
                                    {
                                        LinkType linkType = new LinkType
                                                                {
                                                                    Name = propertyResource.Title,
                                                                    Href = propertyResource.Uri
                                                                };
                                        linkTypeList.Add(linkType);
                                    }
                                }
                                else
                                {
                                    string propValueString = propertyValue.ToString();
                                    if (!string.IsNullOrWhiteSpace(propValueString) &&
                                        !string.IsNullOrWhiteSpace(propValueString))
                                    {
                                        LinkType linkType = new LinkType
                                                                {
                                                                    Name = propValueString,
                                                                    Href = propValueString
                                                                };
                                        linkTypeList.Add(linkType);
                                    }
                                }

                                if (linkTypeList.Count > 0)
                                {
                                    newFacet.Link = linkTypeList.ToArray();
                                    collectionItemFacets.Add(newFacet);
                                }

                                if (resource.EntityState != System.Data.EntityState.Detached)
                                    zentityContext.Detach(resource);
                            }

                            break;
                        }

                    default:
                        break;
                }

                #endregion
            }

            return collectionItemFacets;
        }

        /// <summary>
        /// Creates the collection item source image.
        /// </summary>
        /// <param name="collectionItem">The collection item.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="resourceTypeSetting">The resource type setting.</param>
        /// <returns>The deep zoom source image uri.</returns>
        private string CreateCollectionItemSourceImage(Item collectionItem, Resource resource, ResourceTypeSetting resourceTypeSetting)
        {
            string deepZoomSourceImageUri = string.Empty;
            if (resourceTypeSetting.Visual != null)
            {
                switch (resourceTypeSetting.Visual.Type)
                {
                    case VisualType.ImageResource:
                        {
                            using (ZentityContext zentityContext = Utilities.CreateZentityContext())
                            {
                                object propertyValue = GetResourcePropertyValue(resource, resourceTypeSetting.Visual.PropertyName, true, zentityContext);
                                if (propertyValue == null)
                                {
                                    if (resource.EntityState != System.Data.EntityState.Detached)
                                        zentityContext.Detach(resource);
                                    break;
                                }

                                if (propertyValue is Core.File || propertyValue is IEnumerable<Core.File>)
                                {
                                    IEnumerable<Core.File> fileCollection;
                                    if (propertyValue is Core.File)
                                    {
                                        fileCollection = new[] {propertyValue as Core.File};
                                    }
                                    else
                                    {
                                        fileCollection = propertyValue as IEnumerable<Core.File>;
                                    }

                                    deepZoomSourceImageUri = CreateFileResourceImage(fileCollection);
                                }
                                else if (propertyValue is Core.Resource || propertyValue is IEnumerable<Core.Resource>)
                                {
                                    IEnumerable<Core.File> fileCollection;
                                    if (propertyValue is Core.Resource)
                                    {
                                        fileCollection = (propertyValue as Core.Resource).Files;
                                    }
                                    else
                                    {
                                        List<Core.File> listImageFileResource = new List<Core.File>();
                                        foreach (Core.Resource resourceItem in ((IEnumerable<Resource>) propertyValue))
                                        {
                                            listImageFileResource.AddRange(resourceItem.Files);
                                        }
                                        fileCollection = listImageFileResource;
                                    }

                                    deepZoomSourceImageUri = CreateFileResourceImage(fileCollection);
                                }

                                if (resource.EntityState != System.Data.EntityState.Detached)
                                    zentityContext.Detach(resource);
                            }

                            break;
                        }

                    case VisualType.ImageUri:
                        {
                            object propertyValue = GetResourcePropertyValue(resource, resourceTypeSetting.Visual.PropertyName, false, null);
                            if (propertyValue != null && propertyValue is string)
                            {
                                deepZoomSourceImageUri = (string) propertyValue;
                            }

                            break;
                        }

                    case VisualType.WebCapture:
                        {
                            deepZoomSourceImageUri = this.CreateWebCaptureImage(resource, resourceTypeSetting);
                            break;
                        }

                    case VisualType.Default:
                        {
                            deepZoomSourceImageUri = this.CreateTradeCardImage(collectionItem);
                            break;
                        }
                }

                // Create the Default Trade Card image if other image creation options have failed.
                if (resourceTypeSetting.Visual.Type != VisualType.Default && string.IsNullOrWhiteSpace(deepZoomSourceImageUri))
                {
                    deepZoomSourceImageUri = this.CreateTradeCardImage(collectionItem);
                }
            }

            // Validate and add the image path into the source image dictionary
            if (!string.IsNullOrWhiteSpace(deepZoomSourceImageUri))
            {
                lock (this.deepZoomSourceImageList)
                {
                    if (this.deepZoomSourceImageList.ContainsKey(resource.Id))
                    {
                        this.deepZoomSourceImageList[resource.Id] = deepZoomSourceImageUri;
                    }
                    else
                    {
                        this.deepZoomSourceImageList.Add(resource.Id, deepZoomSourceImageUri);
                    }
                }
                return deepZoomSourceImageUri;
            }

            return string.Empty;
        }

        /// <summary>
        /// Creates the deep zoom images.
        /// </summary>
        /// <param name="sourceImageList">The source image list.</param>
        private void CreateDeepZoomImages(IDictionary<Guid, string> sourceImageList)
        {
            // Argument Validations
            if (sourceImageList == null || sourceImageList.Count == 0)
            {
                return;    
            }

            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<Guid, string>));
            string dictFile = Path.Combine(this.WorkingFolder, "SourceImages.txt");
            using (FileStream dictStream = new FileStream(dictFile, FileMode.Create, FileAccess.Write))
            {
                serializer.WriteObject(dictStream, sourceImageList);
            }

            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "DeepZoom Image Creation Starting for " + this.ResourceTypeFullName + ". Source Image Count = " + this.deepZoomSourceImageList.Count);

            ProgressCounter deepzoomImageCounter = new ProgressCounter {Total = sourceImageList.Count};
            this.ReportProgress(PublishStage.CreatingDeepZoomImages, deepzoomImageCounter);
            using (Process deepZoomCreator = new Process())
            {
                deepZoomCreator.StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(Utilities.AssemblyLocation, "Zentity.Pivot.DeepZoomCreator.exe"),
                    Arguments = string.Format("/operation:createdeepzoom /instanceId:{0} /sourceImageFilePath:\"{1}\"", this.InstanceId, dictFile),
                    UseShellExecute = false
                };
                deepZoomCreator.Start();

                while (!deepZoomCreator.WaitForExit(10000))
                {
                    if (this.cancellationToken.IsCancellationRequested)
                    {
                        deepZoomCreator.Kill();
                    }
                    else
                    {
                        deepZoomCreator.Refresh();
                    }
                }
            }

            try
            {
                System.IO.File.Delete(dictFile);
            }
            catch
            { }

            // Cancel the operation if it has been requested for.
            if (this.cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(this.cancellationToken.Token);
            }
        }

        /// <summary>
        /// Creates the deep zoom collection and assigns the image index to the CXML item element.
        /// </summary>
        /// <param name="collectionItems">The CXML item list.</param>
        private void CreateDeepZoomCollection(IEnumerable<Item> collectionItems)
        {
            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "DeepZoom Collection Creation is starting for " + this.ResourceTypeFullName);

            // Cancel the operation if it has been requested for.
            if (this.cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(this.cancellationToken.Token);
            }

            this.ReportProgress(PublishStage.CreatingDeepZoomCollection, default(ProgressCounter));
            string defaultDeepZoomFileName = Path.GetFileName(this.DefaultDeepZoomPath);
            List<string> deepZoomImageList = new List<string>();
            
            deepZoomImageList.AddRange(Directory.GetFiles(this.DeepZoomFolder, "*" + StringResources.DziExtension));
            deepZoomImageList.RemoveAll(filePath => filePath.Contains(defaultDeepZoomFileName));
            deepZoomImageList.AddRange(collectionItems.Where(item => item.Img == "-").Select(item => this.DefaultDeepZoomPath));

            CollectionCreator collectionCreator = new CollectionCreator();
            string deepZoomCollectionPath = Path.Combine(this.DeepZoomFolder, this.ResourceTypeName + StringResources.DzcExtension);
            collectionCreator.Create(deepZoomImageList, deepZoomCollectionPath);

            // Cancel the operation if it has been requested for.
            if (this.cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(this.cancellationToken.Token);
            }

            DeepZoomClasses.Collection deepZoomCollection = DeserializeDeepZoomCollection(deepZoomCollectionPath);

            if (deepZoomCollection != null)
            {
                foreach (DeepZoomClasses.CollectionI imageItem in deepZoomCollection.Items)
                {
                    bool isDefaultImage = imageItem.Source.Equals(defaultDeepZoomFileName, StringComparison.OrdinalIgnoreCase);
                    if (!isDefaultImage)
                    {
                        string imageItemId = Path.GetFileNameWithoutExtension(imageItem.Source);
                        var collectionItem = (from item in collectionItems
                                              where item != null && !string.IsNullOrWhiteSpace(item.Id) &&
                                                    item.Id.Equals(imageItemId, StringComparison.OrdinalIgnoreCase)
                                              select item).FirstOrDefault();
                        if (collectionItem != default(Item))
                        {
                            collectionItem.Img = "#" + imageItem.Id;
                        }
                    }
                    else
                    {
                        var collectionItem = (from item in collectionItems
                                              where item != null && item.Img == "-"
                                              select item).FirstOrDefault();
                        if (collectionItem != default(Item))
                        {
                            collectionItem.Img = "#" + imageItem.Id;
                        }
                    }
                }
            }

            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "DeepZoom Collection Creation is completed for " + this.ResourceTypeFullName);
        }

        /// <summary>
        /// Creates the deep zoom collection and assigns the image index to the CXML item element.
        /// </summary>
        /// <param name="imagePaths">The image item list.</param>
        private void CreateDeepZoomCollection(IEnumerable<string> imagePaths)
        {
            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "DeepZoom Collection Creation is starting for " + this.ResourceTypeFullName);

            // Cancel the operation if it has been requested for.
            if (this.cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(this.cancellationToken.Token);
            }

            CollectionCreator collectionCreator = new CollectionCreator();
            string deepZoomCollectionPath = Path.Combine(this.DeepZoomFolder, this.ResourceTypeName + StringResources.DzcExtension);
            collectionCreator.Create(imagePaths.ToArray(), deepZoomCollectionPath);

            // Cancel the operation if it has been requested for.
            if (this.cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(this.cancellationToken.Token);
            }

            Parallel.ForEach(this.collectionItemList, (collectionItem, loopState, index) =>
            {
                collectionItem.Img = "#" + index;
            });

            Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "DeepZoom Collection Creation is completed for " + this.ResourceTypeFullName);
        }

        /// <summary>
        /// Creates the default deep zoom image based on an HtmlTemplate.
        /// </summary>
        /// <param name="collectionItem">The collection item.</param>
        /// <returns>The path to the image generated from HtmlTemplate</returns>
        private string CreateTradeCardImage(Item collectionItem)
        {
            try
            {
                using (HtmlImageCreator imageCreator = new HtmlImageCreator())
                {
                    imageCreator.WorkingDirectory = this.ImagesFolder;
                    if (PivotConfigurationSection.Instance != null)
                    {
                        if (PivotConfigurationSection.Instance.GlobalSettings.DefaultDeepZoom != null)
                        {
                            string templateLocation = PivotConfigurationSection.Instance.GlobalSettings.DefaultDeepZoom.TemplateLocation;
                            if (!string.IsNullOrWhiteSpace(templateLocation) && !templateLocation.Equals("default", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    imageCreator.HtmlTemplatePath = templateLocation;
                                }
                                catch (ArgumentException ex)
                                {
                                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                                }
                            }
                        }
                    }

                    return imageCreator.CreateImage(collectionItem);
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the image resource from file entity list.
        /// </summary>
        /// <param name="fileResources">The file resources list.</param>
        /// <returns>The generated image</returns>
        private string CreateFileResourceImage(IEnumerable<Core.File> fileResources)
        {
            string imageFile = string.Empty;

            Core.File imageFileResource = (from fileResource in fileResources
                                           where Utilities.ImageMimeTypes.Contains(fileResource.MimeType, StringComparer.OrdinalIgnoreCase)
                                           select fileResource).FirstOrDefault();

            if (imageFileResource != null)
            {
                string imageFileExtension = string.IsNullOrWhiteSpace(imageFileResource.FileExtension)
                                                ? imageFileResource.MimeType.Split(new[] { StringResources.MimeTypeSplitChar }, StringSplitOptions.RemoveEmptyEntries)[1]
                                                : imageFileResource.FileExtension.Replace(".", string.Empty);

                imageFile = Path.Combine(this.ImagesFolder, string.Format(StringResources.ImageFileNameFormat, imageFileResource.Id, imageFileExtension));
                using (FileStream imageFileStream = new FileStream(imageFile, FileMode.Create, FileAccess.Write))
                {
                    using (ZentityContext zentityContext = Utilities.CreateZentityContext())
                    {
                        zentityContext.Resources.MergeOption = System.Data.Objects.MergeOption.NoTracking;
                        zentityContext.DownloadFileContent(imageFileResource, imageFileStream);
                    }
                }

                return imageFile;
            }

            return imageFile;
        }

        /// <summary>
        /// Gets the Web Capture Image Path
        /// </summary>
        /// <param name="resource">Individual Resource</param>
        /// <param name="resourceTypeSetting">Resource Type Setting</param>
        /// <returns>Returns string containing the path of the deep zoom if it is created or else string.empty</returns>
        private string CreateWebCaptureImage(Resource resource, ResourceTypeSetting resourceTypeSetting)
        {
            object webCaptureUrlPropertyValue = GetResourcePropertyValue(resource, resourceTypeSetting.Visual.PropertyName, false, null);
            if (webCaptureUrlPropertyValue == null || !(webCaptureUrlPropertyValue is string))
            {
                return string.Empty;
            }

            string webCaptureImagePath = string.Empty;
            string webCaptureUrl = (string) webCaptureUrlPropertyValue;
            if (string.IsNullOrWhiteSpace(webCaptureUrl))
            {
                return string.Empty;
            }

            #region Creating Instance of WebCapture Provider

            // Reflecting the type and creating the instance of the WebCapture provider.
            if (PivotConfigurationSection.Instance != null)
            {
                if (PivotConfigurationSection.Instance.GlobalSettings.WebCapture != null)
                {
                    if (!string.IsNullOrWhiteSpace(PivotConfigurationSection.Instance.GlobalSettings.WebCapture.Type))
                    {
                        try
                        {
                            Type imageCaptureProviderType = Type.GetType(PivotConfigurationSection.Instance.GlobalSettings.WebCapture.Type, false, true);
                            if (typeof(IImageCapture).IsAssignableFrom(imageCaptureProviderType))
                            {
                                using (IImageCapture webCaptureProviderInstance = (IImageCapture) Activator.CreateInstance(imageCaptureProviderType))
                                {
                                    if (webCaptureProviderInstance != null)
                                    {
                                        webCaptureProviderInstance.WorkingDirectory = this.ImagesFolder;
                                        webCaptureImagePath = webCaptureProviderInstance.GenerateImage(new Uri(webCaptureUrl), WebSnapshotCreator.DefaultWidth, WebSnapshotCreator.DefaultHeight);
                                    }
                                    else
                                    {
                                        throw new System.Configuration.ConfigurationErrorsException(StringMessages.InvalidWebCaptureType);
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Globals.TraceMessage(TraceEventType.Error, exception.InnerException != null ? exception.InnerException.ToString() : exception.ToString(), exception.Message);
                        }
                    }
                }
            }

            #endregion

            return webCaptureImagePath;
        }

        /// <summary>
        /// Publishes the intermediate collection.
        /// </summary>
        /// <param name="collectionFacetCategories">The collection facet categories.</param>
        private void PublishIntermediateCollection(FacetCategories collectionFacetCategories)
        {
            // Argument Validations
            if (collectionFacetCategories == null)
            {
                throw new ArgumentNullException("collectionFacetCategories", StringMessages.NullFacetCategoriesMessage);
            }

            if (string.IsNullOrWhiteSpace(this.DefaultDeepZoomPath))
            {
                Globals.TraceMessage(TraceEventType.Information, string.Empty, "The default deepzoom image was unavailable. Publishing of intermediate collection will be skipped.");
                return;
            }

            if (this.collectionItemList.Count == 0)
            {
                Globals.TraceMessage(TraceEventType.Information, string.Empty, "The pivot item collection is empty. Publishing of intermediate collection will be skipped.");
                return;
            }

            // Cancel the operation if it has been requested for.
            if (this.cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(this.cancellationToken.Token);
            }

            try
            {
                Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "Intermediate Collection publishing is starting for " + this.ResourceTypeFullName);

                // Create the DeepZoom collection for the default DeepZoom images
                this.ReportProgress(PublishStage.PublishIntermediateCollection, default(ProgressCounter));
                List<string> defaultImageList = new List<string>(this.collectionItemList.Select(item => Path.Combine(this.WorkingFolder, this.DefaultDeepZoomPath)));
                this.CreateDeepZoomCollection(defaultImageList);

                // Split the Final Collection into parts and serialize them into .cxml files
                var orderedCollectionItems = this.collectionItemList.OrderBy(i => i.Name).ToList();
                this.SplitFinalCollection(orderedCollectionItems, collectionFacetCategories);

                // Copy the Web.Config file to the Output Folder to disable caching
                CopyWebConfig(this.ResourceTypeOutputFolder);

                // Delete the existing Pivot Collection
                DeleteFolder(this.ResourceTypeOutputFolder, true);

                // Copy the new intermediate collection to the output folder
                CopyFolder(this.WorkingFolder, this.ResourceTypeOutputFolder, true, true);
                
                Globals.TraceMessage(TraceEventType.Information, "STATUS MESSAGE", "Intermediate Collection publishing is completed for " + this.ResourceTypeFullName);
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Exception occurred while publishing the intermediate collection. Publishing of the complete collection will proceed normally.");
            }

            // Cancel the operation if it has been requested for.
            if (this.cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(this.cancellationToken.Token);
            }
        }

        /// <summary>
        /// Splits the Collection
        /// </summary>
        /// <param name="collectionItems">Cxml Item List</param>
        /// <param name="collectionFacetCategories">Cxml Facet Categories</param>
        private void SplitFinalCollection(IEnumerable<Item> collectionItems, FacetCategories collectionFacetCategories)
        {
            int splitSizeValue;
            int loopCount = GetPageCount(collectionItems.Count(), out splitSizeValue);

            for (int index = 1, offset = 0; index <= loopCount; index++)
            {
                Items pivotCollectionItems = new Items();
                string deepZoomCollectionPath = Path.Combine(this.DeepZoomFolder, this.ResourceTypeName + StringResources.DzcExtension);
                pivotCollectionItems.ImgBase = new Uri(this.WorkingFolder).MakeRelativeUri(new Uri(deepZoomCollectionPath)).ToString();
                List<Item> cxmlSplitItemList = collectionItems.Skip(offset).Take(splitSizeValue).ToList();
                offset = offset + splitSizeValue;

                if (cxmlSplitItemList.Count > 0)
                {
                    if (loopCount >= 2)
                    {
                        this.AddCollectionPageLinks(cxmlSplitItemList, loopCount, index);
                    }
                }
                else
                {
                    Globals.TraceMessage(TraceEventType.Error, StringMessages.NullSplitItemListMessage, string.Empty);
                    return;
                }

                pivotCollectionItems.Item = cxmlSplitItemList.ToArray();
                Collection pivotCollection = new Collection
                {
                    Name = this.ResourceTypeFullName,
                    SchemaVersion = 1.0M,
                    FacetCategories = collectionFacetCategories,
                    Items = new[] { pivotCollectionItems }
                };

                if (pivotCollection.FacetCategories != null && pivotCollection.Items != null)
                {
                    this.SerializeCollection(pivotCollection, (loopCount == 1) ? -1 : index);
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Serialize Collection to Cxml File
        /// </summary>
        /// <param name="pivotCollection">Collection object</param>
        /// <param name="index">Index for the Split Colletions</param>
        private void SerializeCollection(Collection pivotCollection, int index)
        {
            // Argument Validations
            if (pivotCollection == null)
            {
                return;
            }

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("p", StringResources.PivotCollectionSchemaNamespace);

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Collection));
                using (StreamWriter file = new StreamWriter(Path.Combine(this.WorkingFolder, (this.ResourceTypeName + (index == -1 ? string.Empty : index.ToString(CultureInfo.CurrentCulture)) + StringResources.PivotCollectionFileExtension))))
                {
                    XmlWriterSettings writerSettings = new XmlWriterSettings();
                    #if !DEBUG   
                    {
                        writerSettings.Indent = false;
                        writerSettings.NewLineChars = string.Empty;
                        writerSettings.NewLineHandling = NewLineHandling.Replace;
                    }
                    #else
                    {
                        writerSettings.Indent = true;
                    }
                    #endif
                    
                    XmlWriter writer = XmlWriter.Create(file, writerSettings);
                    if (writer != null) xmlSerializer.Serialize(writer, pivotCollection, ns);
                }
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.InnerException != null ? exception.InnerException.ToString() : exception.ToString(), exception.Message);
            }
        }

        /// <summary>
        /// Deserializes the pivot collection from a collection of .cxml files.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <returns>Pivot collection object</returns>
        private Collection DeserializeCollection(string sourceFolder)
        {
            // Argument Validations
            if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                return null;
            }

            try
            {
                IEnumerable<string> collectionFileList = Directory.GetFiles(sourceFolder, StringResources.CxmlExtension);
                FacetCategories newCollectionFacetCategories = null;
                List<Item> newCollectionItemList = new List<Item>();
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Collection));
                string deepZoomCollectionPath = string.Empty;

                foreach (string collectionFilePath in collectionFileList)
                {
                    try
                    {
                        using (StreamReader file = new StreamReader(collectionFilePath))
                        {
                            Collection tempCollection = (Collection) xmlSerializer.Deserialize(file);
                            if (string.IsNullOrWhiteSpace(deepZoomCollectionPath))
                            {
                                deepZoomCollectionPath = tempCollection.Items[0].ImgBase;
                                newCollectionFacetCategories = tempCollection.FacetCategories;
                            }
                            newCollectionItemList.AddRange(tempCollection.Items[0].Item);
                        }
                        System.IO.File.Delete(collectionFilePath);
                    }
                    catch (Exception exception)
                    {
                        Globals.TraceMessage(TraceEventType.Error, exception.InnerException != null ? exception.InnerException.ToString() : exception.ToString(), exception.Message);
                    }
                }

                Collection existingCollection = new Collection
                {
                    FacetCategories = newCollectionFacetCategories,
                    Items = new Items[] {
                                            new Items
                                                {
                                                    Item = newCollectionItemList.ToArray<Item>(),
                                                    ImgBase = deepZoomCollectionPath
                                                }
                                        }
                };

                if (!string.IsNullOrWhiteSpace(deepZoomCollectionPath))
                {
                    deepZoomCollectionPath = Path.Combine(this.WorkingFolder, deepZoomCollectionPath);
                    DeepZoomClasses.Collection deepZoomCollection = DeserializeDeepZoomCollection(deepZoomCollectionPath);
                    if (deepZoomCollection != null)
                    {
                        foreach (DeepZoomClasses.CollectionI imageItem in deepZoomCollection.Items)
                        {
                            Guid itemId;
                            if (Guid.TryParse(Path.GetFileNameWithoutExtension(imageItem.Source), out itemId))
                            {
                                this.deepZoomSourceImageList.Add(itemId, Path.Combine(this.DeepZoomFolder, imageItem.Source));
                            }
                            else
                            {
                                var collectionItem = (from item in newCollectionItemList
                                                      where item != null
                                                      let itemImageIndex = item.Img.Replace("#", string.Empty)
                                                      where itemImageIndex.Equals(imageItem.Id.ToString(), StringComparison.OrdinalIgnoreCase)
                                                      select item).FirstOrDefault();
                                if (collectionItem != default(Item))
                                {
                                    collectionItem.Img = "-";
                                }
                            }
                        }
                    }
                }

                if (existingCollection.FacetCategories == null || existingCollection.FacetCategories.FacetCategory == null || existingCollection.FacetCategories.FacetCategory.Length == 0 || existingCollection.Items.Length == 0)
                {
                    Globals.TraceMessage(TraceEventType.Error, string.Empty, StringMessages.NullCollectionChildMessage);
                    return null;
                }
                
                return existingCollection;
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.InnerException != null ? exception.InnerException.ToString() : exception.ToString(), exception.Message);
            }

            return null;
        }

        /// <summary>
        /// Adds the Page links to other Cxml's.
        /// </summary>
        /// <param name="collectionItems">The Split Cxml ITem List</param>
        /// <param name="loopCount">Number of Cxml's to be generated</param>
        /// <param name="index">Index for the cxml generation</param>
        private void AddCollectionPageLinks(IEnumerable<Item> collectionItems, int loopCount, int index)
        {
            foreach (Item item in collectionItems)
            {
                ItemExtension itemExtension = new ItemExtension();
                List<Copyright> cxmlPageLinks = new List<Copyright>();
                for (int littleIndex = 1; littleIndex <= loopCount; littleIndex++)
                {
                    if (littleIndex != index)
                    {
                        cxmlPageLinks.Add(new Copyright { Name = this.ResourceTypeName + littleIndex, Href = this.ResourceTypeName + littleIndex + StringResources.PivotCollectionFileExtension });
                    }
                }

                if (cxmlPageLinks.Count > 0)
                {
                    itemExtension.Related = cxmlPageLinks.ToArray();
                    item.Extension = itemExtension;
                }
                else
                {
                    Globals.TraceMessage(TraceEventType.Error, StringMessages.NullEmptyMessage, StringMessages.NullItemExtensinTitle);
                }
            }
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        private void Cleanup()
        {
            this.ReportProgress(PublishStage.PerformingCleanup, default(ProgressCounter));

            DeleteFolder(this.WorkingFolder);
            DeleteFolder(this.ImagesFolder);

            this.ReportProgress(PublishStage.Completed, default(ProgressCounter));
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Deletes the folder on thread.
        /// </summary>
        /// <param name="foldersToDelete">The folders to delete.</param>
        public static void DeleteFolderOnThread(object foldersToDelete)
        {
            IEnumerable<string> folderList = foldersToDelete as IEnumerable<string>;
            if (folderList != null)
            {
                foreach (var folder in folderList)
                {
                    try
                    {
                        ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe");
                        procStartInfo.Arguments = string.Format(CultureInfo.InvariantCulture, "/c rmdir \"{0}\" /s /q", folder);
                        procStartInfo.UseShellExecute = false;
                        Process.Start(procStartInfo);
                    }
                    catch (Exception ex)
                    {
                        Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the collection folder recursively.
        /// </summary>
        /// <param name="folderToDelete">The folder to delete.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        public static void DeleteFolder(string folderToDelete, bool throwOnError = false)
        {
            int retryCount = 0;
            Exception lastException = null;
            while (retryCount < Zentity.CollectionCreator.Program.MaxRetries)
            {
                try
                {
                    // TODO: Change the implementation into recursive using Parallel.Foreach
                    if (Directory.Exists(folderToDelete))
                    {
                        Directory.GetFiles(folderToDelete).ToList().ForEach(System.IO.File.Delete);
                        Directory.Delete(folderToDelete, true);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message + " : Retry Count : " + retryCount);
                    lastException = ex;
                    retryCount++;
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }

            if (retryCount == Zentity.CollectionCreator.Program.MaxRetries && throwOnError)
            {
                throw lastException ?? new IOException("Error occured while copying the file : " + folderToDelete);
            }
        }

        /// <summary>
        /// Copies the contents from the SourceFolder to DestinationFolder
        /// </summary>
        /// <param name="sourceFolder">Source Folder</param>
        /// <param name="destinationFolder">Destination Folder</param>
        /// <param name="enableRecursive">if set to <c>true</c> [enable recursive].</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        private static void CopyFolder(string sourceFolder, string destinationFolder, bool enableRecursive = true, bool throwOnError = false)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourceFolder);
            try
            {
                if (!Directory.Exists(destinationFolder))
                {
                    int retryCount = 0;
                    Exception lastException = null;
                    while (retryCount < Zentity.CollectionCreator.Program.MaxRetries)
                    {
                        try
                        {
                            Directory.CreateDirectory(destinationFolder);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message + " : Retry Count : " + retryCount);
                            lastException = ex;
                            retryCount++;
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                        }
                    }

                    if (retryCount == Zentity.CollectionCreator.Program.MaxRetries && throwOnError)
                    {
                        throw lastException ?? new IOException("Error occured while creating the folder : " + destinationFolder);
                    }
                }

                if (enableRecursive)
                {
                    // Copy the sub directories recursively
                    DirectoryInfo[] subDirectories = sourceDirectory.GetDirectories();
                    Parallel.ForEach(subDirectories, dirItem => CopyFolder(dirItem.FullName, Path.Combine(destinationFolder, dirItem.Name), enableRecursive, throwOnError));
                }

                // Copy the files in the source folder
                FileInfo[] sourceDirFiles = sourceDirectory.GetFiles();
                Parallel.ForEach(sourceDirFiles, fileItem =>
                {
                    int retryCount = 0;
                    Exception lastException = null;
                    while (retryCount < Zentity.CollectionCreator.Program.MaxRetries)
                    {
                        try
                        {
                            System.IO.File.Copy(fileItem.FullName, Path.Combine(destinationFolder, fileItem.Name), true);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message + " : Retry Count : " + retryCount);
                            lastException = ex;
                            retryCount++;
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                        }
                    }

                    if (retryCount == Zentity.CollectionCreator.Program.MaxRetries && throwOnError)
                    {
                        throw lastException ?? new IOException("Error occured while copying the file : " + fileItem.FullName);
                    }
                });
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                if (throwOnError)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Copies the web config to disable caching in IIS.
        /// </summary>
        /// <param name="destinationFolder">The destination folder.</param>
        private static void CopyWebConfig(string destinationFolder)
        {
            try
            {
                if (Directory.Exists(destinationFolder))
                {
                    string webConfigFileName = Path.Combine(destinationFolder, StringResources.WebConfigFileName);
                    System.IO.File.WriteAllText(webConfigFileName, StringResources.WebConfigDisableCaching);
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }
        }

        /// <summary>
        /// Creates the FacetCategories for a particualr ResourceTypeSetting
        /// </summary>
        /// <param name="resourceTypeSetting">Resource Type Setting</param>
        /// <returns>Returns the FacetCategories</returns>
        private static FacetCategories CreateCollectionFacetCategories(ResourceTypeSetting resourceTypeSetting)
        {
            // Argument Validations
            if (resourceTypeSetting == null || resourceTypeSetting.Facets == null || resourceTypeSetting.Facets.Length == 0)
            {
                throw new ArgumentNullException("resourceTypeSetting", StringMessages.NullResourceTypeSettingMessage);
            }

            List<FacetCategory> facetCategoryList = new List<FacetCategory>();

            foreach (var configFacet in resourceTypeSetting.Facets)
            {
                FacetCategory facetCategory = new FacetCategory
                {
                    Name = !string.IsNullOrWhiteSpace(configFacet.DisplayName) ? configFacet.DisplayName : configFacet.PropertyName,
                    IsFilterVisible = configFacet.ShowInFilter,
                    IsFilterVisibleSpecified = true,
                    IsMetaDataVisible = configFacet.ShowInInfoPane,
                    IsMetaDataVisibleSpecified = true,
                    IsWordWheelVisible = configFacet.KeywordSearch,
                    IsWordWheelVisibleSpecified = true,
                    Type = (FacetType) configFacet.DataType
                };

                if (!string.IsNullOrWhiteSpace(configFacet.Format))
                {
                    facetCategory.Format = configFacet.Format;
                }

                if (!string.IsNullOrWhiteSpace(facetCategory.Name))
                {
                    facetCategoryList.Add(facetCategory);
                }
            }

            if (facetCategoryList.Count > 0)
            {
                var categoryArray = facetCategoryList.OrderBy(facetCat => facetCat.Name).ToArray();
                FacetCategories cxmlFacetCategories = new FacetCategories { FacetCategory = categoryArray };
                return cxmlFacetCategories;
            }

            return null;
        }

        /// <summary>
        /// Gets the Resource's Property Value for a given Resource and Property
        /// </summary>
        /// <param name="resourceItem">Specific Resource</param>
        /// <param name="propertyName">Resource's Property Name</param>
        /// <param name="loadRelated">if set to <c>true</c> load related entities.</param>
        /// <param name="zentityContext">The zentity context to use for fetching related entities.</param>
        /// <returns>Returns object containing the value.</returns>
        private static object GetResourcePropertyValue(Resource resourceItem, string propertyName, bool loadRelated, ZentityContext zentityContext)
        {
            // Argument Validations
            if (resourceItem == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            Type resourceItemType = resourceItem.GetType();
            PropertyInfo propInfo = resourceItemType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propInfo == null)
            {
                return null;
            }

            object propertyValue = propInfo.GetValue(resourceItem, null);

            // This is for navigation properties and collections.
            // The related entities are loaded.
            if (loadRelated && propertyValue != null && propertyValue is RelatedEnd && zentityContext != null)
            {
                try
                {
                    zentityContext.AttachTo(StringResources.ResourcesName, resourceItem);
                    RelatedEnd relatedEnd = propertyValue as RelatedEnd;
                    if (!relatedEnd.IsLoaded)
                    {
                        relatedEnd.Load(System.Data.Objects.MergeOption.AppendOnly);
                    }
                }
                catch (Exception ex)
                {
                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                }
            }

            return propertyValue;
        }

        /// <summary>
        /// Gets the path for the split collections which is of type relative or absolute
        /// </summary>
        /// <param name="resourceFullName">Resource Full Name</param>
        /// <param name="resourceName">Resource Name</param>
        /// <param name="resourceId">Globally Unique Identifier</param>
        /// <returns>Returns the string representation of the path [relative or absolute]</returns>
        private static string GetRelativeOrAbsolutePath(string resourceFullName, string resourceName, Guid resourceId)
        {
            string navigationPropertyPath;
            if (PivotConfigurationSection.Instance != null)
            {
                if (PivotConfigurationSection.Instance.GlobalSettings.OutputSettings != null)
                {
                    switch (PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.UriFormat.Format)
                    {
                        case UriFormatType.Absolute:
                            if (PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.BaseUri != null &&
                                !string.IsNullOrWhiteSpace(PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.BaseUri.Uri))
                            {
                                navigationPropertyPath =
                                    PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.BaseUri.Uri +
                                    resourceFullName + Path.AltDirectorySeparatorChar + resourceName + StringResources.IdQueryString + resourceId;
                                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, StringMessages.FinishedFunctionMessage);
                                return navigationPropertyPath;
                            }

                            break;

                        case UriFormatType.Relative:
                            navigationPropertyPath = StringResources.ParentDirectory + resourceFullName + Path.AltDirectorySeparatorChar + resourceName + StringResources.IdQueryString + resourceId;
                            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, StringMessages.FinishedFunctionMessage);
                            return navigationPropertyPath;
                    }
                }
            }

            navigationPropertyPath = StringResources.ParentDirectory + resourceFullName + Path.AltDirectorySeparatorChar + resourceName + StringResources.IdQueryString + resourceId;
            return navigationPropertyPath;
        }

        /// <summary>
        /// Gets the PageCount used for splitting the collection.
        /// </summary>
        /// <param name="itemCount">Number of items in the Collection.</param>
        /// <param name="splitSizeValue">Number of items in one Collection.</param>
        /// <returns>Returns the page count.</returns>
        private static int GetPageCount(int itemCount, out int splitSizeValue)
        {
            splitSizeValue = itemCount;
            if (PivotConfigurationSection.Instance != null)
            {
                if (PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.SplitSize != null)
                {
                    if (PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.SplitSize.Value != 0)
                    {
                        splitSizeValue = Math.Abs(PivotConfigurationSection.Instance.GlobalSettings.OutputSettings.SplitSize.Value);
                    }
                }
            }

            return (int) Math.Ceiling(itemCount / Convert.ToDouble(splitSizeValue));
        }

        /// <summary>
        /// Deserializes the deep zoom collection.
        /// </summary>
        /// <param name="dzcFilePath">The DZC file path.</param>
        /// <returns>DeepZoom collection object</returns>
        private static DeepZoomClasses.Collection DeserializeDeepZoomCollection(string dzcFilePath)
        {
            // Argument Validations
            if (string.IsNullOrWhiteSpace(dzcFilePath) || !System.IO.File.Exists(dzcFilePath))
            {
                return null;
            }
            
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof (DeepZoomClasses.Collection));
                using (StreamReader dzcStream = new StreamReader(dzcFilePath))
                {
                    DeepZoomClasses.Collection tempCollection = (DeepZoomClasses.Collection) xmlSerializer.Deserialize(dzcStream);
                    return tempCollection;
                }
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.InnerException != null ? exception.InnerException.ToString() : exception.ToString(), exception.Message);
            }
            
            return null;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.progressServiceClient.Close();
            this.cancellationToken.Dispose();
            DeleteFolder(this.WorkingFolder);
            DeleteFolder(this.ImagesFolder);
        }
        
        #endregion

        #region IPublishingProgressServiceCallback Members

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        public void CancelOperation()
        {
            Globals.TraceMessage(TraceEventType.Information, string.Empty, "Cancellation requested by user for " + this.ResourceTypeFullName);
            if (this.cancellationToken != null && this.cancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    this.cancellationToken.Cancel();
                }
                catch (Exception ex)
                {
                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// FacetCategoryComparer class is used for comparison of FacetCategory instances
    /// </summary>
    internal class FacetCategoryComparer : IEqualityComparer<FacetCategory>
    {
        public bool Equals(FacetCategory x, FacetCategory y)
        {
            return (x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase) && x.Type == y.Type);
        }

        public int GetHashCode(FacetCategory obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
