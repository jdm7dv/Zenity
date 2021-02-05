// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel.Syndication;
    using Zentity.Core;

    #region Feeds Class

    /// <summary>
    /// The Feed class provides all the API's which can be used by the services.
    /// </summary>
    public class Feed
    {
        string connectionString = String.Empty;

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Feed"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public Feed(string connectionString)
        {
            if(String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", "connectionString");
            }
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Feed"/> class.
        /// </summary>
        /// <param name="maximumFeedCount">The maximum feed count associated with the syndication service.</param>
        /// <param name="authorName">The authorName associated with the syndication service.</param>
        /// <param name="cacheExpirationTime">The cache expiration time associated with the syndication service.</param>
        /// <param name="cachingEnabled">A boolean value indicating if caching is enabled or disabled.</param>
        /// <param name="copyright">The Copyrights message associated with the Syndication Service.</param>
        /// <param name="authorEmail">The Author Email associated with the Syndication Service.</param>
        /// <param name="connectionString">Data storage connection string.</param>
        public Feed(ushort maximumFeedCount, string authorName, int cacheExpirationTime,
            bool cachingEnabled, string copyright, string authorEmail,
            string connectionString)
            : this(connectionString)
        {
            PlatformSettings.MaximumFeedCount = maximumFeedCount;
            PlatformSettings.AuthorName = authorName;
            PlatformSettings.CacheExpirationTime = cacheExpirationTime;
            PlatformSettings.CachingEnabled = cachingEnabled;
            PlatformSettings.AuthorEmail = authorEmail;
            PlatformSettings.Copyrights = copyright;
        }

        #endregion

        #region Public member functions

        #region /author

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given <paramref name="authorId"/>
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.ScholarlyWork"/></typeparam>
        /// <param name="authorId">The associated author id</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        /// <example>
        /// This is a overload for retrieving feeds for author by Id. 
        /// Refer to the documentation of the function of GetFeedByAuthor for email overload for a sample usage of GetFeedByAuthor. 
        /// </example>
        public SyndicationFeed GetFeedByAuthor<T>(
                        Guid authorId, ushort numberOfFeedItems) where T : ScholarlyWorks.ScholarlyWork
        {
            if(null == authorId || Guid.Empty == authorId)
            {
                throw new ArgumentNullException("authorId");
            }

            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(
                                                    CultureInfo.InvariantCulture,
                                                    Properties.Resources.RESOURCE_AUTHOR_ID,
                                                    FeedHelper.TypeOfResource<T>(),
                                                    authorId),
                                       string.Format(
                                                    CultureInfo.InvariantCulture,
                                                    Properties.Resources.RESOURCE_LIST_AUTHOR_ID,
                                                    FeedHelper.TypeOfResource<T>(),
                                                    authorId));

            List<T> listOfResources = FeedHelper.GetResourcesByAuthor<T>(authorId, numberOfFeedItems, connectionString);

            feed.Items = this.ConvertResourceList(listOfResources);

            return feed;
        }

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given <paramref name="email"/> as author
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.ScholarlyWork"/></typeparam>
        /// <param name="email">The associated email id for the author</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid.
        /// </exception>
        /// <example>
        /// This example retrieves a SyndicationFeed object from Zentity repository for the given author email Id.
        /// You will need to add references to System.Data.Entity and Microsoft.Practices.EnterpriseLibrary to successfully compile this sample.
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             //&lt;configuration&gt;
        ///             //      &lt;configSections&gt;
        ///             //        &lt;section name="cachingConfiguration" type="Microsoft.Practices.EnterpriseLibrary.Caching.Configuration.CacheManagerSettings, Microsoft.Practices.EnterpriseLibrary.Caching, Version=3.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" /&gt;
        ///             //      &lt;/configSections&gt;
        ///             //      &lt;cachingConfiguration defaultCacheManager="Cache"&gt;
        ///             //        &lt;cacheManagers&gt;
        ///             //          &lt;add expirationPollFrequencyInSeconds="900" maximumElementsInCacheBeforeScavenging="500"
        ///             //            numberToRemoveWhenScavenging="10" backingStoreName="Null Storage"
        ///             //            name="Cache" /&gt;
        ///             //        &lt;/cacheManagers&gt;
        ///             //        &lt;backingStores&gt;
        ///             //          &lt;add encryptionProviderName="" type="Microsoft.Practices.EnterpriseLibrary.Caching.BackingStoreImplementations.NullBackingStore, Microsoft.Practices.EnterpriseLibrary.Caching, Version=3.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
        ///             //            name="Null Storage" /&gt;
        ///             //        &lt;/backingStores&gt;
        ///             //      &lt;/cachingConfiguration&gt;
        ///             //      &lt;connectionStrings&gt;
        ///             //        &lt;add name="ZentityContext"
        ///             //         connectionString="provider=System.Data.SqlClient;
        ///             //         metadata=res://Zentity.Core;
        ///             //         provider connection string='Data Source=.\sqlexpress;
        ///             //         Initial Catalog=Zentity;Integrated Security=True;'"
        ///             //         providerName="System.Data.EntityClient" /&gt;
        ///             //      &lt;/connectionStrings&gt;
        ///             //      &lt;appSettings&gt;
        ///             //        &lt;add key ="MaximumFeedCount" value="20"/&gt;
        ///             //        &lt;add key="AuthorName" value="Your Name"/&gt;
        ///             //        &lt;add key="CacheExpirationTime" value="30"/&gt;
        ///             //        &lt;add key="CachingEnabled" value="False"/&gt;
        ///             //        &lt;add key="Copyright" value="@Copyright"/&gt;
        ///             //        &lt;add key="AuthorEmail" value="email@domain.com"/&gt;
        ///             //      &lt;/appSettings&gt;
        ///             //      &lt;system.web&gt;
        ///             //        &lt;compilation debug="true" /&gt;
        ///             //      &lt;/system.web&gt;
        ///             //      &lt;system.serviceModel&gt;
        ///             //        &lt;services&gt;
        ///             //          &lt;service name="Zentity.Services.ZentityFeed"&gt;
        ///             //            &lt;endpoint address="http://localhost:8000/" behaviorConfiguration="WebGetBehavior"
        ///             //              binding="webHttpBinding" bindingConfiguration="" name="ZentityServiceEndpoint"
        ///             //              contract="Zentity.Services.IZentityFeed" /&gt;
        ///             //          &lt;/service&gt;
        ///             //        &lt;/services&gt;
        ///             //        &lt;behaviors&gt;
        ///             //          &lt;endpointBehaviors&gt;
        ///             //            &lt;behavior name="WebGetBehavior"&gt;
        ///             //              &lt;webHttp /&gt;
        ///             //            &lt;/behavior&gt;
        ///             //          &lt;/endpointBehaviors&gt;
        ///             //        &lt;/behaviors&gt;
        ///             //      &lt;/system.serviceModel&gt;
        ///             //    &lt;/configuration&gt;
        ///             try
        ///             {
        ///                 string authorEmail = "admin@domain.com";
        ///                 ushort noOfFeeds = 10;
        ///                 Feed feed = new Feed();
        ///                 SyndicationFeed syndicationFeed = feed.GetFeedByAuthor&lt;Publication&gt;(authorEmail, noOfFeeds);
        ///                 if (null != syndicationFeed)
        ///                 {
        ///                     Console.WriteLine("Feeds for Author by Email {0}:\n", authorEmail);
        ///                     foreach (SyndicationItem feedItem in syndicationFeed.Items)
        ///                     {
        ///                         Console.WriteLine("\tFeed Title :{0}", feedItem.Title.Text);
        ///                     }
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("\nNo Feeds exists for the specified Author Email");
        ///                 }
        ///                 Console.ReadLine();
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///             catch (InvalidOperationException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public SyndicationFeed GetFeedByAuthor<T>(string email,
                                    ushort numberOfFeedItems) where T : ScholarlyWorks.ScholarlyWork
        {
            if(string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                         "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(CultureInfo.InvariantCulture,
                                        Properties.Resources.RESOURCE_AUTHOR_EMAIL,
                                        FeedHelper.TypeOfResource<T>(), email),
                                       string.Format(CultureInfo.InvariantCulture,
                                        Properties.Resources.RESOURCE_LIST_AUTHOR_EMAIL,
                                        FeedHelper.TypeOfResource<T>(), email));

            List<T> listOfResources = FeedHelper.GetResourcesByAuthor<T>(email, numberOfFeedItems, connectionString);
            feed.Items = this.ConvertResourceList(listOfResources);

            return feed;
        }

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given author's <paramref name="firstName"/> and <paramref name="lastName"/>
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.ScholarlyWork"/></typeparam>
        /// <param name="firstName">The associated first name for the author</param>
        /// <param name="lastName">The associated last name for the author</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        /// <example>
        /// This is a overload for retrieving feeds for author by Name. 
        /// Refer to the documentation of the function of GetFeedByAuthor for email overload for a sample usage of GetFeedByAuthor. 
        /// </example>
        public SyndicationFeed GetFeedByAuthor<T>(string firstName,
                                    string lastName, ushort numberOfFeedItems)
                                    where T : ScholarlyWorks.ScholarlyWork
        {
            if(string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName");
            }
            if(string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                         "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(CultureInfo.InvariantCulture,
                                        Properties.Resources.RESOURCE_AUTHOR_NAME,
                                         FeedHelper.TypeOfResource<T>(), firstName, lastName),
                                       string.Format(CultureInfo.InvariantCulture,
                                        Properties.Resources.RESOURCE_LIST_AUTHOR_NAME,
                                         FeedHelper.TypeOfResource<T>(), firstName, lastName));

            List<T> listOfResources = FeedHelper.GetResourcesByAuthor<T>(firstName, lastName, numberOfFeedItems, connectionString);
            feed.Items = this.ConvertResourceList(listOfResources);

            return feed;
        }

        #endregion

        #region /tag

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given <paramref name="tagId"/>
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="tagId">The associated tag id</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        /// <example>
        /// This is a overload for retrieving feeds for Tags by Id. 
        /// Refer to the documentation of the function of GetFeedByTag for tagLabel overload for a sample usage of GetFeedByTag. 
        /// </example>
        public SyndicationFeed GetFeedByTag<T>(Guid tagId,
                                        ushort numberOfFeedItems) where T : ScholarlyWorks.ScholarlyWorkItem
        {
            if(null == tagId || Guid.Empty == tagId)
            {
                throw new ArgumentNullException("tagId");
            }
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(CultureInfo.InvariantCulture,
                                                        Properties.Resources.RESOURCE_TAG_ID,
                                                         FeedHelper.TypeOfResource<T>(), tagId),
                                       string.Format(CultureInfo.InvariantCulture,
                                                        Properties.Resources.RESOURCE_LIST_TAG_ID,
                                                         FeedHelper.TypeOfResource<T>(), tagId));

            List<T> listOfResources = FeedHelper.GetResourcesByTag<T>(tagId, numberOfFeedItems, connectionString);

            feed.Items = this.ConvertResourceList(listOfResources);

            return feed;
        }

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given <paramref name="tagLabel"/>
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="tagLabel">The associated tag Label</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        /// <example>
        /// This example retrieves a SyndicationFeed object from Zentity repository for the given tag label.
        /// You will need to add references to System.Data.Entity and Microsoft.Practices.EnterpriseLibrary to successfully compile this sample.
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///                 string tagLabel = "Sci 8";
        ///                 ushort noOfFeeds = 10;
        ///                 Feed feed = new Feed();
        ///                 SyndicationFeed syndicationFeed = feed.GetFeedByTag&lt;Publication&gt;(tagLabel, noOfFeeds);
        ///                 if (null != syndicationFeed)
        ///                 {
        ///                     Console.WriteLine("Feeds for Tag by Name {0}:\n", tagLabel);
        ///                     foreach (SyndicationItem feedItem in syndicationFeed.Items)
        ///                     {
        ///                         Console.WriteLine("\tFeed Title :{0}", feedItem.Title.Text);
        ///                     }
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("\nNo Feeds exists for the specified Tag Name");
        ///                 }
        ///                 Console.ReadLine();
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///             catch (InvalidOperationException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public SyndicationFeed GetFeedByTag<T>(string tagLabel,
                                        ushort numberOfFeedItems) where T : ScholarlyWorks.ScholarlyWorkItem
        {
            if(string.IsNullOrEmpty(tagLabel))
            {
                throw new ArgumentNullException("tagLabel");
            }
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(CultureInfo.InvariantCulture,
                                                        Properties.Resources.RESOURCE_TAG_LABEL,
                                                         FeedHelper.TypeOfResource<T>(), tagLabel),
                                       string.Format(CultureInfo.InvariantCulture,
                                                        Properties.Resources.RESOURCE_LIST_TAG_LABEL,
                                                        FeedHelper.TypeOfResource<T>(), tagLabel));

            List<T> allResources = FeedHelper.GetResourcesByTag<T>(tagLabel, numberOfFeedItems, connectionString);

            feed.Items = this.ConvertResourceList(allResources);
            ;

            return feed;
        }

        #endregion

        #region /category

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given <paramref name="categoryId"/>
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="categoryId">The associated category id</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        public SyndicationFeed GetFeedByCategory<T>(Guid categoryId,
                                        ushort numberOfFeedItems) where T : ScholarlyWorks.ScholarlyWorkItem
        {
            if(null == categoryId || Guid.Empty == categoryId)
            {
                throw new ArgumentNullException("categoryId");
            }
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(CultureInfo.InvariantCulture,
                                                        Properties.Resources.RESOURCE_CATEGORY_ID,
                                                         FeedHelper.TypeOfResource<T>(), categoryId),
                                       string.Format(CultureInfo.InvariantCulture,
                                                        Properties.Resources.RESOURCE_LIST_CATEGORY_ID,
                                                         FeedHelper.TypeOfResource<T>(), categoryId));

            List<T> listOfResources = FeedHelper.GetResourcesByCategory<T>(categoryId, numberOfFeedItems, connectionString);

            feed.Items = this.ConvertResourceList(listOfResources);

            return feed;
        }

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given <paramref name="categoryName"/>
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="categoryName">The associated category name</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        public SyndicationFeed GetFeedByCategory<T>(string categoryName,
                                        ushort numberOfFeedItems) where T : ScholarlyWorks.ScholarlyWorkItem
        {
            if(string.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentNullException("categoryName");
            }
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(CultureInfo.InvariantCulture,
                                                Properties.Resources.RESOURCE_CATEGORY_LABEL,
                                                 FeedHelper.TypeOfResource<T>(), categoryName),
                                       string.Format(CultureInfo.InvariantCulture,
                                                Properties.Resources.RESOURCE_LIST_CATEGORY_LABEL,
                                                 FeedHelper.TypeOfResource<T>(), categoryName));

            List<T> allResources = FeedHelper.GetResourcesByCategory<T>(categoryName, numberOfFeedItems, connectionString);

            feed.Items = this.ConvertResourceList(allResources);

            return feed;
        }

        #endregion

        #region /contributor

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given <paramref name="contributorId"/>
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.ScholarlyWork"/></typeparam>
        /// <param name="contributorId">The associated contributor id</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        /// <example>
        /// This is a overload for retrieving feeds for contributor by Id. 
        /// Refer to the documentation of the function of GetFeedByContributor for name overload for a sample usage of GetFeedByContributor. 
        /// </example>
        public SyndicationFeed GetFeedByContributor<T>(Guid contributorId,
                                        ushort numberOfFeedItems) where T : ScholarlyWorks.ScholarlyWork
        {
            if(null == contributorId || Guid.Empty == contributorId)
            {
                throw new ArgumentNullException("contributorId");
            }
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(CultureInfo.InvariantCulture,
                                            Properties.Resources.RESOURCE_CONTRIBUTOR_ID,
                                             FeedHelper.TypeOfResource<T>(), contributorId),
                                       string.Format(CultureInfo.InvariantCulture,
                                            Properties.Resources.RESOURCE_LIST_CONTRIBUTOR_ID,
                                             FeedHelper.TypeOfResource<T>(), contributorId));

            List<T> listOfResources = FeedHelper.GetResourcesByContributor<T>(contributorId, numberOfFeedItems, connectionString);

            feed.Items = this.ConvertResourceList(listOfResources);

            return feed;
        }

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given <paramref name="email"/> as contributor
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.ScholarlyWork"/></typeparam>
        /// <param name="email">The associated email id for the contributor</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        /// <example>
        /// This is a overload for retrieving feeds for contributor by email Id. 
        /// Refer to the documentation of the function of GetFeedByContributor for name overload for a sample usage of GetFeedByContributor. 
        /// </example>
        public SyndicationFeed GetFeedByContributor<T>(string email, ushort numberOfFeedItems)
                                        where T : ScholarlyWorks.ScholarlyWork
        {
            if(string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                         "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(CultureInfo.InvariantCulture,
                                        Properties.Resources.RESOURCE_CONTRIBUTOR_EMAIL,
                                         FeedHelper.TypeOfResource<T>(), email),
                                       string.Format(CultureInfo.InvariantCulture,
                                        Properties.Resources.RESOURCE_LIST_CONTRIBUTOR_EMAIL,
                                         FeedHelper.TypeOfResource<T>(), email));

            List<T> allResources = FeedHelper.GetResourcesByContributor<T>(email, numberOfFeedItems, connectionString);

            feed.Items = this.ConvertResourceList(allResources);

            return feed;
        }

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources for the given  contributor's <paramref name="firstName"/> and <paramref name="lastName"/> 
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.ScholarlyWork"/></typeparam>
        /// <param name="firstName">The associated first name for the contributor</param>
        /// <param name="lastName">The associated last name for the contributor</param>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        /// <example>
        /// This example retrieves a SyndicationFeed object from Zentity repository for the given contributor name.
        /// You will need to add references to System.Data.Entity and Microsoft.Practices.EnterpriseLibrary to successfully compile this sample.
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///                 string contributorFirstName = "FirstName";
        ///                 string contributorLastName = "LastName";
        ///                 ushort noOfFeeds = 10;
        ///                 Feed feed = new Feed();
        ///                 SyndicationFeed syndicationFeed = feed.GetFeedByContributor&lt;Publication&gt;(contributorFirstName, contributorLastName, noOfFeeds);
        ///                 if (null != syndicationFeed)
        ///                 {
        ///                     Console.WriteLine("Feeds for Contributor by Name {0} {1}:\n",contributorFirstName,contributorLastName);
        ///                     foreach (SyndicationItem feedItem in syndicationFeed.Items)
        ///                     {
        ///                         Console.WriteLine("\tFeed Title :{0}", feedItem.Title.Text);
        ///                     }
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("\nNo Feeds exists for the specified Contributor Name");
        ///                 }
        ///                 Console.ReadLine();
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///             catch (InvalidOperationException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public SyndicationFeed GetFeedByContributor<T>(string firstName,
                                        string lastName, ushort numberOfFeedItems)
                                        where T : ScholarlyWorks.ScholarlyWork
        {
            if(string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName");
            }
            if(string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                         "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                       string.Format(CultureInfo.InvariantCulture,
                                        Properties.Resources.RESOURCE_CONTRIBUTOR_NAME,
                                         FeedHelper.TypeOfResource<T>(), firstName, lastName),
                                       string.Format(CultureInfo.InvariantCulture,
                                        Properties.Resources.RESOURCE_LIST_CONTRIBUTOR_NAME,
                                         FeedHelper.TypeOfResource<T>(), firstName, lastName));

            List<T> allResources = FeedHelper.GetResourcesByContributor<T>(firstName, lastName, numberOfFeedItems, connectionString);

            feed.Items = this.ConvertResourceList(allResources);

            return feed;
        }

        #endregion

        #region /dateAdded and /dateModified

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources which have been recently added
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <param name="dateAdded">The date after which the resources were added.</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        public SyndicationFeed GetFeedByDateAdded<T>(ushort numberOfFeedItems, DateTime dateAdded)
                                        where T : Core.Resource
        {
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                            string.Format(CultureInfo.InvariantCulture,
                                                    Properties.Resources.RESOURCE_ADDED_RECENTLY,
                                                    FeedHelper.TypeOfResource<T>()),
                                            string.Format(CultureInfo.InvariantCulture,
                                                   Properties.Resources.RESOURCE_LIST_ADDED_RECENTLY,
                                                   FeedHelper.TypeOfResource<T>()));

            List<T> listOfResources =
                  FeedHelper.GetResourcesByDate<T>(true, numberOfFeedItems, dateAdded, connectionString);

            feed.Items = this.ConvertResourceList(listOfResources);

            return feed;
        }

        /// <summary>
        /// Returns a <typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object which 
        /// contains the resources which have been recently modified
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/></typeparam>
        /// <param name="numberOfFeedItems">The number of feeds that are to be returned</param>
        /// <param name="dateModified">The date after which the resources were modified.</param>
        /// <returns><typeref name="System.ServiceModel.Syndication.SyndicationFeed"/> object </returns>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid
        /// </exception>
        public SyndicationFeed GetFeedByDateModified<T>(ushort numberOfFeedItems, DateTime dateModified)
                                        where T : Core.Resource
        {
            if(0 >= numberOfFeedItems)
            {
                throw new ArgumentException(Properties.Resources.EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = this.CreateSyndicationFeed(
                                            string.Format(CultureInfo.InvariantCulture,
                                                    Properties.Resources.RESOURCE_MODIFIED_RECENTLY,
                                                    FeedHelper.TypeOfResource<T>()),
                                            string.Format(CultureInfo.InvariantCulture,
                                                   Properties.Resources.RESOURCE_LIST_MODIFIED_RECENTLY,
                                                   FeedHelper.TypeOfResource<T>()));

            List<T> listOfResources =
                  FeedHelper.GetResourcesByDate<T>(false, numberOfFeedItems, dateModified, connectionString);

            feed.Items = this.ConvertResourceList(listOfResources);

            return feed;
        }

        #endregion

        #region helper functions

        /// <summary>
        /// Returns the resource Type for the given string. 
        /// </summary>
        /// <param name="resourceType">The resource Type string for which the type object is to be returned.</param>
        /// <returns>Returns the type of </returns>
        public static Type GetResourceType(string resourceType)
        {
            if(string.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType", Properties.Resources.EXCEPTION_ARGUMENTINVALID);
            }
            return CoreHelper.GetSystemResourceType(resourceType);
        }

        /// <summary>
        /// Checks if the given resource is of type ScholarlyWork.
        /// </summary>
        /// <param name="resourceType">The resource Type string for which the type object is to be checked.</param>
        /// <returns>true if it is of type ScholarlyWork else false.</returns>
        public bool IsResourceTypeIsOfTypeScholarlyWork(string resourceType)
        {
            if(string.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType", Properties.Resources.EXCEPTION_ARGUMENTINVALID);
            }

            ResourceType typeInfo = new CoreHelper(connectionString).GetResourceType(resourceType);
            List<string> hierarchy = new CoreHelper(connectionString).GetResourceTypeHierarchy(typeInfo);
            string typeName = hierarchy.Find(typeOf => typeOf.Equals("ScholarlyWork", StringComparison.OrdinalIgnoreCase));

            if(string.IsNullOrEmpty(typeName))
            {
                return false;
            }

            return true;
        }


        #endregion

        #endregion

        #region Private member functions

        /// <summary>
        /// Creates a SyndicationFeed object
        /// </summary>
        /// <param name="title">The title associated with the feed</param>
        /// <param name="description">The description associated with the feed</param>
        /// <returns>SyndicationFeed object</returns>
        private SyndicationFeed CreateSyndicationFeed(string title, string description)
        {
            SyndicationFeed feed = new SyndicationFeed();

            feed.Title = new TextSyndicationContent(title);
            feed.Description = new TextSyndicationContent(description);
            feed.Generator = Properties.Resources.FEED_GENERATOR;
            feed.Copyright = new TextSyndicationContent(PlatformSettings.Copyrights);
            feed.Authors.Add(new SyndicationPerson(PlatformSettings.AuthorEmail, PlatformSettings.AuthorName, string.Empty));
            feed.Language = System.Globalization.CultureInfo.CurrentUICulture.Name;
            feed.LastUpdatedTime = DateTime.Now;

            return feed;
        }

        /// <summary>
        /// Converts the list of resources to List of Syndication Item.
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeref name="Zentity.Core.Resource"/>.</typeparam>
        /// <param name="resourceList">List of resources.</param>
        /// <returns>Returns a list of Syndication Items.</returns>
        private IEnumerable<SyndicationItem> ConvertResourceList<T>(
                IEnumerable<T> resourceList) where T : Core.Resource
        {
            List<SyndicationItem> items = new List<SyndicationItem>();

            foreach(T resource in resourceList)
            {

                SyndicationItem item = this.CreateSyndicationItem(resource);
                if(null != item)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        /// <summary>
        /// Creates the syndication item.
        /// </summary>
        /// <typeparam name="T">Type of resource.</typeparam>
        /// <param name="resource">The associated resource.</param>
        /// <returns>The SyndicationItem Object.</returns>
        private SyndicationItem CreateSyndicationItem<T>(T resource) where T : Core.Resource
        {
            if(null == resource)
            {
                return null;
            }

            SyndicationItem item = new SyndicationItem();

            //TODO: Would Title be Non Empty always.. If no then what would be the heading for the feed?
            item.Title = new TextSyndicationContent(
                            string.IsNullOrEmpty(resource.Title) ? string.Empty : resource.Title);

            //item.Summary = new TextSyndicationContent(
            //    string.IsNullOrEmpty(resource.Description) ? string.Empty : resource.Description);

            //TODO: How do we set the attributes for fields which are there only in Scholarly Works.
            ScholarlyWorks.ScholarlyWork scholarlyWork = resource as ScholarlyWorks.ScholarlyWork;
            if(null != scholarlyWork)
            {
                item.Copyright = new TextSyndicationContent(
                                string.IsNullOrEmpty(scholarlyWork.Copyright) ? PlatformSettings.Copyrights : scholarlyWork.Copyright);


                // add authors 
                scholarlyWork.Authors.ToList().ForEach(delegate(ScholarlyWorks.Contact contact)
                {
                    ScholarlyWorks.Person person = contact as ScholarlyWorks.Person;
                    if(null != person)
                    {
                        string name = CoreHelper.GetCompleteName(person.FirstName, person.MiddleName, person.LastName);
                        item.Authors.Add(new SyndicationPerson(
                                                string.IsNullOrEmpty(person.Email) ? PlatformSettings.AuthorEmail : person.Email,
                                                name,
                                                string.IsNullOrEmpty(person.Uri) ? null : person.Uri));
                    }
                });

                // add Contributors
                scholarlyWork.Contributors.ToList().ForEach(delegate(ScholarlyWorks.Contact contact)
                {
                    ScholarlyWorks.Person person = contact as ScholarlyWorks.Person;
                    if(null != person)
                    {
                        string name = CoreHelper.GetCompleteName(person.FirstName, person.MiddleName, person.LastName);
                        item.Contributors.Add(new SyndicationPerson(
                                                string.IsNullOrEmpty(person.Email) ? PlatformSettings.AuthorEmail : person.Email,
                                                name,
                                                string.IsNullOrEmpty(person.Uri) ? null : person.Uri));
                    }
                });
                // add tags as categories
                scholarlyWork.Tags.ToList().ForEach(delegate(ScholarlyWorks.Tag tag)
                {
                    item.Categories.Add(new SyndicationCategory(tag.Title));
                });
                scholarlyWork.CategoryNodes.ToList().ForEach(delegate(ScholarlyWorks.CategoryNode category)
                {
                    item.Categories.Add(new SyndicationCategory(category.Title));
                });
            }

            item.Id = resource.Id.ToString();
            if(null != resource.DateAdded &&
                DateTime.MinValue != resource.DateAdded.Value)
            {
                item.PublishDate = new DateTimeOffset(resource.DateAdded.Value);
            }
            if(null != resource.DateModified &&
                    DateTime.MinValue != resource.DateModified.Value)
            {
                item.LastUpdatedTime = new DateTimeOffset(resource.DateModified.Value);
            }

            //TODO: Syndication Links to be implemented here.  

            return item;
        }

        #endregion

    }

    #endregion
}
