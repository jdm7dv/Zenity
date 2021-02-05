// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a resource type in the data model.
    /// </summary>
    /// <example>Example below shows how to introduce and remove resource types from the data model.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            string namespaceName = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
    ///
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                ResourceType rtResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
    ///                DataModelModule module = new DataModelModule { NameSpace = namespaceName };
    ///                context.DataModel.Modules.Add(module);
    ///
    ///                // Create a new resource type. 
    ///                ResourceType newBaseType = new ResourceType
    ///                {
    ///                    BaseType = rtResource,
    ///                    Name = &quot;NewBase&quot;,
    ///                    Uri = &quot;urn:zentity-samples:resource-type:new-base&quot;
    ///                };
    ///
    ///                // Add some scalar properties to it. 
    ///                ScalarProperty baseProp1 = new ScalarProperty(&quot;Prop1&quot;, DataTypes.String);
    ///                baseProp1.MaxLength = -1;
    ///                newBaseType.ScalarProperties.Add(baseProp1);
    ///                ScalarProperty baseProp2 = new ScalarProperty(&quot;Prop2&quot;, DataTypes.Int32);
    ///                newBaseType.ScalarProperties.Add(baseProp2);
    ///
    ///                // Add base type to context. 
    ///                module.ResourceTypes.Add(newBaseType);
    ///
    ///                // Create another resource type. Make sure that the base type reffered
    ///                // in the resource type declaration is already present in the resource
    ///                // type collection.
    ///                ResourceType newDerivedType = new ResourceType
    ///                {
    ///                    BaseType = newBaseType,
    ///                    Name = &quot;NewDerived&quot;,
    ///                    Uri = &quot;urn:zentity-samples:resource-type:new-derived&quot;
    ///                };
    ///                ScalarProperty derivedProp1 = new ScalarProperty(&quot;Prop3&quot;, DataTypes.String);
    ///                derivedProp1.MaxLength = 1024;
    ///                newDerivedType.ScalarProperties.Add(derivedProp1);
    ///
    ///                module.ResourceTypes.Add(newDerivedType);
    ///
    ///                // This method takes a few minutes to complete depending on the actions taken by 
    ///                // other modules (such as change history logging) in response to schema changes.
    ///                // Provide a sufficient timeout.
    ///                context.CommandTimeout = 300;
    ///                context.DataModel.Synchronize();
    ///            }
    ///
    ///            // Retrieve the resource types in the repository. 
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                DataModelModule module = context.DataModel.Modules[namespaceName];
    ///                foreach (ResourceType rtInfo in module.ResourceTypes)
    ///                {
    ///                    Console.WriteLine(&quot;----------------------------------------------------------------------&quot;);
    ///                    Console.WriteLine(&quot;Resource type Name = [{0}]&quot;, rtInfo.Name);
    ///                    Console.WriteLine(&quot;Resource type Uri = [{0}]&quot;, rtInfo.Uri);
    ///                    Console.WriteLine(&quot;Resource type scalar properties: &quot;);
    ///                    foreach (ScalarProperty prop in rtInfo.ScalarProperties)
    ///                        Console.WriteLine(&quot;\t&quot; + prop.Name);
    ///                }
    ///            }
    ///
    ///            // Remove resource types.
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                DataModelModule module = context.DataModel.Modules[namespaceName];
    ///                ResourceType newBaseType = module.ResourceTypes[&quot;NewBase&quot;];
    ///                ResourceType newDerivedType = module.ResourceTypes[&quot;NewDerived&quot;];
    ///
    ///                module.ResourceTypes.Remove(newBaseType);
    ///                module.ResourceTypes.Remove(newDerivedType);
    ///
    ///                // Save off the changes to backend. 
    ///                // Provide a sufficient timeout.
    ///                context.CommandTimeout = 300;
    ///                context.DataModel.Synchronize();
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    [DataContract]
    public sealed class ResourceType
    {
        #region Fields

        ResourceType baseType;
        string description;
        int discriminator;
        Guid id;
        string name;
        NavigationPropertyCollection navigationProperties;
        DataModelModule parent;
        ScalarPropertyCollection scalarProperties;
        string uri;
        XElement configurationXml;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the base type of the resource type.
        /// </summary>
        [DataMember]
        public ResourceType BaseType
        {
            get { return baseType; }
            set
            {
                baseType = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of the resource type.
        /// </summary>
        [DataMember]
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
            }
        }

        /// <summary>
        /// Gets the full name of resource type.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        [DataMember(EmitDefaultValue=false)]
        public string FullName
        {
            get
            {
                if (this.Parent == null)
                    throw new ZentityException(DataModellingResources.ExceptionCannotLocateParentModule);

                return Utilities.MergeSubNames(this.Parent.NameSpace, this.name);
            }
            private set
            {
                // Required for Data Service Contract
            }
        }

        /// <summary>
        /// Gets the unique identifier for the resource type in the data model.
        /// </summary>
        [DataMember]
        public Guid Id
        {
            get { return id; }
            internal set { id = value; }
        }

        /// <summary>
        /// Gets or sets the name of resource type. 
        /// </summary>
        /// <remarks>Maximum length of name is 100 and it should be a valid C# Class identifier. 
        /// E.g. it should not be a language keyword, should not contain special characters etc.
        /// </remarks>
        [DataMember]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets a collection of navigation properties for the resource type.
        /// </summary>
        [DataMember]
        public NavigationPropertyCollection NavigationProperties
        {
            get
            {
                return navigationProperties;
            }
            private set
            {
                // Required for Data Service Contract
            }
        }

        /// <summary>
        /// Gets the parent data model module of the resource type.
        /// </summary>
        public DataModelModule Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        /// <summary>
        /// Gets a collection of scalar properties for the resource type.
        /// </summary>
        [DataMember]
        public ScalarPropertyCollection ScalarProperties
        {
            get
            {
                return scalarProperties;
            }
            private set
            {
                // Required for Data Service Contract
            }
        }

        /// <summary>
        /// Gets or sets the Uri for the resource type.
        /// </summary>
        [DataMember]
        public string Uri
        {
            get { return uri; }
            set
            {
                uri = value;
            }
        }

        /// <summary>
        /// Gets or sets the Configuration Xml fragment for the specific resource type.
        /// </summary>
        public XElement ConfigurationXml
        {
            get { return configurationXml; }
            set
            {
                configurationXml = value;
            }
        }

        /// <summary>
        /// Gets the name of the delete procedure.
        /// </summary>
        /// <value>The name of the delete procedure.</value>
        internal string DeleteProcedureName
        {
            get { return "Delete" + this.Id.ToString("N").ToLowerInvariant(); }
        }

        /// <summary>
        /// Gets or sets the Discriminator value.
        /// </summary>
        internal int Discriminator
        {
            get { return discriminator; }
            set { discriminator = value; }
        }

        /// <summary>
        /// Gets the name of the insert procedure.
        /// </summary>
        /// <value>The name of the insert procedure.</value>
        internal string InsertProcedureName
        {
            get { return "Insert" + this.Id.ToString("N").ToLowerInvariant(); }
        }

        /// <summary>
        /// Gets the name of the update procedure.
        /// </summary>
        /// <value>The name of the update procedure.</value>
        internal string UpdateProcedureName
        {
            get { return "Update" + this.Id.ToString("N").ToLowerInvariant(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new ResourceType.
        /// </summary>
        public ResourceType()
            : this(null, null)
        {
        }

        /// <summary>
        /// Instantiates a new ResourceType.
        /// </summary>
        /// <param name="name">Name of resource type.</param>
        /// <param name="baseType">Fully qualified name of the base type.</param>
        public ResourceType(string name, ResourceType baseType)
        {
            this.id = Guid.NewGuid();
            this.name = name;
            this.baseType = baseType;

            scalarProperties = new ScalarPropertyCollection(this);
            navigationProperties = new NavigationPropertyCollection(this);
        }


        #endregion

        #region Helper methods.

        /// <summary>
        /// Allows to update the ResourceType definition in database by calling the currect stored procedure.
        /// </summary>
        /// <param name="connectionString">The context connection string that is used make a direct connection to the database.</param>
        public void Update(string connectionString)
        {
            // We open a new connection here and do not reuse context connection. Using context 
            // connection might raise errors if there is an explicit transaction opened on it by 
            // a client. In that case, the ExecuteNonQuery here should use the same client 
            // transaction and it is difficult to get hold of the client initiated transaction here.
            using (SqlConnection storeConnection = new SqlConnection(connectionString))
            {
                if (storeConnection.State == ConnectionState.Closed)
                    storeConnection.Open();

                using (SqlCommand cmd = storeConnection.CreateCommand())
                {
                    cmd.CommandText = DataModellingResources.Core_CreateOrUpdateResourceType;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = this.Parent.Parent.Parent.OperationTimeout;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    
                    string configXml = this.ConfigurationXml == null ? string.Empty : this.ConfigurationXml.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                    object[] paramArray = new object[] {
                        this.Id,
                        this.Parent.Id,
                        this.BaseType == null ? (object)DBNull.Value : this.BaseType.Id,
                        this.Name,
                        String.IsNullOrEmpty(this.Uri) ? (object)DBNull.Value : this.Uri,
                        String.IsNullOrEmpty(this.Description) ? (object)DBNull.Value : this.Description,
                        this.Discriminator,
                        configXml
                    };

                    int index = 0;
                    foreach (SqlParameter param in cmd.Parameters)
                    {
                        if (param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.Output)
                        {
                            param.Value = paramArray[index++];
                        }
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Updates the CSDL.
        /// </summary>
        /// <param name="coreCsdl">The core CSDL.</param>
        /// <param name="moduleCsdl">The module CSDL.</param>
        internal void UpdateCsdl(XmlDocument coreCsdl, XmlDocument moduleCsdl)
        {
            // Locate the Schema element.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(moduleCsdl.NameTable);
            nsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix, DataModellingResources.CSDLSchemaNameSpace);

            XmlElement schemaElement = moduleCsdl.SelectSingleNode(
                DataModellingResources.XPathCSDLSchema, nsMgr) as XmlElement;

            // Create the EntityType element.
            XmlElement entityTypeElement = Utilities.CreateElement(schemaElement, DataModellingResources.EntityType);
            Utilities.AddAttribute(entityTypeElement, DataModellingResources.Name, this.Name);
            Utilities.AddAttribute(entityTypeElement, DataModellingResources.BaseType, this.BaseType.FullName);

            // Create Documentation element.
            if (!string.IsNullOrEmpty(this.Description))
            {
                XmlElement xDocumentation = Utilities.CreateElement(entityTypeElement,
                    DataModellingResources.Documentation);
                XmlElement xSummary = Utilities.CreateElement(xDocumentation,
                    DataModellingResources.Summary);
                xSummary.InnerText = this.Description;
            }

            // Create Property elements.
            foreach (ScalarProperty item in this.ScalarProperties)
                item.UpdateCsdl(entityTypeElement);

            // Create NavigationProperty elements.
            foreach (NavigationProperty item in this.NavigationProperties)
                item.UpdateCsdl(entityTypeElement);

            #region Create CUD FunctionImports

            nsMgr = new XmlNamespaceManager(coreCsdl.NameTable);
            nsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix, DataModellingResources.CSDLSchemaNameSpace);

            XmlElement entityContainerElement = coreCsdl.SelectSingleNode(
                DataModellingResources.XPathCSDLEntityContainer, nsMgr) as XmlElement;

            List<string> parameters = new List<string>();
            List<string> deleteParameters = new List<string>();
            deleteParameters.Add(DataModellingResources.Id);
            deleteParameters.Add(DataTypes.Guid.ToString());
            deleteParameters.Add(DataModellingResources.In);

            ResourceType tempType = this;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    parameters.Add(scalarProperty.Name);
                    parameters.Add(scalarProperty.DataType.ToString());
                    parameters.Add(DataModellingResources.In);
                }

                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    // Assumption here is that there are no One-To-One associations in the data model.
                    if (navigationProperty.Direction == AssociationEndType.Subject &&
                        navigationProperty.Association.ObjectMultiplicity == AssociationEndMultiplicity.One ||
                        navigationProperty.Direction == AssociationEndType.Object &&
                        navigationProperty.Association.SubjectMultiplicity == AssociationEndMultiplicity.One)
                    {
                        parameters.Add(navigationProperty.Name);
                        parameters.Add(DataTypes.Guid.ToString());
                        parameters.Add(DataModellingResources.In);

                        deleteParameters.Add(navigationProperty.Name);
                        deleteParameters.Add(DataTypes.Guid.ToString());
                        deleteParameters.Add(DataModellingResources.In);
                    }
                }
                tempType = tempType.BaseType;
            }

            Utilities.AddCsdlFunctionImport(entityContainerElement, this.InsertProcedureName,
                parameters.ToArray());

            Utilities.AddCsdlFunctionImport(entityContainerElement, this.UpdateProcedureName,
                parameters.ToArray());

            Utilities.AddCsdlFunctionImport(entityContainerElement, this.DeleteProcedureName,
                deleteParameters.ToArray());

            #endregion
        }

        /// <summary>
        /// Updates the flattened CSDL.
        /// </summary>
        /// <param name="coreCsdl">The core CSDL.</param>
        /// <param name="moduleCsdl">The module CSDL.</param>
        internal void UpdateFlattenedCsdl(XmlDocument coreCsdl, XmlDocument moduleCsdl)
        {
            // Locate the Schema element.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(moduleCsdl.NameTable);
            nsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix, DataModellingResources.CSDLSchemaNameSpace);

            XmlElement schemaElement = moduleCsdl.SelectSingleNode(DataModellingResources.XPathCSDLSchema, nsMgr) as XmlElement;
            string conceptualSchemaNamespace = schemaElement.Attributes[DataModellingResources.Namespace].Value;
            string entityTypeFullName = Utilities.MergeSubNames(conceptualSchemaNamespace, this.Name);

            // Create the EntityType element.
            XmlElement entityTypeElement = Utilities.CreateElement(schemaElement, DataModellingResources.EntityType);
            Utilities.AddAttribute(entityTypeElement, DataModellingResources.Name, this.Name);
            
            // Create Documentation element.
            if (!string.IsNullOrEmpty(this.Description))
            {
                XmlElement xDocumentation = Utilities.CreateElement(entityTypeElement, DataModellingResources.Documentation);
                XmlElement xSummary = Utilities.CreateElement(xDocumentation, DataModellingResources.Summary);
                xSummary.InnerText = this.Description;
            }

            // Add Key and PropertyRef element.
            XmlElement keyElement = Utilities.CreateElement(entityTypeElement, DataModellingResources.Key);
            XmlElement propertyRefElement = Utilities.CreateElement(keyElement, DataModellingResources.PropertyRef);
            Utilities.AddAttribute(propertyRefElement, DataModellingResources.Name, DataModellingResources.Id);

            #region Create EntitySet and set the EntityType properties

            // Locate the EntityContainer element.
            nsMgr = new XmlNamespaceManager(coreCsdl.NameTable);
            nsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix, DataModellingResources.CSDLSchemaNameSpace);
            XmlElement entityContainerElement = coreCsdl.SelectSingleNode(DataModellingResources.XPathCSDLEntityContainer, nsMgr) as XmlElement;

            // Add EntitySet for the ResourceType
            Utilities.AddCsdlEntitySet(entityContainerElement, this.Name, entityTypeFullName);

            ResourceType tempType = this;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    scalarProperty.UpdateCsdl(entityTypeElement, this.Name);
                }

                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    navigationProperty.UpdateFlattenedCsdl(entityTypeElement, this);
                }
                tempType = tempType.BaseType;
            }

            #endregion
        }

        /// <summary>
        /// Updates the MSL.
        /// </summary>
        /// <param name="mslDocument">The MSL document.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
        /// <param name="storageSchemaName">Name of the storage schema.</param>
        internal void UpdateMsl(XmlDocument mslDocument, TableMappingCollection tableMappings, Dictionary<Guid, int> discriminators, string storageSchemaName)
        {
            // Locate the <EntitySetMapping Name="Resources"> element
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(mslDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.MSLNamespacePrefix, DataModellingResources.MSLSchemaNamespace);

            XmlElement entityContainerMappingElement = mslDocument.SelectSingleNode(
                DataModellingResources.XPathMSLEntityContainerMapping, nsMgr) as XmlElement;

            XmlElement entitySetMappingElement = mslDocument.SelectSingleNode(
                string.Format(CultureInfo.InvariantCulture, DataModellingResources.XPathMSLEntitySetMapping,
                DataModellingResources.Resources), nsMgr) as XmlElement;

            // Create <EntityTypeMapping> element.
            XmlElement entityTypeMapping = Utilities.CreateElement(entitySetMappingElement,
                DataModellingResources.EntityTypeMapping);
            Utilities.AddAttribute(entityTypeMapping, DataModellingResources.TypeName,
                this.FullName);

            // Copy over the mapping fragments from base type.
            XmlElement baseEntityTypeMapping = entitySetMappingElement.SelectSingleNode(
                string.Format(CultureInfo.InvariantCulture, DataModellingResources.XPathMSLEntityTypeMapping,
                this.BaseType.FullName), nsMgr) as XmlElement;

            foreach (XmlNode xMappingFragment in baseEntityTypeMapping.SelectNodes(
                DataModellingResources.XPathMSLRelativeMappingFragments, nsMgr))
            {
                XmlNode derivedMappingFragment = entityTypeMapping.AppendChild(
                    mslDocument.ImportNode(xMappingFragment, true));
                XmlElement conditionElement = derivedMappingFragment[DataModellingResources.Condition];
                conditionElement.Attributes[DataModellingResources.Value].Value = discriminators[this.Id].
                    ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
            }

            // Process scalar properties.
            foreach (ScalarProperty scalarProperty in this.ScalarProperties)
                scalarProperty.UpdateMsl(entityTypeMapping, tableMappings, discriminators);

            #region Create ModificationFunctionMapping element.

            // Create Function Import Mappings.
            string insertFunctionFullName =
                Utilities.MergeSubNames(storageSchemaName, InsertProcedureName);
            string updateFunctionFullName =
                Utilities.MergeSubNames(storageSchemaName, UpdateProcedureName);
            string deleteFunctionFullName =
                Utilities.MergeSubNames(storageSchemaName, DeleteProcedureName);
            Utilities.AddMslFunctionImportMapping(entityContainerMappingElement,
                InsertProcedureName, insertFunctionFullName);
            Utilities.AddMslFunctionImportMapping(entityContainerMappingElement,
                UpdateProcedureName, updateFunctionFullName);
            Utilities.AddMslFunctionImportMapping(entityContainerMappingElement,
                DeleteProcedureName, deleteFunctionFullName);

            XmlElement modificationFunctionMappingElement = Utilities.CreateElement(
                entityTypeMapping, DataModellingResources.ModificationFunctionMapping);

            XmlElement insertFunctionElement = Utilities.CreateElement(
                modificationFunctionMappingElement, DataModellingResources.InsertFunction);
            Utilities.AddAttribute(insertFunctionElement, DataModellingResources.FunctionName,
                insertFunctionFullName);
            XmlElement updateFunctionElement = Utilities.CreateElement(
                modificationFunctionMappingElement, DataModellingResources.UpdateFunction);
            Utilities.AddAttribute(updateFunctionElement, DataModellingResources.FunctionName,
                updateFunctionFullName);
            XmlElement deleteFunctionElement = Utilities.CreateElement(
                modificationFunctionMappingElement, DataModellingResources.DeleteFunction);
            Utilities.AddAttribute(deleteFunctionElement, DataModellingResources.FunctionName,
                deleteFunctionFullName);

            // DeleteFunction always has an Id scalar property mapping.
            {
                XmlElement scalarPropertyElement = Utilities.CreateElement(
                    deleteFunctionElement, DataModellingResources.ScalarProperty);
                Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Name,
                    DataModellingResources.Id);
                Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.ParameterName,
                    DataModellingResources.Id);
            }

            ResourceType tempType = this;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    XmlElement scalarPropertyElement = Utilities.CreateElement(
                        insertFunctionElement, DataModellingResources.ScalarProperty);
                    Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Name,
                        scalarProperty.Name);
                    Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.ParameterName,
                        scalarProperty.Name);

                    // Only UpdateFunction has the Version attribute = "Current".
                    scalarPropertyElement = updateFunctionElement.AppendChild(
                        mslDocument.ImportNode(scalarPropertyElement, true)) as XmlElement;
                    Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Version,
                        DataModellingResources.Current);
                }

                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    Association association = navigationProperty.Association;
                    // Assumption here is that there are no One-To-One associations in the data model.
                    if (navigationProperty.Direction == AssociationEndType.Subject &&
                        association.ObjectMultiplicity == AssociationEndMultiplicity.One ||
                        navigationProperty.Direction == AssociationEndType.Object &&
                        association.SubjectMultiplicity == AssociationEndMultiplicity.One)
                    {
                        XmlElement associationEndElement = Utilities.CreateElement(insertFunctionElement,
                            DataModellingResources.AssociationEnd);
                        Utilities.AddAttribute(associationEndElement, DataModellingResources.AssociationSet,
                            association.Name);
                        if (navigationProperty.Direction == AssociationEndType.Subject)
                        {
                            Utilities.AddAttribute(associationEndElement, DataModellingResources.From,
                                association.SubjectRole);
                            Utilities.AddAttribute(associationEndElement, DataModellingResources.To,
                                association.ObjectRole);
                        }
                        else
                        {
                            Utilities.AddAttribute(associationEndElement, DataModellingResources.From,
                                association.ObjectRole);
                            Utilities.AddAttribute(associationEndElement, DataModellingResources.To,
                                association.SubjectRole);
                        }

                        XmlElement scalarPropertyElement = Utilities.CreateElement(associationEndElement,
                            DataModellingResources.ScalarProperty);
                        Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Name,
                            DataModellingResources.Id);
                        Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.ParameterName,
                            navigationProperty.Name);

                        deleteFunctionElement.AppendChild(mslDocument.ImportNode(associationEndElement, true));

                        // Only UpdateFunction has the Version attribute = "Current".
                        associationEndElement = updateFunctionElement.AppendChild(
                            mslDocument.ImportNode(associationEndElement, true)) as XmlElement;
                        scalarPropertyElement = associationEndElement[DataModellingResources.ScalarProperty];
                        Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Version,
                            DataModellingResources.Current);
                    }
                }
                tempType = tempType.BaseType;
            }

            #endregion
        }

        /// <summary>
        /// Updates the flattened MSL.
        /// </summary>
        /// <param name="mslDocument">The MSL document.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
        /// <param name="storageSchemaName">Name of the storage schema.</param>
        internal void UpdateFlattenedMsl(XmlDocument mslDocument, TableMappingCollection tableMappings, Dictionary<Guid, int> discriminators, string storageSchemaName)
        {
            // Locate the <EntitySetMapping Name="Resources"> element
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(mslDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.MSLNamespacePrefix, DataModellingResources.MSLSchemaNamespace);

            XmlElement entityContainerMappingElement = mslDocument.SelectSingleNode(DataModellingResources.XPathMSLEntityContainerMapping, nsMgr) as XmlElement;

            XmlElement entitySetMappingElement = Utilities.CreateElement(entityContainerMappingElement, DataModellingResources.EntitySetMapping);
            Utilities.AddAttribute(entitySetMappingElement, DataModellingResources.Name, this.Name);

            // Create <EntityTypeMapping> elements.
            XmlElement entityTypeMapping = Utilities.CreateElement(entitySetMappingElement, DataModellingResources.EntityTypeMapping);
            XmlElement entityTypeMappingForFunctions = Utilities.CreateElement(entitySetMappingElement, DataModellingResources.EntityTypeMapping);
            Utilities.AddAttribute(entityTypeMapping, DataModellingResources.TypeName, this.FullName);
            Utilities.AddAttribute(entityTypeMappingForFunctions, DataModellingResources.TypeName, this.FullName);

            #region Create ModificationFunctionMapping element.

            // Create Function Import Mappings.
            string insertFunctionFullName = Utilities.MergeSubNames(storageSchemaName, InsertProcedureName);
            string updateFunctionFullName = Utilities.MergeSubNames(storageSchemaName, UpdateProcedureName);
            string deleteFunctionFullName = Utilities.MergeSubNames(storageSchemaName, DeleteProcedureName);

            // Create the MappingFragment and the ModificationFunctionMapping elements
            XmlElement mappingFragmentElement = Utilities.CreateElement(entityTypeMapping, DataModellingResources.MappingFragment);
            Utilities.AddAttribute(mappingFragmentElement, DataModellingResources.StoreEntitySet, this.Name);
            XmlElement modificationFunctionMappingElement = Utilities.CreateElement(entityTypeMappingForFunctions, DataModellingResources.ModificationFunctionMapping);

            // Create the InsertFunction, UpdateFunction and DeleteFunction elements
            XmlElement insertFunctionElement = Utilities.CreateElement(modificationFunctionMappingElement, DataModellingResources.InsertFunction);
            Utilities.AddAttribute(insertFunctionElement, DataModellingResources.FunctionName, insertFunctionFullName);
            XmlElement updateFunctionElement = Utilities.CreateElement(modificationFunctionMappingElement, DataModellingResources.UpdateFunction);
            Utilities.AddAttribute(updateFunctionElement, DataModellingResources.FunctionName, updateFunctionFullName);
            XmlElement deleteFunctionElement = Utilities.CreateElement(modificationFunctionMappingElement, DataModellingResources.DeleteFunction);
            Utilities.AddAttribute(deleteFunctionElement, DataModellingResources.FunctionName, deleteFunctionFullName);

            // DeleteFunction always has an Id scalar property mapping.
            XmlElement scalarPropertyElement = Utilities.CreateElement(deleteFunctionElement, DataModellingResources.ScalarProperty);
            Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Name, DataModellingResources.Id);
            Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.ParameterName, DataModellingResources.Id);

            ResourceType tempType = this;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    scalarPropertyElement = Utilities.CreateElement(insertFunctionElement, DataModellingResources.ScalarProperty);
                    string propertyName = scalarProperty.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase)
                                          ? scalarProperty.Name + DataModellingResources.Property
                                          : scalarProperty.Name;
                    Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Name, propertyName);
                    Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.ParameterName, scalarProperty.Name);

                    // Only UpdateFunction has the Version attribute = "Current".
                    scalarPropertyElement = updateFunctionElement.AppendChild(mslDocument.ImportNode(scalarPropertyElement, true)) as XmlElement;
                    Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Version, DataModellingResources.Current);

                    scalarProperty.UpdateFlattenedMsl(mappingFragmentElement, this.Name);
                }

                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    Association association = navigationProperty.Association;
                    // Assumption here is that there are no One-To-One associations in the data model.
                    if (navigationProperty.Direction == AssociationEndType.Subject &&
                        association.ObjectMultiplicity == AssociationEndMultiplicity.One ||
                        navigationProperty.Direction == AssociationEndType.Object &&
                        association.SubjectMultiplicity == AssociationEndMultiplicity.One)
                    {

                        if (association.ObjectMultiplicity == AssociationEndMultiplicity.One &&
                            association.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne ||
                            association.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                            association.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
                        {
                            XmlElement associationEndElement = Utilities.CreateElement(insertFunctionElement, DataModellingResources.AssociationEnd);
                            Utilities.AddAttribute(associationEndElement, DataModellingResources.AssociationSet, association.Name + "_" + association.SubjectRole + association.ObjectRole);

                            if (navigationProperty.Direction == AssociationEndType.Subject)
                            {
                                Utilities.AddAttribute(associationEndElement, DataModellingResources.From, association.SubjectRole);
                                Utilities.AddAttribute(associationEndElement, DataModellingResources.To, association.ObjectRole);
                            }
                            else
                            {
                                Utilities.AddAttribute(associationEndElement, DataModellingResources.From, association.ObjectRole);
                                Utilities.AddAttribute(associationEndElement, DataModellingResources.To, association.SubjectRole);
                            }

                            scalarPropertyElement = Utilities.CreateElement(associationEndElement, DataModellingResources.ScalarProperty);
                            Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Name, DataModellingResources.Id);
                            Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.ParameterName, navigationProperty.Name);

                            deleteFunctionElement.AppendChild(mslDocument.ImportNode(associationEndElement, true));

                            // Only UpdateFunction has the Version attribute = "Current".
                            associationEndElement = updateFunctionElement.AppendChild(mslDocument.ImportNode(associationEndElement, true)) as XmlElement;
                            scalarPropertyElement = associationEndElement[DataModellingResources.ScalarProperty];
                            Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Version, DataModellingResources.Current);
                        }
                        else
                        {
                            // Scalar Property for MappingFragment - Entity Mapping
                            scalarPropertyElement = Utilities.CreateElement(mappingFragmentElement, DataModellingResources.ScalarProperty);
                            Utilities.AddAttribute(scalarPropertyElement,
                                                   DataModellingResources.Name,
                                                   (association.ObjectMultiplicity == AssociationEndMultiplicity.One)
                                                       ? association.ObjectNavigationProperty.Parent.Name + "_Id"
                                                       : association.SubjectNavigationProperty.Parent.Name + "_Id");
                            Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.ColumnName, navigationProperty.Id.ToString().ToLowerInvariant());

                            // Scalar Property for Insert ModificationFunctionMapping - Entity Function Mapping
                            scalarPropertyElement = Utilities.CreateElement(insertFunctionElement, DataModellingResources.ScalarProperty);
                            Utilities.AddAttribute(scalarPropertyElement,
                                                   DataModellingResources.Name,
                                                   (association.ObjectMultiplicity == AssociationEndMultiplicity.One)
                                                       ? association.ObjectNavigationProperty.Parent.Name + "_Id"
                                                       : association.SubjectNavigationProperty.Parent.Name + "_Id");
                            Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.ParameterName, navigationProperty.Name);

                            // Import the Scalar Property element to the Delete ModificationFunctionMapping - Entity Function Mapping
                            deleteFunctionElement.AppendChild(mslDocument.ImportNode(scalarPropertyElement, true));

                            // Import the Scalar Property element to the Update ModificationFunctionMapping - Entity Function Mapping
                            scalarPropertyElement = updateFunctionElement.AppendChild(mslDocument.ImportNode(scalarPropertyElement, true)) as XmlElement;
                            Utilities.AddAttribute(scalarPropertyElement, DataModellingResources.Version, DataModellingResources.Current);
                        }
                    }
                    
                    
                }
                tempType = tempType.BaseType;
            }

            #endregion
        }

        /// <summary>
        /// Updates the SSDL.
        /// </summary>
        /// <param name="ssdlDocument">The SSDL document.</param>
        /// <param name="tableMappings">The table mappings.</param>
        internal void UpdateSsdl(XmlDocument ssdlDocument, TableMappingCollection tableMappings)
        {
            foreach (ScalarProperty property in this.ScalarProperties)
                property.UpdateSsdl(ssdlDocument, tableMappings);

            #region Create CUD Functions.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ssdlDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.SSDLNamespacePrefix,
                DataModellingResources.SSDLSchemaNameSpace);
            XmlElement schemaElement = ssdlDocument.SelectSingleNode(
                DataModellingResources.XPathSSDLSchema, nsMgr) as XmlElement;

            List<string> parameters = new List<string>();
            List<string> deleteParameters = new List<string>();
            deleteParameters.Add(DataModellingResources.Id);
            deleteParameters.Add(DataModellingResources.DataTypeUniqueidentifier);
            deleteParameters.Add(DataModellingResources.In);

            ResourceType tempType = this;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    parameters.Add(scalarProperty.Name);

                    if (scalarProperty.MaxLength < 0 && 
                        (scalarProperty.DataType == DataTypes.String || 
                        scalarProperty.DataType == DataTypes.Binary))
                        parameters.Add(Utilities.GetSQLType(scalarProperty.DataType) +
                            string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.Paranthesis,
                            DataModellingResources.Max).ToLowerInvariant());
                    else
                        parameters.Add(Utilities.GetSQLType(scalarProperty.DataType));

                    parameters.Add(DataModellingResources.In);
                }
                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    if (tableMappings.GetColumnMappingByPropertyId(navigationProperty.Id) != null)
                    {
                        parameters.Add(navigationProperty.Name);
                        parameters.Add(DataModellingResources.DataTypeUniqueidentifier);
                        parameters.Add(DataModellingResources.In);

                        deleteParameters.Add(navigationProperty.Name);
                        deleteParameters.Add(DataModellingResources.DataTypeUniqueidentifier);
                        deleteParameters.Add(DataModellingResources.In);
                    }
                }
                tempType = tempType.BaseType;
            }

            Utilities.AddSsdlFunction(schemaElement, this.InsertProcedureName, false, false,
                false, false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                parameters.ToArray());

            Utilities.AddSsdlFunction(schemaElement, this.UpdateProcedureName, false, false,
                false, false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                parameters.ToArray());

            Utilities.AddSsdlFunction(schemaElement, this.DeleteProcedureName, false, false, false,
                false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                deleteParameters.ToArray());

            #endregion
        }

        /// <summary>
        /// Updates the flattened SSDL.
        /// </summary>
        /// <param name="ssdlDocument">The SSDL document.</param>
        /// <param name="tableMappings">The table mappings.</param>
        internal void UpdateFlattenedSsdl(XmlDocument ssdlDocument, TableMappingCollection tableMappings)
        {
            #region Create CUD Functions and Defining Query EntitySet and EntityType.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ssdlDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.SSDLNamespacePrefix, DataModellingResources.SSDLSchemaNameSpace);
            XmlElement schemaElement = ssdlDocument.SelectSingleNode(DataModellingResources.XPathSSDLSchema, nsMgr) as XmlElement;
            XmlElement entityContainerElement = schemaElement.SelectSingleNode(DataModellingResources.XPathSSDLEntityContainer, nsMgr) as XmlElement;
            string storageSchemaNamespace = schemaElement.Attributes[DataModellingResources.Namespace].Value;
            string entityTypeFullName = Utilities.MergeSubNames(storageSchemaNamespace, this.Name);
            StringBuilder sbQueryColumns = new StringBuilder();
            
            // Add the EntityType element for the Resource Type defining the new entity
            XmlElement entityTypeElement = Utilities.AddSsdlEntityType(schemaElement, this.Name, DataModellingResources.Id);

            List<string> parameters = new List<string>();
            List<string> deleteParameters = new List<string>();
            deleteParameters.Add(DataModellingResources.Id);
            deleteParameters.Add(DataModellingResources.DataTypeUniqueidentifier);
            deleteParameters.Add(DataModellingResources.In);

            ResourceType tempType = this;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    parameters.Add(scalarProperty.Name);

                    if (scalarProperty.MaxLength < 0 &&
                        (scalarProperty.DataType == DataTypes.String ||
                        scalarProperty.DataType == DataTypes.Binary))
                    {
                        var sqlType = Utilities.GetSQLType(scalarProperty.DataType) +
                                         string.Format(CultureInfo.InvariantCulture,
                                                       DataModellingResources.Paranthesis,
                                                       DataModellingResources.Max).ToLowerInvariant();
                        parameters.Add(sqlType);
                        Utilities.AddSsdlEntityTypeProperty(entityTypeElement, scalarProperty.Name, sqlType, scalarProperty.Nullable);
                    }
                    else
                    {
                        var sqlType = Utilities.GetSQLType(scalarProperty.DataType);
                        parameters.Add(sqlType);

                        if (scalarProperty.MaxLength > 0)
                        {
                            Utilities.AddSsdlEntityTypeProperty(entityTypeElement, scalarProperty.Name, sqlType, scalarProperty.Nullable, scalarProperty.MaxLength);
                        }
                        else
                        {
                            Utilities.AddSsdlEntityTypeProperty(entityTypeElement, scalarProperty.Name, sqlType, scalarProperty.Nullable);
                        }
                    }

                    parameters.Add(DataModellingResources.In);

                    // Generation of the DefiningQuery Columns for the ResourceType
                    if (tempType.BaseType != null)
                    {
                        sbQueryColumns.AppendFormat(DataModellingResources.EntityDefiningQueryColumnFormat, scalarProperty.Id, scalarProperty.Name);
                    }
                }

                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    if (tableMappings.GetColumnMappingByPropertyId(navigationProperty.Id) != null)
                    {
                        parameters.Add(navigationProperty.Name);
                        parameters.Add(DataModellingResources.DataTypeUniqueidentifier);
                        parameters.Add(DataModellingResources.In);

                        deleteParameters.Add(navigationProperty.Name);
                        deleteParameters.Add(DataModellingResources.DataTypeUniqueidentifier);
                        deleteParameters.Add(DataModellingResources.In);

                        string navPropId = navigationProperty.Id.ToString().ToLower();
                        Utilities.AddSsdlEntityTypeProperty(entityTypeElement, navPropId, DataModellingResources.DataTypeUniqueidentifier, false);
                        sbQueryColumns.AppendFormat(DataModellingResources.EntityDefiningQueryColumnFormat, navPropId, navPropId);
                    }
                }
                tempType = tempType.BaseType;
            }

            // Add the EntitySet element for the Resource Type defining the new entity
            string definingQueryBody = string.Format(DataModellingResources.EntityDefiningQueryFormat, sbQueryColumns, this.Id);
            Utilities.AddSsdlEntitySetWithDefiningQuery(entityContainerElement, this.Name, entityTypeFullName, definingQueryBody);

            Utilities.AddSsdlFunction(schemaElement, this.InsertProcedureName, false, false,
                false, false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                parameters.ToArray());

            Utilities.AddSsdlFunction(schemaElement, this.UpdateProcedureName, false, false,
                false, false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                parameters.ToArray());

            Utilities.AddSsdlFunction(schemaElement, this.DeleteProcedureName, false, false, false,
                false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                deleteParameters.ToArray());

            #endregion
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        internal void Validate()
        {
            // Validate Id.
            if (this.Id == Guid.Empty)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionPropertyEmpty,
                    DataModellingResources.Id, this.GetType().ToString()));

            if (!string.IsNullOrEmpty(this.Uri) && this.Uri.Length > 1024)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Uri, 1024));

            // Validate name.
            if (string.IsNullOrEmpty(this.Name))
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionStringPropertyNullOrEmpty, DataModellingResources.Name,
                    this.GetType().ToString()));

            if (this.Name.Length > 100)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Name, 100));

            // Validating the Name for a valid C# class name makes the validation process slow.
            // So we are not including that check here. This check is included at the DataModel
            // level.

            // Validate base type in the resource type.
            // Only Zentity.Core.Resource is allowed to have a NULL basetype.
            if (this.BaseType == null && !(this.FullName).Equals(
                DataModellingResources.ZentityCoreResource))
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionPropertyNull, DataModellingResources.BaseType,
                    this.GetType().ToString()));

            // Validate description.
            if (!string.IsNullOrEmpty(this.Description) && this.Description.Length > 4000)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Description,
                    4000));

            // Detect duplicate property names.
            ResourceType tempType = this;
            List<IResourceTypeProperty> resourceTypeProperties = new List<IResourceTypeProperty>();
            while (tempType != null)
            {
                resourceTypeProperties.AddRange(tempType.ScalarProperties.
                    Select(tuple => tuple as IResourceTypeProperty));
                resourceTypeProperties.AddRange(tempType.NavigationProperties.
                    Select(tuple => tuple as IResourceTypeProperty));
                tempType = tempType.BaseType;
            }

            var nameGroups = resourceTypeProperties.GroupBy(tuple => tuple.Name).
                Where(tuple => tuple.Count() > 1);
            if (nameGroups.Count() > 0)
            {
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionDuplicatePropertiesInResourceType,
                    nameGroups.First().Key, this.FullName));
            }

            // Apply maximum property count restrictions. This is required to avoid errors during
            // while using this resource type. A datarow in SQL Server cannot exceed 8060, so this
            // puts an approximate limit of 307 NOT NULL columns per row assuming each is of the 
            // type nvarchar(max). To be on the safer side, we will put a restriction of 250.
            if (resourceTypeProperties.Count() > 250)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionTooManyProperties, this.Id,
                    this.FullName, 250));

            // Validate scalar properties.
            this.ScalarProperties.Validate();

            // Validate navigation properties.
            this.NavigationProperties.Validate();
        }

        /// <summary>
        /// Gets the derived types.
        /// </summary>
        /// <returns>A collection of derived resource types</returns>
        internal IEnumerable<ResourceType> GetDerivedTypes()
        {
            // Create a list storing the derived ResourceType
            List<ResourceType> listDerivedTypes = new List<ResourceType> {this};

            // Iterate through the derived ResourceType and fetch them recursively
            foreach(ResourceType derivedType in this.Parent.ResourceTypes.Where(resType => resType.BaseType != null && resType.BaseType.Id == this.Id))
            {
                listDerivedTypes.AddRange(derivedType.GetDerivedTypes());
            }

            return listDerivedTypes;
        }

        #endregion
    }
}
