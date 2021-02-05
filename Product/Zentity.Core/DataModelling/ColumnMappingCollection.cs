// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


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

        Collection<ColumnMapping> innerMappings;
        TableMapping parent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        internal TableMapping Parent
        {
            get { return parent; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnMappingCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        internal ColumnMappingCollection(TableMapping parent)
        {
            this.parent = parent;
            this.innerMappings = new Collection<ColumnMapping>();
        }

        #endregion

        #region ICollection<ColumnMapping> Members

        /// <summary>
        /// Adds the specified column mapping item.
        /// </summary>
        /// <param name="item">The column mapping item.</param>
        public void Add(ColumnMapping item)
        {
            item.Parent = this.Parent;
            this.innerMappings.Add(item);
        }

        /// <summary>
        /// Clears this column mapping collection instance.
        /// </summary>
        public void Clear()
        {
            this.innerMappings.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains the specified column mapping item.
        /// </summary>
        /// <param name="item">The column mapping item.</param>
        /// <returns>
        /// 	<c>true</c> if the collection contains the specified column mapping item; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ColumnMapping item)
        {
            // Do a reference comparison.
            return this.innerMappings.Contains(item);
        }

        /// <summary>
        /// Copies a column mapping array into the column mapping collection at the specified index.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(ColumnMapping[] array, int arrayIndex)
        {
            this.innerMappings.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the column mapping item count.
        /// </summary>
        /// <value>The column mapping count.</value>
        public int Count
        {
            get
            {
                return this.innerMappings.Count();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this collection instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this collection instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the specified column mapping item.
        /// </summary>
        /// <param name="item">The column mapping item.</param>
        /// <returns>true if remove was successful.</returns>
        public bool Remove(ColumnMapping item)
        {
            // Do a reference comparison.
            ColumnMapping mapping = this.innerMappings.Where(tuple => tuple == item).First();
            mapping.Parent = null;
            return this.innerMappings.Remove(mapping);
        }

        #endregion

        #region IEnumerable<ColumnMapping> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator instance for the collection</returns>
        public IEnumerator<ColumnMapping> GetEnumerator()
        {
            return this.innerMappings.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.innerMappings.GetEnumerator();
        }

        #endregion
    }
}
