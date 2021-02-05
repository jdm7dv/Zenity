// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Globalization;
    using System.Web;

    /// <summary>
    /// This class is responsible for returning the appropriate Service instance 
    /// for the given HTTP request.
    /// </summary>
    public static class ServiceFactory
    {
        private const string DoubleArgsURI = "{{{0}}}/{{{1}}}/*";
        private const string SingleArgURI = "{{{0}}}/*";

        private const string AtomPub = "ATOMPUB";
        private const string Sword = "SWORD";
        private const string Ore = "OAIORE";
        private const string Pmh = "OAIPMH";
        private const string Syndication = "SYNDICATION";

        /// <summary>
        /// Gets an instance of ZentityService which can handle the specified Http request.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing request details.</param>
        /// <returns>An instance of ZentityService responsible for handling the specified request.</returns>
        /// <remarks>Returns null, if the ServiceFactory could not find any service 
        /// which can handle the give request.</remarks>
        public static IZentityService GetServiceInstance(HttpContext context)
        {
            IZentityService service = null;

            string serviceType = GetServiceType(context.Request.Url);

            switch(serviceType)
            {
                case AtomPub:
                    service = new AtomPubService();
                    break;
                case Sword:
                    service = new SwordService();
                    break;
                case Pmh:
                    service = new OaiPmhService();
                    break;
                case Ore:
                    service = new OreService();
                    break;
                case Syndication:
                    service = new SyndicationService();
                    break;
                default:
                    service = null;
                    break;
            }

            return service;
        }

        /// <summary>
        /// Returns the type of the service e.g. AtomPub, Sword or ORE.
        /// </summary>
        /// <param name="contextUrl">The context URL.</param>
        /// <returns>The service type.</returns>
        private static string GetServiceType(Uri contextUrl)
        {
            const string ServiceType = "serviceType";
            const string SubServiceType = "subServiceType";
            Uri baseUri = new Uri(PlatformConstants.GetServiceHostName());

            UriTemplate doubleArgTemplate = new UriTemplate(string.Format(CultureInfo.InvariantCulture,
                                                        DoubleArgsURI, ServiceType, SubServiceType));

            UriTemplateMatch doubleArgMatches = doubleArgTemplate.Match(baseUri, contextUrl);
            if(doubleArgMatches != null) // Url matches with double template
            {
                string requestedService = doubleArgMatches.BoundVariables[ServiceType].ToUpperInvariant();
                string subRequestedService = doubleArgMatches.BoundVariables[SubServiceType].ToUpperInvariant();

                if((Sword == subRequestedService && AtomPub == requestedService) //is this required?
                    || Ore == subRequestedService)
                {
                    return subRequestedService;
                }
                else
                    return doubleArgMatches.BoundVariables[ServiceType].ToUpperInvariant();
            }
            else // If Url doesn't match with double template, check for single template.
            {
                UriTemplate singleArgTemplate = new UriTemplate(string.Format(CultureInfo.InvariantCulture,
                                                                            SingleArgURI, ServiceType));
                UriTemplateMatch singleArgMatch = singleArgTemplate.Match(baseUri, contextUrl);
                //Single argument match for ATOM PUB
                if(null != singleArgMatch)
                {
                    return singleArgMatch.BoundVariables[ServiceType].ToUpperInvariant();
                }
            }

            return null;
        }
    }
}
