// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;

    /// <summary>
    /// Aggregated resource creator class.
    /// </summary>
    /// <typeparam name="URI">The type of the RI.</typeparam>
    class AggResourceCreator<URI>
    {
        private const int StrippingToSingleLevel = 1;
        
        /// <summary>
        /// Creates a new aggregated resource object
        /// </summary>
        /// <param name="uri">Uri of resource</param>
        /// <returns>aggregated resource object</returns>
        public static AbstractAggregatedResource<URI> Create(URI uri)
        {
            if (uri.GetType() == typeof(Guid))
            {
                Guid guid = new Guid(uri.ToString());
                return new ZentityAggregatedResource(guid, StrippingToSingleLevel) as AbstractAggregatedResource<URI>;
            }

            return null;
        }
    }
}
