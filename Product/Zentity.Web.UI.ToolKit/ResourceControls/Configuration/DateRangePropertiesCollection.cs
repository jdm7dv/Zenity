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
    /// Represents a collection date-range property settings specified in configuration file.
    /// </summary>
    public class DateRangePropertiesCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets or sets the date-range property setting at a specified position 
        /// in current collection.
        /// </summary>
        /// <param name="index">A date-range property setting position in current 
        /// collection.</param>
        /// <returns>Date-range property setting.</returns>
        public DateRangePropertiesElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as DateRangePropertiesElement;
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
        /// Gets the date-range property setting associated 
        /// with the specified key.
        /// </summary>
        /// <param name="key">The key of the date-range property setting to get.</param>
        /// <returns>The date-range property setting associated with the 
        /// specified key.</returns>
        public new DateRangePropertiesElement this[string key]
        {
            get
            {
                return base.BaseGet(key) as DateRangePropertiesElement;
            }
        }

        /// <summary>
        /// Creates a new date-range property setting.
        /// </summary>
        /// <returns>Date-range property setting.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new DateRangePropertiesElement();
        }

        /// <summary>
        /// Gets the key associated with the specified date-range property setting.
        /// </summary>
        /// <param name="element">The date-range property setting associated with the key 
        /// to get.</param>
        /// <returns>The key associated with the specified date-range property setting.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DateRangePropertiesElement)element).Name;
        }
    }
}
