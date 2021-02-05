// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System.Xml;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System;
using System.Globalization;

namespace Zentity.Core
{
    /// <summary>
    /// Defines the private class that handles the loading and update of DataModels
    /// </summary>
    public sealed partial class DataModel
    {
        /// <summary>
        /// This class handles the loading and update of complete DataModels recursively along with ResourceTypes, 
        /// ScalarProperties, NavigationProperties, Associations and RelationShips.
        /// </summary>
        private static class DataModelLoader
        {
            /// <summary>
            /// Refreshes the specified model.
            /// </summary>
            /// <param name="model">The model name.</param>
            /// <param name="storeConnection">The store connection.</param>
            internal static void Refresh(DataModel model, SqlConnection storeConnection)
            {
                // Save off the modules collection reference.
                DataModelModuleCollection savedModules = model.modules;

                try
                {
                    // Pull custom resource type information from backend.
                    XmlDocument xDataModel = new XmlDocument();

                    if (storeConnection.State == ConnectionState.Closed)
                        storeConnection.Open();

                    using (DbCommand cmd = storeConnection.CreateCommand())
                    {
                        cmd.CommandText = DataModellingResources.Core_GetDataModelModules;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = model.Parent.OperationTimeout;

                        DbParameter param = cmd.CreateParameter();
                        param.DbType = DbType.String;
                        param.Direction = ParameterDirection.Output;
                        param.ParameterName = DataModellingResources.DataModelModules;
                        param.Size = -1;
                        cmd.Parameters.Add(param);
                        cmd.ExecuteNonQuery();

                        xDataModel.LoadXml(param.Value.ToString());
                        // TODO: Validate the xml against a schema.
                    }

                    LoadDataModel(model, xDataModel);
                }
                // TODO: Catch a more specific exception.
                catch
                {
                    // Restore the saved modules collection, if anything goes wrong.
                    model.modules = savedModules;
                    throw;
                }
            }

            /// <summary>
            /// Loads the data model.
            /// </summary>
            /// <param name="dataModel">The data model.</param>
            /// <param name="xDataModel">The xml document with the data model information.</param>
            private static void LoadDataModel(DataModel dataModel, XmlDocument xDataModel)
            {
                dataModel.modules = new DataModelModuleCollection(dataModel);

                // Load data model modules.
                LoadDataModelModuleCollection(dataModel.Modules, xDataModel);

                // Assign the MaxDiscriminator.
                XmlElement eDataModel = xDataModel[DataModellingResources.DataModel];
                if (eDataModel.HasAttribute(DataModellingResources.MaxDiscriminator))
                    dataModel.MaxDiscriminator = Convert.ToInt32(xDataModel[DataModellingResources.DataModel].
                        Attributes[DataModellingResources.MaxDiscriminator].Value, CultureInfo.InvariantCulture);

                // Validate the model graph.
                dataModel.Validate();
            }

            /// <summary>
            /// Loads the data model module.
            /// </summary>
            /// <param name="module">The module.</param>
            /// <param name="xdataModelModule">The xml fragment with data model information.</param>
            private static void LoadDataModelModule(DataModelModule module, XmlNode xdataModelModule)
            {
                XmlElement eDataModelModule = xdataModelModule as XmlElement;

                // Assign Id.
                module.Id = new Guid(eDataModelModule.Attributes[DataModellingResources.Id].Value);

                // Assign NameSpace.
                module.NameSpace = eDataModelModule.Attributes[DataModellingResources.Namespace].Value;

                // Assign Uri.
                module.Uri = eDataModelModule.HasAttribute(DataModellingResources.Uri) ?
                    eDataModelModule.Attributes[DataModellingResources.Uri].Value : null;

                // Assign Description.
                module.Description = eDataModelModule.HasAttribute(DataModellingResources.Description) ?
                    eDataModelModule.Attributes[DataModellingResources.Description].Value : null;

                // Clear internal lists.
                module.ResourceTypes.Clear();

                // Load resource types before associations.
                LoadResourceTypeCollection(module.ResourceTypes,
                    xdataModelModule.SelectNodes(DataModellingResources.XPathRelativeResourceType));

                // Assign IsMsShipped in the end.
                module.IsMsShipped = Convert.ToInt32(xdataModelModule.
                    Attributes[DataModellingResources.IsMsShipped].Value, CultureInfo.InvariantCulture) == 1 ?
                    true : false;
            }

            /// <summary>
            /// Loads the data model module collection.
            /// </summary>
            /// <param name="inputModules">The input modules.</param>
            /// <param name="xDataModel">The xml document with data model information.</param>
            private static void LoadDataModelModuleCollection(DataModelModuleCollection inputModules, XmlDocument xDataModel)
            {
                // We load the module collection in three passes.
                // Pass1 - Create all resource types, scalar and navigation properties but do not 
                // assign base types to the resource types. This allows us to process the derived
                // types before the base types. Otherwise, we might run into scenarios where we do
                // not have a base type reference while processing a derived type.
                // Pass2 - Assign base types to all resource types.
                // Pass3 - Create associations between various navigation properties.

                // Pass1 - Create all resource types, scalar and navigation properties but do not 
                // assign base types to the resource types.
                foreach (XmlNode xModule in xDataModel.SelectNodes(DataModellingResources.XPathDataModelModule))
                {
                    DataModelModule module = new DataModelModule();
                    inputModules.Add(module);
                    LoadDataModelModule(module, xModule);
                }

                // Pass2 - Assign base types to all resource types.
                foreach (XmlNode xResourceType in
                    xDataModel.SelectNodes(DataModellingResources.XPathResourceType))
                {
                    XmlElement eResourceType = xResourceType as XmlElement;
                    Guid id = new Guid(eResourceType.Attributes[DataModellingResources.Id].Value);

                    // Assign BaseType.
                    if (eResourceType.HasAttribute(DataModellingResources.BaseTypeId))
                    {
                        Guid baseResourceTypeId =
                            new Guid(eResourceType.Attributes[DataModellingResources.BaseTypeId].Value);

                        ResourceType derivedType = GetResourceTypeById(inputModules, id);
                        derivedType.BaseType = GetResourceTypeById(inputModules, baseResourceTypeId);
                    }
                }

                // Pass3 - Create associations between various navigation properties.
                foreach (XmlNode xAssociation in
                    xDataModel.SelectNodes(DataModellingResources.XPathAssociation))
                {
                    XmlElement eAssociation = xAssociation as XmlElement;
                    Association association = new Association();

                    // Assign Id.
                    association.Id = new Guid(eAssociation.Attributes[DataModellingResources.Id].Value);

                    // Assign Name.
                    association.Name = eAssociation.Attributes[DataModellingResources.Name].Value;

                    // Assign Uri.
                    association.Uri = eAssociation.HasAttribute(DataModellingResources.Uri) ?
                        eAssociation.Attributes[DataModellingResources.Uri].Value : null;

                    // Assign subject navigation property.
                    Guid subjectNavigationPropertyId = new Guid(eAssociation.
                        Attributes[DataModellingResources.SubjectNavigationPropertyId].Value);
                    association.SubjectNavigationProperty = GetNavigationPropertyById(inputModules,
                        subjectNavigationPropertyId);

                    // Assign object navigation property.
                    Guid objectNavigationPropertyId = new Guid(eAssociation.
                        Attributes[DataModellingResources.ObjectNavigationPropertyId].Value);
                    association.ObjectNavigationProperty = GetNavigationPropertyById(inputModules,
                        objectNavigationPropertyId);

                    // Assign predicate id.
                    association.PredicateId = new Guid(eAssociation.
                        Attributes[DataModellingResources.PredicateId].Value);

                    // Assign subject multiplicity.
                    string subjectMultiplicity =
                        eAssociation.Attributes[DataModellingResources.SubjectMultiplicity].Value;
                    association.SubjectMultiplicity = (AssociationEndMultiplicity)Enum.
                        Parse(typeof(AssociationEndMultiplicity), subjectMultiplicity);

                    // Assign object multiplicity.
                    string objectMultiplicity =
                        eAssociation.Attributes[DataModellingResources.ObjectMultiplicity].Value;
                    association.ObjectMultiplicity = (AssociationEndMultiplicity)Enum.
                        Parse(typeof(AssociationEndMultiplicity), objectMultiplicity);

                    // Assign view name.
                    association.ViewName = eAssociation.Attributes[DataModellingResources.ViewName].Value;
                }
            }

            /// <summary>
            /// Loads the navigation property.
            /// </summary>
            /// <param name="navigationProperty">The navigation property.</param>
            /// <param name="xNavigationProperty">The xml node with navigation property information.</param>
            private static void LoadNavigationProperty(NavigationProperty navigationProperty, XmlNode xNavigationProperty)
            {
                XmlElement eNavigationProperty = xNavigationProperty as XmlElement;

                // Assign Id.
                navigationProperty.Id =
                    new Guid(eNavigationProperty.Attributes[DataModellingResources.Id].Value);

                // Assign Name.
                navigationProperty.Name = eNavigationProperty.Attributes[DataModellingResources.Name].Value;

                // Assign Uri.
                if (eNavigationProperty.HasAttribute(DataModellingResources.Uri))
                    navigationProperty.Uri = eNavigationProperty.Attributes[DataModellingResources.Uri].Value;

                // Assign Description.
                if (eNavigationProperty.HasAttribute(DataModellingResources.Description))
                    navigationProperty.Description =
                        eNavigationProperty.Attributes[DataModellingResources.Description].Value;

                // Assign table and column mappings.
                if (eNavigationProperty.HasAttribute(DataModellingResources.TableName))
                {
                    navigationProperty.TableName =
                        eNavigationProperty.Attributes[DataModellingResources.TableName].Value;

                    navigationProperty.ColumnName =
                        eNavigationProperty.Attributes[DataModellingResources.ColumnName].Value;
                }
            }

            /// <summary>
            /// Loads the navigation property collection.
            /// </summary>
            /// <param name="navigationProperties">The navigation properties.</param>
            /// <param name="xNavigationProperties">The xml node list with navigation properties information.</param>
            private static void LoadNavigationPropertyCollection(NavigationPropertyCollection navigationProperties, XmlNodeList xNavigationProperties)
            {
                foreach (XmlNode xNavigationProperty in xNavigationProperties)
                {
                    NavigationProperty navigationProperty = new NavigationProperty();
                    navigationProperties.Add(navigationProperty);
                    LoadNavigationProperty(navigationProperty, xNavigationProperty);
                }
            }

            /// <summary>
            /// Loads the resource types.
            /// </summary>
            /// <param name="resourceType">The resource type.</param>
            /// <param name="xResourceType">The xml node with resource type information.</param>
            private static void LoadResourceType(ResourceType resourceType, XmlNode xResourceType)
            {
                XmlElement eResourceType = xResourceType as XmlElement;

                // Assign Id.
                resourceType.Id = new Guid(eResourceType.Attributes[DataModellingResources.Id].Value);

                // Assign Discriminator.
                resourceType.Discriminator = int.Parse(
                    eResourceType.Attributes[DataModellingResources.Discriminator].Value,
                    CultureInfo.InvariantCulture);

                // Assign Name.
                resourceType.Name = eResourceType.Attributes[DataModellingResources.Name].Value;

                // Assign Uri.
                if (eResourceType.HasAttribute(DataModellingResources.Uri))
                    resourceType.Uri = eResourceType.Attributes[DataModellingResources.Uri].Value;

                // Assign Description.
                if (eResourceType.HasAttribute(DataModellingResources.Description))
                    resourceType.Description = eResourceType.Attributes[DataModellingResources.Description].Value;

                XmlNode configXmlNode = eResourceType.SelectSingleNode(DataModellingResources.XPathRelativeConfigurationXml);
                if (configXmlNode != null)
                {
                    if (!string.IsNullOrWhiteSpace(configXmlNode.InnerXml))
                    {
                        resourceType.ConfigurationXml = System.Xml.Linq.XElement.Parse(configXmlNode.InnerXml, System.Xml.Linq.LoadOptions.PreserveWhitespace);
                    }
                }

                // Clear internal lists.
                resourceType.ScalarProperties.Clear();
                resourceType.NavigationProperties.Clear();

                // Load scalar properties.
                LoadScalarPropertyCollection(resourceType.ScalarProperties,
                    eResourceType.SelectNodes(DataModellingResources.XPathRelativeScalarProperty));

                // Load navigation properties.
                LoadNavigationPropertyCollection(resourceType.NavigationProperties,
                    eResourceType.SelectNodes(DataModellingResources.XPathRelativeNavigationProperty));
            }

            /// <summary>
            /// Loads the resource type collection.
            /// </summary>
            /// <param name="resourceTypes">The resource types.</param>
            /// <param name="xResourceTypes">The xml node list with resource types information.</param>
            private static void LoadResourceTypeCollection(ResourceTypeCollection resourceTypes, XmlNodeList xResourceTypes)
            {
                foreach (XmlNode xResourceType in xResourceTypes)
                {
                    ResourceType resourceType = new ResourceType();
                    resourceTypes.Add(resourceType);
                    LoadResourceType(resourceType, xResourceType);
                }
            }

            /// <summary>
            /// Loads the scalar property.
            /// </summary>
            /// <param name="scalarProperty">The scalar property.</param>
            /// <param name="xScalarProperty">The xml node with scalar property information.</param>
            private static void LoadScalarProperty(ScalarProperty scalarProperty, XmlNode xScalarProperty)
            {
                XmlElement eScalarProperty = xScalarProperty as XmlElement;

                // Assign Id.
                scalarProperty.Id = new Guid(eScalarProperty.Attributes[DataModellingResources.Id].Value);

                // Assign Name.
                scalarProperty.Name = eScalarProperty.Attributes[DataModellingResources.Name].Value;

                // Assign Nullable.
                scalarProperty.Nullable =
                    Convert.ToInt32(eScalarProperty.Attributes[DataModellingResources.Nullable].Value,
                    CultureInfo.InvariantCulture) == 1 ? true : false;

                // Assign Uri.
                if (eScalarProperty.HasAttribute(DataModellingResources.Uri))
                    scalarProperty.Uri = eScalarProperty.Attributes[DataModellingResources.Uri].Value;

                // Assign Description.
                if (eScalarProperty.HasAttribute(DataModellingResources.Description))
                    scalarProperty.Description = eScalarProperty.Attributes[DataModellingResources.Description].Value;

                // Assign DataType.
                string dataType = eScalarProperty.Attributes[DataModellingResources.DataType].Value;
                scalarProperty.DataType = (DataTypes)Enum.Parse(typeof(DataTypes), dataType);

                // Assign MaxLength.
                if (eScalarProperty.HasAttribute(DataModellingResources.MaxLength))
                    scalarProperty.MaxLength = Convert.ToInt32(eScalarProperty.
                        Attributes[DataModellingResources.MaxLength].Value, CultureInfo.InvariantCulture);

                // Assign Scale.
                if (eScalarProperty.HasAttribute(DataModellingResources.Scale))
                    scalarProperty.Scale = Convert.ToInt32(eScalarProperty.
                        Attributes[DataModellingResources.Scale].Value, CultureInfo.InvariantCulture);

                // Assign Precision.
                if (eScalarProperty.HasAttribute(DataModellingResources.Precision))
                    scalarProperty.Precision = Convert.ToInt32(eScalarProperty.
                        Attributes[DataModellingResources.Precision].Value, CultureInfo.InvariantCulture);

                // Assign TableName.
                scalarProperty.TableName = eScalarProperty.Attributes[DataModellingResources.TableName].Value;

                // Assign ColumnName.
                scalarProperty.ColumnName = eScalarProperty.Attributes[DataModellingResources.ColumnName].Value;

                // Assign IsFullTextIndexed.
                scalarProperty.IsFullTextIndexed = Convert.ToInt32(eScalarProperty.
                    Attributes[DataModellingResources.IsFullTextIndexed].Value,
                    CultureInfo.InvariantCulture) == 1 ? true : false;
            }

            /// <summary>
            /// Loads the scalar property collection.
            /// </summary>
            /// <param name="scalarProperties">The scalar properties.</param>
            /// <param name="xScalarProperties">The xml node list with scalar properties information.</param>
            private static void LoadScalarPropertyCollection(ScalarPropertyCollection scalarProperties, XmlNodeList xScalarProperties)
            {
                foreach (XmlNode xScalarProperty in xScalarProperties)
                {
                    ScalarProperty scalarProperty = new ScalarProperty();
                    scalarProperties.Add(scalarProperty);
                    LoadScalarProperty(scalarProperty, xScalarProperty);
                }
            }
        }
    }
}
