// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a navigation property of a resource type.
    /// </summary>
    [DataContract]
    public sealed class NavigationProperty : IResourceTypeProperty
    {
        #region Fields

        Association association;
        string columnName;
        string description;
        AssociationEndType direction;
        Guid id;
        string name;
        ResourceType parent;
        string tableName;
        string uri;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the association in which this navigation property participates.
        /// </summary>
        public Association Association
        {
            get { return association; }
            internal set
            {
                association = value;
            }
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
        /// Gets or sets the description of this navigation property.
        /// </summary>
        [DataMember]
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets the direction of this navigation property in the association.
        /// </summary>
        [DataMember]
        public AssociationEndType Direction
        {
            get { return direction; }
            internal set { direction = value; }
        }

        /// <summary>
        /// Gets the identifier of this navigation property.
        /// </summary>
        [DataMember]
        public Guid Id
        {
            get { return id; }
            internal set { id = value; }
        }

        /// <summary>
        /// Gets or sets the name of this navigation property.
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
        /// Gets the resource type that hosts this property.
        /// </summary>
        public ResourceType Parent
        {
            get { return parent; }
            internal set
            {
                parent = value;
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
        /// Gets or sets the Uri of this navigation property.
        /// </summary>
        [DataMember]
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }

        /// <summary>
        /// Gets the full name of this navigation property.
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

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of <see cref="Zentity.Core.NavigationProperty"/> class.
        /// </summary>
        public NavigationProperty()
            : this(null)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="Zentity.Core.NavigationProperty"/> class.
        /// </summary>
        /// <param name="name">Property name.</param>
        public NavigationProperty(string name)
        {
            this.id = Guid.NewGuid();
            this.Name = name;
            this.Direction = AssociationEndType.Undefined;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates the CSDL.
        /// </summary>
        /// <param name="eEntityType">The entity type element.</param>
        internal void UpdateCsdl(XmlElement eEntityType)
        {
            // Create Property element.
            XmlElement eNavigationProperty =
                Utilities.CreateElement(eEntityType, DataModellingResources.NavigationProperty);

            // Create Name attribute.
            Utilities.AddAttribute(eNavigationProperty, DataModellingResources.Name, this.Name);

            // Create Relationship attribute.
            Utilities.AddAttribute(eNavigationProperty, DataModellingResources.Relationship,
                association.FullName);

            // Set FromRole and ToRole attributes.
            string fromRole, toRole;
            if (this.Direction == AssociationEndType.Subject)
            {
                fromRole = this.Association.SubjectRole;
                toRole = this.Association.ObjectRole;
            }
            else
            {
                fromRole = this.Association.ObjectRole;
                toRole = this.Association.SubjectRole;
            }
            Utilities.AddAttribute(eNavigationProperty, DataModellingResources.FromRole, fromRole);
            Utilities.AddAttribute(eNavigationProperty, DataModellingResources.ToRole, toRole);

            // Create Documentation element.
            if (!string.IsNullOrEmpty(this.Description))
            {
                XmlElement eDocumentation = Utilities.CreateElement(eNavigationProperty,
                    DataModellingResources.Documentation);
                XmlElement eSummary = Utilities.CreateElement(eDocumentation,
                    DataModellingResources.Summary);
                eSummary.InnerText = this.Description;
            }
        }

        /// <summary>
        /// Updates the flattened CSDL.
        /// </summary>
        /// <param name="eEntityType">The entity type element.</param>
        /// <param name="fromResourceType">The resource type.</param>
        internal void UpdateFlattenedCsdl(XmlElement eEntityType, ResourceType fromResourceType)
        {
            if (this.Association.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                this.Association.ObjectMultiplicity == AssociationEndMultiplicity.One)
            {
                if (this.direction == AssociationEndType.Subject)
                {
                    // Create Property element.
                    XmlElement eProperty = Utilities.CreateElement(eEntityType, DataModellingResources.Property);
                    Utilities.AddAttribute(eProperty, DataModellingResources.Name, this.association.ObjectNavigationProperty.Parent.Name + "_Id");
                    Utilities.AddAttribute(eProperty, DataModellingResources.Type, DataTypes.Guid.ToString());
                    Utilities.AddAttribute(eProperty, DataModellingResources.Nullable, false.ToString().ToLowerInvariant());
                }
            }

            if (this.Association.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                this.Association.ObjectMultiplicity == AssociationEndMultiplicity.Many)
            {
                if (this.direction == AssociationEndType.Object)
                {
                    // Create Property element.
                    XmlElement eProperty = Utilities.CreateElement(eEntityType, DataModellingResources.Property);
                    Utilities.AddAttribute(eProperty, DataModellingResources.Name, this.association.SubjectNavigationProperty.Parent.Name + "_Id");
                    Utilities.AddAttribute(eProperty, DataModellingResources.Type, DataTypes.Guid.ToString());
                    Utilities.AddAttribute(eProperty, DataModellingResources.Nullable, false.ToString().ToLowerInvariant());
                }
            }

            // Check the Association direction and create the respective Navigation property elements for the EntityType
            if (this.Direction == AssociationEndType.Subject)
            {
                var objectDerivedTypes = this.Association.ObjectNavigationProperty.Parent.GetDerivedTypes();

                foreach (var objectType in objectDerivedTypes)
                {
                    // Create Property element.
                    XmlElement eNavigationProperty = Utilities.CreateElement(eEntityType, DataModellingResources.NavigationProperty);

                    // Create Name attribute.
                    if (this.Parent.FullName.Equals(typeof(Resource).FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        Utilities.AddAttribute(eNavigationProperty, DataModellingResources.Name, this.Name);
                    }
                    else
                    {
                        Utilities.AddAttribute(eNavigationProperty, DataModellingResources.Name,
                                               string.Format(CultureInfo.InvariantCulture, DataModellingResources.SuffixedNameFormat, this.Name, objectType.Name));
                    }

                    string targetNamespace = fromResourceType.Parent.NameSpace;
                    string associationFullName = (this.Name == DataModellingResources.Files) ? Utilities.MergeSubNames(targetNamespace, this.association.Name) : association.FullName;

                    // Create Relationship attribute.
                    Utilities.AddAttribute(eNavigationProperty, DataModellingResources.Relationship,
                                           string.Format(CultureInfo.InvariantCulture, DataModellingResources.SuffixedNameFormat,
                                                         associationFullName, fromResourceType.Name + objectType.Name));

                    string fromRoleName = fromResourceType.Name;
                    string toRoleName = objectType.Name;

                    if (fromRoleName.Equals(toRoleName, StringComparison.OrdinalIgnoreCase))
                    {
                        fromRoleName += "1";
                        toRoleName += "2";
                    }

                    Utilities.AddAttribute(eNavigationProperty, DataModellingResources.FromRole, fromRoleName);
                    Utilities.AddAttribute(eNavigationProperty, DataModellingResources.ToRole, toRoleName);

                    // Create Documentation element.
                    if (!string.IsNullOrEmpty(this.Description))
                    {
                        XmlElement eDocumentation = Utilities.CreateElement(eNavigationProperty, DataModellingResources.Documentation);
                        XmlElement eSummary = Utilities.CreateElement(eDocumentation, DataModellingResources.Summary);
                        eSummary.InnerText = this.Description;
                    }
                }
            }
            else
            {
                var subjectDerivedTypes = this.Association.SubjectNavigationProperty.Parent.GetDerivedTypes();

                foreach (var subjectType in subjectDerivedTypes)
                {
                    // Create Property element.
                    XmlElement eNavigationProperty = Utilities.CreateElement(eEntityType, DataModellingResources.NavigationProperty);

                    // Create Name attribute.
                    Utilities.AddAttribute(eNavigationProperty, DataModellingResources.Name,
                                           string.Format(CultureInfo.InvariantCulture, DataModellingResources.SuffixedNameFormat, this.Name, subjectType.Name));

                    // Create Relationship attribute.
                    Utilities.AddAttribute(eNavigationProperty, DataModellingResources.Relationship,
                                           string.Format(CultureInfo.InvariantCulture, DataModellingResources.SuffixedNameFormat,
                                                         association.FullName, subjectType.Name + fromResourceType.Name));

                    string fromRoleName = fromResourceType.Name;
                    string toRoleName = subjectType.Name;

                    if (fromRoleName.Equals(toRoleName, StringComparison.OrdinalIgnoreCase))
                    {
                        fromRoleName += "2";
                        toRoleName += "1";
                    }

                    Utilities.AddAttribute(eNavigationProperty, DataModellingResources.FromRole, fromRoleName);
                    Utilities.AddAttribute(eNavigationProperty, DataModellingResources.ToRole, toRoleName);

                    // Create Documentation element.
                    if (!string.IsNullOrEmpty(this.Description))
                    {
                        XmlElement eDocumentation = Utilities.CreateElement(eNavigationProperty, DataModellingResources.Documentation);
                        XmlElement eSummary = Utilities.CreateElement(eDocumentation, DataModellingResources.Summary);
                        eSummary.InnerText = this.Description;
                    }
                }
            }
        }

        /// <summary>
        /// Validates this navigation property instance for inconsistencies.
        /// </summary>
        internal void Validate()
        {
            // Validate Id
            if (this.Id == Guid.Empty)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.InvalidIdEmpty));

            // Validate Name.
            if (string.IsNullOrEmpty(this.Name))
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionStringPropertyNullOrEmpty, DataModellingResources.Name,
                    DataModellingResources.NavigationProperty));

            if (this.Name.Length > MaxLengths.ResourceTypePropertyName)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Name,
                    MaxLengths.ResourceTypePropertyName));

            // Validating the Name for a valid C# property name makes the validation process slow.
            // So we are not including that check here. This check is included at the DataModel
            // level.

            // Validate Uri.
            if (!string.IsNullOrEmpty(this.Uri)
                && this.Uri.Length > MaxLengths.ResourceTypePropertyUri)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength,
                    DataModellingResources.Uri, MaxLengths.ResourceTypePropertyUri));

            // Validate Description.
            if (!string.IsNullOrEmpty(this.Description)
                && this.Description.Length > MaxLengths.ResourceTypePropertyDescription)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionInvalidLength, DataModellingResources.Description,
                    MaxLengths.ResourceTypePropertyDescription));
        }

        #endregion
    }
}
