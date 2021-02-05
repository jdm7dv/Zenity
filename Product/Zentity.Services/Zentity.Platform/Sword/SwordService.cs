// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Web;
    using Zentity.Platform.Properties;

    /// <summary>
    /// This class is responsible for validating and processing sword GET and POST requests.
    /// </summary>
    public class SwordService : IZentityService
    {
        #region .ctor

        /// <summary>
        /// Create new instance for Zentity.Platform.SwordService class.
        /// </summary>
        internal SwordService()
        {
        }

        #endregion

        /// <summary>
        /// Validates the specified Sword request.
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

            Uri baseUri = new Uri(SwordHelper.GetBaseUri());
            AtomPubRequestType requestType = AtomPubHelper.GetAtomPubRequestType(context, baseUri);
            bool isValidRequest = AtomPubRequestType.Unknwon != requestType;
            errorMessage = (isValidRequest) ? string.Empty : Resources.ATOMPUB_BAD_REQUEST;
            return isValidRequest;
        }

        /// <summary>
        /// Processes the GET and POST requests sent to the Sword service.
        /// </summary>
        /// <param name="context">HttpContext object containing the request information.</param>
        /// <param name="statusCode">Out parameter returns the status of the request.</param>
        /// <returns>Response in string format.</returns>
        public string ProcessRequest(HttpContext context, out System.Net.HttpStatusCode statusCode)
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
                    processor = new SwordGetProcessor();
                    break;
                case PlatformConstants.PostRequestType:
                    processor = new SwordPostProcessor();
                    break;
                case PlatformConstants.PutRequestType:
                    processor = new AtomPubPutProcessor(SwordHelper.GetBaseUri());
                    break;
                case PlatformConstants.DeleteRequestType:
                    processor = new AtomPubDeleteProcessor(SwordHelper.GetBaseUri());
                    break;
                default:
                    processor = null;
                    break;
            }

            if(null == processor)
            {
                statusCode = HttpStatusCode.NotImplemented;
                return Resources.SWORD_UNSUPPORTED_REQUEST_TYPE;
            }

            try
            {
                string responseString = string.Empty;
                // Check if it is a valid request
                if(processor.ValidateRequest(context, out responseString))
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
