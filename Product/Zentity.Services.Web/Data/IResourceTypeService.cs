// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Data
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using Zentity.Core;

    /// <summary>
    /// IResourceTypeService
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IResourceTypeService
    {
        /// <summary>
        /// Lists all resource types available within a particular data model aka model namespace.
        /// </summary>
        /// <param name="modelNameSpace">Namespace filter</param>
        /// <returns>Collection of ResourceTypes</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        ResourceTypeCollection GetAllResourceTypesByNamespace(string modelNameSpace);

        /// <summary>
        /// Get count of resources for a resource type.
        /// </summary>
        /// <param name="modelNamespace">Data Model Namespace name</param>
        /// <param name="resourceTypeName">Resource Type Name</param>
        /// <returns>Count of Resources</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        int GetResourceCountForResourceType(string modelNamespace, string resourceTypeName);

        /// <summary>
        /// Gets the resource count for data model.
        /// </summary>
        /// <param name="dataModelNamesapceList">The data model namesapce list.</param>
        /// <param name="totalUniqueItemCount">The total unique item count.</param>
        /// <returns>Dictionary of ResouceType names and the item count</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        IDictionary<string, int> GetResourceCountForDataModel(IEnumerable<string> dataModelNamesapceList, out int totalUniqueItemCount);

        /// <summary>
        /// Lists all scalar properties available within a particular resource type for a data model aka model namespace.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <returns>Returns a Scalar Property Collection</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        ScalarPropertyCollection GetAllScalarPropertiesForResourceType(string modelNamespace, string resourceType);

        /// <summary>
        /// Will update an existing data model resource type by adding a scalar property.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="dataType">Type of the property to be added</param>
        /// <param name="maxLength">Maximum length.</param>
        /// <param name="precision">The precision.</param>
        /// <param name="scale">The scale.</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void AddScalarPropertyToResourceType(string modelNamespace, string resourceType, string propertyName, Zentity.Core.DataTypes dataType, int? maxLength, int? precision, int? scale);

        /// <summary>
        /// Will update an existing data model resource type by deleting a scalar property.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <param name="propertyName">Name of the property to be deleted</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void DeleteScalarPropertyOfResourceType(string modelNamespace, string resourceType, string propertyName);

        /// <summary>
        /// Lists all navigation properties available within a particular resource type for a data model aka model namespace.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <returns>Returns Navigation Property Collection</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        NavigationPropertyCollection GetAllNavigationPropertiesForResourceType(string modelNamespace, string resourceType);

        /// <summary>
        /// Will update an existing data model resource type by adding a navigation property.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="subjectResourceType">Subject esource Type string</param>
        /// <param name="objectResourceType">Object esource Type string</param>
        /// <param name="subjectNavigationPropertyName">Name of the Subject Navigation property to be added </param>
        /// <param name="objectNavigationPropertyName">Name of the Object Navigation property to be added </param>
        /// <param name="associationName">Association name</param>
        /// <param name="subjectMultiplicity">Association Subject Multiplicity of the Navigation property</param>
        /// <param name="objectMultiplicity">Association Object Multiplicity of the Navigation property</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void AddNavigationPropertyToResourceType(string modelNamespace, string subjectResourceType,
                                                 string objectResourceType,
                                                 string subjectNavigationPropertyName,
                                                 string objectNavigationPropertyName, string associationName,
                                                 Core.AssociationEndMultiplicity subjectMultiplicity,
                                                 Core.AssociationEndMultiplicity objectMultiplicity);

        /// <summary>
        /// Will update an existing data model resource type by deleting a navigation property.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <param name="propertyName">Name of the Navigation property to be deleted</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void DeleteNavigationPropertyOfResourceType(string modelNamespace, string resourceType, string propertyName);
    }
}
