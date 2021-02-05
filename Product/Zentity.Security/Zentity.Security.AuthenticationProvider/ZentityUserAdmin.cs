// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider
{
    using System;
    using System.Collections.Generic;
    using Zentity.Security.Authentication;
    using Zentity.Security.AuthenticationProvider.PasswordManagement;

    /// <summary>
    /// This class provides administrative user management functions
    /// </summary>
    public class ZentityUserAdmin
    {
        /// <summary>
        /// Initializes a new instance of the ZentityUserAdmin class 
        /// only if the credentials are correct and the user is set as an admin user
        /// </summary>
        /// <param name="adminUserName">LogOnName of an admin user</param>
        /// <param name="adminPassword">Password of the admin user</param>
        /// <remarks>
        /// The Zentity installation provides a built in administrator account, whose credentials are as
        /// provided in the code below.
        /// It is highly recommended that the admin password should be changed soon after installing Zentity, 
        /// and at least one alternate admin account should be created.
        /// </remarks>
        /// <example>
        /// <code>
        /// //The ZentityUserAdmin constructor initializes an instance only when valid credentials
        ///    //for a user with admin rights are provided.
        ///    //Otherwise the constructor throws AuthenticationException.
        ///    try
        ///    {
        ///        ZentityUserAdmin admin = new ZentityUserAdmin(&quot;Administrator&quot;, &quot;XXXX&quot;);//Supply correct password
        ///        Console.WriteLine(&quot;Administrator logged in.&quot;);
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
        ///
        /// </code>
        /// </example>
        public ZentityUserAdmin(string adminUserName, string adminPassword)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(adminUserName))
            {
                throw new ArgumentNullException("adminUserName");
            }

            if (string.IsNullOrEmpty(adminPassword))
            {
                throw new ArgumentNullException("adminPassword");
            }
            #endregion

            try
            {
                ZentityUser user = new ZentityUser(adminUserName, adminPassword);
                if (user.VerifyPassword())
                {
                    if (!DataAccessLayer.IsAdmin(adminUserName))
                    {
                        throw new AuthenticationException(ConstantStrings.AdminAuthenticationExceptionMessage);
                    }
                }
                else
                {
                    throw new AuthenticationException(ConstantStrings.AdminAuthenticationExceptionMessage);
                }
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ZentityUserAdmin class.
        /// </summary>
        /// <param name="adminToken">Authenticated token of the logged on admin.</param>
        /// <example>
        /// <code>
        /// //Get authentication provider instance from factory
        /// IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;ZentityAuthenticationProvider&quot;);
        /// //Login as administrator
        /// AuthenticatedToken adminToken = provider.Authenticate(new UserNameSecurityToken(&quot;Administrator&quot;, &quot;XXXX&quot;));//Supply correct password
        /// //Create ZentityUserAdmin instance using admin's token
        /// ZentityUserAdmin admin = new ZentityUserAdmin(adminToken);
        /// </code>
        /// </example>
        /// <seealso cref="Zentity.Security.AuthenticationProvider.ZentityAuthenticationProvider"/>
        public ZentityUserAdmin(AuthenticatedToken adminToken)
        {
            #region Input Validation
            if (adminToken == null)
            {
                throw new ArgumentNullException("adminToken");
            }

            if (!adminToken.Validate())
            {
                throw new AuthenticationException(ConstantStrings.InvalidTokenMessage);
            }
            #endregion

            try
            {
                if (!DataAccessLayer.IsAdmin(adminToken.IdentityName))
                {
                    throw new AuthenticationException(ConstantStrings.AdminAuthenticationExceptionMessage);
                }
            }
            catch (TypeInitializationException ex) 
            {
                //// thrown in case of incorrect application configuration
                throw new AuthenticationException(ConstantStrings.TypeInitializationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Deactivates a user. Call this method when admin wants to disable a user account. 
        /// This does not permanently delete the user record. 
        /// </summary>
        /// <param name="logOnName">LogOn Name of the user</param>
        /// <returns>True if user account is deactivated successfully</returns>
        /// <remarks>Typical use case for this functionality is the administrator wanting to disable users who 
        /// are inactive for a long period.
        /// Other use case might be admin disabling users who are misusing the Zentity products and services by posting questionable material etc.
        /// 
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll</item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing the username with valid value.</item>
        /// </list>
        /// </remarks>
        /// 
        /// <example>
        /// <code>
        /// try
        ///    {
        ///        //An admin user instance is created on providing valid credentials for a user with administrative rights.
        ///        //Following are the credentials for a built in administrator account.
        ///        ZentityUserAdmin admin = new ZentityUserAdmin(&quot;Administrator&quot;, &quot;XXXX&quot;);//Supply correct password
        ///        bool deactivated = admin.Deactivate(&quot;JohnDE&quot;);
        ///        if (deactivated)
        ///        {
        ///            Console.WriteLine(&quot;User deactivated&quot;);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;User could not be deactivated. The logon name might not exist.&quot;);
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
        ///
        /// </code>
        /// </example>
        public bool Deactivate(string logOnName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(logOnName))
            {
                throw new ArgumentNullException("logOnName");
            }
            #endregion

            bool success = this.SetAccountStatus(logOnName, ConstantStrings.Disabled);
            return success;
        }

        /// <summary>
        /// Permanently deletes a user record
        /// </summary>
        /// <param name="logOnName">LogOn Name of the user</param>
        /// <returns>True if user account is deleted successfully</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing the username with valid value.</item>
        /// </list>
        /// <code>
        /// try
        ///    {
        ///        //An admin user instance is created on providing valid credentials for a user with administrative rights.
        ///        //Following are the credentials for a built in administrator account.
        ///        ZentityUserAdmin admin = new ZentityUserAdmin(&quot;Administrator&quot;, &quot;XXXX&quot;);//Supply correct password
        ///        bool deleted = admin.Delete(&quot;JohnDE&quot;);
        ///        if (deleted)
        ///        {
        ///            Console.WriteLine(&quot;User deleted&quot;);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;User could not be deleted. The logon name might not exist.&quot;);
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
        ///
        /// </code>
        /// </example>
        public bool Delete(string logOnName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(logOnName))
            {
                throw new ArgumentNullException("logOnName");
            }
            #endregion

            bool success = DataAccessLayer.DeleteUser(logOnName);
            return success;
        }

        /// <summary>
        /// Returns users sorted by log on name with the given start and end indexes
        /// </summary>
        /// <param name="startIndex">Start index for paged retrieval of users</param>
        /// <returns>Users collection</returns>
        /// <example>For code sample please refer to GetUsers() documentation. This method overload varies only by index parameters
        /// added for enabling paging.</example>
        public IEnumerable<ZentityUserProfile> GetUsers(int startIndex)
        {
            IEnumerable<ZentityUserProfile> users = this.GetUsers(startIndex, -1);
            return users;
        }

        /// <summary>
        /// Returns users sorted by log on name with the given start and end indexes
        /// </summary>
        /// <returns>Users collection</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample</item>
        /// </list>
        /// <code>
        /// try
        ///    {
        ///        //An admin user instance is created on providing valid credentials for a user with administrative rights.
        ///        //Following are the credentials for a built in administrator account.
        ///        ZentityUserAdmin admin = new ZentityUserAdmin(&quot;Administrator&quot;, &quot;XXXX&quot;);//Supply correct password
        ///        //This method retrieves all repository users.
        ///        IEnumerable&lt;ZentityUser&gt; users = admin.GetUsers();
        ///        foreach (ZentityUser user in users)
        ///        {
        ///            Console.WriteLine(user.LogOnName);
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
        ///
        /// </code>
        /// </example>
        public IEnumerable<ZentityUserProfile> GetUsers()
        {
            IEnumerable<ZentityUserProfile> users = this.GetUsers(1, -1);
            return users;
        }

        /// <summary>
        /// Returns users sorted by log on name with the given start and end indexes
        /// </summary>
        /// <param name="startIndex">Start index for paged retrieval of users. First record has index 1. </param>
        /// <param name="endIndex">End index for paged retrieval of users. End index is inclusive. e.g Sending (1, 1) as start index and 
        /// end index will return the 1st record in alpabetical order of the user's logon name. </param>
        /// <returns>Users collection</returns>
        /// <example>For code sample please refer to GetUsers() documentation. This method overload varies only by index parameters
        /// added for enabling paging.</example>
        public IEnumerable<ZentityUserProfile> GetUsers(int startIndex, int endIndex)
        {
            IEnumerable<ZentityUserProfile> users = DataAccessLayer.GetUserProfiles(startIndex, endIndex);
            return users;
        }

        /// <summary>
        /// Sets a user as a admin user
        /// </summary>
        /// <param name="logOnName">LogOnName of the new user who is to be made administrator</param>
        /// <returns>True if user is set as administrator in the authentication store</returns>
        /// <remarks>
        /// This functionality is provided for giving admin rights to other user. The other user must be a registered user.
        /// </remarks>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing the logon name for new admin user with a valid value</item>
        /// </list>
        /// <code>
        ///        try
        ///    {
        ///        //Create a ZentityUserAdmin instance using built in or other existing administrator credentials.
        ///        ZentityUserAdmin currentAdmin = new ZentityUserAdmin(&quot;Administrator&quot;, &quot;XXXX&quot;);//Supply correct password
        ///        //Now give admin rights to another user.
        ///        bool adminSet = currentAdmin.SetAdmin(&quot;JohnDE&quot;);
        ///        if (adminSet)
        ///        {
        ///            //verify that John can log on as an admin user now
        ///            ZentityUserAdmin newAdmin = new ZentityUserAdmin(&quot;JohnDE&quot;, &quot;john@123&quot;);
        ///            Console.WriteLine(&quot;Admin rights given to new user.&quot;);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;Errors in giving admin rights to new user.&quot;);
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
        public bool SetAdmin(string logOnName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(logOnName))
            {
                throw new ArgumentNullException("logOnName");
            }
            #endregion

            bool success = DataAccessLayer.SetAdmin(logOnName, true);
            return success;
        }

        /// <summary>
        /// Resets the isAdmin flag of an admin user
        /// </summary>
        /// <param name="logOnName">LogOnName of the new user who is to be made administrator</param>
        /// <returns>True if user is unset as administrator in the authentication store</returns>
        /// <remarks>This functionality is provided for revoking admin rights from a user.</remarks>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Refer to the sample application configuration file given in help, and create a similar one for your application.</item>
        /// <item>Add reference to Zentity.Security.Authentication.dll and Zentity.Security.AuthenticationProvider.dll </item>
        /// <item>Run the sample for registering new users to create the user accounts in the authentication database.</item>
        /// <item>Then run this sample, replacing the logon name for new admin user with a valid value</item>
        /// </list>
        /// <code>
        ///  try
        ///    {
        ///        //Create a ZentityUserAdmin instance using built in or other existing administrator credentials.
        ///        ZentityUserAdmin currentAdmin = new ZentityUserAdmin(&quot;Administrator&quot;, &quot;XXXX&quot;);//Supply correct password
        ///        //Now give admin rights to another user.
        ///        bool adminSet = currentAdmin.SetAdmin(&quot;JohnDE&quot;);
        ///        if (adminSet)
        ///        {
        ///            //verify that John can log on as an admin user now
        ///            ZentityUserAdmin newAdmin = new ZentityUserAdmin(&quot;JohnDE&quot;, &quot;john@123&quot;);
        ///            Console.WriteLine(&quot;Admin rights given to new user.&quot;);
        ///            //Now unset the new admin
        ///            currentAdmin.UnsetAdmin(&quot;JohnDE&quot;);
        ///            //The following call to ZentityUserAdmin constructor will throw AuthenticationException 
        ///            //since John is no longer an admin.
        ///            ZentityUserAdmin john = new ZentityUserAdmin(&quot;JohnDE&quot;, &quot;john@123&quot;);
        ///        }
        ///        else
        ///        {
        ///            Console.WriteLine(&quot;Errors in giving admin rights to new user.&quot;);
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
        ///
        /// </code>
        /// </example>
        public bool UnsetAdmin(string logOnName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(logOnName))
            {
                throw new ArgumentNullException("logOnName");
            }
            #endregion

            bool success = DataAccessLayer.SetAdmin(logOnName, false);
            return success;
        }

        /// <summary>
        /// Returns a user profile
        /// </summary>
        /// <param name="logOnName">LogOnName of the user whose profile is needed</param>
        /// <returns>ZentityUser object filled with property values read from database</returns>
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
        ///        //Create a ZentityUserAdmin instance using built in or other existing administrator credentials.
        ///        ZentityUserAdmin admin = new ZentityUserAdmin(&quot;Administrator&quot;, &quot;XXXX&quot;);//Supply correct password
        ///        ZentityUser user = admin.GetUserProfile(&quot;JohnDE&quot;);
        ///        //The ZentityUser instance contains all property values filled in from his record in the store.
        ///        if (user != null)
        ///        {
        ///            Console.WriteLine(&quot;FirstName: {0}, AccountStatus: {1}&quot;, user.FirstName, user.AccountStatus);
        ///            Console.WriteLine(&quot;Email: {0}&quot;, user.Email);
        ///        }
        ///        
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
        ///
        /// </code>
        /// </example>
        public ZentityUserProfile GetUserProfile(string logOnName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(logOnName))
            {
                throw new ArgumentNullException("logOnName");
            }
            #endregion

            ZentityUserProfile profile = DataAccessLayer.GetUserProfile(logOnName);
            return profile;
        }

         /// <summary>
        /// Checks whether the log on name selected by a new user is available 
        /// </summary>
        /// <param name="logOnName">LogOnName selected by the new user.</param>
        /// <param name="accountStatus">New status to be set for the user.</param>
        /// <returns>True if log on name is not already in use.</returns>
        public bool SetAccountStatus(string logOnName, string accountStatus)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(logOnName))
            {
                throw new ArgumentNullException(logOnName);
            }

            if (string.IsNullOrEmpty(accountStatus))
            {
                throw new ArgumentNullException(accountStatus);
            }
            #endregion

            bool success = DataAccessLayer.UpdateAccountStatus(logOnName, accountStatus);
            return success;
        }

        /// <summary>
        /// Returns list of available account status values.
        /// </summary>
        /// <returns>List of available account status values.</returns>
        public IEnumerable<string> GetAccountStatusValues()
        {
            return DataAccessLayer.GetAccountStatusValues();
        }

        /// <summary>
        /// Resets password for a user who has forgotten both his password as well as security question and answer.
        /// </summary>
        /// <param name="logOnName">Log on name</param>
        /// <returns>List of available account status values.</returns>
        public string ResetPassword(string logOnName)
        {
            #region Parameter Validation
            if (string.IsNullOrEmpty(logOnName))
            {
                throw new ArgumentNullException("logOnName");
            }
            #endregion
            ZentityUserProfile userProfile = DataAccessLayer.GetUserProfile(logOnName);
            if (userProfile != null)
            {
                string newPassword = PasswordManager.ForgotPassword(
                                                            userProfile.LogOnName, 
                                                            userProfile.SecurityQuestion,
                                                            userProfile.Answer);
                return newPassword;
            }

            return string.Empty;
        }
    }
}
