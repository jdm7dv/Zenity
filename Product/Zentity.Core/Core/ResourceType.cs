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
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a resource type in the data model.
    /// </summary>
    /// <example>
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
    ///                    Uri = &quot;ZentitySamples:resource-type:new-base&quot;
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
    ///                    Uri = &quot;ZentitySamples:resource-type:new-derived&quot;
    ///                };
    ///                ScalarProperty derivedProp1 = new ScalarProperty(&quot;Prop3&quot;, DataTypes.String);
    ///                derivedProp1.MaxLength = 1024;
    ///                newDerivedType.ScalarProperties.Add(derivedProp1);
    ///
    ///                module.ResourceTypes.Add(newDerivedType);
    ///
    ///                // Save off the changes to backend. 
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
    ///                context.CommandTimeout = 300;
    ///                context.DataModel.Synchronize();
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public sealed class ResourceType
    {
        #region Fields

        Guid id;
        DataModelModule parent;
        ResourceType baseType;
        string name;
        string uri;
        string description;
        int discriminator;
        ScalarPropertyCollection scalarProperties;
        NavigationPropertyCollection navigationProperties;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique identifier for the resource type in the data model.
        /// </summary>
        public Guid Id
        {
            get { return id; }
            internal set { id = value; }
        }

        /// <summary>
        /// Gets or sets the Uri for the resource type.
        /// </summary>
        public string Uri
        {
            get { return uri; }
            set
            {
                uri = value;
            }
        }

        /// <summary>
        /// Gets or sets the base type of the resource type.
        /// </summary>
        public ResourceType BaseType
        {
            get { return baseType; }
            set
            {
                baseType = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of resource type. 
        /// </summary>
        /// <remarks>Maximum length of name is 100 and it should be a valid C# Class identifier. 
        /// E.g. it should not be a language keyword, should not contain special characters etc.
        /// </remarks>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets the full name of resource type.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public string FullName
        {
            get
            {
                if (this.Parent == null)
                    throw new ZentityException(CoreResources.ExceptionCannotLocateParentModule);

                return Utilities.MergeSubNames(this.Parent.NameSpace, this.name);
            }
        }

        /// <summary>
        /// Gets or sets the description of the resource type.
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
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
        public ScalarPropertyCollection ScalarProperties
        {
            get
            {
                return scalarProperties;
            }
        }

        /// <summary>
        /// Gets a collection of navigation properties for the resource type.
        /// </summary>
        public NavigationPropertyCollection NavigationProperties
        {
            get
            {
                return navigationProperties;
            }
        }

        internal string InsertProcedureName
        {
            get { return "Insert" + this.Id.ToString("N").ToLowerInvariant(); }
        }

        internal string UpdateProcedureName
        {
            get { return "Update" + this.Id.ToString("N").ToLowerInvariant(); }
        }

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

        internal void Validate()
        {
            // Validate Id.
            if (this.Id == Guid.Empty)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionPropertyEmpty,
                    CoreResources.Id, this.GetType().ToString()));

            if (!string.IsNullOrEmpty(this.Uri) && this.Uri.Length > 1024)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Uri, 1024));

            // Validate name.
            if (string.IsNullOrEmpty(this.Name))
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionStringPropertyNullOrEmpty, CoreResources.Name,
                    this.GetType().ToString()));

            if (this.Name.Length > 100)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Name, 100));

            // Validating the Name for a valid C# class name makes the validation process slow.
            // So we are not including that check here. This check is included at the DataModel
            // level.

            // Validate base type in the resource type.
            // Only Zentity.Core.Resource is allowed to have a NULL basetype.
            if (this.BaseType == null && !(this.FullName).Equals(
                CoreResources.ZentityCoreResource))
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionPropertyNull, CoreResources.BaseType,
                    this.GetType().ToString()));

            // Validate description.
            if (!string.IsNullOrEmpty(this.Description) && this.Description.Length > 4000)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Description,
                    4000));

            // Detect duplicate property names.
            List<IResourceTypeProperty> resourceTypeProperties = new List<IResourceTypeProperty>();
            resourceTypeProperties.AddRange(this.ScalarProperties.
                Select(tuple => tuple as IResourceTypeProperty));
            resourceTypeProperties.AddRange(this.NavigationProperties.
                Select(tuple => tuple as IResourceTypeProperty));

            var nameGroups = resourceTypeProperties.GroupBy(tuple => tuple.Name).
                Where(tuple => tuple.Count() > 1);
            if (nameGroups.Count() > 0)
            {
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionDuplicatePropertiesInResourceType,
                    nameGroups.First().Key, this.FullName));
            }

            // Validate scalar properties.
            this.ScalarProperties.Validate();

            // Validate navigation properties.
            this.NavigationProperties.Validate();
        }

        internal void UpdateSsdl(XmlDocument ssdlDocument, TableMappingCollection tableMappings)
        {
            foreach (ScalarProperty property in this.ScalarProperties)
                property.UpdateSsdl(ssdlDocument, tableMappings);


            #region Create CUD Functions.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ssdlDocument.NameTable);
            nsMgr.AddNamespace(CoreResources.SSDLNamespacePrefix,
                CoreResources.SSDLSchemaNameSpace);
            XmlElement schemaElement = ssdlDocument.SelectSingleNode(
                CoreResources.XPathSSDLSchema, nsMgr) as XmlElement;

            List<string> parameters = new List<string>();
            List<string> deleteParameters = new List<string>();
            deleteParameters.Add(CoreResources.Id);
            deleteParameters.Add(CoreResources.DataTypeUniqueidentifier);
            deleteParameters.Add(CoreResources.In);

            ResourceType tempType = this;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    parameters.Add(scalarProperty.Name);
                    parameters.Add(Utilities.GetSQLType(scalarProperty.DataType));
                    parameters.Add(CoreResources.In);
                }
                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    if (tableMappings.GetColumnMappingByPropertyId(navigationProperty.Id) != null)
                    {
                        parameters.Add(navigationProperty.Name);
                        parameters.Add(CoreResources.DataTypeUniqueidentifier);
                        parameters.Add(CoreResources.In);

                        deleteParameters.Add(navigationProperty.Name);
                        deleteParameters.Add(CoreResources.DataTypeUniqueidentifier);
                        deleteParameters.Add(CoreResources.In);
                    }
                }
                tempType = tempType.BaseType;
            }

            Utilities.AddSsdlFunction(schemaElement, this.InsertProcedureName, false, false,
                false, false, CoreResources.AllowImplicitConversion, CoreResources.Core,
                parameters.ToArray());

            Utilities.AddSsdlFunction(schemaElement, this.UpdateProcedureName, false, false,
                false, false, CoreResources.AllowImplicitConversion, CoreResources.Core,
                parameters.ToArray());

            Utilities.AddSsdlFunction(schemaElement, this.DeleteProcedureName, false, false, false,
                false, CoreResources.AllowImplicitConversion, CoreResources.Core,
                deleteParameters.ToArray());

            #endregion

        }

        internal void UpdateCsdl(XmlDocument coreCsdl, XmlDocument moduleCsdl)
        {
            // Locate the Schema element.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(moduleCsdl.NameTable);
            nsMgr.AddNamespace(CoreResources.CSDLNamespacePrefix, CoreResources.CSDLSchemaNameSpace);

            XmlElement schemaElement = moduleCsdl.SelectSingleNode(
                CoreResources.XPathCSDLSchema, nsMgr) as XmlElement;

            // Create the EntityType element.
            XmlElement entityTypeElement = Utilities.CreateElement(schemaElement, CoreResources.EntityType);
            Utilities.AddAttribute(entityTypeElement, CoreResources.Name, this.Name);
            Utilities.AddAttribute(entityTypeElement, CoreResources.BaseType, this.BaseType.FullName);

            // Create Documentation element.
            if (!string.IsNullOrEmpty(this.Description))
            {
                XmlElement xDocumentation = Utilities.CreateElement(entityTypeElement,
                    CoreResources.Documentation);
                XmlElement xSummary = Utilities.CreateElement(xDocumentation,
                    CoreResources.Summary);
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
            nsMgr.AddNamespace(CoreResources.CSDLNamespacePrefix, CoreResources.CSDLSchemaNameSpace);

            XmlElement entityContainerElement = coreCsdl.SelectSingleNode(
                CoreResources.XPathCSDLEntityContainer, nsMgr) as XmlElement;

            List<string> parameters = new List<string>();
            List<string> deleteParameters = new List<string>();
            deleteParameters.Add(CoreResources.Id);
            deleteParameters.Add(DataTypes.Guid.ToString());
            deleteParameters.Add(CoreResources.In);

            ResourceType tempType = this;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    parameters.Add(scalarProperty.Name);
                    parameters.Add(scalarProperty.DataType.ToString());
                    parameters.Add(CoreResources.In);
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
                        parameters.Add(CoreResources.In);

                        deleteParameters.Add(navigationProperty.Name);
                        deleteParameters.Add(DataTypes.Guid.ToString());
                        deleteParameters.Add(CoreResources.In);
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

        internal void UpdateMsl(XmlDocument mslDocument, TableMappingCollection tableMappings, Dictionary<Guid, int> discriminators, string storageSchemaName)
        {
            // Locate the <EntitySetMapping Name="Resources"> element
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(mslDocument.NameTable);
            nsMgr.AddNamespace(CoreResources.MSLNamespacePrefix, CoreResources.MSLSchemaNamespace);

            XmlElement entityContainerMappingElement = mslDocument.SelectSingleNode(
                CoreResources.XPathMSLEntityContainerMapping, nsMgr) as XmlElement;

            XmlElement entitySetMappingElement = mslDocument.SelectSingleNode(
                string.Format(CultureInfo.InvariantCulture, CoreResources.XPathMSLEntitySetMapping,
                CoreResources.Resources), nsMgr) as XmlElement;

            // Create <EntityTypeMapping> element.
            XmlElement entityTypeMapping = Utilities.CreateElement(entitySetMappingElement,
                CoreResources.EntityTypeMapping);
            Utilities.AddAttribute(entityTypeMapping, CoreResources.TypeName,
                this.FullName);

            // Copy over the mapping fragments from base type.
            XmlElement baseEntityTypeMapping = entitySetMappingElement.SelectSingleNode(
                string.Format(CultureInfo.InvariantCulture, CoreResources.XPathMSLEntityTypeMapping,
                this.BaseType.FullName), nsMgr) as XmlElement;

            foreach (XmlNode xMappingFragment in baseEntityTypeMapping.SelectNodes(
                CoreResources.XPathMSLRelativeMappingFragments, nsMgr))
            {
                XmlNode derivedMappingFragment = entityTypeMapping.AppendChild(
                    mslDocument.ImportNode(xMappingFragment, true));
                XmlElement conditionElement = derivedMappingFragment[CoreResources.Condition];
                conditionElement.Attributes[CoreResources.Value].Value = discriminators[this.Id].
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
                entityTypeMapping, CoreResources.ModificationFunctionMapping);

            XmlElement insertFunctionElement = Utilities.CreateElement(
                modificationFunctionMappingElement, CoreResources.InsertFunction);
            Utilities.AddAttribute(insertFunctionElement, CoreResources.FunctionName,
                insertFunctionFullName);
            XmlElement updateFunctionElement = Utilities.CreateElement(
                modificationFunctionMappingElement, CoreResources.UpdateFunction);
            Utilities.AddAttribute(updateFunctionElement, CoreResources.FunctionName,
                updateFunctionFullName);
            XmlElement deleteFunctionElement = Utilities.CreateElement(
                modificationFunctionMappingElement, CoreResources.DeleteFunction);
            Utilities.AddAttribute(deleteFunctionElement, CoreResources.FunctionName,
                deleteFunctionFullName);

            // DeleteFunction always has an Id scalar property mapping.
            {
                XmlElement scalarPropertyElement = Utilities.CreateElement(
                    deleteFunctionElement, CoreResources.ScalarProperty);
                Utilities.AddAttribute(scalarPropertyElement, CoreResources.Name,
                    CoreResources.Id);
                Utilities.AddAttribute(scalarPropertyElement, CoreResources.ParameterName,
                    CoreResources.Id);
            }

            ResourceType tempType = this;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    XmlElement scalarPropertyElement = Utilities.CreateElement(
                        insertFunctionElement, CoreResources.ScalarProperty);
                    Utilities.AddAttribute(scalarPropertyElement, CoreResources.Name,
                        scalarProperty.Name);
                    Utilities.AddAttribute(scalarPropertyElement, CoreResources.ParameterName,
                        scalarProperty.Name);

                    // Only UpdateFunction has the Version attribute = "Current".
                    scalarPropertyElement = updateFunctionElement.AppendChild(
                        mslDocument.ImportNode(scalarPropertyElement, true)) as XmlElement;
                    Utilities.AddAttribute(scalarPropertyElement, CoreResources.Version,
                        CoreResources.Current);
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
                            CoreResources.AssociationEnd);
                        Utilities.AddAttribute(associationEndElement, CoreResources.AssociationSet,
                            association.Name);
                        if (navigationProperty.Direction == AssociationEndType.Subject)
                        {
                            Utilities.AddAttribute(associationEndElement, CoreResources.From,
                                association.SubjectRole);
                            Utilities.AddAttribute(associationEndElement, CoreResources.To,
                                association.ObjectRole);
                        }
                        else
                        {
                            Utilities.AddAttribute(associationEndElement, CoreResources.From,
                                association.ObjectRole);
                            Utilities.AddAttribute(associationEndElement, CoreResources.To,
                                association.SubjectRole);
                        }

                        XmlElement scalarPropertyElement = Utilities.CreateElement(associationEndElement,
                            CoreResources.ScalarProperty);
                        Utilities.AddAttribute(scalarPropertyElement, CoreResources.Name,
                            CoreResources.Id);
                        Utilities.AddAttribute(scalarPropertyElement, CoreResources.ParameterName,
                            navigationProperty.Name);

                        deleteFunctionElement.AppendChild(mslDocument.ImportNode(associationEndElement, true));

                        // Only UpdateFunction has the Version attribute = "Current".
                        associationEndElement = updateFunctionElement.AppendChild(
                            mslDocument.ImportNode(associationEndElement, true)) as XmlElement;
                        scalarPropertyElement = associationEndElement[CoreResources.ScalarProperty];
                        Utilities.AddAttribute(scalarPropertyElement, CoreResources.Version,
                            CoreResources.Current);
                    }
                }
                tempType = tempType.BaseType;
            }

            #endregion
        }

        #endregion
    }
}
