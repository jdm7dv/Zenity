// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Pivot
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    /// <summary>
    /// Interface for Publishing Service
    /// </summary>
    [ServiceContract(SessionMode=SessionMode.NotAllowed)]
    public interface IPublishingService
    {
        /// <summary>
        /// Create Cxml for a new Resource Type
        /// </summary>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type name.</param>
        /// <returns>The instance id of the publish request.</returns>
        [OperationContract(IsOneWay = false)]
        Guid CreateCollection(string modelNamespace, string resourceType);

        /// <summary>
        /// Add, Update, Delete the existing cxml's item list.
        /// </summary>
        /// <param name="resourceChangeMessageFilePath">Path of the serialized file of ResourceChangeMessage(s)</param>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type name.</param>
        /// <param name="changeMessage">The change message.</param>
        /// <returns>The instance id of the publish request.</returns>
        [OperationContract(IsOneWay = false)]
        Guid UpdateCollection(string resourceChangeMessageFilePath, string modelNamespace, string resourceType, ResourceChangeMessage changeMessage = null);

        /// <summary>
        /// Delete the Cxml for resource type.
        /// </summary>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type name.</param>
        [OperationContract(IsOneWay = true)]
        void DeletePublishedCollection(string modelNamespace, string resourceType);

        /// <summary>
        /// Cancels the CXML creation.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <returns>
        /// true if cancellation request was accepted. false otherwise
        /// </returns>
        [OperationContract(IsOneWay = false)]
        bool CancelPublishRequestByInstanceID(Guid instanceId);

        /// <summary>
        /// Cancels the type of the publish request by resource.
        /// </summary>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type name.</param>
        /// <returns>
        /// true if cancellation request was accepted. false otherwise
        /// </returns>
        [OperationContract(IsOneWay = false)]
        bool CancelPublishRequestByResourceType(string modelNamespace, string resourceType);

        /// <summary>
        /// Gets a list of all publishing status.
        /// </summary>
        /// <returns>A list of PublishStatus objects in the tracking dictionary</returns>
        [OperationContract(IsOneWay = false)]
        IEnumerable<PublishStatus> GetAllPublishingStatus();

        /// <summary>
        /// Get the status of Cxml creation for a new Resource Type
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <returns>Publish status.</returns>
        [OperationContract(IsOneWay=false)]
        PublishStatus GetPublishingStatusByInstanceID(Guid instanceId);

        /// <summary>
        /// Gets the publishing status by resource type name.
        /// </summary>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type name.</param>
        /// <returns>Publish status.</returns>
        [OperationContract(IsOneWay = false)]
        PublishStatus GetPublishingStatusByResourceType(string modelNamespace, string resourceType);

        /// <summary>
        /// Gets a list of all publishing requests present in the queue.
        /// </summary>
        /// <returns>A list of PublishStatus objects in the queue</returns>
        [OperationContract(IsOneWay = false)]
        IEnumerable<PublishStatus> GetAllQueuedRequests();

        /// <summary>
        /// Gets the queued request by instance ID.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <returns>A list of PublishStatus objects in the queue</returns>
        [OperationContract(IsOneWay = false)]
        PublishStatus GetQueuedRequestByInstanceID(Guid instanceId);


        /// <summary>
        /// Gets publishing requests for a data model resource type present in the queue.
        /// </summary>
        /// <param name="modelNamespace">The model namespace.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns>A list of PublishStatus objects in the queue</returns>
        [OperationContract(IsOneWay = false)]
        IEnumerable<PublishStatus> GetQueuedRequestsByResourceType(string modelNamespace, string resourceType);

        /// <summary>
        /// Gets the position of queued request by instance ID.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <returns>Position in the queue</returns>
        [OperationContract(IsOneWay = false)]
        int GetQueuedPositionByInstanceID(Guid instanceId);
    }
}
