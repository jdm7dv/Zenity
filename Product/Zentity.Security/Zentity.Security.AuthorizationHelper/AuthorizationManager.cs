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
    using System.Data.Objects;
    using System.Linq;
    using Zentity.Core;
    using Zentity.Security.Authentication;
    using Zentity.Security.Authorization;

    /// <summary>
    /// This class provides LINQ extension methods to authorize operations, and retrieve permissions on resources.
    /// </summary>
    public static class AuthorizationManager
    {
        #region Authorize() overloads

        /// <summary>
        /// Authorizes an operation for a given resource for a given user
        /// </summary>
        /// <typeparam name="T">A type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resource">Resource for which permission is to be verified</param>
        /// <param name="permissionName">Permission Name, 'Read/Update/Delete'</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">User for whom the permission is to be verified</param>
        /// <returns>True if user is authorized to perform the requested operation, false otherwise.</returns>
        /// <remarks>
        /// Authorization process works as follows -
        /// <list type="bullet">
        /// <item>Authorization succeeds for administrators without any checks.</item>
        /// <item>Authorization fails for guest user in case the permission requested is other than read.</item>
        /// <item>Authorization succeeds in case user is owner of the resource.</item>
        /// <item>In case of read permission authorization succeeds if read is not revoked from the user or any of his groups
        /// (Read is implicitly granted.)</item>
        /// <item>In case of other permissions authorization succeeds if the permission is not revoked from the user or
        /// any of his groups, AND the permission is granted to the user or any of his groups.</item>
        /// </list>
        /// IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Check if authorized or not
        /// bool authorized = resource.Authorize("Update", context, token);
        /// </code>
        /// </example>
        public static bool Authorize<T>(
                                this T resource,
                                string permissionName,
                                ZentityContext context,
                                AuthenticatedToken userToken) where T : Resource
        {
            //---------------------------Algorithm-------------------------------------
            //  If user belongs to Administrator's group
            //      Authorized
            //  Else if user == Guest AND permission != Read
            //      Not authorized
            //  Else (Normal user)
            //      If (User is owner of the resource
            //          OR (Permission is NOT denied to user/his groups/AllUsers group
            //          AND (Permission == Read)
            //          OR (Permission is granted to user/his groups/AllUsers group))))
            //          Authorized
            //      Else
            //          Not authorized
            //-------------------------------------------------------------------------
            #region Parameter validation
            ValidateString(permissionName);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                return ProcessAuthorizeRequest<T>(resource, permissionName, context, userToken);
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
            catch (NullReferenceException ex)
            {
                throw new AuthorizationException(ConstantStrings.NullReferenceException, ex);
            }
        }

        /// <summary>
        /// Returns resources out of the given list for which user has the requested permission
        /// </summary>
        /// <typeparam name="T">A type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resources">List of resources to be authorized.</param>
        /// <param name="permissionName">Permission Name, Read/Update/Delete</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">User for whom the permission is to be verified</param>
        /// <returns>Query with authorization criteria</returns>
        /// <remarks>
        /// Authorization process works as follows for the given list of resources.
        /// <list type="bullet">
        /// <item>Authorization succeeds for administrators without any checks.</item>
        /// <item>Authorization fails for guest user in case the permission requested is other than read.</item>
        /// <item>Out of the given resources
        /// <list type="bullet">
        ///     <item>All resources owned by the user are returned as authorized ones.</item>
        ///     <item>All resources for which user is denied the requested permission are filtered out.</item>
        ///     <item>In case of permission other than read, all resources for which user is granted the requested permission are
        ///     returned as authorized resources.</item>
        /// </list>
        /// </item>
        /// <item>The selection of resources as given in step 3 is done by appending the respective criteria to the original query passed. </item>
        /// <item>The result set is not materialized in the query. </item>
        /// </list>
        /// IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources.AsEnumerable();
        ///
        /// // Check if authorized or not
        /// bool authorized = resources.Authorize("Update", context, token);
        /// </code>
        /// </example>
        public static IEnumerable<T> Authorize<T>(
                                            this IEnumerable<T> resources,
                                            string permissionName,
                                            ZentityContext context,
                                            AuthenticatedToken userToken) where T : Resource
        {
            //---------------------------Algorithm-------------------------------------
            //  If user belongs to Administrator's group
            //      He is authorized to access all resources - return the list of resources as it is.
            //  Else if user == Guest AND permission != Read
            //      Not authorized to access any resource - return null
            //  Else (Normal user)
            //      Out of the list of resources
            //          If permission requested is 'read'
            //              Return list of resources INTERSECT denied resources
            //          Else
            //              Return list of owned resources UNION allowed resources INTERSECT denied resources
            //          End If
            //          (Owned resources - the resources for which the user or any of his groups is owner
            //          Allowed resources - the resources for which the user or any of his groups have been granted the permission
            //          Denied resources - the resources for which the user or any of his groups have been revoked the permission)
            //  End If
            //-------------------------------------------------------------------------
            #region Parameter Validation
            ValidateString(permissionName);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                var resObjectSet = context.CreateObjectSet<Resource>();
                var resourceIds = resources.Select(res => res.Id).ToList();
                var resourceList = resObjectSet.OfType<T>().Where(res => resourceIds.Contains(res.Id));
                IEnumerable<T> authorizedResources = ProcessAuthorizeRequest<T>(resourceList, permissionName, context, userToken)
                    as IEnumerable<T>;
                return authorizedResources;
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
            catch (NullReferenceException ex)
            {
                throw new AuthorizationException(ConstantStrings.NullReferenceException, ex);
            }
        }

        /// <summary>
        /// Returns resources out of the given list for which user has the requested permission
        /// </summary>
        /// <typeparam name="T">A type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resources">List of resources to be authorized.</param>
        /// <param name="permissionName">Permission Name, Read/Update/Delete</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">User for whom the permission is to be verified</param>
        /// <returns>Query with authorization criteria</returns>
        /// <remarks>
        /// Authorization process works as follows for the given list of resources.
        /// <list type="bullet">
        /// <item>Authorization succeeds for administrators without any checks.</item>
        /// <item>Authorization fails for guest user in case the permission requested is other than read.</item>
        /// <item>Out of the given resources,
        /// <list type="bullet">
        ///     <item>All resources owned by the user are returned as authorized ones.</item>
        ///     <item>All resources for which user is denied the requested permission are filtered out.</item>
        ///     <item>In case of permission other than read, all resources for which user is granted the requested permission are
        ///     returned as authorized resources. </item>
        /// </list>
        /// </item>
        /// <item>The selection of resources as given in step 3 is done by appending the respective criteria to the original query passed. </item>
        /// <item>The result set is not materialized in the query. </item>
        /// </list>
        /// IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resources
        /// IQueryable&lt;Resource&gt; resources = context.Resources.AsQueryable();
        ///
        /// // Check if authorized or not
        /// bool authorized = resources.Authorize("Update", context, token);
        /// </code>
        /// </example>
        public static IQueryable<T> Authorize<T>(
                                            this IQueryable<T> resources,
                                            string permissionName,
                                            ZentityContext context,
                                            AuthenticatedToken userToken) where T : Resource
        {
            //---------------------------Algorithm-------------------------------------
            //  If user belongs to Administrator's group
            //      He is authorized to access all resources - return the list of resources as it is.
            //  Else if user == Guest AND permission != Read
            //      Not authorized to access any resource - return null
            //  Else (Normal user)
            //      Out of the list of resources
            //          If permission requested is 'read'
            //              Return list of resources INTERSECT denied resources
            //          Else
            //              Return list of owned resources UNION allowed resources INTERSECT denied resources
            //          End If
            //          (Owned resources - the resources for which the user or any of his groups is owner
            //          Allowed resources - the resources for which the user or any of his groups have been granted the permission
            //          Denied resources - the resources for which the user or any of his groups have been revoked the permission)
            //  End If
            //-------------------------------------------------------------------------
            #region Parameter Validation
            ValidateString(permissionName);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                IQueryable<T> authorizedResources = ProcessAuthorizeRequest<T>(resources, permissionName, context, userToken);
                return authorizedResources;
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
            catch (NullReferenceException ex)
            {
                throw new AuthorizationException(ConstantStrings.NullReferenceException, ex);
            }
        }

        /// <summary>
        /// Returns resources out of the given list for which user has the requested permission
        /// </summary>
        /// <typeparam name="T">A type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resources">List of resources to be authorized.</param>
        /// <param name="permissionName">Permission Name, Read/Update/Delete</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">User for whom the permission is to be verified</param>
        /// <returns>Query with authorization criteria</returns>
        /// <remarks>
        /// Authorization process works as follows for the given list of resources.
        /// <list type="bullet">
        /// <item>Authorization succeeds for administrators without any checks.</item>
        /// <item>Authorization fails for guest user in case the permission requested is other than read.</item>
        /// <item>Out of the given resources,
        /// <list type="bullet">
        ///     <item>All resources owned by the user are returned as authorized ones.</item>
        ///     <item>All resources for which user is denied the requested permission are filtered out.</item>
        ///     <item>In case of permission other than read, all resources for which user is granted the requested permission are
        ///     returned as authorized resources. </item>
        /// </list>
        /// </item>
        /// <item>The selection of resources as given in step 3 is done by appending the respective criteria to the original query passed. </item>
        /// <item>The result set is not materialized in the query. </item>
        /// </list>
        /// IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resources
        /// ObjectQuery&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Check if authorized or not
        /// bool authorized = resources.Authorize("Update", context, token);
        /// </code>
        /// </example>
        public static ObjectQuery<T> Authorize<T>(
                                            this ObjectQuery<T> resources,
                                            string permissionName,
                                            ZentityContext context,
                                            AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter Validation
            ValidateString(permissionName);
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                ObjectQuery<T> authorizedResources = ProcessAuthorizeRequest<T>(
                                                                            resources,
                                                                            permissionName, 
                                                                            context, 
                                                                            userToken) as ObjectQuery<T>;
                return authorizedResources;
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
            catch (NullReferenceException ex)
            {
                throw new AuthorizationException(ConstantStrings.NullReferenceException, ex);
            }
        }

        #endregion

        #region Get Permissions
        /// <summary>
        /// Returns set of permissions for the given user on the given resource
        /// </summary>
        /// <typeparam name="T">A type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resource">The resource to be authorized</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">User for whom the permission is to be verified</param>
        /// <returns>Resource and its permissions</returns>
        /// <remarks>
        /// This method returns the permission set for the user represented by the token on the resource. It works as follows.
        /// <list type="bullet">
        /// <item>It returns all permissions for an administrator and owner of the resource. </item>
        /// <item>It returns null if user does not have read access on the resource.</item>
        /// <item>It returns permissions which the user or his group(s) have been granted on the resource. (It excludes the permission if same is
        /// revoked from the user or his group(s). That is, in case one of the user's group has been granted the permission
        /// and other has been revoked, the permission is not returned.)</item>
        /// </list>
        /// IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Get permissions
        /// ResourcePermissions&lt;Resource&gt; permissions = resource.GetPermissions(context, token);
        /// </code>
        /// </example>
        public static ResourcePermissions<T> GetPermissions<T>(this T resource, ZentityContext context, AuthenticatedToken userToken)
                where T : Resource
        {
            //--------------------------Algorithm------------------------------------
            //  If user belongs to administrators group
            //      Return all permissions (Read/Update/Delete)
            //  Else (Normal user OR Guest)
            //      Read permission predicates for user/his groups/AllUsers group.
            //      For each permission predicate
            //          If (User is owner of the resource)
            //              Add all permissions
            //          Else If (DenyPredicate does not exist
            //              AND (AllowPredicate exists
            //              OR (permission == Read)))
            //              Add the permission to the list to be returned
            //          End If
            //  End If
            //------------------------------------------------------------------------
            #region Parameter Validation
            ValidateParameters(context, userToken);
            #endregion
            try
            {
                Identity currentUser = DataAccess.GetIdentity(userToken.IdentityName, context);
                Group allUsers = DataAccess.GetGroup(DataAccess.AllUsersGroupName, context);
                if (currentUser != null)
                {
                    ////set up the resource permissions object to be returned.
                    ResourcePermissions<T> resourcePermissions = new ResourcePermissions<T>();
                    resourcePermissions.Name = userToken.IdentityName;
                    resourcePermissions.Resource = resource;

                    if (DataAccess.IsAdmin(currentUser))
                    {
                        SetAllPermissions<T>(resourcePermissions);
                        return resourcePermissions;
                    }

                    bool isGuest = DataAccess.IsGuest(userToken.IdentityName);
                    if (!isGuest && DataAccess.IsOwner(userToken, resource, context))
                    {
                        SetAllPermissions<T>(resourcePermissions);
                        return resourcePermissions;
                    }

                    ////Get the permissions
                    IQueryable<string> securityPredicateUris = SecurityPredicateAccess.GetResourceLevelPredicateUris();
                    var predicateUrisQuery = resource.GetAuthorizedPredicates(securityPredicateUris, userToken, context);
                    if (allUsers != null)
                    {
                        predicateUrisQuery = predicateUrisQuery.Concat(allUsers.GetAuthorizedPredicates<T>(resource, securityPredicateUris, context));
                    }

                    IEnumerable<string> predicateUris = predicateUrisQuery.ToList();
                    if (predicateUris.Contains(SecurityPredicateAccess.GetInverseUri("Read")))
                    {
                        return null;
                    }
                    else
                    {
                        resourcePermissions.PermissionList.Add("Read");
                    }

                    if (!isGuest)
                    {
                        SetGrantedPermissions<T>(resourcePermissions, predicateUris);
                    }

                    return resourcePermissions;
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
            catch (NullReferenceException ex)
            {
                throw new AuthorizationException(ConstantStrings.NullReferenceException, ex);
            }
        }

        /// <summary>
        /// Returns list of resource permissions for each resource in the given list for the given user
        /// This method enumerates through the resources.
        /// </summary>
        /// <typeparam name="T">A type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resources">List of resources for which the list of permissions is to be retrieved</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">User for whom the permission is to be verified</param>
        /// <returns>List of resources along with permissions on each</returns>
        /// <remarks>
        /// This method returns the permission set for the user represented by the token on the resources. It works as follows.
        /// <list type="bullet">
        /// <item>It returns all permissions for an administrator and owner of a resource. </item>
        /// <item>It returns null if user does not have read access on a resource.</item>
        /// <item>It returns permissions which the user or his group(s) have been granted on a resource. (It excludes the permission if same is
        /// revoked from the user or his group(s). That is, in case one of the user's group has been granted the permission
        /// and other has been revoked, the permission is not returned.)</item>
        /// </list>
        /// IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resources
        /// IEnumerable&lt;Resource&gt; resources = context.Resources.AsEnumerable();
        ///
        /// // Get permissions on every resource
        /// IEnumerable&lt;ResourcePermissions&lt;Resource&gt;&gt; permissions = resources.GetPermissions(context, token);
        /// </code>
        /// </example>
        public static IEnumerable<ResourcePermissions<T>> GetPermissions<T>(
                                                                    this IEnumerable<T> resources,
                                                                    ZentityContext context, 
                                                                    AuthenticatedToken userToken) where T : Resource
        {
            #region Parameter validation
            ValidateParameters(context, userToken);
            #endregion

            IEnumerable<ResourcePermissions<T>> resourcePermissions = GetResourcePermissions<T>(resources.AsQueryable(), context, userToken);
            return resourcePermissions;
        }

        /// <summary>
        /// Returns list of resource permissions for each resource in the given list for the given user
        /// This method enumerates through the resources.
        /// </summary>
        /// <typeparam name="T">A type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resources">List of resources for which the list of permissions is to be retrieved</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">User for whom the permission is to be verified</param>
        /// <returns>List of resources along with permissions on each</returns>
        /// <remarks>
        /// This method returns the permission set for the user represented by the token on the resources. It works as follows.
        /// <list type="bullet">
        /// <item>It returns all permissions for an administrator and owner of a resource. </item>
        /// <item>It returns null if user does not have read access on a resource.</item>
        /// <item>It returns permissions which the user or his group(s) have been granted on a resource. (It excludes the permission if same is
        /// revoked from the user or his group(s). That is, in case one of the user's group has been granted the permission
        /// and other has been revoked, the permission is not returned.)</item>
        /// </list>
        /// IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resources
        /// IQueryable&lt;Resource&gt; resources = context.Resources.AsQueryable();
        ///
        /// // Get permissions on every resource
        /// IQueryable&lt;ResourcePermissions&lt;Resource&gt;&gt; permissions = resources.GetPermissions(context, token);
        /// </code>
        /// </example>
        public static IQueryable<ResourcePermissions<T>> GetPermissions<T>(this IQueryable<T> resources, ZentityContext context, AuthenticatedToken userToken)
            where T : Resource
        {
            #region Parameter validation
            ValidateParameters(context, userToken);
            #endregion

            IQueryable<ResourcePermissions<T>> resourcePermissions = GetResourcePermissions<T>(resources, context, userToken);
            return resourcePermissions;
        }

        /// <summary>
        /// Returns list of resource permissions for each resource in the given list for the given user
        /// This method enumerates through the resources.
        /// </summary>
        /// <typeparam name="T">A type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resources">List of resources for which the list of permissions is to be retrieved</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="userToken">User for whom the permission is to be verified</param>
        /// <returns>List of resources along with permissions on each</returns>
        /// <remarks>
        /// This method returns the permission set for the user represented by the token on the resource. It works as follows.
        /// <list type="bullet">
        /// <item>It returns all permissions for an administrator and owner of the resource. </item>
        /// <item>It returns null if user does not have read access on the resource.</item>
        /// <item>It returns permissions which the user or his group(s) have been granted on the resource. (It excludes the permission if same is
        /// revoked from the user or his group(s). That is, in case one of the user's group has been granted the permission
        /// and other has been revoked, the permission is not returned.)</item>
        /// </list>
        /// IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resources
        /// ObjectQuery&lt;Resource&gt; resources = context.Resources;
        ///
        /// // Get permissions on every resource
        /// IQueryable&lt;ResourcePermissions&lt;Resource&gt;&gt; permissions = resources.GetPermissions(context, token);
        /// </code>
        /// </example>
        public static IQueryable<ResourcePermissions<T>> GetPermissions<T>(this ObjectQuery<T> resources, ZentityContext context, AuthenticatedToken userToken)
            where T : Resource
        {
            #region Parameter validation
            ValidateParameters(context, userToken);
            #endregion

            IQueryable<ResourcePermissions<T>> resourcePermissions = GetResourcePermissions<T>(resources, context, userToken);
            return resourcePermissions;
        }

        /// <summary>
        /// Returns the explicit permission map for the given resource for the given user.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or a type deriving from the same.</typeparam>
        /// <param name="resource">Resource for which the permission map is to be retrieved.</param>
        /// <param name="identity">Identity for whom permission map is to be retrieved.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <returns>Collection of PermissionMap objects, which contains both granted and denied (revoked) permissions.</returns>
        /// <example>
        /// <remarks>
        /// <para>This method returns the permissions granted or revoked from the user represented by the identity parameter. The user requesting
        /// permission map (represented by the token) must be owner of the resource or an administrator.</para>
        ///
        /// <para>If permissions are requested for admin identity an empty list is returned, since administrators are not explicitly
        /// granted permissions, and no permission can be revoked from them.</para>
        ///
        /// <para>If permissions are requested for the guest user a list with read and deny read permissions status is returned.
        /// For other users it returns a list of 4 permissions (read, update, delete, owner) with their grant (allow) and revoke (deny) status.
        /// The status represents explicit permission given to the user, and does not take into account permissions inherited from group.
        /// </para>
        /// <para>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</para>
        /// </remarks>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Administrator", "XXXX");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity
        /// Identity identity = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("User1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Get permissions map
        /// IEnumerable&lt;PermissionMap&gt; permissionsMap = resource.GetPermissionMap(identity, context, adminToken);
        /// </code>
        /// </example>
        public static IEnumerable<PermissionMap> GetPermissionMap<T>(
                                                                this T resource, 
                                                                Identity identity,
                                                                ZentityContext context, 
                                                                AuthenticatedToken token) where T : Resource
        {
            #region Parameter validation
            ValidateIdentity(identity);
            ValidateParameters(context, token);
            #endregion
            ////Only an owner or admin can request a permission map.
            if (!DataAccess.IsAdmin(token.IdentityName, context) && !DataAccess.IsOwner(token, resource, context))
            {
                return new List<PermissionMap>(0).AsEnumerable();
            }

            ////The admin user does not need to be granted any permission, also he cannot be revoked of any permissions.
            ////Hence return an empty permission map.
            if (DataAccess.IsAdmin(identity))
            {
                return new List<PermissionMap>(0).AsEnumerable();
            }

            IQueryable<string> securityPredicateUris = SecurityPredicateAccess.GetResourceLevelPredicateUris();
            IEnumerable<string> uris = identity.GetAuthorizedPredicates<T>(resource, securityPredicateUris, context);
            IEnumerable<PermissionMap> map = GetPermissionMapFromPredicateUris(uris);

            ////A guest cannot be granted any permission other than read, and need not be revoked of any permissions other than read.
            ////Hence remove other permission maps from the list.
            if (DataAccess.IsGuest(identity.IdentityName))
            {
                IEnumerable<PermissionMap> guestMap = map.ToList().Where(m => m.Permission.Equals("Read", StringComparison.OrdinalIgnoreCase))
                    .AsEnumerable();
                return guestMap;
            }

            return map;
        }

        /// <summary>
        /// Returns the explicit permission map for the given resource for the given group.
        /// </summary>
        /// <param name="resource">Resource for which the permission map is to be retrieved.</param>
        /// <param name="group">Group for which permission map is to be retrieved.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <typeparam name="T">Resource or its derivative</typeparam>
        /// <returns>Collection of PermissionMap objects, which contains both granted and denied (revoked) permissions.</returns>
        /// <remarks>
        /// <para>This method returns the permissions granted or revoked from the group. The user requesting
        /// permission map (represented by the token) must be owner of the resource or an administrator.</para>
        /// <para>If permissions are requested for admin group an empty list is returned, since administrators are not explicitly
        /// granted permissions, and no permission can be revoked from them.
        /// For other groups it returns a list of 4 permissions (read, update, delete, owner) with their grant (allow) and revoke (deny) status.
        /// The status represents explicit permission given to the group.</para>
        ///
        /// <para>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Administrator", "XXXX");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group
        /// Group group = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("Group1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Get permissions map
        /// IEnumerable&lt;PermissionMap&gt; permissionsMap = resource.GetPermissionMap(group, context, adminToken);
        /// </code>
        /// </example>
        public static IEnumerable<PermissionMap> GetPermissionMap<T>(
                                                            this T resource, 
                                                            Group group,
                                                            ZentityContext context, 
                                                            AuthenticatedToken token) where T : Resource
        {
            #region Parameter validation
            ValidateGroup(group);
            ValidateParameters(context, token);
            #endregion

            ////Only an owner or admin can request a permission map.
            if (!DataAccess.IsAdmin(token.IdentityName, context) && !DataAccess.IsOwner(token, resource, context))
            {
                return new List<PermissionMap>(0).AsEnumerable();
            }

            ////The admin group does not need to be granted any permission, also he cannot be revoked of any permissions.
            ////Hence return an empty permission map.
            if (DataAccess.IsAdmin(group))
            {
                return new List<PermissionMap>(0).AsEnumerable();
            }

            IQueryable<string> securityPredicateUris = SecurityPredicateAccess.GetResourceLevelPredicateUris();
            IEnumerable<string> uris = group.GetAuthorizedPredicates<T>(resource, securityPredicateUris, context);
            IEnumerable<PermissionMap> mapList = GetPermissionMapFromPredicateUris(uris);
            return mapList;
        }

        /// <summary>
        /// Gets explicit create or deny create permissions for a user or group.
        /// </summary>
        /// <param name="identity">Identity object for whom the permission is to be returned.</param>
        /// <param name="context">Zentity context</param>
        /// <param name="token">AuthenticatedToken of the logged on user. This method requires that the logged on
        /// user must be an administrator.</param>
        /// <returns>PermissionMap list with one permission map for the create permission.</returns>
        /// <remarks>
        /// <para>This method returns the permissions granted or revoked from the user represented by the identity parameter. The user requesting
        /// permission map (represented by the token) must be an administrator.</para>
        /// <para>For admin and guest null is returned, since administrators are not explicitly granted the create permission and they cannot be revoked
        /// the permission as well. Guests cannot be granted and need not be revoked the create permission.
        /// For other users it returns a permission map with grant (allow) and revoke (deny) status of the create permission.
        /// </para>
        /// <para>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Administrator", "XXXX");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity
        /// Identity identity = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("User1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Get permission map
        /// PermissionMap permissionMap = identity.GetCreatePermissionMap(context, adminToken);
        /// </code>
        /// </example>
        public static PermissionMap GetCreatePermissionMap(
                                                    this Identity identity,
                                                    ZentityContext context, 
                                                    AuthenticatedToken token)
        {
            #region Parameter validation
            ValidateIdentity(identity);
            ValidateParameters(context, token);
            #endregion
            ////Only an owner or admin can request a permission map.
            if (!DataAccess.IsAdmin(token.IdentityName, context))
            {
                return null;
            }

            ////The admin user does not need to be granted any permission, also he cannot be revoked of any permissions.
            ////Hence return an empty permission map.
            ////Similarly for a guest user cannot be granted create and need not be revoked create.
            if (DataAccess.IsAdmin(identity) || DataAccess.IsGuest(identity.IdentityName))
            {
                return null;
            }

            IQueryable<string> uris = SecurityPredicateAccess.GetRepositoryLevelPredicateUris();
            IQueryable<string> authorizedUris = identity.GetAuthorizedPredicates(uris, context);
            IEnumerable<PermissionMap> mapList = GetCreatePermissionMapFromPredicateUris(authorizedUris);
            return mapList.FirstOrDefault();
        }

        /// <summary>
        /// Returns explicit create or deny create permission for the group.
        /// </summary>
        /// <param name="group">Group object for which the permission is to be returned.</param>
        /// <param name="context">Zentity Context</param>
        /// <param name="token">AuthenticatedToken of the logged on user. This method requires that the logged on
        /// user must be an administrator.</param>
        /// <returns>PermissionMap list with one permission map for the create permission.</returns>
        /// <remarks>
        /// <para>This method returns the permissions granted or revoked from the group. The user requesting
        /// permission map (represented by the token) must be an administrator.</para>
        /// <para>For admin group null is returned, since administrators are not explicitly granted the create permission and they cannot be revoked
        /// the permission as well.
        /// For other users it returns a permission map with grant (allow) and revoke (deny) status of the create permission.
        /// </para>
        /// <para>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Administrator", "XXXX");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group
        /// Group group = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("Group1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Get permission map
        /// PermissionMap permissionMap = group.GetCreatePermissionMap(context, adminToken);
        /// </code>
        /// </example>
        public static PermissionMap GetCreatePermissionMap(
                                                        this Group group,
                                                        ZentityContext context, 
                                                        AuthenticatedToken token)
        {
            #region Parameter validation
            ValidateGroup(group);
            ValidateParameters(context, token);
            #endregion
            ////Only an admin can request a permission map.
            if (!DataAccess.IsAdmin(token.IdentityName, context))
            {
                return null;
            }

            ////The admin group does not need to be granted any permission, also he cannot be revoked of any permissions.
            ////Hence return an empty permission map.
            if (DataAccess.IsAdmin(group))
            {
                return null;
            }

            ////Create and deny create are identity to identity relationships.
            IQueryable<string> uris = SecurityPredicateAccess.GetRepositoryLevelPredicateUris();
            IQueryable<string> authorizedUris = group.GetAuthorizedPredicates(uris, context);
            IEnumerable<PermissionMap> mapList = GetCreatePermissionMapFromPredicateUris(authorizedUris);
            return mapList.FirstOrDefault();
        }

        /// <summary>
        /// Returns list of users who have explicit permissions on the resource.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derivative</typeparam>
        /// <param name="resource">Resource</param>
        /// <param name="context">ZentityContext</param>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <returns>Set of identities.</returns>
        /// <remarks>
        /// This method returns list of users with explicit permissions on the given resource. The user requesting the list of users
        /// must be owner of the resource or an administrator.
        /// The permissions returned include granted as well as revoked permissions.
        ///
        /// <para>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Administrator", "XXXX");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Get users with explicit permission on resource
        /// IEnumerable&lt;Identity&gt; users = resource.GetUsersWithExplicitPermissions(context, adminToken);
        /// </code>
        /// </example>
        public static IEnumerable<Identity> GetUsersWithExplicitPermissions<T>(
                                                                        this T resource, 
                                                                        ZentityContext context,
                                                                        AuthenticatedToken token) where T : Resource
        {
            #region Parameter validation
            ValidateParameters(context, token);
            #endregion

            ////Only a resource owner or admin can request the list of users associated with the resource.
            if (!DataAccess.IsAdmin(token.IdentityName, context) && !DataAccess.IsOwner(token, resource, context))
            {
                ////Return empty list
                return new List<Identity>(0);
            }

            IQueryable<string> securityPredicateUris = SecurityPredicateAccess.GetResourceLevelPredicateUris();
            var users = Identity.GetAuthorizedIdentities<T>(resource, securityPredicateUris, context);

            ////Add admin identities to the group
            var administrators = DataAccess.GetAdministratorIdentities(context);
            users = users.Concat<Identity>(administrators);
            return users;
        }

        /// <summary>
        /// Returns list of groups who have explicit permissions on the resource.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or a type deriving from Zentity.Core.Resource</typeparam>
        /// <param name="resource">Resource</param>
        /// <param name="context">ZentityContext</param>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <returns>Set of groups.</returns>
        /// <remarks>
        /// This method returns list of groups with explicit permissions on the given resource. The user requesting the list of groups
        /// must be owner of the resource or an administrator.
        /// The permissions returned include granted as well as revoked permissions.
        ///
        /// <para>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Administrator", "XXXX");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Get groups with explicit permission on resource
        /// IEnumerable&lt;Group&gt; groups = resource.GetGroupsWithExplicitPermissions(context, adminToken);
        /// </code>
        /// </example>
        public static IEnumerable<Group> GetGroupsWithExplicitPermissions<T>(
                                                                        this T resource, 
                                                                        ZentityContext context,
                                                                        AuthenticatedToken token) where T : Resource
        {
            #region Parameter validation
            ValidateParameters(context, token);
            #endregion

            ////Only a resource owner or admin can request the list of groups associated with the resource.
            if (!DataAccess.IsAdmin(token.IdentityName, context) && !DataAccess.IsOwner(token, resource, context))
            {
                ////Return empty list
                return new List<Group>(0);
            }

            IQueryable<string> securityPredicateUris = SecurityPredicateAccess.GetResourceLevelPredicateUris();
            var groups = Group.GetAuthorizedGroups<T>(resource, securityPredicateUris, context);

            ////Add admin group to the authorized groups collection
            Group adminGroup = DataAccess.GetGroup(DataAccess.AdminGroupName, context);
            List<Group> adminGroupList = new List<Group>(1);
            adminGroupList.Add(adminGroup);
            groups = groups.Concat(adminGroupList.AsEnumerable());
            return groups;
        }

        /// <summary>
        /// Returns resource list for which the user has explicit (not inherited) permissions.
        /// </summary>
        /// <param name="identity">Identity object with permissions on resources.</param>
        /// <param name="context">Zentity context object</param>
        /// <param name="token">Authenticated token of the logged on user.</param>
        /// <returns>Set of resources</returns>
        /// <remarks>
        /// <para>This method returns a list of resources on which an identity has explicit permission.
        /// An administrator can request list of resources for any user. A non-admin user can request a list of his own resources only.</para>
        /// <para>If resource list is requested for an admin identity then all available resources (except group and identity) are returned.
        /// For other users all resources for which he has been explicitly granted or denied permissions are returned.
        /// </para>
        /// <para>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Administrator", "XXXX");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve identity
        /// Identity identity = context.Resources.OfType&lt;Identity&gt;()
        ///     .Where(iden => iden.IdentityName.Equals("User1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Get resources with explicit permission to identity
        /// IEnumerable&lt;Resource&gt; resources = identity.GetResourcesWithExplicitPermissions(context, adminToken);
        /// </code>
        /// </example>
        public static IEnumerable<Resource> GetResourcesWithExplicitPermissions(
                                                                            this Identity identity, 
                                                                            ZentityContext context,
                                                                            AuthenticatedToken token)
        {
            #region Parameter validation
            ValidateIdentity(identity);
            ValidateParameters(context, token);
            #endregion

            if (!DataAccess.IsAdmin(token.IdentityName, context)
                && !string.Equals(identity.IdentityName, token.IdentityName, StringComparison.OrdinalIgnoreCase))
            {
                return new List<Resource>(0).AsEnumerable();
            }

            if (DataAccess.IsAdmin(identity))
            {
                return context.Resources.Where(tuple => !(tuple is Identity || tuple is Group));
            }

            IQueryable<string> securityPredicateUris = SecurityPredicateAccess.GetResourceLevelPredicateUris();
            var resources = identity.GetAuthorizedResources(context, securityPredicateUris);
            return resources;
        }

        /// <summary>
        /// Returns resources for which a group has explicit permissions.
        /// </summary>
        /// <param name="group">Group object.</param>
        /// <param name="context">Zentity context object</param>
        /// <param name="token">Authenticated token of the logged on user. The user must be resource owner or admin.</param>
        /// <returns>Set of resources</returns>
        /// <remarks>
        /// <para>This method returns a list of resources on which a group has explicit permission.
        /// An administrator can request list of resources for any group. A non-admin user can request a list of resources for a group
        /// of which he is a member.</para>
        /// <para>If resource list is requested for an admin group then all available resources (except group and identity) are returned.
        /// For other groups all resources for which he has been explicitly granted or denied permissions are returned.
        /// </para>
        /// <para>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login as Administrator
        /// SecurityToken credentialToken = new UserNameSecurityToken("Administrator", "XXXX");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken adminToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve group
        /// Group group = context.Resources.OfType&lt;Group&gt;()
        ///     .Where(gp => gp.GroupName.Equals("Group1", System.StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Get resources with explicit permission to group
        /// IEnumerable&lt;Resource&gt; resources = group.GetResourcesWithExplicitPermissions(context, adminToken);
        /// </code>
        /// </example>
        public static IEnumerable<Resource> GetResourcesWithExplicitPermissions(
                                                                            this Group group, 
                                                                            ZentityContext context,
                                                                            AuthenticatedToken token)
        {
            #region Parameter validation
            ValidateGroup(group);
            ValidateParameters(context, token);
            #endregion

            if (!DataAccess.IsAdmin(token.IdentityName, context)
                && !DataAccess.IsMemberOf(token.IdentityName, group))
            {
                return new List<Resource>(0).AsEnumerable();
            }

            if (DataAccess.IsAdmin(group))
            {
                return context.Resources.Where(tuple => !(tuple is Identity || tuple is Group));
            }

            IQueryable<string> securityPredicateUris = SecurityPredicateAccess.GetResourceLevelPredicateUris();
            var resources = group.GetAuthorizedResources(context, securityPredicateUris);
            return resources;
        }

        #endregion

        #region HasCreatePermission()
        /// <summary>
        /// Returns true if the user has create permission
        /// </summary>
        /// <param name="userToken">User for whom the permission is to be verified</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if user has create permission</returns>
        /// <remarks>
        /// This method checks whether the user represented by the token has create permission. It always returns true
        /// for an admin, and always returns false for the guest user.
        ///
        /// <para>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Check if identity has create permission or not
        /// bool hasCreatePermission = token.HasCreatePermission(context);
        /// </code>
        /// </example>
        public static bool HasCreatePermission(this AuthenticatedToken userToken, ZentityContext context)
        {
            //------------------- -------Algorithm------------------------------------
            //  If user is member of Administrators group
            //      Return true
            //  Else if user == Guest
            //      Return false
            //  Else
            //      If (Permission is NOT denied to user/his groups/AllUsers group
            //          AND Permission allowed to the user/his groups/AllUsers group)
            //          Return true
            //      Else
            //          Return false
            //      End If
            //  End If
            //--------------------------------------------------------------------------
            #region Parameter Validation
            ValidateParameters(context, userToken);
            #endregion

            try
            {
                if (DataAccess.IsAdmin(userToken.IdentityName, context))
                {
                    return true;
                }
                else if (userToken.IdentityName.Equals("Guest", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else
                {
                    string denyCreateUri = SecurityPredicateAccess.GetInverseUri("Create");
                    string createUri = SecurityPredicateAccess.GetPredicateUri("Create");
                    Identity currentUser = DataAccess.GetIdentity(userToken.IdentityName, context);
                    Group allUsers = DataAccess.GetGroup(DataAccess.AllUsersGroupName, context);
                    if (currentUser != null)
                    {
                        bool denied = userToken.Authorize(context, denyCreateUri);
                        if (allUsers != null)
                        {
                            denied = denied || allUsers.VerifyAuthorization(denyCreateUri, context);
                        }

                        if (!denied)
                        {
                            bool allowed = userToken.Authorize(context, createUri);
                            if (allUsers != null)
                            {
                                allowed = allowed || allUsers.VerifyAuthorization(createUri, context);
                            }

                            return allowed;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        throw new AuthorizationException(ConstantStrings.TokenNotValidException);
                    }
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
            catch (NullReferenceException ex)
            {
                throw new AuthorizationException(ConstantStrings.NullReferenceException, ex);
            }
        }

        #endregion

        #region Get user type - admin/guest/owner

        /// <summary>
        /// Checks if identity is an admin or not.
        /// </summary>
        /// <param name="token">Token of identity.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <returns>True if identity is admin, else false.</returns>
        /// <remarks>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Check if identity is an admin or not.
        /// bool isAdmin = token.IsAdmin(context);
        /// </code>
        /// </example>
        public static bool IsAdmin(this AuthenticatedToken token, ZentityContext context)
        {
            ValidateParameters(context, token);
            return DataAccess.IsAdmin(token.IdentityName, context);
        }

        /// <summary>
        /// Checks if identity is a guest or not.
        /// </summary>
        /// <param name="token">Token of identity.</param>
        /// <returns>True if identity is guest, else false.</returns>
        /// <remarks>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("Guest", "guest@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Check if identity is a guest or not.
        /// bool isGuest = token.IsGuest();
        /// </code>
        /// </example>
        public static bool IsGuest(this AuthenticatedToken token)
        {
            ValidateToken(token);
            return DataAccess.IsGuest(token.IdentityName);
        }

        /// <summary>
        /// Checks if identity is owner of a resource or not.
        /// </summary>
        /// <typeparam name="T">Type of resource.</typeparam>
        /// <param name="token">Token of identity.</param>
        /// <param name="resource">Resource against which rights are being checked.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <returns>True is identity is owner of resource, else false.</returns>
        /// <remarks>IMPORTANT - The ZentityContext object sent to this method MUST have metadata for Authorization pre-loaded. That is, it is the responsibility of the caller
        /// to call <c>context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly)</c> before calling this method.</remarks>
        /// <example>
        /// <code>
        /// // Login
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1@123");
        /// IAuthenticationProvider authenticationProvider
        ///     = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken token = authenticationProvider.Authenticate(credentialToken);
        ///
        /// // Retrieve resource
        /// Resource resource = context.Resources.Where(res => res.Title.Equals("Test Resource 1",
        ///     StringComparison.OrdinalIgnoreCase))
        ///     .FirstOrDefault();
        ///
        /// // Check if identity is owner of the resource or not.
        /// bool isOwner = token.IsOwner(resource, context);
        /// </code>
        /// </example>
        public static bool IsOwner<T>(this AuthenticatedToken token, T resource, ZentityContext context)
            where T : Resource
        {
            ValidateParameters(context, token);
            return DataAccess.IsOwner(token, resource, context);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Adds the granted permissions to the ResourcePermission object.
        /// </summary>
        /// <typeparam name="T">Specifies the element type</typeparam>
        /// <param name="resourcePermissions">Resource permissions</param>
        /// <param name="predicateUris">Predicate Uri's</param>
        private static void SetGrantedPermissions<T>(ResourcePermissions<T> resourcePermissions, IEnumerable<string> predicateUris)
            where T : Resource
        {
            ////A permission is said to be granted if a grant relationship exists and a revoke (or deny) relationship does not exist.
            foreach (string predicateUri in predicateUris)
            {
                string permissionName = SecurityPredicateAccess.GetPermissionName(predicateUri);
                if (SecurityPredicateAccess.Exists(permissionName))
                {
                    string denyUri = SecurityPredicateAccess.GetInverseUri(permissionName);
                    if (!predicateUris.Contains(denyUri))
                    {
                        resourcePermissions.PermissionList.Add(permissionName);
                    }
                }
            }
        }

        /// <summary>
        /// Adds all permissions to the ResourcePermissions object.
        /// </summary>
        /// <typeparam name="T">Specifies the element type</typeparam>
        /// <param name="resourcePermissions">Resource permissions</param>
        private static void SetAllPermissions<T>(ResourcePermissions<T> resourcePermissions) where T : Resource
        {
            foreach (SecurityPredicate pr in SecurityPredicateAccess.SecurityPredicates)
            {
                if (!pr.Name.Equals("Owner"))
                {
                    resourcePermissions.PermissionList.Add(pr.Name);
                }
            }
        }

        /// <summary>
        /// Authorizes an operation on a resource
        /// </summary>
        /// <typeparam name="T">Specifies the element type</typeparam>
        /// <param name="resource">Resource</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="context">Zentity context</param>
        /// <param name="userToken">Authenticated token</param>
        /// <returns>System.Boolean; <c>true</c> if authorization request is given, <c>false</c> otherwise.</returns>
        private static bool ProcessAuthorizeRequest<T>(
                                                    T resource, 
                                                    string permissionName,
                                                    ZentityContext context,
                                                    AuthenticatedToken userToken) where T : Resource
        {
            var resObjectSet = context.CreateObjectSet<Resource>();
            var resourceList = resObjectSet.OfType<T>().Where(res => res.Id == resource.Id);
            IQueryable<T> authorizedResources = ProcessAuthorizeRequest<T>(resourceList, permissionName, context, userToken);
            return authorizedResources.Any(res => res.Id == resource.Id);
        }

        /// <summary>
        /// Authorizes a list of resources.
        /// </summary>
        /// <typeparam name="T">Specifies the element type</typeparam>
        /// <param name="resources">Resources</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="context">Zentity context</param>
        /// <param name="userToken">Authenticated token</param>
        /// <returns>Return list of resources.</returns>
        private static IQueryable<T> ProcessAuthorizeRequest<T>(
                                                        IQueryable<T> resources,
                                                        string permissionName,
                                                        ZentityContext context,
                                                        AuthenticatedToken userToken) where T : Resource
        {
            //---------------------------Algorithm-------------------------------------
            //  If user belongs to Administrator's group
            //      He is authorized to access all resources - return the list of resources as it is.
            //  Else if user == Guest AND permission != Read
            //      Not authorized to access any resource - return null
            //  Else (Normal user)
            //      Out of the list of resources
            //          If permission requested is 'read'
            //              Return list of resources INTERSECT denied resources
            //          Else
            //              Return list of owned resources UNION allowed resources INTERSECT denied resources
            //          End If
            //          (Owned resources - the resources for which the user or any of his groups is owner
            //          Allowed resources - the resources for which the user or any of his groups have been granted the permission
            //          Denied resources - the resources for which the user or any of his groups have been revoked the permission)
            //  End If
            //-------------------------------------------------------------------------
            string permissionPredicateUri = SecurityPredicateAccess.GetPredicateUri(permissionName);
            if (DataAccess.IsAdmin(userToken.IdentityName, context))
            {
                return resources;
            }
            else if (DataAccess.IsGuest(userToken.IdentityName) && !permissionPredicateUri.Equals(SecurityPredicateAccess.GetPredicateUri("Read")))
            {
                return new List<T>(0).AsQueryable();
            }
            else 
            {
                //// normal registered user or guest user requesting read permission.
                Identity currentUser = DataAccess.GetIdentity(userToken.IdentityName, context);
                Group allUsers = DataAccess.GetGroup(DataAccess.AllUsersGroupName, context);
                if (currentUser != null)
                {
                    IQueryable<T> authorizedResources = null;

                    //// Get denied resources
                    string denyPredicateUri = SecurityPredicateAccess.GetInverseUri(permissionName);
                    var deniedResources = userToken.GetAuthorizedResources(context, denyPredicateUri)
                        .Concat(allUsers.GetAuthorizedResources(context, denyPredicateUri)).OfType<T>();

                    //// Get owned resources
                    var ownedResources = DataAccess.GetOwnedResources<T>(userToken, context);
                    if (permissionName.Equals("Read", StringComparison.OrdinalIgnoreCase))
                    {
                        authorizedResources = resources.Except(deniedResources.Except(ownedResources));
                    }
                    else
                    {
                        //// Get allowed resources
                        var allowedResources = userToken.GetAuthorizedResources(context, permissionPredicateUri)
                            .Concat(allUsers.GetAuthorizedResources(context, permissionPredicateUri)).OfType<T>();
                        IQueryable<Guid> authorizedIds = allowedResources.Except(deniedResources).Union(ownedResources)
                            .Select(res => res.Id).Distinct();
                        authorizedResources = from res in resources
                                              join id in authorizedIds on res.Id equals id
                                              select res;
                    }

                    return authorizedResources.OfType<T>();
                }
                else
                {
                    throw new AuthorizationException(ConstantStrings.TokenNotValidException);
                }
            }
        }

        /// <summary>
        /// Iterates through the list of resources and returns their permissions
        /// </summary>
        /// <typeparam name="T">Specifies the element type</typeparam>
        /// <param name="resources">Resources</param>
        /// <param name="context">Zentity context</param>
        /// <param name="userToken">Authenticated token</param>
        /// <returns>Return list of resource permissions.</returns>
        private static IQueryable<ResourcePermissions<T>> GetResourcePermissions<T>(
                                                                            IQueryable<T> resources,
                                                                            ZentityContext context,
                                                                            AuthenticatedToken userToken) where T : Resource
        {
            ////Simply iterate through all the resources and return the ResourcePermissions list
            List<ResourcePermissions<T>> resourcePermissionsList = new List<ResourcePermissions<T>>();
            List<T> resourceList = resources.ToList();
            foreach (T resource in resourceList)
            {
                ResourcePermissions<T> permissionSet = resource.GetPermissions<T>(context, userToken);
                if (permissionSet != null)
                {
                    resourcePermissionsList.Add(permissionSet);
                }
            }

            return resourcePermissionsList.AsQueryable();
        }

        #region Permission Map Methods

        /// <summary>
        /// Gets the permission map from the list of predicate uri's.
        /// </summary>
        /// <param name="uris">Predicate Uri's</param>
        /// <returns>List of permission map</returns>
        private static IEnumerable<PermissionMap> GetPermissionMapFromPredicateUris(IEnumerable<string> uris)
        {
            List<PermissionMap> mapList = new List<PermissionMap>();
            IEnumerable<SecurityPredicate> resourcePredicates = SecurityPredicateAccess.SecurityPredicates.
                Where(pr => pr.Priority != 0); // Priority 0 predicates are repository level predicates.
            foreach (SecurityPredicate pr in resourcePredicates)
            {
                PermissionMap map = new PermissionMap { Permission = pr.Name, Allow = false, Deny = false };
                mapList.Add(map);
            }

            IEnumerable<SecurityPredicate> securityPredicates = SecurityPredicateAccess.SecurityPredicates;
            foreach (string uri in uris)
            {
                //// check if allowed predicate uri
                SecurityPredicate allowedPredicate = securityPredicates.Where(sp => sp.Uri.Equals(uri, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                SecurityPredicate deniedPredicate = securityPredicates.Where(sp => sp.InverseUri.Equals(uri, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (allowedPredicate != null)
                {
                    AddToPermissionMapList(mapList, allowedPredicate, true);
                }
                else if (deniedPredicate != null)
                {
                    AddToPermissionMapList(mapList, deniedPredicate, false);
                }
            }

            //// process 'read' predicate
            if (!uris.Contains(SecurityPredicateAccess.GetInverseUri("Read")))
            {
                mapList.Where(m => m.Permission.Equals("Read", StringComparison.OrdinalIgnoreCase)).First().Allow = true;
            }

            return mapList.AsEnumerable();
        }

        /// <summary>
        /// Gets the create permission map from predicate uri's.
        /// </summary>
        /// <param name="uris">predicate uri's</param>
        /// <returns>List of create permission map</returns>
        private static IEnumerable<PermissionMap> GetCreatePermissionMapFromPredicateUris(IEnumerable<string> uris)
        {
            List<PermissionMap> mapList = new List<PermissionMap>();
            IEnumerable<SecurityPredicate> repositoryPredicates = SecurityPredicateAccess.SecurityPredicates.
                Where(pr => pr.Priority == 0); // Priority 0 predicates are repository level predicates.
            foreach (SecurityPredicate pr in repositoryPredicates)
            {
                PermissionMap map = new PermissionMap { Permission = pr.Name, Allow = false, Deny = false };
                mapList.Add(map);
            }

            IEnumerable<SecurityPredicate> securityPredicates = SecurityPredicateAccess.SecurityPredicates;
            foreach (string uri in uris)
            {
                //// check if allowed predicate uri
                SecurityPredicate allowedPredicate = securityPredicates.Where(sp => sp.Uri.Equals(uri, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                SecurityPredicate deniedPredicate = securityPredicates.Where(sp => sp.InverseUri.Equals(uri, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (allowedPredicate != null)
                {
                    AddToPermissionMapList(mapList, allowedPredicate, true);
                }
                else if (deniedPredicate != null)
                {
                    AddToPermissionMapList(mapList, deniedPredicate, false);
                }
            }

            return mapList.AsEnumerable();
        }

        /// <summary>
        /// Adds the map with the security predicate to the permission map list.
        /// </summary>
        /// <param name="mapList">Permiossion map list.</param>
        /// <param name="pr">Security predicate</param>
        /// <param name="allow">System.Boolean value indicating the permission status</param>
        private static void AddToPermissionMapList(List<PermissionMap> mapList, SecurityPredicate pr, bool allow)
        {
            PermissionMap map = mapList.Where(m => m.Permission.Equals(pr.Name)).FirstOrDefault();
            if (map == null)
            {
                map = new PermissionMap { Permission = pr.Name };
                mapList.Add(map);
            }

            if (allow)
            {
                map.Allow = true;
            }
            else
            {
                map.Deny = true;
            }
        }

        #endregion

        #region Parameter Validation
        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="context">Zentity context</param>
        /// <param name="token">Authenticated token</param>
        private static void ValidateParameters(ZentityContext context, AuthenticatedToken token)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ValidateToken(token);
        }

        /// <summary>
        /// Validates the authenticated token.
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
                throw new AuthorizationException(ConstantStrings.TokenNotValidException);
            }
        }

        /// <summary>
        /// Validates the group of <see cref="Identity"/>
        /// </summary>
        /// <param name="group">Group of Identity</param>
        private static void ValidateGroup(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
        }

        /// <summary>
        /// Validates the <see cref="Identity"/>.
        /// </summary>
        /// <param name="identity">Identity to validate</param>
        private static void ValidateIdentity(Identity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
        }

        /// <summary>
        /// Validates the permission name
        /// </summary>
        /// <param name="permissionName">Permission name</param>
        private static void ValidateString(string permissionName)
        {
            if (string.IsNullOrEmpty(permissionName))
            {
                throw new ArgumentNullException("permissionName");
            }
        }
        #endregion

        #endregion
    }
}
