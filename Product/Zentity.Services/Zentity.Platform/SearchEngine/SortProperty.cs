// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;

    /// <summary>
    /// Represents a column on which to sort and sort direction.
    /// </summary>
    public class SortProperty
    {
        #region Properties

        /// <summary>
        /// Gets the name of the property on which to sort.
        /// </summary>
        public string PropertyName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the column is to be sorted in ascending order or in 
        /// descending order.
        /// </summary>
        public SortDirection SortDirection { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Zentity.Platform.SortProperty"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property on which to sort.</param>
        /// <param name="sortDirection">Sort direction.</param>
        public SortProperty(string propertyName, SortDirection sortDirection)
        {
            if (String.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            PropertyName = propertyName;
            SortDirection = sortDirection;
        }

        #endregion
    }

}
