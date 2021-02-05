// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;

namespace Zentity.Core
{
    /// <summary>
    /// Structure to store the mapping of a table column with either a scalar or a
    /// navigation property. There might be some columns that have their propertyName
    /// as NULL references. This means that they are not mapped directly to any model
    /// property.
    /// </summary>
    internal sealed class ColumnMapping
    {
        #region Fields

        string columnName;
        bool isMapped; // Initial value = false.
        bool isScalarProperty;
        TableMapping parent;
        Guid propertyId;

        #endregion 

        #region Properties

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        internal string ColumnName
        {
            get { return columnName; }
            set { columnName = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mapped.
        /// </summary>
        /// <value><c>true</c> if this instance is mapped; otherwise, <c>false</c>.</value>
        internal bool IsMapped
        {
            get { return isMapped; }
            set { isMapped = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is scalar property.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is scalar property; otherwise, <c>false</c>.
        /// </value>
        internal bool IsScalarProperty
        {
            get { return isScalarProperty; }
            set { isScalarProperty = value; }
        }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        internal TableMapping Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Gets or sets the property id.
        /// </summary>
        /// <value>The property id.</value>
        internal Guid PropertyId
        {
            get { return propertyId; }
            set { propertyId = value; }
        }

        #endregion
    }
}
