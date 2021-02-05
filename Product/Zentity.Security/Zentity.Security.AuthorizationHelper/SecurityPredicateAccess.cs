// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthorizationHelper
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    /// <summary>
    /// This class contains common utility methods
    /// </summary>
    internal static class SecurityPredicateAccess
    {
        private static List<SecurityPredicate> predicateList;

        /// <summary>
        /// Initializes static members of the SecurityPredicateAccess class.
        /// </summary>
        static SecurityPredicateAccess()
        {
            ReadPredicatesList();
        }

        /// <summary>
        /// Gets the security predicate.
        /// </summary>
        internal static IEnumerable<SecurityPredicate> SecurityPredicates
        {
            get { return predicateList.AsEnumerable(); }
        }

        /// <summary>
        /// Finds if the permission exists.
        /// </summary>
        /// <param name="permissionName">Permission name</param>
        /// <returns>System.Boolean; <c>true</c> if permission exists, <c>false</c> otherwise.</returns>
        internal static bool Exists(string permissionName)
        {
            return predicateList.Any(pr => pr.Name.Equals(permissionName));
        }

        /// <summary>
        /// Gets the predicate Uri
        /// </summary>
        /// <param name="predicateName">Predicate name</param>
        /// <returns>Predicate uri</returns>
        internal static string GetPredicateUri(string predicateName)
        {
            return predicateList.Where(tuple => tuple.Name.Equals(predicateName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault().Uri;
        }

        /// <summary>
        /// Gets the inverse Uri
        /// </summary>
        /// <param name="predicateName">Predicate name</param>
        /// <returns>Inverse Uri</returns>
        internal static string GetInverseUri(string predicateName)
        {
            return predicateList.Where(pr => pr.Name.Equals(predicateName, StringComparison.OrdinalIgnoreCase))
                   .FirstOrDefault().InverseUri;
        }

        /// <summary>
        /// Gets the lower priority predicates.
        /// </summary>
        /// <param name="permissionName">Name of the permission.</param>
        /// <returns>List of predicates.</returns>
        internal static IEnumerable<SecurityPredicate> GetLowerPriorityPredicates(string permissionName)
        {
            int currentPredicatePriority = predicateList
                .Where(pr => pr.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()
                .Priority;
            return predicateList.Where(pr => pr.Priority >= currentPredicatePriority
                && !pr.Name.Equals("Create", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the higher priority predicates.
        /// </summary>
        /// <param name="permissionName">Name of the permission.</param>
        /// <returns>List of predicates.</returns>
        internal static IEnumerable<SecurityPredicate> GetHigherPriorityPredicates(string permissionName)
        {
            int currentPredicatePriority = predicateList
                .Where(pr => pr.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()
                .Priority;
            return predicateList.Where(pr => pr.Priority <= currentPredicatePriority
                && !pr.Name.Equals("Create", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the name of the permission.
        /// </summary>
        /// <param name="predicateUri">The predicate URI.</param>
        /// <returns>Permission name.</returns>
        internal static string GetPermissionName(string predicateUri)
        {
            return predicateList.Where(pr => pr.Uri.Equals(predicateUri, StringComparison.OrdinalIgnoreCase))
                .Select(pr => pr.Name).FirstOrDefault();
        }

        /// <summary>
        /// Gets the resource level predicate uris.
        /// </summary>
        /// <returns>Resource level predicate uri's.</returns>
        internal static IQueryable<string> GetResourceLevelPredicateUris()
        {
            List<string> uris = SecurityPredicateAccess.SecurityPredicates.Where(pr => pr.Priority != 0)
                .Select(pr => pr.Uri).ToList();
            uris.AddRange(SecurityPredicateAccess.SecurityPredicates.Where(pr => pr.Priority != 0)
                .Select(pr => pr.InverseUri));
            return uris.AsQueryable();
        }
        
        /// <summary>
        /// Gets the repository level predicate uris.
        /// </summary>
        /// <returns>Repository level predicate url's.</returns>
        internal static IQueryable<string> GetRepositoryLevelPredicateUris()
        {
            List<string> uris = SecurityPredicateAccess.SecurityPredicates.Where(pr => pr.Priority == 0)
                .Select(pr => pr.Uri).ToList();
            uris.AddRange(SecurityPredicateAccess.SecurityPredicates.Where(pr => pr.Priority == 0)
                .Select(pr => pr.InverseUri));
            return uris.AsQueryable();
        }

        /// <summary>
        /// Gets the permission names.
        /// </summary>
        /// <param name="uris">The uris.</param>
        /// <returns>List of premission names.</returns>
        internal static List<string> GetPermissionNames(IEnumerable<string> uris)
        {
            List<string> permissionNames = new List<string>();
            foreach (string uri in uris)
            {
                SecurityPredicate pr = SecurityPredicateAccess.SecurityPredicates.Where(sp =>
                    sp.Uri.Equals(uri, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (pr != null)
                {
                    permissionNames.Add(pr.Name);
                }
                else
                {
                    pr = SecurityPredicateAccess.SecurityPredicates.Where(sp =>
                        sp.InverseUri.Equals(uri, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (pr != null)
                    {
                        permissionNames.Add(pr.Name);
                    }
                }
            }

            ////Process 'read' permission - if no deny read is present add read permission to the list
            if (!uris.Contains(SecurityPredicateAccess.GetInverseUri("Read")))
            {
                permissionNames.Add("Read");
            }

            return permissionNames;
        }

        /// <summary>
        /// Reads the predicates list.
        /// </summary>
        private static void ReadPredicatesList()
        {
            predicateList = new List<SecurityPredicate>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ConstantStrings.SecurityConfiguration);
            XmlNodeList lst = doc.SelectNodes(@"//SecurityPredicates/add");
            foreach (XmlNode node in lst)
            {
                string name = node.Attributes["Name"].Value;
                string uri = node.Attributes["Uri"].Value;
                string inverseUri = node.Attributes["InversePredicateUri"].Value;
                int priority = Convert.ToInt32(node.Attributes["Priority"].Value, CultureInfo.InvariantCulture);
                SecurityPredicate predicate = new SecurityPredicate { Name = name, Uri = uri, InverseUri = inverseUri, Priority = priority };
                predicateList.Add(predicate);
            }
        }
    }
}
