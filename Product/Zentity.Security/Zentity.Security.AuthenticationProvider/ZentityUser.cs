// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider
{
    using System;
    using System.Globalization;
    using Zentity.Security.Authentication;
    using Zentity.Security.AuthenticationProvider.PasswordManagement;

    /// <summary>
    /// This class represents a user who registers to the zentity installation
    /// </summary>
    /// <remarks>This class contains properties for storing / retrieving user data. It contains methods for registering new user, 
    /// managing account information / password and also retrieving profile information from the authentication store. 
    /// For registering / updating user account an object of this class should be filled in with required property values.
    /// For retrieving user profile, an object of this class should be created with log on and password set with proper values. 
    /// Then call the GetUserProfile() method which will fill in the rest of the properties with values from the user record in 
    /// authentication store.
    /// For managing password again logon name and password should be set to proper values.</remarks>
    /// <example>For code samples please refer to method documentation.</example>
    public class ZentityUser
    {
        private string securePassword;
        private AuthenticatedToken token;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ZentityUser class using the credentials.
        /// </summary>
        /// <param name="logOnName">User's logon name.</param>
        /// <param name="password">Clear text password.</param>
        /// <remarks>For a new user pass the chosen log on name and password for creating this instance.</remarks>
        public ZentityUser(string logOnName, string password)
        {
            #region Parameter Validation
            ValidateParameters("logOnName", logOnName, "password", password);
            #endregion

            this.Profile = new ZentityUserProfile();
            this.LogOnName = logOnName;
            //// Create a hash/encrypt of the password
            this.Password = password;
        }

        /// <summary>
        /// Initializes a new instance of the ZentityUser class using the 
        /// logon name of the user and his authenticated token.
        /// </summary>
        /// <param name="logOnName">LogOn name of the user</param>
        /// <param name="userToken">Authenticated token of the logged on user.</param>
        public ZentityUser(string logOnName, AuthenticatedToken userToken)
        {
            #region Parameter Validation
            ValidateParameters("logOnName", logOnName);
            if (userToken == null)
            {
                throw new ArgumentNullException("userToken");
            }
            #endregion

            this.Profile = new ZentityUserProfile();
            this.LogOnName = logOnName;
            this.Token = userToken;
        }

        /// <summary>
        /// Initializes a new instance of the ZentityUser class using 
        /// the credentials and profile parameters
        /// </summary>
        /// <param name="logOnName">User's logon name.</param>
        /// <param name="password">Clear text password.</param>
        /// <param name="profile">User's profile. Assigned to Profile property if parameter passed is non-null</param>
        /// <remarks>For a new user pass the chosen log on name and password for creating this instance.</remarks>
        public ZentityUser(string logOnName, string password, ZentityUserProfile profile)
            : this(logOnName, password)
        {
            if (profile != null)
            {
                this.Profile = profile;
                this.Profile.LogOnName = logOnName;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ZentityUser class using the logon 
        /// name of the user, his authenticated token and his profile.
        /// </summary>
        /// <param name="logOnName">LogOn name of the user</param>
        /// <param name="userToken">Authenticated token of the logged on user.</param>
        /// <param name="profile">User's profile. Assigned to Profile property if parameter passed is non-null</param>
        public ZentityUser(string logOnName, AuthenticatedToken userToken, ZentityUserProfile profile)
            : this(logOnName, userToken)
        {
            if (profile != null)
            {
                this.Profile = profile;
                this.Profile.LogOnName = logOnName;
            }
        }
        #endregion

        #region Properties for storing user information

        /// <summary>
        /// Gets or sets the user profile.
        /// </summary>
        public ZentityUserProfile Profile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unique login name for the user. 
        /// </summary>
        /// <exception cref="System.ArgumentException">If the login name is not in valid format the setter throws ArgumentException</exception>
        public string LogOnName
        {
            get
            {
                return this.Profile.LogOnName;
            }

            protected set
            {
                this.Profile.LogOnName = value;
            }
        }

        /// <summary>
        /// Gets or sets the password. 
        /// </summary>
        protected internal string Password
        {
            get
            {
                return this.securePassword;
            }

            protected set
            { 
                // Create a hash/encrypt of the password
                this.securePassword = PasswordManager.GetSecurePassword(value);
            }
        }

        /// <summary>
        /// Gets or sets the authenticatedToken of the logged on user.
        /// </summary>
        protected AuthenticatedToken Token
        {
            get
            {
                return this.token;
            }

            set 
            {
                if (!this.ValidateToken(value))
                {
                    throw new AuthenticationException(ConstantStrings.InvalidTokenMessage);
                }

                this.token = value; 
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates a new password for the user in case he forgets the password
        /// </summary>
        /// <param name="logOnName">Log on name</param>
        /// <param name="securityQuestion">Security question</param>
        /// <param name="answer">Answer to security question</param>
        /// <returns>New password generated for the user</returns>
        /// <remarks>In case user forgets his password the security API gives him a new system generated password.
        /// Set user's logon name, security question and answer properties to correct
        /// values (those matching ones entered at the time of user registration) and then call this method. 
        /// It returns a system generated strong password of minimum length as specified by the password policy
        /// </remarks>
        /// <exception cref="System.ArgumentException">Thrown when this method is called without first setting the 
        /// logon name, security question or answer of the ZentityUser object.</exception>
        /// <example>
        /// <code>
        /// try
        ///    {
        ///        //In case user forgets his password the security API gives him a new system generated password.  
        ///        //User needs to provide security question and answer which he had entered at the time of registration.
        ///        
        ///        //Create an instance of ZentityUser and set logon name, security question and answer.
        ///        ZentityUser user = new ZentityUser { LogOnName = &quot;john&quot; };
        ///        user.SetSecurityQuestion(&quot;What is a bit?&quot;);
        ///        user.SetAnswer(&quot;0 or 1&quot;);
        ///        //Call ForgotPassword method. It returns a new strong password which is of minimum length specified in password policy. 
        ///        string newSystemGeneratedPassword = user.ForgotPassword();
        ///        if (!string.IsNullOrEmpty(newSystemGeneratedPassword))
        ///        {
        ///            Console.WriteLine(&quot;Your new password is {0}&quot;, newSystemGeneratedPassword);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;Errors in generating new password. Please verify that logon name, security question and answer are correct.&quot;);
        ///        }
        ///    }
        ///    catch (AuthenticationException ex)
        ///    {
        ///        //    AuthenticationException may be thrown in case of database errors
        ///        Console.WriteLine(ex.Message);
        ///        //    In case of database errors the AuthenticationException object will wrap the sql exception. 
        ///        if (ex.InnerException != null)
        ///        {
        ///            Console.WriteLine(ex.InnerException.Message);
        ///        }
        ///    }
        ///
        /// </code>
        /// </example>
        public static string ForgotPassword(string logOnName, string securityQuestion, string answer)
        {
            string newPassword = PasswordManager.ForgotPassword(
                                                        logOnName,
                                                        securityQuestion,
                                                        HashingUtility.GenerateHash(answer.ToUpper(CultureInfo.CurrentCulture)));
            return newPassword;
        }
        
        /// <summary>
        /// Registers new user account to database. 
        /// </summary>
        /// <returns>True if user account is successfully created in the database</returns>
        /// <remarks>Set all required properties of the user instance and then call this method.</remarks>
        /// <exception cref="System.ArgumentException">Thrown when any of the required properties of ZentityUser object are not set.</exception>
        /// <exception cref="Zentity.Security.Authentication.AuthenticationException">Thrown if password does not conform to password policy.</exception>
        /// <example>
        /// <code>
        /// try
        ///    {
        ///        ZentityUser newUser = new ZentityUser();
        ///        //Set the mandatory properties of the ZentityUser object.
        ///        newUser.FirstName = &quot;John&quot;;
        ///        newUser.Email = &quot;john@abc.com&quot;;
        ///        newUser.LogOnName = &quot;JohnDE&quot;;
        ///        newUser.SetPassword(&quot;john@123&quot;); //In case of a UI accepting user inputs this call would be newUser.SetPassword(passwordBox1.Password)
        ///        newUser.SetSecurityQuestion(&quot;What is a bit?&quot;);
        ///        newUser.SetAnswer(&quot;0 or 1&quot;);
        ///        //Optional properties - the user can be registered with or without setting these properties.
        ///        newUser.MiddleName = &quot;D&quot;;
        ///        newUser.LastName = &quot;Erickson&quot;;
        ///        newUser.City = &quot;New York&quot;;
        ///        newUser.State = &quot;New York State&quot;;
        ///        newUser.Country = &quot;USA&quot;;
        ///
        ///        //Call the Register() method.
        ///        bool newUserRegistered = newUser.Register();
        ///        if (newUserRegistered)
        ///        {
        ///            Console.WriteLine(&quot;User John registered successfully&quot;);
        ///        }
        ///        else
        ///        {
        ///            //false value might mean the logon name is already in use.
        ///            Console.WriteLine(@&quot;User John could not be registered. The logon name chosen might be already in use. 
        /// Try choosing a different logon name.&quot;);
        ///        }
        ///    }
        ///    //AuthenticationException might be thrown in case of errors in connecting to the authentication store
        ///    //or if the chosen password does not conform to password policy.
        ///    catch (AuthenticationException ex)
        ///    {
        ///        Console.WriteLine(ex.Message);
        ///        //In case of database errors the AuthenticationException object will wrap the sql exception. 
        ///        if (ex.InnerException != null)
        ///        {
        ///            Console.WriteLine(ex.InnerException.Message);
        ///        }
        ///    }
        ///
        /// </code>
        /// </example>
        public bool Register()
        {
            #region Validation
            // Check whether all required properties are set
            ValidateParameters(
                        "FirstName", 
                        this.Profile.FirstName, 
                        "Email", 
                        this.Profile.Email, 
                        "LogOnName", 
                        this.Profile.LogOnName, 
                        "Password",
                        this.Password, 
                        "SecurityQuestion", 
                        this.Profile.SecurityQuestion, 
                        "Answer", 
                        this.Profile.Answer);
            string plainTextPassword = PasswordManager.GetPlainPassword(this.Password);
            if (!PasswordPolicyProvider.CheckPolicyConformance(plainTextPassword))
            {
                throw new AuthenticationException(ConstantStrings.PolicyConformanceExceptionMessage);
            }
            #endregion

            //// Set account status to active
            this.Profile.AccountStatus = "Active";

            //// Save the new user to the database
            bool success = DataAccessLayer.RegisterUser(this);
            return success;
        }

        /// <summary>
        /// Unregisters a user account. LogOnName and password must be set for this method to succeed
        /// </summary>
        /// <returns>True if user is successfully unsubscribed</returns>
        /// <remarks>Set the ZentityUser object's logon name and password to correct values and then call this method.</remarks>
        /// <exception cref="System.ArgumentException">Thrown when this method is called without setting logon name or password.</exception>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to System.IdentityModel, Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// try
        ///    {
        ///        //For unregistering create an instance of ZentityUser with logon name and password set to proper values.
        ///        ZentityUser user = new ZentityUser { LogOnName = &quot;JohnDE&quot; };
        ///        user.SetPassword(&quot;john@123&quot;);
        ///        bool isUserUnregistered = user.Unregister();
        ///        if (isUserUnregistered)
        ///        {
        ///            Console.WriteLine(&quot;User unsubscribed successfully&quot;);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;User could not be unsubscribed. Please verify the logon name / password.&quot;);
        ///        }
        ///    }
        ///    //AuthenticationException might be thrown in case of errors in connecting to the authentication store
        ///    catch (AuthenticationException ex)
        ///    {
        ///        Console.WriteLine(ex.Message);
        ///        //In case of database errors the AuthenticationException object will wrap the sql exception. 
        ///        if (ex.InnerException != null)
        ///        {
        ///            Console.WriteLine(ex.InnerException.Message);
        ///        }
        ///    }
        ///
        /// </code>
        /// Example using AuthenticatedToken instead of password.
        /// <code>
        /// //Get authentication provider from factory
        /// IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// //Login as Jimmy
        /// AuthenticatedToken jimmysToken = provider.Authenticate(new UserNameSecurityToken("Jimmy", "jimmy@123"));
        /// //Jimmy unregisters himself.
        /// ZentityUser jimmy = new ZentityUser { LogOnName = "Jimmy" };
        /// jimmy.Token = jimmysToken;
        /// if (jimmy.Unregister())
        /// {
        ///     Console.WriteLine("Unregistered");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Could not unregister");
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="Zentity.Security.AuthenticationProvider.ZentityAuthenticationProvider"/>
        public bool Unregister()
        {
            if (this.IsAuthenticated())
            {
                bool success = DataAccessLayer.UnregisterUser(this.LogOnName);
                return success;
            }

            return false;
        }

        /// <summary>
        /// Updates a user profile. 
        /// </summary>
        /// <returns>True if user profile is updated successfully</returns>
        /// <remarks>Create a ZentityUser instance, set logon name
        /// and password to correct values. Then set the properties which need modification
        /// Then call UpdateProfile()</remarks>
        /// <exception cref="System.ArgumentException">Thrown when this method is called without first setting logon name and password.</exception>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to System.IdentityModel, Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
        /// if (provider != null)
        /// {
        ///    //Login as John
        ///    UserNameSecurityToken token = new UserNameSecurityToken(&quot;JohnDE&quot;, &quot;john@123&quot;);
        ///    AuthenticatedToken johnsToken = provider.Authenticate(token);
        ///
        ///    //John requests his profile and updates email.
        ///    ZentityUser john = ZentityUserManager.GetUserProfile(johnsToken);
        ///    john.LogOnName = &quot;JohnDE&quot;;
        ///    john.Token = johnsToken;
        ///    //Now update email.
        ///    john.Email = &quot;john@sdf.com&quot;;
        ///    bool isProfileUpdated = john.UpdateProfile();
        ///    if (isProfileUpdated)
        ///    {
        ///        Console.WriteLine(&quot;User profile updated successfully.&quot;);
        ///    }
        ///    else
        ///    {
        ///        Console.WriteLine(&quot;User profile could not be updated&quot;);
        ///    }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="Zentity.Security.AuthenticationProvider.ZentityAuthenticationProvider"/>
        public bool UpdateProfile()
        {
            //// Validation
            ValidateParameters(
                            "Email", 
                            this.Profile.Email, 
                            "LogOnName", 
                            this.Profile.LogOnName, 
                            "SecurityQuestion", 
                            this.Profile.SecurityQuestion,
                            "Answer", 
                            this.Profile.Answer);

            if (this.IsAuthenticated())
            {
                bool success = DataAccessLayer.UpdateProfile(this);
                return success;
            }

            return false;
        }

        /// <summary>
        /// Updates a user logon name. 
        /// </summary>
        /// <param name="newLogOn">New logon name. This name should not be already in use.</param>
        /// <returns>True if user logon name is updated successfully</returns>
        /// <remarks>Set logon name, password of the ZentityUser object and then call this method.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when newLogOn parameter is null or empty</exception>
        /// <exception cref="System.ArgumentException">Thrown when this method is called without first setting the 
        /// logon name and password of the ZentityUser object.</exception>
        /// <example>
        /// <code>
        /// try
        ///    {
        ///        //For updating the logon name create an instance of ZentityUser and set logon name and password. 
        ///        //Then call the UpdateLogOnName method.
        ///        ZentityUser user = new ZentityUser { LogOnName = &quot;JohnDE&quot; };
        ///        user.SetPassword(&quot;john@123&quot;);
        ///        bool isLogOnUpdated = user.UpdateLogOnName(&quot;john&quot;);
        ///        if (isLogOnUpdated)
        ///        {
        ///            Console.WriteLine(&quot;LogOn updated&quot;);
        ///        }
        ///        else
        ///        {
        ///            //LogOn name might not be updated if the new logon chosen is already in use.
        ///            Console.WriteLine(&quot;Errors in updating logon. The logon name might be in use. Try choosing a different logon name.&quot;);
        ///        }
        ///    }
        ///
        /// </code>
        /// </example>
        public bool UpdateLogOnName(string newLogOn)
        {
            //// Validations
            ValidateParameters("newLogOn", newLogOn);

            if (this.IsAuthenticated())
            {
                if (ZentityUserProfile.ValidateLogOnName(newLogOn))
                {
                    bool success = DataAccessLayer.UpdateLogOnName(this.LogOnName, newLogOn);
                    return success;
                }
            }

            return false;
        }

        /// <summary>
        /// Changes user password if the current password is verified to be correct
        /// </summary>
        /// <param name="newPassword">New password</param>
        /// <returns>True if user password is changed successfully</returns>
        /// <remarks>Set logon name, password of the ZentityUser object to correct values and then call this method.</remarks>
        /// <exception cref="System.ArgumentException">Thrown when this method is called without first setting the 
        /// logon name and password of the ZentityUser object.</exception>
        /// <exception cref="Zentity.Security.Authentication.AuthenticationException">Thrown when new password does not conform to password policy.</exception>
        /// <example>
        /// <code>
        /// try
        ///    {
        ///        //For changing password create an instance of ZentityUser and set logon name and password.
        ///        //Then call ChangePassword method with new password as the parameter.
        ///        ZentityUser user = new ZentityUser { LogOnName = &quot;john&quot; };
        ///        user.SetPassword(&quot;john@123&quot;); //In case of UI accepting user inputs this call would be something like user.SetPassword(passwordBox1.Password);
        ///        bool isPasswordChanged = user.ChangePassword(&quot;john!@#4&quot;);
        ///        if (isPasswordChanged)
        ///        {
        ///            Console.WriteLine(&quot;Password changed&quot;);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;Errors while changing password. Please verify whether the logon name and current password are correct.&quot;);
        ///        }
        ///    }
        ///    catch (AuthenticationException ex)
        ///    {
        ///        //AuthenticationException may be thrown in case of database errors, or if new password does not conform to password policy.
        ///        Console.WriteLine(ex.Message);
        ///        //In case of database errors the AuthenticationException object will wrap the sql exception. 
        ///        if (ex.InnerException != null)
        ///        {
        ///            Console.WriteLine(ex.InnerException.Message);
        ///        }
        ///    }
        ///
        /// </code>
        /// </example>
        public bool ChangePassword(string newPassword)
        {
            #region Validations
            ValidateParameters("newPassword", newPassword);
            #endregion

            if (PasswordPolicyProvider.CheckPolicyConformance(newPassword))
            {
                bool success = PasswordManager.ChangePassword(this.LogOnName, this.Password, newPassword);
                return success;
            }
            else
            {
                throw new AuthenticationException(ConstantStrings.PolicyConformanceExceptionMessage);
            }
        }

        /// <summary>
        ///  Returns user id
        /// </summary>
        /// <returns>user id</returns>
        /// <remarks>Retrieves user id from the authentication store and returns. Set the ZentityUser's logon name and password
        /// to correct values before calling this method.</remarks>
        /// <exception cref="System.ArgumentException">Thrown when this method is called without setting LogOnName and Password</exception>
        /// <example>
        /// <code>
        /// try
        ///    {
        ///        //Create an instance of ZentityUser and set LogOnName and Password. Then call GetUserId
        ///        ZentityUser user = new ZentityUser { LogOnName = &quot;john&quot; };
        ///        user.SetPassword(&quot;john!@#4&quot;); //for a UI sample this call would be similar to user.SetPassword(passwordBox1.Password)
        ///        Guid id = user.GetUserId();
        ///        if (id != Guid.Empty)
        ///        {
        ///            Console.WriteLine(&quot;User id = {0}&quot;, id);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;Id could not be retrieved. Please verify that the user credentials are correct&quot;);
        ///        }
        ///    }
        ///    catch (AuthenticationException ex)
        ///    {
        ///        //    AuthenticationException may be thrown in case of database errors
        ///        Console.WriteLine(ex.Message);
        ///        //    In case of database errors the AuthenticationException object will wrap the sql exception. 
        ///        if (ex.InnerException != null)
        ///        {
        ///            Console.WriteLine(ex.InnerException.Message);
        ///        }
        ///    }
        ///
        /// </code>
        /// </example>
        public Guid GetUserId()
        {
            if (this.IsAuthenticated())
            {
                Guid id = DataAccessLayer.GetUserId(this.LogOnName);
                return id;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Returns number of days remaining before the password expires
        /// </summary>
        /// <returns>Remaining number of days for password expiry</returns>
        /// <remarks>
        /// This method tells the days remaining for password expiry. It is useful for showing reminder message to the user when 
        /// his password is about to expire in a few days.
        /// For showing reminders call this method right after authenticating user. 
        /// Set the ZentityUser's logon name and password to correct values before calling this method</remarks>
        /// <exception cref="System.ArgumentException">Thrown when this method is called without setting logon name and password
        /// of the ZentityUser object.</exception>
        /// <example>
        /// <code>
        /// using System;
        ///    using System.Collections.Generic;
        ///    using System.Linq;
        ///    using System.Text;
        ///    using Zentity.Security.Authentication;
        ///    using System.IdentityModel.Tokens;
        ///    using Zentity.Security.AuthenticationProvider;
        ///
        ///    namespace Zentity.Security.Authentication.Samples
        ///    {
        ///        class Program
        ///        {
        ///            static void Main(string[] args)
        ///            {
        ///                try
        ///                {
        ///                    //This method returns the number of days remaining for password expiry of the user, as per the 
        ///                    //password policy.
        ///                    //Typically useful for showing reminder message to the user when he logs in. 
        ///
        ///                    //This sample illustrates the idea
        ///                    //Authenticate the user
        ///
        ///                    UserNameSecurityToken userCredentials = ReadUserCredentials();
        ///                    AuthenticatedToken userToken = AuthenticateUser(userCredentials);
        ///                    if (userToken != null)
        ///                    {
        ///                        Console.WriteLine(&quot;You have logged in successfully&quot;);
        ///                        //Create a ZentityUser object and set logon name and password.
        ///                        ZentityUser user = new ZentityUser();
        ///                        user.LogOnName = userCredentials.UserName;
        ///                        user.SetPassword(userCredentials.Password);
        ///                        //Retrieve number of days remaining for password expiry
        ///                        int? passwordExpiresIn = user.GetRemainingDaysToPasswordExpiry();
        ///                        if (passwordExpiresIn != null) //it can be null in case of errors reading password policy.
        ///                        {
        ///                            //If the password is about to expire in 7 days or less, ask user to change the password.
        ///                            if (passwordExpiresIn &lt;= 7)
        ///                            {
        ///                                Console.WriteLine(&quot;Your password expires in {0} days. Do you want to change it now? [y/n]&quot;, passwordExpiresIn);
        ///                                ConsoleKeyInfo keyInfo = Console.ReadKey();
        ///                                char ch = keyInfo.KeyChar;
        ///                                if (ch == 'y' || ch == 'Y')
        ///                                {
        ///                                    ChangeUserPassword(user);
        ///                                }
        ///                            }
        ///                            //else no need to ask user to change the password.
        ///                        }
        ///                        else
        ///                        {
        ///                            Console.WriteLine(&quot;Could not retrive password expiry information.&quot;);
        ///                        }
        ///                    }
        ///                    else
        ///                    {
        ///                        Console.WriteLine(&quot;User is not authenticated. Please verify the credentials provided.&quot;);
        ///                    }
        ///               }
        ///                catch (AuthenticationException ex)
        ///                {
        ///                    //    AuthenticationException may be thrown in case of database errors
        ///                    Console.WriteLine(ex.Message);
        ///                    //    In case of database errors the AuthenticationException object will wrap the sql exception. 
        ///                    if (ex.InnerException != null)
        ///                    {
        ///                        Console.WriteLine(ex.InnerException.Message);
        ///                    }
        ///                }
        ///            }
        ///
        ///            //Changes user password to new password entered by the user on console.
        ///            private static void ChangeUserPassword(ZentityUser user)
        ///            {
        ///                Console.WriteLine(&quot;Enter new password and press &lt;Enter&gt; : &quot;);
        ///                string newPassword = Console.ReadLine();
        ///                bool passwordChanged = user.ChangePassword(newPassword);
        ///                if (passwordChanged)
        ///                {
        ///                    Console.WriteLine(&quot;Your password is changed.&quot;);
        ///                }
        ///                else
        ///                {
        ///                    Console.WriteLine(&quot;Errors in changing password.&quot;);
        ///                }
        ///            }
        ///
        ///            //Creates an AuthenticationProvider instance and calls authenticate() method, passing userCredentials as parameter.
        ///            private static AuthenticatedToken AuthenticateUser(UserNameSecurityToken userCredentials)
        ///            {
        ///                IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
        ///                AuthenticatedToken userToken = provider.Authenticate(userCredentials);
        ///                return userToken;
        ///            }
        ///
        ///            //Reads username and password from console.
        ///            private static UserNameSecurityToken ReadUserCredentials()
        ///            {
        ///                Console.WriteLine(&quot;Enter logon name and press &lt;Enter&gt; : &quot;);
        ///                string userName = Console.ReadLine();
        ///                Console.WriteLine(&quot;Enter password and press &lt;Enter&gt; : &quot;);
        ///                string password = Console.ReadLine();
        ///                UserNameSecurityToken userCredentials = new UserNameSecurityToken(userName, password);
        ///                return userCredentials;
        ///            }
        ///        }
        ///    }
        ///
        /// </code>
        /// </example>
        public int? GetRemainingDaysToPasswordExpiry()
        {
            if (this.IsAuthenticated())
            {
                return PasswordManager.PasswordExpiresInDays(this.LogOnName);
            }

            return null;
        }

        /// <summary>
        /// Retrieves user information for the given user from authentication store and sets the property values 
        /// with values retrieved from database
        /// </summary>
        /// <remarks>Set the ZentityUser's logon name and password to correct values before calling this method</remarks>
        /// <exception cref="System.ArgumentException">Thrown when this method is called without setting LogOnName and Password</exception>
        /// <example>
        /// <code>
        /// try
        ///    {
        ///        ZentityUser user = new ZentityUser { LogOnName = &quot;JohnDE&quot; };
        ///        user.SetPassword(&quot;john@123&quot;);
        ///        user.FillUserProperties();
        ///        Console.WriteLine(&quot;FirstName : {0}&quot;, user.FirstName);
        ///        Console.WriteLine(&quot;MiddleName : {0}&quot;, user.MiddleName);
        ///        Console.WriteLine(&quot;LastName : {0}&quot;, user.LastName);
        ///        Console.WriteLine(&quot;Email : {0}&quot;, user.Email);
        ///        Console.WriteLine(&quot;City : {0}&quot;, user.City);
        ///        Console.WriteLine(&quot;State : {0}&quot;, user.State);
        ///        Console.WriteLine(&quot;Country : {0}&quot;, user.Country);
        ///        Console.WriteLine(&quot;AccountStatus : {0}&quot;, user.AccountStatus);
        ///        Console.WriteLine(&quot;Account Creation Date : {0}&quot;, user.DateCreated);
        ///        Console.WriteLine(&quot;Account Modification Date : {0}&quot;, user.DateModified);
        ///        Console.WriteLine(&quot;Password Creation Date : {0}&quot;, user.PasswordCreationDate);
        ///    }
        ///    catch (AuthenticationException ex)
        ///    {
        ///        Console.WriteLine(ex.Message);
        ///        if (ex.InnerException != null)
        ///        {
        ///            Console.WriteLine(ex.InnerException.Message);
        ///        }
        ///    }
        ///
        /// </code>
        /// </example>
        public void FillUserProperties()
        {
            if (this.IsAuthenticated())
            {
                ZentityUserProfile profile = DataAccessLayer.GetUserProfile(this.LogOnName);
                this.Profile = profile;
            }
        }
        
        #endregion

        #region Internal methods

        /// <summary>
        /// Verifies password - authenticates user.
        /// </summary>
        /// <returns>True if LogOnName and password are correct.</returns>
        internal bool VerifyPassword()
        {
            //// Validation
            ValidateParameters("LogOnName", this.LogOnName, "Password", this.Password);

            bool verified = PasswordManager.VerifyPassword(this.LogOnName, this.Password);
            return verified;
        }

        #endregion

        #region Private methods

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
                        throw new ArgumentException(string.Format(
                                                                CultureInfo.CurrentUICulture,
                                                                ConstantStrings.MandatoryPropertyNotSetExceptionMessage,
                                                                args[i - 1]));
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether the user instance is authenticated - either the AuthenticatedToken is valid
        /// or the username and password are valid.
        /// </summary>
        /// <returns>System.Boolean; <c>true</c> if authenticated, <c>false</c> otherwise</returns>
        private bool IsAuthenticated()
        {
            //// The token should be valid - and it should be of the user whose record is to be updated
            //// or it should be of the administrator.
            if (this.token != null)
            {
                return this.ValidateToken(this.token);
            }
            else if (!string.IsNullOrEmpty(this.LogOnName) && !string.IsNullOrEmpty(this.Password))
            {
                //// The logon name and password must be verified by the password manager.
                return this.VerifyPassword();
            }

            return false;
        }

        private bool ValidateToken(AuthenticatedToken token)
        {
            bool valid = token.Validate();
            if (valid)
            {
                if (string.Equals(token.IdentityName, this.LogOnName, StringComparison.OrdinalIgnoreCase)
                    || DataAccessLayer.IsAdmin(token.IdentityName))
                {
                    return true;
                }
            }

            return false;
        }
       
        #endregion
    }
}
