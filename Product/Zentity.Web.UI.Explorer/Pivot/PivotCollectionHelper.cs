// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.Explorer.Pivot
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml.Serialization;
    using Zentity.Pivot;
    using Zentity.Web.UI.Explorer.PublishingServiceReference;

    /// <summary>
    /// Helper class to aggregate information of all published Pivot collections
    /// </summary>
    public class PivotCollectionHelper
    {
        /// <summary>
        /// Holds the Virtual Directory path configured for Collections output folder
        /// </summary>
        private static string pathPrefix = ConfigurationManager.AppSettings[ConfigurationKeys.PathPrefix];

        /// <summary>
        /// Get data of the Published collections
        /// </summary>
        /// <param name="request">HttpRequest object</param>
        /// <returns>A datatable with all Published collections</returns>
        public static PivotCollectionItemDataSet.PivotCollectionItemDataTable GetAllPivotCollectionItems(HttpRequest request)
        {
            PivotCollectionItemDataSet.PivotCollectionItemDataTable result = new PivotCollectionItemDataSet.PivotCollectionItemDataTable();

            try
            {
                string collectionFilePath = ConfigurationManager.AppSettings[ConfigurationKeys.CollectionFilePath];
                if (string.IsNullOrWhiteSpace(collectionFilePath))
                {
                    throw new ArgumentNullException(Messages.CollectionFilePathEmpty);
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(collectionFilePath);
                if (!directoryInfo.Exists)
                {
                    throw new ArgumentException(Messages.CollectionFilePathInvalid);
                }

                IEnumerable<DirectoryInfo> publishedCollections = directoryInfo.EnumerateDirectories();
                foreach (DirectoryInfo publishedCollection in publishedCollections)
                {
                    var item = result.NewPivotCollectionItemRow();
                    item.Name = publishedCollection.Name;
                    if (item.Name.Contains("."))
                    {
                        item.DataModel = item.Name.Remove(item.Name.LastIndexOf('.'));
                        item.ResourceType = item.Name.Replace(item.DataModel + ".", string.Empty);
                    }
                    else
                    {
                        item.DataModel = item.Name;
                        item.ResourceType = item.Name;
                    }

                    if (!string.IsNullOrWhiteSpace(item.Name))
                    {
                        string path = string.Format(
                                        "{0}://{1}/VisualExplorer/{2}/{3}/{4}.cxml",
                                        request.Url.Scheme,
                                        request.Url.Authority,
                                        pathPrefix, 
                                        item.Name, 
                                        item.ResourceType);
                        path = request.RequestContext.HttpContext.Server.UrlEncode(path);
                        path = string.Format("Viewer.aspx?Uri={0}", path);
                        item.Path = path;
                    }

                    item.LastUpdated = publishedCollection.CreationTimeUtc.ToString();
                    result.Rows.Add(item);
                }
            }
            catch (Exception exception)
            {
                Zentity.Services.Web.Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
            }

            return result;
        }

        /// <summary>
        /// Get status for Pivot collection items
        /// </summary>
        /// <param name="request">HttpRequest object</param>
        /// <returns>List of PublishingCollectionItem objects for all resource types</returns>
        internal static PublishingCollectionItems GetAllPivotCollectionItemsForPivot(HttpRequest request)
        {
            IEnumerable<DirectoryInfo> directories = EnumeratePublishingFolder();

            PublishingServiceReference.PublishingServiceClient client = new PublishingServiceReference.PublishingServiceClient();
            PublishingCollectionItems result = new PublishingCollectionItems();
            IDictionary<string, int> resources = GetResourceCount();
            int totalResourceCount = 0;
            resources.TryGetValue("Zentity.Core.Resource", out totalResourceCount);

            foreach (KeyValuePair<string, int> resource in resources)
            {
                if (resource.Key.Equals("Zentity.Core.Resource"))
                {
                    continue;
                }

                DirectoryInfo specificDirectory = directories.Where(d => d.Name.Equals(resource.Key)).FirstOrDefault();
                string dataModelName = string.Empty, resourceTypeName = string.Empty;
                if (resource.Key.Contains("."))
                {
                    dataModelName = resource.Key.Remove(resource.Key.LastIndexOf('.'));
                    resourceTypeName = resource.Key.Replace(dataModelName + ".", string.Empty);
                }

                PublishingCollectionItem item = new PublishingCollectionItem();
                PublishStatus publishStatus = client.GetPublishingStatusByResourceType(dataModelName, resourceTypeName);
                item.DataModel = dataModelName;
                item.ResourceType = resourceTypeName;
                item.NumberOfResources = resource.Value;
                item.TotalNoOfResources = totalResourceCount;

                if (publishStatus == null)
                {
                    List<PublishStatus> publishStatusFromQueue = client.GetQueuedRequestsByResourceType(dataModelName, resourceTypeName);
                    if (publishStatusFromQueue != null && publishStatusFromQueue.Count > 0)
                    {
                        item.Status = PublishingStatus.Queued;
                    }
                    else
                    {
                        item.Status = PublishingStatus.NotStarted;
                    }
                }
                else
                {
                    if (publishStatus.CurrentStage == PublishStage.Completed || publishStatus.CurrentStage == PublishStage.AbortedOnDemand || publishStatus.CurrentStage == PublishStage.AbortedOnError)
                    {
                        List<PublishStatus> publishStatusFromQueue = client.GetQueuedRequestsByResourceType(dataModelName, resourceTypeName);
                        if (publishStatusFromQueue != null && publishStatusFromQueue.Count > 0)
                        {
                            item.Status = PublishingStatus.Queued;
                        }
                        else
                        {
                            item.LastUpdated = publishStatus.EndTime.Equals(DateTime.MinValue) ? string.Empty : publishStatus.EndTime.ToString();
                            SetItemStatus(item, publishStatus);
                        }
                    }
                    else
                    {
                        item.LastUpdated = publishStatus.EndTime.Equals(DateTime.MinValue) ? string.Empty : publishStatus.EndTime.ToString();
                        SetItemStatus(item, publishStatus);
                    }
                }

                CreateCollectionData(item, specificDirectory, request);
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Helper method to get Dictionary of ResourceTypes and count
        /// </summary>
        /// <returns>Dictionary of ResourceTypes and count</returns>
        private static IDictionary<string, int> GetResourceCount()
        {
            List<string> dataModels;
            using (DataModelingServiceReference.DataModelServiceClient dataModelService = new DataModelingServiceReference.DataModelServiceClient())
            {
                dataModels = dataModelService.GetAllDataModels().ToList();
                if (dataModels.Contains("Zentity.Core"))
                {
                    dataModels.Remove("Zentity.Core");
                }

                if (dataModels.Contains("Zentity.Security.Authorization"))
                {
                    dataModels.Remove("Zentity.Security.Authorization");
                }
            }

            using (ResourceTypeServiceReference.ResourceTypeServiceClient resourceTypeService = new ResourceTypeServiceReference.ResourceTypeServiceClient())
            {
                int totalCount = 0;
                IDictionary<string, int> resourceCountDictionary = resourceTypeService.GetResourceCountForDataModel(out totalCount, dataModels);
                resourceCountDictionary.Add("Zentity.Core.Resource", totalCount);
                return resourceCountDictionary;
            }
        }

        /// <summary>
        /// Helper method to get Dictionary of collection files and count
        /// </summary>
        /// <param name="specificDirectory">DirectoryInfo of the path where Pivot collection files are present</param>
        /// <returns>Dictionary of collection files and count</returns>
        private static IDictionary<string, int> GetItemCount(DirectoryInfo specificDirectory)
        {
            IDictionary<string, int> itemCount = new Dictionary<string, int>();
            int totalItemsCount = 0;
            try
            {
                IEnumerable<FileInfo> fileInfo = specificDirectory.EnumerateFiles("*.cxml");
                XmlSerializer reader = new XmlSerializer(typeof(Collection));
                if (fileInfo != null && fileInfo.Count() > 0)
                {
                    foreach (FileInfo item in fileInfo)
                    {
                        using (System.IO.StreamReader file = new System.IO.StreamReader(item.FullName))
                        {
                            Collection tempCollection = (Collection)reader.Deserialize(file);
                            if (tempCollection != null && tempCollection.Items != null && tempCollection.Items.Count() > 0 && tempCollection.Items[0].Item != null)
                            {
                                totalItemsCount = totalItemsCount + tempCollection.Items[0].Item.Count();
                                itemCount.Add(item.Name, tempCollection.Items[0].Item.Count());
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            itemCount.Add("TotalItemsCount", totalItemsCount);
            return itemCount;
        }

        /// <summary>
        /// Helper method to get all the folders within Pivot collection output folder
        /// </summary>
        /// <returns>List of directories</returns>
        private static IEnumerable<DirectoryInfo> EnumeratePublishingFolder()
        {
            string collectionFilePath = ConfigurationManager.AppSettings[ConfigurationKeys.CollectionFilePath];
            if (string.IsNullOrWhiteSpace(collectionFilePath))
            {
                throw new ArgumentNullException(Messages.CollectionFilePathEmpty);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(collectionFilePath);
            if (!directoryInfo.Exists)
            {
                throw new ArgumentException(Messages.CollectionFilePathInvalid);
            }

            return directoryInfo.EnumerateDirectories();
        }

        /// <summary>
        /// Set status of a Pivot collection
        /// </summary>
        /// <param name="item">PublishingCollectionItem object</param>
        /// <param name="status">status to be set</param>        
        private static void SetItemStatus(PublishingCollectionItem item, PublishStatus status)
        {
            #region Set CurrentStage and populate collection data based on the stage
            switch (status.CurrentStage)
            {
                case PublishStage.NotStarted:
                    item.Status = string.Format("{0}", PublishingStatus.NotStarted);
                    break;
                case PublishStage.Initiating:
                    item.Status = string.Format("{0}", PublishingStatus.Initiating);
                    break;
                case PublishStage.FetchingResourceItems:
                    item.Status = string.Format("{0}", PublishingStatus.FetchingResourceItems);
                    break;
                case PublishStage.ProcessingResourceItems:
                    item.Status = string.Format("{0} ({1} of {2})", PublishingStatus.ProcessingResourceItems, status.ResourceItems.Completed, status.ResourceItems.Total);
                    break;
                case PublishStage.PublishIntermediateCollection:
                    item.Status = string.Format("{0}", PublishingStatus.PublishingIntermediateCollection);
                    break;
                case PublishStage.CreatingImages:
                    item.Status = string.Format("{0} ({1} of {2})", PublishingStatus.CreatingImages, status.Images.Completed, status.Images.Total);
                    break;
                case PublishStage.CreatingDeepZoomImages:
                    item.Status = string.Format("{0} ({1} of {2})", PublishingStatus.CreatingDeepZoomImages, status.DeepZoomImages.Completed, status.DeepZoomImages.Total);
                    break;
                case PublishStage.CreatingDeepZoomCollection:
                    item.Status = string.Format("{0}", PublishingStatus.CreatingDeepZoomCollection);
                    break;
                case PublishStage.DeletingExistingCollection:
                    item.Status = string.Format("{0}", PublishingStatus.DeletingExistingCollection);
                    break;
                case PublishStage.CopyingNewCollection:
                    item.Status = string.Format("{0}", PublishingStatus.CopyingNewCollection);
                    break;
                case PublishStage.CopyingExistingCollection:
                    item.Status = string.Format("{0}", PublishingStatus.CopyingExistingCollection);
                    break;
                case PublishStage.PerformingCleanup:
                    item.Status = string.Format("{0}", PublishingStatus.PerformingCleanup);
                    break;
                case PublishStage.Completed:
                    item.Status = string.Format("{0}", PublishingStatus.Completed);
                    break;
                case PublishStage.AbortedOnError:
                    item.Status = string.Format("{0}", PublishingStatus.AbortedOnError);
                    break;
                case PublishStage.AbortedOnDemand:
                    item.Status = string.Format("{0}", PublishingStatus.AbortedByUser);
                    break;
            }
            #endregion
        }

        /// <summary>
        /// Get collection data for a specified ResourceType
        /// </summary>
        /// <param name="item">PublishingCollectionItem object</param>
        /// <param name="specificDirectory">Directory of Pivot collections</param>
        /// <param name="request">HttpRequest object</param>
        private static void CreateCollectionData(PublishingCollectionItem item, DirectoryInfo specificDirectory, HttpRequest request)
        {
            List<CollectionData> collectionData = new List<CollectionData>();
            if (specificDirectory != null)
            {
                IDictionary<string, int> itemsCount = GetItemCount(specificDirectory);
                int count = 0;
                itemsCount.TryGetValue("TotalItemsCount", out count);
                item.TotalNoOfElements = count;
                itemsCount.Remove("TotalItemsCount");
                if (itemsCount != null && itemsCount.Count > 0)
                {
                    foreach (KeyValuePair<string, int> data in itemsCount)
                    {
                        string path = string.Format(
                                        "{0}://{1}/VisualExplorer/{2}/{3}/{4}",
                                        request.Url.Scheme,
                                        request.Url.Authority,
                                        pathPrefix, 
                                        specificDirectory.Name, 
                                        data.Key);
                        path = request.RequestContext.HttpContext.Server.UrlEncode(path);
                        path = string.Format("Viewer.aspx?Uri={0}", path);
                        collectionData.Add(new CollectionData { Name = data.Key, NumberOfElements = data.Value, Path = path });
                    }

                    item.Collection = collectionData;
                }
                else
                {
                    collectionData.Add(new CollectionData { NumberOfElements = 0 });
                    item.Collection = collectionData;
                }
            }
            else
            {
                collectionData.Add(new CollectionData { NumberOfElements = 0 });
                item.Collection = collectionData;
            }
        }
    }
}