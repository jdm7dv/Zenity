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
    /// Represents the resourcePropertiesSettings configuration section.
    /// </summary>
    public class ResourcePropertiesConfigSection : ConfigurationSection
    {
        /// <summary>
        /// Gets requiredProperties element specified in configuration file.
        /// </summary>
        [ConfigurationProperty("requiredProperties")]
        public ResourcePropertiesConfigElementCollection RequiredProperties
        {
            get
            {
                return base["requiredProperties"] as ResourcePropertiesConfigElementCollection;
            }
        }

        /// <summary>
        /// Gets readOnlyProperties element specified in configuration file.
        /// </summary>
        [ConfigurationProperty("readOnlyProperties")]
        public ResourcePropertiesConfigElementCollection ReadOnlyProperties
        {
            get
            {
                return base["readOnlyProperties"] as ResourcePropertiesConfigElementCollection;
            }
        }

        /// <summary>
        /// Gets emailProperties element specified in configuration file.
        /// </summary>
        [ConfigurationProperty("emailProperties")]
        public EmailPropertyCollection EmailProperties
        {
            get
            {
                return base["emailProperties"] as EmailPropertyCollection;
            }
        }

        /// <summary>
        /// Gets dateRangeProperties element specified in configuration file.
        /// </summary>
        [ConfigurationProperty("dateRangeProperties")]
        public DateRangePropertiesCollection DateRangeProperties
        {
            get
            {
                return base["dateRangeProperties"] as DateRangePropertiesCollection;
            }
        }

        /// <summary>
        /// Gets imageProperties element specified in configuration file.
        /// </summary>
        [ConfigurationProperty("imageProperties")]
        public ImagePropertyCollection ImageProperties
        {
            get
            {
                return base["imageProperties"] as ImagePropertyCollection;
            }
        }

        /// <summary>
        /// Gets excludedProperties element specified in configuration file.
        /// </summary>
        [ConfigurationProperty("excludedProperties")]
        public ResourcePropertiesConfigElementCollection ExcludedProperties
        {
            get
            {
                return base["excludedProperties"] as ResourcePropertiesConfigElementCollection;
            }
        }

        /// <summary>
        /// Gets orderedProperties element specified in configuration file.
        /// </summary>
        [ConfigurationProperty("orderedProperties")]
        public OrderPropertyConfigElementCollection OrderedProperties
        {
            get
            {
                return base["orderedProperties"] as OrderPropertyConfigElementCollection;
            }
        }
    }
}