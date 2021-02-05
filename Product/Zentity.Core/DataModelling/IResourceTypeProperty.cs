// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;

namespace Zentity.Core
{
    /// <summary>
    /// Defines the interface for ResourceType properties
    /// </summary>
    internal interface IResourceTypeProperty
    {
        /// <summary>
        /// Gets the column name to which this property is mapped.
        /// </summary>
        string ColumnName
        {
            get;
        }

        /// <summary>
        /// Gets or sets the description of this property.
        /// </summary>
        string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the identifier of this property.
        /// </summary>
        Guid Id
        {
            get;
        }

        /// <summary>
        /// Gets or sets the name of this property.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the resource type that hosts this property.
        /// </summary>
        ResourceType Parent
        {
            get;
        }

        /// <summary>
        /// Gets the table name to which this property is mapped.
        /// </summary>
        string TableName
        {
            get;
        }

        /// <summary>
        /// Gets or sets the Uri of this property.
        /// </summary>
        string Uri
        {
            get;
            set;
        }
    }
}
