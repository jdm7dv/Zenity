// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class containing constant values for predefined predicates
    /// </summary>
    public static class AuthorizingPredicates
    {
        /// <summary>
        /// String for predicate HasCreateAccess
        /// </summary>
        public const string HasCreateAccess = "urn:zentity/module/zentity-authorization/predicate/has-create-access";

        /// <summary>
        /// String for predicate HasUpdateAccess
        /// </summary>
        public const string HasUpdateAccess = "urn:zentity/module/zentity-authorization/predicate/has-update-access";

        /// <summary>
        /// String for predicate HasDeleteAccess
        /// </summary>
        public const string HasDeleteAccess = "urn:zentity/module/zentity-authorization/predicate/has-delete-access";

        /// <summary>
        /// String for predicate HasReadAccess
        /// </summary>
        public const string HasReadAccess = "urn:zentity/module/zentity-authorization/predicate/has-read-access";

        /// <summary>
        /// String for predicate IdentityBelongsToGroups
        /// </summary>
        public const string IdentityBelongsToGroups = "urn:zentity/module/zentity-authorization/association/identity-belongs-to-groups";
    }
}
