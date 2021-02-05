// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Text;
    using Zentity.Core;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Represents a contains expression node of the Zentity search tree.
    /// </summary>
    internal abstract class ContainsExpression : Expression
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainsExpression"/> class.
        /// </summary>
        /// <param name="type">Node type.</param>
        public ContainsExpression(NodeType type)
            : base(type)
        { }

        #endregion 

        #region Abstract Methods

        /// <summary>
        /// Appends a like clause.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="columnName">Column name operand of like operator.</param>
        protected abstract void AppendLikeClause(StringBuilder transactionSqlBuilder, string columnName);

        /// <summary>
        /// Appends a contains clause.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="columnName">Column name operand of contains operator.</param>
        protected abstract void AppendContainsClause(StringBuilder transactionSqlBuilder, string columnName);

        #endregion 

        #region Overriden Methods

        /// <summary>
        /// Appends the T-SQL equivalent of the specified property.
        /// </summary>
        /// <param name="transactionSqlBuilder"><see cref="StringBuilder" /> instance to append T-SQL to.</param>
        /// <param name="property">Scalar property.</param>
        protected override void AppendPropertyTSql(StringBuilder transactionSqlBuilder, ScalarProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property", Resources.EXCEPTION_ARGUMENTINVALID);
            }

            if (property.IsFullTextIndexed)
            {
                AppendContainsClause(transactionSqlBuilder, Utility.EscapeColumnName(property.ColumnName));
            }
            else
            {
                AppendLikeClause(transactionSqlBuilder, Utility.EscapeColumnName(property.ColumnName));
            }
        }

        #endregion
    }
}