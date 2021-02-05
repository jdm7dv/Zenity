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
    using System.Text;

    #endregion

    #region ServiceUris Class

    /// <summary>
    /// Contains the service uri's.
    /// </summary>
    internal sealed class ServiceUris
    {
        public const string GetDocumentation = "help";

        public const string GetFeedByAuthorIdInDefaultFormat = "{resourceType}/authorId/{authorId}/{numberOfFeedItems}";
        public const string GetFeedByAuthorIdAsRss = "{resourceType}/authorId/{authorId}/{numberOfFeedItems}/rss";
        public const string GetFeedByAuthorIdAsAtom = "{resourceType}/authorId/{authorId}/{numberOfFeedItems}/atom";

        public const string GetFeedByAuthorEmailInDefaultFormat = "{resourceType}/authorEmail/{email}/{numberOfFeedItems}";
        public const string GetFeedByAuthorEmailAsRss = "{resourceType}/authorEmail/{email}/{numberOfFeedItems}/rss";
        public const string GetFeedByAuthorEmailAsAtom = "{resourceType}/authorEmail/{email}/{numberOfFeedItems}/atom";

        public const string GetFeedByAuthorNameInDefaultFormat = "{resourceType}/author/{firstName}/{lastName}/{numberOfFeedItems}";
        public const string GetFeedByAuthorNameAsRss = "{resourceType}/author/{firstName}/{lastName}/{numberOfFeedItems}/rss";
        public const string GetFeedByAuthorNameAsAtom = "{resourceType}/author/{firstName}/{lastName}/{numberOfFeedItems}/atom";

        public const string GetFeedByTagIdInDefaultFormat = "{resourceType}/tagId/{tagId}/{numberOfFeedItems}";
        public const string GetFeedByTagIdAsRss = "{resourceType}/tagId/{tagId}/{numberOfFeedItems}/rss";
        public const string GetFeedByTagIdAsAtom = "{resourceType}/tagId/{tagId}/{numberOfFeedItems}/atom";

        public const string GetFeedByTagLabelInDefaultFormat = "{resourceType}/tag/{tagLabel}/{numberOfFeedItems}";
        public const string GetFeedByTagLabelAsRss = "{resourceType}/tag/{tagLabel}/{numberOfFeedItems}/rss";
        public const string GetFeedByTagLabelAsAtom = "{resourceType}/tag/{tagLabel}/{numberOfFeedItems}/atom";

        public const string GetFeedByCategoryIdInDefaultFormat = "{resourceType}/categoryId/{categoryId}/{numberOfFeedItems}";
        public const string GetFeedByCategoryIdAsRss = "{resourceType}/categoryId/{categoryId}/{numberOfFeedItems}/rss";
        public const string GetFeedByCategoryIdAsAtom = "{resourceType}/categoryId/{categoryId}/{numberOfFeedItems}/atom";

        public const string GetFeedByCategoryNameInDefaultFormat = "{resourceType}/category/{categoryName}/{numberOfFeedItems}";
        public const string GetFeedByCategoryNameAsRss = "{resourceType}/category/{categoryName}/{numberOfFeedItems}/rss";
        public const string GetFeedByCategoryNameAsAtom = "{resourceType}/category/{categoryName}/{numberOfFeedItems}/atom";

        public const string GetFeedByContributorIdInDefaultFormat = "{resourceType}/contributorId/{contributorId}/{numberOfFeedItems}";
        public const string GetFeedByContributorIdAsRss = "{resourceType}/contributorId/{contributorId}/{numberOfFeedItems}/rss";
        public const string GetFeedByContributorIdAsAtom = "{resourceType}/contributorId/{contributorId}/{numberOfFeedItems}/atom";

        public const string GetFeedByContributorEmailInDefaultFormat = "{resourceType}/contributorEmail/{email}/{numberOfFeedItems}";
        public const string GetFeedByContributorEmailAsRss = "{resourceType}/contributorEmail/{email}/{numberOfFeedItems}/rss";
        public const string GetFeedByContributorEmailAsAtom = "{resourceType}/contributorEmail/{email}/{numberOfFeedItems}/atom";

        public const string GetFeedByContributorNameInDefaultFormat = "{resourceType}/contributor/{firstName}/{lastName}/{numberOfFeedItems}";
        public const string GetFeedByContributorNameAsRss = "{resourceType}/contributor/{firstName}/{lastName}/{numberOfFeedItems}/rss";
        public const string GetFeedByContributorNameAsAtom = "{resourceType}/contributor/{firstName}/{lastName}/{numberOfFeedItems}/atom";

        public const string GetFeedByDateAddedInDefaultFormat = "{resourceType}/dateAdded/{dateAdded}/{numberOfFeedItems}";
        public const string GetFeedByDateAddedAsRss = "{resourceType}/dateAdded/{dateAdded}/{numberOfFeedItems}/rss";
        public const string GetFeedByDateAddedAsAtom = "{resourceType}/dateAdded/{dateAdded}/{numberOfFeedItems}/atom";

        public const string GetFeedByDateModifiedInDefaultFormat = "{resourceType}/dateModified/{dateModified}/{numberOfFeedItems}";
        public const string GetFeedByDateModifiedAsRss = "{resourceType}/dateModified/{dateModified}/{numberOfFeedItems}/rss";
        public const string GetFeedByDateModifiedAsAtom = "{resourceType}/dateModified/{dateModified}/{numberOfFeedItems}/atom";

        /// <summary>
        /// Prevents a default instance of the <see cref="ServiceUris"/> class from being created.
        /// </summary>
        private ServiceUris()
        {
        }
    }

    #endregion
}
