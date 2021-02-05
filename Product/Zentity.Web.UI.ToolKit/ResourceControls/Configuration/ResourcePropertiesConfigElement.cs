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
    /// Represents a resource property setting specified in configuration file.
    /// </summary>
    public class ResourcePropertiesConfigElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the class name of the resource property.
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
        /// Gets the name of the resource property.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }
    }
}
