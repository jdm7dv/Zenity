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
    /// Represents an date-range property setting.
    /// </summary>
    public class DateRangePropertiesElement : ResourcePropertiesConfigElement
    {
        private const string EndDateNameKey = "endDateName";
        /// <summary>
        /// Gets the end date value.
        /// </summary>
        [ConfigurationProperty(EndDateNameKey, IsRequired = true)]
        public string EndDateName
        {
            get
            {
                return this[EndDateNameKey] as string;
            }
        }
    }
}
