// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Represents predicate node of the Zentity search tree.
    /// </summary>
    internal class PredicateNode : TreeNode
    {
        #region Properties

        /// <summary>
        /// Gets or sets the predicate token.
        /// </summary>
        public string Token
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the relation is a reverse relationship.
        /// </summary>
        public bool ReverseRelation
        {
            get;
            private set;
        }

        #endregion 

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateNode"/> class.
        /// </summary>
        public PredicateNode()
            : base(NodeType.PredicateNode)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateNode"/> class.
        /// </summary>
        /// <param name="token">The predicate token.</param>
        public PredicateNode(string token)
            : base(NodeType.PredicateNode)
        {
            Token = token;
        }

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
            
            Validate();
            IEnumerable<PredicateToken> predicateTokens = searchTokens.FetchPredicateToken(Token);

            if (predicateTokens.Count() > 0)
            {
                // Assume the relationship direction as the first.
                this.ReverseRelation = predicateTokens.FirstOrDefault().ReverseRelation;

                StringBuilder transactionSqlBuilder = new StringBuilder();
                AppendCriteriaTSql(transactionSqlBuilder, predicateTokens);
                return transactionSqlBuilder.ToString();
            }
            else
            {
                throw new SearchException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_INVALID_PREDICATE_TOKEN, Token));
            }
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
            return ConvertToTSql(searchTokens);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Appends the T-SQL equivalent of the specified predicate tokens.
        /// </summary>
        /// <param name="transactionSqlBuilder">StringBuilder object to append to.</param>
        /// <param name="predicateTokens">Predicate tokens.</param>
        private static void AppendCriteriaTSql(StringBuilder transactionSqlBuilder, IEnumerable<PredicateToken> predicateTokens)
        {
            foreach (PredicateToken predicateToken in predicateTokens)
            {
                AppendCriterionTSql(transactionSqlBuilder, predicateToken);
                transactionSqlBuilder.Append(SearchConstants.OR);
            }
            transactionSqlBuilder.Remove(transactionSqlBuilder.Length - SearchConstants.OR.Length, SearchConstants.OR.Length);
        }

        /// <summary>
        /// Appends the T-SQL equivalent of the specified predicate tokens.
        /// </summary>
        /// <param name="transactionSqlBuilder">StringBuilder object to append to.</param>
        /// <param name="predicateToken">Predicate token.</param>
        private static void AppendCriterionTSql(StringBuilder transactionSqlBuilder, PredicateToken predicateToken)
        {
            transactionSqlBuilder.Append(string.Format(
                                                    CultureInfo.InvariantCulture,
                                                    SearchConstants.TSQL_PREDICATE_CRITERIA,
                                                    predicateToken.PredicateName));
        }

        /// <summary>
        /// Validates the node.
        /// </summary>
        private void Validate()
        {
            string token = Token;
            if (string.IsNullOrEmpty(token))
            {
                throw new SearchException(string.Format(CultureInfo.InvariantCulture,
                    Resources.SEARCH_EMPTY_TOKEN));
            }
        }

        #endregion
    }
}