// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Objects.DataClasses;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel.Syndication;
    using System.Web;
    using Zentity.Core;
    using Zentity.Platform.Properties;
    using Zentity.ScholarlyWorks;
    using Zentity.Security.Authentication;
    using Zentity.Security.AuthorizationHelper;

    /// <summary>
    /// The class exposes methods which can Add, Update or Delete 
    /// the Member Resource, Media from the Zentity Repository.
    /// </summary>
    public class ZentitySwordStoreWriter : ZentityAtomPubStoreWriter, IAtomPubStoreWriter
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ZentitySwordStoreWriter"/> class.
        /// </summary>
        public ZentitySwordStoreWriter()
            : base(SwordHelper.GetBaseUri())
        {
        }

        #endregion

        #region public Methods

        /// <summary>
        /// Creates a new Resource.File for a specified resource of type collectionName in the repository.
        /// </summary>
        /// <param name="collectionName">The resource type.</param>
        /// <param name="mimeType">The MIME type of media.</param>
        /// <param name="media">The new File contents.</param>
        /// <param name="fileExtension">The media file extension.</param>
        /// <returns>A SyndicationItem that describes the newly created resource.</returns>
        SyndicationItem IAtomPubStoreWriter.CreateMedia(string collectionName, string mimeType,
            byte[] media, string fileExtension)
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

            if(SwordConstants.ZipContentType != mimeType)
            {
                return base.CreateMedia(collectionName, mimeType, media, fileExtension);
            }

            // Convert byte array to stream.
            MemoryStream mediaStream = ZentityAtomPubStoreWriter.GetMediaStream(media);
            string extractionPath = ExtractZipContent(mediaStream);
            HttpContext.Current.Items[SwordConstants.ZipExtractedPath] = extractionPath;

            // Get the path of METS xml file.
            string metsFilePath = extractionPath + "\\" + SwordConstants.MetsDocumentName;

            if(!System.IO.File.Exists(metsFilePath))
            {
                //string errorMessage = string.Format(CultureInfo.CurrentCulture,
                //                                    Properties.Resources.SWORD_MISSING_METS_DOCUMENT,
                //                                    SwordConstants.MetsDocumentName);
                //throw new MetsException(errorMessage);
                return base.CreateMedia(collectionName, mimeType, media, fileExtension);
            }

            AuthenticatedToken authenticatedToken = CoreHelper.GetAuthenticationToken();

            using(ZentityContext zentityContext = CoreHelper.CreateZentityContext())
            {
                if(!authenticatedToken.HasCreatePermission(zentityContext))
                {
                    throw new UnauthorizedException(Resources.ATOMPUB_UNAUTHORIZED);
                }

                // Generate METS document from given METS xml file.
                MetsDocument document = new MetsDocument(metsFilePath);

                // Create resource of specified collection type.
                ScholarlyWork resource = CreateScholarlyWork(collectionName);
                resource.DateModified = DateTime.Now;

                // Upload the zip file contents as media for main resource.
                // This will be required in AtomPub get requests and further use.                          
                Core.File mediaResource = AddFileResource(zentityContext, resource, mediaStream);
                mediaResource.MimeType = mimeType;
                mediaResource.FileExtension = AtomPubHelper.GetFileExtension(mimeType);
                // close the stream                
                mediaStream.Close();

                AddChildResources(extractionPath, document, resource, zentityContext);

                resource.GrantDefaultPermissions(zentityContext, authenticatedToken);
                mediaResource.GrantDefaultPermissions(zentityContext, authenticatedToken);

                // Save all changes at the end
                zentityContext.SaveChanges();

                return ZentityAtomPubStoreReader.GenerateSyndicationItem(base.BaseUri, resource);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the child resources.
        /// </summary>
        /// <param name="extractionPath">The extraction path.</param>
        /// <param name="document">The document.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="zentityContext">The zentity context.</param>
        private static void AddChildResources(
                                            string extractionPath, 
                                            MetsDocument document, 
                                            ScholarlyWork resource, 
                                            ZentityContext zentityContext)
        {
            resource.Container = new ScholarlyWorkContainer();
            ScholarlyWorkContainer childContainer = null;

            string[] fileNames = Directory.GetFiles(extractionPath)
                                          .Select(path => GetFileName(path))
                                          .Where(name => SwordConstants.MetsDocumentName != name)
                                          .ToArray();

            if(0 < fileNames.Length)
            {
                childContainer = new ScholarlyWorkContainer();
                resource.Container.ContainedWorks.Add(childContainer);
            }

            // Loop though all files which are extracted.
            foreach(string fileName in fileNames)
            {
                // Get the extension 
                int dotIndex = fileName.LastIndexOf('.');
                string fileExtension = (0 < dotIndex) ? fileName.Substring(dotIndex + 1) : string.Empty;

                #region Upload Zip File Contents

                // Get Metadata for the specified fileName
                MetadataSection dataSection = document.Files[fileName];

                // Create resource against each type as specified in the METS document.
                ScholarlyWork individualResource = CreateResouceUsingMetsMetadata(dataSection);

                UpdateResourceProeprties(zentityContext, individualResource, dataSection);

                // Create Media and Upload file contents.
                Core.File individualMediaResource = AddFileResource(zentityContext,
                                                                    individualResource,
                                                                    extractionPath + "\\" + fileName);
                individualMediaResource.MimeType = AtomPubHelper.GetMimeTypeFromFileExtension(fileExtension);
                individualMediaResource.FileExtension = fileExtension;

                // Save file name in notes for future references.
                individualMediaResource.Description = fileName;

                // Associate with the main resource.
                childContainer.ContainedWorks.Add(individualResource);

                #endregion

                AuthenticatedToken authenticatedToken = CoreHelper.GetAuthenticationToken();

                individualResource.GrantDefaultPermissions(zentityContext, authenticatedToken);
                individualMediaResource.GrantDefaultPermissions(zentityContext, authenticatedToken);
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The file name</returns>
        private static string GetFileName(string filePath)
        {
            // Separate the file name and extension
            int fileStartIndex = filePath.LastIndexOf('\\') + 1;
            string fileName = (0 < fileStartIndex) ? filePath.Substring(fileStartIndex) : filePath;
            return fileName;
        }

        /// <summary>
        /// Extracts the content of the zip.
        /// </summary>
        /// <param name="mediaStream">The media stream.</param>
        /// <returns>The extracted zip content.</returns>
        private static string ExtractZipContent(MemoryStream mediaStream)
        {
            string extractedPath = null;
            try
            {
                // If the content type is zip file then extract the zip contents                
                extractedPath = ZipExtractor.UnzipFileContents(mediaStream);
            }
            catch(Exception ex)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, Properties.Resources.SWORD_CANNOT_EXTRACT_ZIP,
                                                    System.IO.Path.GetTempPath());
                throw new SwordException(ex.Message + errorMessage);
            }

            return extractedPath;
        }

        /// <summary>
        /// Adds the file resource.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="mediaStream">The media stream.</param>
        /// <returns>The added <see cref="Core.File"/> type.</returns>
        private static Core.File AddFileResource(
                                                ZentityContext context,
                                                ScholarlyWork resource,
                                                Stream mediaStream)
        {
            Core.File mediaResource = AddFileResource(context, resource);
            mediaStream.Position = 0;
            context.UploadFileContent(mediaResource, mediaStream);
            return mediaResource;
        }

        /// <summary>
        /// Adds the file resource.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resource">The resource.</param>
        /// <returns>The added <see cref="Core.File"/> type.</returns>
        private static Core.File AddFileResource(ZentityContext context, ScholarlyWork resource)
        {
            Core.File mediaResource = new Core.File();
            context.AddToResources(mediaResource);
            context.SaveChanges();
            resource.Files.Add(mediaResource);
            return mediaResource;
        }

        /// <summary>
        /// Adds the file resource.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="mediaFilePath">The media file path.</param>
        /// <returns>The added <see cref="Core.File"/> type.</returns>
        private static Core.File AddFileResource(
                                                ZentityContext context,
                                                ScholarlyWork resource,
                                                string mediaFilePath)
        {
            Core.File mediaResource = AddFileResource(context, resource);
            context.UploadFileContent(mediaResource, mediaFilePath);
            return mediaResource;
        }

        /// <summary>
        /// Create resource using metadata.
        /// </summary>
        /// <param name="dataSection">Metadata Section containing metadata of the resource.</param>
        /// <returns>A resource of type ScholarlyWork or its derived type.</returns>
        private static ScholarlyWork CreateResouceUsingMetsMetadata(MetadataSection dataSection)
        {
            if(null == dataSection)
            {
                throw new ArgumentNullException("dataSection");
            }

            ScholarlyWork resource = null;
            Type resourceType = null;
            // Get the resource type specified in METS document.            
            if(null != dataSection.DescriptiveMetadata && null != dataSection.DescriptiveMetadata.ResourceType
                && 0 < dataSection.DescriptiveMetadata.ResourceType.Count)
            {
                string resourceTypeName = dataSection.DescriptiveMetadata.ResourceType[0];

                // Check if the resource type is derived from ScholarlyWork,
                // as Sword & AtomPub supports only ScholarlyWork and its derivatives.
                if(AtomPubHelper.IsValidCollectionType(resourceTypeName))
                {
                    resourceType = CoreHelper.GetSystemResourceType(resourceTypeName);
                }
            }

            // Take ScholarlyWork as default resource type.
            if(null == resourceType)
            {
                resourceType = typeof(ScholarlyWork);
            }

            // Create resource of type ScholarlyWork or its derived types.
            resource = Activator.CreateInstance(resourceType, false) as ScholarlyWork;
            if(null == resource)
            {
                throw new SwordException(Properties.Resources.UNSUPPORTED_RESOURCE_TYPE);
            }

            return resource;
        }

        /// <summary>
        /// Update the properties of the resource.
        /// </summary>
        /// <param name="zentityContext">The ZentityContext to which the resource is attached.</param>
        /// <param name="resource">Resource to be update scalar properties.</param>
        /// <param name="dataSection"><typeref name="MetadataSection"/> Metadata which contains property values.</param>
        private static void UpdateResourceProeprties(ZentityContext zentityContext, ScholarlyWork resource,
                                              MetadataSection dataSection)
        {
            if(null == resource)
            {
                throw new ArgumentNullException("resource");
            }

            if(null == dataSection)
            {
                throw new ArgumentNullException("dataSection");
            }

            resource.DateModified = DateTime.Now;

            PropertyInfo[] properties = resource.GetType().GetProperties()
                                            .Where(property => PropertyMapper.ResourceProperties.ContainsKey(property.Name))
                                            .ToArray();

            // Loop for each property of the resource.
            foreach(PropertyInfo property in properties)
            {
                ReadOnlyCollection<string> values = GetPropertyValues(property.Name, dataSection);

                // If no values are present for current property, do not process.
                if(null == values || 0 >= values.Count)
                {
                    continue;
                }

                // Check if the property is scalar property or Navigation Property.
                object[] navigationAttr = property.GetCustomAttributes(typeof(EdmRelationshipNavigationPropertyAttribute), true);

                if(0 < navigationAttr.Length)
                {
                    UpdateNavigationproperties(zentityContext, resource, property, values);
                }
                else
                {
                    UpdateScalarProperties(resource, property, values);
                }
            }
        }

        /// <summary>
        /// Gets the property values.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="dataSection">The data section.</param>
        /// <returns>Readonly list of property values.</returns>
        private static ReadOnlyCollection<string> GetPropertyValues(string propertyName, MetadataSection dataSection)
        {
            //Get the equivalent DC property for a resource property.
            string valueKey = PropertyMapper.ResourceProperties[propertyName];
            ReadOnlyCollection<string> values = null; // 

            // Get the values for the current DC property
            // Check if the current property is 'Rights' property
            if(SwordConstants.RightsProperty == valueKey)
            {
                values = (null != dataSection.AdministrativeMetadata) ?
                          dataSection.AdministrativeMetadata[valueKey] as ReadOnlyCollection<string> : null;
            }
            else
            {
                values = (null != dataSection.DescriptiveMetadata) ?
                          dataSection.DescriptiveMetadata[valueKey] as ReadOnlyCollection<string> : null;
            }
            return values;
        }

        /// <summary>
        /// Updates the navigation properties.
        /// </summary>
        /// <param name="zentityContext">The zentity context.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="property">The property.</param>
        /// <param name="values">The values.</param>
        private static void UpdateNavigationproperties(ZentityContext zentityContext, ScholarlyWork resource, PropertyInfo property, ReadOnlyCollection<string> values)
        {
            EntityCollection<Contact> contactRelation = property.GetValue(resource, null) as EntityCollection<Contact>;

            if(null != contactRelation)
            {

                contactRelation.Clear();
                var authors = values.Distinct().Where(name => name.Length > 0)
                                      .Select(name => new Person
                                      {
                                          Title = name
                                      });
                ZentityAtomPubStoreWriter.AddPersons(zentityContext, contactRelation, authors);
            }
        }

        /// <summary>
        /// Updates the scalar properties.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="property">The property.</param>
        /// <param name="values">The values.</param>
        private static void UpdateScalarProperties(ScholarlyWork resource, PropertyInfo property, ReadOnlyCollection<string> values)
        {
            Type propType = property.PropertyType;
            object scalarValue = null;
            // Change the property value type from string to proper resource type.
            if(propType.ToString() == SwordConstants.TypeNullableDateTime)
            {
                scalarValue = Convert.ChangeType(values[0],
                                typeof(DateTime),
                                CultureInfo.InvariantCulture);
            }
            else
            {
                scalarValue = Convert.ChangeType(values[0],
                                property.PropertyType,
                                CultureInfo.InvariantCulture);
            }

            // Update the property value.
            property.SetValue(resource, scalarValue, null);
        }

        #endregion

    }
}
