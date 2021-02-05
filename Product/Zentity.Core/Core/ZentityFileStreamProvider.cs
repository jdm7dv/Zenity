// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Data.Services;
using System.Data.Services.Providers;
using System.IO;
using System.Linq;
using System.Net;

namespace Zentity.Core
{
    /// <summary>
    /// Provides the core functionality to read and write File entity content to various clients.
    /// </summary>
    public class ZentityFileStreamProvider : IDataServiceStreamProvider
    {
        /// <summary>
        /// Default content type for any unknown MIME format
        /// </summary>
        public const string DefaultContentType = "application/octet-stream";

        /// <summary>
        /// An instance of ZentityContext class to do database operations on.
        /// </summary>
        private readonly ZentityContext zentityContext;

        /// <summary>
        /// A temporary file created for Write Stream.
        /// </summary>
        private readonly string tempFileName = Path.GetTempFileName();

        /// <summary>
        /// A local copy of the File resource.
        /// </summary>
        private FlattenedFile fileResource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityFileStreamProvider"/> class.
        /// </summary>
        /// <param name="context">The instance of the ZentityContext used by the data service.</param>
        public ZentityFileStreamProvider(ZentityContext context)
        {
            this.zentityContext = context;
        }

        /// <summary>
        /// Saves the stream to database.
        /// </summary>
        /// <param name="src">The source object.</param>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void SaveStreamToDatabase(object src, EventArgs args)
        {
            if (fileResource == null || fileResource.Id == Guid.Empty)
            {
                throw new DataServiceException((int) HttpStatusCode.InternalServerError, CoreResources.FileIdIsGuidEmpty);
            }

            try
            {
                // Convert the FlattenedFile resource into a File resource
                File zenFileResource = this.zentityContext.Files.Where<File>(zenFile => zenFile.Id == fileResource.Id).FirstOrDefault();

                if (zenFileResource == null)
                {
                    throw new DataServiceException((int) HttpStatusCode.NotFound, CoreResources.FileNotFound);
                }

                using (FileStream fileStream = new FileStream(tempFileName, FileMode.Open, FileAccess.Read))
                {
                    this.zentityContext.UploadFileContent(zenFileResource, fileStream);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof (DataServiceException))
                {
                    throw new DataServiceException((int) HttpStatusCode.InternalServerError, string.Format("{0} : {1}", CoreResources.FileCannotBeSaved, ex));
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                // Clean-up the temp file.
                System.IO.File.Delete(tempFileName);
            }
        }

        #region IDataServiceStreamProvider Members

        /// <summary>
        /// This method is invoked by the data services framework to obtain metadata about the stream associated with the specified <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity for which the descriptor object should be returned.</param>
        /// <param name="operationContext">The <see cref="T:System.Data.Services.DataServiceOperationContext"/> instance  that processes the request.</param>
        /// <exception cref="T:System.ArgumentNullException">When <paramref name="entity"/> or <paramref name="operationContext"/> are null.</exception>
        /// <exception cref="T:System.ArgumentException">When <paramref name="entity"/> is not an entity that has a binary property to stream.</exception>
        /// <exception cref="T:System.Data.Services.DataServiceException">When the stream associated with the <paramref name="entity"/> cannot be deleted.</exception>
        public void DeleteStream(object entity, DataServiceOperationContext operationContext)
        {
            return;
        }

        /// <summary>
        /// Returns the default stream that is associated with an entity that has a binary property.
        /// </summary>
        /// <param name="entity">The entity that has the associated binary stream.</param>
        /// <param name="etag">The eTag value sent as part of the HTTP request that is sent to the data service.</param>
        /// <param name="checkETagForEquality">A nullable <see cref="T:System.Boolean"/> value that determines the type of eTag that is used.</param>
        /// <param name="operationContext">The <see cref="T:System.Data.Services.DataServiceOperationContext"/> instance used by the data service to process the request.</param>
        /// <returns>
        /// The data <see cref="T:System.IO.Stream"/> that contains the binary property data of the <paramref name="entity"/>.
        /// </returns>
        public Stream GetReadStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext)
        {
            if (checkETagForEquality != null)
            {
                // This implementation does not support eTag associated with BLOBs
                throw new DataServiceException((int) HttpStatusCode.NotImplemented, CoreResources.FileETagsNotSupported);
            }

            // Set the file resource instance.
            fileResource = entity as FlattenedFile;

            if (fileResource == null)
            {
                throw new DataServiceException((int) HttpStatusCode.NotFound, CoreResources.FileNotFound);
            }

            try
            {
                // Convert the FlattenedFile resource into a File resource
                File zenFileResource = this.zentityContext.Files.Where<File>(zenFile => zenFile.Id == fileResource.Id).FirstOrDefault();
                
                if (zenFileResource == null)
                {
                    throw new DataServiceException((int) HttpStatusCode.NotFound, CoreResources.FileNotFound);
                }

                // Create a memory stream for File resource content download
                using (MemoryStream memStream = new MemoryStream())
                {
                    // Download the File content from SQL Server
                    this.zentityContext.DownloadFileContent(zenFileResource, memStream);

                    // Reset the position to the start for reading operations.
                    memStream.Seek(0, SeekOrigin.Begin);

                    return memStream;
                }
            }
            catch (Exception ex)
            {
                throw new DataServiceException((int) HttpStatusCode.InternalServerError, 
                    string.Format(CoreResources.FileContentNotStreamed, fileResource.Id, ex.Message));
            }
        }

        /// <summary>
        /// Returns the URI that is used to request the data stream that is associated with the binary property of an entity.
        /// </summary>
        /// <param name="entity">The entity that has the associated binary data stream.</param>
        /// <param name="operationContext">The <see cref="T:System.Data.Services.DataServiceOperationContext"/> instance used by the data service to process the request.</param>
        /// <returns>
        /// A <see cref="T:System.Uri"/> value that is used to request the binary data stream.
        /// </returns>
        public Uri GetReadStreamUri(object entity, DataServiceOperationContext operationContext)
        {
            return null;
        }

        /// <summary>
        /// Returns the content type of the stream that is associated with the specified entity.
        /// </summary>
        /// <param name="entity">The entity that has the associated binary data stream.</param>
        /// <param name="operationContext">The <see cref="T:System.Data.Services.DataServiceOperationContext"/> instance used by the data service to process the request.</param>
        /// <returns>A valid Content-Type of the binary data.</returns>
        public string GetStreamContentType(object entity, DataServiceOperationContext operationContext)
        {
            // Check if the entity is a File resource
            if (entity != null && entity is FlattenedFile)
            {
                FlattenedFile fileRes = entity as FlattenedFile;

                // Check if the File resource holds a MIME content type.
                if (!string.IsNullOrWhiteSpace(fileRes.MimeType))
                {
                    return fileRes.MimeType;
                }
            }

            return ZentityFileStreamProvider.DefaultContentType;
        }

        /// <summary>
        /// Returns the eTag of the data stream that is associated with the specified entity.
        /// </summary>
        /// <param name="entity">The entity that has the associated binary data stream.</param>
        /// <param name="operationContext">The <see cref="T:System.Data.Services.DataServiceOperationContext"/> instance used by the data service to process the request.</param>
        /// <returns>
        /// eTag of the stream associated with the <paramref name="entity"/>.
        /// </returns>
        public string GetStreamETag(object entity, DataServiceOperationContext operationContext)
        {
            return null;
        }

        /// <summary>
        /// Returns the stream that the data service uses to write the contents of a binary property that is associated with an entity.
        /// </summary>
        /// <param name="entity">The entity that has the associated binary stream.</param>
        /// <param name="etag">The eTag value that is sent as part of the HTTP request that is sent to the data service.</param>
        /// <param name="checkETagForEquality">A nullable <see cref="T:System.Boolean"/> value that determines the type of eTag is used.</param>
        /// <param name="operationContext">The <see cref="T:System.Data.Services.DataServiceOperationContext"/> instance that is used by the data service to process the request.</param>
        /// <returns>
        /// A valid <see cref="T:System.Stream"/> the data service uses to write the contents of a binary property that is associated with the <paramref name="entity"/>.
        /// </returns>
        public Stream GetWriteStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext)
        {
            if (checkETagForEquality != null)
            {
                // This implementation does not support eTag associated with BLOBs
                throw new DataServiceException((int) HttpStatusCode.NotImplemented, CoreResources.FileETagsNotSupported);
            }

            // Set the file resource instance.
            fileResource = entity as FlattenedFile;

            if (fileResource == null)
            {
                throw new DataServiceException((int) HttpStatusCode.NotFound, CoreResources.FileNotFound);
            }

            string slugHeader = operationContext.RequestHeaders.Get(CoreResources.HttpHeaderSlug);
            if (string.IsNullOrWhiteSpace(slugHeader))
            {
                throw new DataServiceException((int) HttpStatusCode.BadRequest, CoreResources.FileIdNotSent);
            }
            
            Guid fileResourceId = Guid.Empty;
            if (Guid.TryParse(slugHeader, out fileResourceId))
            {
                if (fileResourceId == Guid.Empty)
                {
                    throw new DataServiceException((int) HttpStatusCode.BadRequest, CoreResources.FileIdIsGuidEmpty);
                }
                    
                fileResource.Id = fileResourceId;
            }
            else
            {
                throw new DataServiceException((int) HttpStatusCode.BadRequest, CoreResources.FileIdIsInvalid);
            }

            // Return the filestream that the data service uses to 
            // write the requested binary data stream to a temp file.
            return new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Returns a namespace-qualified type name that represents the type that the data service runtime must create for the Media Link Entry that is associated with the data stream for the Media Resource that is being inserted.
        /// </summary>
        /// <param name="entitySetName">Fully-qualified entity set name.</param>
        /// <param name="operationContext">The <see cref="T:System.Data.Services.DataServiceOperationContext"/> instance that is used by the data service to process the request.</param>
        /// <returns>A namespace-qualified type name.</returns>
        public string ResolveType(string entitySetName, DataServiceOperationContext operationContext)
        {
            // We should only be handling Employee types.
            if (entitySetName == DataModellingResources.FlattenedFiles)
            {
                return typeof(FlattenedFile).FullName;
            }
            
            // This will raise an DataServiceException.
            return null;
        }

        /// <summary>
        /// Gets the size of the stream buffer.
        /// </summary>
        /// <value></value>
        /// <returns>Integer that represents the size of buffer.</returns>
        public int StreamBufferSize
        {
            get { return 64000; }
        }

        #endregion
    }
}
