// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider.PasswordManagement
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// This class contains utility methods for creating hashes for strings
    /// </summary>
    internal static class HashingUtility
    {
        /// <summary>
        /// SHA1 Hash algorithm constant
        /// </summary>
        private const string SHA1HashAlgorithm = "SHA1";

        /// <summary>
        /// Returns a SHA1 hash of the given string
        /// </summary>
        /// <param name="value">String to be hashed</param>
        /// <returns>Hashed byte array</returns>
        internal static string GenerateHash(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            byte[] hashedByteValue = GenerateByteHash(value);
            string hashedStringValue = ConvertToString(hashedByteValue);
            return hashedStringValue;
        }

        /// <summary>
        /// Converts given string to byte array
        /// </summary>
        /// <param name="str">String to convert to byte array</param>
        /// <returns>Byte array</returns>
        private static byte[] ConvertToByteArray(string str)
        {
            //// Parameter validation
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(str);
            }
            
            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            byte[] array = unicodeEncoding.GetBytes(str);
            return array;
        }

        /// <summary>
        /// Converts a byte array into string
        /// </summary>
        /// <param name="byteValue">Byte array</param>
        /// <returns>String representation of the byte array</returns>
        private static string ConvertToString(byte[] byteValue)
        {
            if (byteValue == null || byteValue.Length == 0)
            {
                throw new ArgumentNullException("byteValue");
            }

            UnicodeEncoding encoding = new UnicodeEncoding();
            string hashedString = encoding.GetString(byteValue);
            return hashedString;
        }

        /// <summary>
        /// Generates hash of the given string in the form of byte array
        /// </summary>
        /// <param name="value">string value for which bytehash is to be generated</param>
        /// <returns>Byte array hash</returns>
        private static byte[] GenerateByteHash(string value)
        {
            #region parameter validation
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(value);
            }
            #endregion
            //// Compute SHA1 hash of the given value
            //// Convert value string to byte array
            byte[] valueInBytes = ConvertToByteArray(value);
            //// Get an instance of HashAlgorithm
            using (HashAlgorithm hashObject = HashAlgorithm.Create(SHA1HashAlgorithm))
            {
                //// Generate hash of the value
                byte[] hashedValue = hashObject.ComputeHash(valueInBytes);
                return hashedValue;
            }
        }
    }
}
