// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Zentity.Core
{
    /// <summary>
    /// Defines the utility methods for using across the project.
    /// </summary>
    internal static partial class Utilities
    {
        /// <summary>
        /// Gets the path name and transaction token.
        /// </summary>
        /// <param name="storeConnection">The store connection.</param>
        /// <param name="transaction">The transaction to be used for the connection.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="contentId">The content id.</param>
        /// <param name="pathName">Name of the path.</param>
        /// <param name="transactionToken">The transaction token.</param>
        internal static void GetPathNameAndTransactionToken(SqlConnection storeConnection, SqlTransaction transaction, int commandTimeout, Guid contentId, out string pathName, out byte[] transactionToken)
        {
            SqlParameter paramId = new SqlParameter
            {
                DbType = DbType.Guid,
                ParameterName = CoreResources.Id,
                Value = contentId
            };

            SqlParameter paramPathName = new SqlParameter
            {
                DbType = DbType.String,
                Direction = ParameterDirection.Output,
                ParameterName = CoreResources.PathName,
                Size = 2048
            };

            SqlParameter paramTransactionContext = new SqlParameter
            {
                DbType = DbType.Binary,
                Direction = ParameterDirection.Output,
                ParameterName = CoreResources.TransactionContext,
                Size = 4000
            };

            Utilities.ExecuteNonQuery(storeConnection, transaction, commandTimeout,
                CoreResources.CmdGetPathNameAndTransactionToken, CommandType.Text,
                paramId, paramPathName, paramTransactionContext);

            pathName = paramPathName.Value == DBNull.Value ?
                null : paramPathName.Value.ToString();
            transactionToken = paramTransactionContext.Value == DBNull.Value ?
                null : (byte[])paramTransactionContext.Value;
        }

        /// <summary>
        /// Executes a Sql query without the option to return any results.
        /// </summary>
        /// <param name="storeConnection">The store connection.</param>
        /// <param name="transaction">The transaction to be used for the connection.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        internal static void ExecuteNonQuery(SqlConnection storeConnection, SqlTransaction transaction, int commandTimeout, string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                PrepareCommand(storeConnection, transaction, commandTimeout, commandText,
                    commandType, parameters, cmd);

                cmd.ExecuteNonQuery();

                // Update values for output parameters.
                foreach (SqlParameter inputParameter in parameters.
                    Where(tuple => tuple.Direction == ParameterDirection.Output ||
                        tuple.Direction == ParameterDirection.InputOutput))
                {
                    inputParameter.Value = cmd.Parameters[inputParameter.ParameterName].Value;
                }
            }
        }

        /// <summary>
        /// Prepares the command to be executed on Sql server.
        /// </summary>
        /// <param name="storeConnection">The store connection.</param>
        /// <param name="transaction">The transaction to be used for the connection.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cmd">The sql command object.</param>
        private static void PrepareCommand(SqlConnection storeConnection, SqlTransaction transaction, int commandTimeout, string commandText, CommandType commandType, SqlParameter[] parameters, SqlCommand cmd)
        {
            cmd.Connection = storeConnection;
            cmd.CommandTimeout = commandTimeout;
            if (transaction != null)
                cmd.Transaction = transaction;
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;

            // Load command parameters.
            foreach (SqlParameter inputParameter in parameters)
            {
                SqlParameter commandParameter = cmd.CreateParameter();
                commandParameter.DbType = inputParameter.DbType;
                commandParameter.Direction = inputParameter.Direction;
                commandParameter.IsNullable = inputParameter.IsNullable;
                commandParameter.ParameterName = inputParameter.ParameterName;
                commandParameter.Size = inputParameter.Size;
                commandParameter.Value = inputParameter.Value ?? DBNull.Value;
                cmd.Parameters.Add(commandParameter);
            }
        }

        /// <summary>
        /// Executes the Sql query with the option of returning only a scalar value.
        /// </summary>
        /// <param name="storeConnection">The store connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The scalar value</returns>
        internal static object ExecuteScalar(SqlConnection storeConnection, SqlTransaction transaction, int commandTimeout, string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                PrepareCommand(storeConnection, transaction, commandTimeout, commandText,
                    commandType, parameters, cmd);

                var result = cmd.ExecuteScalar();

                // Update values for output parameters.
                foreach (SqlParameter inputParameter in parameters.
                    Where(tuple => tuple.Direction == ParameterDirection.Output ||
                        tuple.Direction == ParameterDirection.InputOutput))
                {
                    inputParameter.Value = cmd.Parameters[inputParameter.ParameterName].Value;
                }

                return result;
            }
        }
    }
}
