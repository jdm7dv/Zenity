// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace Zentity.Services
{
    #region Using namespace

    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Syndication;
    using System.ServiceModel.Web;
    using System.Text;

    #endregion

    #region RepositoryFeed

    /// <summary>
    /// Class represents all the feeds that are exposed as service.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RepositoryFeed : IRepositoryFeed
    {
        #region IRepositoryFeed Members

        private string connectionString = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFeed"/> class.
        /// </summary>
        public RepositoryFeed()
            : this(ConfigurationManager.AppSettings["CoreConnectionString"])
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFeed"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public RepositoryFeed(string connectionString)
        {
            this.connectionString = connectionString;
        }
        #region /help

        /// <summary>
        /// Returns the service documentation.
        /// </summary>
        /// <returns>Service Documentation.</returns>
        /// <remarks>
        /// /help.
        /// </remarks>
        public Stream GetDocumentation()
        {
            MemoryStream stream = new MemoryStream();

            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(Properties.Resources.Help);
            writer.Flush();

            stream.Position = 0;

            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            return stream;
        }

        #endregion

        #region /author

        /// <summary>
        /// Displays the repository feeds by author id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="authorId">The id of the author.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author.</returns>
        public SyndicationFeedFormatter GetFeedByAuthorIdInDefaultFormat(string resourceType, string authorId, string numberOfFeedItems)
        {
            return this.GetFeedByAuthorIdAsRss(resourceType, authorId, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by author id in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="authorId">The id of the author.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author.</returns>
        public Rss20FeedFormatter GetFeedByAuthorIdAsRss(string resourceType, string authorId, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(authorId))
            {
                throw new ArgumentNullException("authorId");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            Guid authorGuid = Guid.Empty;
            try
            {
                authorGuid = new Guid(authorId);
            }
            catch(FormatException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "authorId"),
                    ex);
            }
            catch(OverflowException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "authorId"),
                    ex);
            }

            if(Guid.Empty == authorGuid)
            {
                throw new ArgumentNullException("authorId");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Author,
                                        FeedParameterType.Id,
                                        authorGuid,
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        feedCount, DateTime.MinValue,
                                        connectionString);
            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by author id in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="authorId">The id of the author.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author.</returns>
        public Atom10FeedFormatter GetFeedByAuthorIdAsAtom(string resourceType, string authorId, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(authorId))
            {
                throw new ArgumentNullException("authorId");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            Guid authorGuid = Guid.Empty;
            try
            {
                authorGuid = new Guid(authorId);
            }
            catch(FormatException ex)
            {
                throw new ArgumentException(
                            string.Format(
                            CultureInfo.InvariantCulture,
                            Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                            "authorId"),
                            ex);
            }
            catch(OverflowException ex)
            {
                throw new ArgumentException(
                            string.Format(
                            CultureInfo.InvariantCulture,
                            Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                            "authorId"),
                            ex);
            }

            if(Guid.Empty == authorGuid)
            {
                throw new ArgumentNullException("authorId");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Author,
                                        FeedParameterType.Id,
                                        authorGuid,
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        feedCount, DateTime.MinValue,
                                        connectionString);
            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        /// <summary>
        /// Displays the repository feeds by author email id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="email">The email id of the author.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author.</returns>
        public SyndicationFeedFormatter GetFeedByAuthorEmailInDefaultFormat(string resourceType, string email, string numberOfFeedItems)
        {
            return this.GetFeedByAuthorEmailAsRss(resourceType, email, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by author email id in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="email">The email id of the author.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author.</returns>
        public Rss20FeedFormatter GetFeedByAuthorEmailAsRss(string resourceType, string email, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Author,
                                        FeedParameterType.Email,
                                        Guid.Empty,
                                        string.Empty,
                                        string.Empty,
                                        email,
                                        feedCount, DateTime.MinValue,
                                        connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by author email id in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="email">The email id of the author.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author.</returns>
        public Atom10FeedFormatter GetFeedByAuthorEmailAsAtom(string resourceType, string email, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Author,
                                        FeedParameterType.Email,
                                        Guid.Empty,
                                        string.Empty,
                                        string.Empty,
                                        email,
                                        feedCount, DateTime.MinValue,
                                        connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        /// <summary>
        /// Displays the repository feeds by author name in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="firstName">The associated first name for the author.</param>
        /// <param name="lastName">The associated last name for the author.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author.</returns>
        public SyndicationFeedFormatter GetFeedByAuthorNameInDefaultFormat(
                    string resourceType, string firstName, string lastName, string numberOfFeedItems)
        {
            return this.GetFeedByAuthorNameAsRss(resourceType, firstName, lastName, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by author name in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="firstName">The associated first name for the author.</param>
        /// <param name="lastName">The associated last name for the author.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author.</returns>
        public Rss20FeedFormatter GetFeedByAuthorNameAsRss(
                    string resourceType, string firstName, string lastName, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName");
            }

            if(String.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Author,
                                        FeedParameterType.Name,
                                        Guid.Empty,
                                        firstName,
                                        lastName,
                                        string.Empty,
                                        feedCount, DateTime.MinValue,
                                        connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by author name in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="firstName">The associated first name for the author.</param>
        /// <param name="lastName">The associated last name for the author.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author.</returns>
        public Atom10FeedFormatter GetFeedByAuthorNameAsAtom(
                    string resourceType, string firstName, string lastName, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName");
            }

            if(String.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Author,
                                        FeedParameterType.Name,
                                        Guid.Empty,
                                        firstName,
                                        lastName,
                                        string.Empty,
                                        feedCount, DateTime.MinValue,
                                        connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        #endregion

        #region /tag

        /// <summary>
        /// Displays the repository feeds by tag id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="tagId">The id of the tag.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag id.</returns>
        public SyndicationFeedFormatter GetFeedByTagIdInDefaultFormat(string resourceType, string tagId, string numberOfFeedItems)
        {
            return this.GetFeedByTagIdAsRss(resourceType, tagId, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by tag id in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="tagId">The id of the tag.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag id.</returns>
        public Rss20FeedFormatter GetFeedByTagIdAsRss(string resourceType, string tagId, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(tagId))
            {
                throw new ArgumentNullException("tagId");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            Guid tagGuid = Guid.Empty;
            try
            {
                tagGuid = new Guid(tagId);
            }
            catch(FormatException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "tagId"),
                    ex);
            }
            catch(OverflowException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "tagId"),
                    ex);
            }

            if(Guid.Empty == tagGuid)
            {
                throw new ArgumentNullException("tagId");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                         resourceType,
                                         FeedsOfType.Tag,
                                         FeedParameterType.Id,
                                         tagGuid,
                                         string.Empty,
                                         string.Empty,
                                         string.Empty,
                                         feedCount, DateTime.MinValue,
                                         connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by tag id in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="tagId">The id of the tag.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag id.</returns>
        public Atom10FeedFormatter GetFeedByTagIdAsAtom(string resourceType, string tagId, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(tagId))
            {
                throw new ArgumentNullException("tagId");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            Guid tagGuid = Guid.Empty;
            try
            {
                tagGuid = new Guid(tagId);
            }
            catch(FormatException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "tagId"),
                    ex);
            }
            catch(OverflowException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "tagId"),
                    ex);
            }

            if(Guid.Empty == tagGuid)
            {
                throw new ArgumentNullException("tagId");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                     resourceType,
                                     FeedsOfType.Tag,
                                     FeedParameterType.Id,
                                     tagGuid,
                                     string.Empty,
                                     string.Empty,
                                     string.Empty,
                                     feedCount, DateTime.MinValue,
                                     connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        /// <summary>
        /// Displays the repository feeds by tag label in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="tagLabel">The associated tag label.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag label.</returns>
        public SyndicationFeedFormatter GetFeedByTagLabelInDefaultFormat(string resourceType, string tagLabel, string numberOfFeedItems)
        {
            return this.GetFeedByTagLabelAsRss(resourceType, tagLabel, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by tag label in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="tagLabel">The associated tag label.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag label.</returns>
        public Rss20FeedFormatter GetFeedByTagLabelAsRss(string resourceType, string tagLabel, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(tagLabel))
            {
                throw new ArgumentNullException("tagLabel");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                     resourceType,
                                     FeedsOfType.Tag,
                                     FeedParameterType.Name,
                                     Guid.Empty,
                                     tagLabel,
                                     string.Empty,
                                     string.Empty,
                                     feedCount, DateTime.MinValue,
                                     connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by tag label in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="tagLabel">The associated tag label.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag label.</returns>
        public Atom10FeedFormatter GetFeedByTagLabelAsAtom(string resourceType, string tagLabel, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(tagLabel))
            {
                throw new ArgumentNullException("tagLabel");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                 resourceType,
                                 FeedsOfType.Tag,
                                 FeedParameterType.Name,
                                 Guid.Empty,
                                 tagLabel,
                                 string.Empty,
                                 string.Empty,
                                 feedCount, DateTime.MinValue,
                                 connectionString);
            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        #endregion

        #region /category

        /// <summary>
        /// Displays the repository feeds by category id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="categoryId">The id of the category.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category id.</returns>
        public SyndicationFeedFormatter GetFeedByCategoryIdInDefaultFormat(string resourceType, string categoryId, string numberOfFeedItems)
        {
            return this.GetFeedByCategoryIdAsRss(resourceType, categoryId, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by category id in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="categoryId">The id of the category.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category id.</returns>
        public Rss20FeedFormatter GetFeedByCategoryIdAsRss(string resourceType, string categoryId, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(categoryId))
            {
                throw new ArgumentNullException("categoryId");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            Guid categoryGuid = Guid.Empty;
            try
            {
                categoryGuid = new Guid(categoryId);
            }
            catch(FormatException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "categoryId"),
                    ex);
            }
            catch(OverflowException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "categoryId"),
                    ex);
            }

            if(Guid.Empty == categoryGuid)
            {
                throw new ArgumentNullException("categoryId");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Category,
                                        FeedParameterType.Id,
                                        categoryGuid,
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        feedCount, DateTime.MinValue,
                                        connectionString);
            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by category id in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="categoryId">The id of the category.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category id.</returns>
        public Atom10FeedFormatter GetFeedByCategoryIdAsAtom(string resourceType, string categoryId, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(categoryId))
            {
                throw new ArgumentNullException("categoryId");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            Guid categoryGuid = Guid.Empty;
            try
            {
                categoryGuid = new Guid(categoryId);
            }
            catch(FormatException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "categoryId"),
                    ex);
            }
            catch(OverflowException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "categoryId"),
                    ex);
            }

            if(Guid.Empty == categoryGuid)
            {
                throw new ArgumentNullException("categoryId");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                     resourceType,
                                     FeedsOfType.Category,
                                     FeedParameterType.Id,
                                     categoryGuid,
                                     string.Empty,
                                     string.Empty,
                                     string.Empty,
                                     feedCount, DateTime.MinValue,
                                     connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        /// <summary>
        /// Displays the repository feeds by category name in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="categoryName">The associated category name.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category name.</returns>
        public SyndicationFeedFormatter GetFeedByCategoryNameInDefaultFormat(
                        string resourceType, string categoryName, string numberOfFeedItems)
        {
            return this.GetFeedByCategoryNameAsRss(resourceType, categoryName, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by category name in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="categoryName">The associated category name.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category name.</returns>
        public Rss20FeedFormatter GetFeedByCategoryNameAsRss(
                        string resourceType, string categoryName, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentNullException("categoryName");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Category,
                                        FeedParameterType.Name,
                                        Guid.Empty,
                                        categoryName,
                                        string.Empty,
                                        string.Empty,
                                        feedCount, DateTime.MinValue,
                                        connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by category name in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="categoryName">The associated category name.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category name.</returns>
        public Atom10FeedFormatter GetFeedByCategoryNameAsAtom(
                        string resourceType, string categoryName, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentNullException("categoryName");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Category,
                                        FeedParameterType.Name,
                                        Guid.Empty,
                                        categoryName,
                                        string.Empty,
                                        string.Empty,
                                        feedCount, DateTime.MinValue,
                                        connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        #endregion

        #region /contributor

        /// <summary>
        /// Displays the repository feeds by contributor id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="contributorId">The id of the contributor.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor.</returns>
        public SyndicationFeedFormatter GetFeedByContributorIdInDefaultFormat(
                        string resourceType, string contributorId, string numberOfFeedItems)
        {
            return this.GetFeedByContributorIdAsRss(resourceType, contributorId, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by contributor id in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="contributorId">The id of the contributor.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor.</returns>
        public Rss20FeedFormatter GetFeedByContributorIdAsRss(
                        string resourceType, string contributorId, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(contributorId))
            {
                throw new ArgumentNullException("contributorId");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            Guid contributorGuid = Guid.Empty;
            try
            {
                contributorGuid = new Guid(contributorId);
            }
            catch(FormatException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "contributorId"),
                    ex);
            }
            catch(OverflowException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "contributorId"),
                    ex);
            }

            if(Guid.Empty == contributorGuid)
            {
                throw new ArgumentNullException("contributorId");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Contributor,
                                        FeedParameterType.Id,
                                        contributorGuid,
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        feedCount, DateTime.MinValue,
                                        connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by contributor id in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="contributorId">The id of the contributor.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor.</returns>
        public Atom10FeedFormatter GetFeedByContributorIdAsAtom(
                        string resourceType, string contributorId, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(contributorId))
            {
                throw new ArgumentNullException("contributorId");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            Guid contributorGuid = Guid.Empty;
            try
            {
                contributorGuid = new Guid(contributorId);
            }
            catch(FormatException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "contributorId"),
                    ex);
            }
            catch(OverflowException ex)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.SYNDICATION_EXCEPTION_GUIDFORMAT,
                    "contributorId"),
                    ex);
            }

            if(Guid.Empty == contributorGuid)
            {
                throw new ArgumentNullException("contributorId");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                        resourceType,
                                        FeedsOfType.Contributor,
                                        FeedParameterType.Id,
                                        contributorGuid,
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        feedCount, DateTime.MinValue,
                                        connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        /// <summary>
        /// Displays the repository feeds by contributor email id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="email">The email id of the contributor.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor.</returns>
        public SyndicationFeedFormatter GetFeedByContributorEmailInDefaultFormat(
                        string resourceType, string email, string numberOfFeedItems)
        {
            return this.GetFeedByContributorEmailAsRss(resourceType, email, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by contributor email id in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="email">The email id of the contributor.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor.</returns>
        public Rss20FeedFormatter GetFeedByContributorEmailAsRss(
                        string resourceType, string email, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                     resourceType,
                                     FeedsOfType.Contributor,
                                     FeedParameterType.Email,
                                     Guid.Empty,
                                     string.Empty,
                                     string.Empty,
                                     email,
                                     feedCount, DateTime.MinValue,
                                     connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by contributor email id in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="email">The email id of the contributor.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor.</returns>
        public Atom10FeedFormatter GetFeedByContributorEmailAsAtom(
                        string resourceType, string email, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                      resourceType,
                                      FeedsOfType.Contributor,
                                      FeedParameterType.Email,
                                      Guid.Empty,
                                      string.Empty,
                                      string.Empty,
                                      email,
                                      feedCount, DateTime.MinValue,
                                      connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        /// <summary>
        /// Displays the repository feeds by contributor name in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="firstName">The associated first name for the contributor.</param>
        /// <param name="lastName">The associated last name for the contributor.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor.</returns>
        public SyndicationFeedFormatter GetFeedByContributorNameInDefaultFormat(
                    string resourceType, string firstName, string lastName, string numberOfFeedItems)
        {
            return this.GetFeedByContributorNameAsRss(resourceType, firstName, lastName, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by contributor name in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="firstName">The associated first name for the contributor.</param>
        /// <param name="lastName">The associated last name for the contributor.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor.</returns>
        public Rss20FeedFormatter GetFeedByContributorNameAsRss(
                    string resourceType, string firstName, string lastName, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName");
            }

            if(String.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                      resourceType,
                                      FeedsOfType.Contributor,
                                      FeedParameterType.Name,
                                      Guid.Empty,
                                      firstName,
                                      lastName,
                                      string.Empty,
                                      feedCount, DateTime.MinValue,
                                      connectionString);
            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by contributor name in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="firstName">The associated first name for the contributor.</param>
        /// <param name="lastName">The associated last name for the contributor.</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor.</returns>
        public Atom10FeedFormatter GetFeedByContributorNameAsAtom(
                        string resourceType, string firstName, string lastName, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName");
            }

            if(String.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }

            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                      resourceType,
                                      FeedsOfType.Contributor,
                                      FeedParameterType.Name,
                                      Guid.Empty,
                                      firstName,
                                      lastName,
                                      string.Empty,
                                      feedCount, DateTime.MinValue,
                                      connectionString);
            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        #endregion

        #region /dateAdded

        /// <summary>
        /// Displays the repository feeds by sorted by DateAdded attribute in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="dateAdded">The date added  associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateAdded attribute.</returns>
        public SyndicationFeedFormatter GetFeedByDateAddedInDefaultFormat(string resourceType, string dateAdded, string numberOfFeedItems)
        {
            return this.GetFeedByDateAddedAsRss(resourceType, dateAdded, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by sorted by DateAdded attribute in default form in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="dateAdded">The date added associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateAdded attribute.</returns>
        public Rss20FeedFormatter GetFeedByDateAddedAsRss(string resourceType, string dateAdded, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }
            DateTime dateValue;
            if(!DateTime.TryParse(dateAdded, out dateValue))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "dateAdded");

            }


            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                      resourceType,
                                      FeedsOfType.DateAdded,
                                      FeedParameterType.Date,
                                      Guid.Empty,
                                      string.Empty,
                                      string.Empty,
                                      string.Empty,
                                      feedCount, dateValue,
                                      connectionString);
            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by sorted by DateAdded attribute in default form in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="dateAdded">The date added associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateAdded attribute.</returns>
        public Atom10FeedFormatter GetFeedByDateAddedAsAtom(string resourceType, string dateAdded, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }

            DateTime dateVal;
            if(!DateTime.TryParse(dateAdded, out dateVal))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "dateAdded");

            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                      resourceType,
                                      FeedsOfType.DateAdded,
                                      FeedParameterType.Date,
                                      Guid.Empty,
                                      string.Empty,
                                      string.Empty,
                                      string.Empty,
                                      feedCount, dateVal,
                                      connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        #endregion

        #region /dateModified

        /// <summary>
        /// Displays the repository feeds by sorted by DateModified attribute in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="dateModified">The date modified associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateModified attribute.</returns>
        public SyndicationFeedFormatter GetFeedByDateModifiedInDefaultFormat(string resourceType, string dateModified, string numberOfFeedItems)
        {
            return this.GetFeedByDateModifiedAsRss(resourceType, dateModified, numberOfFeedItems);
        }

        /// <summary>
        /// Displays the repository feeds by sorted by DateModified attribute in default form in RSS format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="dateModified">The date modified associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateModified attribute.</returns>
        public Rss20FeedFormatter GetFeedByDateModifiedAsRss(string resourceType, string dateModified, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }
            DateTime dateVal;
            if(!DateTime.TryParse(dateModified, out dateVal))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "dateModified");

            }

            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                      resourceType,
                                      FeedsOfType.DateModified,
                                      FeedParameterType.Date,
                                      Guid.Empty,
                                      string.Empty,
                                      string.Empty,
                                      string.Empty,
                                      feedCount, dateVal,
                                      connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetRss20Formatter(false);
        }

        /// <summary>
        /// Displays the repository feeds by sorted by DateModified attribute in default form in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param> 
        /// <param name="dateModified">The date modified associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateModified attribute.</returns>
        public Atom10FeedFormatter GetFeedByDateModifiedAsAtom(string resourceType, string dateModified, string numberOfFeedItems)
        {
            if(String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            ushort feedCount = 0;
            if(!ushort.TryParse(numberOfFeedItems, out feedCount))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "numberOfFeedItems");
            }
            DateTime dateVal;
            if(!DateTime.TryParse(dateModified, out dateVal))
            {
                throw new ArgumentException(
                    Properties.Resources.SYNDICATION_EXCEPTION_ARGUMENTINVALID,
                    "dateModified");

            }


            SyndicationFeed feed = Utility.GetSyndicationFeed(
                                      resourceType,
                                      FeedsOfType.DateModified,
                                      FeedParameterType.Date,
                                      Guid.Empty,
                                      string.Empty,
                                      string.Empty,
                                      string.Empty,
                                      feedCount, dateVal,
                                      connectionString);

            if(null == feed)
            {
                return null;
            }

            return feed.GetAtom10Formatter();
        }

        #endregion

        #endregion
    }

    #endregion
}
