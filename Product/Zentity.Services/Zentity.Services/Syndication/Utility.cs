// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace Zentity.Services
{
    #region Using namespace

    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel.Syndication;
    using Zentity.Platform;

    #endregion

    #region FeedsOfType enum

    /// <summary>
    /// FeedSortedBy enumeration specifying the type of feed required. 
    /// </summary>
    internal enum FeedsOfType
    {
        /// <summary>
        /// Feeds for Resources to be retrieved by Authors.
        /// </summary>
        Author = 0,

        /// <summary>
        /// Feeds for Resources to be retrieved by Tag.
        /// </summary>
        Tag,

        /// <summary>
        /// Feeds for Resources to be retrieved by Category.
        /// </summary>
        Category,

        /// <summary>
        /// Feeds for Resources to be retrieved by Contributor.
        /// </summary>
        Contributor,

        /// <summary>
        /// Feeds for Resources to be retrieved by DateAdded.
        /// </summary>
        DateAdded,

        /// <summary>
        /// Feeds for Resources to be retrieved by DateModified.
        /// </summary>
        DateModified,
    }

    #endregion

    #region FeedParameterType enum

    /// <summary>
    /// Feed ParameterType enumeration specifying the parameters passed for the feeds to be retrieved. 
    /// </summary>
    internal enum FeedParameterType
    {


        /// <summary>
        /// Id has been parameter for the feeds to be retrieved.
        /// </summary>
        Id,

        /// <summary>
        /// Email has been parameter for the feeds to be retrieved.
        /// </summary>
        Email,

        /// <summary>
        /// Name has been parameter for the feeds to be retrieved.
        /// </summary>
        Name,
        /// <summary>
        /// Date has been parameter for the feeds to be retrieved.
        /// </summary>
        Date,
    }

    #endregion

    #region Utility class

    /// <summary>
    /// Utility class to retrieve Feeds based on the type of Resource. 
    /// </summary>
    internal static class Utility
    {

        /// <summary>
        /// Retrieves the SyndicationFeed object based on the parameters passed.
        /// </summary>
        /// <param name="resourceType">Requested resource type.</param>
        /// <param name="feedsOfType">Resource Type for the Feeds.</param>
        /// <param name="feedParameterType">Parameter type for the feed.</param>
        /// <param name="id">Parameter id for the feed.</param>
        /// <param name="name">Parameter name for the feed.</param>
        /// <param name="lastName">Parameter lastName for the feed.</param>
        /// <param name="email">Parameter email for the feed.</param>
        /// <param name="numberOfFeeds">Parameter noOfFeeds which indicate number of feeds that are to be retrieved.</param>
        /// <param name="date">Parameter date indicates date on which resources have been added or modified</param>
        /// <param name="connectionString">Data storage connection string.</param>
        /// <returns>Syndication Feed object.</returns>
        internal static SyndicationFeed GetSyndicationFeed(
                        string resourceType,
                        FeedsOfType feedsOfType,
                        FeedParameterType feedParameterType,
                        Guid id,
                        string name,
                        string lastName,
                        string email,
                        ushort numberOfFeeds,
                        DateTime date,
                        string connectionString)
        {
            if(string.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            try
            {
                Feed feed = Utility.Feed(connectionString);

                Type resourceOfType = null;
                resourceOfType = Zentity.Platform.Feed.GetResourceType(resourceType);
                if(null == resourceOfType)
                {
                    throw new ArgumentException(Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID, "resourceType");
                }

                SyndicationFeed syndicationFeed = null;
                //make generic call                
                switch(feedsOfType)
                {
                    case FeedsOfType.Author:
                        if(!feed.IsResourceTypeIsOfTypeScholarlyWork(resourceType))
                        {
                            throw new ArgumentException(Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID, "resourceType");
                        }

                        syndicationFeed = Utility.GetSyndicationFeed(
                                          feed,
                                          "GetFeedByAuthor",
                                          resourceOfType,
                                          feedParameterType,
                                          true,
                                          id,
                                          name,
                                          lastName,
                                          email,
                                          numberOfFeeds, DateTime.MinValue);
                        break;
                    case FeedsOfType.Tag:
                        syndicationFeed = Utility.GetSyndicationFeed(
                                          feed,
                                          "GetFeedByTag",
                                          resourceOfType,
                                          feedParameterType,
                                          false,
                                          id,
                                          name,
                                          lastName,
                                          email,
                                          numberOfFeeds, DateTime.MinValue);
                        break;
                    case FeedsOfType.Category:
                        syndicationFeed = Utility.GetSyndicationFeed(
                                         feed,
                                         "GetFeedByCategory",
                                         resourceOfType,
                                         feedParameterType,
                                         false,
                                         id,
                                         name,
                                         lastName,
                                         email,
                                         numberOfFeeds, DateTime.MinValue);
                        break;
                    case FeedsOfType.Contributor:
                        if(!feed.IsResourceTypeIsOfTypeScholarlyWork(resourceType))
                        {
                            throw new ArgumentException(Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID, "resourceType");
                        }

                        syndicationFeed = Utility.GetSyndicationFeed(
                                         feed,
                                         "GetFeedByContributor",
                                         resourceOfType,
                                         feedParameterType,
                                         true,
                                         id,
                                         name,
                                         lastName,
                                         email,
                                         numberOfFeeds, DateTime.MinValue);
                        break;
                    case FeedsOfType.DateAdded:
                        syndicationFeed = Utility.GetSyndicationFeed(
                                        feed,
                                        "GetFeedByDateAdded",
                                        resourceOfType,
                                        feedParameterType,
                                        false,
                                        id,
                                        name,
                                        lastName,
                                        email,
                                        numberOfFeeds, date);
                        break;
                    case FeedsOfType.DateModified:
                        syndicationFeed = Utility.GetSyndicationFeed(
                                       feed,
                                       "GetFeedByDateModified",
                                       resourceOfType,
                                       feedParameterType,
                                       false,
                                       id,
                                       name,
                                       lastName,
                                       email,
                                       numberOfFeeds, date);
                        break;
                }

                return syndicationFeed;
            }
            catch(TargetInvocationException ex)
            {
                if(null != ex.InnerException)
                {
                    throw ex.InnerException;
                }
                throw;
            }
            catch(ArgumentNullException)
            {
                throw;
            }
            catch(ArgumentException)
            {
                throw;
            }
        }

        #region Private helper fucntions

        /// <summary>
        /// Gets the syndication feed.
        /// </summary>
        /// <param name="feed">The feed.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="resourceOfType">Type of resource.</param>
        /// <param name="feedParameterType">Type of the feed parameter.</param>
        /// <param name="isFeedForContact">Value indicating if its a feed for a contact.</param>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="email">The email.</param>
        /// <param name="numberOfFeeds">The number of feeds.</param>
        /// <param name="date">The date.</param>
        /// <returns>The <see cref="SyndicationFeed"/>.</returns>
        private static SyndicationFeed GetSyndicationFeed(
                        Feed feed,
                        string functionName,
                        Type resourceOfType,
                        FeedParameterType feedParameterType,
                        bool isFeedForContact,
                        Guid id,
                        string name,
                        string lastName,
                        string email,
                        ushort numberOfFeeds, DateTime date)
        {
            if(string.IsNullOrEmpty(functionName))
            {
                return null;
            }

            MethodInfo getFeedMethod = null;
            SyndicationFeed syndicationFeed = null;
            List<Type> types = new List<Type>();
            List<object> parameters = new List<object>();
            switch(feedParameterType)
            {

                case FeedParameterType.Id:
                    types = new List<Type> { id.GetType(), numberOfFeeds.GetType() };
                    parameters = new List<object> { id, numberOfFeeds };
                    break;
                case FeedParameterType.Email:
                    types = new List<Type> { email.GetType(), numberOfFeeds.GetType() };
                    parameters = new List<object> { email, numberOfFeeds };
                    break;
                case FeedParameterType.Name:
                    types = Utility.GetParameterTypeList(isFeedForContact, name, lastName, numberOfFeeds);
                    parameters = Utility.GetParameterList(isFeedForContact, name, lastName, numberOfFeeds);
                    break;
                case FeedParameterType.Date:
                    types = new List<Type>() { numberOfFeeds.GetType(), date.GetType() };
                    parameters = new List<object> { numberOfFeeds, date };
                    break;

            }
            //GetFeedByDateAdded

            getFeedMethod = feed.GetType().GetMethod(
                functionName,
                BindingFlags.Public | BindingFlags.Instance,
                null,
                CallingConventions.Any,
                types.ToArray(), null
                );

            if(null != getFeedMethod)
            {
                syndicationFeed = (SyndicationFeed)getFeedMethod.MakeGenericMethod(resourceOfType).Invoke(feed, parameters.ToArray());
            }
            
            return syndicationFeed;
        }

        /// <summary>
        /// Gets the parameter type list.
        /// </summary>
        /// <param name="isFeedForContact">Indicates if the feed is for contact.</param>
        /// <param name="name">The name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="feedCount">The feed count.</param>
        /// <returns>List of <see cref="Type"/>.</returns>
        private static List<Type> GetParameterTypeList(bool isFeedForContact, string name, string lastName, ushort feedCount)
        {
            if(isFeedForContact)
            {
                return new List<Type> { name.GetType(), lastName.GetType(), feedCount.GetType() };
            }
            return new List<Type> { name.GetType(), feedCount.GetType() };
        }

        /// <summary>
        /// Gets the parameter list.
        /// </summary>
        /// <param name="isFeedForContact">Indicates if the feed is for contact.</param>
        /// <param name="name">The name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="feedCount">The feed count.</param>
        /// <returns>List of parameters.</returns>
        private static List<object> GetParameterList(bool isFeedForContact, string name, string lastName, ushort feedCount)
        {
            if(isFeedForContact)
            {
                return new List<object> { name, lastName, feedCount };
            }
            return new List<object> { name, feedCount };
        }

        /// <summary>
        /// Feeds the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="Feed"/> object.</returns>
        private static Feed Feed(string connectionString)
        {
            string configAuthorName = ConfigurationManager.AppSettings["AuthorName"];
            string configCopyright = ConfigurationManager.AppSettings["Copyright"];
            string configAuthorEmail = ConfigurationManager.AppSettings["AuthorEmail"];
            string configMaximumFeedCount = ConfigurationManager.AppSettings["MaximumFeedCount"];
            string configCacheExpirationTime = ConfigurationManager.AppSettings["CacheExpirationTime"];
            string configCachingEnabled = ConfigurationManager.AppSettings["CachingEnabled"];
            bool cachingEnabled = false;
            ushort maximumFeedCount = 20;
            int cacheExpirationTime = 30;
            if(!string.IsNullOrEmpty(configMaximumFeedCount))
            {
                if(!UInt16.TryParse(configMaximumFeedCount, out maximumFeedCount))
                {
                    maximumFeedCount = 20;
                }
            }

            if(!string.IsNullOrEmpty(configCacheExpirationTime))
            {
                if(!Int32.TryParse(configCacheExpirationTime, out cacheExpirationTime))
                {
                    cacheExpirationTime = 30;
                }
            }

            if(!string.IsNullOrEmpty(configCachingEnabled))
            {
                if(!bool.TryParse(configCachingEnabled, out cachingEnabled))
                {
                    cachingEnabled = false;
                }
            }
            if(String.IsNullOrEmpty(configAuthorName))
            {
                configAuthorName = Constants.AuthorName;
            }
            if(String.IsNullOrEmpty(configAuthorEmail))
            {
                configAuthorEmail = Constants.AuthorEmail;
            }
            if(String.IsNullOrEmpty(configCopyright))
            {
                configCopyright = Constants.Copyright;
            }

            return new Feed(maximumFeedCount, configAuthorName, cacheExpirationTime, cachingEnabled,
                configCopyright, configAuthorEmail, connectionString);
        }
        #endregion
    }

    #endregion
}
