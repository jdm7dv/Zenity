// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider.PasswordManagement
{
    using System.Configuration;

    /// <summary>
    /// Represents configuration section "PasswordPolicy"
    /// </summary>
    public class PasswordPolicy : ConfigurationSection
    {
        /// <summary>
        /// Gets a collection of password policy elements
        /// </summary>
        [ConfigurationProperty("CurrentPolicy", IsRequired = true)]
        public PasswordPolicyElement PolicyElement
        {
            get { return (PasswordPolicyElement)this["CurrentPolicy"]; }
        }
    }
}
