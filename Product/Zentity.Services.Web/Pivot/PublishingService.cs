// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Pivot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using Zentity.Core;

    /// <summary>
    /// Publishing Service class
    /// </summary>
    public class PublishingService : IPublishingService
    {
        #region IPublishingService Members

        /// <summary>
        /// Create Cxml for a new Resource Type
        /// </summary>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type for which cxml is to be generated.</param>
        /// <returns>The instance id of the publish request.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public Guid CreateCollection(string modelNamespace, string resourceType)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);

            if (string.IsNullOrWhiteSpace(modelNamespace) || string.IsNullOrWhiteSpace(resourceType))
            {
                Globals.TraceMessage(TraceEventType.Verbose, Properties.Messages.NullEmptyMessage, Properties.Messages.NullEmptyTitle);
                return Guid.Empty;
            }
            
            // Get the ResourceType list for the given Data Model from the Zentity DB
            IEnumerable<ResourceType> resourceTypes = Utilities.GetResourceTypes(modelNamespace);
            if (resourceTypes == null)
            {
                throw new ArgumentException("modelNamespace", string.Format(Properties.Messages.DataModelNamespaceDoesNotExist, modelNamespace));
            }

            // Check if the given ResourceType is present in the list of ResourceTypes from Zentity DB
            ResourceType specificResourceType = resourceTypes.Where(resType => resType.Name.Equals(resourceType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (specificResourceType == null)
            {
                throw new ArgumentException("resourceType", string.Format(Properties.Messages.ResourceTypeDoesNotExistInNamespace, resourceType, modelNamespace));
            }

            Guid newGuid = Guid.NewGuid();
            PublishRequest publishRequest = new PublishRequest(newGuid, specificResourceType.Parent.NameSpace, specificResourceType.Name);
            publishRequest.CurrentStatus = new PublishStatus(newGuid) { ResourceTypeName = specificResourceType.Name, DataModelNamespace = specificResourceType.Parent.NameSpace };
            publishRequest.Operation = PublishOperation.CreateCollection;
            PublishQueueProcessor.Current.UpdateRequestQueue(publishRequest);
            Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(Properties.Messages.CreateCollectionRequestQueued, specificResourceType.FullName, newGuid));

            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return newGuid;
        }

        /// <summary>
        /// Add, Update, Delete the existing cxml's item list.
        /// </summary>
        /// <param name="resourceChangeMessageFilePath">Path of the serialized file of ResourceChangeMessage(s)</param>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type for which cxml is to be generated.</param>
        /// <param name="changeMessage">The change message.</param>
        /// <returns>The instance id of the publish request.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public Guid UpdateCollection(string resourceChangeMessageFilePath, string modelNamespace, string resourceType, ResourceChangeMessage changeMessage = null)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);

            if (string.IsNullOrWhiteSpace(resourceChangeMessageFilePath) || string.IsNullOrWhiteSpace(modelNamespace) || string.IsNullOrWhiteSpace(resourceType))
            {
                Globals.TraceMessage(TraceEventType.Verbose, Properties.Messages.NullEmptyMessage, Properties.Messages.NullEmptyTitle);
                return Guid.Empty;
            }

            // Get the ResourceType list for the given Data Model from the Zentity DB
            IEnumerable<ResourceType> resourceTypes = Utilities.GetResourceTypes(modelNamespace);
            if (resourceTypes == null)
            {
                throw new ArgumentException("modelNamespace", string.Format(Properties.Messages.DataModelNamespaceDoesNotExist, modelNamespace));
            }

            // Check if the given ResourceType is present in the list of ResourceTypes from Zentity DB
            ResourceType specificResourceType = resourceTypes.Where(resType => resType.Name.Equals(resourceType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (specificResourceType == null)
            {
                throw new ArgumentException("resourceType", string.Format(Properties.Messages.ResourceTypeDoesNotExistInNamespace, resourceType, modelNamespace));
            }

            Guid newGuid = Guid.NewGuid();
            PublishRequest publishRequest = new PublishRequest(newGuid, specificResourceType.Parent.NameSpace, specificResourceType.Name);
            publishRequest.ChangeMessageFilePath = resourceChangeMessageFilePath;
            publishRequest.Operation = PublishOperation.UpdateCollection;
            publishRequest.CurrentStatus = new PublishStatus(newGuid) { ResourceTypeName = specificResourceType.Name, DataModelNamespace = specificResourceType.Parent.NameSpace };
            PublishQueueProcessor.Current.Submit(publishRequest);
            Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(Properties.Messages.UpdateCollectionRequestQueued, specificResourceType.FullName, newGuid));

            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return newGuid;
        }

        /// <summary>
        /// Delete the Cxml for resource type.
        /// </summary>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type for which cxml is to be deleted.</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void DeletePublishedCollection(string modelNamespace, string resourceType)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);

            if (string.IsNullOrWhiteSpace(modelNamespace) || string.IsNullOrWhiteSpace(resourceType))
            {
                Globals.TraceMessage(TraceEventType.Verbose, Properties.Messages.NullEmptyMessage, Properties.Messages.NullEmptyTitle);
                return;
            }

            try
            {
                Admin.ConfigurationService configurationService = new Admin.ConfigurationService();
                if (configurationService.OpenHostConfigurationFile())
                {
                    string destinationOutputFolder = configurationService.GetOutputFolderLocation();
                    if (!string.IsNullOrWhiteSpace(destinationOutputFolder))
                    {
                        string resourceTypeFullName = string.Format(Properties.Resources.ResourceTypeFullNameFormat, modelNamespace, resourceType);
                        destinationOutputFolder = Path.Combine(destinationOutputFolder, resourceTypeFullName);
                        if (Directory.Exists(destinationOutputFolder))
                        {
                            Directory.GetFiles(destinationOutputFolder).ToList().ForEach(System.IO.File.Delete);
                            Directory.Delete(destinationOutputFolder, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }

            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
        }

        /// <summary>
        /// Cancels the CXML creation.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <returns>
        /// true if cancellation request was accepted. false otherwise
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool CancelPublishRequestByInstanceID(Guid instanceId)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            var returnFlag = PublishQueueProcessor.Current.CancelPublishRequest(instanceId);
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return returnFlag;
        }

        /// <summary>
        /// Cancels the CXML creation.
        /// </summary>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type for which cxml is to be deleted.</param>
        /// <returns>
        /// true if cancellation request was accepted. false otherwise
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public bool CancelPublishRequestByResourceType(string modelNamespace, string resourceType)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            var returnFlag = PublishQueueProcessor.Current.CancelPublishRequest(modelNamespace, resourceType);
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return returnFlag;
        }

        /// <summary>
        /// Gets the status of CXML creation.
        /// </summary>
        /// <returns>
        /// Enumeration of PublishStatus for all the resource types or else null
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public IEnumerable<PublishStatus> GetAllPublishingStatus()
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            var publishStatusList = PublishQueueProcessor.RequestTracker.Select(processStatus => processStatus.CurrentStatus);
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return publishStatusList;
        }

        /// <summary>
        /// Gets the status of CXML creation.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <returns>PublishStatus or else null</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public PublishStatus GetPublishingStatusByInstanceID(Guid instanceId)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            PublishRequest publishRequest = PublishQueueProcessor.RequestTracker.Where(requestTracker => requestTracker.InstanceId == instanceId).FirstOrDefault();
            var publishStatus = publishRequest == default(PublishRequest) ? null : publishRequest.CurrentStatus;
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return publishStatus;
        }

        /// <summary>
        /// Gets the status of CXML creation.
        /// </summary>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type for which cxml is to be deleted.</param>
        /// <returns>
        /// Enumeration of PublishStatus for a particular resource type in the model namespace or else null
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public PublishStatus GetPublishingStatusByResourceType(string modelNamespace, string resourceType)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            PublishRequest returnValue = PublishQueueProcessor.RequestTracker.Where(requestTracker => requestTracker.DataModelNamespace.Equals(modelNamespace, StringComparison.OrdinalIgnoreCase) &&
                                                                                                      requestTracker.ResourceTypeName.Equals(resourceType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var publishStatus = returnValue == null ? null : returnValue.CurrentStatus;
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return publishStatus;
        }

        /// <summary>
        /// Gets the status of CXML creation in the queue.
        /// </summary>
        /// <returns>
        /// Enumeration of PublishRequest for all the resource types or else null
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public IEnumerable<PublishStatus> GetAllQueuedRequests()
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            var publishStatusList = PublishQueueProcessor.Current.GetPublishStatusFromQueue().Select(request => request.CurrentStatus).ToList();
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return publishStatusList;
        }

        /// <summary>
        /// Gets the queued request by instance ID.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <returns>
        /// A list of PublishStatus objects in the queue
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public PublishStatus GetQueuedRequestByInstanceID(Guid instanceId)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            IEnumerable<PublishRequest> queuedRequests = PublishQueueProcessor.Current.GetPublishStatusFromQueue().Where(request => request.InstanceId == instanceId);
            var publishStatus = queuedRequests != null && queuedRequests.Count() > 0 ? queuedRequests.Select(request => request.CurrentStatus).FirstOrDefault() : null;
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return publishStatus;
        }

        /// <summary>
        /// Gets publishing requests for a data model resource type present in the queue.
        /// </summary>
        /// <param name="modelNamespace"></param>
        /// <param name="resourceType"></param>
        /// <returns>
        /// A list of PublishStatus objects in the queue
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public IEnumerable<PublishStatus> GetQueuedRequestsByResourceType(string modelNamespace, string resourceType)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            IEnumerable<PublishRequest> queuedRequests = PublishQueueProcessor.Current.GetPublishStatusFromQueue().Where(request => request.DataModelNamespace.Equals(modelNamespace, StringComparison.OrdinalIgnoreCase) &&
                                                                                                                                    request.ResourceTypeName.Equals(resourceType, StringComparison.OrdinalIgnoreCase));
            var publishStatusList = queuedRequests != null && queuedRequests.Count() > 0 ? queuedRequests.Select(request => request.CurrentStatus).ToList() : null;
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);

            return publishStatusList;
        }

        /// <summary>
        /// Gets the position of queued request by instance ID.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <returns>Position in the queue</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public int GetQueuedPositionByInstanceID(Guid instanceId)
        {
            var queuedStatus = GetAllQueuedRequests().Select((item, index) => new { Index = index + 1, Status = item });
            
            // If there are no queued request
            if (queuedStatus.Count() == 0)
            {
                return -1;
            }

            var indexedItem = queuedStatus.Where(item => item.Status.InstanceId == instanceId).FirstOrDefault();
            if (indexedItem != null)
            {
                return indexedItem.Index;
            }

            return -1;
        }

        #endregion
    }
}
