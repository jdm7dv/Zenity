// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authentication
{
    using System;
    using System.Configuration;
    using Zentity.Security.Authentication.Properties;

    /// <summary>
    /// Class for creating an instance of an implementation of “IAuthenticationProvider”.
    /// </summary>
    public static class AuthenticationProviderFactory
    {
        /// <summary>
        /// Gets an instance of the specified implementation of an authentication provider.
        /// </summary>
        /// <param name="providerName">Name of authentication provider(as added in the configuration file).</param>
        /// <returns>Instance of authentication provider.</returns>
        public static IAuthenticationProvider CreateAuthenticationProvider(string providerName)
        {
            IAuthenticationProvider provider = null;
            AuthenticationConfigurationSection authConfig = null;

            try
            {
                authConfig =
                    ConfigurationManager.GetSection("Authentication")
                        as AuthenticationConfigurationSection;
            }
            catch (ConfigurationErrorsException configExp)
            {
                throw new AuthenticationException(Resources.AUTHENTICATION_CONFIG_LOAD_ERROR, configExp);                    
            }

            foreach (AuthenticationProviderElement element in authConfig.Providers)
            {
                if (string.Compare(element.ProviderName, providerName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Type providerType = Type.GetType(element.ProviderType, false);
                    if (providerType != null && providerType.GetInterface("IAuthenticationProvider", false) != null)
                    {
                        provider = Activator.CreateInstance(providerType) as IAuthenticationProvider;
                        break;
                    }
                }
            }

            return provider;
        }
    }
}
