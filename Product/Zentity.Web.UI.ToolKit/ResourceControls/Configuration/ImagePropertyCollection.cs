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
    /// Represents a collection image property settings specified in configuration file.
    /// </summary>
    public class ImagePropertyCollection : ConfigurationElementCollection
    {
        string _regularExpression = String.Empty;

        /// <summary>
        /// Gets the regular expression specified for image file name match.
        /// </summary>
        public string RegularExpression
        {
            get
            {
                if (string.IsNullOrEmpty(_regularExpression))
                    return ResourcePropertyConstants.ImageExtensionExpression;
                return _regularExpression;
            }
        }
        
        /// <summary>
        /// Initializes RegularExpression property from configuration file.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        /// <returns>true, if attribute is 'regularExpression' or unrecognized; else false.
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (name == ResourcePropertyConstants.RegularExpressionAttributeName)
            {
                this._regularExpression = value;
                return true;
            }
            return base.OnDeserializeUnrecognizedAttribute(name, value);
        }

        /// <summary>
        /// Gets or sets the image property setting at a specified position 
        /// in current collection.
        /// </summary>
        /// <param name="index">A image property setting position in current 
        /// collection.</param>
        /// <returns>Image property setting.</returns>
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
        /// Gets the image property setting associated 
        /// with the specified key.
        /// </summary>
        /// <param name="key">The key of the image property setting to get.</param>
        /// <returns>The image property setting associated with the 
        /// specified key.</returns>
        public new ResourcePropertiesConfigElement this[string key]
        {
            get
            {
                return base.BaseGet(key) as ResourcePropertiesConfigElement;
            }
        }

        /// <summary>
        /// Creates a new image property setting.
        /// </summary>
        /// <returns>Image property setting.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ResourcePropertiesConfigElement();
        }

        /// <summary>
        /// Gets the key associated with the specified image property setting.
        /// </summary>
        /// <param name="element">The image property setting associated with the key 
        /// to get.</param>
        /// <returns>The key associated with the specified image property setting.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ResourcePropertiesConfigElement)element).Name;
        }
    }
}
