// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System.Net;
using System.Web;

namespace Zentity.Platform
{
    /// <summary>
    /// The class responsible for handling Http POST request.
    /// </summary>
    public abstract class HttpPostProcessor : HttpRequestProcessor
    {
        /// <summary>
        /// Create new instance of Zentity.Platform.HttpPostProcessor class.
        /// </summary>
        protected HttpPostProcessor()
        {
        }

        /// <summary>
        /// Create new instance of Zentity.Platform.HttpPostProcessor class.
        /// </summary>
        /// <param name="baseAddress">Base Uri to process the HTTP request.</param>
        internal HttpPostProcessor(string baseAddress)
            : base(baseAddress)
        {
        }

        /// <summary>
        /// Validates the specified POST request.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="errorMessage">An error message describing the reason if validation is failed.</param>
        /// <returns>True if the request is valid, otherwise false.</returns>
        public abstract override bool ValidateRequest(HttpContext context, out string errorMessage);

        /// <summary>
        /// Processes the POST request sent to the specified Uri. 
        /// This method assumes that the request is already validated.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="statusCode">The HttpStatusCode indicating status of the request.</param>
        /// <returns>A string containing the response to the request.</returns>
        public abstract override string ProcessRequest(HttpContext context, out HttpStatusCode statusCode);
    }
}
