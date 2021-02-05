// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    /// <summary>
    /// Represents a node of the Zentity search tree.
    /// </summary>
    internal abstract class TreeNode
    {
        #region Properties

        /// <summary>
        /// Gets the type of node.
        /// </summary>
        public NodeType Type { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="TreeNode"/>.
        /// </summary>
        /// <param name="type">Type of node.</param>
        public TreeNode(NodeType type)
        {
            Type = type;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Converts the node to T-SQL.
        /// </summary>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch tokens with.</param>
        /// <returns>T-SQL equivalent of the node.</returns>
        public abstract string ConvertToTSql(SearchTokens searchTokens);

        /// <summary>
        /// Converts the node to T-SQL.
        /// </summary>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch tokens with.</param>
        /// <param name="insertClause">If true, insert the specified clause in TSQL.</param>
        /// <param name="clause">Sort column name or similarity clause to be inserted.</param>
        /// <param name="isSimilarityClause">If true, insert the specified similarity clause in TSQL else insert sort clause.</param>
        /// <returns>T-SQL equivalent of the node.</returns>
        public abstract string ConvertToTSql(SearchTokens searchTokens, bool insertClause, string clause, bool isSimilarityClause);

        #endregion
    }
}