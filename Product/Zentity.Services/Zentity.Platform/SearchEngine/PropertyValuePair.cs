// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

#region .NET Framework Class Namespace Imports

using System;

#endregion

namespace Zentity.Platform
{
    /// <summary>
    /// Represents a pair of property and value.
    /// </summary>
    public class PropertyValuePair
    {
        #region Properties

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string PropertyName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public string PropertyValue
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Zentity.Platform.PropertyValuePair"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Value of the property.</param>
        public PropertyValuePair(string propertyName, string propertyValue)
        {
            if (String.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }
            if (String.IsNullOrEmpty(propertyValue))
            {
                throw new ArgumentNullException("propertyValue");
            }
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }

        #endregion
    }
}
