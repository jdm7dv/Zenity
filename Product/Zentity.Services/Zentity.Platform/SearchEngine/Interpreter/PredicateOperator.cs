// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Globalization;
    using System.Text;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Represents predicate operator between predicate token 
    /// and expression of the Zentity search tree.
    /// </summary>
    internal class PredicateOperator : CompoundOperator
    {
        #region Properties

        /// <summary>
        /// Gets the predicate of the predicate operator.
        /// </summary>
        public new PredicateNode LeftChild
        {
            get
            {
                return base.LeftChild as PredicateNode;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateOperator"/> class.
        /// </summary>
        public PredicateOperator()
            : base(NodeType.Predicate)
        {
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

            // Need to capture in variables since LeftChild.ReverseRelation is set in the ConvertToTSql method.
            string leftChildTSql = LeftChild.ConvertToTSql(searchTokens);
            string rightChildTSql = RightChild.ConvertToTSql(searchTokens);
            return JoinTsql(searchTokens, String.Empty, leftChildTSql, rightChildTSql);
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
            if (insertClause)
            {
                if (searchTokens == null)
                {
                    throw new ArgumentNullException("searchTokens", Resources.EXCEPTION_ARGUMENTINVALID);
                }

                if (!isSimilarityClause)
                {
                    return InsertSortColumn(searchTokens, clause);
                }
                else
                {
                    return InsertSimilarityClause(searchTokens, clause);
                }
            }
            else
            {
                return ConvertToTSql(searchTokens);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Inserts the similarity clause.
        /// </summary>
        /// <param name="searchTokens">The search tokens.</param>
        /// <param name="similarityClause">The similarity clause.</param>
        /// <returns>Search tokens after inserting the similarity clause.</returns>
        private string InsertSimilarityClause(SearchTokens searchTokens, string similarityClause)
        {
            return CreateTsql(searchTokens, SearchConstants.TSQL_SUB + similarityClause);
        }

        /// <summary>
        /// Inserts the sort column.
        /// </summary>
        /// <param name="searchTokens">The search tokens.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>Search tokens after inserting the sort column.</returns>
        private string InsertSortColumn(SearchTokens searchTokens, string columnName)
        {
            if (!String.Equals(columnName, SearchConstants.TSQL_ID, StringComparison.OrdinalIgnoreCase))
            {
                return CreateTsql(searchTokens, SearchConstants.TSQL_SUB + columnName);
            }
            else
            {
                return ConvertToTSql(searchTokens);
            }
        }

        /// <summary>
        /// Creates the Transact-SQL.
        /// </summary>
        /// <param name="searchTokens">The search tokens.</param>
        /// <param name="clause">The clause.</param>
        /// <returns>The transact sql.</returns>
        private string CreateTsql(SearchTokens searchTokens, string clause)
        {
            // Need to capture in variables since LeftChild.ReverseRelation is set in the ConvertToTSql method.
            string leftChildTSql = LeftChild.ConvertToTSql(searchTokens, false, null, false);
            string rightChildTSql = RightChild.ConvertToTSql(searchTokens, false, null, false);
            return JoinTsql(searchTokens, SearchConstants.COMMA + clause,
                leftChildTSql, rightChildTSql);
        }

        /// <summary>
        /// Joins the TSQL.
        /// </summary>
        /// <param name="searchTokens">The search tokens.</param>
        /// <param name="clause">The clause.</param>
        /// <param name="leftChildTSql">The left child T SQL.</param>
        /// <param name="rightChildTSql">The right child T SQL.</param>
        /// <returns>Joins the transact sql.</returns>
        private string JoinTsql(
                            SearchTokens searchTokens,
                            string clause,
                            string leftChildTSql,
                            string rightChildTSql)
        {
            if (string.IsNullOrEmpty(rightChildTSql))
            {
                return string.Empty;
            }
            else
            {
                StringBuilder transactionSqlBuilder = new StringBuilder();
                if (LeftChild.ReverseRelation)
                {
                    transactionSqlBuilder.AppendFormat(CultureInfo.InvariantCulture,
                        SearchConstants.TSQL_REVERSE_PREDICATE_QUERY,
                        clause, leftChildTSql, rightChildTSql);
                }
                else
                {
                    transactionSqlBuilder.AppendFormat(CultureInfo.InvariantCulture,
                        SearchConstants.TSQL_PREDICATE_QUERY,
                        clause, leftChildTSql, rightChildTSql);
                }
                transactionSqlBuilder.Append(SearchConstants.SPACE);
                transactionSqlBuilder.Append(SearchConstants.AND);
                transactionSqlBuilder.Append(SearchConstants.SPACE);
                base.AppendsWhereClause(transactionSqlBuilder, searchTokens);
                return transactionSqlBuilder.ToString();
            }
        }

        #endregion
    }
}