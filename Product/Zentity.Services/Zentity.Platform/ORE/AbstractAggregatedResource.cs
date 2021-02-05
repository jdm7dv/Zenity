// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using Zentity.Platform.Properties;

    /// <summary>
    /// The agregated resource abstract class.
    /// </summary>
    /// <typeparam name="URI">The type of the RI.</typeparam>
    internal abstract class AbstractAggregatedResource<URI> : IOriginator<URI>
    {
        internal URI ResourceUri;
        internal List<AbstractAggregatedResource<URI>> AggreagtedResources = 
                                                        new List<AbstractAggregatedResource<URI>>();
        internal List<string> RelationUris = new List<string>();
        internal List<URI> ObjectResourceIds = new List<URI>();
        internal List<Type> ObjectTypes = new List<Type>();
        internal List<string> CategoryNames = new List<string>();
        internal List<string> TagNames = new List<string>();
        internal string ResourcesType = string.Empty;
        internal string ResourceCreator = string.Empty;
        internal DateTime ResourceModified = DateTime.MinValue;
        internal List<PropertyInformation> ScalarProperties = new List<PropertyInformation>();
        internal List<Type> AggregateTypes = new List<Type>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractAggregatedResource&lt;URI&gt;"/> class.
        /// </summary>
        public AbstractAggregatedResource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractAggregatedResource&lt;URI&gt;"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public AbstractAggregatedResource(URI uri)
        {
            this.ResourceUri = uri;
            this.ResourceCreator = Resources.ORE_REM_CREATOR;
            this.ResourceModified = DateTime.Now;
        }

        /// <summary>
        /// Creates the memento.
        /// </summary>
        /// <returns>The Memento of uri's.</returns>
        abstract public Memento<URI> CreateMemento();
    }
}
