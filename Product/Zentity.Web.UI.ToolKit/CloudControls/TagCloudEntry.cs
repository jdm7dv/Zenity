// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using Zentity.ScholarlyWorks;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Helper class for setting and getting font size and reference count.
    /// </summary>
    internal class TagCloudEntry
    {
        #region Member variables

        #region Private

        private Tag _tag;
        private int _referenceCount;

        #endregion Private

        #endregion Member variables

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the tag for the tag cloud.
        /// </summary>
        public Tag Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of resources related to the tag for the tag cloud.
        /// </summary>
        public int ReferenceCount
        {
            get
            {
                return _referenceCount;
            }
            set
            {
                _referenceCount = value;
            }
        }

        #endregion Public

        #endregion Properties
    }
}