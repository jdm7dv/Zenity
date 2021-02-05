// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Net;
    using System.Web;

    /// <summary>
    /// The abstract class responsible for handling any Http request.
    /// </summary>
    public abstract class HttpRequestProcessor
    {
        #region Fields

        private Uri baseUri;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestProcessor"/> class.
        /// </summary>
        protected HttpRequestProcessor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestProcessor"/> class.
        /// </summary>
        /// <param name="baseAddress">Base Uri to process the HTTP request.</param>
        internal HttpRequestProcessor(string baseAddress)
        {
            baseUri = new Uri(baseAddress);
        }

        #region Properties

        /// <summary>
        /// Gets the base Uri to process the HTTP request. 
        /// </summary>
        protected Uri BaseUri
        {
            get
            {
                return baseUri;
            }
        }

        #endregion

        /// <summary>
        /// Validates the specified Http request.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="errorMessage">An error message describing the reason if validation is failed.</param>
        /// <returns>True if the request is valid, otherwise false.</returns>
        public abstract bool ValidateRequest(HttpContext context, out string errorMessage);

        /// <summary>
        /// Processes the request sent to the specified Uri. 
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="statusCode">The HttpStatusCode indicating status of the request.</param>
        /// <returns>A string containing the response to the request.</returns>
        /// <remarks>This method assumes that the request is already validated.</remarks>
        public abstract string ProcessRequest(HttpContext context, out HttpStatusCode statusCode);
    }
}
