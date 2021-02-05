// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Configuration;
    using System.Globalization;
    using System.Xml.Linq;

    /// <summary>
    /// AtomPub constants.
    /// </summary>
    internal static class AtomPubConstants
    {
        #region Queries

        // Query to get resources of specified type and specified count.
        // Here {0} = Resource Type, {1} = skip value, {2} = count to take
        internal const string EsqlToGetResources = @"Select Value resource From oftype(ZentityContext.Resources, 
                                                     only {0})
                                                     as resource order by resource.DateModified DESC 
                                                     SKIP @SkipCount LIMIT @LimitCount";

        internal const string EsqlToGetAllResources = @"Select Value resource From oftype(ZentityContext.Resources, 
                                                     only {0}) as resource";

        // Query to get resources of specified type and specified Id.
        // Here {0} = Resource Type, {1} = resource Id
        internal const string EsqlToGetResourceById = @"Select Value resource From oftype(ZentityContext.Resources, 
                                                        only {0})
                                                        as resource WHERE resource.Id = @Id";
        // Here {0} = Resource Type
        internal const string EsqlToGetFileContents = @"Select Value F from oftype(ZentityContext.Resources, 
                                                        only {0}) as R, R.Files as F  
                                                        WHERE R.Id = @Id";

        internal const string EsqlToGetResourceCount = @"SELECT 1 FROM OFTYPE(ZentityContext.Resources, ONLY {0}) AS Resource";

        //internal readonly static Func<ZentityContext, Guid, DateTime?> GetResourceDateModified = CompiledQuery.Compile((ZentityContext context, Guid id) =>
        //                                                                                         context.ScholarlyWorks()
        //                                                                                                .Where(resource => resource.Id == id)
        //                                                                                                .Select(resource => resource.DateModified)
        //                                                                                                .FirstOrDefault());

        #endregion

        #region Store related constants

        internal const string CollectionTypesCacheKey = "ZentityCollectionTypes";

        /// <summary>
        /// Gets the core DB connection string.
        /// </summary>
        /// <value>The core DB connection string.</value>
        internal static string CoreDBConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["SqlConnectionString"];
            }
        }

        internal const string KeyDateModified = "DateModified";
        internal const string GuidFormat = @"^[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}$";
        internal const string FirstName = "FirstName";
        internal const string MiddleName = "MiddleName";
        internal const string LastName = "LastName";

        /// <summary>
        /// Gets the person name pattern.
        /// </summary>
        /// <value>The person name pattern.</value>
        internal static string PersonNamePattern
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                                      @"^\s*(?<{0}>\S+)(?>(?:\s+(?<{1}>\S(?:.*\S)?))?(?:\s+(?<{2}>\S+)))?\s*$",
                                      FirstName, MiddleName, LastName);
            }
        }

        #endregion

        #region Service Document constants

        internal const string DefaultWorkSpaceTitle = "Default Repository Workspace";
        internal const string CollectionTitle = @"{0} Collection";
        internal const string AcceptAll = "*/*";

        #endregion

        #region Atom Feed Constants

        internal const string FeedCollectionName = "<atom:title>{0} Collection</atom:title>";

        internal const string First = "first";
        internal const string Previous = "previous";
        internal const string Next = "next";
        internal const string Last = "last";

        internal const int FirstPageValue = 1;

        /// <summary>
        /// Gets the size of the feed page.
        /// </summary>
        /// <value>The size of the feed page.</value>
        internal static string FeedPageSize
        {
            get
            {
                return ConfigurationManager.AppSettings["FeedPageSize"];
            }
        }

        #endregion

        internal const string IdPrefix = "urn:guid:";
        internal const string EditMedia = "edit-media";
        internal const string Edit = "edit";
        internal const string Related = "related";
        internal const string DefaultMimeType = "application/binary";
        internal const string DefaultFileExtension = "txt";

        internal const string KeyIfMatch = "If-Match";
        internal const string KeyIfNoneMatch = "If-None-Match";
        internal const string KeyIfModifiedSince = "If-Modified-Since";
        internal const string KeyIfUnmodifiedSince = "If-Unmodified-Since";
        internal const string KeyETag = "ETag";
        internal const string KeyLocation = "Location";

        internal const string AtomEntryContentType = "application/atom+xml;type=entry";
        internal const string AtomFeedContentType = "application/atom+xml;type=feed";
        internal const string ServiceDocumentContentType = "application/atomsvc+xml";

        private static XNamespace atomEntryNamespace = "http://www.w3.org/2005/Atom";

        /// <summary>
        /// Gets the atom entry namespace.
        /// </summary>
        /// <value>The atom entry namespace.</value>
        internal static XNamespace AtomEntryNamespace
        {
            get
            {
                return AtomPubConstants.atomEntryNamespace;
            }
        }
        internal const string AtomLink = "link";
        internal const string AtomRelative = "rel";
        internal const string AtomSource = "source";

        internal const string ExtensionPropertyBaseUri = "urn:zentity/module/zentity-platform/atompub_entry/";
        internal const string LinksProperty = "Link";
        internal const string SourceProperty = "Source";
        internal const string SummaryProperty = "Summary";
        internal const string ContentUrlProperty = "ContentUrl";
        internal const string DescriptionTypeProperty = "DescriptionType";
        internal const string TitleTypeProperty = "TitleType";
        internal const string SummaryTypeProperty = "SummmaryType";
        internal const string CopyrightTypeProperty = "CopyrightType";
    }
}
