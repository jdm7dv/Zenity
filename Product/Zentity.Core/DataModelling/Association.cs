// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Xml;
using System.Resources;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Represents an Association in the data model.
    /// </summary>
    /// <example>The example below shows how to enumerate the associations of a data model.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///
    ///namespace ZentitySamples
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
        AssociationEndMultiplicity objectMultiplicity;
        NavigationProperty objectNavigationProperty;
        Guid predicateId;
        AssociationEndMultiplicity subjectMultiplicity;
        NavigationProperty subjectNavigationProperty;
        string uri;
        string viewName;

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
        /// Gets or sets the multiplicity of the object end of the association.
        /// </summary>
        public AssociationEndMultiplicity ObjectMultiplicity
        {
            get { return objectMultiplicity; }
            set { objectMultiplicity = value; }
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
                        DataModellingResources.ExceptionUnavailableNavProperty);
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
        /// Gets or sets the multiplicity of the subject end of the association.
        /// </summary>
        public AssociationEndMultiplicity SubjectMultiplicity
        {
            get { return subjectMultiplicity; }
            set { subjectMultiplicity = value; }
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
                        DataModellingResources.ExceptionUnavailableNavProperty);
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
        /// Gets or sets the Uri of this association.
        /// </summary>
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }

        /// <summary>
        /// Gets the view name to which this association is mapped. All such views are created in 
        /// the 'Core' schema. For OneToXXX or XXXToOne associations, these views are used only
        /// to enforce cardinality constraints and are not used for querying the relationship 
        /// information.
        /// </summary>
        public string ViewName
        {
            get { return viewName; }
            internal set { viewName = value; }
        }

        ///////////////// Internal Properties /////////////////

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

        /// <summary>
        /// Gets the name of the insert procedure.
        /// </summary>
        /// <value>The name of the insert procedure.</value>
        internal string InsertProcedureName
        {
            get { return "Insert" + this.Id.ToString("N").ToLowerInvariant(); }
        }

        /// <summary>
        /// Gets the name of the delete procedure.
        /// </summary>
        /// <value>The name of the delete procedure.</value>
        internal string DeleteProcedureName
        {
            get { return "Delete" + this.Id.ToString("N").ToLowerInvariant(); }
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
        /// <param name="name">Association name.</param>
        public Association(string name)
            : this(name, Utilities.GetGuidString())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Zentity.Core.Association"/> class.
        /// </summary>
        /// <param name="name">Association Name.</param>
        /// <param name="uri">Association Uri</param>
        public Association(string name, string uri)
        {
            this.id = Guid.NewGuid();
            this.predicateId = Guid.NewGuid();
            this.name = name;
            this.uri = uri;
            this.subjectMultiplicity = AssociationEndMultiplicity.Many;
            this.objectMultiplicity = AssociationEndMultiplicity.Many;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the CSDL.
        /// </summary>
        /// <param name="coreCsdl">The core CSDL.</param>
        /// <param name="moduleCsdl">The module CSDL.</param>
        internal void UpdateCsdl(XmlDocument coreCsdl, XmlDocument moduleCsdl)
        {
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
                throw new ZentityException(DataModellingResources.InvalidMultiplicityOneToOne);

            // Add AssociationSet element in Core CSDL.
            XmlNamespaceManager coreCsdlNsMgr = new XmlNamespaceManager(coreCsdl.NameTable);
            coreCsdlNsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix,
                DataModellingResources.CSDLSchemaNameSpace);

            XmlElement entityContainerElement = coreCsdl.SelectSingleNode(
                DataModellingResources.XPathCSDLEntityContainer, coreCsdlNsMgr) as XmlElement;

            XmlElement associationSetElement = Utilities.CreateElement(entityContainerElement,
                DataModellingResources.AssociationSet);
            Utilities.AddAttribute(associationSetElement, DataModellingResources.Name, this.Name);
            Utilities.AddAttribute(associationSetElement, DataModellingResources.Association,
                this.FullName);

            XmlElement associationSetEnd1 = Utilities.CreateElement(associationSetElement,
                DataModellingResources.End);
            Utilities.AddAttribute(associationSetEnd1, DataModellingResources.Role, SubjectRole);
            Utilities.AddAttribute(associationSetEnd1, DataModellingResources.EntitySet,
                DataModellingResources.Resources);

            XmlElement associationSetEnd2 = Utilities.CreateElement(associationSetElement,
                DataModellingResources.End);
            Utilities.AddAttribute(associationSetEnd2, DataModellingResources.Role, ObjectRole);
            Utilities.AddAttribute(associationSetEnd2, DataModellingResources.EntitySet,
                DataModellingResources.Resources);

            // Add FunctionImport elements for ViewAssociations.
            if (this.SubjectMultiplicity != AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity != AssociationEndMultiplicity.One)
            {
                Utilities.AddCsdlFunctionImport(entityContainerElement,
                    InsertProcedureName,
                    new string[] { DataModellingResources.SubjectResourceId, DataTypes.Guid.ToString(), 
                        DataModellingResources.In, DataModellingResources.ObjectResourceId, 
                        DataTypes.Guid.ToString(), DataModellingResources.In });

                Utilities.AddCsdlFunctionImport(entityContainerElement,
                    DeleteProcedureName,
                    new string[] { DataModellingResources.SubjectResourceId, DataTypes.Guid.ToString(), 
                        DataModellingResources.In, DataModellingResources.ObjectResourceId, 
                        DataTypes.Guid.ToString(), DataModellingResources.In });
            }



            // Add Association element in Module CSDL.
            XmlNamespaceManager moduleCsdlNsMgr = new XmlNamespaceManager(moduleCsdl.NameTable);
            moduleCsdlNsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix,
                DataModellingResources.CSDLSchemaNameSpace);

            XmlElement schemaElement = moduleCsdl.SelectSingleNode(
                DataModellingResources.XPathCSDLSchema, moduleCsdlNsMgr) as XmlElement;

            XmlElement associationElement = Utilities.CreateElement(schemaElement,
                DataModellingResources.Association);
            Utilities.AddAttribute(associationElement, DataModellingResources.Name, this.Name);

            XmlElement associationEnd1 = Utilities.CreateElement(associationElement,
                DataModellingResources.End);
            Utilities.AddAttribute(associationEnd1, DataModellingResources.Role, SubjectRole);
            Utilities.AddAttribute(associationEnd1, DataModellingResources.Type,
                SubjectNavigationProperty.Parent.FullName);
            Utilities.AddAttribute(associationEnd1, DataModellingResources.Multiplicity,
                Utilities.GetEdmxMultiplicityValue(SubjectMultiplicity));

            XmlElement associationEnd2 = Utilities.CreateElement(associationElement,
                DataModellingResources.End);
            Utilities.AddAttribute(associationEnd2, DataModellingResources.Role, ObjectRole);
            Utilities.AddAttribute(associationEnd2, DataModellingResources.Type,
                ObjectNavigationProperty.Parent.FullName);
            Utilities.AddAttribute(associationEnd2, DataModellingResources.Multiplicity,
                Utilities.GetEdmxMultiplicityValue(ObjectMultiplicity));
        }

        /// <summary>
        /// Updates the flattened CSDL.
        /// </summary>
        /// <param name="coreCsdl">The core CSDL.</param>
        /// <param name="moduleCsdl">The module CSDL.</param>
        internal void UpdateFlattenedCsdl(XmlDocument coreCsdl, XmlDocument moduleCsdl)
        {
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
                throw new ZentityException(DataModellingResources.InvalidMultiplicityOneToOne);

            var subjectDerivedTypes = this.SubjectNavigationProperty.Parent.GetDerivedTypes();
            var objectDerivedTypes = this.ObjectNavigationProperty.Parent.GetDerivedTypes();
            var crossDerivedTypes = from ResourceType resTypeSubject in subjectDerivedTypes
                                    from ResourceType resTypeObject in objectDerivedTypes
                                    select new
                                    {
                                        SubjectResourceType = resTypeSubject,
                                        ObjectResourceType = resTypeObject,
                                        SuffixName = "_" + resTypeSubject.Name + resTypeObject.Name
                                    };

            // Add AssociationSet element in Core CSDL.
            XmlNamespaceManager coreCsdlNsMgr = new XmlNamespaceManager(coreCsdl.NameTable);
            coreCsdlNsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix, DataModellingResources.CSDLSchemaNameSpace);

            XmlElement entityContainerElement = coreCsdl.SelectSingleNode(DataModellingResources.XPathCSDLEntityContainer, coreCsdlNsMgr) as XmlElement;

            // Add FunctionImport elements for ViewAssociations.
            if (this.SubjectMultiplicity != AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity != AssociationEndMultiplicity.One)
            {
                Utilities.AddCsdlFunctionImport(entityContainerElement, InsertProcedureName,
                    new string[] { DataModellingResources.SubjectResourceId, DataTypes.Guid.ToString(), 
                        DataModellingResources.In, DataModellingResources.ObjectResourceId, 
                        DataTypes.Guid.ToString(), DataModellingResources.In });

                Utilities.AddCsdlFunctionImport(entityContainerElement, DeleteProcedureName,
                    new string[] { DataModellingResources.SubjectResourceId, DataTypes.Guid.ToString(), 
                        DataModellingResources.In, DataModellingResources.ObjectResourceId, 
                        DataTypes.Guid.ToString(), DataModellingResources.In });
            }

            foreach (var crossItem in crossDerivedTypes)
            {
                XmlElement associationSetElement = Utilities.CreateElement(entityContainerElement, DataModellingResources.AssociationSet);
                Utilities.AddAttribute(associationSetElement, DataModellingResources.Name, this.Name + crossItem.SuffixName);
                Utilities.AddAttribute(associationSetElement, DataModellingResources.Association, this.FullName + crossItem.SuffixName);

                string associationSetEnd1Name = crossItem.SubjectResourceType.Name;
                string associationSetEnd2Name = crossItem.ObjectResourceType.Name;

                if (associationSetEnd1Name.Equals(associationSetEnd2Name, StringComparison.OrdinalIgnoreCase))
                {
                    associationSetEnd1Name += "1";
                    associationSetEnd2Name += "2";
                }

                XmlElement associationSetEnd1 = Utilities.CreateElement(associationSetElement, DataModellingResources.End);
                Utilities.AddAttribute(associationSetEnd1, DataModellingResources.Role, associationSetEnd1Name);
                Utilities.AddAttribute(associationSetEnd1, DataModellingResources.EntitySet, crossItem.SubjectResourceType.Name);

                XmlElement associationSetEnd2 = Utilities.CreateElement(associationSetElement, DataModellingResources.End);
                Utilities.AddAttribute(associationSetEnd2, DataModellingResources.Role, associationSetEnd2Name);
                Utilities.AddAttribute(associationSetEnd2, DataModellingResources.EntitySet, crossItem.ObjectResourceType.Name); 
            }

            // Add Association element in Module CSDL.
            XmlNamespaceManager moduleCsdlNsMgr = new XmlNamespaceManager(moduleCsdl.NameTable);
            moduleCsdlNsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix, DataModellingResources.CSDLSchemaNameSpace);

            XmlElement schemaElement = moduleCsdl.SelectSingleNode(DataModellingResources.XPathCSDLSchema, moduleCsdlNsMgr) as XmlElement;

            foreach (var crossItem in crossDerivedTypes)
            {
                XmlElement associationElement = Utilities.CreateElement(schemaElement, DataModellingResources.Association);
                Utilities.AddAttribute(associationElement, DataModellingResources.Name, this.Name + crossItem.SuffixName);

                string associationEnd1Name = crossItem.SubjectResourceType.Name;
                string associationEnd2Name = crossItem.ObjectResourceType.Name;

                if (associationEnd1Name.Equals(associationEnd2Name, StringComparison.OrdinalIgnoreCase))
                {
                    associationEnd1Name += "1";
                    associationEnd2Name += "2";
                }

                XmlElement associationEnd1 = Utilities.CreateElement(associationElement, DataModellingResources.End);
                Utilities.AddAttribute(associationEnd1, DataModellingResources.Role, associationEnd1Name);
                Utilities.AddAttribute(associationEnd1, DataModellingResources.Type, crossItem.SubjectResourceType.FullName);
                Utilities.AddAttribute(associationEnd1, DataModellingResources.Multiplicity, Utilities.GetEdmxMultiplicityValue(SubjectMultiplicity));

                XmlElement associationEnd2 = Utilities.CreateElement(associationElement, DataModellingResources.End);
                Utilities.AddAttribute(associationEnd2, DataModellingResources.Role, associationEnd2Name);
                Utilities.AddAttribute(associationEnd2, DataModellingResources.Type, crossItem.ObjectResourceType.FullName);
                Utilities.AddAttribute(associationEnd2, DataModellingResources.Multiplicity, Utilities.GetEdmxMultiplicityValue(ObjectMultiplicity)); 

                if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                    this.ObjectMultiplicity == AssociationEndMultiplicity.Many)
                {
                    Utilities.AddCsdlAssociationReferentialConstraint(associationElement, associationEnd1Name, DataModellingResources.Id, associationEnd2Name, this.SubjectNavigationProperty.Parent.Name + "_Id");
                }

                if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                    this.ObjectMultiplicity == AssociationEndMultiplicity.One)
                {
                    Utilities.AddCsdlAssociationReferentialConstraint(associationElement, associationEnd2Name, DataModellingResources.Id, associationEnd1Name, this.ObjectNavigationProperty.Parent.Name + "_Id");
                }
            }
        }

        /// <summary>
        /// Updates the flattened CSDL for file entity.
        /// </summary>
        /// <param name="coreCsdl">The core CSDL.</param>
        /// <param name="moduleCsdl">The module CSDL.</param>
        /// <param name="resourceType">Type of the resource.</param>
        internal void UpdateFlattenedCsdlForFile(XmlDocument coreCsdl, XmlDocument moduleCsdl, ResourceType resourceType)
        {
            // Add AssociationSet element in Core CSDL.
            XmlNamespaceManager coreCsdlNsMgr = new XmlNamespaceManager(coreCsdl.NameTable);
            coreCsdlNsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix, DataModellingResources.CSDLSchemaNameSpace);

            XmlElement schemaElement = coreCsdl.SelectSingleNode(DataModellingResources.XPathCSDLSchema, coreCsdlNsMgr) as XmlElement;
            XmlElement entityContainerElement = coreCsdl.SelectSingleNode(DataModellingResources.XPathCSDLEntityContainer, coreCsdlNsMgr) as XmlElement;

            string targetNamespace = resourceType.Parent.NameSpace;
            string fileAssociationFullName = Utilities.MergeSubNames(targetNamespace, this.Name);

            // Create the AssociationSet element
            XmlElement associationSetElement = Utilities.CreateElement(entityContainerElement, DataModellingResources.AssociationSet);
            Utilities.AddAttribute(associationSetElement, DataModellingResources.Name, string.Format(DataModellingResources.SuffixedNameFormat, this.Name, resourceType.Name + this.ObjectRole));
            Utilities.AddAttribute(associationSetElement, DataModellingResources.Association, string.Format(DataModellingResources.SuffixedNameFormat, fileAssociationFullName, resourceType.Name + this.ObjectRole));

            string associationSetEnd1Name = resourceType.Name;
            string associationSetEnd2Name = this.ObjectRole;

            XmlElement associationSetEnd1 = Utilities.CreateElement(associationSetElement, DataModellingResources.End);
            Utilities.AddAttribute(associationSetEnd1, DataModellingResources.Role, associationSetEnd1Name);
            Utilities.AddAttribute(associationSetEnd1, DataModellingResources.EntitySet, associationSetEnd1Name);

            XmlElement associationSetEnd2 = Utilities.CreateElement(associationSetElement, DataModellingResources.End);
            Utilities.AddAttribute(associationSetEnd2, DataModellingResources.Role, associationSetEnd2Name);
            Utilities.AddAttribute(associationSetEnd2, DataModellingResources.EntitySet, DataModellingResources.FlattenedFiles);

            //// Add Association element in Module CSDL.
            XmlNamespaceManager moduleCsdlNsMgr = new XmlNamespaceManager(moduleCsdl.NameTable);
            moduleCsdlNsMgr.AddNamespace(DataModellingResources.CSDLNamespacePrefix, DataModellingResources.CSDLSchemaNameSpace);
            schemaElement = moduleCsdl.SelectSingleNode(DataModellingResources.XPathCSDLSchema, moduleCsdlNsMgr) as XmlElement;

            // Create the Association element
            XmlElement associationElement = Utilities.CreateElement(schemaElement, DataModellingResources.Association);
            Utilities.AddAttribute(associationElement, DataModellingResources.Name, string.Format(DataModellingResources.SuffixedNameFormat, this.Name, resourceType.Name + this.ObjectRole));

            string associationEnd1Name = resourceType.Name;
            string associationEnd2Name = this.ObjectRole;
            string associationEnd2FullName = Utilities.MergeSubNames(DataModellingResources.ZentityCore, DataModellingResources.FlattenedFile);
            
            XmlElement associationEnd1 = Utilities.CreateElement(associationElement, DataModellingResources.End);
            Utilities.AddAttribute(associationEnd1, DataModellingResources.Role, associationEnd1Name);
            Utilities.AddAttribute(associationEnd1, DataModellingResources.Type, resourceType.FullName);
            Utilities.AddAttribute(associationEnd1, DataModellingResources.Multiplicity, Utilities.GetEdmxMultiplicityValue(SubjectMultiplicity));

            XmlElement associationEnd2 = Utilities.CreateElement(associationElement, DataModellingResources.End);
            Utilities.AddAttribute(associationEnd2, DataModellingResources.Role, associationEnd2Name);
            Utilities.AddAttribute(associationEnd2, DataModellingResources.Type, associationEnd2FullName);
            Utilities.AddAttribute(associationEnd2, DataModellingResources.Multiplicity, Utilities.GetEdmxMultiplicityValue(ObjectMultiplicity));
        }

        /// <summary>
        /// Updates the MSL.
        /// </summary>
        /// <param name="mslDocument">The MSL document.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="storageSchemaName">Name of the storage schema.</param>
        internal void UpdateMsl(XmlDocument mslDocument, TableMappingCollection tableMappings, string storageSchemaName)
        {
            // Locate the EntityContainer element.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(mslDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.MSLNamespacePrefix, DataModellingResources.MSLSchemaNamespace);

            XmlElement entityContainerMappingElement = mslDocument.SelectSingleNode(
                DataModellingResources.XPathMSLEntityContainerMapping, nsMgr) as XmlElement;

            // We don't support One to One associations.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
                throw new ZentityException(DataModellingResources.InvalidMultiplicityOneToOne);

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
                string createFunctionFullName = Utilities.MergeSubNames(storageSchemaName, InsertProcedureName);
                string deleteFunctionFullName = Utilities.MergeSubNames(storageSchemaName, DeleteProcedureName);

                Utilities.AddMslFunctionImportMapping(entityContainerMappingElement,
                    InsertProcedureName, createFunctionFullName);

                Utilities.AddMslFunctionImportMapping(entityContainerMappingElement,
                    DeleteProcedureName, deleteFunctionFullName);

                // Create AssociationSetMapping element.
                UpdateMslCreateAssociationSetMappingElement(entityContainerMappingElement, this.Name,
                    this.FullName, this.ViewName, this.SubjectRole, DataModellingResources.Id,
                    DataModellingResources.SubjectResourceId, this.ObjectRole, DataModellingResources.Id,
                    DataModellingResources.ObjectResourceId, createFunctionFullName, deleteFunctionFullName,
                    DataModellingResources.SubjectResourceId, DataModellingResources.ObjectResourceId);
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
                       columnMapping.Parent.TableName, this.SubjectRole, DataModellingResources.Id,
                       columnMapping.ColumnName, this.ObjectRole, DataModellingResources.Id,
                       DataModellingResources.Id);
                }
                // Handle XXXToOne mappings except One to One.
                else
                {
                    columnMapping = tableMappings.GetColumnMappingByPropertyId(
                        this.SubjectNavigationProperty.Id);

                    associationSetMappingElement = UpdateMslCreateAssociationSetMappingElement(
                        entityContainerMappingElement, this.Name, this.FullName,
                        columnMapping.Parent.TableName, this.SubjectRole, DataModellingResources.Id,
                        DataModellingResources.Id, this.ObjectRole, DataModellingResources.Id,
                        columnMapping.ColumnName);
                }

                // Add the NOT NULL Condition element for foreign key column.
                XmlElement conditionElement = Utilities.CreateElement(associationSetMappingElement,
                    DataModellingResources.Condition);
                Utilities.AddAttribute(conditionElement, DataModellingResources.ColumnName,
                    columnMapping.ColumnName);
                Utilities.AddAttribute(conditionElement, DataModellingResources.IsNull,
                    false.ToString().ToLowerInvariant());
            }
        }

        /// <summary>
        /// Updates the flattened MSL.
        /// </summary>
        /// <param name="mslDocument">The MSL document.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="storageSchemaName">Name of the storage schema.</param>
        internal void UpdateFlattenedMsl(XmlDocument mslDocument, TableMappingCollection tableMappings, string storageSchemaName)
        {
            // Locate the EntityContainer element.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(mslDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.MSLNamespacePrefix, DataModellingResources.MSLSchemaNamespace);

            XmlElement entityContainerMappingElement = mslDocument.SelectSingleNode(DataModellingResources.XPathMSLEntityContainerMapping, nsMgr) as XmlElement;

            // We don't support One to One associations.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
                throw new ZentityException(DataModellingResources.InvalidMultiplicityOneToOne);

            var subjectDerivedTypes = this.SubjectNavigationProperty.Parent.GetDerivedTypes();
            var objectDerivedTypes = this.ObjectNavigationProperty.Parent.GetDerivedTypes();
            var crossDerivedTypes = from ResourceType resTypeSubject in subjectDerivedTypes
                                    from ResourceType resTypeObject in objectDerivedTypes
                                    select new
                                    {
                                        SubjectResourceType = resTypeSubject,
                                        ObjectResourceType = resTypeObject,
                                        SuffixName = "_" + resTypeSubject.Name + resTypeObject.Name
                                    };

            // Handle view mappings.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
            {
                // Create FunctionImportMapping elements.
                string createFunctionFullName = Utilities.MergeSubNames(storageSchemaName, InsertProcedureName);
                string deleteFunctionFullName = Utilities.MergeSubNames(storageSchemaName, DeleteProcedureName);

                Utilities.AddMslFunctionImportMapping(entityContainerMappingElement, InsertProcedureName, createFunctionFullName);
                Utilities.AddMslFunctionImportMapping(entityContainerMappingElement, DeleteProcedureName, deleteFunctionFullName);

                foreach (var crossItem in crossDerivedTypes)
                {
                    string endProperty1Name = crossItem.SubjectResourceType.Name;
                    string endProperty2Name = crossItem.ObjectResourceType.Name;
                    
                    if (endProperty1Name.Equals(endProperty2Name, StringComparison.OrdinalIgnoreCase))
                    {
                        endProperty1Name += "1";
                        endProperty2Name += "2";
                    }

                    // Create AssociationSetMapping element.
                    UpdateMslCreateAssociationSetMappingElement(entityContainerMappingElement, this.Name + crossItem.SuffixName,
                                                                this.FullName + crossItem.SuffixName, this.ViewName + crossItem.SuffixName,
                                                                endProperty1Name, DataModellingResources.Id, DataModellingResources.SubjectResourceId,
                                                                endProperty2Name, DataModellingResources.Id, DataModellingResources.ObjectResourceId,
                                                                createFunctionFullName, deleteFunctionFullName,
                                                                DataModellingResources.SubjectResourceId, DataModellingResources.ObjectResourceId);
                }
            }

            else if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                     this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne ||
                     this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                     this.ObjectMultiplicity == AssociationEndMultiplicity.One)
            {
                // TODO: Need to check these associations and handle them accordingly
                ColumnMapping columnMapping = null;
                XmlElement associationSetMappingElement = null;

                foreach (var crossItem in crossDerivedTypes)
                {
                    string endProperty1Name = crossItem.SubjectResourceType.Name;
                    string endProperty2Name = crossItem.ObjectResourceType.Name;

                    if (endProperty1Name.Equals(endProperty2Name, StringComparison.OrdinalIgnoreCase))
                    {
                        endProperty1Name += "1";
                        endProperty2Name += "2";
                    }

                    // Handle OneToXXX mappings except One to One.
                    if (this.SubjectMultiplicity == AssociationEndMultiplicity.One)
                    {
                        columnMapping = tableMappings.GetColumnMappingByPropertyId(this.ObjectNavigationProperty.Id);

                        associationSetMappingElement = UpdateMslCreateAssociationSetMappingElement(
                            entityContainerMappingElement, this.Name + crossItem.SuffixName, this.FullName + crossItem.SuffixName,
                            crossItem.ObjectResourceType.Name, endProperty1Name, DataModellingResources.Id,
                            columnMapping.ColumnName, endProperty2Name, DataModellingResources.Id,
                            DataModellingResources.Id);
                    }
                    // Handle XXXToOne mappings except One to One.
                    else
                    {
                        columnMapping = tableMappings.GetColumnMappingByPropertyId(this.SubjectNavigationProperty.Id);

                        associationSetMappingElement = UpdateMslCreateAssociationSetMappingElement(
                            entityContainerMappingElement, this.Name + crossItem.SuffixName, this.FullName + crossItem.SuffixName,
                            crossItem.SubjectResourceType.Name, endProperty1Name, DataModellingResources.Id,
                            DataModellingResources.Id, endProperty2Name, DataModellingResources.Id,
                            columnMapping.ColumnName);
                    }

                    // Add the NOT NULL Condition element for foreign key column.
                    XmlElement conditionElement = Utilities.CreateElement(associationSetMappingElement, DataModellingResources.Condition);
                    Utilities.AddAttribute(conditionElement, DataModellingResources.ColumnName, columnMapping.ColumnName);
                    Utilities.AddAttribute(conditionElement, DataModellingResources.IsNull, false.ToString().ToLowerInvariant());
                }
            }
        }

        /// <summary>
        /// Updates the flattened MSL for file entity.
        /// </summary>
        /// <param name="mslDocument">The MSL document.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="storageSchemaName">Name of the storage schema.</param>
        internal void UpdateFlattenedMslForFile(XmlDocument mslDocument, ResourceType resourceType, string storageSchemaName)
        {
            // Locate the EntityContainer element.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(mslDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.MSLNamespacePrefix, DataModellingResources.MSLSchemaNamespace);

            XmlElement entityContainerMappingElement = mslDocument.SelectSingleNode(DataModellingResources.XPathMSLEntityContainerMapping, nsMgr) as XmlElement;

            string endProperty1Name = resourceType.Name;
            string endProperty2Name = this.ObjectRole;
            string insertProcedureName = Utilities.MergeSubNames(storageSchemaName, "Insert" + this.ViewName);
            string deleteProcedureName = Utilities.MergeSubNames(storageSchemaName, "Delete" + this.ViewName);

            string targetNamespace = resourceType.Parent.NameSpace;
            string fileAssociationFullName = Utilities.MergeSubNames(targetNamespace, this.Name);

            // Create AssociationSetMapping element.
            UpdateMslCreateAssociationSetMappingElement(entityContainerMappingElement, string.Format(DataModellingResources.SuffixedNameFormat, this.Name, resourceType.Name + this.ObjectRole),
                                                        string.Format(DataModellingResources.SuffixedNameFormat, fileAssociationFullName, resourceType.Name + this.ObjectRole),
                                                        string.Format(DataModellingResources.SuffixedNameFormat, this.Name, resourceType.Name + this.ObjectRole),
                                                        endProperty1Name, DataModellingResources.Id, DataModellingResources.SubjectResourceId,
                                                        endProperty2Name, DataModellingResources.Id, DataModellingResources.ObjectResourceId,
                                                        insertProcedureName, deleteProcedureName,
                                                        DataModellingResources.SubjectResourceId, DataModellingResources.ObjectResourceId);
        }

        /// <summary>
        /// Updates the SSDL.
        /// </summary>
        /// <param name="ssdlDocument">The SSDL document.</param>
        /// <param name="tableMappings">The table mappings.</param>
        internal void UpdateSsdl(XmlDocument ssdlDocument, TableMappingCollection tableMappings)
        {
            // Pick the Schema and EntityContainer elements.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ssdlDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.SSDLNamespacePrefix,
                DataModellingResources.SSDLSchemaNameSpace);

            XmlElement schemaElement = ssdlDocument.SelectSingleNode(
                DataModellingResources.XPathSSDLSchema, nsMgr) as XmlElement;

            XmlElement entityContainerElement = ssdlDocument.SelectSingleNode(
                DataModellingResources.XPathSSDLEntityContainer, nsMgr) as XmlElement;

            // Parameters for View associations 
            // (ManyToMany, ManyToZeroOrOne, ZeroOrOneToMany, ZeroOrOneToZeroOrOne).

            string subjectFKConstraintName = string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.FKConstraintName, this.SubjectNavigationProperty.Id.ToString());
            string objectFKConstraintName = string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.FKConstraintName, this.ObjectNavigationProperty.Id.ToString());


            // One to One.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
            {
                throw new ZentityException(DataModellingResources.InvalidMultiplicityOneToOne);
            }

            // Many to Many.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many)
            {
                UpdateSsdlHandleViewAssociations(entityContainerElement, schemaElement, this.ViewName,
                    subjectFKConstraintName, objectFKConstraintName,
                    new string[] { DataModellingResources.SubjectResourceId, 
                        DataModellingResources.ObjectResourceId }, DataModellingResources.One, DataModellingResources.Many,
                        DataModellingResources.One, DataModellingResources.Many, InsertProcedureName,
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
                UpdateSsdlHandleViewAssociations(entityContainerElement, schemaElement, this.ViewName,
                    subjectFKConstraintName, objectFKConstraintName,
                    new string[] { DataModellingResources.SubjectResourceId }, DataModellingResources.One,
                        DataModellingResources.ZeroOrOne, DataModellingResources.One, DataModellingResources.Many,
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
                UpdateSsdlHandleViewAssociations(entityContainerElement, schemaElement, this.ViewName,
                    subjectFKConstraintName, objectFKConstraintName,
                    new string[] { DataModellingResources.ObjectResourceId }, DataModellingResources.One,
                        DataModellingResources.Many, DataModellingResources.One, DataModellingResources.ZeroOrOne,
                        InsertProcedureName, DeleteProcedureName);
            }
        }

        /// <summary>
        /// Updates the flattened SSDL.
        /// </summary>
        /// <param name="ssdlDocument">The SSDL document.</param>
        /// <param name="tableMappings">The table mappings.</param>
        internal void UpdateFlattenedSsdl(XmlDocument ssdlDocument, TableMappingCollection tableMappings)
        {
            // Pick the Schema and EntityContainer elements.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ssdlDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.SSDLNamespacePrefix, DataModellingResources.SSDLSchemaNameSpace);

            XmlElement schemaElement = ssdlDocument.SelectSingleNode(DataModellingResources.XPathSSDLSchema, nsMgr) as XmlElement;
            XmlElement entityContainerElement = ssdlDocument.SelectSingleNode(DataModellingResources.XPathSSDLEntityContainer, nsMgr) as XmlElement;

            // Parameters for View associations 
            // (ManyToMany, ManyToZeroOrOne, ZeroOrOneToMany, ZeroOrOneToZeroOrOne).

            string subjectFKConstraintName = string.Format(CultureInfo.InvariantCulture, DataModellingResources.FKConstraintName, this.SubjectNavigationProperty.Id);
            string objectFKConstraintName = string.Format(CultureInfo.InvariantCulture, DataModellingResources.FKConstraintName, this.ObjectNavigationProperty.Id);

            var subjectDerivedTypes = this.SubjectNavigationProperty.Parent.GetDerivedTypes();
            var objectDerivedTypes = this.ObjectNavigationProperty.Parent.GetDerivedTypes();
            var crossDerivedTypes = from ResourceType resTypeSubject in subjectDerivedTypes
                                    from ResourceType resTypeObject in objectDerivedTypes
                                    select new
                                    {
                                        SubjectResourceType = resTypeSubject,
                                        ObjectResourceType = resTypeObject,
                                        SuffixName = "_" + resTypeSubject.Name + resTypeObject.Name
                                    };

            // One to One.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
            {
                throw new ZentityException(DataModellingResources.InvalidMultiplicityOneToOne);
            }

            // Many to Many.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many)
            {
                foreach (var crossItem in crossDerivedTypes)
                {
                    string subjectFKConstraintNewName = subjectFKConstraintName + crossItem.SuffixName;
                    string objectFKConstraintNewName = objectFKConstraintName + crossItem.SuffixName;

                    UpdateFlattenedSsdlHandleViewAssociations(entityContainerElement, schemaElement, this.ViewName,
                                                              crossItem.SubjectResourceType.Name,
                                                              crossItem.ObjectResourceType.Name,
                                                              subjectFKConstraintNewName, objectFKConstraintNewName,
                                                              new[]
                                                                  {
                                                                      DataModellingResources.SubjectResourceId,
                                                                      DataModellingResources.ObjectResourceId
                                                                  },
                                                              DataModellingResources.One, DataModellingResources.Many,
                                                              DataModellingResources.One, DataModellingResources.Many,
                                                              InsertProcedureName, DeleteProcedureName);
                }
            }

            // Many to One or ZeroOrOne to One.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.One)
            {
                // TODO: Need to test this situation and modify the function accordingly
                //UpdateFlattenedSsdlHandleOneToXxxAssociations(entityContainerElement, schemaElement, this.SubjectNavigationProperty.Id, this.SubjectNavigationProperty.Parent.Name, tableMappings);
            }

            // Many to ZeroOrOne OR ZeroOrOne to ZeroOrOne.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
            {
                foreach (var crossItem in crossDerivedTypes)
                {
                    string subjectFKConstraintNewName = subjectFKConstraintName + crossItem.SuffixName;
                    string objectFKConstraintNewName = objectFKConstraintName + crossItem.SuffixName;

                    UpdateFlattenedSsdlHandleViewAssociations(entityContainerElement, schemaElement, this.ViewName,
                                                              crossItem.SubjectResourceType.Name,
                                                              crossItem.ObjectResourceType.Name,
                                                              subjectFKConstraintNewName, objectFKConstraintNewName,
                                                              new[]
                                                                  {
                                                                      DataModellingResources.SubjectResourceId
                                                                  },
                                                              DataModellingResources.One, DataModellingResources.ZeroOrOne,
                                                              DataModellingResources.One, DataModellingResources.Many,
                                                              InsertProcedureName, DeleteProcedureName);
                }
            }

            // One to Many or One to ZeroOrOne.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many ||
                this.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
            {
                // TODO: Need to test this situation and modify the function accordingly
                //UpdateFlattenedSsdlHandleOneToXxxAssociations(entityContainerElement, schemaElement, this.ObjectNavigationProperty.Id, this.ObjectNavigationProperty.Parent.Name, tableMappings);
            }

            // ZeroOrOne to Many.
            if (this.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                this.ObjectMultiplicity == AssociationEndMultiplicity.Many)
            {
                foreach (var crossItem in crossDerivedTypes)
                {
                    string subjectFKConstraintNewName = subjectFKConstraintName + crossItem.SuffixName;
                    string objectFKConstraintNewName = objectFKConstraintName + crossItem.SuffixName;

                    UpdateFlattenedSsdlHandleViewAssociations(entityContainerElement, schemaElement, this.ViewName,
                                                              crossItem.SubjectResourceType.Name,
                                                              crossItem.ObjectResourceType.Name,
                                                              subjectFKConstraintNewName, objectFKConstraintNewName,
                                                              new[]
                                                                  {
                                                                      DataModellingResources.ObjectResourceId
                                                                  },
                                                              DataModellingResources.One,
                                                              DataModellingResources.Many, DataModellingResources.One,
                                                              DataModellingResources.ZeroOrOne,
                                                              InsertProcedureName, DeleteProcedureName);
                }
                
            }
        }

        /// <summary>
        /// Updates the flattened SSDL for file.
        /// </summary>
        /// <param name="ssdlDocument">The SSDL document.</param>
        /// <param name="resourceType">Type of the resource.</param>
        internal void UpdateFlattenedSsdlForFile(XmlDocument ssdlDocument, ResourceType resourceType)
        {
            // Pick the Schema and EntityContainer elements.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ssdlDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.SSDLNamespacePrefix, DataModellingResources.SSDLSchemaNameSpace);

            XmlElement schemaElement = ssdlDocument.SelectSingleNode(DataModellingResources.XPathSSDLSchema, nsMgr) as XmlElement;
            XmlElement entityContainerElement = ssdlDocument.SelectSingleNode(DataModellingResources.XPathSSDLEntityContainer, nsMgr) as XmlElement;

            string subjectRoleName = resourceType.Name;
            string objectRoleName = this.ObjectRole;
            string storageNamespace = schemaElement.Attributes[DataModellingResources.Namespace].Value;
            string viewFullName = Utilities.MergeSubNames(storageNamespace, this.ViewName);
            string viewCustomName = this.ViewName + "_" + subjectRoleName + objectRoleName;

            // Create EntitySet element.
            Utilities.AddSsdlEntitySetWithDefiningQuery(entityContainerElement, viewCustomName, viewFullName,
                                                        string.Format(DataModellingResources.ViewDefiningQueryFormat, viewName));
        }

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
                    DataModellingResources.ValidationExceptionPropertyEmpty,
                    DataModellingResources.Id, this.GetType().FullName));

            // Validate Name.
            if (string.IsNullOrEmpty(this.Name))
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionStringPropertyNullOrEmpty,
                    DataModellingResources.Name, this.GetType().FullName));

            if (this.Name.Length > MaxLengths.AssociationName)
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Name,
                    MaxLengths.AssociationName));

            // Validate PredicateId.
            if (this.PredicateId == Guid.Empty)
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionPropertyEmpty,
                    DataModellingResources.PredicateId, this.GetType().FullName));

            // Validate Multiplicities.
            if (subjectMultiplicity == AssociationEndMultiplicity.One &&
                objectMultiplicity == AssociationEndMultiplicity.One)
                throw new NotSupportedException(DataModellingResources.InvalidMultiplicityOneToOne);

            // Validate Uri.
            if (string.IsNullOrEmpty(this.Uri))
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionStringPropertyNullOrEmpty,
                    DataModellingResources.Uri, this.GetType().FullName));

            if (this.Uri.Length > MaxLengths.AssociationUri)
                throw new ModelItemValidationException(
                    string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Uri,
                    MaxLengths.AssociationUri));
        }

        /// <summary>
        /// Updates the MSL to creates an association set mapping element.
        /// </summary>
        /// <param name="entityContainerMappingElement">The entity container mapping element.</param>
        /// <param name="associationSetName">Name of the association set.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="storeEntitySet">The store entity set.</param>
        /// <param name="endPropertyName1">The first end property name.</param>
        /// <param name="scalarPropertyName1">The first scalar property name.</param>
        /// <param name="columnName1">The first column name.</param>
        /// <param name="endPropertyName2">The second end property name.</param>
        /// <param name="scalarPropertyName2">The second scalar property name.</param>
        /// <param name="columnName2">The second column name.</param>
        /// <returns>The updated mapping element.</returns>
        private static XmlElement UpdateMslCreateAssociationSetMappingElement(XmlElement entityContainerMappingElement, string associationSetName, string typeName, string storeEntitySet, string endPropertyName1, string scalarPropertyName1, string columnName1, string endPropertyName2, string scalarPropertyName2, string columnName2)
        {
            // Create AssociationSetMapping element.
            XmlElement associationSetMappingElement = Utilities.CreateElement(
                entityContainerMappingElement, DataModellingResources.AssociationSetMapping);
            Utilities.AddAttribute(associationSetMappingElement, DataModellingResources.Name,
                associationSetName);
            Utilities.AddAttribute(associationSetMappingElement, DataModellingResources.TypeName,
                typeName);
            Utilities.AddAttribute(associationSetMappingElement, DataModellingResources.StoreEntitySet,
                storeEntitySet);

            // Create subject EndProperty element.
            XmlElement subjectEndPropertyElement = Utilities.CreateElement(
                associationSetMappingElement, DataModellingResources.EndProperty);
            Utilities.AddAttribute(subjectEndPropertyElement, DataModellingResources.Name,
                endPropertyName1);

            // Create subject ScalarProperty element.
            XmlElement subjectScalarProperty = Utilities.CreateElement(subjectEndPropertyElement,
                DataModellingResources.ScalarProperty);
            Utilities.AddAttribute(subjectScalarProperty, DataModellingResources.Name, scalarPropertyName1);
            Utilities.AddAttribute(subjectScalarProperty, DataModellingResources.ColumnName,
                columnName1);

            // Create object EndProperty element.
            XmlElement objectEndPropertyElement = Utilities.CreateElement(
                associationSetMappingElement, DataModellingResources.EndProperty);
            Utilities.AddAttribute(objectEndPropertyElement, DataModellingResources.Name, endPropertyName2);

            // Create object ScalarProperty element.
            XmlElement objectScalarProperty = Utilities.CreateElement(objectEndPropertyElement,
                DataModellingResources.ScalarProperty);
            Utilities.AddAttribute(objectScalarProperty, DataModellingResources.Name, scalarPropertyName2);
            Utilities.AddAttribute(objectScalarProperty, DataModellingResources.ColumnName,
                columnName2);

            return associationSetMappingElement;
        }

        /// <summary>
        /// Updates the MSL to create association set mapping element.
        /// </summary>
        /// <param name="entityContainerMappingElement">The entity container mapping element.</param>
        /// <param name="associationSetName">Name of the association set.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="storeEntitySet">The store entity set.</param>
        /// <param name="endPropertyName1">The first end property name.</param>
        /// <param name="scalarPropertyName1">The first scalar property name.</param>
        /// <param name="columnName1">The first column name.</param>
        /// <param name="endPropertyName2">The second end property name.</param>
        /// <param name="scalarPropertyName2">The second scalar property name.</param>
        /// <param name="columnName2">The second column name.</param>
        /// <param name="insertFunctionFullName">Full name of the insert function.</param>
        /// <param name="deleteFunctionFullName">Full name of the delete function.</param>
        /// <param name="end1ParameterName">Name of the first end parameter.</param>
        /// <param name="end2ParameterName">Name of the second end parameter.</param>
        /// <returns>The updated mapping element.</returns>
        private static XmlElement UpdateMslCreateAssociationSetMappingElement(XmlElement entityContainerMappingElement, string associationSetName, string typeName, string storeEntitySet, string endPropertyName1, string scalarPropertyName1, string columnName1, string endPropertyName2, string scalarPropertyName2, string columnName2, string insertFunctionFullName, string deleteFunctionFullName, string end1ParameterName, string end2ParameterName)
        {
            // Create AssociationSetMapping element.
            XmlElement associationSetMappingElement = UpdateMslCreateAssociationSetMappingElement(
                entityContainerMappingElement, associationSetName, typeName, storeEntitySet, endPropertyName1,
                scalarPropertyName1, columnName1, endPropertyName2, scalarPropertyName2, columnName2);

            // Create ModificationFunctionMapping element.
            XmlElement modificationFunctionMappingElement =
                Utilities.CreateElement(associationSetMappingElement,
                DataModellingResources.ModificationFunctionMapping);

            // Create InsertFunction element.
            XmlElement insertFunctionElement =
                Utilities.CreateElement(modificationFunctionMappingElement,
                DataModellingResources.InsertFunction);
            Utilities.AddAttribute(insertFunctionElement, DataModellingResources.FunctionName,
                insertFunctionFullName);
            XmlElement subjectEndPropertyElement = Utilities.CreateElement(
                insertFunctionElement, DataModellingResources.EndProperty);
            Utilities.AddAttribute(subjectEndPropertyElement, DataModellingResources.Name,
                endPropertyName1);
            XmlElement subjectScalarProperty = Utilities.CreateElement(subjectEndPropertyElement,
                DataModellingResources.ScalarProperty);
            Utilities.AddAttribute(subjectScalarProperty, DataModellingResources.Name, scalarPropertyName1);
            Utilities.AddAttribute(subjectScalarProperty, DataModellingResources.ParameterName,
                end1ParameterName);
            XmlElement objectEndPropertyElement = Utilities.CreateElement(
                insertFunctionElement, DataModellingResources.EndProperty);
            Utilities.AddAttribute(objectEndPropertyElement, DataModellingResources.Name,
                endPropertyName2);
            XmlElement objectScalarProperty = Utilities.CreateElement(objectEndPropertyElement,
                DataModellingResources.ScalarProperty);
            Utilities.AddAttribute(objectScalarProperty, DataModellingResources.Name, scalarPropertyName2);
            Utilities.AddAttribute(objectScalarProperty, DataModellingResources.ParameterName,
                end2ParameterName);

            // Create DeleteFunction element.
            XmlElement deleteFunctionElement =
                Utilities.CreateElement(modificationFunctionMappingElement,
                DataModellingResources.DeleteFunction);
            Utilities.AddAttribute(deleteFunctionElement, DataModellingResources.FunctionName,
                deleteFunctionFullName);
            subjectEndPropertyElement = Utilities.CreateElement(
                deleteFunctionElement, DataModellingResources.EndProperty);
            Utilities.AddAttribute(subjectEndPropertyElement, DataModellingResources.Name,
                endPropertyName1);
            subjectScalarProperty = Utilities.CreateElement(subjectEndPropertyElement,
                DataModellingResources.ScalarProperty);
            Utilities.AddAttribute(subjectScalarProperty, DataModellingResources.Name, scalarPropertyName1);
            Utilities.AddAttribute(subjectScalarProperty, DataModellingResources.ParameterName,
                end1ParameterName);
            objectEndPropertyElement = Utilities.CreateElement(
                deleteFunctionElement, DataModellingResources.EndProperty);
            Utilities.AddAttribute(objectEndPropertyElement, DataModellingResources.Name,
                endPropertyName2);
            objectScalarProperty = Utilities.CreateElement(objectEndPropertyElement,
                DataModellingResources.ScalarProperty);
            Utilities.AddAttribute(objectScalarProperty, DataModellingResources.Name, scalarPropertyName2);
            Utilities.AddAttribute(objectScalarProperty, DataModellingResources.ParameterName,
                end2ParameterName);

            return associationSetMappingElement;
        }

        /// <summary>
        /// Updates the SSDL to handle one to XXX associations.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="schemaElement">The schema element.</param>
        /// <param name="xxxSidePropertyId">The XXX side property id.</param>
        /// <param name="tableMappings">The table mappings.</param>
        private static void UpdateSsdlHandleOneToXxxAssociations(XmlElement entityContainerElement, XmlElement schemaElement, Guid xxxSidePropertyId, TableMappingCollection tableMappings)
        {
            // Locate the table and column mappings for this property.
            ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                xxxSidePropertyId);
            TableMapping tableMapping = columnMapping.Parent;

            string tableName = tableMapping.TableName;
            string columnName = columnMapping.ColumnName;
            string storageSchemaNamespace = schemaElement.Attributes[DataModellingResources.Namespace].
                Value;
            string tableFullName = Utilities.MergeSubNames(
                storageSchemaNamespace, tableName);

            // Locate the EntityType for this table.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(
                entityContainerElement.OwnerDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.SSDLNamespacePrefix,
                DataModellingResources.SSDLSchemaNameSpace);
            XmlNodeList entityTypes = schemaElement.SelectNodes(string.Format(
                CultureInfo.InvariantCulture, DataModellingResources.XPathSSDLEntityType, tableName),
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
                    DataModellingResources.Core);

                // Create AssociationSet element.
                string fkConstraintName = string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.FKConstraintName, tableName);
                string fkConstraintFullName = Utilities.MergeSubNames(
                    storageSchemaNamespace, fkConstraintName);
                Utilities.AddSsdlAssociationSet(entityContainerElement, fkConstraintName,
                    fkConstraintFullName, DataModellingResources.Resource, DataModellingResources.Resource,
                    tableName, tableName);

                // Create EntityType element
                entityTypeElement = Utilities.AddSsdlEntityType(schemaElement, tableName,
                    DataModellingResources.Id);

                // Add Id and Discriminator properties.
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, DataModellingResources.Id,
                    DataModellingResources.DataTypeUniqueidentifier, false);
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement,
                    DataModellingResources.Discriminator, DataModellingResources.DataTypeInt, false);

                // Create Association element.
                Utilities.AddSsdlAssociation(schemaElement, fkConstraintName,
                    DataModellingResources.Resource, Utilities.MergeSubNames(
                    storageSchemaNamespace, DataModellingResources.Resource),
                    DataModellingResources.One, tableName, tableFullName, DataModellingResources.ZeroOrOne,
                    DataModellingResources.Resource, new string[] { DataModellingResources.Id }, tableName,
                    new string[] { DataModellingResources.Id });
            }
            else
                entityTypeElement = entityTypes[0] as XmlElement;

            // Add the foreign key column to EntityType element. Take note that all
            // foreign key columns are nullable. This is necessary since the table
            // might also host rows for some other entity type and while inserting
            // rows for the other entity type, EF inserts NULL values for this entity
            // type.
            Utilities.AddSsdlEntityTypeProperty(entityTypeElement, columnName,
                DataModellingResources.DataTypeUniqueidentifier, true);

            // Add AssociationSet element for the foreign key. It is possible that the FK
            // column is hosted by Resource table. To create different roles, we use the
            // column name to distinguish between the roles.
            string associationFKConstraintName = string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.FKConstraintName, columnName);
            string associationFKConstraintFullName = Utilities.MergeSubNames(
                storageSchemaNamespace, associationFKConstraintName);
            Utilities.AddSsdlAssociationSet(entityContainerElement, associationFKConstraintName,
                associationFKConstraintFullName, DataModellingResources.Resource, DataModellingResources.Resource,
                columnName, tableName);

            // Add Association element.
            Utilities.AddSsdlAssociation(schemaElement, associationFKConstraintName,
                DataModellingResources.Resource, Utilities.MergeSubNames(
                storageSchemaNamespace, DataModellingResources.Resource),
                DataModellingResources.ZeroOrOne, columnName, tableFullName, DataModellingResources.Many,
                DataModellingResources.Resource, new string[] { DataModellingResources.Id }, columnName,
                new string[] { columnName });
        }

        /// <summary>
        /// Updates the flattened SSDL to handle one to XXX associations.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="schemaElement">The schema element.</param>
        /// <param name="xxxSidePropertyId">The XXX side property id.</param>
        /// <param name="xxxSideRoleName">Name of the XXX side role.</param>
        /// <param name="tableMappings">The table mappings.</param>
        private static void UpdateFlattenedSsdlHandleOneToXxxAssociations(XmlElement entityContainerElement, XmlElement schemaElement, Guid xxxSidePropertyId, string xxxSideRoleName, TableMappingCollection tableMappings)
        {
            // Locate the table and column mappings for this property.
            ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(xxxSidePropertyId);
            TableMapping tableMapping = columnMapping.Parent;

            string tableName = tableMapping.TableName;
            string columnName = columnMapping.ColumnName;
            string storageSchemaNamespace = schemaElement.Attributes[DataModellingResources.Namespace].Value;
            string tableFullName = Utilities.MergeSubNames(storageSchemaNamespace, tableName);

            // Locate the EntityType for this table.
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(entityContainerElement.OwnerDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.SSDLNamespacePrefix, DataModellingResources.SSDLSchemaNameSpace);
            XmlNodeList entityTypes = schemaElement.SelectNodes(string.Format(CultureInfo.InvariantCulture, DataModellingResources.XPathSSDLEntityType, tableName), nsMgr);
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
                Utilities.AddSsdlEntitySetForTables(entityContainerElement, tableName, tableFullName, DataModellingResources.Core);

                // Create AssociationSet element.
                string fkConstraintName = string.Format(CultureInfo.InvariantCulture, DataModellingResources.FKConstraintName, tableName);
                string fkConstraintFullName = Utilities.MergeSubNames(storageSchemaNamespace, fkConstraintName);
                Utilities.AddSsdlAssociationSet(entityContainerElement, fkConstraintName,
                    fkConstraintFullName, xxxSideRoleName, xxxSideRoleName,
                    tableName, tableName);

                // Create EntityType element
                entityTypeElement = Utilities.AddSsdlEntityType(schemaElement, tableName, DataModellingResources.Id);

                // Add Id and Discriminator properties.
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, DataModellingResources.Id, DataModellingResources.DataTypeUniqueidentifier, false);
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, DataModellingResources.Discriminator, DataModellingResources.DataTypeInt, false);

                // Create Association element.
                Utilities.AddSsdlAssociation(schemaElement, fkConstraintName,
                    DataModellingResources.Resource, Utilities.MergeSubNames(
                    storageSchemaNamespace, DataModellingResources.Resource),
                    DataModellingResources.One, tableName, tableFullName, DataModellingResources.ZeroOrOne,
                    DataModellingResources.Resource, new string[] { DataModellingResources.Id }, tableName,
                    new string[] { DataModellingResources.Id });
            }
            else
                entityTypeElement = entityTypes[0] as XmlElement;

            // Add the foreign key column to EntityType element. Take note that all
            // foreign key columns are nullable. This is necessary since the table
            // might also host rows for some other entity type and while inserting
            // rows for the other entity type, EF inserts NULL values for this entity
            // type.
            Utilities.AddSsdlEntityTypeProperty(entityTypeElement, columnName, DataModellingResources.DataTypeUniqueidentifier, true);

            // Add AssociationSet element for the foreign key. It is possible that the FK
            // column is hosted by Resource table. To create different roles, we use the
            // column name to distinguish between the roles.
            string associationFKConstraintName = string.Format(CultureInfo.InvariantCulture, DataModellingResources.FKConstraintName, columnName);
            string associationFKConstraintFullName = Utilities.MergeSubNames(storageSchemaNamespace, associationFKConstraintName);
            Utilities.AddSsdlAssociationSet(entityContainerElement, associationFKConstraintName,
                associationFKConstraintFullName, xxxSideRoleName, xxxSideRoleName,
                columnName, tableName);

            // Add Association element.
            Utilities.AddSsdlAssociation(schemaElement, associationFKConstraintName,
                xxxSideRoleName, Utilities.MergeSubNames(storageSchemaNamespace, xxxSideRoleName),
                DataModellingResources.ZeroOrOne, columnName, tableFullName, DataModellingResources.Many,
                xxxSideRoleName, new string[] { DataModellingResources.Id }, 
                columnName, new string[] { columnName });
        }

        /// <summary>
        /// Updates the SSDL handle view associations.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="schemaElement">The schema element.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="subjectFKConstraintName">Name of the subject FK constraint.</param>
        /// <param name="objectFKConstraintName">Name of the object FK constraint.</param>
        /// <param name="viewKeyColumns">The view key columns.</param>
        /// <param name="subjectSideResourceMultiplicity">The subject side resource multiplicity.</param>
        /// <param name="subjectSideViewMultiplicity">The subject side view multiplicity.</param>
        /// <param name="objectSideResourceMultiplicity">The object side resource multiplicity.</param>
        /// <param name="objectSideViewMultiplicity">The object side view multiplicity.</param>
        /// <param name="insertProcedureName">Name of the insert procedure.</param>
        /// <param name="deleteProcedureName">Name of the delete procedure.</param>
        private static void UpdateSsdlHandleViewAssociations(XmlElement entityContainerElement, XmlElement schemaElement, string viewName, string subjectFKConstraintName, string objectFKConstraintName, string[] viewKeyColumns, string subjectSideResourceMultiplicity, string subjectSideViewMultiplicity, string objectSideResourceMultiplicity, string objectSideViewMultiplicity, string insertProcedureName, string deleteProcedureName)
        {
            string storageNamespace = schemaElement.Attributes[DataModellingResources.Namespace].Value;
            string viewFullName = Utilities.MergeSubNames(
                storageNamespace, viewName);
            string subjectFKConstraintFullName = Utilities.MergeSubNames(
                storageNamespace, subjectFKConstraintName);
            string objectFKConstraintFullName = Utilities.MergeSubNames(
                storageNamespace, objectFKConstraintName);
            string resourceTableFullName = Utilities.MergeSubNames(
                storageNamespace, DataModellingResources.Resource);

            // Create EntitySet element.
            Utilities.AddSsdlEntitySetForViews(entityContainerElement, viewName, viewFullName,
                DataModellingResources.Core, viewName);

            // Create AssociationSet element for subject foreign key.
            Utilities.AddSsdlAssociationSet(entityContainerElement, subjectFKConstraintName,
                subjectFKConstraintFullName, viewName, viewName, DataModellingResources.Resource,
                DataModellingResources.Resource);

            // Create AssociationSet element for object foreign key.
            Utilities.AddSsdlAssociationSet(entityContainerElement, objectFKConstraintName,
                objectFKConstraintFullName, viewName, viewName, DataModellingResources.Resource,
                DataModellingResources.Resource);

            // Create EntityType element with PropertyRef element.
            XmlElement entityTypeElement = Utilities.AddSsdlEntityType(schemaElement, viewName,
                viewKeyColumns);

            // Add Property elements to EntityType element.
            Utilities.AddSsdlEntityTypeProperty(entityTypeElement,
                DataModellingResources.SubjectResourceId, Utilities.GetSQLType(DataTypes.Guid), false);
            Utilities.AddSsdlEntityTypeProperty(entityTypeElement,
                DataModellingResources.ObjectResourceId, Utilities.GetSQLType(DataTypes.Guid), false);

            // Create Association element for subject foreign key.
            Utilities.AddSsdlAssociation(schemaElement, subjectFKConstraintName, viewName,
                viewFullName, subjectSideViewMultiplicity, DataModellingResources.Resource,
                resourceTableFullName, subjectSideResourceMultiplicity, DataModellingResources.Resource,
                new string[] { DataModellingResources.Id }, viewName,
                new string[] { DataModellingResources.SubjectResourceId });

            // Create Association element for object foreign key.
            Utilities.AddSsdlAssociation(schemaElement, objectFKConstraintName, viewName,
                viewFullName, objectSideViewMultiplicity, DataModellingResources.Resource,
                resourceTableFullName, objectSideResourceMultiplicity, DataModellingResources.Resource,
                new string[] { DataModellingResources.Id }, viewName,
                new string[] { DataModellingResources.ObjectResourceId });

            // Add Create Function.
            Utilities.AddSsdlFunction(schemaElement, insertProcedureName, false, false, false,
                false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                new string[] { DataModellingResources.SubjectResourceId, 
                    DataModellingResources.DataTypeUniqueidentifier, DataModellingResources.In, 
                    DataModellingResources.ObjectResourceId, 
                    DataModellingResources.DataTypeUniqueidentifier, DataModellingResources.In });

            // Add Delete Function.
            Utilities.AddSsdlFunction(schemaElement, deleteProcedureName, false, false, false,
                false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                new string[] { DataModellingResources.SubjectResourceId, 
                    DataModellingResources.DataTypeUniqueidentifier, DataModellingResources.In, 
                    DataModellingResources.ObjectResourceId, 
                    DataModellingResources.DataTypeUniqueidentifier, DataModellingResources.In });
        }

        /// <summary>
        /// Updates the flattened SSDL to handle view associations.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="schemaElement">The schema element.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="subjectRoleName">Name of the subject role.</param>
        /// <param name="objectRoleName">Name of the object role.</param>
        /// <param name="subjectFKConstraintName">Name of the subject FK constraint.</param>
        /// <param name="objectFKConstraintName">Name of the object FK constraint.</param>
        /// <param name="viewKeyColumns">The view key columns.</param>
        /// <param name="subjectSideResourceMultiplicity">The subject side resource multiplicity.</param>
        /// <param name="subjectSideViewMultiplicity">The subject side view multiplicity.</param>
        /// <param name="objectSideResourceMultiplicity">The object side resource multiplicity.</param>
        /// <param name="objectSideViewMultiplicity">The object side view multiplicity.</param>
        /// <param name="insertProcedureName">Name of the insert procedure.</param>
        /// <param name="deleteProcedureName">Name of the delete procedure.</param>
        private static void UpdateFlattenedSsdlHandleViewAssociations(XmlElement entityContainerElement, XmlElement schemaElement, string viewName, string subjectRoleName, string objectRoleName, string subjectFKConstraintName, string objectFKConstraintName, string[] viewKeyColumns, string subjectSideResourceMultiplicity, string subjectSideViewMultiplicity, string objectSideResourceMultiplicity, string objectSideViewMultiplicity, string insertProcedureName, string deleteProcedureName)
        {
            string storageNamespace = schemaElement.Attributes[DataModellingResources.Namespace].Value;
            string viewFullName = Utilities.MergeSubNames(storageNamespace, viewName);
            string viewCustomName = viewName + "_" + subjectRoleName + objectRoleName;
            string viewCustomFullName = Utilities.MergeSubNames(storageNamespace, viewCustomName);
            string subjectFKConstraintFullName = Utilities.MergeSubNames(storageNamespace, subjectFKConstraintName);
            string objectFKConstraintFullName = Utilities.MergeSubNames(storageNamespace, objectFKConstraintName);
            string subjectTableFullName = Utilities.MergeSubNames(storageNamespace, subjectRoleName);
            string objectTableFullName = Utilities.MergeSubNames(storageNamespace, objectRoleName);

            // Create EntitySet element.
            //Utilities.AddSsdlEntitySetForViews(entityContainerElement, viewCustomName, viewFullName, DataModellingResources.Core, viewName);
            Utilities.AddSsdlEntitySetWithDefiningQuery(entityContainerElement, viewCustomName, viewFullName,
                                                        string.Format(DataModellingResources.ViewDefiningQueryFormat, viewName));

            //// Create AssociationSet element for subject foreign key.
            //Utilities.AddSsdlAssociationSet(entityContainerElement, subjectFKConstraintName,
            //    subjectFKConstraintFullName, viewCustomName, viewCustomName, subjectRoleName, subjectRoleName);

            //// Create AssociationSet element for object foreign key.
            //Utilities.AddSsdlAssociationSet(entityContainerElement, objectFKConstraintName,
            //    objectFKConstraintFullName, viewCustomName, viewCustomName, objectRoleName, objectRoleName);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(entityContainerElement.OwnerDocument.NameTable);
            nsMgr.AddNamespace(DataModellingResources.SSDLNamespacePrefix, DataModellingResources.SSDLSchemaNameSpace);
            XmlNodeList entityTypes = schemaElement.SelectNodes(string.Format(CultureInfo.InvariantCulture, DataModellingResources.XPathSSDLEntityType, viewName), nsMgr);

            if (entityTypes == null || entityTypes.Count == 0)
            {
                // Create EntityType element with PropertyRef element.
                XmlElement entityTypeElement = Utilities.AddSsdlEntityType(schemaElement, viewName, viewKeyColumns);

                // Add Property elements to EntityType element.
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, DataModellingResources.SubjectResourceId,
                                                    Utilities.GetSQLType(DataTypes.Guid), false);
                Utilities.AddSsdlEntityTypeProperty(entityTypeElement, DataModellingResources.ObjectResourceId,
                                                    Utilities.GetSQLType(DataTypes.Guid), false);

                // Add Create Function.
                Utilities.AddSsdlFunction(schemaElement, insertProcedureName, false, false, false,
                    false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                    new string[] { DataModellingResources.SubjectResourceId, 
                    DataModellingResources.DataTypeUniqueidentifier, DataModellingResources.In, 
                    DataModellingResources.ObjectResourceId, 
                    DataModellingResources.DataTypeUniqueidentifier, DataModellingResources.In });

                // Add Delete Function.
                Utilities.AddSsdlFunction(schemaElement, deleteProcedureName, false, false, false,
                    false, DataModellingResources.AllowImplicitConversion, DataModellingResources.Core,
                    new string[] { DataModellingResources.SubjectResourceId, 
                    DataModellingResources.DataTypeUniqueidentifier, DataModellingResources.In, 
                    DataModellingResources.ObjectResourceId, 
                    DataModellingResources.DataTypeUniqueidentifier, DataModellingResources.In });
            }

            // Create Association element for subject foreign key.
            //Utilities.AddSsdlAssociation(schemaElement, subjectFKConstraintName, viewCustomName,
            //    viewFullName, subjectSideViewMultiplicity, subjectRoleName,
            //    subjectTableFullName, subjectSideResourceMultiplicity, 
            //    subjectRoleName, new string[] { DataModellingResources.Id },
            //    viewCustomName, new string[] { DataModellingResources.SubjectResourceId });

            //// Create Association element for object foreign key.
            //Utilities.AddSsdlAssociation(schemaElement, objectFKConstraintName, viewCustomName,
            //    viewFullName, objectSideViewMultiplicity, objectRoleName,
            //    objectTableFullName, objectSideResourceMultiplicity, 
            //    objectRoleName, new string[] { DataModellingResources.Id },
            //    viewCustomName, new string[] { DataModellingResources.ObjectResourceId });
        }

        #endregion
    }
}
