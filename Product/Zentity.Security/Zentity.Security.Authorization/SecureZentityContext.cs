// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.EntityClient;
    using System.Data.Objects;
    using System.Linq;
    using System.Threading;
    using Zentity.Core;
    using Zentity.Security.Authentication;

    /// <summary>
    /// Represents a secure zentity object context that authorizes the access
    /// on the resources in zentity core.
    /// </summary>
    public class SecureZentityContext : ZentityContext
    {
        #region Private Members

        private AuthenticatedToken token;
        private string authorizingPredicateUri;
        private Identity currentIdentity;
        private IQueryable<Resource> resources;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the SecureZentityContext class.
        /// </summary>
        /// <example>
        /// <code>
        ///    //Add the following connection string to the application configuration file. Modify it to suit your environment
        ///    //&lt;connectionStrings&gt;
        ///    //    &lt;add name=&quot;SecureZentityContext&quot; connectionString=&quot;metadata=res://Zentity.Security.Authorization;
        ///    //         provider=System.Data.SqlClient;
        ///    //         provider connection string=&quot;Data Source=localhost;Initial Catalog=Zentity;Integrated Security=True;MultipleActiveResultSets=True&quot;&quot;
        ///    //providerName=&quot;System.Data.EntityClient&quot;/&gt;
        ///    //  &lt;/connectionStrings&gt;
        ///    using (SecureZentityContext context = new SecureZentityContext())
        ///    {
        ///        //Use the secure zentity context
        ///    }
        /// </code>
        /// </example>
        public SecureZentityContext()
            : base(ConfigurationManager.ConnectionStrings["SecureZentityContext"].ConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SecureZentityContext class.
        /// </summary>
        /// <param name="connectionString">Connection string for the authorization store.</param>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// string connectionString = "metadata=res://Zentity.Security.Authorization;" +
        /// "provider=System.Data.SqlClient;" +
        /// "provider connection string=\"Data Source=.; Initial Catalog=Zentity; Integrated Security=True; MultipleActiveResultSets=True\"";
        ///
        /// SecureZentityContext secureContext = new SecureZentityContext(connectionString);
        /// secureContext.Token = authenticatedToken;
        /// secureContext.AuthorizingPredicateUri = authorizingPredicate;
        /// </code>
        /// </example>
        public SecureZentityContext(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SecureZentityContext class.
        /// </summary>
        /// <param name="connection">Connection to the authorization store.</param>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// string connectionString = "metadata=res://Zentity.Security.Authorization;" +
        /// "provider=System.Data.SqlClient;" +
        /// "provider connection string=\"Data Source=.; Initial Catalog=Zentity; Integrated Security=True; MultipleActiveResultSets=True\"";
        /// EntityConnection connection = new EntityConnection(connectionString);
        ///
        /// SecureZentityContext secureContext = new SecureZentityContext(connection);
        /// secureContext.Token = authenticatedToken;
        /// secureContext.AuthorizingPredicateUri = authorizingPredicate;
        /// </code>
        /// </example>
        public SecureZentityContext(EntityConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SecureZentityContext class.
        /// The authenticated token is assumed to be in
        /// TLS(Thread Local Storage) under the named data slot 'AuthenticatedToken'.
        /// </summary>
        /// <param name="connectionString">Connection string for the authorization store.</param>
        /// <param name="authorizingPredicateUri">
        /// Predicate corresponding to the permission on the basis of which the context will
        /// perform authorization.
        /// </param>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// string connectionString = "metadata=res://Zentity.Security.Authorization;" +
        /// "provider=System.Data.SqlClient;" +
        /// "provider connection string=\"Data Source=.; Initial Catalog=Zentity; Integrated Security=True; MultipleActiveResultSets=True\"";
        ///
        /// LocalDataStoreSlot localSlot = Thread.AllocateNamedDataSlot("AuthenticatedToken");
        /// Thread.SetData(localSlot, authenticatedToken);
        ///
        /// SecureZentityContext secureContext = new SecureZentityContext(connectionString, authorizingPredicate);
        /// </code>
        /// </example>
        public SecureZentityContext(string connectionString, string authorizingPredicateUri) :
            this(connectionString, GetTokenFromThreadLocalStorage(), authorizingPredicateUri)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SecureZentityContext class.
        /// The authenticated token is assumed to be in
        /// TLS(Thread Local Storage) under the named data slot 'AuthenticatedToken'.
        /// </summary>
        /// <param name="connection">Connection to the authorization store.</param>
        /// <param name="authorizingPredicateUri">
        /// Predicate corresponding to the permission on the basis of which the context will
        /// perform authorization.
        /// </param>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// string connectionString = "metadata=res://Zentity.Security.Authorization;" +
        /// "provider=System.Data.SqlClient;" +
        /// "provider connection string=\"Data Source=.; Initial Catalog=Zentity; Integrated Security=True; MultipleActiveResultSets=True\"";
        /// EntityConnection connection = new EntityConnection(connectionString);
        ///
        /// LocalDataStoreSlot localSlot = Thread.AllocateNamedDataSlot("AuthenticatedToken");
        /// Thread.SetData(localSlot, authenticatedToken);
        ///
        /// SecureZentityContext secureContext = new SecureZentityContext(connection, authorizingPredicate);
        /// </code>
        /// </example>
        public SecureZentityContext(EntityConnection connection, string authorizingPredicateUri) :
            this(connection, GetTokenFromThreadLocalStorage(), authorizingPredicateUri)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SecureZentityContext class.
        /// </summary>
        /// <param name="connectionString">Connection string for the authorization store.</param>
        /// <param name="token">
        /// The <see cref="AuthenticatedToken"/> corresponding to the identity that is to be
        /// authorized.
        /// </param>
        /// <param name="authorizingPredicateUri">
        /// Predicate corresponding to the permission on the basis of which the context will
        /// perform authorization.
        /// </param>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// string connectionString = "metadata=res://Zentity.Security.Authorization;" +
        /// "provider=System.Data.SqlClient;" +
        /// "provider connection string=\"Data Source=.; Initial Catalog=Zentity; Integrated Security=True; MultipleActiveResultSets=True\"";
        ///
        /// SecureZentityContext secureContext = new SecureZentityContext(connectionString, authenticatedToken, authorizingPredicate);
        /// </code>
        /// </example>
        public SecureZentityContext(string connectionString, AuthenticatedToken token, string authorizingPredicateUri)
            : base(connectionString)
        {
            this.token = token;
            this.authorizingPredicateUri = authorizingPredicateUri;
            this.SavingChanges += new EventHandler(this.SecureZentityContext_SavingChanges);
        }

        /// <summary>
        /// Initializes a new instance of the SecureZentityContext class.
        /// </summary>
        /// <param name="connection">Connection to the authorization store.</param>
        /// <param name="token">
        /// The <see cref="AuthenticatedToken"/> corresponding to the identity that is to be
        /// authorized.
        /// </param>
        /// <param name="authorizingPredicateUri">
        /// Predicate corresponding to the permission on the basis of which the context will
        /// perform authorization.
        /// </param>
        /// <example>
        /// <code>
        /// SecurityToken credentialToken = new UserNameSecurityToken("User1", "User1!@#");
        /// IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
        /// AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);
        ///
        /// string authorizingPredicate = "urn:zentity/module/zentity-authorization/predicate/has-read-access";
        /// string connectionString = "metadata=res://Zentity.Security.Authorization;" +
        /// "provider=System.Data.SqlClient;" +
        /// "provider connection string=\"Data Source=.; Initial Catalog=Zentity; Integrated Security=True; MultipleActiveResultSets=True\"";
        /// EntityConnection connection = new EntityConnection(connectionString);
        ///
        /// SecureZentityContext secureContext = new SecureZentityContext(connection, authenticatedToken, authorizingPredicate);
        /// </code>
        /// </example>
        public SecureZentityContext(EntityConnection connection, AuthenticatedToken token, string authorizingPredicateUri)
            : base(connection)
        {
            this.token = token;
            this.authorizingPredicateUri = authorizingPredicateUri;
            this.SavingChanges += new EventHandler(this.SecureZentityContext_SavingChanges);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the accessible resources through the context.
        /// </summary>
        public new IQueryable<Resource> Resources
        {
            get
            {
                if (this.resources == null)
                {
                    this.SetAuthorizedResources();
                }

                return this.resources;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="AuthenticatedToken"/> corresponding to the identity that has been
        /// authenticated.
        /// </summary>
        public AuthenticatedToken Token
        {
            get
            {
                return this.token;
            }

            set
            {
                this.token = value;
                this.resources = null;
            }
        }

        /// <summary>
        /// Gets or sets the predicate corresponding to the permission against which
        /// authorization is to be done.
        /// </summary>
        public string AuthorizingPredicateUri
        {
            get
            {
                return this.authorizingPredicateUri;
            }

            set
            {
                this.authorizingPredicateUri = value;
                this.resources = null;
            }
        }

        /// <summary>
        /// Gets the authenticated <see cref="Identity"/> in the current authenticated token.
        /// </summary>
        public Identity CurrentIdentity
        {
            get { return this.currentIdentity; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The authenticated token is extracted from TLS(Thread Local Storage)
        /// from the named data slot 'AuthenticatedToken'.
        /// </summary>
        /// <returns>The <see cref="AuthenticatedToken"/> corresponding to the identity that has been
        /// authenticated.</returns>
        private static AuthenticatedToken GetTokenFromThreadLocalStorage()
        {
            try
            {
                LocalDataStoreSlot slot = Thread.GetNamedDataSlot("AuthenticatedToken");
                return Thread.GetData(slot) as AuthenticatedToken;
            }
            catch
            {
                throw new AuthorizationException(Zentity.Security.Authorization.Properties.Resources.TokenUnavailableInTLS);
            }
        }

        /// <summary>
        /// Set the resources on which <see cref="CurrentIdentity"/> has the access.
        /// </summary>
        private void SetAuthorizedResources()
        {
            this.resources = null;

            if (this.token != null && !string.IsNullOrEmpty(this.authorizingPredicateUri))
            {
                this.currentIdentity = base.Resources.OfType<Identity>()
                    .Where(s => s.IdentityName == this.token.IdentityName).FirstOrDefault();

                if (this.currentIdentity != null)
                {
                    this.resources = base.Relationships
                        .Where(rel => rel.Predicate.Uri == this.authorizingPredicateUri && rel.Subject.Id == this.currentIdentity.Id)
                        .Select(rel => rel.Object) as ObjectQuery<Resource>;

                    IQueryable<Group> groupsOfIdentity = base.Relationships
                        .Where(tuple => tuple.Predicate.Uri == AuthorizingPredicates.IdentityBelongsToGroups && tuple.Subject.Id == this.currentIdentity.Id)
                        .Select(tuple => tuple.Object as Group);

                    if (groupsOfIdentity != null)
                    {
                        foreach (Group group in groupsOfIdentity)
                        {
                            this.resources = this.resources.Concat(
                                    this.Relationships.Where(tuple => tuple.Predicate.Uri == this.authorizingPredicateUri
                                    && tuple.Subject.Id == group.Id).Select(tuple => tuple.Object)) as ObjectQuery<Resource>;
                        }
                    }
                }
                else
                {
                    throw new AuthenticationException(Zentity.Security.Authorization.Properties.Resources.InvalidToken);
                }
            }
        }

        /// <summary>
        /// Event handler that is raised before SaveChanges method on context is called.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Argument of the event.</param>
        private void SecureZentityContext_SavingChanges(object sender, EventArgs e)
        {
            #region Validation

            if (this.token == null || string.IsNullOrEmpty(this.authorizingPredicateUri))
            {
                throw new ArgumentNullException(Zentity.Security.Authorization.Properties.Resources.NullTokenOrUriException);
            }

            Identity authIdentity = base.Resources.OfType<Identity>().
                    Include("RelationshipsAsSubject.Predicate").FirstOrDefault(s => s.IdentityName == this.token.IdentityName);
            if (authIdentity == null)
            {
                throw new AuthenticationException(Zentity.Security.Authorization.Properties.Resources.InvalidToken);
            }

            #endregion

            #region Create Resource Permission Check

            foreach (ObjectStateEntry entry in this.ObjectStateManager.GetObjectStateEntries(
                         EntityState.Added).Where(tuple => tuple.Entity is Resource))
            {
                if (authIdentity.RelationshipsAsSubject.Where(r => r.Predicate.Uri == AuthorizingPredicates.HasCreateAccess).Count() == 0)
                {
                    throw new AuthorizationException(Zentity.Security.Authorization.Properties.Resources.CreateAccessDeniedException);
                }
            }

            #endregion

            #region Update Resource Permission Check

            foreach (ObjectStateEntry entry in this.ObjectStateManager.GetObjectStateEntries(
                EntityState.Modified).Where(tuple => tuple.Entity is Resource))
            {
                int count = 0;
                try
                {
                    Guid id = (entry.Entity as Resource).Id;

                    IEnumerable<Relationship> relationships = authIdentity.RelationshipsAsSubject
                        .Where(r => (r.Predicate.Uri == AuthorizingPredicates.HasUpdateAccess) && (r.Object.Id == id));

                    count = relationships.Count();
                }
                catch (EntityException)
                {
                    count = 0;
                }
                catch (NullReferenceException)
                {
                    count = 0;
                }

                if (count == 0)
                {
                    throw new AuthorizationException(Zentity.Security.Authorization.Properties.Resources.UpdateAccessDeniedException);
                }
            }

            #endregion

            #region Delete Resource Permission Check

            foreach (ObjectStateEntry entry in this.ObjectStateManager.GetObjectStateEntries(
                EntityState.Deleted).Where(tuple => tuple.Entity is Resource))
            {
                using (ZentityContext context
                    = new ZentityContext(this.Connection.ConnectionString))
                {
                    context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly);

                    Guid id = (entry.Entity as Resource).Id;

                    Relationship relationship = context.Relationships.Where(tuple => tuple.Subject.Id == authIdentity.Id
                        && tuple.Predicate.Uri == AuthorizingPredicates.HasDeleteAccess
                        && tuple.Object.Id == id).FirstOrDefault();

                    if (relationship == null)
                    {
                        throw new AuthorizationException(Zentity.Security.Authorization.Properties.Resources.DeleteAccessDeniedException);
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}
