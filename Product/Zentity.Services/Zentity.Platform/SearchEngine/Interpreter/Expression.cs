// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Zentity.Core;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Represents an expression node of the Zentity search tree.
    /// </summary>
    internal abstract class Expression : AllResources
    {
        #region Properties

        /// <summary>
        /// Gets or sets the type of expression token.
        /// </summary>
        public ExpressionTokenType TokenType { get; set; }

        /// <summary>
        /// Gets or sets the token name (SpecialToken/PropertyToken/ImplicitPropertiesToken).
        /// </summary>
        public string ExpressionToken { get; set; }

        /// <summary>
        /// Gets or sets the value of the resource property.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the data type of the expression.
        /// </summary>
        public DataTypes DataType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// </summary>
        /// <param name="type">Node type.</param>
        public Expression(NodeType type)
            : base(type)
        {
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Appends the T-SQL equivalent of the specified property.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="property">Scalar property.</param>
        protected abstract void AppendPropertyTSql(StringBuilder transactionSqlBuilder, ScalarProperty property);

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Appends the where clause of the query.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch tokens with.</param>
        public override void AppendsWhereClause(StringBuilder transactionSqlBuilder, SearchTokens searchTokens)
        {
            ValidateInputs();
            IEnumerable<ScalarProperty> properties = ExtractTokenInformation(searchTokens);
            if (properties.Count() != 0)
            {
                transactionSqlBuilder.Append(SearchConstants.OPEN_ROUND_BRACKET);
                AppendPropertiesTSql(transactionSqlBuilder, properties);
                transactionSqlBuilder.Append(SearchConstants.CLOSE_ROUND_BRACKET);
                transactionSqlBuilder.Append(SearchConstants.SPACE);
                transactionSqlBuilder.Append(SearchConstants.AND);
                transactionSqlBuilder.Append(SearchConstants.SPACE);
                base.AppendsWhereClause(transactionSqlBuilder, searchTokens);
            }
            else
            {
                transactionSqlBuilder.Remove(0, transactionSqlBuilder.Length);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Extracts the data type and properties of the token.
        /// </summary>
        /// <param name="searchTokens">The search tokens.</param>
        /// <returns>List of <see cref="ScalarProperty"/>.</returns>
        private IEnumerable<ScalarProperty> ExtractTokenInformation(SearchTokens searchTokens)
        {
            IEnumerable<ScalarProperty> properties;
            switch (TokenType)
            {
                case ExpressionTokenType.SpecialToken:
                    {
                        SpecialToken token = searchTokens.FetchSpecialToken(ExpressionToken, ResourceTypeFullName);
                        if (token == null)
                        {
                            throw new SearchException(string.Format(CultureInfo.CurrentCulture,
                                Resources.SEARCH_INVALID_SPECIAL_TOKEN, ExpressionToken));
                        }
                        properties = token.Properties;
                    }
                    break;
                case ExpressionTokenType.PropertyToken:
                    {
                        ScalarProperty token = searchTokens.FetchPropertyToken(ExpressionToken, ResourceTypeFullName);
                        if (token == null)
                        {
                            throw new SearchException(string.Format(CultureInfo.CurrentCulture,
                                Resources.SEARCH_INVALID_PROPERTY_TOKEN, ExpressionToken));
                        }
                        properties = new ScalarProperty[] { token };
                    }
                    break;
                case ExpressionTokenType.ImplicitPropertiesToken:
                default:
                    {
                        IEnumerable<ScalarProperty> tokens =
                           searchTokens.FetchImplicitProperties(ResourceTypeFullName);
                        properties = tokens;
                    }
                    break;
            }
            return properties;
        }

        /// <summary>
        /// Validates if the value is of the correct data type.
        /// </summary>
        protected void ValidateValueForDataType()
        {
            if (TokenType == ExpressionTokenType.ImplicitPropertiesToken &&
                DataType != DataTypes.String)
            {
                throw new SearchException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_INVALID_IMPLICITPROPERTIES_DATATYPE, DataType));
            }
            else if (!Utility.ValidateValueForDataType(DataType, Value))
            {
                throw new SearchException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_INVALID_VALUE, Value));
            }
        }

        /// <summary>
        /// Appends the T-SQL equivalent of the specified properties.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="properties">Scalar properties.</param>
        protected void AppendPropertiesTSql(
                                        StringBuilder transactionSqlBuilder,
                                        IEnumerable<ScalarProperty> properties)
        {
            if (properties.Count() != 0)
            {
                foreach (ScalarProperty property in properties)
                {
                    AppendPropertyTSql(transactionSqlBuilder, property);
                    transactionSqlBuilder.Append(SearchConstants.OR);
                }
                transactionSqlBuilder.Remove(transactionSqlBuilder.Length - SearchConstants.OR.Length, SearchConstants.OR.Length);
            }
        }

        /// <summary>
        /// Validates the node.
        /// </summary>
        protected virtual void ValidateInputs()
        {
            string expressionToken = ExpressionToken;
            if (string.IsNullOrEmpty(expressionToken))
            {
                if (TokenType != ExpressionTokenType.ImplicitPropertiesToken)
                {
                    string message = Resources.SEARCH_EMPTY_TOKEN;
                    if (TokenType == ExpressionTokenType.SpecialToken)
                    {
                        message = string.Format(CultureInfo.CurrentCulture,
                            Resources.SEARCH_INVALID_SPECIAL_TOKEN, expressionToken);
                    }
                    else if (TokenType == ExpressionTokenType.PropertyToken)
                    {
                        message = string.Format(CultureInfo.CurrentCulture,
                           Resources.SEARCH_INVALID_PROPERTY_TOKEN, expressionToken);
                    }
                    throw new SearchException(message);
                }
            }

            if (string.IsNullOrEmpty(ResourceTypeFullName))
            {
                throw new SearchException(string.Format(CultureInfo.CurrentCulture,
                       Resources.SEARCH_INVALID_RESOURCETYPE, ResourceTypeFullName));
            }

            if (Value == null)
            {
                throw new SearchException(string.Format(CultureInfo.InvariantCulture,
                    Resources.SEARCH_INVALID_VALUE, Value));
            }

            ValidateValueForDataType();
        }

        #endregion
    }
}