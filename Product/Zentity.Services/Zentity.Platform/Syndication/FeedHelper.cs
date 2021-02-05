// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.EnterpriseLibrary.Caching;
    using Zentity.ScholarlyWorks;

    #region FeedHelper Class

    /// <summary>
    /// Feed Helper class handles all the queries which interact with the database.
    /// </summary>
    internal static class FeedHelper
    {
        #region Resources by Author

        /// <summary>
        /// Retrieves a collection of all the resources owned by the authorId
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="authorId">The associated author id</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A key value pair of author and his resources</returns>
        internal static List<T> GetResourcesByAuthor<T>(Guid authorId,
                                            ushort count, string connectionString) where T : ScholarlyWorks.ScholarlyWork
        {
            if(null == authorId || Guid.Empty == authorId)
            {
                throw new ArgumentNullException("authorId");
            }
            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {

                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.Contacts()
                                    .Where(author => author.Id == authorId)
                                    .SelectMany(author => author.AuthoredWorks)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }
            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;
        }

        /// <summary>
        /// Retrieves a collection of all the resources owned by the author whose email id has been specified
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="emailId">The associated author's email id</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A collection of key value pairs of author and his associated resources</returns>
        internal static List<T> GetResourcesByAuthor<T>(string emailId,
                                                                                  ushort count, string connectionString) where T : ScholarlyWorks.ScholarlyWork
        {
            if(string.IsNullOrEmpty(emailId))
            {
                throw new ArgumentNullException("emailId");
            }
            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {
                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;

                    listOfResources = zentityContext.Contacts()
                                    .Where(author => author.Email == emailId)
                                    .SelectMany(author => author.AuthoredWorks)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }
            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;

        }

        /// <summary>
        /// Retrieves a collection of all the resources owned by the author
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="firstName">The associated author's first name</param>
        /// <param name="lastName">The associated author's last name</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A collection of key value pairs of author and his associated resources</returns>
        internal static List<T> GetResourcesByAuthor<T>(string firstName,
                                                                                 string lastName,
                                                                                 ushort count,
                                                                                 string connectionString) where T : ScholarlyWorks.ScholarlyWork
        {
            if(string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName");
            }
            if(string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }
            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }


            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {
                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.People()
                                    .Where(author => author.FirstName == firstName &&
                                        author.LastName == lastName)
                                    .SelectMany(author => author.AuthoredWorks)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }
            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;

        }

        #endregion

        #region Resources by Tag

        /// <summary>
        /// Retrieves a collection of all the resources having with the specified tagId 
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="tagId">The associated tag id</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A key value pair of tag and its resources</returns>
        internal static List<T> GetResourcesByTag<T>(Guid tagId,
                                                                             ushort count,
                                                                             string connectionString) where T : ScholarlyWorks.ScholarlyWorkItem
        {
            if(null == tagId || Guid.Empty == tagId)
            {
                throw new ArgumentNullException("tagId");
            }

            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {
                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.Tags()
                                    .Where(tag => tag.Id == tagId)
                                    .SelectMany(tag => tag.ScholarlyWorkItems)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }
            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;

        }

        /// <summary>
        /// Retrieves a collection of all the resources having with the specified label for the tag 
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="tagLabel">The associated tag label</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A key value pair of tag and its resources</returns>
        internal static List<T> GetResourcesByTag<T>(string tagLabel,
                                                                           ushort count,
                                                                           string connectionString) where T : ScholarlyWorks.ScholarlyWorkItem
        {
            if(string.IsNullOrEmpty(tagLabel))
            {
                throw new ArgumentNullException("tagLabel");
            }
            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {
                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.Tags()
                                    .Where(tag => tag.Title == tagLabel)
                                    .SelectMany(tag => tag.ScholarlyWorkItems)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }

            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;

        }

        #endregion

        #region Resources by Category

        /// <summary>
        /// Retrieves a collection of all the resources having with the specified tagId 
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="tagId">The associated tag id</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A key value pair of tag and its resources</returns>
        internal static List<T> GetResourcesByCategory<T>(Guid tagId,
                                                                             ushort count,
                                                                             string connectionString) where T : ScholarlyWorks.ScholarlyWorkItem
        {
            if(null == tagId || Guid.Empty == tagId)
            {
                throw new ArgumentNullException("tagId");
            }

            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {
                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.CategoryNodes()
                                    .Where(tag => tag.Id == tagId)
                                    .SelectMany(tag => tag.ScholarlyWorkItems)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }
            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;

        }

        /// <summary>
        /// Retrieves a collection of all the resources having with the specified label for the tag 
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="tagLabel">The associated tag label</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A key value pair of tag and its resources</returns>
        internal static List<T> GetResourcesByCategory<T>(string tagLabel,
                                                                           ushort count,
                                                                           string connectionString) where T : ScholarlyWorks.ScholarlyWorkItem
        {
            if(string.IsNullOrEmpty(tagLabel))
            {
                throw new ArgumentNullException("tagLabel");
            }
            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {
                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.CategoryNodes()
                                    .Where(tag => tag.Title == tagLabel)
                                    .SelectMany(tag => tag.ScholarlyWorkItems)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }

            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;

        }

        #endregion

        #region Resources by Contributor

        /// <summary>
        /// Retrieves a collection of all the resources owned by the contributorId
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="contributorId">The associated contributor id</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A key value pair of author and his resources</returns>
        internal static List<T> GetResourcesByContributor<T>(Guid contributorId,
                                            ushort count, string connectionString) where T : ScholarlyWorks.ScholarlyWork
        {
            if(null == contributorId || Guid.Empty == contributorId)
            {
                throw new ArgumentNullException("contributorId");
            }
            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {

                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.Contacts()
                                    .Where(contributor => contributor.Id == contributorId)
                                    .SelectMany(contributor => contributor.ContributionInWorks)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }
            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;

        }

        /// <summary>
        /// Retrieves a collection of all the resources whose contributor's email Id has been specified
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="emailId">The associated contributor's email id</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A collection of key value pairs of author and his associated resources</returns>
        internal static List<T> GetResourcesByContributor<T>(string emailId,
                                            ushort count, string connectionString) where T : ScholarlyWorks.ScholarlyWork
        {
            if(string.IsNullOrEmpty(emailId))
            {
                throw new ArgumentNullException("emailId");
            }
            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {

                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.Contacts()
                                    .Where(contributor => contributor.Email == emailId)
                                    .SelectMany(contributor => contributor.ContributionInWorks)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }
            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;

        }

        /// <summary>
        /// Retrieves a collection of all the resources whose contributor's name has been specified 
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="firstName">The associated contributor's first name</param>
        /// <param name="lastName">The associated contributor's last name</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>A collection of key value pairs of author and his associated resources</returns>
        internal static List<T> GetResourcesByContributor<T>(string firstName,
                                            string lastName,
                                            ushort count, string connectionString) where T : ScholarlyWorks.ScholarlyWork
        {
            if(string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName");
            }
            if(string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }
            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;

            try
            {

                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.People()
                                    .Where(contributor => contributor.FirstName == firstName &&
                                            contributor.LastName == lastName)
                                    .SelectMany(author => author.ContributionInWorks)
                                    .OfType<T>()
                                    .OrderByDescending(resource => resource.DateModified)
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }
            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;
        }

        #endregion

        #region Resources by Date

        /// <summary>
        /// Retrieves a collection of all the resources that have recently been added or modified 
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="isForDateAdded">
        /// boolean indicating if we want to retrieve resources that have been currently added
        /// or those resources that have been recently modified</param>
        /// <param name="count">The number of resources that are to be retrieved</param>
        /// <param name="date">The date on which resources have been added or modified</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>List of resources</returns>
        internal static List<T> GetResourcesByDate<T>(bool isForDateAdded,
                                                      ushort count,
                                                      DateTime date,
                                                      string connectionString) where T : Core.Resource
        {
            if(0 >= count)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID, "count");
            }

            if(count > PlatformSettings.MaximumFeedCount)
            {
                count = PlatformSettings.MaximumFeedCount;
            }

            List<T> listOfResources = null;
            Func<T, bool> predicate = null;

            if(isForDateAdded)
            {
                predicate = resource => resource.DateAdded >= date;
            }
            else
            {
                predicate = resource => resource.DateModified >= date;
            }

            try
            {

                using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext(connectionString))
                {
                    zentityContext.CommandTimeout = PlatformSettings.ConnectionTimeout;
                    listOfResources = zentityContext.Resources
                                    .OfType<T>()
                                    .Where(predicate)
                                    .OrderByDescending(resource => resource.DateModified)
                                    .AsEnumerable()
                                    .Where(resource => !(resource is Contact || resource is Tag || resource is CategoryNode))
                                    .Take(count)
                                    .ToList();
                    LoadResources<T>(listOfResources);
                }
            }
            catch(ArgumentNullException)
            {
                throw;
            }

            return listOfResources;
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// retrieves the type of resource for the given <typeref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <returns>The name of <typeref name="T"/></returns>
        internal static string TypeOfResource<T>() where T : Core.Resource
        {
            string typeOfValue = typeof(T).ToString();
            string[] splitValues = typeOfValue.Split(
                new char[] { '.' },
                StringSplitOptions.RemoveEmptyEntries);
            if(null != splitValues)
                return splitValues[splitValues.Length - 1];
            return typeOfValue;

        }

        #endregion

        #region Private member functions

        //private static void LoadRelatedContacts<T>(IEnumerable<T> listOfResources) where T : Core.Resource
        //{
        //    foreach(var resource in listOfResources)
        //    {
        //        ScholarlyWork scholarltWork = resource as ScholarlyWork;

        //        if(null != scholarltWork)
        //        {
        //            if(!scholarltWork.Authors.IsLoaded)
        //            {
        //                scholarltWork.Authors.Load();
        //            }

        //            if(!scholarltWork.Contributors.IsLoaded)
        //            {
        //                scholarltWork.Contributors.Load();
        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// Loads all the dependent collection set for the given resource 
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="resources">List of resource</param>
        private static void LoadResources<T>(IEnumerable<T> resources)
                                                    where T : Core.Resource
        {
            if(null == resources)
            {
                return;
            }

            var scholarlyWorks = resources.OfType<ScholarlyWork>()
                                           .Where(resource => resource != null);

            foreach(ScholarlyWork scholarlyWork in scholarlyWorks)
            {
                if(!scholarlyWork.Authors.IsLoaded)
                {
                    scholarlyWork.Authors.Load();
                }

                if(!scholarlyWork.Contributors.IsLoaded)
                {
                    scholarlyWork.Contributors.Load();
                }

                if(!scholarlyWork.Tags.IsLoaded)
                {
                    scholarlyWork.Tags.Load();
                }

                if(!scholarlyWork.CategoryNodes.IsLoaded)
                {
                    scholarlyWork.CategoryNodes.Load();
                }
            }
        }

        #endregion
    }

    #endregion

    #region CustomCacheRefreshAction class

    /// <summary>
    /// Custom cache refresh action class.
    /// </summary>
    [Serializable]
    internal class CustomCacheRefreshAction : ICacheItemRefreshAction
    {
        /// <summary>
        /// Refresh functionality for refreshing the cache 
        /// </summary>
        /// <param name="key">
        /// The key that is to be refreshed
        /// </param>
        /// <param name="expiredValue">
        /// The old value for the key that is to be refreshed
        /// </param>
        /// <param name="removalReason">
        /// Reason for removal
        /// </param>
        public void Refresh(string key, object expiredValue, CacheItemRemovedReason removalReason)
        {
            // Item has been removed from cache. Perform desired actions here, based upon
            // the removal reason (e.g. refresh the cache with the item).
        }
    }

    #endregion
}
