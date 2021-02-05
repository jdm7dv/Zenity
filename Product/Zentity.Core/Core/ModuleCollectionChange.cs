// *********************************************************
// 
//     Copyright (c) Microsoft. All rights reserved.
//     This code is licensed under the Apache License, Version 2.0.
//     THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//     ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//     IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//     PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// 
// *********************************************************

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

        List<DataModelModule> addedDataModelModules;
        List<DataModelModule> deletedDataModelModules;
        Dictionary<DataModelModule, DataModelModule> updatedDataModelModules;

        List<ResourceType> addedResourceTypes;
        List<ResourceType> deletedResourceTypes;
        Dictionary<ResourceType, ResourceType> updatedResourceTypes;

        List<ScalarProperty> addedScalarProperties;
        List<ScalarProperty> deletedScalarProperties;
        Dictionary<ScalarProperty, ScalarProperty> updatedScalarProperties;

        List<NavigationProperty> addedNavigationProperties;
        List<NavigationProperty> deletedNavigationProperties;
        Dictionary<NavigationProperty, NavigationProperty> updatedNavigationProperties;
        
        List<Association> addedAssociations;
        List<Association> deletedAssociations;
        Dictionary<Association, Association> updatedAssociations;

        #endregion

        #region Properties

        internal List<DataModelModule> AddedDataModelModules
        {
            get { return addedDataModelModules; }
        }

        internal List<DataModelModule> DeletedDataModelModules
        {
            get { return deletedDataModelModules; }

        }

        internal Dictionary<DataModelModule, DataModelModule> UpdatedDataModelModules
        {
            get { return updatedDataModelModules; }
        }
        
        internal List<ResourceType> AddedResourceTypes
        {
            get { return addedResourceTypes; }
        }

        internal List<ResourceType> DeletedResourceTypes
        {
            get { return deletedResourceTypes; }
        }

        internal Dictionary<ResourceType, ResourceType> UpdatedResourceTypes
        {
            get { return updatedResourceTypes; }
        }

        internal List<ScalarProperty> AddedScalarProperties
        {
            get { return addedScalarProperties; }
        }

        internal List<ScalarProperty> DeletedScalarProperties
        {
            get { return deletedScalarProperties; }
        }

        internal Dictionary<ScalarProperty, ScalarProperty> UpdatedScalarProperties
        {
            get { return updatedScalarProperties; }
        }

        internal List<NavigationProperty> AddedNavigationProperties
        {
            get { return addedNavigationProperties; }
        }

        internal List<NavigationProperty> DeletedNavigationProperties
        {
            get { return deletedNavigationProperties; }
        }

        internal Dictionary<NavigationProperty, NavigationProperty> UpdatedNavigationProperties
        {
            get { return updatedNavigationProperties; }
        }

        internal List<Association> AddedAssociations
        {
            get { return addedAssociations; }
        }

        internal List<Association> DeletedAssociations
        {
            get { return deletedAssociations; }
        }

        internal Dictionary<Association, Association> UpdatedAssociations
        {
            get { return updatedAssociations; }
        }

        #endregion

        #region Constructors

        internal ModuleCollectionChange()
        {
            // Initialize the lists.
            updatedDataModelModules = new Dictionary<DataModelModule, DataModelModule>();
            deletedDataModelModules = new List<DataModelModule>();
            addedDataModelModules = new List<DataModelModule>();

            addedResourceTypes = new List<ResourceType>();
            deletedResourceTypes = new List<ResourceType>();
            updatedResourceTypes = new Dictionary<ResourceType, ResourceType>();

            addedScalarProperties = new List<ScalarProperty>();
            deletedScalarProperties = new List<ScalarProperty>();
            updatedScalarProperties = new Dictionary<ScalarProperty, ScalarProperty>();

            updatedNavigationProperties = new Dictionary<NavigationProperty, NavigationProperty>();
            deletedNavigationProperties = new List<NavigationProperty>();
            addedNavigationProperties = new List<NavigationProperty>();

            updatedAssociations = new Dictionary<Association, Association>();
            deletedAssociations = new List<Association>();
            addedAssociations = new List<Association>();

        }

        #endregion
    }
}
