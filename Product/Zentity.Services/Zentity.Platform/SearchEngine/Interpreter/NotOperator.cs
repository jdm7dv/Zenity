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
    /// Represents not operator between special token and expression of the Zentity search tree.
    /// </summary>
    internal class NotOperator : CompoundOperator
    {
        #region Properties

        /// <summary>
        /// Gets all the resources node of the not operator.
        /// </summary>
        public new AllResources LeftChild
        {
            get
            {
                return base.LeftChild as AllResources;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotOperator"/> class.
        /// </summary>
        public NotOperator()
            : base(NodeType.Not)
        { }

        #endregion 

        #region Overriden Methods

        /// <summary>
        /// Converts the node to T-SQL.
        /// </summary>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch tokens with.</param>
        /// <returns>T-SQL equivalent of the node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parameter is null.</exception>
        public override string ConvertToTSql(SearchTokens searchTokens)
        {
            if (searchTokens == null)
            {
                throw new ArgumentNullException("searchTokens", Resources.EXCEPTION_ARGUMENTINVALID);
            }

            string leftQuery = (LeftChild == null) ? null : LeftChild.ConvertToTSql(searchTokens);
            string rightQuery = (RightChild == null) ? null : RightChild.ConvertToTSql(searchTokens);
            return JoinQueries(leftQuery, SearchConstants.TSQL_EXCEPT, rightQuery);
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
            if (searchTokens == null)
            {
                throw new ArgumentNullException("searchTokens", Resources.EXCEPTION_ARGUMENTINVALID);
            }

            string leftQuery =
                (LeftChild == null)
                ? null : LeftChild.ConvertToTSql(searchTokens, insertClause, clause, isSimilarityClause);
            string rightQuery =
                (RightChild == null)
                ? null : RightChild.ConvertToTSql(searchTokens, insertClause, clause, isSimilarityClause);
            return JoinQueries(leftQuery, SearchConstants.TSQL_EXCEPT, rightQuery);
        }

        #endregion
    }
}