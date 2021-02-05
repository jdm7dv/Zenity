// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

#region .NET Framework Class Namespace Imports

using System.Globalization;

#endregion

namespace Zentity.Platform
{
    /// <summary>
    /// Represents a compound condition node of the Zentity search tree.
    /// </summary>
    internal abstract class CompoundOperator : AllResources
    {
        #region Properties

        /// <summary>
        /// Gets or sets the left child of the compound condition.
        /// </summary>
        public virtual TreeNode LeftChild { get; set; }

        /// <summary>
        /// Gets or sets the right child of the compound condition.
        /// </summary>
        public virtual TreeNode RightChild { get; set; }

        #endregion 

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CompoundOperator"/>.
        /// </summary>
        /// <param name="type">Type of node.</param>
        public CompoundOperator(NodeType type)
            : base(type)
        { }

        #endregion 

        #region Protected Methods

        /// <summary>
        /// Joins left query and right query with the separator.
        /// </summary>
        /// <param name="leftQuery">Left query.</param>
        /// <param name="separator">Separator.</param>
        /// <param name="rightQuery">Right query.</param>
        /// <returns>Joined query.</returns>
        protected static string JoinQueries(string leftQuery, string separator, string rightQuery)
        {
            if (string.IsNullOrEmpty(leftQuery))
            {
                if (string.IsNullOrEmpty(rightQuery))
                {
                    return string.Empty;
                }
                else
                {
                    return rightQuery;
                }
            }
            else if (string.IsNullOrEmpty(rightQuery))
            {
                return leftQuery;
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, SearchConstants.TSQL_JOIN_QUERIES,
                    leftQuery, separator, rightQuery);
            }
        }

        #endregion
    }
}