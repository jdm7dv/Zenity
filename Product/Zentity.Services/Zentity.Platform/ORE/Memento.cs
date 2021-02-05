// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;

    #region Memento Class
    /// <summary>
    /// Creates a Memento
    /// </summary>
    /// <typeparam name="T">Type of memento.</typeparam>
    public class Memento<T>
    {
        #region Member Variables

        /// <summary>
        /// Stores the aggregated resource list
        /// </summary>
        internal List<AbstractAggregatedResource<T>> AggreagtedResources;

        /// <summary>
        /// Stores the uri of the resource map
        /// </summary>
        internal string ResourceMapUri;

        /// <summary>
        /// Stores the creator of the resource map
        /// </summary>
        internal string ResourceMapCreator;

        /// <summary>
        /// Stores the date when the resource map was modified
        /// </summary>
        internal DateTime ResourceMapDateModified;

        /// <summary>
        /// Stores the relationship uri
        /// </summary>
        internal List<string> RelationUris;

        /// <summary>
        /// Stores the object resource ids
        /// </summary>
        internal List<T> ObjectResourceIds;

        /// <summary>
        /// Stores the object resource types
        /// </summary>
        internal List<Type> ObjectTypes;

        /// <summary>
        /// Stores the names of categories
        /// </summary>
        internal List<string> CategoryNames;

        /// <summary>
        /// Stores the names of tags
        /// </summary>
        internal List<string> TagNames;

        /// <summary>
        /// Stores the type of resource
        /// </summary>
        internal string ResourceType;

        /// <summary>
        /// Stores the resource Creator
        /// </summary>
        internal string ResourceCreator;

        /// <summary>
        /// Stores the resource modified date
        /// </summary>
        internal DateTime ResourceModified;

        /// <summary>
        /// Stores the resource scalar properties
        /// </summary>
        internal List<PropertyInformation> ScalarProperties;

        /// <summary>
        /// Stores the resource type of aggregated resources
        /// </summary>
        internal List<Type> AggregateTypes;
        #endregion

        #region .ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Memento&lt;T&gt;"/> class.
        /// </summary>
        public Memento()
        {
        }
        #endregion
    }
    #endregion
}

