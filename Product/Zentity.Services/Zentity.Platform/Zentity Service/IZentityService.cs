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
    /// This class represents the interface for Zentity Services.
    /// </summary>
    public interface IZentityService
    {
        /// <summary>
        /// Validates the specified request.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="errorMessage">An error message describing the reason if validation is failed.</param>
        /// <returns>True if the request is valid, otherwise false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        bool ValidateRequest(HttpContext context, out string errorMessage);

        /// <summary>
        /// Processes the request sent to the specified Uri. 
        /// This method assumes that the request is already validated.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="statusCode">The HttpStatusCode indicating status of the request.</param>
        /// <returns>A string containing the response to the request.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        string ProcessRequest(HttpContext context, out HttpStatusCode statusCode);

    }
}
