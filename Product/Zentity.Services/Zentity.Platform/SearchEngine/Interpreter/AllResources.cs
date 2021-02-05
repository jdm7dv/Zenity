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
    /// Represents predicate node of the Zentity search tree.
    /// </summary>
    internal class AllResources : TreeNode
    {
        #region Properties

        /// <summary>
        /// Gets or sets the full name of the resource type.
        /// </summary>
        public string ResourceTypeFullName { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AllResources"/> class.
        /// </summary>
        /// <param name="type">Type of node.</param>
        public AllResources(NodeType type)
            : base(type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AllResources"/> class.
        /// </summary>
        public AllResources()
            : base(NodeType.AllResources)
        { 
        }

        #endregion

        #region Public Methods

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

            return CreateTsql(searchTokens, String.Empty);
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

        /// <summary>
        /// Appends the where clause of the query.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch tokens with.</param>
        public virtual void AppendsWhereClause(StringBuilder transactionSqlBuilder, SearchTokens searchTokens)
        {
            Utility.AppendResourceTypesTSql(transactionSqlBuilder, ResourceTypeFullName, searchTokens);
        }

        #endregion 

        #region Private Methods

        /// <summary>
        /// Gets the t-sql with the insert sort column query.
        /// </summary>
        /// <param name="searchTokens">The search tokens.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>Insert sort column query.</returns>
        private string InsertSortColumn(SearchTokens searchTokens, string columnName)
        {
            return CreateTsql(searchTokens,
                SearchConstants.COMMA + SearchConstants.TSQL_SUB + columnName);
        }

        /// <summary>
        /// Gets the t-sql with the insert similarity clause query.
        /// </summary>
        /// <param name="searchTokens">The search tokens.</param>
        /// <param name="similarityClause">The similarity clause.</param>
        /// <returns>The insert similarity clause query.</returns>
        private string InsertSimilarityClause(SearchTokens searchTokens, string similarityClause)
        {
            return CreateTsql(searchTokens, SearchConstants.COMMA + similarityClause);
        }

        /// <summary>
        /// Creates the TSQL.
        /// </summary>
        /// <param name="searchTokens">The search tokens.</param>
        /// <param name="clause">The clause.</param>
        /// <returns>The t-sql.</returns>
        private string CreateTsql(SearchTokens searchTokens, string clause)
        {
            StringBuilder transactionSqlBuilder = new StringBuilder();

            transactionSqlBuilder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    SearchConstants.TSQL_RESOURCE_QUERY, clause);

            AppendsWhereClause(transactionSqlBuilder, searchTokens);
            return transactionSqlBuilder.ToString();
        }

        #endregion
    }
}
