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
using System.Xml;
using System.Collections.Generic;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a scalar property of an asset type.
    /// </summary>
    public sealed class ScalarProperty : IResourceTypeProperty
    {
        #region Fields

        Guid id;
        ResourceType parent;
        string uri;
        string name;
        string description;
        DataTypes dataType;
        bool nullable;
        int maxLength;
        int scale;
        int precision;
        string tableName;
        string columnName;
        bool isFullTextIndexed;

        #endregion

        #region Properties

        /// <summary>
        /// Indicates whether the database column corresponding to this scalar property 
        /// is part of a full-text index.
        /// </summary>
        public bool IsFullTextIndexed
        {
            get { return isFullTextIndexed; }
            internal set { isFullTextIndexed = value; }
        }

        /// <summary>
        /// Gets the column name to which this property is mapped.
        /// </summary>
        public string ColumnName
        {
            get { return columnName; }
            internal set { columnName = value; }
        }

        /// <summary>
        /// Gets the table name to which this property is mapped.
        /// </summary>
        public string TableName
        {
            get { return tableName; }
            internal set { tableName = value; }
        }

        /// <summary>
        /// Gets the identifier of the scalar property.
        /// </summary>
        public Guid Id
        {
            get { return id; }
            internal set { id = value; }
        }

        /// <summary>
        /// Gets the resource type that hosts this property.
        /// </summary>
        public ResourceType Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        /// <summary>
        /// The maximum total number of decimal digits that can be stored, both to the left and to the right of the decimal point. 
        /// The precision must be a value from 1 through the maximum precision of 38. The default precision is 29.
        /// </summary>
        public int Precision
        {
            get { return precision; }
            set
            {
                precision = value;
            }
        }

        /// <summary>
        /// The maximum number of decimal digits that can be stored to the right of the decimal point. Scale must be a value from 0 through Precision upto a maximum of 26.
        /// </summary>
        public int Scale
        {
            get { return scale; }
            set
            {
                scale = value;
            }
        }

        /// <summary>
        /// The maximum length of String and Binary properties. Set the value to -1 for SQL nvarchar(max) or varbinary(max) columns.
        /// </summary>
        public int MaxLength
        {
            get
            {
                return maxLength;
            }
            set
            {
                maxLength = value;
            }
        }

        /// <summary>
        /// Whether the property is nullable.
        /// </summary>
        public bool Nullable
        {
            get { return nullable; }
            set
            {
                nullable = value;
            }
        }

        /// <summary>
        /// Gets or sets the data type of this scalar property.
        /// </summary>
        public DataTypes DataType
        {
            get { return dataType; }
            set
            {
                dataType = value;

                // Set the default values depending on data type.
                switch (dataType)
                {
                    case DataTypes.Binary:
                    case DataTypes.String:
                        maxLength = -1;
                        break;
                    case DataTypes.Decimal:
                        precision = 29;
                        scale = 0;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of this scalar property.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of this scalar property.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets or sets the uri of this scalar property.
        /// </summary>
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }

        /// <summary>
        /// Gets the full name of this scalar property.
        /// </summary>
        internal string FullName
        {
            get
            {
                if (this.Parent == null)
                    throw new ZentityException(CoreResources.ExceptionCannotLocateParentType);

                return Utilities.MergeSubNames(this.Parent.FullName, this.name);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new scalar property with a random name.
        /// </summary>
        public ScalarProperty()
            : this(null, DataTypes.Int32)
        {
        }

        /// <summary>
        /// Creates a new scalar property with the specified name.
        /// </summary>
        /// <param name="name">Name of the scalar property.</param>
        public ScalarProperty(string name)
            : this(name, DataTypes.Int32)
        {
        }

        /// <summary>
        /// Creates a new scalar property with the specified name and data type.
        /// </summary>
        /// <param name="name">Name of the scalar property.</param>
        /// <param name="dataType">Data type of the scalar property</param>
        public ScalarProperty(string name, DataTypes dataType)
        {
            this.id = Guid.NewGuid();
            this.name = name;
            this.DataType = dataType;
            this.nullable = true;
        }

        #endregion

        #region Helper methods.

        internal void Validate()
        {
            // Validate Id
            if (this.Id == Guid.Empty)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionPropertyEmpty,
                    CoreResources.Id, this.GetType().ToString()));

            // Validate Name.
            if (string.IsNullOrEmpty(this.Name))
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionStringPropertyNullOrEmpty,
                    CoreResources.Name, this.GetType().ToString()));

            if (this.Name.Length > MaxLengths.resourceTypePropertyName)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Name,
                    MaxLengths.resourceTypePropertyName));

            // Validating the Name for a valid C# property name makes the validation process slow.
            // So we are not including that check here. This check is included at the DataModel
            // level.

            // Validate DataType.
            switch (this.DataType)
            {
                case DataTypes.Binary:
                    if (this.MaxLength != -1 && this.MaxLength <= 0 ||
                        this.MaxLength > 0 && this.MaxLength > 8000)
                        throw new ModelItemValidationException(
                            string.Format(CultureInfo.InvariantCulture,
                            CoreResources.ExceptionInvalidMaxLength,
                            this.MaxLength, this.DataType, 8000));
                    break;
                case DataTypes.String:
                    if (this.MaxLength != -1 && this.MaxLength <= 0 ||
                        this.MaxLength > 0 && this.MaxLength > 4000)
                        throw new ModelItemValidationException(
                            string.Format(CultureInfo.InvariantCulture,
                            CoreResources.ExceptionInvalidMaxLength,
                            this.MaxLength, this.DataType, 4000));
                    break;
                case DataTypes.Decimal:
                    if (this.Precision < 1 || this.Precision > 38)
                        throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                            CoreResources.ExceptionInvalidPrecision,
                            this.Precision.ToString(CultureInfo.InvariantCulture)));
                    if (this.Scale < 0 || this.Scale > this.Precision || this.Scale > 26)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture, CoreResources.ExceptionInvalidScale,
                            this.Scale.ToString(CultureInfo.InvariantCulture)));
                    break;
            }

            // Validate Uri.
            if (!string.IsNullOrEmpty(this.Uri)
                && this.Uri.Length > MaxLengths.resourceTypePropertyUri)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Uri,
                    MaxLengths.resourceTypePropertyUri));

            // Validate Description.
            if (!string.IsNullOrEmpty(this.Description)
                && this.Description.Length > MaxLengths.resourceTypePropertyDescription)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Description,
                    MaxLengths.resourceTypePropertyDescription));
        }

        internal void UpdateSsdl(XmlDocument ssdlDocument, TableMappingCollection tableMappings)
        {
            // Locate the table and column mappings for this property.
            ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                this.Id);
            TableMapping tableMapping = columnMapping.Parent;

            string cachedTableName = tableMapping.TableName;
            string cachedColumnName = columnMapping.ColumnName;

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ssdlDocument.NameTable);
            nsMgr.AddNamespace(CoreResources.SSDLNamespacePrefix,
                CoreResources.SSDLSchemaNameSpace);

            // Locate the Schema element.
            XmlElement schemaElement = ssdlDocument.SelectSingleNode(
                CoreResources.XPathSSDLSchema, nsMgr) as XmlElement;
            string storageSchemaNamespace = schemaElement.Attributes[CoreResources.Namespace].
                Value;
            string tableFullName =
                Utilities.MergeSubNames(storageSchemaNamespace, cachedTableName);

            // Locate the EntityType for this table.
            XmlElement entityContainerElement = ssdlDocument.SelectSingleNode(
                CoreResources.XPathSSDLEntityContainer, nsMgr) as XmlElement;
            XmlNodeList entityTypes = schemaElement.SelectNodes(string.Format(
                CultureInfo.InvariantCulture, CoreResources.XPathSSDLEntityType, cachedTableName),
                nsMgr);
            XmlElement entityTypeElement = null;

            // If not found, create the EntitySet and the EntityType elements.
            // NOTE: We have added and FK from new table to Resource table. This is necessary 
            // so that EF inserts entries in the correct order across multiple tables. For 
            // example, let's say EntityA spans across 3 tables, Resource, Tab1 and Tab2. The 
            // entity has a one-to-many association with itself. The FK column resides in Tab2. 
            // Now let's say we create just one resource and associate it with itself. While 
            // entering the details for this resource, if EF inserts values into Tab2 before
            // Resource, the foreign key constraint might fail.
            if (entityTypes.Count == 0)
            {
                // Create EntitySet element.
                Utilities.AddSsdlEntitySetForTables(entityContainerElement, cachedTableName, tableFullName,
                    CoreResources.Core);

                // Create AssociationSet element.
                string fkConstraintName = string.Format(CultureInfo.InvariantCulture,
                    CoreResources.FKConstraintName, cachedTableName);
                string fkConstraintFullName =
                    Utilities.MergeSubNames(storageSchemaNamespace, fkConstraintName);
                Utilities.AddSsdlAssociationSet(entityContainerElement, fkConstraintName,
                    fkConstraintFullName, CoreResources.Resource, CoreResources.Resource,
                    cachedTableName, cachedTableName);

                // Create EntityType element
                entityTypeElement = Utilities.AddSsdlEntityType(schemaElement, cachedTableName,
                    CoreResources.Id);

                // Add Id and Discriminator properties.
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, CoreResources.Id,
                    CoreResources.DataTypeUniqueidentifier, false);
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement,
                    CoreResources.Discriminator, CoreResources.DataTypeInt, false);

                // Create Association element.
                Utilities.AddSsdlAssociation(schemaElement, fkConstraintName,
                    CoreResources.Resource,
                    Utilities.MergeSubNames(storageSchemaNamespace, CoreResources.Resource),
                    CoreResources.One, cachedTableName,
                    Utilities.MergeSubNames(storageSchemaNamespace, cachedTableName),
                    CoreResources.ZeroOrOne,
                    CoreResources.Resource, new string[] { CoreResources.Id }, cachedTableName,
                    new string[] { CoreResources.Id });
            }
            else
                entityTypeElement = entityTypes[0] as XmlElement;

            // Add Name, Type, Nullable and data type specific attributes.
            // NOTE: Nullable is always true in the SSDL. This is important for the TPH
            // type of mapping.
            if (this.DataType == DataTypes.String || this.DataType == DataTypes.Binary)
            {
                if (this.MaxLength < 0)
                    Utilities.AddSsdlEntityTypeProperty(entityTypeElement, cachedColumnName,
                        Utilities.GetSQLType(this.DataType) +
                        string.Format(CultureInfo.InvariantCulture, CoreResources.Paranthesis,
                        CoreResources.Max).ToLowerInvariant(),
                        true);
                else
                    Utilities.AddSsdlEntityTypeProperty(entityTypeElement, cachedColumnName,
                        Utilities.GetSQLType(this.DataType), true, this.MaxLength);
            }
            else if (this.DataType == DataTypes.Decimal)
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, cachedColumnName,
                    Utilities.GetSQLType(this.DataType), true, this.Precision, this.Scale);
            else
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, cachedColumnName,
                    Utilities.GetSQLType(this.DataType), true);
        }

        internal void UpdateCsdl(XmlElement eEntityType)
        {
            // Create Property element.
            XmlElement eProperty = Utilities.CreateElement(eEntityType, CoreResources.Property);

            // Add Name, Type and Nullable attributes to all properties.
            Utilities.AddAttribute(eProperty, CoreResources.Name, this.Name);
            Utilities.AddAttribute(eProperty, CoreResources.Type, this.DataType.ToString());
            Utilities.AddAttribute(eProperty, CoreResources.Nullable,
                this.Nullable.ToString().ToLowerInvariant());

            // DataType specific attributes.
            switch (this.DataType)
            {
                case DataTypes.String:
                    // Unicode.
                    Utilities.AddAttribute(eProperty, CoreResources.Unicode,
                        true.ToString().ToLowerInvariant());
                    goto case DataTypes.Binary;

                case DataTypes.Binary:
                    // MaxLength.
                    Utilities.AddAttribute(eProperty, CoreResources.MaxLength,
                        this.MaxLength == -1 ? CoreResources.Max :
                        this.MaxLength.ToString(CultureInfo.InvariantCulture));

                    // FixedLength.
                    Utilities.AddAttribute(eProperty, CoreResources.FixedLength,
                    false.ToString().ToLowerInvariant());
                    break;

                case DataTypes.Decimal:
                    // Precision.
                    Utilities.AddAttribute(eProperty, CoreResources.Precision,
                        this.Precision.ToString(CultureInfo.InvariantCulture));

                    // Scale.
                    Utilities.AddAttribute(eProperty, CoreResources.Scale,
                        this.Scale.ToString(CultureInfo.InvariantCulture));
                    break;
            }

            // Create Documentation element.
            if (!string.IsNullOrEmpty(this.Description))
            {
                XmlElement eDocumentation = Utilities.CreateElement(eProperty,
                    CoreResources.Documentation);
                XmlElement eSummary = Utilities.CreateElement(eDocumentation,
                    CoreResources.Summary);
                eSummary.InnerText = this.Description;
            }
        }

        internal void UpdateMsl(XmlElement entityTypeMapping, TableMappingCollection tableMappings, Dictionary<Guid, int> discriminators)
        {
            // Locate the table and column mappings for this property.
            ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(this.Id);
            TableMapping tableMapping = columnMapping.Parent;

            string cachedTableName = tableMapping.TableName;
            string cachedColumnName = columnMapping.ColumnName;

            // Locate or create <MappingFragment> element.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(
                entityTypeMapping.OwnerDocument.NameTable);
            nsMgr.AddNamespace(CoreResources.MSLNamespacePrefix, CoreResources.MSLSchemaNamespace);
            XmlNodeList mappingFragments = entityTypeMapping.SelectNodes(
                string.Format(CultureInfo.InvariantCulture, CoreResources.XPathMSLMappingFragment,
                cachedTableName), nsMgr);
            XmlElement mappingFragment = null;
            if (mappingFragments.Count == 0)
            {
                mappingFragment = Utilities.CreateElement(entityTypeMapping,
                    CoreResources.MappingFragment);
                Utilities.AddAttribute(mappingFragment, CoreResources.StoreEntitySet, cachedTableName);

                // Add mapping for the Id column.
                XmlElement idMapping = Utilities.CreateElement(mappingFragment,
                    CoreResources.ScalarProperty);
                Utilities.AddAttribute(idMapping, CoreResources.Name, CoreResources.Id);
                Utilities.AddAttribute(idMapping, CoreResources.ColumnName, CoreResources.Id);

                // Add Condition element.
                XmlElement conditionElement = Utilities.CreateElement(mappingFragment,
                    CoreResources.Condition);
                Utilities.AddAttribute(conditionElement, CoreResources.ColumnName,
                    CoreResources.Discriminator);
                Utilities.AddAttribute(conditionElement, CoreResources.Value,
                    discriminators[this.Parent.Id].ToString(CultureInfo.InvariantCulture).
                    ToLowerInvariant());
            }
            else
                mappingFragment = mappingFragments[0] as XmlElement;

            // Create <ScalarProperty> element.
            XmlElement scalarProperty = Utilities.CreateElement(mappingFragment,
                CoreResources.ScalarProperty);
            Utilities.AddAttribute(scalarProperty, CoreResources.Name, this.Name);
            Utilities.AddAttribute(scalarProperty, CoreResources.ColumnName, cachedColumnName);
        }

        #endregion
    }
}
