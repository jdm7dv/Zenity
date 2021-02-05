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
using System.Linq;
using System.Xml;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a navigation property of a resource type.
    /// </summary>
    public sealed class NavigationProperty : IResourceTypeProperty
    {
        #region Fields

        Guid id;
        ResourceType parent;
        string name;
        string uri;
        string description;
        Association association;
        AssociationEndType direction;
        string tableName;
        string columnName;

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
        /// Gets the table name to which this property is mapped.
        /// </summary>
        public string TableName
        {
            get { return tableName; }
            internal set { tableName = value; }
        }

        /// <summary>
        /// Gets the identifier of this navigation property.
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
            internal set
            {
                parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of this navigation property.
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
        /// Gets or sets the Uri of this navigation property.
        /// </summary>
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }

        /// <summary>
        /// Gets or sets the description of this navigation property.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

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
        /// Gets the direction of this navigation property in the association.
        /// </summary>
        public AssociationEndType Direction
        {
            get { return direction; }
            internal set { direction = value; }
        }

        /// <summary>
        /// Gets the full name of this navigation property.
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

        internal void Validate()
        {
            // Validate Id
            if (this.Id == Guid.Empty)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.InvalidIdEmpty));

            // Validate Name.
            if (string.IsNullOrEmpty(this.Name))
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionStringPropertyNullOrEmpty, CoreResources.Name,
                    CoreResources.NavigationProperty));

            if (this.Name.Length > MaxLengths.resourceTypePropertyName)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Name,
                    MaxLengths.resourceTypePropertyName));

            // Validating the Name for a valid C# property name makes the validation process slow.
            // So we are not including that check here. This check is included at the DataModel
            // level.

            // Validate Uri.
            if (!string.IsNullOrEmpty(this.Uri)
                && this.Uri.Length > MaxLengths.resourceTypePropertyUri)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength,
                    CoreResources.Uri, MaxLengths.resourceTypePropertyUri));

            // Validate Description.
            if (!string.IsNullOrEmpty(this.Description)
                && this.Description.Length > MaxLengths.resourceTypePropertyDescription)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Description,
                    MaxLengths.resourceTypePropertyDescription));
        }

        internal void UpdateCsdl(XmlElement eEntityType)
        {
            // Create Property element.
            XmlElement eNavigationProperty =
                Utilities.CreateElement(eEntityType, CoreResources.NavigationProperty);

            // Create Name attribute.
            Utilities.AddAttribute(eNavigationProperty, CoreResources.Name, this.Name);

            // Create Relationship attribute.
            Utilities.AddAttribute(eNavigationProperty, CoreResources.Relationship,
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
            Utilities.AddAttribute(eNavigationProperty, CoreResources.FromRole, fromRole);
            Utilities.AddAttribute(eNavigationProperty, CoreResources.ToRole, toRole);

            // Create Documentation element.
            if (!string.IsNullOrEmpty(this.Description))
            {
                XmlElement eDocumentation = Utilities.CreateElement(eNavigationProperty,
                    CoreResources.Documentation);
                XmlElement eSummary = Utilities.CreateElement(eDocumentation,
                    CoreResources.Summary);
                eSummary.InnerText = this.Description;
            }
        }

        #endregion
    }
}
