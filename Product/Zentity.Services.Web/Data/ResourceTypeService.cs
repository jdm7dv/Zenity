// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Data
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Security.Permissions;
    using System.ServiceModel;
    using Zentity.Core;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Resource Type Service
    /// </summary>
    public class ResourceTypeService : IResourceTypeService
    {
        #region IResourceTypeService Methods

        /// <summary>
        /// Lists all resource types available within a particular data model aka model namespace.
        /// </summary>
        /// <param name="modelNameSpace">Namespace filter</param>
        /// <returns>Collection of ResourceTypes</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public ResourceTypeCollection GetAllResourceTypesByNamespace(string modelNameSpace)
        {
            #region Validating Parameters

            if (string.IsNullOrEmpty(modelNameSpace) || string.IsNullOrWhiteSpace(modelNameSpace))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullModelNamespaceMessage));
                //throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.ModelConfigurationParameter), new FaultReason(Properties.Messages.NullModelNamespaceMessage));
            }

            #endregion

            DataModel dataModel;
            DataModelModule dataModelModule;
            ResourceTypeCollection resourceTypes;
            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        dataModelModule = dataModel.Modules[modelNameSpace];
                        if (dataModelModule != null)
                        {
                            resourceTypes = dataModelModule.ResourceTypes;
                            if (resourceTypes == null || resourceTypes.Count == 0)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.NullDataModelFaultReason));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNameSpace)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                        }
                    }
                    else
                    {
                        throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
                //throw new FaultException<Exception>(exception, new FaultReason(Properties.Messages.ExceptionFaultReason));
            }

            return resourceTypes;
        }

        /// <summary>
        /// Get count of resources for a resource type.
        /// </summary>
        /// <param name="modelNamespace">Namespace filter</param>
        /// <param name="resourceTypeName">Resource Type Name</param>
        /// <returns>Count of Resource</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public int GetResourceCountForResourceType(string modelNamespace, string resourceTypeName)
        {
            #region Validating Parameters
            if (string.IsNullOrWhiteSpace(modelNamespace) || string.IsNullOrWhiteSpace(resourceTypeName))
            {
                throw new FaultException(new FaultReason(Properties.Messages.ArgumentsNullOrEmpty));
            }

            DataModel dataModel;
            DataModelModule dataModelModule;
            ResourceTypeCollection resourceTypes;
            try
            {
                using (ZentityContext zentityContext = Utilities.CreateZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        dataModelModule = dataModel.Modules[modelNamespace];
                        if (dataModelModule != null)
                        {
                            resourceTypes = dataModelModule.ResourceTypes;
                            if (resourceTypes != null || resourceTypes.Count > 0)
                            {
                                if (resourceTypes[resourceTypeName] == null)
                                {
                                    throw new FaultException(new FaultReason(string.Format(Properties.Messages.InvalidResourceType, resourceTypeName)));
                                }
                            }
                            else
                            {
                                throw new FaultException(new FaultReason(string.Format(Properties.Messages.EmptyResourceFiles, resourceTypeName)));
                            }
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(string.Format("{0}: {1}", Properties.Messages.NullDataModelFaultReason, modelNamespace)));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNameSpace)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                        }
                    }
                    else
                    {
                        throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
                //throw new FaultException<Exception>(exception, new FaultReason(Properties.Messages.ExceptionFaultReason));
            }
            #endregion

            // Fetch the resource type item count from Zentity Database
            IDictionary<string, int> resourceItemCount;
            using (ZentityContext context = Utilities.CreateZentityContext())
            {
                int totalItemCount;
                resourceItemCount = DataModel.GetResourceItemCount(context, new[] { modelNamespace }, out totalItemCount);
            }

            if (resourceItemCount != null && resourceItemCount.Count > 0)
            {
                string resourceTypeFullName = string.Format(Properties.Resources.ResourceTypeFullNameFormat, modelNamespace, resourceTypeName);
                return resourceItemCount.Where(itemCountRow => itemCountRow.Key.Equals(resourceTypeFullName, StringComparison.OrdinalIgnoreCase)).Select(itemCountRow => itemCountRow.Value).FirstOrDefault();
            }

            return 0;
        }

        /// <summary>
        /// Gets the resource count for data model.
        /// </summary>
        /// <param name="dataModelNamesapceList">The data model namesapce list.</param>
        /// <param name="totalUniqueItemCount">The total unique item count.</param>
        /// <returns>
        /// Dictionary of ResouceType names and the item count
        /// </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public IDictionary<string, int> GetResourceCountForDataModel(IEnumerable<string> dataModelNamesapceList, out int totalUniqueItemCount)
        {
            if (dataModelNamesapceList == null)
            {
                throw new ArgumentException("dataModelNamesapceList");
            }

            DataModel dataModel;
            DataModelModule dataModelModule;
            try
            {
                using (ZentityContext zentityContext = Utilities.CreateZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        List<string> invalidDataModelNamespaceList = new List<string>();
                        foreach (string modelNamespace in dataModelNamesapceList)
                        {
                            dataModelModule = dataModel.Modules[modelNamespace];
                            if (dataModelModule == null)
                            {
                                invalidDataModelNamespaceList.Add(modelNamespace);
                            }
                        }

                        if (invalidDataModelNamespaceList.Count > 0)
                        {
                            throw new FaultException(new FaultReason(string.Format(Properties.Messages.NamespacesNotFound, string.Join(",", invalidDataModelNamespaceList))));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNameSpace)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                        }
                    }
                    else
                    {
                        throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
                //throw new FaultException<Exception>(exception, new FaultReason(Properties.Messages.ExceptionFaultReason));
            }

            // Fetch the resource type item count from Zentity Database
            IDictionary<string, int> resourceItemCount;
            using (ZentityContext context = Utilities.CreateZentityContext())
            {
                resourceItemCount = DataModel.GetResourceItemCount(context, dataModelNamesapceList, out totalUniqueItemCount);
            }

            return resourceItemCount;
        }

        /// <summary>
        /// Will update an existing data model resource type by adding a scalar property.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="dataType">Type of the property to be added</param>
        /// <param name="maxLength">Max length for a string or binary property</param>
        /// <param name="precision">Precision of double values</param>
        /// <param name="scale">Scale for double values</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void AddScalarPropertyToResourceType(string modelNamespace, string resourceType, string propertyName, Zentity.Core.DataTypes dataType, int? maxLength, int? precision, int? scale)
        {
            #region Validating Parameters

            if (string.IsNullOrEmpty(modelNamespace) || string.IsNullOrWhiteSpace(modelNamespace) ||
                string.IsNullOrEmpty(resourceType) || string.IsNullOrWhiteSpace(resourceType) ||
                string.IsNullOrEmpty(propertyName) || string.IsNullOrWhiteSpace(propertyName))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
                //throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.NullEmptyTitle), new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            switch (dataType)
            {
                case DataTypes.String:
                    if (maxLength == 0 || maxLength < -1 || maxLength > 4000)
                    {
                        maxLength = -1;
                    }
                    break;
                case DataTypes.Binary:
                    if (maxLength == 0 || maxLength < -1 || maxLength > 8000)
                    {
                        maxLength = -1;
                    }
                    break;
                case DataTypes.Decimal:
                    if (precision <= 0 || precision > 38 || precision < scale)
                    {
                        precision = 2;
                    }
                    break;
            }

            #endregion

            DataModel dataModel;
            DataModelModule dataModelModule;
            ResourceTypeCollection resourceTypes;

            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        dataModelModule = dataModel.Modules[modelNamespace];
                        if (dataModelModule != null)
                        {
                            resourceTypes = dataModelModule.ResourceTypes;
                            if (resourceTypes != null && resourceTypes.Count > 0)
                            {
                                ResourceType currentResourceType = resourceTypes[resourceType];
                                if (currentResourceType != null)
                                {
                                    ScalarProperty scalarProperty = new ScalarProperty
                                                                        {
                                                                            Name = propertyName,
                                                                            DataType = dataType,
                                                                        };
                                    if (maxLength.HasValue)
                                    {
                                        scalarProperty.MaxLength = maxLength.Value;
                                    }
                                    if (precision.HasValue)
                                    {
                                        scalarProperty.Precision = precision.Value;
                                    }
                                    if (scale.HasValue)
                                    {
                                        scalarProperty.Scale = scale.Value;
                                    }

                                    if (currentResourceType.ScalarProperties == null || currentResourceType.ScalarProperties.Count == 0)
                                    {
                                        currentResourceType.ScalarProperties.Add(scalarProperty);
                                    }
                                    else
                                    {
                                        ScalarProperty scalarPropertyExists = currentResourceType.ScalarProperties.FirstOrDefault(s => s.Name.Equals(propertyName));
                                        if (scalarPropertyExists == null)
                                        {
                                            currentResourceType.ScalarProperties.Add(scalarProperty);
                                        }
                                        else
                                        {
                                            throw new FaultException(new FaultReason(Properties.Messages.DuplicateScalarPropertyFaultReason));
                                            //throw new FaultException<DuplicateNameException>(new DuplicateNameException(Properties.Messages.ScalarPropertyException), new FaultReason(Properties.Messages.DuplicateScalarPropertyFaultReason));
                                        }
                                    }
                                }
                                else
                                {
                                    throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                    //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.ResourceTypeFormat, resourceType)), new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                }
                            }
                            else
                            {
                                throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                                //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(), new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                            }
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNamespace)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                        }

                        dataModel.Synchronize();
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
            }
        }

        /// <summary>
        /// Will update an existing data model resource type by deleting a scalar property.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <param name="propertyName">Name of the property to be deleted</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void DeleteScalarPropertyOfResourceType(string modelNamespace, string resourceType, string propertyName)
        {
            #region Validating Parameters

            if (string.IsNullOrEmpty(modelNamespace) || string.IsNullOrWhiteSpace(modelNamespace) ||
                string.IsNullOrEmpty(resourceType) || string.IsNullOrWhiteSpace(resourceType) ||
                string.IsNullOrEmpty(propertyName) || string.IsNullOrWhiteSpace(propertyName))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
                //throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.NullEmptyTitle), new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            #endregion

            DataModel dataModel;
            DataModelModule dataModelModule;
            ResourceTypeCollection resourceTypes = null;

            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        dataModelModule = dataModel.Modules[modelNamespace];
                        if (dataModelModule != null)
                        {
                            resourceTypes = dataModelModule.ResourceTypes;
                            if (resourceTypes != null && resourceTypes.Count > 0)
                            {
                                ResourceType currentResourceType = resourceTypes[resourceType];
                                if (currentResourceType != null)
                                {
                                    if (currentResourceType.ScalarProperties == null || currentResourceType.ScalarProperties.Count == 0)
                                    {
                                        throw new FaultException(new FaultReason(Properties.Messages.NullScalarPropertyFaultReason));
                                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ScalarPropertyException), new FaultReason(Properties.Messages.NullScalarPropertyFaultReason));
                                    }
                                    else
                                    {
                                        ScalarProperty scalarPropertyExists = currentResourceType.ScalarProperties.FirstOrDefault(s => s.Name.Equals(propertyName));
                                        if (scalarPropertyExists != null)
                                        {
                                            currentResourceType.ScalarProperties.Remove(scalarPropertyExists);
                                        }
                                        else
                                        {
                                            throw new FaultException(new FaultReason(Properties.Messages.NotFoundScalarPropertyFaultReason));
                                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(), new FaultReason(Properties.Messages.NotFoundScalarPropertyFaultReason));
                                        }
                                    }
                                }
                                else
                                {
                                    throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                    //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.ResourceTypeFormat, resourceType)), new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                }
                            }
                            else
                            {
                                throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                                //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(), new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                            }
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNamespace)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                        }

                        dataModel.Synchronize();
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
            }
        }

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
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void AddNavigationPropertyToResourceType(string modelNamespace, string subjectResourceType, string objectResourceType,
                                                        string subjectNavigationPropertyName, string objectNavigationPropertyName, string associationName,
                                                        Core.AssociationEndMultiplicity subjectMultiplicity, Core.AssociationEndMultiplicity objectMultiplicity)
        {
            #region Validating Parameters

            if (string.IsNullOrEmpty(modelNamespace) || string.IsNullOrWhiteSpace(modelNamespace) ||
                string.IsNullOrEmpty(subjectResourceType) || string.IsNullOrWhiteSpace(subjectResourceType) ||
                string.IsNullOrEmpty(objectResourceType) || string.IsNullOrWhiteSpace(objectResourceType) ||
                string.IsNullOrEmpty(subjectNavigationPropertyName) || string.IsNullOrWhiteSpace(subjectNavigationPropertyName) ||
                string.IsNullOrEmpty(objectNavigationPropertyName) || string.IsNullOrWhiteSpace(objectNavigationPropertyName) ||
                string.IsNullOrEmpty(associationName) || string.IsNullOrWhiteSpace(associationName))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
                //throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.NullEmptyTitle), new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            #endregion

            DataModel dataModel;
            DataModelModule dataModelModule;
            ResourceTypeCollection resourceTypes = null;

            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        dataModelModule = dataModel.Modules[modelNamespace];
                        if (dataModelModule != null)
                        {
                            resourceTypes = dataModelModule.ResourceTypes;
                            if (resourceTypes != null && resourceTypes.Count > 0)
                            {
                                ResourceType subjectNavigationResourceType = resourceTypes[subjectResourceType];
                                ResourceType objectNavigationResourceType = resourceTypes[objectResourceType];
                                if (subjectNavigationResourceType != null && objectNavigationResourceType != null)
                                {
                                    #region Create Navigation Property

                                    NavigationProperty subjectNavigationProperty = new NavigationProperty { Name = subjectNavigationPropertyName };
                                    NavigationProperty objectNavigationProperty = new NavigationProperty { Name = objectNavigationPropertyName };

                                    #endregion

                                    if (subjectNavigationProperty != null && objectNavigationProperty != null)
                                    {
                                        if (subjectNavigationResourceType.NavigationProperties == null && subjectNavigationResourceType.NavigationProperties.Count == 0)
                                        {
                                            subjectNavigationResourceType.NavigationProperties.Add(subjectNavigationProperty);
                                        }
                                        else
                                        {
                                            NavigationProperty subjectNavigationPropertyExists = subjectNavigationResourceType.NavigationProperties.FirstOrDefault(n => n.Name.Equals(subjectNavigationPropertyName));
                                            if (subjectNavigationPropertyExists == null)
                                            {
                                                subjectNavigationResourceType.NavigationProperties.Add(subjectNavigationProperty);
                                            }
                                            else
                                            {
                                                throw new FaultException(new FaultReason(Properties.Messages.NavigationPropertyFaultReason));
                                                //throw new FaultException<DuplicateNameException>(new DuplicateNameException(Properties.Messages.NavigationPropertyException), new FaultReason(Properties.Messages.NavigationPropertyFaultReason));
                                            }
                                        }

                                        if (objectNavigationResourceType.NavigationProperties == null && objectNavigationResourceType.NavigationProperties.Count == 0)
                                        {
                                            objectNavigationResourceType.NavigationProperties.Add(objectNavigationProperty);
                                        }
                                        else
                                        {
                                            NavigationProperty objectNavigationPropertyExists = objectNavigationResourceType.NavigationProperties.FirstOrDefault(n => n.Name.Equals(objectNavigationPropertyName));
                                            if (objectNavigationPropertyExists == null)
                                            {
                                                objectNavigationResourceType.NavigationProperties.Add(objectNavigationProperty);
                                            }
                                            else
                                            {
                                                throw new FaultException(new FaultReason(Properties.Messages.NavigationPropertyFaultReason));
                                                //throw new FaultException<DuplicateNameException>(new DuplicateNameException(Properties.Messages.NavigationPropertyException), new FaultReason(Properties.Messages.NavigationPropertyFaultReason));
                                            }
                                        }

                                        Association associationExists = dataModelModule.Associations.FirstOrDefault(a => a.Name.Equals(associationName));
                                        if (associationExists == null)
                                        {
                                            Association navigationPropertyAssociation = new Association
                                            {
                                                Name = associationName,
                                                SubjectNavigationProperty = subjectNavigationProperty,
                                                ObjectNavigationProperty = objectNavigationProperty,
                                                SubjectMultiplicity = subjectMultiplicity,
                                                ObjectMultiplicity = objectMultiplicity
                                            };
                                        }
                                        else
                                        {
                                            throw new FaultException(new FaultReason(Properties.Messages.AssociationFaultReason));
                                            //throw new FaultException<DuplicateNameException>(new DuplicateNameException(Properties.Messages.AssociationException), new FaultReason(Properties.Messages.AssociationFaultReason));
                                        }
                                    }
                                    else
                                    {
                                        throw new FaultException(new FaultReason(Properties.Messages.NullNavigationPropertyFaultReason));
                                        //throw new FaultException<NullReferenceException>(new NullReferenceException(Properties.Messages.NavigationPropertyException), new FaultReason(Properties.Messages.NullNavigationPropertyFaultReason));
                                    }
                                }
                                else
                                {
                                    throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                    //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.SubjectObjectResourceTypes, subjectResourceType, objectResourceType)), new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                }
                            }
                            else
                            {
                                throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                                //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(), new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                            }
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNamespace)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                        }

                        dataModel.Synchronize();
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
            }
        }

        /// <summary>
        /// Will update an existing data model resource type by deleting a navigation property.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <param name="propertyName">Name of the Navigation property to be deleted</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void DeleteNavigationPropertyOfResourceType(string modelNamespace, string resourceType, string propertyName)
        {
            #region Validating Parameters

            if (string.IsNullOrEmpty(modelNamespace) || string.IsNullOrWhiteSpace(modelNamespace) ||
                string.IsNullOrEmpty(resourceType) || string.IsNullOrWhiteSpace(resourceType) ||
                string.IsNullOrEmpty(propertyName) || string.IsNullOrWhiteSpace(propertyName))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
                //throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.NullEmptyTitle), new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            #endregion

            DataModel dataModel;
            DataModelModule dataModelModule;
            ResourceTypeCollection resourceTypes = null;

            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        dataModelModule = dataModel.Modules[modelNamespace];
                        if (dataModelModule != null)
                        {
                            resourceTypes = dataModelModule.ResourceTypes;
                            if (resourceTypes != null && resourceTypes.Count > 0)
                            {
                                ResourceType currentResourceType = resourceTypes[resourceType];
                                if (currentResourceType != null)
                                {
                                    if (currentResourceType.NavigationProperties == null || currentResourceType.NavigationProperties.Count == 0)
                                    {
                                        throw new FaultException(new FaultReason(Properties.Messages.NotFoundNavigationPropertyFaultReason));
                                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.NavigationPropertyException), new FaultReason(Properties.Messages.NotFoundNavigationPropertyFaultReason));
                                    }
                                    else
                                    {
                                        NavigationProperty navigationPropertyExists = currentResourceType.NavigationProperties.FirstOrDefault(n => n.Name.Equals(propertyName));
                                        if (navigationPropertyExists != null)
                                        {
                                            Association a = navigationPropertyExists.Association;
                                            switch (navigationPropertyExists.Direction)
                                            {
                                                case AssociationEndType.Subject:
                                                    NavigationProperty o = a.ObjectNavigationProperty;
                                                    ResourceType ort = o.Parent;
                                                    ort.NavigationProperties.Remove(o);
                                                    break;
                                                case AssociationEndType.Object:
                                                    NavigationProperty onp = a.SubjectNavigationProperty;
                                                    ResourceType srt = onp.Parent;
                                                    srt.NavigationProperties.Remove(onp);
                                                    break;
                                                default:
                                                    break;
                                            }

                                            currentResourceType.NavigationProperties.Remove(navigationPropertyExists);
                                        }
                                        else
                                        {
                                            throw new FaultException(new FaultReason(Properties.Messages.NotFoundNavigationPropertyFaultReason));
                                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(), new FaultReason(Properties.Messages.NotFoundNavigationPropertyFaultReason));
                                        }
                                    }
                                }
                                else
                                {
                                    throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                    //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.ResourceTypeFormat, resourceType)), new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                }
                            }
                            else
                            {
                                throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                                //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(), new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                            }
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNamespace)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                        }

                        dataModel.Synchronize();
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
            }
        }

        /// <summary>
        /// Lists all scalar properties available within a particular resource type for a data model aka model namespace.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <returns>Returns a Scalar Property Collection</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public ScalarPropertyCollection GetAllScalarPropertiesForResourceType(string modelNamespace, string resourceType)
        {
            #region Validating Parameters

            if (string.IsNullOrEmpty(modelNamespace) || string.IsNullOrWhiteSpace(modelNamespace) ||
                string.IsNullOrEmpty(resourceType) || string.IsNullOrWhiteSpace(resourceType))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
                //throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.NullEmptyTitle), new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            #endregion

            DataModel dataModel;
            DataModelModule dataModelModule;
            ResourceTypeCollection resourceTypes;
            ScalarPropertyCollection scalarPropertyCollection = null;

            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        dataModelModule = dataModel.Modules[modelNamespace];
                        if (dataModelModule != null)
                        {
                            resourceTypes = dataModelModule.ResourceTypes;
                            if (resourceTypes != null && resourceTypes.Count > 0)
                            {
                                ResourceType currentResourceType = resourceTypes[resourceType];
                                if (currentResourceType != null)
                                {
                                    scalarPropertyCollection = currentResourceType.ScalarProperties;
                                    if (scalarPropertyCollection == null || scalarPropertyCollection.Count == 0)
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                    //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.ResourceTypeFormat, resourceType)), new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                }
                            }
                            else
                            {
                                throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                                //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(), new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                            }
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNamespace)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                        }
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
            }

            return scalarPropertyCollection;
        }

        /// <summary>
        /// Lists all navigation properties available within a particular resource type for a data model aka model namespace.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        /// <param name="resourceType">Resource Type string</param>
        /// <returns>Returns Navigation Property Collection</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public NavigationPropertyCollection GetAllNavigationPropertiesForResourceType(string modelNamespace, string resourceType)
        {
            #region Validating Parameters

            if (string.IsNullOrEmpty(modelNamespace) || string.IsNullOrWhiteSpace(modelNamespace) ||
                string.IsNullOrEmpty(resourceType) || string.IsNullOrWhiteSpace(resourceType))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
                //throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.NullEmptyTitle), new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            #endregion

            DataModel dataModel;
            DataModelModule dataModelModule;
            ResourceTypeCollection resourceTypes;
            NavigationPropertyCollection navigationPropertyCollection = null;

            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        dataModelModule = dataModel.Modules[modelNamespace];
                        if (dataModelModule != null)
                        {
                            resourceTypes = dataModelModule.ResourceTypes;
                            if (resourceTypes != null && resourceTypes.Count > 0)
                            {
                                ResourceType currentResourceType = resourceTypes[resourceType];
                                if (currentResourceType != null)
                                {
                                    navigationPropertyCollection = currentResourceType.NavigationProperties;
                                    if (navigationPropertyCollection == null || navigationPropertyCollection.Count == 0)
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                    //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.ResourceTypeFormat, resourceType)), new FaultReason(Properties.Messages.NullResourceTypeFaultReason));
                                }
                            }
                            else
                            {
                                throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                                //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(), new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                            }
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNamespace)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                        }
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
            }

            return navigationPropertyCollection;
        }

        #endregion
    }
}
