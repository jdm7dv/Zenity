// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    internal class Aggregation<URI> : IOriginator<URI>
    {
        private URI uri;
        private AbstractAggregatedResource<URI> resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aggregation&lt;URI&gt;"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public Aggregation(URI uri)
        {
            this.uri = uri;
            this.resource = AggResourceCreator<URI>.Create(uri);
        }

        /// <summary>
        /// Creates the memento.
        /// </summary>
        /// <returns>The Memento of uri's.</returns>
        public Memento<URI> CreateMemento()
        {
            Memento<URI> state = resource.CreateMemento();
            return state;
        }
    }
}
