// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using System.Collections.Generic;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a scalar property of an asset type.
    /// </summary>
    [DataContract]
    public sealed class ScalarProperty : IResourceTypeProperty
    {
        #region Fields

        string columnName;
        DataTypes dataType;
        string description;
        Guid id;
        bool isFullTextIndexed;
        int maxLength;
        string name;
        bool nullable;
        ResourceType parent;
        int precision;
        int scale;
        string tableName;
        string uri;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the column name to which this property is mapped.
        /// </summary>
        public string ColumnName
        {
            get { return columnName; }
            internal set { columnName = value; }
        }

        /// <summary>
        /// Gets or sets the data type of this scalar property.
        /// </summary>
        [DataMember]
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
                    // precision of 30 allows for Decimal.Min and Decimal.Max to be used.
                    case DataTypes.Decimal:
                        precision = 30;
                        scale = 0;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the description of this scalar property.
        /// </summary>
        [DataMember]
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets the full name of this scalar property.
        /// </summary>
        [DataMember]
        internal string FullName
        {
            get
            {
                if (this.Parent == null)
                    throw new ZentityException(DataModellingResources.ExceptionCannotLocateParentType);

                return Utilities.MergeSubNames(this.Parent.FullName, this.name);
            }
            private set
            {
                // Required for Data Service Contract
            }
        }

        /// <summary>
        /// Gets the identifier of the scalar property.
        /// </summary>
        [DataMember]
        public Guid Id
        {
            get { return id; }
            internal set { id = value; }
        }

        /// <summary>
        /// Gets whether the database column corresponding to this scalar property 
        /// is part of a full-text index.
        /// </summary>
        [DataMember]
        public bool IsFullTextIndexed
        {
            get { return isFullTextIndexed; }
            internal set { isFullTextIndexed = value; }
        }

        /// <summary>
        /// Gets or sets the maximum length. The maximum length of String and Binary properties. 
        /// Set the value to -1 for SQL nvarchar(max) or varbinary(max) columns.
        /// </summary>
        [DataMember]
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
        /// Gets or sets the name of this scalar property.
        /// </summary>
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
        /// Gets or sets a value indicating whether this <see cref="ScalarProperty"/> is nullable.
        /// </summary>
        /// <value><c>true</c> if nullable; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool Nullable
        {
            get { return nullable; }
            set
            {
                nullable = value;
            }
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
        /// Gets or sets the precision. The maximum total number of decimal digits that can be stored, 
        /// both to the left and to the right of the decimal point. 
        /// The precision must be a value from 1 through the maximum precision of 38. 
        /// The default precision is 29.
        /// </summary>
        [DataMember]
        public int Precision
        {
            get { return precision; }
            set
            {
                precision = value;
            }
        }

        /// <summary>
        /// Gets or sets the scale. The maximum number of decimal digits that can be stored to the right of the decimal point. Scale must be a value from 0 through Precision upto a maximum of 26.
        /// </summary>
        /// <value>The scale.</value>
        [DataMember]
        public int Scale
        {
            get { return scale; }
            set
            {
                scale = value;
            }
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
        /// Gets or sets the uri of this scalar property.
        /// </summary>
        [DataMember]
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
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

        /// <summary>
        /// Validates this instance.
        /// </summary>
        internal void Validate()
        {
            // Validate Id
            if (this.Id == Guid.Empty)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionPropertyEmpty,
                    DataModellingResources.Id, this.GetType().ToString()));

            // Validate Name.
            if (string.IsNullOrEmpty(this.Name))
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionStringPropertyNullOrEmpty,
                    DataModellingResources.Name, this.GetType().ToString()));

            if (this.Name.Length > MaxLengths.ResourceTypePropertyName)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Name,
                    MaxLengths.ResourceTypePropertyName));

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
                            DataModellingResources.ExceptionInvalidMaxLength,
                            this.MaxLength, this.DataType, 8000));
                    break;
                case DataTypes.String:
                    if (this.MaxLength != -1 && this.MaxLength <= 0 ||
                        this.MaxLength > 0 && this.MaxLength > 4000)
                        throw new ModelItemValidationException(
                            string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.ExceptionInvalidMaxLength,
                            this.MaxLength, this.DataType, 4000));
                    break;
                case DataTypes.Decimal:
                    if (this.Precision < 1 || this.Precision > 38)
                        throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.ExceptionInvalidPrecision,
                            this.Precision.ToString(CultureInfo.InvariantCulture)));
                    if (this.Scale < 0 || this.Scale > this.Precision || this.Scale > 26)
                        throw new ModelItemValidationException(string.Format(
                            CultureInfo.InvariantCulture, DataModellingResources.ExceptionInvalidScale,
                            this.Scale.ToString(CultureInfo.InvariantCulture)));
                    break;
            }

            // Validate Uri.
            if (!string.IsNullOrEmpty(this.Uri)
                && this.Uri.Length > MaxLengths.ResourceTypePropertyUri)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Uri,
                    MaxLengths.ResourceTypePropertyUri));

            // Validate Description.
            if (!string.IsNullOrEmpty(this.Description)
                && this.Description.Length > MaxLengths.ResourceTypePropertyDescription)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Description,
                    MaxLengths.ResourceTypePropertyDescription));
        }

        /// <summary>
        /// Updates the CSDL.
        /// </summary>
        /// <param name="eEntityType">Type of the entity.</param>
        /// <param name="parentTypeName">Name of the parent type.</param>
        internal void UpdateCsdl(XmlElement eEntityType, string parentTypeName = "")
        {
            // Create Property element.
            XmlElement eProperty = Utilities.CreateElement(eEntityType, DataModellingResources.Property);

            // Add Name, ManagerType and Nullable attributes to all properties.
            string propertyName = this.Name.Equals(parentTypeName, StringComparison.OrdinalIgnoreCase)
                                      ? this.Name + DataModellingResources.Property
                                      : this.Name;
            Utilities.AddAttribute(eProperty, DataModellingResources.Name, propertyName);
            Utilities.AddAttribute(eProperty, DataModellingResources.Type, this.DataType.ToString());
            Utilities.AddAttribute(eProperty, DataModellingResources.Nullable, this.Nullable.ToString().ToLowerInvariant());

            // DataType specific attributes.
            switch (this.DataType)
            {
                case DataTypes.String:
                    // Unicode.
                    Utilities.AddAttribute(eProperty, DataModellingResources.Unicode,
                        true.ToString().ToLowerInvariant());
                    goto case DataTypes.Binary;

                case DataTypes.Binary:
                    // MaxLength.
                    Utilities.AddAttribute(eProperty, DataModellingResources.MaxLength,
                        this.MaxLength == -1 ? DataModellingResources.Max :
                        this.MaxLength.ToString(CultureInfo.InvariantCulture));

                    // FixedLength.
                    Utilities.AddAttribute(eProperty, DataModellingResources.FixedLength,
                    false.ToString().ToLowerInvariant());
                    break;

                case DataTypes.Decimal:
                    // Precision.
                    Utilities.AddAttribute(eProperty, DataModellingResources.Precision,
                        this.Precision.ToString(CultureInfo.InvariantCulture));

                    // Scale.
                    Utilities.AddAttribute(eProperty, DataModellingResources.Scale,
                        this.Scale.ToString(CultureInfo.InvariantCulture));
                    break;
            }

            // Create Documentation element.
            if (!string.IsNullOrEmpty(this.Description))
            {
                XmlElement eDocumentation = Utilities.CreateElement(eProperty,
                    DataModellingResources.Documentation);
                XmlElement eSummary = Utilities.CreateElement(eDocumentation,
                    DataModellingResources.Summary);
                eSummary.InnerText = this.Description;
            }
        }

        /// <summary>
        /// Updates the MSL.
        /// </summary>
        /// <param name="entityTypeMapping">The entity type mapping.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
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
            nsMgr.AddNamespace(DataModellingResources.MSLNamespacePrefix, DataModellingResources.MSLSchemaNamespace);
            XmlNodeList mappingFragments = entityTypeMapping.SelectNodes(
                string.Format(CultureInfo.InvariantCulture, DataModellingResources.XPathMSLMappingFragment,
                cachedTableName), nsMgr);
            XmlElement mappingFragment = null;
            if (mappingFragments.Count == 0)
            {
                mappingFragment = Utilities.CreateElement(entityTypeMapping,
                    DataModellingResources.MappingFragment);
                Utilities.AddAttribute(mappingFragment, DataModellingResources.StoreEntitySet, cachedTableName);

                // Add mapping for the Id column.
                XmlElement idMapping = Utilities.CreateElement(mappingFragment,
                    DataModellingResources.ScalarProperty);
                Utilities.AddAttribute(idMapping, DataModellingResources.Name, DataModellingResources.Id);
                Utilities.AddAttribute(idMapping, DataModellingResources.ColumnName, DataModellingResources.Id);

                // Add Condition element.
                XmlElement conditionElement = Utilities.CreateElement(mappingFragment,
                    DataModellingResources.Condition);
                Utilities.AddAttribute(conditionElement, DataModellingResources.ColumnName,
                    DataModellingResources.Discriminator);
                Utilities.AddAttribute(conditionElement, DataModellingResources.Value,
                    discriminators[this.Parent.Id].ToString(CultureInfo.InvariantCulture).
                    ToLowerInvariant());
            }
            else
                mappingFragment = mappingFragments[0] as XmlElement;

            // Create <ScalarProperty> element.
            XmlElement scalarProperty = Utilities.CreateElement(mappingFragment,
                DataModellingResources.ScalarProperty);
            Utilities.AddAttribute(scalarProperty, DataModellingResources.Name, this.Name);
            Utilities.AddAttribute(scalarProperty, DataModellingResources.ColumnName, cachedColumnName);
        }

        /// <summary>
        /// Updates the flattened MSL.
        /// </summary>
        /// <param name="mappingFragment">The mapping fragment.</param>
        /// <param name="parentTypeName">Name of the parent type.</param>
        internal void UpdateFlattenedMsl(XmlElement mappingFragment, string parentTypeName)
        {
            // Create <ScalarProperty> element.
            string propertyName = this.Name.Equals(parentTypeName, StringComparison.OrdinalIgnoreCase)
                                      ? this.Name + DataModellingResources.Property
                                      : this.Name;
            XmlElement scalarProperty = Utilities.CreateElement(mappingFragment, DataModellingResources.ScalarProperty);
            Utilities.AddAttribute(scalarProperty, DataModellingResources.Name, propertyName);
            Utilities.AddAttribute(scalarProperty, DataModellingResources.ColumnName, this.Name);
        }

        /// <summary>
        /// Updates the SSDL.
        /// </summary>
        /// <param name="ssdlDocument">The SSDL document.</param>
        /// <param name="tableMappings">The table mappings.</param>
        internal void UpdateSsdl(XmlDocument ssdlDocument, TableMappingCollection tableMappings)
        {
            // Locate the table and column mappings for this property.
            ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                this.Id);
            TableMapping tableMapping = columnMapping.Parent;

            string cachedTableName = tableMapping.TableName;
            string cachedColumnName = columnMapping.ColumnName;

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ssdlDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.SSDLNamespacePrefix,
                DataModellingResources.SSDLSchemaNameSpace);

            // Locate the Schema element.
            XmlElement schemaElement = ssdlDocument.SelectSingleNode(
                DataModellingResources.XPathSSDLSchema, nsMgr) as XmlElement;
            string storageSchemaNamespace = schemaElement.Attributes[DataModellingResources.Namespace].
                Value;
            string tableFullName =
                Utilities.MergeSubNames(storageSchemaNamespace, cachedTableName);

            // Locate the EntityType for this table.
            XmlElement entityContainerElement = ssdlDocument.SelectSingleNode(
                DataModellingResources.XPathSSDLEntityContainer, nsMgr) as XmlElement;
            XmlNodeList entityTypes = schemaElement.SelectNodes(string.Format(
                CultureInfo.InvariantCulture, DataModellingResources.XPathSSDLEntityType, cachedTableName),
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
                    DataModellingResources.Core);

                // Create AssociationSet element.
                string fkConstraintName = string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.FKConstraintName, cachedTableName);
                string fkConstraintFullName =
                    Utilities.MergeSubNames(storageSchemaNamespace, fkConstraintName);
                Utilities.AddSsdlAssociationSet(entityContainerElement, fkConstraintName,
                    fkConstraintFullName, DataModellingResources.Resource, DataModellingResources.Resource,
                    cachedTableName, cachedTableName);

                // Create EntityType element
                entityTypeElement = Utilities.AddSsdlEntityType(schemaElement, cachedTableName,
                    DataModellingResources.Id);

                // Add Id and Discriminator properties.
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, DataModellingResources.Id,
                    DataModellingResources.DataTypeUniqueidentifier, false);
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement,
                    DataModellingResources.Discriminator, DataModellingResources.DataTypeInt, false);

                // Create Association element.
                Utilities.AddSsdlAssociation(schemaElement, fkConstraintName,
                    DataModellingResources.Resource,
                    Utilities.MergeSubNames(storageSchemaNamespace, DataModellingResources.Resource),
                    DataModellingResources.One, cachedTableName,
                    Utilities.MergeSubNames(storageSchemaNamespace, cachedTableName),
                    DataModellingResources.ZeroOrOne,
                    DataModellingResources.Resource, new string[] { DataModellingResources.Id }, cachedTableName,
                    new string[] { DataModellingResources.Id });
            }
            else
                entityTypeElement = entityTypes[0] as XmlElement;

            // Add Name, ManagerType, Nullable and data type specific attributes.
            // NOTE: Nullable is always true in the SSDL. This is important for the TPH
            // type of mapping.
            if (this.DataType == DataTypes.String || this.DataType == DataTypes.Binary)
            {
                if (this.MaxLength < 0)
                    Utilities.AddSsdlEntityTypeProperty(entityTypeElement, cachedColumnName,
                        Utilities.GetSQLType(this.DataType) +
                        string.Format(CultureInfo.InvariantCulture, DataModellingResources.Paranthesis,
                        DataModellingResources.Max).ToLowerInvariant(),
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

        #endregion
    }
}
