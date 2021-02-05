// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authentication
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents AuthenticationProvider elements collection in the Authentication configuration section.
    /// </summary>
    public class AuthenticationProviderElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Adds an AuthenticationProvider element to the collection in the Authentication configuration section.
        /// </summary>
        /// <param name="provider">AuthenticationProvider element.</param>
        public void Add(AuthenticationProviderElement provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            this.BaseAdd(provider);
        }

        /// <summary>
        /// Creates a new AuthenticationProvider element.
        /// </summary>
        /// <returns>AuthenticationProvider element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new AuthenticationProviderElement();
        }

        /// <summary>
        /// Returns key attribute value for AuthenticationProvider element.
        /// </summary>
        /// <param name="element">AuthenticationProvider element.</param>
        /// <returns>Key (Name) for AuthenticationProvider element.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AuthenticationProviderElement)element).ProviderName;
        }
    }
}
