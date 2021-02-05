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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Globalization;
using System.Linq;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a collection of <see cref="Zentity.Core.ResourceType"/> objects.
    /// </summary>
    /// <example>
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                foreach (ResourceType rtInfo in context.DataModel.Modules[0].ResourceTypes)
    ///                {
    ///                    Console.WriteLine(&quot;----------------------------------------------------&quot;);
    ///                    Console.WriteLine(&quot;Resource type Name = [{0}]&quot;, rtInfo.Name);
    ///                    Console.WriteLine(&quot;Resource type Uri = [{0}]&quot;, rtInfo.Uri);
    ///                    Console.WriteLine(&quot;Resource type scalar properties: &quot;);
    ///                    foreach (ScalarProperty prop in rtInfo.ScalarProperties)
    ///                        Console.WriteLine(&quot;\t{0}\t{1}&quot;, prop.Name, prop.DataType);
    ///                    Console.WriteLine(&quot;Resource type navigation properties: &quot;);
    ///                    foreach (NavigationProperty prop in rtInfo.NavigationProperties)
    ///                        Console.WriteLine(&quot;\t{0}&quot;, prop.Name);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public sealed class ResourceTypeCollection : ICollection<ResourceType>
    {
        #region Fields

        Collection<ResourceType> innerCollection;
        DataModelModule parent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the parent data model module of this collection.
        /// </summary>
        public DataModelModule Parent
        {
            get { return parent; }
        }

        #endregion

        #region Constructors

        internal ResourceTypeCollection(DataModelModule parentModel)
        {
            // Set the parent property.
            if (parentModel == null)
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ExceptionArgumentIsNull, "parentModel"));
            this.parent = parentModel;
            this.innerCollection = new Collection<ResourceType>();
        }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the first <see cref="Zentity.Core.ResourceType"/> object with the input
        /// Name if found, null otherwise.
        /// </summary>
        /// <param name="name">Name of the resource type.</param>
        /// <returns>The first <see cref="Zentity.Core.ResourceType"/> object with the input
        /// Name if found, null otherwise.</returns>
        public ResourceType this[string name]
        {
            get
            {
                return this.Where(tuple => name.Equals(tuple.Name)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the <see cref="Zentity.Core.ResourceType"/> object at the specified index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>The <see cref="Zentity.Core.ResourceType"/> object at the specified 
        /// index.</returns>
        public ResourceType this[int index]
        {
            get
            {
                return this.innerCollection[index];
            }
        }

        #endregion

        #region Helper Methods

        internal void Validate()
        {
            // Perform validation of resourceTypes.
            foreach (ResourceType resourceType in this)
            {
                resourceType.Validate();
            }

            int length = this.Count;
            for (int i = 0; i < length - 1; i++)
                CheckDuplicates(this[i], i + 1, length - 1);
        }

        private void CheckDuplicates(ResourceType subjectType, int startIndex, int endIndex)
        {
            ResourceType objectType = null;
            for (int i = startIndex; i <= endIndex; i++)
            {
                objectType = this[i];

                // Check for duplicate names. We do a case insensitive comparison here 
                // to be more strict.
                if (!string.IsNullOrEmpty(subjectType.Name) &&
                    !string.IsNullOrEmpty(objectType.Name) &&
                    subjectType.Name.Equals(objectType.Name,
                    StringComparison.OrdinalIgnoreCase))
                    throw new ModelItemValidationException(string.Format(
                        CultureInfo.InvariantCulture,
                        CoreResources.ValidationExceptionDuplicateItemInCollection,
                        CoreResources.ResourceType, CoreResources.Name,
                        subjectType.Name));

                // Check for duplicate Ids. This eliminates the need of reference comparison
                // since each resource type is born with an Id and same references must 
                // have same Id values.
                if (subjectType.Id == objectType.Id)
                    throw new ModelItemValidationException(string.Format(
                        CultureInfo.InvariantCulture,
                        CoreResources.ValidationExceptionDuplicateItemInCollection,
                        CoreResources.ResourceType, CoreResources.Id,
                        subjectType.Id));
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return innerCollection.GetEnumerator();
        }

        #endregion

        #region ICollection<ResourceTypeInfo> Members

        /// <summary>
        /// Clears this collection. The Parent property of all the earlier resource types is 
        /// set to a null reference.
        /// </summary>
        public void Clear()
        {
            List<ResourceType> clonedList = new List<ResourceType>();
            clonedList.AddRange(this.innerCollection);
            this.innerCollection.Clear();
            foreach (ResourceType resourceType in clonedList)
                resourceType.Parent = null;
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public bool Contains(ResourceType item)
        {
            return this.innerCollection.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an System.Array, starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from collection. The System.Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(ResourceType[] array, int arrayIndex)
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
        /// Adds a resource type to this collection. The Parent property of added resource type
        /// is set to the parent module of this collection.
        /// </summary>
        /// <param name="item"></param>
        public void Add(ResourceType item)
        {
            // Perform parent check.
            if (item.Parent != null && item.Parent != this.Parent)
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ExceptionItemAndCollectionParentsDoNotMatch,
                    item.GetType().FullName));

            // Perfom duplicate checks.
            CheckDuplicates(item, 0, this.Count - 1);

            item.Parent = this.Parent;
            this.innerCollection.Add(item);
        }

        /// <summary>
        /// Removes a resource type from this collection. Parent property of the removed type
        /// is set to a null reference.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(ResourceType item)
        {
            bool result = this.innerCollection.Remove(item);
            if (result == true)
                item.Parent = null;
            return result;
        }

        #endregion

        #region IEnumerable<ResourceTypeInfo> Members

        IEnumerator<ResourceType> IEnumerable<ResourceType>.GetEnumerator()
        {
            return this.innerCollection.GetEnumerator();
        }

        #endregion
    }
}
