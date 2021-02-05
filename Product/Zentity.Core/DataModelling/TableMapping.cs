// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Core
{
    /// <summary>
    /// Structure to store the mapping of table columns with scalar or navigation properties.
    /// </summary>
    internal sealed class TableMapping
    {
        ColumnMappingCollection columnMappings;
        string tableName;

        /// <summary>
        /// Gets the column mappings.
        /// </summary>
        /// <value>The column mappings.</value>
        internal ColumnMappingCollection ColumnMappings
        {
            get { return columnMappings; }
        }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        internal string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableMapping"/> class.
        /// </summary>
        internal TableMapping()
        {
            this.columnMappings = new ColumnMappingCollection(this);
        }
    }
}
