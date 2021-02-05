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

namespace Zentity.Core
{
    internal interface IResourceTypeProperty
    {

        /// <summary>
        /// Gets the identifier of this property.
        /// </summary>
        Guid Id
        {
            get;
        }

        /// <summary>
        /// Gets the resource type that hosts this property.
        /// </summary>
        ResourceType Parent
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
        /// Gets or sets the Uri of this property.
        /// </summary>
        string Uri
        {
            get;
            set;
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
        /// Gets the table name to which this property is mapped.
        /// </summary>
        string TableName
        {
            get;
        }

        /// <summary>
        /// Gets the column name to which this property is mapped.
        /// </summary>
        string ColumnName
        {
            get;
        }
    }
}
