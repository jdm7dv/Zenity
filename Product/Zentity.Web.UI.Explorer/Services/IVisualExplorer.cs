// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Explorer
{
    using System.ServiceModel;

    /// <summary>
    /// Contract for VisualExplorerService used by VisualExplorer Silverlight client
    /// </summary>
    [ServiceContract]    
    public interface IVisualExplorerService
    {
        /// <summary>
        /// Gets VisualExplorer graph by the searched keyword
        /// </summary>
        /// <param name="keyword">Searched keyword</param>
        /// <returns>VisualExplorerGraph object</returns>
        [OperationContract]
        VisualExplorerGraph GetVisualExplorerGraphBySearchKeyword(string keyword);

        /// <summary>
        /// Gets VisualExplorer graph by ResourceId
        /// </summary>
        /// <param name="resourceId">ResourceId from Zentity store</param>
        /// <returns>VisualExplorerGraph object</returns>
        [OperationContract]
        VisualExplorerGraph GetVisualExplorerGraphByResourceId(string resourceId);

        /// <summary>
        /// Get metadata for the specified ResourceId
        /// </summary>
        /// <param name="resourceId">ResourceId from Zentity store</param>
        /// <returns>Resource metadata</returns>
        [OperationContract]
        string GetResourceMetadataByResourceId(string resourceId);

        /// <summary>
        /// Get Resource relation by ResourceId
        /// </summary>
        /// <param name="subjectResourceId">Identifier of the subject resource</param>
        /// <param name="objectResourceId">Identifier of the object resource</param>
        /// <returns>Resource relation metadata</returns>
        [OperationContract]
        string GetResourceRelationByResourceId(string subjectResourceId, string objectResourceId);

        /// <summary>
        /// Get resources by search keyword
        /// </summary>
        /// <param name="keyword">search keyword</param>
        /// <returns>List of resources</returns>
        [OperationContract]
        string GetResourcesByKeyword(string keyword);

        /// <summary>
        /// Get List of ResourceTypes and RelationTypes in Zentity store
        /// </summary>
        /// <returns>VisualExplorerFilterList object</returns>
        [OperationContract]
        VisualExplorerFilterList GetVisualExplorerFilterList();
    }
}
