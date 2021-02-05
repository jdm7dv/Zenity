// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider
{
    using System;
    using Zentity.Security.Authentication;
    using Zentity.Security.AuthenticationProvider.PasswordManagement;

    /// <summary>
    /// This class represents a token assigned to a user after he is authenticated
    /// </summary>
    /// /// <example>
    /// Pre-requisites for running this code sample
    /// <list type="bullet">
    /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
    /// <item>Make sure Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll are present in
    /// the application's bin\debug or bin\release folder.</item>
    /// <item>Run the authentication database script (present in Program Files\Zentity 1.0\Scripts folder)</item>
    /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
    /// <item>Then run this sample, replacing the username, password with valid values.</item>
    /// </list>
    /// <code>
    ///    //Grab an instance of authentication provider
    ///    IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
    ///    //Get user credentials and create a security token
    ///    UserNameSecurityToken userCredentials = new UserNameSecurityToken(&quot;UserName&quot;, &quot;Password&quot;); // OR new UserNameSecurityToken(txtUserName.Text, txtPassword.Text);
    ///    //Call Authenticate() method on the provider. The AuthenticatedToken returned by the method is used for all authorization operations.
    ///    AuthenticatedToken userToken = provider.Authenticate(userCredentials);
    ///    if (userToken != null)
    ///    {
    ///        Console.WriteLine(&quot;User authenticated successfully&quot;);
    ///        //Other processing, e.g. calling other methods, passing token to other component / page for authorizing operations
    ///        //Every component / page / form which receives an instance of AuthenticatedToken should call Validate() on the method
    ///        //to ensure that the token is not tampered with while travelling on network, or otherwise.
    ///        bool isTokenValid = userToken.Validate();
    ///        if (isTokenValid)
    ///        {
    ///            Console.WriteLine(&quot;Token is valid&quot;);
    ///        }
    /// </code>
    /// </example>
    public class ZentityAuthenticatedToken : AuthenticatedToken
    {
        private string identity;
        private string hashedIdentity;
        private DateTime? validFrom;
        private DateTime? validUpTo;

        /// <summary>
        /// Initializes a new instance of the ZentityAuthenticatedToken class.
        /// </summary>
        /// <param name="identityName">User name</param>
        internal ZentityAuthenticatedToken(string identityName)
        {
            #region Input validation
            if (string.IsNullOrEmpty(identityName))
            {
                throw new ArgumentNullException("identityName");
            }
            #endregion
            this.identity = identityName;
            this.hashedIdentity = HashingUtility.GenerateHash(this.identity);
        }

        /// <summary>
        /// Gets or sets the date time the token is valid upto.
        /// If ValidUpto is not null the value being assigned to ValidFrom must be less than ValidUpto.
        /// </summary>
        /// <remarks>
        /// The properties ValidFrom and ValidUpTo are specific to the ZentityAuthenticatedToken class. These are not present in the parent 
        /// AuthenticatedToken class. Hence you need to typecast AuthenticatedToken object to ZentityAuthenticatedToken object to access 
        /// these properties.
        /// The Validate() method of ZentityAuthenticatedToken also checks whether the token has expired, 
        /// along with checking whether the token has been tampered with.
        /// </remarks>
        /// <example>
        /// <code>
        ///    //Grab an instance of authentication provider
        ///    IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
        ///    //Get user credentials and create a security token
        ///    UserNameSecurityToken userCredentials = new UserNameSecurityToken(&quot;UserName&quot;, &quot;Password&quot;); // OR new UserNameSecurityToken(txtUserName.Text, txtPassword.Text);
        ///    //Call Authenticate() method on the provider. The AuthenticatedToken returned by the method is used for all authorization operations.
        ///    AuthenticatedToken userToken = provider.Authenticate(userCredentials);
        ///    if (userToken != null)
        ///    {
        ///        Console.WriteLine(&quot;User authenticated successfully&quot;);
        ///
        ///        //Set valid from and valid upto values for the user token.
        ///        //The properties ValidFrom and ValidUpTo are specific to the ZentityAuthenticatedToken class. These are not present in the parent 
        ///        //AuthenticatedToken class. Hence you need to typecast AuthenticatedToken object to ZentityAuthenticatedToken object to access 
        ///        //these properties.
        ///        ZentityAuthenticatedToken zentityUserToken = (ZentityAuthenticatedToken)userToken;
        ///        zentityUserToken.ValidFrom = DateTime.Now;
        ///        zentityUserToken.ValidUpTo = DateTime.Now.AddSeconds(30);
        ///
        ///        //This call should print 'Token is valid' - as it will be called immediately after setting valid upto.
        ///        bool tokenValid = zentityUserToken.Validate();
        ///        if (tokenValid)
        ///        {
        ///            Console.WriteLine(&quot;Token is valid&quot;);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;Token has expired&quot;);
        ///        }
        ///        //introduce a 40 second delay
        ///        Thread.Sleep(40000);
        ///        //The token should be expired now. The following code should print 'Token has expired'.
        ///        tokenValid = zentityUserToken.Validate();
        ///        if (tokenValid)
        ///        {
        ///            Console.WriteLine(&quot;Token is valid&quot;);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;Token has expired&quot;);
        ///        }
        ///    }
        /// </code>
        /// </example>
        public DateTime? ValidFrom
        {
            get
            {
                return this.validFrom;
            }
            set
            {
                if (this.ValidUpTo != null)
                {
                    if (value < this.ValidUpTo)
                    {
                        this.validFrom = value;
                    }
                    else
                    {
                        throw new ArgumentException(ConstantStrings.ValidFromValidUpToExceptionMessage);
                    }
                }
                else
                {
                    this.validFrom = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the date time the token is valid upto.
        /// If ValidFrom is not null the value being assigned to ValidUpto must be greater than ValidFrom.
        /// </summary>
        /// <remarks>
        /// The properties ValidFrom and ValidUpTo are specific to the ZentityAuthenticatedToken class. These are not present in the parent 
        /// AuthenticatedToken class. Hence you need to typecast AuthenticatedToken object to ZentityAuthenticatedToken object to access 
        /// these properties.
        /// The Validate() method of ZentityAuthenticatedToken also checks whether the token has expired, 
        /// along with checking whether the token has been tampered with.
        /// </remarks>
        /// <example>Please refer to ValidFrom help page for code sample.</example>
        public DateTime? ValidUpTo
        {
            get
            {
                return this.validUpTo;
            }
            set
            {
                if (this.ValidFrom != null)
                {
                    if (value > this.ValidFrom)
                    {
                        this.validUpTo = value;
                    }
                    else
                    {
                        throw new ArgumentException(ConstantStrings.ValidFromValidUpToExceptionMessage);
                    }
                }
                else
                {
                    this.validUpTo = value;
                }
            }
        }

        /// <summary>
        /// Validates whether the token is valid, that is it is not tampered with, and it is not expired.
        /// </summary>
        /// <returns>True if token is valid</returns>
        /// <example>Please refer to the ZentityAuthenticatedToken class help page for a code sample.</example>
        public override bool Validate()
        {
            string hashOfCurrentIdentity = HashingUtility.GenerateHash(this.identity);
            if (string.Compare(hashOfCurrentIdentity, this.hashedIdentity, StringComparison.Ordinal) == 0)
            {
                if (this.ValidUpTo != null)
                {
                    if (this.ValidUpTo >= DateTime.Now)
                    {
                        return true;
                    }

                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Authenticated identity instance which is represented by this token
        /// </summary>
        public override string IdentityName
        {
            get 
            {
                return this.identity; 
            }
        }
    }
}
