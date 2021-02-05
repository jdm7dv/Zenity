// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using Zentity.Platform.Properties;

    #region ResourceMap Class
    /// <summary>
    /// This class is responsible for Generating a resource map for a resource
    /// </summary>
    /// <typeparam name="T">Resource map type</typeparam>
    public class ResourceMap<T> : IOriginator<T>
    {

        #region Private Members
        private string creator;
        private DateTime modified;
        private T uri;
        private Aggregation<T> aggregation;
        #endregion

        #region .ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMap&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public ResourceMap(T uri)
        {
            this.uri = uri;
            this.creator = Resources.ORE_REM_CREATOR;
            this.modified = DateTime.Now;
            this.aggregation = new Aggregation<T>(uri);
        }

        /// <summary>
        /// Creates the memento.
        /// </summary>
        /// <returns>The memento.</returns>
        public Memento<T> CreateMemento()
        {
            Memento<T> state = aggregation.CreateMemento();
            state.ResourceMapUri = uri.ToString();
            state.ResourceMapCreator = creator;
            state.ResourceMapDateModified = modified;

            return state;
        }
        #endregion
    }
    #endregion
}
