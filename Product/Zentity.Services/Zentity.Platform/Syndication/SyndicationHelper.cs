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
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Web;
    using System.Xml;
    using Zentity.Core;

    class SyndicationHelper
    {
        #region Private members
        private static Dictionary<SyndicationRequestType, UriTemplate> syndicationTemplates;
        #endregion

        #region internal properties
        /// <summary>
        /// Gets the list of all syndication templates supported.
        /// </summary>
        internal static Dictionary<SyndicationRequestType, UriTemplate> SyndicationTemplates
        {
            get
            {
                if(null == syndicationTemplates || 0 == syndicationTemplates.Count)
                {
                    LoadUriTemplates();
                }
                return syndicationTemplates;
            }
        }
        #endregion

        #region internal methods

        /// <summary>
        /// Gets the base uri from the configuration file.
        /// </summary>
        /// <returns>Base uri for the service.</returns>
        internal static Uri GetBaseUri()
        {
            string baseAddress = ConfigurationManager.AppSettings["SyndicationBaseUri"];
            return new Uri(PlatformConstants.GetServiceHostName() + baseAddress);
        }

        /// <summary>
        /// Gets the value of specified parameter from the request Uri.
        /// </summary>
        /// <param name="context">HttpContext containing the request uri.</param>
        /// <param name="baseUri">Base Uri for the specified request.</param>
        /// <param name="requestType">Type of Syndication request.</param>
        /// <param name="parameterName"><typeref name="SyndicationParameterType"/> 
        /// name of the parameter whose value is to be retrieved.</param>
        /// <returns>String containing value for the specified parameter.</returns>
        internal static string GetValueOfParameterFromUri(HttpContext context, Uri baseUri,
            SyndicationRequestType requestType, SyndicationParameterType parameterName)
        {
            UriTemplateMatch matchResult = SyndicationTemplates[requestType].Match(baseUri, context.Request.Url);

            if(null == matchResult)
            {
                return string.Empty;
            }

            return matchResult.BoundVariables[parameterName.ToString()];
        }

        /// <summary>
        /// Creates a RSS document from a syndication feed using Rss20FeedFormatter
        /// </summary>
        /// <param name="feed">Syndication feed</param>
        /// <param name="requestType">Type of Syndication request</param>
        /// <returns>string, containing ATOM/RSS feed in XML format</returns>
        internal static string GetResponseDocument(SyndicationFeed feed, SyndicationRequestType requestType)
        {
            XmlWriter writer = null;
            StringBuilder feedData = new StringBuilder();
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.ConformanceLevel = ConformanceLevel.Auto;
                settings.OmitXmlDeclaration = false;

                writer = XmlWriter.Create(feedData, settings);
                SyndicationFeedFormatter formatter;

                if(SyndicationRequestType.RSSSearch == requestType ||
                SyndicationRequestType.RSSSearchWithPageNo == requestType)
                {
                    formatter = feed.GetRss20Formatter();
                }
                else
                {
                    formatter = feed.GetAtom10Formatter();
                }
                formatter.WriteTo(writer);
            }
            finally
            {
                if (null != writer)
                {
                    writer.Close();
                }
            }

            return feedData.ToString();
        }

        /// <summary>
        /// Gets the type of request i.e. Atom Search, Rss Search etc.
        /// </summary>
        /// <param name="context">HttpContext containing the request uri.</param>
        /// <param name="baseUri">Base Uri for the specified request.</param>
        /// <returns>Syndication Request Type of Uri</returns>
        internal static SyndicationRequestType GetSyndicationRequestType
            (HttpContext context, Uri baseUri)
        {
            UriTemplateMatch match;
            foreach(KeyValuePair<SyndicationRequestType, UriTemplate> pair in
                SyndicationHelper.SyndicationTemplates)
            {
                match = pair.Value.Match(baseUri, context.Request.Url);
                if(null != match)
                {
                    if(SyndicationRequestType.DefaultSearchWithPageNo == pair.Key ||
                        SyndicationRequestType.ATOMSearchWithPageNo == pair.Key ||
                        SyndicationRequestType.RSSSearchWithPageNo == pair.Key)
                    {
                        int dummy;
                        if(int.TryParse(match.BoundVariables[SyndicationParameterType.PageSize.ToString()], out dummy))
                        {
                            return pair.Key;
                        }
                        else
                        {
                            return SyndicationRequestType.Unknown;
                        }
                    }
                    else
                    {
                        return pair.Key;
                    }
                }
            }
            return SyndicationRequestType.Unknown;

        }

        /// <summary>
        /// Creates a SyndicationFeed object
        /// </summary>
        /// <param name="resources">List of resources for which the feed needs to be created</param>
        /// <param name="searchQuery">search query, who's result will be return in feed</param>
        /// <param name="requestUrl">feed url</param>
        /// <returns>SyndicationFeed object</returns>
        internal static SyndicationFeed CreateSyndicationFeed(IEnumerable<Resource> resources, string searchQuery, Uri requestUrl)
        {
            SyndicationFeed feed = new SyndicationFeed(ConfigurationManager.AppSettings["SyndicationFeedTitle"] + searchQuery, ConfigurationManager.AppSettings["SyndicationFeedDescription"], requestUrl);
            feed.Generator = Properties.Resources.FEED_GENERATOR;

            string copyright = ConfigurationManager.AppSettings["Copyright"];
            copyright = string.IsNullOrEmpty(copyright) ? PlatformSettings.Copyrights : copyright;
            feed.Copyright = new TextSyndicationContent(copyright);

            string authorName = ConfigurationManager.AppSettings["AuthorName"];
            authorName = string.IsNullOrEmpty(authorName) ? PlatformSettings.AuthorName : authorName;
            string authorEmail = ConfigurationManager.AppSettings["AuthorEmail"];
            authorEmail = string.IsNullOrEmpty(authorEmail) ? PlatformSettings.AuthorEmail : authorEmail;
            feed.Authors.Add(new SyndicationPerson(authorEmail, authorName, string.Empty));

            feed.Language = System.Globalization.CultureInfo.CurrentUICulture.Name;
            feed.LastUpdatedTime = DateTime.Now;
            if(null != resources)
            {
                feed.Items = ConvertResourceList(resources);
            }

            return feed;
        }




        #endregion

        #region private methods

        /// <summary>
        /// Converts the list of resources to List of Syndication Item.
        /// </summary>
        /// <param name="resourceList">List of resources.</param>
        /// <returns>Returns a list of Syndication Items.</returns>
        private static IEnumerable<SyndicationItem> ConvertResourceList(
                IEnumerable<Resource> resourceList)
        {
            if(null == resourceList)
            {
                return null;
            }

            List<SyndicationItem> items = new List<SyndicationItem>();

            foreach(Resource resource in resourceList)
            {

                SyndicationItem item = CreateSyndicationItem(resource);
                if(null != item)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        /// <summary>
        /// Loads the URI templates.
        /// </summary>
        private static void LoadUriTemplates()
        {
            syndicationTemplates = new Dictionary<SyndicationRequestType, UriTemplate>();
            //The order of the templates is importent if "/{PageSize}" goes first it will match
            //the "/RSS" or "/ATOM" as well and break the system.
            syndicationTemplates.Add(SyndicationRequestType.DefaultSearch,
                new UriTemplate(""));
            syndicationTemplates.Add(SyndicationRequestType.RSSSearch,
                new UriTemplate("/RSS"));
            syndicationTemplates.Add(SyndicationRequestType.RSSSearchWithPageNo,
                new UriTemplate("/RSS/{" + SyndicationParameterType.PageSize + "}"));
            syndicationTemplates.Add(SyndicationRequestType.ATOMSearch,
                new UriTemplate("/Atom"));
            syndicationTemplates.Add(SyndicationRequestType.ATOMSearchWithPageNo,
                new UriTemplate("/Atom/{" + SyndicationParameterType.PageSize + "}"));
            syndicationTemplates.Add(SyndicationRequestType.Help,
                new UriTemplate("/Help"));
            syndicationTemplates.Add(SyndicationRequestType.DefaultSearchWithPageNo,
                new UriTemplate("/{" + SyndicationParameterType.PageSize + "}"));
        }

        /// <summary>
        /// Creates a <typeref name="System.ServiceModel.Syndication.SyndicationItem"/> item 
        /// for the given resource.
        /// </summary>
        /// <param name="resource">The associated resource.</param>
        /// <returns>The SyndicationItem Object.</returns>
        private static SyndicationItem CreateSyndicationItem(Resource resource)
        {
            if(null == resource)
            {
                return null;
            }

            SyndicationItem item = new SyndicationItem();
            ScholarlyWorks.ScholarlyWork scholarlyWork = resource as ScholarlyWorks.ScholarlyWork;
            if(null != scholarlyWork)
            {
                item = ZentityAtomPubStoreReader.GenerateSyndicationItem(new Uri(
                    ConfigurationManager.AppSettings["ServiceHost"] +
                    ConfigurationManager.AppSettings["AtomPubBaseUri"]), scholarlyWork);

            }
            else
            {
                //TODO: Would Title be Non Empty always.. If no then what would be the heading for the feed?
                item.Title = new TextSyndicationContent(
                            string.IsNullOrEmpty(resource.Title) ? string.Empty : resource.Title);
                item.Id = "urn:guid:" + resource.Id.ToString();
                item.Summary = new TextSyndicationContent(resource.Description);

                if(null != resource.DateAdded &&
                        DateTime.MinValue != resource.DateAdded.Value)
                {
                    item.PublishDate = new DateTimeOffset(resource.DateAdded.Value);
                }
                if(null != resource.DateModified &&
                        DateTime.MinValue != resource.DateModified.Value)
                {
                    item.LastUpdatedTime = new DateTimeOffset(resource.DateModified.Value);
                }

            }

            return item;
        }
        #endregion
    }




}
