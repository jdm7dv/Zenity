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
    using System.Xml;
    using Zentity.Core;
    using Zentity.Security.Authentication;
    using Zentity.Security.Authorization;

    /// <summary>
    /// This class provides methods to query the identities and groups data
    /// </summary>
    internal static class DataAccess
    {
        #region Private variables
        private static string adminGroupName;
        private static string adminUserName;
        private static string memberOfPredicateUri;
        private static string allUsersGroupName;
        private static string guestUserName;
        #endregion

        #region Static constructor

        /// <summary>
        /// Initializes static members of the DataAccess class.
        /// </summary>
        static DataAccess()
        {
            ReadSecuritySettings();
        }
        #endregion

        #region Internal properties
        /// <summary>
        /// Gets the Guest user name from security config setting.
        /// </summary>
        internal static string GuestUserName
        {
            get { return guestUserName; }
        }

        /// <summary>
        /// Gets the all users group name from security config setting.
        /// </summary>
        internal static string AllUsersGroupName
        {
            get { return allUsersGroupName; }
        }

        /// <summary>
        /// Gets the admin group name from security config setting.
        /// </summary>
        internal static string AdminGroupName
        {
            get { return adminGroupName; }
        }

        /// <summary>
        /// Gets the admin user name from security config setting.
        /// </summary>
        internal static string AdminUserName
        {
            get { return adminUserName; }
        }

        /// <summary>
        /// Gets the member of uri.
        /// </summary>
        internal static string MemberOfUri
        {
            get { return memberOfPredicateUri; }
        }
        #endregion

        #region Get objects from context
        /// <summary>
        /// Retrieves group having the given name
        /// </summary>
        /// <param name="groupName">Group name to retieve</param>
        /// <param name="context">Zentity context</param>
        /// <returns>Group name</returns>
        internal static Group GetGroup(string groupName, ZentityContext context)
        {
            Group group = context.Resources.OfType<Group>()
                .Where(iden => iden.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            return group;
        }

        /// <summary>
        /// Gets the identity with the given name
        /// </summary>
        /// <param name="identityName">Identity name</param>
        /// <param name="context">Zentity context</param>
        /// <returns>Identity with the given name</returns>
        internal static Identity GetIdentity(string identityName, ZentityContext context)
        {
            Identity user = context.Resources.OfType<Identity>()
                .Where(iden => iden.IdentityName.Equals(identityName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Gets the predicate with the given uri.
        /// </summary>
        /// <param name="predicateUri">Predicate uri</param>
        /// <param name="context">Zentity context</param>
        /// <returns>Predicate with the given uri</returns>
        internal static Predicate GetPredicate(string predicateUri, ZentityContext context)
        {
            Predicate createPredicate = context.Predicates.Where<Zentity.Core.Predicate>(
                pr => pr.Uri.Equals(predicateUri, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            return createPredicate;
        }

        #endregion

        #region IsAdmin overloads
        
        /// <summary>
        /// Determines whether the specified identity name is admin.
        /// </summary>
        /// <param name="identityName">Name of the identity.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// 	<c>true</c> if the specified identity name is admin; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsAdmin(string identityName, ZentityContext context)
        {
            Identity currentUser = GetIdentity(identityName, context);
            if (currentUser == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, ConstantStrings.IdentityDoesNotExist, identityName));
            }

            return IsAdmin(currentUser);
        }

        /// <summary>
        /// Indicates if the given identity is an adminstrator.
        /// </summary>
        /// <param name="user">Identity</param>
        /// <returns>System.Boolean; <c>true</c> if its an admin identity, <c>false</c> otherwise.</returns>
        internal static bool IsAdmin(Identity user)
        {
            user.Groups.Load();
            bool isAdmin = user.Groups.Where(grp => grp.GroupName.Equals(adminGroupName, StringComparison.OrdinalIgnoreCase)).Any();
            return isAdmin;
        }

        /// <summary>
        /// Indicates if the given group is an adminstrator group.
        /// </summary>
        /// <param name="group">Group</param>
        /// <returns>System.Boolean; <c>true</c> if its an admin group, <c>false</c> otherwise.</returns>
        internal static bool IsAdmin(Group group)
        {
            return string.Equals(adminGroupName, group.GroupName, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region IsOwner() overloads

        /// <summary>
        /// Determines whether the specified token is owner.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="token">The token.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// 	<c>true</c> if the specified token is owner; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsOwner<T>(AuthenticatedToken token, T resource, ZentityContext context)
            where T : Resource
        {
            return GetOwnedResources<T>(token, context).Any(res => res.Id == resource.Id);
        }

        /// <summary>
        /// Returns a value indicating if the identity is the explicit 
        /// owner of the resource.
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="group">Group</param>
        /// <param name="resource">Resource</param>
        /// <param name="context">Zentity context</param>
        /// <returns>System.Boolean; <c>true</c> if is explicit 
        /// owner, <c>false</c> otherwise</returns>
        internal static bool IsExplicitOwner<T>(Group group, Resource resource, ZentityContext context)
            where T : Resource
        {
            return !group.VerifyAuthorization(SecurityPredicateAccess.GetInverseUri("Owner"), resource, context)
                && group.VerifyAuthorization(SecurityPredicateAccess.GetPredicateUri("Owner"), resource, context);
        }

        /// <summary>
        /// Returns a value indicating if the identity is the explicit 
        /// owner of the resource.
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="identity">Identity</param>
        /// <param name="resource">Resource</param>
        /// <param name="context">Zentity context</param>
        /// <returns>System.Boolean; <c>true</c> if the identity is the explicit 
        /// owner, <c>false</c> otherwise</returns>
        internal static bool IsExplicitOwner<T>(Identity identity, Resource resource, ZentityContext context)
            where T : Resource
        {
            return !identity.VerifyAuthorization(SecurityPredicateAccess.GetInverseUri("Owner"), resource, context)
                && identity.VerifyAuthorization(SecurityPredicateAccess.GetPredicateUri("Owner"), resource, context);
        }

        #endregion

        /// <summary>
        /// Gets a value indicating if the user with the given 
        /// username is a guest.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <returns>System.Boolean; <c>true</c> if the user is a guest, 
        /// <c>false</c> otherwise</returns>
        internal static bool IsGuest(string userName)
        {
            return userName.Equals(GuestUserName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Finds if an identity is a member of the given group.
        /// </summary>
        /// <param name="identityName">Identity name</param>
        /// <param name="group">Group</param>
        /// <returns>System.Boolean; <c>true</c> if the identity is a member of the group, 
        /// <c>false</c> otherwise</returns>
        internal static bool IsMemberOf(string identityName, Group group)
        {
            return group.Identities.Any(iden => iden.IdentityName.Equals(identityName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the owned resources.
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="token">Authenticated token</param>
        /// <param name="context">Zentity context</param>
        /// <returns>List of resources types</returns>
        internal static IQueryable<T> GetOwnedResources<T>(AuthenticatedToken token, ZentityContext context)
            where T : Resource
        {
            string ownerUri = SecurityPredicateAccess.GetPredicateUri("Owner");
            string denyOwnerUri = SecurityPredicateAccess.GetInverseUri("Owner");
            Identity currentUser = GetIdentity(token.IdentityName, context);
            Group allUsers = GetGroup(AllUsersGroupName, context);
            if (currentUser != null)
            {
                IQueryable<T> explicitOwnedResources = currentUser.GetAuthorizedResources(context, ownerUri).OfType<T>();
                IQueryable<T> allOwnedResources = token.GetAuthorizedResources(context, ownerUri)
                    .Concat(allUsers.GetAuthorizedResources(context, ownerUri)).OfType<T>();
                IQueryable<T> allDeniedResources = token.GetAuthorizedResources(context, denyOwnerUri)
                    .Concat(allUsers.GetAuthorizedResources(context, denyOwnerUri)).OfType<T>();
                return allOwnedResources.Except(allDeniedResources).Union(explicitOwnedResources);
            }
            else
            {
                return new List<T>(0).AsQueryable();
            }
        }

        /// <summary>
        /// Gets the administrator identities.
        /// </summary>
        /// <param name="context">Zentity context</param>
        /// <returns>List of identities</returns>
        internal static IEnumerable<Identity> GetAdministratorIdentities(ZentityContext context)
        {
            Group adminGroup = (context.Resources.OfType<Group>()
                .Where(grp => grp.GroupName.Equals(AdminGroupName, StringComparison.OrdinalIgnoreCase))
                as ObjectQuery<Group>)
                .Include("Identities")
                .FirstOrDefault();
            return adminGroup.Identities;
        }

        #region Private methods
        /// <summary>
        /// Reads the security settings.
        /// </summary>
        private static void ReadSecuritySettings()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ConstantStrings.SecurityConfiguration);
            XmlNodeList lst = doc.SelectNodes(@"//appSettings/add");
            foreach (XmlNode setting in lst)
            {
                switch (setting.Attributes["key"].Value)
                {
                    case "AdminGroupName": adminGroupName = setting.Attributes["value"].Value;
                        break;
                    case "AdminUserName": adminUserName = setting.Attributes["value"].Value;
                        break;
                    case "AllUsersGroupName": allUsersGroupName = setting.Attributes["value"].Value;
                        break;
                    case "GuestUserName": guestUserName = setting.Attributes["value"].Value;
                        break;
                    case "MemberOfUri": memberOfPredicateUri = setting.Attributes["value"].Value;
                        break;
                }
            }
        }

        #endregion
    }
}
