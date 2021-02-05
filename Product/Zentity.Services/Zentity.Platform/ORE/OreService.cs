// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Web;
    using Zentity.Core;
    using Zentity.Platform.Properties;

    #region OreService Class

    /// <summary>
    /// This class is responsible to handle Ore requests
    /// </summary>
    public class OreService : IZentityService
    {
        #region Public Methods

        /// <summary>
        /// This method validates the request sent to Ore service
        /// </summary>
        /// <param name="context">HttpContext object containing the request information.</param>
        /// <param name="errorMessage">error message to display if validation fails</param>
        /// <returns>true,if request is valid else false</returns>
        public bool ValidateRequest(HttpContext context, out string errorMessage)
        {
            Guid id = ExtractGuidFromRequestUri(GetBaseUri(), context.Request.Url);

            if(Guid.Empty != id)
            {
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                errorMessage = Resources.ORE_RESOURCE_NOT_FOUND;
                return false;
            }
        }

        /// <summary>
        /// This method processes the request sent to the Ore service.
        /// </summary>
        /// <param name="context">HttpContext object containing the request information.</param>
        /// <param name="statusCode">Out parameter returns the status of the request.</param>
        /// <returns>Response in string format.</returns>
        public string ProcessRequest(HttpContext context, out System.Net.HttpStatusCode statusCode)
        {

            string response = string.Empty;
            try
            {
                Guid id = ExtractGuidFromRequestUri(GetBaseUri(), context.Request.Url);
                using(ZentityContext zentityContext = CoreHelper.CreateZentityContext())
                {
                    ResourceMap<Guid> resourceMap = new ResourceMap<Guid>(id);
                    SerializeRDFXML<Guid> serializer = new SerializeRDFXML<Guid>(resourceMap.CreateMemento());
                    string deployedAt = GetBaseUri().AbsoluteUri;
                    response = serializer.Serialize(deployedAt);
                    context.Response.ContentType = GetContentType();
                    statusCode = HttpStatusCode.OK;
                }
            }
            catch(ArgumentNullException)
            {
                statusCode = HttpStatusCode.NotFound;
                response = Properties.Resources.ORE_RESOURCE_NOT_FOUND;

            }

            return response;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the base Uri of the request
        /// </summary>
        /// <returns>base uri of request</returns>
        internal static Uri GetBaseUri()
        {
            return new Uri(ConfigurationManager.AppSettings["ServiceHost"].ToString() +
                ConfigurationManager.AppSettings["OreBaseUri"].ToString());
        }

        /// <summary>
        /// Extracts the Guid from the request URI.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <returns>The extracted Guid.</returns>
        internal static Guid ExtractGuidFromRequestUri(Uri baseUri, Uri requestUri)
        {
            UriTemplate template = new UriTemplate("{guid}.rdf");
            UriTemplateMatch results = template.Match(baseUri, requestUri);

            if(results != null)
            {
                return new Guid(results.BoundVariables["guid"]);
            }
            else
            {
                return Guid.Empty;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the response content type
        /// </summary>
        /// <returns>The content type</returns>
        private static string GetContentType()
        {
            return Resources.ORE_CONTENTTYPE;
        }
        #endregion
    }
    #endregion
}
