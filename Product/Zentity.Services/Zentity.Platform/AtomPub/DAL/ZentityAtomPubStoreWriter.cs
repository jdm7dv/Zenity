// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Zentity.Core;
    using Zentity.Platform.Properties;
    using Zentity.ScholarlyWorks;
    using Zentity.Security.Authentication;
    using Zentity.Security.AuthorizationHelper;

    /// <summary>
    /// Implements an AtomPub Store writer that can write to a Zentity repository.
    /// </summary>
    public class ZentityAtomPubStoreWriter : IAtomPubStoreWriter
    {
        private readonly Uri baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityAtomPubStoreWriter"/> class.
        /// </summary>
        public ZentityAtomPubStoreWriter()
            : this(AtomPubHelper.GetBaseUri())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityAtomPubStoreWriter"/> class.
        /// </summary>
        /// <param name="baseAddress">Base Uri to be added in all Uri properties of SyndicationItem object.</param>
        public ZentityAtomPubStoreWriter(string baseAddress)
        {
            baseUri = new Uri(baseAddress);
        }


        #region Properties

        /// <summary>
        /// Gets the base Uri to be added in all Uri properties of SyndicationItem object.
        /// </summary>
        protected Uri BaseUri
        {
            get
            {
                return baseUri;
            }
        }

        #endregion

        #region IAtomPubStoreWriter Members

        /// <summary>
        /// Creates a new resource of type collectionName in the repository.
        /// </summary>
        /// <param name="collectionName">The resource type.</param>
        /// <param name="atomEntry">Information about the resource.</param>
        /// <returns>A SyndicationItem that describes the newly created resource.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if collectionName is null/empty 
        /// or atomEntry is null/empty .</exception>
        SyndicationItem IAtomPubStoreWriter.CreateMember(string collectionName, AtomEntryDocument atomEntry)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(null == atomEntry)
            {
                throw new ArgumentNullException("atomEntry");
            }

            AuthenticatedToken authenticatedToken = CoreHelper.GetAuthenticationToken();

            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                if(!authenticatedToken.HasCreatePermission(context))
                {
                    throw new UnauthorizedException(Resources.ATOMPUB_UNAUTHORIZED);
                }

                ScholarlyWork resource = CreateScholarlyWork(collectionName);
                context.AddToResources(resource);
                ZentityAtomPubStoreWriter.UpdateResourceProperty(context, resource, atomEntry);

                resource.GrantDefaultPermissions(context, authenticatedToken);

                context.SaveChanges();

                return ZentityAtomPubStoreReader.GenerateSyndicationItem(this.BaseUri, resource);
            }
        }

        /// <summary>
        /// Creates the media.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="media">The media.</param>
        /// <param name="fileExtention">The file extention.</param>
        /// <returns>A SyndicationItem that describes the updated resource.</returns>
        SyndicationItem IAtomPubStoreWriter.CreateMedia(string collectionName, string mimeType, byte[] media, string fileExtention)
        {
            return this.CreateMedia(collectionName, mimeType, media, fileExtention);
        }

        /// <summary>
        /// Updates the metadata for the specified resource.
        /// </summary>
        /// <param name="collectionName">Name of the target collection.</param>
        /// <param name="memberResourceId">The Id of the member resource.</param>
        /// <param name="atomEntry">Describes the resource to be modified.</param>
        /// <returns>A SyndicationItem that describes the updated resource.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if collectionName is null/empty 
        /// or atomEntry is null.</exception>
        /// <exception cref="ArgumentException">Throws exception if requested memberResourceId is not a unique identifier.</exception>
        SyndicationItem IAtomPubStoreWriter.UpdateMemberInfo(string collectionName, string memberResourceId, AtomEntryDocument atomEntry)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(string.IsNullOrEmpty(memberResourceId))
            {
                throw new ArgumentNullException("memberResourceId");
            }

            if(null == atomEntry)
            {
                throw new ArgumentNullException("atomEntry");
            }

            using (ZentityContext context = CoreHelper.CreateZentityContext())
            {
                ScholarlyWork resource = (ScholarlyWork)AtomPubHelper.GetMember(context, collectionName, memberResourceId, "Update");
                resource.Files.Load();

                // Bug Fix : 177689 - AtomPub (M2): Author node is appending after every PUT request instead of overwriting it.            
                resource.Authors.Load();
                resource.Contributors.Load();

                UpdateResourceProperty(context, resource, atomEntry);
                context.SaveChanges();
                
                return ZentityAtomPubStoreReader.GenerateSyndicationItem(this.BaseUri, resource);
            }
        }

        /// <summary>
        /// Updates the Resource.File of the specified resource.
        /// </summary>
        /// <param name="collectionName">The type of the resource.</param>
        /// <param name="memberResourceId">The resource whose File needs to be updated.</param>
        /// <param name="mimeType">The MIME type of media.</param>
        /// <param name="media">The new File contents.</param>
        /// <returns>A SyndicationItem that describes the updated resource.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if collectionName is null/empty 
        /// or media is null.</exception>
        /// <exception cref="ArgumentException">Throws exception if requested memberResourceId is not a unique identifier.</exception>
        SyndicationItem IAtomPubStoreWriter.UpdateMedia(string collectionName, string memberResourceId, string mimeType, byte[] media)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(null == media)
            {
                throw new ArgumentNullException("media");
            }

            if(string.IsNullOrEmpty(memberResourceId))
            {
                throw new ArgumentNullException("memberResourceId");
            }

            if(!AtomPubHelper.IsValidGuid(memberResourceId))
            {
                throw new ArgumentException(Resources.ATOMPUB_INVALID_RESOURCE_ID, "memberResourceId");
            }

            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                Type collectionType = CoreHelper.GetSystemResourceType(collectionName);
                // Prepare a query to get a resource with specified Id and specified type.
                string commandText = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.EsqlToGetFileContents,
                                                   collectionType.FullName);

                ObjectQuery<Core.File> query = new ObjectQuery<Core.File>(commandText, context);
                query.Parameters.Add(new ObjectParameter("Id", new Guid(memberResourceId)));
                Core.File mediaResource = query.FirstOrDefault();

                if(null == mediaResource)
                {
                    throw new ResourceNotFoundException(Resources.ATOMPUB_RESOURCE_NOT_FOUND);
                }

                if(!mediaResource.Authorize("Update", context, CoreHelper.GetAuthenticationToken()))
                {
                    throw new UnauthorizedException(Resources.ATOMPUB_UNAUTHORIZED);
                }

                mediaResource.Resources.Load();
                ScholarlyWork resource = (ScholarlyWork)mediaResource.Resources.First();
                resource.DateModified = DateTime.Now;
                mediaResource.MimeType = mimeType;
                mediaResource.FileExtension = AtomPubHelper.GetFileExtension(mimeType);

                MemoryStream mediaStream = ZentityAtomPubStoreWriter.GetMediaStream(media);
                context.UploadFileContent(mediaResource, mediaStream);

                // Bug Fix : 180811 - Save Changes once mime type and contents are set.
                context.SaveChanges();

                return ZentityAtomPubStoreReader.GenerateSyndicationItem(this.BaseUri, resource);
            }
        }

        /// <summary>
        /// Deletes the specified resource.
        /// </summary>
        /// <param name="collectionName">The type of the resource.</param>
        /// <param name="memberResourceId">The Guid of the resource.</param>
        /// <returns>True if the operation succeeds, False otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if collectionName is null/empty.</exception>
        /// <exception cref="ArgumentException">Throws exception if requested memberResourceId is not a unique identifier.</exception>        
        bool IAtomPubStoreWriter.DeleteMember(string collectionName, string memberResourceId)
        {
            using (ZentityContext context = CoreHelper.CreateZentityContext())
            {
                ScholarlyWork resource = (ScholarlyWork)AtomPubHelper.GetMember(context, collectionName, memberResourceId, "Delete");

                // Load to delete all Core.File resource associated to requested scholarlywork.
                if (!resource.Files.IsLoaded)
                {
                    resource.Files.Load();
                }

                Zentity.Core.File[] resourceFiles = resource.Files.ToArray();

                for (int i = 0; i < resourceFiles.Length; i++)
                {
                    DeleteRelationships(context, resourceFiles[i]);
                    context.DeleteObject(resourceFiles[i]);
                }

                DeleteRelationships(context, resource);

                // Delete associated Resource propertes
                resource.ResourceProperties.Load();
                List<ResourceProperty> resProperties = resource.ResourceProperties.ToList();
                foreach (ResourceProperty property in resProperties)
                {
                    resource.ResourceProperties.Remove(property);
                    context.DeleteObject(property);
                }

                context.DeleteObject(resource);
                context.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Deletes the Resource.File for the specified resource.
        /// </summary>
        /// <param name="collectionName">The type of the resource.</param>
        /// <param name="memberResourceId">The Guid of the resource.</param>
        /// <returns>True if the operation succeeds, False otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if collectionName is null/empty.</exception>
        /// <exception cref="ArgumentException">Throws exception if requested memberResourceId is not a unique identifier.</exception>        
        bool IAtomPubStoreWriter.DeleteMedia(string collectionName, string memberResourceId)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(!AtomPubHelper.IsValidGuid(memberResourceId))
            {
                throw new ArgumentException(Resources.ATOMPUB_INVALID_RESOURCE_ID, "memberResourceId");
            }

            using (ZentityContext context = CoreHelper.CreateZentityContext())
            {
                Type collectionType = CoreHelper.GetSystemResourceType(collectionName);
                string commandText = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.EsqlToGetFileContents,
                                               collectionType.FullName);

                ObjectQuery<Core.File> query = new ObjectQuery<Core.File>(commandText, context);
                query.Parameters.Add(new ObjectParameter("Id", new Guid(memberResourceId)));

                Core.File mediaFile = query.FirstOrDefault();

                if (null == mediaFile)
                {
                    throw new ResourceNotFoundException(Resources.ATOMPUB_RESOURCE_NOT_FOUND);
                }

                if (!mediaFile.Authorize("Delete", context, CoreHelper.GetAuthenticationToken()))
                {
                    throw new UnauthorizedException(Resources.ATOMPUB_UNAUTHORIZED);
                }

                DeleteRelationships(context, mediaFile);
                context.DeleteObject(mediaFile);
                context.SaveChanges();
                return true;
            }
        }

        #endregion

        #region private Methods

        /// <summary>
        /// Deletes the relationships.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resource">The resource.</param>
        private static void DeleteRelationships(ZentityContext context, Resource resource)
        {
            if(!resource.RelationshipsAsObject.IsLoaded)
            {
                resource.RelationshipsAsObject.Load();
            }

            if(!resource.RelationshipsAsSubject.IsLoaded)
            {
                resource.RelationshipsAsSubject.Load();
            }

            Relationship[] relationships = resource.RelationshipsAsObject
                                         .Union(resource.RelationshipsAsSubject)
                                         .ToArray();

            for(int i = 0; i < relationships.Length; i++)
            {
                context.DeleteObject(relationships[i]);
            }
        }

        /// <summary>
        /// Creates a new Resource.File for a specified resource of type collectionName in the repository.
        /// </summary>
        /// <param name="collectionName">The resource type.</param>
        /// <param name="mimeType">The MIME type of media.</param>
        /// <param name="media">The new File contents.</param>
        /// <param name="fileExtension">The media file extension.</param>
        /// <returns>A SyndicationItem that describes the newly created resource.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if collectionName is null/empty 
        /// or mimeType is null/empty or media is null.</exception>
        protected SyndicationItem CreateMedia(string collectionName, string mimeType, byte[] media,
            string fileExtension)
        {

            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(string.IsNullOrEmpty(mimeType))
            {
                throw new ArgumentNullException("mimeType");
            }

            if(null == media)
            {
                throw new ArgumentNullException("media");
            }

            AuthenticatedToken authenticatedToken = CoreHelper.GetAuthenticationToken();

            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                if(!authenticatedToken.HasCreatePermission(context))
                {
                    throw new UnauthorizedException(Resources.ATOMPUB_UNAUTHORIZED);
                }

                ScholarlyWork resource = CreateScholarlyWork(collectionName);
                resource.DateModified = DateTime.Now;

                Core.File mediaResource = new Core.File();
                mediaResource.MimeType = mimeType;
                mediaResource.FileExtension = string.IsNullOrEmpty(fileExtension) ?
                    AtomPubHelper.GetFileExtension(mimeType) : fileExtension;
                context.AddToResources(mediaResource);
                context.SaveChanges();
                resource.Files.Add(mediaResource);

                MemoryStream mediaStream = ZentityAtomPubStoreWriter.GetMediaStream(media);
                context.UploadFileContent(mediaResource, mediaStream);
                mediaStream.Close();

                resource.GrantDefaultPermissions(context, authenticatedToken);
                mediaResource.GrantDefaultPermissions(context, authenticatedToken);

                context.SaveChanges();

                return ZentityAtomPubStoreReader.GenerateSyndicationItem(this.BaseUri, resource);
            }
        }

        /// <summary>
        /// Creates an instance of specified resource type.
        /// </summary>
        /// <param name="resourceType">The name of the collection in which the member resource should be created.</param>
        /// <returns>An instance of ScholarlyWork resource.</returns>
        protected internal static ScholarlyWork CreateScholarlyWork(string resourceType)
        {
            // Get the resource type to create new instance.
            Type collectionType = CoreHelper.GetSystemResourceType(resourceType);
            ScholarlyWork resource = Activator.CreateInstance(collectionType, false) as ScholarlyWork;

            return resource;
        }

        /// <summary>
        /// Gets an instance of MemoryStream from the specified byte array.
        /// </summary>
        /// <param name="media">Byte array for which MemoryStream has to be returned.</param>
        /// <returns>A <see cref="MemoryStream"/>.</returns>
        protected internal static MemoryStream GetMediaStream(byte[] media)
        {
            MemoryStream mediaStream = new MemoryStream(media);

            return mediaStream;
        }

        /// <summary>
        /// Updates the resource property.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="atomEntry">The atom entry.</param>
        private static void UpdateResourceProperty(
                                                ZentityContext context, 
                                                ScholarlyWork resource, 
                                                AtomEntryDocument atomEntry)
        {
            resource.Title = atomEntry.Title.Text;
            SetContentType(context, resource, atomEntry.Title.Type, AtomPubConstants.TitleTypeProperty);
            resource.DateModified = DateTime.Now;
            Publication publication = resource as Publication;

            if(null != publication && atomEntry.PublishDate != DateTimeOffset.MinValue)
            {
                publication.DatePublished = atomEntry.PublishDate.DateTime;
            }

            if(null != atomEntry.Copyright)
            {
                resource.Copyright = atomEntry.Copyright.Text;
                SetContentType(context, resource, atomEntry.Copyright.Type, AtomPubConstants.CopyrightTypeProperty);
            }

            if(null != atomEntry.Summary && !string.IsNullOrEmpty(atomEntry.Summary.Text))
            {
                SetExtensionProperty(context, resource, AtomPubConstants.SummaryProperty, atomEntry.Summary.Text);
                SetContentType(context, resource, atomEntry.Summary.Type, AtomPubConstants.SummaryTypeProperty);
            }

            if(null != atomEntry.Content)
            {
                UrlSyndicationContent urlContent = atomEntry.Content as UrlSyndicationContent;

                if(null != urlContent)
                {
                    resource.Description = null;
                    SetExtensionProperty(context, resource, AtomPubConstants.ContentUrlProperty, urlContent.Url.AbsoluteUri);
                }
                else
                {
                    ResourceProperty urlContentProperty = ZentityAtomPubStoreReader.GetResourceProperty(resource, AtomPubConstants.ContentUrlProperty);

                    if(null != urlContentProperty)
                    {
                        resource.ResourceProperties.Remove(urlContentProperty);
                    }

                    TextSyndicationContent textDescription = atomEntry.Content as TextSyndicationContent;

                    if(null != textDescription)
                    {
                        resource.Description = textDescription.Text;
                    }
                    else
                    {
                        XmlSyndicationContent content = atomEntry.Content as XmlSyndicationContent;

                        if(null != content)
                        {
                            XmlDictionaryReader contentReader = content.GetReaderAtContent();
                            StringBuilder contentValue = new StringBuilder(151);

                            try
                            {
                                while(contentReader.Read())
                                {
                                    contentValue.Append(contentReader.Value);
                                }

                            }
                            finally
                            {
                                contentReader.Close();
                            }

                            resource.Description = contentValue.ToString();
                        }
                    }
                }

                SetContentType(context, resource, atomEntry.Content.Type, AtomPubConstants.DescriptionTypeProperty);
            }

            if(null != atomEntry.Source)
            {
                ResourceProperty source = ZentityAtomPubStoreReader.GetResourceProperty(resource, AtomPubConstants.SourceProperty);

                if(null == source)
                {
                    Property sourceProperty = GetProperty(context, AtomPubConstants.SourceProperty);
                    source = new ResourceProperty
                    {
                        Property = sourceProperty,
                        Resource = resource,
                    };
                }

                source.Value = atomEntry.Source;
            }

            #region Add Links

            List<ResourceProperty> links = ZentityAtomPubStoreReader.GetResourceProperties(resource, AtomPubConstants.LinksProperty);

            if(0 == atomEntry.XmlLinks.Count && null != links)
            {
                foreach(var item in links)
                {
                    resource.ResourceProperties.Remove(item);
                }
            }

            Property linkProperty = GetProperty(context, AtomPubConstants.LinksProperty);

            foreach(string xmlLink in atomEntry.XmlLinks)
            {
                resource.ResourceProperties.Add(new ResourceProperty
                {
                    Resource = resource,
                    Property = linkProperty,
                    Value = xmlLink
                });
            }

            #endregion


            var authors = atomEntry.Authors.Select(author => new Person
            {
                Title = author.Name,
                Email = author.Email,
                Uri = author.Uri
            });
            // Bug Fix : 177689 - AtomPub (M2): Author node is appending after every PUT request instead 
            //                    of overwriting it.
            // Remove previous authors.
            resource.Authors.Clear();
            AddPersons(context, resource.Authors, authors);

            resource.Contributors.Clear();
            var contributors = atomEntry.Contributors.Select(author => new Person
            {
                Title = author.Name,
                Email = author.Email,
                Uri = author.Uri
            });
            AddPersons(context, resource.Contributors, contributors);
        }

        /// <summary>
        /// Sets the extension property.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        private static void SetExtensionProperty(
                                                ZentityContext context, 
                                                ScholarlyWork resource, 
                                                string propertyName, 
                                                string propertyValue)
        {
            ResourceProperty resourceProperty = ZentityAtomPubStoreReader.GetResourceProperty(resource, propertyName);

            if(null == resourceProperty)
            {
                Property extensionProperty = GetProperty(context, propertyName);
                resourceProperty = new ResourceProperty
                {
                    Property = extensionProperty,
                    Resource = resource,
                };
            }

            resourceProperty.Value = propertyValue;
        }

        /// <summary>
        /// Sets the type of the content.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="propertyName">Name of the property.</param>
        private static void SetContentType(
                                        ZentityContext context, 
                                        ScholarlyWork resource, 
                                        string contentType, 
                                        string propertyName)
        {
            if(contentType.ToUpperInvariant() == "TEXT")
            {
                return;
            }
            else
            {
                SetExtensionProperty(context, resource, propertyName, contentType);
            }
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>A <see cref="Property"/> type.</returns>
        private static Property GetProperty(ZentityContext context, string propertyName)
        {
            string propertyUri = AtomPubHelper.GetPropertyUri(propertyName);
            Property property = context.Properties.FirstOrDefault(prp => prp.Uri == propertyUri);

            if(null == property)
            {
                property = new Property
                {
                    Name = propertyName,
                    Uri = propertyUri
                };
            }

            return property;
        }

        /// <summary>
        /// Add Core.Person items to Navigation property of a resource.
        /// </summary>
        /// <param name="context">ZentityContext to search a person in database</param>
        /// <param name="contacts">Navigation property of resource to add persons</param>
        /// <param name="authors">List of persons to add to navigation property.</param>
        protected static void AddPersons(ZentityContext context, EntityCollection<Contact> contacts, IEnumerable<Person> authors)
        {
            foreach(Person author in authors)
            {
                if(!string.IsNullOrEmpty(author.Title))
                {
                    Match nameMatch = Regex.Match(author.Title, AtomPubConstants.PersonNamePattern, RegexOptions.IgnoreCase);
                    author.FirstName = nameMatch.Groups[AtomPubConstants.FirstName].Value;
                    author.MiddleName = nameMatch.Groups[AtomPubConstants.MiddleName].Value;
                    author.LastName = nameMatch.Groups[AtomPubConstants.LastName].Value;
                }

                System.Linq.Expressions.Expression filter = null;
                ParameterExpression param = System.Linq.Expressions.Expression.Parameter(typeof(Person), "person");

                if(!string.IsNullOrEmpty(author.Uri))
                {
                    AtomPubHelper.GeneratePersonFilterExpression("Uri", author.Uri, param, ref filter);
                }
                else if(!string.IsNullOrEmpty(author.Email))
                {
                    AtomPubHelper.GeneratePersonFilterExpression("Email", author.Email, param, ref filter);
                }
                else
                {
                    AtomPubHelper.GeneratePersonFilterExpression(AtomPubConstants.FirstName, author.FirstName, param, ref filter);
                    AtomPubHelper.GeneratePersonFilterExpression(AtomPubConstants.MiddleName, author.MiddleName, param, ref filter);
                    AtomPubHelper.GeneratePersonFilterExpression(AtomPubConstants.LastName, author.LastName, param, ref filter);
                }

                if(null == filter)
                {
                    continue;
                }

                Expression<Func<Person, bool>> predicate = System.Linq.Expressions.Expression.Lambda(filter, param) as Expression<Func<Person, bool>>;

                Person contact = context.People().FirstOrDefault(predicate);

                if(null == contact)
                {
                    contact = author;
                }

                contacts.Add(contact);
            }
        }

        #endregion
    }
}
