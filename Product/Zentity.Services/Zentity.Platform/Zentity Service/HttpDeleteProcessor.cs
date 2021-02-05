// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Net;
    using System.Web;

    /// <summary>
    /// The class responsible for handling Http Delete request.
    /// </summary>
    public abstract class HttpDeleteProcessor : HttpRequestProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeleteProcessor"/> class.
        /// </summary>
        protected HttpDeleteProcessor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeleteProcessor"/> class.
        /// </summary>
        /// <param name="baseAddress">Base Uri to process the HTTP request.</param>
        internal HttpDeleteProcessor(string baseAddress)
            : base(baseAddress)
        {
        }

        /// <summary>
        /// Validates the specified DELETE request.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="errorMessage">An error message describing the reason if validation is failed.</param>
        /// <returns>True if the request is valid, otherwise false.</returns>
        public abstract override bool ValidateRequest(HttpContext context, out string errorMessage);

        /// <summary>
        /// Processes the DELETE request sent to the specified Uri. 
        /// This method assumes that the request is already validated.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="statusCode">The HttpStatusCode indicating status of the request.</param>
        /// <returns>A string containing the response to the request.</returns>
        public abstract override string ProcessRequest(HttpContext context, out HttpStatusCode statusCode);
    }
}
