// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthorizationHelper
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Globalization;
    using System.Linq;
    using Zentity.Core;
    using Zentity.Security.Authentication;
    using Zentity.Security.AuthenticationProvider;
    using Zentity.Security.Authorization;

    /// <summary>
    /// This class provides API for user creation.
    /// </summary>
    public static class UserManager
    {
        #region Properties for guest/admin/all users config settings
        /// <summary>
        /// Gets the name of the guest user.
        /// </summary>
        public static string GuestUserName
        {
            get { return DataAccess.GuestUserName; }
        }

        /// <summary>
        /// Gets the All users group name.
        /// </summary>
        public static string AllUsersGroupName
        {
            get { return DataAccess.AllUsersGroupName; }
        }

        /// <summary>
        /// Gets the admin group name.
        /// </summary>
        public static string AdminGroupName
        {
            get { return DataAccess.AdminGroupName; }
        }

        /// <summary>
        /// Gets the built in administrator's logon name.
        /// </summary>
        public static string AdminUserName
        {
            get { return DataAccess.AdminUserName; }
        }
        #endregion

        #region Create user overloads

        /// <summary>
        /// Creates a new user and adds it to authentication and authorization stores.
        /// </summary>
        /// <param name="user">User object with all information.</param>
        /// <param name="token">Token of user who is allowed to create a user.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if user is created successfully, else false.</returns>
        public static bool CreateUser(ZentityUser user, AuthenticatedToken token, string connectionString)
        {
            ////The CreateUser overload called below handles parameter validation
            using (ZentityContext context = new ZentityContext(connectionString))
            {
                return CreateUser(user, token, context); 
            }
        }

        /// <summary>
        /// Creates a new user and adds it to authentication and authorization stores.
        /// </summary>
        /// <param name="user">User object with all information.</param>
        /// <param name="token">Token of user who is allowed to create a user.</param>
        /// <returns>True if user is created successfully, else false.</returns>
        public static bool CreateUser(ZentityUser user, AuthenticatedToken token)
        {
            ////The CreateUser overload called below handles parameter validation
            using (ZentityContext context = new ZentityContext())
            {
                return CreateUser(user, token, context);
            }
        }

        #endregion

        #region Update user overloads

        /// <summary>
        /// Updates user information.
        /// </summary>
        /// <param name="user">User object with all information.</param>
        /// <param name="token">Token of user who is allowed to update the user.</param>
        /// <returns>True if user is updated successfully, else false.</returns>
        public static bool UpdateUser(ZentityUser user, AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateToken(token);
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.LogOnName == AdminUserName || user.LogOnName == GuestUserName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorOrGuestUpdateException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext())
            {
                Identity identity = GetIdentity(user.LogOnName, context);
                identity.Title = user.Profile.FirstName + " " + user.Profile.LastName;

                return UpdateUser(user, identity, token);
            }
        }

        /// <summary>
        /// Updates user information.
        /// </summary>
        /// <param name="user">User object with all information.</param>
        /// <param name="token">Token of user who is allowed to update the user.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if user is updated successfully, else false.</returns>
        public static bool UpdateUser(ZentityUser user, AuthenticatedToken token, string connectionString)
        {
            #region Parameter Validation
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.LogOnName == AdminUserName || user.LogOnName == GuestUserName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorOrGuestUpdateException);
            }

            ValidateStrings("connectionString", connectionString);
            #endregion

            using (ZentityContext context = new ZentityContext(connectionString))
            {

                Identity identity = GetIdentity(user.LogOnName, context);
                identity.Title = user.Profile.FirstName + " " + user.Profile.LastName;

                return UpdateUser(user, identity, token);
            }
        }

        /// <summary>
        /// Updates user information.
        /// </summary>
        /// <param name="user">User object with all information.</param>
        /// <param name="identity">Identity object with all information.</param>
        /// <param name="token">Token of user who is allowed to update the user.</param>
        /// <returns>True if user is updated successfully, else false.</returns>
        public static bool UpdateUser(ZentityUser user, Identity identity, AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateIdentity(identity);
            ValidateToken(token);
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.LogOnName == AdminUserName || user.LogOnName == GuestUserName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorOrGuestUpdateException);
            }

            if (user.LogOnName != identity.IdentityName)
            {
                throw new ArgumentException(string.Format(
                                                    CultureInfo.CurrentUICulture,
                                                    ConstantStrings.InvalidZentityUserAndIdentityObject));
            }
            #endregion

            using (ZentityContext context = new ZentityContext())
            {
                if (!UpdateIdentity(identity, token, context))
                {
                    return false;
                }

                if (!UpdateZentityUser(user))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Updates user information.
        /// </summary>
        /// <param name="user">User object with all information.</param>
        /// <param name="identity">Identity object with all information.</param>
        /// <param name="token">Token of user who is allowed to update the user.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if user is updated successfully, else false.</returns>
        public static bool UpdateUser(ZentityUser user, Identity identity, AuthenticatedToken token, string connectionString)
        {
            #region Parameter Validation
            ValidateIdentity(identity);
            ValidateToken(token);
            ValidateStrings("connectionString", connectionString);
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.LogOnName == AdminUserName || user.LogOnName == GuestUserName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorOrGuestUpdateException);
            }

            if (user.LogOnName != identity.IdentityName)
            {
                throw new ArgumentException(string.Format(
                                                        CultureInfo.CurrentUICulture,
                                                        ConstantStrings.InvalidZentityUserAndIdentityObject));
            }
            #endregion

            using (ZentityContext context = new ZentityContext(connectionString))
            {

                if (!UpdateIdentity(identity, token, context))
                {
                    return false;
                }

                if (!UpdateZentityUser(user))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Updates user information.
        /// </summary>
        /// <param name="identity">Identity object with all information.</param>
        /// <param name="token">Token of user who is allowed to update the user.</param>
        /// <returns>True if user is updated successfully, else false.</returns>
        public static bool UpdateUser(Identity identity, AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateToken(token);
            ValidateIdentity(identity);
            if (identity.IdentityName == AdminUserName || identity.IdentityName == GuestUserName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorOrGuestUpdateException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext())
            {
                return UpdateIdentity(identity, token, context);
            }
        }

        /// <summary>
        /// Updates user information.
        /// </summary>
        /// <param name="identity">Identity object with all information.</param>
        /// <param name="token">Token of user who is allowed to create the user.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if user is updated successfully, else false.</returns>
        public static bool UpdateUser(Identity identity, AuthenticatedToken token, string connectionString)
        {
            #region Parameter Validation
            ValidateToken(token);
            ValidateStrings("connectionString", connectionString);
            ValidateIdentity(identity);
            if (identity.IdentityName == AdminUserName || identity.IdentityName == GuestUserName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorOrGuestUpdateException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext(connectionString))
            {
                return UpdateIdentity(identity, token, context);
            }
        }

        #endregion

        #region Delete user overloads

        /// <summary>
        /// Deletes a user from authentication and authorization stores.
        /// </summary>
        /// <param name="user">User object with all information.</param>
        /// <param name="token">Token of user who is allowed to delete the user.</param>
        /// <returns>True if user is deleted successfully, else false.</returns>
        public static bool DeleteUser(ZentityUser user, AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateToken(token);
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.LogOnName == AdminUserName || user.LogOnName == GuestUserName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorOrGuestUpdateException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext())
            {
                return DeleteUser(user, token, context); 
            }
        }

        /// <summary>
        /// Deletes a user from authentication and authorization stores.
        /// </summary>
        /// <param name="user">User object with all information.</param>
        /// <param name="token">Token of user who is allowed to delete the user.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if user is deleted successfully, else false.</returns>
        public static bool DeleteUser(ZentityUser user, AuthenticatedToken token, string connectionString)
        {
            #region Parameter Validation
            ValidateToken(token);
            ValidateStrings("connectionString", connectionString);
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.LogOnName == AdminUserName || user.LogOnName == GuestUserName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorOrGuestUpdateException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext(connectionString))
            {
                return DeleteUser(user, token, context); 
            }
        }

        #endregion

        #region Create group overloads

        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <param name="group">Group object with all information.</param>
        /// <param name="token">Token of user who is allowed to create a group.</param>
        /// <returns>True if group is created successfully, else false.</returns>
        public static bool CreateGroup(Group group, AuthenticatedToken token)
        {
            using (ZentityContext context = new ZentityContext())
            {
                return CreateGroup(group, token, context);
            }
        }

        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <param name="group">Group object with all information.</param>
        /// <param name="token">Token of user who is allowed to create a group.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if group is created successfully, else false.</returns>
        public static bool CreateGroup(Group group, AuthenticatedToken token, string connectionString)
        {
            #region Parameter Validation
            ValidateStrings("connectionString", connectionString);
            #endregion

            using (ZentityContext context = new ZentityContext(connectionString))
            {
                return CreateGroup(group, token, context); 
            }
        }

        #endregion

        #region Update group overloads

        /// <summary>
        /// Updates group information.
        /// </summary>
        /// <param name="group">Group object with all information.</param>
        /// <param name="token">Token of user who is allowed to update the group.</param>
        /// <returns>True if group is updated successfully, else false.</returns>
        public static bool UpdateGroup(Group group, AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateGroup(group);
            ValidateToken(token);
            #endregion

            if (group.GroupName == AdminGroupName || group.GroupName == AllUsersGroupName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorsOrAllUsersUpdateException);
            }

            using (ZentityContext context = new ZentityContext())
            {
                return UpdateGroup(group, token, context);
            }
        }

        /// <summary>
        /// Updates group information.
        /// </summary>
        /// <param name="group">Group object with all information.</param>
        /// <param name="token">Token of user who is allowed to update the group.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if group is updated successfully, else false.</returns>
        public static bool UpdateGroup(Group group, AuthenticatedToken token, string connectionString)
        {
            #region Parameter Validation
            ValidateStrings("connectionString", connectionString);
            ValidateToken(token);
            ValidateGroup(group);
            if (group.GroupName == AdminGroupName || group.GroupName == AllUsersGroupName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorsOrAllUsersUpdateException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext(connectionString))
            {
                return UpdateGroup(group, token, context); 
            }
        }

        #endregion

        #region Delete group overloads

        /// <summary>
        /// Deletes a group.
        /// </summary>
        /// <param name="group">Group object with all information.</param>
        /// <param name="token">Token of user who is allowed to delete the group.</param>
        /// <returns>True if group is deleted successfully, else false.</returns>
        public static bool DeleteGroup(Group group, AuthenticatedToken token)
        {
            #region Parameter Validation
            ValidateToken(token);
            ValidateGroup(group);
            if (group.GroupName == AdminGroupName || group.GroupName == AllUsersGroupName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorsOrAllUsersUpdateException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext())
            {
                return DeleteGroup(group, token, context); 
            }
        }

        /// <summary>
        /// Deletes a group.
        /// </summary>
        /// <param name="group">Group object with all information.</param>
        /// <param name="token">Token of user who is allowed to delete the group.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if group is deleted successfully, else false.</returns>
        public static bool DeleteGroup(Group group, AuthenticatedToken token, string connectionString)
        {
            #region Parameter Validation
            ValidateToken(token);
            ValidateStrings("connectionString", connectionString);
            ValidateGroup(group);
            if (group.GroupName == AdminGroupName || group.GroupName == AllUsersGroupName)
            {
                throw new ArgumentException(ConstantStrings.AdministratorsOrAllUsersUpdateException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext(connectionString))
            {
                return DeleteGroup(group, token, context); 
            }
        }

        #endregion

        #region Retrieval operations

        /// <summary>
        /// Returns all Identity objects
        /// </summary>
        /// <param name="context">ZentityContext object</param>
        /// <returns>Query which will return all identities when it is enumerated</returns>
        public static ObjectQuery<Identity> GetAllIdentities(ZentityContext context)
        {
            #region Parameter Validation
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            #endregion

            return context.Resources.OfType<Identity>();
        }

        /// <summary>
        /// Returns all Group objects
        /// </summary>
        /// <param name="context">ZentityContext object</param>
        /// <returns>Query which will return all groups when it is enumerated</returns>
        public static ObjectQuery<Group> GetAllGroups(ZentityContext context)
        {
            #region Parameter Validation
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            #endregion
            return context.Resources.OfType<Group>();
        }

        /// <summary>
        /// Returns an Identity object based on identity name.
        /// </summary>
        /// <param name="identityName">Name of identity.</param>
        /// <param name="context">Object of ZentityContext.</param>
        /// <returns>Returns Identity object.</returns>
        public static Identity GetIdentity(string identityName, ZentityContext context)
        {
            #region Parameter Validation
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ValidateStrings("identityName", identityName);
            #endregion
            context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly);

            return context.Resources.OfType<Identity>()
                .Where(res => res.IdentityName == identityName).FirstOrDefault();
        }

        /// <summary>
        /// Returns a Group object based on group name.
        /// </summary>
        /// <param name="groupName">Name of group.</param>
        /// <param name="context">Object of ZentityContext.</param>
        /// <returns>Returns Group object.</returns>
        public static Group GetGroup(string groupName, ZentityContext context)
        {
            #region Parameter Validation
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ValidateStrings("groupName", groupName);
            #endregion
            context.MetadataWorkspace.LoadFromAssembly(typeof(Group).Assembly);
            return context.Resources.OfType<Group>()
                .Where(res => res.GroupName == groupName).FirstOrDefault();
        }

        #endregion

        #region Add Identity to Group overloads

        /// <summary>
        /// Adds an identity to group.
        /// </summary>
        /// <param name="identity">Identity object with all information.</param>
        /// <param name="group">Group object with all information.</param>
        /// <param name="authenticatedToken">Token of user who is allowed to add identity to group.</param>
        /// <returns>True if identity is added successfully to group, else false.</returns>
        public static bool AddIdentityToGroup(Identity identity, Group group, AuthenticatedToken authenticatedToken)
        {
            #region Parameter Validation
            ValidateIdentity(identity);
            ValidateGroup(group);
            ValidateToken(authenticatedToken);
            if (identity.IdentityName == AdminUserName || identity.IdentityName == GuestUserName
                || group.GroupName == AllUsersGroupName)
            {
                throw new ArgumentException(ConstantStrings.BuiltInUserToBuiltInGroupException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext())
            {
                return AddIdentityToGroup(identity, group, authenticatedToken, context);
            }
        }

        /// <summary>
        /// Adds an identity to group.
        /// </summary>
        /// <param name="identity">Identity object with all information.</param>
        /// <param name="group">Group object with all information.</param>
        /// <param name="authenticatedToken">Token of user who is allowed to add identity to group.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if identity is added successfully to group, else false.</returns>
        public static bool AddIdentityToGroup(
                                        Identity identity, 
                                        Group group,
                                        AuthenticatedToken authenticatedToken, 
                                        string connectionString)
        {
            #region Parameter Validation
            ValidateIdentity(identity);
            ValidateGroup(group);
            ValidateToken(authenticatedToken);
            ValidateStrings("connectionString", connectionString);
            if (identity.IdentityName == AdminUserName || identity.IdentityName == GuestUserName
                || group.GroupName == AllUsersGroupName)
            {
                throw new ArgumentException(ConstantStrings.BuiltInUserToBuiltInGroupException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext(connectionString))
            {
                return AddIdentityToGroup(identity, group, authenticatedToken, context);
            }
        }

        #endregion

        #region Remove Identity from Group overloads

        /// <summary>
        /// Removes identity from group.
        /// </summary>
        /// <param name="identity">Identity object with all information.</param>
        /// <param name="group">Group object with all information.</param>
        /// <param name="authenticatedToken">Token of user who is allowed to remove identity from group.</param>
        /// <returns>True if identity is removed successfully to group, else false.</returns>
        public static bool RemoveIdentityFromGroup(
                                                Identity identity, 
                                                Group group,
                                                AuthenticatedToken authenticatedToken)
        {
            #region Parameter Validation
            ValidateIdentity(identity);
            ValidateGroup(group);
            ValidateToken(authenticatedToken);
            if (identity.IdentityName == AdminUserName || identity.IdentityName == GuestUserName
                || group.GroupName == AllUsersGroupName)
            {
                throw new ArgumentException(ConstantStrings.BuiltInUserToBuiltInGroupException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext())
            {
                return RemoveIdentityFromGroup(identity, group, authenticatedToken, context); 
            }
        }

        /// <summary>
        /// Removes identity from group.
        /// </summary>
        /// <param name="identity">Identity object with all information.</param>
        /// <param name="group">Group object with all information.</param>
        /// <param name="authenticatedToken">Token of user who is allowed to remove identity from group.</param>
        /// <param name="connectionString">Connection string of authorization store.</param>
        /// <returns>True if identity is removed successfully to group, else false.</returns>
        public static bool RemoveIdentityFromGroup(
                                                Identity identity, 
                                                Group group,
                                                AuthenticatedToken authenticatedToken, 
                                                string connectionString)
        {
            #region Parameter Validation
            ValidateIdentity(identity);
            ValidateGroup(group);
            ValidateToken(authenticatedToken);
            ValidateStrings("connectionString", connectionString);
            if (identity.IdentityName == AdminUserName || identity.IdentityName == GuestUserName
                || group.GroupName == AllUsersGroupName)
            {
                throw new ArgumentException(ConstantStrings.BuiltInUserToBuiltInGroupException);
            }
            #endregion

            using (ZentityContext context = new ZentityContext(connectionString))
            {
                return RemoveIdentityFromGroup(identity, group, authenticatedToken, context); 
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool CreateUser(ZentityUser user, AuthenticatedToken token, ZentityContext context)
        {
            #region Parameter Validation
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            ValidateParameters(context, token);
            #endregion

            context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly);

            if (!DataAccess.IsAdmin(token.IdentityName, context))
            {
                throw new UnauthorizedAccessException(ConstantStrings.UnauthorizedAccessException);
            }

            if (context.Resources.OfType<Identity>().Where(res => res.IdentityName == user.LogOnName).Count() == 1)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, ConstantStrings.IdentityExists, user.LogOnName));
            }

            if (!user.Register())
            {
                return false;
            }

            Identity identity = new Identity();
            identity.Title = user.Profile.FirstName + " " + user.Profile.LastName;
            identity.IdentityName = user.LogOnName;
            context.AddToResources(identity);
            return context.SaveChanges() == 0 ? false : true;
        }

        /// <summary>
        /// Updates the zentity user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool UpdateZentityUser(ZentityUser user)
        {
            return user.UpdateProfile();
        }

        /// <summary>
        /// Updates the identity.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool UpdateIdentity(Identity identity, AuthenticatedToken token, ZentityContext context)
        {
            if (!DataAccess.IsAdmin(token.IdentityName, context))
            {
                throw new UnauthorizedAccessException(ConstantStrings.UnauthorizedAccessException);
            }

            Identity originalIdentity = GetIdentity(identity.Id, context);

            if (originalIdentity == null)
            {
                throw new ArgumentException(string.Format(
                                                        CultureInfo.CurrentUICulture,
                                                        ConstantStrings.IdentityDoesNotExist, 
                                                        identity.IdentityName));
            }

            originalIdentity.Title = identity.Title;
            originalIdentity.Description = identity.Description;
            originalIdentity.Uri = identity.Uri;

            return context.SaveChanges() == 0 ? false : true;
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool DeleteUser(ZentityUser user, AuthenticatedToken token, ZentityContext context)
        {
            if (!DataAccess.IsAdmin(token.IdentityName, context))
            {
                throw new UnauthorizedAccessException(ConstantStrings.UnauthorizedAccessException);
            }

            Identity identity = GetIdentity(user.LogOnName, context);

            if (identity == null)
            {
                throw new ArgumentException(string.Format(
                                                        CultureInfo.CurrentUICulture,
                                                        ConstantStrings.IdentityDoesNotExist, 
                                                        identity.IdentityName));
            }

            // Remove relationships
            identity.RelationshipsAsObject.Load();
            identity.RelationshipsAsSubject.Load();
            foreach (Relationship relationship in identity.RelationshipsAsObject.Union(
                identity.RelationshipsAsSubject).ToList())
            {
                context.DeleteObject(relationship);
            }

            // Remove identity
            context.DeleteObject(identity);

            if (context.SaveChanges() == 0)
            {
                return false;
            }

            if (!user.Unregister())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool CreateGroup(Group group, AuthenticatedToken token, ZentityContext context)
        {
            #region Parameter Validation
            ValidateGroup(group);
            ValidateParameters(context, token);
            #endregion

            if (!DataAccess.IsAdmin(token.IdentityName, context))
            {
                throw new UnauthorizedAccessException(ConstantStrings.UnauthorizedAccessException);
            }

            Group existingGroup = context.Resources.OfType<Group>()
                .Where(res => res.GroupName == group.GroupName).FirstOrDefault();

            if (existingGroup != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, ConstantStrings.GroupExists, group.GroupName));
            }

            context.AddToResources(group);
            return context.SaveChanges() == 0 ? false : true;
        }

        /// <summary>
        /// Updates the group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool UpdateGroup(Group group, AuthenticatedToken token, ZentityContext context)
        {
            context.MetadataWorkspace.LoadFromAssembly(typeof(Group).Assembly);

            if (!DataAccess.IsAdmin(token.IdentityName, context))
            {
                throw new UnauthorizedAccessException(ConstantStrings.UnauthorizedAccessException);
            }

            Group originalGroup = GetGroup(group.Id, context);

            if (originalGroup == null)
            {
                throw new ArgumentException(string.Format(
                                                        CultureInfo.CurrentUICulture,
                                                        ConstantStrings.GroupDoesNotExist, 
                                                        group.GroupName));
            }

            if (group.GroupName != originalGroup.GroupName)
            {
                if (context.Resources.OfType<Group>()
                    .Where(res => res.GroupName == group.GroupName).Count() > 0)
                {
                    throw new ArgumentException(string.Format(
                                                            CultureInfo.CurrentUICulture,
                                                            ConstantStrings.GroupUpdateException, 
                                                            group.GroupName));
                }
            }

            originalGroup.GroupName = group.GroupName;
            originalGroup.Title = group.Title;
            originalGroup.Description = group.Description;
            originalGroup.Uri = group.Uri;

            return context.SaveChanges() == 0 ? false : true;
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool DeleteGroup(Group group, AuthenticatedToken token, ZentityContext context)
        {
            if (!DataAccess.IsAdmin(token.IdentityName, context))
            {
                throw new UnauthorizedAccessException(ConstantStrings.UnauthorizedAccessException);
            }

            Group existingGroup = GetGroup(group.Id, context);

            if (existingGroup == null)
            {
                throw new ArgumentException(string.Format(
                                                        CultureInfo.CurrentUICulture,
                                                        ConstantStrings.IdentityDoesNotExist, 
                                                        existingGroup.GroupName));
            }

            existingGroup.RelationshipsAsObject.Load();
            existingGroup.RelationshipsAsSubject.Load();

            List<Relationship> relationships = existingGroup.RelationshipsAsSubject.ToList();
            foreach (Relationship relationship in relationships)
            {
                context.DeleteObject(relationship);
            }

            relationships = existingGroup.RelationshipsAsObject.ToList();
            foreach (Relationship relationship in relationships)
            {
                context.DeleteObject(relationship);
            }

            context.DeleteObject(existingGroup);
            return context.SaveChanges() == 0 ? false : true;
        }

        /// <summary>
        /// Gets the identity.
        /// </summary>
        /// <param name="identityID">The identity ID.</param>
        /// <param name="context">The context.</param>
        /// <returns>Identity with the given id.</returns>
        private static Identity GetIdentity(Guid identityID, ZentityContext context)
        {
            context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly);

            return context.Resources.OfType<Identity>()
                .Where(res => res.Id == identityID).FirstOrDefault();
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <param name="groupID">The group ID.</param>
        /// <param name="context">The context.</param>
        /// <returns>Group with the given group id.</returns>
        private static Group GetGroup(Guid groupID, ZentityContext context)
        {
            context.MetadataWorkspace.LoadFromAssembly(typeof(Group).Assembly);

            return context.Resources.OfType<Group>()
                .Where(res => res.Id == groupID).FirstOrDefault();
        }

        /// <summary>
        /// Adds the identity to group.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="group">The group.</param>
        /// <param name="authenticatedToken">The authenticated token.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool AddIdentityToGroup(
                                            Identity identity, 
                                            Group group,
                                            AuthenticatedToken authenticatedToken, 
                                            ZentityContext context)
        {
            if (!DataAccess.IsAdmin(authenticatedToken.IdentityName, context))
            {
                throw new UnauthorizedAccessException(ConstantStrings.UnauthorizedAccessException);
            }

            Identity originalIdentity = GetIdentity(identity.Id, context);
            Group originalGroup = GetGroup(group.Id, context);

            if (originalIdentity == null || originalGroup == null)
            {
                throw new ArgumentException(ConstantStrings.InvalidIdentityOrGroup);
            }

            originalIdentity.Groups.Load();
            if (originalIdentity.Groups.Contains(originalGroup))
            {
                return true;
            }

            if (group.GroupName == AdminGroupName)
            {
                ZentityUserAdmin admin = new ZentityUserAdmin(authenticatedToken);
                admin.SetAdmin(identity.IdentityName);
            }

            originalIdentity.Groups.Add(originalGroup);
            return context.SaveChanges() == 0 ? false : true;
        }

        /// <summary>
        /// Removes the identity from group.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="group">The group.</param>
        /// <param name="authenticatedToken">The authenticated token.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean; true if successful, false otherwise.</returns>
        private static bool RemoveIdentityFromGroup(
                                                Identity identity, 
                                                Group group,
                                                AuthenticatedToken authenticatedToken, 
                                                ZentityContext context)
        {
            if (!DataAccess.IsAdmin(authenticatedToken.IdentityName, context))
            {
                throw new UnauthorizedAccessException(ConstantStrings.UnauthorizedAccessException);
            }

            Identity originalIdentity = GetIdentity(identity.Id, context);
            Group originalGroup = GetGroup(group.Id, context);

            if (originalIdentity == null || originalGroup == null)
            {
                throw new ArgumentException(ConstantStrings.InvalidIdentityOrGroup);
            }

            originalIdentity.Groups.Load();
            if (!originalIdentity.Groups.Contains(originalGroup))
            {
                return true;
            }

            if (group.GroupName == AdminGroupName)
            {
                ZentityUserAdmin admin = new ZentityUserAdmin(authenticatedToken);
                admin.UnsetAdmin(identity.IdentityName);
            }

            originalIdentity.Groups.Remove(originalGroup);
            return context.SaveChanges() == 0 ? false : true;
        }

        #region Parameter validation

        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="context">Zentity context</param>
        /// <param name="userToken">Authenticated token</param>
        private static void ValidateParameters(ZentityContext context, AuthenticatedToken userToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ValidateToken(userToken);
        }

        /// <summary>
        /// Validates the strings
        /// </summary>
        /// <param name="args">Argument array</param>
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
        /// Validates the Identity
        /// </summary>
        /// <param name="identity">Identity</param>
        private static void ValidateIdentity(Identity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
        }

        /// <summary>
        /// Validates teh group
        /// </summary>
        /// <param name="group">Group</param>
        private static void ValidateGroup(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
        }

        /// <summary>
        /// Validates the authenticated token
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

        #endregion

        #endregion
    }
}
