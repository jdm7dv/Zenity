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
    using System.Linq;
    using System.Text;
    using Zentity.Core;
    using Zentity.Security.Authorization.Properties;

    /// <summary>
    /// Represents a group of <see cref="Identity"/>.
    /// </summary>
    public partial class Group
    {
        /// <summary>
        /// Returns all groups which have given predicates authorization on the resource.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derivative</typeparam>
        /// <param name="resource">Resource for which the authorization is to be verified</param>
        /// <param name="predicateUris">List of predicate uris for authorization</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>List of Group objects with authorization on the given resource.</returns>
        public static IQueryable<Group> GetAuthorizedGroups<T>(T resource, IQueryable<string> predicateUris, ZentityContext context)
            where T : Resource
        {
            #region Parameter Validation
            ValidateParameters(predicateUris, context);
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            #endregion

            ////Get all identities which have (identity predicate resource) relationship
            IQueryable<Relationship> allAuthorizedRelationships = (context.Relationships.Where(rel => rel.Object.Id == resource.Id && rel.Subject is Group) as ObjectQuery<Relationship>)
                                                                    .Include("Predicate")
                                                                    .Include("Subject");
            IQueryable<Relationship> authorizedRelationships = from uri in predicateUris
                                                               join rel in allAuthorizedRelationships on uri equals rel.Predicate.Uri
                                                               select rel;
            IQueryable<Group> authorizedGroups = authorizedRelationships.Select(rel => rel.Subject).OfType<Group>().Distinct();
            return authorizedGroups;
        }

        /// <summary>
        /// Verfies whether the current group has a particular permission.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derived type</typeparam>
        /// <param name="authorizingPredicateUri">
        /// Predicate corresponding to the permission.
        /// </param>
        /// <param name="resource">
        /// Resource against which permission is to be checked.
        /// </param>
        /// <param name="context">
        /// Zentity Object Context.
        /// </param>
        /// <returns>
        /// Boolean indicating whether the group has access or not.
        /// </returns>
        /// <exception cref="AuthorizationException">Exception to indicate invalid information or other general exception.</exception>
        /// <example>
        /// <code>
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// using (ZentityContext context = new ZentityContext())
        /// {
        ///     Group group = context.Resources.OfType&lt;Group&gt;()
        ///         .Where(tuple => tuple.GroupName.Equals("Group1")).First();
        ///
        ///     Resource resource = context.Resources.Where(tuple => tuple.Title.Equals("Resource1")).First();
        ///
        ///     bool hasAccess = group.VerifyAuthorization(authorizingPredicate, resource, context));
        /// }
        /// </code>
        /// </example>
        public bool VerifyAuthorization<T>(string authorizingPredicateUri, T resource, ZentityContext context) where T : Resource
        {
            #region Parameter validation
            ValidateParameters(authorizingPredicateUri, context);
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            #endregion

            try
            {
                IEnumerable<Relationship> relationships = context.Relationships
                    .Where(tuple => tuple.Subject.Id == this.Id
                        && tuple.Predicate.Uri == authorizingPredicateUri
                        && tuple.Object.Id == resource.Id);

                if (relationships == null || relationships.Count() == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (EntityException exception)
            {
                throw new AuthorizationException(Resources.VerifyAuthorizationException, exception);
            }
        }

        /// <summary>
        /// This method verifies authorization for a repository level predicate, e.g. Create.
        /// </summary>
        /// <param name="authorizingPredicateUri">Predicate URI for a repository level, (which is NOT resource specific)
        /// predicate. e.g. Create</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if the current instance has permission for the given repository level predicate.</returns>
        public bool VerifyAuthorization(string authorizingPredicateUri, ZentityContext context)
        {
            #region Parameter Validation
            ValidateParameters(authorizingPredicateUri, context);
            #endregion
            try
            {
                IEnumerable<Relationship> relationships = context.Relationships
                    .Where(tuple => tuple.Subject.Id == this.Id
                        && tuple.Predicate.Uri == authorizingPredicateUri
                        && tuple.Object.Id == this.Id);

                if (relationships == null || relationships.Count() == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                throw new AuthorizationException(Resources.VerifyAuthorizationException, exception);
            }
        }

        /// <summary>
        /// Grants permission on resource for the current group.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derived type</typeparam>
        /// <param name="authorizingPredicateUri">
        /// Predicate corresponding to the permission.
        /// </param>
        /// <param name="resource">
        /// Resource on which the permission is to be granted.
        /// </param>
        /// <param name="context">
        /// Zentity Object Context.
        /// </param>
        /// <returns>
        /// Boolean indicating whether the permission was successfully granted or not.
        /// </returns>
        /// <exception cref="AuthorizationException">Exception to indicate invalid information or other general exception.</exception>
        /// <example>
        /// <code>
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// using (ZentityContext context = new ZentityContext())
        /// {
        ///     Group group = context.Resources.OfType&lt;Group&gt;()
        ///         .Where(tuple => tuple.GroupName.Equals("Group1")).First();
        ///     Resource resource = context.Resources.Where(tuple => tuple.Title.Equals("Resource1")).First();
        ///
        ///     group.GrantAuthorization(authorizingPredicate, resource, context);
        ///     context.SaveChanges();
        /// }
        /// </code>
        /// </example>
        public bool GrantAuthorization<T>(string authorizingPredicateUri, T resource, ZentityContext context) where T : Resource
        {
            #region Parameter Validation
            ValidateParameters(authorizingPredicateUri, context);
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            #endregion
            try
            {
                Relationship existingRelationship = context.Relationships.Where(rel =>
                    rel.Subject.Id == this.Id
                    && rel.Object.Id == resource.Id
                    && rel.Predicate.Uri.Equals(authorizingPredicateUri, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (existingRelationship == null)
                {
                    // Check if relationship is present in context in added state
                    foreach (ObjectStateEntry objectStateEntry in
                        context.ObjectStateManager.GetObjectStateEntries(EntityState.Added))
                    {
                        existingRelationship = objectStateEntry.Entity as Relationship;

                        // Check if relationship is same on which Grant is requested
                        if (existingRelationship != null
                            && (existingRelationship.Subject.Id == this.Id
                                && existingRelationship.Object.Id == resource.Id
                                && existingRelationship.Predicate.Uri.Equals(authorizingPredicateUri, StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }
                    }

                    Relationship relationship = new Relationship();
                    relationship.Subject = this;
                    relationship.Object = resource;
                    relationship.Predicate = context.Predicates.Where(s => s.Uri == authorizingPredicateUri).First<Predicate>();

                    context.AddToRelationships(relationship);
                }
                else
                {
                    // Check if relationship is present in context in added, modified or deleted state
                    foreach (ObjectStateEntry objectStateEntry in
                        context.ObjectStateManager.GetObjectStateEntries(EntityState.Deleted
                        | EntityState.Modified))
                    {
                        Relationship relationshipEntity = objectStateEntry.Entity as Relationship;

                        if (relationshipEntity != null
                            && relationshipEntity.Id == existingRelationship.Id)
                        {
                            //// If its in deleted state, make it in unchanged
                            if (objectStateEntry.State == EntityState.Deleted)
                            {
                                objectStateEntry.AcceptChanges();
                            }
                            else
                            {
                                //// If modified, then add a new relationship
                                Relationship relationship = new Relationship();
                                relationship.Subject = this;
                                relationship.Object = resource;
                                relationship.Predicate = context.Predicates.Where(s => s.Uri == authorizingPredicateUri).First<Predicate>();

                                context.AddToRelationships(relationship);
                            }

                            break;
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                throw new AuthorizationException(Resources.GrantAuthorizationException, exception);
            }
        }

        /// <summary>
        /// This method grants authorization for repository level (non-resource-specific) predicates, e.g. Create.
        /// </summary>
        /// <param name="authorizingPredicateUri">Uri for a non-resource specific predicate, e.g. Create</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if the current group instance has authorization for the given predicate, false otherwise.</returns>
        public bool GrantAuthorization(string authorizingPredicateUri, ZentityContext context)
        {
            #region Parameter validation
            ValidateParameters(authorizingPredicateUri, context);
            #endregion
            try
            {
                Relationship existingRelationship = context.Relationships.Where(rel =>
                    rel.Subject.Id == this.Id
                    && rel.Object.Id == this.Id
                    && rel.Predicate.Uri.Equals(authorizingPredicateUri, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (existingRelationship == null)
                {
                    // Check if relationship is present in context in added state
                    foreach (ObjectStateEntry objectStateEntry in
                        context.ObjectStateManager.GetObjectStateEntries(EntityState.Added))
                    {
                        existingRelationship = objectStateEntry.Entity as Relationship;

                        // Check if relationship is same on which Grant is requested
                        if (existingRelationship != null
                            && (existingRelationship.Subject.Id == this.Id
                                && existingRelationship.Object.Id == this.Id
                                && existingRelationship.Predicate.Uri.Equals(authorizingPredicateUri, StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }
                    }

                    Relationship relationship = new Relationship();
                    relationship.Subject = this;
                    relationship.Object = this;
                    relationship.Predicate = context.Predicates.Where(s => s.Uri == authorizingPredicateUri).First<Predicate>();

                    context.AddToRelationships(relationship);
                }
                else
                {
                    // Check if relationship is present in context in added, modified or deleted state
                    foreach (ObjectStateEntry objectStateEntry in
                        context.ObjectStateManager.GetObjectStateEntries(EntityState.Deleted
                        | EntityState.Modified))
                    {
                        Relationship relationshipEntity = objectStateEntry.Entity as Relationship;

                        if (relationshipEntity != null
                            && relationshipEntity.Id == existingRelationship.Id)
                        {
                            // If its in deleted state, make it in unchanged
                            if (objectStateEntry.State == EntityState.Deleted)
                            {
                                objectStateEntry.AcceptChanges();
                            }
                            else
                            {
                                //// If modified, then add a new relationship
                                Relationship relationship = new Relationship();
                                relationship.Subject = this;
                                relationship.Object = this;
                                relationship.Predicate = context.Predicates.Where(s => s.Uri == authorizingPredicateUri).First<Predicate>();

                                context.AddToRelationships(relationship);
                            }

                            break;
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                throw new AuthorizationException(Resources.GrantAuthorizationException, exception);
            }
        }

        /// <summary>
        /// Revokes permission on resource for the current group.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derived type</typeparam>
        /// <param name="authorizingPredicateUri">
        /// Predicate corresponding to the permission.
        /// </param>
        /// <param name="resource">
        /// Resource on which the permission is to be granted.
        /// </param>
        /// <param name="context">
        /// Zentity Object Context.
        /// </param>
        /// <returns>
        /// Boolean indicating whether the permission was successfully revoked or not.
        /// </returns>
        /// <exception cref="AuthorizationException">Exception to indicate invalid information or other general exception.</exception>
        /// <example>
        /// <code>
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// using (ZentityContext context = new ZentityContext())
        /// {
        ///     Group group = context.Resources.OfType&lt;Group&gt;()
        ///         .Where(tuple => tuple.GroupName.Equals("Group1")).First();
        ///     Resource resource = context.Resources.Where(tuple => tuple.Title.Equals("Resource1")).First();
        ///
        ///     group.RevokeAuthorization(authorizingPredicate, resource, context);
        ///     context.SaveChanges();
        /// }
        /// </code>
        /// </example>
        public bool RevokeAuthorization<T>(string authorizingPredicateUri, T resource, ZentityContext context)
            where T : Resource
        {
            #region Parameter Validation
            ValidateParameters(authorizingPredicateUri, context);
            #endregion
            try
            {
                Relationship relationship =
                    context.Relationships.Where(rel => rel.Subject.Id == this.Id && rel.Object.Id == resource.Id &&
                            rel.Predicate.Uri == authorizingPredicateUri).FirstOrDefault();
                if (relationship != null)
                {
                    context.DeleteObject(relationship);
                }
                else
                {
                    // Check if relationship is present in context in added, modified or deleted state
                    foreach (ObjectStateEntry objectStateEntry in
                        context.ObjectStateManager.GetObjectStateEntries(EntityState.Added))
                    {
                        Relationship existingRelationship = objectStateEntry.Entity as Relationship;

                        // Check if relationship is same on which Grant is requested
                        if (existingRelationship != null
                            && (existingRelationship.Subject.Id == this.Id
                                && existingRelationship.Object.Id == resource.Id
                                && existingRelationship.Predicate.Uri.Equals(authorizingPredicateUri, StringComparison.OrdinalIgnoreCase)))
                        {
                            objectStateEntry.Delete();
                            break;
                        }
                    }
                }

                return true;
            }
            catch (EntityException exception)
            {
                throw new AuthorizationException(Resources.RevokeAuthorizationException, exception);
            }
        }

        /// <summary>
        /// This method revokes authorization for a repository level predicate. e.g. Create
        /// </summary>
        /// <param name="authorizingPredicateUri">Uri of a repository level predicate. A repository level predicate is not
        /// specific to a single resource. e.g. Create is a repository level permission.</param>
        /// <param name="context">ZentityContext object</param>
        /// <returns>True if revoke is successful, false otherwise.</returns>
        public bool RevokeAuthorization(string authorizingPredicateUri, ZentityContext context)
        {
            #region Parameter Validation
            ValidateParameters(authorizingPredicateUri, context);
            #endregion

            try
            {
                Relationship relationship =
                    context.Relationships.Where(rel => rel.Subject.Id == this.Id && rel.Object.Id == this.Id &&
                            rel.Predicate.Uri == authorizingPredicateUri).FirstOrDefault();
                if (relationship != null)
                {
                    context.DeleteObject(relationship);
                }
                else
                {
                    // Check if relationship is present in context in added, modified or deleted state
                    foreach (ObjectStateEntry objectStateEntry in
                        context.ObjectStateManager.GetObjectStateEntries(EntityState.Added))
                    {
                        Relationship existingRelationship = objectStateEntry.Entity as Relationship;

                        // Check if relationship is same on which Grant is requested
                        if (existingRelationship != null
                            && (existingRelationship.Subject.Id == this.Id
                                && existingRelationship.Object.Id == this.Id
                                && existingRelationship.Predicate.Uri.Equals(authorizingPredicateUri, StringComparison.OrdinalIgnoreCase)))
                        {
                            objectStateEntry.Delete();
                            break;
                        }
                    }
                }

                return true;
            }
            catch (EntityException exception)
            {
                throw new AuthorizationException(Resources.RevokeAuthorizationException, exception);
            }
        }

        /// <summary>
        /// This method returns all authorized resources for the current group for the given predicate.
        /// </summary>
        /// <param name="context">ZentityContext object</param>
        /// <param name="authorizingPredicateUri">Uri for the permission predicate</param>
        /// <returns>Query which, when enumerated, will give all resources for which the group has given permission.</returns>
        public IQueryable<Resource> GetAuthorizedResources(ZentityContext context, string authorizingPredicateUri)
        {
            #region Parameter Validation
            ValidateParameters(authorizingPredicateUri, context);
            #endregion

            IQueryable<Resource> groupFilteredResources =
                context.Relationships.Where(tuple => tuple.Predicate.Uri.Equals(authorizingPredicateUri, StringComparison.OrdinalIgnoreCase)
                && tuple.Subject.Id == this.Id).Select(tuple => tuple.Object);

            return groupFilteredResources;
        }

        /// <summary>
        /// This method returns all authorized resources for the current group for the given predicate.
        /// </summary>
        /// <param name="context">ZentityContext object</param>
        /// <param name="authorizingPredicateUris">Predicate uris to be authorized</param>
        /// <returns>Query which, when enumerated, will give all resources for which the group has given permission.</returns>
        public IQueryable<Resource> GetAuthorizedResources(ZentityContext context, IQueryable<string> authorizingPredicateUris)
        {
            #region Parameter Validation
            ValidateParameters(authorizingPredicateUris, context);
            #endregion

            IQueryable<Relationship> allAuthorizedRelationships =
                (context.Relationships.Where(tuple => tuple.Subject.Id == this.Id) as ObjectQuery<Relationship>)
                .Include("Predicate")
                .Include("Object");
            IQueryable<Relationship> authorizedRelationships = from uri in authorizingPredicateUris
                                                    join rel in allAuthorizedRelationships on uri equals rel.Predicate.Uri
                                                    select rel;
            return authorizedRelationships.Select(rel => rel.Object).Distinct();
        }

        /// <summary>
        /// This method returns the authorized predicates out of the given list of predicates for the given resource.
        /// </summary>
        /// <typeparam name="T">Zentity.Core.Resource or its derivative</typeparam>
        /// <param name="resource">Resource for which the authorized predicates are to be retrieved.</param>
        /// <param name="predicateUris">List of predicate uri's to be looked for.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <returns>List of predicate uris (out of the list given by the user)
        /// for which the current group instance has authorization.</returns>
        public IQueryable<string> GetAuthorizedPredicates<T>(T resource, IQueryable<string> predicateUris, ZentityContext context)
            where T : Resource
        {
            #region Parameter Validation
            ValidateParameters(predicateUris, context);
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            #endregion

            IQueryable<string> predicatesQuery = context.Relationships.Where(
                        rel => rel.Subject.Id == this.Id
                            && rel.Object.Id == resource.Id)
                            .Select(rel => rel.Predicate.Uri);
            var authorizedPredicates = from uri1 in predicateUris
                                       join uri2 in predicatesQuery.ToList() on uri1 equals uri2
                                       select uri1;
            return authorizedPredicates.Distinct();
        }

        /// <summary>
        /// This method returns the authorized predicates out of the given list of repository level predicates.
        /// </summary>
        /// <param name="predicateUris">List of predicate uri's to be looked for. A list of repository level predicates like 'Create' is expected
        /// by this method.</param>
        /// <param name="context">ZentityContext object.</param>
        /// <returns>List of repository level predicate uris (out of the given list)
        /// for which the current group instance has authorization.</returns>
        public IQueryable<string> GetAuthorizedPredicates(IQueryable<string> predicateUris, ZentityContext context)
        {
            #region Parameter Validation
            ValidateParameters(predicateUris, context);
            #endregion

            ////Query for user's relationships as subject with the resource.
            IQueryable<string> predicatesQuery = context.Relationships.Where(
                        rel => rel.Subject.Id == this.Id
                            && rel.Object.Id == this.Id)
                            .Select(rel => rel.Predicate.Uri);

            var authorizedPredicates = from uri1 in predicateUris
                                       join uri2 in predicatesQuery.ToList() on uri1 equals uri2
                                       select uri1;
            return authorizedPredicates.Distinct();
        }

        #region Private Methods
        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="authorizingPredicateUri">Authorizing predicate uri</param>
        /// <param name="context">Zentity context</param>
        private static void ValidateParameters(string authorizingPredicateUri, ZentityContext context)
        {
            if (string.IsNullOrEmpty(authorizingPredicateUri))
            {
                throw new ArgumentNullException("authorizingPredicateUri");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
        }

        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="authorizingPredicateUris">Authorizing predicate uri</param>
        /// <param name="context">Zentity context</param>
        private static void ValidateParameters(IQueryable<string> authorizingPredicateUris, ZentityContext context)
        {
            if (authorizingPredicateUris == null)
            {
                throw new ArgumentNullException("authorizingPredicateUris");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
        }
        #endregion
    }
}
