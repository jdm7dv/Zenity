// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zentity.Security.Authentication;
using System.Configuration;
using System.Security.Cryptography;
using System.Collections.Specialized;
using Zentity.Services.Properties;
using Zentity.Security.AuthenticationProvider;
using Zentity.Security.AuthenticationProvider.DigestAuthentication;
using System.Globalization;

namespace Zentity.Services.HttpAuthentication
{
    /// <summary>
    /// Provides HTTP Digest authentication.
    /// </summary>
    internal class DigestAuthentication
    {
        #region Private variables
        private IAuthenticationProvider provider;
        private readonly string opaque;
        private readonly string realm = "Zentity";
        private const string PrivateKey = "BC00C507-B079-4739-80C8-294BA8627DF2";
        private string algoForChecksum, algoForDigest;
        private string httpMethod;
        Dictionary<string, string> reqInfo;
        private int nonceExpiryInMinutes;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the DigestAuthentication class. 
        /// </summary>
        /// <param name="authenticationProvider">HttpDigestAuthenticationProvider instance.</param>
        /// <remarks>This class contains functions to decode digest authorization header, 
        /// and authenticate user using HttpDigestAuthenticationProvider instance passed as a parameter to the constuctor.</remarks>
        internal DigestAuthentication(IAuthenticationProvider authenticationProvider)
        {
            if (authenticationProvider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this.provider = authenticationProvider;
            opaque = Guid.NewGuid().ToString();
            algoForChecksum = "MD5";
            algoForDigest = "MD5";
            nonceExpiryInMinutes = Convert.ToInt32(
                ConfigurationManager.AppSettings["NonceExpiryInMinutes"], CultureInfo.InvariantCulture);
        }
        #endregion

        /// <summary>
        /// Authenticates a user whose credentials are sent in digest authorization header.
        /// </summary>
        /// <param name="authorizationHeader">Authorization header</param>
        /// <param name="httpMethod">Request HttpMethod</param>
        /// <param name="statusCode">Request status code</param>
        /// <returns>AuthenticatedToken if user is authenticated, null otherwise.</returns>
        public AuthenticatedToken Authenticate(string authorizationHeader, string httpMethod, 
            out int statusCode)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                statusCode = 401;
                return null;
            }
            if (string.IsNullOrEmpty(httpMethod))
            {
                statusCode = 400;
                return null;
            }
            #endregion

            this.httpMethod = httpMethod;
            if (!IsAuthorizationHeaderValid(authorizationHeader))
            {
                statusCode = 400;
                return null;
            }
            //Extract various parts of the header.
            reqInfo = ExtractAuthorizationHeaderElements(authorizationHeader);
            if (ValidateAuthorizationHeader(reqInfo))
            {
                AuthenticatedToken userToken = AuthenticateUser();
                if (userToken == null)
                {
                    statusCode = 401;
                    return null;
                }
                else
                {
                    if (IsNonceStale(reqInfo["nonce"]))
                    {
                        statusCode = 401;
                        return null;
                    }
                    statusCode = 200;
                    return userToken;
                }
            }
            else
            {
                statusCode = 400;
                return null;
            }
        }

        /// <summary>
        /// Returns the user name present in the header.
        /// </summary>
        /// <param name="authorizationHeader">Authorization header for http digest authentication sent in the request.</param>
        /// <returns>User name present in header.</returns>
        public string GetUserName(string authorizationHeader)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return null;
            }
            #endregion

            reqInfo = ExtractAuthorizationHeaderElements(authorizationHeader);

            if (!ValidateAuthorizationHeader(reqInfo))
            {
                return null;
            }

            return reqInfo["username"];
        }

        /// <summary>
        /// Returns the WWW-Authenticate header string for digest authentication.
        /// </summary>
        /// <returns>The www-authenticate header string.</returns>
        public string GetWwwAuthenticateHeader()
        {
            StringBuilder headerBuilder = new StringBuilder("Digest ");
            headerBuilder.Append("realm=\"" + realm + "\",");

            string nonce = GetNonce();
            headerBuilder.Append("nonce=\"" + nonce + "\",");
            headerBuilder.Append("opaque=\"" + opaque + "\",");
            headerBuilder.Append("stale=" + IsNonceStale(nonce).ToString() + ",");
            //headerBuilder.Append("algorithm=\"" + _algoForDigest + "," + _algoForChecksum + "\"");
            headerBuilder.Append("algorithm=\"" + algoForDigest + "\"");
            return headerBuilder.ToString();
        }
        
        /// <summary>
        /// Returns the authentication info header to be sent in response for an authenticated request.
        /// </summary>
        /// <returns>Authentication-Info header to be sent along with a response for authenticated request.</returns>
        public string GetAuthenticationInfoHeader()
        {
            string nonceValue = GetNonce();
            string authenticationInfoHeader = "Authentication-Info : nextnonce=\"" + nonceValue + "\"";
            return authenticationInfoHeader;
        }

        #region Private methods

        #region Authorization Header Extraction

        /// <summary>
        /// Splits the authorization header elements and creates a dictionary of the elements.
        /// </summary>
        /// <param name="authorizationHeader">The authorization header.</param>
        /// <returns>Dictionary of authorization header elements.</returns>
        private static Dictionary<string, string> ExtractAuthorizationHeaderElements(string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return new Dictionary<string, string>(0);
            }

            Dictionary<string, string> reqInfo = new Dictionary<string, string>();
            if (authorizationHeader.Length > 7)
            {
                authorizationHeader = authorizationHeader.Substring(7);

                string[] elems = authorizationHeader.Split(',');
                if (elems == null || elems.Length == 0)
                {
                    return new Dictionary<string, string>(0);
                }
                foreach (string elem in elems)
                {
                    // form key="value"
                    string[] parts = elem.Split(new char[] { '=' }, 2);
                    if (parts.Length != 2)
                    {
                        continue;
                    }
                    string key = parts[0].Trim(new char[] { ' ', '\"' });
                    string val = parts[1].Trim(new char[] { ' ', '\"' });
                    reqInfo.Add(key, val);
                }
                return reqInfo;
            }
            else
            {
                return new Dictionary<string, string>(0);
            }
        }

        #endregion

        #region Validations

        /// <summary>
        /// Check whether authorization header is present and starts with 'Digest'.
        /// </summary>
        /// <param name="authorizationHeader">The authorization header.</param>
        /// <returns>
        /// 	<c>true</c> if the authorization header is present and starts with <c>Digest</c>; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsAuthorizationHeaderValid(string authorizationHeader)
        {
            if (!authorizationHeader.StartsWith("Digest", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates the elements collection extracted from the authorization header.
        /// </summary>
        /// <param name="reqInfo">The req info.</param>
        /// <returns>System.Boolean; <c>true</c> if the authorization header is valid; otherwise <c>false</c>.</returns>
        private static bool ValidateAuthorizationHeader(Dictionary<string, string> reqInfo)
        {
            if (reqInfo.Count == 0)
            {
                return false;
            }
            if (!reqInfo.ContainsKey("username")
                || !reqInfo.ContainsKey("realm")
                || !reqInfo.ContainsKey("nonce")
                || !reqInfo.ContainsKey("uri")
                || !reqInfo.ContainsKey("response"))
            {
                return false;
            }
            //Check that all the required directive values are present
            if (string.IsNullOrEmpty(reqInfo["username"])
                || string.IsNullOrEmpty(reqInfo["realm"])
                || string.IsNullOrEmpty(reqInfo["nonce"])
                || string.IsNullOrEmpty(reqInfo["uri"])
                || string.IsNullOrEmpty(reqInfo["response"]))
            {
                return false;
            }
            //Since we do not send 'qop' in the WWW-Authenticate header the following keys must not 
            //be sent by the client in the header.
            if (reqInfo.ContainsKey("qop")
                || reqInfo.ContainsKey("cnonce")
                || reqInfo.ContainsKey("nonce-count"))
            {
                return false;
            }
            return true;
        }

        #endregion

        #endregion

        #region Nonce Creation
        /// <summary>
        /// Returns the nonce header element nonce="noncevalue".
        /// </summary>
        /// <returns>Nonce header value.</returns>
        private string GetNonce()
        {
            //Nonce value is timestamp:H(timestamp:privatekey)
            //This method does the following - 
            //Check if authorization header consisted a nonce - 
            //If so, is it stale? 
            //If not return the same nonce.
            //Else
            //get the current timestamp
            //Calculate hash of timestamp:privatekey
            //Build string - timestamp:H(timestamp:privatekey)

            bool newNonceRequired = false;
            if (reqInfo == null || string.IsNullOrEmpty(reqInfo["nonce"]))
            {
                newNonceRequired = true;
            }
            else if (IsNonceStale(reqInfo["nonce"]))
            {
                newNonceRequired = true;
            }
            if (newNonceRequired)
            {
                string timeStamp = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
                string nonceHash = GetNonceValue(timeStamp);
                StringBuilder nonceBuilder = new StringBuilder(timeStamp);
                nonceBuilder.Append(':');
                nonceBuilder.Append(nonceHash);
                return nonceBuilder.ToString();
            }
            return reqInfo["nonce"];
        }

        /// <summary>
        /// Gets the nonce value - timestamp:H(timestamp:privatekey)
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <returns>The nonce value.</returns>
        private string GetNonceValue(string timeStamp)
        {
            StringBuilder nonceValueBuilder = new StringBuilder(timeStamp);
            nonceValueBuilder.Append(':');
            nonceValueBuilder.Append(PrivateKey);
            string nonceValue = nonceValueBuilder.ToString();

            string nonceStringHash = ComputeHash(nonceValue, algoForChecksum);
            return nonceStringHash;
        }

        /// <summary>
        /// Checks when was the nonce created. If it was created more than 'N' minutes back (configurable)
        /// it returns true. 
        /// </summary>
        /// <param name="nonce">The nonce.</param>
        /// <returns>
        /// 	<c>true</c> if the nonce was created more than 'N' minutes back (configurable); otherwise, <c>false</c>.
        /// </returns>
        private bool IsNonceStale(string nonce)
        {
            if (string.IsNullOrEmpty(nonce))
            {
                return false;
            }
            long ticks = Convert.ToInt64(nonce.Split(':')[0], CultureInfo.InvariantCulture);
            long nonceMinutes = (long)TimeSpan.FromTicks(ticks).TotalMinutes; //ignore fractional part
            long currentMinutes = (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMinutes;
            if (currentMinutes - nonceMinutes > nonceExpiryInMinutes) //nonce was created more than <n> minutes ago
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Computes the hash of the string.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="algorithm">The algorithm.</param>
        /// <returns>THe hash of the string.</returns>
        private static string ComputeHash(string str, string algorithm)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException("str");
            }

            StringBuilder hashBuilder = new StringBuilder();
            using (HashAlgorithm algo = HashAlgorithm.Create(algorithm))
            {
                Encoding encoding = new ASCIIEncoding();
                byte[] byteStr = encoding.GetBytes(str);
                byte[] hashedBytes = algo.ComputeHash(byteStr);
                for (int i = 0; i < 16; i++)
                {
                    hashBuilder.Append(String.Format(CultureInfo.InvariantCulture, "{0:x02}", hashedBytes[i]));
                }
            }

            return hashBuilder.ToString();
        }
        #endregion

        #region Authenticate user
        /// <summary>
        /// Call the authentication API to validate user credentials present in the request digest
        /// </summary>
        /// <returns>The <see cref="AuthenticatedToken"/>.</returns>
        private AuthenticatedToken AuthenticateUser()
        {
            if (reqInfo == null)
            {
                return null;
            }
            DigestSecurityToken userToken = new DigestSecurityToken(reqInfo["response"],
                reqInfo["username"],
                reqInfo["nonce"],
                reqInfo["realm"],
                reqInfo["uri"],
                httpMethod,
                algoForDigest, 
                algoForChecksum);
            AuthenticatedToken token = provider.Authenticate(userToken);
            return token;
        }
        #endregion
    }
}
