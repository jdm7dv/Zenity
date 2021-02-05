// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using Zentity.Core;

    /// <summary>
    /// Represents records matched in the repository.
    /// It holds matching resources with percentage match for the specified criteria.
    /// </summary>
    [Serializable]
    public class SimilarRecord
    {
        #region Private Fields

        private Resource matchResource;
        private float percentageMatch;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the resource matched in the repository for the specified criteria.
        /// </summary>
        public Resource MatchingResource
        {
            set
            {
                this.matchResource = value;
            }
            get
            {
                return this.matchResource;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how much percentage the matched resource is close to the specified criteria.
        /// </summary>
        public float PercentageMatch
        {
            set
            {
                this.percentageMatch = value;
            }
            get
            {
                return this.percentageMatch;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Zentity.Platform.SimilarRecord"/> class.
        /// </summary>
        public SimilarRecord()
        {
        }

        #endregion
    }
}