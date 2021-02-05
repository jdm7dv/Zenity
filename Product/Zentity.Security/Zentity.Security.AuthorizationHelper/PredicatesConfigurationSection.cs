using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Zentity.Security.AuthorizationHelper
{
    /// <summary>
    /// This class represents 'Predicates' configuration section
    /// </summary>
    public class PredicatesConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("SecurityPredicates")]
        public SecurityPredicatesCollection SecurityPredicates
        {
            get { return base["SecurityPredicates"] as SecurityPredicatesCollection; }
        }
    }

    /// <summary>
    /// This class represents security predicates collection
    /// </summary>
    public class SecurityPredicatesCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SecurityPredicateElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SecurityPredicateElement)element).Name;
        }
    }

    /// <summary>
    /// This class represents a security predicate element
    /// </summary>
    public class SecurityPredicateElement : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return base["Name"] as string; }
        }

        [ConfigurationProperty("Priority", IsRequired = true)]
        public int Priority
        {
            get { return (int)base["Priority"]; }
        }

        [ConfigurationProperty("Uri", IsRequired = true)]
        public string Uri
        {
            get { return base["Uri"] as string; }
        }

        [ConfigurationProperty("InversePredicateUri", IsRequired = false)]
        public string InversePredicateUri
        {
            get { return base["InversePredicateUri"] as string; }
        }
    }
}
