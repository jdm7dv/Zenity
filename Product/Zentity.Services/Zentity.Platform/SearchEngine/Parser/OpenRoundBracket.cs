// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Represents an open round bracket.
    /// </summary>
    internal class OpenRoundBracket : TreeNode
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenRoundBracket"/> class.
        /// </summary>
        public OpenRoundBracket()
            : base(NodeType.OpenRoundBracket)
        {
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Converts the node to T-SQL.
        /// </summary>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch tokens with.</param>
        /// <returns>T-SQL equivalent of the node.</returns>
        public override string ConvertToTSql(SearchTokens searchTokens)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the node to T-SQL.
        /// </summary>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch tokens with.</param>
        /// <param name="insertClause">If true, insert the specified clause in TSQL.</param>
        /// <param name="clause">Sort column name or similarity clause to be inserted.</param>
        /// <param name="isSimilarityClause">If true, insert the specified similarity clause in 
        /// TSQL else insert sort clause.</param>
        /// <returns>T-SQL equivalent of the node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parameter is null.</exception>
        public override string ConvertToTSql(SearchTokens searchTokens,
            bool insertClause, string clause, bool isSimilarityClause)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
