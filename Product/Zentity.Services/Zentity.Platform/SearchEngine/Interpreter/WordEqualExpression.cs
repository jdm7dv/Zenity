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
    /// Represents a contains word equal to expression node of the Zentity search tree.
    /// </summary>
    internal class WordEqualExpression : ContainsExpression
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WordEqualExpression"/> class.
        /// </summary>
        public WordEqualExpression()
            : base(NodeType.WordEqual)
        { 
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Appends a contains clause.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="columnName">Column name operand of contains operator.</param>
        protected override void AppendContainsClause(StringBuilder transactionSqlBuilder, string columnName)
        {
            if (transactionSqlBuilder == null)
            {
                throw new ArgumentNullException("tSqlBuilder", Resources.EXCEPTION_ARGUMENTINVALID);
            }

            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException(Resources.EXCEPTION_ARGUMENTINVALID, "columnName");
            }

            transactionSqlBuilder.Append(string.Format(CultureInfo.InvariantCulture,
                SearchConstants.TSQL_WORDEQUAL_CONTAINS_CRITERIA, columnName,
                Utility.EscapeTSqlContainsValue(Value)));
        }

        /// <summary>
        /// Appends a like clause.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="columnName">Column name operand of contains operator.</param>
        protected override void AppendLikeClause(StringBuilder transactionSqlBuilder, string columnName)
        {
            if (transactionSqlBuilder == null)
            {
                throw new ArgumentNullException("tSqlBuilder", Resources.EXCEPTION_ARGUMENTINVALID);
            }

            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException(Resources.EXCEPTION_ARGUMENTINVALID, "columnName");
            }

            transactionSqlBuilder.Append(string.Format(CultureInfo.InvariantCulture,
                SearchConstants.TSQL_WORDEQUAL_LIKE_CRITERIA, columnName,
                Utility.EscapeTSqlLikeValue(Value)));
        }

        #endregion
    }
}