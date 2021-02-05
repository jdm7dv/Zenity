// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authentication
{
    using System.Configuration;

    /// <summary>
    /// Represents configuration section for configuring authentication in the application configuration file.
    /// </summary>
    public class AuthenticationConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Gets the authenticationProvider elements collection.
        /// </summary>
        [ConfigurationProperty("Providers", DefaultValue = null)]
        public AuthenticationProviderElementCollection Providers
        {
            get
            {
                return (AuthenticationProviderElementCollection)base["Providers"];
            }
        }
    }
}
