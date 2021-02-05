// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Zentity.Core;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Represents a comparison expression node of the Zentity search tree.
    /// </summary>
    internal class ComparisonExpression : Expression
    {
        #region Private Fields

        /// <summary>
        /// List of valid operators.
        /// </summary>
        public static string[] ValidOperators = new string[] { 
            SearchConstants.EQUAL_TO,
            SearchConstants.GREATER_THAN,
            SearchConstants.GREATER_THAN_OR_EQUAL,
            SearchConstants.LESS_THAN,
            SearchConstants.LESS_THAN_OR_EQUAL };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the operator of the expression.
        /// </summary>
        public string Operator { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonExpression"/> class.
        /// </summary>
        public ComparisonExpression()
            : base(NodeType.ComparisonOperator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonExpression"/> class.
        /// </summary>
        /// <param name="comparisonOperator">Operator of the expression.</param>
        public ComparisonExpression(string comparisonOperator)
            : base(NodeType.ComparisonOperator)
        {
            Operator = comparisonOperator;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Appends the T-SQL equivalent of the specified property.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="property">Scalar property.</param>
        protected override void AppendPropertyTSql(StringBuilder transactionSqlBuilder, ScalarProperty property)
        {
            if (transactionSqlBuilder == null)
            {
                throw new ArgumentNullException("tSqlBuilder", Resources.EXCEPTION_ARGUMENTINVALID);
            }

            if (property == null)
            {
                throw new ArgumentNullException("property", Resources.EXCEPTION_ARGUMENTINVALID);
            }

            if (DataType == DataTypes.DateTime)
            {
                DateValue date = DateValue.Parse(Value);

                switch (Operator)
                {
                    case SearchConstants.EQUAL_TO:
                        {
                            transactionSqlBuilder.Append(string.Format(CultureInfo.CurrentCulture,
                                SearchConstants.TSQL_DATE_EQUAL_COMPARISON_CRITERIA,
                                Utility.EscapeColumnName(property.ColumnName), date.StartDate, date.NextDayOfEndDate));
                            break;
                        }
                    case SearchConstants.GREATER_THAN:
                        {
                            transactionSqlBuilder.Append(string.Format(CultureInfo.CurrentCulture,
                                SearchConstants.TSQL_COMPARISON_CRITERIA,
                                Utility.EscapeColumnName(property.ColumnName), SearchConstants.GREATER_THAN_OR_EQUAL, date.NextDayOfEndDate));
                            break;
                        }
                    case SearchConstants.GREATER_THAN_OR_EQUAL:
                        {
                            transactionSqlBuilder.Append(string.Format(CultureInfo.CurrentCulture,
                                SearchConstants.TSQL_COMPARISON_CRITERIA,
                                Utility.EscapeColumnName(property.ColumnName), Operator, date.StartDate));
                            break;
                        }
                    case SearchConstants.LESS_THAN:
                        {
                            transactionSqlBuilder.Append(string.Format(CultureInfo.CurrentCulture,
                                SearchConstants.TSQL_COMPARISON_CRITERIA,
                                Utility.EscapeColumnName(property.ColumnName), Operator, date.StartDate));
                            break;
                        }
                    case SearchConstants.LESS_THAN_OR_EQUAL:
                        {
                            transactionSqlBuilder.Append(string.Format(CultureInfo.CurrentCulture,
                                SearchConstants.TSQL_COMPARISON_CRITERIA,
                                Utility.EscapeColumnName(property.ColumnName), SearchConstants.LESS_THAN, date.NextDayOfEndDate));
                            break;
                        }
                }
            }
            else
            {
                transactionSqlBuilder.Append(string.Format(CultureInfo.CurrentCulture,
                    SearchConstants.TSQL_COMPARISON_CRITERIA,
                    Utility.EscapeColumnName(property.ColumnName), Operator,
                    Utility.EscapeTSqlSingleQuoteValue(Value)));
            }
        }

        /// <summary>
        /// Validates the node.
        /// </summary>
        protected override void ValidateInputs()
        {
            base.ValidateInputs();
            ValidateOperator();
        }

        #endregion 

        #region Private Methods

        /// <summary>
        /// Validates the operator.
        /// </summary>
        private void ValidateOperator()
        {
            if (string.IsNullOrEmpty(Operator) || !ValidOperators.Contains(Operator))
            {
                throw new SearchException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_INVALID_OPERATOR, Operator));
            }
        }

        #endregion
    }
}