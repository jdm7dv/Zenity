// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthorizationHelper
{
    /// <summary>
    /// This class stores the permission name and state of allowing / denying the permission.
    /// </summary>
    public class PermissionMap
    {
        /// <summary>
        /// Gets or sets the permission name
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grant status is given
        /// </summary>
        public bool Allow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the revoke status is true
        /// </summary>
        public bool Deny { get; set; }
    }
}
