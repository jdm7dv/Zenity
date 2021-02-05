// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authentication
{
    using System.Configuration;

    /// <summary>
    /// Represents AuthenticationProvider element in the Authentication configuration section.
    /// </summary>
    public class AuthenticationProviderElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the name attribute of the AuthenticationProvider element.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string ProviderName
        {
            get
            {
                return (string)base["name"];
            }
        }

        /// <summary>
        /// Gets the type attribute of the AuthenticationProvider element.
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string ProviderType
        {
            get
            {
                return (string)base["type"];
            }
        }
    }
}
