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
using System.IdentityModel.Tokens;
using Zentity.Services.Properties;
using Zentity.Security.AuthorizationHelper;

namespace Zentity.Services.HttpAuthentication
{
    /// <summary>
    /// Provides HTTP Basic authentication.
    /// </summary>
    internal class BasicAuthentication
    {
        private IAuthenticationProvider provider;

        /// <summary>
        /// Initializes a new instance of the BasicAuthentication class. 
        /// Sets authentication provider instance to the parameter. 
        /// </summary>
        /// <param name="authenticationProvider">ZentityAuthenticationProvider instance</param>
        /// <remarks>This class decodes basic authorization header sent in an http request, and 
        /// authenticates user using ZentityAuthenticationProvider instance passed to the constructor as a parameter.</remarks>
        internal BasicAuthentication(IAuthenticationProvider authenticationProvider)
        {
            if (authenticationProvider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this.provider = authenticationProvider;
        }

        /// <summary>
        /// Authenticates user whose credentials are sent in the authorization header. Calls the authentication API for validating 
        /// user credentials.
        /// </summary>
        /// <param name="authorizationHeader">Authorization header for http basic authentication sent in the request.</param>
        /// <param name="statusCode">Status code indicating user authentication status.</param>
        /// <returns>Authenticated token representing the user if the user credentials are valid, null otherwise.</returns>
        /// <remarks>This method returns AuthenticatedToken for the user if credentials are valid. It also sets the http 
        /// status code to appropriate value.</remarks>
        public AuthenticatedToken Authenticate(string authorizationHeader, out int statusCode)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                statusCode = 401;//Unauthorized
                return null;
            }
            #endregion

            string userName;
            string password;
            bool isAuthorizationHeaderValid = ExtractCredentials(authorizationHeader, out userName, out password);
            if (!isAuthorizationHeaderValid)
            {
                statusCode = 400;//Bad request
                return null;
            }
            ;
            AuthenticatedToken token = AuthenticateUser(userName, password);

            if (token == null)
            {
                statusCode = 401;
                return null;
            }
            statusCode = 200;
            return token;
        }

        /// <summary>
        /// Returns the user name present in the header.
        /// </summary>
        /// <param name="authorizationHeader">Authorization header for http basic authentication sent in the request.</param>
        /// <returns>User name present in header.</returns>
        public static string GetUserName(string authorizationHeader)
        {
            #region Parameter validation
            if (string.IsNullOrEmpty(authorizationHeader))
            {                
                return null;
            }
            #endregion

            string userName;
            string password;
            bool isAuthorizationHeaderValid = ExtractCredentials(authorizationHeader, out userName, out password);

            if (!isAuthorizationHeaderValid)
            {                
                return null;
            }

            return userName;
        }

        /// <summary>
        /// Returns www-authenticate header for basic authentication.
        /// </summary>
        /// <returns>Www-authenticate header to be sent along with a 'unauthorized' response.</returns>
        public static string GetWwwAuthenticateHeader()
        {
            string header = "Basic realm=\"" + ConfigurationManager.AppSettings["Realm"] + "\"";
            return header;
        }

        #region Private methods
        /// <summary>
        /// Authenticates a user by calling the authentication provider's Authenticate API.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns>The <see cref="AuthenticatedToken"/>.</returns>
        private AuthenticatedToken AuthenticateUser(string userName, string password)
        {
            #region Parameter Validation
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }
            #endregion

            UserNameSecurityToken token = new UserNameSecurityToken(userName, password);
            AuthenticatedToken userToken = provider.Authenticate(token);
            return userToken;
        }

        /// <summary>
        /// Extracts username and password from the authorization header.
        /// </summary>
        /// <param name="authorizationHeader">The authorization header.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns>System.Boolean; <c>true</c> if the operation was successful, <c>false</c> otherwise.</returns>
        private static bool ExtractCredentials(string authorizationHeader, out string userName, out string password)
        {
            if (!authorizationHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            {
                userName = string.Empty;
                password = string.Empty;
                return false;
            }

            if (authorizationHeader.Length > 6)
            {
                authorizationHeader = authorizationHeader.Substring(6);
                ASCIIEncoding encoding = new ASCIIEncoding();
                try
                {
                    byte[] credentialBytes = Convert.FromBase64String(authorizationHeader);
                    string credentials = encoding.GetString(credentialBytes);

                    string[] credentialParts = credentials.Split(':');
                    if (credentialParts.Length != 2)
                    {
                        userName = string.Empty;
                        password = string.Empty;
                        return false;
                    }
                    userName = credentialParts[0];
                    password = credentialParts[1];
                    return true;
                }
                catch (FormatException)
                {
                    userName = string.Empty;
                    password = string.Empty;
                    return false;
                }
            }
            else
            {
                userName = string.Empty;
                password = string.Empty;
                return false;
            }
        }
        #endregion
    }
}
