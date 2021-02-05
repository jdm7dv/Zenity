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
    using System.Text;
    using Zentity.Core;
    using Zentity.Platform.Properties;
    using Zentity.Security.Authentication;

    /// <summary>
    /// Represents the interpreter.
    /// </summary>
    internal class Interpreter
    {
        #region Private Fields

        private SearchTokens searchTokens;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the maximum number of matching results to fetch.
        /// </summary>
        public int MaximumResultCount { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Interpreter"/> class.
        /// </summary>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch tokens with.</param>
        /// <param name="maximumResultCount">Maximum number of results to return in paged search.</param>
        public Interpreter(SearchTokens searchTokens, int maximumResultCount)
        {
            if (searchTokens == null)
            {
                throw new ArgumentNullException("searchTokens");
            }

            this.searchTokens = searchTokens;
            MaximumResultCount = maximumResultCount;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Interprets a Zentity search tree into a list of matching resource ids. Results will be 
        /// paged according to input criteria.
        /// </summary>
        /// <param name="parseTree">Zentity search tree.</param>
        /// <param name="token">Authenticated token representing the user.</param>
        /// <param name="currentCursor">The number of matching records to skip.</param>
        /// <param name="totalRecords">The total number of records matching the search criteria.</param>
        /// <returns>Ids of matching resources.</returns>
        public IEnumerable<Guid> Interpret(TreeNode parseTree, AuthenticatedToken token,
            int currentCursor, out int totalRecords)
        {
            return Interpret(parseTree, null, null, token, currentCursor, out totalRecords);
        }

        /// <summary>
        /// Interprets a Zentity search tree into a list of matching resource ids. Results will be 
        /// paged according to input criteria.
        /// </summary>
        /// <param name="parseTree">Zentity search tree.</param>
        /// <param name="sortProperty">A property on which to sort and sort direction.</param>
        /// <param name="resourceTypeFullName">Resource type full name.</param>
        /// <param name="token">Authenticated token representing the user.</param>
        /// <param name="currentCursor">The number of matching records to skip.</param>
        /// <param name="totalRecords">The total number of records matching the search criteria.</param>
        /// <returns>Ids of matching resources.</returns>
        public IEnumerable<Guid> Interpret(TreeNode parseTree, SortProperty sortProperty, 
            string resourceTypeFullName, AuthenticatedToken token, int currentCursor, out int totalRecords)
        {
            if (parseTree == null)
            {
                throw new ArgumentNullException("parseTree", Resources.EXCEPTION_ARGUMENTINVALID);
            }

            string transactionSqlQuery = string.Empty;
            string sortColumnName = string.Empty;

            if (sortProperty == null
                || String.Equals(sortProperty.PropertyName, SearchConstants.TSQL_ID,
                    StringComparison.OrdinalIgnoreCase))
            {
                transactionSqlQuery = parseTree.ConvertToTSql(searchTokens);
            }
            else
            {
                //Get scalar property column name
                sortColumnName =
                    Utility.GetColumnName(sortProperty.PropertyName, searchTokens, resourceTypeFullName);

                transactionSqlQuery = 
                    parseTree.ConvertToTSql(searchTokens, true, sortColumnName, false);
            }

            if (string.IsNullOrEmpty(transactionSqlQuery))
            {
                totalRecords = 0;
                return Utility.CreateEmptyEnumerable<Guid>();
            }

            if (token != null)
            {
                transactionSqlQuery = 
                    Utility.AppendAuthorizationCriteria(token, transactionSqlQuery, searchTokens.SqlConnectionString);
            }

            return Utility.FetchIdsOfMatchingResources(transactionSqlQuery,
                searchTokens.SqlConnectionString, sortProperty,
                sortColumnName, currentCursor, MaximumResultCount, out totalRecords);
        }

        /// <summary>
        /// Interprets a Zentity search tree into a list of matching resource ids. 
        /// Results will be paged according to input criteria.
        /// </summary>
        /// <param name="resourceTypeFullName">Resource type full name.</param>
        /// <param name="parseTree">Zentity search tree.</param>
        /// <param name="searchCriteria">Search criteria.</param>
        /// <param name="token">Authenticated token representing the user.</param>
        /// <param name="currentCursor">The number of matching records to skip.</param>
        /// <param name="totalRecords">The total number of records matching the search criteria.</param>
        /// <returns>Ids of matching resources.</returns>
        public Dictionary<Guid, float> Interpret(
            string resourceTypeFullName, TreeNode parseTree, IEnumerable<PropertyValuePair> searchCriteria,
            AuthenticatedToken token, int currentCursor, out int totalRecords)
        {
            if (parseTree == null)
            {
                throw new ArgumentNullException("parseTree", Resources.EXCEPTION_ARGUMENTINVALID);
            }

            string matchingPercentageClause =
                GetMatchingPercentageClause(searchCriteria, resourceTypeFullName);
            string transactionSqlQuery =
                parseTree.ConvertToTSql(searchTokens, true, matchingPercentageClause, true);

            if (string.IsNullOrEmpty(transactionSqlQuery))
            {
                totalRecords = 0;
                return new Dictionary<Guid, float>();
            }

            if (token != null)
            {
                transactionSqlQuery = 
                    Utility.AppendAuthorizationCriteria(token, transactionSqlQuery, searchTokens.SqlConnectionString);
            }

            totalRecords = 
                Utility.FetchNumberOfMatchingResources(transactionSqlQuery, searchTokens.SqlConnectionString);
            if (totalRecords == 0)
            {
                return new Dictionary<Guid, float>();
            }
            else
            {
                return Utility.FetchIdsAndPercentageMatchOfMatchingResources(
                    transactionSqlQuery, searchTokens.SqlConnectionString, 
                    currentCursor, MaximumResultCount);
            }
        }

        #endregion 

        #region Private Methods

        /// <summary>
        /// Gets the matching percentage clause.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="resourceTypeFullName">Full name of the resource type.</param>
        /// <returns>The matching percentage clause.</returns>
        private string GetMatchingPercentageClause(
                                            IEnumerable<PropertyValuePair> searchCriteria, 
                                            string resourceTypeFullName)
        {
            StringBuilder numerator = new StringBuilder();
            StringBuilder denominator = new StringBuilder();
            int numeratorLength = 0;
            foreach (PropertyValuePair pair in searchCriteria)
            {
                ScalarProperty scalarProperty =
                    searchTokens.FetchPropertyToken(pair.PropertyName, resourceTypeFullName);
                string columnName = Utility.EscapeColumnName(scalarProperty.ColumnName);

                if (scalarProperty.DataType == DataTypes.String)
                {
                    numeratorLength += pair.PropertyValue.Length;
                }
                else // if not string type then add the actual length to both numerator and denominator.
                {
                    numerator.AppendFormat(
                        CultureInfo.InvariantCulture, SearchConstants.TSQL_CAST_LEN,
                        SearchConstants.TSQL_SUB + columnName);
                    numerator.Append(SearchConstants.TSQL_PLUS);
                }
                    denominator.AppendFormat(
                        CultureInfo.InvariantCulture, SearchConstants.TSQL_CAST_LEN,
                        SearchConstants.TSQL_SUB + columnName);
                    denominator.Append(SearchConstants.TSQL_PLUS);
            }
            numerator.Append(numeratorLength.ToString());
            denominator.Remove(denominator.Length - 1, SearchConstants.TSQL_PLUS.Length);

            return
                String.Format(CultureInfo.InvariantCulture,
                SearchConstants.TSQL_PERCENTAGE_MATCH_FORMULA, numerator.ToString(),
                denominator.ToString());
        }

        #endregion
    }
}