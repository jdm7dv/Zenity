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
    /// This class provides API for user management like user registration, profile 
    /// management and password management
    /// </summary>
    public static class ZentityUserManager
    {
        #region constants
        private const string Active = "Active";
        private const string Inactive = "Inactive";
        #endregion

        /// <summary>
        /// Registers a new user to Zentity. 
        /// Set all required properties of ZentityUser instance and then call Register to 
        /// create the user account
        /// </summary>
        /// <param name="newUser">ZentityUser instance with all mandatory properties filled in</param>
        /// <returns>True if user account created successfully</returns>
        /// <exception cref="System.ArgumentException">Thrown when ZentityUser object is not filled in with all required property values.</exception>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the authentication database script (present in Program Files\Zentity 1.0\Scripts folder)</item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
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
        ///        bool registered = ZentityUserManager.Register(newUser);
        ///        if (registered)
        ///        {
        ///            Console.WriteLine(&quot;User John registered successfully&quot;);
        ///        }
        ///        else
        ///        {
        ///            //false value might mean the logon name is already in use.
        ///            Console.WriteLine(@&quot;User John could not be registered. The logon name chosen might be already in use. 
        ///    Try choosing a different logon name.&quot;);
        ///        }
        ///    }
        ///    //AuthenticationException might be thrown in case of errors in connecting to the authentication store
        ///    //or if admin credentials are incorrect.
        ///    catch (AuthenticationException ex)
        ///    {
        ///        Console.WriteLine(ex.Message);
        ///        //In case of database errors the AuthenticationException object will wrap the sql exception. 
        ///        if (ex.InnerException != null)
        ///        {
        ///            Console.WriteLine(ex.InnerException.Message);
        ///        }
        ///    }
        /// </code>
        /// </example>
        public static bool Register(ZentityUser newUser)
        {
            #region Input Validation
            if (newUser == null)
            {
                throw new ArgumentNullException("newUser");
            }
            #endregion

            return newUser.Register();
        }

        #region Unregister overloads
        
        /// <summary>
        /// Unregisters a user account.
        /// </summary>
        /// <param name="logOnName">LogOnName of the user</param>
        /// <param name="password">Password of the user account</param>
        /// <returns>True if user is successfully unsubscribed</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// try
        ///    {
        ///        //Call unregister method passing user credentials
        ///        bool isUserUnregistered = ZentityUserManager.Unregister(&quot;JohnDE&quot;, &quot;john@123&quot;);
        ///        if (isUserUnregistered)
        ///        {
        ///            Console.WriteLine(&quot;User unregistered successfully&quot;);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;User could not be unregistered. Please verify the logon name / password.&quot;);
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
        /// </example>
        public static bool Unregister(string logOnName, string password)
        {
            #region Input Validation
            ValidateParameters("logOnName", logOnName, "password", password);
            #endregion

            try
            {
                ZentityUser user = new ZentityUser(logOnName, password);
                bool success = user.Unregister();
                return success;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Unregisters a user. An admin can unregister any user, while a user can request unregistration for himself.
        /// </summary>
        /// <param name="token">Authenticated token of the logged on user.</param>
        /// <returns>True if unregister operation completes successfully.</returns>
        /// <remarks>This method should be called with the token for a user who is logged on currently and wants to unregister himself.</remarks>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to System.IdentityModel, Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// //Get authentication provider from factory
        /// IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
        /// //Login as Jimmy
        /// AuthenticatedToken jimmysToken = provider.Authenticate(new UserNameSecurityToken(&quot;Jimmy&quot;, &quot;jimmy@123&quot;));
        /// //Jimmy unregisters from the Zentity site.
        /// bool unregistered = ZentityUserManager.Unregister(jimmysToken);
        /// if (unregistered)
        /// {
        ///     Console.WriteLine(&quot;Unregistered successfully&quot;);
        /// }
        /// else
        /// {
        ///     Console.WriteLine(&quot;Could not unregister.&quot;);
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="Zentity.Security.AuthenticationProvider.ZentityAuthenticationProvider"/>
        public static bool Unregister(AuthenticatedToken token)
        {
            #region Input Validation
            ValidateToken(token);
            #endregion
            return Unregister(token, token.IdentityName);
        }

        /// <summary>
        /// Unregisters a user. An admin can unregister any user, while a user can request unregistration for himself.
        /// </summary>
        /// <param name="token">Authenticated token of the logged on user.</param>
        /// <param name="logOnName">LogOn name of the user whose account is to be unregistered.</param>
        /// <returns>True if unregister operation completes successfully.</returns>
        /// <remarks>This method succeeds for any user's logon name when an admin token is passed. 
        /// When a user token is passed, the logon name must be of the same user. Or else call the overload with just 
        /// AuthenticatedToken parameter for unregistering a user. </remarks>
        public static bool Unregister(AuthenticatedToken token, string logOnName)
        {
            #region Input Validation
            ValidateToken(token);
            ValidateParameters("logOnName", logOnName);
            #endregion

            try
            {
                //// The token must be of the admin or the user who wants to unregister.
                if (!string.Equals(token.IdentityName, logOnName, StringComparison.OrdinalIgnoreCase)
                    && !DataAccessLayer.IsAdmin(token.IdentityName))
                {
                    return false;
                }

                ZentityUser user = new ZentityUser(logOnName, token);
                bool success = user.Unregister();
                return success;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        #endregion

        /// <summary>
        /// Updates a user profile. Create a ZentityUser instance, set login name
        /// and password to correct values. Then set the properties which need modification
        /// Then call UpdateProfile()
        /// </summary>
        /// <param name="user">ZentityUser object filled in with logon name, password and 
        /// properties which are to be updated</param>
        /// <returns>True if user profile is updated successfully</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to System.IdentityModel, Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// ZentityUser john = ZentityUserManager.GetUserProfile(&quot;JohnDE&quot;, &quot;john@123&quot;);
        /// john.LogOnName = &quot;JohnDE&quot;;
        /// john.SetPassword(&quot;john@123&quot;);
        /// //Update security question and email
        /// john.SetSecurityQuestion(&quot;Define Bit&quot;);
        /// john.Email = &quot;john@pqr.com&quot;;
        /// bool updated = ZentityUserManager.UpdateProfile(john);
        /// </code>
        /// </example>
        public static bool UpdateProfile(ZentityUser user)
        {
            #region Validation
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            #endregion

            return user.UpdateProfile();
        }

        #region UpdateLogOnName overloads
        
        /// <summary>
        /// Updates a user logon name. 
        /// </summary>
        /// <param name="currentLogOn">Current log on name</param>
        /// <param name="newLogOn">New log on name</param>
        /// <param name="password">User Password</param>
        /// <returns>True if user logon name is updated successfully</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// try
        ///    {
        ///        bool isLogOnUpdated = ZentityUserManager.UpdateLogOnName(&quot;JohnDE&quot;, &quot;john&quot;, &quot;john@123&quot;);
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
        public static bool UpdateLogOnName(string currentLogOn, string newLogOn, string password)
        {
            #region Input Validation
            ValidateParameters("currentLogOn", currentLogOn, "newLogOn", newLogOn, "password", password);
            #endregion

            try
            {
                ZentityUser user = new ZentityUser(currentLogOn, password);
                bool success = user.UpdateLogOnName(newLogOn);
                return success;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Updates log on name of the current logged on user.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <param name="newLogOn">New log on name.</param>
        /// <returns>True if operation succeeds.</returns>
        public static bool UpdateLogOnName(AuthenticatedToken token, string newLogOn)
        {
            #region Parameter validation
            ValidateToken(token);
            ValidateParameters("newLogOn", newLogOn);
            #endregion

            return UpdateLogOnName(token, token.IdentityName, newLogOn);
        }

        /// <summary>
        /// Updates log on name of the current logged on user.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <param name="currentLogOn">Current logon name of the user whose logon name is to be updated.</param>
        /// <param name="newLogOn">New log on name.</param>
        /// <returns>True if operation succeeds.</returns>
        /// <remarks>An administrator can update any user's log on name. Other users can update only their own logon name. 
        /// Hence for non-admin users the current log on must match the logged on user's log on name.</remarks>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to System.IdentityModel, Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// //Get authentication provider from factory
        /// IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
        /// //Login as admin
        /// AuthenticatedToken adminToken = provider.Authenticate(new UserNameSecurityToken(&quot;Administrator&quot;, &quot;XXXX&quot;));//Supply correct password
        /// //Administrator changes Jimmy's log on name.
        /// bool updated = ZentityUserManager.UpdateLogOnName(adminToken, &quot;Jimmy&quot;, &quot;Jimmy12&quot;);
        /// if (updated)
        /// {
        ///     Console.WriteLine(&quot;Updated log on name successfully&quot;);
        /// }
        /// else
        /// {
        ///     Console.WriteLine(&quot;Could not update log on name.&quot;);
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="Zentity.Security.AuthenticationProvider.ZentityAuthenticationProvider"/>
        public static bool UpdateLogOnName(AuthenticatedToken token, string currentLogOn, string newLogOn)
        {
            #region Input Validation
            ValidateToken(token);
            ValidateParameters("currentLogOn", currentLogOn, "newLogOn", newLogOn);
            #endregion

            try
            {
                //// The token must be of the user who is updating the logon name or it must be an admin's token.
                if (!string.Equals(token.IdentityName, currentLogOn, StringComparison.OrdinalIgnoreCase)
                    && !DataAccessLayer.IsAdmin(token.IdentityName))
                {
                    return false;
                }

                ZentityUser user = new ZentityUser(currentLogOn, token);
                bool success = user.UpdateLogOnName(newLogOn);
                return success;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        #endregion

        /// <summary>
        /// Changes user password if the current password is verified to be correct
        /// </summary>
        /// <param name="logOnName">LogOn name of the user</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if user password is changed successfully</returns>'
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// try
        ///    {
        ///        bool passwordChanged = ZentityUserManager.ChangePassword(&quot;JohnDE&quot;, &quot;john@123&quot;, &quot;john@12345&quot;);
        ///        if (passwordChanged)
        ///        {
        ///            Console.WriteLine(&quot;Password changed&quot;);
        ///        }
        ///        else
        ///        {
        ///            //LogOn name might not be updated if the new logon chosen is already in use.
        ///            Console.WriteLine(&quot;Errors in changing password. Current credentials may be incorrect.&quot;);
        ///        }
        ///    }
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
        public static bool ChangePassword(string logOnName, string currentPassword, string newPassword)
        {
            #region Input Validation
            ValidateParameters("logOnName", logOnName, "currentPassword", currentPassword, "newPassword", newPassword);
            #endregion

            try
            {
                ZentityUser user = new ZentityUser(logOnName, currentPassword);
                bool success = user.ChangePassword(newPassword);
                return success;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Generates a new password for the user in case he forgets the password
        /// </summary>
        /// <param name="logOnName">Login name of the user</param>
        /// <param name="securityQuestion">Security question selected by the user</param>
        /// <param name="answer">User's answer to the security question</param>
        /// <returns>New password generated for the user</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// try
        ///    {
        ///        //For getting a new password you need to provide the security question and answer
        ///        //entered at the time of registration.
        ///        string newSystemGeneratedPassword = ZentityUserManager.ForgotPassword(&quot;JohnDE&quot;, &quot;What is a bit?&quot;, &quot;0 or 1&quot;);
        ///        if (!string.IsNullOrEmpty(newSystemGeneratedPassword))
        ///        {
        ///            Console.WriteLine(&quot;New password generated is {0}&quot;, newSystemGeneratedPassword);
        ///            //Try logging in using new password - refer to ZentityAuthenticationProvider.Authenticate() method documentation
        ///        }
        ///        else
        ///        {
        ///            //LogOn name might not be updated if the new logon chosen is already in use.
        ///            Console.WriteLine(&quot;Errors in getting a new password. Security question / answer might be incorrect.&quot;);
        ///        }
        ///    }
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
        public static string ForgotPassword(string logOnName, string securityQuestion, string answer)
        {
            #region Input Validation
            ValidateParameters("logOnName", logOnName, "securityQuestion", securityQuestion, "answer", answer);
            #endregion

            try
            {
                string newPassword = ZentityUser.ForgotPassword(logOnName, securityQuestion, answer);
                return newPassword;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        #region GetUserId overloads
        
        /// <summary>
        ///  Returns user id
        /// </summary>
        /// <param name="logOnName">Log on name of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>user id</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// try
        ///    {
        ///        Guid userId = ZentityUserManager.GetUserId(&quot;JohnDE&quot;, &quot;john@123&quot;);
        ///        if (userId != Guid.Empty)
        ///        {
        ///            Console.WriteLine(&quot;Guid is {0}&quot;, userId);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;Guid could not be retrieved. Please check the user credentials&quot;);
        ///        }
        ///    }
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
        public static Guid GetUserId(string logOnName, string password)
        {
            #region Input Validation
            ValidateParameters("logOnName", logOnName, "password", password);
            #endregion

            try
            {
                ZentityUser user = new ZentityUser(logOnName, password);
                Guid id = user.GetUserId();
                return id;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Returns GUID of the logged on user.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <returns>Id of the user represented by the token.</returns>
        public static Guid GetUserId(AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateToken(token);
            #endregion

            return GetUserId(token, token.IdentityName);
        }

        /// <summary>
        /// Returns user id of the user represented by 'logOnName'.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the admin, in case id of any user is to be retrieved. </param>
        /// <param name="logOnName">LogOnName of the user whose id is to be returned.</param>
        /// <returns>Id of the user represented by the token.</returns>
        /// <remarks>Call this method when an admin wants to retrieve id of any user. For a non-admin user who wants to 
        /// retrieve his id, call other overload with just token parameter.</remarks>
        /// <example>
        /// <code>
        /// //Get authentication provider from factory
        /// IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
        /// //Login as admin
        /// AuthenticatedToken adminToken = provider.Authenticate(new UserNameSecurityToken(&quot;Administrator&quot;, &quot;XXXX&quot;));//Supply correct password
        /// //Admin retrieves Jimmy's id
        /// Guid id = ZentityUserManager.GetUserId(adminToken, &quot;Jimmy&quot;);
        /// Console.WriteLine(&quot;id = {0}&quot;, id);
        /// </code>
        /// </example>
        /// <seealso cref="Zentity.Security.AuthenticationProvider.ZentityAuthenticationProvider"/>
        public static Guid GetUserId(AuthenticatedToken token, string logOnName)
        {
            //// Input Validation
            ValidateToken(token);
            ValidateParameters("logOnName", logOnName);

            try
            {
                //// The token must be of the user who is updating the logon name or it must be an admin's token.
                if (!string.Equals(token.IdentityName, logOnName, StringComparison.OrdinalIgnoreCase)
                    && !DataAccessLayer.IsAdmin(token.IdentityName))
                {
                    return Guid.Empty;
                }

                ZentityUser user = new ZentityUser(logOnName, token);
                Guid id = user.GetUserId();
                return id;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        #endregion

        #region GetRemainingDaysToPasswordExpiry overloads

        /// <summary>
        /// Returns number of days remaining before the password expires. 
        /// </summary>
        /// <param name="logOnName">Logon name of the user</param>
        /// <param name="password">User password</param>
        /// <returns>Remaining days for password expiry. </returns>
        /// <remarks>Useful for displaying warning to the user that he needs to change his password in n days</remarks>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// For a complete sample please refer to ZentityUser.GetRemainingDaysToPasswordExpiry() documentation
        /// <code>
        /// try
        ///    {
        ///        //This method returns the number of days remaining for password expiry of the user, as per the 
        ///        //password policy.
        ///        //Typically useful for showing reminder message to the user when he logs in. 
        ///        int? remainingDays = ZentityUserManager.GetRemainingDaysToPasswordExpiry(&quot;JohnDE&quot;, &quot;john@123&quot;);
        ///        Console.WriteLine(&quot;Your password expires in {0} days&quot;, remainingDays);
        ///    }
        ///    catch (AuthenticationException ex)
        ///    {
        ///        Console.WriteLine(ex.Message);
        ///        //In case of database errors the AuthenticationException object will wrap the sql exception. 
        ///        if (ex.InnerException != null)
        ///        {
        ///            Console.WriteLine(ex.InnerException.Message);
        ///        }
        ///    }
        /// </code>
        /// </example>
        /// <seealso cref="ZentityUser.GetRemainingDaysToPasswordExpiry"/>
        public static int? GetRemainingDaysToPasswordExpiry(string logOnName, string password)
        {
            #region Input Validation
            ValidateParameters("logOnName", logOnName, "password", password);
            #endregion

            try
            {
                ZentityUser user = new ZentityUser(logOnName, password);
                int? remainingDays = user.GetRemainingDaysToPasswordExpiry();
                return remainingDays;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Returns number of days remaining for password expiry for the logged on user.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <returns>Remaining days, if operation succeeds, null otherwise. It will return -1 if the password policy is not being applied.</returns>
        /// <remarks>Configuration setting 'ApplyPasswordPolicy' decides whether password policy will be enforced or not.</remarks>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to System.IdentityModel, Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// //Get authentication provider from factory
        /// IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
        /// //Login as Jimmy
        /// AuthenticatedToken jimmysToken = provider.Authenticate(new UserNameSecurityToken(&quot;Jimmy&quot;, &quot;jimmy@123&quot;));
        /// //Jimmy retrieves remaining days for password expiry. This will require password policy to be enforced in the application configuration file.
        /// int? days = ZentityUserManager.GetRemainingDaysToPasswordExpiry(jimmysToken);
        /// Console.WriteLine(&quot;Jimmy's password expires in {0} days&quot;, days);
        /// </code>
        /// </example>
        /// <seealso cref="Zentity.Security.AuthenticationProvider.ZentityAuthenticationProvider"/>
        public static int? GetRemainingDaysToPasswordExpiry(AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateToken(token);
            #endregion

            return GetRemainingDaysToPasswordExpiry(token, token.IdentityName);
        }

        /// <summary>
        /// Returns number of days remaining for password expiry for the user represented by logOnName.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the logged on user, typically an admin.</param>
        /// <param name="logOnName">LogOnName of the user whose information is requested.</param>
        /// <returns>Remaining days, if operation succeeds, null otherwise.</returns>
        /// <remarks>Call this method when an admin wants information for a user. For a user requesting his information, 
        /// call the overload with just AuthenticatedToken parameter.</remarks>
        public static int? GetRemainingDaysToPasswordExpiry(AuthenticatedToken token, string logOnName)
        {
            //// Input Validation
            ValidateToken(token);
            ValidateParameters("logOnName", logOnName);

            try
            {
                //// The token must be of the user who is updating the logon name or it must be an admin's token.
                if (!string.Equals(token.IdentityName, logOnName, StringComparison.OrdinalIgnoreCase)
                    && !DataAccessLayer.IsAdmin(token.IdentityName))
                {
                    return null;
                }

                ZentityUser user = new ZentityUser(logOnName, token);
                int? remainingDays = user.GetRemainingDaysToPasswordExpiry();
                return remainingDays;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        #endregion

        #region GetUserProfile overloads
        
        /// <summary>
        /// Retrieves user information for the given user
        /// </summary>
        /// <param name="logOnName">LogOn Name of the user whose information is to be retrieved</param>
        /// <param name="password">User password</param>
        /// <returns>ZentityUser instance in which property values are filled in from database</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// try
        ///    {
        ///        ZentityUser user = ZentityUserManager.GetUserProfile(&quot;JohnDE&quot;, &quot;john@123&quot;);
        ///        //The ZentityUser instance returned contains all property values filled in from the authentication store.
        ///        if (user != null)
        ///        {
        ///            Console.WriteLine(&quot;Email : {0}&quot;, user.Email);
        ///            //Display remaining properties.
        ///        }
        ///    }
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
        public static ZentityUserProfile GetUserProfile(string logOnName, string password)
        {
            #region Input Validation
            ValidateParameters("logOnName", logOnName, "password", password);
            #endregion

            try
            {
                ZentityUser user = new ZentityUser(logOnName, password);
                user.FillUserProperties();
                return user.Profile;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Returns profile of the current logged on user. 
        /// </summary>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <returns>ZentityUser instance filled with user properties.</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to System.IdentityModel, Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing inputs with valid values</item>
        /// </list>
        /// <code>
        /// //Get authentication provider from factory
        /// IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
        /// //Login as Jimmy
        /// AuthenticatedToken jimmysToken = provider.Authenticate(new UserNameSecurityToken(&quot;Jimmy&quot;, &quot;jimmy@123&quot;));
        /// //Jimmy retrieves his profile.
        /// ZentityUser jimmy = ZentityUserManager.GetUserProfile(jimmysToken);
        /// Console.WriteLine(&quot;Jimmy's email is {0} &quot;, jimmy.Email);
        /// </code>
        /// </example>
        /// <seealso cref="Zentity.Security.AuthenticationProvider.ZentityAuthenticationProvider"/>
        public static ZentityUserProfile GetUserProfile(AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateToken(token);
            #endregion

            return GetUserProfile(token, token.IdentityName);
        }

        /// <summary>
        /// Returns the user profile. An admin can get any user's profile. 
        /// </summary>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <param name="logOnName">LogOnName of the user whose profile is requested.</param>
        /// <returns>ZentityUser instance filled with properties.</returns>
        /// <remarks>This method is expected to be called for the scenario where the administrator is requesting profile of a user. 
        /// This method works when the AuthenticatedToken belongs to the administrator, or when the token 
        /// belongs to the user whose logon name is passed as the second parameter.</remarks>
        public static ZentityUserProfile GetUserProfile(AuthenticatedToken token, string logOnName)
        {
            #region Input Validation
            ValidateToken(token);
            #endregion

            try
            {
                //// The token must be of the user who is updating the logon name or it must be an admin's token.
                if (!string.Equals(token.IdentityName, logOnName, StringComparison.OrdinalIgnoreCase)
                    && !DataAccessLayer.IsAdmin(token.IdentityName))
                {
                    return null;
                }

                ZentityUser user = new ZentityUser(logOnName, token);
                user.FillUserProperties();
                return user.Profile;
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        #endregion

        /// <summary>
        /// Checks whether the log on name selected by a new user is available 
        /// </summary>
        /// <param name="name">LogOnName selected by the new user</param>
        /// <returns>True if log on name is not already in use.</returns>
        public static bool IsLogOnNameAvailable(string name)
        {
            //// Input Validation
            ValidateParameters("name", name);

            bool isAvailable = DataAccessLayer.IsLogOnNameAvailable(name);
            return isAvailable;
        }

        /// <summary>
        /// Returns password.
        /// </summary>
        /// <param name="logOnName">Log on name of the user whose password is requested.</param>
        /// <returns>User's password.</returns>
        /// <remarks>This is a utility method for authentication providers in the same assembly 
        /// which need plain text password for authenticating user, e.g. HttpDigestAuthenticationProvider.
        /// </remarks>
        internal static string GetPassword(string logOnName)
        {
            //// Parameter Validation
            ValidateParameters("logOnName", logOnName);

            string password = DataAccessLayer.GetPassword(logOnName);
            if (string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }

            return PasswordManager.GetPlainPassword(password);
        }

        /// <summary>
        /// Validates the token. The token is valid if it is
        /// not null and Validate() on the token returns true.
        /// </summary>
        /// <param name="token">Authenticated token</param>
        private static void ValidateToken(AuthenticatedToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            if (!token.Validate())
            {
                throw new AuthenticationException(ConstantStrings.InvalidTokenMessage);
            }
        }

        /// <summary>
        /// Checks whether the string parameters sent are null/empty and 
        /// throws ArgumentNullException if so.
        /// Call this method with "paramName", "paramValue" pairs for each argument to be validated.
        /// </summary>
        /// <param name="args">Arguments; <c>paramName</c>, <c>paramValue</c> pairs for each argument to be validated.</param>
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
