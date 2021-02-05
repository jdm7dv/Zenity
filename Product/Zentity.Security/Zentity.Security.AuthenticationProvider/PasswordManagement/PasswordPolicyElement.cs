// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider.PasswordManagement
{
    using System.Configuration;

    /// <summary>
    /// Represents a policy element
    /// </summary>
    public class PasswordPolicyElement : ConfigurationElement
    {
        /// <summary>
        /// Gets number of days in which a password must be changed
        /// </summary>
        [ConfigurationProperty("ExpiresInDays", IsRequired = true, DefaultValue = 60)]
        [IntegerValidator(MinValue = 7, MaxValue = 100)]
        public int ExpiresInDays
        {
            get { return (int)this["ExpiresInDays"]; }
        }

        /// <summary>
        /// Gets minimum length of the password
        /// </summary>
        [ConfigurationProperty("MinimumLength", IsRequired = true, DefaultValue = 6)]
        [IntegerValidator(MinValue = 6)]
        public int MinimumLength
        {
            get { return (int)this["MinimumLength"]; }
        }

        /// <summary>
        /// Gets maximum length of the password
        /// </summary>
        [ConfigurationProperty("MaximumLength", IsRequired = true, DefaultValue = 20)]
        [IntegerValidator(MaxValue = 100)]
        public int MaximumLength
        {
            get { return (int)this["MaximumLength"]; }
        }

        /// <summary>
        /// Gets a value indicating whether the password must start with an alphabet
        /// </summary>
        [ConfigurationProperty("StartWithAlphabet", IsRequired = false, DefaultValue = false)]
        public bool StartWithAlphabet
        {
            get { return (bool)this["StartWithAlphabet"]; }
        }

        /// <summary>
        /// Gets a value indicating whether the password must contain at least one numeral
        /// </summary>
        [ConfigurationProperty("MustContainDigit", IsRequired = false, DefaultValue = true)]
        public bool MustContainDigit
        {
            get { return (bool)this["MustContainDigit"]; }
        }

        /// <summary>
        /// Gets a value indicating whether the password must contain a special char to make it strong password
        /// </summary>
        [ConfigurationProperty("MustContainSpecialCharacter", IsRequired = false, DefaultValue = true)]
        public bool MustContainSpecialCharacter
        {
            get { return (bool)this["MustContainSpecialCharacter"]; }
        }
    }
}
