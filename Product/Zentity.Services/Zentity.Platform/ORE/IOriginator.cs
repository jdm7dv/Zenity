// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    /// <summary>
    /// Originator interface.
    /// </summary>
    /// <typeparam name="URI">The type of the RI.</typeparam>
    interface IOriginator<URI>
    {
        /// <summary>
        /// Creates the memento.
        /// </summary>
        /// <returns>The memento of uri's.</returns>
        Memento<URI> CreateMemento();
    }
}
