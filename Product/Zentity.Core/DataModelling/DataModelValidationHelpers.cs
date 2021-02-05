// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Defines a partial class with validation helper functions for the DataModel class.
    /// </summary>
    public sealed partial class DataModel
    {
        /// <summary>
        /// Delegate to find the duplicates in DataModel
        /// </summary>
        private delegate void DuplicateFinder(object item, Guid id);

        /// <summary>
        /// Validates this instance.
        /// </summary>
        private void Validate()
        {
            // Validate ResourceTypes across all data model modules for duplicate names
            ValidateDuplicateResourceTypes();

            // Validate unique Ids before anything. This is necessary for error 
            // messages that show these Ids. Otherwise, there could be items with Id
            // same as that of the rogue data model item and the error messages become
            // ambiguous.
            ValidateUniqueForwardPathsAndIds();

            // Each data model item should be traceable back to this data model.
            ValidateReversePathsAndAssociations();

            // Verify that there is no cyclic dependency between modules.
            try
            {
                SortModules(this.Modules.ToList());
            }
            catch (ZentityException)
            {
                throw new ModelItemValidationException(
                    DataModellingResources.ExceptionCycleInDependencyGraph);
            }

            // Verify that there is no cycle in the inheritance hierarchy between the the 
            // resource types of a module.
            foreach (DataModelModule module in this.Modules)
            {
                try
                {
                    SortByHierarchy(module.ResourceTypes.ToList());
                }
                catch (ZentityException)
                {
                    throw new ModelItemValidationException(
                        DataModellingResources.InvalidResourceTypesCyclicInheritance);
                }
            }

            // Perform data model item specific validations. For example, duplicate names in
            // collections, maximum length of identifiers etc.
            try
            {
                this.Modules.Validate();
            }
            catch (ZentityException ex)
            {
                throw new ModelItemValidationException(
                    DataModellingResources.ValidationExceptionSeeInnerException,
                    ex);
            }

            // Validate that the module NameSpace, resource type Name etc are valid C# identifiers.
            ValidateCSharpIdentifiers();
        }

        /// <summary>
        /// Validates the DataModel by checking for duplicate resource types.
        /// </summary>
        private void ValidateDuplicateResourceTypes()
        {
            // Fetch all the ResourceType names from all DataModels
            var resourceTypeNameList = this.Modules.SelectMany(dataModelModule => dataModelModule.ResourceTypes, (dataModelModule, resourceTypes) => resourceTypes.Name);

            // Find the duplicate ResourceType names
            var duplicateResourceTypeNames = resourceTypeNameList.GroupBy(resourceTypeName => resourceTypeName).Where(resTypeGroup => resTypeGroup.Count() > 1).Select(resTypeGroup => resTypeGroup.Key);

            // If there are duplicate ResourceType names, then throw a ZentityException with the list of duplicate ResourceType names
            if (duplicateResourceTypeNames.Count() > 0)
            {
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionDuplicateResourceTypeName, 
                    string.Join<string>(DataModellingResources.CommaWithTrailingSpace, duplicateResourceTypeNames)));
            }
        }

        /// <summary>
        /// Validates the C# sharp identifiers for inconsistencies.
        /// </summary>
        private void ValidateCSharpIdentifiers()
        {
            // TODO: Provide a more foolproof algorithm. It is possible to inject incorrect
            // values here. For example, property name could be 'asdf; string qqq'.
            StringBuilder cSharpCode = new StringBuilder();
            foreach (DataModelModule module in this.Modules)
            {
                StringBuilder classStatements = new StringBuilder();

                foreach (ResourceType resourceType in module.ResourceTypes)
                {
                    StringBuilder propertyStatements = new StringBuilder();

                    foreach (ScalarProperty scalarProperty in resourceType.ScalarProperties)
                        propertyStatements.Append(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.CSharpPropertyFormat, scalarProperty.Name));

                    foreach (NavigationProperty navigationProperty in
                        resourceType.NavigationProperties)
                        propertyStatements.Append(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.CSharpPropertyFormat, navigationProperty.Name));

                    classStatements.Append(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.CSharpClassFormat, resourceType.Name,
                        propertyStatements.ToString()));
                }

                foreach (Association association in module.Associations)
                    classStatements.Append(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.CSharpClassFormat, association.Name,
                        string.Empty));

                cSharpCode.Append(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CSharpNamespaceFormat, module.NameSpace,
                    classStatements.ToString()));
            }

            using (CSharpCodeProvider codeProvider = new CSharpCodeProvider())
            {
                CompilerResults results = codeProvider.CompileAssemblyFromSource(
                    new CompilerParameters(), cSharpCode.ToString());
                if (results.Errors.Count > 0)
                {
                    StringBuilder errorText = new StringBuilder();
                    foreach (CompilerError error in results.Errors)
                        errorText.Append(error.ErrorText);

                    throw new ModelItemValidationException(string.Format(
                        CultureInfo.InvariantCulture,
                        DataModellingResources.ValidationExceptionInvalidCSharpIdentifier,
                        errorText.ToString()));
                }
            }
        }

        /// <summary>
        /// Validates the module changes and prevents changes to Microsoft shipped data models.
        /// </summary>
        /// <param name="originalModel">The original model.</param>
        /// <param name="newModel">The new model.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void ValidateMsShippedModuleChanges(DataModel originalModel, DataModel newModel)
        {
            // Verify that the MsShipped modules are not changed.
            ModuleCollectionChange changes = ComputeDifferences(originalModel, newModel);

            // MsShipped modules cannot be added. The constructor of DataModelModule ensures that
            // this flag is turned off for custom modules.

            // Detect deleted MsShipped module.
            var deletedModule = changes.DeletedDataModelModules.
                Where(tuple => tuple.IsMsShipped).FirstOrDefault();
            if (deletedModule != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedModuleDeleted,
                    deletedModule.NameSpace));

            var updatedModule = changes.UpdatedDataModelModules.
                Where(tuple => tuple.Key.IsMsShipped).FirstOrDefault();
            if (updatedModule.Key != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedModuleUpdated,
                    updatedModule.Key.NameSpace));

            var addedResourceType = changes.AddedResourceTypes.
                Where(tuple => tuple.Parent.IsMsShipped).FirstOrDefault();
            if (addedResourceType != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedResourceTypeAdded,
                    addedResourceType.FullName));

            var deletedResourceType = changes.DeletedResourceTypes.
                Where(tuple => tuple.Parent.IsMsShipped).FirstOrDefault();
            if (deletedResourceType != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedResourceTypeDeleted,
                    deletedResourceType.FullName));

            var updatedResourceType = changes.UpdatedResourceTypes.
                Where(tuple => tuple.Key.Parent.IsMsShipped).FirstOrDefault();
            if (updatedResourceType.Key != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedResourceTypeUpdated,
                    updatedResourceType.Key.FullName));

            var addedScalarProperty = changes.AddedScalarProperties.
                Where(tuple => tuple.Parent.Parent.IsMsShipped).FirstOrDefault();
            if (addedScalarProperty != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedScalarPropertyAdded,
                    addedScalarProperty.FullName));

            var deletedScalarProperty = changes.DeletedScalarProperties.
                Where(tuple => tuple.Parent.Parent.IsMsShipped).FirstOrDefault();
            if (deletedScalarProperty != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedScalarPropertyDeleted,
                    deletedScalarProperty.FullName));

            var updatedScalarProperty = changes.UpdatedScalarProperties.
                Where(tuple => tuple.Key.Parent.Parent.IsMsShipped).FirstOrDefault();
            if (updatedScalarProperty.Key != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedScalarPropertyUpdated,
                    updatedScalarProperty.Key.FullName));

            var addedNavigationProperty = changes.AddedNavigationProperties.
                Where(tuple => tuple.Parent.Parent.IsMsShipped).FirstOrDefault();
            if (addedNavigationProperty != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedNavigationPropertyAdded,
                    addedNavigationProperty.FullName));

            var deletedNavigationProperty = changes.DeletedNavigationProperties.
                Where(tuple => tuple.Parent.Parent.IsMsShipped).FirstOrDefault();
            if (deletedNavigationProperty != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedNavigationPropertyDeleted,
                    deletedNavigationProperty.FullName));

            var updatedNavigationProperty = changes.UpdatedNavigationProperties.
                Where(tuple => tuple.Key.Parent.Parent.IsMsShipped).FirstOrDefault();
            if (updatedNavigationProperty.Key != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedNavigationPropertyUpdated,
                    updatedNavigationProperty.Key.FullName));

            // It is necessary to also check for association changes. NavigationProperty changes
            // are not sufficient to ensure this. For example,
            // Association assoc1 = GetWellKnownAssociation();
            // NavigationProperty subjectProperty = assoc1.SubjectNavigationProperty;
            // assoc1.SubjectNavigationProperty = assoc1.ObjectNavigationProperty;
            // assoc1.ObjectNavigationProperty = subjectProperty;
            var addedAssociation = changes.AddedAssociations.
                Where(tuple => tuple.Parent.IsMsShipped).FirstOrDefault();
            if (addedAssociation != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedAssociationAdded,
                    addedAssociation.FullName));

            var deletedAssociation = changes.DeletedAssociations.
                Where(tuple => tuple.Parent.IsMsShipped).FirstOrDefault();
            if (deletedAssociation != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedAssociationDeleted,
                    deletedAssociation.FullName));

            var updatedAssociation = changes.UpdatedAssociations.
                Where(tuple => tuple.Key.Parent.IsMsShipped).FirstOrDefault();
            if (updatedAssociation.Key != null)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionMsShippedAssociationUpdated,
                    updatedAssociation.Key.FullName));
        }

        /// <summary>
        /// Validates the DataModel for reverse paths and associations.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void ValidateReversePathsAndAssociations()
        {
            // Since the error messages in this method use item Id, it is necessary that the data
            // model is already verified for uniqueness of Id values. The method ensures that the 
            // model items are not shared across models. For example,
            // Core model1 = new ZentityContext().Core;
            // Core model2 = new ZentityContext().Core;
            // model1.Modules.Add(model2.Modules[0]);
            // Here all the model2 items can be reached from the root reference and are touched
            // only once. But when we try to trace back from any item, we will reach model1
            // reference! This might cause incorrect behaviors.
            //
            // This method also validates the correctness of data model associations.

            List<ResourceType> modelResourceTypes = new List<ResourceType>();
            modelResourceTypes.AddRange(this.Modules.SelectMany(tuple => tuple.ResourceTypes));

            foreach (DataModelModule module in this.Modules)
            {
                List<NavigationProperty> moduleNavigationProperties =
                    new List<NavigationProperty>();

                if (module.Parent != this)
                    throw new ModelItemValidationException(string.Format(
                        CultureInfo.InvariantCulture,
                        DataModellingResources.ValidationExceptionCannotTraceItem,
                        DataModellingResources.DataModelModule, module.Id,
                        DataModellingResources.Namespace,
                        module.NameSpace == null ? string.Empty : module.NameSpace));

                foreach (ResourceType resourceType in module.ResourceTypes)
                {
                    if (resourceType.Parent != module)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionCannotTraceItem,
                            DataModellingResources.ResourceType, resourceType.Id,
                            DataModellingResources.Name,
                            resourceType.Name == null ? string.Empty : resourceType.Name));

                    // BaseType cannot be null.
                    if (resourceType.BaseType == null &&
                        resourceType.FullName != DataModellingResources.ZentityCoreResource)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionNullBaseType,
                            resourceType.Id,
                            resourceType.Name == null ? string.Empty : resourceType.Name));

                    // The base type must be one of those in the same model.
                    if (resourceType.BaseType != null &&
                        !modelResourceTypes.Contains(resourceType.BaseType))
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionBaseTypeNotInModel,
                            resourceType.Id,
                            resourceType.Name == null ? string.Empty : resourceType.Name));

                    foreach (ScalarProperty scalarProperty in resourceType.ScalarProperties)
                        if (scalarProperty.Parent != resourceType)
                            throw new ModelItemValidationException(string.Format(
                                CultureInfo.InvariantCulture,
                                DataModellingResources.ValidationExceptionCannotTraceItem,
                                DataModellingResources.ScalarProperty, scalarProperty.Id,
                                DataModellingResources.Name,
                                scalarProperty.Name == null ? string.Empty : scalarProperty.Name));

                    foreach (NavigationProperty navigationProperty in
                        resourceType.NavigationProperties)
                    {
                        if (navigationProperty.Parent != resourceType)
                            throw new ModelItemValidationException(string.Format(
                                CultureInfo.InvariantCulture,
                                DataModellingResources.ValidationExceptionCannotTraceItem,
                                DataModellingResources.NavigationProperty, navigationProperty.Id,
                                DataModellingResources.Name, navigationProperty.Name == null ?
                                string.Empty : navigationProperty.Name));

                        moduleNavigationProperties.Add(navigationProperty);
                    }
                } // End of ResourceType processing.

                // Validate module associations.
                foreach (NavigationProperty navigationProperty in moduleNavigationProperties)
                {
                    // Each navigation property must have an association.
                    if (navigationProperty.Association == null)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionAssociationNotFound,
                            navigationProperty.Id, navigationProperty.Name == null ?
                            string.Empty : navigationProperty.Name));

                    Association association = navigationProperty.Association;

                    // Association subject navigation property cannot be a null reference.
                    if (association.SubjectNavigationProperty == null)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionNullNavigationProperty,
                            DataModellingResources.SubjectNavigationProperty, association.Id,
                            association.Name == null ? string.Empty : association.Name));

                    // Association object navigation property cannot be a null reference.
                    if (association.ObjectNavigationProperty == null)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionNullNavigationProperty,
                            DataModellingResources.ObjectNavigationProperty, association.Id,
                            association.Name == null ? string.Empty : association.Name));

                    // Direction should be defined and should be in sync.
                    if (navigationProperty.Direction == AssociationEndType.Undefined)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionDirectionNotDefined,
                            navigationProperty.Id, navigationProperty.Name == null ?
                            string.Empty : navigationProperty.Name));

                    if (navigationProperty.Direction == AssociationEndType.Subject &&
                        association.SubjectNavigationProperty != navigationProperty)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionDirectionOutOfSync,
                            navigationProperty.Id, navigationProperty.Name == null ?
                            string.Empty : navigationProperty.Name, DataModellingResources.Subject,
                            association.Id, association.Name == null ? string.Empty :
                            association.Name, DataModellingResources.SubjectNavigationProperty));

                    if (navigationProperty.Direction == AssociationEndType.Object &&
                        association.ObjectNavigationProperty != navigationProperty)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionDirectionOutOfSync,
                            navigationProperty.Id, navigationProperty.Name == null ?
                            string.Empty : navigationProperty.Name, DataModellingResources.Object,
                            association.Id, association.Name == null ? string.Empty :
                            association.Name, DataModellingResources.ObjectNavigationProperty));

                    // Validate the other end of the association. The navigation property at 
                    // the other end must be one of those in the same module. We have already
                    // verified that each navigation property of this module points correctly
                    // back to its parent.
                    NavigationProperty otherEndProperty =
                        navigationProperty.Direction == AssociationEndType.Subject ?
                        association.ObjectNavigationProperty :
                        association.SubjectNavigationProperty;

                    if (!moduleNavigationProperties.Contains(otherEndProperty))
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.ValidationExceptionOtherEndNotInModel,
                            association.Id, association.Name == null ? string.Empty :
                            association.Name, otherEndProperty.Id,
                            otherEndProperty.Name == null ? string.Empty : otherEndProperty.Name));
                } // End of module associations processing.
            } // End of DataModelModule processing.
        }

        /// <summary>
        /// Validates the DataModel for unique forward paths and ids.
        /// </summary>
        private void ValidateUniqueForwardPathsAndIds()
        {
            // Traverse the model in breadth first order and verify that any Id is not hit
            // twice, except for associations. This method also ensures that there is a unique 
            // path from the Core reference to its items. For example,
            // ScalarProperty prop = GetProperty();
            // type1.ScalarProperties.Add(prop);
            // type2.ScalarProperties.Add(prop);
            // Although the Parent property prop is set to type2, it can be reached from both 
            // type1.ScalarProperties and type2.ScalarProperties. This is incorrect. We do not
            // allow sharing of scalar properties between resource types. This case will be
            // detected when we try to add the Id for prop again in the visited dictionary.
            //
            // For associations, we do not ensure unique paths. We just make sure that the Id
            // of the association is not in conflict with the Ids of other items.
            Dictionary<object, Guid> visited = new Dictionary<object, Guid>();
            DuplicateFinder d = delegate(object item, Guid id)
            {
                if (visited.Values.Contains(id))
                    throw new ModelItemValidationException(string.Format(
                        CultureInfo.InvariantCulture,
                        DataModellingResources.ValidationExceptionDuplicateId, id));

                visited.Add(item, id);
            };

            foreach (DataModelModule module in this.Modules)
            {
                d(module, module.Id);
                foreach (ResourceType resourceType in module.ResourceTypes)
                {
                    d(resourceType, resourceType.Id);
                    foreach (ScalarProperty scalarProperty in resourceType.ScalarProperties)
                        d(scalarProperty, scalarProperty.Id);
                    foreach (NavigationProperty navigationProperty in
                        resourceType.NavigationProperties)
                    {
                        d(navigationProperty, navigationProperty.Id);

                        if (navigationProperty.Association != null)
                        {
                            Association association = navigationProperty.Association;
                            var visitedRecordsForAssociation =
                                visited.Where(tuple => tuple.Value == association.Id);
                            if (visitedRecordsForAssociation.Count() > 0)
                            {
                                // Id found. Verify that it is the same association.
                                var visitedRecord = visitedRecordsForAssociation.First();
                                if (visitedRecord.Key != association)
                                    throw new ModelItemValidationException(string.Format(
                                        CultureInfo.InvariantCulture,
                                        DataModellingResources.ValidationExceptionDuplicateId,
                                        association.Id));
                            }
                        }
                    }
                }
            }
        }
    }
}
