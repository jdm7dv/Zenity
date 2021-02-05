// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System.Collections.Generic;
using System.Linq;
using System;

namespace Zentity.Core
{
    /// <summary>
    /// Structure to store the changes between two data models.
    /// </summary>
    internal sealed class ModuleCollectionChange
    {
        #region Fields

        List<Association> addedAssociations;
        List<DataModelModule> addedDataModelModules;
        List<NavigationProperty> addedNavigationProperties;
        List<ResourceType> addedResourceTypes;
        List<ScalarProperty> addedScalarProperties;
        List<Association> deletedAssociations;
        List<DataModelModule> deletedDataModelModules;
        List<NavigationProperty> deletedNavigationProperties;
        List<ResourceType> deletedResourceTypes;
        List<ScalarProperty> deletedScalarProperties;
        Dictionary<Association, Association> updatedAssociations;
        Dictionary<DataModelModule, DataModelModule> updatedDataModelModules;
        Dictionary<NavigationProperty, NavigationProperty> updatedNavigationProperties;
        Dictionary<ResourceType, ResourceType> updatedResourceTypes;
        Dictionary<ScalarProperty, ScalarProperty> updatedScalarProperties;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the added associations.
        /// </summary>
        /// <value>The added associations.</value>
        internal List<Association> AddedAssociations
        {
            get { return addedAssociations; }
        }

        /// <summary>
        /// Gets the added data model modules.
        /// </summary>
        /// <value>The added data model modules.</value>
        internal List<DataModelModule> AddedDataModelModules
        {
            get { return addedDataModelModules; }
        }

        /// <summary>
        /// Gets the added navigation properties.
        /// </summary>
        /// <value>The added navigation properties.</value>
        internal List<NavigationProperty> AddedNavigationProperties
        {
            get { return addedNavigationProperties; }
        }

        /// <summary>
        /// Gets the added resource types.
        /// </summary>
        /// <value>The added resource types.</value>
        internal List<ResourceType> AddedResourceTypes
        {
            get { return addedResourceTypes; }
        }

        /// <summary>
        /// Gets the added scalar properties.
        /// </summary>
        /// <value>The added scalar properties.</value>
        internal List<ScalarProperty> AddedScalarProperties
        {
            get { return addedScalarProperties; }
        }

        /// <summary>
        /// Gets the deleted associations.
        /// </summary>
        /// <value>The deleted associations.</value>
        internal List<Association> DeletedAssociations
        {
            get { return deletedAssociations; }
        }

        /// <summary>
        /// Gets the deleted data model modules.
        /// </summary>
        /// <value>The deleted data model modules.</value>
        internal List<DataModelModule> DeletedDataModelModules
        {
            get { return deletedDataModelModules; }

        }

        /// <summary>
        /// Gets the deleted navigation properties.
        /// </summary>
        /// <value>The deleted navigation properties.</value>
        internal List<NavigationProperty> DeletedNavigationProperties
        {
            get { return deletedNavigationProperties; }
        }

        /// <summary>
        /// Gets the deleted resource types.
        /// </summary>
        /// <value>The deleted resource types.</value>
        internal List<ResourceType> DeletedResourceTypes
        {
            get { return deletedResourceTypes; }
        }

        /// <summary>
        /// Gets the deleted scalar properties.
        /// </summary>
        /// <value>The deleted scalar properties.</value>
        internal List<ScalarProperty> DeletedScalarProperties
        {
            get { return deletedScalarProperties; }
        }

        /// <summary>
        /// Gets the updated associations.
        /// </summary>
        /// <value>The updated associations.</value>
        internal Dictionary<Association, Association> UpdatedAssociations
        {
            get { return updatedAssociations; }
        }

        /// <summary>
        /// Gets the updated data model modules.
        /// </summary>
        /// <value>The updated data model modules.</value>
        internal Dictionary<DataModelModule, DataModelModule> UpdatedDataModelModules
        {
            get { return updatedDataModelModules; }
        }

        /// <summary>
        /// Gets the updated navigation properties.
        /// </summary>
        /// <value>The updated navigation properties.</value>
        internal Dictionary<NavigationProperty, NavigationProperty> UpdatedNavigationProperties
        {
            get { return updatedNavigationProperties; }
        }

        /// <summary>
        /// Gets the updated resource types.
        /// </summary>
        /// <value>The updated resource types.</value>
        internal Dictionary<ResourceType, ResourceType> UpdatedResourceTypes
        {
            get { return updatedResourceTypes; }
        }

        /// <summary>
        /// Gets the updated scalar properties.
        /// </summary>
        /// <value>The updated scalar properties.</value>
        internal Dictionary<ScalarProperty, ScalarProperty> UpdatedScalarProperties
        {
            get { return updatedScalarProperties; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleCollectionChange"/> class.
        /// </summary>
        internal ModuleCollectionChange()
        {
            // Initialize lists.
            addedAssociations = new List<Association>();
            addedDataModelModules = new List<DataModelModule>();
            addedNavigationProperties = new List<NavigationProperty>();
            addedResourceTypes = new List<ResourceType>();
            addedScalarProperties = new List<ScalarProperty>();

            deletedAssociations = new List<Association>();
            deletedDataModelModules = new List<DataModelModule>();
            deletedNavigationProperties = new List<NavigationProperty>(); 
            deletedResourceTypes = new List<ResourceType>();
            deletedScalarProperties = new List<ScalarProperty>();

            updatedAssociations = new Dictionary<Association, Association>(); 
            updatedDataModelModules = new Dictionary<DataModelModule, DataModelModule>();
            updatedNavigationProperties = new Dictionary<NavigationProperty, NavigationProperty>();
            updatedResourceTypes = new Dictionary<ResourceType, ResourceType>();
            updatedScalarProperties = new Dictionary<ScalarProperty, ScalarProperty>();
        }

        #endregion
    }
}
