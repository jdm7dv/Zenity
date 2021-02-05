// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a base class to implement tokens to be returned 
    /// after authentication.
    /// </summary>
    /// <example>
    /// For a code sample please refer to IAuthenticationProvider documentation.
    /// </example>
    public abstract class AuthenticatedToken
    {
        /// <summary>
        /// Gets the identity that was authenticated.
        /// </summary>
        public abstract string IdentityName
        {
            get;
        }

        /// <summary>
        /// Checks the validity of the token at any given time.
        /// </summary>
        /// <returns>True if the token is valid.</returns>
        /// <example>For a code sample please refer to IAuthenticationProvider documentation.</example>
        public abstract bool Validate();
    }
}
