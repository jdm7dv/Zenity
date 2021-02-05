// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data.Objects;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Web;
    using System.Xml;
    using Zentity.Core;
    using Zentity.Platform.Properties;

    /// <summary>
    /// This class is responsible for handling AtomPub-GET request.
    /// </summary>
    public class AtomPubGetProcessor : HttpGetProcessor
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomPubGetProcessor"/> class.
        /// </summary>
        public AtomPubGetProcessor()
            : this(AtomPubHelper.GetBaseUri())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomPubGetProcessor"/> class.
        /// </summary>
        /// <param name="baseAddress">Base Uri to process the HTTP request.</param>
        internal AtomPubGetProcessor(string baseAddress)
            : base(baseAddress)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Validates the request uri and returns the appropriate error message. 
        /// If the request is valid, 'error' with empty string is returned.
        /// Following requests are considered as valid :
        /// 1. Service Document Uri
        /// 2. Collection Uri
        /// 3. Collection Uri for next / previous page
        /// 4. Member resource Uri
        /// 5. Media Entry resource Uri
        /// </summary>
        /// <param name="context">HttpContext containing the request object.</param>
        /// <param name="errorMessage">Contains the error message.</param>
        /// <returns>True if the request is valid, else false.</returns>
        public override bool ValidateRequest(HttpContext context, out string errorMessage)
        {
            errorMessage = string.Empty;
            AtomPubRequestType requestType = AtomPubHelper.GetAtomPubRequestType(context, base.BaseUri);

            if(AtomPubRequestType.Unknwon == requestType)
            {
                errorMessage = Resources.ATOMPUB_INVALID_URL;
                return false;
            }

            // If ServiceDocument uri then no need of further validations.
            if(AtomPubRequestType.ServiceDocument == requestType)
            {
                return true;
            }

            // Get collection name
            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                  AtomPubParameterType.CollectionName);

            bool isValid = false;

            // If valid collection name, then proceed.
            if(!string.IsNullOrEmpty(collectionName)
                 && AtomPubHelper.IsValidCollectionType(collectionName))
            {
                switch(requestType)
                {
                    case AtomPubRequestType.Collection:
                        isValid = true;
                        break;
                    case AtomPubRequestType.CollectionWithPageNo:
                        string strPageNo = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                         AtomPubParameterType.PageNo);
                        long pageNumber = 0;

                        if(!long.TryParse(strPageNo, out pageNumber))
                        {
                            errorMessage = Resources.ATOMPUB_INVALID_PAGE_NUMBER;
                        }
                        else
                        {
                            isValid = true;
                        }

                        break;
                    case AtomPubRequestType.EditMember:
                    case AtomPubRequestType.EditMedia:
                        // Get the requested id for verify it is GUID
                        string memberId = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                        AtomPubParameterType.Id);
                        if(AtomPubHelper.IsValidGuid(memberId))
                        {
                            isValid = true;
                        }
                        else
                        {
                            errorMessage = Resources.ATOMPUB_INVALID_RESOURCE_ID;
                        }

                        break;
                    default:
                        isValid = false;
                        errorMessage = Resources.ATOMPUB_INVALID_URL;
                        break;
                }
            }
            else
            {
                errorMessage = Resources.ATOMPUB_UNSUPPORTED_COLLECTION_NAME;
            }

            return isValid;
        }

        /// <summary>
        /// Handles the GET request send to the following uri:
        /// 1. Service Document Uri
        /// 2. Collection Uri
        /// 3. Collection Uri for next / previous page
        /// 4. Member resource Uri
        /// 5. Media Entry resource Uri
        /// The method assumes that the request is already validated using ValidateRequest method.
        /// </summary>
        /// <param name="context">HttpContext containing the request object.</param>
        /// <param name="statusCode">returns the status of the request.</param>
        /// <returns>A string containing the response for the specified AtomPub request.</returns>
        public override string ProcessRequest(HttpContext context, out System.Net.HttpStatusCode statusCode)
        {
            string response = string.Empty;

            AtomPubRequestType requestType = AtomPubHelper.GetAtomPubRequestType(context, base.BaseUri);

            // Get collection name
            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                  AtomPubParameterType.CollectionName);
            statusCode = HttpStatusCode.OK;

            // This assumes that the collection name is verified by ValidateRequest method.
            switch(requestType)
            {
                case AtomPubRequestType.ServiceDocument:
                    response = this.GetServiceDocument(context);
                    context.Response.ContentType = AtomPubConstants.ServiceDocumentContentType;
                    break;
                case AtomPubRequestType.Collection:
                    response = this.GetAtomFeed(collectionName);
                    break;
                case AtomPubRequestType.CollectionWithPageNo:
                    string strPageNo = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                             AtomPubParameterType.PageNo);
                    long pageNumber = long.Parse(strPageNo, CultureInfo.InvariantCulture);
                    response = this.GetAtomFeed(collectionName, pageNumber);
                    context.Response.ContentType = AtomPubConstants.AtomFeedContentType;
                    break;
                case AtomPubRequestType.EditMember:
                    {
                        // Get the requested member type and its id for verifying it existents.                        
                        string memberId = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                        AtomPubParameterType.Id);

                        try
                        {
                            if(AtomPubHelper.ValidatePrecondition(context, base.BaseUri, out statusCode))
                            {
                                string eTag = AtomPubHelper.CalculateETag(memberId);
                                context.Response.AddHeader(AtomPubConstants.KeyETag, eTag);
                                response = this.GetAtomEntryDocument(collectionName, memberId).AtomEntry;
                                context.Response.ContentType = AtomPubConstants.AtomEntryContentType;
                                statusCode = HttpStatusCode.OK;
                            }
                            else
                            {
                                if(HttpStatusCode.PreconditionFailed == statusCode)
                                {
                                    response = Properties.Resources.ATOMPUB_PRECONDITION_FAILED;
                                }
                                else
                                {
                                    response = Properties.Resources.ATOMPUB_RESOURCE_NOT_MODIFIED;
                                }
                            }
                        }
                        catch(ResourceNotFoundException)
                        {
                            statusCode = HttpStatusCode.NotFound;
                            response = Properties.Resources.ATOMPUB_RESOURCE_NOT_FOUND;
                        }
                    }
                    break;
                case AtomPubRequestType.EditMedia:
                    {
                        // Get the requested member type and its id for verifying it existents.                        
                        string memberId = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                       AtomPubParameterType.Id);
                        try
                        {
                            if(AtomPubHelper.ValidatePrecondition(context, base.BaseUri, out statusCode))
                            {
                                //context.Response.Clear();
                                string eTag = AtomPubHelper.CalculateETag(memberId);
                                context.Response.AddHeader(AtomPubConstants.KeyETag, eTag);

                                byte[] content = null;

                                // Write the media contents on stream.
                                this.GetMediaResource(collectionName, memberId, out content);
                                context.Response.OutputStream.Write(content, 0, content.Length);

                                string fileExtension = string.Empty;

                                context.Response.ContentType = GetMediaContentType(collectionName, memberId, out fileExtension);

                                string attachedFileName = string.Format(CultureInfo.InvariantCulture, "attachment; filename={0}.{1}", memberId, fileExtension);

                                context.Response.AddHeader("Content-Disposition", attachedFileName);
                                context.Response.AddHeader("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
                                statusCode = HttpStatusCode.OK;
                            }
                            else
                            {
                                if(HttpStatusCode.PreconditionFailed == statusCode)
                                {
                                    response = Properties.Resources.ATOMPUB_PRECONDITION_FAILED;
                                }
                                else
                                {
                                    response = Properties.Resources.ATOMPUB_RESOURCE_NOT_MODIFIED;
                                }
                            }
                        }
                        catch(ResourceNotFoundException)
                        {
                            statusCode = HttpStatusCode.NotFound;
                            response = Properties.Resources.ATOMPUB_RESOURCE_NOT_FOUND;
                        }
                    }
                    break;
                case AtomPubRequestType.Unknwon:
                default:
                    statusCode = HttpStatusCode.BadRequest;
                    response = Properties.Resources.ATOMPUB_BAD_REQUEST;
                    break;
            }

            return response;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets service document in xml string format.
        /// </summary>
        /// <param name="serviceDocument">service document to be convert.</param>
        /// <returns>Service document xml string</returns>
        protected static string GetServiceDocument(ServiceDocument serviceDocument)
        {
            XmlWriter writer = null;
            MemoryStream serviceDocStream = new MemoryStream();
            StreamReader reader = null;

            try
            {
                writer = XmlWriter.Create(serviceDocStream);
                serviceDocument.Save(writer);

                serviceDocStream.Seek(0, SeekOrigin.Begin);
                reader = new StreamReader(serviceDocStream);

                return reader.ReadToEnd();
            }
            finally
            {
                if(null != writer)
                {
                    writer.Close();
                }

                if(null != reader)
                {
                    reader.Close();
                }

                if(null != serviceDocStream)
                {
                    serviceDocStream.Close();
                }
            }
        }

        /// <summary>
        /// Gets the AtomPub Service Document for this instance of the AtomPub Service.
        /// </summary>        
        /// <param name="context">HttpContext containing the request object.</param>
        /// <param name="baseUri">Base Uri for the specified request.</param>
        /// <returns>A string that contains the AtomPub Service Document.</returns>
        protected ServiceDocument GetServiceDocument(HttpContext context, Uri baseUri)
        {
            string[] collectionNames;

            // Get the list of collection names
            if(null != context.Cache[AtomPubConstants.CollectionTypesCacheKey])
            {
                collectionNames = (string[])context.Cache[AtomPubConstants.CollectionTypesCacheKey];
            }
            else
            {
                IAtomPubStoreReader storeReader = AtomPubStoreFactory.GetAtomPubStoreReader(base.BaseUri.OriginalString);
                collectionNames = storeReader.GetCollectionNames();

                // Put the collection names in cache
                context.Cache.Insert(AtomPubConstants.CollectionTypesCacheKey,
                                      collectionNames,
                                      null,
                                      DateTime.UtcNow.AddMinutes(20),
                                      System.Web.Caching.Cache.NoSlidingExpiration);
            }

            ServiceDocument serviceDocument = new ServiceDocument();

            List<ResourceCollectionInfo> collections = new List<ResourceCollectionInfo>();
            foreach(string collectionType in collectionNames)
            {
                // Create collection Url based on resource type
                Uri collectionUri = AtomPubHelper.AtomPubTemplates[AtomPubRequestType.Collection]
                                                    .BindByPosition(baseUri,
                                                                    new string[] { collectionType });

                // Create collection node and append to the list.                
                string collectionTitle = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.CollectionTitle, collectionType);

                ResourceCollectionInfo collection = new ResourceCollectionInfo(collectionTitle, collectionUri);

                collection.Accepts.Add(AtomPubConstants.AtomEntryContentType);
                collection.Accepts.Add(AtomPubConstants.AcceptAll);

                collections.Add(collection);
            }

            Workspace defaultWorkspace = new Workspace(AtomPubConstants.DefaultWorkSpaceTitle, collections);
            serviceDocument.Workspaces.Add(defaultWorkspace);

            return serviceDocument;
        }

        /// <summary>
        /// Gets the AtomPub Service Document for this instance of the AtomPub Service.
        /// </summary>        
        /// <param name="context">HttpContext containing the request object.</param>
        /// <returns>A string that contains the AtomPub Service Document.</returns>
        private string GetServiceDocument(HttpContext context)
        {
            ServiceDocument serviceDocument = this.GetServiceDocument(context, base.BaseUri);
            return GetServiceDocument(serviceDocument);
        }

        /// <summary>
        /// Gets the Atom Feed for the specified collection.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <returns>A string that contains the Atom Feed.</returns>
        private string GetAtomFeed(string collectionName)
        {
            string feedDocument = string.Empty;

            // Get the page number if specified.
            int pageNumber = AtomPubConstants.FirstPageValue; // Default value should be 1

            feedDocument = GetAtomFeed(collectionName, pageNumber);

            return feedDocument;
        }

        /// <summary>
        /// Gets the Atom Feed for the specified collection.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="pageNumber">A long value indicating the current page of the Feed Document.</param>
        /// <returns>A string that contains the Atom Feed.</returns>
        private string GetAtomFeed(string collectionName, long pageNumber)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(AtomPubConstants.FirstPageValue > pageNumber)
            {
                throw new ArgumentNullException("pageNumber");
            }

            IAtomPubStoreReader storeReader = AtomPubStoreFactory.GetAtomPubStoreReader(base.BaseUri.OriginalString);

            // Count the number of resources
            long memberCount = storeReader.GetMembersCount(collectionName);

            // Count the number of pages.
            int pageSize = int.Parse(AtomPubConstants.FeedPageSize, CultureInfo.InvariantCulture);
            long numberOfPages = AtomPubConstants.FirstPageValue;

            if(0 < memberCount)
            {
                numberOfPages = memberCount / pageSize;
                if(0 != (memberCount % pageSize))
                {
                    numberOfPages++; // Increment pages if its not a perfect division.
                }

                // Find effective numberOfPages
                numberOfPages = AtomPubConstants.FirstPageValue + (numberOfPages - 1);

                // Check if page number is valid
                if(pageNumber > numberOfPages)
                {
                    return Properties.Resources.ATOMPUB_INVALID_PAGE_NUMBER;
                }
            }

            // Get the Members
            long skip = (pageNumber - 1) * pageSize;
            SyndicationFeed feed = storeReader.GetMembers(collectionName, skip, pageSize);

            #region Add First, Next, Prev and Last links

            // If number of pages are greater than FirstPageValue, then display page links.
            if(AtomPubConstants.FirstPageValue != numberOfPages)
            {
                // For second page, links will be as follows :
                // <link rel="first"
                //      href="{BaseUri}?Page=1" />
                // <link rel="previous"
                //      href="{BaseUri}?Page=1" />
                // <link rel="next"
                //      href="{BaseUri}?Page=3" />
                // <link rel="last"
                //      href="{BaseUri}?Page=10" />                

                #region First

                NameValueCollection parameters = new NameValueCollection();
                parameters.Add(AtomPubParameterType.CollectionName.ToString(), collectionName);
                parameters.Add(AtomPubParameterType.PageNo.ToString(), AtomPubConstants.FirstPageValue.ToString(CultureInfo.InvariantCulture));

                Uri firstPageUri = AtomPubHelper.AtomPubTemplates[AtomPubRequestType.CollectionWithPageNo]
                                                .BindByName(base.BaseUri, parameters);

                SyndicationLink firstPageLink = new SyndicationLink();
                firstPageLink.RelationshipType = AtomPubConstants.First;
                firstPageLink.Uri = firstPageUri;

                feed.Links.Add(firstPageLink);

                #endregion

                #region Previous

                if(AtomPubConstants.FirstPageValue != pageNumber)
                {
                    parameters = new NameValueCollection();
                    parameters.Add(AtomPubParameterType.CollectionName.ToString(), collectionName);
                    parameters.Add(AtomPubParameterType.PageNo.ToString(),
                                   (pageNumber - 1).ToString(CultureInfo.InstalledUICulture));

                    Uri prevPageUri = AtomPubHelper.AtomPubTemplates[AtomPubRequestType.CollectionWithPageNo]
                                                    .BindByName(base.BaseUri, parameters);

                    SyndicationLink prevPageLink = new SyndicationLink();
                    prevPageLink.RelationshipType = AtomPubConstants.Previous;
                    prevPageLink.Uri = prevPageUri;

                    feed.Links.Add(prevPageLink);
                }

                #endregion

                #region Next

                if(numberOfPages != pageNumber)
                {
                    parameters = new NameValueCollection();
                    parameters.Add(AtomPubParameterType.CollectionName.ToString(), collectionName);
                    parameters.Add(AtomPubParameterType.PageNo.ToString(),
                                   (pageNumber + 1).ToString(CultureInfo.InstalledUICulture));

                    Uri nextPageUri = AtomPubHelper.AtomPubTemplates[AtomPubRequestType.CollectionWithPageNo]
                                                    .BindByName(base.BaseUri, parameters);

                    SyndicationLink nextPageLink = new SyndicationLink();
                    nextPageLink.RelationshipType = AtomPubConstants.Next;
                    nextPageLink.Uri = nextPageUri;

                    feed.Links.Add(nextPageLink);
                }

                #endregion

                #region Last
                {
                    parameters = new NameValueCollection();
                    parameters.Add(AtomPubParameterType.CollectionName.ToString(), collectionName);
                    parameters.Add(AtomPubParameterType.PageNo.ToString(),
                                   (numberOfPages).ToString(CultureInfo.InstalledUICulture));

                    Uri lastPageUri = AtomPubHelper.AtomPubTemplates[AtomPubRequestType.CollectionWithPageNo]
                                                    .BindByName(base.BaseUri, parameters);

                    SyndicationLink lastPageLink = new SyndicationLink();
                    lastPageLink.RelationshipType = AtomPubConstants.Last;
                    lastPageLink.Uri = lastPageUri;

                    feed.Links.Add(lastPageLink);
                }
                #endregion
            }
            #endregion

            // Generate Feed document from Syndication Feed.
            return GetFeedDocument(feed);
        }

        /// <summary>
        /// Gets the feed document.
        /// </summary>
        /// <param name="feed">The feed.</param>
        /// <returns>The feed document data.</returns>
        private static string GetFeedDocument(SyndicationFeed feed)
        {
            XmlWriter writer = null;
            StringBuilder feedData = new StringBuilder();
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.ConformanceLevel = ConformanceLevel.Auto;
                settings.OmitXmlDeclaration = false;

                writer = XmlWriter.Create(feedData, settings);

                Atom10FeedFormatter atomFormatter = feed.GetAtom10Formatter();
                atomFormatter.WriteTo(writer);
            }
            finally
            {
                if(null != writer)
                {
                    writer.Close();
                }
            }

            return feedData.ToString();
        }

        /// <summary>
        /// Gets the AtomEntryDocument from the specified collection and for the specified member resource.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="memberResourceId">The Id of the member resource.</param>
        /// <returns>An AtomEntryDocument containing information for the member resource.</returns>
        private AtomEntryDocument GetAtomEntryDocument(string collectionName, string memberResourceId)
        {
            IAtomPubStoreReader storeReader = AtomPubStoreFactory.GetAtomPubStoreReader(base.BaseUri.OriginalString);
            SyndicationItem syndicationItem = storeReader.GetMember(collectionName, memberResourceId);

            AtomEntryDocument atomEntryDoc = new AtomEntryDocument(syndicationItem);

            return atomEntryDoc;
        }

        /// <summary>
        /// Gets the media resource for the specified member resource.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="memberResourceId">The member resource Id.</param>
        /// <param name="content">A byte array to get media resource content.</param>
        private void GetMediaResource(string collectionName, string memberResourceId, out byte[] content)
        {
            Stream contentStream = new MemoryStream();
            IAtomPubStoreReader storeReader = AtomPubStoreFactory.GetAtomPubStoreReader(base.BaseUri.OriginalString);
            storeReader.GetMedia(collectionName, memberResourceId, contentStream);
            contentStream.Seek(0, SeekOrigin.Begin);
            content = new byte[contentStream.Length];
            contentStream.Read(content, 0, content.Length);
            contentStream.Close();
        }

        /// <summary>
        /// Gets the type of the media content.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="memberResourceId">The member resource id.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>The media content type.</returns>
        private static string GetMediaContentType(
                                                string collectionName, 
                                                string memberResourceId, 
                                                out string fileExtension)
        {
            ResourceType collectionType = new CoreHelper().GetResourceType(collectionName);

            // Prepare a query to get a resource with specified Id and specified type.
            string commandText = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.EsqlToGetFileContents,
                                               collectionType.FullName);
            ObjectQuery<Core.File> query = new ObjectQuery<Core.File>(commandText, CoreHelper.CreateZentityContext());
            query.Parameters.Add(new ObjectParameter("Id", new Guid(memberResourceId)));

            var fileProperty = query.Select(file => new
            {
                file.MimeType,
                file.FileExtension
            }).FirstOrDefault();

            if(null != fileProperty)
            {
                fileExtension = fileProperty.FileExtension;
                return fileProperty.MimeType;
            }
            else
            {
                fileExtension = string.Empty;
                return null;
            }
        }

        #endregion
    }
}
