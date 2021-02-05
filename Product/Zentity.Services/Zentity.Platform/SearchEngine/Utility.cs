// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Zentity.Core;
    using Zentity.Platform.Properties;
    using Zentity.Security.Authentication;
    using Zentity.Security.AuthorizationHelper;

    /// <summary>
    /// Provides utility methods.
    /// </summary>
    internal static class Utility
    {
        #region Private Fields

        private static Dictionary<string, XDocument> configFiles =
                                                        new Dictionary<string, XDocument>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Fetches Ids of matching resources.
        /// </summary>
        /// <param name="transactionSqlQuery">Query to be executed.</param>
        /// <param name="sqlConnectionString">Connection string to connect to DB.
        /// </param>
        /// <param name="sortProperty">A property on which to sort and 
        /// sort direction.</param>
        /// <param name="sortColumnName">Sort column name.</param>
        /// <param name="currentCursor">The number of matching records to skip.
        /// </param>
        /// <param name="maximumResultCount">Maximum number of matching 
        /// results to fetch.</param>
        /// <param name="totalRecords">The total number of matching records.</param>
        /// <returns>List of Guid.</returns>
        public static IEnumerable<Guid> FetchIdsOfMatchingResources(
                                                                string transactionSqlQuery,
                                                                string sqlConnectionString, 
                                                                SortProperty sortProperty, 
                                                                string sortColumnName,
                                                                int currentCursor, 
                                                                int maximumResultCount, 
                                                                out int totalRecords)
        {
            totalRecords = 
                Utility.FetchNumberOfMatchingResources(transactionSqlQuery, sqlConnectionString);

            if (totalRecords == 0)
            {
                return Utility.CreateEmptyEnumerable<Guid>();
            }
            else
            {
                if (sortProperty == null)
                {
                    return FetchIdsOfMatchingResources(transactionSqlQuery,
                        sqlConnectionString, currentCursor, maximumResultCount);
                }
                else
                {
                    return FetchIdsOfMatchingResources(
                        transactionSqlQuery, sqlConnectionString, sortColumnName,
                        sortProperty.SortDirection, currentCursor, maximumResultCount);
                }
            }
        }

        /// <summary>
        /// Fetches Ids and percentage match of matching resources.
        /// </summary>
        /// <param name="transactionSqlQuery">Query to be executed.</param>
        /// <param name="sqlConnectionString">Connection string to connect to DB.</param>
        /// <param name="currentCursor">The number of matching records to skip.</param>
        /// <param name="maximumResultCount">Maximum number of matching results to fetch.</param>
        /// <returns>List of Guid and matching percentage.</returns>
        public static Dictionary<Guid, float> FetchIdsAndPercentageMatchOfMatchingResources(
                                                                                string transactionSqlQuery, 
                                                                                string sqlConnectionString,
                                                                                int currentCursor, 
                                                                                int maximumResultCount)
        {
            string query = string.Format(
                CultureInfo.CurrentCulture, SearchConstants.TSQL_PAGING_PERCENTAGE_MATCH,
                transactionSqlQuery, (currentCursor + 1),
                (currentCursor + maximumResultCount));
            return Utility.ExecuteQueryForMatchingResources(query, sqlConnectionString);
        }

        /// <summary>
        /// Fetches number of matching resources.
        /// </summary>
        /// <param name="transactionSqlQuery">Query to be executed.</param>
        /// <param name="sqlConnectionString">Connection string to connect to DB.</param>
        /// <returns>List of Guid.</returns>
        public static int FetchNumberOfMatchingResources(
                                                        string transactionSqlQuery, 
                                                        string sqlConnectionString)
        {
            string query = string.Format(CultureInfo.CurrentCulture,
                SearchConstants.TSQL_TOTAL_COUNT, transactionSqlQuery);
            return Convert.ToInt32(Utility.ExecuteScalar(query, sqlConnectionString),
                CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Escapes characters with appropriate escape sequence.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Escaped value.</returns>
        public static string EscapeTSqlSingleQuoteValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            else
            {
                return value.Replace(
                    SearchConstants.SINGLE_QUOTE, SearchConstants.TWO_SINGLE_QUOTES);
            }
        }

        /// <summary>
        /// Escapes characters with appropriate escape sequence.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Escaped value.</returns>
        public static string EscapeTSqlLikeValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            value = EscapeTSqlSingleQuoteValue(value);
            return Regex.Replace(value, SearchConstants.TSQL_LIKE_ESCAPE_REGEX,
                SearchConstants.TSQL_LIKE_ESCAPE_REGEX_REPLACE);
        }

        /// <summary>
        /// Escapes characters with appropriate escape sequence.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Escaped value.</returns>
        public static string EscapeTSqlContainsValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            value = EscapeTSqlSingleQuoteValue(value);
            return value.Replace(
                SearchConstants.DOUBLE_QUOTE, SearchConstants.TWO_DOUBLE_QUOTES);
        }

        /// <summary>
        /// Establishes a connection with the Database and executes the query.
        /// </summary>
        /// <param name="query">query to be executed</param>
        /// <param name="sqlConnectionString">Connection string to connect to DB</param>
        /// <returns>List of Guid.</returns>
        public static IQueryable<Guid> ExecuteQuery(string query, 
            string sqlConnectionString)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException(
                    Resources.EXCEPTION_ARGUMENTINVALID, "query");
            }

            if (string.IsNullOrEmpty(sqlConnectionString))
            {
                throw new ArgumentException(
                    Resources.EXCEPTION_ARGUMENTINVALID, "sqlConnectionString");
            }

            List<Guid> idList = new List<Guid>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                SqlCommand command = 
                    new SqlCommand(SearchConstants.TSQL_SP_EXECUTESQL, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@statement", query));
                command.CommandTimeout = 3600;
                connection.Open();
                using (SqlDataReader dataReader = 
                    command.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    while (dataReader.Read())
                    {
                        idList.Add((Guid)dataReader[0]);
                    }
                }
            }
            return idList.AsQueryable();
        }

        /// <summary>
        /// Establishes a connection with the Database and executes the query.
        /// </summary>
        /// <param name="query">query to be executed</param>
        /// <param name="sqlConnectionString">Connection string to connect to DB</param>
        /// <returns>List of Guid and percentage match.</returns>
        public static Dictionary<Guid, float> ExecuteQueryForMatchingResources(
            string query, string sqlConnectionString)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException(
                    Resources.EXCEPTION_ARGUMENTINVALID, "query");
            }

            if (string.IsNullOrEmpty(sqlConnectionString))
            {
                throw new ArgumentException(
                    Resources.EXCEPTION_ARGUMENTINVALID, "sqlConnectionString");
            }

            Dictionary<Guid, float> matches = new Dictionary<Guid, float>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                SqlCommand command = 
                    new SqlCommand(SearchConstants.TSQL_SP_EXECUTESQL, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@statement", query));
                command.CommandTimeout = 3600;
                connection.Open();
                using (SqlDataReader dataReader = 
                    command.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    while (dataReader.Read())
                    {
                        matches.Add((Guid)dataReader[0], (float)(decimal)dataReader[1]);
                    }
                }
            }
            return matches;
        }

        /// <summary>
        /// Establishes a connection with the Database and executes the query.
        /// </summary>
        /// <param name="query">query to be executed</param>
        /// <param name="sqlConnectionString">Connection string to connect to DB</param>
        /// <returns>Scalar object.</returns>
        public static object ExecuteScalar(string query, string sqlConnectionString)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException(
                    Resources.EXCEPTION_ARGUMENTINVALID, "query");
            }

            if (string.IsNullOrEmpty(sqlConnectionString))
            {
                throw new ArgumentException(
                    Resources.EXCEPTION_ARGUMENTINVALID, "sqlConnectionString");
            }

            using (SqlConnection connection = 
                new SqlConnection(sqlConnectionString))
            {
                SqlCommand command = 
                    new SqlCommand(SearchConstants.TSQL_SP_EXECUTESQL, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@statement", query));
                command.CommandTimeout = 3600;
                connection.Open();
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Validates a value against a data type.
        /// </summary>
        /// <param name="dataType">Data type.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if valid; else false.</returns>
        public static bool ValidateValueForDataType(DataTypes dataType, string value)
        {
            bool isValid = false;
            switch (dataType)
            {
                case DataTypes.String:
                    {
                        isValid = true;
                    }
                    break;
                case DataTypes.DateTime:
                    {
                        DateValue date;
                        isValid = DateValue.TryParse(value, out date);
                    }
                    break;
                case DataTypes.Guid:
                    {
                        isValid = ValidateGuid(value);
                    }
                    break;
                case DataTypes.Boolean:
                case DataTypes.Byte:
                case DataTypes.Decimal:
                case DataTypes.Double:
                case DataTypes.Int16:
                case DataTypes.Int32:
                case DataTypes.Int64:
                case DataTypes.Single:
                    {
                        isValid = ValidateUsingReflection(dataType, value);
                    }
                    break;
            }
            return isValid;
        }

        /// <summary>
        /// Reads the XML configuration file corresponding to the application setting key.
        /// </summary>
        /// <param name="fileNameAppSettingsKey">Application setting key for 
        /// configuration file name.</param>
        /// <returns>Configuration file. Null if not specified.</returns>
        /// <exception cref="ConfigurationErrorsException">Thrown when file specified in the 
        /// application settings is not found.</exception>
        public static XDocument ReadXmlConfigurationFile(string fileNameAppSettingsKey)
        {
            // Read config file only once on its 1st call.
            if (fileNameAppSettingsKey != null &&
                configFiles.ContainsKey(fileNameAppSettingsKey))
            {
                return configFiles[fileNameAppSettingsKey];
            }
            else
            {
                string fileName = ConfigurationManager.AppSettings[fileNameAppSettingsKey];

                // If key not found in app.config return null.
                if (string.IsNullOrEmpty(fileName))
                {
                    return null;
                }

                // Throw exception if file extension is other than '.config'
                string extension = Path.GetExtension(fileName);

                if (string.IsNullOrEmpty(extension) 
                    || extension != SearchConstants.XML_FILE_EXTENSION)
                {
                    throw new ConfigurationErrorsException(
                        string.Format(CultureInfo.CurrentCulture,
                          Resources.SEARCH_INVALID_CONFIG_FILE_EXT,
                          SearchConstants.XML_FILE_EXTENSION, fileNameAppSettingsKey));
                }

                string filePath = 
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                // Check if file is present.
                if (!System.IO.File.Exists(filePath))
                {
                    throw new ConfigurationErrorsException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.SEARCH_CONFIG_FILE_NOT_FOUND,
                        fileName, AppDomain.CurrentDomain.BaseDirectory, 
                        fileNameAppSettingsKey));
                }

                // Read Xml file and validate against its XSD.
                using (FileStream stream = 
                    new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        XDocument document = null;
                        try
                        {
                            document = XDocument.Parse(reader.ReadToEnd());
                            ValidateXml(document, fileNameAppSettingsKey);
                        }
                        catch (XmlSchemaValidationException exception)
                        {
                            throw new ConfigurationErrorsException(
                                string.Format(CultureInfo.CurrentCulture,
                                  Resources.SEARCH_INVALID_CONFIG_FILE,
                                  fileName, fileNameAppSettingsKey), exception);
                        }
                        catch (XmlException exception)
                        {
                            throw new ConfigurationErrorsException(
                                string.Format(CultureInfo.CurrentCulture,
                                  Resources.SEARCH_INVALID_CONFIG_FILE,
                                  fileName, fileNameAppSettingsKey), exception);
                        }
                        // Add to already read config files list.
                        configFiles.Add(fileNameAppSettingsKey, document);
                        return document;
                    }
                }
            }
        }

        /// <summary>
        /// Returns an empty enumerable collection of type <typeref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of enumerable collection to return.</typeparam>
        /// <returns>Empty enumerable collection of type <typeref name="T" />.</returns>
        public static IEnumerable<T> CreateEmptyEnumerable<T>()
        {
            return new T[0].AsEnumerable<T>();
        }

        /// <summary>
        /// Appends the box braces to the specified column name.
        /// </summary>
        /// <param name="columnName">Database column name.</param>
        /// <returns>Column name with box braces.</returns>
        public static string EscapeColumnName(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException(
                    Resources.EXCEPTION_ARGUMENTINVALID, "columnName");
            }

            return SearchConstants.OPEN_BOX_BRACKET +
                columnName + SearchConstants.CLOSE_BOX_BRACKET;
        }

        /// <summary>
        /// Fetches the column name based on the resource property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch 
        /// tokens with.</param>
        /// <param name="resourceTypeFullName">Resource type full name.</param>
        /// <returns>Column name exclosed within box brackets.</returns>
        public static string GetColumnName(string propertyName,
            SearchTokens searchTokens, string resourceTypeFullName)
        {
            ScalarProperty scalarProperty =
                searchTokens.FetchPropertyToken(propertyName, resourceTypeFullName);
            if (scalarProperty != null)
            {
                return Utility.EscapeColumnName(scalarProperty.ColumnName);
            }
            else
            {
                throw new ArgumentException(
                    String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.SEARCH_INVALID_SORT_PROPERTY, propertyName));
            }
        }

        /// <summary>
        /// Appends the authorization clause to TSQL query.
        /// </summary>
        /// <param name="token">Authenticated token representing the user.</param>
        /// <param name="transactionSqlQuery">Tsql query</param>
        /// <param name="storeConnectionString">Store connection string.</param>
        /// <returns>TSQL with authorization clause.</returns>
        public static string AppendAuthorizationCriteria(
                                                        AuthenticatedToken token, 
                                                        string transactionSqlQuery, 
                                                        string storeConnectionString)
        {
            string securityWhereClause =
                TsqlAuthorization.GetAuthorizationCriteria(token, storeConnectionString);

            string matchingResourcesSqlQuery = String.Format(CultureInfo.InvariantCulture,
                SearchConstants.TSQL_AUTHORIZATION, transactionSqlQuery, securityWhereClause);

            return matchingResourcesSqlQuery;
        }

        /// <summary>
        /// Validates input string.
        /// </summary>
        /// <param name="searchQuery">Input string.</param>
        public static void ValidateInputQuery(string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                throw new ArgumentNullException("searchQuery");
            }
            bool isValid = ValidateParentheses(searchQuery);
            if (!isValid)
            {
                throw new SearchException(
                    Resources.SEARCH_PARENTHESES_NOT_AT_APPROPRIATE_PLACES);
            }
        }

        /// <summary>
        /// Returns the first string in quotes if the input string starts with a 
        /// single/double quote.
        /// </summary>
        /// <param name="searchQuery">Input string.</param>
        /// <returns>Quoted string is input string starts with a 
        /// single/double quote else empty.</returns>
        public static string GetQuotedString(ref string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                return String.Empty;
            }
            searchQuery = searchQuery.TrimStart(SearchConstants.SPACE[0]);

            if (searchQuery.StartsWith(
                SearchConstants.DOUBLE_QUOTE, StringComparison.Ordinal)
                || searchQuery.StartsWith(
                SearchConstants.SINGLE_QUOTE, StringComparison.Ordinal))
            {
                char quote = searchQuery[0];
                StringBuilder firstTokenBuilder = new StringBuilder();
                firstTokenBuilder.Append(quote.ToString());
                bool endQuoteFound = false;
                for (int i = 1; i < searchQuery.Length; i++)
                {
                    firstTokenBuilder.Append(searchQuery[i].ToString());
                    if (searchQuery[i] == quote &&
                        firstTokenBuilder[firstTokenBuilder.Length - 2]
                        != SearchConstants.ESCAPE_CHARACTER[0]
                        )
                    {
                        endQuoteFound = true;
                        break;
                    }
                }
                if (!endQuoteFound)
                {
                    // If matching end quote not found, then return empty.
                    return String.Empty;
                }
                string firstToken = firstTokenBuilder.ToString();
                searchQuery = searchQuery.Remove(0, firstToken.Length);
                return firstToken;
            }
            return String.Empty;
        }

        /// <summary>
        /// Appends the T-SQL equivalent of resource type.
        /// </summary>
        /// <param name="transactionSqlBuilder">
        /// <see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="resourceTypeFullName">Resource type full name.</param>
        /// <param name="searchTokens">
        /// <see cref="SearchTokens" /> instance to fetch tokens with.</param>
        public static void AppendResourceTypesTSql(
                                                StringBuilder transactionSqlBuilder, 
                                                string resourceTypeFullName, 
                                                SearchTokens searchTokens)
        {
            transactionSqlBuilder.Append(SearchConstants.OPEN_ROUND_BRACKET);
            ResourceType type = searchTokens.FetchResourceType(resourceTypeFullName);

            IEnumerable<string> excludedResourceTypeFullNames =
                SearchTokens.FetchExcludedResourceTypeFullNames();

            if (!excludedResourceTypeFullNames
                .Contains(resourceTypeFullName.ToLower()))
            {
                AppendResourceTypeTSql(transactionSqlBuilder, type.Id);
            }
            else
            {
                // If all the types in hierarchy are in excluded list, 
                // query formed should return zero results,
                // hence attaching a random Guid as a resource Id.
                AppendResourceTypeTSql(transactionSqlBuilder, Guid.NewGuid());
            }

            IEnumerable<ResourceType> derivedTypes =
                searchTokens.FetchResourceDerivedTypes(type)
                .Where(resourceType => !excludedResourceTypeFullNames
                    .Contains(resourceType.FullName.ToLower()));

            IEnumerable<Guid> derivedTypeIds = derivedTypes
                .Select(resourceType => resourceType.Id);

            foreach (Guid derivedTypeId in derivedTypeIds)
            {
                transactionSqlBuilder.Append(SearchConstants.OR);
                AppendResourceTypeTSql(transactionSqlBuilder, derivedTypeId);
            }
            transactionSqlBuilder.Append(SearchConstants.CLOSE_ROUND_BRACKET);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Appends resource type id to TSQL.
        /// </summary>
        /// <param name="transactionSqlBuilder">Transaction sql.</param>
        /// <param name="typeId">Resource type id.</param>
        private static void AppendResourceTypeTSql(StringBuilder transactionSqlBuilder, Guid typeId)
        {
            transactionSqlBuilder.Append(string.Format(CultureInfo.CurrentCulture,
                    SearchConstants.TSQL_RESOURCETYPEID_CRITERIA, typeId));
        }

        /// <summary>
        /// Validates Xml document against XSD.
        /// </summary>
        /// <param name="document">Xml document.</param>
        /// <param name="fileNameAppSettingsKey">app setting key.</param>
        private static void ValidateXml(XDocument document, 
            string fileNameAppSettingsKey)
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            XmlReader schemaDocument = GetSchemaDocument(fileNameAppSettingsKey);

            if (schemaDocument != null)
            {
                schemas.Add(SearchConstants.XMLNS_NAMESPACE, schemaDocument);
                document.Validate(schemas, null);
            }
        }

        /// <summary>
        /// Gets appropriate XSD based on the specified app setting key.
        /// </summary>
        /// <param name="fileNameAppSettingsKey">App setting key.</param>
        /// <returns>Xml schema document.</returns>
        private static XmlReader GetSchemaDocument(string fileNameAppSettingsKey)
        {
            StringReader reader = null;
            if (fileNameAppSettingsKey == Resources.SEARCH_PREDICATETOKENS_FILENAME)
            {
                reader = new StringReader(Resources.PredicateTokensSchema);
            }
            else if (fileNameAppSettingsKey == Resources.SEARCH_SPECIALTOKENS_FILENAME)
            {
                reader = new StringReader(Resources.SpecialTokensSchema);
            }
            else if (fileNameAppSettingsKey == Resources.SEARCH_IMPLICITPROPERTIES_FILENAME)
            {
                reader = new StringReader(Resources.ImplicitPropertiesSchema);
            }
            else if (fileNameAppSettingsKey == Resources.SEARCH_EXCLUDEDPREDICATES_FILENAME)
            {
                reader = new StringReader(Resources.ExcludedPredicatesSchema);
            }
            else if (fileNameAppSettingsKey == Resources.SEARCH_EXCLUDEDRESOURCETYPES_FILENAME)
            {
                reader = new StringReader(Resources.ExcludedResourceTypesSchema);
            }
            else
            {
                return null;
            }
            return XmlReader.Create(reader);
        }

        /// <summary>
        /// Validates parentheses.
        /// </summary>
        /// <param name="searchQuery">Input string.</param>
        /// <returns>true, if parentheses are at appropriate places; 
        /// else false.</returns>
        private static bool ValidateParentheses(string searchQuery)
        {
            Stack<string> parenthesisStack = new Stack<string>();

            while (!String.IsNullOrEmpty(searchQuery))
            {
                string token = GetQuotedString(ref searchQuery);
                // Discard quoted characters.
                if (String.IsNullOrEmpty(token) && !String.IsNullOrEmpty(searchQuery))
                {
                    token = searchQuery[0].ToString();
                    searchQuery = searchQuery.Remove(0, 1);

                    if (String.Equals(token, SearchConstants.OPEN_ROUND_BRACKET))
                    {
                        parenthesisStack.Push(SearchConstants.OPEN_ROUND_BRACKET);
                    }
                    else if (String.Equals(token, SearchConstants.CLOSE_ROUND_BRACKET))
                    {
                        if (parenthesisStack.Count == 0)
                        {
                            return false;
                        }
                        parenthesisStack.Pop();
                    }
                }
            }
            if (parenthesisStack.Count == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fetches Ids of matching resources.
        /// </summary>
        /// <param name="transactionSqlQuery">Query to be executed.</param>
        /// <param name="sqlConnectionString">Connection string to connect to DB.
        /// </param>
        /// <param name="currentCursor">The number of matching records to skip.
        /// </param>
        /// <param name="maximumResultCount">Maximum number of matching results 
        /// to fetch.</param>
        /// <returns>List of Guid.</returns>
        private static IEnumerable<Guid> FetchIdsOfMatchingResources(
                                                                    string transactionSqlQuery, 
                                                                    string sqlConnectionString,
                                                                    int currentCursor, 
                                                                    int maximumResultCount)
        {
            string query = string.Format(
                CultureInfo.CurrentCulture, SearchConstants.TSQL_PAGING,
                SearchConstants.TSQL_SELECT_1, transactionSqlQuery, (currentCursor + 1), 
                (currentCursor + maximumResultCount));

            return Utility.ExecuteQuery(query, sqlConnectionString);
        }

        /// <summary>
        /// Fetches Ids of matching resources.
        /// </summary>
        /// <param name="transactionSqlQuery">Query to be executed.</param>
        /// <param name="sqlConnectionString">Connection string to connect to DB.
        /// </param>
        /// <param name="sortColumnName">olumn name on which to sort.</param>
        /// <param name="sortDirection">Sort direction.</param>
        /// <param name="currentCursor">The number of matching records to skip.
        /// </param>
        /// <param name="maximumResultCount">Maximum number of matching 
        /// results to fetch.</param>
        /// <returns>List of Guid.</returns>
        private static IEnumerable<Guid> FetchIdsOfMatchingResources(
                                                                    string transactionSqlQuery, 
                                                                    string sqlConnectionString,
                                                                    string sortColumnName, 
                                                                    SortDirection sortDirection,
                                                                    int currentCursor, 
                                                                    int maximumResultCount)
        {
            StringBuilder sortCondition = new StringBuilder();
            sortCondition.Append(sortColumnName + SearchConstants.SPACE);
            if (sortDirection == SortDirection.Ascending)
            {
                sortCondition.Append(SearchConstants.TSQL_ASC);
            }
            else
            {
                sortCondition.Append(SearchConstants.TSQL_DESC);
            }
            string query = string.Format(
                CultureInfo.CurrentCulture, SearchConstants.TSQL_PAGING,
                sortCondition.ToString(), transactionSqlQuery,
                (currentCursor + 1), (currentCursor + maximumResultCount));
            return Utility.ExecuteQuery(query, sqlConnectionString);
        }

        /// <summary>
        /// Validates a value against guid data type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if valid; else false.</returns>
        private static bool ValidateGuid(string value)
        {
            try
            {
                Guid result = new Guid(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates a value against a data type using reflection.
        /// </summary>
        /// <param name="dataType">Data type.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if valid; else false.</returns>
        private static bool ValidateUsingReflection(DataTypes dataType, string value)
        {
            bool isValid;
            Type type = Type.GetType(
                SearchConstants.SYSTEM_TYPE_NAMESPACE_PREFIX + dataType.ToString());
            if (type == null)
            {
                isValid = false;
            }
            else
            {
                MethodInfo method = type.GetMethod(SearchConstants.TRY_PARSE,
                    new Type[] { typeof(string), type.MakeByRefType() });
                if (method == null)
                {
                    isValid = false;
                }
                else
                {
                    isValid = (bool)method.Invoke(null, new object[] { value, null });
                }
            }
            return isValid;
        }

        #endregion
    }
}