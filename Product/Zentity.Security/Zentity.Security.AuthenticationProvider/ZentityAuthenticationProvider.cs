// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider
{
    using System;
    using System.IdentityModel.Tokens;
    using Zentity.Security.Authentication;

    /// <summary>
    /// This class provides user authentication for Zentity users
    /// </summary>
    /// <example>
    /// Pre-requisites for running this code sample
    /// <list type="bullet">
    /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
    /// <item>Make sure Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll are present in
    /// the application's bin\debug or bin\release folder.</item>
    /// <item>Run the authentication database script (present in Program Files\Zentity 1.0\Scripts folder)</item>
    /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
    /// <item>Then run this sample, replacing the username, password with valid values.</item>
    /// </list>
    /// 
    ///  <code>
    /// //--------------------------------------------- Sample code for authenticating a user ------------------------
    ///        //Grab an instance of authentication provider
    ///        IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
    ///        //Get user credentials and create a security token
    ///        UserNameSecurityToken userCredentials = new UserNameSecurityToken(&quot;UserName&quot;, &quot;Password&quot;); // OR, say, new UserNameSecurityToken(txtUserName.Text, txtPassword.Text);
    ///        //Call Authenticate() method on the provider. The AuthenticatedToken returned by the method is used for all authorization operations.
    ///        AuthenticatedToken userToken = provider.Authenticate(userCredentials);
    ///        if (userToken != null)
    ///        {
    ///            Console.WriteLine(&quot;User authenticated successfully&quot;);
    ///            //Other processing, e.g. calling other methods, passing token to other component / page for authorizing operations
    ///        }
    /// //------------------------------------------------------------------------------------------------------------
    /// </code>
    /// </example>
    public class ZentityAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Gets a value indicating whether an instance of this class can be reused for multiple user authentications
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Authenticates a user based on the UserNameSecurityToken passed
        /// </summary>
        /// <param name="credentialToken">This parameter is expected to be a UserNameSecurityToken</param>
        /// <returns>AuthenticatedToken instance for the user if authentication succeeds, null otherwise</returns>
        public AuthenticatedToken Authenticate(SecurityToken credentialToken)
        {
            #region Input validation
            // Validate input
            if (credentialToken == null)
            {
                throw new ArgumentNullException("credentialToken");
            }

            UserNameSecurityToken credential = credentialToken as UserNameSecurityToken;
            if (credential == null)
            {
                throw new ArgumentException(ConstantStrings.InvalidTokenTypeMessage, "credentialToken");
            }
            #endregion

            try
            {
                // Authenticate user
                if (!string.IsNullOrEmpty(credential.UserName) && !string.IsNullOrEmpty(credential.Password))
                {
                    ZentityUser user = new ZentityUser(credential.UserName, credential.Password);
                    bool authenticated = user.VerifyPassword();
                    if (authenticated)
                    {
                        ZentityAuthenticatedToken token = new ZentityAuthenticatedToken(credential.UserName.ToString());
                        return token;
                    }
                }

                // If username/password are not provided, or are invalid, return a null token.
                return null;
            }
            catch (TypeInitializationException ex) 
            {
                // thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }
    }
}
