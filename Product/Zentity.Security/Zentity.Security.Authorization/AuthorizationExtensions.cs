// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Zentity.Core;
    using Zentity.Security.Authentication;
    using Zentity.Security.Authorization.Properties;

    /// <summary>
    /// This class contains extension methods for authorizing resources.
    /// </summary>
    public static class AuthorizationExtensions
    {
        #region Authorize overloads for a single resource
        /// <summary>
        /// Determines whether the permission 'authorizingPredicate' is available on the specified
        /// resource for the authenticated identity.The authenticated token is assumed to be in
        /// TLS(Thread Local Storage) under the named data slot 'AuthenticatedToken'.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derived type</typeparam>
        /// <param name="resource">Resource to be authorized.</param>
        /// <param name="context">Instance of ZentityContext.</param>
        /// <param name="authorizingPredicateUri">The predicate corresponding to the permission against which
        /// authorization is to be done.</param>
        /// <returns>Boolean value to indicate if identity has permission on resource or not.</returns>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// LocalDataStoreSlot localSlot = Thread.AllocateNamedDataSlot("AuthenticatedToken");
        /// Thread.SetData(localSlot, authenticatedToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// using (ZentityContext context = new ZentityContext())
        /// {
        ///     Resource resource = context.Resources.Where(tuple => tuple.Title.Equals("Resource1")).First();
        ///     resource.Authorize(context, authorizingPredicate);
        /// }
        /// </code>
        /// </example>
        public static bool Authorize<T>(
                                    this T resource,
                                    ZentityContext context, 
                                    string authorizingPredicateUri) where T : Resource
        {
            #region Parameter validation
            ValidateParameters(context, authorizingPredicateUri);
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            #endregion

            AuthenticatedToken token = null;
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot("AuthenticatedToken");
            token = Thread.GetData(slot) as AuthenticatedToken;
            if (token != null)
            {
                return Authorize<T>(resource, context, authorizingPredicateUri, token);
            }
            else
            {
                Thread.FreeNamedDataSlot("AuthenticatedToken");
                throw new AuthorizationException(Resources.TokenUnavailableInTLS);
            }
        }

        /// <summary>
        /// Determines whether the permission 'authorizingPredicate' is available on the specified
        /// resource for the identity in the specified authenticated token.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derived type</typeparam>
        /// <param name="resource">Resource to be authorized.</param>
        /// <param name="context">Instance of ZentityContext.</param>
        /// <param name="authorizingPredicateUri">The predicate corresponding to the permission against which
        /// authorization is to be done.</param>
        /// <param name="token">The AuthenticatedToken corresponding to the identity that is to be authorized.</param>
        /// <returns>Boolean value to indicate if identity has permission on resource or not.</returns>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// using (ZentityContext context = new ZentityContext())
        /// {
        ///     Resource resource = context.Resources.Where(tuple => tuple.Title.Equals("Resource1")).First();
        ///     resource.Authorize(context, authorizingPredicate, authenticatedToken);
        /// }
        /// </code>
        /// </example>
        public static bool Authorize<T>(
                                    this T resource,
                                    ZentityContext context, 
                                    string authorizingPredicateUri, 
                                    AuthenticatedToken token) where T : Resource
        {
            #region Parameter validation
            ValidateParameters(context, authorizingPredicateUri);
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            ValidateToken(token);
            #endregion
            try
            {
                IQueryable<T> identityFilteredResources = token.GetAuthorizedResources(context, authorizingPredicateUri).OfType<T>();
                return identityFilteredResources.Where(tuple => tuple.Id == resource.Id).Count() > 0;
            }
            catch (EntityException exception)
            {
                throw new AuthorizationException(Resources.AuthorizeException, exception);
            }
        }

        #endregion

        #region Authorize overloads for a list of resources
        /// <summary>
        /// Limits the query to resources on which the permission 'authorizingPredicate' has been provided
        /// for the authenticated identity.The authenticated token is assumed to be in
        /// TLS(Thread Local Storage) under the named data slot 'AuthenticatedToken'.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derived type</typeparam>
        /// <param name="resources">Resources to be authorized.</param>
        /// <param name="context">Instance of ZentityContext.</param>
        /// <param name="authorizingPredicateUri">The predicate corresponding to the permission against which
        /// authorization is to be done.</param>
        /// <returns>Returns the list of resources which have the permission.</returns>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// LocalDataStoreSlot localSlot = Thread.AllocateNamedDataSlot("AuthenticatedToken");
        /// Thread.SetData(localSlot, authenticatedToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// using (ZentityContext context = new ZentityContext())
        /// {
        ///     IQueryable&lt;Resource&gt; resources = context.Resources.AsQueryable&lt;Resource&gt;();
        ///     IQueryable&lt;Resource&gt; accessibleResources = resources.Authorize(context, authorizingPredicate);
        /// }
        /// </code>
        /// </example>
        public static IQueryable<T> Authorize<T>(
                                            this IQueryable<T> resources,
                                            ZentityContext context, 
                                            string authorizingPredicateUri) where T : Resource
        {
            #region Parameter Validation
            if (resources == null)
            {
                throw new ArgumentNullException("resources");
            }

            ValidateParameters(context, authorizingPredicateUri);
            #endregion

            AuthenticatedToken token = null;
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot("AuthenticatedToken");
            token = Thread.GetData(slot) as AuthenticatedToken;
            if (token != null)
            {
                return Authorize<T>(resources, context, authorizingPredicateUri, token);
            }
            else
            {
                Thread.FreeNamedDataSlot("AuthenticatedToken");
                throw new AuthorizationException(Resources.TokenUnavailableInTLS);
            }
        }

        /// <summary>
        /// Limits the query to resources on which the permission 'authorizingPredicate' has been provided
        /// for the identity in the specified authenticated token.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derived type</typeparam>
        /// <param name="resources">Resources to be authorized.</param>
        /// <param name="context">Instance of ZentityContext.</param>
        /// <param name="authorizingPredicateUri">The predicate corresponding to the permission against which
        /// authorization is to be done.</param>
        /// <param name="token">The AuthenticatedToken corresponding to the identity that is to be authorized.</param>
        /// <returns>Returns the list of resources which have the permission.</returns>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// using (ZentityContext context = new ZentityContext())
        /// {
        ///     IQueryable&lt;Resource&gt; resources = context.Resources.AsQueryable&lt;Resource&gt;();
        ///     IQueryable&lt;Resource&gt; accessibleResources = resources.Authorize(context, authorizingPredicate, authenticatedToken);
        /// }
        /// </code>
        /// </example>
        public static IQueryable<T> Authorize<T>(
                                            this IQueryable<T> resources,
                                            ZentityContext context, 
                                            string authorizingPredicateUri, 
                                            AuthenticatedToken token) where T : Resource
        {
            #region Parameter Validation
            if (resources == null)
            {
                throw new ArgumentNullException("resources");
            }

            ValidateParameters(context, authorizingPredicateUri);
            ValidateToken(token);
            #endregion

            try
            {
                IQueryable<T> identityFilteredResources = token.GetAuthorizedResources(context, authorizingPredicateUri).OfType<T>();
                var query = from res in resources
                            join authRes in identityFilteredResources
                            on res.Id equals authRes.Id
                            select res;
                return query;
            }
            catch (EntityException exception)
            {
                throw new AuthorizationException(Resources.AuthorizeException, exception);
            }
        }

        /// <summary>
        /// Limits the query to resources on which the permission 'authorizingPredicate' has been provided
        /// for the authenticated identity.The authenticated token is assumed to be in
        /// TLS(Thread Local Storage) under the named data slot 'AuthenticatedToken'.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derived type</typeparam>
        /// <param name="resources">Resources to be authorized.</param>
        /// <param name="context">Instance of ZentityContext.</param>
        /// <param name="authorizingPredicateUri">The predicate corresponding to the permission against which
        /// authorization is to be done.</param>
        /// <returns>Returns the list of resources which have the permission.</returns>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// LocalDataStoreSlot localSlot = Thread.AllocateNamedDataSlot("AuthenticatedToken");
        /// Thread.SetData(localSlot, authenticatedToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// using (ZentityContext context = new ZentityContext())
        /// {
        ///     ObjectQuery&lt;Resource&gt; resources = context.Resources;
        ///     ObjectQuery&lt;Resource&gt; accessibleResources = resources.Authorize(context, authorizingPredicate);
        /// }
        /// </code>
        /// </example>
        public static ObjectQuery<T> Authorize<T>(
                                            this ObjectQuery<T> resources,
                                            ZentityContext context, 
                                            string authorizingPredicateUri) where T : Resource
        {
            #region Parameter Validation
            if (resources == null)
            {
                throw new ArgumentNullException("resources");
            }

            ValidateParameters(context, authorizingPredicateUri);
            #endregion

            AuthenticatedToken token = null;
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot("AuthenticatedToken");
            token = Thread.GetData(slot) as AuthenticatedToken;
            if (token != null)
            {
                if (!token.Validate())
                {
                    throw new AuthorizationException(Resources.InvalidToken);
                }

                return Authorize<T>(resources, context, authorizingPredicateUri, token);
            }
            else
            {
                Thread.FreeNamedDataSlot("AuthenticatedToken");
                throw new AuthorizationException(Resources.TokenUnavailableInTLS);
            }
        }

        /// <summary>
        /// Limits the query to resources on which the permission 'authorizingPredicate' has been provided
        /// for the identity in the specified authenticated token.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derived type</typeparam>
        /// <param name="resources">Resources to be authorized.</param>
        /// <param name="context">Instance of ZentityContext.</param>
        /// <param name="authorizingPredicateUri">The predicate corresponding to the permission against which
        /// authorization is to be done.</param>
        /// <param name="token">The AuthenticatedToken corresponding to the identity that is to be authorized.</param>
        /// <returns>Returns the list of resources which have the permission.</returns>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// using (ZentityContext context = new ZentityContext())
        /// {
        ///     ObjectQuery&lt;Resource&gt; resources = context.Resources;
        ///     ObjectQuery&lt;Resource&gt; accessibleResources = resources.Authorize(context, authorizingPredicate, authenticatedToken);
        /// }
        /// </code>
        /// </example>
        public static ObjectQuery<T> Authorize<T>(
                                            this ObjectQuery<T> resources,
                                            ZentityContext context, 
                                            string authorizingPredicateUri, 
                                            AuthenticatedToken token) where T : Resource
        {
            #region Parameter Validation
            if (resources == null)
            {
                throw new ArgumentNullException("resources");
            }

            ValidateParameters(context, authorizingPredicateUri);
            ValidateToken(token);
            #endregion

            try
            {
                ObjectQuery<T> identityFilteredResources = token.GetAuthorizedResources(context, authorizingPredicateUri).OfType<T>()
                    as ObjectQuery<T>;
                var query = from res in resources
                            join authRes in identityFilteredResources
                            on res.Id equals authRes.Id
                            select res;
                return query as ObjectQuery<T>;
            }
            catch (EntityException exception)
            {
                throw new AuthorizationException(Resources.AuthorizeException, exception);
            }
        }

        #endregion

        #region Authorize overloads for repository level predicates
        /// <summary>
        /// This method authorizes a repository level predicate, e.g. Create. for the user represented by the token.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="authorizingPredicateUri">Uri for the repository level predicate. It should not be a resource specific predicate.
        /// e.g. 'Update' is a resource specific predicate, while 'Create' is a repository level predicate.</param>
        /// <returns>True if the given identity or any of his groups has authorization for the given predicate, false otherwise.</returns>
        public static bool Authorize(this AuthenticatedToken token, ZentityContext context, string authorizingPredicateUri)
        {
            #region Parameter Validation
            ValidateToken(token);
            ValidateParameters(context, authorizingPredicateUri);
            #endregion

            try
            {
                var identityResources = token.GetAuthorizedResources(context, authorizingPredicateUri).OfType<Identity>();
                var groupResources = token.GetAuthorizedResources(context, authorizingPredicateUri).OfType<Group>();
                return identityResources.Any() || groupResources.Any();
            }
            catch (EntityException exception)
            {
                throw new AuthorizationException(Resources.AuthorizeException, exception);
            }
        }

        /// <summary>
        /// This method picks up the token from Thread Local Storage and authorizes a repository level predicate, e.g. Create.
        /// </summary>
        /// <param name="context">ZentityContext object</param>
        /// <param name="authorizingPredicateUri">Uri for the repository level predicate. It should not be a resource specific predicate.
        /// e.g. 'Update' is a resource specific predicate, while 'Create' is a repository level predicate.</param>
        /// <returns>True if the given identity or any of his groups has authorization for the given predicate, false otherwise.</returns>
        public static bool Authorize(ZentityContext context, string authorizingPredicateUri)
        {
            #region Parameter Validation
            ValidateParameters(context, authorizingPredicateUri);
            #endregion

            AuthenticatedToken token = null;
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot("AuthenticatedToken");
            token = Thread.GetData(slot) as AuthenticatedToken;
            if (token != null)
            {
                if (!token.Validate())
                {
                    throw new AuthorizationException(Resources.InvalidToken);
                }

                try
                {
                    var identityResources = token.GetAuthorizedResources(context, authorizingPredicateUri).OfType<Identity>();
                    var groupResources = token.GetAuthorizedResources(context, authorizingPredicateUri).OfType<Group>();
                    return identityResources.Any() || groupResources.Any();
                }
                catch (EntityException exception)
                {
                    throw new AuthorizationException(Resources.AuthorizeException, exception);
                }
            }
            else
            {
                Thread.FreeNamedDataSlot("AuthenticatedToken");
                throw new AuthorizationException(Resources.TokenUnavailableInTLS);
            }
        }
        #endregion

        #region Get Authorized Resources
        /// <summary>
        /// This method returns all authorized resources for the given user (represented by the token) for the given predicate.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the user.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="authorizingPredicateUri">Uri for the permission predicate</param>
        /// <returns>Query which, when enumerated, will give all resources for which the user has given permission.</returns>
        public static IQueryable<Resource> GetAuthorizedResources(
                                                            this AuthenticatedToken token,
                                                            ZentityContext context, 
                                                            string authorizingPredicateUri)
        {
            #region Parameter Validation
            ValidateToken(token);
            ValidateParameters(context, authorizingPredicateUri);
            #endregion

            Identity identity = GetIdentity(context, token);
            if (identity == null)
            {
                throw new AuthenticationException(Resources.InvalidToken);
            }

            var groups = context.Relationships.Where(tuple => tuple.Predicate.Uri.Equals(AuthorizingPredicates.IdentityBelongsToGroups)
                && tuple.Subject.Id == identity.Id).Select(tuple => tuple.Object).OfType<Group>().ToArray();
            IQueryable<Resource> identityFilteredResources =
                context.Relationships.Where(tuple => tuple.Predicate.Uri.Equals(authorizingPredicateUri, StringComparison.OrdinalIgnoreCase)
                && tuple.Subject.Id == identity.Id).Select(tuple => tuple.Object);

            if (groups != null)
            {
                for (int i = 0; i < groups.Length; i++)
                {
                    Group group = groups[i];
                    identityFilteredResources = identityFilteredResources.Concat<Resource>(
                            context.Relationships.Where(tuple => tuple.Predicate.Uri.Equals(authorizingPredicateUri, StringComparison.OrdinalIgnoreCase)
                            && tuple.Subject.Id == group.Id).Select(tuple => tuple.Object).Distinct());
                }
            }

            return identityFilteredResources;
        }

        /// <summary>
        /// This method returns all authorized resources for the given user (represented by the token) for the given predicate.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the user.</param>
        /// <param name="context">ZentityContext object</param>
        /// <param name="authorizingPredicateUris">Predicate uris to be authorized</param>
        /// <returns>Query which, when enumerated, will give all resources for which the user has given permission.</returns>
        public static IQueryable<Resource> GetAuthorizedResources(
                                                            this AuthenticatedToken token,
                                                            ZentityContext context, 
                                                            IQueryable<string> authorizingPredicateUris)
        {
            #region Parameter Validation
            ValidateToken(token);
            ValidateParameters(context, authorizingPredicateUris);
            #endregion

            Identity identity = GetIdentity(context, token);
            if (identity == null)
            {
                throw new AuthenticationException(Resources.InvalidToken);
            }

            var groups = context.Relationships.Where(tuple => tuple.Predicate.Uri.Equals(AuthorizingPredicates.IdentityBelongsToGroups)
                && tuple.Subject.Id == identity.Id).Select(tuple => tuple.Object).OfType<Group>().ToArray();
            IQueryable<Relationship> allAuthorizedRelationships =
                context.Relationships.Where(tuple => tuple.Subject.Id == identity.Id);

            if (groups != null)
            {
                for (int i = 0; i < groups.Length; i++)
                {
                    Group group = groups[i];
                    allAuthorizedRelationships = allAuthorizedRelationships.Concat(
                            context.Relationships.Where(tuple => tuple.Subject.Id == group.Id));
                }
            }

            allAuthorizedRelationships = (allAuthorizedRelationships as ObjectQuery<Relationship>).Include("Predicate");
            IQueryable<Resource> authorizedResources = from uri in authorizingPredicateUris
                                                    join rel in allAuthorizedRelationships on uri equals rel.Predicate.Uri
                                                    select rel.Object;
            return authorizedResources.Distinct();
        }

        /// <summary>
        /// This method returns all authorized resources for the given user (represented by the token) for the given predicate.
        /// </summary>
        /// <param name="context">ZentityContext object</param>
        /// <param name="authorizingPredicateUri">Uri for the permission predicate</param>
        /// <returns>Query which, when enumerated, will give all resources for which the user has given permission.</returns>
        /// <remarks>This method assumes that the authenticated token is stored in a TLS slot named 'AuthenticatedToken'</remarks>
        public static IQueryable<Resource> GetAuthorizedResources(ZentityContext context, string authorizingPredicateUri)
        {
            #region Parameter Validation
            ValidateParameters(context, authorizingPredicateUri);
            #endregion

            AuthenticatedToken token = null;
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot("AuthenticatedToken");
            token = Thread.GetData(slot) as AuthenticatedToken;
            if (token != null)
            {
                var authorizedResources = token.GetAuthorizedResources(context, authorizingPredicateUri);
                return authorizedResources;
            }
            else
            {
                Thread.FreeNamedDataSlot("AuthenticatedToken");
                throw new AuthorizationException(Resources.TokenUnavailableInTLS);
            }
        }

        /// <summary>
        /// This method returns all authorized resources for the given user (represented by the token) for the given predicate.
        /// </summary>
        /// <param name="context">ZentityContext object</param>
        /// <param name="authorizingPredicateUris">Predicate uris to be authorized</param>
        /// <returns>Query which, when enumerated, will give all resources for which the user has given permission.</returns>
        /// <remarks>This method assumes that the authenticated token is stored in a TLS slot named 'AuthenticatedToken'</remarks>
        public static IQueryable<Resource> GetAuthorizedResources(ZentityContext context, IQueryable<string> authorizingPredicateUris)
        {
            #region Parameter Validation
            ValidateParameters(context, authorizingPredicateUris);
            #endregion
            AuthenticatedToken token = null;
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot("AuthenticatedToken");
            token = Thread.GetData(slot) as AuthenticatedToken;
            if (token != null)
            {
                var authorizedResources = token.GetAuthorizedResources(context, authorizingPredicateUris);
                return authorizedResources;
            }
            else
            {
                Thread.FreeNamedDataSlot("AuthenticatedToken");
                throw new AuthorizationException(Resources.TokenUnavailableInTLS);
            }
        }
        #endregion

        #region Get Authorized Predicates
        /// <summary>
        /// This method returns the authorized predicates out of the given list of predicates for the given resource.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derivative</typeparam>
        /// <param name="resource">Resource for which the authorized predicates are to be retrieved.</param>
        /// <param name="predicateUris">List of predicate uri's to be looked for.</param>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <returns>List of predicate uris (out of the list given by the user)
        /// for which the logged on user has authorization.</returns>
        public static IQueryable<string> GetAuthorizedPredicates<T>(
                                                                this T resource, 
                                                                IQueryable<string> predicateUris,
                                                                AuthenticatedToken token, 
                                                                ZentityContext context) where T : Resource
        {
            #region Parameter Validation
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            ValidateParameters(context, predicateUris);
            ValidateToken(token);
            #endregion
            Identity currentUser = GetIdentity(context, token);

            ////Query for user's relationships as subject with the resource.
            IQueryable<string> predicatesQuery = context.Relationships.Where(
                        rel => rel.Subject.Id == currentUser.Id
                            && rel.Object.Id == resource.Id)
                            .Select(rel => rel.Predicate.Uri);

            ////Get the group ids for groups to which the user belongs.
            var groupIds = context.Relationships.Where(tuple =>
                tuple.Predicate.Uri.Equals(AuthorizingPredicates.IdentityBelongsToGroups, StringComparison.OrdinalIgnoreCase)
                && tuple.Subject.Id == currentUser.Id).Select(tuple => tuple.Object)
                .OfType<Group>().Select(tuple => tuple.Id).ToArray();
            if (groupIds != null)
            {
                for (int i = 0; i < groupIds.Length; i++)
                {
                    Guid id = groupIds[i];

                    ////Append the query criteria for each group's relationships as subject with the resource.
                    predicatesQuery = predicatesQuery.Union(context.Relationships.Where(
                        rel => rel.Subject.Id == id
                            && rel.Object.Id == resource.Id)
                            .Select(rel => rel.Predicate.Uri));
                }
            }

            var authorizedPredicates = from uri1 in predicateUris
                                       join uri2 in predicatesQuery.ToList() on uri1 equals uri2
                                       select uri1;
            return authorizedPredicates.Distinct();
        }

        /// <summary>
        /// This method returns the authorized predicates out of the given list of repository level predicates.
        /// </summary>
        /// <param name="token">AuthenticatedToken of the logged on user.</param>
        /// <param name="predicateUris">List of predicate uri's to be looked for. A list of repository level predicates like 'Create' is expected
        /// by this method.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <returns>List of repository level predicate uris (out of the list given by the user)
        /// for which the logged on user has authorization.</returns>
        public static IQueryable<string> GetAuthorizedPredicates(
                                                            this AuthenticatedToken token, 
                                                            IQueryable<string> predicateUris,
                                                            ZentityContext context)
        {
            #region Parameter Validation
            ValidateToken(token);
            ValidateParameters(context, predicateUris);
            #endregion
            Identity currentUser = GetIdentity(context, token);

            ////Query for user's relationships as subject with the resource.
            IQueryable<string> predicatesQuery = context.Relationships.Where(
                        rel => rel.Subject.Id == currentUser.Id
                            && rel.Object.Id == currentUser.Id)
                            .Select(rel => rel.Predicate.Uri);

            ////Get the group ids for groups to which the user belongs.
            var groupIds = context.Relationships.Where(tuple =>
                tuple.Predicate.Uri.Equals(AuthorizingPredicates.IdentityBelongsToGroups, StringComparison.OrdinalIgnoreCase)
                && tuple.Subject.Id == currentUser.Id).Select(tuple => tuple.Object)
                .OfType<Group>().Select(tuple => tuple.Id).ToArray();
            if (groupIds != null)
            {
                for (int i = 0; i < groupIds.Length; i++)
                {
                    Guid id = groupIds[i];

                    ////Append the query criteria for each group's relationships as subject with the resource.
                    predicatesQuery = predicatesQuery.Union(context.Relationships.Where(
                        rel => rel.Subject.Id == id
                            && rel.Object.Id == id)
                            .Select(rel => rel.Predicate.Uri));
                }
            }

            var authorizedPredicates = from uri1 in predicateUris
                                       join uri2 in predicatesQuery.ToList() on uri1 equals uri2
                                       select uri1;
            return authorizedPredicates.Distinct();
        }

        /// <summary>
        /// This method returns the authorized predicates out of the given list of predicates for the given resource.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derivative</typeparam>
        /// <param name="resource">Resource for which the authorized predicates are to be retrieved.</param>
        /// <param name="predicateUris">List of predicate uri's to be looked for.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <returns>List of predicate uris (out of the list given by the user)
        /// for which the logged on user has authorization.</returns>
        /// <remarks>This method assumes that the authenticated token has been stored in TLS slot named 'AuthenticatedToken'</remarks>
        public static IQueryable<string> GetAuthorizedPredicates<T>(
                                                                this T resource, 
                                                                IQueryable<string> predicateUris,
                                                                ZentityContext context) where T : Resource
        {
            #region Parameter Validation
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            ValidateParameters(context, predicateUris);
            #endregion
            AuthenticatedToken token = null;
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot("AuthenticatedToken");
            token = Thread.GetData(slot) as AuthenticatedToken;
            if (token != null)
            {
                var authorizedPredicates = resource.GetAuthorizedPredicates<T>(predicateUris, token, context);
                return authorizedPredicates;
            }
            else
            {
                Thread.FreeNamedDataSlot("AuthenticatedToken");
                throw new AuthorizationException(Resources.TokenUnavailableInTLS);
            }
        }

        /// <summary>
        /// This method returns the authorized predicates out of the given list of repository level predicates.
        /// </summary>
        /// <param name="predicateUris">List of predicate uri's to be looked for. A list of repository level predicates like 'Create' is expected
        /// by this method.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <returns>List of repository level predicate uris (out of the list given by the user)
        /// for which the logged on user has authorization.</returns>
        /// <remarks>This method assumes that the authenticated token has been stored in TLS slot named 'AuthenticatedToken'</remarks>
        public static IQueryable<string> GetAuthorizedPredicates(IQueryable<string> predicateUris, ZentityContext context)
        {
            #region Parameter Validation
            ValidateParameters(context, predicateUris);
            #endregion
            AuthenticatedToken token = null;
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot("AuthenticatedToken");
            token = Thread.GetData(slot) as AuthenticatedToken;
            if (token != null)
            {
                var authorizedPredicates = token.GetAuthorizedPredicates(predicateUris, context);
                return authorizedPredicates;
            }
            else
            {
                Thread.FreeNamedDataSlot("AuthenticatedToken");
                throw new AuthorizationException(Resources.TokenUnavailableInTLS);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Ges the identity of the entity accessessing zentity core.
        /// </summary>
        /// <param name="context">Zentity context</param>
        /// <param name="token">Authenticated tokend </param>
        /// <returns>Identity of the entity accessessing zentity core</returns>
        private static Identity GetIdentity(ZentityContext context, AuthenticatedToken token)
        {
            Identity identity = context.Resources.OfType<Identity>()
               .Where(s => s.IdentityName == token.IdentityName).First();
            if (identity == null)
            {
                throw new AuthenticationException(Resources.InvalidToken);
            }

            return identity;
        }

        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="context">Zentity context</param>
        /// <param name="authorizingPredicateUri">Authorizing predicate uri</param>
        private static void ValidateParameters(ZentityContext context, string authorizingPredicateUri)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (string.IsNullOrEmpty(authorizingPredicateUri))
            {
                throw new ArgumentNullException("authorizingPredicateUri");
            }
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
                throw new AuthorizationException(Resources.InvalidToken);
            }
        }

        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="context">Zentity context</param>
        /// <param name="authorizingPredicateUris">Authorizing predicate uri</param>
        private static void ValidateParameters(ZentityContext context, IQueryable<string> authorizingPredicateUris)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (authorizingPredicateUris == null)
            {
                throw new ArgumentNullException("authorizingPredicateUris");
            }
        }
        #endregion
    }
}
