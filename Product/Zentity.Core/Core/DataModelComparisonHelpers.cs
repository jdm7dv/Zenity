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

using System;
using System.Linq;
using System.Collections.Generic;

namespace Zentity.Core
{
    public sealed partial class DataModel
    {
        internal static ModuleCollectionChange ComputeDifferences(DataModel originalModel, DataModel newModel)
        {
            DataModelModuleCollection originalModules = originalModel.Modules;
            DataModelModuleCollection newModules = newModel.Modules;

            // All the comparisons are based on data model item Id values. It is thus important
            // to make all Id properties on the data model items internal so that once an item
            // is assigned an Id, that cannot change in its entire life.
            // Assumption: Earlier validations ensure that all the items are present within this
            // model and are not shared between data models. For example, there is no possibility
            // that an association present in this data model has one of its navigation properties
            // in this model but the other navigation property in some other model. Likewise, it
            // should not be possible for two data models to reach the same resource type while 
            // enumerating the hosted types. An example source code is shown below. Here, it may
            // be possible to reach 'Resource' resource type from module2 but its Parent property
            // points to module1.
            //var module1 = new ZentityContext().DataModel.Modules[0];
            //var module2 = new ZentityContext().DataModel.Modules[0];
            //module1.ResourceTypes.Add(module2.ResourceTypes["Resource"]);

            ModuleCollectionChange changes = new ModuleCollectionChange();

            CompareDataModelModules(changes, originalModules, newModules);

            CompareResourceTypes(changes, originalModules, newModules);

            CompareScalarProperties(changes, originalModules, newModules);

            CompareNavigationProperties(changes, originalModules, newModules);

            CompareAssociations(changes, originalModules, newModules);

            return changes;
        }

        private static void CompareDataModelModules(ModuleCollectionChange changes, DataModelModuleCollection originalModules, DataModelModuleCollection targetModules)
        {
            // Compute added modules.
            List<Guid> addedGuids = new List<Guid>();

            addedGuids.AddRange(targetModules.Select(tuple => tuple.Id).
                Except(originalModules.Select(tuple => tuple.Id)));

            changes.AddedDataModelModules.AddRange(targetModules.
                Where(tuple => addedGuids.Contains(tuple.Id)));

            // Compute removed modules.
            List<Guid> removedGuids = new List<Guid>();

            removedGuids.AddRange(originalModules.Select(tuple => tuple.Id).
                Except(targetModules.Select(tuple => tuple.Id)));

            changes.DeletedDataModelModules.AddRange(originalModules.
                Where(tuple => removedGuids.Contains(tuple.Id)));

            // Compute updated modules. We ignore the parent property of the modules.
            foreach (DataModelModule originalModule in originalModules)
            {
                DataModelModule newModule = targetModules.
                    Where(tuple => tuple.Id == originalModule.Id).FirstOrDefault();

                if (newModule != null)
                {
                    if (originalModule.Description != newModule.Description ||
                        originalModule.NameSpace != newModule.NameSpace ||
                        originalModule.Uri != newModule.Uri)
                        changes.UpdatedDataModelModules.Add(originalModule, newModule);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void CompareResourceTypes(ModuleCollectionChange changes, DataModelModuleCollection originalModules, DataModelModuleCollection targetModules)
        {
            List<ResourceType> sourceResourceTypes = originalModules.
                SelectMany(tuple => tuple.ResourceTypes).ToList();

            List<ResourceType> targetResourceTypes = targetModules.
                SelectMany(tuple => tuple.ResourceTypes).ToList();

            // Compute added resource types.
            List<Guid> addedGuids = new List<Guid>();

            addedGuids.AddRange(targetResourceTypes.Select(tuple => tuple.Id).
                Except(sourceResourceTypes.Select(tuple => tuple.Id)));

            changes.AddedResourceTypes.AddRange(targetResourceTypes.
                Where(tuple => addedGuids.Contains(tuple.Id)));

            // Compute deleted resource types.
            List<Guid> removedGuids = new List<Guid>();

            removedGuids.AddRange(originalModules.SelectMany(tuple => tuple.ResourceTypes).
                Select(tuple => tuple.Id).
                Except
                (targetModules.SelectMany(tuple => tuple.ResourceTypes).
                Select(tuple => tuple.Id)));

            changes.DeletedResourceTypes.AddRange(originalModules.
                SelectMany(tuple => tuple.ResourceTypes).
                Where(tuple => removedGuids.Contains(tuple.Id)));

            // Compute updated resource types. Parent module of the resource type can change.
            // However, the new parent MUST be within the same data model.
            foreach (ResourceType originalResourceType in
                originalModules.SelectMany(tuple => tuple.ResourceTypes))
            {
                ResourceType newResourceType = targetModules.
                    SelectMany(tuple => tuple.ResourceTypes).
                    Where(tuple => tuple.Id == originalResourceType.Id).FirstOrDefault();
                if (newResourceType != null)
                {
                    if (originalResourceType.BaseType == null && newResourceType.BaseType != null ||
                        originalResourceType.BaseType != null && newResourceType.BaseType == null ||
                        originalResourceType.BaseType != null && newResourceType.BaseType != null &&
                        originalResourceType.BaseType.Id != newResourceType.BaseType.Id ||
                        originalResourceType.Description != newResourceType.Description ||
                        originalResourceType.Name != newResourceType.Name ||
                        originalResourceType.Parent.Id != newResourceType.Parent.Id ||
                        originalResourceType.Uri != newResourceType.Uri)
                        changes.UpdatedResourceTypes.Add(originalResourceType, newResourceType);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void CompareScalarProperties(ModuleCollectionChange changes, DataModelModuleCollection originalModules, DataModelModuleCollection targetModules)
        {
            List<ScalarProperty> sourceScalarProperties = originalModules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.ScalarProperties).ToList();

            List<ScalarProperty> targetScalarProperties = targetModules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.ScalarProperties).ToList();

            // Compute added scalar properties.
            List<Guid> addedGuids = new List<Guid>();

            addedGuids.AddRange(targetScalarProperties.Select(tuple => tuple.Id).
                Except(sourceScalarProperties.Select(tuple => tuple.Id)));

            changes.AddedScalarProperties.AddRange(targetScalarProperties.
                Where(tuple => addedGuids.Contains(tuple.Id)));

            // Compute deleted scalar properties.
            List<Guid> removedGuids = new List<Guid>();

            removedGuids.AddRange(originalModules.SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.ScalarProperties).Select(tuple => tuple.Id).
                Except
                (targetModules.SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.ScalarProperties).Select(tuple => tuple.Id)));

            changes.DeletedScalarProperties.AddRange(originalModules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.ScalarProperties).
                Where(tuple => removedGuids.Contains(tuple.Id)));

            // Compute updated scalar properties. Parent for the scalar property can change.
            foreach (ScalarProperty property in originalModules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.ScalarProperties))
            {
                ScalarProperty newProperty = targetModules.
                    SelectMany(tuple => tuple.ResourceTypes).
                    SelectMany(tuple => tuple.ScalarProperties).
                    Where(tuple => tuple.Id == property.Id).FirstOrDefault();
                if (newProperty != null)
                {
                    if (
                        property.DataType != newProperty.DataType ||
                        property.Description != newProperty.Description ||
                        property.MaxLength != newProperty.MaxLength ||
                        property.Name != newProperty.Name ||
                        property.Nullable != newProperty.Nullable ||
                        property.Parent.Id != newProperty.Parent.Id ||
                        property.Precision != newProperty.Precision ||
                        property.Scale != newProperty.Scale ||
                        property.Uri != newProperty.Uri
                    )
                        changes.UpdatedScalarProperties.Add(property, newProperty);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void CompareNavigationProperties(ModuleCollectionChange changes, DataModelModuleCollection originalModules, DataModelModuleCollection targetModules)
        {
            List<NavigationProperty> sourceNavigationProperties = originalModules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties).ToList();

            List<NavigationProperty> targetNavigationProperties = targetModules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties).ToList();

            // Compute added navigation properties.
            List<Guid> addedGuids = new List<Guid>();

            addedGuids.AddRange(targetNavigationProperties.Select(tuple => tuple.Id).
                Except(sourceNavigationProperties.Select(tuple => tuple.Id)));

            changes.AddedNavigationProperties.AddRange(targetNavigationProperties.
                Where(tuple => addedGuids.Contains(tuple.Id)));

            // Compute deleted navigation properties.
            List<Guid> removedGuids = new List<Guid>();

            removedGuids.AddRange(originalModules.SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties).Select(tuple => tuple.Id).
                Except
                (targetModules.SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties).Select(tuple => tuple.Id)));

            changes.DeletedNavigationProperties.AddRange(originalModules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties).
                Where(tuple => removedGuids.Contains(tuple.Id)));

            // Compute updated navigation properties. Parent of the navigation property can change.
            // No need to compare the Association. This will be handled while comparing associations.
            foreach (NavigationProperty property in originalModules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties))
            {
                NavigationProperty newProperty = targetModules.
                    SelectMany(tuple => tuple.ResourceTypes).
                    SelectMany(tuple => tuple.NavigationProperties).
                    Where(tuple => tuple.Id == property.Id).FirstOrDefault();
                if (newProperty != null)
                {
                    if (
                        property.Description != newProperty.Description ||
                        property.Name != newProperty.Name ||
                        property.Parent.Id != newProperty.Parent.Id ||
                        property.Uri != newProperty.Uri
                    )
                        changes.UpdatedNavigationProperties.Add(property, newProperty);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void CompareAssociations(ModuleCollectionChange changes, DataModelModuleCollection originalModules, DataModelModuleCollection targetModules)
        {
            List<Association> sourceAssociations = originalModules.
                SelectMany(tuple => tuple.Associations).ToList();

            List<Association> targetAssociations = targetModules.
                SelectMany(tuple => tuple.Associations).ToList();

            // Compute added associations.
            List<Guid> addedGuids = new List<Guid>();

            addedGuids.AddRange(targetAssociations.Select(tuple => tuple.Id).
                Except(sourceAssociations.Select(tuple => tuple.Id)));

            changes.AddedAssociations.AddRange(targetAssociations.
                Where(tuple => addedGuids.Contains(tuple.Id)));

            // Compute deleted associations.
            List<Guid> removedGuids = new List<Guid>();

            removedGuids.AddRange(originalModules.SelectMany(tuple => tuple.Associations).
                Select(tuple => tuple.Id).
                Except
                (targetModules.SelectMany(tuple => tuple.Associations).
                Select(tuple => tuple.Id)));

            changes.DeletedAssociations.AddRange(originalModules.
                SelectMany(tuple => tuple.Associations).
                Where(tuple => removedGuids.Contains(tuple.Id)));

            // Compute updated associations. 
            foreach (Association originalAssociation in
                originalModules.SelectMany(tuple => tuple.Associations))
            {
                Association newAssociation = targetModules.
                    SelectMany(tuple => tuple.Associations).
                    Where(tuple => tuple.Id == originalAssociation.Id).FirstOrDefault();
                if (newAssociation != null)
                {
                    if (originalAssociation.Name != newAssociation.Name ||
                        originalAssociation.ObjectMultiplicity !=
                        newAssociation.ObjectMultiplicity ||
                        originalAssociation.ObjectNavigationProperty.Id !=
                        newAssociation.ObjectNavigationProperty.Id ||
                        originalAssociation.Parent.Id != newAssociation.Parent.Id ||
                        originalAssociation.SubjectMultiplicity !=
                        newAssociation.SubjectMultiplicity ||
                        originalAssociation.SubjectNavigationProperty.Id !=
                        newAssociation.SubjectNavigationProperty.Id ||
                        originalAssociation.Uri != newAssociation.Uri)
                        changes.UpdatedAssociations.Add(originalAssociation, newAssociation);
                }
            }
        }
    }
}
