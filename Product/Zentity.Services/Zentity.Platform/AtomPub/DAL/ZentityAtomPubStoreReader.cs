// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel.Syndication;
    using System.Xml;
    using Zentity.Core;
    using Zentity.Platform.Properties;
    using Zentity.ScholarlyWorks;
    using Zentity.Security.Authentication;
    using Zentity.Security.AuthorizationHelper;

    /// <summary>
    /// Implements an AtomPub Store reader that can read from a Zentity Repository.
    /// </summary>
    public sealed class ZentityAtomPubStoreReader : IAtomPubStoreReader
    {
        private readonly Uri baseUri;
        private readonly CoreHelper coreHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityAtomPubStoreReader"/> class.
        /// </summary>
        public ZentityAtomPubStoreReader()
            : this(AtomPubHelper.GetBaseUri())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityAtomPubStoreReader"/> class.
        /// </summary>
        /// <param name="baseAddress">Base Uri to be added in all Uri properties of SyndicationItem object.</param>
        public ZentityAtomPubStoreReader(string baseAddress)
        {
            baseUri = new Uri(baseAddress);
            coreHelper = new CoreHelper();
        }


        #region Properties

        /// <summary>
        /// Gets the base Uri to be added in all Uri properties of SyndicationItem object.
        /// </summary>
        private Uri BaseUri
        {
            get
            {
                return baseUri;
            }
        }

        #endregion

        #region IAtomPubStoreReader Members

        /// <summary>
        /// Gets all the resource types that inherit from ScholarlyWork, including ScholarlyWork.
        /// </summary>
        /// <returns>An array of strings containing the resource type names.</returns>
        string[] IAtomPubStoreReader.GetCollectionNames()
        {
            List<string> resourceTypes = new List<string>();

            using(ZentityContext zentityContext = CoreHelper.CreateZentityContext())
            {
                resourceTypes = new List<string>();

                // Get all resource types which are derived from ScholarlyWork as
                // only ScholarlyWork type supports authors list.
                foreach(ResourceType typeInfo in CoreHelper.GetResourceTypes(zentityContext))
                {
                    if(AtomPubHelper.IsValidCollectionType(typeInfo.Name))
                    {
                        resourceTypes.Add(typeInfo.Name);
                    }
                }
            }

            return resourceTypes
                    .OrderBy(name => name)
                    .ToArray();
        }

        /// <summary>
        /// Gets a SyndicationFeed containing resources of type collectionName.
        /// </summary>
        /// <param name="collectionName">The name of the resource type.</param>
        /// <param name="skip">The number of resources to skip from the start.</param>
        /// <param name="count">The number of resources to return.</param>
        /// <returns>A SyndicationFeed of resources of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if collectionName is null/empty.</exception>
        /// <exception cref="ArgumentException">Throws exception if skip value is negative or count value is negative.</exception>
        SyndicationFeed IAtomPubStoreReader.GetMembers(string collectionName, long skip, long count)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(0 > skip)
            {
                throw new ArgumentException(Resources.ATOMPUB_INVALID_VALUE, "skip");
            }
            if(0 > count)
            {
                throw new ArgumentException(Resources.ATOMPUB_INVALID_VALUE, "count");
            }

            int skipCount = (int)skip;
            int takeCount = (int)count;

            ResourceType collectionType = coreHelper.GetResourceType(collectionName);


            // Prepare a query to get a resource with specified Id and specified type.
            string commandText = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.EsqlToGetAllResources,
                                               collectionType.FullName);
            AuthenticatedToken authenticatedToken = CoreHelper.GetAuthenticationToken();

            using(ZentityContext zentityContext = CoreHelper.CreateZentityContext())
            {
                ObjectQuery<ScholarlyWork> resourcesQuery = new ObjectQuery<ScholarlyWork>(commandText, zentityContext);
                List<ScholarlyWork> resources = resourcesQuery.Authorize("Read", zentityContext, authenticatedToken)
                                                .OrderByDescending(resource => resource.DateModified)
                                                .Skip(skipCount).Take(takeCount)
                                                .ToList();

                List<SyndicationItem> syndicationItems = new List<SyndicationItem>();

                if(null != resources && 0 < resources.Count)
                {
                    foreach(ScholarlyWork resource in resources)
                    {
                        SyndicationItem syndicationItem = ZentityAtomPubStoreReader.GenerateSyndicationItem(this.BaseUri, resource);
                        syndicationItems.Add(syndicationItem);
                    }
                }

                return new SyndicationFeed(syndicationItems);
            }
        }

        /// <summary>
        /// Gets the specified resource.
        /// </summary>
        /// <param name="collectionName">The name of the resource type.</param>
        /// <param name="memberResourceId">The Guid of the resource to return.</param>
        /// <returns>A SyndicationItem for the specified resource.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if collectionName is null/empty.</exception>
        SyndicationItem IAtomPubStoreReader.GetMember(string collectionName, string memberResourceId)
        {
            using (ZentityContext context = CoreHelper.CreateZentityContext())
            {
                ScholarlyWork resource = (ScholarlyWork)AtomPubHelper.GetMember(context, collectionName, memberResourceId, "Read");
                return ZentityAtomPubStoreReader.GenerateSyndicationItem(this.BaseUri, resource);
            }
        }

        /// <summary>
        /// Gets the number of resources present in a collection.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <returns>Number of members present in the specified collection.</returns>
        long IAtomPubStoreReader.GetMembersCount(string collectionName)
        {
            Type collectionType = CoreHelper.GetSystemResourceType(collectionName);
            // Prepare a query to get a resource with specified Id and specified type.
            string commandText = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.EsqlToGetAllResources,
                                               collectionType.FullName);
            AuthenticatedToken authenticatedToken = CoreHelper.GetAuthenticationToken();

            using(ZentityContext zentityContext = CoreHelper.CreateZentityContext())
            {
                ObjectQuery<ScholarlyWork> resourcesQuery = new ObjectQuery<ScholarlyWork>(commandText, zentityContext);
                return Convert.ToInt64(resourcesQuery.Authorize("Read", zentityContext, authenticatedToken)
                                                     .Count());
            }
        }

        /// <summary>
        /// Checks if the member of given type a specified id is present in the given collection.
        /// </summary>
        /// <param name="collectionName">Name of the target collection.</param>
        /// <param name="memberResourceId">Id of the member.</param>
        /// <returns>True if member is present in the given collection, else false.</returns>
        bool IAtomPubStoreReader.IsMemberPresent(string collectionName, string memberResourceId)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(string.IsNullOrEmpty(memberResourceId))
            {
                throw new ArgumentNullException("memberResourceId");
            }

            ResourceType collectionType = coreHelper.GetResourceType(collectionName);

            // Prepare a query to get a resource with specified Id and specified type.
            string commandText = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.EsqlToGetResourceById,
                                               collectionType.FullName);

            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                ObjectQuery<ScholarlyWork> query = new ObjectQuery<ScholarlyWork>(commandText, context);
                query.Parameters.Add(new ObjectParameter("Id", new Guid(memberResourceId)));

                return 0 < query.Count();
            }
        }

        /// <summary>
        /// Gets a media contents of a member resource.
        /// </summary>
        /// <param name="collectionName">Name of the target collection.</param>
        /// <param name="memberResourceId">The Id of the member resource.</param>       
        /// <param name="outputStream">A stream containing the media corresponding to the specified resource. If no matching media is found, null is returned.</param>
        /// <exception cref="ArgumentNullException">Throws exception if memberResourceId is null/empty
        /// or outputStream is null.</exception>
        /// <exception cref="ArgumentException">Throws exception if outputStream stream is not readable.</exception>
        void IAtomPubStoreReader.GetMedia(string collectionName, string memberResourceId, Stream outputStream)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(string.IsNullOrEmpty(memberResourceId))
            {
                throw new ArgumentNullException("memberResourceId");
            }

            if(null == outputStream)
            {
                throw new ArgumentNullException("outputStream");
            }

            if(!AtomPubHelper.IsValidGuid(memberResourceId))
            {
                throw new ArgumentException(Resources.ATOMPUB_INVALID_RESOURCE_ID);
            }

            if(!outputStream.CanWrite)
            {
                throw new ArgumentException(Properties.Resources.ATOMPUB_CANNOT_WRITE_ON_STREAM, "outputStream");
            }

            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                ResourceType collectionType = coreHelper.GetResourceType(collectionName);

                // Prepare a query to get a resource with specified Id and specified type.
                string commandText = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.EsqlToGetFileContents,
                                                   collectionType.FullName);

                ObjectQuery<Core.File> query = new ObjectQuery<Core.File>(commandText, context);
                query.Parameters.Add(new ObjectParameter("Id", new Guid(memberResourceId)));
                Core.File mediaFile = query.FirstOrDefault();

                if(null == mediaFile)
                {
                    throw new ResourceNotFoundException(Resources.ATOMPUB_RESOURCE_NOT_FOUND);
                }

                if(!mediaFile.Authorize("Read", context, CoreHelper.GetAuthenticationToken()))
                {
                    throw new UnauthorizedException(Resources.ATOMPUB_UNAUTHORIZED);
                }

                context.DownloadFileContent(mediaFile, outputStream);
            }
        }

        /// <summary>
        /// Checks if the member of given type a specified id is present in the given collection.
        /// </summary>
        /// <param name="collectionName">Name of the target collection.</param>
        /// <param name="memberResourceId">Id of the member.</param>
        /// <returns>True if media is present for the given member, else false.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if memberResourceId is null/empty.</exception>
        bool IAtomPubStoreReader.IsMediaPresentForMember(string collectionName, string memberResourceId)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(string.IsNullOrEmpty(memberResourceId))
            {
                throw new ArgumentNullException("memberResourceId");
            }

            ResourceType collectionType = coreHelper.GetResourceType(collectionName);

            // Prepare a query to get a resource with specified Id and specified type.
            string commandText = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.EsqlToGetFileContents,
                                               collectionType.FullName);

            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                ObjectQuery<ScholarlyWork> query = new ObjectQuery<ScholarlyWork>(commandText, context);
                query.Parameters.Add(new ObjectParameter("Id", new Guid(memberResourceId)));

                return 0 < query.Count();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Generates the Syndication Item from the specified resource.
        /// </summary>
        /// <param name="baseUri">Base Uri to generate links Url and content Url in SyndicationItem.</param>
        /// <param name="resource">An instance of resource from which SyndicationItem is to be generated.</param>
        /// <returns>A <see cref="SyndicationItem"/>.</returns>
        internal static SyndicationItem GenerateSyndicationItem(Uri baseUri, ScholarlyWork resource)
        {

            #region Load Navigation properties
            // Load navigation properties explicitly.

            try
            {
                if(!resource.Files.IsLoaded)
                {
                    resource.Files.Load();
                }

                if(!resource.Authors.IsLoaded)
                {
                    resource.Authors.Load();
                }

                if(!resource.Contributors.IsLoaded)
                {
                    resource.Contributors.Load();
                }

                if(!resource.ResourceProperties.IsLoaded)
                {
                    resource.ResourceProperties.Load();
                }

                if(null != resource.ResourceProperties)
                {
                    foreach(ResourceProperty resourceProperty in resource.ResourceProperties)
                    {
                        if(!resourceProperty.PropertyReference.IsLoaded)
                        {
                            resourceProperty.PropertyReference.Load();
                        }
                    }
                }

                // Load parent container of resource.
                if(!resource.ContainerReference.IsLoaded)
                {
                    resource.ContainerReference.Load();
                }

                if(null != resource.Container)
                {
                    // Load child container of resource.
                    if(!resource.Container.ContainedWorks.IsLoaded)
                    {
                        resource.Container.ContainedWorks.Load();
                    }

                    foreach(ScholarlyWorkContainer childContainer in resource.Container.ContainedWorks.OfType<ScholarlyWorkContainer>())
                    {
                        // Load child resources of parent resource.
                        if(!childContainer.ContainedWorks.IsLoaded)
                        {
                            childContainer.ContainedWorks.Load();
                        }
                    }
                }
            }
            catch(InvalidOperationException)
            {
                // DO nothing if resource not attached to the context.
            }

            #endregion

            SyndicationItem item = new SyndicationItem();
            item.Id = AtomPubConstants.IdPrefix + resource.Id.ToString();
            item.LastUpdatedTime = (null != resource.DateModified) ? resource.DateModified.Value : DateTimeOffset.MinValue;
            item.Title = GetContent<TextSyndicationContent>(resource.Title, resource.ResourceProperties, AtomPubConstants.TitleTypeProperty, true);
            item.Copyright = GetContent<TextSyndicationContent>(resource.Copyright, resource.ResourceProperties, AtomPubConstants.CopyrightTypeProperty, false);

            Publication publication = resource as Publication;

            if(null != publication && null != publication.DatePublished)
            {
                item.PublishDate = publication.DatePublished.Value;
            }

            #region Add Authors and Contributors

            AddSyndicationPerson(resource.Authors, item.Authors);

            if(item.Authors.Count == 0)
            {
                item.Authors.Add(new SyndicationPerson(null, string.Empty, null));
            }

            AddSyndicationPerson(resource.Contributors, item.Contributors);

            #endregion

            #region Edit and Edit-Media links for main resource

            AddLinksAndSource(baseUri, resource, ref item);

            #endregion

            ResourceProperty summary = GetResourceProperty(resource, AtomPubConstants.SummaryProperty);

            if(null != summary)
            {
                item.Summary = GetContent<TextSyndicationContent>(summary.Value, resource.ResourceProperties, AtomPubConstants.SummaryTypeProperty, false);
            }

            if(!(item.Content is TextSyndicationContent))
            {
                item.Summary = new TextSyndicationContent(string.Empty);
            }

            return item;
        }

        /// <summary>
        /// Adds the links and source.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="item">The syndication item.</param>
        private static void AddLinksAndSource(Uri baseUri, ScholarlyWork resource, ref SyndicationItem item)
        {
            // NameValue pair for creating Uri from UriTemplate.
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add(AtomPubParameterType.CollectionName.ToString(), resource.GetType().Name);
            parameters.Add(AtomPubParameterType.Id.ToString(), resource.Id.ToString());

            Core.File mediaFile = resource.Files.FirstOrDefault();

            if (null != mediaFile)
            {
                Uri mediaUri = AtomPubHelper.AtomPubTemplates[AtomPubRequestType.EditMedia]
                                            .BindByName(baseUri, parameters);

                // Add content linnk and edit-media link
                if (string.IsNullOrEmpty(mediaFile.MimeType))
                {
                    mediaFile.MimeType = AtomPubConstants.DefaultMimeType;
                }

                SyndicationLink resourceMediaLink = new SyndicationLink(mediaUri);
                resourceMediaLink.RelationshipType = AtomPubConstants.EditMedia;
                item.Links.Add(resourceMediaLink);

                item.Content = new UrlSyndicationContent(mediaUri, mediaFile.MimeType);
            }
            else
            {
                ResourceProperty urlContent = GetResourceProperty(resource, AtomPubConstants.ContentUrlProperty);

                if (null != urlContent && !string.IsNullOrEmpty(urlContent.Value))
                {
                    item.Content = GetContent<UrlSyndicationContent>(urlContent.Value, resource.ResourceProperties, AtomPubConstants.DescriptionTypeProperty, false);
                }
                else
                {
                    item.Content = GetContent<SyndicationContent>(resource.Description, resource.ResourceProperties, AtomPubConstants.DescriptionTypeProperty, true);
                }
            }

            // Add resource Edit link
            Uri resourceUri = AtomPubHelper.AtomPubTemplates[AtomPubRequestType.EditMember].BindByName(baseUri, parameters);
            SyndicationLink resourceEditLink = new SyndicationLink(resourceUri);
            resourceEditLink.RelationshipType = AtomPubConstants.Edit;
            item.Links.Add(resourceEditLink);

            if (null != resource.Container)
            {
                var relatedItems = resource.Container.ContainedWorks.OfType<ScholarlyWorkContainer>()
                                           .SelectMany(tuple => tuple.ContainedWorks)
                                           .Where(tuple => !(tuple is ScholarlyWorkContainer));

                foreach (ScholarlyWork containedItem in relatedItems)
                {
                    parameters.Set(AtomPubParameterType.CollectionName.ToString(), containedItem.GetType().Name);
                    parameters.Set(AtomPubParameterType.Id.ToString(), containedItem.Id.ToString());
                    resourceUri = AtomPubHelper.AtomPubTemplates[AtomPubRequestType.EditMember].BindByName(baseUri, parameters);
                    SyndicationLink containsEditLink = new SyndicationLink(resourceUri);
                    containsEditLink.RelationshipType = AtomPubConstants.Related;
                    item.Links.Add(containsEditLink);
                }
            }

            AtomEntryDocument entry = new AtomEntryDocument(item);

            IEnumerable<ResourceProperty> links = ZentityAtomPubStoreReader.GetResourceProperties(resource, AtomPubConstants.LinksProperty);

            if (null != links)
            {
                foreach (ResourceProperty link in links)
                {
                    entry.XmlLinks.Add(link.Value);
                }
            }

            ResourceProperty source = GetResourceProperty(resource, AtomPubConstants.SourceProperty);

            if (null != source)
            {
                entry.Source = source.Value;
            }

            using (XmlReader reader = new XmlTextReader(entry.AtomEntry, XmlNodeType.Document, null))
            {
                item = SyndicationItem.Load(reader);
            }
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <typeparam name="T">Type of content.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="typeProperty">The type property.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <returns>Content of the required type.</returns>
        private static T GetContent<T>(
                                    string content, 
                                    IEnumerable<ResourceProperty> properties, 
                                    string typeProperty, 
                                    bool isRequired) where T : SyndicationContent
        {
            if(string.IsNullOrEmpty(content))
            {
                if(isRequired)
                {
                    return new TextSyndicationContent(string.Empty) as T;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                string propertyUri = AtomPubHelper.GetPropertyUri(typeProperty);
                ResourceProperty contentTypeProperty = properties
                                                   .Where(tuple => null != tuple.Property && tuple.Property.Uri == propertyUri)
                                                   .FirstOrDefault();

                SyndicationContent syndicationContent = null;

                if(null == contentTypeProperty)
                {
                    syndicationContent = new TextSyndicationContent(content, TextSyndicationContentKind.Plaintext);
                }
                else
                {
                    switch(contentTypeProperty.Value.ToUpperInvariant())
                    {
                        case "HTML":
                            syndicationContent = new TextSyndicationContent(content, TextSyndicationContentKind.Html);
                            break;
                        case "XHTML":
                            syndicationContent = new TextSyndicationContent(content, TextSyndicationContentKind.XHtml);
                            break;
                        case "TEXT":
                            syndicationContent = new TextSyndicationContent(content, TextSyndicationContentKind.Plaintext);
                            break;
                        default:

                            if(typeof(T) == typeof(UrlSyndicationContent))
                            {
                                Uri contentUrl = new Uri(content);
                                syndicationContent = SyndicationContent.CreateUrlContent(contentUrl, contentTypeProperty.Value);
                            }
                            else
                            {
                                syndicationContent = new XmlSyndicationContent(contentTypeProperty.Value, content, new NetDataContractSerializer());
                            }
                            break;
                    }
                }

                return syndicationContent as T;
            }

        }

        /// <summary>
        /// Gets the resource properties.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>List of <see cref="ResourceProperty"/>.</returns>
        internal static List<ResourceProperty> GetResourceProperties(ScholarlyWork resource, string propertyName)
        {
            if(!resource.ResourceProperties.IsLoaded)
            {
                try
                {
                    resource.ResourceProperties.Load();
                }
                catch(InvalidOperationException)
                {
                    // Do nothing if resource is not attached to the context.
                }
            }
            string propertyUri = AtomPubHelper.GetPropertyUri(propertyName);
            return resource.ResourceProperties
                           .Where(tuple => tuple.Property.Uri == propertyUri)
                           .ToList();
        }

        /// <summary>
        /// Gets the resource property.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>A <see cref="ResourceProperty"/>.</returns>
        internal static ResourceProperty GetResourceProperty(ScholarlyWork resource, string propertyName)
        {
            if(!resource.ResourceProperties.IsLoaded)
            {
                try
                {
                    resource.ResourceProperties.Load();

                    foreach(ResourceProperty resourceProperties in resource.ResourceProperties)
                    {
                        if(!resourceProperties.PropertyReference.IsLoaded)
                        {
                            resourceProperties.PropertyReference.Load();
                        }
                    }
                }
                catch(InvalidOperationException)
                {
                    // Do nothing if resource is not attached to the context.
                }
            }
            string propertyUri = AtomPubHelper.GetPropertyUri(propertyName);
            return resource.ResourceProperties
                           .Where(tuple => tuple.Property.Uri == propertyUri)
                           .FirstOrDefault();
        }

        /// <summary>
        /// Adds the syndication person.
        /// </summary>
        /// <param name="contacts">The contacts.</param>
        /// <param name="persons">The persons.</param>
        private static void AddSyndicationPerson(EntityCollection<Contact> contacts, Collection<SyndicationPerson> persons)
        {
            foreach(Person person in contacts.OfType<Person>())
            {
                List<string> nameList = new List<string>();
                nameList.Add(person.FirstName);
                nameList.Add(person.MiddleName);
                nameList.Add(person.LastName);
                string name = string.Join(" ", nameList.Where(namepart => !string.IsNullOrEmpty(namepart))
                                                .ToArray());
                persons.Add(new SyndicationPerson(person.Email, name, person.Uri));
            }
        }

        #endregion
    }
}
