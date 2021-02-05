// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthorizationHelper
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Zentity.Core;
    using Zentity.Security.Authentication;
    using Zentity.Security.Authorization;

    /// <summary>
    /// This class can be used to grant and revoke permissions to an identity or group.
    /// </summary>
    public static class PermissionManager
    {
        #region Grant() overloads

        #region Grant permission on single resource
        /// <summary>
        /// Helper API method which grants given permission to the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resource">Resource on which the permission is to be granted</param>
        /// <param name="permissionName">Permission name e.g. 'Read', 'Update' etc.</param>
        /// <param name="identityOrGroupName">Name of the user or group to whom the permission is to be granted</param>
        /// <param name="isIdentity">Indicate whether the name passed in the previous parameter represents a user or a group</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to grant permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>true if grant succeeds</returns>
        /// <remarks>
        /// Permission can be granted to another user if the following conditions are satisfied
        /// <list type="bullet">
        /// <item>The AuthenticatedToken represents an admin user or owner of the resource</item>
        /// <item>The user to whom the permission is to be granted is not 'Guest' user.</item>
        /// </list>
        /// <para>Permissions need not be granted to admin users or group.
        /// 'Read' permission is implicit and hence Grant() need not be called explicitly for granting read.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority. Granting a permission automatically
        /// grants permissions which have lower priority than the given permission. e.g. granting delete grants update too. (read is implicitly granted).
        /// It also removes any revoked permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. granting update removes update-revoke, if any is present.</para>
        ///
        /// <para>IMPORTANT -
        /// <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary. </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <para>Pre-requisites to run this sample
        /// <list type="bullet">
        /// <item>References to the following assemblies should be added to the application running this sample
        /// <list type="bullet">
        /// <item>System.Data.Entity</item>
        /// <item>System.IdentityModel</item>
        /// <item>Zentity.Core</item>
        /// <item>Zentity.Security.Authentication</item>
        /// <item>Zentity.Security.AuthenticationProvider</item>
        /// <item>Zentity.Security.Authorization</item>
        /// <item>Zentity.Security.AuthorizationHelper</item>
        /// </list>
        /// </item>
        /// <item>Application configuration file should contain all sections given in the sample configuration file in this help file.</item>
        /// <item>The users, groups and resources mentioned in this sample should be existing in the repository. Use the UserManager class
        /// for creating users and groups.</item>
        /// </list>
        /// </para>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Identity to whom access is to be granted
        /// string identityNameToBeGrantedAccess = "NewUser";
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Grant access
        /// bool granted = resource.Grant("Update", identityNameToBeGrantedAccess, true, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static bool Grant<T>(
                                this T resource, 
                                string permissionName, 
                                string identityOrGroupName,
                                bool isIdentity, 
                                ZentityContext context,
                                AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName, "identityOrGroupName", identityOrGroupName);
            ValidateParameters(context, userToken);
            #endregion

            ////Retrieve the identity or group objects from the repository
            ////and call grant overload which takes identity or group parameter.
            if (isIdentity)
            {
                Identity user = DataAccess.GetIdentity(identityOrGroupName, context);
                if (user != null)
                {
                    bool granted = resource.Grant<T>(permissionName, user, context, userToken);
                    return granted;
                }

                return false;
            }
            else
            {
                Group group = DataAccess.GetGroup(identityOrGroupName, context);
                if (group != null)
                {
                    bool granted = resource.Grant<T>(permissionName, group, context, userToken);
                    return granted;
                }

                return false;
            }
        }

        /// <summary>
        /// Grants the permission to the identity
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resource">Resource on which the permission is to be granted</param>
        /// <param name="permissionName">Name for the permission predicate e.g. 'Read'</param>
        /// <param name="identity">Identity object to whom the permission is to be granted</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to grant permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>true if grant succeeds</returns>
        /// <remarks>
        /// Permission can be granted to another user if the following conditions are satisfied
        /// <list type="bullet">
        /// <item>The AuthenticatedToken represents an admin user or owner of the resource</item>
        /// <item>The user to whom the permission is to be granted is not 'Guest' user.</item>
        /// </list>
        /// <para>Permissions need not be granted to admin users.
        /// 'Read' permission is implicit and hence Grant() need not be called explicitly for granting read.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority. Granting a permission automatically
        /// grants permissions which have lower priority than the given permission. e.g. granting delete grants update too. (read is implicitly granted).
        /// It also removes any revoked permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. granting update removes update-revoke, if any is present.</para>
        ///
        /// <para>IMPORTANT -
        /// <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary. </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity to whom access is to be granted
        /// Identity identityToBeGrantedAccess = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("NewUser", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Grant access
        /// bool granted = resource.Grant("Update", identityToBeGrantedAccess, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static bool Grant<T>(
                                this T resource, 
                                string permissionName, 
                                Identity identity,
                                ZentityContext context, 
                                AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //-----------------------------------Algorithm----------------------------------
            //      Check for existence of permission predicates.
            //      If user is an administrator
            //          Return true (No need to actually store permissions)
            //      Else if user == Guest and permission is not read
            //          Return false (Guests cannot be granted any permissions except read)
            //      Else (Normal user or guest with read permission)
            //              If user represented by token is owner of the resource or an administrator
            //                  Grant the requested permission, plus all permissions which have lower priority than the requested one.
            //                  (e.g. Granting 'delete' should also grant 'update'.)
            //                  In case of read we do not actually create the relationship.
            //                  Remove the denied permissions, if any for these. That is, if delete is granted, deny-delete is removed.
            //              Else
            //                  Return false (only owner or an admin can grant permissions on a resource)
            //              End If
            //          End If
            //      End If
            //------------------------------------------------------------------------------
            #endregion

            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateIdentity(identity);
            ValidateParameters(context, userToken);
            #endregion
            try
            {
                return ExecuteGrant<T>(resource, permissionName, identity, context, userToken);
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Grants the permission to the group
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resource">Resource on which the permission is to be granted</param>
        /// <param name="permissionName">Permission name, e.g. 'read'</param>
        /// <param name="group">Group object to whom the permission is to be granted</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to grant permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>true if grant succeeds</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>Permission can be granted to group if the following conditions are satisfied -
        /// The AuthenticatedToken represents an admin user or owner of the resource.</para>
        ///
        /// <para>Permissions need not be granted to admin group.
        /// 'Read' permission is implicit and hence Grant() need not be called explicitly for granting read.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority. Granting a permission automatically
        /// grants permissions which have lower priority than the given permission. e.g. granting delete grants update too. (read is implicitly granted).
        /// It also removes any revoked permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. granting update removes update-revoke, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group to which access is to be granted
        /// Group groupToBeGrantedAccess = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("NewGroup", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Grant access
        /// bool granted = resource.Grant("Update", groupToBeGrantedAccess, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static bool Grant<T>(
                                this T resource, 
                                string permissionName, 
                                Group group,
                                ZentityContext context, 
                                AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //-----------------------------------Algorithm----------------------------------
            //      Check for existence of permission predicates.
            //      If group is administrator's group
            //          Return true (No need to actually store permissions)
            //      Else (Normal group)
            //              If user represented by token is owner of the resource or an administrator
            //                  Grant the requested permission, plus all permissions which have lower priority than the requested one.
            //                  (e.g. Granting 'delete' should also grant 'update'.)
            //                  In case of read we do not actually create the relationship.
            //                  Remove the denied permissions, if any for these. That is, if delete is granted, deny-delete is removed.
            //              Else
            //                  Return false (only owner or an admin can grant permissions on a resource)
            //              End If
            //          End If
            //      End If
            //------------------------------------------------------------------------------
            #endregion

            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateGroup(group);
            ValidateParameters(context, userToken);
            #endregion
            try
            {
                return ExecuteGrant<T>(resource, permissionName, group, context, userToken);
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        #endregion

        #region Grant permission on a list of resources
        /// <summary>
        /// Grants the given permission to the user or group on all resources in the given list.
        /// This method enumerates through all the resources.
        /// Hence paging and filtering should be done BEFORE calling this method.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resources">List of resources on which permission is to be granted</param>
        /// <param name="permissionName">Name of the permission predicate, e.g. 'Read'</param>
        /// <param name="identityOrGroupName">Name of the user or group to whom the permission is to be granted</param>
        /// <param name="isIdentity">Indicate whether the name passed in the previous parameter represents a user or a group</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to grant permission to another user. This user must be
        /// either administrator or owner of the resources.</param>
        /// <returns>List of resources for which grant succeeded.</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method grants the given permission to the identity on the list of resources passed as parameter.
        /// It materializes the list of resources, iterating through the list, grants the permission to each resource.
        /// If grant is successful it adds the resource to the list of granted resources which is returned by this method.</para>
        ///
        /// <para>Grant need not be called for admin users. It will return false for guest user.
        /// The user requesting grant (represented by the token) must be an administrator or owner of the resource.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority. Granting a permission automatically
        /// grants permissions which have lower priority than the given permission. e.g. granting delete grants update too. (read is implicitly granted).
        /// It also removes any revoked permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. granting update removes update-revoke, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Identity to whom access is to be granted
        /// string identityNameToBeGrantedAccess = "NewUser";
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Grant access
        /// IEnumerable&lt;Resource&gt; grantedResources = resources.Grant("Update", identityNameToBeGrantedAccess, true, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static IEnumerable<T> Grant<T>(
                                            this IEnumerable<T> resources, 
                                            string permissionName, 
                                            string identityOrGroupName,
                                            bool isIdentity, 
                                            ZentityContext context, 
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName, "identityOrGroupName", identityOrGroupName);
            ValidateParameters(context, userToken);
            #endregion

            List<T> originalResources = resources.ToList();
            List<T> grantedResources = new List<T>();

            ////Enumerate through the resources and call grant for each resource
            foreach (T resource in originalResources)
            {
                bool success = resource.Grant(permissionName, identityOrGroupName, isIdentity, context, userToken);
                if (success)
                {
                    grantedResources.Add(resource);
                }
            }

            return grantedResources.AsEnumerable();
        }

        /// <summary>
        /// Grants the permission to the identity on all resources in the given list
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resources">List of resources on which permission is to be granted</param>
        /// <param name="permissionName">Uri for the permission predicate</param>
        /// <param name="identity">Identity object to whom the permission is to be granted</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to grant permission to another user. This user must be
        /// either administrator or owner of the resources.</param>
        /// <returns>List of resources for which grant succeeded.</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method grants the given permission to the identity on the list of resources passed as parameter.
        /// It materializes the list of resources, iterating through the list, grants the permission to each resource.
        /// If grant is successful it adds the resource to the list of granted resources which is returned by this method.</para>
        ///
        /// <para>Grant need not be called for admin users. It will return false for guest user.
        /// The user requesting grant (represented by the token) must be an administrator or owner of the resource.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority. Granting a permission automatically
        /// grants permissions which have lower priority than the given permission. e.g. granting delete grants update too. (read is implicitly granted).
        /// It also removes any revoked permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. granting update removes update-revoke, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity to whom access is to be granted
        /// Identity identityToBeGrantedAccess = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("NewUser", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Grant access
        /// IEnumerable&lt;Resource&gt; grantedResources = resources.Grant("Update", identityToBeGrantedAccess, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static IEnumerable<T> Grant<T>(
                                        this IEnumerable<T> resources, 
                                        string permissionName, 
                                        Identity identity,
                                        ZentityContext context, 
                                        AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateIdentity(identity);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                List<T> originalResources = resources.ToList();
                List<T> grantedResources = new List<T>();

                ////Enumerate through the resources and call grant for each resource
                foreach (T resource in originalResources)
                {
                    bool success = ExecuteGrant<T>(resource, permissionName, identity, context, userToken);
                    if (success)
                    {
                        grantedResources.Add(resource);
                    }
                }

                return grantedResources.AsEnumerable();
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Grants the permission to the group on all resources in the given list.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resources">List of resources on which permission is to be granted</param>
        /// <param name="permissionName">Uri for the permission predicate</param>
        /// <param name="group">Group object to whom the permission is to be granted</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to grant permission to another user. This user must be
        /// either administrator or owner of the resources.</param>
        /// <returns>List of resources for which grant succeeded.</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method grants the given permission to the identity on the list of resources passed as parameter.
        /// It materializes the list of resources, iterating through the list, grants the permission to each resource.
        /// If grant is successful it adds the resource to the list of granted resources which is returned by this method.</para>
        ///
        /// <para>Grant need not be called for admin users. It will return false for guest user.
        /// The user requesting grant (represented by the token) must be an administrator or owner of the resource.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority. Granting a permission automatically
        /// grants permissions which have lower priority than the given permission. e.g. granting delete grants update too. (read is implicitly granted).
        /// It also removes any revoked permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. granting update removes update-revoke, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group to which access is to be granted
        /// Group groupToBeGrantedAccess = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("NewGroup", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Grant access
        /// IEnumerable&lt;Resource&gt; grantedResources = resources.Grant("Update", groupToBeGrantedAccess, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static IEnumerable<T> Grant<T>(
                                            this IEnumerable<T> resources, 
                                            string permissionName, 
                                            Group group,
                                            ZentityContext context, 
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateGroup(group);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                if (userToken.Validate())
                {
                    List<T> originalResources = resources.ToList();
                    List<T> grantedResources = new List<T>();

                    ////Enumerate through the resources and call grant for each resource
                    foreach (T resource in originalResources)
                    {
                        bool success = ExecuteGrant<T>(resource, permissionName, group, context, userToken);
                        if (success)
                        {
                            grantedResources.Add(resource);
                        }
                    }

                    return grantedResources.AsEnumerable();
                }
                else
                {
                    throw new AuthorizationException(ConstantStrings.TokenNotValidException);
                }
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }
        #endregion
        #endregion

        #region Revoke() overloads

        #region Revoke permission on a single resource
        /// <summary>
        /// Revokes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resource">Resource on which the permission is to be revoked</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="identityOrGroupName">Name of the user or group to whom the permission is to be revoked</param>
        /// <param name="isIdentity">Indicate whether the name passed in the previous parameter represents a user or a group</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to revoke permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>true if revoke succeeds</returns>
        /// <remarks>
        /// <para>Permission can be revoked from another user/group if the following conditions are satisfied
        /// The AuthenticatedToken represents an admin user or owner of the resource</para>
        ///
        /// <para>Permissions need not be revoked from admin users or group.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority.
        /// Revoking a permission automatically revokes permissions which have higher priority than the given permission.
        /// e.g. revoking delete revokes owner too.
        /// It also removes any granted permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. revoking update removes update-grant, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary. </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Identity for whom access is to be revoked
        /// string identityNameToBeRevokedAccess = "User1";
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Revoke access
        /// bool revoked = resource.Revoke("Update", identityNameToBeRevokedAccess, true, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static bool Revoke<T>(
                                    this T resource, 
                                    string permissionName, 
                                    string identityOrGroupName,
                                    bool isIdentity, 
                                    ZentityContext context, 
                                    AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName, "identityOrGroupName", identityOrGroupName);
            ValidateParameters(context, userToken);
            #endregion

            if (isIdentity)
            {
                Identity user = DataAccess.GetIdentity(identityOrGroupName, context);
                if (user != null)
                {
                    bool revoked = resource.Revoke(permissionName, user, context, userToken);
                    return revoked;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Group group = DataAccess.GetGroup(identityOrGroupName, context);
                if (group != null)
                {
                    bool revoked = resource.Revoke(permissionName, group, context, userToken);
                    return revoked;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Revokes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resource">Resource on which the permission is to be revoked</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="identity">Identity object from whom the permission is to be revoked.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to revoke permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>true if revoke succeeds</returns>
        /// <remarks>
        /// <para>Permission can be revoked from another user if the following conditions are satisfied
        /// The AuthenticatedToken represents an admin user or owner of the resource</para>
        ///
        /// <para>Permissions need not be revoked from admin users or group.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority.
        /// Revoking a permission automatically revokes permissions which have higher priority than the given permission.
        /// e.g. revoking delete revokes owner too.
        /// It also removes any granted permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. revoking update removes update-grant, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity for whom access is to be revoked
        /// Identity identityToBeRevokedAccess = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("User1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Revoke access
        /// bool revoked = resource.Revoke("Update", identityToBeRevokedAccess, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static bool Revoke<T>(
                                    this T resource, 
                                    string permissionName, 
                                    Identity identity,
                                    ZentityContext context, 
                                    AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //---------------------------------------------Algorithm---------------------------------------------------------
            //  Check for existence of permission
            //  If user is an administrator
            //      Return false (Can't revoke any permission from administrator)
            //  Else if token belongs to owner of the resource or an administrator
            //      If user == Guest
            //          If permission == read
            //              Grant 'deny-read' (revoke read permission means granting deny-read)
            //          Else
            //              Return true - (the permission need not be revoked from Guest, since he is not granted the permission too.)
            //          End If
            //      Else (Normal user)
            //          Grant deny permission for the permission and all permissions with higher priority
            //          Remove allow permission for all the permissions above
            //  Else
            //      Return false - (for revoking a permission user represented by token needs to be owner of the resource or an administrator.)
            //  End If
            //---------------------------------------------------------------------------------------------------------------
            #endregion

            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateIdentity(identity);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                return ExecuteRevoke<T>(resource, permissionName, identity, context, userToken);
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Revokes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resource">Resource on which the permission is to be revoked</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="group">Group object from whom the permission is to be revoked.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to revoke permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>true if revoke succeeds</returns>
        /// <remarks>
        /// <para>Permission can be revoked from another group if the following conditions are satisfied
        /// The AuthenticatedToken represents an admin user or owner of the resource.</para>
        ///
        /// <para>Permissions need not be revoked from admin users or group.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority.
        /// Revoking a permission automatically revokes permissions which have higher priority than the given permission.
        /// e.g. revoking delete revokes owner too.
        /// It also removes any granted permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. revoking update removes update-grant, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group for which access is to be revoked
        /// Group groupToBeRevokedAccess = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("Group1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Revoke access
        /// bool revoked = resource.Revoke("Update", groupToBeRevokedAccess, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static bool Revoke<T>(
                                    this T resource, 
                                    string permissionName, 
                                    Group group,
                                    ZentityContext context, 
                                    AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //---------------------------------------------Algorithm---------------------------------------------------------
            //  Check for existence of permission
            //  If group is administrator's group
            //      Return false (Can't revoke any permission from administrators)
            //  Else if token belongs to owner of the resource or an administrator
            //      Grant deny permission for the permission and all permissions with higher priority
            //      Remove allow permission for all the permissions above
            //  Else
            //      Return false - (for revoking a permission user represented by token needs to be owner of the resource or an administrator.)
            //  End If
            //---------------------------------------------------------------------------------------------------------------
            #endregion

            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateGroup(group);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                return ExecuteRevoke<T>(resource, permissionName, group, context, userToken);
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        #endregion

        #region Revoke permissions on a list of resources

        /// <summary>
        /// Revokes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resources">List of resources on which the permission is to be revoked</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="identityOrGroupName">Name of identity or group from whom the permission is to be revoked.</param>
        /// <param name="isIdentity">Set to true if identity name is passed as parameter.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to revoke permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>List of resources for which the revoke succeeded</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method revokes the given permission from the identity/group on the list of resources passed as parameter.
        /// It materializes the list of resources, iterating through the list, revokes the permission to each resource.
        /// If revoke is successful it adds the resource to the list of revoked resources which is returned by this method.
        /// The user requesting revoke (represented by the token) must be an administrator or owner of the resource.</para>
        ///
        /// <para>Permissions need not be revoked from admin users or group.
        /// It will return false for guest user, if permission to be revoked is other than 'read'.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority.
        /// Revoking a permission automatically revokes permissions which have higher priority than the given permission.
        /// e.g. revoking delete revokes owner too.
        /// It also removes any granted permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. revoking update removes update-grant, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary. </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Identity for whom access is to be revoked
        /// string identityNameToBeRevokedAccess = "User1";
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Revoke access
        /// IEnumerable&lt;Resource&gt; revokedResources = resources.Revoke("Update", identityNameToBeRevokedAccess, true, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static IEnumerable<T> Revoke<T>(
                                            this IEnumerable<T> resources, 
                                            string permissionName, 
                                            string identityOrGroupName,
                                            bool isIdentity, 
                                            ZentityContext context, 
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName, "identityOrGroupName", identityOrGroupName);
            ValidateParameters(context, userToken);
            #endregion
            List<T> originalResources = resources.ToList();
            List<T> revokedResources = new List<T>();

            ////Call Revoke for each resource in the list
            foreach (T resource in originalResources)
            {
                bool revoked = resource.Revoke<T>(permissionName, identityOrGroupName, isIdentity, context, userToken);
                if (revoked)
                {
                    revokedResources.Add(resource);
                }
            }

            return revokedResources;
        }

        /// <summary>
        /// Revokes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resources">List of resources on which the permission is to be revoked</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="identity">Identity object from whom the permission is to be revoked.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to revoke permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>List of resources for which the revoke succeeded</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method revokes the given permission from the identity on the list of resources passed as parameter.
        /// It materializes the list of resources, iterating through the list, revokes the permission to each resource.
        /// If revoke is successful it adds the resource to the list of revoked resources which is returned by this method.
        /// revoke need not be called for admin users. It will return false for guest user.
        /// The user requesting revoke (represented by the token) must be an administrator or owner of the resource.</para>
        ///
        /// <para>Permissions need not be revoked from admin users or group. Only read permission can be revoked from guest user.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority.
        /// Revoking a permission automatically revokes permissions which have higher priority than the given permission.
        /// e.g. revoking delete revokes owner too.
        /// It also removes any granted permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. revoking update removes update-grant, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity for whom access is to be revoked
        /// Identity identityToBeRevokedAccess = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("User1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Revoke access
        /// IEnumerable&lt;Resource&gt; revokedResources = resources.Revoke("Update", identityToBeRevokedAccess, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static IEnumerable<T> Revoke<T>(
                                            this IEnumerable<T> resources, 
                                            string permissionName, 
                                            Identity identity,
                                            ZentityContext context, 
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateIdentity(identity);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                List<T> originalResources = resources.ToList();
                List<T> revokedResources = new List<T>();

                ////Revoke permission for each resource in the list
                foreach (T resource in originalResources)
                {
                    bool revoked = ExecuteRevoke<T>(resource, permissionName, identity, context, userToken);
                    if (revoked)
                    {
                        revokedResources.Add(resource);
                    }
                }

                return revokedResources;
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Revokes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resources">List of resources on which the permission is to be revoked</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="group">Group object from whom the permission is to be revoked.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to revoke permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>List of resources for which the revoke succeeded</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method revokes the given permission from the group on the list of resources passed as parameter.
        /// It materializes the list of resources, iterating through the list, revokes the permission to each resource.
        /// If revoke is successful it adds the resource to the list of revoked resources which is returned by this method.
        /// revoke need not be called for admin users. It will return false for guest user.
        /// The user requesting revoke (represented by the token) must be an administrator or owner of the resource.</para>
        ///
        /// <para>Permissions need not be revoked from admin users or group. Only read permission can be revoked from Guest user.</para>
        ///
        /// <para>Permissions are hierarchical - read, update, delete, owner, in increasing order of priority.
        /// Revoking a permission automatically revokes permissions which have higher priority than the given permission.
        /// e.g. revoking delete revokes owner too.
        /// It also removes any granted permissions, in order to maintain consistency in the permission state for a user/group on a resource.
        /// i.e. revoking update removes update-grant, if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>Pre-requisites to run this sample
        /// <list type="bullet">
        /// <item>References to the following assemblies should be added to the application running this sample
        /// <list type="bullet">
        /// <item>System.Data.Entity</item>
        /// <item>System.IdentityModel</item>
        /// <item>Zentity.Core</item>
        /// <item>Zentity.Security.Authentication</item>
        /// <item>Zentity.Security.AuthenticationProvider</item>
        /// <item>Zentity.Security.Authorization</item>
        /// <item>Zentity.Security.AuthorizationHelper</item>
        /// </list>
        /// </item>
        /// <item>Application configuration file should contain all sections given in the sample configuration file in this help file.</item>
        /// <item>The users, groups and resources mentioned in this sample should be existing in the repository. Use the UserManager class
        /// for creating users and groups.</item>
        /// </list>
        /// </para>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group for which access is to be revoked
        /// Group groupToBeRevokedAccess = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("NewGroup", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Revoke access
        /// IEnumerable&lt;Resource&gt; revokedResources = resources.Revoke("Update", groupToBeRevokedAccess, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static IEnumerable<T> Revoke<T>(
                                            this IEnumerable<T> resources, 
                                            string permissionName, 
                                            Group group,
                                            ZentityContext context, 
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateGroup(group);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                List<T> originalResources = resources.ToList();
                List<T> revokedResources = new List<T>();

                ////Call revoke for each resource in the list
                foreach (T resource in originalResources)
                {
                    bool revoked = ExecuteRevoke<T>(resource, permissionName, group, context, userToken);
                    if (revoked)
                    {
                        revokedResources.Add(resource);
                    }
                }

                return revokedResources;
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        #endregion
        #endregion

        #region Grant default permissions
        /// <summary>
        /// Grants default permissions on a resource to the user
        /// </summary>
        /// <typeparam name="T">A type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resource">Resource on which default permissions are to be granted.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Logged in user token</param>
        /// <returns>true if all default permissions are granted.</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// This method grants ownership of the resource to the user represented by the token.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken userToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Grant access
        /// bool granted = resource.GrantDefaultPermissions(context, userToken);
        /// </code>
        /// </example>
        public static bool GrantDefaultPermissions<T>(this T resource, ZentityContext context, AuthenticatedToken userToken)
            where T : Resource
        {
            #region Algorithm
            //--------------------------Algorithm---------------------------------
            //  If user is an administrator
            //      Return true (No need to actually store permissions)
            //  Else if user == Guest
            //      Return false (No permission can be granted to guest user)
            //  Else (Normal user)
            //      Grant Owner, Delete and Update permissions to the user.
            //--------------------------------------------------------------------
            #endregion

            #region Parameter Validation
            ValidateParameters(context, userToken);
            #endregion
            try
            {
                ////No permission can be granted to guest user
                if (DataAccess.IsGuest(userToken.IdentityName))
                {
                    return false;
                }

                Identity user = DataAccess.GetIdentity(userToken.IdentityName, context);
                if (user != null)
                {
                    //// If user is an administrator - return true
                    //// (No need to actually store permissions since admin has full access to all resources implicitly)
                    if (DataAccess.IsAdmin(user))
                    {
                        return true;
                    }
                    else
                    {
                        //// normal user
                        bool success = user.GrantAuthorization(SecurityPredicateAccess.GetPredicateUri("Owner"), resource, context);
                        success = user.GrantAuthorization(SecurityPredicateAccess.GetPredicateUri("Delete"), resource, context);
                        success = user.GrantAuthorization(SecurityPredicateAccess.GetPredicateUri("Update"), resource, context);
                        return success;
                    }
                }
                else
                {
                    throw new AuthorizationException(ConstantStrings.TokenNotValidException);
                }
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }
        #endregion

        #region GrantCreate() overloads
        /// <summary>
        /// Grants create permission to the Identity
        /// </summary>
        /// <param name="identity">Identity object to whom the permission is to be assigned</param>
        /// <param name="adminToken">Token for a user from administrator's group</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if grant succeeds</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method grants create permission to the identity. The user requesting grant (represented by token) must be an administrator.
        /// </para>
        /// <para>This method need not be called for admin identities, since they have create access by default.
        /// It will return false for guest user since guest cannot be granted create.
        /// </para>
        /// <para>Granting create will remove deny-create if any.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Admin", "XXXX"); //Supply correct password
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity to whom access is to be granted
        /// Identity identityToBeGrantedAccess = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("NewUser", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Grant access
        /// bool granted = identityToBeGrantedAccess.GrantCreate(adminToken, context);
        /// </code>
        /// </example>
        public static bool GrantCreate(this Identity identity, AuthenticatedToken adminToken, ZentityContext context)
        {
            #region Algorithm
            //--------------------------------------------Algorithm-----------------------------------------------------------
            //  If user == Guest
            //      Return false (guests cannot be granted create permission)
            //  Else if user is an administrator
            //      Return true (no need to actually store create relationship - admins have the create permission by default)
            //  Else
            //      If token represents administrator
            //          Grant create and remove deny-create permission, if any
            //          Return true
            //      Else
            //          Return false (only an admin can grant create permissions to other groups)
            //      End If
            //  End If
            //---------------------------------------------------------------------------------------------------------------
            #endregion

            #region Parameter Validation
            ValidateParameters(context, adminToken);
            ValidateIdentity(identity);
            #endregion

            try
            {
                //// If user == Guest return false (guests cannot be granted create permission)
                if (DataAccess.IsGuest(identity.IdentityName))
                {
                    return false;
                }
                else if (DataAccess.IsAdmin(identity))
                {
                    //// If user belongs to admin group return true
                    //// No need to actually store create relationship - admins have the create permission implicitly
                    return true;
                }
                else if (DataAccess.IsAdmin(adminToken.IdentityName, context))
                {
                    //// Only an admin can grant create permissions to users
                    bool granted = ExecuteGrantCreate(identity, context);
                    return granted;
                }
                else
                {
                    return false;
                }
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Grants create to the group
        /// </summary>
        /// <param name="group">Group object on which permission is to be assigned.</param>
        /// <param name="adminToken">Token for a user from administrator's group.</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if grant succeeds</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method grants create permission to the group. The user requesting grant (represented by token) must be an administrator.
        /// </para>
        /// <para>This method need not be called for admin group, since all its members have create access by default.
        /// It will return false for guest user since guest cannot be granted create.
        /// </para>
        /// <para>
        /// Granting create will remove revoke-create if any is present.
        /// </para>
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Admin", "XXXX"); //Supply correct password
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group to whom access is to be granted
        /// Group groupToBeGrantedAccess = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("NewGroup", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Grant access
        /// bool granted = groupToBeGrantedAccess.GrantCreate(adminToken, context);
        /// </code>
        /// </example>
        public static bool GrantCreate(this Group group, AuthenticatedToken adminToken, ZentityContext context)
        {
            #region Algorithm
            //--------------------------------------------Algorithm-----------------------------------------------------------
            //  If group is an administrator's group
            //      Return true (no need to actually store create relationship - admins have the create permission by default)
            //  Else
            //      If token represents administrator
            //          Grant create permission and remove deny-create permission if any
            //          Return true
            //      Else
            //          Return false (only an admin can grant create permissions to other groups)
            //      End If
            //  End If
            //----------------------------------------------------------------------------------------------------------------
            #endregion

            #region Parameter Validation
            ValidateParameters(context, adminToken);
            ValidateGroup(group);
            #endregion

            try
            {
                //// If group is an administrator's group return true
                //// No need to actually store create relationship - admins have the create permission by default
                if (DataAccess.IsAdmin(group))
                {
                    return true;
                }
                else if (DataAccess.IsAdmin(adminToken.IdentityName, context))
                {
                    //// Only an admin can grant create permissions to other groups
                    //// Grant create permission and remove deny-create permission if any
                    bool granted = ExecuteGrantCreate(group, context);
                    return granted;
                }
                else
                {
                    return false;
                }
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Grants create to the identity or group
        /// </summary>
        /// <param name="adminToken">Token for a user from administrator's group.</param>
        /// <param name="identityOrGroupName">Name of identity or group</param>
        /// <param name="isIdentity">Indicates whether the name passed is that of an identity</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if grant succeeds.</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method grants create permission to the identity or group.
        /// The user requesting grant (represented by token) must be an administrator.</para>
        /// <para>
        /// This method need not be called for admin identity/group, since all its members have create access by default.
        /// It will return false for guest user since guest cannot be granted create.
        /// </para>
        /// <para>Granting create will remove revoke-create if any is present.</para>
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>Pre-requisites to run this sample
        /// <list type="bullet">
        /// <item>References to the following assemblies should be added to the application running this sample
        /// <list type="bullet">
        /// <item>System.Data.Entity</item>
        /// <item>System.IdentityModel</item>
        /// <item>Zentity.Core</item>
        /// <item>Zentity.Security.Authentication</item>
        /// <item>Zentity.Security.AuthenticationProvider</item>
        /// <item>Zentity.Security.Authorization</item>
        /// <item>Zentity.Security.AuthorizationHelper</item>
        /// </list>
        /// </item>
        /// <item>Application configuration file should contain all sections given in the sample configuration file in this help file.</item>
        /// <item>The users, groups and resources mentioned in this sample should be existing in the repository. Use the UserManager class
        /// for creating users and groups.</item>
        /// </list>
        /// </para>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Admin", "XXXX"); //Supply correct password
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Identity to whom access is to be granted
        /// Identity identityNameToBeGrantedAccess = "NewUser";
        ///
        /// // Grant access
        /// bool granted = adminToken.GrantCreate(identityNameToBeGrantedAccess, true, context);
        /// </code>
        /// </example>
        public static bool GrantCreate(this AuthenticatedToken adminToken, string identityOrGroupName, bool isIdentity, ZentityContext context)
        {
            #region Parameter Validation
            ValidateParameters(context, adminToken);
            ValidateStrings("identityOrGroupName", identityOrGroupName);
            #endregion

            try
            {
                if (isIdentity)
                {
                    Identity user = DataAccess.GetIdentity(identityOrGroupName, context);
                    if (user != null)
                    {
                        bool granted = user.GrantCreate(adminToken, context);
                        return granted;
                    }
                    else
                    {
                        return false;
                    }
                }

                Group group = DataAccess.GetGroup(identityOrGroupName, context);
                if (group != null)
                {
                    bool granted = group.GrantCreate(adminToken, context);
                    return granted;
                }
                else
                {
                    return false;
                }
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }
        #endregion

        #region RevokeCreate() overloads
        /// <summary>
        /// Revokes create from the identity
        /// </summary>
        /// <param name="identity">Identity object to whom the permission is to be assigned</param>
        /// <param name="adminToken">Token of logged on user. He should be an admin for the method call to succed.</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if revoke succeeds</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// This method revokes create access from identity. The user requesting revoke (represented by token) must be an administrator.
        /// Revoking create will remove granted-create if any.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Admin", "XXXX"); //Supply correct password
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity for whom access is to be revoked
        /// Identity identityToBeRevokedAccess = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("User1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Revoke access
        /// bool revoked = identityToBeRevokedAccess.RevokeCreate(adminToken, context);
        /// </code>
        /// </example>
        public static bool RevokeCreate(this Identity identity, AuthenticatedToken adminToken, ZentityContext context)
        {
            #region Algorithm
            //---------------------------------------------Algorithm-----------------------------------------------------------------
            //  If the identity belongs to administrator's group
            //      return false (permission cannot be revoked from admin)
            //  Else if the identity is guest
            //      return true but do not add deny relationship (guests anyway dont have create permission)
            //  Else
            //      If userToken represents a user from Administrators group
            //          Grant deny-create to the user represented by identity and remove create permission.
            //      Else
            //          Return false.
            //-----------------------------------------------------------------------------------------------------------------------
            #endregion
            #region Parameter Validation
            ValidateParameters(context, adminToken);
            ValidateIdentity(identity);
            #endregion

            try
            {
                ////If the identity belongs to administrator's group return false (permission cannot be revoked from admin)
                if (DataAccess.IsAdmin(identity))
                {
                    return false;
                }

                ////For guest return true but do not add deny relationship (guests anyway dont have create permission)
                if (DataAccess.IsGuest(identity.IdentityName))
                {
                    return true;
                }

                if (DataAccess.IsAdmin(adminToken.IdentityName, context))
                {
                    bool revoked = ExecuteRevokeCreate(identity, context);
                    return revoked;
                }
                else
                {
                    return false;
                }
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Revokes create permission from group
        /// </summary>
        /// <param name="group">Group object on which permission is to be assigned.</param>
        /// <param name="adminToken">Token for a user from administrator's group.</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if revoke succeeds</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// This method revokes create access from group. The user requesting revoke (represented by token) must be an administrator.
        /// Revoking create will remove granted-create if any.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Admin", "XXXX"); //Supply correct password
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group for whom access is to be revoked
        /// Group groupToBeRevokedAccess = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("Group1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Revoke access
        /// bool revoked = groupToBeRevokedAccess.RevokeCreate(adminToken, context);
        /// </code>
        /// </example>
        public static bool RevokeCreate(this Group group, AuthenticatedToken adminToken, ZentityContext context)
        {
            #region Algorithm
            //---------------------------------------------Algorithm-----------------------------------------------------------------
            //  If group is administrator's group
            //      return false (permission cannot be revoked from admin)
            //  Else
            //      If userToken represents a user from Administrators group
            //          Grant deny-create to the user represented by identity and remove create permission.
            //      Else
            //          Return false. (Only an administrator can revoke create permission from groups)
            //-----------------------------------------------------------------------------------------------------------------------
            #endregion

            #region Parameter Validation
            ValidateParameters(context, adminToken);
            ValidateGroup(group);
            #endregion

            try
            {
                ////If group is administrator's group return false
                ////(permission cannot be revoked from admin)
                if (DataAccess.IsAdmin(group))
                {
                    return false;
                }
                else if (DataAccess.IsAdmin(adminToken.IdentityName, context))
                {
                    ////Only an administrator can revoke create permission from groups
                    ////Grant deny-create to the group and remove create permission.
                    return ExecuteRevokeCreate(group, context);
                }
                else
                {
                    return false;
                }
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Revokes create from identity or group
        /// </summary>
        /// <param name="adminToken">Token for a user from administrator's group.</param>
        /// <param name="identityOrGroupName">Name of identity or group</param>
        /// <param name="isIdentity">Indicates whether the name passed is that of an identity</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if revoke succeeds.</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// This method revokes create access from identity or group. The user requesting revoke (represented by token) must be an administrator.
        /// Revoking create will remove granted-create if any.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>Pre-requisites to run this sample
        /// <list type="bullet">
        /// <item>References to the following assemblies should be added to the application running this sample
        /// <list type="bullet">
        /// <item>System.Data.Entity</item>
        /// <item>System.IdentityModel</item>
        /// <item>Zentity.Core</item>
        /// <item>Zentity.Security.Authentication</item>
        /// <item>Zentity.Security.AuthenticationProvider</item>
        /// <item>Zentity.Security.Authorization</item>
        /// <item>Zentity.Security.AuthorizationHelper</item>
        /// </list>
        /// </item>
        /// <item>Application configuration file should contain all sections given in the sample configuration file in this help file.</item>
        /// <item>The users, groups and resources mentioned in this sample should be existing in the repository. Use the UserManager class
        /// for creating users and groups.</item>
        /// </list>
        /// </para>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Admin", "XXXX"); //Supply correct password
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Identity for whom access is to be revoked
        /// Identity identityNameToBeRevokedAccess = "User1";
        ///
        /// // Grant access
        /// bool granted = adminToken.GrantCreate(identityNameToBeRevokedAccess, true, context);
        /// </code>
        /// </example>
        public static bool RevokeCreate(this AuthenticatedToken adminToken, string identityOrGroupName, bool isIdentity, ZentityContext context)
        {
            #region Parameter Validation
            ValidateParameters(context, adminToken);
            ValidateStrings("identityOrGroupName", identityOrGroupName);
            #endregion

            try
            {
                if (isIdentity)
                {
                    Identity user = DataAccess.GetIdentity(identityOrGroupName, context);
                    if (user != null)
                    {
                        bool revoked = user.RevokeCreate(adminToken, context);
                        return revoked;
                    }
                    else
                    {
                        return false;
                    }
                }

                Group group = DataAccess.GetGroup(identityOrGroupName, context);
                if (group != null)
                {
                    bool revoked = group.RevokeCreate(adminToken, context);
                    return revoked;
                }

                return false;
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }
        #endregion

        #region Retrieve Security Predicates

        /// <summary>
        /// Returns the security predicates.
        /// </summary>
        /// <returns>List of all available security predicate uri's.</returns>
        public static IEnumerable<string> GetSecurityPredicates()
        {
            IEnumerable<SecurityPredicate> predicates = SecurityPredicateAccess.SecurityPredicates;
            List<string> predicateUris = predicates.Select(pr => pr.Uri).ToList();
            predicateUris.Add(DataAccess.MemberOfUri);
            List<string> inversePredicateUris = predicates.Select(pr => pr.InverseUri).ToList();
            List<string> allUris = predicateUris.Concat(inversePredicateUris).ToList();
            return allUris;
        }

        /// <summary>
        /// Returns the security permissions hierarchy
        /// </summary>
        /// <returns>Returns hierarchical list of permissions along with priority of each permission.</returns>
        /// <remarks>The permissions are hierarchical, read, update, delete, owner in increasing order of priority.</remarks>
        public static Dictionary<string, int> GetPermissionsHierarchy()
        {
            Dictionary<string, int> perm = new Dictionary<string, int>(4);
            IEnumerable<SecurityPredicate> securityPredicates = SecurityPredicateAccess.SecurityPredicates
                .Where(sp => sp.Priority != 0); // Priority 0 predicates are repository level predicates.
            foreach (SecurityPredicate pr in securityPredicates)
            {
                perm.Add(pr.Name, pr.Priority);
            }

            return perm;
        }

        /// <summary>
        /// Returns the security permissions hierarchy
        /// </summary>
        /// <returns>Returns hierarchical list of inverse (deny) permissions along with priority of each permission.</returns>
        /// <remarks>The inverse permissions are hierarchical, deny-read, deny-update, deny-delete, deny-owner in decreasing order of priority.</remarks>
        public static Dictionary<string, int> GetInversePermissionsHierarchy()
        {
            Dictionary<string, int> perm = new Dictionary<string, int>(4);
            IEnumerable<SecurityPredicate> securityPredicates = SecurityPredicateAccess.SecurityPredicates
                .Where(sp => sp.Priority != 0); // Priority 0 predicates are repository level predicates.
            int highestPriority = securityPredicates.OrderByDescending(sp => sp.Priority).Select(sp => sp.Priority).First();
            foreach (SecurityPredicate pr in securityPredicates)
            {
                perm.Add("Deny" + pr.Name, (highestPriority - pr.Priority + 1));
            }

            return perm;
        }

        /// <summary>
        /// Returns permissions list which are not resource specific.
        /// </summary>
        /// <returns>Create and deny create permissions</returns>
        /// <remarks>Create and deny-create permissions are not resource specific, and hence they are called repository level permissions.</remarks>
        public static IEnumerable<string> GetRepositoryLevelPermissions()
        {
            ////Repository level permissions are the permissions with priority set to 0.
            IEnumerable<string> perm = SecurityPredicateAccess.SecurityPredicates
                .Where(sp => sp.Priority == 0).Select(sp => sp.Name);
            return perm;
        }

        #endregion

        #region Remove() overloads

        #region Remove permission on a single resource
        /// <summary>
        /// Removes given permission and its deny counterpart, e.g. Update and DenyUpdate from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resource">Resource on which the permission is to be Removed</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="identityOrGroupName">Name of the user or group to whom the permission is to be Removed</param>
        /// <param name="isIdentity">Indicate whether the name passed in the previous parameter represents a user or a group</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to Remove permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>true if Remove succeeds</returns>
        /// <remarks>
        /// Permission can be Removed from another user if the following conditions are satisfied
        /// <list type="bullet">
        /// <item>The AuthenticatedToken represents an admin user or owner of the resource</item>
        /// <item>The user from whom the permission is to be removed is not 'Guest' user.</item>
        /// </list>
        /// This method removes the given permission, and all permissions which have higher priority than the given permission.
        /// It also removes the inverse permission, if any is present, and all inverse permissions which have lower priority than the given
        /// permission.
        /// e.g. removing 'delete' will remove 'owner', 'deny-delete', and 'deny-owner'.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Identity for whom permission is to be removed
        /// string identityName = "User1";
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Remove permission
        /// bool removed = resource.Remove("Update", identityName, true, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static bool Remove<T>(
                                    this T resource, 
                                    string permissionName, 
                                    string identityOrGroupName,
                                    bool isIdentity, 
                                    ZentityContext context, 
                                    AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName, "identityOrGroupName", identityOrGroupName);
            ValidateParameters(context, userToken);
            #endregion

            if (isIdentity)
            {
                Identity user = DataAccess.GetIdentity(identityOrGroupName, context);
                if (user != null)
                {
                    bool removed = resource.Remove(permissionName, user, context, userToken);
                    return removed;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Group group = DataAccess.GetGroup(identityOrGroupName, context);
                if (group != null)
                {
                    bool removed = resource.Remove(permissionName, group, context, userToken);
                    return removed;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resource">Resource on which the permission is to be Removed</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="identity">Identity object from whom the permission is to be Removed.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to Remove permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>true if Remove succeeds</returns>
        /// <remarks>
        /// Permission can be Removed from another user if the following conditions are satisfied
        /// <list type="bullet">
        /// <item>The AuthenticatedToken represents an admin user or owner of the resource.</item>
        /// <item>The user from whom the permission is to be removed is not 'Guest' user.</item>
        /// </list>
        /// This method removes the given permission, and all permissions which have higher priority than the given permission.
        /// It also removes the inverse permission, if any is present, and all inverse permissions which have lower priority than the given
        /// permission.
        /// e.g. removing 'delete' will remove 'owner', 'deny-delete', and 'deny-owner'.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity for whom permission is to be removed
        /// Identity identity = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("User1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Remove access
        /// bool removed = resource.Remove("Update", identity, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static bool Remove<T>(
                                this T resource, 
                                string permissionName, 
                                Identity identity,
                                ZentityContext context, 
                                AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //-----------------------------------------------Algorithm---------------------------------------------------------
            //  Check for existence of the permission predicate
            //  If user is an administrator
            //      Return false (Can't Remove any permission from administrator)
            //  Else if user represented by userToken is owner of the resource or an administrator
            //      If identity == Guest
            //          If permission == 'Read'
            //              Remove deny-read permission if it exists. (Read is implicitly granted and hence it need not be removed.)
            //          Else
            //              Return true (Guests do not have any permissions other than 'Read' - no need to Remove)
            //          End If
            //      End If
            //      Else (Normal user)
            //          Remove the given permission (allow as well as deny, e.g. update or deny-update)
            //          and all permissions which are higher up in the predicates hierarchy.
            //          (e.g. removing update will remove update, deny-update, delete, deny-delete, owner and deny owner.)
            //      End If
            //  Else
            //      Return false (for revoking permission user must be owner of the resource or an administrator)
            //  End If
            //------------------------------------------------------------------------------------------------------------------
            #endregion

            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateIdentity(identity);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                return ExecuteRemove<T>(resource, permissionName, identity, context, userToken);
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Removes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resource">Resource on which the permission is to be Removed</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="group">Group object from whom the permission is to be Removed.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to Remove permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>true if Remove succeeds</returns>
        /// <remarks>
        /// Permission can be Removed from another user if the following conditions are satisfied
        /// <list type="bullet">
        /// <item>The AuthenticatedToken represents an admin user or owner of the resource.</item>
        /// <item>The user from whom the permission is to be removed is not 'Guest' user.</item>
        /// </list>
        /// This method removes the given permission, and all permissions which have higher priority than the given permission.
        /// It also removes the inverse permission, if any is present, and all inverse permissions which have lower priority than the given
        /// permission.
        /// e.g. removing 'delete' will remove 'owner', 'deny-delete', and 'deny-owner'.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group for which permission is to be removed
        /// Group group = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("Group1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Remove access
        /// bool removed = resource.Remove("Update", group, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static bool Remove<T>(
                                    this T resource, 
                                    string permissionName, 
                                    Group group,
                                    ZentityContext context, 
                                    AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //-----------------------------------------------Algorithm---------------------------------------------------------
            //  Check for existence of the permission predicate
            //  If user is an administrator
            //      Return false (Can't Remove any permission from administrator)
            //  Else if user represented by userToken is owner of the resource or an administrator
            //      Remove the given permission (allow as well as deny, e.g. update or deny-update)
            //      and all permissions which are higher up in the predicates hierarchy.
            //      (e.g. removing update will remove update, deny-update, delete, deny-delete, owner and deny owner.)
            //  Else
            //      Return false (for revoking permission user must be owner of the resource or an administrator)
            //  End If
            //------------------------------------------------------------------------------------------------------------------
            #endregion

            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateGroup(group);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                return ExecuteRemove<T>(resource, permissionName, group, context, userToken);
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        #endregion

        #region Remove permissions on a list of resources
        /// <summary>
        /// Removes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resources">List of resources on which the permission is to be Removed</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="identityOrGroupName">Name of the identity or group object from whom the permission is to be Removed.</param>
        /// <param name="isIdentity">Set to true if identity name is passed, false for group name.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to Remove permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>List of resources for which the Remove succeeded</returns>
        /// <remarks>
        /// Permission can be Removed from another user if the following conditions are satisfied
        /// <list type="bullet">
        /// <item>The AuthenticatedToken represents an admin user or owner of the resource</item>
        /// <item>The user from whom the permission is to be removed is not 'Guest' user.</item>
        /// </list>
        /// This method removes the given permission, and all permissions which have higher priority than the given permission.
        /// It also removes the inverse permission, if any is present, and all inverse permissions which have lower priority than the given
        /// permission.
        /// e.g. removing 'delete' will remove 'owner', 'deny-delete', and 'deny-owner'.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Identity for whom permission is to be removed
        /// string identity = "User1";
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Remove access
        /// IEnumerable&lt;Resource&gt; removedAccessResources
        ///     = resources.Remove("Update", identity, true, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static IEnumerable<T> Remove<T>(
                                            this IEnumerable<T> resources, 
                                            string permissionName, 
                                            string identityOrGroupName,
                                            bool isIdentity, 
                                            ZentityContext context, 
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName, "identityOrGroupName", identityOrGroupName);
            ValidateParameters(context, userToken);
            #endregion

            List<T> originalResources = resources.ToList();
            List<T> removedResources = new List<T>();
            foreach (T resource in originalResources)
            {
                bool removed = resource.Remove<T>(permissionName, identityOrGroupName, isIdentity, context, userToken);
                if (removed)
                {
                    removedResources.Add(resource);
                }
            }

            return removedResources;
        }

        /// <summary>
        /// Removes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resources">List of resources on which the permission is to be Removed</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="identity">Identity object from whom the permission is to be Removed.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to Remove permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>List of resources for which the Remove succeeded</returns>
        /// <remarks>
        /// <para>This method removes the given permission to the identity on the list of resources passed as parameter.
        /// It materializes the list of resources, iterating through the list, removes the permission to each resource.
        /// If remove is successful it adds the resource to the list of removed resources which is returned by this method.
        /// The user requesting remove (represented by the token) must be an administrator or owner of the resource.</para>
        /// <para>Remove will return false for admin identity / group and guest identity.</para>
        /// Permission can be Removed from another user if the following conditions are satisfied
        /// <list type="bullet">
        /// <item>The AuthenticatedToken represents an admin user or owner of the resource</item>
        /// <item>The user from whom the permission is to be removed is not 'Guest' user.</item>
        /// </list>
        /// <para>
        /// This method removes the given permission, and all permissions which have higher priority than the given permission.
        /// It also removes the inverse permission, if any is present, and all inverse permissions which have lower priority than the given
        /// permission.
        /// e.g. removing 'delete' will remove 'owner', 'deny-delete', and 'deny-owner'.
        /// </para>
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <example>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity for whom permission is to be removed
        /// Identity identity = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("User1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Remove access
        /// IEnumerable&lt;Resource&gt; removedAccessResources
        ///     = resources.Remove("Update", identity, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static IEnumerable<T> Remove<T>(
                                        this IEnumerable<T> resources, 
                                        string permissionName, 
                                        Identity identity,
                                        ZentityContext context, 
                                        AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateIdentity(identity);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                List<T> originalResources = resources.ToList();
                List<T> removedResources = new List<T>();
                foreach (T resource in originalResources)
                {
                    bool removed = ExecuteRemove<T>(resource, permissionName, identity, context, userToken);
                    if (removed)
                    {
                        removedResources.Add(resource);
                    }
                }

                return removedResources;
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Removes given permission from the identity or group on the given resource.
        /// </summary>
        /// <typeparam name="T">A type in the Zentity.Core.Resource hierarchy</typeparam>
        /// <param name="resources">List of resources on which the permission is to be Removed</param>
        /// <param name="permissionName">Permission name e.g. 'read'</param>
        /// <param name="group">Group object from whom the permission is to be Removed.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">Authenticated user who wants to Remove permission to another user. This user must be
        /// either administrator or owner of the resource.</param>
        /// <returns>List of resources for which the Remove succeeded</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// <para>This method removes the given permission to the identity on the list of resources passed as parameter.
        /// It materializes the list of resources, iterating through the list, removes the permission to each resource.
        /// If remove is successful it adds the resource to the list of removed resources which is returned by this method.
        /// The user requesting remove (represented by the token) must be an administrator or owner of the resource.</para>
        /// <para>Remove will return false for admin identity / group and guest identity.</para>
        /// Permission can be Removed from another user if the following conditions are satisfied
        /// <list type="bullet">
        /// <item>The AuthenticatedToken represents an admin user or owner of the resource.</item>
        /// <item>The user from whom the permission is to be removed is not 'Guest' user.</item>
        /// </list>
        /// This method removes the given permission, and all permissions which have higher priority than the given permission.
        /// It also removes the inverse permission, if any is present, and all inverse permissions which have lower priority than the given
        /// permission.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>Pre-requisites to run this sample
        /// <list type="bullet">
        /// <item>References to the following assemblies should be added to the application running this sample
        /// <list type="bullet">
        /// <item>System.Data.Entity</item>
        /// <item>System.IdentityModel</item>
        /// <item>Zentity.Core</item>
        /// <item>Zentity.Security.Authentication</item>
        /// <item>Zentity.Security.AuthenticationProvider</item>
        /// <item>Zentity.Security.Authorization</item>
        /// <item>Zentity.Security.AuthorizationHelper</item>
        /// </list>
        /// </item>
        /// <item>Application configuration file should contain all sections given in the sample configuration file in this help file.</item>
        /// <item>The users, groups and resources mentioned in this sample should be existing in the repository. Use the UserManager class
        /// for creating users and groups.</item>
        /// </list>
        /// </para>
        /// <code>
        /// // Login as ResourceOwner
        /// SecurityToken credentialToken = new UserNameSecurityToken("ResourceOwner", "ResourceOwner@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken resourceOwnerToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group for which permission is to be removed
        /// Group group = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("NewGroup", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Remove access
        /// IEnumerable&lt;Resource&gt; removedAccessResources
        ///     = resources.Remove("Update", group, context, resourceOwnerToken);
        /// </code>
        /// </example>
        public static IEnumerable<T> Remove<T>(
                                            this IEnumerable<T> resources, 
                                            string permissionName, 
                                            Group group,
                                            ZentityContext context, 
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateStrings("permissionName", permissionName);
            ValidateGroup(group);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                List<T> originalResources = resources.ToList();
                List<T> removedResources = new List<T>();
                foreach (T resource in originalResources)
                {
                    bool removed = ExecuteRemove<T>(resource, permissionName, group, context, userToken);
                    if (removed)
                    {
                        removedResources.Add(resource);
                    }
                }

                return removedResources;
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        #endregion

        #region Remove Create Permission

        /// <summary>
        /// Removes create permission from identity.
        /// </summary>
        /// <param name="identity">Identity object on which permission is to be assigned.</param>
        /// <param name="adminToken">Token for a user from administrator's group.</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if Remove succeeds</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// Removes create and deny-create permission. The user requesting remove-create (represented by the token) must be admin user.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Admin", "XXXX"); //Supply correct password
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity for whom access is to be removed
        /// Identity identity = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("User1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Remove access
        /// bool removed = identity.RemoveCreate(adminToken, context);
        /// </code>
        /// </example>
        public static bool RemoveCreate(this Identity identity, AuthenticatedToken adminToken, ZentityContext context)
        {
            #region Algorithm
            //----------------------------------------------Algorithm--------------------------------------------------
            //  If the identity is an administrator
            //      return false (permission cannot be removed from admin)
            //  Else if the identity is guest
            //      return true but do not add deny relationship (guests anyway do not have create permission)
            //  Else
            //      If userToken represents a user from Administrators group
            //          Remove create or deny-create from identity
            //      Else
            //          Return false. (Only administrators can remove create permission)
            //      End If
            //  End If
            //----------------------------------------------------------------------------------------------------------
            #endregion
            #region Parameter Validation
            ValidateParameters(context, adminToken);
            ValidateIdentity(identity);
            #endregion

            try
            {
                ////If the identity is an administrator return false (permission cannot be removed from admin)
                if (DataAccess.IsAdmin(identity))
                {
                    return false;
                }

                ////if the identity is guest return true but do not add deny relationship (guests anyway do not have create permission)
                if (DataAccess.IsGuest(identity.IdentityName))
                {
                    return true;
                }

                ////Only administrators can remove create permission
                if (DataAccess.IsAdmin(adminToken.IdentityName, context))
                {
                    bool removed = ExecuteRemoveCreate(identity, context);
                    return removed;
                }
                else
                {
                    return false;
                }
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }

        /// <summary>
        /// Removes create permission from group.
        /// </summary>
        /// <param name="group">Group object on which permission is to be assigned.</param>
        /// <param name="adminToken">Token for a user from administrator's group.</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if Remove succeeds</returns>
        /// <exception cref="Zentity.Security.Authorization.AuthorizationException">Thrown when application is not configured correctly
        /// or a database error occurs, or the userToken passed is not valid.</exception>
        /// <remarks>
        /// Removes create and deny-create permission. The user requesting remove-create (represented by the token) must be admin user.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Admin", "XXXX"); //Supply correct password
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group for which access is to be removed
        /// Group group = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("Group1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Remove access
        /// bool removed = group.RemoveCreate(adminToken, context);
        /// </code>
        /// </example>
        public static bool RemoveCreate(this Group group, AuthenticatedToken adminToken, ZentityContext context)
        {
            #region Algorithm
            //----------------------------------------------Algorithm--------------------------------------------------
            //  If the group is administrator's group
            //      return false (permission cannot be removed from admin)
            //  Else if userToken represents a user from Administrators group
            //      Remove create or deny-create from group
            //  Else
            //      Return false. (Only administrators can remove create permission)
            //  End If
            //----------------------------------------------------------------------------------------------------------
            #endregion
            #region Parameter Validation
            ValidateParameters(context, adminToken);
            ValidateGroup(group);
            #endregion

            try
            {
                //// If the group is administrator's group return false (permission cannot be removed from admin)
                if (DataAccess.IsAdmin(group))
                {
                    return false;
                }
                else if (DataAccess.IsAdmin(adminToken.IdentityName, context))
                {
                    //// If userToken represents a user from Administrators group
                    //// Remove create or deny-create from group
                    return ExecuteRemoveCreate(group, context);
                }
                else
                {
                    return false;
                }
            }
            catch (TypeInitializationException ex)
            {
                //// thrown when application is not configured correctly
                throw new AuthorizationException(ConstantStrings.ConfigurationException, ex);
            }
            catch (EntityException ex)
            {
                throw new AuthorizationException(ConstantStrings.DatabaseException, ex);
            }
        }
        #endregion

        #endregion

        #region SetPermissions overloads
        /// <summary>
        /// Sets permissions as per the set map.
        /// </summary>
        /// <param name="resource">Resource on which permissions are to be granted/revoked/removed</param>
        /// <param name="permissionMap">Permission map with permission names and allow/deny status of each.</param>
        /// <param name="identity">Identity to whom permissions are to be granted.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <param name="token">Logged on user token.</param>
        /// <returns>True if all permissions grant/revoke/remove works successfully.</returns>
        /// <remarks>
        /// This method compares the state of a given permission with the current state in the repository,
        /// and calls grant/revoke/remove as necessary.
        /// e.g. if the current state of update is 'deny-update' and the new state is 'allow-update' then - update is granted to the user and
        /// deny update is removed.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static bool SetPermissionMap(
                                        this Resource resource, 
                                        IEnumerable<PermissionMap> permissionMap,
                                        Identity identity, 
                                        ZentityContext context, 
                                        AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateSetPermissionMapParameters(permissionMap, identity, context, token);
            #endregion

            ////Then call Grant() or Revoke() for each permission
            bool success = true;
            IEnumerable<PermissionMap> current = resource.GetPermissionMap(identity, context, token);
            foreach (PermissionMap map in permissionMap)
            {
                PermissionMap currMap = current.Where(pm => pm.Permission.Equals(
                                                                       map.Permission,
                                                                       StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (map.Allow && !currMap.Allow)
                {
                    success = resource.Grant(map.Permission, identity, context, token);
                    if (!success)
                    {
                        break;
                    }
                }
                else if (map.Deny && !currMap.Deny)
                {
                    success = resource.Revoke(map.Permission, identity, context, token);
                    if (!success)
                    {
                        break;
                    }
                }
                else if (!map.Allow && !map.Deny
                    && (currMap.Allow || currMap.Deny)) 
                {
                    // remove the existing permissions
                    success = resource.Remove(map.Permission, identity, context, token);
                    if (!success)
                    {
                        break;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Sets permissions as per the set map.
        /// </summary>
        /// <param name="resource">Resource on which permissions are to be granted/revoked/removed</param>
        /// <param name="permissionMap">Permission map with permission names and allow/deny status of each</param>
        /// <param name="group">group to which permissions are to be granted.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <param name="token">Logged on user token.</param>
        /// <returns>True if all permissions grant/revoke/remove works successfully.</returns>
        /// <remarks>
        /// This method compares the state of a given permission with the current state in the repository,
        /// and calls grant/revoke/remove as necessary.
        /// e.g. if the current state of update is 'deny-update' and the new state is 'allow-update' then - update is granted to the user and
        /// deny update is removed.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static bool SetPermissionMap(
                                        this Resource resource, 
                                        IEnumerable<PermissionMap> permissionMap,
                                        Group group, 
                                        ZentityContext context, 
                                        AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateSetPermissionMapParameters(permissionMap, group, context, token);
            #endregion

            ////Call Remove()
            ////Then call Grant() or Revoke() for each permission
            bool success = true;
            IEnumerable<PermissionMap> current = resource.GetPermissionMap(group, context, token);
            foreach (PermissionMap map in permissionMap)
            {
                PermissionMap currMap = current.Where(pm => pm.Permission.Equals(
                                                                    map.Permission,
                                                                    StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (map.Allow && !currMap.Allow)
                {
                    success = resource.Grant(map.Permission, group, context, token);
                    if (!success)
                    {
                        break;
                    }
                }
                else if (map.Deny && !currMap.Deny)
                {
                    success = resource.Revoke(map.Permission, group, context, token);
                    if (!success)
                    {
                        break;
                    }
                }
                else if (!map.Allow && !map.Deny
                    && (currMap.Allow || currMap.Deny)) 
                {
                    // remove the existing permissions
                    success = resource.Remove(map.Permission, group, context, token);
                    if (!success)
                    {
                        break;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Sets the create permissions for the user as per the permission map.
        /// </summary>
        /// <param name="identity">Identity object to whom the create is to be granted or denied.</param>
        /// <param name="map">Permission map object.</param>
        /// <param name="context">Zentity context</param>
        /// <param name="token">AuthenticatedToken for the logged on user. The logged on user must be an admin for
        /// granting / revoking deny.</param>
        /// <returns>True if grant/revoke is successful.</returns>
        /// <remarks>
        /// This method checks the current state of create permission for the group,
        /// and grants/revokes/removes create comparing the state with the new requested state.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static bool SetCreatePermissionMap(
                                            this Identity identity, 
                                            PermissionMap map,
                                            ZentityContext context, 
                                            AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateSetPermissionMapParameters(map, context, token);
            #endregion

            ////Then call Grant() or Revoke() for each permission
            bool success = true;
            PermissionMap currMap = identity.GetCreatePermissionMap(context, token);
            if (map.Allow && !currMap.Allow)
            {
                success = identity.GrantCreate(token, context);
            }
            else if (map.Deny && !currMap.Deny)
            {
                success = identity.RevokeCreate(token, context);
            }
            else if (!map.Allow && !map.Deny
                && (currMap.Allow || currMap.Deny)) 
            {
                // remove the existing permissions
                success = identity.RemoveCreate(token, context);
            }

            return success;
        }

        /// <summary>
        /// Sets the create permissions for the given group.
        /// </summary>
        /// <param name="group">Group to which create is to be granted/denied.</param>
        /// <param name="map">Permission map for create</param>
        /// <param name="context">Zentity context</param>
        /// <param name="token">AuthenticatedToken of the logged on user. The user must be an admin.</param>
        /// <returns>True if grant/revoke is successful.</returns>
        /// <remarks>
        /// This method checks the current state of create permission for the group,
        /// and grants/revokes/removes create comparing the state with the new requested state.
        ///
        /// <para>IMPORTANT - <list type="bullet">
        /// <item>The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</item>
        /// <item>Call context.SaveChanges() after calling this method. This method leaves it to the caller to save or abort the changes.
        /// Permissions will not reflect in the repository unless SaveChanges() are called.</item>
        /// <item>This method does NOT run transaction. The caller should call this method under TransactionScope, if necessary.</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static bool SetCreatePermissionMap(
                                                this Group group, 
                                                PermissionMap map,
                                                ZentityContext context, 
                                                AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateSetPermissionMapParameters(map, context, token);
            #endregion

            ////Then call Grant() or Revoke() for each permission
            bool success = false;
            PermissionMap currMap = group.GetCreatePermissionMap(context, token);
            if (map.Allow && !currMap.Allow)
            {
                success = group.GrantCreate(token, context);
            }
            else if (map.Deny && !currMap.Deny)
            {
                success = group.RevokeCreate(token, context);
            }
            else if (!map.Allow && !map.Deny
                && (currMap.Allow || currMap.Deny)) 
            {
                // remove the existing permissions
                success = group.RemoveCreate(token, context);
            }

            return success;
        }
        #endregion

        #region Private Methods

        #region ExecuteGrant() overloads

        /// <summary>
        /// Executes the grant permission command.
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="context">Zentity context.</param>
        /// <param name="userToken">The user authenticated token.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool ExecuteGrant<T>(
                                        T resource, 
                                        string permissionName,
                                        Identity identity, 
                                        ZentityContext context, 
                                        AuthenticatedToken userToken) where T : Resource
        {
            //-----------------------------------Algorithm----------------------------------
            //      Check for existence of permission predicates.
            //      If user is an administrator
            //          Return true (No need to actually store permissions)
            //      Else if user == Guest and permission is not read
            //          Return false (Guests cannot be granted any permissions except read)
            //      Else (Normal user or guest with read permission)
            //              If user represented by token is owner of the resource or an administrator
            //                  Grant the requested permission, plus all permissions which have lower priority than the requested one.
            //                  (e.g. Granting 'delete' should also grant 'update'.)
            //                  In case of read we do not actually create the relationship.
            //                  Remove the denied permissions, if any for these. That is, if delete is granted, deny-delete is removed.
            //              Else
            //                  Return false (only owner or an admin can grant permissions on a resource)
            //              End If
            //          End If
            //      End If
            //------------------------------------------------------------------------------
            if (!SecurityPredicateAccess.Exists(permissionName))
            {
                throw new ArgumentException(ConstantStrings.InvalidPermissionName, "permissionName");
            }
            else
            {
                if (DataAccess.IsAdmin(identity))
                {
                    return true;
                }
                else if (DataAccess.IsGuest(identity.IdentityName) && !permissionName.Equals("Read"))
                {
                    return false;
                }
                else 
                {
                    // normal user
                    if (DataAccess.IsOwner(userToken, resource, context) || DataAccess.IsAdmin(userToken.IdentityName, context))
                    {
                        bool granted = GrantPermissions<T>(resource, permissionName, identity, context);
                        return granted;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Executes the grant permission command.
        /// </summary>
        /// <typeparam name="T">The resource type</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="group">The group.</param>
        /// <param name="context">The zentity context.</param>
        /// <param name="userToken">The user authenticated token.</param>
        /// <returns>System.Boolean; true if the grant is executed.</returns>
        private static bool ExecuteGrant<T>(
                                        T resource, 
                                        string permissionName, 
                                        Group group, 
                                        ZentityContext context,
                                        AuthenticatedToken userToken) where T : Resource
        {
            //-----------------------------------Algorithm----------------------------------
            //      Check for existence of permission predicates.
            //      If group is administrator's group
            //          Return true (No need to actually store permissions)
            //      Else (Normal group)
            //              If user represented by token is owner of the resource or an administrator
            //                  Grant the requested permission, plus all permissions which have lower priority than the requested one.
            //                  (e.g. Granting 'delete' should also grant 'update'.)
            //                  In case of read we do not actually create the relationship.
            //                  Remove the denied permissions, if any for these. That is, if delete is granted, deny-delete is removed.
            //              Else
            //                  Return false (only owner or an admin can grant permissions on a resource)
            //              End If
            //          End If
            //      End If
            //------------------------------------------------------------------------------
            if (permissionName.Equals("Create", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(ConstantStrings.InvalidPermissionName, "permissionName");
            }

            if (!SecurityPredicateAccess.Exists(permissionName))
            {
                throw new ArgumentException(ConstantStrings.InvalidPermissionName, "permissionName");
            }

            if (DataAccess.IsAdmin(group))
            {
                return true;
            }
            else 
            {
                // normal group
                if (DataAccess.IsOwner(userToken, resource, context) || DataAccess.IsAdmin(userToken.IdentityName, context))
                {
                    bool success = GrantPermissions<T>(resource, permissionName, group, context);
                    return success;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region ExecuteRevoke() overloads
        /// <summary>
        /// Executes the revoke permissions command.
        /// </summary>
        /// <typeparam name="T">Resource type</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="context">The zentity context.</param>
        /// <param name="userToken">The user authenticated token.</param>
        /// <returns>System.Boolean; true if the grant has been revoked, false otherwise.</returns>
        private static bool ExecuteRevoke<T>(
                                            T resource, 
                                            string permissionName, 
                                            Identity identity, 
                                            ZentityContext context,
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //---------------------------------------------Algorithm---------------------------------------------------------
            //  Check for existence of permission
            //  If user is an administrator
            //      Return false (Can't revoke any permission from administrator)
            //  Else if token belongs to owner of the resource or an administrator
            //      If user == Guest
            //          If permission == read
            //              Grant 'deny-read' (revoke read permission means granting deny-read)
            //          Else
            //              Return true - (the permission need not be revoked from Guest, since he is not granted the permission too.)
            //          End If
            //      Else (Normal user)
            //          Grant deny permission for the permission and all permissions with higher priority
            //          Remove allow permission for all the permissions above
            //  Else
            //      Return false - (for revoking a permission user represented by token needs to be owner of the resource or an administrator.)
            //  End If
            //---------------------------------------------------------------------------------------------------------------
            #endregion

            // Check for existence of permission
            if (!SecurityPredicateAccess.Exists(permissionName))
            {
                throw new ArgumentException(ConstantStrings.InvalidPermissionName, "permissionName");
            }
            else
            {
                ////If user is an administrator return false (Can't revoke any permission from administrator)
                if (DataAccess.IsAdmin(identity))
                {
                    return false;
                }

                ////For revoking a permission user represented by token needs to be owner of the resource or an administrator.
                if (DataAccess.IsOwner(userToken, resource, context) || DataAccess.IsAdmin(userToken.IdentityName, context))
                {
                    ////Revoke read from Guest
                    if (DataAccess.IsGuest(identity.IdentityName))
                    {
                        if (permissionName.Equals("Read"))
                        {
                            bool revoked = RevokePermissions<T>(resource, permissionName, identity, context);
                            return revoked;
                        }
                        else
                        {
                            ////the permission need not be revoked from Guest, since he is not granted the permission too.
                            return true;
                        }
                    }
                    else
                    {
                        ////Grant deny permission for the permission and all permissions with higher priority
                        ////Remove allow permission for all the permissions above
                        bool revoked = RevokePermissions<T>(resource, permissionName, identity, context);
                        return revoked;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Executes the revoke permissions command.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="group">The group.</param>
        /// <param name="context">The zentity context.</param>
        /// <param name="userToken">The users authenticated token.</param>
        /// <returns>System.Boolean; true if the grant has been revoked, false otherwise.</returns>
        private static bool ExecuteRevoke<T>(
                                            T resource, 
                                            string permissionName, 
                                            Group group, 
                                            ZentityContext context,
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //---------------------------------------------Algorithm---------------------------------------------------------
            //  Check for existence of permission
            //  If group is administrator's group
            //      Return false (Can't revoke any permission from administrators)
            //  Else if token belongs to owner of the resource or an administrator
            //      Grant deny permission for the permission and all permissions with higher priority
            //      Remove allow permission for all the permissions above
            //  Else
            //      Return false - (for revoking a permission user represented by token needs to be owner of the resource or an administrator.)
            //  End If
            //---------------------------------------------------------------------------------------------------------------
            #endregion

            ////Check for existence of permission
            if (!SecurityPredicateAccess.Exists(permissionName))
            {
                throw new ArgumentException(ConstantStrings.InvalidPermissionName, "permissionName");
            }
            else
            {
                ////If group is administrator's group return false (Can't revoke any permission from administrators)
                if (DataAccess.IsAdmin(group))
                {
                    return false;
                }

                ////for revoking a permission user represented by token needs to be owner of the resource or an administrator.
                if (DataAccess.IsOwner(userToken, resource, context) || DataAccess.IsAdmin(userToken.IdentityName, context))
                {
                    ////Grant deny permission for the permission and all permissions with higher priority
                    ////Remove allow permission for all the permissions above
                    bool revoked = RevokePermissions<T>(resource, permissionName, group, context);
                    return revoked;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region GrantPermissions() overloads
        /// <summary>
        /// Grants the permissions.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool GrantPermissions<T>(T resource, string permissionName, Identity identity, ZentityContext context)
            where T : Resource
        {
            ////string permissionPredicateUri = SecurityPredicateAccess.GetPredicateUri(permissionName);
            ////Grant the permission for each of the predicates in the hierarchy which have lower priority than the given predicate.
            IEnumerable<SecurityPredicate> lowerPriorityPredicates = SecurityPredicateAccess.GetLowerPriorityPredicates(permissionName);
            if (lowerPriorityPredicates == null || lowerPriorityPredicates.Count() == 0)
            {
                return false;
            }

            foreach (SecurityPredicate predicate in lowerPriorityPredicates)
            {
                ////Read permission is not actually stored. It is assumed by default.
                if (!string.Equals(predicate.Uri, SecurityPredicateAccess.GetPredicateUri("Read"), StringComparison.OrdinalIgnoreCase))
                {
                    ////Grant permission
                    if (!identity.GrantAuthorization<T>(predicate.Uri, resource, context))
                    {
                        return false;
                    }
                }

                if (!identity.RevokeAuthorization(predicate.InverseUri, resource, context))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Grants the permissions.
        /// </summary>
        /// <typeparam name="T">Resource type</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="group">The group.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool GrantPermissions<T>(T resource, string permissionName, Group group, ZentityContext context) where T : Resource
        {
            ////string permissionPredicateUri = SecurityPredicateAccess.GetPredicateUri(permissionName);
            IEnumerable<SecurityPredicate> lowerPriorityPredicates = SecurityPredicateAccess.GetLowerPriorityPredicates(permissionName);
            if (lowerPriorityPredicates == null || lowerPriorityPredicates.Count() == 0)
            {
                return false;
            }

            foreach (SecurityPredicate predicate in lowerPriorityPredicates)
            {
                ////Read permission is not actually stored. It is assumed by default.
                if (!string.Equals(predicate.Uri, SecurityPredicateAccess.GetPredicateUri("Read"), StringComparison.OrdinalIgnoreCase))
                {
                    if (!group.GrantAuthorization(predicate.Uri, resource, context))
                    {
                        return false;
                    }
                }

                if (!group.RevokeAuthorization(predicate.InverseUri, resource, context))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region RevokePermissions() overloads
        /// <summary>
        /// Revokes the permissions.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="context">The zentity context.</param>
        /// <returns>System.Boolean; true if the grant is revoked, false otherwise.</returns>
        private static bool RevokePermissions<T>(T resource, string permissionName, Identity identity, ZentityContext context)
            where T : Resource
        {
            ////For guest we need not revoke update, delete and owner permissions since guests can never be granted these permissions.
            if (DataAccess.IsGuest(identity.IdentityName))
            {
                if (permissionName.Equals("Read", StringComparison.OrdinalIgnoreCase))
                {
                    SecurityPredicate readPredicate = SecurityPredicateAccess.SecurityPredicates
                                                                                .Where(pr => pr.Name.Equals(
                                                                                                        permissionName,
                                                                                                        StringComparison.OrdinalIgnoreCase))
                                                                                 .FirstOrDefault();
                    if (!identity.GrantAuthorization<T>(readPredicate.InverseUri, resource, context))
                    {
                        return false;
                    }

                    return true;

                    ////read is not explicitly granted hence there is no need to remove read-grant.
                }

                return false; // cant revoke any permission other than read from guest
            }

            ////a registered user. For this user we need to revoke the given permission plus all higher priority permissions.
            IEnumerable<SecurityPredicate> higherPriorityPredicates = SecurityPredicateAccess.GetHigherPriorityPredicates(permissionName);
            if (higherPriorityPredicates == null || higherPriorityPredicates.Count() == 0)
            {
                return false;
            }

            foreach (SecurityPredicate predicate in higherPriorityPredicates)
            {
                if (!identity.GrantAuthorization<T>(predicate.InverseUri, resource, context))
                {
                    return false;
                }
                else if (!identity.RevokeAuthorization<T>(predicate.Uri, resource, context))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Revokes the permissions.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="group">The group.</param>
        /// <param name="context">The zentity context.</param>
        /// <returns>System.Boolean; true if the premission is revoked, false otherwise.</returns>
        private static bool RevokePermissions<T>(T resource, string permissionName, Group group, ZentityContext context)
            where T : Resource
        {
            IEnumerable<SecurityPredicate> higherPriorityPredicates = SecurityPredicateAccess.GetHigherPriorityPredicates(permissionName);
            if (higherPriorityPredicates == null || higherPriorityPredicates.Count() == 0)
            {
                return false;
            }

            foreach (SecurityPredicate predicate in higherPriorityPredicates)
            {
                if (!group.GrantAuthorization<T>(predicate.InverseUri, resource, context))
                {
                    return false;
                }
                else if (!group.RevokeAuthorization<T>(predicate.Uri, resource, context))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region ExecuteGrantCreate() overloads
        /// <summary>
        /// Executes the grant create command.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool ExecuteGrantCreate(Identity identity, ZentityContext context)
        {
            ////Add a relationship 'identity hascreateaccess identity'.
            ////and remove relationship 'identity denycreateaccess identity' if it exists.
            string createUri = SecurityPredicateAccess.GetPredicateUri("Create");
            string denyCreateUri = SecurityPredicateAccess.GetInverseUri("Create");
            bool createGranted = identity.GrantAuthorization(createUri, context);
            bool denyCreateRevoked = identity.RevokeAuthorization(denyCreateUri, context);
            return createGranted && denyCreateRevoked;
        }

        /// <summary>
        /// Executes the grant create command.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool ExecuteGrantCreate(Group group, ZentityContext context)
        {
            ////Add a relationship 'group hascreateaccess group'.
            ////and remove relationship 'group denycreateaccess group' if it exists.
            string createUri = SecurityPredicateAccess.GetPredicateUri("Create");
            string denyCreateUri = SecurityPredicateAccess.GetInverseUri("Create");
            bool createGranted = group.GrantAuthorization(createUri, context);
            bool denyCreateRevoked = group.RevokeAuthorization(denyCreateUri, context);
            return createGranted && denyCreateRevoked;
        }
        #endregion

        #region ExecuteRevokeCreate() overloads
        /// <summary>
        /// Executes the revoke create command.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="context">The zentity context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool ExecuteRevokeCreate(Group group, ZentityContext context)
        {
            ////remove  relationship 'group hascreateaccess group'.
            ////and add relationship 'group denycreateaccess group'
            string createUri = SecurityPredicateAccess.GetPredicateUri("Create");
            string denyCreateUri = SecurityPredicateAccess.GetInverseUri("Create");
            bool createRevoked = group.RevokeAuthorization(createUri, context);
            bool denyCreateGranted = group.GrantAuthorization(denyCreateUri, context);
            return createRevoked && denyCreateGranted;
        }

        /// <summary>
        /// Executes the revoke create command.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool ExecuteRevokeCreate(Identity identity, ZentityContext context)
        {
            ////remove  relationship 'identity hascreateaccess identity'.
            ////and add relationship 'identity denycreateaccess identity'
            string createUri = SecurityPredicateAccess.GetPredicateUri("Create");
            string denyCreateUri = SecurityPredicateAccess.GetInverseUri("Create");
            bool createRevoked = identity.RevokeAuthorization(createUri, context);
            bool denyCreateGranted = identity.GrantAuthorization(denyCreateUri, context);
            return createRevoked && denyCreateGranted;
        }
        #endregion

        #region ExecuteRemove() overloads
        /// <summary>
        /// Executes the remove command.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="context">The zentity context.</param>
        /// <param name="userToken">The user authenticated token.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool ExecuteRemove<T>(
                                            T resource, 
                                            string permissionName, 
                                            Identity identity, 
                                            ZentityContext context,
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //-----------------------------------------------Algorithm---------------------------------------------------------
            //  Check for existence of the permission predicate
            //  If user is an administrator
            //      Return false (Can't Remove any permission from administrator)
            //  Else if user represented userToken is owner of the resource or an administrator
            //      If identity == Guest
            //          If permission == 'Read'
            //              Remove deny-read permission if it exists. (Read is implicitly granted and hence it need not be removed.)
            //          Else
            //              Return true (Guests do not have any permissions other than 'Read' - no need to Remove)
            //          End If
            //      End If
            //      Else (Normal user)
            //          Remove the given permission (allow as well as deny, e.g. update or deny-update)
            //          and all permissions which are higher up in the predicates hierarchy.
            //          (e.g. removing update will remove update, deny-update, delete, deny-delete, owner and deny owner.)
            //      End If
            //  Else
            //      Return false (for revoking permission user must be owner of the resource or an administrator)
            //  End If
            //------------------------------------------------------------------------------------------------------------------
            #endregion

            ////Check for existence of the permission predicate
            if (!SecurityPredicateAccess.Exists(permissionName))
            {
                throw new ArgumentException(ConstantStrings.InvalidPermissionName, "permissionName");
            }
            else
            {
                ////If user is an administrator return false
                ////(Can't Remove any permission from administrator)
                if (DataAccess.IsAdmin(identity))
                {
                    return false;
                }

                ////For revoking permission user must be owner of the resource or an administrator
                if (DataAccess.IsOwner(userToken, resource, context) || DataAccess.IsAdmin(userToken.IdentityName, context))
                {
                    if (DataAccess.IsGuest(identity.IdentityName))
                    {
                        ////Remove deny-read permission if it exists. (Read is implicitly granted and hence it need not be removed.)
                        if (permissionName.Equals("Read", StringComparison.OrdinalIgnoreCase))
                        {
                            bool removed = RemovePermissions<T>(resource, permissionName, identity, context);
                            return removed;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (DataAccess.IsAdmin(identity))
                        {
                            return false;
                        }

                        ////Remove the given permission (allow as well as deny, e.g. update or deny-update)
                        ////and all permissions which are higher up in the predicates hierarchy.
                        bool removed = RemovePermissions<T>(resource, permissionName, identity, context);
                        return removed;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Executes the remove command.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="group">The group.</param>
        /// <param name="context">The context.</param>
        /// <param name="userToken">The user token.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool ExecuteRemove<T>(
                                            T resource, 
                                            string permissionName, 
                                            Group group, 
                                            ZentityContext context,
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Algorithm
            //-----------------------------------------------Algorithm---------------------------------------------------------
            //  Check for existence of the permission predicate
            //  If user is an administrator
            //      Return false (Can't Remove any permission from administrator)
            //  Else if user represented userToken is owner of the resource or an administrator
            //      Remove the given permission (allow as well as deny, e.g. update or deny-update)
            //      and all permissions which are higher up in the predicates hierarchy.
            //      (e.g. removing update will remove update, deny-update, delete, deny-delete, owner and deny owner.)
            //  Else
            //      Return false (for revoking permission user must be owner of the resource or an administrator)
            //  End If
            //------------------------------------------------------------------------------------------------------------------
            #endregion
            if (!SecurityPredicateAccess.Exists(permissionName))
            {
                throw new ArgumentException(ConstantStrings.InvalidPermissionName, "permissionName");
            }
            else
            {
                if (DataAccess.IsAdmin(group))
                {
                    return false;
                }

                if (DataAccess.IsOwner(userToken, resource, context) || DataAccess.IsAdmin(userToken.IdentityName, context))
                {
                    bool removed = RemovePermissions<T>(resource, permissionName, group, context);
                    return removed;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region RemovePermissions() overloads

        /// <summary>
        /// Removes the permissions.
        /// </summary>
        /// <typeparam name="T">Resource type</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool RemovePermissions<T>(T resource, string permissionName, Identity identity, ZentityContext context)
            where T : Resource
        {
            IEnumerable<SecurityPredicate> lowerPriorityPredicates = SecurityPredicateAccess.GetLowerPriorityPredicates(permissionName);
            IEnumerable<SecurityPredicate> higherPriorityPredicates = SecurityPredicateAccess.GetHigherPriorityPredicates(permissionName);
            if (lowerPriorityPredicates == null || lowerPriorityPredicates.Count() == 0)
            {
                return false;
            }

            foreach (SecurityPredicate predicate in higherPriorityPredicates)
            {
                if (!identity.RevokeAuthorization<T>(predicate.Uri, resource, context))
                {
                    return false;
                }
            }

            foreach (SecurityPredicate predicate in lowerPriorityPredicates)
            {
                if (!identity.RevokeAuthorization<T>(predicate.InverseUri, resource, context))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes the permissions.
        /// </summary>
        /// <typeparam name="T">Resour</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="group">The group.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool RemovePermissions<T>(T resource, string permissionName, Group group, ZentityContext context)
            where T : Resource
        {
            IEnumerable<SecurityPredicate> lowerPriorityPredicates = SecurityPredicateAccess.GetLowerPriorityPredicates(permissionName);
            IEnumerable<SecurityPredicate> higherPriorityPredicates = SecurityPredicateAccess.GetHigherPriorityPredicates(permissionName);
            if (lowerPriorityPredicates == null || lowerPriorityPredicates.Count() == 0)
            {
                return false;
            }

            foreach (SecurityPredicate predicate in higherPriorityPredicates)
            {
                if (!group.RevokeAuthorization<T>(predicate.Uri, resource, context))
                {
                    return false;
                }
            }

            foreach (SecurityPredicate predicate in lowerPriorityPredicates)
            {
                if (!group.RevokeAuthorization<T>(predicate.InverseUri, resource, context))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region ExecuteRemoveCreate() overloads

        /// <summary>
        /// Executes the remove create command.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool ExecuteRemoveCreate(Group group, ZentityContext context)
        {
            ////remove  relationship 'group hascreateaccess group'.
            ////remove relationship 'group denycreateaccess group'
            string createUri = SecurityPredicateAccess.GetPredicateUri("Create");
            string denyCreateUri = SecurityPredicateAccess.GetInverseUri("Create");
            bool createRevoked = group.RevokeAuthorization(createUri, context);
            bool denyCreateRevoked = group.RevokeAuthorization(denyCreateUri, context);
            return createRevoked && denyCreateRevoked;
        }

        /// <summary>
        /// Executes the remove create command.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool ExecuteRemoveCreate(Identity identity, ZentityContext context)
        {
            ////remove  relationship 'identity hascreateaccess identity'.
            ////remove relationship 'identity denycreateaccess identity'
            string createUri = SecurityPredicateAccess.GetPredicateUri("Create");
            string denyCreateUri = SecurityPredicateAccess.GetInverseUri("Create");
            bool createRevoked = identity.RevokeAuthorization(createUri, context);
            bool denyCreateRevoked = identity.RevokeAuthorization(denyCreateUri, context);
            return createRevoked && denyCreateRevoked;
        }
        #endregion

        #region Parameter validation methods

        /// <summary>
        /// Validates the set permission map parameters.
        /// </summary>
        /// <param name="permissionMap">The permission map.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="context">The context.</param>
        /// <param name="token">The token.</param>
        private static void ValidateSetPermissionMapParameters(
                                                            IEnumerable<PermissionMap> permissionMap,
                                                            Identity identity, 
                                                            ZentityContext context, 
                                                            AuthenticatedToken token)
        {
            ValidateToken(token);
            ValidatePermissionMap(permissionMap);
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
        }

        /// <summary>
        /// Validates the set permission map parameters.
        /// </summary>
        /// <param name="permissionMap">The permission map.</param>
        /// <param name="group">The group.</param>
        /// <param name="context">The context.</param>
        /// <param name="token">The token.</param>
        private static void ValidateSetPermissionMapParameters(
                                                            IEnumerable<PermissionMap> permissionMap,
                                                            Group group, 
                                                            ZentityContext context, 
                                                            AuthenticatedToken token)
        {
            ValidateToken(token);
            ValidatePermissionMap(permissionMap);
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
        }

        /// <summary>
        /// Validates the set permission map parameters.
        /// </summary>
        /// <param name="permissionMap">The permission map.</param>
        /// <param name="context">The context.</param>
        /// <param name="token">The token.</param>
        private static void ValidateSetPermissionMapParameters(
                                                            PermissionMap permissionMap,
                                                            ZentityContext context, 
                                                            AuthenticatedToken token)
        {
            ValidateToken(token);
            ValidatePermissionMap(permissionMap);
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
        }

        /// <summary>
        /// Validates the permission map.
        /// </summary>
        /// <param name="permissionMap">The permission map.</param>
        private static void ValidatePermissionMap(PermissionMap permissionMap)
        {
            if (permissionMap == null)
            {
                throw new ArgumentNullException("permissionMap");
            }

            if (permissionMap.Allow && permissionMap.Deny)
            {
                throw new InvalidOperationException(ConstantStrings.AllowAndDenyException);
            }
        }

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="token">The token.</param>
        private static void ValidateToken(AuthenticatedToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            if (!token.Validate())
            {
                throw new AuthorizationException(ConstantStrings.TokenNotValidException);
            }
        }

        /// <summary>
        /// Validates the permission map.
        /// </summary>
        /// <param name="permissionMap">The permission map.</param>
        private static void ValidatePermissionMap(IEnumerable<PermissionMap> permissionMap)
        {
            if (permissionMap == null)
            {
                throw new ArgumentNullException("permissionMap");
            }

            if (permissionMap.Where(pm => pm.Allow && pm.Deny).Any())
            {
                throw new InvalidOperationException(ConstantStrings.AllowAndDenyException);
            }
        }

        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userToken">The user token.</param>
        private static void ValidateParameters(ZentityContext context, AuthenticatedToken userToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ValidateToken(userToken);
        }

        /// <summary>
        /// Validates the strings.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void ValidateStrings(params string[] args)
        {
            int len = args.Length;
            for (int i = 1; i < len; i += 2)
            {
                if (string.IsNullOrEmpty(args[i]))
                {
                    throw new ArgumentNullException(args[i - 1]);
                }
            }
        }

        /// <summary>
        /// Validates the identity.
        /// </summary>
        /// <param name="identity">The identity.</param>
        private static void ValidateIdentity(Identity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
        }

        /// <summary>
        /// Validates the group.
        /// </summary>
        /// <param name="group">The group.</param>
        private static void ValidateGroup(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
        }
        #endregion

        #endregion
    }
}
