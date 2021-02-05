// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Linq;
    using Zentity.Security.Authentication;

    /// <summary>
    /// This class contains methods for accessing the authentication database
    /// </summary>
    internal static class DataAccessLayer
    {
        #region Constant strings

        private const string AuthenticateUserFunction = "SELECT dbo.AuthenticateUser(@LogOnName, @Password, @PasswordExpiresInDays)";
        private const string GetPasswordCreationDateFunction = "SELECT dbo.GetPasswordCreationDate(@LogOnName)";
        private const string ExecuteGetPagedUserRecordsFunction =
            "SELECT * FROM dbo.GetPagedUserRecords(@StartIndex,@EndIndex)";
        private const string ExecuteGetUserInfoFunction = "SELECT * FROM dbo.GetUserInfo(@LogOnName)";
        private const string IsAdminFunction = "SELECT dbo.IsAdmin(@LogOnName)";
        private const string GetAccountStatusFunction = "SELECT dbo.GetAccountStatus(@LogOnName)";
        private const string GetUserIdFunction = "SELECT dbo.GetUserId(@LogOnName)";
        private const string IsLogOnNameAvailableFunction = "SELECT dbo.IsLogOnNameAvailable(@name)";
        private const string GetPasswordFunction = "SELECT dbo.GetPassword(@logOnName)";
        private const string GetAccountStatusValuesFunction = "SELECT * FROM dbo.GetAccountStatusValues()";

        #endregion

        private static string connectionString = ConfigurationManager.ConnectionStrings["AuthenticationConnection"]
        .ConnectionString;

        #region Internal methods

        /// <summary>
        /// Authenticates a user by verifying his credentials from the authentication store. 
        /// </summary>
        /// <param name="logOnName">LogOnName of the user</param>
        /// <param name="password">secure password</param>
        /// <param name="passwordExpiresInDays">Number of days in which a password expires as per password policy</param>
        /// <returns>True if user is authenticated</returns>
        internal static bool AuthenticateUser(string logOnName, string password, int passwordExpiresInDays)
        {
            //// Parameter validation
            ValidateParameters("logOnName", logOnName, "password", password);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(AuthenticateUserFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        SetCommandParameter(cmd, "LogOnName", logOnName);
                        SetCommandParameter(cmd, "Password", password);

                        //// add int parameter
                        SetCommandParameter(passwordExpiresInDays, cmd);

                        conn.Open();
                        object returnValue = cmd.ExecuteScalar();
                        if (returnValue != null && !string.IsNullOrEmpty(returnValue.ToString()))
                        {
                            return (bool)returnValue;
                        }

                        return false;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Verifies the current password, and if correct, changes the user password to the new one.
        /// </summary>
        /// <param name="logOnName">LogOnName of the user</param>
        /// <param name="currentPassword">Current password in hashed format</param>
        /// <param name="newPassword">New password in hashed format</param>
        /// <returns>True if the password change is successful</returns>
        internal static bool ChangePassword(string logOnName, string currentPassword, string newPassword)
        {
            //// Parameter Validation
            ValidateParameters("logOnName", logOnName, "currentPassword", currentPassword, "newPassword", newPassword);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("ChangePassword", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SetCommandParameter(cmd, "LogOnName", logOnName);
                        SetCommandParameter(cmd, "CurrentPassword", currentPassword);
                        SetCommandParameter(cmd, "NewPassword", newPassword);

                        //// Execute and check number of rows affected
                        return ExecuteNonQuery(conn, cmd);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Verifies the security question and answer provided and updates the new password to the database
        /// </summary>
        /// <param name="logOnName">LogOnName of the user</param>
        /// <param name="securityQuestion">Security question supplied by the user as a means of identification, 
        /// since he has forgotten the password</param>
        /// <param name="answer">Answer supplied by the user in hashed format</param>
        /// <param name="newPassword">New password to be set for the user account if the security question and 
        /// answer are verified to be correct</param>
        /// <returns>System.Boolean; <c>true</c> if successful, <c>false</c> otherwise.</returns>
        internal static bool ForgotPassword(string logOnName, string securityQuestion, string answer, string newPassword)
        {
            //// Parameter Validation
            ValidateParameters("logOnName", logOnName, "securityQuestion", securityQuestion, "answer", answer, "newPassword", newPassword);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("ForgotPassword", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SetCommandParameter(cmd, "LogOnName", logOnName);
                        SetCommandParameter(cmd, "SecurityQuestion", securityQuestion.ToUpper(CultureInfo.CurrentCulture));
                        SetCommandParameter(cmd, "Answer", answer);
                        SetCommandParameter(cmd, "NewPassword", newPassword);

                        return ExecuteNonQuery(conn, cmd);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Registers new user account to the database.
        /// </summary>
        /// <param name="newUser">Zentity user object with all required properties set to proper values</param>
        /// <returns>True if user registration is successful</returns>
        internal static bool RegisterUser(ZentityUser newUser)
        {
            //// Parameter Validation
            if (newUser == null)
            {
                throw new ArgumentNullException("newUser");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("RegisterUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //// Set mandatory properties
                        SetCommandParameter(cmd, "FirstName", newUser.Profile.FirstName);
                        SetCommandParameter(cmd, "Email", newUser.Profile.Email);
                        SetCommandParameter(cmd, "LogOnName", newUser.Profile.LogOnName);
                        SetCommandParameter(cmd, "Password", newUser.Password);
                        SetCommandParameter(cmd, "AccountStatus", newUser.Profile.AccountStatus);
                        SetCommandParameter(cmd, "SecurityQuestion", newUser.Profile.SecurityQuestion);
                        SetCommandParameter(cmd, "Answer", newUser.Profile.Answer);

                        //// Set optional properties
                        SetCommandParameter(cmd, "MiddleName", newUser.Profile.MiddleName);
                        SetCommandParameter(cmd, "LastName", newUser.Profile.LastName);
                        SetCommandParameter(cmd, "City", newUser.Profile.City);
                        SetCommandParameter(cmd, "State", newUser.Profile.State);
                        SetCommandParameter(cmd, "Country", newUser.Profile.Country);

                        return ExecuteNonQuery(conn, cmd);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Updates a user profile to database.
        /// </summary>
        /// <param name="user">ZentityUser object with property values to be modified set to new values</param>
        /// <returns>True if profile is updated in the database</returns>
        internal static bool UpdateProfile(ZentityUser user)
        {
            //// Parameter Validation
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateProfile", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //// Set command parameters for changed properties
                        SetCommandParameter(cmd, "FirstName", user.Profile.FirstName);
                        SetCommandParameter(cmd, "MiddleName", user.Profile.MiddleName);
                        SetCommandParameter(cmd, "LastName", user.Profile.LastName);
                        SetCommandParameter(cmd, "Email", user.Profile.Email);
                        SetCommandParameter(cmd, "City", user.Profile.City);
                        SetCommandParameter(cmd, "State", user.Profile.State);
                        SetCommandParameter(cmd, "Country", user.Profile.Country);
                        SetCommandParameter(cmd, "SecurityQuestion", user.Profile.SecurityQuestion);
                        SetCommandParameter(cmd, "Answer", user.Profile.Answer);

                        //// Add logon name
                        SetCommandParameter(cmd, "LogOnName", user.LogOnName);

                        return ExecuteNonQuery(conn, cmd);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Returns the user account status. Only an admin user can view a user's account status.
        /// </summary>
        /// <param name="logOnName">LogOnName of the user whose account status was requested</param>
        /// <returns>User account status</returns>
        internal static string GetAccountStatus(string logOnName)
        {
            //// Parameter Validation
            ValidateParameters("logOnName", logOnName);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(GetAccountStatusFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;

                        //// Set parameters
                        SetCommandParameter(cmd, "LogOnName", logOnName);

                        conn.Open();
                        object returnValue = cmd.ExecuteScalar();
                        if (returnValue != null && !string.IsNullOrEmpty(returnValue.ToString()))
                        {
                            return returnValue.ToString();
                        }

                        return string.Empty;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Deletes the user from the system.
        /// </summary>
        /// <param name="logOnName">LogOnName of the user who is to be unregistered.</param>
        /// <returns>True if operation succeeds, false otherwise</returns>
        internal static bool UnregisterUser(string logOnName)
        {
            return DeleteUser(logOnName);
        }

        /// <summary>
        /// Updates the log on name. The operation may fail if the new logon is already in use for some other user.
        /// </summary>
        /// <param name="currentLogOn">Current log on</param>
        /// <param name="newLogOn">New log on</param>
        /// <returns>True if operation succeeds, false otherwise</returns>
        internal static bool UpdateLogOnName(string currentLogOn, string newLogOn)
        {
            //// Parameter Validation
            ValidateParameters("currentLogOn", currentLogOn, "newLogOn", newLogOn);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateLogOnName", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //// Set parameters
                        SetCommandParameter(cmd, "CurrentLogOnName", currentLogOn);
                        SetCommandParameter(cmd, "NewLogOnName", newLogOn);

                        return ExecuteNonQuery(conn, cmd);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Returns user id
        /// </summary>
        /// <param name="logOnName">Logon name of the user</param>
        /// <returns>Guid for the user represented by logOnName</returns>
        internal static Guid GetUserId(string logOnName)
        {
            //// Parameter Validation
            ValidateParameters("logOnName", logOnName);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(GetUserIdFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;

                        SetCommandParameter(cmd, "LogOnName", logOnName);

                        conn.Open();
                        object returnValue = cmd.ExecuteScalar();
                        if (returnValue != null && !string.IsNullOrEmpty(returnValue.ToString()))
                        {
                            Guid id = (Guid)returnValue;
                            return id;
                        }

                        return Guid.Empty;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Returns ZentityUser instance filled with property values from the database.
        /// </summary>
        /// <param name="logOnName">Logon name of the user.</param>
        /// <returns>ZentityUserProfile instance filled with property values stored in the database.</returns>
        internal static ZentityUserProfile GetUserProfile(string logOnName)
        {
            ZentityUserProfile profile = null;
            #region Parameter validation
            ValidateParameters("logOnName", logOnName);
            #endregion

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(ExecuteGetUserInfoFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        SetCommandParameter(cmd, "LogOnName", logOnName);

                        //// Execute stored procedure
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            profile = SetUserProperties(reader);
                        }
                    }
                }
                return profile;
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
        /// Returns the password creation date for the given user.
        /// </summary>
        /// <param name="logOnName">LogOn name of the user.</param>
        /// <returns>Password creation date if logon name exists, null otherwise.</returns>
        internal static DateTime? GetPasswordCreationDate(string logOnName)
        {
            //// Parameter Validation
            ValidateParameters("logOnName", logOnName);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(GetPasswordCreationDateFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;

                        SetCommandParameter(cmd, "LogOnName", logOnName);

                        conn.Open();
                        object returnValue = cmd.ExecuteScalar();
                        if (returnValue != null && !string.IsNullOrEmpty(returnValue.ToString()))
                        {
                            DateTime? passwordCreationDate = returnValue as DateTime?;
                            return passwordCreationDate;
                        }

                        return null;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Checks whether the given user is an administrator.
        /// </summary>
        /// <param name="logOnName">LogOnName of the user.</param>
        /// <returns>True if the logon name represents an admin user, false otherwise.</returns>
        internal static bool IsAdmin(string logOnName)
        {
            //// Parameter validation
            ValidateParameters("logOnName", logOnName);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(IsAdminFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        SetCommandParameter(cmd, "LogOnName", logOnName);

                        conn.Open();
                        object returnValue = cmd.ExecuteScalar();
                        if (returnValue != null && !string.IsNullOrEmpty(returnValue.ToString()))
                        {
                            bool isAdmin = (bool)returnValue;
                            return isAdmin;
                        }
                        
                        return false;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Deletes a user record from the database.
        /// </summary>
        /// <param name="logOnName">LogOnName of the user.</param>
        /// <returns>True if operation succeeds, false otherwise.</returns>
        internal static bool DeleteUser(string logOnName)
        {
            //// Parameter Validation
            ValidateParameters("logOnName", logOnName);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("DeleteUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SetCommandParameter(cmd, "LogOnName", logOnName);

                        return ExecuteNonQuery(conn, cmd);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Returns user profiles for the given range, sorted by LogOnName.
        /// </summary>
        /// <param name="startIndex">Start index for paged retrieval of users. First record has index 1. </param>
        /// <param name="endIndex">End index for paged retrieval of users. End index is inclusive - meaning startIndex = 1 and 
        /// endIndex = 1 will return record at position 1. </param>
        /// <returns>List of ZentityUserProfile instances filled with property values from database records.</returns>
        internal static IEnumerable<ZentityUserProfile> GetUserProfiles(int startIndex, int endIndex)
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

            //// Call SP GetPagedUserRecords
            try
            {
                Collection<ZentityUserProfile> pagedUsers = new Collection<ZentityUserProfile>();
                //// Execute stored procedure GetUserInfo
                using (SqlConnection conn = new SqlConnection(connectionString)) {
                    using (SqlCommand cmd = new SqlCommand(ExecuteGetPagedUserRecordsFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        SetCommandParameter(cmd, startIndex, endIndex);

                        //// Execute stored procedure
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            //// Read the records one by one and create a Zentity user instance
                            //// Fill in list of users
                            ZentityUserProfile profile = SetUserProperties(reader);
                            pagedUsers.Add(profile);
                        }
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

        /// <summary>
        /// Marks or unmarks the user as an admin user.
        /// </summary>
        /// <param name="logOnName">LogOnName of the user.</param>
        /// <param name="isAdmin">Flag indicating whether the user is to be marked as admin. If set to true the user is marked as an admin user.</param>
        /// <returns>True if operation succeeds, false otherwise.</returns>
        internal static bool SetAdmin(string logOnName, bool isAdmin)
        {
            //// Parameter validation
            ValidateParameters("logOnName", logOnName);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SetAdminUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SetCommandParameter(cmd, "LogOnName", logOnName);
                        SetCommandParameter(cmd, isAdmin);
                        return ExecuteNonQuery(conn, cmd);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Checks whether the given logon name is already in use.
        /// </summary>
        /// <param name="name">The logon name chosen by a new user.</param>
        /// <returns>True if logon name is not already in use, false otherwise.</returns>
        internal static bool IsLogOnNameAvailable(string name)
        {
            //// Parameter validation
            ValidateParameters("name", name);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) {
                    using (SqlCommand cmd = new SqlCommand(IsLogOnNameAvailableFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;

                        SetCommandParameter(cmd, "Name", name);
                        conn.Open();

                        object returnValue = cmd.ExecuteScalar();
                        if (returnValue != null)
                        {
                            bool isAvailable = (bool)returnValue;
                            return isAvailable;
                        }
                    }
                }

                return false;
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Updates account status value.
        /// </summary>
        /// <param name="logOnName">Log on name</param>
        /// <param name="newStatus">New status</param>
        /// <returns>True if operation succeeds, false otherwise.</returns>
        internal static bool UpdateAccountStatus(string logOnName, string newStatus)
        {
            //// Parameter Validation
            ValidateParameters("logOnName", logOnName, "newStatus", newStatus);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateAccountStatus", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //// Set parameters
                        SetCommandParameter(cmd, "LogOnName", logOnName);
                        SetCommandParameter(cmd, "AccountStatus", newStatus);

                        return ExecuteNonQuery(conn, cmd);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Returns password for the user
        /// </summary>
        /// <param name="logOnName">Log On Name</param>
        /// <returns>Password in clear text</returns>
        internal static string GetPassword(string logOnName)
        {
            //// Parameter Validation
            ValidateParameters("logOnName", logOnName);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(GetPasswordFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;

                        //// Set parameters
                        SetCommandParameter(cmd, "LogOnName", logOnName);
                        conn.Open();
                        string password = cmd.ExecuteScalar() as string;
                        return password;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Returns a list of available account status values.
        /// </summary>
        /// <returns>List of account status values available in the database.</returns>
        internal static IEnumerable<string> GetAccountStatusValues()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(GetAccountStatusValuesFunction, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        List<string> statusValues = new List<string>();
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            string status = reader[0] as String;
                            statusValues.Add(status);
                        }

                        return statusValues;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AuthenticationException(ConstantStrings.DatabaseExceptionMessage, ex);
            }
        }

        #endregion

        #region Private methods
        //// 
        /// <summary>
        /// Adds IsAdmin boolean parameter to the SqlCommand.
        /// </summary>
        /// <param name="cmd">The sql command.</param>
        /// <param name="setAdmin">if set to <c>true</c> then set as admin.</param>
        private static void SetCommandParameter(SqlCommand cmd, bool setAdmin)
        {
            //// Validation
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }

            SqlParameter isAdmin = new SqlParameter();
            isAdmin.ParameterName = "IsAdmin";
            isAdmin.DbType = DbType.Boolean;
            isAdmin.Value = setAdmin;
            cmd.Parameters.Add(isAdmin);
        }

        /// <summary>
        ///  Adds a string input parameter with the given name and value to the given command.
        /// </summary>
        /// <param name="cmd">The CMD.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterValue">The parameter value.</param>
        private static void SetCommandParameter(SqlCommand cmd, string parameterName, string parameterValue)
        {
            //// Parameter Validation
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }

            ValidateParameters("parameterName", parameterName);

            SqlParameter p = new SqlParameter();
            p.ParameterName = parameterName;
            p.DbType = DbType.String;
            p.Direction = ParameterDirection.Input;
            p.Value = parameterValue;

            cmd.Parameters.Add(p);
        }

        /// <summary>
        /// Adds password expiry parameter to the SqlCommand object
        /// </summary>
        /// <param name="passwordExpiresInDays">The password expires in days.</param>
        /// <param name="cmd">The sql command.</param>
        private static void SetCommandParameter(int passwordExpiresInDays, SqlCommand cmd)
        {
            //// Parameter Validation
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }

            SqlParameter passwordExpiryParam = new SqlParameter();
            passwordExpiryParam.ParameterName = "PasswordExpiresInDays";
            passwordExpiryParam.DbType = DbType.Int32;
            passwordExpiryParam.Direction = ParameterDirection.Input;
            passwordExpiryParam.Value = passwordExpiresInDays;
            cmd.Parameters.Add(passwordExpiryParam);
        }

        /// <summary>
        /// Sets the sql command to the command parameter.
        /// </summary>
        /// <param name="cmd">The sql command.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        private static void SetCommandParameter(SqlCommand cmd, int startIndex, int endIndex)
        {
            //// Parameter Validation
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }

            cmd.Parameters.Add("StartIndex", SqlDbType.Int);
            cmd.Parameters["StartIndex"].Value = startIndex;

            cmd.Parameters.Add("EndIndex", SqlDbType.Int);
            cmd.Parameters["EndIndex"].Value = endIndex;
        }

        /// <summary>
        /// Checks whether the string parameters sent are null/empty and throws ArgumentNullException if so.
        /// Call this method with "paramName", "paramValue" pairs
        /// </summary>
        /// <param name="args">Arguments - <c>paramName</c>, <c>paramValue</c> pairs</param>
        private static void ValidateParameters(params string[] args)
        {
            int numArgs = args.Length;
            for (int i = 1; i < numArgs; i += 2)
            {
                if (string.IsNullOrEmpty(args[i]))
                {
                    throw new ArgumentNullException(args[i - 1]);
                }
            }
        }
              
        /// <summary>
        /// Reads records from the SqlDataReader, and fills in the ZentityUser instance with values read by the reader.
        /// </summary>
        /// <param name="reader">Sql data reader</param>
        /// <returns>Zentity user profile</returns>
        private static ZentityUserProfile SetUserProperties(SqlDataReader reader)
        {
            //// Parameter validation
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (reader.IsClosed)
            {
                throw new ArgumentException(ConstantStrings.ReaderClosedExceptionMessage);
            }

            string firstName = reader["FirstName"] as string;
            string middleName = reader["MiddleName"] as string;
            string lastName = reader["LastName"] as string;
            string logOnName = reader[4] as string;
            string city = reader["City"] as string;
            string state = reader["State"] as string;
            string country = reader["Country"] as string;
            string email = reader["Email"] as string;
            string accStatus = reader["AccountStatus"] as string;
            object dateCreated = reader["DateCreated"];
            object dateModified = reader["DateModified"];
            string ques = reader["SecurityQuestion"] as string;
            string ans = reader["Answer"] as string;
            object passwordCreationDate = reader["PasswordCreationDate"];

            ZentityUserProfile profile = new ZentityUserProfile();
            object id = reader["UserId"];
            if (id != null && !string.IsNullOrEmpty(id.ToString()))
            {
                profile.Id = (Guid)id;
            }

            if (!string.IsNullOrEmpty(logOnName))
            {
                profile.LogOnName = logOnName;
            }

            if (!string.IsNullOrEmpty(firstName))
            {
                profile.FirstName = firstName;
            }

            if (!string.IsNullOrEmpty(middleName))
            {
                profile.MiddleName = middleName;
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                profile.LastName = lastName;
            }

            if (!string.IsNullOrEmpty(email))
            {
                profile.Email = email;
            }

            if (!string.IsNullOrEmpty(city))
            {
                profile.City = city;
            }

            if (!string.IsNullOrEmpty(state))
            {
                profile.State = state;
            }

            if (!string.IsNullOrEmpty(country))
            {
                profile.Country = country;
            }

            if (!string.IsNullOrEmpty(accStatus))
            {
                profile.AccountStatus = accStatus;
            }

            if (!string.IsNullOrEmpty(ques))
            {
                profile.SecurityQuestion = ques;
            }

            if (!string.IsNullOrEmpty(ans))
            {
                profile.SetHashedAnswer(ans);
            }

            if (dateCreated != null && !string.IsNullOrEmpty(dateCreated.ToString()))
            {
                profile.DateCreated = dateCreated as DateTime?;
            }

            if (dateModified != null && !string.IsNullOrEmpty(dateModified.ToString()))
            {
                profile.DateModified = dateModified as DateTime?;
            }

            if (passwordCreationDate != null && !string.IsNullOrEmpty(passwordCreationDate.ToString()))
            {
                profile.PasswordCreationDate = passwordCreationDate as DateTime?;
            }

            return profile;
        }

        /// <summary>
        /// Executes a command and returns true or false based on whether any rows were affected.
        /// </summary>
        /// <param name="conn">Sql connection</param>
        /// <param name="cmd">Sql command</param>
        /// <returns>System.Boolean; <c>true</c> if successful, <c>false</c> otherwise.</returns>
        private static bool ExecuteNonQuery(SqlConnection conn, SqlCommand cmd)
        {
            //// Parameter Validation
            if (conn == null)
            {
                throw new ArgumentNullException("conn");
            }

            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            conn.Close();
            if (rowsAffected <= 0)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
