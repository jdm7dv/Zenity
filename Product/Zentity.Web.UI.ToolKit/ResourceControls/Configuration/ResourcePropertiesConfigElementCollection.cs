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
    /// Represents a collection resource property settings specified in configuration file.
    /// </summary>
    public class ResourcePropertiesConfigElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets or sets the resource property setting at a specified position in current collection.
        /// </summary>
        /// <param name="index">A resource property setting position in current collection.</param>
        /// <returns>Resource property setting.</returns>
        public ResourcePropertiesConfigElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as ResourcePropertiesConfigElement;
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
        /// Gets the resource property setting associated 
        /// with the specified key.
        /// </summary>
        /// <param name="key">The key of the resource property setting to get.</param>
        /// <returns>The resource property setting associated with the 
        /// specified key.</returns>
        public new ResourcePropertiesConfigElement this[string key]
        {
            get
            {
                return base.BaseGet(key) as ResourcePropertiesConfigElement;
            }
        }

        /// <summary>
        /// Creates a new resource property setting.
        /// </summary>
        /// <returns>Resource property setting.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ResourcePropertiesConfigElement();
        }

        /// <summary>
        /// Gets the key associated with the specified resource property setting.
        /// </summary>
        /// <param name="element">The resource property setting associated with the key 
        /// to get.</param>
        /// <returns>The key associated with the specified resource property setting.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ResourcePropertiesConfigElement)element).Name;
        }
    }
}
