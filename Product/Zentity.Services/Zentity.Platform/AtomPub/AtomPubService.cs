// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using Zentity.Platform.Properties;

namespace Zentity.Platform
{
    /// <summary>
    /// This class handles AtomPub GET, POST, PUT and DELETE requests.
    /// </summary>
    public class AtomPubService : IZentityService
    {
        #region .ctor

        /// <summary>
        /// Create new instance of Zentity.Platform.AtomPubService class.
        /// </summary>
        public AtomPubService()
        {
        }

        #endregion

        /// <summary>
        /// Validates the specified AtomPub request.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="errorMessage">An error message describing the reason if validation is failed.</param>
        /// <returns>True if the request is valid, otherwise false.</returns>
        public bool ValidateRequest(HttpContext context, out string errorMessage)
        {
            string httpRequestType = context.Request.RequestType.ToUpperInvariant();

            // Verify request type is GET, PUT, POST, DELETE
            if(!(PlatformConstants.GetRequestType == httpRequestType
                || PlatformConstants.PutRequestType == httpRequestType
                || PlatformConstants.PostRequestType == httpRequestType
                || PlatformConstants.DeleteRequestType == httpRequestType))
            {
                errorMessage = Resources.ATOMPUB_INVALID_METHOD;
                return false;
            }

            Uri baseUri = new Uri(AtomPubHelper.GetBaseUri());
            AtomPubRequestType requestType = AtomPubHelper.GetAtomPubRequestType(context, baseUri);
            bool isValidRequest = AtomPubRequestType.Unknwon != requestType;
            errorMessage = (isValidRequest) ? string.Empty : Resources.ATOMPUB_BAD_REQUEST;
            return isValidRequest;
        }

        /// <summary>
        /// Processes the request sent to the specified AtomPub Uri. 
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="statusCode">The HttpStatusCode indicating status of the request.</param>
        /// <returns>A string containing the response to the request.</returns>
        /// <remarks>This method assumes that the request is already validated.</remarks>
        public string ProcessRequest(HttpContext context, out HttpStatusCode statusCode)
        {
            var unknownContentHeaders = context.Request.Headers.AllKeys
                                                  .Intersect(AtomPubHelper.UnknownContentHeaders);

            if(0 < unknownContentHeaders.Count())
            {
                statusCode = HttpStatusCode.NotImplemented;
                return string.Format(CultureInfo.InvariantCulture,
                                    Resources.ATOMPUB_UNKNOWN_CONTENT_HEADER,
                                    unknownContentHeaders.First());
            }

            HttpRequestProcessor processor = null;

            switch(context.Request.RequestType)
            {
                case PlatformConstants.GetRequestType:
                    processor = new AtomPubGetProcessor();
                    break;
                case PlatformConstants.PostRequestType:
                    processor = new AtomPubPostProcessor();
                    break;
                case PlatformConstants.PutRequestType:
                    processor = new AtomPubPutProcessor();
                    break;
                case PlatformConstants.DeleteRequestType:
                    processor = new AtomPubDeleteProcessor();
                    break;
                default:
                    processor = null;
                    break;
            }

            if(null == processor)
            {
                statusCode = HttpStatusCode.NotImplemented;
                return Properties.Resources.ATOMPUB_UNSUPPORTED_REQUEST;
            }

            try
            {
                bool isValidrequest = false;
                string responseString = string.Empty;

                isValidrequest = processor.ValidateRequest(context, out responseString);

                if(isValidrequest)
                {
                    return processor.ProcessRequest(context, out statusCode);
                }
                else
                {
                    statusCode = HttpStatusCode.BadRequest;
                    return responseString;
                }
            }
            catch(UnauthorizedException ex)
            {
                statusCode = HttpStatusCode.Forbidden;
                return ex.Message;
            }
        }

    }
}
