// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Runtime.Serialization;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a collection of <see cref="Zentity.Core.ScalarProperty"/> objects.
    /// </summary>
    [CollectionDataContract]
    public sealed class ScalarPropertyCollection : ICollection<ScalarProperty>
    {
        #region Fields

        Collection<ScalarProperty> innerCollection;
        ResourceType parent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Parent of the property collection.
        /// </summary>
        [IgnoreDataMember]
        public ResourceType Parent
        {
            get { return parent; }
        }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the first <see cref="Zentity.Core.ScalarProperty"/> object with the input
        /// Name if found, null otherwise.
        /// </summary>
        /// <param name="name">Name of the scalar property.</param>
        /// <returns>The first <see cref="Zentity.Core.ScalarProperty"/> object with the input
        /// Name if found, null otherwise.</returns>
        public ScalarProperty this[string name]
        {
            get
            {
                return this.Where(p => p.Name == name).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the <see cref="Zentity.Core.ScalarProperty"/> object at the specified index.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>The <see cref="Zentity.Core.ScalarProperty"/> object at the specified 
        /// index.</returns>
        public ScalarProperty this[int index]
        {
            get
            {
                return this.innerCollection[index];
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarPropertyCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        internal ScalarPropertyCollection(ResourceType parent)
        {
            this.parent = parent;
            this.innerCollection = new Collection<ScalarProperty>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarPropertyCollection"/> class.
        /// </summary>
        private ScalarPropertyCollection()
        {
            this.innerCollection = new Collection<ScalarProperty>();
            // Required for Data Service Contract
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates this instance.
        /// </summary>
        internal void Validate()
        {
            // Perform validation of scalar properties.
            foreach (ScalarProperty prop in this)
            {
                prop.Validate();
            }

            int length = this.Count;
            for (int i = 0; i < length - 1; i++)
                CheckDuplicates(this[i], i + 1, length - 1);
        }

        /// <summary>
        /// Checks the duplicates.
        /// </summary>
        /// <param name="subjectProperty">The subject property.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        private void CheckDuplicates(ScalarProperty subjectProperty, int startIndex, int endIndex)
        {
            ScalarProperty objectProperty = null;
            for (int i = startIndex; i <= endIndex; i++)
            {
                objectProperty = this[i];

                // Check for duplicate names. We do a case insensitive comparison here to 
                // be more strict.
                if (!string.IsNullOrEmpty(subjectProperty.Name) &&
                    !string.IsNullOrEmpty(objectProperty.Name) &&
                    subjectProperty.Name.Equals(objectProperty.Name,
                    StringComparison.OrdinalIgnoreCase))
                    throw new ModelItemValidationException(string.Format(
                        CultureInfo.InvariantCulture,
                        DataModellingResources.ValidationExceptionDuplicateItemInCollection,
                        DataModellingResources.ScalarProperty, DataModellingResources.Name,
                        subjectProperty.Name));

                // Check for duplicate Ids. This eliminates the need of reference comparison
                // since each scalar property is born with an Id and same references must 
                // have same Id values.
                if (subjectProperty.Id == objectProperty.Id)
                    throw new ModelItemValidationException(string.Format(
                        CultureInfo.InvariantCulture,
                        DataModellingResources.ValidationExceptionDuplicateItemInCollection,
                        DataModellingResources.ScalarProperty, DataModellingResources.Id,
                        subjectProperty.Id));
            }
        }

        #endregion

        #region ICollection<ScalarProperty> Members

        /// <summary>
        /// Clears this collection. Parent property of each of the earlier scalar properties
        /// is set to a null reference.
        /// </summary>
        public void Clear()
        {
            List<ScalarProperty> scalarProperties = new List<ScalarProperty>();
            scalarProperties.AddRange(this.innerCollection);
            this.innerCollection.Clear();
            foreach (ScalarProperty scalarProperty in scalarProperties)
                scalarProperty.Parent = null;
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public bool Contains(ScalarProperty item)
        {
            return this.innerCollection.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an System.Array, starting at a particular 
        /// System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the 
        /// elements copied from collection. The System.Array must 
        /// have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.
        /// </param>
        public void CopyTo(ScalarProperty[] array, int arrayIndex)
        {
            this.innerCollection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return this.innerCollection.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a new scalar property to the collection. Parent property of the new 
        /// scalar property is set to the Parent resource type of this collection.
        /// </summary>
        /// <param name="item">The object to add to the collection.</param>
        public void Add(ScalarProperty item)
        {
            // Perform parent check.
            if (item.Parent != null && item.Parent != this.Parent)
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ExceptionItemAndCollectionParentsDoNotMatch,
                    item.GetType().FullName));

            // Perfom duplicate checks.
            CheckDuplicates(item, 0, this.Count - 1);

            // Add to collection.
            this.innerCollection.Add(item);

            // Set the parent.
            item.Parent = this.Parent;
        }

        /// <summary>
        /// Removes a scalar property from this collection. Parent property of the 
        /// removed scalar property is set to a null reference.
        /// </summary>
        /// <param name="item">The object to remove from the collection.</param>
        /// <returns>true if item was successfully removed from the collection; otherwise, false. 
        /// This method also returns false if item is not found in the original collection.
        /// </returns>
        public bool Remove(ScalarProperty item)
        {
            bool result = this.innerCollection.Remove(item);
            if (result == true)
                item.Parent = null;
            return result;
        }

        #endregion

        #region IEnumerable<ScalarProperty> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ScalarProperty> GetEnumerator()
        {
            return this.innerCollection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.innerCollection.GetEnumerator();
        }

        #endregion
    }
}
