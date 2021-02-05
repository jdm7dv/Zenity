// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace Zentity.Services
{
    #region Using namespace

    using System;
    using System.Data;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Security;
    using System.Xml.Linq;

    #endregion

    #region Constants Class

    /// <summary>
    /// Constants strings that are being used by the OAiPmhService. 
    /// </summary>
    internal static class Constants
    {
        #region OAI-PMH Service

        #region Protocol Specific

        /// <summary>
        /// MetadataPrefix associated with the OAI-PMH protocol; Currently only "oai_dc" is supported.
        /// </summary>
        public const string MetadataFormat = "oai_dc";

        /// <summary>
        /// protocol associated with OAI-PMH set to "2.0".
        /// </summary>
        public const string Protocolversion = "2.0";

        /// <summary>
        /// Compression Supported associated with OAI-PMH Protocol set to "no".
        /// </summary>
        public const string CompressionSupported = "no";

        #endregion

        #region Miscellaneous

        /// <summary>
        /// The content type associated with the HTTP Response for e.g. "text/xml".
        /// </summary>
        public const string ContentType = "text/xml";

        /// <summary>
        /// The prefix associated with the oai_dc namespace.
        /// </summary>
        public const string OaidcPrefix = "dc:";

        /// <summary>
        /// The date time format granularity that is supported by the Repository.
        /// </summary>
        public const string DateTimeGranularity = "yyyy-MM-ddTHH:mm:ssZ";

        /// <summary>
        /// The short date time format to check if the input date is correct or not.
        /// </summary>
        public const string DateTimeShortFormat = "yyyy-MM-dd";

        /// <summary>
        /// The date time format granularity that is supported by the Repository in UTC Format.
        /// </summary>
        public const string DateTimeGranularityUtcFormat = "YYYY-MM-DDThh:mm:ssZ";

        #endregion

        #endregion

        #region Syndication Service

        /// <summary>
        /// The default value for Copyright for feed object
        /// </summary>
        public const string Copyright = "@Copyright";

        /// <summary>
        /// The default value for Author Name for Feed object
        /// </summary>
        public const string AuthorName = "Your Name";

        /// <summary>
        /// The default value for AuthorEmail for Feed object
        /// </summary>
        public const string AuthorEmail = "email@domain.com";

        #endregion

        #region Sword Service

        public const string MetsDocumentName = "mets.xml";
        public const string DefaultMimeType = "text/plain";
        public const string FormatResourceType = "Zentity.Core.{0}, Zentity, Version=1.5.807.1801, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        public const string TypeNullableDateTime = "System.Nullable`1[System.DateTime]";
        
        public const string ServiceDocumentFirstPart = @"<?xml version=""1.0"" encoding='utf-8'?>" + "\n" +
                                         @"<service xmlns=""http://www.w3.org/2007/app""" + " \n" +
                                         @"xmlns:atom=""http://www.w3.org/2005/Atom""" + " \n" +
                                         @"xmlns:sword=""http://purl.org/net/sword/"">" + " \n" +
                                         @"<sword:level>0</sword:level>" + " \n" +
                                         @"<workspace>" + " \n";
        public const string ServiceDocumentLaterPart = @"     <atom:title>Zentity Repository : Resource Collection</atom:title>" + " \n" +
                                         @"     <accept>application/atom+xml;type=entry</accept>" + " \n" +
                                         @"     <accept>application/zip</accept>" + " \n" +
                                         @"</collection>
                                     </workspace>
                                     </service>
                                     ";
        public const string CollectionPlaceHolder = @"<collection" +
                                         @"     href="" {0} "" >" + " \n";


        /// <summary>
        /// SWORD error codes.
        /// </summary>
        public enum SwordErrorCodes
        {
            /// <summary>
            /// Created. 
            /// </summary>
            Created = 201,

            /// <summary>
            /// Accepted.
            /// </summary>
            Accepted = 202,

            /// <summary>
            /// Bad request.
            /// </summary>
            BadRequest = 400,

            /// <summary>
            /// Unauthorized.
            /// </summary>
            Unauthorized = 401,

            /// <summary>
            /// Forbidden.
            /// </summary>
            Forbidden = 403,

            /// <summary>
            /// Pre-condition failed.
            /// </summary>
            PreconditionFailed = 412,

            /// <summary>
            /// Un-supported media type.
            /// </summary>
            UnsupportedMediaType = 415,

            /// <summary>
            /// Internal server error.
            /// </summary>
            InternalServerError = 500,

            /// <summary>
            /// Not implemented.
            /// </summary>
            NotImplemented = 501
        };


        public const string KeyResourceType = "ResourceType";

        public const string MappingXPath = "/MappingsList/Mapping[@FileExtension ='{0}']"; // Replaces file extension.
        public const string MimeTypeAttribute = "MimeType";

        public const string AtomPrefix = "atom";
        public const string AtomEntryContentXPath = "/atom:entry/atom:content[@type='application/zip']";


        #region Atom Entry Response Constants

        public const string AtomNamespace = "http://www.w3.org/2005/Atom";
        public const string SwordNamespace = "http://purl.org/net/sword/";
        public const string Entry = "entry";
        public const string AttrXmlns = "xmlns";
        public const string XmlnsSword = "xmlns:sword";
        public const string Title = "title";
        public const string Id = "id";
        public const string Updated = "updated";
        public const string Author = "author";
        public const string Name = "name";
        public const string SummaryType = "text";
        public const string Summary = "summary";
        public const string TypeAttribute = "type";
        public const string Teatment = "sword:treatment";
        public const string TreatmentInnerText = "Successfully created a ScholarlyWork.";
        public const string Content = "content";
        public const string SourceAttribute = "src";
        public const string RelAttribute = "rel";
        public const string HrefAttribute = "href";
        public const string EditMedia = "edit - media";
        public const string Edit = "edit";

        #endregion

        #endregion

        #region Services Authentication Constants
        public const string GuestPassword = "guest@123";
        #endregion
    }

    #endregion
    
}
