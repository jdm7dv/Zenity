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

namespace Zentity.Core
{
    /// <summary>
    /// Structure to store the mapping of table columns with scalar or navigation properties.
    /// </summary>
    internal sealed class TableMapping
    {
        string tableName;
        ColumnMappingCollection columnMappings;

        internal string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        internal ColumnMappingCollection ColumnMappings
        {
            get { return columnMappings; }
        }

        internal TableMapping()
        {
            this.columnMappings = new ColumnMappingCollection(this);
        }
    }
}
