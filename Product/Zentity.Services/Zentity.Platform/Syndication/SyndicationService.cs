// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Syndication;
    using System.Web;
    using Zentity.Core;
    using Zentity.Platform.Properties;
    using Zentity.Security.Authentication;

    class SyndicationService : IZentityService
    {
        private Uri baseUri = SyndicationHelper.GetBaseUri();

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationService"/> class.
        /// </summary>
        public SyndicationService()
        {
        }
        #endregion

        /// <summary>
        /// Validates the specified request.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="errorMessage">An error message describing the reason if validation is failed.</param>
        /// <returns>
        /// True if the request is valid, otherwise false.
        /// </returns>
        public bool ValidateRequest(HttpContext context, out string errorMessage)
        {
            errorMessage = string.Empty;
            string httpRequestType = context.Request.RequestType.ToUpperInvariant();

            // Verify request type is GET
            if(!(PlatformConstants.GetRequestType.Equals(httpRequestType)))
            {
                errorMessage = Resources.SYNDICATION_INVALID_METHOD;
                return false;
            }

            SyndicationRequestType requestType = SyndicationHelper.GetSyndicationRequestType(context, this.baseUri);
            if(SyndicationRequestType.Unknown == requestType)
            {
                errorMessage = Resources.SYNDICATION_BAD_REQUEST;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Processes the request sent to the specified Uri.
        /// This method assumes that the request is already validated.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing the request details.</param>
        /// <param name="statusCode">The HttpStatusCode indicating status of the request.</param>
        /// <returns>
        /// A string containing the response to the request.
        /// </returns>
        public string ProcessRequest(HttpContext context, out HttpStatusCode statusCode)
        {
            string response = string.Empty;
            SyndicationRequestType requestType = SyndicationHelper.GetSyndicationRequestType(context, this.baseUri);
            if(SyndicationRequestType.Help == requestType)
            {
                statusCode = HttpStatusCode.OK;
                response = Properties.Resources.Help;
            }
            else
            {
                string searchQuery = HttpUtility.UrlDecode(context.Request.QueryString.ToString());
                int pageSize = int.Parse(ConfigurationManager.AppSettings["DefaultPageSize"], CultureInfo.InvariantCulture);
                int maxPageSize = int.Parse(ConfigurationManager.AppSettings["MaxPageSize"], CultureInfo.InvariantCulture);

                if(SyndicationRequestType.DefaultSearchWithPageNo == requestType ||
                    SyndicationRequestType.RSSSearchWithPageNo == requestType ||
                    SyndicationRequestType.ATOMSearchWithPageNo == requestType)
                {
                    pageSize = int.Parse(SyndicationHelper.GetValueOfParameterFromUri(context, this.baseUri,
                           requestType, SyndicationParameterType.PageSize), CultureInfo.InvariantCulture);
                    pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
                }

                AuthenticatedToken token = (AuthenticatedToken)context.Items["AuthenticatedToken"];
                List<Resource> resources;
                try
                {
                    SearchEngine searchEngin =
                        new SearchEngine(pageSize, true, token);
                    int totalRecords;
                    ZentityContext zentityContext = CoreHelper.CreateZentityContext();
                    SortProperty sortProperty =
                        new SortProperty("dateModified", SortDirection.Descending);

                    resources = searchEngin.SearchResources(searchQuery,
                        zentityContext, sortProperty, 0, out totalRecords).ToList();
                    ;
                }
                catch(SearchException exception)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    response = exception.Message;
                    return response;
                }
                SyndicationFeed feed = SyndicationHelper.CreateSyndicationFeed(resources, searchQuery, context.Request.Url);

                response = SyndicationHelper.GetResponseDocument(feed, requestType);

                statusCode = HttpStatusCode.OK;
            }
            return response;

        }
    }
}
