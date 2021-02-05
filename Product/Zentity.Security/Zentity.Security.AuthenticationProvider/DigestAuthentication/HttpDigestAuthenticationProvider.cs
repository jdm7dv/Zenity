// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider.DigestAuthentication
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.Text;
    using Zentity.Security.Authentication;

    /// <summary>
    /// This class provides authentication for user credentials embedded in HTTP digest header.
    /// </summary>
    public class HttpDigestAuthenticationProvider : IAuthenticationProvider
    {
        #region IAuthenticationProvider Members

        /// <summary>
        /// Gets a value indicating whether this provider instance can be 
        /// used to process multiple authentication requests.
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Authenticates a user whose credentials are represented by the digest security token.
        /// </summary>
        /// <param name="credentialToken">Must be of type DigestSecurityToken</param>
        /// <returns>AuthenticatedToken of the user if credentials are valid, null otherwise.</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.
        /// Make sure the AuthenticationConnection is pointing to ZentityAuthentication database in your Zentity installation.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Add reference to System.IdentityModel namespace.</item>
        /// <item>Add a main method and call the DigestAuthenticationSample.DigestSample() method from Main()</item>
        /// </list>
        /// <code>
        ///    using System;
        ///    using System.Collections.Generic;
        ///    using System.Linq;
        ///    using System.Text;
        ///    using Zentity.Security.Authentication;
        ///    using Zentity.Security.AuthenticationProvider.DigestAuthentication;
        ///
        ///    namespace SecuritySamples
        ///    {
        ///        /// &lt;summary&gt;
        ///        /// Provides a sample for using HttpDigestAuthenticationProvider.
        ///        /// &lt;/summary&gt;
        ///        internal class DigestAuthenticationSample
        ///        {
        ///           internal static void DigestSample()
        ///            {
        ///                //Get an HttpDigestAuthenticationProvider instance from AuthenticationProviderFactory.
        ///                IAuthenticationProvider digestAuthenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;HttpDigestAuthenticationProvider&quot;);
        ///
        ///                //Get authorization header. This sample is using the following hardcoded header created with built in administrator credentials. 
        ///                //However in practice this header will be sent by the digest client - e.g. IE/FF.
        ///                string authorizationHeader = &quot;Digest  username=\&quot;Administrator\&quot;,realm=\&quot;\&quot;,nonce=\&quot;\&quot;,uri=\&quot;http://localhost:9090/AtomPub\&quot;,response=\&quot;ba4573d45d799a1cee56edfe03ed3161\&quot;,algorithm=\&quot;MD5,MD5\&quot;,opaque=\&quot;\&quot;&quot;;
        ///                Dictionary&lt;string, string&gt; reqInfo = ExtractAuthorizationHeaderElements(authorizationHeader); 
        ///
        ///                //Create DigestSecurityToken using the authorization header elements.
        ///                if (reqInfo != null)
        ///                {
        ///                    DigestSecurityToken userToken = new DigestSecurityToken(reqInfo[&quot;response&quot;],
        ///                        reqInfo[&quot;username&quot;],
        ///                        reqInfo[&quot;nonce&quot;],
        ///                        reqInfo[&quot;realm&quot;],
        ///                        reqInfo[&quot;uri&quot;],
        ///                        &quot;GET&quot;,
        ///                        reqInfo[&quot;algorithm&quot;],//Most of the times both algorithm for digest and algorithm for checksum are set to 'MD5'.
        ///                        reqInfo[&quot;algorithm&quot;]);
        ///                    AuthenticatedToken token = digestAuthenticationProvider.Authenticate(userToken);
        ///                    if (token == null)
        ///                    {
        ///                        Console.WriteLine(&quot;Authentication failed&quot;);
        ///                    }
        ///                    else
        ///                    {
        ///                        Console.WriteLine(&quot;Authentication succeeded&quot;);
        ///                    }
        ///                }
        ///            }
        ///
        ///            //Splits the authorization header elements and creates a dictionary of the elements.
        ///            private static Dictionary&lt;string, string&gt; ExtractAuthorizationHeaderElements(string authorizationHeader)
        ///            {
        ///                if (string.IsNullOrEmpty(authorizationHeader))
        ///                {
        ///                    return new Dictionary&lt;string, string&gt;(0);
        ///                }
        ///
        ///                Dictionary&lt;string, string&gt; reqInfo = new Dictionary&lt;string, string&gt;();
        ///                if (authorizationHeader.Length &gt; 7)
        ///                {
        ///                    authorizationHeader = authorizationHeader.Substring(7);
        ///
        ///                    string[] elems = authorizationHeader.Split(',');
        ///                    if (elems == null || elems.Length == 0)
        ///                    {
        ///                        return new Dictionary&lt;string, string&gt;(0);
        ///                    }
        ///                    foreach (string elem in elems)
        ///                    {
        ///                        // form key=&quot;value&quot;
        ///                        string[] parts = elem.Split(new char[] { '=' }, 2);
        ///                        if (parts.Length != 2)
        ///                        {
        ///                            continue;
        ///                        }
        ///                        string key = parts[0].Trim(new char[] { ' ', '\&quot;' });
        ///                        string val = parts[1].Trim(new char[] { ' ', '\&quot;' });
        ///                        reqInfo.Add(key, val);
        ///                    }
        ///                    //This logic will extract one algorithm value. Most of the times, both algorithm values are set to 'MD5'. If this is not 
        ///                    //the case, a different extraction logic is required.
        ///                    return reqInfo;
        ///                }
        ///                else
        ///                {
        ///                    return new Dictionary&lt;string, string&gt;(0);
        ///                }
        ///            }
        ///        }
        ///    }
        ///
        /// </code>
        /// </example>
        public AuthenticatedToken Authenticate(SecurityToken credentialToken)
        {
            DigestSecurityToken userToken;

            //// Parameter validation
            if (credentialToken == null)
            {
                throw new ArgumentNullException("credentialToken");
            }

            userToken = credentialToken as DigestSecurityToken;
            if (userToken == null)
            {
                throw new ArgumentException(ConstantStrings.InvalidDigestToken);
            }

            if (!ValidateDigestToken(userToken))
            {
                throw new ArgumentException(
                            string.Format(
                                CultureInfo.CurrentUICulture,
                                ConstantStrings.MandatoryPropertyNotSetExceptionMessage,
                                ConstantStrings.DigestTokenProperties));
            }

            //// Process Authentication using the DigestSecurityToken
            ZentityAuthenticatedToken token = ProcessAuthentication(userToken);
            return token;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates a digest for the given user and his password stored in the database, and compares against the 
        /// digest sent in the response.
        /// This method assumes that the userToken is validated for having values for all required properties.
        /// </summary>
        /// <param name="userToken">User token</param>
        /// <returns>Zentity authenticated token</returns>
        private static ZentityAuthenticatedToken ProcessAuthentication(DigestSecurityToken userToken)
        {
            //// Get user's password from db
            //// Compute H(A2) based using the request uri stored in the token.
            //// Compute H(A1) using user name and realm stored in the token, and password retrieved from the database.
            //// Compute Hash of H(A1):nonce:H(A2)
            //// Compare against the digest hash stored in the token.
            ZentityAuthenticatedToken token = null;
            string databasePassword = ZentityUserManager.GetPassword(userToken.UserName);

            //// No db password means invalid user name, since, we do not allow empty passwords.
            if (string.IsNullOrEmpty(databasePassword)) 
            {
                return null;
            }

            string hA1 = ComputeHash(
                            userToken.UserName + ":" + userToken.Realm + ":" + databasePassword, 
                            userToken.ChecksumAlgorithm);
            string hA2 = ComputeHash(
                            userToken.HttpMethod + ":" + userToken.RequestUri, 
                            userToken.ChecksumAlgorithm);
            string databaseDigest = hA1 + ":" + userToken.Nonce + ":" + hA2;
            string databaseDigestHash = ComputeHash(databaseDigest, userToken.DigestAlgorithm);
            if (string.Equals(databaseDigestHash, userToken.DigestResponse))
            {
                token = new ZentityAuthenticatedToken(userToken.UserName);
            }

            return token;
        }

        /// <summary>
        /// Compute the hash for the given string.
        /// </summary>
        /// <param name="str">String for which hash is to be computed</param>
        /// <param name="algorithm">Algorithm</param>
        /// <returns>Hash code of the string</returns>
        private static string ComputeHash(string str, string algorithm)
        {
            using (HashAlgorithm algo = HashAlgorithm.Create(algorithm))
            {
                Encoding encoding = new ASCIIEncoding();
                byte[] byteStr = encoding.GetBytes(str);
                byte[] hashedBytes = algo.ComputeHash(byteStr);
                StringBuilder hashBuilder = new StringBuilder();
                for (int i = 0; i < 16; i++)
                {
                    hashBuilder.Append(String.Format(CultureInfo.InvariantCulture, "{0:x02}", hashedBytes[i]));
                }

                return hashBuilder.ToString();
            }
        }

        /// <summary>
        /// Validates the digest token.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <returns>System.Boolean; <c>true</c> if successful, <c>false</c> otherwise.</returns>
        private static bool ValidateDigestToken(DigestSecurityToken userToken)
        {
            if (string.IsNullOrEmpty(userToken.DigestAlgorithm)
                || string.IsNullOrEmpty(userToken.DigestResponse)
                || string.IsNullOrEmpty(userToken.HttpMethod)
                || string.IsNullOrEmpty(userToken.Nonce)
                || string.IsNullOrEmpty(userToken.Realm)
                || string.IsNullOrEmpty(userToken.RequestUri)
                || string.IsNullOrEmpty(userToken.UserName)
                || string.IsNullOrEmpty(userToken.ChecksumAlgorithm))
            {
                return false;
            }

            return true;
        }        
        #endregion
    }
}
