using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Security.Principal;
using System.Configuration;
using Zentity.Security.Authentication;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Data.SqlTypes;

namespace Zentity.Security.AuthenticationProvider
{
    /// <summary>
    /// This class contains helper methods to perform tasks on authentication database
    /// </summary>
    internal static class DatabaseHelper
    {
        private static string _connectionString = ConfigurationManager.ConnectionStrings["AuthenticationConnection"]
            .ConnectionString;
        private const string _executeFunctionWithOneParameter = "SELECT dbo.{0}('{1}')";
        private const string _executeFunction = "SELECT dbo.{0}(";

        /// <summary>
        /// Executes a stored procedure and returns number of rows affected
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure to be executed</param>
        /// <param name="parameters">Parameters required to be passed to stored procedure in 
        /// the form of key value pairs</param>
        /// <returns>Number of rows affected</returns>
        internal static int ExecuteNonQuery(string storedProcedureName, Dictionary<string, string> parameters)
        {
            Console.WriteLine("ExecuteNonQuery called for executing " + storedProcedureName);
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SetCommandParameters(cmd, parameters);
                        conn.Open();
                        int numRows = cmd.ExecuteNonQuery();
                        conn.Close();
                        return numRows;
                    }
                }
            }
            catch (SqlNullValueException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Executes a function and returns the result
        /// </summary>
        /// <param name="functionName">Name of the scalar valued function</param>
        /// <param name="param">Parameter to be passed to the function</param>
        /// <returns>Value returned by the function</returns>
        internal static object ExecuteScalar(string functionName, KeyValuePair<string, string> param)
        {
            Console.WriteLine("ExecuteScalar called for executing " + functionName);
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string cmdText = string.Format(CultureInfo.InvariantCulture, 
                        _executeFunctionWithOneParameter, functionName, param.Value);
                    using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        //Add parameter
                        SqlParameter p = new SqlParameter(param.Key, param.Value);
                        p.DbType = DbType.String;
                        p.Direction = ParameterDirection.Input;
                        cmd.Parameters.Add(p);

                        //Execute stored procedure
                        conn.Open();
                        object returnValue = cmd.ExecuteScalar();
                        conn.Close();
                        return returnValue;
                    }
                }
            }
            catch (SqlNullValueException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Executes a stored procedure and returns number of rows affected
        /// To be used for executing stored procedures which perform DML operations
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure to be executed</param>
        /// <param name="param">Parameter to be passed to the stored procedure</param>
        /// <returns>Number of rows affected</returns>
        internal static int ExecuteNonQuery(string storedProcedureName, 
            KeyValuePair<string, string> param)
        {
            Console.WriteLine("ExecuteNonQuery called for executing " + storedProcedureName);
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //Add parameter
                        SqlParameter p = new SqlParameter(param.Key, param.Value);
                        p.Direction = ParameterDirection.Input;
                        p.DbType = DbType.String;
                        cmd.Parameters.Add(p);

                        //Execute command
                        conn.Open();
                        int numRows = cmd.ExecuteNonQuery();
                        conn.Close();
                        return numRows;
                    }
                }
            }
            catch (SqlNullValueException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Executes a function and returns the result
        /// </summary>
        /// <param name="functionName">Name of the scalar valued function</param>
        /// <param name="parameters">Parameters to be passed to the function</param>
        /// <returns>Return value of the function</returns>
        internal static object ExecuteScalar(string functionName, Dictionary<string, string> parameters)
        {
            Console.WriteLine("ExecuteScalar called for executing " + functionName);
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string cmdText = BuildCommandText(functionName, parameters);
                    using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        SetCommandParameters(cmd, parameters);

                        //Execute stored procedure
                        conn.Open();
                        object returnValue = cmd.ExecuteScalar();
                        conn.Close();
                        return returnValue;
                    }
                }
            }
            catch (SqlNullValueException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        private static string BuildCommandText(string functionName, Dictionary<string, string> parameters)
        {
            #region Parameter validation

            if (string.IsNullOrEmpty(functionName))
            {
                throw new ArgumentNullException("functionName");
            }
            
            #endregion
            StringBuilder cmdTextBuilder = new StringBuilder(string.Format(CultureInfo.InvariantCulture,
                _executeFunction, functionName));
            if (parameters == null)
            {
                cmdTextBuilder.Append(")");
            }
            else
            {
                foreach (string paramKey in parameters.Keys)
                {
                    cmdTextBuilder.Append("@" + paramKey + ",");
                }
                //Remove the last comma
                cmdTextBuilder = cmdTextBuilder.Remove(cmdTextBuilder.Length - 1, 1);
                cmdTextBuilder.Append(")");
            }
            return cmdTextBuilder.ToString();
        }

        //Adds each key value pair in the parameters dictionary to the command's parameter collection
        private static void SetCommandParameters(SqlCommand cmd, Dictionary<string, string> parameters)
        {
            #region Parameter validation
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            #endregion
            foreach (KeyValuePair<string, string> paramInfo in parameters)
            {
                if (string.IsNullOrEmpty(paramInfo.Key))
                {
                    throw new ArgumentNullException("parameters", ConstantStrings.ParameterKeyNullExceptionMessage);
                }
                SqlParameter param = new SqlParameter(paramInfo.Key, paramInfo.Value);
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(param);
            }
        }

        private const string _executeGetUserInfoFunction = "SELECT * FROM dbo.GetUserInfo(@LogOnName)";
        
        /// <summary>
        /// Retrieves user information from database and returns ZentityUser object
        /// filled in with property values
        /// </summary>
        /// <param name="logOnName">LogOn name of the user</param>
        /// <returns>ZentityUser object filled with property values stored in the database</returns>
        internal static ZentityUser GetUser(string logOnName)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(logOnName))
            {
                throw new ArgumentNullException("logOnName");
            }
            #endregion
            ZentityUser user = new ZentityUser();
            try
            {
                //Execute stored procedure GetUserInfo
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>(1);
                    parameters.Add("LogOnName", logOnName);
                    using (SqlCommand cmd = new SqlCommand(_executeGetUserInfoFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        SetCommandParameters(cmd, parameters);

                        //Execute stored procedure
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            SetUserProperties(user, reader, false);
                        }
                        conn.Close();
                    }
                }
                return user;
            }
            catch (SqlNullValueException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        private static void SetUserProperties(ZentityUser user, SqlDataReader reader, bool readLogOnName)
        {
            #region Parameter validation
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.IsClosed)
            {
                throw new ArgumentException(ConstantStrings.ReaderClosedExceptionMessage);
            }
            #endregion

            user.SetUserId(reader.GetGuid(0));
            user.FirstName = reader.GetString(1);
            //For nullable columns GetString() throws exception in case the record does not have the value filled
            object value = reader.GetValue(2);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                user.MiddleName = value.ToString();
            }
            value = reader.GetValue(3);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                user.LastName = value.ToString();
            }
            user.Email = reader.GetString(4);
            value = reader.GetValue(5);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                user.City = value.ToString();
            }
            value = reader.GetValue(6);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                user.State = value.ToString();
            }
            value = reader.GetValue(7);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                user.Country = value.ToString();
            }
            int i = 8;
            if (readLogOnName)
            {
                user.LogOnName = reader.GetString(8);
                i = 9;
            }
            value = reader.GetValue(i++);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                user.SetAccountStatus(value.ToString());
            }
            value = reader.GetValue(i++);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                user.SetDateCreated(Convert.ToDateTime(value));
            }
            value = reader.GetValue(i++);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                user.SetDateModified(Convert.ToDateTime(value));
            }
            value = reader.GetValue(i++);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                user.SetPasswordCreationDate(Convert.ToDateTime(value));
            }
        }

        private const string _executeGetPagedUserRecordsFunction =
            "SELECT * FROM dbo.GetPagedUserRecords(@StartIndex, @EndIndex)";
        /// <summary>
        /// Retrieves user information from database and returns collection of 
        /// Zentity user objects
        /// </summary>
        /// <param name="startIndex">Set this to proper value for paged retrieval of users. </param>
        /// <param name="endIndex">Set this to proper value for paged retrieval of users. </param>
        /// <returns>Collection of users</returns>
        internal static IEnumerable<ZentityUser> GetUsers(int startIndex, int endIndex)
        {
            #region Parameter validation
            if (startIndex <= 0 || endIndex == 0)
            {
                throw new ArgumentException(ConstantStrings.IndexValuesExceptionMessage);
            }
            if (startIndex > endIndex && endIndex != -1)
            {
                throw new ArgumentException(ConstantStrings.IndexValuesExceptionMessage);
            }
            #endregion

            //Call SP GetPagedUserRecords
            try
            {
                Collection<ZentityUser> pagedUsers = new Collection<ZentityUser>();
                //Execute stored procedure GetUserInfo
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    Dictionary<string, string> parameters = new Dictionary<string,string>(2);
                    parameters.Add("StartIndex", startIndex.ToString());
                    parameters.Add("EndIndex", endIndex.ToString());
                    using (SqlCommand cmd = new SqlCommand(_executeGetPagedUserRecordsFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        SetCommandParameters(cmd, parameters);

                        //Execute stored procedure
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                        {
                            ZentityUser user = new ZentityUser();
                            //Read the records one by one and create a Zentity user instance
                            //Fill in list of users
                            SetUserProperties(user, reader, true);
                            pagedUsers.Add(user);
                        }
                        conn.Close();
                    }
                }
                return pagedUsers.AsEnumerable();
            }
            catch (SqlNullValueException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        private const string _authenticateUserStoredProcedure = "dbo.AuthenticateUser";
        /// <summary>
        /// Calls AuthenticateUser procedure
        /// </summary>
        /// <param name="logOnName">Log on name of the user</param>
        /// <param name="passwordHash">Password</param>
        /// <param name="passwordExpiryInDays">Password expiry policy in number of days</param>
        /// <param name="userId">User id is returned if user is authenticated</param>
        internal static void AuthenticateUser(string logOnName, string passwordHash, int passwordExpiryInDays, out Guid userId)
        {
            Console.WriteLine("AuthenticateUser called");
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string cmdText = _authenticateUserStoredProcedure;
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    #region Set command parameters
                    SqlParameter logOnParam = new SqlParameter("LogOnName", logOnName);
                    logOnParam.DbType = DbType.String;
                    logOnParam.Direction = ParameterDirection.Input;
                    logOnParam.Value = logOnName;
                    cmd.Parameters.Add(logOnParam);

                    SqlParameter passwordParam = new SqlParameter();
                    passwordParam.Direction = ParameterDirection.Input;
                    passwordParam.DbType = DbType.String;
                    passwordParam.ParameterName = "Password";
                    passwordParam.Value = passwordHash;
                    cmd.Parameters.Add(passwordParam);

                    SqlParameter passwordExpiryParam = new SqlParameter();
                    passwordExpiryParam.ParameterName = "PasswordExpiresInDays";
                    passwordExpiryParam.DbType = DbType.Int32;
                    passwordExpiryParam.Value = passwordExpiryInDays;
                    passwordExpiryParam.Direction = ParameterDirection.Input;
                    cmd.Parameters.Add(passwordExpiryParam);

                    SqlParameter userIdParam = new SqlParameter();
                    userIdParam.Direction = ParameterDirection.Output;
                    userIdParam.ParameterName = "UserId";
                    userIdParam.DbType = DbType.Guid;
                    cmd.Parameters.Add(userIdParam);
                    #endregion

                    conn.Open();
                    int numRows = cmd.ExecuteNonQuery();
                    if (cmd.Parameters["UserId"].Value != null
                        && !string.IsNullOrEmpty(cmd.Parameters["UserId"].Value.ToString()))
                    {
                        userId = (Guid)cmd.Parameters["UserId"].Value;
                    }
                    else
                    {
                        userId = Guid.Empty;
                    }
                    conn.Close();
                }
            }
        }
    }
}
