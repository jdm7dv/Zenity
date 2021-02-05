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
    using System.Linq;
    using System.Text;
    using Zentity.Security.Authentication;

    /// <summary>
    /// This class provides access to the password policy set in the application configuration file
    /// </summary>
    internal static class PasswordPolicyProvider
    {
        #region Private variables

        private static bool applyPasswordPolicy;
        private static int expiresInDays;
        private static int minLength;
        private static int maxLength;
        private static bool mustStartWithAlphabet;
        private static bool mustContainDigit;
        private static bool mustContainSpecialCharacter;
        #endregion

        #region Constructor
        
        /// <summary>
        /// Initializes a new instance of the PasswordPolicyProvider class.
        /// Static constructor. Reads the PasswordPolicy configuration section.
        /// </summary>
        static PasswordPolicyProvider()
        {
            applyPasswordPolicy = Convert.ToBoolean(ConfigurationManager.AppSettings["ApplyPasswordPolicy"], 
                CultureInfo.InvariantCulture);
            if (applyPasswordPolicy)
            {
                ReadPolicy();
            }
            else
            {
                expiresInDays = -1;
                minLength = 7;
            }
        }

        #endregion

        /// <summary>
        /// Gets the current value of password expiry in days as per the current password policy
        /// </summary>
        internal static int PasswordExpiryInDays
        {
            get { return expiresInDays; }
        }

        /// <summary>
        /// Checks whether the password sent as a parameter conforms to the
        /// current password policy
        /// </summary>
        /// <param name="password">Password in secure format (encrypted)</param>
        /// <returns>True if all password policy constraints are satisfied</returns>
        internal static bool CheckPolicyConformance(string password)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }
            #endregion

            if (applyPasswordPolicy)
            {
                string clearTextPassword = PasswordManager.GetPlainPassword(password);
                //// Check the password against each policy
                bool success = ConformsToLengthPolicy(clearTextPassword)
                    && ConformsToDigitPolicy(clearTextPassword)
                    && ConformsToSpecialCharPolicy(clearTextPassword)
                    && ConformsToStartWithAlphabetPolicy(clearTextPassword);
                return success;
            }

            //// if password policy is not to be applied any password is fine.
            return true; 
        }

        /// <summary>
        /// Generates a new password of minimum length specified in the policy
        /// The generated password starts with an alphabet and contains 1 digit and 1 special character
        /// </summary>
        /// <returns>New password</returns>
        internal static string GenerateNewPassword()
        {
            int minimumLength = minLength;
            //// Generate a string of random characters of minimum allowed length for a password
            char[] newPassword = new char[minimumLength];
            Random random = new Random((int)DateTime.Now.Ticks);
            //// Choose a random capital letter as the first character of the password
            newPassword[0] = Char.Parse(Char.ConvertFromUtf32(random.Next(65, 90)));
            for (int i = 1; i < minLength; i++)
            {
                newPassword[i] = Char.Parse(Char.ConvertFromUtf32(random.Next(97, 122)));
            }

            //// Insert a special character into the password
            newPassword[2] = SpecialCharacters[random.Next(0, 30)];
            //// Insert a digit into the new password
            newPassword[4] = Char.Parse(Char.ConvertFromUtf32(random.Next(48, 58)));
            StringBuilder builder = new StringBuilder(minLength);
            builder.Append(newPassword);
            return builder.ToString();
        }

        #region Methods checking policy conformance

        /// <summary>
        /// Checks whether the password contains a digit if policy says at least 
        /// one digit must be present
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>System.Boolean; <c>true</c> if the password conforms to the digit policy, 
        /// <c>false</c> otherwise.</returns>
        private static bool ConformsToDigitPolicy(string password)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }
            #endregion
            if (mustContainDigit)
            {
                foreach (char c in password)
                {
                    if (char.IsDigit(c))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        private const string SpecialCharacters = "~`!@#$%^&*()_-+={[}]|\\:;\"'<,>.?/";

        /// <summary>
        /// Checks whether the password contains a special character if policy
        /// says a password must include a special character
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>System.Boolean; <c>true</c> if the password conforms to the special
        /// character policy, <c>false</c> otherwise.</returns>
        private static bool ConformsToSpecialCharPolicy(string password)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }
            #endregion
            if (mustContainSpecialCharacter)
            {
                foreach (char c in password)
                {
                    if (SpecialCharacters.Contains(c))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether password starts with an alphabet if policy enforces that 
        /// a password must start with an alphabet.
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>System.Boolean; <c>true</c> if the password conforms to the start
        /// with alphabet policy, <c>false</c> otherwise.</returns>
        private static bool ConformsToStartWithAlphabetPolicy(string password)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }
            #endregion
            if (mustStartWithAlphabet)
            {
                if (char.IsLetter(password[0]))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether password conforms to minimum and maximum length policy
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>System.Boolean; <c>true</c> if the password conforms to the
        /// policy, <c>false</c> otherwise.</returns>
        private static bool ConformsToLengthPolicy(string password)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }
            #endregion
            return password.Length >= minLength && password.Length <= maxLength;
        }

        #endregion

        /// <summary>
        /// Reads the policy.
        /// </summary>
        private static void ReadPolicy()
        {
            try
            {
                PasswordPolicy policySection = ConfigurationManager.GetSection("PasswordPolicy")
                    as PasswordPolicy;

                if (policySection != null)
                {
                    PasswordPolicyElement policyElement = policySection.PolicyElement;

                    expiresInDays = policyElement.ExpiresInDays;
                    maxLength = policyElement.MaximumLength;
                    minLength = policyElement.MinimumLength;
                    mustContainDigit = policyElement.MustContainDigit;
                    mustContainSpecialCharacter = policyElement.MustContainSpecialCharacter;
                    mustStartWithAlphabet = policyElement.StartWithAlphabet;
                }
                else
                {
                    //// Indicates no policy read exception
                    throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage);
                }
            }
            catch (ConfigurationErrorsException)
            {
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage);
            }
        }
    }
}
