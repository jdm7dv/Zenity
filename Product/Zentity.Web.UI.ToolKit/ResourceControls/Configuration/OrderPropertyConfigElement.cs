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
    /// Represents an order property setting.
    /// </summary>
    public class OrderPropertyConfigElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the class name.
        /// </summary>
        [ConfigurationProperty("class", IsRequired = true)]
        public string Class
        {
            get
            {
                return this["class"] as string;
            }
        }

        /// <summary>
        /// Gets the order of the properties.
        /// </summary>
        [ConfigurationProperty("order", IsRequired = true)]
        public string Order
        {
            get
            {
                return this["order"] as string;
            }
        }
    }
}
