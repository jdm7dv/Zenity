// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





namespace Zentity.Platform
{
    #region Using NameSpace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    #region PropertyInformation Class
    /// <summary>
    /// This class stores information about the scalar properties
    /// </summary>
    public class PropertyInformation
    {

        #region Private members
        private string propertyName;
        private string propertyType;
        private string propertyValue;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the propertyName
        /// </summary>
        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
            set
            {
                this.propertyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the propertyValue
        /// </summary>
        public string PropertyValue
        {
            get
            {
                return this.propertyValue;
            }
            set
            {
                this.propertyValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the propertyType
        /// </summary>
        public string PropertyType
        {
            get
            {
                return this.propertyType;
            }
            set
            {
                this.propertyType = value;
            }
        }
        #endregion
    }
    #endregion
}
