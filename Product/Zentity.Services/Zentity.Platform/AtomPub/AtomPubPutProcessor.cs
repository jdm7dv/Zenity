// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.IO;
    using System.Net;
    using System.ServiceModel.Syndication;
    using System.Web;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Processes AtomPub PUT requests.
    /// </summary>
    public class AtomPubPutProcessor : HttpPutProcessor
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomPubPutProcessor"/> class.
        /// </summary>
        public AtomPubPutProcessor()
            : this(AtomPubHelper.GetBaseUri())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomPubPutProcessor"/> class.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        internal AtomPubPutProcessor(string baseAddress)
            : base(baseAddress)
        {
        }

        #endregion

        /// <summary>
        /// Validates the incoming request.
        /// </summary>
        /// <param name="context">The HttpContext for the incoming HTTP request.</param>        
        /// <param name="errorMessage">The HTTP Status Code if the request is invalid.</param>
        /// <returns>True if the request is valid, False otherwise.</returns>
        public override bool ValidateRequest(HttpContext context, out string errorMessage)
        {
            // Verify that type of request is edit member or edit media request.
            AtomPubRequestType requestType = AtomPubHelper.GetAtomPubRequestType(context, base.BaseUri);

            if(!(AtomPubRequestType.EditMember == requestType || AtomPubRequestType.EditMedia == requestType))
            {
                errorMessage = Resources.ATOMPUB_INVALID_URL;
                return false;
            }

            bool isAtomEntryType = AtomPubHelper.IsAtomEntryMediaType(context.Request.ContentType);

            if(requestType == AtomPubRequestType.EditMember && !isAtomEntryType)
            {
                errorMessage = Resources.ATOMPUB_UNSUPPORTED_CONTENT_TYPE;
                return false;
            }

            // Get the requested member type and its id for verifying it existents.
            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.CollectionName);

            if(!string.IsNullOrEmpty(collectionName)
                         && AtomPubHelper.IsValidCollectionType(collectionName))
            {
                string memberId = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.Id);

                if(!string.IsNullOrEmpty(memberId) && AtomPubHelper.IsValidGuid(memberId))
                {
                    errorMessage = string.Empty;
                    return true;
                }
                else
                {
                    errorMessage = Resources.ATOMPUB_INVALID_RESOURCE_ID;
                    return false;
                }
            }
            else
            {
                errorMessage = Resources.ATOMPUB_UNSUPPORTED_COLLECTION_NAME;
                return false;
            }
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="context">The HttpContext for the incoming HTTP request.</param>
        /// <param name="statusCode">The HTTP Status Code that must be returned to the client.</param>
        /// <returns>The response that must be sent to the client.</returns>
        public override string ProcessRequest(HttpContext context, out HttpStatusCode statusCode)
        {
            if(!AtomPubHelper.ValidatePrecondition(context, base.BaseUri, out statusCode))
            {
                if(HttpStatusCode.PreconditionFailed == statusCode)
                {
                    return Properties.Resources.ATOMPUB_PRECONDITION_FAILED;
                }
                else
                {
                    return Properties.Resources.ATOMPUB_RESOURCE_NOT_MODIFIED;
                }
            }

            string errorMessage = string.Empty;
            // used to hold updated member metadata.
            AtomEntryDocument atomDocument = null;
            AtomPubRequestType requestType = AtomPubHelper.GetAtomPubRequestType(context, base.BaseUri);

            try
            {
                switch(requestType)
                {
                    case AtomPubRequestType.EditMember:
                        atomDocument = UpdateMember(context);
                        break;

                    case AtomPubRequestType.EditMedia:
                        atomDocument = UpdateMedia(context);
                        break;

                    case AtomPubRequestType.ServiceDocument:
                    case AtomPubRequestType.Collection:
                    case AtomPubRequestType.CollectionWithPageNo:
                    case AtomPubRequestType.Unknwon:
                    default:
                        statusCode = HttpStatusCode.BadRequest;
                        errorMessage = Resources.ATOMPUB_INVALID_URL;
                        break;
                }
            }
            catch(ResourceNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                return Properties.Resources.ATOMPUB_RESOURCE_NOT_FOUND;
            }

            if(null != atomDocument)
            {
                statusCode = HttpStatusCode.OK;
                string memberId = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.Id);
                string eTag = AtomPubHelper.CalculateETag(memberId);
                context.Response.AddHeader(AtomPubConstants.KeyETag, eTag);
                return atomDocument.AtomEntry;
            }
            else
            {
                return errorMessage;
            }
        }

        /// <summary>
        /// Updates a member resource using the information present in the HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext for the incoming HTTP request.</param>
        /// <returns>An AtomEntryDocument corresponding to the updated member resource.</returns>
        private AtomEntryDocument UpdateMember(HttpContext context)
        {
            // Gets the atom request template match result to get requested collection name.
            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.CollectionName);
            string memberId = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.Id);

            IAtomPubStoreWriter atomPubStoreWriter = AtomPubStoreFactory.GetAtomPubStoreWriter(base.BaseUri.OriginalString);

            //Get AtomEntryDocument for request to update the member information.
            AtomEntryDocument atomDocument = new AtomEntryDocument(context.Request.InputStream);

            SyndicationItem item = atomPubStoreWriter.UpdateMemberInfo(collectionName, memberId, atomDocument);
            // Get AtomEntryDocument for response.            
            return new AtomEntryDocument(item);
        }

        /// <summary>
        /// Updates a media resource using the information present in the HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext for the incoming HTTP request.</param>
        /// <returns>An AtomEntryDocument corresponding to the updated media resource.</returns>
        private AtomEntryDocument UpdateMedia(HttpContext context)
        {
            // Gets the atom request template match result to get requested collection name.
            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.CollectionName);
            string memberId = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.Id);

            IAtomPubStoreWriter atomPubStoreWriter = AtomPubStoreFactory.GetAtomPubStoreWriter(base.BaseUri.OriginalString);

            // Get byte array to update media.
            BinaryReader reader = new BinaryReader(context.Request.InputStream);
            byte[] media = new byte[context.Request.InputStream.Length];
            reader.Read(media, 0, media.Length);

            SyndicationItem item = atomPubStoreWriter.UpdateMedia(collectionName, memberId, context.Request.ContentType, media);
            // Get AtomEntryDocument for response.            
            return new AtomEntryDocument(item);
        }
    }
}
