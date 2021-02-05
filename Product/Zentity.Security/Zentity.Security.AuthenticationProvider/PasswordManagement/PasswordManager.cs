// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider.PasswordManagement
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// This class contains utility methods for manipulating passwords
    /// </summary>
    internal static class PasswordManager
    {
        #region Private variables

        private static bool applyPasswordPolicy = Convert.ToBoolean(
                                                            ConfigurationManager.AppSettings["ApplyPasswordPolicy"], 
                                                            CultureInfo.InvariantCulture);
        private static bool encryptPassword = Convert.ToBoolean(
                                                            ConfigurationManager.AppSettings["EncryptPassword"], 
                                                            CultureInfo.InvariantCulture);

        #endregion

        /// <summary>
        /// Verifies whether the password for the given user matches the one stored in 
        /// the authentication store
        /// </summary>
        /// <param name="logOnName">Login name of the user</param>
        /// <param name="password">Password entered by the user</param>
        /// <returns>True if the password is correct</returns>
        internal static bool VerifyPassword(string logOnName, string password)
        {
            #region Input validation
            ValidateParameters("logOnName", logOnName, "password", password);
            #endregion

            //// Verify this password against the one stored in the database
            bool authenticated = DataAccessLayer.AuthenticateUser(logOnName, password, PasswordPolicyProvider.PasswordExpiryInDays);
            return authenticated;
        }

        /// <summary>
        /// Generates a new password for the user if the security question and answer
        /// sent matches the one stored in the authentication store
        /// </summary>
        /// <param name="logOnName">Login name of the user</param>
        /// <param name="securityQuestion">Security question selected by the user</param>
        /// <param name="answer">User's answer to the security question in hashed format</param>
        /// <returns>New password</returns>
        internal static string ForgotPassword(string logOnName, string securityQuestion, string answer)
        {
            #region Input validation
            ValidateParameters("logOnName", logOnName, "securityQuestion", securityQuestion, "answer", answer);
            #endregion

            //// Generate a new password 
            string newPassword = PasswordPolicyProvider.GenerateNewPassword();
            //// Generate encrypted password
            string newSecurePassword = GetSecurePassword(newPassword);
            bool success = DataAccessLayer.ForgotPassword(logOnName, securityQuestion, answer, newSecurePassword);
            if (success)
            {
                return newPassword;
            }

            return string.Empty;
        }

        /// <summary>
        /// Changes user password provided the current password submitted by the user 
        /// matches the one stored in the authentication store
        /// </summary>
        /// <param name="logOnName">Login name of the user</param>
        /// <param name="currentPassword">Current password of the user. This should be a secure (encrypted) password.</param>
        /// <param name="newPassword">New password chosen by the user</param>
        /// <returns>True if password change was successful</returns>
        internal static bool ChangePassword(string logOnName, string currentPassword, string newPassword)
        {
            #region Input validation
            ValidateParameters("logOnName", logOnName, "currentPassword", currentPassword, "newPassword", newPassword);
            #endregion

            string newSecurePassword = GetSecurePassword(newPassword);
            bool success = DataAccessLayer.ChangePassword(logOnName, currentPassword, newSecurePassword);
            return success;
        }

        /// <summary>
        /// Returns the number of days remaining for password expiry for the given user
        /// </summary>
        /// <param name="logOnName">Log on name of the user</param>
        /// <returns>Number of days in which the password will expire</returns>
        internal static int? PasswordExpiresInDays(string logOnName)
        {
            #region Input validation
            ValidateParameters("logOnName", logOnName);
            #endregion

            if (!applyPasswordPolicy)
            {
                return -1;
            }

            //// Get password creation date from database
            DateTime? passwordCreationDate = DataAccessLayer.GetPasswordCreationDate(logOnName);

            if (passwordCreationDate == null)
            {
                return null;
            }

            //// Get the number of days for password expiry as set in the password policy
            int passwordExpiryInDays = PasswordPolicyProvider.PasswordExpiryInDays;

            //// The number of days remaining for password expiry are password expiry date minus current date
            DateTime passwordExpiryDate = passwordCreationDate.Value.AddDays(passwordExpiryInDays);
            int days = (int)((passwordExpiryDate - DateTime.Now).TotalDays);
            return days;
        }

        /// <summary>
        /// Encrypts the password depending on config setting.
        /// </summary>
        /// <param name="password">clear text password</param>
        /// <returns>Encrypted/hashed/clear text password depending on config setting.</returns>
        internal static string GetSecurePassword(string password)
        {
            #region Parameter Validation
            ValidateParameters("password", password);
            #endregion

            string securePassword;

            if (encryptPassword)
            {
                securePassword = EncryptPassword(password);
            }
            else
            {
                securePassword = password;
            }

            return securePassword;
        }

        /// <summary>
        /// Decrypts the password, if it is encrypted (depending on the config setting).
        /// </summary>
        /// <param name="password">Password to be decrypted.</param>
        /// <returns>Clear text password.</returns>
        internal static string GetPlainPassword(string password)
        {
            #region Parameter Validation
            ValidateParameters("password", password);
            #endregion

            string insecurePassword;

            if (encryptPassword)
            {
                insecurePassword = DecryptPassword(password);
            }
            else
            {
                insecurePassword = password;
            }

            return insecurePassword;
        }

        /// <summary>
        /// Encrypts the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>Encrypted password.</returns>
        private static string EncryptPassword(string password)
        {
            string key64bit = ConfigurationManager.AppSettings["encryptionKey"];

            if (String.IsNullOrEmpty(password))
            {
                return String.Empty;
            }

            if (String.IsNullOrEmpty(key64bit))
            {
                return password;
            }

            RijndaelManaged encryptionProvider = new RijndaelManaged();

            byte[] passwordInBytes = System.Text.Encoding.Unicode.GetBytes(password);
            byte[] saltValue = Encoding.ASCII.GetBytes(key64bit.Length.ToString(CultureInfo.InvariantCulture));

            PasswordDeriveBytes finalKey =
                new PasswordDeriveBytes(key64bit, saltValue);

            ICryptoTransform cryptoTransform =
                encryptionProvider.CreateEncryptor(finalKey.GetBytes(32), finalKey.GetBytes(16));
            using (MemoryStream stream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(passwordInBytes, 0, passwordInBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    byte[] encryptedPasswordInBytes = stream.ToArray();

                    return Convert.ToBase64String(encryptedPasswordInBytes);
                }
            }
        }

        /// <summary>
        /// Decrypts the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>Decrypted password.</returns>
        private static string DecryptPassword(string password)
        {
            string key64bit = ConfigurationManager.AppSettings["encryptionKey"];

            if (String.IsNullOrEmpty(password))
            {
                return String.Empty;
            }

            if (String.IsNullOrEmpty(key64bit))
            {
                return password;
            }

            RijndaelManaged decryptionProvider = new RijndaelManaged();

            byte[] encryptedPasswordInBytes = Convert.FromBase64String(password);
            byte[] saltValue = Encoding.ASCII.GetBytes(key64bit.Length.ToString(CultureInfo.InvariantCulture));

            PasswordDeriveBytes finalKey =
                new PasswordDeriveBytes(key64bit, saltValue);

            ICryptoTransform cryptoTransform =
                decryptionProvider.CreateDecryptor(
                    finalKey.GetBytes(32), finalKey.GetBytes(16));

            using (MemoryStream stream = new MemoryStream(encryptedPasswordInBytes))
            {
                using (CryptoStream cryptoStream =
                               new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Read))
                {
                    byte[] passwordInBytes = new byte[encryptedPasswordInBytes.Length];

                    int readBytesCount =
                        cryptoStream.Read(passwordInBytes, 0, passwordInBytes.Length);
                    return Encoding.Unicode.GetString(passwordInBytes, 0, readBytesCount);
                }
            }
        }

        /// <summary>
        /// Checks whether the string parameters sent are null/empty and throws ArgumentNullException if so.
        /// Call this method with "paramName", "paramValue" pairs for each argument to be validated.
        /// </summary>
        /// <param name="args">Arguments; "paramName", "paramValue" pairs for each argument to be validated</param>
        private static void ValidateParameters(params string[] args)
        {
            if (args != null)
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
        }
    }
}
