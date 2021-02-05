// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System.Net;
using System.Web;
using Zentity.Platform.Properties;

namespace Zentity.Platform
{
    /// <summary>
    /// Processes AtomPub DELETE requests.
    /// </summary>
    public class AtomPubDeleteProcessor : HttpDeleteProcessor
    {
        #region .ctor

        /// <summary>
        /// Create new instance of Zentity.Platform.AtomPubDeleteProcessor class.
        /// </summary>
        public AtomPubDeleteProcessor()
            : this(AtomPubHelper.GetBaseUri())
        {
        }

        /// <summary>
        /// Create new instance of Zentity.Platform.AtomPubDeleteProcessor class.
        /// </summary>
        /// <param name="baseAddress">Base Uri to process the HTTP request.</param>
        internal AtomPubDeleteProcessor(string baseAddress)
            : base(baseAddress)
        {
        }

        #endregion

        #region Public Methods

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
        public override string ProcessRequest(HttpContext context, out System.Net.HttpStatusCode statusCode)
        {
            string response = string.Empty;
            bool isDeleted;
            AtomPubRequestType requestType = AtomPubHelper.GetAtomPubRequestType(context, base.BaseUri);

            try
            {
                switch(requestType)
                {
                    case AtomPubRequestType.EditMember:
                        isDeleted = DeleteMember(context);
                        break;

                    case AtomPubRequestType.EditMedia:
                        isDeleted = DeleteMedia(context);
                        break;

                    case AtomPubRequestType.ServiceDocument:
                    case AtomPubRequestType.Collection:
                    case AtomPubRequestType.CollectionWithPageNo:
                    case AtomPubRequestType.Unknwon:
                    default:
                        isDeleted = false;
                        response = Resources.ATOMPUB_INVALID_URL;
                        break;
                }
            }
            catch(ResourceNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                return Properties.Resources.ATOMPUB_RESOURCE_NOT_FOUND;
            }

            if(isDeleted)
            {
                statusCode = HttpStatusCode.OK;
            }
            else
            {
                statusCode = HttpStatusCode.BadRequest;
            }

            return response;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Deletes a member resource using the information present in the HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext for the incoming HTTP request.</param>
        /// <returns>True if the operation succeeds, False otherwise.</returns>
        private bool DeleteMember(HttpContext context)
        {
            // Gets the atom request template match result to get requested collection name.
            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.CollectionName);
            string memberId = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.Id);

            IAtomPubStoreWriter atomPubStoreWriter = AtomPubStoreFactory.GetAtomPubStoreWriter(base.BaseUri.OriginalString);

            return atomPubStoreWriter.DeleteMember(collectionName, memberId);
        }

        /// <summary>
        /// Deletes a media resource using the information present in the HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext for the incoming HTTP request.</param>
        /// <returns>True if the operation succeeds, False otherwise.</returns>
        private bool DeleteMedia(HttpContext context)
        {
            // Gets the atom request template match result to get requested collection name.
            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.CollectionName);
            string memberId = AtomPubHelper.GetValueOfParameterFromUri(context, base.BaseUri, AtomPubParameterType.Id);

            IAtomPubStoreWriter atomPubStoreWriter = AtomPubStoreFactory.GetAtomPubStoreWriter(base.BaseUri.OriginalString);

            return atomPubStoreWriter.DeleteMedia(collectionName, memberId);
        }

        #endregion
    }
}
