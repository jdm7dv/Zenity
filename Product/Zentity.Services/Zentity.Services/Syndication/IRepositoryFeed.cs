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
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Syndication;
    using System.ServiceModel.Web;
    using System.Text;
    using System.IO;

    #endregion

    #region IRepositoryFeeds Interface

    /// <summary>
    /// Service contract for the Feed service used to retrieve the Repository feeds
    /// </summary>
    [ServiceContract,
    ServiceKnownType(typeof(Atom10FeedFormatter)),
    ServiceKnownType(typeof(Rss20FeedFormatter))]
    public interface IRepositoryFeed
    {
        #region /help

        /// <summary>
        /// Displays the service documentation
        /// </summary>
        /// <returns>Service Documentation</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetDocumentation)]
        Stream GetDocumentation();

        #endregion

        #region /author {email} or {id} or {firstName}{lastName}

        /// <summary>
        /// Displays the repository feeds by author id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="authorId">The id of the author</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByAuthorIdInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByAuthorIdInDefaultFormat(string resourceType, string authorId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by author id in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="authorId">The id of the author</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByAuthorIdAsRss)]
        Rss20FeedFormatter GetFeedByAuthorIdAsRss(string resourceType, string authorId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by author id in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="authorId">The id of the author</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByAuthorIdAsAtom)]
        Atom10FeedFormatter GetFeedByAuthorIdAsAtom(string resourceType, string authorId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by author email id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="email">The email id of the author</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByAuthorEmailInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByAuthorEmailInDefaultFormat(string resourceType, string email, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by author email id in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="email">The email id of the author</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByAuthorEmailAsRss)]
        Rss20FeedFormatter GetFeedByAuthorEmailAsRss(string resourceType, string email, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by author email id in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="email">The email id of the author</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByAuthorEmailAsAtom)]
        Atom10FeedFormatter GetFeedByAuthorEmailAsAtom(string resourceType, string email, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by author name in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="firstName">The associated first name for the author</param>
        /// <param name="lastName">The associated last name for the author</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByAuthorNameInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByAuthorNameInDefaultFormat(
                                    string resourceType, string firstName, string lastName, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by author name in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="firstName">The associated first name for the author</param>
        /// <param name="lastName">The associated last name for the author</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByAuthorNameAsRss)]
        Rss20FeedFormatter GetFeedByAuthorNameAsRss(
                                    string resourceType, string firstName, string lastName, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by author name in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="firstName">The associated first name for the author</param>
        /// <param name="lastName">The associated last name for the author</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given author</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByAuthorNameAsAtom)]
        Atom10FeedFormatter GetFeedByAuthorNameAsAtom(
                                    string resourceType, string firstName, string lastName, string numberOfFeedItems);

        #endregion

        #region /tag {id} or {tagLabel}

        /// <summary>
        /// Displays the repository feeds by tag id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="tagId">The id of the tag</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag id</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByTagIdInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByTagIdInDefaultFormat(string resourceType, string tagId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by tag id in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="tagId">The id of the tag</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag id</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByTagIdAsRss)]
        Rss20FeedFormatter GetFeedByTagIdAsRss(string resourceType, string tagId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by tag id in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="tagId">The id of the tag</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag id</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByTagIdAsAtom)]
        Atom10FeedFormatter GetFeedByTagIdAsAtom(string resourceType, string tagId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by tag label in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="tagLabel">The associated tag label</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag label</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByTagLabelInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByTagLabelInDefaultFormat(string resourceType, string tagLabel, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by tag label in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="tagLabel">The associated tag label</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag label</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByTagLabelAsRss)]
        Rss20FeedFormatter GetFeedByTagLabelAsRss(string resourceType, string tagLabel, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by tag label in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="tagLabel">The associated tag label</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given tag label</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByTagLabelAsAtom)]
        Atom10FeedFormatter GetFeedByTagLabelAsAtom(string resourceType, string tagLabel, string numberOfFeedItems);

        #endregion

        #region /category {id} or {coategoryName}

        /// <summary>
        /// Displays the repository feeds by category id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="categoryId">The id of the category</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category id</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByCategoryIdInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByCategoryIdInDefaultFormat(string resourceType, string categoryId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by category id in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="categoryId">The id of the category</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category id</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByCategoryIdAsRss)]
        Rss20FeedFormatter GetFeedByCategoryIdAsRss(string resourceType, string categoryId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by category id in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="categoryId">The id of the category</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category id</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByCategoryIdAsAtom)]
        Atom10FeedFormatter GetFeedByCategoryIdAsAtom(string resourceType, string categoryId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by category name in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="categoryName">The associated category name</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category name</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByCategoryNameInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByCategoryNameInDefaultFormat(string resourceType, string categoryName, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by category name in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="categoryName">The associated category name</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category name</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByCategoryNameAsRss)]
        Rss20FeedFormatter GetFeedByCategoryNameAsRss(string resourceType, string categoryName, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by category name in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="categoryName">The associated category name</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given category name</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByCategoryNameAsAtom)]
        Atom10FeedFormatter GetFeedByCategoryNameAsAtom(string resourceType, string categoryName, string numberOfFeedItems);

        #endregion

        #region /contributor {email} or {id} or {firstName}{lastName}

        /// <summary>
        /// Displays the repository feeds by contributor id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="contributorId">The id of the contributor</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByContributorIdInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByContributorIdInDefaultFormat(string resourceType, string contributorId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by contributor id in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="contributorId">The id of the contributor</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByContributorIdAsRss)]
        Rss20FeedFormatter GetFeedByContributorIdAsRss(string resourceType, string contributorId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by contributor id in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="contributorId">The id of the contributor</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByContributorIdAsAtom)]
        Atom10FeedFormatter GetFeedByContributorIdAsAtom(string resourceType, string contributorId, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by contributor email id in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="email">The email id of the contributor</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByContributorEmailInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByContributorEmailInDefaultFormat(string resourceType, string email, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by contributor email id in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="email">The email id of the contributor</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByContributorEmailAsRss)]
        Rss20FeedFormatter GetFeedByContributorEmailAsRss(string resourceType, string email, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by contributor email id in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="email">The email id of the contributor</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByContributorEmailAsAtom)]
        Atom10FeedFormatter GetFeedByContributorEmailAsAtom(string resourceType, string email, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by contributor name in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="firstName">The associated first name for the contributor</param>
        /// <param name="lastName">The associated last name for the contributor</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByContributorNameInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByContributorNameInDefaultFormat(
                                string resourceType, string firstName, string lastName, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by contributor name in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="firstName">The associated first name for the contributor</param>
        /// <param name="lastName">The associated last name for the contributor</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByContributorNameAsRss)]
        Rss20FeedFormatter GetFeedByContributorNameAsRss(
                                string resourceType, string firstName, string lastName, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by contributor name in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="firstName">The associated first name for the contributor</param>
        /// <param name="lastName">The associated last name for the contributor</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources for the given contributor</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByContributorNameAsAtom)]
        Atom10FeedFormatter GetFeedByContributorNameAsAtom(
                                string resourceType, string firstName, string lastName, string numberOfFeedItems);

        #endregion

        #region /dateAdded

        /// <summary>
        /// Displays the repository feeds by sorted by DateAdded attribute in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="dateAdded">The date added associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateAdded attribute</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByDateAddedInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByDateAddedInDefaultFormat(string resourceType, string dateAdded, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by sorted by DateAdded attribute in default form in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="dateAdded">The date added or modified associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateAdded attribute</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByDateAddedAsRss)]
        Rss20FeedFormatter GetFeedByDateAddedAsRss(string resourceType, string dateAdded, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by sorted by DateAdded attribute in default form in Atom format.
        /// </summary>
        /// <param name="resourceType">The associated resource type.</param>
        /// <param name="dateAdded">The date added or modified associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateAdded attribute.</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByDateAddedAsAtom)]
        Atom10FeedFormatter GetFeedByDateAddedAsAtom(string resourceType, string dateAdded, string numberOfFeedItems);


        #endregion

        #region /dateModified

        /// <summary>
        /// Displays the repository feeds by sorted by DateModified attribute in default form, i.e. RSS.
        /// </summary>
        /// <param name="resourceType">The associated resource type</param>
        /// <param name="dateModified">The date modified associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateModified attribute</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByDateModifiedInDefaultFormat)]
        SyndicationFeedFormatter GetFeedByDateModifiedInDefaultFormat(string resourceType, string dateModified, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by sorted by DateModified attribute in default form in RSS format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="dateModified">The date modified associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateModified attribute</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByDateModifiedAsRss)]
        Rss20FeedFormatter GetFeedByDateModifiedAsRss(string resourceType, string dateModified, string numberOfFeedItems);

        /// <summary>
        /// Displays the repository feeds by sorted by DateModified attribute in default form in Atom format
        /// </summary>
        /// <param name="resourceType">The associated resource type</param> 
        /// <param name="dateModified">The date modified associated with the resource</param>
        /// <param name="numberOfFeedItems">The number of feed items that are to be retrieved.</param>
        /// <returns>The Syndication Feed containing resources sorted by DateModified attribute</returns>
        [OperationContract]
        [WebGet(UriTemplate = ServiceUris.GetFeedByDateModifiedAsAtom)]
        Atom10FeedFormatter GetFeedByDateModifiedAsAtom(string resourceType, string dateModified, string numberOfFeedItems);

        #endregion
    }

    #endregion
}
