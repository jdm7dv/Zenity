using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Zentity.Security.AuthenticationProvider.PasswordManagement
{
    /// <summary>
    /// Represents a collection of password policy elements
    /// </summary>
    public class PasswordPolicyCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Returns a new password policy element
        /// </summary>
        /// <returns>New password policy element</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new PasswordPolicyElement();
        }

        /// <summary>
        /// This method returns the key of a configuration element defined as a member of this collection
        /// </summary>
        /// <param name="element">A PasswordPolicyElement which is a member of this collection</param>
        /// <returns>Name element of the given PasswordPolicyElement</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PasswordPolicyElement)element).Name;
        }
    }
}
