// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Represents a collection ordered property settings specified in configuration file.
    /// </summary>
    public class OrderPropertyConfigElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets or sets the ordered property setting at a specified position in current collection.
        /// </summary>
        /// <param name="index">A ordered property setting position in current collection.</param>
        /// <returns>Ordered property setting.</returns>
        public OrderPropertyConfigElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as OrderPropertyConfigElement;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Gets the ordered property setting associated 
        /// with the specified key.
        /// </summary>
        /// <param name="key">The key of the ordered property setting to get.</param>
        /// <returns>The ordered property setting associated with the 
        /// specified key.</returns>
        public new OrderPropertyConfigElement this[string key]
        {
            get
            {
                return base.BaseGet(key) as OrderPropertyConfigElement;
            }
        }

        /// <summary>
        /// Creates a new ordered property setting.
        /// </summary>
        /// <returns>Ordered property setting.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new OrderPropertyConfigElement();
        }

        /// <summary>
        /// Gets the key associated with the specified ordered property setting.
        /// </summary>
        /// <param name="element">The ordered property setting associated with the key 
        /// to get.</param>
        /// <returns>The key associated with the specified ordered property setting.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OrderPropertyConfigElement)element).Order;
        }
    }
}

