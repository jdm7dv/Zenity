// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthorizationHelper
{
    using System.Collections.Generic;
    using System.Linq;
    using Zentity.Core;

    /// <summary>
    /// This class is a data type to store a resource along with its permissions for an identity.
    /// </summary>
    /// <typeparam name="T">Resource, usually the research output of an organization</typeparam>
    public class ResourcePermissions<T> where T : Resource
    {
        private List<string> permissions;

        /// <summary>
        /// Initializes a new instance of the ResourcePermissions class.
        /// </summary>
        internal ResourcePermissions()
        {
            this.permissions = new List<string>();
        }

        /// <summary>
        /// Gets or sets the Resource
        /// </summary>
        public T Resource { get; set; }

        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the permissions for the user on the resource
        /// </summary>
        public IEnumerable<string> Permissions
        {
            get { return this.permissions.AsEnumerable(); }
        }

        /// <summary>
        /// Gets the permissions for the user on the resource
        /// </summary>
        internal List<string> PermissionList
        {
            get { return this.permissions; }
        }
    }
}
