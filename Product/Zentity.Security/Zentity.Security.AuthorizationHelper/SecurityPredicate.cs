// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthorizationHelper
{
    /// <summary>
    /// This class defines a data type for storing security predicate information.
    /// </summary>
    internal class SecurityPredicate
    {
        /// <summary>
        /// Gets or sets the store name of the predicate. This is the key.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Gets or sets the priority of the predicate
        /// </summary>
        internal int Priority { get; set; }
        
        /// <summary>
        /// Gets or sets the predicate uri
        /// </summary>
        internal string Uri { get; set; }
        
        /// <summary>
        /// Gets or sets the inverse predicate uri
        /// </summary>
        internal string InverseUri { get; set; }
    }
}
