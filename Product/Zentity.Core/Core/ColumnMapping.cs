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
    /// <summary>
    /// Structure to store the mapping of a table column with either a scalar or a
    /// navigation property. There might be some columns that have their propertyName
    /// as NULL references. This means that they are not mapped directly to any model
    /// property.
    /// </summary>
    internal sealed class ColumnMapping
    {
        #region Fields

        TableMapping parent;
        string columnName;
        bool isMapped; // Initial value = false.
        bool isScalarProperty;
        Guid propertyId;

        #endregion 

        #region Properties

        internal TableMapping Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        internal string ColumnName
        {
            get { return columnName; }
            set { columnName = value; }
        }

        internal bool IsMapped
        {
            get { return isMapped; }
            set { isMapped = value; }
        }

        internal bool IsScalarProperty
        {
            get { return isScalarProperty; }
            set { isScalarProperty = value; }
        }

        internal Guid PropertyId
        {
            get { return propertyId; }
            set { propertyId = value; }
        }

        #endregion
    }
}
