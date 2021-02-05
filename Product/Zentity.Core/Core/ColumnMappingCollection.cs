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
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a collection of <see cref="Zentity.Core.ColumnMapping"/> objects.
    /// </summary>
    internal sealed class ColumnMappingCollection : ICollection<ColumnMapping>
    {
        #region Fields

        TableMapping parent;
        Collection<ColumnMapping> innerMappings;

        #endregion

        #region Properties

        internal TableMapping Parent
        {
            get { return parent; }
        }

        #endregion

        #region Constructors

        internal ColumnMappingCollection(TableMapping parent)
        {
            this.parent = parent;
            this.innerMappings = new Collection<ColumnMapping>();
        }

        #endregion

        #region ICollection<ColumnMapping> Members

        public void Add(ColumnMapping item)
        {
            item.Parent = this.Parent;
            this.innerMappings.Add(item);
        }

        public void Clear()
        {
            this.innerMappings.Clear();
        }

        public bool Contains(ColumnMapping item)
        {
            // Do a reference comparison.
            return this.innerMappings.Contains(item);
        }

        public void CopyTo(ColumnMapping[] array, int arrayIndex)
        {
            this.innerMappings.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return this.innerMappings.Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(ColumnMapping item)
        {
            // Do a reference comparison.
            ColumnMapping mapping = this.innerMappings.Where(tuple => tuple == item).First();
            mapping.Parent = null;
            return this.innerMappings.Remove(mapping);
        }

        #endregion

        #region IEnumerable<ColumnMapping> Members

        public IEnumerator<ColumnMapping> GetEnumerator()
        {
            return this.innerMappings.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.innerMappings.GetEnumerator();
        }

        #endregion
    }
}
