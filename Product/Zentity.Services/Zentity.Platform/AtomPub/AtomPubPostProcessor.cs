// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Web;

namespace Zentity.Platform
{
    /// <summary>
    /// This class is responsible for handling AtomPub POST request.
    /// </summary>
    public class AtomPubPostProcessor : HttpPostProcessor
    {
        #region .ctor

        /// <summary>
        /// Create new instance of Zentity.Platform.AtomPubPostProcessor class.
        /// </summary>
        public AtomPubPostProcessor()
            : this(AtomPubHelper.GetBaseUri())
        {
        }

        /// <summary>
        /// Create new instance of Zentity.Platform.AtomPubPostProcessor class.
        /// </summary>
        /// <param name="baseAddress">Base Uri to process the HTTP request.</param>
        internal AtomPubPostProcessor(string baseAddress)
            : base(baseAddress)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validates the request uri and returns the appropriate error message. 
        /// If the request is valid, 'error' with empty string is returned.
        /// </summary>
        /// <param name="context">HttpContext containing the request object.</param>
        /// <param name="errorMessage">Contains the error message.</param>
        /// <returns>True if the request is valid, else false.</returns>
        public override bool ValidateRequest(HttpContext context, out string errorMessage)
        {
            bool isValid = false;

            AtomPubRequestType requestType = AtomPubHelper.GetAtomPubRequestType(context, base.BaseUri);

            switch(requestType)
            {
                case AtomPubRequestType.Collection:

                    // Get collection name
                    string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                          AtomPubParameterType.CollectionName);

                    // If valid collection name, then proceed.
                    if(!string.IsNullOrEmpty(collectionName)
                         && AtomPubHelper.IsValidCollectionType(collectionName))
                    {
                        errorMessage = string.Empty;
                        isValid = true;
                    }
                    else
                    {
                        errorMessage = Properties.Resources.ATOMPUB_UNSUPPORTED_COLLECTION_NAME;
                        isValid = false;
                    }

                    break;
                case AtomPubRequestType.Unknwon:
                default:
                    errorMessage = Properties.Resources.ATOMPUB_INVALID_URL;
                    isValid = false;
                    break;
            }

            return isValid;
        }

        /// <summary>
        /// Handles the POST request send to the collection Uri. The method assumes that the request is 
        /// already validated using ValidateRequest method.
        /// </summary>
        /// <param name="context">HttpContext containing the request object.</param>
        /// <param name="statusCode">returns the status of the request.</param>
        /// <returns>A string containing the response for the specified AtomPub request.</returns>
        public override string ProcessRequest(HttpContext context, out System.Net.HttpStatusCode statusCode)
        {
            string response = string.Empty;

            AtomEntryDocument atomEntryDocument = null;
            bool isAtomEntryType = AtomPubHelper.IsAtomEntryMediaType(context.Request.ContentType);

            if(isAtomEntryType)
            {
                // If request stream contains AtomEntryDocument, then client wants to create new Member.
                atomEntryDocument = this.CreateMember(context);
            }
            else
            {
                // If request stream contains something other than AtomEntryDocument, 
                // then client wants to create new member with Media.
                atomEntryDocument = this.CreateMedia(context);
            }

            // Add Location Header
            SyndicationLink editLink = atomEntryDocument.Links
                                                        .Where(link => link.RelationshipType == AtomPubConstants.Edit)
                                                        .FirstOrDefault();
            if(null != editLink)
            {
                context.Response.AddHeader(AtomPubConstants.KeyLocation, editLink.Uri.AbsoluteUri);
            }

            // Add ETag header
            string[] memberIds = atomEntryDocument.Id.Split(new string[] { AtomPubConstants.IdPrefix },
                                                            StringSplitOptions.RemoveEmptyEntries);
            if(0 < memberIds.Length)
            {
                string eTag = AtomPubHelper.CalculateETag(memberIds[0]);

                context.Response.AddHeader(AtomPubConstants.KeyETag, eTag);
            }

            response = atomEntryDocument.AtomEntry;
            statusCode = HttpStatusCode.Created;

            return response;
        }

        /// <summary>
        /// Creates a member resource using the information present in the HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext for the incoming HTTP request.</param>
        /// <returns>An AtomEntryDocument corresponding to the newly created member resource.</returns>
        private AtomEntryDocument CreateMember(HttpContext context)
        {
            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                    AtomPubParameterType.CollectionName);

            //Get AtomEntryDocument for request to update the member information.
            AtomEntryDocument atomEntry = new AtomEntryDocument(context.Request.InputStream);

            IAtomPubStoreWriter atomPubStoreWriter = AtomPubStoreFactory.GetAtomPubStoreWriter(base.BaseUri.OriginalString);
            SyndicationItem item = atomPubStoreWriter.CreateMember(collectionName, atomEntry);

            // Create atom entry document from syndication item.
            return new AtomEntryDocument(item);
        }

        /// <summary>
        /// Creates a media resource using the information present in the HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext for the incoming HTTP request.</param>
        /// <returns>An AtomEntryDocument corresponding to the newly created media resource.</returns>
        private AtomEntryDocument CreateMedia(HttpContext context)
        {
            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri,
                                                    AtomPubParameterType.CollectionName);

            string fileExtention = string.Empty;
            if (context.Request.Headers.AllKeys.Contains("Content-Disposition"))
            {
                fileExtention = AtomPubHelper.GetFileExtentionFromContentDisposition(
                    context.Request.Headers["Content-Disposition"]);
            }
            // Get byte array to update media.
            BinaryReader reader = new BinaryReader(context.Request.InputStream);
            byte[] media = new byte[context.Request.InputStream.Length];
            reader.Read(media, 0, media.Length);

            IAtomPubStoreWriter atomPubStoreWriter = AtomPubStoreFactory.GetAtomPubStoreWriter(base.BaseUri.OriginalString);
            SyndicationItem item = atomPubStoreWriter.CreateMedia(collectionName, context.Request.ContentType, media, fileExtention);

            // Create atom entry document from syndication item.
            return new AtomEntryDocument(item);
        }

        #endregion
    }
}
