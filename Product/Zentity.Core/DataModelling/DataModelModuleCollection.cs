// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a collection of <see cref="Zentity.Core.DataModelModule"/> objects.
    /// </summary>
    public sealed class DataModelModuleCollection : ICollection<DataModelModule>
    {
        #region Fields

        Collection<DataModelModule> innerCollection;
        DataModel parent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the parent model of this module collection.
        /// </summary>
        public DataModel Parent
        {
            get { return parent; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataModelModuleCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        internal DataModelModuleCollection(DataModel parent)
        {
            this.parent = parent;
            this.innerCollection = new Collection<DataModelModule>();
        }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the first <see cref="Zentity.Core.DataModelModule"/> object with the input
        /// NameSpace name if found, null otherwise.
        /// </summary>
        /// <param name="namespaceName">NameSpace name.</param>
        /// <returns>First <see cref="Zentity.Core.DataModelModule"/> object with the input
        /// NameSpace name if found, null otherwise.</returns>
        public DataModelModule this[string namespaceName]
        {
            get
            {
                return this.Where(p => p.NameSpace == namespaceName).
                    FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the <see cref="Zentity.Core.DataModelModule"/> object at the specified index.
        /// </summary>
        /// <param name="index">Index value.</param>
        /// <returns>The <see cref="Zentity.Core.DataModelModule"/> object at the specified 
        /// index</returns>
        public DataModelModule this[int index]
        {
            get
            {
                return this.innerCollection[index];
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates the DataModelModule collection for inconsistencies.
        /// </summary>
        internal void Validate()
        {
            // Perform validation of dataModelModule properties.
            foreach (DataModelModule module in this)
                module.Validate();

            int length = this.Count;
            for (int i = 0; i < length - 1; i++)
                CheckDuplicates(this[i], i + 1, length - 1);

            // Check for duplicate association names across modules.
            List<Association> allAssociations = this.
                SelectMany(tuple => tuple.Associations).ToList();

            var moduleAssociations = allAssociations.
                GroupBy(tuple => tuple.Name).Where(tuple => tuple.Count() > 1);
            if (moduleAssociations.Count() > 0)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ValidationExceptionDuplicateAssociationName,
                    moduleAssociations.First().Key));

            // Validate each association.
            foreach (Association association in allAssociations)
                association.Validate();
        }

        /// <summary>
        /// Checks the duplicates in the collection.
        /// </summary>
        /// <param name="subjectModule">The subject module.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        private void CheckDuplicates(DataModelModule subjectModule, int startIndex, int endIndex)
        {
            DataModelModule objectModule = null;
            for (int i = startIndex; i <= endIndex; i++)
            {
                objectModule = this[i];

                // Check for modules with duplicate namespaces. We do a case insensitive 
                // comparison here to be more strict.
                if (!string.IsNullOrEmpty(subjectModule.NameSpace) &&
                    !string.IsNullOrEmpty(objectModule.NameSpace) &&
                    subjectModule.NameSpace.Equals(objectModule.NameSpace,
                    StringComparison.OrdinalIgnoreCase))
                    throw new ModelItemValidationException(string.Format(
                        CultureInfo.InvariantCulture,
                        DataModellingResources.ValidationExceptionDuplicateItemInCollection,
                        DataModellingResources.DataModelModule, DataModellingResources.Namespace,
                        subjectModule.NameSpace));

                // Check for duplicate Ids. This eliminates the need of reference comparison
                // since each module is born with an Id and same references must have same
                // Id values.
                if (subjectModule.Id == objectModule.Id)
                    throw new ModelItemValidationException(string.Format(
                        CultureInfo.InvariantCulture,
                        DataModellingResources.ValidationExceptionDuplicateItemInCollection,
                        DataModellingResources.DataModelModule, DataModellingResources.Id,
                        subjectModule.Id));
            }
        }

        #endregion

        #region ICollection<DataModelModule> Members

        /// <summary>
        /// Adds a new module to the collection. The Parent property of the new module is set to 
        /// the Parent data model of this collection.
        /// </summary>
        /// <param name="item">The DataModel module to add</param>
        public void Add(DataModelModule item)
        {
            // Perform parent check.
            if (item.Parent != null && item.Parent != this.Parent)
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ExceptionItemAndCollectionParentsDoNotMatch,
                    item.GetType().FullName));

            // Perfom duplicate checks.
            CheckDuplicates(item, 0, this.Count - 1);

            this.innerCollection.Add(item);
            item.Parent = this.Parent;
        }

        /// <summary>
        /// Clears this collection. Parent property of each of the earlier modules is set to a 
        /// null reference.
        /// </summary>
        public void Clear()
        {
            List<DataModelModule> clonedModules = new List<DataModelModule>();
            clonedModules.AddRange(this.innerCollection);
            this.innerCollection.Clear();
            foreach (DataModelModule module in clonedModules)
                module.Parent = null;
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public bool Contains(DataModelModule item)
        {
            return innerCollection.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an System.Array, 
        /// starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the 
        /// elements copied from collection. The System.Array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(DataModelModule[] array, int arrayIndex)
        {
            innerCollection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count
        {
            get { return innerCollection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes a module from the collection. The Parent property of the module is set to
        /// a null reference.
        /// </summary>
        /// <param name="item">The module to remove from the collection.</param>
        /// <returns>true if removed successfully, otherwise false.</returns>
        public bool Remove(DataModelModule item)
        {
            bool result = this.innerCollection.Remove(item);
            if (result == true)
                item.Parent = null;
            return result;
        }

        #endregion

        #region IEnumerable<DataModelModule> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<DataModelModule> GetEnumerator()
        {
            return innerCollection.GetEnumerator();
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
            return innerCollection.GetEnumerator();
        }

        #endregion
    }
}
