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
using System.Xml;
using System.Resources;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Zentity.Core
{
    /// <summary>
    /// Represents an Association in the data model.
    /// </summary>
    /// <example>
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///
    ///namespace CodeSamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Console.WriteLine(&quot;Associations in the model:&quot;);
    ///                foreach (Association assoc in context.DataModel.Modules.
    ///                    SelectMany(module =&gt; module.Associations))
    ///                {
    ///                    Console.WriteLine(&quot;\t{0}&quot;, assoc.Name);
    ///                    Console.WriteLine(&quot;\t\tSubject Resource Type:Subject Navigation Property - {0}:{1}&quot;,
    ///                        assoc.SubjectNavigationProperty.Parent.Name, assoc.SubjectNavigationProperty.Name);
    ///                    Console.WriteLine(&quot;\t\tObject Resource Type:Object Navigation Property - {0}:{1}&quot;,
    ///                        assoc.ObjectNavigationProperty.Parent.Name, assoc.ObjectNavigationProperty.Name);
    ///                    Console.WriteLine(&quot;\t\tCardinality - {0}-To-{1}&quot;, assoc.SubjectMultiplicity,
    ///                        assoc.ObjectMultiplicity);
    ///                    Console.WriteLine(&quot;\t\tPredicateId - {0}&quot;, assoc.PredicateId);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public sealed class Association
    {
        #region Fields

        Guid id;
        string name;
        string uri;
        NavigationProperty subjectNavigationProperty;
        NavigationProperty objectNavigationProperty;
        Guid predicateId;
        AssociationEndMultiplicity subjectMultiplicity;
        AssociationEndMultiplicity objectMultiplicity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the identifier of association.
        /// </summary>
        public Guid Id
        {
            get { return id; }
            internal set { id = value; }
        }

        /// <summary>
        /// Gets or sets the association name.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the subject navigation property.
        /// </summary>
        public NavigationProperty SubjectNavigationProperty
        {
            get { return subjectNavigationProperty; }
            set
            {
                // Check whether the new subject is available.
                if (value != null && value.Association != null)
                {
                    throw new InvalidPropertyValueException(
                        CoreResources.ExceptionUnavailableNavProperty);
                }

                // Detach this association from its previous subject.
                if (subjectNavigationProperty != null)
                {
                    subjectNavigationProperty.Association = null;
                    SubjectNavigationProperty.Direction = AssociationEndType.Undefined;
                }

                // Attach to new or null subject.
                subjectNavigationProperty = value;

                // If attached to a non-null subject, set the Association property of the subject.
                if (subjectNavigationProperty != null)
                {
                    subjectNavigationProperty.Association = this;
                    SubjectNavigationProperty.Direction = AssociationEndType.Subject;
                }
            }
        }

        /// <summary>
        /// Gets or sets an object navigation property.
        /// </summary>
        public NavigationProperty ObjectNavigationProperty
        {
            get { return objectNavigationProperty; }
            set
            {
                // Check whether the new object is available.
                if (value != null && value.Association != null)
                {
                    throw new InvalidPropertyValueException(
                        CoreResources.ExceptionUnavailableNavProperty);
                }

                // Detach this association from its previous object.
                if (objectNavigationProperty != null)
                {
                    objectNavigationProperty.Association = null;
                    objectNavigationProperty.Direction = AssociationEndType.Undefined;
                }

                // Attach to new or null object.
                objectNavigationProperty = value;

                // If attached to a non-null object, set the Association property of the object.
                if (objectNavigationProperty != null)
                {
                    objectNavigationProperty.Association = this;
                    objectNavigationProperty.Direction = AssociationEndType.Object;
                }
            }
        }

        /// <summary>
        /// Gets or sets the multiplicity of the subject end of the association.
        /// </summary>
        public AssociationEndMultiplicity SubjectMultiplicity
        {
            get { return subjectMultiplicity; }
            set { subjectMultiplicity = value; }
        }

        /// <summary>
        /// Gets or sets the multiplicity of the object end of the association.
        /// </summary>
        public AssociationEndMultiplicity ObjectMultiplicity
        {
            get { return objectMultiplicity; }
            set { objectMultiplicity = value; }
        }

        /// <summary>
        /// Gets the parent DataModelModule for this association. Returns null if the parent 
        /// cannot be evaluated or is ambiguous. An association is considered to be in the module 
        /// if both its object and subject navigation properties are present in the same module. 
        /// </summary>
        public DataModelModule Parent
        {
            get
            {
                DataModelModule subjectModule = null;
                DataModelModule objectModule = null;

                if (this.SubjectNavigationProperty != null &&
                    this.SubjectNavigationProperty.Parent != null &&
                    this.SubjectNavigationProperty.Parent.Parent != null)
                    subjectModule = this.SubjectNavigationProperty.Parent.Parent;

                if (this.ObjectNavigationProperty != null &&
                    this.ObjectNavigationProperty.Parent != null &&
                    this.ObjectNavigationProperty.Parent.Parent != null)
                    objectModule = this.ObjectNavigationProperty.Parent.Parent;

                if (subjectModule != null && objectModule != null && subjectModule == objectModule)
                    return subjectModule;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the identifier of the predicate that defines this association. Predicates cannot
        /// be shared between associations.
        /// </summary>
        public Guid PredicateId
        {
            get { return predicateId; }
            internal set { predicateId = value; }
        }

        /// <summary>
        /// Gets or sets the Uri of this association.
        /// </summary>
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }

        /// <summary>
        /// Gets namespace qualified name of this association. Returns null if the Parent property
        /// of this association cannot be evaluated.
        /// </summary>
        internal string FullName
        {
            get
            {
                return (this.Parent == null) ? null :
                    Utilities.MergeSubNames(this.Parent.NameSpace, this.Name);
            }
        }

        /// <summary>
        /// Gets the role of the subject entity in the association. The property raises exceptions
        /// if the subject or object resource types cannot be reached via the navigation 
        /// properties.
        /// </summary>
        internal string SubjectRole
        {
            get
            {
                ResourceType subjectResourceType = this.SubjectNavigationProperty.Parent;
                ResourceType objectResourceType = this.ObjectNavigationProperty.Parent;

                if (subjectResourceType == objectResourceType)
                    return subjectResourceType.Name + "1";
                else
                    return subjectResourceType.Name;
            }
        }

        /// <summary>
        /// Gets the role of the object entity in the association. The property raises exceptions
        /// if the subject or object resource types cannot be reached via the navigation 
        /// properties.
        /// </summary>
        internal string ObjectRole
        {
            get
            {
                ResourceType subjectResourceType = this.SubjectNavigationProperty.Parent;
                ResourceType objectResourceType = this.ObjectNavigationProperty.Parent;

                if (subjectResourceType == objectResourceType)
                    return subjectResourceType.Name + "2";
                else
                    return objectResourceType.Name;
            }
        }

        internal string ViewName
        {
            get { return this.Id.ToString("N").ToLowerInvariant(); }
        }

        internal string InsertProcedureName
        {
            get { return "Insert" + ViewName; }
        }

        internal string DeleteProcedureName
        {
            get { return "Delete" + ViewName; }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Zentity.Core.Association"/> class.
        /// </summary>
        public Association()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Zentity.Core.Association"/> class.
        /// </summary>
        /// <param name="name">The association name to use.</param>
        public Association(string name)
        {
            this.id = Guid.NewGuid();
            this.predicateId = Guid.NewGuid();
            this.name = name;
            this.subjectMultiplicity = AssociationEndMultiplicity.Many;
            this.objectMultiplicity = AssociationEndMultiplicity.Many;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validates an association. This method assumes that the graph validations are already
        /// done. Thus it does not validate conditions like the association ends being in 
        /// different modules, association navigation properties are null reference, or the
        /// navigation properties are dangling nodes in the graph etc.
        /// </summary>
        internal void Validate()
        {
            // Validate Id.
            if (this.Id == Guid.Empty)
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionPropertyEmpty,
                    CoreResources.Id, this.GetType().FullName));

            // Validate Name.
            if (string.IsNullOrEmpty(this.Name))
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionStringPropertyNullOrEmpty,
                    CoreResources.Name, this.GetType().FullName));

            if (this.Name.Length > MaxLengths.associationName)
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Name,
                    MaxLengths.associationName));

            // Validate PredicateId.
            if (this.PredicateId == Guid.Empty)
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionPropertyEmpty,
                    CoreResources.PredicateId, this.GetType().FullName));

            // Validate Multiplicities.
            if (subjectMultiplicity == AssociationEndMultiplicity.One &&
                objectMultiplicity == AssociationEndMultiplicity.One)
                throw new NotSupportedException(CoreResources.InvalidMultiplicityOneToOne);

            // Validate Uri.
            if (!string.IsNullOrEmpty(this.Uri) && this.Uri.Length > MaxLengths.associationUri)
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Uri,
                    MaxLengths.associationUri));
        }

        internal void UpdateSsdl(XmlDocument ssdlDocument, TableMappingCollection tableMappings)
        {
            // Pick the Schema and EntityContainer elements.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ssdlDocument.NameTable);
            nsMgr.AddNamespace(CoreResources.SSDLNamespacePrefix,
                CoreResources.SSDLSchemaNameSpace);

            XmlElement schemaElement = ssdlDocument.SelectSingleNode(
                CoreResources.XPathSSDLSchema, nsMgr) as XmlElement;

            XmlElement entityContainerElement = ssdlDocument.SelectSingleNode(
                CoreResources.XPathSSDLEntityContainer, nsMgr) as XmlElement;

            // Parameters for View associations 
            // (ManyToMany, ManyToZeroOrOne, ZeroOrOneToMany, ZeroOrOneToZeroOrOne).
            string viewName = this.ViewName;

            string subjectFKConstraintName = string.Format(CultureInfo.InvariantCulture,
                CoreResources.FKConstraintName, this.SubjectNavigationProperty.Id.ToString());
            string objectFKConstraintName = string.Format(CultureInfo.InvariantCulture,
                CoreResources.FKConstraintName, this.ObjectNavigationProperty.Id.ToString());


            // One to One.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
            {
                throw new ZentityException(CoreResources.InvalidMultiplicityOneToOne);
            }

            // Many to Many.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many)
            {
                UpdateSsdlHandleViewAssociations(entityContainerElement, schemaElement, viewName,
                    subjectFKConstraintName, objectFKConstraintName,
                    new string[] { CoreResources.SubjectResourceId, 
                        CoreResources.ObjectResourceId }, CoreResources.One, CoreResources.Many,
                        CoreResources.One, CoreResources.Many, InsertProcedureName,
                        DeleteProcedureName);
            }

            // Many to One or ZeroOrOne to One.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
            {
                UpdateSsdlHandleOneToXxxAssociations(entityContainerElement, schemaElement,
                    this.SubjectNavigationProperty.Id, tableMappings);
            }

            // Many to ZeroOrOne OR ZeroOrOne to ZeroOrOne.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
            {
                UpdateSsdlHandleViewAssociations(entityContainerElement, schemaElement, viewName,
                    subjectFKConstraintName, objectFKConstraintName,
                    new string[] { CoreResources.SubjectResourceId }, CoreResources.One,
                        CoreResources.ZeroOrOne, CoreResources.One, CoreResources.Many,
                        InsertProcedureName, DeleteProcedureName);
            }

            // One to Many or One to ZeroOrOne.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
            {
                UpdateSsdlHandleOneToXxxAssociations(entityContainerElement, schemaElement,
                    this.ObjectNavigationProperty.Id, tableMappings);
            }

            // ZeroOrOne to Many.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many)
            {
                UpdateSsdlHandleViewAssociations(entityContainerElement, schemaElement, viewName,
                    subjectFKConstraintName, objectFKConstraintName,
                    new string[] { CoreResources.ObjectResourceId }, CoreResources.One,
                        CoreResources.Many, CoreResources.One, CoreResources.ZeroOrOne,
                        InsertProcedureName, DeleteProcedureName);
            }
        }

        internal void UpdateCsdl(XmlDocument coreCsdl, XmlDocument moduleCsdl)
        {
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
                throw new ZentityException(CoreResources.InvalidMultiplicityOneToOne);

            // Add AssociationSet element in Core CSDL.
            XmlNamespaceManager coreCsdlNsMgr = new XmlNamespaceManager(coreCsdl.NameTable);
            coreCsdlNsMgr.AddNamespace(CoreResources.CSDLNamespacePrefix,
                CoreResources.CSDLSchemaNameSpace);

            XmlElement entityContainerElement = coreCsdl.SelectSingleNode(
                CoreResources.XPathCSDLEntityContainer, coreCsdlNsMgr) as XmlElement;

            XmlElement associationSetElement = Utilities.CreateElement(entityContainerElement,
                CoreResources.AssociationSet);
            Utilities.AddAttribute(associationSetElement, CoreResources.Name, this.Name);
            Utilities.AddAttribute(associationSetElement, CoreResources.Association,
                this.FullName);

            XmlElement associationSetEnd1 = Utilities.CreateElement(associationSetElement,
                CoreResources.End);
            Utilities.AddAttribute(associationSetEnd1, CoreResources.Role, SubjectRole);
            Utilities.AddAttribute(associationSetEnd1, CoreResources.EntitySet,
                CoreResources.Resources);

            XmlElement associationSetEnd2 = Utilities.CreateElement(associationSetElement,
                CoreResources.End);
            Utilities.AddAttribute(associationSetEnd2, CoreResources.Role, ObjectRole);
            Utilities.AddAttribute(associationSetEnd2, CoreResources.EntitySet,
                CoreResources.Resources);

            // Add FunctionImport elements for ViewAssociations.
            if (this.SubjectMultiplicity != AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity != AssociationEndMultiplicity.One)
            {
                Utilities.AddCsdlFunctionImport(entityContainerElement,
                    InsertProcedureName,
                    new string[] { CoreResources.SubjectResourceId, DataTypes.Guid.ToString(), 
                        CoreResources.In, CoreResources.ObjectResourceId, 
                        DataTypes.Guid.ToString(), CoreResources.In });

                Utilities.AddCsdlFunctionImport(entityContainerElement,
                    DeleteProcedureName,
                    new string[] { CoreResources.SubjectResourceId, DataTypes.Guid.ToString(), 
                        CoreResources.In, CoreResources.ObjectResourceId, 
                        DataTypes.Guid.ToString(), CoreResources.In });
            }

            // Add Association element in Module CSDL.
            XmlNamespaceManager moduleCsdlNsMgr = new XmlNamespaceManager(moduleCsdl.NameTable);
            moduleCsdlNsMgr.AddNamespace(CoreResources.CSDLNamespacePrefix,
                CoreResources.CSDLSchemaNameSpace);

            XmlElement schemaElement = moduleCsdl.SelectSingleNode(
                CoreResources.XPathCSDLSchema, moduleCsdlNsMgr) as XmlElement;

            XmlElement associationElement = Utilities.CreateElement(schemaElement,
                CoreResources.Association);
            Utilities.AddAttribute(associationElement, CoreResources.Name, this.Name);

            XmlElement associationEnd1 = Utilities.CreateElement(associationElement,
                CoreResources.End);
            Utilities.AddAttribute(associationEnd1, CoreResources.Role, SubjectRole);
            Utilities.AddAttribute(associationEnd1, CoreResources.Type,
                SubjectNavigationProperty.Parent.FullName);
            Utilities.AddAttribute(associationEnd1, CoreResources.Multiplicity,
                Utilities.GetStringMultiplicityFromEnum(SubjectMultiplicity));

            XmlElement associationEnd2 = Utilities.CreateElement(associationElement,
                CoreResources.End);
            Utilities.AddAttribute(associationEnd2, CoreResources.Role, ObjectRole);
            Utilities.AddAttribute(associationEnd2, CoreResources.Type,
                ObjectNavigationProperty.Parent.FullName);
            Utilities.AddAttribute(associationEnd2, CoreResources.Multiplicity,
                Utilities.GetStringMultiplicityFromEnum(ObjectMultiplicity));
        }

        internal void UpdateMsl(XmlDocument mslDocument, TableMappingCollection tableMappings, string storageSchemaName)
        {
            // Locate the EntityContainer element.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(mslDocument.NameTable);
            nsMgr.AddNamespace(CoreResources.MSLNamespacePrefix, CoreResources.MSLSchemaNamespace);

            XmlElement entityContainerMappingElement = mslDocument.SelectSingleNode(
                CoreResources.XPathMSLEntityContainerMapping, nsMgr) as XmlElement;

            // We don't support One to One associations.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
                throw new ZentityException(CoreResources.InvalidMultiplicityOneToOne);

            // Handle view mappings.
            else if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
            {
                // Create FunctionImportMapping elements.
                string createFunctionFullName = Utilities.MergeSubNames(
                    storageSchemaName, InsertProcedureName);
                string deleteFunctionFullName = Utilities.MergeSubNames(
                    storageSchemaName, DeleteProcedureName);

                Utilities.AddMslFunctionImportMapping(entityContainerMappingElement,
                    InsertProcedureName, createFunctionFullName);

                Utilities.AddMslFunctionImportMapping(entityContainerMappingElement,
                    DeleteProcedureName, deleteFunctionFullName);

                // Create AssociationSetMapping element.
                UpdateMslCreateAssociationSetMappingElement(entityContainerMappingElement, this.Name,
                    this.FullName, this.ViewName, this.SubjectRole, CoreResources.Id,
                    CoreResources.SubjectResourceId, this.ObjectRole, CoreResources.Id,
                    CoreResources.ObjectResourceId, createFunctionFullName, deleteFunctionFullName,
                    CoreResources.SubjectResourceId, CoreResources.ObjectResourceId);
            }

            else
            {
                ColumnMapping columnMapping = null;
                XmlElement associationSetMappingElement = null;

                // Handle OneToXXX mappings except One to One.
                if (this.SubjectMultiplicity == AssociationEndMultiplicity.One)
                {
                    columnMapping = tableMappings.GetColumnMappingByPropertyId(
                        this.ObjectNavigationProperty.Id);

                    associationSetMappingElement = UpdateMslCreateAssociationSetMappingElement(
                       entityContainerMappingElement, this.Name, this.FullName,
                       columnMapping.Parent.TableName, this.SubjectRole, CoreResources.Id,
                       columnMapping.ColumnName, this.ObjectRole, CoreResources.Id,
                       CoreResources.Id);
                }
                // Handle XXXToOne mappings except One to One.
                else
                {
                    columnMapping = tableMappings.GetColumnMappingByPropertyId(
                        this.SubjectNavigationProperty.Id);

                    associationSetMappingElement = UpdateMslCreateAssociationSetMappingElement(
                        entityContainerMappingElement, this.Name, this.FullName,
                        columnMapping.Parent.TableName, this.SubjectRole, CoreResources.Id,
                        CoreResources.Id, this.ObjectRole, CoreResources.Id,
                        columnMapping.ColumnName);
                }

                // Add the NOT NULL Condition element for foreign key column.
                XmlElement conditionElement = Utilities.CreateElement(associationSetMappingElement,
                    CoreResources.Condition);
                Utilities.AddAttribute(conditionElement, CoreResources.ColumnName,
                    columnMapping.ColumnName);
                Utilities.AddAttribute(conditionElement, CoreResources.IsNull,
                    false.ToString().ToLowerInvariant());
            }
        }

        private static void UpdateSsdlHandleOneToXxxAssociations(XmlElement entityContainerElement, XmlElement schemaElement, Guid xxxSidePropertyId, TableMappingCollection tableMappings)
        {
            // Locate the table and column mappings for this property.
            ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                xxxSidePropertyId);
            TableMapping tableMapping = columnMapping.Parent;

            string tableName = tableMapping.TableName;
            string columnName = columnMapping.ColumnName;
            string storageSchemaNamespace = schemaElement.Attributes[CoreResources.Namespace].
                Value;
            string tableFullName = Utilities.MergeSubNames(
                storageSchemaNamespace, tableName);

            // Locate the EntityType for this table.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(
                entityContainerElement.OwnerDocument.NameTable);
            nsMgr.AddNamespace(CoreResources.SSDLNamespacePrefix,
                CoreResources.SSDLSchemaNameSpace);
            XmlNodeList entityTypes = schemaElement.SelectNodes(string.Format(
                CultureInfo.InvariantCulture, CoreResources.XPathSSDLEntityType, tableName),
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
                Utilities.AddSsdlEntitySetForTables(entityContainerElement, tableName, tableFullName,
                    CoreResources.Core);

                // Create AssociationSet element.
                string fkConstraintName = string.Format(CultureInfo.InvariantCulture,
                    CoreResources.FKConstraintName, tableName);
                string fkConstraintFullName = Utilities.MergeSubNames(
                    storageSchemaNamespace, fkConstraintName);
                Utilities.AddSsdlAssociationSet(entityContainerElement, fkConstraintName,
                    fkConstraintFullName, CoreResources.Resource, CoreResources.Resource,
                    tableName, tableName);

                // Create EntityType element
                entityTypeElement = Utilities.AddSsdlEntityType(schemaElement, tableName,
                    CoreResources.Id);

                // Add Id and Discriminator properties.
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, CoreResources.Id,
                    CoreResources.DataTypeUniqueidentifier, false);
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement,
                    CoreResources.Discriminator, CoreResources.DataTypeInt, false);

                // Create Association element.
                Utilities.AddSsdlAssociation(schemaElement, fkConstraintName,
                    CoreResources.Resource, Utilities.MergeSubNames(
                    storageSchemaNamespace, CoreResources.Resource),
                    CoreResources.One, tableName, tableFullName, CoreResources.ZeroOrOne,
                    CoreResources.Resource, new string[] { CoreResources.Id }, tableName,
                    new string[] { CoreResources.Id });
            }
            else
                entityTypeElement = entityTypes[0] as XmlElement;

            // Add the foreign key column to EntityType element. Take note that all
            // foreign key columns are nullable. This is necessary since the table
            // might also host rows for some other entity type and while inserting
            // rows for the other entity type, EF inserts NULL values for this entity
            // type.
            Utilities.AddSsdlEntityTypeProperty(entityTypeElement, columnName,
                CoreResources.DataTypeUniqueidentifier, true);

            // Add AssociationSet element for the foreign key. It is possible that the FK
            // column is hosted by Resource table. To create different roles, we use the
            // column name to distinguish between the roles.
            string associationFKConstraintName = string.Format(CultureInfo.InvariantCulture,
                CoreResources.FKConstraintName, columnName);
            string associationFKConstraintFullName = Utilities.MergeSubNames(
                storageSchemaNamespace, associationFKConstraintName);
            Utilities.AddSsdlAssociationSet(entityContainerElement, associationFKConstraintName,
                associationFKConstraintFullName, CoreResources.Resource, CoreResources.Resource,
                columnName, tableName);

            // Add Association element.
            Utilities.AddSsdlAssociation(schemaElement, associationFKConstraintName,
                CoreResources.Resource, Utilities.MergeSubNames(
                storageSchemaNamespace, CoreResources.Resource),
                CoreResources.ZeroOrOne, columnName, tableFullName, CoreResources.Many,
                CoreResources.Resource, new string[] { CoreResources.Id }, columnName,
                new string[] { columnName });
        }

        private static void UpdateSsdlHandleViewAssociations(XmlElement entityContainerElement, XmlElement schemaElement,
            string viewName, string subjectFKConstraintName, string objectFKConstraintName, string[] viewKeyColumns,
            string subjectSideResourceMultiplicity, string subjectSideViewMultiplicity, string objectSideResourceMultiplicity,
            string objectSideViewMultiplicity, string insertProcedureName, string deleteProcedureName)
        {
            string storageNamespace = schemaElement.Attributes[CoreResources.Namespace].Value;
            string viewFullName = Utilities.MergeSubNames(
                storageNamespace, viewName);
            string subjectFKConstraintFullName = Utilities.MergeSubNames(
                storageNamespace, subjectFKConstraintName);
            string objectFKConstraintFullName = Utilities.MergeSubNames(
                storageNamespace, objectFKConstraintName);
            string resourceTableFullName = Utilities.MergeSubNames(
                storageNamespace, CoreResources.Resource);

            // Create EntitySet element.
            Utilities.AddSsdlEntitySetForViews(entityContainerElement, viewName, viewFullName,
                CoreResources.Core, viewName);

            // Create AssociationSet element for subject foreign key.
            Utilities.AddSsdlAssociationSet(entityContainerElement, subjectFKConstraintName,
                subjectFKConstraintFullName, viewName, viewName, CoreResources.Resource,
                CoreResources.Resource);

            // Create AssociationSet element for object foreign key.
            Utilities.AddSsdlAssociationSet(entityContainerElement, objectFKConstraintName,
                objectFKConstraintFullName, viewName, viewName, CoreResources.Resource,
                CoreResources.Resource);

            // Create EntityType element with PropertyRef element.
            XmlElement entityTypeElement = Utilities.AddSsdlEntityType(schemaElement, viewName,
                viewKeyColumns);

            // Add Property elements to EntityType element.
            Utilities.AddSsdlEntityTypeProperty(entityTypeElement,
                CoreResources.SubjectResourceId, Utilities.GetSQLType(DataTypes.Guid), false);
            Utilities.AddSsdlEntityTypeProperty(entityTypeElement,
                CoreResources.ObjectResourceId, Utilities.GetSQLType(DataTypes.Guid), false);

            // Create Association element for subject foreign key.
            Utilities.AddSsdlAssociation(schemaElement, subjectFKConstraintName, viewName,
                viewFullName, subjectSideViewMultiplicity, CoreResources.Resource,
                resourceTableFullName, subjectSideResourceMultiplicity, CoreResources.Resource,
                new string[] { CoreResources.Id }, viewName,
                new string[] { CoreResources.SubjectResourceId });

            // Create Association element for object foreign key.
            Utilities.AddSsdlAssociation(schemaElement, objectFKConstraintName, viewName,
                viewFullName, objectSideViewMultiplicity, CoreResources.Resource,
                resourceTableFullName, objectSideResourceMultiplicity, CoreResources.Resource,
                new string[] { CoreResources.Id }, viewName,
                new string[] { CoreResources.ObjectResourceId });

            // Add Create Function.
            Utilities.AddSsdlFunction(schemaElement, insertProcedureName, false, false, false,
                false, CoreResources.AllowImplicitConversion, CoreResources.Core,
                new string[] { CoreResources.SubjectResourceId, 
                    CoreResources.DataTypeUniqueidentifier, CoreResources.In, 
                    CoreResources.ObjectResourceId, 
                    CoreResources.DataTypeUniqueidentifier, CoreResources.In });

            // Add Delete Function.
            Utilities.AddSsdlFunction(schemaElement, deleteProcedureName, false, false, false,
                false, CoreResources.AllowImplicitConversion, CoreResources.Core,
                new string[] { CoreResources.SubjectResourceId, 
                    CoreResources.DataTypeUniqueidentifier, CoreResources.In, 
                    CoreResources.ObjectResourceId, 
                    CoreResources.DataTypeUniqueidentifier, CoreResources.In });
        }

        private static XmlElement UpdateMslCreateAssociationSetMappingElement(XmlElement entityContainerMappingElement, string associationSetName,
           string typeName, string storeEntitySet, string endPropertyName1, string scalarPropertyName1, string columnName1, string endPropertyName2,
           string scalarPropertyName2, string columnName2)
        {
            // Create AssociationSetMapping element.
            XmlElement associationSetMappingElement = Utilities.CreateElement(
                entityContainerMappingElement, CoreResources.AssociationSetMapping);
            Utilities.AddAttribute(associationSetMappingElement, CoreResources.Name,
                associationSetName);
            Utilities.AddAttribute(associationSetMappingElement, CoreResources.TypeName,
                typeName);
            Utilities.AddAttribute(associationSetMappingElement, CoreResources.StoreEntitySet,
                storeEntitySet);

            // Create subject EndProperty element.
            XmlElement subjectEndPropertyElement = Utilities.CreateElement(
                associationSetMappingElement, CoreResources.EndProperty);
            Utilities.AddAttribute(subjectEndPropertyElement, CoreResources.Name,
                endPropertyName1);

            // Create subject ScalarProperty element.
            XmlElement subjectScalarProperty = Utilities.CreateElement(subjectEndPropertyElement,
                CoreResources.ScalarProperty);
            Utilities.AddAttribute(subjectScalarProperty, CoreResources.Name, scalarPropertyName1);
            Utilities.AddAttribute(subjectScalarProperty, CoreResources.ColumnName,
                columnName1);

            // Create object EndProperty element.
            XmlElement objectEndPropertyElement = Utilities.CreateElement(
                associationSetMappingElement, CoreResources.EndProperty);
            Utilities.AddAttribute(objectEndPropertyElement, CoreResources.Name, endPropertyName2);

            // Create object ScalarProperty element.
            XmlElement objectScalarProperty = Utilities.CreateElement(objectEndPropertyElement,
                CoreResources.ScalarProperty);
            Utilities.AddAttribute(objectScalarProperty, CoreResources.Name, scalarPropertyName2);
            Utilities.AddAttribute(objectScalarProperty, CoreResources.ColumnName,
                columnName2);

            return associationSetMappingElement;
        }

        private static XmlElement UpdateMslCreateAssociationSetMappingElement(XmlElement entityContainerMappingElement, string associationSetName,
            string typeName, string storeEntitySet, string endPropertyName1, string scalarPropertyName1, string columnName1, string endPropertyName2,
            string scalarPropertyName2, string columnName2, string insertFunctionFullName, string deleteFunctionFullName, string end1ParameterName,
            string end2ParameterName)
        {
            // Create AssociationSetMapping element.
            XmlElement associationSetMappingElement = UpdateMslCreateAssociationSetMappingElement(
                entityContainerMappingElement, associationSetName, typeName, storeEntitySet, endPropertyName1,
                scalarPropertyName1, columnName1, endPropertyName2, scalarPropertyName2, columnName2);

            // Create ModificationFunctionMapping element.
            XmlElement modificationFunctionMappingElement =
                Utilities.CreateElement(associationSetMappingElement,
                CoreResources.ModificationFunctionMapping);

            // Create InsertFunction element.
            XmlElement insertFunctionElement =
                Utilities.CreateElement(modificationFunctionMappingElement,
                CoreResources.InsertFunction);
            Utilities.AddAttribute(insertFunctionElement, CoreResources.FunctionName,
                insertFunctionFullName);
            XmlElement subjectEndPropertyElement = Utilities.CreateElement(
                insertFunctionElement, CoreResources.EndProperty);
            Utilities.AddAttribute(subjectEndPropertyElement, CoreResources.Name,
                endPropertyName1);
            XmlElement subjectScalarProperty = Utilities.CreateElement(subjectEndPropertyElement,
                CoreResources.ScalarProperty);
            Utilities.AddAttribute(subjectScalarProperty, CoreResources.Name, scalarPropertyName1);
            Utilities.AddAttribute(subjectScalarProperty, CoreResources.ParameterName,
                end1ParameterName);
            XmlElement objectEndPropertyElement = Utilities.CreateElement(
                insertFunctionElement, CoreResources.EndProperty);
            Utilities.AddAttribute(objectEndPropertyElement, CoreResources.Name,
                endPropertyName2);
            XmlElement objectScalarProperty = Utilities.CreateElement(objectEndPropertyElement,
                CoreResources.ScalarProperty);
            Utilities.AddAttribute(objectScalarProperty, CoreResources.Name, scalarPropertyName2);
            Utilities.AddAttribute(objectScalarProperty, CoreResources.ParameterName,
                end2ParameterName);

            // Create DeleteFunction element.
            XmlElement deleteFunctionElement =
                Utilities.CreateElement(modificationFunctionMappingElement,
                CoreResources.DeleteFunction);
            Utilities.AddAttribute(deleteFunctionElement, CoreResources.FunctionName,
                deleteFunctionFullName);
            subjectEndPropertyElement = Utilities.CreateElement(
                deleteFunctionElement, CoreResources.EndProperty);
            Utilities.AddAttribute(subjectEndPropertyElement, CoreResources.Name,
                endPropertyName1);
            subjectScalarProperty = Utilities.CreateElement(subjectEndPropertyElement,
                CoreResources.ScalarProperty);
            Utilities.AddAttribute(subjectScalarProperty, CoreResources.Name, scalarPropertyName1);
            Utilities.AddAttribute(subjectScalarProperty, CoreResources.ParameterName,
                end1ParameterName);
            objectEndPropertyElement = Utilities.CreateElement(
                deleteFunctionElement, CoreResources.EndProperty);
            Utilities.AddAttribute(objectEndPropertyElement, CoreResources.Name,
                endPropertyName2);
            objectScalarProperty = Utilities.CreateElement(objectEndPropertyElement,
                CoreResources.ScalarProperty);
            Utilities.AddAttribute(objectScalarProperty, CoreResources.Name, scalarPropertyName2);
            Utilities.AddAttribute(objectScalarProperty, CoreResources.ParameterName,
                end2ParameterName);

            return associationSetMappingElement;
        }

        #endregion
    }
}
