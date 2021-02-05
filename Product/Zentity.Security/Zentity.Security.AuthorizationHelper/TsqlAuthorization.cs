// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthorizationHelper
{
    using System;
    using System.Data.SqlClient;
    using Zentity.Security.Authentication;
    using Zentity.Security.Authorization;

    /// <summary>
    /// This class provides t-sql authorization condition.
    /// </summary>
    public static class TsqlAuthorization
    {
        /// <summary>
        /// Authorization condition function.
        /// </summary>
        private const string AuthorizationConditionFunction = "select Core.GetAuthorizationCondition(@IdentityName)";

        /// <summary>
        /// This method retrieves the authorization criteria for the identity represented by the token.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the user</param>
        /// <param name="connection">SqlConnection to the Core database</param>
        /// <returns>authorization criteria</returns>
        public static string GetAuthorizationCriteria(AuthenticatedToken token, SqlConnection connection)
        {
            return ExecuteGetAuthorization(token, connection);
        }

        /// <summary>
        /// This method retrieves the authorization criteria for the identity represented by the token.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the user</param>
        /// <param name="connectionString">Sql Connection string to connect to the core database</param>
        /// <returns>authorization criteria</returns>
        public static string GetAuthorizationCriteria(AuthenticatedToken token, string connectionString)
        {
            #region Parameter Validation
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            #endregion
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                return ExecuteGetAuthorization(token, conn);
            }
            catch (SqlException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }
   
        /// <summary>
        /// This method executes the scalar valued function for getting authorization criteria.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="conn">The conn.</param>
        /// <returns>Authorization criteria</returns>
        private static string ExecuteGetAuthorization(AuthenticatedToken token, SqlConnection conn)
        {
            #region Parameter Validation
            ValidateToken(token);
            if (conn == null)
            {
                throw new ArgumentNullException("conn");
            }

            #endregion
            try
            {
                string criteria = null;
                using (SqlCommand cmd = new SqlCommand(AuthorizationConditionFunction, conn))
                {
                    cmd.CommandType = System.Data.CommandType.Text;

                    SqlParameter identityName = new SqlParameter();
                    identityName.DbType = System.Data.DbType.String;
                    identityName.ParameterName = "IdentityName";
                    identityName.Value = token.IdentityName;
                    cmd.Parameters.Add(identityName);

                    conn.Open();
                    object criteriaObj = cmd.ExecuteScalar();
                    conn.Close();

                    if (criteriaObj != null && criteriaObj != DBNull.Value)
                    {
                        criteria = criteriaObj.ToString();
                    }
                }

                return criteria;
            }
            catch (SqlException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="token">The token.</param>
        private static void ValidateToken(AuthenticatedToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            if (!token.Validate())
            {
                throw new AuthorizationException(ConstantStrings.TokenNotValidException);
            }
        }
    }
}
